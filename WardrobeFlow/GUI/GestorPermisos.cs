using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Servicios.Multiidioma;

namespace GUI
{
    /// <summary>
    /// T04 — Gestión de Perfiles de Usuario (Patrón Composite) — UX "árbol + panel de acciones
    /// con modo Crear/Editar" (misma experiencia que la pantalla de Gestión de Roles de referencia):
    ///
    ///   • Izquierda: TreeView con TODA la estructura (Roles 👥 → sub-roles → Patentes 🔑).
    ///     Se selecciona el nodo sobre el que se quiere operar.
    ///   • Centro: panel de acciones con un TOGGLE de modo:
    ///       – Crear:  nombre + "Crear rol raíz" / "Crear sub-rol" (bajo el rol seleccionado).
    ///       – Editar: renombrar / eliminar el rol, asignar un permiso o rol por ComboBox,
    ///                 y quitar el ítem seleccionado de su rol padre.
    ///   • Derecha: árbol de PERMISOS EFECTIVOS (recursivo) del nodo seleccionado — lo que ese
    ///     rol concede tras resolver toda la jerarquía (visualización del Composite).
    ///
    /// La composición se persiste en [PermisoRelacion] de forma inmediata (cada acción guarda y
    /// re-aplica la seguridad en vivo). La validación anti-ciclos vive en la BLL.
    /// </summary>
    public class GestorPermisos : FormBase, IIdiomaObserver
    {
        protected override Label MensajeLabel => _lblMensaje;

        private readonly BLL.Familia _familiaBLL = new BLL.Familia();

        // ── Controles ──────────────────────────────────────────────────────────
        private Panel    _panelHeader, _panelFooter;
        private ToolTip  _tip;
        private Label    _lblTitulo, _lblSubtitulo, _lblMensaje, _lblEstructura,
                         _lblDetalle, _lblModo, _lblNombreRol, _lblAsignar;
        private TreeView _tvEstructura;
        private RadioButton _rbCrear, _rbEditar;
        private TextBox  _txtNombreRol;
        private GroupBox _grpCrear, _grpEditar, _grpAsignar;
        private ComboBox _cmbAsignables;
        private Button   _btnCrearRaiz, _btnCrearSub, _btnEditarNombre, _btnEliminarRol,
                         _btnAsignar, _btnQuitar, _btnActualizar, _btnExplorador, _btnCerrar;

        // ── Estado ───────────────────────────────────────────────────────────────
        private List<BE.Componente> _raices = new List<BE.Componente>();
        private readonly Dictionary<int, BE.Componente> _todos = new Dictionary<int, BE.Componente>();
        private BE.Componente _seleccionado;

        // Envoltura para mostrar un componente en el ComboBox con su ícono.
        private class Item
        {
            public BE.Componente Comp;
            public override string ToString() => Etiqueta(Comp);
        }

        public GestorPermisos() { ConstruirUI(); }

        // ── Ciclo de vida ────────────────────────────────────────────────────────
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GestorIdioma.SuscribirObservador(this);
            Traducir();
            CargarArbol();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            GestorIdioma.DesuscribirObservador(this);
            base.OnFormClosing(e);
        }

        public void UpdateLanguage(Idioma idioma) { Traducir(); }

        private string T(string key, string fallback)
        {
            var t = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
            return t.ContainsKey(key) ? t[key].Texto : fallback;
        }

