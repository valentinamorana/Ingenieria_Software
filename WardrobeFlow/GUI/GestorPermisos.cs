using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Servicios.Multiidioma;

namespace GUI
{
    /// <summary>
    /// T04 — Gestión de Perfiles de Usuario (Patrón Composite) — formulario funcional.
    ///
    /// Permite al Administrador ver y modificar los permisos asignados a cada rol.
    ///
    /// Flujo:
    ///   1. El admin selecciona un rol en el ComboBox.
    ///   2. Se carga el árbol Composite (Familias y Patentes) con checkboxes.
    ///   3. Tildar/destildar patentes asigna/quita permisos para ese rol en [RolPermiso].
    ///   4. "Ver Explorador Composite" abre la vista académica del patrón (solo lectura).
    ///
    /// Para la demostración académica del patrón Composite, ver ExploradorCompositeForm.
    /// </summary>
    public class GestorPermisos : FormBase, IIdiomaObserver
    {
        protected override Label MensajeLabel => _lblMensaje;

        private readonly BLL.Familia _familiaBLL = new BLL.Familia();

        // Nombres internos de BD (usados para consultas). El ComboBox muestra la traducción.
        private List<string> _rolesOriginales = new List<string>();

        private string RolOriginalActual =>
            _cmbRol.SelectedIndex >= 0 && _cmbRol.SelectedIndex < _rolesOriginales.Count
                ? _rolesOriginales[_cmbRol.SelectedIndex]
                : null;

        // ── Controles ─────────────────────────────────────────────────────────
        private Label    _lblTitulo;
        private Label    _lblMensaje;
        private Label    _lblRol;
        private ComboBox _cmbRol;
        private TreeView _treeView;
        private Button   _btnGuardar;
        private Button   _btnExplorador;
        private Button   _btnCerrar;

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
            CargarRoles();
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
            _lblTitulo.Text      = Tx("lbl.permisos.titulo",    "Perfiles y Permisos");
            _lblRol.Text         = Tx("lbl.permisos.rol",       "Rol:");
            _btnGuardar.Text     = Tx("btn.permisos.guardar",   "Guardar cambios");
            _btnExplorador.Text  = Tx("btn.explorador",         "🌳 Ver Explorador Composite");
            _btnCerrar.Text      = Tx("btn.permisos.cerrar",    "Cerrar");

            if (_rolesOriginales.Count > 0)
            {
                int prevIdx = _cmbRol.SelectedIndex;
                _cmbRol.SelectedIndexChanged -= CmbRol_SelectedIndexChanged;
                _cmbRol.Items.Clear();
                foreach (string rol in _rolesOriginales)
                    _cmbRol.Items.Add(Tx($"perm.rol.{rol.ToLowerInvariant().Replace(" ", "")}", rol));
                if (prevIdx >= 0 && prevIdx < _cmbRol.Items.Count)
                    _cmbRol.SelectedIndex = prevIdx;
                _cmbRol.SelectedIndexChanged += CmbRol_SelectedIndexChanged;
                if (_cmbRol.SelectedIndex >= 0) MostrarPermisos();
            }
        }

        private string T(string key, string fallback)
        {
            var t = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
            return t.ContainsKey(key) ? t[key].Texto : fallback;
        }

        // ── Carga de datos ────────────────────────────────────────────────────

        private void CargarRoles()
        {
            try
            {
                _rolesOriginales = _familiaBLL.ObtenerRoles();
                _cmbRol.Items.Clear();
                foreach (string rol in _rolesOriginales)
                    _cmbRol.Items.Add(T($"perm.rol.{rol.ToLowerInvariant().Replace(" ", "")}", rol));
                if (_cmbRol.Items.Count > 0) _cmbRol.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MostrarError($"Error al cargar roles: {ex.Message}");
            }
        }

        // ── Construcción del TreeView (Composite) ─────────────────────────────

        private void MostrarPermisos()
        {
            string rol = RolOriginalActual;
            if (string.IsNullOrEmpty(rol)) return;

            try
            {
                _treeView.BeginUpdate();
                _treeView.Nodes.Clear();

                BE.Familia arbol = _familiaBLL.ObtenerArbolPorRol(rol);
                MostrarPermisosRecursivo(arbol, null);

                _treeView.ExpandAll();
                _treeView.EndUpdate();

                string rolTrad = _cmbRol.SelectedItem?.ToString() ?? rol;
                MostrarOk(string.Format(T("msg.permisos.mostrando", "Mostrando permisos del rol '{0}'."), rolTrad));
                _btnGuardar.Enabled = true;
            }
            catch (Exception ex)
            {
                MostrarError($"Error al cargar árbol de permisos: {ex.Message}");
            }
        }

