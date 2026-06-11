using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Servicios.Multiidioma;

namespace GUI
{
    /// <summary>
    /// T04 — Explorador del Patrón Composite (demostración académica, solo lectura).
    ///
    /// Muestra la estructura organizacional completa de WardrobeFlow como árbol Composite:
    ///
    ///   📁 WardrobeFlow
    ///     📁 Administración
    ///       📁 Administrador
    ///         🔑 Gestionar Usuarios
    ///         🔑 Ver Auditoría
    ///         ...
    ///       📁 Auditor / Supervisor
    ///     📁 Comercial
    ///       📁 Gerente Comercial / Vendedor
    ///     📁 Inventario y Logística
    ///       📁 ...
    ///
    /// El árbol se obtiene de BLL.Familia.ObtenerArbol() (árbol real desde BD vía
    /// PermisoRelacion) usando BE.Rol/BE.Familia (nodos compuestos) y BE.Patente (hojas).
    ///
    /// Implementa IIdiomaObserver: todos los controles se traducen al cambiar de idioma.
    /// NO modifica permisos ni afecta la autorización.
    /// </summary>
    public class ExploradorCompositeForm : Form, IIdiomaObserver
    {
        private readonly BLL.Familia _familiaBLL = new BLL.Familia();

        private TreeView _treeView;
        private Label    _lblTitulo;
        private Label    _lblDescripcion;
        private Label    _lblLeyenda;
        private Button   _btnCerrar;
        private Button   _btnExpandir;
        private Button   _btnColapsar;
        private Button   _btnActualizar;
        private Panel    _panelHeader;
        private Panel    _panelLeyenda;
        private Panel    _panelBotones;

        public ExploradorCompositeForm()
        {
            ConstruirUI();
        }

        // Helper de traducción — obtiene texto con fallback.
        private string T(string key, string fallback)
        {
            var t = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
            return t.ContainsKey(key) ? t[key].Texto : fallback;
        }

