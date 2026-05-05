using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Servicios.Multiidioma;

namespace GUI
{
    /// <summary>
    /// Capa de Presentación — Módulo de Pedidos Realizados (OperadorDeInventario).
    ///
    /// Permite al OperadorDeInventario gestionar el ciclo de vida post-venta:
    ///   ✓ Ver todos los pedidos con su estado actual
    ///   ✓ Filtrar por estado (Todos / Pendiente / Despachado / Entregado / Cancelado)
    ///   ✓ Despachar un pedido Pendiente → estado Despachado
    ///   ✓ Marcar Entregado un pedido Despachado → estado Entregado
    ///   ✓ Ver el detalle de prendas de cada pedido
    ///   ✓ Ver notificación de despacho (resumen para comunicar al cliente)
    ///
    /// Hereda de <see cref="FormBase"/>:
    ///   - MostrarOk() y MostrarError() → heredados, no se redeclaran
    ///   - MensajeLabel → sobreescrito para devolver el lblMensaje de este formulario
    ///
    /// Accesible desde Menú → Ventas → Pedidos Realizados (permiso mnuPedidosRealizados).
    /// </summary>
    public partial class PedidosRealizados : FormBase, IIdiomaObserver
    {
        protected override Label MensajeLabel => lblMensaje;

        private readonly BLL.Pedido pedidoBLL = new BLL.Pedido();

        private List<BE.Pedido> _pedidos = new List<BE.Pedido>();

        // Idioma activo — se actualiza en UpdateLanguage para poder usarlo
        // en los helpers EstadoLabel() y ComputarUrgencia() que no reciben parámetro.
        private Idioma _idioma = GestorIdioma.IdiomaActual;

        public PedidosRealizados()
        {
            InitializeComponent();
            this.Load += new EventHandler(PedidosRealizados_Load);
        }

        // ── Observer de idioma ────────────────────────────────────────────────

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GestorIdioma.SuscribirObservador(this);
            Traducir(GestorIdioma.IdiomaActual);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            GestorIdioma.DesuscribirObservador(this);
            base.OnFormClosing(e);
        }

        public void UpdateLanguage(Idioma idioma)
        {
            Traducir(idioma);
            // Re-aplicar filtro para que las celdas (Urgencia, Estado) se re-generen
            // con las etiquetas del nuevo idioma.
            AplicarFiltro();
        }

        private void Traducir(Idioma idioma)
        {
            _idioma = idioma;  // centralizado aquí para que EstadoLabel/ComputarUrgencia estén siempre sincronizados
            var t = Traductor.ObtenerTraducciones(idioma);
            if (this.Tag != null && t.ContainsKey(this.Tag.ToString()))
                this.Text = t[this.Tag.ToString()].Texto;
            Aplicar(lblEstado,          t);
            Aplicar(lblUltimos,         t);
            Aplicar(lblDias,            t);
            Aplicar(btnDespachar,       t);
            Aplicar(btnEntregado,       t);
            Aplicar(btnVerNotificacion, t);
            Aplicar(btnDevolucion,      t);
            Aplicar(lblDetalleTitulo,   t);
        }

        private static void Aplicar(Control c, IDictionary<string, Traduccion> t)
        {
            if (c?.Tag != null && t.ContainsKey(c.Tag.ToString()))
                c.Text = t[c.Tag.ToString()].Texto;
        }

        // ── Eventos del Designer ──────────────────────────────────────────────

        private void PedidosRealizados_Load(object sender, EventArgs e)
        {
            CargarPedidos();
        }

        private void CmbFiltroEstado_SelectedIndexChanged(object sender, EventArgs e)
        {
            AplicarFiltro();
        }

        private void NudDiasFiltro_ValueChanged(object sender, EventArgs e)
        {
            AplicarFiltro();
        }

        private void BtnRefrescar_Click(object sender, EventArgs e)
        {
            CargarPedidos();
        }

        // Carga y filtrado
        private void CargarPedidos()
        {
            try
            {
                _pedidos = pedidoBLL.ObtenerTodos();
                AplicarFiltro();
                MostrarOk($"{_pedidos.Count} pedido(s) en el sistema.");
            }
            catch (Exception ex)
            {
                MostrarError($"Error al cargar: {ex.Message}");
            }
        }

