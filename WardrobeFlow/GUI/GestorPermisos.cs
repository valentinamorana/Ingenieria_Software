using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Servicios.Multiidioma;

namespace GUI
{
    /// <summary>
    /// T04 — Gestión de Perfiles de Usuario (Patrón Composite).
    ///
    /// Rediseño con dos paneles que demuestran el Composite en dos niveles:
    ///
    ///   PANEL IZQUIERDO — Jerarquía de Roles (Composite de JerarquiaRol):
    ///     Área Comercial
    ///       └── Gerente Comercial
    ///             └── Vendedor
    ///     Área Inventario
    ///       └── Gerente de Inventario
    ///             ├── Encargado de Stock
    ///             └── Operador Logístico
    ///
    ///   PANEL DERECHO — Árbol de Permisos del rol seleccionado (Composite Familia/Patente):
    ///     📁 Inventario
    ///       ☑ Ver Prendas
    ///       ☑ Gestionar Stock
    ///     📁 Ventas
    ///       ☐ Gestionar Clientes
    ///       ...
    ///
    /// Al seleccionar un rol en el árbol izquierdo, el árbol derecho
    /// muestra y permite editar los permisos asignados a ese rol.
    /// Los permisos heredados del rol padre se muestran en color diferente (solo lectura informativa).
    /// </summary>
    public class GestorPermisos : FormBase, IIdiomaObserver
    {
        protected override Label MensajeLabel => _lblMensaje;

        private readonly BLL.Familia _familiaBLL = new BLL.Familia();

        // ── Panel izquierdo — Jerarquía de roles ──────────────────────────────
        private TreeView _treeRoles;
        private Label    _lblRolesTitulo;

        // ── Panel derecho — Permisos del rol seleccionado ─────────────────────
        private TreeView _treePermisos;
        private Label    _lblPermisosTitulo;
        private Label    _lblRolSeleccionado;

        // ── Controles comunes ─────────────────────────────────────────────────
        private Label    _lblMensaje;
        private Button   _btnGuardar;
        private Button   _btnCerrar;
        private SplitContainer _split;

        // Rol seleccionado actualmente en el árbol izquierdo
        private string _rolActual = null;

        // Permisos heredados del padre del rol actual (solo informativos)
        private HashSet<string> _permisosHeredados = new HashSet<string>();

        public GestorPermisos()
        {
            ConstruirUI();
        }

        // ── Ciclo de vida ─────────────────────────────────────────────────────

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GestorIdioma.SuscribirObservador(this);
            Traducir(GestorIdioma.IdiomaActual);
            CargarJerarquiaRoles();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            GestorIdioma.DesuscribirObservador(this);
            base.OnFormClosing(e);
        }

        // ── IIdiomaObserver ───────────────────────────────────────────────────

        public void UpdateLanguage(Idioma idioma) => Traducir(idioma);

        private void Traducir(Idioma idioma)
        {
            var t = Traductor.ObtenerTraducciones(idioma);
            string Tx(string k, string fb) => t.ContainsKey(k) ? t[k].Texto : fb;

            this.Text            = Tx("frm.gestorpermisos",     "Gestor de Perfiles — Permisos");
            _lblRolesTitulo.Text = Tx("lbl.jerarquia.roles",    "Jerarquía de Roles");
            _lblPermisosTitulo.Text = Tx("lbl.permisos.titulo", "Permisos del rol seleccionado");
            _btnGuardar.Text     = Tx("btn.permisos.guardar",   "Guardar cambios");
            _btnCerrar.Text      = Tx("btn.permisos.cerrar",    "Cerrar");

            ActualizarLabelRolSeleccionado();

            // Reconstruir árboles con nuevas traducciones
            CargarJerarquiaRoles();
        }

        private string T(string key, string fallback)
        {
            var t = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
            return t.ContainsKey(key) ? t[key].Texto : fallback;
        }

        // ── Árbol izquierdo: jerarquía de roles ───────────────────────────────

