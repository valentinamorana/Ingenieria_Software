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

        private List<BE.PlanSuscripcion> _planes = new List<BE.PlanSuscripcion>();
        private int _idEnEdicion = 0;  // 0 = modo alta

        public Planes()
        {
            InitializeComponent();
            this.Load += new EventHandler(Planes_Load);
        }

        // ── Eventos del Designer ──────────────────────────────────────────────

        private void Planes_Load(object sender, EventArgs e)
        {
            CargarPlanes();
        }

        private void BtnNuevo_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
        }

        private void DgvPlanes_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            CargarPlanEnFormulario();
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

        // ── Eventos de botones ────────────────────────────────────────────────

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
