using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Servicios.Multiidioma;

namespace GUI
{
    /// <summary>
    /// Administración de Usuarios (ABM de datos administrativos NO sensibles).
    ///
    /// Panel de EDICIÓN de datos del usuario, complementario al panel "Gestión de Usuarios"
    /// (que concentra las operaciones de cuenta: alta, reset de contraseña, desbloqueo,
    /// archivado y purga). Acá se:
    ///   • busca/filtra por nombre, apellido o email (datos NO sensibles),
    ///   • modifica nombre, apellido, nombre de usuario, fecha de nacimiento y email,
    ///   • cambia el rol del usuario (con validación de "último administrador"),
    ///   • consulta el Historial de Cambios (campo / valor anterior / valor nuevo / quién / cuándo).
    ///
    /// La contraseña NUNCA se edita ni se muestra desde acá. Todas las operaciones quedan
    /// registradas en bitácora y en el Historial de Cambios (Memento) vía la BLL.
    /// </summary>
    public class AdministracionUsuariosForm : FormBase, IIdiomaObserver
    {
        protected override Label MensajeLabel => _lblMensaje;

        private readonly BLL.Usuario _usuarioBLL = new BLL.Usuario();
        private readonly BLL.Familia _familiaBLL = new BLL.Familia();

        private DataGridView _dgv;
        private TextBox  _txtBuscar, _txtNombre, _txtApellido, _txtUsername, _txtEmail;
        private DateTimePicker _dtpNacimiento;
        private ComboBox _cmbRol;
        private Button   _btnBuscar, _btnNuevo, _btnGuardar, _btnCambiarRol, _btnHistorial, _btnRefrescar, _btnCerrar;
        private Label    _lblTitulo, _lblBuscar, _lblNombre, _lblApellido, _lblUsername, _lblEmail,
                         _lblNacimiento, _lblRol, _lblMensaje, _lblDatos;
        private Panel    _panelHeader, _panelEdicion;

        private List<BE.Usuario> _usuarios = new List<BE.Usuario>();
        private int  _idSeleccionado = 0;
        private bool _modoAlta = false;   // true mientras se cargan los datos de un usuario NUEVO

        private static readonly Color RosaPrimario = Color.FromArgb(210, 100, 135);
        private static readonly Color RosaOscuro   = Color.FromArgb(176, 62, 96);
        private static readonly Color PanelClaro   = Color.FromArgb(245, 245, 250);

        public AdministracionUsuariosForm() { ConstruirUI(); }

        // ── Ciclo de vida ────────────────────────────────────────────────────────
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GestorIdioma.SuscribirObservador(this);
            Traducir();
            CargarRoles();
            CargarUsuarios();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            GestorIdioma.DesuscribirObservador(this);
            base.OnFormClosing(e);
        }

        public void UpdateLanguage(Idioma idioma) { Traducir(); CargarUsuarios(); }

        private string T(string key, string fallback)
        {
            var t = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
            return t.ContainsKey(key) ? t[key].Texto : fallback;
        }

        private void Traducir()
        {
            this.Text            = T("frm.adminusuarios",      "Administración de Usuarios");
            _lblTitulo.Text      = T("lbl.adminusr.titulo",    "Administración de Usuarios");
            _lblBuscar.Text      = T("lbl.adminusr.buscar",    "Buscar (nombre, apellido o email):");
            _btnBuscar.Text      = T("btn.adminusr.buscar",    "🔍 Buscar");
            _btnRefrescar.Text   = T("btn.adminusr.refrescar", "↻ Ver todos");
            _btnNuevo.Text       = T("btn.adminusr.nuevo",     "➕ Nuevo usuario");
            _lblDatos.Text       = T("lbl.adminusr.datos",     "Datos del usuario seleccionado");
            _lblNombre.Text      = T("lbl.adminusr.nombre",    "Nombre:");
            _lblApellido.Text    = T("lbl.adminusr.apellido",  "Apellido:");
            _lblUsername.Text    = T("lbl.adminusr.username",  "Nombre de usuario:");
            _lblEmail.Text       = T("lbl.adminusr.email",     "Email:");
            _lblNacimiento.Text  = T("lbl.adminusr.nacimiento","Fecha de nacimiento:");
            _lblRol.Text         = T("lbl.adminusr.rol",       "Rol:");
            _btnGuardar.Text     = T("btn.adminusr.guardar",   "💾 Guardar cambios");
            _btnCambiarRol.Text  = T("btn.adminusr.cambiarrol","🔁 Cambiar rol");
            _btnHistorial.Text   = T("btn.adminusr.historial", "📜 Ver historial de cambios");
            _btnCerrar.Text      = T("btn.permisos.cerrar",    "Cerrar");
            TraducirHeaders();
        }

