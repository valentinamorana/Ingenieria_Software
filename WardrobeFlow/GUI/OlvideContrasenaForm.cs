using System;
using System.Drawing;
using System.Windows.Forms;
using DAL;

namespace GUI
{
    /// <summary>
    /// Formulario de recuperación de contraseña para empleados.
    ///
    /// Como WardrobeFlow es un portal de escritorio de red interna (sin correo),
    /// la recuperación se gestiona a través del administrador del sistema.
    ///
    /// Flujo:
    ///   1. El empleado ingresa su username y presiona "Enviar solicitud"
    ///   2. El sistema verifica que el username existe en la BD
    ///   3. Muestra un mensaje indicando que debe contactar al administrador
    ///   4. El administrador resetea la clave desde Menú → Administrar → Usuarios
    /// </summary>
    public partial class OlvideContrasenaForm : Form
    {
        private TextBox txtUsername;
        private Button  btnEnviar;
        private Button  btnCerrar;
        private Label   lblMensaje;

        public OlvideContrasenaForm()
        {
            InitializeComponent();

            this.Text            = "Recuperar Contraseña";
            this.ClientSize      = new Size(380, 300);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.BackColor       = Color.White;

            // ── Barra de acento superior ──────────────────────────────────────
            var accent = new Label
            {
                BackColor = Color.FromArgb(64, 0, 64),
                Dock      = DockStyle.Top,
                Height    = 4
            };

            // ── Título ────────────────────────────────────────────────────────
            var lblTitulo = new Label
            {
                Text      = "Recuperar Contraseña",
                Font      = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 60),
                Left      = 24, Top    = 22,
                Width     = 332, Height = 28
            };

            // ── Descripción ───────────────────────────────────────────────────
            var lblDesc = new Label
            {
                Text      = "Ingresá tu nombre de usuario. Un administrador\npodrá resetear tu contraseña desde el sistema.",
                Font      = new Font("Segoe UI", 9),
                ForeColor = Color.DimGray,
                Left      = 24, Top    = 58,
                Width     = 332, Height = 40
            };

            // ── Campo username ────────────────────────────────────────────────
            var lblUser = new Label
            {
                Text      = "Nombre de usuario:",
                Font      = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(80, 80, 80),
                Left      = 24, Top    = 108,
                Width     = 332, Height = 18
            };

            txtUsername = new TextBox
            {
                Left        = 24,  Top    = 128,
                Width       = 332, Height = 25,
                Font        = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor   = Color.FromArgb(245, 245, 248)
            };

            // ── Label de resultado (oculto hasta el envío) ────────────────────
            lblMensaje = new Label
            {
                Left      = 24,  Top    = 164,
                Width     = 332, Height = 52,
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = Color.DarkGreen,
                Text      = string.Empty
            };

            // ── Botones ───────────────────────────────────────────────────────
            btnEnviar = new Button
            {
                Text      = "Enviar solicitud",
                Left      = 24,  Top    = 224,
                Width     = 160, Height = 34,
                BackColor = Color.FromArgb(64, 0, 64),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btnEnviar.FlatAppearance.BorderSize = 0;
            btnEnviar.Click += BtnEnviar_Click;

            btnCerrar = new Button
            {
                Text      = "Cerrar",
                Left      = 196, Top    = 224,
                Width     = 160, Height = 34,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9),
                Cursor    = Cursors.Hand,
                BackColor = Color.FromArgb(240, 240, 240),
                ForeColor = Color.FromArgb(80, 80, 80)
            };
            btnCerrar.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
            btnCerrar.Click += (s, e) => this.Close();

            this.AcceptButton = btnEnviar;
            this.CancelButton = btnCerrar;

            this.Controls.AddRange(new Control[]
            {
                accent, lblTitulo, lblDesc,
                lblUser, txtUsername,
                lblMensaje,
                btnEnviar, btnCerrar
            });
        }

        /// <summary>
        /// Verifica que el username existe y muestra el mensaje de contacto al admin.
        /// No resetea la contraseña aquí — eso lo hace el Administrador desde Usuarios.
        /// </summary>
        private void BtnEnviar_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();

            if (string.IsNullOrWhiteSpace(username))
            {
                MostrarError("Ingresá tu nombre de usuario.");
                return;
            }

            try
            {
                // Verificar si el usuario existe en la BD
                var bll     = new BLL.Usuario();
                bool existe = bll.ExisteUsername(username);

                if (!existe)
                {
                    MostrarError($"No se encontró el usuario '{username}'.\nVerificá que escribiste tu nombre correctamente.");
                    return;
                }

                // Usuario encontrado — registrar en bitácora (criticidad 5) e indicar al empleado
                try
                {
                    var dal = new DAL.Bitacora();
                    dal.Registrar(new BE.Bitacora
                    {
                        Fecha      = DateTime.Now,
                        IdUsuario  = 0,
                        Modulo     = "Recuperar Contrasena",
                        Actividad  = "Solicitud Recuperacion Clave",
                        Criticidad = BE.Criticidad.RecuperacionClave,
                        Detalle    = $"Solicitud de recuperacion de clave para '{username}' " +
                                     $"a las {DateTime.Now:HH:mm:ss}."
                    });
                }
                catch { }

                lblMensaje.ForeColor = Color.FromArgb(30, 120, 60);
                lblMensaje.Text =
                    $"Usuario '{username}' encontrado.\n" +
                    "Contacta al administrador para que resetee\n" +
                    "tu contrasena desde Administrar -> Usuarios.";

                btnEnviar.Enabled = false;
            }
            catch (Exception ex)
            {
                MostrarError($"Error al verificar el usuario: {ex.Message}");
            }
        }

        private void MostrarError(string mensaje)
        {
            lblMensaje.ForeColor = Color.Crimson;
            lblMensaje.Text      = mensaje;
        }
    }
}