        private void Traducir()
        {
            this.Text            = T("frm.gestorpermisos",      "Gestor de Perfiles — Roles y Permisos (Composite)");
            _lblTitulo.Text      = T("lbl.permisos.titulo",     "Perfiles y Permisos");
            _lblSubtitulo.Text   = T("lbl.permisos.subtitulo",  "Gestión de roles (los permisos son un catálogo fijo)");
            _lblEstructura.Text  = T("lbl.permisos.estructura", "Estructura del sistema (roles y permisos)");

            _lblModo.Text        = T("lbl.permisos.modo",       "Modo:");
            _rbCrear.Text        = T("rb.permisos.crear",       "Crear");
            _rbEditar.Text       = T("rb.permisos.editar",      "Editar / Eliminar");
            _lblNombreRol.Text   = T("lbl.permisos.nombrerol",  "Nombre del rol:");
            _grpCrear.Text       = T("grp.permisos.crear",      "Crear rol");
            _grpEditar.Text      = T("grp.permisos.editar",     "Editar rol");
            _grpAsignar.Text     = T("grp.permisos.asignar",    "Asignar permiso o rol");

            _btnCrearRaiz.Text   = T("btn.permisos.crearraiz",  "➕ Crear rol raíz");
            _btnCrearSub.Text    = T("btn.permisos.crearsub",   "➕ Crear sub-rol");
            _btnEditarNombre.Text= T("btn.permisos.editarnom",  "✏ Renombrar rol");
            _btnEliminarRol.Text = T("btn.permisos.eliminar",   "🗑 Eliminar rol");
            _btnAsignar.Text     = T("btn.permisos.asignar",    "Asignar ↓");
            _btnQuitar.Text      = T("btn.permisos.quitar",     "Quitar ítem seleccionado");
            _btnActualizar.Text  = T("btn.permisos.actualizar", "↻ Actualizar");
            _btnExplorador.Text  = T("btn.explorador",          "🌳 Ver vista completa del sistema");
            _btnCerrar.Text      = T("btn.permisos.cerrar",     "Cerrar");

            if (_tip != null)
                _tip.SetToolTip(_rbCrear, T("help.permisos.rol",
                    "Rol = perfil que se asigna a un usuario. Puede contener permisos (patentes) y otros roles (rol-en-rol)."));

            ActualizarControles();
        }

        // ── Carga / refresco del árbol ─────────────────────────────────────────────
        private void CargarArbol()
        {
            int idPrev = _seleccionado?.Id ?? 0;
            try
            {
                _raices = _familiaBLL.ObtenerArbol();

                _todos.Clear();
                foreach (var r in _raices) Aplanar(r, new HashSet<int>());

                _tvEstructura.BeginUpdate();
                _tvEstructura.Nodes.Clear();
                foreach (var raiz in _raices)
                    _tvEstructura.Nodes.Add(CrearNodo(raiz, new HashSet<int>()));
                _tvEstructura.ExpandAll();
                _tvEstructura.EndUpdate();

                // Reseleccionar el nodo previo (si sigue existiendo) o limpiar.
                _seleccionado = idPrev != 0 && _todos.ContainsKey(idPrev) ? _todos[idPrev] : null;
                if (_seleccionado != null) SeleccionarNodoPorId(_tvEstructura.Nodes, idPrev);
                ActualizarSeleccion();
            }
            catch (Exception ex)
            {
                MostrarError(string.Format(T("err.generico.cargar", "Error al cargar: {0}"), ex.Message));
            }
        }

        // Aplana el árbol a un diccionario id→componente (deduplicado) para la lista de asignables.
        private void Aplanar(BE.Componente nodo, HashSet<int> vis)
        {
            if (nodo.Id != 0 && !vis.Add(nodo.Id)) return;
            if (!_todos.ContainsKey(nodo.Id)) _todos[nodo.Id] = nodo;
            foreach (var h in nodo.Hijos) Aplanar(h, vis);
        }

        private TreeNode CrearNodo(BE.Componente comp, HashSet<int> vis)
        {
            var nodo = new TreeNode(Etiqueta(comp)) { Tag = comp };
            if (comp is BE.Rol)
            {
                nodo.NodeFont  = new Font("Segoe UI", 9f, FontStyle.Bold);
                nodo.ForeColor = Color.FromArgb(176, 62, 96);
            }
            if (comp.Id != 0 && vis.Add(comp.Id))
                foreach (var h in comp.Hijos)
                    nodo.Nodes.Add(CrearNodo(h, vis));
            return nodo;
        }

