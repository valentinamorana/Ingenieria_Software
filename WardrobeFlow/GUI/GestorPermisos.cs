using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Servicios.Multiidioma;

namespace GUI
{
    /// <summary>
    /// T04 — Gestión de Perfiles de Usuario (Patrón Composite) — formulario funcional.
    ///
    /// Asignación mediante DOS LISTAS (según apunte de diseño):
    ///   • Lista de Familias  → agrega/quita permisos COMPUESTOS al rol.
    ///   • Lista de Patentes  → agrega/quita permisos SIMPLES al rol.
    /// Además permite embeber un rol dentro de otro (composición de roles),
    /// crear patentes/familias/roles y eliminar roles (con bloqueo si están en uso).
    ///
    /// El TreeView de la derecha muestra, de forma RECURSIVA, la composición
    /// efectiva resultante del rol seleccionado, y se actualiza al guardar.
    ///
    /// La composición se persiste en [PermisoRelacion] (motor real de autorización).
    /// </summary>
    public class GestorPermisos : FormBase, IIdiomaObserver
    {
        protected override Label MensajeLabel => _lblMensaje;

        private readonly BLL.Familia _familiaBLL = new BLL.Familia();

        // Item con Id + nombre para los CheckedListBox / ComboBox.
        private class Item
        {
            public int Id;
            public string Nombre;
            public string NombreMenu;   // solo patentes
            public bool EsFamilia;
            public override string ToString() => Nombre;
        }

        private List<string> _roles = new List<string>();
        private string RolActual => _cmbRol.SelectedItem is Item it ? it.Nombre : null;
        private int RolActualId => _cmbRol.SelectedItem is Item it ? it.Id : 0;

        // ── Controles ──────────────────────────────────────────────────────────
        private Label    _lblTitulo, _lblMensaje, _lblRol, _lblFamilias, _lblPatentes, _lblArbol, _lblEmbeber;
        private ComboBox _cmbRol, _cmbEmbeber;
        private CheckedListBox _clbFamilias, _clbPatentes;
        private TreeView _treeView;
        // Toggle: ver la composición del rol seleccionado (off) o TODOS los roles a la vez (on).
        private CheckBox _chkVerTodos;
        private Button   _btnGuardar, _btnEmbeber, _btnNuevoRol, _btnEliminarRol,
                         _btnNuevaPatente, _btnNuevaFamilia, _btnExplorador, _btnCerrar,
                         _btnModificarComp, _btnEliminarComp;

        public GestorPermisos() { ConstruirUI(); }

        // ── Ciclo de vida ────────────────────────────────────────────────────
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GestorIdioma.SuscribirObservador(this);
            Traducir();
            CargarRoles();
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

        // Aplica el idioma actual a TODOS los textos fijos (título, labels y botones).
        private void Traducir()
        {
            this.Text              = T("frm.gestorpermisos",      "Gestor de Perfiles — Permisos (Composite)");
            _lblTitulo.Text        = T("lbl.permisos.titulo",     "Perfiles y Permisos");
            _lblRol.Text           = T("lbl.permisos.rol",        "Rol:");
            _lblFamilias.Text      = T("lbl.permisos.familias",   "Lista de Familias (compuestos)");
            _lblPatentes.Text      = T("lbl.permisos.patentes",   "Lista de Patentes (simples)");
            _lblArbol.Text         = T("lbl.permisos.composicion","Composición efectiva (recursiva)");
            _chkVerTodos.Text      = T("chk.permisos.vertodos",   "Ver todos los roles");
            _lblEmbeber.Text       = T("lbl.permisos.embeber",    "Embeber rol:");
            _btnNuevoRol.Text      = T("btn.permisos.nuevorol",     "➕ Nuevo rol");
            _btnEliminarRol.Text   = T("btn.permisos.eliminarrol",  "🗑 Eliminar rol");
            _btnNuevaFamilia.Text  = T("btn.permisos.nuevafamilia", "➕ Nueva familia");
            _btnNuevaPatente.Text  = T("btn.permisos.nuevapatente", "➕ Nueva patente");
            _btnEmbeber.Text       = T("btn.permisos.embeber",      "Embeber ➜");
            _btnModificarComp.Text = T("btn.permisos.modificar",    "✏ Modificar selección");
            _btnEliminarComp.Text  = T("btn.permisos.eliminar",     "🗑 Eliminar selección");
            _btnGuardar.Text       = T("btn.permisos.guardar",      "💾 Guardar cambios");
            _btnExplorador.Text    = T("btn.explorador",            "🌳 Explorador Composite");
            _btnCerrar.Text        = T("btn.permisos.cerrar",       "Cerrar");
        }

