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

        private readonly BLL.Interfaces.IPedidoService pedidoBLL = new BLL.Pedido();

        private List<BE.Pedido> _pedidos = new List<BE.Pedido>();

        // Idioma activo — se actualiza en Traducir() para usarlo en EstadoLabel() y ColorearFilasPedidos()
        private Idioma _idioma = GestorIdioma.IdiomaActual;

        public PedidosVenta()
        {
            InitializeComponent();
            AgregarBotonHistorial();
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
            // Botón Historial (dinámico, sin Tag en Designer)
            if (_btnHistorial != null && t.ContainsKey("btn.historial"))
                _btnHistorial.Text = t["btn.historial"].Texto;
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

                var tConteo = Traductor.ObtenerTraducciones(_idioma);
                string fmtConteo   = tConteo.ContainsKey("msg.ped.conteo")   ? tConteo["msg.ped.conteo"].Texto   : "{0} pedido(s)";
                string fmtCargados = tConteo.ContainsKey("msg.ped.cargados") ? tConteo["msg.ped.cargados"].Texto : "{0} pedido(s) cargado(s).";
                lblConteo.Text = string.Format(fmtConteo, _pedidos.Count);
                dgvDetallePrendas.DataSource = null;
                // lblDetalleTitulo se traduce via Tag en Traducir(); no hardcodear aquí
                Aplicar(lblDetalleTitulo, tConteo);

                MostrarOk(string.Format(fmtCargados, _pedidos.Count));
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void ColorearFilasPedidos()
        {
            foreach (DataGridViewRow row in dgvPedidos.Rows)
            {
                // Usar _EstadoKey (int del enum) en lugar del texto visible,
                // así el coloreado funciona independientemente del idioma activo.
                if (!dgvPedidos.Columns.Contains("_EstadoKey")) continue;
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
            RH("Motivo",   "col.ped.motivo");
        }

        private void DgvPedidos_SelectionChanged(object sender, EventArgs e)
        {
            bool hay = dgvPedidos.SelectedRows.Count > 0;
            dgvDetallePrendas.DataSource = null;

            if (!hay)
            {
                btnCancelar.Enabled    = false;
                btnDesCancelar.Enabled = false;
                if (_btnHistorial != null) _btnHistorial.Enabled = false;
                return;
            }

            var pedido = ObtenerPedidoSeleccionado();
            if (pedido == null) return;

            btnCancelar.Enabled    = pedido.Estado == BE.EstadoPedido.Pendiente;
            btnDesCancelar.Enabled = pedido.Estado == BE.EstadoPedido.Cancelado;
            if (_btnHistorial != null) _btnHistorial.Enabled = true;

            // Cargar detalle de prendas del pedido seleccionado
            CargarDetallePrendas(pedido.IdPedido);

            var tMotivo = Traductor.ObtenerTraducciones(_idioma);
            string T_sel(string k, string fb) => tMotivo.ContainsKey(k) ? tMotivo[k].Texto : fb;
            string motivoLabel = T_sel("lbl.motivo", "Motivo:");
            lblDetalleTitulo.Text = string.Format(
                T_sel("lbl.ped.seleccionado", "Pedido #{0} — {1} — {2}"),
                pedido.IdPedido, pedido.NombreCliente, EstadoLabel(pedido.Estado)) +
                (!string.IsNullOrEmpty(pedido.MotivoCancelacion)
                    ? $"  |  {motivoLabel} {pedido.MotivoCancelacion}" : "");
        }

        private void CargarDetallePrendas(int idPedido)
        {
            try
            {
                var pedidoCompleto = pedidoBLL.ObtenerPorId(idPedido);
                if (pedidoCompleto == null) return;

                var tD = Traductor.ObtenerTraducciones(_idioma);
                string TD(string k, string fb) => tD.ContainsKey(k) ? tD[k].Texto : fb;

                var tabla = new DataTable();
                tabla.Columns.Add(TD("col.prenda.nombre",    "Prenda"),    typeof(string));
                tabla.Columns.Add(TD("col.prenda.categoria", "Categoría"), typeof(string));
                tabla.Columns.Add(TD("col.prenda.talle",     "Talle"),     typeof(string));
                tabla.Columns.Add(TD("col.prenda.color",     "Color"),     typeof(string));
                tabla.Columns.Add(TD("col.prenda.estado",    "Estado"),    typeof(string));

                foreach (var p in pedidoCompleto.Prendas)
                    tabla.Rows.Add(p.Nombre, p.Categoria ?? "—",
                        p.Talle ?? "—", p.Color ?? "—", EstadoPrendaLabel(p.Estado));

                dgvDetallePrendas.DataSource = tabla;
            }
            catch (Exception ex) { System.Diagnostics.Trace.TraceError($"[PedidosVenta] Error al cargar detalle: {ex.Message}"); }
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

        // ── Eventos ───────────────────────────────────────────────────────────

        private void BtnNuevoPedido_Click(object sender, EventArgs e)
        {
            using (var form = new NuevoPedidoForm())
            {
                if (form.ShowDialog(this) != DialogResult.OK) return;

                var tCreado = Traductor.ObtenerTraducciones(_idioma);
                string fmtCreado = tCreado.ContainsKey("msg.ped.creado") ? tCreado["msg.ped.creado"].Texto : "Pedido #{0} creado. Estado: Pendiente.";
                MostrarOk(string.Format(fmtCreado, form.IdPedidoCreado));
                CargarPedidos();
            }
        }

        private void BtnCancelarPedido_Click(object sender, EventArgs e)
        {
            var pedido = ObtenerPedidoSeleccionado();
            if (pedido == null) return;

            // Pedir motivo de cancelación con un dialog inline
            var tCanc = Traductor.ObtenerTraducciones(_idioma);
            string T_c(string k, string fb) => tCanc.ContainsKey(k) ? tCanc[k].Texto : fb;

            string motivo = PedirTexto(
                $"{T_c("dlg.cancelped.titulo", "Motivo de Cancelación")} — Pedido #{pedido.IdPedido} ({pedido.NombreCliente}):",
                T_c("dlg.cancelped.titulo", "Motivo de Cancelación"));

            if (string.IsNullOrWhiteSpace(motivo))
            {
                MostrarError(T_c("msg.cancelped.req", "La cancelación requiere un motivo."));
                return;
            }

            string bodyCanc = string.Format(
                T_c("conf.cancelped.body", "¿Cancelar el Pedido #{0} de {1}?\n\nMotivo: {2}\n\nLas prendas volverán a estado Disponible."),
                pedido.IdPedido, pedido.NombreCliente, motivo);

            var confirmar = MessageBox.Show(
                bodyCanc,
                T_c("conf.cancelped.titulo", "Confirmar Cancelación"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (confirmar != DialogResult.Yes) return;

            try
            {
                pedidoBLL.Cancelar(this.Text, pedido, motivo);
                string fmtCancelado = T_c("msg.ped.cancelado", "Pedido #{0} cancelado. Prendas liberadas.");
                MostrarOk(string.Format(fmtCancelado, pedido.IdPedido));
                CargarPedidos();
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void BtnDesCancelarPedido_Click(object sender, EventArgs e)
        {
            var pedido = ObtenerPedidoSeleccionado();
            if (pedido == null) return;

            var tDesc = Traductor.ObtenerTraducciones(_idioma);
            string T_d(string k, string fb) => tDesc.ContainsKey(k) ? tDesc[k].Texto : fb;

            string bodyDesc = string.Format(
                T_d("conf.descancelar.body", "¿Des-cancelar el Pedido #{0} de {1}?\n\nSe verificará que las prendas originales estén disponibles\ny el pedido volverá a estado Pendiente."),
                pedido.IdPedido, pedido.NombreCliente);

            var confirmar = MessageBox.Show(
                bodyDesc,
                T_d("conf.descancelar.titulo", "Confirmar Des-cancelación"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);

            if (confirmar != DialogResult.Yes) return;

            try
            {
                pedidoBLL.DesCancelar(this.Text, pedido);
                string fmtReact = T_d("msg.ped.reactivado", "Pedido #{0} reactivado — volvió a Pendiente.");
                MostrarOk(string.Format(fmtReact, pedido.IdPedido));
                CargarPedidos();
            }
            catch (Exception ex)
            {
                MostrarError(ex);
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

                var tPedir = Traductor.ObtenerTraducciones(_idioma);
                string aceptarTxt  = tPedir.ContainsKey("btn.aceptar")  ? tPedir["btn.aceptar"].Texto  : "Aceptar";
                string cancelarTxt = tPedir.ContainsKey("btn.cancelar") ? tPedir["btn.cancelar"].Texto : "Cancelar";

                var btnOk = new Button
                {
                    Text         = aceptarTxt, Left = 220, Top = 84,
                    Width = 90, Height = 30,
                    DialogResult = DialogResult.OK,
                    BackColor    = Color.SteelBlue, ForeColor = Color.White,
                    FlatStyle    = FlatStyle.Flat
                };
                btnOk.FlatAppearance.BorderSize = 0;

                var btnCancel = new Button
                {
                    Text         = cancelarTxt, Left = 318, Top = 84,
                    Width = 90, Height = 30,
                    DialogResult = DialogResult.Cancel,
                    FlatStyle    = FlatStyle.Flat
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

        // ── Historial de cambios ──────────────────────────────────────────────

        private Button _btnHistorial;

        /// <summary>
        /// Agrega el botón "📋 Historial" a panelTop en tiempo de ejecución
        /// (el control no está en el Designer para no alterar el layout existente).
        /// </summary>
        private void AgregarBotonHistorial()
        {
            var tH = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
            string histText = tH.ContainsKey("btn.historial") ? tH["btn.historial"].Texto : "📋 Historial";
            _btnHistorial = new Button
            {
                Text      = histText,
                Size      = new Size(110, 28),
                BackColor = Color.FromArgb(100, 80, 160),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
                Enabled   = false
            };
            _btnHistorial.FlatAppearance.BorderSize = 0;
            // Botón entre btnRefrescar (x=412, w=32) y lblConteo (x=452).
            // Se inserta en x=452 y se desplaza lblConteo para que no solapen.
            _btnHistorial.Location = new Point(452, 11);
            lblConteo.Location     = new Point(570, lblConteo.Location.Y);
            _btnHistorial.Click   += BtnHistorial_Click;
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
