using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Servicios.Multiidioma;

namespace GUI
{
    /// <summary>
    /// Capa de Presentación — Módulo de Pedidos de Venta.
    ///
    /// Permite al Vendedor:
    ///   ✓ Ver todos los pedidos realizados con su estado actual
    ///   ✓ Crear un nuevo pedido (abre NuevoPedidoForm)
    ///   ✓ Cancelar un pedido pendiente (libera prendas)
    ///   ✓ Ver detalle de prendas de cada pedido al seleccionarlo
    ///
    /// Hereda de <see cref="FormBase"/>:
    ///   - MostrarOk() y MostrarError() → heredados, no se redeclaran
    ///   - MensajeLabel → sobreescrito para devolver el lblMensaje de este formulario
    ///
    /// Accesible desde Menú → Ventas → Pedidos de Venta (permiso mnuPedidosVenta).
    /// </summary>
    public partial class PedidosVenta : FormBase, IIdiomaObserver
    {
        protected override Label MensajeLabel => lblMensaje;

        private readonly BLL.Pedido pedidoBLL = new BLL.Pedido();

        private List<BE.Pedido> _pedidos = new List<BE.Pedido>();

        // Idioma activo — se actualiza en Traducir() para usarlo en EstadoLabel() y ColorearFilasPedidos()
        private Idioma _idioma = GestorIdioma.IdiomaActual;

        public PedidosVenta()
        {
            InitializeComponent();
            this.Load += new EventHandler(PedidosVenta_Load);
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
            // Recargar la grilla para que EstadoLabel() y los headers usen el nuevo idioma
            CargarPedidos();
        }

        private void Traducir(Idioma idioma)
        {
            _idioma = idioma;  // mantener sincronizado para EstadoLabel y ColorearFilasPedidos
            var t = Traductor.ObtenerTraducciones(idioma);
            if (this.Tag != null && t.ContainsKey(this.Tag.ToString()))
                this.Text = t[this.Tag.ToString()].Texto;
            Aplicar(btnNuevoPedido,   t);
            Aplicar(btnCancelar,      t);
            Aplicar(btnDesCancelar,   t);
            Aplicar(lblDetalleTitulo, t);
        }

        private static void Aplicar(Control c, IDictionary<string, Traduccion> t)
        {
            if (c?.Tag != null && t.ContainsKey(c.Tag.ToString()))
                c.Text = t[c.Tag.ToString()].Texto;
        }

        // ── Eventos del Designer ──────────────────────────────────────────────

        private void PedidosVenta_Load(object sender, EventArgs e)
        {
            CargarPedidos();
        }

        private void BtnRefrescar_Click(object sender, EventArgs e)
        {
            CargarPedidos();
        }

        // ── Carga ─────────────────────────────────────────────────────────────

        private void CargarPedidos()
        {
            try
            {
                _pedidos = pedidoBLL.ObtenerTodos();
                var tabla = new DataTable();
                tabla.Columns.Add("ID",         typeof(int));
                tabla.Columns.Add("Fecha",      typeof(string));
                tabla.Columns.Add("Cliente",    typeof(string));
                tabla.Columns.Add("Vendedor",   typeof(string));
                tabla.Columns.Add("Prendas",    typeof(int));
                tabla.Columns.Add("Estado",     typeof(string));
                tabla.Columns.Add("Despacho",   typeof(string));
                tabla.Columns.Add("Entrega",    typeof(string));
                tabla.Columns.Add("Motivo",     typeof(string));
                // Columna interna: int del enum para colorear sin depender del idioma activo
                tabla.Columns.Add("_EstadoKey", typeof(int));

                foreach (var p in _pedidos)
                {
                    tabla.Rows.Add(
                        p.IdPedido,
                        p.FechaPedido.ToString("dd/MM/yyyy HH:mm"),
                        p.NombreCliente,
                        p.NombreEmpleado,
                        p.CantidadPrendas,
                        EstadoLabel(p.Estado),
                        p.FechaDespacho.HasValue ? p.FechaDespacho.Value.ToString("dd/MM/yyyy") : "—",
                        p.FechaEntrega.HasValue  ? p.FechaEntrega.Value.ToString("dd/MM/yyyy")  : "—",
                        p.MotivoCancelacion ?? "",
                        (int)p.Estado);
                }

                dgvPedidos.DataSource = tabla;
                ColorearFilasPedidos();

                if (dgvPedidos.Columns.Contains("ID"))
                    dgvPedidos.Columns["ID"].Width = 44;

                // Ocultar la columna interna y traducir headers
                if (dgvPedidos.Columns.Contains("_EstadoKey"))
                    dgvPedidos.Columns["_EstadoKey"].Visible = false;
                TraducirHeadersGrilla();

                lblConteo.Text = $"{_pedidos.Count} pedido(s)";
                dgvDetallePrendas.DataSource = null;
                // lblDetalleTitulo se traduce via Tag en Traducir(); no hardcodear aquí
                Aplicar(lblDetalleTitulo, Traductor.ObtenerTraducciones(_idioma));

                MostrarOk($"{_pedidos.Count} pedido(s) cargado(s).");
            }
            catch (Exception ex)
            {
                MostrarError($"Error al cargar pedidos: {ex.Message}");
            }
        }

