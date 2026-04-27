using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Windows.Forms;

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
    /// </summary>
    public partial class Menu : Form
    {
        public Menu()
        {
            InitializeComponent();

            // Obtener usuario activo via BLL (GUI nunca toca SessionManager directamente)
            BLL.Usuario bll = new BLL.Usuario();
            BE.Usuario usuarioActivo = bll.ObtenerUsuarioActivo();

            if (usuarioActivo != null)
            {
                this.Text = "WardrobeFlow  —  " + usuarioActivo.Username +
                            (usuarioActivo.Perfil != null ? "  [" + usuarioActivo.Perfil + "]" : "");
            }

            // Construir menú dinámico según permisos del rol
            AplicarPermisos(usuarioActivo?.Permisos);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Aplicar fondo rosa con monograma WF al área MDI.
            // Se accede al MdiClient interno y se le asigna un BackgroundImage
            // generado en código — más confiable que interceptar WM_ERASEBKGND.
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

            // ── Administrar (Usuarios) ────────────────────────────────────────
            bool tieneUsuarios = nombresMenu.Contains("mnuUsuarios");
            usuariosToolStripMenuItem.Visible = tieneUsuarios;
            gestionToolStripMenuItem.Visible  = tieneUsuarios;

            // ── Bitácora ──────────────────────────────────────────────────────
            bitacoraToolStripMenuItem.Visible = nombresMenu.Contains("mnuAuditoria");
        }

        /// <summary>
        /// Cierra la sesión y reinicia la aplicación para volver al Login con estado limpio.
        /// </summary>
        private void cerrarSesionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var resultado = MessageBox.Show(
                "¿Está seguro que desea cerrar la sesión?",
                "Cerrar Sesión",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (resultado == DialogResult.Yes)
            {
                new BLL.Usuario().Logout(this);
                Application.Restart();
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
            MessageBox.Show("El módulo de Outfits aún no está disponible.",
                "Próximamente", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Abre el módulo de Categorías como hijo MDI.
        /// TODO: implementar cuando se cree el formulario Categorias.
        /// </summary>
        private void categoriasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("El módulo de Categorías aún no está disponible.",
                "Próximamente", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

    }
}
