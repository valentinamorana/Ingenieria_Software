using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Windows.Forms;
using Servicios.Multiidioma;

namespace GUI
{
    /// <summary>
    /// Capa de Presentación — Formulario Menú Principal (MDI Container).
    ///
    /// Al iniciarse, construye el menú dinámicamente según los permisos
    /// del usuario logueado (cargados desde RolPermiso en el Login).
    ///
    /// Roles del sistema (documento G04 — WardrobeFlow_Iteracion1.docx):
    ///
    ///   Administrador     → TODO: Inventario | Ventas | Administrar | Bitácora
    ///   Supervisor        → Bitácora
    ///   OperadorLogistico → Inventario (Prendas, Outfits, Categorias, Pedidos Realizados)
    ///
    /// Roles adicionales (implementación, no están en G04):
    ///   Vendedor             → Ventas (Clientes, Planes, Pedidos de Venta)
    ///   ControladorDeStock   → Inventario (Prendas, Stock)
    ///   OperadorDeInventario → Ventas (Pedidos Realizados)
    ///
    /// Los permisos se leen de BE.Usuario.Permisos via BLL.ObtenerUsuarioActivo().
    /// La GUI nunca accede directamente a Seguridad ni a DAL.
    ///
    /// PATRÓN OBSERVER — T05 Gestión de Múltiples Idiomas:
    ///   Implementa IIdiomaObserver. Se suscribe al GestorIdioma en Load
    ///   y se desuscribe en FormClosing. Al recibir UpdateLanguage() llama
    ///   a Traducir() que reasigna el .Text de todos los ítems del menú.
    ///   La barra de idiomas (ToolStrip con 3 botones) se construye en el constructor.
    /// </summary>
    public partial class Menu : Form, IIdiomaObserver
    {
        // Botones de idioma — se guardan para poder marcar el activo con negrita
        private ToolStripButton _btnES, _btnEN, _btnRU;
        // Label "Idioma:" / "Language:" / "Язык:" — dinámico por Observer
        private ToolStripLabel _lblIdioma;
        // Usuario cargado en el constructor; reutilizado en OnLoad para no hacer dos SELECT
        private BE.Usuario _usuarioActivo;

        public Menu()
        {
            InitializeComponent();

            // ── Barra de selección de idioma ─────────────────────────────────
            // Se agrega un ToolStrip justo debajo del MenuStrip existente.
            // Los 3 botones llaman a GestorIdioma.CambiarIdioma() → notifica a
            // todos los formularios abiertos de forma automática (patrón Observer).
            var tsIdioma = new ToolStrip
            {
                Dock      = DockStyle.Top,
                BackColor = Color.FromArgb(40, 40, 55),
                GripStyle = ToolStripGripStyle.Hidden,
                Padding   = new Padding(4, 0, 4, 0),
                Height    = 28
            };

            _lblIdioma = new ToolStripLabel
            {
                Text      = "Idioma:",
                ForeColor = Color.FromArgb(200, 200, 210),
                Font      = new System.Drawing.Font("Segoe UI", 8.5f)
            };

            _btnES = CrearBotonIdioma("Español", "ES");
            _btnEN = CrearBotonIdioma("English", "EN");
            _btnRU = CrearBotonIdioma("Русский", "RU");

            tsIdioma.Items.Add(_lblIdioma);
            tsIdioma.Items.Add(new ToolStripSeparator());
            tsIdioma.Items.Add(_btnES);
            tsIdioma.Items.Add(_btnEN);
            tsIdioma.Items.Add(_btnRU);

            // Insertar el ToolStrip después del MenuStrip (índice 0 = primero visible abajo del borde)
            this.Controls.Add(tsIdioma);
            tsIdioma.BringToFront();

            // Marcar ES como activo por defecto
            MarcarIdiomaActivo("ES");

            // Obtener usuario activo via BLL (GUI nunca toca SessionManager directamente)
            _usuarioActivo = new BLL.Usuario().ObtenerUsuarioActivo();

            if (_usuarioActivo != null)
            {
                this.Text = "WardrobeFlow  —  " + _usuarioActivo.Username +
                            (_usuarioActivo.Perfil != null ? "  [" + _usuarioActivo.Perfil + "]" : "");
            }

            // Construir menú dinámico según permisos del rol
            AplicarPermisos(_usuarioActivo?.Permisos);
        }