        private void ColorearFilasPedidos()
        {
            foreach (DataGridViewRow row in dgvPedidos.Rows)
            {
                // Usar _EstadoKey (int del enum) en lugar del texto visible,
                // así el coloreado funciona independientemente del idioma activo.
                if (!row.Cells.Contains("_EstadoKey")) continue;
                if (!int.TryParse(row.Cells["_EstadoKey"].Value?.ToString(), out int estadoKey)) continue;
                row.DefaultCellStyle.ForeColor = estadoKey switch
                {
                    (int)BE.EstadoPedido.Pendiente  => Color.FromArgb(160, 100, 0),
                    (int)BE.EstadoPedido.Despachado => Color.FromArgb(30, 100, 170),
                    (int)BE.EstadoPedido.Entregado  => Color.FromArgb(30, 130, 30),
                    (int)BE.EstadoPedido.Cancelado  => Color.FromArgb(160, 50, 50),
                    _                               => Color.Black
                };
            }
        }

        /// <summary>
        /// Renombra los HeaderText de las columnas de dgvPedidos según el idioma activo.
        /// Los nombres internos del DataTable no cambian (se usan en ObtenerPedidoSeleccionado).
        /// </summary>
        private void TraducirHeadersGrilla()
        {
            var t = Traductor.ObtenerTraducciones(_idioma);

            void RH(string col, string clave)
            {
                if (dgvPedidos.Columns.Contains(col) && t.ContainsKey(clave))
                    dgvPedidos.Columns[col].HeaderText = t[clave].Texto;
            }

            RH("Fecha",    "col.ped.fecha");
            RH("Cliente",  "col.ped.cliente");
            RH("Vendedor", "col.ped.vendedor");
            RH("Prendas",  "col.ped.prendas");
            RH("Estado",   "col.ped.estado");
            RH("Despacho", "col.ped.despacho");
            RH("Entrega",  "col.ped.entrega");
        }

        private void DgvPedidos_SelectionChanged(object sender, EventArgs e)
        {
            bool hay = dgvPedidos.SelectedRows.Count > 0;
            dgvDetallePrendas.DataSource = null;

            if (!hay)
            {
                btnCancelar.Enabled    = false;
                btnDesCancelar.Enabled = false;
                return;
            }

            var pedido = ObtenerPedidoSeleccionado();
            if (pedido == null) return;

            btnCancelar.Enabled    = pedido.Estado == BE.EstadoPedido.Pendiente;
            btnDesCancelar.Enabled = pedido.Estado == BE.EstadoPedido.Cancelado;

            // Cargar detalle de prendas del pedido seleccionado
            CargarDetallePrendas(pedido.IdPedido);

            lblDetalleTitulo.Text =
                $"Pedido #{pedido.IdPedido} — {pedido.NombreCliente} — {EstadoLabel(pedido.Estado)}" +
                (!string.IsNullOrEmpty(pedido.MotivoCancelacion)
                    ? $"  |  Motivo: {pedido.MotivoCancelacion}" : "");
        }

        private void CargarDetallePrendas(int idPedido)
        {
            try
            {
                var pedidoCompleto = pedidoBLL.ObtenerPorId(idPedido);
                if (pedidoCompleto == null) return;

                var tabla = new DataTable();
                tabla.Columns.Add("Prenda",    typeof(string));
                tabla.Columns.Add("Categoría", typeof(string));
                tabla.Columns.Add("Talle",     typeof(string));
                tabla.Columns.Add("Color",     typeof(string));
                tabla.Columns.Add("Estado",    typeof(string));

                foreach (var p in pedidoCompleto.Prendas)
                    tabla.Rows.Add(p.Nombre, p.Categoria ?? "—",
                        p.Talle ?? "—", p.Color ?? "—", p.Estado.ToString());

                dgvDetallePrendas.DataSource = tabla;
            }
            catch { /* No interrumpir si el detalle falla */ }
        }