        private void TraducirHeaders()
        {
            if (_dgv.Columns.Count == 0) return;
            void RH(string col, string clave, string fb)
            {
                if (_dgv.Columns.Contains(col)) _dgv.Columns[col].HeaderText = T(clave, fb);
            }
            RH("Username", "col.usr.username", "Usuario");
            RH("Nombre",   "col.adminusr.nombre",   "Nombre");
            RH("Apellido", "col.adminusr.apellido", "Apellido");
            RH("Email",    "col.adminusr.email",    "Email");
            RH("Perfil",   "col.usr.perfil",        "Rol");
        }

        // ── Carga ─────────────────────────────────────────────────────────────────
        private void CargarRoles()
        {
            try
            {
                var roles = _familiaBLL.ObtenerRoles();
                var items = new List<RolItem>();
                foreach (var r in roles) items.Add(new RolItem(r, TraductorPerfil.Nombre(r)));
                _cmbRol.DataSource    = items;
                _cmbRol.DisplayMember = "Label";
                _cmbRol.ValueMember   = "Value";
                _cmbRol.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[AdministracionUsuariosForm.CargarRoles] " + ex.Message);
            }
        }

        private void CargarUsuarios(string filtro = null)
        {
            try
            {
                _usuarios = string.IsNullOrWhiteSpace(filtro)
                    ? _usuarioBLL.ObtenerTodos()
                    : _usuarioBLL.Buscar(filtro);

                var tabla = new DataTable();
                tabla.Columns.Add("ID",       typeof(int));
                tabla.Columns.Add("Username", typeof(string));
                tabla.Columns.Add("Nombre",   typeof(string));
                tabla.Columns.Add("Apellido", typeof(string));
                tabla.Columns.Add("Email",    typeof(string));
                tabla.Columns.Add("Perfil",   typeof(string));
                foreach (var u in _usuarios)
                    tabla.Rows.Add(u.Id, u.Username, u.Nombre, u.Apellido, u.Email,
                                   TraductorPerfil.Nombre(u.Perfil));

                _dgv.DataSource = tabla;
                if (_dgv.Columns.Contains("ID")) _dgv.Columns["ID"].Visible = false;
                // El email es más largo que el resto: darle más ancho relativo para que no se trunque.
                if (_dgv.Columns.Contains("Email")) _dgv.Columns["Email"].FillWeight = 170;
                TraducirHeaders();

                _lblMensaje.ForeColor = Color.DarkGreen;
                _lblMensaje.Text = string.Format(T("msg.adminusr.cargados", "{0} usuario(s)."), _usuarios.Count);
                LimpiarEdicion();
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void DgvSelectionChanged(object sender, EventArgs e)
        {
            if (_dgv.SelectedRows.Count == 0) { LimpiarEdicion(); return; }
            int id = Convert.ToInt32(_dgv.SelectedRows[0].Cells["ID"].Value);
            var u = _usuarios.Find(x => x.Id == id);
            if (u == null) { LimpiarEdicion(); return; }

            // Seleccionar un usuario existente sale del modo alta.
            _modoAlta = false;
            _btnGuardar.Text = T("btn.adminusr.guardar", "💾 Guardar cambios");
            _idSeleccionado     = u.Id;
            _txtNombre.Text     = u.Nombre   ?? "";
            _txtApellido.Text   = u.Apellido ?? "";
            _txtUsername.Text   = u.Username ?? "";
            _txtEmail.Text      = u.Email    ?? "";
            if (u.FechaNacimiento.HasValue)
            {
                _dtpNacimiento.Checked = true;
                _dtpNacimiento.Value   = u.FechaNacimiento.Value;
            }
            else _dtpNacimiento.Checked = false;

            // Preseleccionar el rol actual en el combo.
            for (int i = 0; i < _cmbRol.Items.Count; i++)
                if (_cmbRol.Items[i] is RolItem ri &&
                    string.Equals(ri.Value, u.Perfil, StringComparison.OrdinalIgnoreCase))
                { _cmbRol.SelectedIndex = i; break; }

            HabilitarEdicion(true);
        }

        private void LimpiarEdicion()
        {
            _idSeleccionado = 0;
            _modoAlta = false;
            _txtNombre.Text = _txtApellido.Text = _txtUsername.Text = _txtEmail.Text = "";
            _dtpNacimiento.Checked = false;
            _cmbRol.SelectedIndex = -1;
            HabilitarEdicion(false);
            if (_btnGuardar != null) _btnGuardar.Text = T("btn.adminusr.guardar", "💾 Guardar cambios");
        }

        private void HabilitarEdicion(bool on)
        {
            _txtNombre.Enabled = _txtApellido.Enabled = _txtUsername.Enabled = on;
            _txtEmail.Enabled  = _dtpNacimiento.Enabled = _cmbRol.Enabled = on;
            _btnGuardar.Enabled = _btnCambiarRol.Enabled = _btnHistorial.Enabled = on;
        }

        // ── Acciones ────────────────────────────────────────────────────────────────

        // Entra en modo "alta": habilita los campos vacíos para cargar un usuario NUEVO.
        // La contraseña la genera la BLL y se exporta a un archivo (no se ingresa acá).
        private void ModoAlta()
        {
            _dgv.ClearSelection();   // dispara LimpiarEdicion; luego activamos el modo alta
            _modoAlta = true;
            _idSeleccionado = 0;
            _txtNombre.Text = _txtApellido.Text = _txtUsername.Text = _txtEmail.Text = "";
            _dtpNacimiento.Checked = false;
            _cmbRol.SelectedIndex = _cmbRol.Items.Count > 0 ? 0 : -1;
            HabilitarEdicion(true);
            _btnCambiarRol.Enabled = false;   // el rol se elige en el alta; "cambiar rol" es para existentes
            _btnHistorial.Enabled  = false;
            _btnGuardar.Text = T("btn.adminusr.crear", "💾 Crear usuario");
            _lblMensaje.ForeColor = System.Drawing.Color.DimGray;
            _lblMensaje.Text = T("msg.adminusr.modoalta", "Cargá los datos del nuevo usuario y presioná Crear.");
            _txtNombre.Focus();
        }

        // Guarda: crea (modo alta) o actualiza (usuario seleccionado). En edición, "Guardar cambios"
        // persiste TANTO los datos administrativos COMO el rol si cambió (el botón "Cambiar rol" queda
        // como atajo). Así no hay confusión de "cambié el rol pero no se guardó".
        private void Guardar()
        {
            if (_modoAlta) { Alta(); return; }
            if (_idSeleccionado == 0) return;

            var actual = _usuarios.Find(x => x.Id == _idSeleccionado);
            string rolSel = (_cmbRol.SelectedItem as RolItem)?.Value;
            try
            {
                bool huboCambio = false;
                DateTime? fnac = _dtpNacimiento.Checked ? (DateTime?)_dtpNacimiento.Value.Date : null;

                // Datos administrativos (si no cambió nada, la BLL lanza 'sin_cambios' y lo ignoramos).
                try
                {
                    _usuarioBLL.Modificar(this.Text, _idSeleccionado,
                        _txtNombre.Text, _txtApellido.Text, _txtUsername.Text, fnac, _txtEmail.Text);
                    huboCambio = true;
                }
                catch (BE.AppException ex) when (ex.Clave == "err.bll.usuario.sin_cambios") { }

                // Rol (solo si el seleccionado difiere del actual).
                if (!string.IsNullOrWhiteSpace(rolSel) && actual != null
                    && !string.Equals(rolSel, actual.Perfil, StringComparison.OrdinalIgnoreCase))
                {
                    _usuarioBLL.CambiarRol(this.Text, _idSeleccionado, rolSel);
                    GUI.Menu.RefrescarSeguridadAbierta();
                    huboCambio = true;
                }

                if (!huboCambio)
                { MostrarError(T("err.bll.usuario.sin_cambios", "No hay cambios para guardar.")); return; }

                MostrarOk(T("msg.adminusr.guardado", "Datos del usuario actualizados correctamente."));
                CargarUsuarios(_txtBuscar.Text);
            }
            catch (Exception ex) { MostrarError(ex); }
        }

        // Alta de un usuario nuevo con sus datos administrativos. La BLL genera la contraseña,
        // la exporta a un .txt y registra en bitácora + versión base del historial.
        private void Alta()
        {
            string rol = (_cmbRol.SelectedItem as RolItem)?.Value;
            if (string.IsNullOrWhiteSpace(rol))
            { MostrarError(T("err.adminusr.rol_vacio", "Seleccioná un rol.")); return; }
            try
            {
                DateTime? fnac = _dtpNacimiento.Checked ? (DateTime?)_dtpNacimiento.Value.Date : null;
                string ruta = _usuarioBLL.Alta(this.Text, _txtUsername.Text, rol,
                    _txtNombre.Text, _txtApellido.Text, fnac, _txtEmail.Text);
                _modoAlta = false;
                _btnGuardar.Text = T("btn.adminusr.guardar", "💾 Guardar cambios");
                CargarUsuarios();
                MostrarOk(string.Format(
                    T("msg.adminusr.creado", "Usuario '{0}' creado. Credenciales en: {1}"),
                    _txtUsername.Text.Trim(), ruta));
            }
            catch (Exception ex) { MostrarError(ex); }
        }

        private void CambiarRol()
        {
            if (_idSeleccionado == 0) return;
            string rol = (_cmbRol.SelectedItem as RolItem)?.Value;
            if (string.IsNullOrWhiteSpace(rol))
            { MostrarError(T("err.adminusr.rol_vacio", "Seleccioná un rol.")); return; }
            try
            {
                _usuarioBLL.CambiarRol(this.Text, _idSeleccionado, rol);
                GUI.Menu.RefrescarSeguridadAbierta();   // re-aplica seguridad en vivo si tocó al usuario en sesión
                MostrarOk(T("msg.adminusr.rolcambiado", "Rol del usuario actualizado correctamente."));
                CargarUsuarios(_txtBuscar.Text);
            }
            catch (Exception ex) { MostrarError(ex); }
        }

        private void VerHistorial()
        {
            if (_idSeleccionado == 0) return;
            try { new VersionHistorialForm(_idSeleccionado).ShowDialog(this); }
            catch (Exception ex) { MostrarError(ex); }
        }

        // ── Combo de roles (valor técnico + etiqueta visible) ──────────────────────
        private class RolItem
        {
            public string Value { get; }
            public string Label { get; }
            public RolItem(string value, string label) { Value = value; Label = label; }
            public override string ToString() => Label;
        }

        // ── Construcción de UI ──────────────────────────────────────────────────────
        private void ConstruirUI()
        {
            this.Text          = "Administración de Usuarios";
            this.Size          = new Size(900, 600);
            this.MinimumSize   = new Size(820, 540);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor     = Color.White;
            this.Font          = new Font("Segoe UI", 9f);

            // Header
            _panelHeader = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = RosaOscuro };
            _lblTitulo = new Label { Text = "Administración de Usuarios", ForeColor = Color.White,
                Font = new Font("Segoe UI", 13f, FontStyle.Bold), AutoSize = true, Location = new Point(16, 10),
                BackColor = Color.Transparent };
            _panelHeader.Controls.Add(_lblTitulo);

            // Búsqueda
            _lblBuscar = new Label { Text = "Buscar (nombre, apellido o email):", Location = new Point(16, 60),
                AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = RosaOscuro };
            _txtBuscar = new TextBox { Location = new Point(16, 82), Size = new Size(260, 24) };
            _txtBuscar.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; CargarUsuarios(_txtBuscar.Text); } };
            _btnBuscar    = MakeBtn("🔍 Buscar",  new Point(284, 81), 90,  RosaPrimario, Color.White);
            _btnBuscar.Click += (s, e) => CargarUsuarios(_txtBuscar.Text);
            _btnRefrescar = MakeBtn("↻ Ver todos", new Point(380, 81), 96, Color.White, RosaOscuro, true);
            _btnRefrescar.Click += (s, e) => { _txtBuscar.Clear(); CargarUsuarios(); };
            _btnNuevo = MakeBtn("➕ Nuevo usuario", new Point(482, 81), 100, RosaPrimario, Color.White);
            _btnNuevo.Click += (s, e) => ModoAlta();

