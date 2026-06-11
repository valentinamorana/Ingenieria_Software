using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Servicios.Multiidioma;

namespace GUI
{
    /// <summary>
    /// Historial de cambios de un Pedido — T06b.
    ///
    /// Muestra todos los eventos registrados sobre el pedido (quién, cuándo, qué campo,
    /// valor anterior y valor nuevo). Permite filtrar por fecha y por tipo de acción,
    /// y restaurar el pedido al estado previo a cualquier operación registrada.
    ///
    /// Accesible desde PedidosVenta y PedidosRealizados mediante un botón "📋 Historial".
    /// </summary>
    public class PedidoHistorialForm : FormBase, IIdiomaObserver
    {
        protected override Label MensajeLabel => _lblMensaje;

        // ── Dependencias ──────────────────────────────────────────────────────
        private readonly BLL.Interfaces.IPedidoService _pedidoBLL = new BLL.Pedido();
        private readonly int        _idPedido;

        // ── Estado interno ────────────────────────────────────────────────────
        private Idioma _idioma = GestorIdioma.IdiomaActual;

        // ── Controles generados en código ─────────────────────────────────────
        private Label            _lblPedidoInfo;
        private Label            _lblMensaje;

        // Panel de filtros
        private GroupBox         _grpFiltros;
        private Label            _lblDesde;
        private DateTimePicker   _dtpDesde;
        private Label            _lblHasta;
        private DateTimePicker   _dtpHasta;
        private Label            _lblAccion;
        private ComboBox         _cmbAccion;
        private CheckBox         _chkDesde;
        private CheckBox         _chkHasta;
        private Button           _btnBuscar;

        // Grilla
        private DataGridView     _dgv;

        // Botones de acción
        private Button           _btnRestaurar;
        private Button           _btnCerrar;

        // ── Constructor ───────────────────────────────────────────────────────

        public PedidoHistorialForm(int idPedido)
        {
            _idPedido = idPedido;
            ConstruirUI();
        }

        // ── Ciclo de vida ─────────────────────────────────────────────────────

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GestorIdioma.SuscribirObservador(this);
            Traducir(_idioma);
            Buscar();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            GestorIdioma.DesuscribirObservador(this);
            base.OnFormClosing(e);
        }

        // ── IIdiomaObserver ───────────────────────────────────────────────────

        public void UpdateLanguage(Idioma idioma)
        {
            Traducir(idioma);
        }

        // ── Traducción ────────────────────────────────────────────────────────

        private void Traducir(Idioma idioma)
        {
            _idioma = idioma;   // sincronizar antes de llamar a TraducirHeadersGrilla()
            var t = Traductor.ObtenerTraducciones(idioma);

            string T(string clave, string fallback) =>
                t.ContainsKey(clave) ? t[clave].Texto : fallback;

            this.Text              = T("frm.historial",      "Historial de Cambios — Pedido");
            _grpFiltros.Text       = T("lbl.hist.filtros",   "Filtros");
            _lblDesde.Text         = T("lbl.hist.desde",     "Desde:");
            _lblHasta.Text         = T("lbl.hist.hasta",     "Hasta:");
            _lblAccion.Text        = T("lbl.hist.accion",    "Acción:");
            _btnBuscar.Text        = T("btn.hist.buscar",    "🔍 Buscar");
            _btnRestaurar.Text     = T("btn.hist.restaurar", "⟲ Restaurar");
            _btnCerrar.Text        = T("btn.hist.cerrar",    "Cerrar");
            _lblPedidoInfo.Text    = T("lbl.hist.pedido",    "Pedido #") + _idPedido;

            RellenarComboAcciones(idioma);
            TraducirHeadersGrilla();
        }

