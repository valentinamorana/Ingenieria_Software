using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Servicios.Multiidioma;

namespace GUI
{
    /// <summary>
    /// T04 — Gestión de Perfiles de Usuario (Patrón Composite) — UI estilo "árbol + dos listas"
    /// (como el ejemplo de cátedra de lotes-dentro-de-lotes):
    ///
    ///   • Izquierda: TreeView con TODA la estructura del sistema (Roles 👥, Familias 📁, Patentes 🔑).
    ///     Se selecciona el NODO PADRE que se quiere editar.
    ///   • Centro: dos listas — "Disponibles para agregar" y "Miembros de [nodo]". Con Agregar/Quitar
    ///     se anida cualquier componente DENTRO del nodo seleccionado (familia-en-familia, rol-en-rol,
    ///     patente-en-familia, etc.). La validación anti-ciclos vive en la BLL.
    ///   • Derecha: árbol de PERMISOS EFECTIVOS (recursivo) del nodo seleccionado — lo que realmente
    ///     concede ese rol/familia tras resolver toda la jerarquía.
    ///
    /// La composición se persiste en [PermisoRelacion] (motor real de autorización) de forma
    /// INMEDIATA (cada Agregar/Quitar guarda y re-aplica la seguridad en vivo).
    /// </summary>
    public class GestorPermisos : FormBase, IIdiomaObserver
    {
        protected override Label MensajeLabel => _lblMensaje;

        private readonly BLL.Familia _familiaBLL = new BLL.Familia();

        // ── Controles ──────────────────────────────────────────────────────────
        private Label    _lblTitulo, _lblMensaje, _lblEstructura, _lblDisponibles, _lblMiembros, _lblEfectivo;
        private TreeView _tvEstructura, _tvEfectivo;
        private ListBox  _lstDisponibles, _lstMiembros;
        private Button   _btnAgregar, _btnQuitar, _btnNuevaPatente, _btnNuevaFamilia, _btnNuevoRol,
                         _btnRenombrar, _btnEliminar, _btnExplorador, _btnCerrar;

        // ── Estado ───────────────────────────────────────────────────────────────
        private List<BE.Componente> _raices = new List<BE.Componente>();
        private readonly Dictionary<int, BE.Componente> _todos = new Dictionary<int, BE.Componente>();
        private BE.Componente _seleccionado;

