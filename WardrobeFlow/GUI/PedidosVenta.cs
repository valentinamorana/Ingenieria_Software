using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace GUI
{
    /// <summary>
    /// Capa de Presentación — Módulo de Pedidos de Venta.
    ///
    /// Permite al Vendedor:
    ///   ✓ Ver todos los pedidos realizados con su estado actual
    ///   ✓ Crear un nuevo pedido (abre NuevoPedidoForm)
    ///   ✓ Cancelar un pedido pendiente (libera prendas)
    ///   ✓ Ver detalle de prendas de cada pedido al seleccionarlo
    ///
    /// Accesible desde Menú → Ventas → Pedidos de Venta (permiso mnuPedidosVenta).
    /// </summary>
    public partial class PedidosVenta : Form
    {
        private readonly BLL.Pedido pedidoBLL = new BLL.Pedido();

        private List<BE.Pedido> _pedidos = new List<BE.Pedido>();

        public PedidosVenta()
        {
            InitializeComponent();
            this.Load += new EventHandler(PedidosVenta_Load);
        }

        private void PedidosVenta_Load(object sender, EventArgs e)
        {
            CargarPedidos();
        }

        private void BtnRefrescar_Click(object sender, EventArgs e)
        {
            CargarPedidos();
        }

        // ── Carga ─────────────────────────────────────────────────────────────

        private void CargarPedidos()
        {
            try
            {
                _pedidos = pedidoBLL.ObtenerTodos();
                var tabla = new DataTable();
                tabla.Columns.Add("ID",         typeof(int));
                tabla.Columns.Add("Fecha",      typeof(string));
                tabla.Columns.Add("Cliente",    typeof(string));
                tabla.Columns.Add("Vendedor",   typeof(string));
                tabla.Columns.Add("Prendas",    typeof(int));
                tabla.Columns.Add("Estado",     typeof(string));
                tabla.Columns.Add("Despacho",   typeof(string));
                tabla.Columns.Add("Entrega",    typeof(string));
                tabla.Columns.Add("Motivo",     typeof(string));

                foreach (var p in _pedidos)
                {
                    tabla.Rows.Add(
                        p.IdPedido,
                        p.FechaPedido.ToString("dd/MM/yyyy HH:mm"),
                        p.NombreCliente,
                        p.NombreEmpleado,
                        p.CantidadPrendas,
                        EstadoLabel(p.Estado),
                        p.FechaDespacho.HasValue ? p.FechaDespacho.Value.ToString("dd/MM/yyyy") : "—",
                        p.FechaEntrega.HasValue  ? p.FechaEntrega.Value.ToString("dd/MM/yyyy")  : "—",
                        p.MotivoCancelacion ?? "");
                }

                dgvPedidos.DataSource = tabla;
                ColorearFilasPedidos();

                if (dgvPedidos.Columns.Contains("ID"))
                    dgvPedidos.Columns["ID"].Width = 44;

                lblConteo.Text = $"{_pedidos.Count} pedido(s)";
                dgvDetallePrendas.DataSource = null;
                lblDetalleTitulo.Text = "Prendas del pedido seleccionado";

                MostrarOk($"{_pedidos.Count} pedido(s) cargado(s).");
            }
            catch (Exception ex)
            {
                MostrarError($"Error al cargar pedidos: {ex.Message}");
            }
        }

        private void ColorearFilasPedidos()
        {
            foreach (DataGridViewRow row in dgvPedidos.Rows)
            {
                string estado = row.Cells["Estado"].Value?.ToString() ?? "";
                row.DefaultCellStyle.ForeColor = estado switch
                {
                    "Pendiente"  => Color.FromArgb(160, 100, 0),
                    "Despachado" => Color.FromArgb(30, 100, 170),
                    "Entregado"  => Color.FromArgb(30, 130, 30),
                    "Cancelado"  => Color.FromArgb(160, 50, 50),
                    _            => Color.Black
                };
            }
        }

        private void DgvPedidos_SelectionChanged(object sender, EventArgs e)
        {
            bool hay = dgvPedidos.SelectedRows.Count > 0;
            dgvDetallePrendas.DataSource = null;

            if (!hay)
            {
                btnCancelar.Enabled    = false;
                btnDesCancelar.Enabled = false;
                return;
            }

            var pedido = ObtenerPedidoSeleccionado();
            if (pedido == null) return;

            btnCancelar.Enabled    = pedido.Estado == BE.EstadoPedido.Pendiente;
            btnDesCancelar.Enabled = pedido.Estado == BE.EstadoPedido.Cancelado;

            // Cargar detalle de prendas del pedido seleccionado
            CargarDetallePrendas(pedido.IdPedido);

            lblDetalleTitulo.Text =
                $"Pedido #{pedido.IdPedido} — {pedido.NombreCliente} — {EstadoLabel(pedido.Estado)}" +
                (!string.IsNullOrEmpty(pedido.MotivoCancelacion)
                    ? $"  |  Motivo: {pedido.MotivoCancelacion}" : "");
        }

        private void CargarDetallePrendas(int idPedido)
        {
            try
            {
                var pedidoCompleto = pedidoBLL.ObtenerPorId(idPedido);
                if (pedidoCompleto == null) return;

                var tabla = new DataTable();
                tabla.Columns.Add("Prenda",    typeof(string));
                tabla.Columns.Add("Categoría", typeof(string));
                tabla.Columns.Add("Talle",     typeof(string));
                tabla.Columns.Add("Color",     typeof(string));
                tabla.Columns.Add("Estado",    typeof(string));

                foreach (var p in pedidoCompleto.Prendas)
                    tabla.Rows.Add(p.Nombre, p.Categoria ?? "—",
                        p.Talle ?? "—", p.Color ?? "—", p.Estado.ToString());

                dgvDetallePrendas.DataSource = tabla;
            }
            catch { /* No interrumpir si el detalle falla */ }
        }

        // ── Eventos ───────────────────────────────────────────────────────────

        private void BtnNuevoPedido_Click(object sender, EventArgs e)
        {
            using (var form = new NuevoPedidoForm())
            {
                if (form.ShowDialog(this) != DialogResult.OK) return;

                MostrarOk($"Pedido #{form.IdPedidoCreado} creado exitosamente. Estado: Pendiente.");
                CargarPedidos();
            }
        }

        private void BtnCancelarPedido_Click(object sender, EventArgs e)
        {
            var pedido = ObtenerPedidoSeleccionado();
            if (pedido == null) return;

            // Pedir motivo de cancelación con un dialog inline
            string motivo = PedirTexto(
                $"Motivo de cancelación del Pedido #{pedido.IdPedido} ({pedido.NombreCliente}):",
                "Motivo de Cancelación");

            if (string.IsNullOrWhiteSpace(motivo))
            {
                MostrarError("La cancelación requiere un motivo.");
                return;
            }

            var confirmar = MessageBox.Show(
                $"¿Cancelar el Pedido #{pedido.IdPedido} de {pedido.NombreCliente}?\n\n" +
                $"Motivo: {motivo}\n\nLas prendas volverán a estado Disponible.",
                "Confirmar Cancelación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (confirmar != DialogResult.Yes) return;

            try
            {
                pedidoBLL.Cancelar(this, pedido, motivo);
                MostrarOk($"Pedido #{pedido.IdPedido} cancelado. Prendas liberadas.");
                CargarPedidos();
            }
            catch (Exception ex)
            {
                MostrarError(ex.Message);
            }
        }

        private void BtnDesCancelarPedido_Click(object sender, EventArgs e)
        {
            var pedido = ObtenerPedidoSeleccionado();
            if (pedido == null) return;

            var confirmar = MessageBox.Show(
                $"¿Des-cancelar el Pedido #{pedido.IdPedido} de {pedido.NombreCliente}?\n\n" +
                "Se verificará que las prendas originales estén disponibles\n" +
                "y el pedido volverá a estado Pendiente.",
                "Confirmar Des-cancelación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);

            if (confirmar != DialogResult.Yes) return;

            try
            {
                pedidoBLL.DesCancelar(this, pedido);
                MostrarOk($"Pedido #{pedido.IdPedido} reactivado — volvió a Pendiente.");
                CargarPedidos();
            }
            catch (Exception ex)
            {
                MostrarError(ex.Message);
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private BE.Pedido ObtenerPedidoSeleccionado()
        {
            if (dgvPedidos.SelectedRows.Count == 0) return null;
            int id = Convert.ToInt32(dgvPedidos.SelectedRows[0].Cells["ID"].Value);
            return _pedidos.Find(p => p.IdPedido == id);
        }

        private string EstadoLabel(BE.EstadoPedido estado)
        {
            switch (estado)
            {
                case BE.EstadoPedido.Pendiente:  return "Pendiente";
                case BE.EstadoPedido.Despachado: return "Despachado";
                case BE.EstadoPedido.Entregado:  return "Entregado";
                case BE.EstadoPedido.Cancelado:  return "Cancelado";
                default: return estado.ToString();
            }
        }

        /// <summary>
        /// Muestra un dialog simple para pedir texto al usuario.
        /// Devuelve null si cancela o deja vacío.
        /// </summary>
        private string PedirTexto(string prompt, string titulo)
        {
            string resultado = null;
            using (var dlg = new Form())
            {
                dlg.Text            = titulo;
                dlg.ClientSize      = new Size(420, 130);
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox     = false;
                dlg.MinimizeBox     = false;
                dlg.StartPosition   = FormStartPosition.CenterParent;

                dlg.Controls.Add(new Label
                {
                    Text = prompt, Left = 12, Top = 12,
                    Width = 396, Height = 36,
                    Font = new Font("Segoe UI", 9f)
                });

                var txt = new TextBox { Left = 12, Top = 52, Width = 396 };
                dlg.Controls.Add(txt);

                var btnOk = new Button
                {
                    Text = "Aceptar", Left = 220, Top = 84,
                    Width = 90, Height = 30,
                    DialogResult = DialogResult.OK,
                    BackColor = Color.SteelBlue, ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                btnOk.FlatAppearance.BorderSize = 0;

                var btnCancel = new Button
                {
                    Text = "Cancelar", Left = 318, Top = 84,
                    Width = 90, Height = 30,
                    DialogResult = DialogResult.Cancel,
                    FlatStyle = FlatStyle.Flat
                };

                dlg.Controls.Add(btnOk);
                dlg.Controls.Add(btnCancel);
                dlg.AcceptButton = btnOk;
                dlg.CancelButton = btnCancel;

                if (dlg.ShowDialog(this) == DialogResult.OK)
                    resultado = txt.Text.Trim();
            }
            return resultado;
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
