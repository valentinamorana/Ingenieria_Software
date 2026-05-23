using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Servicios.Multiidioma;

namespace GUI
{
    /// <summary>
    /// Capa de Presentación — Formulario de Gestión de Usuarios.
    ///
    /// Permite administrar los usuarios del sistema: ver la lista completa,
    /// crear nuevos usuarios y resetear contraseñas (solo Administrador).
    ///
    /// Se abre como formulario hijo MDI desde el Menú → Administrar → Usuarios.
    ///
    /// CUMPLE REQUISITOS T02:
    ///   ✓ Gestión de usuarios del sistema
    ///   ✓ Las contraseñas se hashean con PBKDF2 antes de guardarse
    ///   ✓ Se registra la actividad en la bitácora (via BLL)
    ///   ✓ Solo un Administrador puede resetear contraseñas ajenas
    /// </summary>
    /// <summary>
    /// Hereda de <see cref="FormBase"/>:
    ///   - MostrarOk() y MostrarError() → heredados, no se redeclaran
    ///   - MensajeLabel → sobreescrito para devolver el lblMensaje de este formulario
    /// </summary>
    public partial class Usuarios : FormBase, IIdiomaObserver
    {
        protected override Label MensajeLabel => lblMensaje;

        // BLL de usuarios para operaciones de negocio
        private readonly BLL.Usuario usuarioBLL = new BLL.Usuario();

        // Idioma activo — sincronizado en Traducir() para usar en CargarUsuarios
        private Idioma _idioma = GestorIdioma.IdiomaActual;

