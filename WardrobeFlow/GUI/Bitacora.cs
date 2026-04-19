using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace GUI
{
    /// <summary>
    /// Capa de Presentación — Formulario de Gestión de Bitácora.
    ///
    /// Permite visualizar y buscar los registros de auditoría del sistema.
    /// Se abre como formulario hijo MDI dentro del Menú principal.
    ///
    /// FUNCIONALIDADES:
    ///   - Visualización de todos los registros ordenados por fecha (más recientes primero)
    ///   - Acceso rápido: "Últimos N días" con cantidad configurable por el usuario
    ///   - Búsqueda combinada por: rango de fechas, usuario (ID), actividad y criticidad
    ///   - Carga automática al abrir el formulario
    ///
    /// CUMPLE REQUISITOS T06a:
    ///   ✓ Registra fecha, hora, usuario, actividad, información asociada
    ///   ✓ Permite búsquedas por los datos almacenados de manera combinada
    ///   ✓ Trae los registros de los últimos N días (N definible por el usuario)
    /// </summary>
    public partial class Bitacora : Form
    {
        // BLL que provee los métodos de consulta de bitácora
        private readonly BLL.Bitacora bitacoraBLL = new BLL.Bitacora();

        // ── Controles de filtro rápido (últimos N días) ───────────────────────
        private NumericUpDown nudDias;
        private Button btnUltimosDias;

        // ── Controles de filtros combinados ──────────────────────────────────
        private DateTimePicker dtpDesde;
        private DateTimePicker dtpHasta;
        private CheckBox chkFiltrarFecha;
        private TextBox txtUsuario;
        private TextBox txtActividad;
        private ComboBox cmbCriticidad;
        private Button btnBuscar;
        private Button btnLimpiar;

        // ── Grilla y estado ───────────────────────────────────────────────────
        private DataGridView dgvBitacora;
        private Label lblResultados;

        /// <summary>
        /// Constructor: inicializa el formulario y construye la interfaz programáticamente.
        /// </summary>
        public Bitacora()
        {
            InitializeComponent();
            this.Text        = "Bitácora del Sistema";
            this.ClientSize  = new Size(980, 600);
            this.MinimumSize = new Size(820, 520);

            ConstruirInterfaz();

            // Cargar todos los registros al abrir el formulario
            this.Load += (s, e) => CargarRegistros();
        }

        /// <summary>
        /// Construye toda la interfaz del formulario programáticamente.
        /// El panel superior tiene dos zonas: acceso rápido (últimos N días) y filtros combinados.
        /// </summary>
        private void ConstruirInterfaz()
        {
            // ── Panel superior de filtros ─────────────────────────────────────
            Panel panelFiltros = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 120,
                BackColor = Color.FromArgb(240, 240, 240),
                Padding   = new Padding(8)
            };

            // ── Zona 1: Acceso rápido "Últimos N días" ────────────────────────
            var lblAccesoRapido = new Label
            {
                Text      = "Acceso rápido:",
                Left      = 10, Top = 10,
                Width     = 100,
                Font      = new Font(SystemFonts.DefaultFont, FontStyle.Bold)
            };

            var lblUltimos = new Label { Text = "Últimos", Left = 10, Top = 38, Width = 55 };

            // NumericUpDown para definir la cantidad de días (1 – 365)
            nudDias = new NumericUpDown
            {
                Left     = 68, Top    = 34,
                Width    = 55, Height = 24,
                Minimum  = 1,  Maximum = 365,
                Value    = 7           // Por defecto: última semana
            };

            var lblDias = new Label { Text = "días", Left = 128, Top = 38, Width = 35 };

            btnUltimosDias = new Button
            {
                Text      = "Ver",
                Left      = 166, Top = 33,
                Width     = 55,  Height = 26,
                BackColor = Color.FromArgb(70, 130, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnUltimosDias.FlatAppearance.BorderSize = 0;
            btnUltimosDias.Click += BtnUltimosDias_Click;

            // Separador visual entre las dos zonas
            var separador = new Label
            {
                Text      = "│",
                Left      = 235, Top = 10,
                Width     = 15,  Height = 80,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Gray
            };

            // ── Zona 2: Filtros combinados ────────────────────────────────────
            // Checkbox para activar filtro de fechas
            chkFiltrarFecha = new CheckBox
            {
                Text  = "Filtrar por fecha",
                Left  = 255, Top = 10,
                Width = 130
            };
            chkFiltrarFecha.CheckedChanged += (s, e) =>
            {
                dtpDesde.Enabled = chkFiltrarFecha.Checked;
                dtpHasta.Enabled = chkFiltrarFecha.Checked;
            };

            // Fecha Desde
            var lblDesde = new Label { Text = "Desde:", Left = 255, Top = 38, Width = 50 };
            dtpDesde = new DateTimePicker
            {
                Left    = 307, Top    = 34,
                Width   = 130,
                Format  = DateTimePickerFormat.Short,
                Enabled = false
            };

            // Fecha Hasta
            var lblHasta = new Label { Text = "Hasta:", Left = 448, Top = 38, Width = 45 };
            dtpHasta = new DateTimePicker
            {
                Left    = 496, Top    = 34,
                Width   = 130,
                Format  = DateTimePickerFormat.Short,
                Enabled = false
            };

            // Filtro por Usuario ID
            var lblUsuario = new Label { Text = "Usuario ID:", Left = 638, Top = 38, Width = 75 };
            txtUsuario = new TextBox { Left = 716, Top = 35, Width = 55, Text = "0" };

            // Filtro por Actividad (texto parcial, búsqueda LIKE)
            var lblActividad = new Label { Text = "Actividad:", Left = 255, Top = 80, Width = 70 };
            txtActividad = new TextBox { Left = 327, Top = 77, Width = 200 };

            // Filtro por Criticidad
            var lblCriticidad = new Label { Text = "Criticidad:", Left = 540, Top = 80, Width = 70 };
            cmbCriticidad = new ComboBox
            {
                Left          = 612, Top           = 77,
                Width         = 120,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbCriticidad.Items.AddRange(new object[] { "Todas", "None (0)", "Baja (1)", "Media (2)", "Alta (3)" });
            cmbCriticidad.SelectedIndex = 0;

            // Botón Buscar (filtros combinados)
            btnBuscar = new Button
            {
                Text      = "Buscar",
                Left      = 745, Top   = 75,
                Width     = 80,
                BackColor = Color.SteelBlue,
                ForeColor = Color.White
            };
            btnBuscar.Click += BtnBuscar_Click;

            // Botón Limpiar
            btnLimpiar = new Button { Text = "Limpiar", Left = 833, Top = 75, Width = 80 };
            btnLimpiar.Click += BtnLimpiar_Click;

            // Agregar todos los controles al panel
            panelFiltros.Controls.AddRange(new Control[]
            {
                lblAccesoRapido, lblUltimos, nudDias, lblDias, btnUltimosDias, separador,
                chkFiltrarFecha, lblDesde, dtpDesde, lblHasta, dtpHasta,
                lblUsuario, txtUsuario, lblActividad, txtActividad,
                lblCriticidad, cmbCriticidad, btnBuscar, btnLimpiar
            });

            // ── Label contador de resultados ──────────────────────────────────
            lblResultados = new Label
            {
                Dock      = DockStyle.Bottom,
                Height    = 24,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(4, 0, 0, 0),
                BackColor = Color.FromArgb(230, 230, 230)
            };

            // ── DataGridView de resultados ────────────────────────────────────
            dgvBitacora = new DataGridView
            {
                Dock                            = DockStyle.Fill,
                ReadOnly                        = true,
                AllowUserToAddRows              = false,
                AllowUserToDeleteRows           = false,
                SelectionMode                   = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode             = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor                 = Color.White,
                RowHeadersVisible               = false,
                BorderStyle                     = BorderStyle.None,
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(248, 248, 255)
                }
            };

            // Agregar controles al formulario (orden importa para el Dock layout)
            this.Controls.Add(dgvBitacora);
            this.Controls.Add(lblResultados);
            this.Controls.Add(panelFiltros);
        }

        /// <summary>
        /// Carga todos los registros de la bitácora sin filtros y los muestra en la grilla.
        /// </summary>
        private void CargarRegistros()
        {
            try
            {
                DataTable datos = bitacoraBLL.ObtenerTodos();
                MostrarEnGrilla(datos);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar la bitácora:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Evento del botón "Ver" en el acceso rápido.
        /// Consulta los registros de los últimos N días según el valor del NumericUpDown.
        /// Cumple el requisito de "traer los últimos N días definibles por el usuario".
        /// </summary>
        private void BtnUltimosDias_Click(object sender, EventArgs e)
        {
            try
            {
                int dias = (int)nudDias.Value;
                DataTable datos = bitacoraBLL.ObtenerUltimosNDias(dias);
                MostrarEnGrilla(datos, $"últimos {dias} días");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar registros:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Evento del botón Buscar: aplica los filtros combinados seleccionados.
        /// Implementa la búsqueda combinada requerida por T06a.
        /// </summary>
        private void BtnBuscar_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime? desde     = chkFiltrarFecha.Checked ? dtpDesde.Value : (DateTime?)null;
                DateTime? hasta     = chkFiltrarFecha.Checked ? dtpHasta.Value : (DateTime?)null;
                int    usuarioId    = int.TryParse(txtUsuario.Text, out int uid) ? uid : 0;
                string actividad    = txtActividad.Text.Trim();
                int    criticidad   = cmbCriticidad.SelectedIndex; // 0=Todas, 1=None(0), 2=Baja(1)...

                DataTable datos = bitacoraBLL.BuscarPorFiltros(desde, hasta, usuarioId, actividad, criticidad);
                MostrarEnGrilla(datos);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en la búsqueda:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Limpia todos los filtros y recarga todos los registros.
        /// </summary>
        private void BtnLimpiar_Click(object sender, EventArgs e)
        {
            chkFiltrarFecha.Checked     = false;
            dtpDesde.Value              = DateTime.Today;
            dtpHasta.Value              = DateTime.Today;
            txtUsuario.Text             = "0";
            txtActividad.Clear();
            cmbCriticidad.SelectedIndex = 0;
            nudDias.Value               = 7;

            CargarRegistros();
        }

        /// <summary>
        /// Asigna un DataTable a la grilla y actualiza el contador de resultados.
        /// </summary>
        /// <param name="datos">DataTable con los registros a mostrar.</param>
        /// <param name="contexto">Descripción opcional del filtro aplicado (para el label).</param>
        private void MostrarEnGrilla(DataTable datos, string contexto = null)
        {
            dgvBitacora.DataSource = datos;

            string label = $"  {datos.Rows.Count} registro(s) encontrado(s)";
            if (!string.IsNullOrEmpty(contexto))
                label += $"  —  {contexto}";

            lblResultados.Text = label;
        }
    }
}
