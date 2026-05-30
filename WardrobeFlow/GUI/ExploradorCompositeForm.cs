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
    /// El árbol se construye en BLL.Familia.ConstruirArbolOrganizacional()
    /// usando BE.Familia (nodo compuesto) y BE.Patente (hoja) — exactamente como Stach.
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
            this.Text               = T("frm.explorador",             "Explorador del Patrón Composite — T04");
            _lblTitulo.Text         = T("lbl.explorador.titulo",      "Explorador del Patrón Composite");
            _lblDescripcion.Text    = T("lbl.explorador.descripcion", "Estructura organizacional de WardrobeFlow — Solo lectura");
            _lblLeyenda.Text        = T("lbl.explorador.leyenda",     "📁 Familia (nodo compuesto — Área o Rol)    🔑 Patente (hoja — permiso atómico)");
            _btnCerrar.Text         = T("btn.explorador.cerrar",      "Cerrar");
            _btnColapsar.Text       = T("btn.explorador.colapsar",    "⊟ Colapsar todo");
            _btnExpandir.Text       = T("btn.explorador.expandir",    "⊞ Expandir todo");
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
                BE.Familia empresa = _familiaBLL.ConstruirArbolOrganizacional();

                // Construir TreeView recursivamente — idéntico al patrón de Stach
                foreach (BE.Componente hijo in empresa.Hijos)
                    _treeView.Nodes.Add(CrearNodoRecursivo(hijo, traducciones));

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
                    ? Color.FromArgb(40, 80, 160)
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
            this.Text            = "Explorador del Patrón Composite — T04";
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
                BackColor = Color.FromArgb(64, 0, 64),
                Padding   = new Padding(14, 10, 14, 10)
            };

            _lblTitulo = new Label
            {
                Text      = "Explorador del Patrón Composite",
                Font      = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize  = true,
                Location  = new Point(14, 10)
            };

            _lblDescripcion = new Label
            {
                Text      = "Estructura organizacional de WardrobeFlow — Solo lectura",
                Font      = new Font("Segoe UI", 9f, FontStyle.Italic),
                ForeColor = Color.FromArgb(220, 180, 220),
                AutoSize  = true,
                Location  = new Point(16, 42)
            };

            _panelHeader.Controls.AddRange(new Control[] { _lblTitulo, _lblDescripcion });

            // ── Leyenda ────────────────────────────────────────────────────────
            _panelLeyenda = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 30,
                BackColor = Color.FromArgb(245, 240, 255),
                Padding   = new Padding(14, 5, 0, 0)
            };

            _lblLeyenda = new Label
            {
                Text      = "📁 Familia (nodo compuesto — Área o Rol)    🔑 Patente (hoja — permiso atómico)",
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(60, 40, 80),
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
                BackColor     = Color.FromArgb(252, 250, 255),
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
                BackColor     = Color.FromArgb(245, 240, 255)
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
                BackColor = Color.FromArgb(180, 160, 200),
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
                BackColor = Color.FromArgb(64, 0, 64),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9f),
                Cursor    = Cursors.Hand
            };
            _btnExpandir.FlatAppearance.BorderSize = 0;
            _btnExpandir.Click += (s, e) => _treeView.ExpandAll();

            _panelBotones.Controls.AddRange(new Control[] { _btnCerrar, _btnColapsar, _btnExpandir });

            this.Controls.Add(_treeView);
            this.Controls.Add(_panelBotones);
            this.Controls.Add(_panelLeyenda);
            this.Controls.Add(_panelHeader);
        }
    }
}
