using Servicios.Multiidioma;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace GUI
{
    public partial class VersionHistorialForm : Form, IIdiomaObserver
    {
        private readonly BLL.VersionUsuario _bll         = new BLL.VersionUsuario();
        private readonly BLL.Usuario        _bllUsuario  = new BLL.Usuario();

        private List<BE.VersionUsuario> _versiones = new List<BE.VersionUsuario>();

        public VersionHistorialForm()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GestorIdioma.SuscribirObservador(this);
            Traducir(GestorIdioma.IdiomaActual);
            CargarUsuarios();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            GestorIdioma.DesuscribirObservador(this);
            base.OnFormClosing(e);
        }

        public void UpdateLanguage(Idioma idioma) => Traducir(idioma);

        private void Traducir(Idioma idioma)
        {
            var t = Traductor.ObtenerTraducciones(idioma);
            string T(string key, string fallback) => t.ContainsKey(key) ? t[key].Texto : fallback;

            this.Text           = T("frm.historialusr",    "Historial de Cambios de Usuarios");
            lblTitulo.Text      = T("frm.historialusr",    "Historial de Cambios de Usuarios");
            lblUsuario.Text     = T("lbl.ver.usuario",     "Usuario:");
            btnCargar.Text      = T("btn.ver.cargar",      "Cargar");
            btnRestaurar.Text   = T("btn.ver.restaurar",   "Restaurar Versión Seleccionada");

            if (dgv.Columns.Count > 0)
            {
                dgv.Columns["colId"].HeaderText      = T("col.ver.id",       "ID");
                dgv.Columns["colFecha"].HeaderText   = T("col.ver.fecha",    "Fecha");
                dgv.Columns["colActor"].HeaderText   = T("col.ver.actor",    "Modificado por");
                dgv.Columns["colDetalle"].HeaderText = T("col.ver.detalle",  "Detalle");
                dgv.Columns["colEstado"].HeaderText  = T("col.ver.estado",   "Estado");
            }
        }

        private void CargarUsuarios()
        {
            try
            {
                var usuarios = _bllUsuario.ObtenerTodos();
                cboUsuario.DisplayMember = "Username";
                cboUsuario.ValueMember   = "Id";
                cboUsuario.DataSource    = usuarios;
                cboUsuario.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[VersionHistorialForm.CargarUsuarios] {ex.Message}");
            }
        }

        private void btnCargar_Click(object sender, EventArgs e)
        {
            if (cboUsuario.SelectedValue == null) return;

            try
            {
                int idUsuario = (int)cboUsuario.SelectedValue;
                _versiones = _bll.ObtenerPorUsuario(idUsuario);
                CargarGrilla();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar historial:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarGrilla()
        {
            dgv.Rows.Clear();
            foreach (var v in _versiones)
            {
                dgv.Rows.Add(
                    v.Id,
                    v.Fecha.ToString("dd/MM/yyyy HH:mm:ss"),
                    v.Actor,
                    v.Detalle,
                    v.EstadoSnapshot ? "Activo" : "Bloqueado"
                );
            }
        }

        private void btnRestaurar_Click(object sender, EventArgs e)
        {
            if (dgv.CurrentRow == null)
            {
                MessageBox.Show("Seleccioná una versión de la grilla.",
                    "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int idVersion = Convert.ToInt32(dgv.CurrentRow.Cells["colId"].Value);
            var ver = _versiones.Find(v => v.Id == idVersion);
            if (ver == null) return;

            string msg = $"¿Restaurar al usuario '{ver.UsernameSnapshot}' al estado del {ver.Fecha:dd/MM/yyyy HH:mm}?\n\n" +
                         $"Detalle del snapshot: {ver.Detalle}\n\n" +
                         "Esta acción es reversible (se graba un nuevo snapshot antes de restaurar).";

            if (MessageBox.Show(msg, "Confirmar Restauración",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                _bll.RestaurarVersion(this.Text, idVersion);
                MessageBox.Show("Versión restaurada correctamente.",
                    "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                btnCargar_Click(null, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al restaurar versión:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