        private void RellenarComboAcciones(Idioma idioma)
        {
            // Opciones fijas de acción — incluye "Todas" como opción vacía
            var t = Traductor.ObtenerTraducciones(idioma);

            string T(string clave, string fallback) =>
                t.ContainsKey(clave) ? t[clave].Texto : fallback;

            var items = new List<ComboItem>
            {
                new ComboItem("",           T("combo.hist.todas",      "— Todas —")),
                new ComboItem("CREAR",      T("accion.crear",          "Crear")),
                new ComboItem("DESPACHAR",  T("accion.despachar",      "Despachar")),
                new ComboItem("ENTREGAR",   T("accion.entregar",       "Entregar")),
                new ComboItem("CANCELAR",   T("accion.cancelar",       "Cancelar")),
                new ComboItem("DESCANCELAR",T("accion.descancelar",    "Des-cancelar")),
                new ComboItem("DEVOLUCION", T("accion.devolucion",     "Devolución")),
                new ComboItem("RESTAURAR",  T("accion.restaurar",      "Restaurar")),
            };

            string prevValue = (_cmbAccion.SelectedItem as ComboItem)?.Value ?? "";
            _cmbAccion.DataSource    = null;
            _cmbAccion.DisplayMember = "Label";
            _cmbAccion.ValueMember   = "Value";
            _cmbAccion.DataSource    = items;

            // Restaurar selección previa
            foreach (ComboItem it in items)
                if (it.Value == prevValue) { _cmbAccion.SelectedItem = it; break; }
        }

        private void TraducirHeadersGrilla()
        {
            var t = Traductor.ObtenerTraducciones(_idioma);
            void RH(string col, string clave, string fallback)
            {
                if (_dgv.Columns.Contains(col) && t.ContainsKey(clave))
                    _dgv.Columns[col].HeaderText = t[clave].Texto;
                else if (_dgv.Columns.Contains(col))
                    _dgv.Columns[col].HeaderText = fallback;
            }

            RH("IdOperacion",   "col.hist.op",       "Op.");
            RH("Fecha",         "col.hist.fecha",     "Fecha");
            RH("NombreUsuario", "col.hist.usuario",   "Usuario");
            RH("Accion",        "col.hist.accion",    "Acción");
            RH("Campo",         "col.hist.campo",     "Campo");
            RH("ValorAnterior", "col.hist.anterior",  "Valor Anterior");
            RH("ValorNuevo",    "col.hist.nuevo",     "Valor Nuevo");
        }

        // ── Búsqueda ──────────────────────────────────────────────────────────

