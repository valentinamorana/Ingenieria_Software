using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace GUI
{
    /// <summary>
    /// Capa de Presentación — Formulario Menú Principal (MDI Container).
    ///
    /// Al iniciarse, construye el menú dinámicamente según los permisos
    /// del usuario logueado (cargados desde RolPermiso en el Login):
    ///
    ///   Administrador        → Perfil | Administrar (Usuarios) | Bitácora
    ///   Supervisor           → Perfil | Bitácora
    ///   OperadorLogistico    → Perfil | Inventario (Prendas, Outfits, Categorias)
    ///   Vendedor             → Perfil | Ventas (Clientes, Planes, Pedidos de Venta)
    ///   ControladorDeStock   → Perfil | Inventario (Prendas, Stock)
    ///   OperadorDeInventario → Perfil | Ventas (Pedidos Realizados)
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

        /// <summary>
        /// Muestra u oculta los ítems del menú según los permisos del usuario.
        ///
        /// Mapeo NombreMenu → ToolStripMenuItem:
        ///   mnuPrendas            → prendasToolStripMenuItem       (bajo Inventario)
        ///   mnuOutfits            → outfitsToolStripMenuItem        (bajo Inventario)
        ///   mnuCategorias         → categoriasToolStripMenuItem     (bajo Inventario)
        ///   mnuStock              → (futuro: stockToolStripMenuItem bajo Inventario)
        ///   mnuClientes           → clientesToolStripMenuItem       (bajo Ventas)
        ///   mnuPlanSuscripciones  → planesToolStripMenuItem         (bajo Ventas)
        ///   mnuPedidosVenta       → pedidosVentaToolStripMenuItem   (bajo Ventas)
        ///   mnuUsuarios           → gestionToolStripMenuItem + usuariosToolStripMenuItem
        ///   mnuAuditoria          → bitacoraToolStripMenuItem
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
            bool tienePrendas    = nombresMenu.Contains("mnuPrendas");
            bool tieneOutfits    = nombresMenu.Contains("mnuOutfits");
            bool tieneCategorias = nombresMenu.Contains("mnuCategorias");

            prendasToolStripMenuItem.Visible    = tienePrendas;
            outfitsToolStripMenuItem.Visible    = tieneOutfits;
            categoriasToolStripMenuItem.Visible = tieneCategorias;

            inventarioToolStripMenuItem.Visible = tienePrendas || tieneOutfits || tieneCategorias;

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
            gestionToolStripMenuItem.Visible = nombresMenu.Contains("mnuUsuarios");

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
        /// </summary>
        private void outfitsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form hijo in this.MdiChildren)
            {
                if (hijo is Outfits) { hijo.BringToFront(); return; }
            }
            new Outfits { MdiParent = this }.Show();
        }

        /// <summary>
        /// Abre el módulo de Categorías como hijo MDI.
        /// </summary>
        private void categoriasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form hijo in this.MdiChildren)
            {
                if (hijo is Categorias) { hijo.BringToFront(); return; }
            }
            new Categorias { MdiParent = this }.Show();
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