        // ── Eventos ───────────────────────────────────────────────────────────

        private void BtnNuevoPedido_Click(object sender, EventArgs e)
        {
            using (var form = new NuevoPedidoForm())
            {
                if (form.ShowDialog(this) != DialogResult.OK) return;

                MostrarOk($"Pedido #{form.IdPedidoCreado} creado exitosamente. Estado: Pendiente.");
                CargarPedidos();
            }
        }

        private void BtnCancelarPedido_Click(object sender, EventArgs e)
        {
            var pedido = ObtenerPedidoSeleccionado();
            if (pedido == null) return;

            // Pedir motivo de cancelación con un dialog inline
            string motivo = PedirTexto(
                $"Motivo de cancelación del Pedido #{pedido.IdPedido} ({pedido.NombreCliente}):",
                "Motivo de Cancelación");

            if (string.IsNullOrWhiteSpace(motivo))
            {
                MostrarError("La cancelación requiere un motivo.");
                return;
            }

            var confirmar = MessageBox.Show(
                $"¿Cancelar el Pedido #{pedido.IdPedido} de {pedido.NombreCliente}?\n\n" +
                $"Motivo: {motivo}\n\nLas prendas volverán a estado Disponible.",
                "Confirmar Cancelación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (confirmar != DialogResult.Yes) return;

            try
            {
                pedidoBLL.Cancelar(this, pedido, motivo);
                MostrarOk($"Pedido #{pedido.IdPedido} cancelado. Prendas liberadas.");
                CargarPedidos();
            }
            catch (Exception ex)
            {
                MostrarError(ex.Message);
            }
        }

        private void BtnDesCancelarPedido_Click(object sender, EventArgs e)
        {
            var pedido = ObtenerPedidoSeleccionado();
            if (pedido == null) return;

            var confirmar = MessageBox.Show(
                $"¿Des-cancelar el Pedido #{pedido.IdPedido} de {pedido.NombreCliente}?\n\n" +
                "Se verificará que las prendas originales estén disponibles\n" +
                "y el pedido volverá a estado Pendiente.",
                "Confirmar Des-cancelación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);

            if (confirmar != DialogResult.Yes) return;

            try
            {
                pedidoBLL.DesCancelar(this, pedido);
                MostrarOk($"Pedido #{pedido.IdPedido} reactivado — volvió a Pendiente.");
                CargarPedidos();
            }
            catch (Exception ex)
            {
                MostrarError(ex.Message);
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

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

        /// <summary>
        /// Muestra un dialog simple para pedir texto al usuario.
        /// Devuelve null si cancela o deja vacío.
        /// </summary>
        private string PedirTexto(string prompt, string titulo)
        {
            string resultado = null;
            using (var dlg = new Form())
            {
                dlg.Text            = titulo;
                dlg.ClientSize      = new Size(420, 130);
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox     = false;
                dlg.MinimizeBox     = false;
                dlg.StartPosition   = FormStartPosition.CenterParent;

                dlg.Controls.Add(new Label
                {
                    Text = prompt, Left = 12, Top = 12,
                    Width = 396, Height = 36,
                    Font = new Font("Segoe UI", 9f)
                });

                var txt = new TextBox { Left = 12, Top = 52, Width = 396 };
                dlg.Controls.Add(txt);

                var btnOk = new Button
                {
                    Text = "Aceptar", Left = 220, Top = 84,
                    Width = 90, Height = 30,
                    DialogResult = DialogResult.OK,
                    BackColor = Color.SteelBlue, ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                btnOk.FlatAppearance.BorderSize = 0;

                var btnCancel = new Button
                {
                    Text = "Cancelar", Left = 318, Top = 84,
                    Width = 90, Height = 30,
                    DialogResult = DialogResult.Cancel,
                    FlatStyle = FlatStyle.Flat
                };

                dlg.Controls.Add(btnOk);
                dlg.Controls.Add(btnCancel);
                dlg.AcceptButton = btnOk;
                dlg.CancelButton = btnCancel;

                if (dlg.ShowDialog(this) == DialogResult.OK)
                    resultado = txt.Text.Trim();
            }
            return resultado;
        }

    }
}
