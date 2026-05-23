using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Servicios.Multiidioma;

namespace GUI
{
    /// <summary>
    /// Patrón Composite — T04 Gestión de Perfiles de Usuario.
    ///
    /// Permite al Administrador visualizar y modificar los permisos asignados
    /// a cada rol del sistema. El árbol se construye recursivamente usando las
    /// clases BE.Familia (nodo compuesto) y BE.Patente (hoja).
    ///
    /// Flujo:
    ///   1. Al abrir, se cargan los roles disponibles en el ComboBox.
    ///   2. Al seleccionar un rol, MostrarPermisos() reconstruye el TreeView
    ///      llamando a MostrarPermisosRecursivo() — función recursiva requerida.
    ///   3. El admin tilda/destilda permisos y presiona Guardar.
    ///   4. GuardarRecursivo() recorre el árbol del TreeView y aplica los cambios.
    ///
    /// Accesible desde Administrar → Perfiles (solo para Administrador).
    /// Construido completamente en código — no usa el Designer.
    /// </summary>
    public class GestorPermisos : FormBase, IIdiomaObserver
    {
        protected override Label MensajeLabel => _lblMensaje;

        // ── Dependencias ──────────────────────────────────────────────────────
        private readonly BLL.Familia _familiaBLL = new BLL.Familia();

        // ── Controles ─────────────────────────────────────────────────────────
        private Label        _lblTitulo;
        private Label        _lblMensaje;
        private Label        _lblRol;
        private ComboBox     _cmbRol;
        private TreeView     _treeView;
        private Button       _btnGuardar;
        private Button       _btnCerrar;

        // ── Constructor ───────────────────────────────────────────────────────

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

            this.Text        = Tx("frm.gestorpermisos",    "Gestor de Perfiles — Permisos");
            _lblTitulo.Text  = Tx("lbl.permisos.titulo",   "Perfiles y Permisos");
            _lblRol.Text     = Tx("lbl.permisos.rol",      "Rol:");
            _btnGuardar.Text = Tx("btn.permisos.guardar",  "Guardar cambios");
            _btnCerrar.Text  = Tx("btn.permisos.cerrar",   "Cerrar");
        }

        // Helper para traducir claves dinámicas en métodos que no reciben Idioma.
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
                List<string> roles = _familiaBLL.ObtenerRoles();
                _cmbRol.Items.Clear();
                foreach (string rol in roles)
                    _cmbRol.Items.Add(rol);

