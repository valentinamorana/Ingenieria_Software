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
    ///   ✓ Editar plan existente (doble clic en la grilla)
    ///   ✓ Desactivar plan (baja lógica)
    ///   ✓ Reactivar plan desactivado
    ///
    /// Accesible desde Menú → Ventas → Planes (permiso mnuPlanSuscripciones).
    /// </summary>
    public partial class Planes : Form
    {
        private readonly BLL.PlanSuscripcion planBLL = new BLL.PlanSuscripcion();

        // ── Controles ─────────────────────────────────────────────────────────
        private DataGridView  dgvPlanes;
        private TextBox       txtNombre;
        private NumericUpDown nudLimite;
        private NumericUpDown nudPrecio;
        private Button        btnGuardar;
        private Button        btnNuevo;
        private Button        btnDesactivar;
        private Button        btnActivar;
        private Label         lblMensaje;
        private Label         lblFormTitulo;

        private List<BE.PlanSuscripcion> _planes = new List<BE.PlanSuscripcion>();
        private int _idEnEdicion = 0;  // 0 = modo alta

        public Planes()
        {
            InitializeComponent();
            this.Text        = "Planes de Suscripción";
            this.ClientSize  = new Size(900, 560);
            this.MinimumSize = new Size(780, 500);

            ConstruirInterfaz();
            this.Load += (s, e) => CargarPlanes();
        }

        private void ConstruirInterfaz()
        {
            // ── Panel derecho: formulario ─────────────────────────────────────
            Panel panelForm = new Panel
            {
                Dock      = DockStyle.Right,
                Width     = 320,
                Padding   = new Padding(14),
                BackColor = Color.FromArgb(245, 245, 250)
            };

            lblFormTitulo = new Label
            {
                Text = "Nuevo Plan",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Left = 14, Top = 12, Width = 286, Height = 24
            };

            var lblNombre = new Label { Text = "Nombre del plan *", Left = 14, Top = 46, Width = 286 };
            txtNombre = new TextBox { Left = 14, Top = 64, Width = 286 };

            var lblLimite = new Label { Text = "Límite de prendas *", Left = 14, Top = 100, Width = 286 };
            nudLimite = new NumericUpDown
            {
                Left = 14, Top = 120, Width = 130,
                Minimum = 1, Maximum = 50, Value = 3
            };

            var lblPrecio = new Label { Text = "Precio mensual ($) *", Left = 14, Top = 158, Width = 286 };
            nudPrecio = new NumericUpDown
            {
                Left = 14, Top = 178, Width = 180,
                Minimum = 0, Maximum = 999999, DecimalPlaces = 2,
                Value = 0
            };

            btnGuardar = new Button
            {
                Text      = "Guardar Plan",
                Left      = 14, Top = 222,
                Width     = 286, Height = 36,
                BackColor = Color.FromArgb(210, 100, 135), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Click += BtnGuardar_Click;

            btnNuevo = new Button
            {
                Text      = "Limpiar / Nuevo",
                Left      = 14, Top = 266,
                Width     = 286, Height = 30,
                FlatStyle = FlatStyle.Flat
            };
            btnNuevo.Click += (s, e) => LimpiarFormulario();

            // ── Separador ─────────────────────────────────────────────────────
            var separador = new Label
            {
                Left = 14, Top = 310, Width = 286, Height = 1,
                BackColor = Color.Silver
            };

            var lblAcciones = new Label
            {
                Text      = "Acciones sobre plan seleccionado",
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.DimGray,
                Left      = 14, Top = 320, Width = 286, Height = 18
            };

            btnDesactivar = new Button
            {
                Text      = "Desactivar Plan",
                Left      = 14, Top = 344,
                Width     = 286, Height = 34,
                BackColor = Color.FromArgb(160, 80, 30), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Enabled = false
            };
            btnDesactivar.FlatAppearance.BorderSize = 0;
            btnDesactivar.Click += BtnDesactivar_Click;

            btnActivar = new Button
            {
                Text      = "Activar Plan",
                Left      = 14, Top = 386,
                Width     = 286, Height = 34,
                BackColor = Color.FromArgb(30, 140, 80), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Enabled = false
            };
            btnActivar.FlatAppearance.BorderSize = 0;
            btnActivar.Click += BtnActivar_Click;

            lblMensaje = new Label
            {
                Left      = 14, Top = 432,
                Width     = 286, Height = 90,
                Font      = new Font("Segoe UI", 8.5f)
            };

            panelForm.Controls.AddRange(new Control[]
            {
                lblFormTitulo,
                lblNombre, txtNombre,
                lblLimite, nudLimite,
                lblPrecio, nudPrecio,
                btnGuardar, btnNuevo,
                separador, lblAcciones,
                btnDesactivar, btnActivar,
                lblMensaje
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
            dgvPlanes.SelectionChanged += DgvPlanes_SelectionChanged;
            dgvPlanes.CellDoubleClick  += (s, e) => CargarPlanEnFormulario();

            var lblTituloGrilla = new Label
            {
                Text      = "Planes registrados",
                Dock      = DockStyle.Top,
                Height    = 28,
                Font      = new Font("Segoe UI", 9, FontStyle.Bold),
                Padding   = new Padding(6, 6, 0, 0),
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
                tabla.Columns.Add("ID",      typeof(int));
                tabla.Columns.Add("Nombre",  typeof(string));
                tabla.Columns.Add("Prendas", typeof(int));
                tabla.Columns.Add("Precio",  typeof(string));
                tabla.Columns.Add("Estado",  typeof(string));

                foreach (var p in _planes)
                    tabla.Rows.Add(p.IdPlan, p.Nombre, p.LimitePrendas,
                        $"${p.Precio:N2}", p.Estado ? "Activo" : "Inactivo");

                dgvPlanes.DataSource = tabla;
                if (dgvPlanes.Columns.Contains("ID"))
                    dgvPlanes.Columns["ID"].Width = 40;

                // Resaltar planes inactivos en gris claro
                foreach (DataGridViewRow fila in dgvPlanes.Rows)
                {
                    if (fila.Cells["Estado"].Value?.ToString() == "Inactivo")
                    {
                        fila.DefaultCellStyle.ForeColor = Color.Gray;
                        fila.DefaultCellStyle.Font = new Font("Segoe UI", 8.5f, FontStyle.Italic);
                    }
                }

                MostrarOk($"{_planes.Count} plan(es) cargado(s).");
            }
            catch (Exception ex)
            {
                MostrarError($"Error al cargar: {ex.Message}");
            }
        }

        private void DgvPlanes_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvPlanes.SelectedRows.Count == 0)
            {
                btnDesactivar.Enabled = false;
                btnActivar.Enabled    = false;
                return;
            }

            int id   = Convert.ToInt32(dgvPlanes.SelectedRows[0].Cells["ID"].Value);
            var plan = _planes.Find(p => p.IdPlan == id);
            if (plan == null) return;

            // Solo se muestra el botón relevante según el estado actual
            btnDesactivar.Enabled = plan.Estado;   // activo → puede desactivar
            btnActivar.Enabled    = !plan.Estado;  // inactivo → puede activar
        }

        private void CargarPlanEnFormulario()
        {
            if (dgvPlanes.SelectedRows.Count == 0) return;
            int id   = Convert.ToInt32(dgvPlanes.SelectedRows[0].Cells["ID"].Value);
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
            int id   = Convert.ToInt32(dgvPlanes.SelectedRows[0].Cells["ID"].Value);
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
            catch (Exception ex) { MostrarError(ex.Message); }
        }

        private void BtnActivar_Click(object sender, EventArgs e)
        {
            if (dgvPlanes.SelectedRows.Count == 0) return;
            int id   = Convert.ToInt32(dgvPlanes.SelectedRows[0].Cells["ID"].Value);
            var plan = _planes.Find(p => p.IdPlan == id);
            if (plan == null) return;

            var confirm = MessageBox.Show(
                $"¿Reactivar el plan '{plan.Nombre}'?\n\nEl plan volverá a estar disponible para nuevas suscripciones.",
                "Confirmar Activación",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);

            if (confirm != DialogResult.Yes) return;

            try
            {
                planBLL.Activar(id);
                MostrarOk($"Plan '{plan.Nombre}' reactivado.");
                LimpiarFormulario();
                CargarPlanes();
            }
            catch (Exception ex) { MostrarError(ex.Message); }
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