        // 👥 Rol · 📁 Familia · 🔑 Patente
        private static string Etiqueta(BE.Componente c)
            => (c is BE.Rol ? "👥 " : c is BE.Familia ? "📁 " : "🔑 ") + (c?.Nombre ?? "");

        private void Tv_AfterSelect(object sender, TreeViewEventArgs e)
        {
            _seleccionado = e.Node?.Tag as BE.Componente;
            ActualizarSeleccion();
        }

        private static bool EsRol(BE.Componente c) => c is BE.Rol;

        // ── Refresco central: efectivos + combo + estado de controles ──────────────
        private void ActualizarSeleccion()
        {
            _cmbAsignables.DataSource = null;

            // ComboBox de asignables (solo aplica a un Rol): patentes + roles del sistema,
            // excluyendo el propio nodo, su subárbol (evita ciclos obvios) y sus hijos directos.
            if (_seleccionado != null && EsRol(_seleccionado))
                _cmbAsignables.DataSource = CalcularAsignables((BE.Rol)_seleccionado);

            ActualizarControles();
        }

        // Patentes + roles asignables a 'rol' (excluye sí mismo, su subárbol y sus hijos directos).
        private List<Item> CalcularAsignables(BE.Rol rol)
        {
            var idsHijos = new HashSet<int>();
            foreach (var h in rol.Hijos) idsHijos.Add(h.Id);

            var subarbol = new HashSet<int>();
            RecolectarSubarbol(rol, subarbol, new HashSet<int>());

            var asignables = new List<Item>();
            foreach (var kv in _todos)
            {
                var c = kv.Value;
                if (c.Id == rol.Id)          continue;
                if (idsHijos.Contains(c.Id)) continue;
                if (subarbol.Contains(c.Id)) continue;
                asignables.Add(new Item { Comp = c });
            }
            asignables.Sort((a, b) => string.Compare(a.Comp.Nombre, b.Comp.Nombre, StringComparison.OrdinalIgnoreCase));
            return asignables;
        }

        // Habilita/oculta los controles según el MODO (Crear/Editar) y la selección. (Estilo referencia.)
        private void ActualizarControles()
        {
            bool crear = _rbCrear.Checked;
            _grpCrear.Visible  = crear;
            _grpEditar.Visible = !crear;

            bool esRol = EsRol(_seleccionado);

            // Detalle de la selección.
            _lblDetalle.Text = _seleccionado == null
                ? T("lbl.permisos.detalle.vacio", "Detalle: (sin selección)")
                : T("lbl.permisos.detalle", "Detalle: ") + Etiqueta(_seleccionado);

            if (crear)
            {
                _txtNombreRol.Enabled = true;
                _btnCrearRaiz.Enabled = true;
                // Sub-rol solo si hay un Rol seleccionado como padre.
                _btnCrearSub.Enabled  = esRol;
            }
            else
            {
                _btnEditarNombre.Enabled = esRol;
                _btnEliminarRol.Enabled  = esRol;
                _grpAsignar.Enabled      = esRol;
                _txtNombreRol.Enabled    = esRol;
                if (esRol) _txtNombreRol.Text = _seleccionado.Nombre;

                // Quitar: habilitado si el nodo seleccionado cuelga de un padre en el árbol.
                _btnQuitar.Enabled = _tvEstructura.SelectedNode?.Parent != null;
            }
        }

        private void RecolectarSubarbol(BE.Componente nodo, HashSet<int> acc, HashSet<int> vis)
        {
            if (nodo.Id != 0 && !vis.Add(nodo.Id)) return;
            foreach (var h in nodo.Hijos) { acc.Add(h.Id); RecolectarSubarbol(h, acc, vis); }
        }