        // Función recursiva que construye el TreeView a partir del árbol Composite.
        private void MostrarPermisosRecursivo(BE.Componente componente, TreeNode nodoParent)
        {
            foreach (BE.Componente hijo in componente.Hijos)
            {
                string prefijo = hijo is BE.Familia ? "perm.grp." : "perm.pat.";
                string clave   = prefijo + hijo.Nombre.ToLowerInvariant().Replace(" ", "");
                var nodo = new TreeNode(T(clave, hijo.Nombre)) { Tag = hijo };

                if (hijo is BE.Familia)
                {
                    nodo.NodeFont  = new Font("Segoe UI", 9f, FontStyle.Bold);
                    nodo.ForeColor = Color.FromArgb(40, 80, 140);
                }
                else if (hijo is BE.Patente patente)
                {
                    nodo.Checked = patente.Asignado;
                }

                if (nodoParent == null) _treeView.Nodes.Add(nodo);
                else                   nodoParent.Nodes.Add(nodo);

                MostrarPermisosRecursivo(hijo, nodo);
            }
        }

        // ── Guardar cambios ───────────────────────────────────────────────────

        private void GuardarCambios()
        {
            string rol = RolOriginalActual;
            if (string.IsNullOrEmpty(rol)) return;

            try
            {
                int asignados = 0, quitados = 0;
                GuardarRecursivo(_treeView.Nodes, rol, ref asignados, ref quitados);
                MostrarPermisos();
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
                    if (nodo.Checked && !patente.Asignado)      { _familiaBLL.AsignarPermiso(rol, patente.Id); asignados++; }
                    else if (!nodo.Checked && patente.Asignado) { _familiaBLL.QuitarPermiso(rol, patente.Id);  quitados++; }
                }
                GuardarRecursivo(nodo.Nodes, rol, ref asignados, ref quitados);
            }
        }

        // ── Eventos ───────────────────────────────────────────────────────────

        private void CmbRol_SelectedIndexChanged(object sender, EventArgs e) => MostrarPermisos();
        private void BtnGuardar_Click(object sender, EventArgs e)             => GuardarCambios();
        private void BtnCerrar_Click(object sender, EventArgs e)              => this.Close();

        private void BtnExplorador_Click(object sender, EventArgs e)
        {
            // Abrir el Explorador Composite como ventana independiente (no MDI)
            var explorador = new ExploradorCompositeForm();
            explorador.Show(this);
        }

        private void TreeView_BeforeCheck(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node?.Tag is BE.Familia) e.Cancel = true;
        }

        // ── Construcción de UI ────────────────────────────────────────────────

        private void ConstruirUI()
        {
            this.Text            = "Gestor de Perfiles — Permisos";
            this.Size            = new Size(560, 620);
            this.MinimumSize     = new Size(460, 500);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.BackColor       = Color.White;

            _lblTitulo = new Label
            {
                Text      = "Perfiles y Permisos",
                Font      = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 80, 140),
                AutoSize  = true,
                Location  = new Point(12, 12)
            };

            _lblMensaje = new Label
            {
                AutoSize  = false,
                Size      = new Size(520, 20),
                Location  = new Point(12, 42),
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = Color.DimGray
            };

            _lblRol = new Label
            {
                Text     = "Rol:",
                Location = new Point(12, 76),
                AutoSize = true,
                Font     = new Font("Segoe UI", 9f)
            };

            _cmbRol = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location      = new Point(50, 72),
                Size          = new Size(260, 24),
                Font          = new Font("Segoe UI", 9f)
            };
            _cmbRol.SelectedIndexChanged += CmbRol_SelectedIndexChanged;

            _treeView = new TreeView
            {
                Location        = new Point(12, 108),
                Size            = new Size(520, 400),
                Anchor          = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                CheckBoxes      = true,
                Font            = new Font("Segoe UI", 9f),
                ShowLines       = true,
                ShowPlusMinus   = true,
                BorderStyle     = BorderStyle.FixedSingle,
                BackColor       = Color.WhiteSmoke
            };
            _treeView.BeforeCheck += TreeView_BeforeCheck;

            _btnGuardar = new Button
            {
                Text      = "Guardar cambios",
                Location  = new Point(12, 524),
                Size      = new Size(150, 32),
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

            _btnExplorador = new Button
            {
                Text      = "🌳 Ver Explorador Composite",
                Location  = new Point(170, 524),
                Size      = new Size(190, 32),
                Anchor    = AnchorStyles.Bottom | AnchorStyles.Left,
                BackColor = Color.FromArgb(64, 0, 64),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9f),
                Cursor    = Cursors.Hand
            };
            _btnExplorador.FlatAppearance.BorderSize = 0;
            _btnExplorador.Click += BtnExplorador_Click;

            _btnCerrar = new Button
            {
                Text      = "Cerrar",
                Location  = new Point(434, 524),
                Size      = new Size(98, 32),
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
                _lblTitulo, _lblMensaje,
                _lblRol, _cmbRol,
                _treeView,
                _btnGuardar, _btnExplorador, _btnCerrar
            });
        }
    }
}
