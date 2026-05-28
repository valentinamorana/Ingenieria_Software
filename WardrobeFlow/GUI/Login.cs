using BLL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Reflection;
using System.Windows.Forms;
using Servicios.Multiidioma;

namespace GUI
{
    /// <summary>
    /// WardrobeFlow — Formulario de Login para Empleados.
    ///
    /// PATRÓN OBSERVER — T05 Gestión de Múltiples Idiomas:
    ///   Implementa IIdiomaObserver. Se suscribe al GestorIdioma en Load
    ///   y se desuscribe en FormClosing. Al recibir UpdateLanguage() aplica
    ///   las traducciones del nuevo idioma a todos sus controles.
    ///   Las pastillas de idioma (ES / EN / RU) viven en el panel izquierdo.
    ///   Al cambiar el idioma acá, el Menu ya abre traducido cuando el usuario ingresa.
    /// </summary>
    public partial class Login : Form, IIdiomaObserver
    {
        private readonly Usuario usuarioBLL = new Usuario();

        // Botones-pastilla de idioma — índice por código para soporte dinámico
        private readonly Dictionary<string, Button> _loginBtnsIdioma = new Dictionary<string, Button>();
        // Etiqueta de descripción de marca (creada en código para ser traducible)
        private Label _lblBrandDesc;

        public Login()
        {
            InitializeComponent();

            // ── Decoraciones dibujadas mediante eventos Paint ─────────────────────
            // Evita BackgroundImage bitmaps y permite que los controles transparentes
            // muestren correctamente el fondo #FBF0F6 del panel derecho.
            pnlLeft.Paint += PnlLeft_Paint;
            pnlCard.Paint += PnlCard_Paint;

            // ── Elementos de marca en el panel izquierdo ──────────────────────────
            AgregarBrandElements();

            // ── Ojito mostrar/ocultar contraseña ─────────────────────────────────
            // Se achica el textbox para que el botón quede en el borde derecho.
            txtContraseña.Width -= 28;
            var btnOjo = new Button
            {
                Text      = "👁",
                Font      = new Font("Segoe UI Emoji", 9f),
                Size      = new Size(26, txtContraseña.Height),
                Location  = new Point(txtContraseña.Right + 2, txtContraseña.Top),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(192, 168, 180),
                Cursor    = Cursors.Hand,
                TabStop   = false
            };
            btnOjo.FlatAppearance.BorderSize = 0;
            btnOjo.Click += (s, e) =>
            {
                var b = (Button)s;
                if (txtContraseña.PasswordChar == '\0')
                {
                    txtContraseña.PasswordChar = '●';
                    b.Font = new Font("Segoe UI Emoji", 9f);
                }
                else
                {
                    txtContraseña.PasswordChar = '\0';
                    b.Font = new Font("Segoe UI Emoji", 9f, FontStyle.Strikeout);
                }
            };
            pnlCard.Controls.Add(btnOjo);
            btnOjo.BringToFront();

            // ── Líneas del separador "o" dibujadas vía Paint ──────────────────────
            lblDivider.Paint += LblDivider_Paint;

            // ── Pastillas de idioma en el panel izquierdo ─────────────────────────
            AgregarBotonesIdioma();

            this.AcceptButton = btnIngresar;

            lblError.AutoSize    = false;
            lblError.MaximumSize = new Size(lblError.Width, 0);
        }

        // ── Pintura decorativa del panel izquierdo ────────────────────────────────
        // Fondo blanco + círculos rosados suaves (equivalente al SVG del HTML de referencia).

        private void PnlLeft_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Círculo relleno superior-derecho  #F9EEF4
            using (var b = new SolidBrush(Color.FromArgb(249, 238, 244)))
                g.FillEllipse(b, 120, -30, 220, 220);

            // Círculo relleno inferior-izquierdo  #F5E6EF
            using (var b = new SolidBrush(Color.FromArgb(245, 230, 239)))
                g.FillEllipse(b, -65, 295, 260, 260);

            // Círculos de contorno suaves
            using (var pen = new Pen(Color.FromArgb(232, 197, 216), 1f))
                g.DrawEllipse(pen, 145, 218, 110, 110);
            using (var pen = new Pen(Color.FromArgb(237, 213, 229), 0.8f))
                g.DrawEllipse(pen, 30, 100, 60, 60);

            // Puntos decorativos
            foreach (var (dx, dy, a) in new[] { (80, 80, 80), (110, 60, 60), (210, 200, 90), (150, 380, 90) })
                using (var b = new SolidBrush(Color.FromArgb(a, 192, 130, 168)))
                    g.FillEllipse(b, dx - 3, dy - 3, 7, 7);
        }

