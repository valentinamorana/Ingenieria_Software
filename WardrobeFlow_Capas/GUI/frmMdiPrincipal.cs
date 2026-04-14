using System;
using System.Windows.Forms;
using BLL;
using BE.Composite;
using Seguridad;

namespace GUI
{
    // Formulario MDI principal de WardrobeFlow.
    // Basado en frmMdiPrincipal del proyecto de referencia.
    // Usa SessionManagerSL (Singleton) para control de sesion y Sesion como helper estatico.
    public partial class frmMdiPrincipal : Form
    {
        // BLL de usuarios para el logout
        private readonly UsuarioBLL _bllUsuarios;

        // Constructor: inicializa componentes y valida el estado inicial del formulario
        public frmMdiPrincipal()
        {
            InitializeComponent();
            _bllUsuarios = new UsuarioBLL();
            ValidarForm();
        }

        // Actualiza el estado del menu segun si hay sesion activa y segun permisos.
        // Delega la verificacion de sesion a SessionManagerSL y la de roles al patron Composite.
        public void ValidarForm()
        {
            bool haySession = SessionManagerSL.Instancia.TieneSesionActiva();

            // Habilitar/deshabilitar items de sesion segun si hay usuario logueado
            this.itemLogin.Enabled   = !haySession;
            this.itemLogout.Enabled  = haySession;
            this.mnuGestores.Enabled = haySession;

            // Mostrar el nombre y rol del usuario activo en la barra de estado
            if (haySession)
            {
                var usr = SessionManagerSL.Instancia.ObtenerUsuarioActual();
                this.toolStripSesion.Text = usr.NombreCompleto + " [" + usr.Rol + "]";
            }
            else
                this.toolStripSesion.Text = "[ Sesion no iniciada ]";

            // Habilitar cada gestor segun el permiso correspondiente (patron Composite)
            this.mnuGestorCategorias.Enabled = SessionManagerSL.Instancia.IsInRole(TipoPermiso.GestorCategorias);
            this.mnuGestorPrendas.Enabled    = SessionManagerSL.Instancia.IsInRole(TipoPermiso.GestorPrendas);
            this.mnuGestorOutfits.Enabled    = SessionManagerSL.Instancia.IsInRole(TipoPermiso.GestorOutfits);
            this.mnuGestorUsuarios.Enabled   = SessionManagerSL.Instancia.IsInRole(TipoPermiso.GestorUsuarios);
            this.mnuGestorPermisos.Enabled   = SessionManagerSL.Instancia.IsInRole(TipoPermiso.GestorPermisos);
            // Bitacora: solo administradores (GestorUsuarios implica acceso admin)
            this.mnuGestorBitacora.Enabled   = SessionManagerSL.Instancia.IsInRole(TipoPermiso.GestorUsuarios);
        }

        // Abre el formulario de login como hijo MDI
        private void itemLogin_Click(object sender, EventArgs e)
        {
            frmLogin frm = new frmLogin();
            frm.MdiParent = this;
            frm.Show();
        }

        // Cierra la sesion activa tras confirmacion del usuario
        private void itemLogout_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Esta seguro de cerrar la sesion?", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _bllUsuarios.Logout();
                ValidarForm();
            }
        }

        // Abre el gestor de categorias como hijo MDI
        private void mnuGestorCategorias_Click(object sender, EventArgs e)
        {
            var frm = new frmGestorCategorias();
            frm.MdiParent = this;
            frm.Show();
        }

        // Abre el gestor de prendas como hijo MDI
        private void mnuGestorPrendas_Click(object sender, EventArgs e)
        {
            var frm = new frmGestorPrendas();
            frm.MdiParent = this;
            frm.Show();
        }

        // Abre el gestor de outfits como hijo MDI
        private void mnuGestorOutfits_Click(object sender, EventArgs e)
        {
            var frm = new frmGestorOutfits();
            frm.MdiParent = this;
            frm.Show();
        }

        // Abre el gestor de usuarios como hijo MDI
        private void mnuGestorUsuarios_Click(object sender, EventArgs e)
        {
            var frm = new frmGestorUsuarios();
            frm.MdiParent = this;
            frm.Show();
        }

        // Abre el gestor de permisos como hijo MDI
        private void mnuGestorPermisos_Click(object sender, EventArgs e)
        {
            var frm = new frmGestorPermisos();
            frm.MdiParent = this;
            frm.Show();
        }

        // Abre la bitacora de eventos del sistema como hijo MDI
        private void mnuGestorBitacora_Click(object sender, EventArgs e)
        {
            var frm = new frmGestorBitacora();
            frm.MdiParent = this;
            frm.Show();
        }
    }
}
