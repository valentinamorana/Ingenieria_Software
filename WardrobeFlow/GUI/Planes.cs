using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace GUI
{
    /// <summary>
    /// Capa de Presentación — Módulo de Gestión de Planes de Suscripción.
    ///
    /// Permite al Vendedor consultar los planes disponibles y al Supervisor/Admin
    /// crear y modificar planes.
    ///
    ///   ✓ Ver listado de planes (activos e inactivos)
    ///   ✓ Crear nuevo plan
    ///   ✓ Editar plan existente
    ///   ✓ Desactivar plan (baja lógica)
    ///
    /// Accesible desde Menú → Ventas → Planes (permiso mnuPlanSuscripciones).
    /// </summary>
    public partial class Planes : Form
    {
        private readonly BLL.PlanSuscripcion planBLL = new BLL.PlanSuscripcion();

        // ── Controles ─────────────────────────────────────────────────────────
        private DataGridView dgvPlanes;
        private TextBox      txtNombre;
        private NumericUpDown nudLimite;
        private NumericUpDown nudPrecio;
        private Button       btnGuardar;
        private Button       btnNuevo;
        private Button       btnDesactivar;
        private Label        lblMensaje;
        private Label        lblFormTitulo;

        private List<BE.PlanSuscripcion> _planes = new List<BE.PlanSuscripcion>();
        private int _idEnEdicion = 0;  // 0 = modo alta

        public Planes()
        {
            InitializeComponent();
            this.Text        = "Planes de Suscripción";
            this.ClientSize  = new Size(860, 540);
            this.MinimumSize = new Size(760, 500);

            ConstruirInterfaz();
            this.Load += (s, e) => CargarPlanes();
        }

        private void ConstruirInterfaz()
        {
            // ── Panel derecho: formulario ─────────────────────────────────────
            Panel panelForm = new Panel
            {
                Dock = DockStyle.Right, Width = 280,
                Padding = new Padding(12),
                BackColor = Color.FromArgb(245, 245, 250)
            };

            lblFormTitulo = new Label
            {
                Text = "Nuevo Plan", Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Left = 12, Top = 12, Width = 230
            };

            var lblNombre = new Label { Text = "Nombre del plan *", Left = 12, Top = 50, Width = 250 };
            txtNombre = new TextBox { Left = 12, Top = 70, Width = 250 };

            var lblLimite = new Label { Text = "Límite de prendas *", Left = 12, Top = 106, Width = 250 };
            nudLimite = new NumericUpDown
            {
                Left = 12, Top = 126, Width = 120,
                Minimum = 1, Maximum = 50, Value = 3
            };

            var lblPrecio = new Label { Text = "Precio mensual ($) *", Left = 12, Top = 162, Width = 250 };
            nudPrecio = new NumericUpDown
            {
                Left = 12, Top = 182, Width = 160,
                Minimum = 0, Maximum = 999999, DecimalPlaces = 2,
                Value = 0
            };

            btnGuardar = new Button
            {
                Text = "Guardar Plan", Left = 12, Top = 226,
                Width = 250, Height = 36,
                BackColor = Color.FromArgb(210, 100, 135), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Click += BtnGuardar_Click;

            btnNuevo = new Button
            {
                Text = "Limpiar / Nuevo", Left = 12, Top = 270,
                Width = 250, Height = 30,
                FlatStyle = FlatStyle.Flat
            };
            btnNuevo.Click += (s, e) => LimpiarFormulario();

            var separador = new Label
            {
                Left = 12, Top = 314, Width = 250, Height = 1,
                BackColor = Color.Silver
            };

            btnDesactivar = new Button
            {
                Text = "Desactivar Plan Seleccionado", Left = 12, Top = 324,
                Width = 250, Height = 36,
                BackColor = Color.FromArgb(160, 80, 30), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Enabled = false
            };
            btnDesactivar.FlatAppearance.BorderSize = 0;
            btnDesactivar.Click += BtnDesactivar_Click;

            lblMensaje = new Label
            {
                Left = 12, Top = 372, Width = 250, Height = 90,
                Font = new Font("Segoe UI", 8.5f)
            };

            panelForm.Controls.AddRange(new Control[]
            {
                lblFormTitulo, lblNombre, txtNombre,
                lblLimite, nudLimite,
                lblPrecio, nudPrecio,
                btnGuardar, btnNuevo,
                separador, btnDesactivar, lblMensaje
            });

            // ── DataGridView ──────────────────────────────────────────────────
            dgvPlanes = new DataGridView
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
                    BackColor = Color.FromArgb(255, 248, 252)
                },
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    SelectionBackColor = Color.FromArgb(255, 182, 193),
                    SelectionForeColor = Color.Black
                }
            };
            dgvPlanes.SelectionChanged     += DgvPlanes_SelectionChanged;
            dgvPlanes.CellDoubleClick      += (s, e) => CargarPlanEnFormulario();

            var lblTituloGrilla = new Label
            {
                Text = "Planes registrados",
                Dock = DockStyle.Top, Height = 28,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Padding = new Padding(6, 6, 0, 0),
                BackColor = Color.FromArgb(230, 230, 240)
            };

            this.Controls.Add(dgvPlanes);
            this.Controls.Add(lblTituloGrilla);
            this.Controls.Add(panelForm);
        }

        // ── Carga ─────────────────────────────────────────────────────────────

        private void CargarPlanes()
        {
            try
            {
                _planes = planBLL.ObtenerTodos();
                var tabla = new DataTable();
                tabla.Columns.Add("ID",       typeof(int));
                tabla.Columns.Add("Nombre",   typeof(string));
                tabla.Columns.Add("Prendas",  typeof(int));
                tabla.Columns.Add("Precio",   typeof(string));
                tabla.Columns.Add("Estado",   typeof(string));

                foreach (var p in _planes)
                    tabla.Rows.Add(p.IdPlan, p.Nombre, p.LimitePrendas,
                        $"${p.Precio:N2}", p.Estado ? "Activo" : "Inactivo");

                dgvPlanes.DataSource = tabla;
                if (dgvPlanes.Columns.Contains("ID"))
                    dgvPlanes.Columns["ID"].Width = 40;

                MostrarOk($"{_planes.Count} plan(es) cargado(s).");
            }
            catch (Exception ex)
            {
                MostrarError($"Error al cargar: {ex.Message}");
            }
        }

        private void DgvPlanes_SelectionChanged(object sender, EventArgs e)
        {
            btnDesactivar.Enabled = dgvPlanes.SelectedRows.Count > 0;
        }

        private void CargarPlanEnFormulario()
        {
            if (dgvPlanes.SelectedRows.Count == 0) return;
            int id = Convert.ToInt32(dgvPlanes.SelectedRows[0].Cells["ID"].Value);
            var plan = _planes.Find(p => p.IdPlan == id);
            if (plan == null) return;

            _idEnEdicion       = plan.IdPlan;
            lblFormTitulo.Text = "Editar Plan";
            txtNombre.Text     = plan.Nombre;
            nudLimite.Value    = plan.LimitePrendas;
            nudPrecio.Value    = plan.Precio;
        }

        private void LimpiarFormulario()
        {
            _idEnEdicion       = 0;
            lblFormTitulo.Text = "Nuevo Plan";
            txtNombre.Clear();
            nudLimite.Value    = 3;
            nudPrecio.Value    = 0;
            lblMensaje.Text    = string.Empty;
            dgvPlanes.ClearSelection();
        }

        // ── Eventos ───────────────────────────────────────────────────────────

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            lblMensaje.Text = string.Empty;
            try
            {
                var plan = new BE.PlanSuscripcion
                {
                    IdPlan        = _idEnEdicion,
                    Nombre        = txtNombre.Text.Trim(),
                    LimitePrendas = (int)nudLimite.Value,
                    Precio        = nudPrecio.Value,
                    Estado        = true
                };

                if (_idEnEdicion == 0)
                {
                    planBLL.Alta(plan);
                    MostrarOk($"Plan '{plan.Nombre}' creado.");
                }
                else
                {
                    planBLL.Modificar(plan);
                    MostrarOk($"Plan '{plan.Nombre}' actualizado.");
                }

                LimpiarFormulario();
                CargarPlanes();
            }
            catch (Exception ex)
            {
                MostrarError(ex.Message);
            }
        }

        private void BtnDesactivar_Click(object sender, EventArgs e)
        {
            if (dgvPlanes.SelectedRows.Count == 0) return;
            int id = Convert.ToInt32(dgvPlanes.SelectedRows[0].Cells["ID"].Value);
            var plan = _planes.Find(p => p.IdPlan == id);
            if (plan == null) return;

            var confirm = MessageBox.Show(
                $"¿Desactivar el plan '{plan.Nombre}'?\n\nLos clientes con este plan no serán afectados.",
                "Confirmar Desactivación",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (confirm != DialogResult.Yes) return;

            try
            {
                planBLL.Desactivar(id);
                MostrarOk($"Plan '{plan.Nombre}' desactivado.");
                LimpiarFormulario();
                CargarPlanes();
            }
            catch (Exception ex)
            {
                MostrarError(ex.Message);
            }
        }

        private void MostrarOk(string msg)
        {
            lblMensaje.ForeColor = Color.DarkGreen;
            lblMensaje.Text      = $"✓ {msg}";
        }

        private void MostrarError(string msg)
        {
            lblMensaje.ForeColor = Color.DarkRed;
            lblMensaje.Text      = $"✗ {msg}";
        }
    }
}