        private void Buscar()
        {
            try
            {
                string accion = (_cmbAccion.SelectedItem as ComboItem)?.Value;
                if (string.IsNullOrEmpty(accion)) accion = null;

                DateTime? desde = _chkDesde.Checked ? (DateTime?)_dtpDesde.Value.Date : null;
                DateTime? hasta = _chkHasta.Checked ? (DateTime?)_dtpHasta.Value.Date : null;

                DataTable dt = _pedidoBLL.ObtenerHistorial(_idPedido, accion, desde, hasta);

                _dgv.DataSource = dt;
                ConfigurarColumnas();
                TraducirHeadersGrilla();

                var tb = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
                MostrarOk(string.Format(tb.ContainsKey("pedidos.hist.encontrados") ? tb["pedidos.hist.encontrados"].Texto : "{0} registro(s) encontrado(s).", dt.Rows.Count));
                _btnRestaurar.Enabled = false;
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void ConfigurarColumnas()
        {
            if (!_dgv.Columns.Contains("IdHistorial")) return;

            // Ocultar columna PK interna
            _dgv.Columns["IdHistorial"].Visible = false;

            // Anchos razonables
            void W(string col, int w)
            {
                if (_dgv.Columns.Contains(col)) _dgv.Columns[col].Width = w;
            }
            W("IdOperacion",   44);
            W("Fecha",        145);
            W("NombreUsuario", 90);
            W("Accion",        95);
            W("Campo",         95);
            W("ValorAnterior",160);
            W("ValorNuevo",   160);
        }

        // ── Restaurar ─────────────────────────────────────────────────────────

        private void Restaurar()
        {
            if (_dgv.SelectedRows.Count == 0)
            {
                var tHR = Traductor.ObtenerTraducciones(_idioma);
                MostrarError(tHR.ContainsKey("err.hist.restaurar") ? tHR["err.hist.restaurar"].Texto : "Seleccioná una fila del historial para restaurar.");
                return;
            }

            DataRow row = (_dgv.SelectedRows[0].DataBoundItem as DataRowView)?.Row;
            if (row == null) return;

            int idOperacion = Convert.ToInt32(row["IdOperacion"]);
            string accion   = row["Accion"].ToString();

            var tH = Traductor.ObtenerTraducciones(_idioma);
            string TH(string k, string fb) => tH.ContainsKey(k) ? tH[k].Texto : fb;

            string tpl = TH("conf.hist.restaurar.msg",
                "¿Restaurar el pedido #{0} al estado anterior a '{1}' (op. #{2})?\n\n⚠ Nota: esta operación modifica el estado del Pedido en la base de datos.\nEl estado de las Prendas asociadas NO se revierte automáticamente.\n\n¿Confirmar?");
            string advertencia = string.Format(tpl, _idPedido, accion, idOperacion);

            if (MessageBox.Show(advertencia, TH("msg.backup.titulorestaura", "Confirmar Restauración"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                return;

            try
            {
                _pedidoBLL.RestaurarOperacion(this.Text, _idPedido, idOperacion);
                MostrarOk(string.Format(TH("msg.hist.restaurado", "Pedido #{0} restaurado correctamente."), _idPedido));
                Buscar();   // Recargar historial para ver el evento RESTAURAR
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        // ── Eventos ───────────────────────────────────────────────────────────

        private void BtnBuscar_Click(object sender, EventArgs e)     => Buscar();
        private void BtnRestaurar_Click(object sender, EventArgs e)  => Restaurar();
        private void BtnCerrar_Click(object sender, EventArgs e)     => this.Close();

        private void Dgv_SelectionChanged(object sender, EventArgs e)
        {
            _btnRestaurar.Enabled = _dgv.SelectedRows.Count > 0;
        }

        private void ChkDesde_CheckedChanged(object sender, EventArgs e)
            => _dtpDesde.Enabled = _chkDesde.Checked;

        private void ChkHasta_CheckedChanged(object sender, EventArgs e)
            => _dtpHasta.Enabled = _chkHasta.Checked;

        // ── Construcción de UI en código ──────────────────────────────────────

        private void ConstruirUI()
        {
            this.Text            = "Historial de Cambios — Pedido";
            this.Size            = new Size(860, 560);
            this.MinimumSize     = new Size(760, 460);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.BackColor       = Color.White;

            // ── Encabezado ─────────────────────────────────────────────────────
            _lblPedidoInfo = new Label
            {
                Text      = $"Pedido #{_idPedido}",
                Font      = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.FromArgb(176, 62, 96),
                AutoSize  = true,
                Location  = new Point(12, 12)
            };

            _lblMensaje = new Label
            {
                AutoSize  = false,
                Size      = new Size(820, 20),
                Location  = new Point(12, 40),
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = Color.DimGray
            };

            // ── Grupo de filtros ───────────────────────────────────────────────
            _grpFiltros = new GroupBox
            {
                Text     = "Filtros",
                Location = new Point(12, 68),
                Size     = new Size(820, 64),
                Font     = new Font("Segoe UI", 8.5f)
            };

            _chkDesde = new CheckBox { Text = "", Location = new Point(8, 30),   Size = new Size(20, 20), Checked = false };
            _lblDesde = new Label    { Text = "Desde:", Location = new Point(28, 32),  AutoSize = true };
            _dtpDesde = new DateTimePicker
            {
                Format   = DateTimePickerFormat.Short,
                Value    = DateTime.Today.AddMonths(-1),
                Location = new Point(80, 28),
                Size     = new Size(105, 22),
                Enabled  = false
            };

            _chkHasta = new CheckBox { Text = "", Location = new Point(200, 30),  Size = new Size(20, 20), Checked = false };
            _lblHasta = new Label    { Text = "Hasta:", Location = new Point(220, 32), AutoSize = true };
            _dtpHasta = new DateTimePicker
            {
                Format   = DateTimePickerFormat.Short,
                Value    = DateTime.Today,
                Location = new Point(272, 28),
                Size     = new Size(105, 22),
                Enabled  = false
            };

            _lblAccion = new Label { Text = "Acción:", Location = new Point(394, 32), AutoSize = true };
            _cmbAccion = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location      = new Point(446, 28),
                Size          = new Size(148, 22),
                Font          = new Font("Segoe UI", 8.5f)
            };

            _btnBuscar = new Button
            {
                Text      = "🔍 Buscar",
                Location  = new Point(610, 26),
                Size      = new Size(100, 28),
                BackColor = Color.FromArgb(176, 62, 96),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            _btnBuscar.FlatAppearance.BorderSize = 0;

            _grpFiltros.Controls.AddRange(new Control[]
            {
                _chkDesde, _lblDesde, _dtpDesde,
                _chkHasta, _lblHasta, _dtpHasta,
                _lblAccion, _cmbAccion, _btnBuscar
            });

            // ── Grilla ─────────────────────────────────────────────────────────
            _dgv = new DataGridView
            {
                Location              = new Point(12, 142),
                Size                  = new Size(820, 320),
                Anchor                = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly              = true,
                AllowUserToAddRows    = false,
                SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect           = false,
                AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.None,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight   = 28,
                RowHeadersVisible     = false,
                BorderStyle           = BorderStyle.None,
                BackgroundColor       = Color.WhiteSmoke,
                Font                  = new Font("Segoe UI", 8.5f),
                GridColor             = Color.FromArgb(220, 220, 220)
            };
            _dgv.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            _dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(176, 62, 96);
            _dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _dgv.EnableHeadersVisualStyles               = false;
            _dgv.DefaultCellStyle.SelectionBackColor     = Color.FromArgb(255, 182, 193);
            _dgv.DefaultCellStyle.SelectionForeColor     = Color.Black;
            _dgv.DefaultCellStyle.SelectionForeColor     = Color.Black;

            // ── Botones de acción ──────────────────────────────────────────────
            _btnRestaurar = new Button
            {
                Text      = "⟲ Restaurar",
                Location  = new Point(12, 472),
                Size      = new Size(130, 32),
                Anchor    = AnchorStyles.Bottom | AnchorStyles.Left,
                BackColor = Color.FromArgb(210, 100, 135),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor    = Cursors.Hand,
                Enabled   = false
            };
            _btnRestaurar.FlatAppearance.BorderSize = 0;

            _btnCerrar = new Button
            {
                Text      = "Cerrar",
                Location  = new Point(714, 472),
                Size      = new Size(118, 32),
                Anchor    = AnchorStyles.Bottom | AnchorStyles.Right,
                BackColor = Color.FromArgb(210, 210, 210),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9f),
                Cursor    = Cursors.Hand
            };
            _btnCerrar.FlatAppearance.BorderSize = 0;

            // ── Suscribir eventos ──────────────────────────────────────────────
            _btnBuscar.Click              += BtnBuscar_Click;
            _btnRestaurar.Click           += BtnRestaurar_Click;
            _btnCerrar.Click              += BtnCerrar_Click;
            _dgv.SelectionChanged         += Dgv_SelectionChanged;
            _chkDesde.CheckedChanged      += ChkDesde_CheckedChanged;
            _chkHasta.CheckedChanged      += ChkHasta_CheckedChanged;

            // ── Agregar controles al formulario ───────────────────────────────
            this.Controls.AddRange(new Control[]
            {
                _lblPedidoInfo, _lblMensaje,
                _grpFiltros,
                _dgv,
                _btnRestaurar, _btnCerrar
            });
        }

        // ── Helper interno ────────────────────────────────────────────────────

        /// <summary>Par clave/label para el ComboBox de acciones.</summary>
        private class ComboItem
        {
            public string Value { get; }
            public string Label { get; }
            public ComboItem(string value, string label) { Value = value; Label = label; }
            public override string ToString() => Label;
        }
    }
}