        private void CargarJerarquiaRoles()
        {
            _treeRoles.BeginUpdate();
            _treeRoles.Nodes.Clear();

            try
            {
                var raices = _familiaBLL.ObtenerArbolJerarquiaRoles();

                // Agrupar por Área usando nodos de área como raíces visuales
                var areaNodes = new Dictionary<string, TreeNode>(StringComparer.OrdinalIgnoreCase);

                foreach (var raiz in raices)
                {
                    string area = raiz.Area ?? "General";
                    if (!areaNodes.ContainsKey(area))
                    {
                        var areaNode = new TreeNode(area)
                        {
                            NodeFont  = new Font("Segoe UI", 9f, FontStyle.Bold | FontStyle.Italic),
                            ForeColor = Color.FromArgb(100, 60, 130),
                            Tag       = null  // área, no es un rol seleccionable
                        };
                        areaNodes[area] = areaNode;
                        _treeRoles.Nodes.Add(areaNode);
                    }
                    AgregarNodoRolRecursivo(raiz, areaNodes[area]);
                }

                _treeRoles.ExpandAll();
            }
            catch (Exception ex)
            {
                MostrarError($"Error al cargar jerarquía de roles: {ex.Message}");
            }

            _treeRoles.EndUpdate();
        }

        private void AgregarNodoRolRecursivo(BE.JerarquiaRol rolNodo, TreeNode padre)
        {
            string clave  = $"perm.rol.{rolNodo.Rol.ToLowerInvariant().Replace(" ", "")}";
            string nombre = T(clave, rolNodo.Rol);

            var nodo = new TreeNode(nombre)
            {
                Tag       = rolNodo.Rol,
                NodeFont  = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 80, 140)
            };

            // Marcar el rol actualmente seleccionado
            if (_rolActual != null &&
                string.Equals(rolNodo.Rol, _rolActual, StringComparison.OrdinalIgnoreCase))
            {
                nodo.ForeColor = Color.FromArgb(180, 50, 50);
            }

            padre.Nodes.Add(nodo);

