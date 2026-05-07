using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Servicios.Multiidioma;

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
    /// <summary>
    /// Hereda de <see cref="FormBase"/>:
    ///   - MostrarError() → heredado, no se redeclara
    ///   - MensajeLabel → sobreescrito para devolver el lblMensaje de este formulario
    /// </summary>
    public partial class OlvideContrasenaForm : FormBase, IIdiomaObserver
    {
        protected override Label MensajeLabel => lblMensaje;

        public OlvideContrasenaForm()
        {
            InitializeComponent();
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
            var t = Traductor.ObtenerTraducciones(idioma);
            if (this.Tag != null && t.ContainsKey(this.Tag.ToString()))
                this.Text = t[this.Tag.ToString()].Texto;
            Aplicar(lblTitulo, t);
            Aplicar(lblDesc,   t);
            Aplicar(lblUser,   t);
            Aplicar(btnEnviar, t);
            Aplicar(btnCerrar, t);
        }

        private static void Aplicar(Control c, IDictionary<string, Traduccion> t)
        {
            if (c?.Tag != null && t.ContainsKey(c.Tag.ToString()))
                c.Text = t[c.Tag.ToString()].Texto;
        }

        // ── Eventos ───────────────────────────────────────────────────────────

        private void BtnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
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

                // Usuario encontrado — registrar en bitácora sin sesión activa
                new Servicios.Bitacora().RegistrarSinSesion(
                    modulo:     "Recuperar Contrasena",
                    actividad:  "Solicitud Recuperacion Clave",
                    criticidad: BE.Criticidad.RecuperacionClave,
                    detalle:    $"Solicitud de recuperacion de clave para '{username}' a las {DateTime.Now:HH:mm:ss}."
                );

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

    }
}