        private void AplicarFiltro()
        {
            int estadoIdx = cmbFiltroEstado.SelectedIndex;
            int dias      = (int)nudDiasFiltro.Value;
            DateTime? corte = dias > 0 ? (DateTime?)DateTime.Now.AddDays(-dias) : null;

            var lista = _pedidos.FindAll(p =>
            {
                bool pasaEstado =
                    estadoIdx == 0 ||
                    (estadoIdx == 1 && p.Estado == BE.EstadoPedido.Pendiente)  ||
                    (estadoIdx == 2 && p.Estado == BE.EstadoPedido.Despachado) ||
                    (estadoIdx == 3 && p.Estado == BE.EstadoPedido.Entregado)  ||
                    (estadoIdx == 4 && p.Estado == BE.EstadoPedido.Cancelado);

                bool pasaDias = corte == null || p.FechaPedido >= corte;

                return pasaEstado && pasaDias;
            });

            var tabla = new DataTable();
            tabla.Columns.Add("ID",         typeof(int));
            tabla.Columns.Add("Urgencia",   typeof(string));
            tabla.Columns.Add("Fecha",      typeof(string));
            tabla.Columns.Add("Cliente",    typeof(string));
            tabla.Columns.Add("Vendedor",   typeof(string));
            tabla.Columns.Add("Prendas",    typeof(int));
            tabla.Columns.Add("Estado",     typeof(string));
            tabla.Columns.Add("Despacho",   typeof(string));
            tabla.Columns.Add("Entrega",    typeof(string));
            // Columna interna: almacena el int del enum para colorear sin depender del idioma
            tabla.Columns.Add("_EstadoKey", typeof(int));

            foreach (var p in lista)
            {
                tabla.Rows.Add(
                    p.IdPedido,
                    ComputarUrgencia(p),
                    p.FechaPedido.ToString("dd/MM/yyyy HH:mm"),
                    p.NombreCliente,
                    p.NombreEmpleado,
                    p.CantidadPrendas,
                    EstadoLabel(p.Estado),
                    p.FechaDespacho?.ToString("dd/MM/yyyy") ?? "—",
                    p.FechaEntrega?.ToString("dd/MM/yyyy")  ?? "—",
                    (int)p.Estado);
            }

            dgvPedidos.DataSource = tabla;
            ColorearFilas();

            if (dgvPedidos.Columns.Contains("ID"))
                dgvPedidos.Columns["ID"].Width = 44;
            if (dgvPedidos.Columns.Contains("Urgencia"))
                dgvPedidos.Columns["Urgencia"].Width = 90;

            // Una sola llamada a ObtenerTraducciones para headers + conteo
            var t = Traductor.ObtenerTraducciones(_idioma);
            TraducirHeadersGrilla(t);

            string urgLabel = t.ContainsKey("urg.urgente") ? t["urg.urgente"].Texto : "Urgentes";
            string norLabel = t.ContainsKey("urg.normal")  ? t["urg.normal"].Texto  : "Normales";
            // Conteo de urgentes/normales usando el emoji (independiente del idioma)
            int nUrgentes = lista.Count(p => ComputarUrgencia(p).StartsWith("🔴"));
            int nNormales = lista.Count(p => ComputarUrgencia(p).StartsWith("🟡"));
            lblConteo.Text = $"Mostrando {lista.Count} de {_pedidos.Count}  |  " +
                             $"🔴 {urgLabel}: {nUrgentes}  🟡 {norLabel}: {nNormales}";
            LimpiarDetalle();
        }

        /// <summary>
        /// Calcula el nivel de urgencia según el tiempo transcurrido desde la fecha del pedido.
        /// 🔴 Urgente: Pendiente > 3 días o Despachado > 5 días
        /// 🟡 Normal:  1–3 días pendiente
        /// 🟢 Reciente: menos de 1 día
        /// El prefijo emoji queda fijo (lo usa ColorearFilas); el texto se traduce.
        /// </summary>
        private string ComputarUrgencia(BE.Pedido p)
        {
            if (p.Estado == BE.EstadoPedido.Entregado || p.Estado == BE.EstadoPedido.Cancelado)
                return "—";

            var t      = Traductor.ObtenerTraducciones(_idioma);
            string urg = t.ContainsKey("urg.urgente")  ? t["urg.urgente"].Texto  : "Urgente";
            string nor = t.ContainsKey("urg.normal")   ? t["urg.normal"].Texto   : "Normal";
            string rec = t.ContainsKey("urg.reciente") ? t["urg.reciente"].Texto : "Reciente";

            double dias = (DateTime.Now - p.FechaPedido).TotalDays;

            if (p.Estado == BE.EstadoPedido.Pendiente)
            {
                if (dias > 3) return $"🔴 {urg}";
                if (dias > 1) return $"🟡 {nor}";
                return $"🟢 {rec}";
            }
            if (p.Estado == BE.EstadoPedido.Despachado)
            {
                double diasDespacho = p.FechaDespacho.HasValue
                    ? (DateTime.Now - p.FechaDespacho.Value).TotalDays : dias;
                if (diasDespacho > 5) return $"🔴 {urg}";
                if (diasDespacho > 2) return $"🟡 {nor}";
                return $"🟢 {rec}";
            }
            return "—";
        }