        // ── Pintura decorativa del panel derecho ──────────────────────────────────
        // Fondo #FBF0F6 + círculos vino muy sutiles (alpha ≈ 5-10%).

        private void PnlCard_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int w = pnlCard.Width, h = pnlCard.Height;

            // Círculo superior-derecho
            using (var b = new SolidBrush(Color.FromArgb(14, 146, 62, 96)))
                g.FillEllipse(b, w - 170, -90, 180, 180);

            // Círculo inferior-izquierdo
            using (var b = new SolidBrush(Color.FromArgb(11, 146, 62, 96)))
                g.FillEllipse(b, -80, h - 150, 200, 200);

            // Círculo de contorno
            using (var pen = new Pen(Color.FromArgb(25, 146, 62, 96), 0.8f))
                g.DrawEllipse(pen, w - 100, 220, 80, 80);
        }

        // ── Líneas horizontales del separador "o" ─────────────────────────────────

        private void LblDivider_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            var lbl = (Label)sender;
            int midY = lbl.Height / 2;
            using (var pen = new Pen(Color.FromArgb(224, 200, 216), 0.8f))
            {
                var sz = e.Graphics.MeasureString(lbl.Text, lbl.Font);
                float cx = lbl.Width / 2f;
                float hw = sz.Width / 2f + 10;
                e.Graphics.DrawLine(pen, 0, midY, cx - hw, midY);
                e.Graphics.DrawLine(pen, cx + hw, midY, lbl.Width, midY);
            }
        }

        // ── Elementos de marca en pnlLeft ─────────────────────────────────────────

        private void AgregarBrandElements()
        {
            // 1. Ícono-logo (cuadrado vino redondeado 36×36 con "W" blanca)
            var pnlLogo = new Panel { Location = new Point(20, 24), Size = new Size(36, 36), BackColor = Color.White };
            pnlLogo.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Fondo vino redondeado
                using (var path = BuildRoundedRect(new Rectangle(0, 0, 35, 35), 8))
                using (var br   = new SolidBrush(Color.FromArgb(146, 62, 96)))
                    g.FillPath(br, path);

                // Ícono de percha blanca (equivalente a ti-hanger de Tabler Icons)
                using (var pen = new Pen(Color.White, 1.8f) {
                    StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
                {
                    // Círculo / anillo superior (la argolla de la percha)
                    g.DrawEllipse(pen, 14.5f, 6f, 6f, 6f);
                    // Arco del gancho: sale de la argolla hacia la derecha (el gancho sobre el caño)
                    g.DrawArc(pen, 17.5f, 5f, 5f, 5f, 180, -200);
                    // Brazo izquierdo
                    g.DrawLine(pen, 17.5f, 12f, 9f, 26f);
                    // Brazo derecho
                    g.DrawLine(pen, 17.5f, 12f, 26f, 26f);
                    // Barra inferior (se extiende más allá de los brazos)
                    g.DrawLine(pen, 5.5f, 26f, 29.5f, 26f);
                }
            };
            pnlLeft.Controls.Add(pnlLogo);
            pnlLogo.BringToFront();

            // lblTitle ya está en pnlLeft (del Designer): solo reposicionar si es necesario
            lblTitle.BringToFront();

            // 2. Wordmark "Wardrobe" + "Flow" en dos colores (panel con Paint)
            var pnlWordmark = new Panel { Location = new Point(18, 200), Size = new Size(244, 40), BackColor = Color.White };
            pnlWordmark.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode     = SmoothingMode.AntiAlias;
                e.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                using (var fnt = new Font("Segoe UI", 19f))
                {
                    using (var bDark = new SolidBrush(Color.FromArgb(44, 26, 36)))
                        e.Graphics.DrawString("Wardrobe", fnt, bDark, 0, 0);
                    // Posicionar "Flow" inmediatamente después de "Wardrobe"
                    var sz = e.Graphics.MeasureString("Wardrobe", fnt);
                    using (var bVino = new SolidBrush(Color.FromArgb(146, 62, 96)))
                        e.Graphics.DrawString("Flow", fnt, bVino, sz.Width - 4f, 0);
                }
            };
            pnlLeft.Controls.Add(pnlWordmark);
            pnlWordmark.BringToFront();

            // 3. Descripción de marca (traducible vía tag)
            _lblBrandDesc = new Label
            {
                Location   = new Point(20, 246),
                Size       = new Size(240, 38),
                Font       = new Font("Segoe UI", 9f),
                ForeColor  = Color.FromArgb(160, 136, 152),
                BackColor  = Color.White,
                Text       = "Acceso seguro y centralizado\na todos los módulos del sistema.",
                Tag        = "lbl.brand.desc",
                AutoSize   = false
            };
            pnlLeft.Controls.Add(_lblBrandDesc);
            _lblBrandDesc.BringToFront();
        }

        // Construye un GraphicsPath de rectángulo redondeado.
        private static GraphicsPath BuildRoundedRect(Rectangle rect, int r)
        {
            var path = new GraphicsPath();
            path.AddArc(rect.Left,            rect.Top,             r * 2, r * 2, 180, 90);
            path.AddArc(rect.Right - r * 2,   rect.Top,             r * 2, r * 2, 270, 90);
            path.AddArc(rect.Right - r * 2,   rect.Bottom - r * 2, r * 2, r * 2, 0,   90);
            path.AddArc(rect.Left,            rect.Bottom - r * 2, r * 2, r * 2, 90,  90);
            path.CloseFigure();
            return path;
        }

        // ── Pastillas de idioma en pnlLeft ────────────────────────────────────────

        private void AgregarBotonesIdioma()
        {
            ConstruirBotonesIdioma(Traductor.ObtenerIdiomas());
            MarcarIdiomaActivoLogin(GestorIdioma.IdiomaActual?.Id ?? "ES");
        }

        private void ConstruirBotonesIdioma(IList<Idioma> idiomas)
        {
            foreach (var btn in _loginBtnsIdioma.Values)
                pnlLeft.Controls.Remove(btn);
            _loginBtnsIdioma.Clear();

            int y = pnlLeft.Height - 54;
            int x = 22;

            foreach (var idioma in idiomas)
            {
                var btn = new Button
                {
                    Text      = idioma.Id,
                    Size      = new Size(40, 22),
                    Location  = new Point(x, y),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(176, 136, 152),
                    Font      = new Font("Segoe UI", 8f),
                    Cursor    = Cursors.Hand,
                    TabStop   = false
                };
                btn.FlatAppearance.BorderSize  = 1;
                btn.FlatAppearance.BorderColor = Color.FromArgb(224, 200, 216);
                x += 46;

                string cod = idioma.Id;
                btn.Click += (s, e) =>
                {
                    foreach (var idm in Traductor.ObtenerIdiomas())
                        if (idm.Id == cod)
                        { GestorIdioma.CambiarIdioma(idm); break; }
                    MarcarIdiomaActivoLogin(cod);
                };

                pnlLeft.Controls.Add(btn);
                btn.BringToFront();
                _loginBtnsIdioma[idioma.Id] = btn;
            }
        }

        private void MarcarIdiomaActivoLogin(string codigo)
        {
            void Marcar(Button b, bool activo)
            {
                if (b == null) return;
                b.Font      = new Font("Segoe UI", 8f, activo ? FontStyle.Bold : FontStyle.Regular);
                b.ForeColor = activo ? Color.FromArgb(146, 62, 96) : Color.FromArgb(176, 136, 152);
                b.BackColor = activo ? Color.FromArgb(243, 234, 240) : Color.White;
                b.FlatAppearance.BorderColor = activo
                    ? Color.FromArgb(201, 160, 186)
                    : Color.FromArgb(224, 200, 216);
            }
            foreach (var kv in _loginBtnsIdioma)
                Marcar(kv.Value, kv.Key == codigo);
        }

        // ── Ciclo de vida ─────────────────────────────────────────────────────────

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GestorIdioma.SuscribirObservador(this);

            try
            {
                var idiomas = new BLL.IdiomaService().ObtenerIdiomasActivosComoIdioma();
                if (idiomas.Count > 0)
                {
                    GestorIdioma.SetIdiomasDisponibles(idiomas);
                    ConstruirBotonesIdioma(idiomas);
                    MarcarIdiomaActivoLogin(GestorIdioma.IdiomaActual.Id);
                }
            }
            catch { /* sin conexión: usa botones hardcodeados del constructor */ }

            Traducir(GestorIdioma.IdiomaActual);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            GestorIdioma.DesuscribirObservador(this);
            base.OnFormClosing(e);
        }

        // ══════════════════════════════════════════════════════════════════════════
        // PATRÓN OBSERVER — T05 Gestión de Múltiples Idiomas
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Recibe la notificación del GestorIdioma cuando el idioma cambia.
        /// Equivalente a UpdateLanguage(IIdioma idioma) del ejemplo de cátedra.
        /// </summary>
        public void UpdateLanguage(Idioma idioma)
        {
            Traducir(idioma);
        }

        /// <summary>
        /// Reasigna el .Text de cada control leyendo su propiedad Tag como clave
        /// de traducción — exactamente igual que en el ejemplo de cátedra (frmLogin).
        /// </summary>
        private void Traducir(Idioma idioma)
        {
            var t = Traductor.ObtenerTraducciones(idioma);
            string Tx(string tag) => (tag != null && t.ContainsKey(tag)) ? t[tag].Texto : null;

            // Título del formulario
            var tituloForm = Tx(this.Tag?.ToString());
            if (tituloForm != null) this.Text = tituloForm;

            // Panel izquierdo
            var sub = Tx(lblSubtitulo.Tag?.ToString());
            if (sub != null) lblSubtitulo.Text = sub;

            var desc = Tx(_lblBrandDesc?.Tag?.ToString());
            if (desc != null && _lblBrandDesc != null) _lblBrandDesc.Text = desc;

            // Panel derecho — título y subtítulo
            var bienvenido = Tx(lblAccent.Tag?.ToString());
            if (bienvenido != null) lblAccent.Text = bienvenido;

            var cred = Tx(lblLoginSub.Tag?.ToString());
            if (cred != null) lblLoginSub.Text = cred;

            // Campos
            var usr = Tx(lblUsuario.Tag?.ToString());
            if (usr != null) lblUsuario.Text = usr;

            var pwd = Tx(lblContraseña.Tag?.ToString());
            if (pwd != null) lblContraseña.Text = pwd;

            // Botones y link
            var ingresar = Tx(btnIngresar.Tag?.ToString());
            if (ingresar != null) btnIngresar.Text = ingresar;

            var salir = Tx(btnSalir.Tag?.ToString());
            if (salir != null) btnSalir.Text = salir;

            var olvide = Tx(lnkOlvidaste.Tag?.ToString());
            if (olvide != null) lnkOlvidaste.Text = olvide;

            // Separador
            var divider = Tx(lblDivider.Tag?.ToString());
            if (divider != null) lblDivider.Text = divider;
        }

        // ── Eventos de negocio ────────────────────────────────────────────────────

        private void btnIngresar_Click(object sender, EventArgs e)
        {
            lblError.Text = string.Empty;
            var t = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
            string Tx(string key, string fallback) => t.ContainsKey(key) ? t[key].Texto : fallback;

            try
            {
                bool esValido = usuarioBLL.Login(this.Text, txtUsuario.Text, txtContraseña.Text);
                if (esValido)
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    // Usuario no encontrado — mismo mensaje que credenciales inválidas (evita enumeración)
                    MostrarErrorLogin(Tx("err.login.credenciales", "Usuario o contraseña incorrectos."), false);
                }
            }
            catch (BE.LoginException ex) when (ex.Tipo == BE.LoginException.TipoError.LimiteAlcanzado)
            {
                string titulo = Tx("dlg.login.sesion.titulo", "Sesión terminada");
                string cuerpo = Tx("err.login.limitesesion",  "Demasiados intentos fallidos en esta sesión.");
                string cierre = Tx("dlg.login.sesion.cierre", "La aplicación se cerrará.");
                MessageBox.Show(cuerpo + "\n\n" + cierre, titulo, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                Application.Exit();
            }
            catch (BE.LoginException ex) when (ex.Tipo == BE.LoginException.TipoError.CuentaBloqueada)
            {
                MostrarErrorLogin(Tx("err.login.bloqueada", ex.Message), bloqueado: true);
            }
            catch (BE.LoginException ex) when (ex.Tipo == BE.LoginException.TipoError.CredencialesInvalidas)
            {
                string msg = ex.IntentosRestantes.HasValue
                    ? string.Format(Tx("err.login.intentos", "Usuario o contraseña incorrectos.\nIntentos restantes: {0}."), ex.IntentosRestantes.Value)
                    : Tx("err.login.credenciales", "Usuario o contraseña incorrectos.");
                MostrarErrorLogin(msg, bloqueado: false);
            }
            catch (BE.LoginException ex) when (ex.Tipo == BE.LoginException.TipoError.CamposVacios)
            {
                MostrarErrorLogin(Tx("err.login.camposvacio", ex.Message), bloqueado: false);
            }
            catch (BE.LoginException ex)
            {
                MostrarErrorLogin(ex.Message, bloqueado: false);
            }
        }

        private void MostrarErrorLogin(string mensaje, bool bloqueado)
        {
            lblError.Text      = mensaje;
            lblError.ForeColor = bloqueado
                ? Color.FromArgb(140, 0, 0)
                : Color.FromArgb(180, 50, 50);

            if (bloqueado)
            {
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

        private void btnSalir_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void lnkOlvidaste_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            using (var form = new OlvideContrasenaForm())
                form.ShowDialog(this);
        }
    }
}
