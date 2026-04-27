using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GUI
{
    /// <summary>
    /// Capa de Presentación — Módulo de Pedidos Realizados (OperadorDeInventario).
    ///
    /// Permite al OperadorDeInventario gestionar el ciclo de vida post-venta:
    ///   ✓ Ver todos los pedidos con su estado actual
    ///   ✓ Filtrar por estado (Todos / Pendiente / Despachado / Entregado / Cancelado)
    ///   ✓ Despachar un pedido Pendiente → estado Despachado
    ///   ✓ Marcar Entregado un pedido Despachado → estado Entregado
    ///   ✓ Ver el detalle de prendas de cada pedido
    ///   ✓ Ver notificación de despacho (resumen para comunicar al cliente)
    ///
    /// Accesible desde Menú → Ventas → Pedidos Realizados (permiso mnuPedidosRealizados).
    /// </summary>
    public partial class PedidosRealizados : Form
    {
        private readonly BLL.Pedido pedidoBLL = new BLL.Pedido();

        private List<BE.Pedido> _pedidos = new List<BE.Pedido>();

        public PedidosRealizados()
        {
            InitializeComponent();
            this.Load += new EventHandler(PedidosRealizados_Load);
        }

        private void PedidosRealizados_Load(object sender, EventArgs e)
        {
            CargarPedidos();
        }

        private void CmbFiltroEstado_SelectedIndexChanged(object sender, EventArgs e)
        {
            AplicarFiltro();
        }

        private void NudDiasFiltro_ValueChanged(object sender, EventArgs e)
        {
            AplicarFiltro();
        }

        private void BtnRefrescar_Click(object sender, EventArgs e)
        {
            CargarPedidos();
        }

        // Carga y filtrado
        private void CargarPedidos()
        {
            try
            {
                _pedidos = pedidoBLL.ObtenerTodos();
                AplicarFiltro();
                MostrarOk($"{_pedidos.Count} pedido(s) en el sistema.");
            }
            catch (Exception ex)
            {
                MostrarError($"Error al cargar: {ex.Message}");
            }
        }

        private void AplicarFiltro()
        {
            int estadoIdx = cmbFiltroEstado.SelectedIndex;
            int dias      = (int)nudDiasFiltro.Value;
            DateTime? corte = dias > 0 ? (DateTime?)DateTime.Now.AddDays(-dias) : null;

            var lista = _pedidos.FindAll(p =>
            {
                bool pasaEstado =
                    estadoIdx == 0 ||
                    (estadoIdx == 1 && p.Estado == BE.EstadoPedido.Pendiente)  ||
                    (estadoIdx == 2 && p.Estado == BE.EstadoPedido.Despachado) ||
                    (estadoIdx == 3 && p.Estado == BE.EstadoPedido.Entregado)  ||
                    (estadoIdx == 4 && p.Estado == BE.EstadoPedido.Cancelado);

                bool pasaDias = corte == null || p.FechaPedido >= corte;

                return pasaEstado && pasaDias;
            });

            var tabla = new DataTable();
            tabla.Columns.Add("ID",        typeof(int));
            tabla.Columns.Add("Urgencia",  typeof(string));
            tabla.Columns.Add("Fecha",     typeof(string));
            tabla.Columns.Add("Cliente",   typeof(string));
            tabla.Columns.Add("Vendedor",  typeof(string));
            tabla.Columns.Add("Prendas",   typeof(int));
            tabla.Columns.Add("Estado",    typeof(string));
            tabla.Columns.Add("Despacho",  typeof(string));
            tabla.Columns.Add("Entrega",   typeof(string));

            foreach (var p in lista)
            {
                tabla.Rows.Add(
                    p.IdPedido,
                    ComputarUrgencia(p),
                    p.FechaPedido.ToString("dd/MM/yyyy HH:mm"),
                    p.NombreCliente,
                    p.NombreEmpleado,
                    p.CantidadPrendas,
                    EstadoLabel(p.Estado),
                    p.FechaDespacho?.ToString("dd/MM/yyyy") ?? "—",
                    p.FechaEntrega?.ToString("dd/MM/yyyy")  ?? "—");
            }

            dgvPedidos.DataSource = tabla;
            ColorearFilas();

            if (dgvPedidos.Columns.Contains("ID"))
                dgvPedidos.Columns["ID"].Width = 44;
            if (dgvPedidos.Columns.Contains("Urgencia"))
                dgvPedidos.Columns["Urgencia"].Width = 80;

            lblConteo.Text = $"Mostrando {lista.Count} de {_pedidos.Count}  |  " +
                             $"🔴 Urgentes: {lista.Count(p => ComputarUrgencia(p).StartsWith("🔴"))}  " +
                             $"🟡 Normales: {lista.Count(p => ComputarUrgencia(p).StartsWith("🟡"))}";
            LimpiarDetalle();
        }

        /// <summary>
        /// Calcula el nivel de urgencia según el tiempo transcurrido desde la fecha del pedido.
        /// 🔴 Urgente: Pendiente > 3 días o Despachado > 5 días
        /// 🟡 Normal:  1–3 días pendiente
        /// 🟢 Reciente: menos de 1 día
        /// </summary>
        private string ComputarUrgencia(BE.Pedido p)
        {
            if (p.Estado == BE.EstadoPedido.Entregado || p.Estado == BE.EstadoPedido.Cancelado)
                return "—";

            double dias = (DateTime.Now - p.FechaPedido).TotalDays;

            if (p.Estado == BE.EstadoPedido.Pendiente)
            {
                if (dias > 3) return "🔴 Urgente";
                if (dias > 1) return "🟡 Normal";
                return "🟢 Reciente";
            }
            if (p.Estado == BE.EstadoPedido.Despachado)
            {
                double diasDespacho = p.FechaDespacho.HasValue
                    ? (DateTime.Now - p.FechaDespacho.Value).TotalDays : dias;
                if (diasDespacho > 5) return "🔴 Urgente";
                if (diasDespacho > 2) return "🟡 Normal";
                return "🟢 Reciente";
            }
            return "—";
        }

        private void ColorearFilas()
        {
            foreach (DataGridViewRow row in dgvPedidos.Rows)
            {
                string estado    = row.Cells["Estado"].Value?.ToString()   ?? "";
                string urgencia  = row.Cells["Urgencia"].Value?.ToString() ?? "";

                // Color de fondo por urgencia (solo pedidos activos)
                if (urgencia.StartsWith("🔴"))
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 225, 225);
                else if (urgencia.StartsWith("🟡"))
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 250, 210);

                // Color de texto por estado
                row.DefaultCellStyle.ForeColor = estado switch
                {
                    "Pendiente"  => Color.FromArgb(160, 100, 0),
                    "Despachado" => Color.FromArgb(30, 100, 170),
                    "Entregado"  => Color.FromArgb(30, 130, 30),
                    "Cancelado"  => Color.FromArgb(150, 50, 50),
                    _            => Color.Black
                };
            }
        }

        // ── Selección ─────────────────────────────────────────────────────────

        private void DgvPedidos_SelectionChanged(object sender, EventArgs e)
        {
            LimpiarDetalle();
            if (dgvPedidos.SelectedRows.Count == 0)
            {
                DeshabilitarBotones();
                return;
            }

            var pedido = ObtenerPedidoSeleccionado();
            if (pedido == null) { DeshabilitarBotones(); return; }

            btnDespachar.Enabled        = pedido.Estado == BE.EstadoPedido.Pendiente;
            btnEntregado.Enabled        = pedido.Estado == BE.EstadoPedido.Despachado;
            btnVerNotificacion.Enabled  = pedido.Estado == BE.EstadoPedido.Despachado ||
                                          pedido.Estado == BE.EstadoPedido.Entregado;

            CargarDetallePrendas(pedido);
        }

        private void CargarDetallePrendas(BE.Pedido pedidoResumen)
        {
            try
            {
                var pedido = pedidoBLL.ObtenerPorId(pedidoResumen.IdPedido);
                if (pedido == null) return;

                lblDetalleTitulo.Text =
                    $"Pedido #{pedido.IdPedido}  ·  {pedido.NombreCliente}  ·  " +
                    $"{EstadoLabel(pedido.Estado)}  ·  {pedido.CantidadPrendas} prenda(s)";

                var tabla = new DataTable();
                tabla.Columns.Add("Prenda",    typeof(string));
                tabla.Columns.Add("Categoría", typeof(string));
                tabla.Columns.Add("Talle",     typeof(string));
                tabla.Columns.Add("Color",     typeof(string));
                tabla.Columns.Add("Estado",    typeof(string));

                foreach (var p in pedido.Prendas)
                    tabla.Rows.Add(
                        p.Nombre,
                        p.Categoria ?? "—",
                        p.Talle     ?? "—",
                        p.Color     ?? "—",
                        p.Estado.ToString());

                dgvDetalle.DataSource = tabla;
            }
            catch { /* No interrumpir la UI si falla el detalle */ }
        }

        // ── Acciones ──────────────────────────────────────────────────────────

        private void BtnDespachar_Click(object sender, EventArgs e)
        {
            var pedido = ObtenerPedidoSeleccionado();
            if (pedido == null) return;

            var confirmar = MessageBox.Show(
                $"¿Despachar el Pedido #{pedido.IdPedido}?\n\n" +
                $"Cliente: {pedido.NombreCliente}\n" +
                $"Prendas: {pedido.CantidadPrendas}\n\n" +
                "El pedido pasará a estado Despachado.",
                "Confirmar Despacho",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);

            if (confirmar != DialogResult.Yes) return;

            try
            {
                pedidoBLL.Despachar(this, pedido);
                MostrarOk($"Pedido #{pedido.IdPedido} despachado correctamente.");
                CargarPedidos();
            }
            catch (Exception ex) { MostrarError(ex.Message); }
        }

        private void BtnEntregado_Click(object sender, EventArgs e)
        {
            var pedido = ObtenerPedidoSeleccionado();
            if (pedido == null) return;

            var confirmar = MessageBox.Show(
                $"¿Confirmar entrega del Pedido #{pedido.IdPedido} a {pedido.NombreCliente}?",
                "Confirmar Entrega",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);

            if (confirmar != DialogResult.Yes) return;

            try
            {
                pedidoBLL.MarcarEntregado(this, pedido);
                MostrarOk($"Pedido #{pedido.IdPedido} marcado como Entregado.");
                CargarPedidos();
            }
            catch (Exception ex) { MostrarError(ex.Message); }
        }

        private void BtnVerNotificacion_Click(object sender, EventArgs e)
        {
            var pedidoResumen = ObtenerPedidoSeleccionado();
            if (pedidoResumen == null) return;

            try
            {
                var pedido = pedidoBLL.ObtenerPorId(pedidoResumen.IdPedido);
                if (pedido == null) return;

                using (var notif = new NotificacionDespachoForm(pedido))
                    notif.ShowDialog(this);
            }
            catch (Exception ex) { MostrarError(ex.Message); }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private BE.Pedido ObtenerPedidoSeleccionado()
        {
            if (dgvPedidos.SelectedRows.Count == 0) return null;
            int id = Convert.ToInt32(dgvPedidos.SelectedRows[0].Cells["ID"].Value);
            return _pedidos.Find(p => p.IdPedido == id);
        }

        private void LimpiarDetalle()
        {
            dgvDetalle.DataSource = null;
            lblDetalleTitulo.Text = "Detalle del pedido seleccionado";
        }

        private void DeshabilitarBotones()
        {
            btnDespachar.Enabled       = false;
            btnEntregado.Enabled       = false;
            btnVerNotificacion.Enabled = false;
        }

        private string EstadoLabel(BE.EstadoPedido e)
        {
            switch (e)
            {
                case BE.EstadoPedido.Pendiente:  return "Pendiente";
                case BE.EstadoPedido.Despachado: return "Despachado";
                case BE.EstadoPedido.Entregado:  return "Entregado";
                case BE.EstadoPedido.Cancelado:  return "Cancelado";
                default: return e.ToString();
            }
        }

        private void MostrarOk(string msg)
        {
            lblMensaje.ForeColor = Color.DarkGreen;
            lblMensaje.Text      = $"✓ {msg}";
        }

        private void MostrarError(string msg)
        {
            lblMensaje.ForeColor = Color.DarkRed;
            lblMensaje.Text      = $"✗ {msg}";
        }
    }
}