                if (_cmbRol.Items.Count > 0)
                    _cmbRol.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MostrarError($"Error al cargar los roles: {ex.Message}");
            }
        }

        // ── Construcción del TreeView (Composite) ─────────────────────────────

        private void MostrarPermisos()
        {
            string rol = _cmbRol.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(rol)) return;

            try
            {
                _treeView.BeginUpdate();
                _treeView.Nodes.Clear();

                // Obtener el árbol Composite desde la BLL
                BE.Familia arbol = _familiaBLL.ObtenerArbolPorRol(rol);

                // Construir el TreeView recursivamente (patrón requerido por T04)
                MostrarPermisosRecursivo(arbol, null);

                _treeView.ExpandAll();
                _treeView.EndUpdate();

                MostrarOk(string.Format(T("msg.permisos.mostrando", "Mostrando permisos del rol '{0}'."), rol));
                _btnGuardar.Enabled = true;
            }
            catch (Exception ex)
            {
                MostrarError($"Error al cargar el árbol de permisos: {ex.Message}");
            }
        }

        // Función recursiva que construye el TreeView a partir del árbol Composite.
        // Procesa tanto nodos Familia (grupos) como Patente (hojas con checkbox).
        private void MostrarPermisosRecursivo(BE.Componente componente, TreeNode nodoParent)
        {
            foreach (BE.Componente hijo in componente.Hijos)
            {
                var nodo = new TreeNode(hijo.Nombre)
                {
                    Tag = hijo
                };

                if (hijo is BE.Familia)
                {
                    // Nodo de grupo — fondo diferenciado, sin checkbox funcional
                    nodo.NodeFont = new Font("Segoe UI", 9f, FontStyle.Bold);
                    nodo.ForeColor = Color.FromArgb(40, 80, 140);
                }
                else if (hijo is BE.Patente patente)
                {
                    // Hoja — checkbox indica si el permiso está asignado al rol
                    nodo.Checked = patente.Asignado;
                }

                if (nodoParent == null)
                    _treeView.Nodes.Add(nodo);
                else
                    nodoParent.Nodes.Add(nodo);

                // Recursión — procesar hijos del nodo actual
                MostrarPermisosRecursivo(hijo, nodo);
            }
        }

        // ── Guardar cambios ───────────────────────────────────────────────────

        private void GuardarCambios()
        {
            string rol = _cmbRol.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(rol)) return;

            try
            {
                int asignados = 0;
                int quitados  = 0;

                // Recorrer el árbol del TreeView recursivamente para aplicar cambios
                GuardarRecursivo(_treeView.Nodes, rol, ref asignados, ref quitados);

                // Recargar el árbol para reflejar el estado actualizado
                MostrarPermisos();
                MostrarOk(string.Format(T("msg.permisos.guardados", "Cambios guardados: {0} permiso(s) asignado(s), {1} quitado(s)."), asignados, quitados));
            }
            catch (Exception ex)
            {
                MostrarError($"Error al guardar los cambios: {ex.Message}");
            }
        }

        // Recorre recursivamente los nodos del TreeView y aplica asignaciones/bajas.
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

                // Recursión — procesar hijos del nodo actual
                GuardarRecursivo(nodo.Nodes, rol, ref asignados, ref quitados);
            }
        }

        // ── Eventos ───────────────────────────────────────────────────────────

        private void CmbRol_SelectedIndexChanged(object sender, EventArgs e) => MostrarPermisos();
        private void BtnGuardar_Click(object sender, EventArgs e)             => GuardarCambios();
        private void BtnCerrar_Click(object sender, EventArgs e)              => this.Close();

        // Evitar que checkear un Familia propague el check a todos sus hijos de forma recursiva involuntaria
        private void TreeView_BeforeCheck(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node?.Tag is BE.Familia)
                e.Cancel = true;  // Los nodos de grupo no son checkables
        }

        // ── Construcción de UI ────────────────────────────────────────────────

        private void ConstruirUI()
        {
            this.Text            = "Gestor de Perfiles — Permisos";
            this.Size            = new Size(540, 560);
            this.MinimumSize     = new Size(460, 460);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.BackColor       = Color.White;

            // ── Título ─────────────────────────────────────────────────────────
            _lblTitulo = new Label
            {
                Text        = "Perfiles y Permisos",
                Font        = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor   = Color.FromArgb(40, 80, 140),
                AutoSize    = true,
                Location    = new Point(12, 12),
                UseMnemonic = false
            };

            _lblMensaje = new Label
            {
                AutoSize  = false,
                Size      = new Size(500, 20),
                Location  = new Point(12, 42),
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = Color.DimGray
            };

            // ── Selector de rol ────────────────────────────────────────────────
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
                Size          = new Size(240, 24),
                Font          = new Font("Segoe UI", 9f)
            };

            // ── TreeView de permisos ───────────────────────────────────────────
            _treeView = new TreeView
            {
                Location        = new Point(12, 108),
                Size            = new Size(500, 360),
                Anchor          = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                CheckBoxes      = true,
                Font            = new Font("Segoe UI", 9f),
                ShowLines       = true,
                ShowPlusMinus   = true,
                BorderStyle     = BorderStyle.FixedSingle,
                BackColor       = Color.WhiteSmoke,
                ForeColor       = Color.FromArgb(30, 30, 30)
            };

            // ── Botones ────────────────────────────────────────────────────────
            _btnGuardar = new Button
            {
                Text      = "Guardar cambios",
                Location  = new Point(12, 478),
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

            _btnCerrar = new Button
            {
                Text      = "Cerrar",
                Location  = new Point(394, 478),
                Size      = new Size(118, 32),
                Anchor    = AnchorStyles.Bottom | AnchorStyles.Right,
                BackColor = Color.FromArgb(210, 210, 210),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9f),
                Cursor    = Cursors.Hand
            };
            _btnCerrar.FlatAppearance.BorderSize = 0;

            // ── Suscribir eventos ──────────────────────────────────────────────
            _cmbRol.SelectedIndexChanged += CmbRol_SelectedIndexChanged;
            _btnGuardar.Click            += BtnGuardar_Click;
            _btnCerrar.Click             += BtnCerrar_Click;
            _treeView.BeforeCheck        += TreeView_BeforeCheck;

            // ── Agregar controles ──────────────────────────────────────────────
            this.Controls.AddRange(new Control[]
            {
                _lblTitulo, _lblMensaje,
                _lblRol, _cmbRol,
                _treeView,
                _btnGuardar, _btnCerrar
            });
        }
    }
}
