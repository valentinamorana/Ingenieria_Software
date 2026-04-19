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
    ///   - Búsqueda combinada por: rango de fechas, usuario (ID), actividad y criticidad
    ///   - Carga automática al abrir el formulario
    ///
    /// CUMPLE REQUISITOS T06a:
    ///   ✓ Registra fecha, hora, usuario, actividad, información asociada
    ///   ✓ Permite búsquedas por los datos almacenados de manera combinada
    /// </summary>
    public partial class Bitacora : Form
    {
        // BLL que provee los métodos de consulta de bitácora
        private readonly BLL.Bitacora bitacoraBLL = new BLL.Bitacora();

        // ── Controles de filtros ──────────────────────────────────────────────
        private DateTimePicker dtpDesde;
        private DateTimePicker dtpHasta;
        private CheckBox chkFiltrarFecha;
        private TextBox txtUsuario;
        private TextBox txtActividad;
        private ComboBox cmbCriticidad;
        private Button btnBuscar;
        private Button btnLimpiar;
        private DataGridView dgvBitacora;
        private Label lblResultados;

        /// <summary>
        /// Constructor: inicializa el formulario y construye la interfaz programáticamente.
        /// </summary>
        public Bitacora()
        {
            InitializeComponent();
            this.Text        = "Bitácora del Sistema";
            this.ClientSize  = new Size(960, 560);
            this.MinimumSize = new Size(800, 480);

            ConstruirInterfaz();

            // Cargar todos los registros al abrir el formulario
            this.Load += (s, e) => CargarRegistros();
        }

        /// <summary>
        /// Construye toda la interfaz del formulario programáticamente,
        /// incluyendo el panel de filtros y la grilla de resultados.
        /// </summary>
        private void ConstruirInterfaz()
        {
            // ── Panel superior de filtros ─────────────────────────────────────
            Panel panelFiltros = new Panel
            {
                Dock        = DockStyle.Top,
                Height      = 110,
                BackColor   = Color.FromArgb(240, 240, 240),
                Padding     = new Padding(8)
            };

            // Checkbox para activar filtro de fechas
            chkFiltrarFecha = new CheckBox { Text = "Filtrar por fecha", Left = 10, Top = 10, Width = 130 };
            chkFiltrarFecha.CheckedChanged += (s, e) =>
            {
                dtpDesde.Enabled = chkFiltrarFecha.Checked;
                dtpHasta.Enabled = chkFiltrarFecha.Checked;
            };

            // Fecha Desde
            var lblDesde = new Label { Text = "Desde:", Left = 10, Top = 38, Width = 50 };
            dtpDesde = new DateTimePicker { Left = 62, Top = 34, Width = 150, Format = DateTimePickerFormat.Short, Enabled = false };

            // Fecha Hasta
            var lblHasta = new Label { Text = "Hasta:", Left = 225, Top = 38, Width = 50 };
            dtpHasta = new DateTimePicker { Left = 277, Top = 34, Width = 150, Format = DateTimePickerFormat.Short, Enabled = false };

            // Filtro por Usuario (ID)
            var lblUsuario = new Label { Text = "Usuario ID:", Left = 440, Top = 38, Width = 75 };
            txtUsuario = new TextBox { Left = 518, Top = 35, Width = 60, Text = "0" };

            // Filtro por Actividad (texto parcial)
            var lblActividad = new Label { Text = "Actividad:", Left = 10, Top = 75, Width = 70 };
            txtActividad = new TextBox { Left = 82, Top = 72, Width = 200 };

            // Filtro por Criticidad
            var lblCriticidad = new Label { Text = "Criticidad:", Left = 296, Top = 75, Width = 70 };
            cmbCriticidad = new ComboBox { Left = 368, Top = 72, Width = 130, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCriticidad.Items.AddRange(new object[] { "Todas", "None (0)", "Baja (1)", "Media (2)", "Alta (3)" });
            cmbCriticidad.SelectedIndex = 0;

            // Botones de acción
            btnBuscar = new Button { Text = "Buscar", Left = 518, Top = 70, Width = 80, BackColor = Color.SteelBlue, ForeColor = Color.White };
            btnBuscar.Click += BtnBuscar_Click;

            btnLimpiar = new Button { Text = "Limpiar", Left = 606, Top = 70, Width = 80 };
            btnLimpiar.Click += BtnLimpiar_Click;

            // Agregar controles al panel
            panelFiltros.Controls.AddRange(new Control[]
            {
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
                Dock                     = DockStyle.Fill,
                ReadOnly                 = true,
                AllowUserToAddRows       = false,
                AllowUserToDeleteRows    = false,
                SelectionMode            = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode      = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor          = Color.White,
                RowHeadersVisible        = false,
                BorderStyle              = BorderStyle.None,
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(248, 248, 255) }
            };

            // Agregar controles al formulario
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
                MessageBox.Show($"Error al cargar la bitácora:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Evento del botón Buscar: aplica los filtros seleccionados y actualiza la grilla.
        /// Implementa la búsqueda combinada requerida por T06a.
        /// </summary>
        private void BtnBuscar_Click(object sender, EventArgs e)
        {
            try
            {
                // Recoger valores de filtros (null si no están activos)
                DateTime? desde     = chkFiltrarFecha.Checked ? dtpDesde.Value  : (DateTime?)null;
                DateTime? hasta     = chkFiltrarFecha.Checked ? dtpHasta.Value  : (DateTime?)null;
                int       usuarioId = int.TryParse(txtUsuario.Text, out int uid) ? uid : 0;
                string    actividad = txtActividad.Text.Trim();
                int       criticidad = cmbCriticidad.SelectedIndex;   // 0=todos, 1=None, 2=Baja...

                DataTable datos = bitacoraBLL.BuscarPorFiltros(desde, hasta, usuarioId, actividad, criticidad);
                MostrarEnGrilla(datos);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en la búsqueda:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Limpia todos los filtros y recarga todos los registros.
        /// </summary>
        private void BtnLimpiar_Click(object sender, EventArgs e)
        {
            chkFiltrarFecha.Checked  = false;
            dtpDesde.Value           = DateTime.Today;
            dtpHasta.Value           = DateTime.Today;
            txtUsuario.Clear();
            txtActividad.Clear();
            cmbCriticidad.SelectedIndex = 0;

            CargarRegistros();
        }

        /// <summary>
        /// Asigna un DataTable a la grilla y actualiza el contador de resultados.
        /// </summary>
        /// <param name="datos">DataTable con los registros a mostrar.</param>
        private void MostrarEnGrilla(DataTable datos)
        {
            dgvBitacora.DataSource = datos;
            lblResultados.Text     = $"  {datos.Rows.Count} registro(s) encontrado(s)";
        }
    }
}
