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

            //// Ojito mostrar/ocultar contraseña — se achica el textbox para que el botón quede afuera del borde
            //txtContraseña.Width -= 28;
            //var btnOjo = new Button
            //{
            //    Text      = "👁",
            //    Font      = new Font("Segoe UI Emoji", 9f),
            //    Size      = new Size(26, txtContraseña.Height),
            //    Location  = new Point(txtContraseña.Right + 2, txtContraseña.Top),
            //    FlatStyle = FlatStyle.Flat,
            //    BackColor = txtContraseña.BackColor,
            //    ForeColor = Color.FromArgb(100, 100, 100),
            //    Cursor    = Cursors.Hand,
            //    TabStop   = false
            //};
            //btnOjo.FlatAppearance.BorderSize = 1;
            //btnOjo.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
            //btnOjo.Click += (s, e) =>
            //    txtContraseña.PasswordChar = txtContraseña.PasswordChar == '\0' ? '●' : '\0';
            //pnlCard.Controls.Add(btnOjo);
            //btnOjo.BringToFront();
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
                // No hay else: credenciales inválidas y bloqueos son manejados
                // por excepciones en BLL.Usuario.Login() → caen al catch de abajo
            }
            catch (Exception ex)
            {
                lblError.Text = ex.Message;
                txtContraseña.Clear();
                txtContraseña.Focus();
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