        // ── Acciones: modo CREAR ───────────────────────────────────────────────────
        private void CrearRolRaiz()
        {
            string nombre = _txtNombreRol.Text.Trim();
            if (string.IsNullOrWhiteSpace(nombre))
            { MostrarError(T("perm.msg.nombrevacio", "Escribí un nombre para el rol.")); return; }
            try
            {
                _familiaBLL.CrearRol(nombre);
                GUI.Menu.RefrescarSeguridadAbierta();
                MostrarOk(string.Format(T("perm.ok.rolcreado", "Rol raíz '{0}' creado."), nombre));
                _txtNombreRol.Text = "";
                CargarArbol();
            }
            catch (Exception ex) { MostrarError(ex); }
        }

        private void CrearSubRol()
        {
            if (!(_seleccionado is BE.Rol padre))
            { MostrarError(T("perm.msg.selpadrerol", "Seleccioná un Rol del árbol para crearle un sub-rol.")); return; }

            string nombre = _txtNombreRol.Text.Trim();
            if (string.IsNullOrWhiteSpace(nombre))
            { MostrarError(T("perm.msg.nombrevacio", "Escribí un nombre para el sub-rol.")); return; }
            try
            {
                int nuevoId = _familiaBLL.CrearRol(nombre);
                _familiaBLL.AgregarComponente(padre.Id, nuevoId);   // valida ciclos en la BLL
                GUI.Menu.RefrescarSeguridadAbierta();
                MostrarOk(string.Format(T("perm.ok.subcreado", "Sub-rol '{0}' creado dentro de '{1}'."), nombre, padre.Nombre));
                _txtNombreRol.Text = "";
                CargarArbol();
            }
            catch (Exception ex) { MostrarError(ex); }
        }

        // ── Acciones: modo EDITAR ──────────────────────────────────────────────────
        private void EditarNombre()
        {
            if (!(_seleccionado is BE.Rol rol))
            { MostrarError(T("perm.msg.selmodrol", "Seleccioná un ROL del árbol para renombrar (los permisos no se editan).")); return; }

            string nombre = _txtNombreRol.Text.Trim();
            if (string.IsNullOrWhiteSpace(nombre))
            { MostrarError(T("perm.msg.nombrevacio", "El nombre del rol no puede estar vacío.")); return; }
            try
            {
                _familiaBLL.RenombrarComponente(rol.Id, nombre, nombre);
                GUI.Menu.RefrescarSeguridadAbierta();
                MostrarOk(string.Format(T("perm.ok.modificado", "Rol actualizado a '{0}'."), nombre));
                CargarArbol();
            }
            catch (Exception ex) { MostrarError(ex); }
        }