            foreach (var hijo in rolNodo.Hijos)
                AgregarNodoRolRecursivo(hijo, nodo);
        }

        private void TreeRoles_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node?.Tag == null) return;  // nodo de área — no seleccionable

            string rolSeleccionado = e.Node.Tag.ToString();
            if (string.Equals(rolSeleccionado, _rolActual, StringComparison.OrdinalIgnoreCase))
                return;

            _rolActual = rolSeleccionado;
            ActualizarLabelRolSeleccionado();
            CargarPermisosRol(_rolActual);
            _btnGuardar.Enabled = true;
        }

        private void ActualizarLabelRolSeleccionado()
        {
            if (_rolActual == null)
            {
                string clave = $"perm.rol.{(_rolActual ?? "").ToLowerInvariant().Replace(" ", "")}";
                _lblRolSeleccionado.Text = T("lbl.jerarquia.selecciona", "Seleccioná un rol en el árbol izquierdo.");
            }
            else
            {
                string clave  = $"perm.rol.{_rolActual.ToLowerInvariant().Replace(" ", "")}";
                string nombre = T(clave, _rolActual);
                string fmt    = T("msg.permisos.mostrando", "Mostrando permisos del rol '{0}'.");
                _lblRolSeleccionado.Text = string.Format(fmt, nombre);
            }
        }

        // ── Árbol derecho: permisos del rol seleccionado ──────────────────────

        private void CargarPermisosRol(string rol)
        {
            _treePermisos.BeginUpdate();
            _treePermisos.Nodes.Clear();

            try
            {
                // Cargar permisos heredados del rol padre (solo visuales)
                var heredadosList = _familiaBLL.ObtenerPermisosHeredados(rol);
                _permisosHeredados = new HashSet<string>(heredadosList, StringComparer.OrdinalIgnoreCase);

                // Construir árbol Composite de permisos
                BE.Familia arbol = _familiaBLL.ObtenerArbolPorRol(rol);
                MostrarPermisosRecursivo(arbol, null);

                _treePermisos.ExpandAll();
            }
            catch (Exception ex)
            {
                MostrarError($"Error al cargar permisos: {ex.Message}");
            }

            _treePermisos.EndUpdate();
            MostrarOk(string.Format(T("msg.permisos.mostrando", "Mostrando permisos del rol '{0}'."), _rolActual));
        }

        private void MostrarPermisosRecursivo(BE.Componente componente, TreeNode nodoParent)
        {
            foreach (BE.Componente hijo in componente.Hijos)
            {
                string prefijo = hijo is BE.Familia ? "perm.grp." : "perm.pat.";
                string clave   = prefijo + hijo.Nombre.ToLowerInvariant().Replace(" ", "");
                string nombre  = T(clave, hijo.Nombre);

                var nodo = new TreeNode(nombre) { Tag = hijo };

                if (hijo is BE.Familia)
                {
                    nodo.NodeFont  = new Font("Segoe UI", 9f, FontStyle.Bold);
                    nodo.ForeColor = Color.FromArgb(40, 80, 140);
                }
                else if (hijo is BE.Patente patente)
                {
                    nodo.Checked = patente.Asignado;

                    // Permiso heredado del rol padre: mostrar en color diferente (informativo)
                    bool esHeredado = !string.IsNullOrEmpty(patente.NombreMenu) &&
                                      _permisosHeredados.Contains(patente.NombreMenu);
                    if (esHeredado && !patente.Asignado)
                    {
                        nodo.ForeColor = Color.FromArgb(100, 160, 100);
                        string heredMsg = T("lbl.permiso.heredado", " (heredado)");
                        nodo.Text += heredMsg;
                    }
                }

                if (nodoParent == null) _treePermisos.Nodes.Add(nodo);
                else                   nodoParent.Nodes.Add(nodo);

                MostrarPermisosRecursivo(hijo, nodo);
            }
        }

        // ── Guardar cambios ───────────────────────────────────────────────────

        private void GuardarCambios()
        {
            if (string.IsNullOrEmpty(_rolActual)) return;

            try
            {
                int asignados = 0, quitados = 0;
                GuardarRecursivo(_treePermisos.Nodes, _rolActual, ref asignados, ref quitados);

                CargarPermisosRol(_rolActual);
                MostrarOk(string.Format(
                    T("msg.permisos.guardados", "Cambios guardados: {0} permiso(s) asignado(s), {1} quitado(s)."),
                    asignados, quitados));
            }
            catch (Exception ex)
            {
                MostrarError($"Error al guardar: {ex.Message}");
            }
        }

        private void GuardarRecursivo(TreeNodeCollection nodos, string rol,
                                       ref int asignados, ref int quitados)
        {
            foreach (TreeNode nodo in nodos)
            {
                if (nodo.Tag is BE.Patente patente)
                {
                    bool ahora    = nodo.Checked;
                    bool anterior = patente.Asignado;

                    if (ahora && !anterior)
                    {
                        _familiaBLL.AsignarPermiso(rol, patente.Id);
                        asignados++;
                    }
                    else if (!ahora && anterior)
                    {
                        _familiaBLL.QuitarPermiso(rol, patente.Id);
                        quitados++;
                    }
                }
                GuardarRecursivo(nodo.Nodes, rol, ref asignados, ref quitados);
            }
        }

        // ── Eventos ───────────────────────────────────────────────────────────

        private void BtnGuardar_Click(object sender, EventArgs e) => GuardarCambios();
        private void BtnCerrar_Click(object sender, EventArgs e)  => this.Close();

        private void TreePermisos_BeforeCheck(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node?.Tag is BE.Familia) e.Cancel = true;
        }

        // ── Construcción de UI ────────────────────────────────────────────────

        private void ConstruirUI()
        {
            this.Text            = "Gestor de Perfiles — Permisos";
            this.Size            = new Size(900, 620);
            this.MinimumSize     = new Size(750, 520);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.BackColor       = Color.White;

            // ── Leyenda de encabezado ──────────────────────────────────────────
            var lblHeader = new Label
            {
                Text      = "Gestor de Perfiles — Permisos",
                Font      = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 80, 140),
                AutoSize  = true,
                Location  = new Point(12, 12)
            };

            _lblMensaje = new Label
            {
                AutoSize  = false,
                Size      = new Size(860, 20),
                Location  = new Point(12, 42),
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = Color.DimGray
            };

            // ── SplitContainer (izquierda = roles, derecha = permisos) ─────────
            _split = new SplitContainer
            {
                Location     = new Point(12, 68),
                Size         = new Size(860, 460),
                Anchor       = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                SplitterWidth = 6,
                SplitterDistance = 280,
                BackColor    = Color.FromArgb(220, 220, 230)
            };

            // ── Panel izquierdo ────────────────────────────────────────────────
            _lblRolesTitulo = new Label
            {
                Text      = "Jerarquía de Roles",
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 40, 100),
                Dock      = DockStyle.Top,
                Height    = 26,
                Padding   = new Padding(4, 4, 0, 0)
            };

            _treeRoles = new TreeView
            {
                Dock            = DockStyle.Fill,
                CheckBoxes      = false,
                Font            = new Font("Segoe UI", 9f),
                ShowLines       = true,
                ShowPlusMinus   = true,
                BorderStyle     = BorderStyle.None,
                BackColor       = Color.FromArgb(248, 246, 255),
                ForeColor       = Color.FromArgb(30, 30, 50),
                ItemHeight      = 22,
                Indent          = 18
            };
            _treeRoles.AfterSelect += TreeRoles_AfterSelect;

            var leftPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(248, 246, 255) };
            leftPanel.Controls.Add(_treeRoles);
            leftPanel.Controls.Add(_lblRolesTitulo);
            _split.Panel1.Controls.Add(leftPanel);

            // ── Panel derecho ──────────────────────────────────────────────────
            _lblPermisosTitulo = new Label
            {
                Text      = "Permisos del rol seleccionado",
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 80, 140),
                Dock      = DockStyle.Top,
                Height    = 26,
                Padding   = new Padding(4, 4, 0, 0)
            };

            _lblRolSeleccionado = new Label
            {
                Text      = "Seleccioná un rol en el árbol izquierdo.",
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = Color.DimGray,
                Dock      = DockStyle.Top,
                Height    = 20,
                Padding   = new Padding(4, 2, 0, 0)
            };

            _treePermisos = new TreeView
            {
                Dock            = DockStyle.Fill,
                CheckBoxes      = true,
                Font            = new Font("Segoe UI", 9f),
                ShowLines       = true,
                ShowPlusMinus   = true,
                BorderStyle     = BorderStyle.None,
                BackColor       = Color.WhiteSmoke,
                ForeColor       = Color.FromArgb(30, 30, 30),
                ItemHeight      = 22,
                Indent          = 16
            };
            _treePermisos.BeforeCheck += TreePermisos_BeforeCheck;

            // Leyenda de colores
            var lblLeyenda = new Label
            {
                Text      = "■ Azul: asignado   ■ Verde: heredado del rol padre   ■ Gris: sin asignar",
                Font      = new Font("Segoe UI", 7.5f),
                ForeColor = Color.DimGray,
                Dock      = DockStyle.Bottom,
                Height    = 18,
                Padding   = new Padding(4, 0, 0, 0)
            };

            var rightPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.WhiteSmoke };
            rightPanel.Controls.Add(_treePermisos);
            rightPanel.Controls.Add(_lblRolSeleccionado);
            rightPanel.Controls.Add(_lblPermisosTitulo);
            rightPanel.Controls.Add(lblLeyenda);
            _split.Panel2.Controls.Add(rightPanel);

            // ── Botones ────────────────────────────────────────────────────────
            _btnGuardar = new Button
            {
                Text      = "Guardar cambios",
                Location  = new Point(12, 540),
                Size      = new Size(160, 32),
                Anchor    = AnchorStyles.Bottom | AnchorStyles.Left,
                BackColor = Color.FromArgb(40, 110, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor    = Cursors.Hand,
                Enabled   = false
            };
            _btnGuardar.FlatAppearance.BorderSize = 0;
            _btnGuardar.Click += BtnGuardar_Click;

            _btnCerrar = new Button
            {
                Text      = "Cerrar",
                Location  = new Point(754, 540),
                Size      = new Size(118, 32),
                Anchor    = AnchorStyles.Bottom | AnchorStyles.Right,
                BackColor = Color.FromArgb(210, 210, 210),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9f),
                Cursor    = Cursors.Hand
            };
            _btnCerrar.FlatAppearance.BorderSize = 0;
            _btnCerrar.Click += BtnCerrar_Click;

            this.Controls.AddRange(new Control[]
            {
                lblHeader, _lblMensaje,
                _split,
                _btnGuardar, _btnCerrar
            });
        }
    }
}