        /// <summary>
        /// Renombra el HeaderText de las columnas de dgvPedidos según el idioma activo.
        /// Recibe el diccionario ya obtenido para evitar una llamada redundante a ObtenerTraducciones.
        /// Los nombres internos del DataTable no cambian (se usan como índices en ColorearFilas).
        /// </summary>
        private void TraducirHeadersGrilla(IDictionary<string, Traduccion> t)
        {
            void RH(string col, string clave)
            {
                if (dgvPedidos.Columns.Contains(col) && t.ContainsKey(clave))
                    dgvPedidos.Columns[col].HeaderText = t[clave].Texto;
            }

            RH("Urgencia", "col.ped.urgencia");
            RH("Fecha",    "col.ped.fecha");
            RH("Cliente",  "col.ped.cliente");
            RH("Vendedor", "col.ped.vendedor");
            RH("Prendas",  "col.ped.prendas");
            RH("Estado",   "col.ped.estado");
            RH("Despacho", "col.ped.despacho");
            RH("Entrega",  "col.ped.entrega");

            // Ocultar la columna interna de clave de estado
            if (dgvPedidos.Columns.Contains("_EstadoKey"))
                dgvPedidos.Columns["_EstadoKey"].Visible = false;
        }

        private void ColorearFilas()
        {
            foreach (DataGridViewRow row in dgvPedidos.Rows)
            {
                string urgencia  = row.Cells["Urgencia"].Value?.ToString() ?? "";
                // El emoji prefijo es independiente del idioma: 🔴 / 🟡 / 🟢
                if (urgencia.StartsWith("🔴"))
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 225, 225);
                else if (urgencia.StartsWith("🟡"))
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 250, 210);

