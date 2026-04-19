using System;
using System.Windows.Forms;

namespace GUI
{
    /// <summary>
    /// Capa de Presentación — Formulario Menú Principal (MDI Container).
    ///
    /// Es la ventana principal de la aplicación, implementada como contenedor MDI
    /// (Multiple Document Interface). Los formularios hijos (Bitácora, Usuarios)
    /// se abren dentro de esta ventana.
    ///
    /// ESTRUCTURA DEL MENÚ:
    ///   - Perfil → Cerrar Sesión
    ///   - Administrar → Usuarios
    ///   - Bitácora (acceso directo)
    ///
    /// NOTAS MDI:
    ///   - IsMdiContainer = true (configurado en Designer)
    ///   - Formularios hijo se asignan con hijo.MdiParent = this
    ///   - Al cerrar sesión se reinicia la aplicación (Application.Restart)
    ///     para limpiar toda la memoria de estado
    /// </summary>
    public partial class Menu : Form
    {
        /// <summary>
        /// Constructor: inicializa el formulario MDI con el menú de navegación.
        /// </summary>
        public Menu()
        {
            InitializeComponent();

            // Conectar el evento Click del ítem "Usuarios" del menú (estaba sin handler)
            this.usuariosToolStripMenuItem.Click += new EventHandler(usuariosToolStripMenuItem_Click);

            // Mostrar el nombre del usuario logueado en el título de la ventana.
            // Se obtiene via BLL para respetar la arquitectura (GUI no referencia Seguridad directamente).
            BLL.Usuario bll = new BLL.Usuario();
            BE.Usuario usuarioActivo = bll.ObtenerUsuarioActivo();
            if (usuarioActivo != null)
            {
                this.Text = "Sistema IS  —  Usuario: " + usuarioActivo.Username +
                            (usuarioActivo.Perfil != null ? "  [" + usuarioActivo.Perfil + "]" : "");
            }
        }

        /// <summary>
        /// Cierra la sesión del usuario activo y reinicia la aplicación.
        /// El reinicio garantiza que toda la memoria de estado quede limpia
        /// (SessionManager, conexiones, formularios abiertos, etc.).
        /// </summary>
        private void cerrarSesionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Confirmar cierre de sesión
            var resultado = MessageBox.Show(
                "¿Está seguro que desea cerrar la sesión?",
                "Cerrar Sesión",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (resultado == DialogResult.Yes)
            {
                // Registrar el logout en bitácora y limpiar la sesión
                BLL.Usuario usuarioBLL = new BLL.Usuario();
                usuarioBLL.Logout(this);

                // Reiniciar la aplicación para volver al Login con estado limpio
                Application.Restart();
            }
        }

        /// <summary>
        /// Abre el formulario de Bitácora como hijo MDI dentro del menú principal.
        /// Si ya existe una instancia abierta, la trae al frente en lugar de crear otra.
        /// </summary>
        private void bitacoraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Verificar si ya hay una instancia de Bitácora abierta para evitar duplicados
            foreach (Form hijo in this.MdiChildren)
            {
                if (hijo is Bitacora)
                {
                    hijo.BringToFront();
                    return;
                }
            }

            // Crear y mostrar el formulario hijo de Bitácora
            Bitacora frmBitacora = new Bitacora();
            frmBitacora.MdiParent = this;
            frmBitacora.Show();
        }

        /// <summary>
        /// Abre el formulario de Gestión de Usuarios como hijo MDI.
        /// Solo debería ser accesible para usuarios con perfil Administrador.
        /// </summary>
        private void usuariosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Verificar si ya hay una instancia de Usuarios abierta para evitar duplicados
            foreach (Form hijo in this.MdiChildren)
            {
                if (hijo is Usuarios)
                {
                    hijo.BringToFront();
                    return;
                }
            }

            // Crear y mostrar el formulario hijo de Gestión de Usuarios
            Usuarios frmUsuarios = new Usuarios();
            frmUsuarios.MdiParent = this;
            frmUsuarios.Show();
        }
    }
}