            // Grilla
            _dgv = new DataGridView
            {
                Location = new Point(16, 116), Size = new Size(560, 410),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AllowUserToAddRows = false, AllowUserToDeleteRows = false, ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false,
                RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White, BorderStyle = BorderStyle.FixedSingle
            };
            _dgv.SelectionChanged += DgvSelectionChanged;

            // Panel de edición (derecha)
            _panelEdicion = new Panel { Location = new Point(588, 116), Size = new Size(286, 410),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right, BackColor = PanelClaro,
                BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(10) };

            _lblDatos = new Label { Text = "Datos del usuario seleccionado", Location = new Point(10, 8),
                AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = RosaOscuro };

            int y = 38;
            _lblNombre   = EtiquetaEd("Nombre:", ref y);
            _txtNombre   = CampoEd(ref y);
            _lblApellido = EtiquetaEd("Apellido:", ref y);
            _txtApellido = CampoEd(ref y);
            _lblUsername = EtiquetaEd("Nombre de usuario:", ref y);
            _txtUsername = CampoEd(ref y);
            _lblEmail    = EtiquetaEd("Email:", ref y);
            _txtEmail    = CampoEd(ref y);
            _lblNacimiento = EtiquetaEd("Fecha de nacimiento:", ref y);
            _dtpNacimiento = new DateTimePicker { Location = new Point(10, y), Size = new Size(260, 24),
                Format = DateTimePickerFormat.Short, ShowCheckBox = true, Checked = false };
            y += 32;
            _lblRol = EtiquetaEd("Rol:", ref y);
            _cmbRol = new ComboBox { Location = new Point(10, y), Size = new Size(260, 24),
                DropDownStyle = ComboBoxStyle.DropDownList };
            y += 38;

