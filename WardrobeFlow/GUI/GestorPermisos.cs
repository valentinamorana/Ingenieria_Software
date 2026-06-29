using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
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
        private Panel    _panelHeader, _panelFooter;
        private ToolTip  _tip;
        private Label    _lblTitulo, _lblSubtitulo, _lblMensaje, _lblEstructura, _lblDisponibles, _lblMiembros, _lblEfectivo, _lblAyuda;
        private TreeView _tvEstructura, _tvEfectivo;
        private ListBox  _lstDisponibles, _lstMiembros;
        private Button   _btnAgregar, _btnQuitar, _btnNuevoRol,
                         _btnRenombrar, _btnEliminar, _btnActualizar, _btnExplorador, _btnCerrar;

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
            this.Text             = T("frm.gestorpermisos",      "Gestor de Perfiles — Roles y Permisos (Composite)");
            _lblTitulo.Text       = T("lbl.permisos.titulo",     "Perfiles y Permisos");
            _lblSubtitulo.Text    = T("lbl.permisos.subtitulo",  "Gestión de roles (los permisos son un catálogo fijo)");
            _lblEstructura.Text   = T("lbl.permisos.estructura", "Estructura del sistema (roles y permisos)");
            _lblDisponibles.Text  = T("lbl.permisos.disponibles","Permisos disponibles para asignar");
            _lblEfectivo.Text     = T("lbl.permisos.efectivos",  "Permisos efectivos (recursivo)");
            _btnAgregar.Text      = T("btn.permisos.agregar",    "Asignar ↓");
            _btnQuitar.Text       = T("btn.permisos.quitar",     "Quitar ↑");
            _btnNuevoRol.Text     = T("btn.permisos.crearrol",   "➕ Rol");
            _btnRenombrar.Text    = T("btn.permisos.modificar",  "✏ Renombrar rol");
            _btnEliminar.Text     = T("btn.permisos.eliminar",   "🗑 Eliminar rol");
            _btnActualizar.Text   = T("btn.permisos.actualizar", "↻ Actualizar");
            _btnExplorador.Text   = T("btn.explorador",          "🌳 Ver vista completa del sistema");
            _btnCerrar.Text       = T("btn.permisos.cerrar",     "Cerrar");

            // Ayuda contextual (tooltips): general en "❔ Ayuda" y específica en el botón ➕ Rol.
            if (_lblAyuda != null) _lblAyuda.Text = T("lbl.permisos.ayuda", "❔ Ayuda");
            if (_tip != null)
            {
                _tip.SetToolTip(_lblAyuda, T("help.permisos.general", AyudaPorDefecto()));
                _tip.SetToolTip(_btnNuevoRol, T("help.permisos.rol",
                    "Rol = perfil que se asigna a un usuario. Puede contener permisos (patentes) y otros roles (rol-en-rol)."));
            }

            ActualizarLabelMiembros();
        }

        // Texto de ayuda por defecto (ES) — fallback si no hay traducción en BD.
        private static string AyudaPorDefecto()
        {
            return "🔑 PERMISO (PATENTE) — permiso atómico: una acción o pantalla concreta (ej. \"Ver Prendas\").\n" +
                   "    Es un catálogo FIJO del sistema: no se crean ni se eliminan, solo se ASIGNAN a roles.\n\n" +
                   "👥 ROL — es el perfil que SÍ se asigna a un usuario. Se puede crear, renombrar y eliminar.\n" +
                   "    Puede contener permisos y OTROS ROLES (rol-dentro-de-rol = patrón Composite).\n\n" +
                   "El usuario tiene un rol; el sistema recorre el árbol de forma recursiva\n" +
                   "y junta todos los permisos que cuelgan de él = sus permisos efectivos.";
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

        // Tras eliminar las Familias, el único contenedor editable es el ROL: se le asignan
        // permisos (patentes) y, opcionalmente, otros roles (rol-en-rol = Composite).
        private static bool EsContenedor(BE.Componente c) => c is BE.Rol;

        private void ActualizarPanelSeleccion()
        {
            _lstDisponibles.DataSource = null;
            _lstMiembros.DataSource    = null;
            _tvEfectivo.Nodes.Clear();
            ActualizarLabelMiembros();

            if (_seleccionado == null) { HabilitarTransfer(false); return; }

            // Panel derecho: ESTRUCTURA efectiva (recursiva) del nodo seleccionado, distinguiendo
            // permisos DIRECTOS (hijos inmediatos) de los HEREDADOS (alcanzados a través de un
            // rol/familia anidado — patrón Composite). Idea de visualización tomada del proyecto Agus.
            var raizEf = new TreeNode(Etiqueta(_seleccionado))
            { NodeFont = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(176, 62, 96) };
            foreach (var hijo in _seleccionado.Hijos)
                AgregarNodoEfectivo(raizEf, hijo, 1, new HashSet<int>());
            _tvEfectivo.Nodes.Add(raizEf);
            raizEf.ExpandAll();

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

        // Dibuja el subárbol del nodo en el panel "Permisos efectivos", marcando cada permiso (Patente)
        // como (directo) si cuelga directamente del nodo seleccionado, o (heredado) si llega a través de
        // un rol/familia anidado. Protegido contra ciclos por 'vis' y un tope de profundidad.
        private void AgregarNodoEfectivo(TreeNode padreUI, BE.Componente comp, int nivel, HashSet<int> vis)
        {
            if (comp == null || nivel > 50) return;

            if (comp is BE.Patente pat)
            {
                string menu  = string.IsNullOrEmpty(pat.NombreMenu) ? "" : "   (" + pat.NombreMenu + ")";
                string marca = nivel == 1 ? T("perm.tag.directo", "directo") : T("perm.tag.heredado", "heredado");
                var nodoPat  = new TreeNode("🔑 " + pat.Nombre + menu + "   · " + marca);
                if (nivel > 1) nodoPat.ForeColor = Color.FromArgb(120, 120, 130);   // heredado: atenuado
                padreUI.Nodes.Add(nodoPat);
                return;
            }

            // Familia o Rol: nodo contenedor + recursión sobre sus hijos.
            var nodoCont = new TreeNode(Etiqueta(comp)) { ForeColor = Color.FromArgb(176, 62, 96) };
            padreUI.Nodes.Add(nodoCont);
            if (comp.Id != 0 && !vis.Add(comp.Id)) return;   // corta ciclos / nodos compartidos
            foreach (var h in comp.Hijos)
                AgregarNodoEfectivo(nodoCont, h, nivel + 1, vis);
        }

        private void HabilitarTransfer(bool on) { _btnAgregar.Enabled = on; _btnQuitar.Enabled = on; }

        private void ActualizarLabelMiembros()
        {
            if (_lblMiembros == null) return;
            _lblMiembros.Text = _seleccionado != null && EsContenedor(_seleccionado)
                ? string.Format(T("lbl.permisos.miembros", "Permisos del rol: {0}"), _seleccionado.Nombre)
                : T("lbl.permisos.miembros.vacio", "Permisos (seleccioná un Rol)");
        }

        // ── Transferencia (anidar / desanidar) ─────────────────────────────────────
        private void Agregar()
        {
            if (!EsContenedor(_seleccionado))
            { MostrarError(T("perm.msg.selpadre", "Seleccioná un Rol para asignarle permisos.")); return; }
            if (!(_lstDisponibles.SelectedItem is Item it)) return;
            try
            {
                _familiaBLL.AgregarComponente(_seleccionado.Id, it.Comp.Id);
                GUI.Menu.RefrescarSeguridadAbierta();   // re-aplica seguridad en vivo
                MostrarOk(string.Format(T("perm.ok.agregado", "'{0}' agregado a '{1}'."), it.Comp.Nombre, _seleccionado.Nombre));
                CargarArbol();
            }
            catch (Exception ex) { MostrarError(ex); }
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
            catch (Exception ex) { MostrarError(ex); }
        }

        // ── CRUD de ROLES (los permisos son un catálogo fijo: no se crean/eliminan) ──
        // Alta de rol: nombre + selección de permisos (catálogo FIJO) a asignar en el momento
        // de crearlo. Los permisos del rol se pueden seguir ajustando luego con Asignar/Quitar.
        private void NuevoRol()
        {
            var patentes = _familiaBLL.ObtenerPatentesDisponibles();
            patentes.Sort((a, b) => string.Compare(a.Nombre, b.Nombre, StringComparison.OrdinalIgnoreCase));

            if (!PedirRolConPermisos(patentes, out string nombre, out List<int> idsSel)) return;
            try
            {
                _familiaBLL.CrearRol(nombre.Trim());
                if (idsSel.Count > 0)
                    _familiaBLL.GuardarAsignacionRol(nombre.Trim(), idsSel);   // asigna los permisos elegidos
                GUI.Menu.RefrescarSeguridadAbierta();
                MostrarOk(string.Format(T("perm.ok.rolcreado", "Rol '{0}' creado con {1} permiso(s)."), nombre.Trim(), idsSel.Count));
                CargarArbol();
            }
            catch (Exception ex) { MostrarError(ex); }
        }

        // Diálogo de alta de rol: nombre + checklist de permisos del catálogo (no se crean permisos).
        private bool PedirRolConPermisos(List<BE.Patente> patentes, out string nombre, out List<int> ids)
        {
            nombre = null; ids = new List<int>();
            var tr = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
            string txtOk = tr.ContainsKey("btn.aceptar")  ? tr["btn.aceptar"].Texto  : "Aceptar";
            string txtCa = tr.ContainsKey("btn.cancelar") ? tr["btn.cancelar"].Texto : "Cancelar";
            using (var f = new Form())
            {
                f.Text = T("perm.dlg.rol.t", "Nuevo rol");
                f.Size = new Size(430, 470); f.StartPosition = FormStartPosition.CenterParent;
                f.FormBorderStyle = FormBorderStyle.FixedDialog; f.MinimizeBox = false; f.MaximizeBox = false;
                f.Font = new Font("Segoe UI", 9f);

                var lblN = new Label { Text = T("perm.dlg.rol.p", "Nombre del nuevo rol:"),
                    Location = new Point(14, 14), AutoSize = true, ForeColor = RosaOscuro, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
                var txt  = new TextBox { Location = new Point(16, 38), Size = new Size(390, 24) };
                var lblP = new Label { Text = T("perm.dlg.rol.permisos", "Permisos a asignar (catálogo fijo del sistema):"),
                    Location = new Point(14, 72), AutoSize = true, ForeColor = RosaOscuro, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
                var clb  = new CheckedListBox { Location = new Point(16, 96), Size = new Size(390, 290),
                    CheckOnClick = true, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 9f) };
                foreach (var p in patentes) clb.Items.Add(p);   // BE.Patente.ToString() == Nombre

                var ok = new Button { Text = txtOk, DialogResult = DialogResult.OK, Location = new Point(232, 398), Size = new Size(82, 30),
                    BackColor = RosaPrimario, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                ok.FlatAppearance.BorderSize = 0;
                var ca = new Button { Text = txtCa, DialogResult = DialogResult.Cancel, Location = new Point(322, 398), Size = new Size(84, 30) };
                f.AcceptButton = ok; f.CancelButton = ca;
                f.Controls.AddRange(new Control[] { lblN, txt, lblP, clb, ok, ca });

                if (f.ShowDialog(this) != DialogResult.OK) return false;
                nombre = txt.Text;
                if (string.IsNullOrWhiteSpace(nombre))
                { MostrarError(T("perm.msg.nombrevacio", "El nombre del rol no puede estar vacío.")); return false; }
                foreach (var o in clb.CheckedItems)
                    if (o is BE.Patente pat) ids.Add(pat.Id);
                return true;
            }
        }

        private void Renombrar()
        {
            // Solo ROLES son editables; los permisos (patentes) son un catálogo fijo.
            if (!(_seleccionado is BE.Rol))
            { MostrarError(T("perm.msg.selmodrol", "Seleccioná un ROL del árbol para renombrar (los permisos no se editan).")); return; }

            string nombre = Pedir(T("perm.dlg.mod.t", "Renombrar rol"), T("perm.dlg.mod.p", "Nuevo nombre:"));
            if (string.IsNullOrWhiteSpace(nombre)) return;

            try
            {
                _familiaBLL.RenombrarComponente(_seleccionado.Id, nombre.Trim(), nombre.Trim());
                GUI.Menu.RefrescarSeguridadAbierta();
                MostrarOk(string.Format(T("perm.ok.modificado", "Rol actualizado a '{0}'."), nombre.Trim()));
                CargarArbol();
            }
            catch (Exception ex) { MostrarError(ex); }
        }

        private void Eliminar()
        {
            // Solo ROLES se pueden eliminar; los permisos son un catálogo fijo del sistema.
            if (!(_seleccionado is BE.Rol))
            { MostrarError(T("perm.msg.selelirol", "Seleccioná un ROL del árbol para eliminar (los permisos no se eliminan).")); return; }

            if (MessageBox.Show(
                    string.Format(T("perm.conf.elirol", "¿Eliminar el rol '{0}'? No se permite si tiene usuarios asignados."), _seleccionado.Nombre),
                    T("perm.conf.titulo", "Confirmar"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            try
            {
                _familiaBLL.EliminarRol(_seleccionado.Nombre);   // valida que no tenga usuarios asignados

                string nombre = _seleccionado.Nombre;
                _seleccionado = null;
                GUI.Menu.RefrescarSeguridadAbierta();
                MostrarOk(string.Format(T("perm.ok.eliminado", "Rol '{0}' eliminado."), nombre));
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

        // ── Paleta de marca ────────────────────────────────────────────────────────
        private static readonly Color RosaPrimario = Color.FromArgb(210, 100, 135);  // #D26487
        private static readonly Color RosaOscuro   = Color.FromArgb(176, 62, 96);    // #B03E60
        private static readonly Color Peligro      = Color.FromArgb(200, 60, 60);    // #C83C3C
        private static readonly Color PanelClaro   = Color.FromArgb(245, 245, 250);  // #F5F5FA
        private static readonly Color Neutro       = Color.FromArgb(236, 236, 242);

        // ── Construcción de UI ─────────────────────────────────────────────────────
        private void ConstruirUI()
        {
            this.Text          = "Gestor de Perfiles — Permisos (Composite)";
            this.Size          = new Size(980, 700);
            this.MinimumSize   = new Size(920, 620);
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
            _lblSubtitulo = new Label { Text = "Gestión de roles, familias y patentes", Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = Color.FromArgb(255, 224, 236), BackColor = Color.Transparent, AutoSize = true, Location = new Point(20, 34) };
            _panelHeader.Controls.Add(_lblTitulo);
            _panelHeader.Controls.Add(_lblSubtitulo);

            // Tooltip de ayuda: explica patente/familia/rol. Pasá el mouse por "❔ Ayuda"
            // (o por los botones ➕). Clic en "❔ Ayuda" abre la explicación completa.
            _tip = new ToolTip { AutoPopDelay = 30000, InitialDelay = 250, ReshowDelay = 100, ShowAlways = true, IsBalloon = true };
            _lblAyuda = new Label { Dock = DockStyle.Right, Width = 92, Text = "❔ Ayuda",
                ForeColor = Color.White, BackColor = Color.Transparent, TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold), Cursor = Cursors.Help };
            _lblAyuda.Click += (s, e) => MessageBox.Show(this,
                T("help.permisos.general", AyudaPorDefecto()),
                T("help.permisos.titulo", "¿Patente, Familia o Rol?"),
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            _panelHeader.Controls.Add(_lblAyuda);
            _lblAyuda.BringToFront();

            // ── Footer: mensaje (izq) + acciones (der) ───────────────────────────
            _panelFooter = new Panel { Dock = DockStyle.Bottom, Height = 52, BackColor = PanelClaro };

            // Las acciones van en un FlowLayoutPanel dock-derecha (se acomodan solas, sin
            // depender de posiciones fijas que se rompen al cambiar el ancho del panel).
            var flAcciones = new FlowLayoutPanel { Dock = DockStyle.Right, FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false, AutoSize = false, Width = 500, Padding = new Padding(0, 11, 10, 0), BackColor = PanelClaro };
            _btnCerrar = MakeBtn("Cerrar", Point.Empty, 96, EstiloBtn.Neutral);
            _btnCerrar.Margin = new Padding(8, 0, 0, 0); _btnCerrar.Click += (s, e) => this.Close();
            _btnExplorador = MakeBtn("🌳 Ver vista completa del sistema", Point.Empty, 240, EstiloBtn.Light);
            _btnExplorador.Margin = new Padding(8, 0, 0, 0); _btnExplorador.Click += (s, e) => new ExploradorCompositeForm().Show(this);
            _btnActualizar = MakeBtn("↻ Actualizar", Point.Empty, 120, EstiloBtn.Light);
            _btnActualizar.Margin = new Padding(8, 0, 0, 0); _btnActualizar.Click += (s, e) => CargarArbol();
            // En RightToLeft, el primero agregado queda más a la derecha → Cerrar, Vista, Actualizar.
            flAcciones.Controls.Add(_btnCerrar);
            flAcciones.Controls.Add(_btnExplorador);
            flAcciones.Controls.Add(_btnActualizar);

            _lblMensaje = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(16, 0, 0, 0), Font = new Font("Segoe UI", 8.5f), ForeColor = Color.DimGray };

            _panelFooter.Controls.Add(_lblMensaje);
            _panelFooter.Controls.Add(flAcciones);

            // ── Columna izquierda: estructura ──────────────────────────────────────
            _lblEstructura = SeccionLbl("Estructura del sistema", new Point(16, 70));
            _tvEstructura = new TreeView { Location = new Point(16, 92), Size = new Size(304, 420),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left,
                Font = new Font("Segoe UI", 9f), BorderStyle = BorderStyle.FixedSingle, HideSelection = false };
            _tvEstructura.AfterSelect += Tv_AfterSelect;

            _btnNuevoRol     = MakeBtn("➕ Rol",     new Point(16, 520), 304, EstiloBtn.Light);
            _btnNuevoRol.Click += (s, e) => NuevoRol();
            _btnRenombrar    = MakeBtn("✏ Renombrar rol", new Point(16, 554), 148, EstiloBtn.Light);
            _btnRenombrar.Click += (s, e) => Renombrar();
            _btnEliminar     = MakeBtn("🗑 Eliminar rol",  new Point(172, 554), 148, EstiloBtn.Danger);
            _btnEliminar.Click += (s, e) => Eliminar();
            foreach (var b in new[] { _btnNuevoRol, _btnRenombrar, _btnEliminar })
                b.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            // ── Columna central: dos listas (Disponibles / Miembros) ───────────────
            _lblDisponibles = SeccionLbl("Disponibles para agregar", new Point(336, 70));
            _lstDisponibles = new ListBox { Location = new Point(336, 92), Size = new Size(280, 180),
                Font = new Font("Segoe UI", 9f), BorderStyle = BorderStyle.FixedSingle, HorizontalScrollbar = true };
            _lstDisponibles.DoubleClick += (s, e) => { if (_lstDisponibles.SelectedItem != null) Agregar(); };

            _btnAgregar = MakeBtn("Agregar  ↓", new Point(336, 280), 135, EstiloBtn.Primary);
            _btnAgregar.Click += (s, e) => Agregar();
            _btnQuitar  = MakeBtn("Quitar  ↑",  new Point(481, 280), 135, EstiloBtn.Light);
            _btnQuitar.Click += (s, e) => Quitar();

            _lblMiembros = SeccionLbl("Miembros", new Point(336, 318));
            _lstMiembros = new ListBox { Location = new Point(336, 340), Size = new Size(280, 172),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left,
                Font = new Font("Segoe UI", 9f), BorderStyle = BorderStyle.FixedSingle, HorizontalScrollbar = true };
            _lstMiembros.DoubleClick += (s, e) => { if (_lstMiembros.SelectedItem != null) Quitar(); };

            // ── Columna derecha: efectivos ─────────────────────────────────────────
            _lblEfectivo = SeccionLbl("Permisos efectivos (recursivo)", new Point(632, 70));
            _lblEfectivo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _tvEfectivo = new TreeView { Location = new Point(632, 92), Size = new Size(316, 420),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right,
                Font = new Font("Segoe UI", 9f), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.FromArgb(250, 246, 250) };

            this.Controls.AddRange(new Control[]
            {
                _lblEstructura, _tvEstructura,
                _btnNuevoRol, _btnRenombrar, _btnEliminar,
                _lblDisponibles, _lstDisponibles, _btnAgregar, _btnQuitar, _lblMiembros, _lstMiembros,
                _lblEfectivo, _tvEfectivo
            });
            this.Controls.Add(_panelFooter);  _panelFooter.BringToFront();
            this.Controls.Add(_panelHeader);  _panelHeader.BringToFront();

            HabilitarTransfer(false);
        }

        // Etiqueta de sección con el estilo de marca.
        private static Label SeccionLbl(string text, Point loc)
            => new Label { Text = text, Location = loc, AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = RosaOscuro };

        private enum EstiloBtn { Primary, Danger, Light, Neutral }

        // Fábrica de botones con la paleta unificada: un solo primario (rosa), un solo
        // peligro (rojo), y secundarios "light" (contorno rosa) / neutro (gris).
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