        private void EliminarRolSel()
        {
            if (!(_seleccionado is BE.Rol rol))
            { MostrarError(T("perm.msg.selelirol", "Seleccioná un ROL del árbol para eliminar (los permisos no se eliminan).")); return; }

            if (MessageBox.Show(
                    string.Format(T("perm.conf.elirol", "¿Eliminar el rol '{0}'? No se permite si tiene usuarios asignados."), rol.Nombre),
                    T("perm.conf.titulo", "Confirmar"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            try
            {
                string nombre = rol.Nombre;
                _familiaBLL.EliminarRol(nombre);   // valida que no tenga usuarios asignados
                _seleccionado = null;
                GUI.Menu.RefrescarSeguridadAbierta();
                MostrarOk(string.Format(T("perm.ok.eliminado", "Rol '{0}' eliminado."), nombre));
                CargarArbol();
            }
            catch (Exception ex) { MostrarError(ex); }
        }

        private void Asignar()
        {
            if (!(_seleccionado is BE.Rol rol))
            { MostrarError(T("perm.msg.selpadre", "Seleccioná un Rol para asignarle un permiso o rol.")); return; }
            if (!(_cmbAsignables.SelectedItem is Item it))
            { MostrarError(T("perm.msg.selasignable", "Elegí un permiso o rol de la lista para asignar.")); return; }
            try
            {
                _familiaBLL.AgregarComponente(rol.Id, it.Comp.Id);
                GUI.Menu.RefrescarSeguridadAbierta();
                MostrarOk(string.Format(T("perm.ok.agregado", "'{0}' asignado a '{1}'."), it.Comp.Nombre, rol.Nombre));
                CargarArbol();
            }
            catch (Exception ex) { MostrarError(ex); }
        }

        private void QuitarItem()
        {
            var node = _tvEstructura.SelectedNode;
            if (node?.Parent == null)
            { MostrarError(T("perm.msg.selquitar", "Seleccioná un ítem que cuelgue de un rol para quitarlo.")); return; }

            var padre = node.Parent.Tag as BE.Componente;
            var hijo  = node.Tag as BE.Componente;
            if (padre == null || hijo == null) return;

            if (MessageBox.Show(
                    string.Format(T("perm.conf.quitar", "¿Quitar '{0}' del rol '{1}'?"), hijo.Nombre, padre.Nombre),
                    T("perm.conf.titulo", "Confirmar"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try
            {
                _familiaBLL.QuitarComponente(padre.Id, hijo.Id);
                GUI.Menu.RefrescarSeguridadAbierta();
                MostrarOk(string.Format(T("perm.ok.quitado", "'{0}' quitado de '{1}'."), hijo.Nombre, padre.Nombre));
                CargarArbol();
            }
            catch (Exception ex) { MostrarError(ex); }
        }

        // ── Helpers ────────────────────────────────────────────────────────────────
        private void SeleccionarNodoPorId(TreeNodeCollection nodos, int id)
        {
            foreach (TreeNode n in nodos)
            {
                if (n.Tag is BE.Componente c && c.Id == id) { _tvEstructura.SelectedNode = n; return; }
                SeleccionarNodoPorId(n.Nodes, id);
            }
        }

        // ── Paleta de marca ────────────────────────────────────────────────────────
        private static readonly Color RosaPrimario = Color.FromArgb(210, 100, 135);  // #D26487
        private static readonly Color RosaOscuro   = Color.FromArgb(176, 62, 96);    // #B03E60
        private static readonly Color Peligro      = Color.FromArgb(200, 60, 60);    // #C83C3C
        private static readonly Color PanelClaro   = Color.FromArgb(245, 245, 250);  // #F5F5FA
        private static readonly Color Neutro       = Color.FromArgb(236, 236, 242);

        // ── Construcción de UI ─────────────────────────────────────────────────────
        private void ConstruirUI()
        {
            this.Text          = "Gestor de Perfiles — Roles y Permisos (Composite)";
            this.Size          = new Size(840, 700);
            this.MinimumSize   = new Size(800, 640);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor     = Color.White;
            this.Font          = new Font("Segoe UI", 9f);

            // ── Header con degradé de marca ──────────────────────────────────────
            _panelHeader = new Panel { Dock = DockStyle.Top, Height = 58 };
            _panelHeader.Paint += (s, pe) =>
            {
                using (var br = new LinearGradientBrush(_panelHeader.ClientRectangle,
                    RosaPrimario, RosaOscuro, LinearGradientMode.Horizontal))
                    pe.Graphics.FillRectangle(br, _panelHeader.ClientRectangle);
            };
            _lblTitulo = new Label { Text = "Perfiles y Permisos", Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.White, BackColor = Color.Transparent, AutoSize = true, Location = new Point(18, 8) };
            _lblSubtitulo = new Label { Text = "Gestión de roles (los permisos son un catálogo fijo)", Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = Color.FromArgb(255, 224, 236), BackColor = Color.Transparent, AutoSize = true, Location = new Point(20, 34) };
            _panelHeader.Controls.Add(_lblTitulo);
            _panelHeader.Controls.Add(_lblSubtitulo);

            _tip = new ToolTip { AutoPopDelay = 30000, InitialDelay = 250, ReshowDelay = 100, ShowAlways = true, IsBalloon = true };

            // ── Footer: mensaje (izq) + acciones globales (der) ───────────────────
            _panelFooter = new Panel { Dock = DockStyle.Bottom, Height = 52, BackColor = PanelClaro };
            var flAcciones = new FlowLayoutPanel { Dock = DockStyle.Right, FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false, AutoSize = false, Width = 500, Padding = new Padding(0, 11, 10, 0), BackColor = PanelClaro };
            _btnCerrar = MakeBtn("Cerrar", Point.Empty, 96, EstiloBtn.Neutral);
            _btnCerrar.Margin = new Padding(8, 0, 0, 0); _btnCerrar.Click += (s, e) => this.Close();
            _btnExplorador = MakeBtn("🌳 Ver vista completa del sistema", Point.Empty, 240, EstiloBtn.Light);
            _btnExplorador.Margin = new Padding(8, 0, 0, 0); _btnExplorador.Click += (s, e) => new ExploradorCompositeForm().Show(this);
            _btnActualizar = MakeBtn("↻ Actualizar", Point.Empty, 120, EstiloBtn.Light);
            _btnActualizar.Margin = new Padding(8, 0, 0, 0); _btnActualizar.Click += (s, e) => CargarArbol();
            flAcciones.Controls.Add(_btnCerrar);
            flAcciones.Controls.Add(_btnExplorador);
            flAcciones.Controls.Add(_btnActualizar);

            _lblMensaje = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(16, 0, 0, 0), Font = new Font("Segoe UI", 8.5f), ForeColor = Color.DimGray };
            _panelFooter.Controls.Add(_lblMensaje);
            _panelFooter.Controls.Add(flAcciones);

            // ── Columna izquierda: estructura (TreeView) ───────────────────────────
            _lblEstructura = SeccionLbl("Estructura del sistema", new Point(16, 70));
            _tvEstructura = new TreeView { Location = new Point(16, 92), Size = new Size(380, 452),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left,
                Font = new Font("Segoe UI", 9f), BorderStyle = BorderStyle.FixedSingle, HideSelection = false };
            _tvEstructura.AfterSelect += Tv_AfterSelect;

            // ── Columna central: panel de acciones con modo Crear/Editar ───────────
            int cx = 420;                          // x de la columna central
            _lblDetalle = new Label { Location = new Point(cx, 72), AutoSize = false, Size = new Size(340, 20),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic), ForeColor = RosaOscuro,
                Text = "Detalle: (sin selección)" };

            _lblModo = new Label { Text = "Modo:", Location = new Point(cx, 100), AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = RosaOscuro };
            _rbCrear  = new RadioButton { Text = "Crear", Location = new Point(cx + 52, 98), AutoSize = true, Checked = true };
            _rbEditar = new RadioButton { Text = "Editar / Eliminar", Location = new Point(cx + 120, 98), AutoSize = true };
            _rbCrear.CheckedChanged  += (s, e) => { if (_rbCrear.Checked)  { _txtNombreRol.Text = ""; ActualizarControles(); } };
            _rbEditar.CheckedChanged += (s, e) => { if (_rbEditar.Checked) ActualizarControles(); };

            _lblNombreRol = new Label { Text = "Nombre del rol:", Location = new Point(cx, 130), AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = RosaOscuro };
            _txtNombreRol = new TextBox { Location = new Point(cx, 152), Size = new Size(340, 24), Font = new Font("Segoe UI", 9f) };

            // Grupo CREAR
            _grpCrear = new GroupBox { Text = "Crear rol", Location = new Point(cx, 186), Size = new Size(340, 110),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = RosaOscuro };
            _btnCrearRaiz = MakeBtn("➕ Crear rol raíz", new Point(14, 26), 312, EstiloBtn.Primary);
            _btnCrearRaiz.Click += (s, e) => CrearRolRaiz();
            _btnCrearSub  = MakeBtn("➕ Crear sub-rol", new Point(14, 64), 312, EstiloBtn.Light);
            _btnCrearSub.Click += (s, e) => CrearSubRol();
            _grpCrear.Controls.Add(_btnCrearRaiz);
            _grpCrear.Controls.Add(_btnCrearSub);

            // Grupo EDITAR
            _grpEditar = new GroupBox { Text = "Editar rol", Location = new Point(cx, 186), Size = new Size(340, 300),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = RosaOscuro, Visible = false };
            _btnEditarNombre = MakeBtn("✏ Renombrar rol", new Point(14, 26), 312, EstiloBtn.Light);
            _btnEditarNombre.Click += (s, e) => EditarNombre();
            _btnEliminarRol  = MakeBtn("🗑 Eliminar rol", new Point(14, 62), 312, EstiloBtn.Danger);
            _btnEliminarRol.Click += (s, e) => EliminarRolSel();

            _grpAsignar = new GroupBox { Text = "Asignar permiso o rol", Location = new Point(14, 104), Size = new Size(312, 116),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = RosaOscuro };
            _lblAsignar = new Label { Text = "Elegí un ítem:", Location = new Point(12, 26), AutoSize = true, Font = new Font("Segoe UI", 8.5f) };
            _cmbAsignables = new ComboBox { Location = new Point(12, 46), Size = new Size(288, 24),
                DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9f) };
            _btnAsignar = MakeBtn("Asignar ↓", new Point(12, 76), 288, EstiloBtn.Primary);
            _btnAsignar.Click += (s, e) => Asignar();
            _grpAsignar.Controls.Add(_lblAsignar);
            _grpAsignar.Controls.Add(_cmbAsignables);
            _grpAsignar.Controls.Add(_btnAsignar);

            _btnQuitar = MakeBtn("Quitar ítem seleccionado", new Point(14, 230), 312, EstiloBtn.Light);
            _btnQuitar.Click += (s, e) => QuitarItem();

            _grpEditar.Controls.Add(_btnEditarNombre);
            _grpEditar.Controls.Add(_btnEliminarRol);
            _grpEditar.Controls.Add(_grpAsignar);
            _grpEditar.Controls.Add(_btnQuitar);

            this.Controls.AddRange(new Control[]
            {
                _lblEstructura, _tvEstructura,
                _lblDetalle, _lblModo, _rbCrear, _rbEditar, _lblNombreRol, _txtNombreRol,
                _grpCrear, _grpEditar
            });
            this.Controls.Add(_panelFooter);  _panelFooter.BringToFront();
            this.Controls.Add(_panelHeader);  _panelHeader.BringToFront();

            ActualizarControles();
        }

        // Etiqueta de sección con el estilo de marca.
        private static Label SeccionLbl(string text, Point loc)
            => new Label { Text = text, Location = loc, AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = RosaOscuro };

        private enum EstiloBtn { Primary, Danger, Light, Neutral }

        // Fábrica de botones con la paleta unificada.
        private static Button MakeBtn(string text, Point loc, int width, EstiloBtn estilo)
        {
            Color back, fore, borde = Color.Empty; int bsize = 0;
            switch (estilo)
            {
                case EstiloBtn.Primary: back = RosaPrimario; fore = Color.White; break;
                case EstiloBtn.Danger:  back = Peligro;      fore = Color.White; break;
                case EstiloBtn.Neutral: back = Neutro;       fore = Color.FromArgb(70, 70, 80); break;
                default:                back = Color.White;   fore = RosaOscuro; bsize = 1; borde = RosaOscuro; break; // Light
            }
            var b = new Button { Text = text, Location = loc, Size = new Size(width, 30), BackColor = back,
                ForeColor = fore, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8.5f), Cursor = Cursors.Hand };
            b.FlatAppearance.BorderSize = bsize;
            if (bsize > 0) b.FlatAppearance.BorderColor = borde;
            return b;
        }
    }
}