        /// <summary>
        /// Genera un tile 160×148 con el monograma WF en patrón de ladrillos
        /// (filas alternadas desplazadas medio tile) sobre fondo rosa claro.
        /// El tile repite perfectamente en ambas direcciones sin cortes visibles.
        /// </summary>
        private static Bitmap GenerarTileWF()
        {
            const int TW = 80, TH = 74;
            var bmp = new Bitmap(TW * 2, TH * 2);

            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.FromArgb(252, 228, 235));
                g.SmoothingMode     = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.AntiAlias;

                var tinta = Color.FromArgb(40, 180, 70, 100);

                using (var fontW = new Font("Georgia", 22, FontStyle.Bold, GraphicsUnit.Point))
                using (var fontF = new Font("Georgia", 18, FontStyle.Bold, GraphicsUnit.Point))
                using (var br    = new SolidBrush(tinta))
                {
                    // Fila 0 — sin offset (y = 0)
                    g.DrawString("W", fontW, br, 0,      0);
                    g.DrawString("F", fontF, br, 24,     20);
                    g.DrawString("W", fontW, br, TW,     0);
                    g.DrawString("F", fontF, br, TW + 24, 20);

                    // Fila 1 — offset medio tile (y = TH)
                    g.DrawString("W", fontW, br, TW / 2f,       TH);
                    g.DrawString("F", fontF, br, TW / 2f + 24,  TH + 20);
                    g.DrawString("W", fontW, br, TW / 2f + TW,  TH);
                    g.DrawString("F", fontF, br, TW / 2f + TW + 24, TH + 20);
                }
            }