        // ── Ciclo de vida ─────────────────────────────────────────────────────

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                string ico = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico");
                if (System.IO.File.Exists(ico)) this.Icon = new Icon(ico);
            }
            catch { }
            GestorIdioma.SuscribirObservador(this);
            AplicarIdioma();
            CargarArbol();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            GestorIdioma.DesuscribirObservador(this);
            base.OnFormClosing(e);
        }

        // ── IIdiomaObserver ───────────────────────────────────────────────────

        public void UpdateLanguage(Idioma idioma)
        {
            AplicarIdioma();
            CargarArbol(); // reconstruye el árbol con nombres de patentes/familias en el nuevo idioma
        }

        // Actualiza TODOS los controles de texto del formulario con las traducciones activas.
        private void AplicarIdioma()
        {
            this.Text               = T("frm.explorador",             "Vista completa del sistema");
            _lblTitulo.Text         = T("lbl.explorador.titulo",      "Vista completa del sistema");
            _lblDescripcion.Text    = T("lbl.explorador.descripcion", "Estructura organizacional de WardrobeFlow — Solo lectura");
            _lblLeyenda.Text        = T("lbl.explorador.leyenda",     "📁 Familia (nodo compuesto — Área o Rol)    🔑 Patente (hoja — permiso atómico)");
            _btnCerrar.Text         = T("btn.explorador.cerrar",      "Cerrar");
            _btnColapsar.Text       = T("btn.explorador.colapsar",    "⊟ Colapsar todo");
            _btnExpandir.Text       = T("btn.explorador.expandir",    "⊞ Expandir todo");
            _btnActualizar.Text     = T("btn.permisos.actualizar",    "↻ Actualizar");
        }

        // ── Construcción del árbol ────────────────────────────────────────────

        private void CargarArbol()
        {
            // Obtener traducciones una sola vez para toda la construcción del árbol
            var traducciones = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);

            _treeView.BeginUpdate();
            _treeView.Nodes.Clear();

            try
            {
                // Árbol REAL desde BD (vía PermisoRelacion): roles, familias y patentes.
                // Los nodos raíz son los que no tienen padre (roles y familias huérfanas).
                var raices = _familiaBLL.ObtenerArbol();
                var empresa = new TreeNode("📁 WardrobeFlow")
                {
                    NodeFont  = new Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(176, 62, 96)
                };
                foreach (BE.Componente raiz in raices)
                    empresa.Nodes.Add(CrearNodoRecursivo(raiz, traducciones));
                _treeView.Nodes.Add(empresa);

                _treeView.ExpandAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(T("err.explorador.cargar", "Error al cargar árbol Composite:\n{0}"), ex.Message),
                    T("diag.err.titulo", "Error"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            _treeView.EndUpdate();
        }

        /// <summary>
        /// Construye un TreeNode recursivamente a partir de un BE.Componente.
        /// Familias → nodo azul en negrita con prefijo 📁
        /// Patentes  → nodo verde con prefijo 🔑
        ///
        /// Idéntico al método CrearNodoRecursivo de Stach/GUI/PermisosForm.cs.
        /// Las traducciones se pasan como parámetro para no llamar a ObtenerTraducciones en cada nodo.
        /// </summary>
        private TreeNode CrearNodoRecursivo(BE.Componente componente,
                                             IDictionary<string, Traduccion> t)
        {
            bool esFamilia = componente is BE.Familia;
            string prefijo = esFamilia ? "📁 " : "🔑 ";
            string nombre  = componente.Nombre;

            if (esFamilia)
            {
                // Intentar traducir como grupo de permisos
                string claveGrp = "perm.grp." + nombre.ToLowerInvariant()
                                      .Replace(" ", "").Replace("(", "").Replace(")", "");
                if (t.ContainsKey(claveGrp))
                    nombre = t[claveGrp].Texto;
                else
                {
                    // Intentar traducir como rol
                    string claveRol = "perm.rol." + componente.Nombre.ToLowerInvariant()
                                          .Replace(" ", "").Replace("(", "").Replace(")", "");
                    if (t.ContainsKey(claveRol)) nombre = t[claveRol].Texto;
                }
            }
            else
            {
                string clavePat = "perm.pat." + nombre.ToLowerInvariant()
                                      .Replace(" ", "").Replace("(", "").Replace(")", "");
                if (t.ContainsKey(clavePat)) nombre = t[clavePat].Texto;
            }

            var nodo = new TreeNode(prefijo + nombre)
            {
                Tag       = componente,
                NodeFont  = esFamilia
                    ? new Font("Segoe UI", 9f, FontStyle.Bold)
                    : new Font("Segoe UI", 9f),
                ForeColor = esFamilia
                    ? Color.FromArgb(176, 62, 96)
                    : Color.FromArgb(30, 110, 50)
            };

            // Recursión sobre los hijos — profundidad arbitraria
            if (esFamilia)
            {
                foreach (BE.Componente hijo in componente.Hijos)
                    nodo.Nodes.Add(CrearNodoRecursivo(hijo, t));
            }

            return nodo;
        }

        // ── Construcción de UI ────────────────────────────────────────────────

        private void ConstruirUI()
        {
            this.Text            = "Vista completa del sistema";
            this.Size            = new Size(680, 660);
            this.MinimumSize     = new Size(500, 500);
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.BackColor       = Color.White;

            // ── Encabezado ─────────────────────────────────────────────────────
            _panelHeader = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 80,
                BackColor = Color.FromArgb(176, 62, 96),
                Padding   = new Padding(14, 10, 14, 10)
            };

            _lblTitulo = new Label
            {
                Text      = "Vista completa del sistema",
                Font      = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize  = true,
                Location  = new Point(14, 10)
            };

            _lblDescripcion = new Label
            {
                Text      = "Estructura organizacional de WardrobeFlow — Solo lectura",
                Font      = new Font("Segoe UI", 9f, FontStyle.Italic),
                ForeColor = Color.FromArgb(244, 212, 226),
                AutoSize  = true,
                Location  = new Point(16, 42)
            };

            _panelHeader.Controls.AddRange(new Control[] { _lblTitulo, _lblDescripcion });

            // ── Leyenda ────────────────────────────────────────────────────────
            _panelLeyenda = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 30,
                BackColor = Color.FromArgb(252, 240, 246),
                Padding   = new Padding(14, 5, 0, 0)
            };

            _lblLeyenda = new Label
            {
                Text      = "📁 Familia (nodo compuesto — Área o Rol)    🔑 Patente (hoja — permiso atómico)",
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(110, 42, 74),
                AutoSize  = true,
                Location  = new Point(14, 6)
            };
            _panelLeyenda.Controls.Add(_lblLeyenda);

            // ── TreeView ───────────────────────────────────────────────────────
            _treeView = new TreeView
            {
                Dock          = DockStyle.Fill,
                CheckBoxes    = false,
                Font          = new Font("Segoe UI", 9.5f),
                ShowLines     = true,
                ShowPlusMinus = true,
                BorderStyle   = BorderStyle.None,
                BackColor     = Color.FromArgb(252, 250, 252),
                Indent        = 20,
                ItemHeight    = 24,
                FullRowSelect = true,
                HideSelection = false
            };

            // ── Panel de botones ───────────────────────────────────────────────
            _panelBotones = new FlowLayoutPanel
            {
                Dock          = DockStyle.Bottom,
                Height        = 46,
                FlowDirection = FlowDirection.RightToLeft,
                Padding       = new Padding(6, 6, 6, 6),
                BackColor     = Color.FromArgb(252, 240, 246)
            };

            _btnCerrar = new Button
            {
                Text      = "Cerrar",
                Size      = new Size(100, 32),
                BackColor = Color.FromArgb(210, 200, 220),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9f),
                Cursor    = Cursors.Hand
            };
            _btnCerrar.FlatAppearance.BorderSize = 0;
            _btnCerrar.Click += (s, e) => this.Close();

            _btnColapsar = new Button
            {
                Text      = "⊟ Colapsar todo",
                Size      = new Size(130, 32),
                BackColor = Color.FromArgb(210, 100, 135),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9f),
                Cursor    = Cursors.Hand
            };
            _btnColapsar.FlatAppearance.BorderSize = 0;
            _btnColapsar.Click += (s, e) => _treeView.CollapseAll();

            _btnExpandir = new Button
            {
                Text      = "⊞ Expandir todo",
                Size      = new Size(130, 32),
                BackColor = Color.FromArgb(176, 62, 96),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9f),
                Cursor    = Cursors.Hand
            };
            _btnExpandir.FlatAppearance.BorderSize = 0;
            _btnExpandir.Click += (s, e) => _treeView.ExpandAll();

            // Actualizar: recarga el árbol desde la BD sin reabrir el form (estilo contorno
            // para diferenciarlo de expandir/colapsar).
            _btnActualizar = new Button
            {
                Text      = "↻ Actualizar",
                Size      = new Size(120, 32),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(176, 62, 96),
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9f),
                Cursor    = Cursors.Hand
            };
            _btnActualizar.FlatAppearance.BorderSize = 1;
            _btnActualizar.FlatAppearance.BorderColor = Color.FromArgb(176, 62, 96);
            _btnActualizar.Click += (s, e) => CargarArbol();

            _panelBotones.Controls.AddRange(new Control[] { _btnCerrar, _btnColapsar, _btnExpandir, _btnActualizar });

            this.Controls.Add(_treeView);
            this.Controls.Add(_panelBotones);
            this.Controls.Add(_panelLeyenda);
            this.Controls.Add(_panelHeader);
        }
    }
}
