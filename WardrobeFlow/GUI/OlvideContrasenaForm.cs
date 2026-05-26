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
            var t = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
            string T_o(string k, string fb) => t.ContainsKey(k) ? t[k].Texto : fb;

            if (string.IsNullOrWhiteSpace(username))
            {
                MostrarError(T_o("err.recup.nousername", "Ingresá tu nombre de usuario."));
                return;
            }

            try
            {
                bool existe = new BLL.Usuario().SolicitarRecuperacionClave(username);

                if (!existe)
                {
                    MostrarError(string.Format(
                        T_o("err.recup.nousuario",
                            "No se encontró el usuario '{0}'.\nVerificá que escribiste tu nombre correctamente."),
                        username));
                    return;
                }

                lblMensaje.ForeColor = Color.FromArgb(30, 120, 60);
                lblMensaje.Text = string.Format(
                    T_o("msg.recup.exito",
                        "Usuario '{0}' encontrado.\nContacta al administrador para que resetee\ntu contrasena desde Administrar -> Usuarios."),
                    username);

                btnEnviar.Enabled = false;
            }
            catch (Exception ex)
            {
                MostrarError(string.Format(T_o("err.recup.verificar", "Error al verificar el usuario: {0}"), ex.Message));
            }
        }

    }
}
