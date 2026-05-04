using BLL;
using System;
using System.Drawing;
using System.Windows.Forms;
using Servicios.Multiidioma;

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
    ///
    /// PATRÓN OBSERVER — T05 Gestión de Múltiples Idiomas:
    ///   Implementa IIdiomaObserver. Se suscribe al GestorIdioma en Load
    ///   y se desuscribe en FormClosing. Al recibir UpdateLanguage() aplica
    ///   las traducciones del nuevo idioma a todos sus controles.
    ///   El cambio de idioma ocurre desde la barra del Menu principal,
    ///   sin abrir ningún formulario adicional.
    /// </summary>
    public partial class Login : Form, IIdiomaObserver
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

            // lblError necesita más altura para mostrar mensajes de bloqueo (multi-línea).
            // Se desplazan los controles de abajo para hacer lugar.
            int extra = 36;
            lblError.Height    += extra;
            btnIngresar.Top    += extra;
            lnkOlvidaste.Top   += extra;
            btnSalir.Top       += extra;
            pnlCard.Height     += extra;

            // Ojito mostrar/ocultar contraseña — se achica el textbox para que el botón quede afuera del borde
            txtContraseña.Width -= 28;
            var btnOjo = new Button
            {
                Text      = "👁",
                Font      = new Font("Segoe UI Emoji", 9f),
                Size      = new Size(26, txtContraseña.Height),
                Location  = new Point(txtContraseña.Right + 2, txtContraseña.Top),
                FlatStyle = FlatStyle.Flat,
                BackColor = txtContraseña.BackColor,
                ForeColor = Color.FromArgb(100, 100, 100),
                Cursor    = Cursors.Hand,
                TabStop   = false
            };
            btnOjo.FlatAppearance.BorderSize = 1;
            btnOjo.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
            btnOjo.Click += (s, e) =>
                txtContraseña.PasswordChar = txtContraseña.PasswordChar == '\0' ? '●' : '\0';
            pnlCard.Controls.Add(btnOjo);
            btnOjo.BringToFront();
        }

        // ── Eventos del ciclo de vida ─────────────────────────────────────────

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // Suscribirse al Observer de idioma — equivalente a frmLogin_Load del ejemplo de cátedra
            GestorIdioma.SuscribirObservador(this);
            // Aplicar el idioma activo en el momento en que se abre el formulario
            Traducir(GestorIdioma.IdiomaActual);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Desuscribirse al cerrar — equivalente a frmLogin_FormClosing del ejemplo de cátedra
            GestorIdioma.DesuscribirObservador(this);
            base.OnFormClosing(e);
        }

        // ══════════════════════════════════════════════════════════════════════
        // PATRÓN OBSERVER — T05 Gestión de Múltiples Idiomas
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Recibe la notificación del GestorIdioma cuando el idioma cambia.
        /// Equivalente a UpdateLanguage(IIdioma idioma) del ejemplo de cátedra.
        /// El formulario se traduce al instante sin reinicios ni ventanas adicionales.
        /// </summary>
        public void UpdateLanguage(Idioma idioma)
        {
            Traducir(idioma);
        }

        /// <summary>
        /// Reasigna el .Text de cada control leyendo su propiedad Tag como clave
        /// de traducción — exactamente igual que en el ejemplo de cátedra (frmLogin).
        /// Los Tags se asignan en el Designer; el código no hardcodea ninguna clave.
        /// </summary>
        private void Traducir(Idioma idioma)
        {
            var t = Traductor.ObtenerTraducciones(idioma);

            // Título del formulario — Tag asignado en el Designer: "frm.login"
            if (this.Tag != null && t.ContainsKey(this.Tag.ToString()))
                this.Text = t[this.Tag.ToString()].Texto;

            // Controles — cada uno lee su propio Tag para obtener la clave
            if (lblUsuario.Tag   != null && t.ContainsKey(lblUsuario.Tag.ToString()))
                lblUsuario.Text   = t[lblUsuario.Tag.ToString()].Texto;

            if (lblContraseña.Tag != null && t.ContainsKey(lblContraseña.Tag.ToString()))
                lblContraseña.Text = t[lblContraseña.Tag.ToString()].Texto;

            if (btnIngresar.Tag  != null && t.ContainsKey(btnIngresar.Tag.ToString()))
                btnIngresar.Text  = t[btnIngresar.Tag.ToString()].Texto;

            if (btnSalir.Tag     != null && t.ContainsKey(btnSalir.Tag.ToString()))
                btnSalir.Text     = t[btnSalir.Tag.ToString()].Texto;

            if (lnkOlvidaste.Tag != null && t.ContainsKey(lnkOlvidaste.Tag.ToString()))
                lnkOlvidaste.Text = t[lnkOlvidaste.Tag.ToString()].Texto;
        }

        // ── Eventos de negocio ────────────────────────────────────────────────

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
                bool bloqueado = ex.Message.Contains("bloqueada");

                lblError.Text      = ex.Message;
                lblError.ForeColor = bloqueado
                    ? Color.FromArgb(140, 0, 0)
                    : Color.FromArgb(180, 50, 50);

                if (bloqueado)
                {
                    // Cuenta bloqueada: deshabilitar todo para que no siga intentando
                    txtUsuario.Enabled    = false;
                    txtContraseña.Enabled = false;
                    btnIngresar.Enabled   = false;
                    this.AcceptButton     = null;
                }
                else
                {
                    txtContraseña.Clear();
                    txtContraseña.Focus();
                }
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