        // Envoltura para mostrar un componente en las ListBox con su ícono.
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
            this.Text             = T("frm.gestorpermisos",      "Gestor de Perfiles — Permisos (Composite)");
            _lblTitulo.Text       = T("lbl.permisos.titulo",     "Perfiles y Permisos");
            _lblEstructura.Text   = T("lbl.permisos.estructura", "Estructura del sistema (roles, familias y patentes)");
            _lblDisponibles.Text  = T("lbl.permisos.disponibles","Disponibles para agregar");
            _lblEfectivo.Text     = T("lbl.permisos.efectivos",  "Permisos efectivos (recursivo)");
            _btnAgregar.Text      = T("btn.permisos.agregar",    "Agregar ↓");
            _btnQuitar.Text       = T("btn.permisos.quitar",     "Quitar ↑");
            _btnNuevaPatente.Text = T("btn.permisos.nuevapatente","➕ Patente");
            _btnNuevaFamilia.Text = T("btn.permisos.nuevafamilia","➕ Familia");
            _btnNuevoRol.Text     = T("btn.permisos.nuevorol",   "➕ Rol");
            _btnRenombrar.Text    = T("btn.permisos.modificar",  "✏ Renombrar");
            _btnEliminar.Text     = T("btn.permisos.eliminar",   "🗑 Eliminar");
            _btnExplorador.Text   = T("btn.explorador",          "🌳 Ver vista completa del sistema");
            _btnCerrar.Text       = T("btn.permisos.cerrar",     "Cerrar");
            ActualizarLabelMiembros();
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
                ActualizarPanelSeleccion();
            }
            catch (Exception ex)
            {
                MostrarError(string.Format(T("err.generico.cargar", "Error al cargar: {0}"), ex.Message));
            }
        }

        // Aplana el árbol a un diccionario id→componente (deduplicado) para la lista de Disponibles.
        private void Aplanar(BE.Componente nodo, HashSet<int> vis)
        {
            if (nodo.Id != 0 && !vis.Add(nodo.Id)) return;
            if (!_todos.ContainsKey(nodo.Id)) _todos[nodo.Id] = nodo;
            foreach (var h in nodo.Hijos) Aplanar(h, vis);
        }

        private TreeNode CrearNodo(BE.Componente comp, HashSet<int> vis)
        {
            var nodo = new TreeNode(Etiqueta(comp)) { Tag = comp };
            if (comp is BE.Familia)
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
            ActualizarPanelSeleccion();
        }

        // Un nodo es "contenedor" si es Familia (Rol hereda de Familia) → puede tener hijos.
        private static bool EsContenedor(BE.Componente c) => c is BE.Familia;

        private void ActualizarPanelSeleccion()
        {
            _lstDisponibles.DataSource = null;
            _lstMiembros.DataSource    = null;
            _tvEfectivo.Nodes.Clear();
            ActualizarLabelMiembros();

            if (_seleccionado == null) { HabilitarTransfer(false); return; }

            // Panel derecho: permisos EFECTIVOS (recursivos) del nodo seleccionado.
            var raizEf = new TreeNode(Etiqueta(_seleccionado))
            { NodeFont = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(176, 62, 96) };
            foreach (var p in _seleccionado.ObtenerPatentesEfectivas())
            {
                string menu = string.IsNullOrEmpty(p.NombreMenu) ? "" : "   (" + p.NombreMenu + ")";
                raizEf.Nodes.Add(new TreeNode("🔑 " + p.Nombre + menu));
            }
            _tvEfectivo.Nodes.Add(raizEf);
            raizEf.Expand();

            // Solo los contenedores (Familia/Rol) admiten miembros.
            if (!EsContenedor(_seleccionado)) { HabilitarTransfer(false); return; }
            HabilitarTransfer(true);

            var contenedor = (BE.Familia)_seleccionado;

            // Miembros = hijos directos.
            var miembros = new List<Item>();
            var idsHijos = new HashSet<int>();
            foreach (var h in contenedor.Hijos) { miembros.Add(new Item { Comp = h }); idsHijos.Add(h.Id); }
            _lstMiembros.DataSource = miembros;

            // Excluir de "Disponibles": el propio nodo y todo su subárbol (evita ciclos obvios)
            // y los que ya son miembros directos. El resto de ciclos los rechaza la BLL.
            var subarbol = new HashSet<int>();
            RecolectarSubarbol(_seleccionado, subarbol, new HashSet<int>());

            var disponibles = new List<Item>();
            foreach (var kv in _todos)
            {
                var c = kv.Value;
                if (c.Id == _seleccionado.Id) continue;
                if (idsHijos.Contains(c.Id))    continue;
                if (subarbol.Contains(c.Id))    continue;
                disponibles.Add(new Item { Comp = c });
            }
            disponibles.Sort((a, b) => string.Compare(a.Comp.Nombre, b.Comp.Nombre, StringComparison.OrdinalIgnoreCase));
            _lstDisponibles.DataSource = disponibles;
        }

        private void RecolectarSubarbol(BE.Componente nodo, HashSet<int> acc, HashSet<int> vis)
        {
            if (nodo.Id != 0 && !vis.Add(nodo.Id)) return;
            foreach (var h in nodo.Hijos) { acc.Add(h.Id); RecolectarSubarbol(h, acc, vis); }
        }

        private void HabilitarTransfer(bool on) { _btnAgregar.Enabled = on; _btnQuitar.Enabled = on; }

        private void ActualizarLabelMiembros()
        {
            if (_lblMiembros == null) return;
            _lblMiembros.Text = _seleccionado != null && EsContenedor(_seleccionado)
                ? string.Format(T("lbl.permisos.miembros", "Miembros de: {0}"), _seleccionado.Nombre)
                : T("lbl.permisos.miembros.vacio", "Miembros (seleccioná una Familia o Rol)");
        }

        // ── Transferencia (anidar / desanidar) ─────────────────────────────────────
        private void Agregar()
        {
            if (!EsContenedor(_seleccionado))
            { MostrarError(T("perm.msg.selpadre", "Seleccioná una Familia o Rol para agregarle hijos.")); return; }
            if (!(_lstDisponibles.SelectedItem is Item it)) return;
            try
            {
                _familiaBLL.AgregarComponente(_seleccionado.Id, it.Comp.Id);
                GUI.Menu.RefrescarSeguridadAbierta();   // re-aplica seguridad en vivo
                MostrarOk(string.Format(T("perm.ok.agregado", "'{0}' agregado a '{1}'."), it.Comp.Nombre, _seleccionado.Nombre));
                CargarArbol();
            }
            catch (Exception ex) { MostrarError(ex.Message); }
        }

        private void Quitar()
        {
            if (!EsContenedor(_seleccionado)) return;
            if (!(_lstMiembros.SelectedItem is Item it)) return;
            try
            {
                _familiaBLL.QuitarComponente(_seleccionado.Id, it.Comp.Id);
                GUI.Menu.RefrescarSeguridadAbierta();
                MostrarOk(string.Format(T("perm.ok.quitado", "'{0}' quitado de '{1}'."), it.Comp.Nombre, _seleccionado.Nombre));
                CargarArbol();
            }
            catch (Exception ex) { MostrarError(ex.Message); }
        }

        // ── CRUD de componentes ────────────────────────────────────────────────────
        private void NuevaPatente()
        {
            string nombre = Pedir(T("perm.dlg.pat.t", "Nueva patente"), T("perm.dlg.pat.p", "Nombre de la patente (permiso simple):"));
            if (string.IsNullOrWhiteSpace(nombre)) return;
            string menu = Pedir(T("perm.dlg.pat.t", "Nueva patente"), T("perm.dlg.pat.menu", "NombreMenu asociado (opcional, ej: mnuClientes):"));
            try
            {
                _familiaBLL.CrearPatente(nombre.Trim(), string.IsNullOrWhiteSpace(menu) ? null : menu.Trim());
                MostrarOk(string.Format(T("perm.ok.patcreada", "Patente '{0}' creada. Seleccioná una familia/rol y agregala."), nombre.Trim()));
                CargarArbol();
            }
            catch (Exception ex) { MostrarError(ex.Message); }
        }

        private void NuevaFamilia()
        {
            string nombre = Pedir(T("perm.dlg.fam.t", "Nueva familia"), T("perm.dlg.fam.p", "Nombre de la familia (permiso compuesto):"));
            if (string.IsNullOrWhiteSpace(nombre)) return;
            try
            {
                _familiaBLL.CrearFamilia(nombre.Trim());
                MostrarOk(string.Format(T("perm.ok.famcreada", "Familia '{0}' creada."), nombre.Trim()));
                CargarArbol();
            }
            catch (Exception ex) { MostrarError(ex.Message); }
        }

        private void NuevoRol()
        {
            string nombre = Pedir(T("perm.dlg.rol.t", "Nuevo rol"), T("perm.dlg.rol.p", "Nombre del nuevo rol:"));
            if (string.IsNullOrWhiteSpace(nombre)) return;
            try
            {
                _familiaBLL.CrearRol(nombre.Trim());
                MostrarOk(string.Format(T("perm.ok.rolcreado", "Rol '{0}' creado."), nombre.Trim()));
                CargarArbol();
            }
            catch (Exception ex) { MostrarError(ex.Message); }
        }

        private void Renombrar()
        {
            if (_seleccionado == null)
            { MostrarError(T("perm.msg.selmod", "Seleccioná un componente del árbol para modificar.")); return; }

            string nombre = Pedir(T("perm.dlg.mod.t", "Modificar componente"), T("perm.dlg.mod.p", "Nuevo nombre:"));
            if (string.IsNullOrWhiteSpace(nombre)) return;

            bool esPatente = !(_seleccionado is BE.Familia);
            string menu = esPatente
                ? (Pedir(T("perm.dlg.mod.t", "Modificar componente"), T("perm.dlg.mod.menu", "NombreMenu asociado:")) ?? nombre.Trim())
                : nombre.Trim();
            try
            {
                _familiaBLL.RenombrarComponente(_seleccionado.Id, nombre.Trim(), menu);
                GUI.Menu.RefrescarSeguridadAbierta();
                MostrarOk(string.Format(T("perm.ok.modificado", "Componente actualizado a '{0}'."), nombre.Trim()));
                CargarArbol();
            }
            catch (Exception ex) { MostrarError(ex.Message); }
        }

        private void Eliminar()
        {
            if (_seleccionado == null)
            { MostrarError(T("perm.msg.seleli", "Seleccioná un componente del árbol para eliminar.")); return; }

            string tipo = _seleccionado is BE.Rol ? T("perm.tipo.rol", "el rol")
                        : _seleccionado is BE.Familia ? T("perm.tipo.familia", "la familia")
                        : T("perm.tipo.patente", "la patente");

            if (MessageBox.Show(
                    string.Format(T("perm.conf.elicomp", "¿Eliminar {0} '{1}'? Se quitará de todos los nodos."), tipo, _seleccionado.Nombre),
                    T("perm.conf.titulo", "Confirmar"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            try
            {
                if (_seleccionado is BE.Rol)
                    _familiaBLL.EliminarRol(_seleccionado.Nombre);   // valida que no tenga usuarios asignados
                else
                    _familiaBLL.EliminarComponente(_seleccionado.Id);

                string nombre = _seleccionado.Nombre;
                _seleccionado = null;
                GUI.Menu.RefrescarSeguridadAbierta();
                MostrarOk(string.Format(T("perm.ok.eliminado", "'{0}' eliminado."), nombre));
                CargarArbol();
            }
            catch (Exception ex) { MostrarError(ex.Message); }
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

        // Mini diálogo de entrada de texto (sin dependencias externas).
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

        // ── Construcción de UI ─────────────────────────────────────────────────────
        private void ConstruirUI()
        {
            this.Text          = "Gestor de Perfiles — Permisos (Composite)";
            this.Size          = new Size(950, 660);
            this.MinimumSize   = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor     = Color.White;

            _lblTitulo  = new Label { Text = "Perfiles y Permisos", Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.FromArgb(176, 62, 96), AutoSize = true, Location = new Point(12, 12) };
            _lblMensaje = new Label { AutoSize = false, Size = new Size(910, 20), Location = new Point(12, 40),
                Font = new Font("Segoe UI", 8.5f), ForeColor = Color.DimGray, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };

            // ── Columna izquierda: estructura ──
            _lblEstructura = new Label { Text = "Estructura del sistema", Location = new Point(12, 64), AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(176, 62, 96) };
            _tvEstructura = new TreeView { Location = new Point(12, 86), Size = new Size(300, 400),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left,
                Font = new Font("Segoe UI", 9f), BorderStyle = BorderStyle.FixedSingle, HideSelection = false };
            _tvEstructura.AfterSelect += Tv_AfterSelect;

            _btnNuevaPatente = Btn("➕ Patente", new Point(12, 494),  95, Color.FromArgb(210, 100, 135));
            _btnNuevaPatente.Click += (s, e) => NuevaPatente();
            _btnNuevaFamilia = Btn("➕ Familia", new Point(110, 494), 95, Color.FromArgb(210, 100, 135));
            _btnNuevaFamilia.Click += (s, e) => NuevaFamilia();
            _btnNuevoRol     = Btn("➕ Rol",     new Point(208, 494), 95, Color.FromArgb(40, 110, 60));
            _btnNuevoRol.Click += (s, e) => NuevoRol();
            _btnRenombrar    = Btn("✏ Renombrar", new Point(12, 526), 145, Color.FromArgb(210, 100, 135));
            _btnRenombrar.Click += (s, e) => Renombrar();
            _btnEliminar     = Btn("🗑 Eliminar",  new Point(160, 526), 152, Color.FromArgb(170, 50, 50));
            _btnEliminar.Click += (s, e) => Eliminar();
            foreach (var b in new[] { _btnNuevaPatente, _btnNuevaFamilia, _btnNuevoRol, _btnRenombrar, _btnEliminar })
                b.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            // ── Columna central: dos listas (Disponibles / Miembros) ──
            _lblDisponibles = new Label { Text = "Disponibles para agregar", Location = new Point(322, 64), AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(176, 62, 96) };
            _lstDisponibles = new ListBox { Location = new Point(322, 86), Size = new Size(270, 180),
                Font = new Font("Segoe UI", 9f), BorderStyle = BorderStyle.FixedSingle, HorizontalScrollbar = true };

            _btnAgregar = Btn("Agregar ↓", new Point(322, 272), 130, Color.FromArgb(40, 110, 60));
            _btnAgregar.Click += (s, e) => Agregar();
            _btnQuitar  = Btn("Quitar ↑",  new Point(462, 272), 130, Color.FromArgb(170, 50, 50));
            _btnQuitar.Click += (s, e) => Quitar();

            _lblMiembros = new Label { Text = "Miembros", Location = new Point(322, 308), AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(176, 62, 96) };
            _lstMiembros = new ListBox { Location = new Point(322, 330), Size = new Size(270, 156),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left,
                Font = new Font("Segoe UI", 9f), BorderStyle = BorderStyle.FixedSingle, HorizontalScrollbar = true };

            // Doble-clic como atajo: en Disponibles agrega al nodo seleccionado, en Miembros quita.
            _lstDisponibles.DoubleClick += (s, e) => { if (_lstDisponibles.SelectedItem != null) Agregar(); };
            _lstMiembros.DoubleClick    += (s, e) => { if (_lstMiembros.SelectedItem    != null) Quitar();  };

            // ── Columna derecha: efectivos ──
            _lblEfectivo = new Label { Text = "Permisos efectivos (recursivo)", Location = new Point(602, 64), AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(176, 62, 96),
                Anchor = AnchorStyles.Top | AnchorStyles.Right };
            _tvEfectivo = new TreeView { Location = new Point(602, 86), Size = new Size(330, 400),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right,
                Font = new Font("Segoe UI", 9f), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.WhiteSmoke };

            // ── Acciones inferiores ──
            _btnExplorador = Btn("🌳 Ver vista completa del sistema", new Point(322, 568), 270, Color.FromArgb(176, 62, 96));
            _btnExplorador.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            _btnExplorador.Click += (s, e) => new ExploradorCompositeForm().Show(this);
            _btnCerrar = Btn("Cerrar", new Point(834, 568), 98, Color.FromArgb(210, 210, 210));
            _btnCerrar.ForeColor = Color.Black; _btnCerrar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _btnCerrar.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[]
            {
                _lblTitulo, _lblMensaje,
                _lblEstructura, _tvEstructura,
                _btnNuevaPatente, _btnNuevaFamilia, _btnNuevoRol, _btnRenombrar, _btnEliminar,
                _lblDisponibles, _lstDisponibles, _btnAgregar, _btnQuitar, _lblMiembros, _lstMiembros,
                _lblEfectivo, _tvEfectivo,
                _btnExplorador, _btnCerrar
            });

            HabilitarTransfer(false);
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