        // ── Carga de datos ─────────────────────────────────────────────────────
        private void CargarRoles()
        {
            try
            {
                _roles = _familiaBLL.ObtenerRoles();
                _cmbRol.SelectedIndexChanged -= CmbRol_Changed;
                _cmbRol.Items.Clear();
                foreach (string rol in _roles)
                    _cmbRol.Items.Add(new Item { Id = 0, Nombre = rol });
                _cmbRol.SelectedIndexChanged += CmbRol_Changed;
                if (_cmbRol.Items.Count > 0) _cmbRol.SelectedIndex = 0;
                else { _clbFamilias.Items.Clear(); _clbPatentes.Items.Clear(); _treeView.Nodes.Clear(); }
            }
            catch (Exception ex) { MostrarError(string.Format(T("err.generico.cargar", "Error al cargar: {0}"), ex.Message)); }
        }

        private void CargarListas()
        {
            string rol = RolActual;
            if (string.IsNullOrEmpty(rol)) return;
            try
            {
                var directos = _familiaBLL.ObtenerIdsDirectosDelRol(rol);

                _clbFamilias.Items.Clear();
                foreach (var f in _familiaBLL.ObtenerFamiliasDisponibles())
                    _clbFamilias.Items.Add(new Item { Id = f.Id, Nombre = f.Nombre, EsFamilia = true }, directos.Contains(f.Id));

                _clbPatentes.Items.Clear();
                foreach (var p in _familiaBLL.ObtenerPatentesDisponibles())
                    _clbPatentes.Items.Add(new Item { Id = p.Id, Nombre = p.Nombre, NombreMenu = p.NombreMenu, EsFamilia = false }, directos.Contains(p.Id));

                // Otros roles para embeber (excluye el actual).
                _cmbEmbeber.Items.Clear();
                foreach (string r in _roles)
                    if (!string.Equals(r, rol, StringComparison.OrdinalIgnoreCase))
                        _cmbEmbeber.Items.Add(new Item { Id = 0, Nombre = r });
                if (_cmbEmbeber.Items.Count > 0) _cmbEmbeber.SelectedIndex = 0;

                RefrescarArbol();
                _btnGuardar.Enabled = true;
            }
            catch (Exception ex) { MostrarError(string.Format(T("err.generico.cargar", "Error al cargar: {0}"), ex.Message)); }
        }

        // Decide qué mostrar en el árbol según el toggle "Ver todos los roles".
        private void RefrescarArbol()
        {
            if (_chkVerTodos != null && _chkVerTodos.Checked) MostrarTodosLosRoles();
            else MostrarComposicion();
        }

        // Vista GENERAL: todos los roles del sistema con su composición efectiva (recursiva),
        // colgados de una raíz común. Útil para comparar de un vistazo qué hace cada rol.
        private void MostrarTodosLosRoles()
        {
            try
            {
                _treeView.BeginUpdate();
                _treeView.Nodes.Clear();
                var raiz = new TreeNode("🗂 " + T("perm.todoslosroles", "Todos los roles"))
                {
                    NodeFont  = new Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(64, 0, 64)
                };
                foreach (BE.Componente comp in _familiaBLL.ObtenerArbol())
                {
                    string icono = comp is BE.Rol ? "👥 " : comp is BE.Familia ? "📁 " : "• ";
                    var nodo = new TreeNode(icono + comp.Nombre)
                    {
                        Tag      = comp,
                        NodeFont = new Font("Segoe UI", 9f, FontStyle.Bold)
                    };
                    raiz.Nodes.Add(nodo);
                    AgregarNodos(comp, nodo);
                }
                _treeView.Nodes.Add(raiz);
                _treeView.ExpandAll();
                _treeView.EndUpdate();
            }
            catch (Exception ex) { MostrarError(string.Format(T("err.generico.cargar", "Error al cargar: {0}"), ex.Message)); }
        }

