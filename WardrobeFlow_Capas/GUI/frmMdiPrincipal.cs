using System;
using System.Windows.Forms;
using BLL;
using BE.Composite;
using Seguridad;

namespace GUI
{
    // Formulario MDI principal de WardrobeFlow.
    // Usa SessionManagerSL (Singleton) para control de sesion.
    public partial class frmMdiPrincipal : Form
    {
        private readonly UsuarioBLL _bllUsuarios;

        public frmMdiPrincipal()
        {
            InitializeComponent();
            _bllUsuarios = new UsuarioBLL();

            // Color de fondo del area MDI (lavanda suave)
            foreach (Control c in this.Controls)
            {
                if (c is MdiClient mdiClient)
                {
                    mdiClient.BackColor = System.Drawing.Color.FromArgb(235, 228, 242);
                    break;
                }
            }

            pnlBienvenida.BringToFront();
            CentrarPanel();
            this.Resize += (s, e) => CentrarPanel();
            this.MdiChildActivate += (s, e) => ActualizarPanelBienvenida();
            ValidarForm();
        }

        // Centra pnlBienvenida en el area MDI visible
        private void CentrarPanel()
        {
            int mdiTop = menuStrip.Height;
            int mdiBottom = this.ClientSize.Height - statusStrip.Height;
            int mdiH = mdiBottom - mdiTop;
            pnlBienvenida.Left = (this.ClientSize.Width - pnlBienvenida.Width) / 2;
            pnlBienvenida.Top = mdiTop + (mdiH - pnlBienvenida.Height) / 2;
        }

        // Oculta o muestra el panel de bienvenida segun si hay formularios hijos abiertos
        private void ActualizarPanelBienvenida()
        {
            bool hayHijos = this.MdiChildren.Length > 0;
            pnlBienvenida.Visible = !hayHijos && !SessionManagerSL.Instancia.TieneSesionActiva();
        }

        // Actualiza el estado del menu segun sesion activa y permisos (patron Composite)
        public void ValidarForm()
        {
            bool haySession = SessionManagerSL.Instancia.TieneSesionActiva();

            pnlBienvenida.Visible = !haySession && this.MdiChildren.Length == 0;
            this.itemLogin.Enabled = !haySession;
            this.itemLogout.Enabled = haySession;
            this.mnuGestores.Enabled = haySession;

            if (haySession)
            {
                var usr = SessionManagerSL.Instancia.ObtenerUsuarioActual();
                this.toolStripSesion.Text = usr.NombreCompleto + " [" + usr.Rol + "]";
            }
            else
                this.toolStripSesion.Text = "[ Sesi\u00f3n no iniciada ]";

            this.mnuGestorCategorias.Enabled = SessionManagerSL.Instancia.IsInRole(TipoPermiso.GestorCategorias);
            this.mnuGestorPrendas.Enabled = SessionManagerSL.Instancia.IsInRole(TipoPermiso.GestorPrendas);
            this.mnuGestorOutfits.Enabled = SessionManagerSL.Instancia.IsInRole(TipoPermiso.GestorOutfits);
            this.mnuGestorUsuarios.Enabled = SessionManagerSL.Instancia.IsInRole(TipoPermiso.GestorUsuarios);
            this.mnuGestorPermisos.Enabled = SessionManagerSL.Instancia.IsInRole(TipoPermiso.GestorPermisos);
            this.mnuGestorBitacora.Enabled = SessionManagerSL.Instancia.IsInRole(TipoPermiso.GestorUsuarios);
        }

        private void itemLogin_Click(object sender, EventArgs e)
        {
            frmLogin frm = new frmLogin();
            frm.MdiParent = this;
            frm.Show();
        }

        private void itemLogout_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("\u00bfEst\u00e1 seguro de cerrar la sesi\u00f3n?", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _bllUsuarios.Logout();
                ValidarForm();
            }
        }

        private void mnuGestorCategorias_Click(object sender, EventArgs e)
        {
            var frm = new frmGestorCategorias();
            frm.MdiParent = this;
            frm.Show();
        }

        private void mnuGestorPrendas_Click(object sender, EventArgs e)
        {
            var frm = new frmGestorPrendas();
            frm.MdiParent = this;
            frm.Show();
        }

        private void mnuGestorOutfits_Click(object sender, EventArgs e)
        {
            var frm = new frmGestorOutfits();
            frm.MdiParent = this;
            frm.Show();
        }

        private void mnuGestorUsuarios_Click(object sender, EventArgs e)
        {
            var frm = new frmGestorUsuarios();
            frm.MdiParent = this;
            frm.Show();
        }

        private void mnuGestorPermisos_Click(object sender, EventArgs e)
        {
            var frm = new frmGestorPermisos();
            frm.MdiParent = this;
            frm.Show();
        }

        private void mnuGestorBitacora_Click(object sender, EventArgs e)
        {
            var frm = new frmGestorBitacora();
            frm.MdiParent = this;
            frm.Show();
        }
    }
}
