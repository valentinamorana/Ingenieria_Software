using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Servicios.Multiidioma;

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
    /// <summary>
    /// Hereda de <see cref="FormBase"/>:
    ///   - MostrarOk() y MostrarError() → heredados, no se redeclaran
    ///   - MensajeLabel → sobreescrito para devolver el lblMensaje de este formulario
    /// </summary>
    public partial class Planes : FormBase, IIdiomaObserver
    {
        protected override Label MensajeLabel => lblMensaje;

        private readonly BLL.Interfaces.IPlanSuscripcionService planBLL = new BLL.PlanSuscripcion();

        private List<BE.PlanSuscripcion> _planes = new List<BE.PlanSuscripcion>();
        private int _idEnEdicion = 0;  // 0 = modo alta

        // Idioma activo — sincronizado en Traducir() para usar en CargarPlanes
        private Idioma _idioma = GestorIdioma.IdiomaActual;

        public Planes()
        {
            InitializeComponent();
            this.Load += new EventHandler(Planes_Load);
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

        public void UpdateLanguage(Idioma idioma) => Traducir(idioma);

        private void Traducir(Idioma idioma)
        {
            _idioma = idioma;
            var t = Traductor.ObtenerTraducciones(idioma);
            if (this.Tag != null && t.ContainsKey(this.Tag.ToString()))
                this.Text = t[this.Tag.ToString()].Texto;
            Aplicar(lblFormTitulo,  t);
            Aplicar(lblNombrePlan,  t);
            Aplicar(lblLimite,      t);
            Aplicar(lblPrecio,      t);
            Aplicar(btnGuardar,     t);
            Aplicar(btnNuevo,       t);
            Aplicar(lblAcciones,    t);
            Aplicar(btnDesactivar,  t);
            Aplicar(btnActivar,     t);
            Aplicar(lblTituloGrilla,t);
            TraducirHeadersGrilla();
            // Recargar la grilla para que "Activo"/"Inactivo" y el conteo reflejen el idioma nuevo
            if (_planes.Count > 0) CargarPlanes();
        }

        /// <summary>Traduce los HeaderText de la grilla de planes según el idioma activo.</summary>
        private void TraducirHeadersGrilla()
        {
            var t = Traductor.ObtenerTraducciones(_idioma);
            void RH(string col, string key, string fallback)
            {
                if (dgvPlanes.Columns.Contains(col) && t.ContainsKey(key))
                    dgvPlanes.Columns[col].HeaderText = t[key].Texto;
                else if (dgvPlanes.Columns.Contains(col))
                    dgvPlanes.Columns[col].HeaderText = fallback;
            }
            RH("Nombre",  "col.plan.nombre",  "Nombre");
            RH("Prendas", "col.plan.prendas", "Prendas");
            RH("Precio",  "col.plan.precio",  "Precio");
            RH("Estado",  "col.plan.estado",  "Estado");
        }

        private static void Aplicar(Control c, IDictionary<string, Traduccion> t)
        {
            if (c?.Tag != null && t.ContainsKey(c.Tag.ToString()))
                c.Text = t[c.Tag.ToString()].Texto;
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

                var t = Traductor.ObtenerTraducciones(_idioma);
                string T(string key, string fallback) => t.ContainsKey(key) ? t[key].Texto : fallback;
                string lblActivo   = T("plan.activo",   "Activo");
                string lblInactivo = T("plan.inactivo", "Inactivo");

                foreach (var p in _planes)
                    tabla.Rows.Add(p.IdPlan, p.Nombre, p.LimitePrendas,
                        $"${p.Precio:N2}", p.Estado ? lblActivo : lblInactivo);

                dgvPlanes.DataSource = tabla;
                if (dgvPlanes.Columns.Contains("ID"))
                    dgvPlanes.Columns["ID"].Width = 40;

                TraducirHeadersGrilla();

                // Resaltar planes inactivos en gris claro (detección por bool, no por texto)
                int rowIdx = 0;
                foreach (DataGridViewRow fila in dgvPlanes.Rows)
                {
                    if (rowIdx < _planes.Count && !_planes[rowIdx].Estado)
                    {
                        fila.DefaultCellStyle.ForeColor = Color.Gray;
                        fila.DefaultCellStyle.Font = new Font("Segoe UI", 8.5f, FontStyle.Italic);
                    }
                    rowIdx++;
                }

                string fmt = T("msg.planes.cargados", "{0} plan(es) cargado(s).");
                MostrarOk(string.Format(fmt, _planes.Count));
            }
            catch (Exception ex)
            {
                var te = Traductor.ObtenerTraducciones(_idioma);
                MostrarError(string.Format(te.ContainsKey("err.generico.cargar") ? te["err.generico.cargar"].Texto : "Error al cargar: {0}", ex.Message));
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
            var tE = Traductor.ObtenerTraducciones(_idioma);
            lblFormTitulo.Text = tE.ContainsKey("lbl.editplan") ? tE["lbl.editplan"].Texto : "Editar Plan";
            txtNombre.Text     = plan.Nombre;
            nudLimite.Value    = plan.LimitePrendas;
            nudPrecio.Value    = plan.Precio;
        }

        private void LimpiarFormulario()
        {
            _idEnEdicion       = 0;
            var tN = Traductor.ObtenerTraducciones(_idioma);
            lblFormTitulo.Text = tN.ContainsKey("lbl.nuevopla") ? tN["lbl.nuevopla"].Texto : "Nuevo Plan";
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
                var t = Traductor.ObtenerTraducciones(_idioma);
                string T(string k, string fb) => t.ContainsKey(k) ? t[k].Texto : fb;

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
                    MostrarOk(string.Format(T("msg.planes.creado", "Plan '{0}' creado."), plan.Nombre));
                }
                else
                {
                    planBLL.Modificar(plan);
                    MostrarOk(string.Format(T("msg.planes.actualizado", "Plan '{0}' actualizado."), plan.Nombre));
                }

                LimpiarFormulario();
                CargarPlanes();
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void BtnDesactivar_Click(object sender, EventArgs e)
        {
            if (dgvPlanes.SelectedRows.Count == 0) return;
            int id   = Convert.ToInt32(dgvPlanes.SelectedRows[0].Cells["ID"].Value);
            var plan = _planes.Find(p => p.IdPlan == id);
            if (plan == null) return;

            var td = Traductor.ObtenerTraducciones(_idioma);
            string Td(string k, string fb) => td.ContainsKey(k) ? td[k].Texto : fb;

            var confirm = MessageBox.Show(
                string.Format(Td("conf.planes.desat.msg", "¿Desactivar el plan '{0}'?\n\nLos clientes con este plan no serán afectados."), plan.Nombre),
                Td("conf.planes.desat.tit", "Confirmar Desactivación"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (confirm != DialogResult.Yes) return;

            try
            {
                planBLL.Desactivar(id);
                MostrarOk(string.Format(Td("msg.planes.desactivado", "Plan '{0}' desactivado."), plan.Nombre));
                LimpiarFormulario();
                CargarPlanes();
            }
            catch (Exception ex) { MostrarError(ex); }
        }

        private void BtnActivar_Click(object sender, EventArgs e)
        {
            if (dgvPlanes.SelectedRows.Count == 0) return;
            int id   = Convert.ToInt32(dgvPlanes.SelectedRows[0].Cells["ID"].Value);
            var plan = _planes.Find(p => p.IdPlan == id);
            if (plan == null) return;

            var ta = Traductor.ObtenerTraducciones(_idioma);
            string Ta(string k, string fb) => ta.ContainsKey(k) ? ta[k].Texto : fb;

            var confirm = MessageBox.Show(
                string.Format(Ta("conf.planes.act.msg", "¿Reactivar el plan '{0}'?\n\nEl plan volverá a estar disponible para nuevas suscripciones."), plan.Nombre),
                Ta("conf.planes.act.tit", "Confirmar Activación"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);

            if (confirm != DialogResult.Yes) return;

            try
            {
                planBLL.Activar(id);
                MostrarOk(string.Format(Ta("msg.planes.reactivado", "Plan '{0}' reactivado."), plan.Nombre));
                LimpiarFormulario();
                CargarPlanes();
            }
            catch (Exception ex) { MostrarError(ex); }
        }

    }
}
