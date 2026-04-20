using BLL;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace GUI
{
    /// <summary>
    /// WardrobeFlow — Formulario de Login para Empleados.
    ///
    /// Pantalla de acceso exclusiva para empleados del sistema.
    /// Se muestra como diálogo modal al iniciar la aplicación.
    ///
    /// FLUJO:
    ///   Empleado ingresa credenciales → BLL.Usuario.Login() → verifica en BD con PBKDF2
    ///   → Si OK: el formulario desaparece (DialogResult.OK) y se abre el sistema
    ///   → Si falla: muestra mensaje de error sin cerrar el formulario
    ///   → Botón Salir: cierra la aplicación directamente
    /// </summary>
    public partial class Login : Form
    {
        private readonly Usuario usuarioBLL = new Usuario();

        public Login()
        {
            InitializeComponent();

            // Permitir presionar Enter para ingresar (UX de empleados)
            this.AcceptButton = btnIngresar;

            // Efecto de hover manual en el botón Ingresar para cambiar color de texto
            btnIngresar.MouseEnter += (s, e) => btnIngresar.ForeColor = Color.FromArgb(18, 18, 30);
            btnIngresar.MouseLeave += (s, e) => btnIngresar.ForeColor = Color.White;
        }

        /// <summary>
        /// Autentica al empleado. Si las credenciales son válidas,
        /// el formulario se cierra (desaparece) y se abre el sistema.
        /// </summary>
        private void btnIngresar_Click(object sender, EventArgs e)
        {
            lblError.Text = string.Empty;

            try
            {
                bool esValido = usuarioBLL.Login(this, txtUsuario.Text, txtContraseña.Text);

                if (esValido)
                {
                    // Login exitoso → el formulario desaparece y Program.Main() abre el Menú
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    // Registrar intento fallido en bitácora (criticidad 4)
                    usuarioBLL.RegistrarIntentoFallido(this, txtUsuario.Text);

                    lblError.Text = "Usuario o contraseña incorrectos.";
                    txtContraseña.Clear();
                    txtContraseña.Focus();
                }
            }
            catch (Exception ex)
            {
                lblError.Text = ex.Message;
            }
        }

        /// <summary>
        /// Cierra la aplicación completamente al presionar Salir.
        /// </summary>
        private void btnSalir_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        /// <summary>
        /// Abre el formulario de recuperación de contraseña.
        /// Permite al usuario ingresar su username para que el administrador
        /// pueda resetear su clave desde el módulo de Usuarios.
        /// </summary>
        private void lnkOlvidaste_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            using (var form = new OlvideContrasenaForm())
            {
                form.ShowDialog(this);
            }
        }
    }
}