        // TreeView recursivo con la composición efectiva del rol.
        private void MostrarComposicion()
        {
            string rol = RolActual;
            if (string.IsNullOrEmpty(rol)) return;
            try
            {
                _treeView.BeginUpdate();
                _treeView.Nodes.Clear();
                BE.Familia arbol = _familiaBLL.ObtenerArbolPorRol(rol);
                var raiz = new TreeNode("👤 " + rol) { NodeFont = new Font("Segoe UI", 9f, FontStyle.Bold) };
                _treeView.Nodes.Add(raiz);
                AgregarNodos(arbol, raiz);
                raiz.Expand();
                _treeView.ExpandAll();
                _treeView.EndUpdate();
            }
            catch (Exception ex) { MostrarError(string.Format(T("err.generico.cargar", "Error al cargar: {0}"), ex.Message)); }
        }

        private void AgregarNodos(BE.Componente componente, TreeNode parent)
        {
            foreach (BE.Componente hijo in componente.Hijos)
            {
                string icono = hijo is BE.Rol ? "👥 " : hijo is BE.Familia ? "📁 " : "• ";
                var nodo = new TreeNode(icono + hijo.Nombre) { Tag = hijo };
                if (hijo is BE.Familia)
                {
                    nodo.NodeFont  = new Font("Segoe UI", 9f, FontStyle.Bold);
                    nodo.ForeColor = Color.FromArgb(40, 80, 140);
                }
                parent.Nodes.Add(nodo);
                AgregarNodos(hijo, nodo);
            }
        }

        // ── Guardar ──────────────────────────────────────────────────────────
        private void GuardarCambios()
        {
            string rol = RolActual;
            if (string.IsNullOrEmpty(rol)) return;
            try
            {
                var ids = new List<int>();
                foreach (Item it in _clbFamilias.CheckedItems) ids.Add(it.Id);
                foreach (Item it in _clbPatentes.CheckedItems) ids.Add(it.Id);

                var (agregados, quitados) = _familiaBLL.GuardarAsignacionRol(rol, ids);
                CargarListas();
                MostrarOk(string.Format(T("perm.ok.guardado", "Cambios guardados para '{0}': {1} agregado(s), {2} quitado(s)."), rol, agregados, quitados));
            }
            catch (Exception ex) { MostrarError(string.Format(T("err.generico.guardar", "Error al guardar: {0}"), ex.Message)); }
        }

        // ── CRUD / acciones ────────────────────────────────────────────────────
        private void EmbeberRol()
        {
            string rol = RolActual;
            if (string.IsNullOrEmpty(rol) || !(_cmbEmbeber.SelectedItem is Item hijo)) return;
            try
            {
                int idHijo  = ObtenerIdRol(hijo.Nombre);
                int idPadre = ObtenerIdRol(rol);
                if (idPadre == 0 || idHijo == 0) { MostrarError(T("perm.msg.norol", "No se pudo resolver el rol.")); return; }
                _familiaBLL.AgregarComponente(idPadre, idHijo);
                CargarListas();
                MostrarOk(string.Format(T("perm.ok.embebido", "Rol '{0}' embebido dentro de '{1}'."), hijo.Nombre, rol));
            }
            catch (Exception ex) { MostrarError(string.Format(T("perm.err.embeber", "No se pudo embeber: {0}"), ex.Message)); }
        }

        // Resuelve el Id de un rol buscando su nodo en el árbol.
        private int ObtenerIdRol(string rol)
        {
            foreach (var raiz in _familiaBLL.ObtenerArbol())
            {
                int id = BuscarIdRol(raiz, rol);
                if (id != 0) return id;
            }
            return 0;
        }
        private int BuscarIdRol(BE.Componente nodo, string rol)
        {
            if (nodo is BE.Rol && string.Equals(nodo.Nombre, rol, StringComparison.OrdinalIgnoreCase))
                return nodo.Id;
            if (nodo is BE.Familia fam)
                foreach (var h in fam.Hijos) { int id = BuscarIdRol(h, rol); if (id != 0) return id; }
            return 0;
        }