        /// <summary>
        /// Constructor: inicializa el formulario y construye la interfaz de gestión de usuarios.
        /// </summary>
        public Usuarios()
        {
            InitializeComponent();
            this.Load += new EventHandler(Usuarios_Load);
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

        public void UpdateLanguage(Idioma idioma)
        {
            Traducir(idioma);
            // Refrescar grilla para que headers y valores de estado reflejen el nuevo idioma
            CargarUsuarios();
        }

        private void Traducir(Idioma idioma)
        {
            _idioma = idioma;
            var t = Traductor.ObtenerTraducciones(idioma);
            if (this.Tag != null && t.ContainsKey(this.Tag.ToString()))
                this.Text = t[this.Tag.ToString()].Texto;
            Aplicar(lblTitulo,            t);
            Aplicar(lblUser,              t);
            Aplicar(lblPass,              t);
            Aplicar(lblPerfil,            t);
            Aplicar(btnAgregar,           t);
            Aplicar(btnRefrescar,         t);
            Aplicar(lblResetTitulo,       t);
            Aplicar(lblResetInfo,         t);
            Aplicar(btnResetearClave,     t);
            Aplicar(lblDesbloquearTitulo, t);
            Aplicar(lblDesbloquearInfo,   t);
            Aplicar(btnDesbloquear,       t);
            Aplicar(lblListaTitulo,       t);
            TraducirHeadersGrilla();
        }

        /// <summary>Traduce los HeaderText de la grilla de usuarios según el idioma activo.</summary>
        private void TraducirHeadersGrilla()
        {
            var t = Traductor.ObtenerTraducciones(_idioma);
            void RH(string col, string clave, string fallback)
            {
                if (dgvUsuarios.Columns.Contains(col) && t.ContainsKey(clave))
                    dgvUsuarios.Columns[col].HeaderText = t[clave].Texto;
                else if (dgvUsuarios.Columns.Contains(col))
                    dgvUsuarios.Columns[col].HeaderText = fallback;
            }
            RH("Username", "col.usr.username", "Usuario");
            RH("Perfil",   "col.usr.perfil",   "Perfil");
            RH("Estado",   "col.usr.estado",   "Estado");

            // Ocultar columna interna de clave de bloqueo
            if (dgvUsuarios.Columns.Contains("_BloqueadoKey"))
                dgvUsuarios.Columns["_BloqueadoKey"].Visible = false;
        }

        private static void Aplicar(Control c, IDictionary<string, Traduccion> t)
        {
            if (c?.Tag != null && t.ContainsKey(c.Tag.ToString()))
                c.Text = t[c.Tag.ToString()].Texto;
        }

        // ── Eventos del Designer ──────────────────────────────────────────────

        private void Usuarios_Load(object sender, EventArgs e)
        {
            // Se crean aquí (en Load, no en constructor) para que la escala DPI del
            // formulario ya esté aplicada y las posiciones/tamaños sean correctos.

            // Botón reset masivo — debajo del último control del panel
            var btnResetMasivo = new Button
            {
                Text      = "Resetear todas las claves a temporal",
                Size      = new Size(216, 30),
                Location  = new Point(12, btnDesbloquear.Bottom + 20),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(80, 80, 80),
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btnResetMasivo.FlatAppearance.BorderSize = 0;
            btnResetMasivo.Click += BtnResetMasivo_Click;
            panelAlta.Controls.Add(btnResetMasivo);

            // Botón recalcular DV — T07: regenera DVH y DVV para todas las filas de Usuario
            var btnRecalcularDV = new Button
            {
                Text      = "Recalcular integridad (DV)",
                Size      = new Size(216, 30),
                Location  = new Point(12, btnResetMasivo.Bottom + 8),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 80, 140),
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btnRecalcularDV.FlatAppearance.BorderSize = 0;
            btnRecalcularDV.Click += BtnRecalcularDV_Click;
            panelAlta.Controls.Add(btnRecalcularDV);

            // Ojito mostrar/ocultar contraseña — se posiciona a la derecha del textbox
            // una vez que el layout ya tiene las medidas escaladas.
            txtContraseña.Width -= 28;
            var btnOjo = new Button
            {
                Text      = "👁",
                Font      = new Font("Segoe UI", 11f),
                Size      = new Size(28, txtContraseña.Height),
                Location  = new Point(txtContraseña.Right + 2, txtContraseña.Top),
                FlatStyle = FlatStyle.Flat,
                BackColor = txtContraseña.BackColor,
                ForeColor = Color.FromArgb(80, 80, 80),
                Cursor    = Cursors.Hand,
                TabStop   = false
            };
            btnOjo.FlatAppearance.BorderSize = 1;
            btnOjo.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
            btnOjo.Click += (s, e2) =>
                txtContraseña.PasswordChar = txtContraseña.PasswordChar == '\0' ? '●' : '\0';
            panelAlta.Controls.Add(btnOjo);
            btnOjo.BringToFront();

            CargarUsuarios();
        }

        private void BtnRefrescar_Click(object sender, EventArgs e)
        {
            CargarUsuarios();
        }

        private void DgvUsuarios_SelectionChanged(object sender, EventArgs e)
        {
            bool haySeleccion = dgvUsuarios.SelectedRows.Count > 0;
            btnResetearClave.Enabled = haySeleccion;

            // Desbloquear solo se habilita si el usuario seleccionado está bloqueado.
            // Usamos la columna interna _BloqueadoKey (int) para ser independientes del idioma.
            if (haySeleccion && dgvUsuarios.Columns.Contains("_BloqueadoKey"))
            {
                var bloqCell = dgvUsuarios.SelectedRows[0].Cells["_BloqueadoKey"];
                btnDesbloquear.Enabled = bloqCell?.Value?.ToString() == "1";
            }
            else
            {
                btnDesbloquear.Enabled = false;
            }
        }

        // ── Carga ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Carga la lista de usuarios desde la BLL → DAL y la muestra en la grilla.
        /// Las contraseñas NO se muestran por seguridad.
        /// </summary>
        private void CargarUsuarios()
        {
            try
            {
                List<BE.Usuario> usuarios = usuarioBLL.ObtenerTodos();

                var t = Traductor.ObtenerTraducciones(_idioma);
                string lblActivo    = t.ContainsKey("usr.activo")   ? t["usr.activo"].Texto   : "Activo";
                string lblBloqueada = t.ContainsKey("usr.bloqueada") ? t["usr.bloqueada"].Texto : "Bloqueada";

                var tabla = new DataTable();
                tabla.Columns.Add("ID",           typeof(int));
                tabla.Columns.Add("Username",     typeof(string));
                tabla.Columns.Add("Perfil",       typeof(string));
                tabla.Columns.Add("Estado",       typeof(string));
                // Columna interna: 1 = bloqueado, 0 = activo — independiente del idioma
                tabla.Columns.Add("_BloqueadoKey", typeof(int));

                foreach (var u in usuarios)
                    tabla.Rows.Add(
                        u.Id,
                        u.Username,
                        u.Perfil ?? "—",
                        u.Bloqueado ? lblBloqueada : lblActivo,
                        u.Bloqueado ? 1 : 0);

                dgvUsuarios.DataSource = tabla;
                TraducirHeadersGrilla();

                // Colorear filas bloqueadas usando la columna interna (independiente del idioma)
                foreach (DataGridViewRow fila in dgvUsuarios.Rows)
                {
                    if (fila.Cells["_BloqueadoKey"].Value?.ToString() == "1")
                    {
                        fila.DefaultCellStyle.BackColor = Color.FromArgb(255, 220, 220);
                        fila.DefaultCellStyle.ForeColor = Color.DarkRed;
                    }
                }

                string fmt = t.ContainsKey("msg.usr.cargados")
                    ? t["msg.usr.cargados"].Texto
                    : "{0} usuario(s) registrado(s).";
                lblMensaje.ForeColor = Color.DarkGreen;
                lblMensaje.Text      = string.Format(fmt, usuarios.Count);
            }
            catch (Exception ex)
            {
                MostrarError($"Error al cargar: {ex.Message}");
            }
        }

        // ── Eventos de botones ────────────────────────────────────────────────

        /// <summary>Crea un nuevo usuario. Las validaciones se delegan a BLL.Usuario.Alta().</summary>
        private void BtnAgregar_Click(object sender, EventArgs e)
        {
            string username  = txtUsername.Text.Trim();
            string password  = txtContraseña.Text;
            string perfil    = cmbPerfil.SelectedItem?.ToString() ?? "";

            try
            {
                usuarioBLL.Alta(this, username, password, perfil);

                // Limpiar campos y refrescar lista
                txtUsername.Clear();
                txtContraseña.Clear();
                cmbPerfil.SelectedIndex = 2;

                CargarUsuarios();
                var tCr = Traductor.ObtenerTraducciones(_idioma);
                string fmtCr = tCr.ContainsKey("msg.usr.creado") ? tCr["msg.usr.creado"].Texto : "Usuario '{0}' [{1}] creado correctamente.";
                MostrarOk(string.Format(fmtCr, username, perfil));
            }
            catch (Exception ex)
            {
                MostrarError(ex.Message);
            }
        }

        /// <summary>
        /// Resetea la contraseña del usuario seleccionado en la grilla.
        /// Pide la nueva contraseña mediante un diálogo.
        /// Solo funciona si el usuario en sesión es Administrador.
        /// </summary>
        private void BtnResetearClave_Click(object sender, EventArgs e)
        {
            if (dgvUsuarios.SelectedRows.Count == 0)
            {
                var tRSel = Traductor.ObtenerTraducciones(_idioma);
                MostrarError(tRSel.ContainsKey("err.usr.selecciona") ? tRSel["err.usr.selecciona"].Texto : "Seleccioná un usuario de la lista.");
                return;
            }

            int    idUsuario = Convert.ToInt32(dgvUsuarios.SelectedRows[0].Cells["ID"].Value);
            string username  = dgvUsuarios.SelectedRows[0].Cells["Username"].Value?.ToString() ?? "";

            var tR = Traductor.ObtenerTraducciones(_idioma);
            string T_r(string k, string fb) => tR.ContainsKey(k) ? tR[k].Texto : fb;
            string promptReset = string.Format(T_r("dlg.resetclave.prompt", "Nueva contraseña para '{0}' (mínimo 6 caracteres):"), username);
            string nuevaClave = PedirTexto(promptReset, T_r("frm.resetclave", "Resetear Contraseña"));

            if (string.IsNullOrWhiteSpace(nuevaClave)) return;

            try
            {
                usuarioBLL.ResetearClave(this, idUsuario, nuevaClave);
                MostrarOk(string.Format(T_r("msg.usr.clave.reseteada", "Contraseña de '{0}' reseteada correctamente."), username));
            }
            catch (Exception ex)
            {
                MostrarError(ex.Message);
            }
        }

        /// <summary>
        /// Desbloquea la cuenta del usuario seleccionado en la grilla.
        /// Solo funciona si el usuario en sesión es Administrador.
        /// </summary>
        private void BtnDesbloquear_Click(object sender, EventArgs e)
        {
            if (dgvUsuarios.SelectedRows.Count == 0)
            {
                var tDSel = Traductor.ObtenerTraducciones(_idioma);
                MostrarError(tDSel.ContainsKey("err.usr.sel.bloqueado") ? tDSel["err.usr.sel.bloqueado"].Texto : "Seleccioná un usuario bloqueado de la lista.");
                return;
            }

            int    idUsuario = Convert.ToInt32(dgvUsuarios.SelectedRows[0].Cells["ID"].Value);
            string username  = dgvUsuarios.SelectedRows[0].Cells["Username"].Value?.ToString() ?? "";

            var tD = Traductor.ObtenerTraducciones(_idioma);
            string T_d(string k, string fb) => tD.ContainsKey(k) ? tD[k].Texto : fb;

            var confirm = MessageBox.Show(
                string.Format(T_d("conf.desbloquear.body",  "¿Desbloquear la cuenta de '{0}'?"), username),
                T_d("conf.desbloquear.titulo", "Confirmar Desbloqueo"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (confirm != DialogResult.Yes) return;

            try
            {
                usuarioBLL.Desbloquear(this, idUsuario, username);
                CargarUsuarios();
                MostrarOk(string.Format(T_d("msg.usr.desbloqueada", "Cuenta '{0}' desbloqueada correctamente."), username));
            }
            catch (Exception ex)
            {
                MostrarError(ex.Message);
            }
        }

        // ── Recalcular integridad DV ──────────────────────────────────────────

        private void BtnRecalcularDV_Click(object sender, EventArgs e)
        {
            try
            {
                BLL.Configuracion.RecalcularIntegridadDV();
                MostrarOk("DVH y DVV recalculados correctamente para todos los usuarios.");
            }
            catch (Exception ex)
            {
                MostrarError($"Error al recalcular DV: {ex.Message}");
            }
        }

        // ── Reset masivo ──────────────────────────────────────────────────────

        private void BtnResetMasivo_Click(object sender, EventArgs e)
        {
            const string claveTemporal = "Wardrobe1!";

            var tM = Traductor.ObtenerTraducciones(_idioma);
            string T_m(string k, string fb) => tM.ContainsKey(k) ? tM[k].Texto : fb;

            var confirm = MessageBox.Show(
                string.Format(T_m("conf.resetmasivo.body", "Esto va a resetear la contraseña de TODOS los usuarios a:\n\n   {0}\n\nComunicate con cada empleado para que la cambien.\n\n¿Confirmar?"), claveTemporal),
                T_m("conf.resetmasivo.titulo", "Resetear todas las claves"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (confirm != DialogResult.Yes) return;

            try
            {
                usuarioBLL.ResetearTodasLasClaves(this, claveTemporal);
                MostrarOk(string.Format(T_m("msg.usr.resetmasivo", "Todas las claves fueron reseteadas a: {0}"), claveTemporal));
            }
            catch (Exception ex)
            {
                MostrarError(ex.Message);
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>
        /// Diálogo de entrada de texto simple — reemplaza Microsoft.VisualBasic.Interaction.InputBox
        /// para evitar dependencia externa en el proyecto GUI.
        /// </summary>
        private string PedirTexto(string mensaje, string titulo)
        {
            using (Form dlg = new Form())
            {
                dlg.Text            = titulo;
                dlg.ClientSize      = new System.Drawing.Size(360, 130);
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition  = FormStartPosition.CenterParent;
                dlg.MaximizeBox    = false;
                dlg.MinimizeBox    = false;

                var lbl = new Label
                {
                    Text     = mensaje,
                    Left     = 12, Top   = 12,
                    Width    = 336, Height = 32,
                    Font     = new Font("Segoe UI", 9f)
                };

                var txt = new TextBox
                {
                    Left         = 12,  Top   = 50,
                    Width        = 336, Height = 24,
                    PasswordChar = '●'
                };

                var tPT = Traductor.ObtenerTraducciones(_idioma);
                var btnOk = new Button
                {
                    Text         = tPT.ContainsKey("btn.aceptar")  ? tPT["btn.aceptar"].Texto  : "Aceptar",
                    Left         = 168, Top    = 88,
                    Width        = 80,  Height = 28,
                    DialogResult = DialogResult.OK
                };
                var btnCancelar = new Button
                {
                    Text         = tPT.ContainsKey("btn.cancelar") ? tPT["btn.cancelar"].Texto : "Cancelar",
                    Left         = 260, Top    = 88,
                    Width        = 88,  Height = 28,
                    DialogResult = DialogResult.Cancel
                };

                dlg.AcceptButton = btnOk;
                dlg.CancelButton = btnCancelar;
                dlg.Controls.AddRange(new Control[] { lbl, txt, btnOk, btnCancelar });

                return dlg.ShowDialog(this) == DialogResult.OK ? txt.Text : string.Empty;
            }
        }
    }
}
