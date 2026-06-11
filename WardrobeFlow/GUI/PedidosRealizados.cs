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

        private readonly BLL.Interfaces.IPedidoService pedidoBLL = new BLL.Pedido();

        private List<BE.Pedido> _pedidos = new List<BE.Pedido>();

        // Idioma activo — se actualiza en UpdateLanguage para poder usarlo
        // en los helpers EstadoLabel() y ComputarUrgencia() que no reciben parámetro.
        private Idioma _idioma = GestorIdioma.IdiomaActual;

        public PedidosRealizados()
        {
            InitializeComponent();
            AgregarBotonHistorial();
            this.Load += new EventHandler(PedidosRealizados_Load);
        }

        // ── Observer de idioma ────────────────────────────────────────────────

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GestorIdioma.SuscribirObservador(this);
            Traducir(GestorIdioma.IdiomaActual);
            // Re-aplicar después de que el combo queda correctamente inicializado
            // (el Designer no fija SelectedIndex=0, por eso AplicarFiltro del Load
            //  veía estadoIdx=-1 y mostraba 0 filas)
            AplicarFiltro();
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
            RellenarComboEstado(idioma);
            // Botón Historial (dinámico, sin Tag en Designer)
            if (_btnHistorial != null && t.ContainsKey("btn.historial"))
                _btnHistorial.Text = t["btn.historial"].Texto;
        }

        /// <summary>
        /// Rellena el combo de filtro de estado con valores traducidos.
        /// Mantiene el orden fijo (índice 0–4) para que el filtro por índice siga funcionando.
        /// </summary>
        private void RellenarComboEstado(Idioma idioma)
        {
            int idx = cmbFiltroEstado.SelectedIndex;
            cmbFiltroEstado.SelectedIndexChanged -= CmbFiltroEstado_SelectedIndexChanged;
            cmbFiltroEstado.Items.Clear();
            var t = Traductor.ObtenerTraducciones(idioma);
            string T(string key, string fallback) => t.ContainsKey(key) ? t[key].Texto : fallback;
            cmbFiltroEstado.Items.Add(T("combo.prenda.todos", "Todos"));
            cmbFiltroEstado.Items.Add(T("est.pendiente",  "Pendiente"));
            cmbFiltroEstado.Items.Add(T("est.despachado", "Despachado"));
            cmbFiltroEstado.Items.Add(T("est.entregado",  "Entregado"));
            cmbFiltroEstado.Items.Add(T("est.cancelado",  "Cancelado"));
            cmbFiltroEstado.SelectedIndex = idx >= 0 && idx < cmbFiltroEstado.Items.Count ? idx : 0;
            cmbFiltroEstado.SelectedIndexChanged += CmbFiltroEstado_SelectedIndexChanged;
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
                var tSis = Traductor.ObtenerTraducciones(_idioma);
                string fmtSis = tSis.ContainsKey("msg.ped.ensistema") ? tSis["msg.ped.ensistema"].Texto : "{0} pedido(s) en el sistema.";
                MostrarOk(string.Format(fmtSis, _pedidos.Count));
            }
            catch (Exception ex)
            {
                MostrarError(ex);
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
            string fmtMostr = t.ContainsKey("msg.ped.mostrando") ? t["msg.ped.mostrando"].Texto : "Mostrando {0} de {1}";
            // Conteo de urgentes/normales usando el emoji (independiente del idioma)
            int nUrgentes = lista.Count(p => ComputarUrgencia(p).StartsWith("🔴"));
            int nNormales = lista.Count(p => ComputarUrgencia(p).StartsWith("🟡"));
            lblConteo.Text = string.Format(fmtMostr, lista.Count, _pedidos.Count) +
                             $"  |  🔴 {urgLabel}: {nUrgentes}  🟡 {norLabel}: {nNormales}";
            LimpiarDetalle();
        }

        /// <summary>
        /// Mapea el NivelUrgencia (calculado en BLL) a emoji + texto traducido.
        /// El prefijo emoji queda fijo (lo usa ColorearFilas); el texto se traduce.
        /// </summary>
        private string ComputarUrgencia(BE.Pedido p)
        {
            var nivel = pedidoBLL.CalcularNivelUrgencia(p);
            if (nivel == BE.NivelUrgencia.NoAplica) return "—";

            var t      = Traductor.ObtenerTraducciones(_idioma);
            string urg = t.ContainsKey("urg.urgente")  ? t["urg.urgente"].Texto  : "Urgente";
            string nor = t.ContainsKey("urg.normal")   ? t["urg.normal"].Texto   : "Normal";
            string rec = t.ContainsKey("urg.reciente") ? t["urg.reciente"].Texto : "Reciente";

            switch (nivel)
            {
                case BE.NivelUrgencia.Urgente:  return $"🔴 {urg}";
                case BE.NivelUrgencia.Normal:   return $"🟡 {nor}";
                case BE.NivelUrgencia.Reciente: return $"🟢 {rec}";
                default:                        return "—";
            }
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
                if (!dgvPedidos.Columns.Contains("_EstadoKey")) continue;
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
            if (_btnHistorial != null) _btnHistorial.Enabled = true;

            CargarDetallePrendas(pedido);
        }

        private void CargarDetallePrendas(BE.Pedido pedidoResumen)
        {
            try
            {
                var pedido = pedidoBLL.ObtenerPorId(pedidoResumen.IdPedido);
                if (pedido == null) return;

                var tDet = Traductor.ObtenerTraducciones(_idioma);
                string T_det(string k, string fb) => tDet.ContainsKey(k) ? tDet[k].Texto : fb;
                string prendasLbl = T_det("col.ped.prendas", "prenda(s)");
                lblDetalleTitulo.Text = string.Format(
                    T_det("lbl.ped.detalletitulo", "Pedido #{0}  ·  {1}  ·  {2}  ·  {3} {4}"),
                    pedido.IdPedido, pedido.NombreCliente,
                    EstadoLabel(pedido.Estado), pedido.CantidadPrendas, prendasLbl);

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
                        EstadoPrendaLabel(p.Estado));

                dgvDetalle.DataSource = tabla;
                TraducirHeadersDetalle();
            }
            catch (Exception ex) { System.Diagnostics.Trace.TraceError($"[PedidosRealizados] Error al cargar detalle: {ex.Message}"); }
        }

        /// <summary>Traduce los HeaderText de la grilla de detalle de prendas según el idioma activo.</summary>
        private void TraducirHeadersDetalle()
        {
            var t = Traductor.ObtenerTraducciones(_idioma);
            void RH(string col, string clave, string fallback)
            {
                if (dgvDetalle.Columns.Contains(col) && t.ContainsKey(clave))
                    dgvDetalle.Columns[col].HeaderText = t[clave].Texto;
                else if (dgvDetalle.Columns.Contains(col))
                    dgvDetalle.Columns[col].HeaderText = fallback;
            }
            RH("Prenda",    "col.det.prenda",      "Prenda");
            RH("Categoría", "col.prenda.categoria", "Categoría");
            RH("Talle",     "col.prenda.talle",     "Talle");
            RH("Color",     "col.prenda.color",     "Color");
            RH("Estado",    "col.prenda.estado",    "Estado");
        }

        // ── Acciones ──────────────────────────────────────────────────────────

        private void BtnDespachar_Click(object sender, EventArgs e)
        {
            var pedido = ObtenerPedidoSeleccionado();
            if (pedido == null) return;

            var tDesp = Traductor.ObtenerTraducciones(_idioma);
            string T_desp(string k, string fb) => tDesp.ContainsKey(k) ? tDesp[k].Texto : fb;
            string bodyDesp = string.Format(
                T_desp("conf.despachar.body", "¿Despachar el Pedido #{0}?\n\nCliente: {1}\nPrendas: {2}\n\nEl pedido pasará a estado Despachado."),
                pedido.IdPedido, pedido.NombreCliente, pedido.CantidadPrendas);

            var confirmar = MessageBox.Show(
                bodyDesp,
                T_desp("conf.despachar.titulo", "Confirmar Despacho"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);

            if (confirmar != DialogResult.Yes) return;

            try
            {
                pedidoBLL.Despachar(this.Text, pedido);
                MostrarOk(string.Format(T_desp("msg.ped.despachado", "Pedido #{0} despachado correctamente."), pedido.IdPedido));
                CargarPedidos();
            }
            catch (Exception ex) { MostrarError(ex); }
        }

        private void BtnEntregado_Click(object sender, EventArgs e)
        {
            var pedido = ObtenerPedidoSeleccionado();
            if (pedido == null) return;

            var tEntr = Traductor.ObtenerTraducciones(_idioma);
            string T_entr(string k, string fb) => tEntr.ContainsKey(k) ? tEntr[k].Texto : fb;
            string bodyEntr = string.Format(
                T_entr("conf.entrega.body", "¿Confirmar entrega del Pedido #{0} a {1}?"),
                pedido.IdPedido, pedido.NombreCliente);

            var confirmar = MessageBox.Show(
                bodyEntr,
                T_entr("conf.entrega.titulo", "Confirmar Entrega"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);

            if (confirmar != DialogResult.Yes) return;

            try
            {
                pedidoBLL.MarcarEntregado(this.Text, pedido);
                MostrarOk(string.Format(T_entr("msg.ped.entregado", "Pedido #{0} marcado como Entregado."), pedido.IdPedido));
                CargarPedidos();
            }
            catch (Exception ex) { MostrarError(ex); }
        }

        private void BtnDevolucion_Click(object sender, EventArgs e)
        {
            var pedido = ObtenerPedidoSeleccionado();
            if (pedido == null) return;

            var pedidoCompleto = pedidoBLL.ObtenerPorId(pedido.IdPedido);
            if (pedidoCompleto == null) return;

            var tDev = Traductor.ObtenerTraducciones(_idioma);
            string T_dev(string k, string fb) => tDev.ContainsKey(k) ? tDev[k].Texto : fb;
            string bodyDev = string.Format(
                T_dev("conf.devolucion.body", "¿Registrar devolución del Pedido #{0}?\n\nCliente: {1}\nPrendas: {2}\n\nLas prendas pasarán a estado EnLimpieza."),
                pedidoCompleto.IdPedido, pedidoCompleto.NombreCliente, pedidoCompleto.CantidadPrendas);

            var confirmar = MessageBox.Show(
                bodyDev,
                T_dev("conf.devolucion.titulo", "Confirmar Devolución"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);

            if (confirmar != DialogResult.Yes) return;

            try
            {
                pedidoBLL.RegistrarDevolucion(this.Text, pedidoCompleto);
                MostrarOk(string.Format(T_dev("msg.ped.devolucion", "Devolución registrada — {0} prenda(s) pasan a EnLimpieza."), pedidoCompleto.CantidadPrendas));
                CargarPedidos();
            }
            catch (Exception ex) { MostrarError(ex); }
        }

        private void BtnVerNotificacion_Click(object sender, EventArgs e)
        {
            var pedido = ObtenerPedidoSeleccionado();
            if (pedido == null) return;

            var completo = pedidoBLL.ObtenerPorId(pedido.IdPedido);
            if (completo == null) return;

            var tN = Traductor.ObtenerTraducciones(_idioma);
            string T_n(string k, string fb) => tN.ContainsKey(k) ? tN[k].Texto : fb;

            // Reutilizar claves de columnas existentes para las etiquetas de la notificación
            string notifTitulo   = T_n("notif.titulo",   "NOTIFICACIÓN DE PEDIDO");
            string notifNumero   = T_n("notif.numero",   "Pedido #:");
            string notifCliente  = T_n("col.ped.cliente","Cliente")  + ":";
            string notifEstado   = T_n("col.ped.estado", "Estado")   + ":";
            string notifFecha    = T_n("col.ped.fecha",  "Fecha")    + ":";
            string notifDespacho = T_n("col.ped.despacho","Despacho")+ ":";
            string notifEntrega  = T_n("col.ped.entrega","Entrega")  + ":";
            string notifPrendas  = T_n("col.ped.prendas","Prendas")  + ":";

            string texto =
                $"=== {notifTitulo} ===\n\n" +
                $"{notifNumero,-12}{completo.IdPedido}\n" +
                $"{notifCliente,-12}{completo.NombreCliente}\n" +
                $"{notifEstado,-12}{EstadoLabel(completo.Estado)}\n" +
                $"{notifFecha,-12}{completo.FechaPedido:dd/MM/yyyy HH:mm}\n" +
                $"{notifDespacho,-12}{(completo.FechaDespacho.HasValue ? completo.FechaDespacho.Value.ToString("dd/MM/yyyy") : "—")}\n" +
                $"{notifEntrega,-12}{(completo.FechaEntrega.HasValue  ? completo.FechaEntrega.Value.ToString("dd/MM/yyyy")  : "—")}\n" +
                $"{notifPrendas,-12}{completo.CantidadPrendas}\n";

            string tituloMsgBox = string.Format(T_n("notif.msgbox.titulo", "Notificación — Pedido #{0}"), completo.IdPedido);
            MessageBox.Show(texto, tituloMsgBox,
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void DeshabilitarBotones()
        {
            btnDespachar.Enabled       = false;
            btnEntregado.Enabled       = false;
            btnDevolucion.Enabled      = false;
            btnVerNotificacion.Enabled = false;
            if (_btnHistorial != null) _btnHistorial.Enabled = false;
        }

        private void LimpiarDetalle()
        {
            dgvDetalle.DataSource = null;
            lblDetalleTitulo.Text = "";
        }

        private BE.Pedido ObtenerPedidoSeleccionado()
        {
            if (dgvPedidos.SelectedRows.Count == 0) return null;
            int id = Convert.ToInt32(dgvPedidos.SelectedRows[0].Cells["ID"].Value);
            return _pedidos.Find(p => p.IdPedido == id);
        }

        private string EstadoLabel(BE.EstadoPedido estado)
        {
            var t = Traductor.ObtenerTraducciones(_idioma);
            switch (estado)
            {
                case BE.EstadoPedido.Pendiente:  return t.ContainsKey("est.pendiente")  ? t["est.pendiente"].Texto  : "Pendiente";
                case BE.EstadoPedido.Despachado: return t.ContainsKey("est.despachado") ? t["est.despachado"].Texto : "Despachado";
                case BE.EstadoPedido.Entregado:  return t.ContainsKey("est.entregado")  ? t["est.entregado"].Texto  : "Entregado";
                case BE.EstadoPedido.Cancelado:  return t.ContainsKey("est.cancelado")  ? t["est.cancelado"].Texto  : "Cancelado";
                default: return estado.ToString();
            }
        }

        private string EstadoPrendaLabel(BE.EstadoPrenda estado)
        {
            var t = Traductor.ObtenerTraducciones(_idioma);
            switch (estado)
            {
                case BE.EstadoPrenda.Disponible:  return t.ContainsKey("prenda.disponible")  ? t["prenda.disponible"].Texto  : "Disponible";
                case BE.EstadoPrenda.EnUso:       return t.ContainsKey("prenda.enuso")       ? t["prenda.enuso"].Texto       : "En Uso";
                case BE.EstadoPrenda.EnLimpieza:  return t.ContainsKey("prenda.enlimpieza")  ? t["prenda.enlimpieza"].Texto  : "En Limpieza";
                case BE.EstadoPrenda.Baja:        return t.ContainsKey("prenda.baja")        ? t["prenda.baja"].Texto        : "Baja";
                default: return estado.ToString();
            }
        }

        // ── Historial de cambios ──────────────────────────────────────────────

        private Button _btnHistorial;

        /// <summary>
        /// Agrega el botón "📋 Historial" al panelTop en tiempo de ejecución.
        /// Se ubica a continuación de btnRefrescar (Location.X = 640).
        /// </summary>
        private void AgregarBotonHistorial()
        {
            var tHR = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
            string histTextR = tHR.ContainsKey("btn.historial") ? tHR["btn.historial"].Texto : "📋 Historial";
            _btnHistorial = new Button
            {
                Text      = histTextR,
                Size      = new Size(110, 28),
                Location  = new Point(728, 50),
                BackColor = Color.FromArgb(100, 80, 160),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
                Enabled   = false
            };
            _btnHistorial.FlatAppearance.BorderSize = 0;
            _btnHistorial.Click += BtnHistorial_Click;
            panelTop.Controls.Add(_btnHistorial);
        }

        private void BtnHistorial_Click(object sender, EventArgs e)
        {
            var pedido = ObtenerPedidoSeleccionado();
            if (pedido == null) return;

            using (var frm = new PedidoHistorialForm(pedido.IdPedido))
                frm.ShowDialog(this);
        }
    }
}