                // Coloreado por estado usando la columna interna _EstadoKey (int enum)
                // para ser independiente del idioma de la etiqueta visible.
                if (!row.Cells.Contains("_EstadoKey")) continue;
                if (!int.TryParse(row.Cells["_EstadoKey"].Value?.ToString(), out int estadoKey)) continue;
                row.DefaultCellStyle.ForeColor = estadoKey switch
                {
                    (int)BE.EstadoPedido.Pendiente  => Color.FromArgb(160, 100, 0),
                    (int)BE.EstadoPedido.Despachado => Color.FromArgb(30, 100, 170),
                    (int)BE.EstadoPedido.Entregado  => Color.FromArgb(30, 130, 30),
                    (int)BE.EstadoPedido.Cancelado  => Color.FromArgb(150, 50, 50),
                    _                               => Color.Black
                };
            }
        }

        // ── Selección ─────────────────────────────────────────────────────────

        private void DgvPedidos_SelectionChanged(object sender, EventArgs e)
        {
            LimpiarDetalle();
            if (dgvPedidos.SelectedRows.Count == 0)
            {
                DeshabilitarBotones();
                return;
            }

            var pedido = ObtenerPedidoSeleccionado();
            if (pedido == null) { DeshabilitarBotones(); return; }

            btnDespachar.Enabled        = pedido.Estado == BE.EstadoPedido.Pendiente;
            btnEntregado.Enabled        = pedido.Estado == BE.EstadoPedido.Despachado;
            btnDevolucion.Enabled       = pedido.Estado == BE.EstadoPedido.Entregado;
            btnVerNotificacion.Enabled  = pedido.Estado == BE.EstadoPedido.Despachado ||
                                          pedido.Estado == BE.EstadoPedido.Entregado;

            CargarDetallePrendas(pedido);
        }

        private void CargarDetallePrendas(BE.Pedido pedidoResumen)
        {
            try
            {
                var pedido = pedidoBLL.ObtenerPorId(pedidoResumen.IdPedido);
                if (pedido == null) return;

                lblDetalleTitulo.Text =
                    $"Pedido #{pedido.IdPedido}  ·  {pedido.NombreCliente}  ·  " +
                    $"{EstadoLabel(pedido.Estado)}  ·  {pedido.CantidadPrendas} prenda(s)";

                var tabla = new DataTable();
                tabla.Columns.Add("Prenda",    typeof(string));
                tabla.Columns.Add("Categoría", typeof(string));
                tabla.Columns.Add("Talle",     typeof(string));
                tabla.Columns.Add("Color",     typeof(string));
                tabla.Columns.Add("Estado",    typeof(string));

                foreach (var p in pedido.Prendas)
                    tabla.Rows.Add(
                        p.Nombre,
                        p.Categoria ?? "—",
                        p.Talle     ?? "—",
                        p.Color     ?? "—",
                        p.Estado.ToString());

                dgvDetalle.DataSource = tabla;
            }
            catch { /* No interrumpir la UI si falla el detalle */ }
        }

        // ── Acciones ──────────────────────────────────────────────────────────

        private void BtnDespachar_Click(object sender, EventArgs e)
        {
            var pedido = ObtenerPedidoSeleccionado();
            if (pedido == null) return;

            var confirmar = MessageBox.Show(
                $"¿Despachar el Pedido #{pedido.IdPedido}?\n\n" +
                $"Cliente: {pedido.NombreCliente}\n" +
                $"Prendas: {pedido.CantidadPrendas}\n\n" +
                "El pedido pasará a estado Despachado.",
                "Confirmar Despacho",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);

            if (confirmar != DialogResult.Yes) return;

            try
            {
                pedidoBLL.Despachar(this, pedido);
                MostrarOk($"Pedido #{pedido.IdPedido} despachado correctamente.");
                CargarPedidos();
            }
            catch (Exception ex) { MostrarError(ex.Message); }
        }

        private void BtnEntregado_Click(object sender, EventArgs e)
        {
            var pedido = ObtenerPedidoSeleccionado();
            if (pedido == null) return;

            var confirmar = MessageBox.Show(
                $"¿Confirmar entrega del Pedido #{pedido.IdPedido} a {pedido.NombreCliente}?",
                "Confirmar Entrega",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);

            if (confirmar != DialogResult.Yes) return;

            try
            {
                pedidoBLL.MarcarEntregado(this, pedido);
                MostrarOk($"Pedido #{pedido.IdPedido} marcado como Entregado.");
                CargarPedidos();
            }
            catch (Exception ex) { MostrarError(ex.Message); }
        }

        private void BtnDevolucion_Click(object sender, EventArgs e)
        {
            var pedido = ObtenerPedidoSeleccionado();
            if (pedido == null) return;

            var pedidoCompleto = pedidoBLL.ObtenerPorId(pedido.IdPedido);
            if (pedidoCompleto == null) return;

            string listaPrendas = string.Join("\n  • ",
                pedidoCompleto.Prendas.ConvertAll(p => $"{p.Nombre} ({p.Talle} — {p.Color})"));

            var confirmar = MessageBox.Show(
                $"¿Registrar la devolución del Pedido #{pedido.IdPedido}?\n\n" +
                $"Cliente: {pedido.NombreCliente}\n\n" +
                $"Prendas que volverán a EnLimpieza:\n  • {listaPrendas}\n\n" +
                "Las prendas quedarán en estado 'En Limpieza' hasta ser revisadas\n" +
                "y marcadas Disponibles por el ControladorDeStock.",
                "Confirmar Devolución",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);

            if (confirmar != DialogResult.Yes) return;

            try
            {
                pedidoBLL.RegistrarDevolucion(this, pedido);
                MostrarOk($"Devolución del Pedido #{pedido.IdPedido} registrada. " +
                          $"{pedidoCompleto.CantidadPrendas} prenda(s) pasaron a EnLimpieza.");
                CargarPedidos();
            }
            catch (Exception ex) { MostrarError(ex.Message); }
        }

        private void BtnVerNotificacion_Click(object sender, EventArgs e)
        {
            var pedidoResumen = ObtenerPedidoSeleccionado();
            if (pedidoResumen == null) return;

            try
            {
                var pedido = pedidoBLL.ObtenerPorId(pedidoResumen.IdPedido);
                if (pedido == null) return;

                using (var notif = new NotificacionDespachoForm(pedido))
                    notif.ShowDialog(this);
            }
            catch (Exception ex) { MostrarError(ex.Message); }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private BE.Pedido ObtenerPedidoSeleccionado()
        {
            if (dgvPedidos.SelectedRows.Count == 0) return null;
            int id = Convert.ToInt32(dgvPedidos.SelectedRows[0].Cells["ID"].Value);
            return _pedidos.Find(p => p.IdPedido == id);
        }

        private void LimpiarDetalle()
        {
            dgvDetalle.DataSource = null;
            lblDetalleTitulo.Text = "Detalle del pedido seleccionado";
        }

        private void DeshabilitarBotones()
        {
            btnDespachar.Enabled       = false;
            btnEntregado.Enabled       = false;
            btnDevolucion.Enabled      = false;
            btnVerNotificacion.Enabled = false;
        }

        private string EstadoLabel(BE.EstadoPedido e)
        {
            var t = Traductor.ObtenerTraducciones(_idioma);
            switch (e)
            {
                case BE.EstadoPedido.Pendiente:  return t.ContainsKey("est.pendiente")  ? t["est.pendiente"].Texto  : "Pendiente";
                case BE.EstadoPedido.Despachado: return t.ContainsKey("est.despachado") ? t["est.despachado"].Texto : "Despachado";
                case BE.EstadoPedido.Entregado:  return t.ContainsKey("est.entregado")  ? t["est.entregado"].Texto  : "Entregado";
                case BE.EstadoPedido.Cancelado:  return t.ContainsKey("est.cancelado")  ? t["est.cancelado"].Texto  : "Cancelado";
                default: return e.ToString();
            }
        }

    }
}
