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

        // ── Controles ─────────────────────────────────────────────────────────
        private DataGridView dgvPedidos;
        private DataGridView dgvDetallePrendas;
        private Button       btnNuevoPedido;
        private Button       btnCancelar;
        private Button       btnRefrescar;
        private Label        lblMensaje;
        private Label        lblConteo;
        private Label        lblDetalleTitulo;

        private List<BE.Pedido> _pedidos = new List<BE.Pedido>();

        public PedidosVenta()
        {
            InitializeComponent();
            this.Text        = "Pedidos de Venta";
            this.ClientSize  = new Size(1000, 600);
            this.MinimumSize = new Size(820, 480);

            ConstruirInterfaz();
            this.Load += (s, e) => CargarPedidos();
        }

        private void ConstruirInterfaz()
        {
            // ── Panel superior: acciones ───────────────────────────────────────
            Panel panelTop = new Panel
            {
                Dock = DockStyle.Top, Height = 52,
                BackColor = Color.FromArgb(230, 230, 240),
                Padding = new Padding(8, 8, 8, 4)
            };

            btnNuevoPedido = new Button
            {
                Text = "+ Nuevo Pedido", Left = 8, Top = 11,
                Width = 140, Height = 28,
                BackColor = Color.FromArgb(60, 140, 60), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnNuevoPedido.FlatAppearance.BorderSize = 0;
            btnNuevoPedido.Click += BtnNuevoPedido_Click;

            btnCancelar = new Button
            {
                Text = "✕ Cancelar Pedido", Left = 156, Top = 11,
                Width = 140, Height = 28,
                BackColor = Color.FromArgb(200, 60, 60), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Enabled = false
            };
            btnCancelar.FlatAppearance.BorderSize = 0;
            btnCancelar.Click += BtnCancelarPedido_Click;

            btnRefrescar = new Button
            {
                Text = "↻", Left = 304, Top = 11,
                Width = 32, Height = 28, FlatStyle = FlatStyle.Flat
            };
            btnRefrescar.Click += (s, e) => CargarPedidos();

            lblConteo = new Label
            {
                Left = 344, Top = 16, Width = 300,
                ForeColor = Color.DimGray, Font = new Font("Segoe UI", 8.5f)
            };

            panelTop.Controls.AddRange(new Control[]
                { btnNuevoPedido, btnCancelar, btnRefrescar, lblConteo });

            // ── Panel inferior: detalle de prendas del pedido seleccionado ─────
            Panel panelDetalle = new Panel
            {
                Dock = DockStyle.Bottom, Height = 180,
                BackColor = Color.FromArgb(245, 245, 250),
                Padding = new Padding(8)
            };

            lblDetalleTitulo = new Label
            {
                Text = "Prendas del pedido seleccionado",
                Dock = DockStyle.Top, Height = 22,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Padding = new Padding(4, 2, 0, 0),
                BackColor = Color.FromArgb(215, 215, 230)
            };

            dgvDetallePrendas = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                BackgroundColor = Color.White,
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            panelDetalle.Controls.Add(dgvDetallePrendas);
            panelDetalle.Controls.Add(lblDetalleTitulo);

            // ── Status bar ────────────────────────────────────────────────────
            Panel panelStatus = new Panel
            {
                Dock = DockStyle.Bottom, Height = 26,
                BackColor = Color.FromArgb(230, 230, 240),
                Padding = new Padding(8, 4, 8, 4)
            };
            lblMensaje = new Label { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 8.5f) };
            panelStatus.Controls.Add(lblMensaje);

            // ── DataGridView de pedidos ───────────────────────────────────────
            dgvPedidos = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.None,
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(248, 248, 255)
                }
            };
            dgvPedidos.SelectionChanged += DgvPedidos_SelectionChanged;

            this.Controls.Add(dgvPedidos);
            this.Controls.Add(panelDetalle);
            this.Controls.Add(panelStatus);
            this.Controls.Add(panelTop);
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
                        p.FechaEntrega.HasValue  ? p.FechaEntrega.Value.ToString("dd/MM/yyyy")  : "—");
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

            if (!hay) { btnCancelar.Enabled = false; return; }

            var pedido = ObtenerPedidoSeleccionado();
            if (pedido == null) return;

            btnCancelar.Enabled = pedido.Estado == BE.EstadoPedido.Pendiente;

            // Cargar detalle de prendas del pedido seleccionado
            CargarDetallePrendas(pedido.IdPedido);

            lblDetalleTitulo.Text =
                $"Pedido #{pedido.IdPedido} — {pedido.NombreCliente} — {pedido.Estado}";
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

            var confirmar = MessageBox.Show(
                $"¿Cancelar el Pedido #{pedido.IdPedido} de {pedido.NombreCliente}?\n\n" +
                "Las prendas volverán a estado Disponible.",
                "Confirmar Cancelación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (confirmar != DialogResult.Yes) return;

            try
            {
                pedidoBLL.Cancelar(this, pedido);
                MostrarOk($"Pedido #{pedido.IdPedido} cancelado. Prendas liberadas.");
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