            _btnGuardar    = MakeBtn("💾 Guardar cambios", new Point(10, y), 260, RosaPrimario, Color.White); y += 38;
            _btnCambiarRol = MakeBtn("🔁 Cambiar rol",      new Point(10, y), 260, Color.White, RosaOscuro, true); y += 38;
            _btnHistorial  = MakeBtn("📜 Ver historial de cambios", new Point(10, y), 260, Color.White, RosaOscuro, true); y += 40;
            _btnGuardar.Click    += (s, e) => Guardar();
            _btnCambiarRol.Click += (s, e) => CambiarRol();
            _btnHistorial.Click  += (s, e) => VerHistorial();

            _lblMensaje = new Label { Location = new Point(10, y), Size = new Size(262, 60),
                Font = new Font("Segoe UI", 8.5f), ForeColor = Color.DimGray };

            _panelEdicion.Controls.AddRange(new Control[]
            {
                _lblDatos, _lblNombre, _txtNombre, _lblApellido, _txtApellido,
                _lblUsername, _txtUsername, _lblEmail, _txtEmail,
                _lblNacimiento, _dtpNacimiento, _lblRol, _cmbRol,
                _btnGuardar, _btnCambiarRol, _btnHistorial, _lblMensaje
            });

            // Footer
            _btnCerrar = MakeBtn("Cerrar", new Point(786, 532), 90, Color.FromArgb(236, 236, 242), Color.FromArgb(70, 70, 80));
            _btnCerrar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _btnCerrar.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[]
            {
                _lblBuscar, _txtBuscar, _btnBuscar, _btnRefrescar, _btnNuevo, _dgv, _panelEdicion, _btnCerrar
            });
            this.Controls.Add(_panelHeader);
            _panelHeader.BringToFront();

            // Nombres para UI Automation / accesibilidad (permiten localizar los controles por AutomationId).
            _dgv.Name="dgvUsuarios"; _txtBuscar.Name="txtBuscar"; _txtNombre.Name="txtNombre";
            _txtApellido.Name="txtApellido"; _txtUsername.Name="txtUsername"; _txtEmail.Name="txtEmail";
            _cmbRol.Name="cmbRol"; _btnGuardar.Name="btnGuardar"; _btnNuevo.Name="btnNuevo";
            _btnCambiarRol.Name="btnCambiarRol"; _btnHistorial.Name="btnHistorial"; _btnBuscar.Name="btnBuscar";

            HabilitarEdicion(false);
        }

        private Label EtiquetaEd(string text, ref int y)
        {
            var l = new Label { Text = text, Location = new Point(10, y), AutoSize = true,
                Font = new Font("Segoe UI", 8.5f), ForeColor = Color.DimGray };
            y += 18;
            return l;
        }

        private TextBox CampoEd(ref int y)
        {
            var t = new TextBox { Location = new Point(10, y), Size = new Size(260, 24), MaxLength = 200 };
            y += 30;
            return t;
        }

        private static Button MakeBtn(string text, Point loc, int width, Color back, Color fore, bool light = false)
        {
            var b = new Button { Text = text, Location = loc, Size = new Size(width, 30), BackColor = back,
                ForeColor = fore, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8.5f), Cursor = Cursors.Hand };
            b.FlatAppearance.BorderSize = light ? 1 : 0;
            if (light) b.FlatAppearance.BorderColor = RosaOscuro;
            return b;
        }
    }
}
