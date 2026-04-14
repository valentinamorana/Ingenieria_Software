using System;
using System.Windows.Forms;
using BLL;
using Seguridad;

namespace GUI
{
    // Formulario de login de WardrobeFlow.
    // Basado en frmLogin del proyecto de referencia, adaptado para usar Documento.
    public partial class frmLogin : Form
    {
        // BLL de usuarios para realizar el login
        private readonly UsuarioBLL _usuarioBLL;

        // Constructor: inicializa componentes y la BLL
        public frmLogin()
        {
            InitializeComponent();
            _usuarioBLL = new UsuarioBLL();
        }

        // Boton Ingresar: intenta hacer login con documento y password
        private void btnIngresar_Click(object sender, EventArgs e)
        {
            try
            {
                // Llamar al metodo Login de la BLL (igual que en el proyecto referencia)
                var resultado = _usuarioBLL.Login(txtDocumento.Text, txtPassword.Text);

                // Login exitoso: notificar al MDI para que refresque los menus
                frmMdiPrincipal frm = (frmMdiPrincipal)this.MdiParent;
                frm.ValidarForm();

                // Cerrar el formulario de login
                this.Close();
            }
            catch (LoginException error)
            {
                // Mostrar mensaje segun el tipo de error (igual que en el proyecto referencia)
                switch (error.Result)
                {
                    case LoginResult.InvalidUsername:
                        MessageBox.Show("Documento incorrecto.", "Error de acceso",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        break;
                    case LoginResult.InvalidPassword:
                        MessageBox.Show("Contrasena incorrecta.", "Error de acceso",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        break;
                }
            }
        }

        // Boton Salir: cierra el formulario de login
        private void btnSalir_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
