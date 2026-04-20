using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace GUI
{
    /// <summary>
    /// Capa de Presentación — Módulo de Auditoría (Bitácora).
    ///
    /// Presenta dos pestañas:
    ///   Tab 1 — Sistema    : eventos de seguridad (login, logout, resets, intentos fallidos)
    ///   Tab 2 — Negocio    : eventos de negocio (ventas, despachos, stock, clientes)
    ///
    /// Accesible para Administrador (mnuAuditoria) y Supervisor (mnuAuditoria).
    /// </summary>
    public partial class Bitacora : Form
    {
        private readonly BLL.Bitacora        bllSistema  = new BLL.Bitacora();
        private readonly BLL.BitacoraNegocio bllNegocio  = new BLL.BitacoraNegocio();

        // ── Controles compartidos ─────────────────────────────────────────────
        private TabControl tabControl;

        // ── Tab Sistema ───────────────────────────────────────────────────────
        private DataGridView dgvSistema;
        private NumericUpDown nudDias;
        private Button       btnUltimosDias;
        private DateTimePicker dtpDesde, dtpHasta;
        private CheckBox     chkFiltrarFecha;
        private TextBox      txtUsuario, txtActividad;
        private ComboBox     cmbCriticidad;
        private Button       btnBuscar, btnLimpiar;
        private Label        lblResultadosSistema;

        // ── Tab Negocio ───────────────────────────────────────────────────────
        private DataGridView dgvNegocio;
        private DateTimePicker dtpNegDesde, dtpNegHasta;
        private CheckBox     chkNegFecha;
        private ComboBox     cmbTipoEvento;
        private TextBox      txtNegPedido, txtNegCliente;
        private Button       btnNegBuscar, btnNegLimpiar;
        private Label        lblResultadosNegocio;

        public Bitacora()
        {
            InitializeComponent();
            this.Text        = "Auditoría — Bitácoras del Sistema";
            this.ClientSize  = new Size(1020, 640);
            this.MinimumSize = new Size(860, 540);

            ConstruirInterfaz();
            this.Load += (s, e) =>
            {
                CargarSistema();
                CargarNegocio();
            };
        }

        private void ConstruirInterfaz()
        {
            tabControl = new TabControl { Dock = DockStyle.Fill };

            tabControl.TabPages.Add(ConstruirTabSistema());
            tabControl.TabPages.Add(ConstruirTabNegocio());

            this.Controls.Add(tabControl);
        }

        // ══════════════════════════════════════════════════════════════════════
        // TAB SISTEMA
        // ══════════════════════════════════════════════════════════════════════

        private TabPage ConstruirTabSistema()
        {
            var tab = new TabPage("🔐  Bitácora del Sistema");

            // ── Panel de filtros ──────────────────────────────────────────────
            Panel panelFiltros = new Panel
            {
                Dock = DockStyle.Top, Height = 120,
                BackColor = Color.FromArgb(240, 240, 240),
                Padding = new Padding(8)
            };

            // Acceso rápido
            panelFiltros.Controls.Add(new Label
                { Text = "Acceso rápido:", Left = 10, Top = 10, Width = 100,
                  Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold) });
            panelFiltros.Controls.Add(new Label { Text = "Últimos", Left = 10, Top = 38, Width = 55 });

            nudDias = new NumericUpDown
                { Left = 68, Top = 34, Width = 55, Minimum = 1, Maximum = 365, Value = 7 };
            panelFiltros.Controls.Add(nudDias);
            panelFiltros.Controls.Add(new Label { Text = "días", Left = 128, Top = 38, Width = 35 });

            btnUltimosDias = new Button
            {
                Text = "Ver", Left = 166, Top = 33, Width = 55, Height = 26,
                BackColor = Color.SteelBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat
            };
            btnUltimosDias.FlatAppearance.BorderSize = 0;
            btnUltimosDias.Click += (s, e) =>
            {
                var dt = bllSistema.ObtenerUltimosNDias((int)nudDias.Value);
                MostrarEnGrilla(dgvSistema, lblResultadosSistema, dt, $"últimos {(int)nudDias.Value} días");
            };
            panelFiltros.Controls.Add(btnUltimosDias);

            panelFiltros.Controls.Add(new Label
                { Text = "│", Left = 235, Top = 10, Width = 15, Height = 80,
                  TextAlign = System.Drawing.ContentAlignment.MiddleCenter, ForeColor = Color.Gray });

            // Filtros combinados
            chkFiltrarFecha = new CheckBox { Text = "Filtrar por fecha", Left = 255, Top = 10, Width = 130 };
            chkFiltrarFecha.CheckedChanged += (s, e) =>
            {
                dtpDesde.Enabled = chkFiltrarFecha.Checked;
                dtpHasta.Enabled = chkFiltrarFecha.Checked;
            };
            panelFiltros.Controls.Add(chkFiltrarFecha);

            panelFiltros.Controls.Add(new Label { Text = "Desde:", Left = 255, Top = 38, Width = 50 });
            dtpDesde = new DateTimePicker { Left = 307, Top = 34, Width = 130, Format = DateTimePickerFormat.Short, Enabled = false };
            panelFiltros.Controls.Add(dtpDesde);

            panelFiltros.Controls.Add(new Label { Text = "Hasta:", Left = 448, Top = 38, Width = 45 });
            dtpHasta = new DateTimePicker { Left = 496, Top = 34, Width = 130, Format = DateTimePickerFormat.Short, Enabled = false };
            panelFiltros.Controls.Add(dtpHasta);

            panelFiltros.Controls.Add(new Label { Text = "Usuario ID:", Left = 638, Top = 38, Width = 75 });
            txtUsuario = new TextBox { Left = 716, Top = 35, Width = 55, Text = "0" };
            panelFiltros.Controls.Add(txtUsuario);

            panelFiltros.Controls.Add(new Label { Text = "Actividad:", Left = 255, Top = 80, Width = 70 });
            txtActividad = new TextBox { Left = 327, Top = 77, Width = 200 };
            panelFiltros.Controls.Add(txtActividad);

            panelFiltros.Controls.Add(new Label { Text = "Criticidad:", Left = 540, Top = 80, Width = 70 });
            cmbCriticidad = new ComboBox
            {
                Left = 612, Top = 77, Width = 120, DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbCriticidad.Items.AddRange(new object[] { "Todas", "None", "Baja", "Media", "Alta" });
            cmbCriticidad.SelectedIndex = 0;
            panelFiltros.Controls.Add(cmbCriticidad);

            btnBuscar = new Button
            {
                Text = "Buscar", Left = 745, Top = 75, Width = 80,
                BackColor = Color.SteelBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat
            };
            btnBuscar.FlatAppearance.BorderSize = 0;
            btnBuscar.Click += BtnBuscarSistema_Click;
            panelFiltros.Controls.Add(btnBuscar);

            btnLimpiar = new Button { Text = "Limpiar", Left = 833, Top = 75, Width = 80 };
            btnLimpiar.Click += (s, e) =>
            {
                chkFiltrarFecha.Checked = false; txtUsuario.Text = "0";
                txtActividad.Clear(); cmbCriticidad.SelectedIndex = 0; nudDias.Value = 7;
                CargarSistema();
            };
            panelFiltros.Controls.Add(btnLimpiar);

            // Grilla
            dgvSistema = CrearDgv();

            lblResultadosSistema = new Label
            {
                Dock = DockStyle.Bottom, Height = 24,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Padding = new Padding(4, 0, 0, 0),
                BackColor = Color.FromArgb(230, 230, 230)
            };

            tab.Controls.Add(dgvSistema);
            tab.Controls.Add(lblResultadosSistema);
            tab.Controls.Add(panelFiltros);
            return tab;
        }

        // ══════════════════════════════════════════════════════════════════════
        // TAB NEGOCIO
        // ══════════════════════════════════════════════════════════════════════

        private TabPage ConstruirTabNegocio()
        {
            var tab = new TabPage("📦  Bitácora de Negocio");

            Panel panelFiltros = new Panel
            {
                Dock = DockStyle.Top, Height = 80,
                BackColor = Color.FromArgb(238, 245, 238),
                Padding = new Padding(8)
            };

            chkNegFecha = new CheckBox { Text = "Filtrar por fecha", Left = 8, Top = 10, Width = 130 };
            chkNegFecha.CheckedChanged += (s, e) =>
            {
                dtpNegDesde.Enabled = chkNegFecha.Checked;
                dtpNegHasta.Enabled = chkNegFecha.Checked;
            };
            panelFiltros.Controls.Add(chkNegFecha);

            panelFiltros.Controls.Add(new Label { Text = "Desde:", Left = 8, Top = 40, Width = 50 });
            dtpNegDesde = new DateTimePicker { Left = 60, Top = 37, Width = 120, Format = DateTimePickerFormat.Short, Enabled = false };
            panelFiltros.Controls.Add(dtpNegDesde);

            panelFiltros.Controls.Add(new Label { Text = "Hasta:", Left = 190, Top = 40, Width = 45 });
            dtpNegHasta = new DateTimePicker { Left = 238, Top = 37, Width = 120, Format = DateTimePickerFormat.Short, Enabled = false };
            panelFiltros.Controls.Add(dtpNegHasta);

            panelFiltros.Controls.Add(new Label { Text = "Tipo:", Left = 370, Top = 12, Width = 40 });
            cmbTipoEvento = new ComboBox
            {
                Left = 412, Top = 9, Width = 160, DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbTipoEvento.Items.AddRange(new object[]
            {
                "Todos", "Venta", "Cancelacion", "Despacho", "Entrega",
                "AltaPrenda", "CambioEstadoPrenda", "AltaCliente", "BajaCliente"
            });
            cmbTipoEvento.SelectedIndex = 0;
            panelFiltros.Controls.Add(cmbTipoEvento);

            panelFiltros.Controls.Add(new Label { Text = "ID Pedido:", Left = 370, Top = 42, Width = 70 });
            txtNegPedido = new TextBox { Left = 442, Top = 39, Width = 60, Text = "0" };
            panelFiltros.Controls.Add(txtNegPedido);

            panelFiltros.Controls.Add(new Label { Text = "ID Cliente:", Left = 514, Top = 42, Width = 70 });
            txtNegCliente = new TextBox { Left = 586, Top = 39, Width = 60, Text = "0" };
            panelFiltros.Controls.Add(txtNegCliente);

            btnNegBuscar = new Button
            {
                Text = "Buscar", Left = 660, Top = 9, Width = 80, Height = 28,
                BackColor = Color.FromArgb(60, 140, 60), ForeColor = Color.White, FlatStyle = FlatStyle.Flat
            };
            btnNegBuscar.FlatAppearance.BorderSize = 0;
            btnNegBuscar.Click += BtnBuscarNegocio_Click;
            panelFiltros.Controls.Add(btnNegBuscar);

            btnNegLimpiar = new Button { Text = "Limpiar", Left = 748, Top = 9, Width = 80, Height = 28 };
            btnNegLimpiar.Click += (s, e) =>
            {
                chkNegFecha.Checked = false; cmbTipoEvento.SelectedIndex = 0;
                txtNegPedido.Text = "0"; txtNegCliente.Text = "0";
                CargarNegocio();
            };
            panelFiltros.Controls.Add(btnNegLimpiar);

            dgvNegocio = CrearDgv();

            lblResultadosNegocio = new Label
            {
                Dock = DockStyle.Bottom, Height = 24,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Padding = new Padding(4, 0, 0, 0),
                BackColor = Color.FromArgb(220, 235, 220)
            };

            tab.Controls.Add(dgvNegocio);
            tab.Controls.Add(lblResultadosNegocio);
            tab.Controls.Add(panelFiltros);
            return tab;
        }

        // ── Carga ─────────────────────────────────────────────────────────────

        private void CargarSistema()
        {
            try
            {
                var dt = bllSistema.ObtenerTodos();
                MostrarEnGrilla(dgvSistema, lblResultadosSistema, dt);
            }
            catch (Exception ex) { MostrarError(ex.Message); }
        }

        private void CargarNegocio()
        {
            try
            {
                var dt = bllNegocio.ObtenerTodos();
                MostrarEnGrilla(dgvNegocio, lblResultadosNegocio, dt);
            }
            catch (Exception ex) { MostrarError(ex.Message); }
        }

        // ── Eventos ───────────────────────────────────────────────────────────

        private void BtnBuscarSistema_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime? desde    = chkFiltrarFecha.Checked ? dtpDesde.Value  : (DateTime?)null;
                DateTime? hasta    = chkFiltrarFecha.Checked ? dtpHasta.Value  : (DateTime?)null;
                int uid            = int.TryParse(txtUsuario.Text, out int u) ? u : 0;
                string actividad   = txtActividad.Text.Trim();
                int criticidad     = cmbCriticidad.SelectedIndex;

                var dt = bllSistema.BuscarPorFiltros(desde, hasta, uid, actividad, criticidad);
                MostrarEnGrilla(dgvSistema, lblResultadosSistema, dt);
            }
            catch (Exception ex) { MostrarError(ex.Message); }
        }

        private void BtnBuscarNegocio_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime? desde   = chkNegFecha.Checked ? dtpNegDesde.Value : (DateTime?)null;
                DateTime? hasta   = chkNegFecha.Checked ? dtpNegHasta.Value : (DateTime?)null;
                string tipo       = cmbTipoEvento.SelectedIndex == 0 ? null : cmbTipoEvento.SelectedItem.ToString();
                int? idPedido     = int.TryParse(txtNegPedido.Text, out int p) && p > 0 ? (int?)p : null;
                int? idCliente    = int.TryParse(txtNegCliente.Text, out int c) && c > 0 ? (int?)c : null;

                var dt = bllNegocio.BuscarPorFiltros(desde, hasta, tipo, idCliente, idPedido);
                MostrarEnGrilla(dgvNegocio, lblResultadosNegocio, dt);
            }
            catch (Exception ex) { MostrarError(ex.Message); }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private DataGridView CrearDgv()
        {
            return new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.None,
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(248, 248, 255)
                }
            };
        }

        private void MostrarEnGrilla(DataGridView dgv, Label lbl, DataTable datos, string contexto = null)
        {
            dgv.DataSource = datos;
            string texto = $"  {datos.Rows.Count} registro(s)";
            if (!string.IsNullOrEmpty(contexto)) texto += $"  —  {contexto}";
            lbl.Text = texto;
        }

        private void MostrarError(string msg)
        {
            MessageBox.Show($"Error: {msg}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
