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

        // Referencias a botones dinámicos para poder traducirlos en UpdateLanguage
        // RF-10 — baja lógica / archivado / purga
        private Button _btnEliminar;
        private Button _btnVerArchivados;
        private Button _btnPurgar;
        private bool   _viendoArchivados = false;

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
            Aplicar(_btnEliminar,         t);
            Aplicar(_btnVerArchivados,    t);
            Aplicar(_btnPurgar,           t);
            RellenarComboPerfil(t);
            TraducirHeadersGrilla();
        }

        // Recarga cmbPerfil con etiquetas traducidas manteniendo los valores internos (DB keys).
        private void RellenarComboPerfil(IDictionary<string, Servicios.Multiidioma.Traduccion> t)
        {
            string TT(string k, string fb) => t.ContainsKey(k) ? t[k].Texto : fb;

            var items = new[]
            {
                // Jerarquía consolidada (2da entrega):
                //   Comercial:   GerenteComercial ⊃ Vendedor
                //   Inventario:  GerenteInventario ⊃ OperadorLogistico + OperadorDeInventario
                //   Transversal: Auditor (solo lectura) · Administrador (todo)
                new PerfilItem("Administrador",       TT("perfil.administrador",       "Administrador")),
                new PerfilItem("Auditor",             TT("perfil.auditor",             "Auditor")),
                new PerfilItem("GerenteComercial",    TT("perfil.gerentecomercial",    "Gerente Comercial")),
                new PerfilItem("Vendedor",            TT("perfil.vendedor",            "Vendedor")),
                new PerfilItem("GerenteInventario",   TT("perfil.gerenteinventario",   "Gerente de Inventario")),
                new PerfilItem("OperadorDeInventario",TT("perfil.operador",            "Operador de Inventario")),
                new PerfilItem("OperadorLogistico",   TT("perfil.operadorlogistico",   "Operador Logístico")),
            };

            int prevIdx = cmbPerfil.SelectedIndex < 0 ? 2 : cmbPerfil.SelectedIndex;
            cmbPerfil.DataSource    = null;
            cmbPerfil.DisplayMember = "Label";
            cmbPerfil.ValueMember   = "Value";
            cmbPerfil.DataSource    = items;
            cmbPerfil.SelectedIndex = prevIdx < items.Length ? prevIdx : 2;
        }

        private class PerfilItem
        {
            public string Value { get; }
            public string Label { get; }
            public PerfilItem(string value, string label) { Value = value; Label = label; }
            public override string ToString() => Label;
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

            // El panel lateral muestra varias acciones apiladas; con scroll se garantiza que
            // todas queden accesibles aunque la resolución/DPI reduzca el alto disponible.
            panelAlta.AutoScroll = true;

            // Revisión: el ALTA de usuario ("Nuevo usuario") se movió a "Administración de Usuarios".
            // Esta pantalla queda como gestión de CUENTA: resetear contraseña, desbloquear y archivar/purgar.
            // Se ocultan los controles de alta y se suben las secciones de reset/desbloqueo para no dejar hueco.
            foreach (var c in new Control[] { lblTitulo, lblUser, txtUsername, lblPerfil, cmbPerfil, btnAgregar })
                if (c != null) c.Visible = false;
            btnRefrescar.Location = new Point(12, 12);
            const int subir = 230;
            foreach (var c in new Control[] { separador1, lblResetTitulo, lblResetInfo, btnResetearClave,
                                              separador2, lblDesbloquearTitulo, lblDesbloquearInfo, btnDesbloquear })
                if (c != null) c.Location = new Point(c.Location.X, c.Location.Y - subir);

            // ── RF-10 — Archivar / Ver archivados / Purgar ────────────────────────
            // (El reseteo masivo de claves y el recálculo de DV se quitaron de acá: el primero es
            //  una operación peligrosa innecesaria; el recálculo de DV vive en Diagnóstico de Integridad.)
            _btnEliminar = new Button
            {
                Tag       = "btn.usr.eliminar",
                Text      = "🗑 Archivar usuario",
                Size      = new Size(210, 30),
                Location  = new Point(12, btnDesbloquear.Bottom + 20),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(170, 50, 50),
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor    = Cursors.Hand,
                Enabled   = false
            };
            _btnEliminar.FlatAppearance.BorderSize = 0;
            _btnEliminar.Click += BtnEliminar_Click;
            panelAlta.Controls.Add(_btnEliminar);

            _btnVerArchivados = new Button
            {
                Tag       = "btn.usr.verarchivados",
                Text      = "Ver archivados",
                Size      = new Size(102, 28),
                Location  = new Point(12, _btnEliminar.Bottom + 8),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(110, 110, 120),
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 8.5f),
                Cursor    = Cursors.Hand
            };
            _btnVerArchivados.FlatAppearance.BorderSize = 0;
            _btnVerArchivados.Click += BtnVerArchivados_Click;
            panelAlta.Controls.Add(_btnVerArchivados);

            _btnPurgar = new Button
            {
                Tag       = "btn.usr.purgar",
                Text      = "Purgar (>1 año)",
                Size      = new Size(102, 28),
                Location  = new Point(120, _btnEliminar.Bottom + 8),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(60, 60, 70),
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 8.5f),
                Cursor    = Cursors.Hand
            };
            _btnPurgar.FlatAppearance.BorderSize = 0;
            _btnPurgar.Click += BtnPurgar_Click;
            panelAlta.Controls.Add(_btnPurgar);

            // El cartel de estado va al FINAL del panel, debajo de TODOS los botones. Antes estaba
            // fijo (Designer) en una Y que se superponía con estos botones agregados por código.
            lblMensaje.Location = new Point(12, _btnPurgar.Bottom + 14);
            lblMensaje.Size     = new Size(210, 48);

            // Campo de contraseña oculto: la contraseña se genera automáticamente en la BLL.
            lblPass.Visible      = false;
            txtContraseña.Visible = false;

            CargarUsuarios();
        }

        private void BtnRefrescar_Click(object sender, EventArgs e)
        {
            CargarUsuarios();
        }

        private void DgvUsuarios_SelectionChanged(object sender, EventArgs e)
        {
            bool haySeleccion = dgvUsuarios.SelectedRows.Count > 0;

            // En la vista de archivados no hay acciones individuales (solo purga masiva).
            if (_viendoArchivados)
            {
                btnResetearClave.Enabled = false;
                btnDesbloquear.Enabled   = false;
                if (_btnEliminar != null) _btnEliminar.Enabled = false;
                return;
            }

            btnResetearClave.Enabled = haySeleccion;
            if (_btnEliminar != null) _btnEliminar.Enabled = haySeleccion;

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
                List<BE.Usuario> usuarios = _viendoArchivados
                    ? usuarioBLL.ObtenerArchivados()
                    : usuarioBLL.ObtenerTodos();

                var t = Traductor.ObtenerTraducciones(_idioma);
                string lblActivo    = t.ContainsKey("usr.activo")   ? t["usr.activo"].Texto   : "Activo";
                string lblBloqueada = t.ContainsKey("usr.bloqueada") ? t["usr.bloqueada"].Texto : "Bloqueada";
                string lblArchivado = t.ContainsKey("usr.archivado") ? t["usr.archivado"].Texto : "Archivado";

                var tabla = new DataTable();
                tabla.Columns.Add("ID",           typeof(int));
                tabla.Columns.Add("Username",     typeof(string));
                tabla.Columns.Add("Perfil",       typeof(string));
                tabla.Columns.Add("Estado",       typeof(string));
                // Columna interna: 1 = bloqueado/archivado, 0 = activo — independiente del idioma
                tabla.Columns.Add("_BloqueadoKey", typeof(int));

                foreach (var u in usuarios)
                {
                    string estado = _viendoArchivados
                        ? (u.FechaBaja.HasValue ? $"{lblArchivado} ({u.FechaBaja.Value:dd/MM/yyyy})" : lblArchivado)
                        : (u.Bloqueado ? lblBloqueada : lblActivo);
                    tabla.Rows.Add(
                        u.Id,
                        u.Username,
                        TraductorPerfil.Nombre(u.Perfil),
                        estado,
                        (_viendoArchivados || u.Bloqueado) ? 1 : 0);
                }

                dgvUsuarios.DataSource = tabla;
                TraducirHeadersGrilla();

                // Colorear filas bloqueadas/archivadas usando la columna interna (independiente del idioma)
                foreach (DataGridViewRow fila in dgvUsuarios.Rows)
                {
                    if (fila.Cells["_BloqueadoKey"].Value?.ToString() == "1")
                    {
                        fila.DefaultCellStyle.BackColor = _viendoArchivados ? Color.FromArgb(235, 235, 235) : Color.FromArgb(255, 220, 220);
                        fila.DefaultCellStyle.ForeColor = _viendoArchivados ? Color.DimGray : Color.DarkRed;
                    }
                }

                string fmt = _viendoArchivados
                    ? (t.ContainsKey("msg.usr.archivados") ? t["msg.usr.archivados"].Texto : "{0} usuario(s) archivado(s).")
                    : (t.ContainsKey("msg.usr.cargados")   ? t["msg.usr.cargados"].Texto   : "{0} usuario(s) registrado(s).");
                lblMensaje.ForeColor = Color.DarkGreen;
                lblMensaje.Text      = string.Format(fmt, usuarios.Count);

                // En vista de archivados, las acciones sobre activos no aplican.
                btnAgregar.Enabled       = !_viendoArchivados;
                btnResetearClave.Enabled = false;
                btnDesbloquear.Enabled   = false;
                _btnEliminar.Enabled     = false;
            }
            catch (Exception ex)
            {
                var te = Traductor.ObtenerTraducciones(_idioma);
                MostrarError(string.Format(te.ContainsKey("err.generico.cargar") ? te["err.generico.cargar"].Texto : "Error al cargar: {0}", ex.Message));
            }
        }

        // ── Eventos de botones ────────────────────────────────────────────────

        /// <summary>Crea un nuevo usuario. La contraseña se genera automáticamente en la BLL.</summary>
        private void BtnAgregar_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string perfil   = (cmbPerfil.SelectedItem as PerfilItem)?.Value ?? cmbPerfil.SelectedItem?.ToString() ?? "";

            try
            {
                string rutaArchivo = usuarioBLL.Alta(this.Text, username, perfil);

                txtUsername.Clear();
                cmbPerfil.SelectedIndex = 2;

                CargarUsuarios();
                var tCr = Traductor.ObtenerTraducciones(_idioma);
                string fmt = tCr.ContainsKey("msg.usr.creado.exportado")
                    ? tCr["msg.usr.creado.exportado"].Texto
                    : "Usuario '{0}' [{1}] creado.\nCredenciales en: {2}";
                MostrarOk(string.Format(fmt, username, perfil, rutaArchivo));
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        /// <summary>
        /// Resetea la contraseña del usuario seleccionado generándola automáticamente.
        /// No solicita contraseña al administrador — la BLL la genera y exporta a archivo.
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

            try
            {
                string rutaArchivo = usuarioBLL.ResetearClave(this.Text, idUsuario, username);
                var tR = Traductor.ObtenerTraducciones(_idioma);
                string fmt = tR.ContainsKey("msg.usr.clave.exportada")
                    ? tR["msg.usr.clave.exportada"].Texto
                    : "Contraseña de '{0}' regenerada.\nCredenciales en: {1}";
                MostrarOk(string.Format(fmt, username, rutaArchivo));
            }
            catch (Exception ex)
            {
                MostrarError(ex);
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
                usuarioBLL.Desbloquear(this.Text, idUsuario, username);
                CargarUsuarios();
                MostrarOk(string.Format(T_d("msg.usr.desbloqueada", "Cuenta '{0}' desbloqueada correctamente."), username));
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        // ── RF-10 — Archivar / Ver archivados / Purgar ────────────────────────

        // Archiva (baja lógica) el usuario seleccionado. La BLL protege al último admin y al self.
        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (_viendoArchivados || dgvUsuarios.SelectedRows.Count == 0) return;

            var t = Traductor.ObtenerTraducciones(_idioma);
            string T_e(string k, string fb) => t.ContainsKey(k) ? t[k].Texto : fb;

            int    idUsuario = Convert.ToInt32(dgvUsuarios.SelectedRows[0].Cells["ID"].Value);
            string username  = dgvUsuarios.SelectedRows[0].Cells["Username"].Value?.ToString() ?? "";

            var confirm = MessageBox.Show(
                string.Format(T_e("conf.usr.archivar.body",
                    "¿Archivar al usuario '{0}'?\n\nNo podrá iniciar sesión y saldrá de la lista, pero se conserva su historial.\nPodrá eliminarse definitivamente tras 1 año."), username),
                T_e("conf.usr.archivar.titulo", "Confirmar Archivado"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

            if (confirm != DialogResult.Yes) return;

            try
            {
                usuarioBLL.Eliminar(this.Text, idUsuario, username);
                CargarUsuarios();
                MostrarOk(string.Format(T_e("msg.usr.archivado", "Usuario '{0}' archivado correctamente."), username));
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        // Alterna entre la lista de usuarios activos y la de archivados.
        private void BtnVerArchivados_Click(object sender, EventArgs e)
        {
            _viendoArchivados = !_viendoArchivados;
            var t = Traductor.ObtenerTraducciones(_idioma);
            _btnVerArchivados.Text = _viendoArchivados
                ? (t.ContainsKey("btn.usr.veractivos")   ? t["btn.usr.veractivos"].Texto   : "Ver activos")
                : (t.ContainsKey("btn.usr.verarchivados") ? t["btn.usr.verarchivados"].Texto : "Ver archivados");
            CargarUsuarios();
        }

        // Purga (eliminación física) de todos los usuarios archivados hace más de 1 año.
        private void BtnPurgar_Click(object sender, EventArgs e)
        {
            var t = Traductor.ObtenerTraducciones(_idioma);
            string T_p(string k, string fb) => t.ContainsKey(k) ? t[k].Texto : fb;

            int elegibles;
            try { elegibles = usuarioBLL.ObtenerArchivadosParaPurga().Count; }
            catch (Exception ex) { MostrarError(ex); return; }

            if (elegibles == 0)
            {
                MostrarOk(T_p("msg.usr.purga.ninguno", "No hay usuarios archivados con más de 1 año para purgar."));
                return;
            }

            var confirm = MessageBox.Show(
                string.Format(T_p("conf.usr.purgar.body",
                    "Se eliminarán DEFINITIVAMENTE {0} usuario(s) archivado(s) hace más de 1 año.\nEsta acción no se puede deshacer.\n\n¿Continuar?"), elegibles),
                T_p("conf.usr.purgar.titulo", "Confirmar Purga"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

            if (confirm != DialogResult.Yes) return;

            try
            {
                int eliminados = usuarioBLL.PurgarArchivados(this.Text);
                CargarUsuarios();
                MostrarOk(string.Format(T_p("msg.usr.purga.ok", "{0} usuario(s) eliminado(s) definitivamente."), eliminados));
            }
            catch (Exception ex)
            {
                MostrarError(ex);
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