        private void NuevoRol()
        {
            string nombre = Pedir(T("perm.dlg.rol.t", "Nuevo rol"), T("perm.dlg.rol.p", "Nombre del nuevo rol:"));
            if (string.IsNullOrWhiteSpace(nombre)) return;
            try { _familiaBLL.CrearRol(nombre.Trim()); CargarRoles(); SeleccionarRol(nombre.Trim());
                  MostrarOk(string.Format(T("perm.ok.rolcreado", "Rol '{0}' creado."), nombre.Trim())); }
            catch (Exception ex) { MostrarError(ex.Message); }
        }

        private void EliminarRol()
        {
            string rol = RolActual;
            if (string.IsNullOrEmpty(rol)) return;
            if (MessageBox.Show(string.Format(T("perm.conf.elirol", "¿Eliminar el rol '{0}'?"), rol), T("perm.conf.titulo", "Confirmar"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            try { _familiaBLL.EliminarRol(rol); CargarRoles(); MostrarOk(string.Format(T("perm.ok.roleli", "Rol '{0}' eliminado."), rol)); }
            catch (Exception ex) { MostrarError(ex.Message); }
        }

        private void NuevaPatente()
        {
            string nombre = Pedir(T("perm.dlg.pat.t", "Nueva patente"), T("perm.dlg.pat.p", "Nombre de la patente (permiso simple):"));
            if (string.IsNullOrWhiteSpace(nombre)) return;
            string menu = Pedir(T("perm.dlg.pat.t", "Nueva patente"), T("perm.dlg.pat.menu", "NombreMenu asociado (opcional, ej: mnuClientes):"));
            try { _familiaBLL.CrearPatente(nombre.Trim(), string.IsNullOrWhiteSpace(menu) ? null : menu.Trim());
                  CargarListas(); MostrarOk(string.Format(T("perm.ok.patcreada", "Patente '{0}' creada."), nombre.Trim())); }
            catch (Exception ex) { MostrarError(ex.Message); }
        }

        private void NuevaFamilia()
        {
            string nombre = Pedir(T("perm.dlg.fam.t", "Nueva familia"), T("perm.dlg.fam.p", "Nombre de la familia (permiso compuesto):"));
            if (string.IsNullOrWhiteSpace(nombre)) return;
            try { _familiaBLL.CrearFamilia(nombre.Trim()); CargarListas();
                  MostrarOk(string.Format(T("perm.ok.famcreada", "Familia '{0}' creada."), nombre.Trim())); }
            catch (Exception ex) { MostrarError(ex.Message); }
        }

        // Componente (patente o familia) seleccionado en cualquiera de las dos listas.
        private Item ComponenteSeleccionado()
        {
            if (_clbFamilias.SelectedItem is Item f) return f;
            if (_clbPatentes.SelectedItem is Item p) return p;
            return null;
        }

        private void ModificarComponente()
        {
            var item = ComponenteSeleccionado();
            if (item == null) { MostrarError(T("perm.msg.selmod", "Seleccioná una patente o familia para modificar.")); return; }
            string nombre = Pedir(T("perm.dlg.mod.t", "Modificar componente"), T("perm.dlg.mod.p", "Nuevo nombre:"));
            if (string.IsNullOrWhiteSpace(nombre)) return;
            // Para patentes se conserva (o edita) el NombreMenu; para familias se usa el nombre.
            string menu = item.EsFamilia ? nombre.Trim()
                                         : (Pedir(T("perm.dlg.mod.t", "Modificar componente"), T("perm.dlg.mod.menu", "NombreMenu asociado:")) ?? item.NombreMenu);
            try
            {
                _familiaBLL.RenombrarComponente(item.Id, nombre.Trim(), menu);
                CargarListas();
                MostrarOk(string.Format(T("perm.ok.modificado", "Componente actualizado a '{0}'."), nombre.Trim()));
            }
            catch (Exception ex) { MostrarError(ex.Message); }
        }

        private void EliminarComponente()
        {
            var item = ComponenteSeleccionado();
            if (item == null) { MostrarError(T("perm.msg.seleli", "Seleccioná una patente o familia para eliminar.")); return; }
            string tipo = item.EsFamilia ? T("perm.tipo.familia", "la familia") : T("perm.tipo.patente", "la patente");
            if (MessageBox.Show(string.Format(T("perm.conf.elicomp", "¿Eliminar {0} '{1}'? Se quitará de todos los roles."), tipo, item.Nombre),
                    T("perm.conf.titulo", "Confirmar"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            try
            {
                _familiaBLL.EliminarComponente(item.Id);
                CargarListas();
                MostrarOk(string.Format(T("perm.ok.eliminado", "'{0}' eliminado."), item.Nombre));
            }
            catch (Exception ex) { MostrarError(ex.Message); }
        }

        private void SeleccionarRol(string rol)
        {
            for (int i = 0; i < _cmbRol.Items.Count; i++)
                if (_cmbRol.Items[i] is Item it && string.Equals(it.Nombre, rol, StringComparison.OrdinalIgnoreCase))
                { _cmbRol.SelectedIndex = i; return; }
        }

        // Mini cuadro de diálogo de entrada de texto (sin dependencias externas).
        private static string Pedir(string titulo, string prompt)
        {
            var tr = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
            string txtOk = tr.ContainsKey("btn.aceptar")  ? tr["btn.aceptar"].Texto  : "Aceptar";
            string txtCa = tr.ContainsKey("btn.cancelar") ? tr["btn.cancelar"].Texto : "Cancelar";
            using (var f = new Form())
            {
                f.Text = titulo; f.Size = new Size(420, 160); f.StartPosition = FormStartPosition.CenterParent;
                f.FormBorderStyle = FormBorderStyle.FixedDialog; f.MinimizeBox = false; f.MaximizeBox = false;
                var lbl = new Label { Text = prompt, Location = new Point(12, 15), AutoSize = true };
                var txt = new TextBox { Location = new Point(15, 45), Size = new Size(380, 24) };
                var ok  = new Button { Text = txtOk, DialogResult = DialogResult.OK, Location = new Point(225, 80), Size = new Size(80, 30) };
                var ca  = new Button { Text = txtCa, DialogResult = DialogResult.Cancel, Location = new Point(315, 80), Size = new Size(80, 30) };
                f.Controls.AddRange(new Control[] { lbl, txt, ok, ca });
                f.AcceptButton = ok; f.CancelButton = ca;
                return f.ShowDialog() == DialogResult.OK ? txt.Text : null;
            }
        }

        // ── Eventos ──────────────────────────────────────────────────────────
        private void CmbRol_Changed(object sender, EventArgs e) => CargarListas();

        // ── Construcción de UI ───────────────────────────────────────────────
        private void ConstruirUI()
        {
            this.Text            = "Gestor de Perfiles — Permisos (Composite)";
            this.Size            = new Size(940, 640);
            this.MinimumSize     = new Size(880, 560);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.BackColor       = Color.White;

            _lblTitulo = new Label { Text = "Perfiles y Permisos", Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 80, 140), AutoSize = true, Location = new Point(12, 12) };
            _lblMensaje = new Label { AutoSize = false, Size = new Size(900, 20), Location = new Point(12, 42),
                Font = new Font("Segoe UI", 8.5f), ForeColor = Color.DimGray };

            _lblRol = new Label { Text = "Rol:", Location = new Point(12, 76), AutoSize = true, Font = new Font("Segoe UI", 9f) };
            _cmbRol = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(50, 72),
                Size = new Size(240, 24), Font = new Font("Segoe UI", 9f) };
            _cmbRol.SelectedIndexChanged += CmbRol_Changed;

            _btnNuevoRol = Btn("➕ Nuevo rol", new Point(300, 71), 110, Color.FromArgb(40, 110, 60));
            _btnNuevoRol.Click += (s, e) => NuevoRol();
            _btnEliminarRol = Btn("🗑 Eliminar rol", new Point(416, 71), 120, Color.FromArgb(170, 50, 50));
            _btnEliminarRol.Click += (s, e) => EliminarRol();

            // Lista de Familias
            _lblFamilias = new Label { Text = "Lista de Familias (compuestos)", Location = new Point(12, 112),
                AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(40, 80, 140) };
            _clbFamilias = new CheckedListBox { Location = new Point(12, 134), Size = new Size(270, 300),
                CheckOnClick = true, Font = new Font("Segoe UI", 9f), BorderStyle = BorderStyle.FixedSingle };
            _btnNuevaFamilia = Btn("➕ Nueva familia", new Point(12, 440), 130, Color.FromArgb(80, 100, 150));
            _btnNuevaFamilia.Click += (s, e) => NuevaFamilia();

            // Lista de Patentes
            _lblPatentes = new Label { Text = "Lista de Patentes (simples)", Location = new Point(300, 112),
                AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(40, 80, 140) };
            _clbPatentes = new CheckedListBox { Location = new Point(300, 134), Size = new Size(270, 300),
                CheckOnClick = true, Font = new Font("Segoe UI", 9f), BorderStyle = BorderStyle.FixedSingle };
            _btnNuevaPatente = Btn("➕ Nueva patente", new Point(300, 440), 130, Color.FromArgb(80, 100, 150));
            _btnNuevaPatente.Click += (s, e) => NuevaPatente();

            // Embeber rol
            _lblEmbeber = new Label { Text = "Embeber rol:", Location = new Point(12, 478), AutoSize = true, Font = new Font("Segoe UI", 9f) };
            _cmbEmbeber = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(95, 474),
                Size = new Size(185, 24), Font = new Font("Segoe UI", 9f) };
            _btnEmbeber = Btn("Embeber ➜", new Point(290, 473), 110, Color.FromArgb(64, 0, 64));
            _btnEmbeber.Click += (s, e) => EmbeberRol();

            // ABM de la patente/familia seleccionada (modificar / eliminar)
            _btnModificarComp = Btn("✏ Modificar selección", new Point(12, 508), 175, Color.FromArgb(80, 100, 150));
            _btnModificarComp.Click += (s, e) => ModificarComponente();
            _btnEliminarComp = Btn("🗑 Eliminar selección", new Point(195, 508), 175, Color.FromArgb(170, 50, 50));
            _btnEliminarComp.Click += (s, e) => EliminarComponente();

            // Árbol de composición
            _lblArbol = new Label { Text = "Composición efectiva (recursiva)", Location = new Point(588, 112),
                AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(40, 80, 140) };
            _chkVerTodos = new CheckBox { Text = "Ver todos los roles", Location = new Point(760, 112),
                AutoSize = true, Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(64, 0, 64),
                Anchor = AnchorStyles.Top | AnchorStyles.Right };
            _chkVerTodos.CheckedChanged += (s, e) => RefrescarArbol();
            _treeView = new TreeView { Location = new Point(588, 134), Size = new Size(324, 360),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right,
                Font = new Font("Segoe UI", 9f), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.WhiteSmoke };

            // Acciones inferiores
            _btnGuardar = Btn("💾 Guardar cambios", new Point(12, 558), 160, Color.FromArgb(40, 110, 60));
            _btnGuardar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left; _btnGuardar.Enabled = false;
            _btnGuardar.Click += (s, e) => GuardarCambios();
            _btnExplorador = Btn("🌳 Explorador Composite", new Point(180, 558), 190, Color.FromArgb(64, 0, 64));
            _btnExplorador.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            _btnExplorador.Click += (s, e) => new ExploradorCompositeForm().Show(this);
            _btnCerrar = Btn("Cerrar", new Point(814, 558), 98, Color.FromArgb(210, 210, 210));
            _btnCerrar.ForeColor = Color.Black; _btnCerrar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _btnCerrar.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[]
            {
                _lblTitulo, _lblMensaje, _lblRol, _cmbRol, _btnNuevoRol, _btnEliminarRol,
                _lblFamilias, _clbFamilias, _btnNuevaFamilia,
                _lblPatentes, _clbPatentes, _btnNuevaPatente,
                _lblEmbeber, _cmbEmbeber, _btnEmbeber,
                _btnModificarComp, _btnEliminarComp,
                _lblArbol, _chkVerTodos, _treeView,
                _btnGuardar, _btnExplorador, _btnCerrar
            });
        }

        private static Button Btn(string text, Point loc, int width, Color back)
        {
            var b = new Button { Text = text, Location = loc, Size = new Size(width, 30), BackColor = back,
                ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8.5f), Cursor = Cursors.Hand };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }
    }
}