            return bmp;
        }

        /// <summary>
        /// Muestra u oculta los ítems del menú según los permisos del usuario.
        /// La lógica es completamente basada en permisos (NombreMenu), no en roles.
        ///
        /// Mapeo NombreMenu → ToolStripMenuItem:
        ///   mnuPrendas            → prendasToolStripMenuItem       (bajo Inventario)
        ///   mnuOutfits            → outfitsToolStripMenuItem        (bajo Inventario)
        ///   mnuCategorias         → categoriasToolStripMenuItem     (bajo Inventario)
        ///   mnuStock              → stockToolStripMenuItem          (bajo Inventario — pendiente en Designer)
        ///   mnuClientes           → clientesToolStripMenuItem       (bajo Ventas)
        ///   mnuPlanSuscripciones  → planesToolStripMenuItem         (bajo Ventas)
        ///   mnuPedidosVenta       → pedidosVentaToolStripMenuItem   (bajo Ventas)
        ///   mnuPedidosRealizados  → pedidosRealizadosToolStripMenuItem (bajo Ventas)
        ///   mnuUsuarios           → gestionToolStripMenuItem        (bajo Administrar)
        ///   mnuAuditoria          → bitacoraToolStripMenuItem
        ///
        /// Administrador tiene los 10 permisos → ve todo el menú.
        /// </summary>
        private void AplicarPermisos(List<BE.Permiso> permisos)
        {
            // Ocultar todo por defecto
            inventarioToolStripMenuItem.Visible = false;
            ventasToolStripMenuItem.Visible     = false;
            gestionToolStripMenuItem.Visible    = false;
            bitacoraToolStripMenuItem.Visible   = false;

            if (permisos == null || permisos.Count == 0) return;

            // HashSet para búsqueda O(1) por NombreMenu
            var nombresMenu = new HashSet<string>();
            foreach (var p in permisos)
                nombresMenu.Add(p.NombreMenu);

            // ── Inventario ────────────────────────────────────────────────────
            bool tienePrendas = nombresMenu.Contains("mnuPrendas");
            bool tieneStock   = nombresMenu.Contains("mnuStock");

            // Outfits y Categorías eliminados de la interfaz (módulos no implementados)
            prendasToolStripMenuItem.Visible    = tienePrendas;
            outfitsToolStripMenuItem.Visible    = false;
            categoriasToolStripMenuItem.Visible = false;

            inventarioToolStripMenuItem.Visible = tienePrendas || tieneStock;

            // ── Ventas ────────────────────────────────────────────────────────
            bool tieneClientes     = nombresMenu.Contains("mnuClientes");
            bool tienePlanes       = nombresMenu.Contains("mnuPlanSuscripciones");
            bool tienePedidosVenta = nombresMenu.Contains("mnuPedidosVenta");
            bool tienePedidosReal  = nombresMenu.Contains("mnuPedidosRealizados");

            clientesToolStripMenuItem.Visible          = tieneClientes;
            planesToolStripMenuItem.Visible            = tienePlanes;
            pedidosVentaToolStripMenuItem.Visible      = tienePedidosVenta;
            pedidosRealizadosToolStripMenuItem.Visible = tienePedidosReal;

            ventasToolStripMenuItem.Visible =
                tieneClientes || tienePlanes || tienePedidosVenta || tienePedidosReal;

            // ── Administrar (Usuarios + Perfiles) ─────────────────────────────
            bool tieneUsuarios = nombresMenu.Contains("mnuUsuarios");
            usuariosToolStripMenuItem.Visible = tieneUsuarios;
            perfilesToolStripMenuItem.Visible = tieneUsuarios;
            idiomasToolStripMenuItem.Visible  = tieneUsuarios;   // solo Admin gestiona traducciones
            gestionToolStripMenuItem.Visible  = tieneUsuarios;

            // ── Bitácora ──────────────────────────────────────────────────────
            bitacoraToolStripMenuItem.Visible = nombresMenu.Contains("mnuAuditoria");
        }

        /// <summary>
        /// Cierra la sesión y reinicia la aplicación para volver al Login con estado limpio.
        /// </summary>
        private void cerrarSesionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ConfirmarCerrarSesion())
            {
                new BLL.Usuario().Logout(this);
                Application.Restart();
            }
        }

        /// <summary>
        /// Diálogo de confirmación de cierre de sesión con botones traducidos al idioma activo.
        /// Reemplaza MessageBox.Show() cuyo "Yes"/"No" es siempre en inglés (Windows).
        /// </summary>
        private bool ConfirmarCerrarSesion()
        {
            var t = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
            string T(string key, string fallback) => t.ContainsKey(key) ? t[key].Texto : fallback;

            using (var dlg = new Form())
            {
                dlg.Text            = T("dlg.cerrarsesion.titulo", "Cerrar Sesión");
                dlg.ClientSize      = new Size(340, 126);
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition   = FormStartPosition.CenterParent;
                dlg.MaximizeBox     = false;
                dlg.MinimizeBox     = false;

                var lbl = new Label
                {
                    Text      = T("dlg.cerrarsesion.msg", "¿Está seguro que desea cerrar la sesión?"),
                    Left = 16, Top = 20, Width = 308, Height = 44,
                    Font      = new System.Drawing.Font("Segoe UI", 9.5f),
                    TextAlign = System.Drawing.ContentAlignment.MiddleCenter
                };

                var btnSi = new Button
                {
                    Text         = T("btn.si", "Sí"),
                    Left = 84, Top = 76, Width = 76, Height = 30,
                    DialogResult = DialogResult.Yes,
                    BackColor    = Color.FromArgb(60, 110, 160),
                    ForeColor    = Color.White,
                    FlatStyle    = FlatStyle.Flat
                };
                btnSi.FlatAppearance.BorderSize = 0;

                var btnNo = new Button
                {
                    Text         = T("btn.no", "No"),
                    Left = 176, Top = 76, Width = 76, Height = 30,
                    DialogResult = DialogResult.No,
                    FlatStyle    = FlatStyle.Flat
                };

                dlg.Controls.AddRange(new Control[] { lbl, btnSi, btnNo });
                dlg.AcceptButton = btnSi;
                dlg.CancelButton = btnNo;

                return dlg.ShowDialog(this) == DialogResult.Yes;
            }
        }

        /// <summary>
        /// Abre Bitácora como hijo MDI. Accesible para Administrador y Supervisor.
        /// </summary>
        private void bitacoraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form hijo in this.MdiChildren)
            {
                if (hijo is Bitacora) { hijo.BringToFront(); return; }
            }
            new Bitacora { MdiParent = this }.Show();
        }

        /// <summary>
        /// Abre Gestión de Usuarios como hijo MDI. Accesible solo para Administrador.
        /// </summary>
        private void usuariosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form hijo in this.MdiChildren)
            {
                if (hijo is Usuarios) { hijo.BringToFront(); return; }
            }
            new Usuarios { MdiParent = this }.Show();
        }

        /// <summary>
        /// Abre el Gestor de Perfiles y Permisos como hijo MDI — T04 Composite Pattern.
        /// Accesible solo para Administrador.
        /// </summary>
        private void perfilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form hijo in this.MdiChildren)
            {
                if (hijo is GestorPermisos) { hijo.BringToFront(); return; }
            }
            new GestorPermisos { MdiParent = this }.Show();
        }

        private void idiomasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form hijo in this.MdiChildren)
            {
                if (hijo is FormIdiomas) { hijo.BringToFront(); return; }
            }
            new FormIdiomas { MdiParent = this }.Show();
        }

        /// <summary>
        /// Abre el módulo de Prendas como hijo MDI.
        /// </summary>
        private void prendasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form hijo in this.MdiChildren)
            {
                if (hijo is Prendas) { hijo.BringToFront(); return; }
            }
            new Prendas { MdiParent = this }.Show();
        }

        /// <summary>
        /// Abre el módulo de Outfits como hijo MDI.
        /// TODO: implementar cuando se cree el formulario Outfits.
        /// </summary>
        private void outfitsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var tM = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
            string T_m(string k, string fb) => tM.ContainsKey(k) ? tM[k].Texto : fb;
            MessageBox.Show(
                T_m("msg.modulo.outfits",    "El módulo de Outfits aún no está disponible."),
                T_m("lbl.proximamente",       "Próximamente"),
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Abre el módulo de Categorías como hijo MDI.
        /// TODO: implementar cuando se cree el formulario Categorias.
        /// </summary>
        private void categoriasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var tM = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
            string T_m(string k, string fb) => tM.ContainsKey(k) ? tM[k].Texto : fb;
            MessageBox.Show(
                T_m("msg.modulo.categorias", "El módulo de Categorías aún no está disponible."),
                T_m("lbl.proximamente",       "Próximamente"),
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Abre el módulo de Clientes como hijo MDI. Accesible para Vendedor.
        /// </summary>
        private void clientesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form hijo in this.MdiChildren)
            {
                if (hijo is Clientes) { hijo.BringToFront(); return; }
            }
            new Clientes { MdiParent = this }.Show();
        }

        /// <summary>
        /// Abre el módulo de Planes de Suscripción como hijo MDI.
        /// </summary>
        private void planesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form hijo in this.MdiChildren)
            {
                if (hijo is Planes) { hijo.BringToFront(); return; }
            }
            new Planes { MdiParent = this }.Show();
        }

        /// <summary>
        /// Abre el módulo de Pedidos de Venta como hijo MDI. Accesible para Vendedor.
        /// </summary>
        private void pedidosVentaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form hijo in this.MdiChildren)
            {
                if (hijo is PedidosVenta) { hijo.BringToFront(); return; }
            }
            new PedidosVenta { MdiParent = this }.Show();
        }

        /// <summary>
        /// Abre el módulo de Pedidos Realizados como hijo MDI.
        /// Accesible para OperadorDeInventario (mnuPedidosRealizados).
        /// </summary>
        private void pedidosRealizadosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form hijo in this.MdiChildren)
            {
                if (hijo is PedidosRealizados) { hijo.BringToFront(); return; }
            }
            new PedidosRealizados { MdiParent = this }.Show();
        }

        // ══════════════════════════════════════════════════════════════════════
        // PATRÓN OBSERVER — T05 Gestión de Múltiples Idiomas
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Suscribe este formulario al GestorIdioma al abrirse.
        /// Equivalente a frmMain_Load → ManejadorDeSesion.SuscribirObservador(this)
        /// del ejemplo de cátedra.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Fondo MDI rosa con monograma WF
            foreach (Control c in Controls)
            {
                if (c.GetType().Name == "MdiClient")
                {
                    c.BackColor             = Color.FromArgb(252, 228, 235);
                    c.BackgroundImage       = GenerarTileWF();
                    c.BackgroundImageLayout = ImageLayout.Tile;
                    break;
                }
            }

            // Suscribirse al Observer de idioma
            GestorIdioma.SuscribirObservador(this);

            // Continuar con el idioma activo en el Login (seleccionado por el usuario).
            // El DB preference se aplica cuando el usuario clickea un botón en el Menu.
            string codigoPref = GestorIdioma.IdiomaActual?.Id ?? "ES";

            try
            {
                var dictTrad = new BLL.IdiomaService().CargarTraducciones(codigoPref);
                foreach (var idm in Traductor.ObtenerIdiomas())
                {
                    if (idm.Id == codigoPref)
                    {
                        GestorIdioma.CambiarIdioma(idm, dictTrad);
                        MarcarIdiomaActivo(codigoPref);
                        break;
                    }
                }
            }
            catch
            {
                // Sin conexión: usa fallback hardcodeado
                Traducir(GestorIdioma.IdiomaActual);
            }
        }

        /// <summary>
        /// Desuscribe este formulario del GestorIdioma al cerrarse.
        /// Equivalente a frmMain_FormClosing → ManejadorDeSesion.DesuscribirObservador(this)
        /// del ejemplo de cátedra.
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            GestorIdioma.DesuscribirObservador(this);
            base.OnFormClosing(e);
        }

        /// <summary>
        /// Recibe la notificación del GestorIdioma cuando el idioma cambia.
        /// Equivalente a UpdateLanguage(IIdioma idioma) del ejemplo de cátedra.
        /// </summary>
        public void UpdateLanguage(Idioma idioma)
        {
            Traducir(idioma);
            MarcarIdiomaActivo(idioma.Id);
        }

        /// <summary>
        /// Reasigna el .Text de cada ítem del menú leyendo su propiedad Tag como
        /// clave de traducción — exactamente igual que en el ejemplo de cátedra (frmMain).
        /// Los Tags se asignan en el Designer; el código no hardcodea ninguna clave.
        /// </summary>
        private void Traducir(Idioma idioma)
        {
            var t = Traductor.ObtenerTraducciones(idioma);

            // Label dinámico "Idioma:" / "Language:" / "Язык:"
            if (t.ContainsKey("lbl.idioma"))
                _lblIdioma.Text = t["lbl.idioma"].Texto;

            Aplicar(usuarioToolStripMenuItem,           t);
            Aplicar(inventarioToolStripMenuItem,        t);
            Aplicar(prendasToolStripMenuItem,           t);
            Aplicar(ventasToolStripMenuItem,            t);
            Aplicar(clientesToolStripMenuItem,          t);
            Aplicar(planesToolStripMenuItem,            t);
            Aplicar(pedidosVentaToolStripMenuItem,      t);
            Aplicar(pedidosRealizadosToolStripMenuItem, t);
            Aplicar(gestionToolStripMenuItem,           t);
            Aplicar(usuariosToolStripMenuItem,          t);
            Aplicar(perfilesToolStripMenuItem,          t);
            Aplicar(idiomasToolStripMenuItem,           t);
            Aplicar(bitacoraToolStripMenuItem,          t);
            Aplicar(cerrarSesionToolStripMenuItem,      t);
        }

        /// <summary>
        /// Lee el Tag del ítem para obtener la clave y aplica la traducción.
        /// Equivalente al patrón if (item.Tag != null && traducciones.ContainsKey(...))
        /// del ejemplo de cátedra — el Tag actúa como clave del diccionario.
        /// </summary>
        private static void Aplicar(ToolStripMenuItem item,
            IDictionary<string, Traduccion> t)
        {
            if (item != null && item.Tag != null && t.ContainsKey(item.Tag.ToString()))
                item.Text = t[item.Tag.ToString()].Texto;
        }

        // ── Helpers de la barra de idioma ─────────────────────────────────────

        /// <summary>
        /// Crea un botón de idioma para el ToolStrip. Al hacer click llama a
        /// GestorIdioma.CambiarIdioma() que notifica a todos los observers.
        /// </summary>
        private ToolStripButton CrearBotonIdioma(string texto, string codigoIdioma)
        {
            var btn = new ToolStripButton
            {
                Text        = texto,
                ForeColor   = Color.FromArgb(210, 210, 220),
                BackColor   = Color.Transparent,
                Font        = new System.Drawing.Font("Segoe UI", 8.5f),
                AutoSize    = true,
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                Padding     = new Padding(6, 0, 6, 0)
            };
            btn.Click += (s, e) =>
            {
                foreach (var idioma in Traductor.ObtenerIdiomas())
                {
                    if (idioma.Id != codigoIdioma) continue;

                    try
                    {
                        // Un solo SELECT a BD → cache en GestorIdioma → todos los forms se traducen
                        var dictTrad = new BLL.IdiomaService().CargarTraducciones(codigoIdioma);
                        GestorIdioma.CambiarIdioma(idioma, dictTrad);
                    }
                    catch
                    {
                        // Sin conexión: usa fallback hardcodeado
                        GestorIdioma.CambiarIdioma(idioma);
                    }

                    // Persistir preferencia en BD — reutiliza el campo del formulario
                    try
                    {
                        if (_usuarioActivo != null)
                            new BLL.Usuario().GuardarPreferenciaIdioma(_usuarioActivo.Id, codigoIdioma);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"[Menu] Error al guardar preferencia de idioma: {ex.Message}");
                    }
                    break;
                }
            };
            return btn;
        }

        /// <summary>
        /// Resalta en negrita el botón del idioma activo y normaliza los demás.
        /// </summary>
        private void MarcarIdiomaActivo(string codigoIdioma)
        {
            var fontNormal  = new System.Drawing.Font("Segoe UI", 8.5f, FontStyle.Regular);
            var fontActivo  = new System.Drawing.Font("Segoe UI", 8.5f, FontStyle.Bold);
            var colorActivo = Color.White;
            var colorNormal = Color.FromArgb(170, 170, 180);

            _btnES.Font      = codigoIdioma == "ES" ? fontActivo  : fontNormal;
            _btnES.ForeColor = codigoIdioma == "ES" ? colorActivo : colorNormal;
            _btnEN.Font      = codigoIdioma == "EN" ? fontActivo  : fontNormal;
            _btnEN.ForeColor = codigoIdioma == "EN" ? colorActivo : colorNormal;
            _btnRU.Font      = codigoIdioma == "RU" ? fontActivo  : fontNormal;
            _btnRU.ForeColor = codigoIdioma == "RU" ? colorActivo : colorNormal;
        }

    }
}
