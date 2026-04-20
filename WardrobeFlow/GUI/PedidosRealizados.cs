using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
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

        // ── Controles ─────────────────────────────────────────────────────────
        private DataGridView dgvPedidos;
        private DataGridView dgvDetalle;
        private ComboBox     cmbFiltroEstado;
        private Button       btnDespachar;
        private Button       btnEntregado;
        private Button       btnVerNotificacion;
        private Button       btnRefrescar;
        private Label        lblMensaje;
        private Label        lblConteo;
        private Label        lblDetalleTitulo;

        private List<BE.Pedido> _pedidos = new List<BE.Pedido>();

        public PedidosRealizados()
        {
            InitializeComponent();
            this.Text        = "Pedidos Realizados";
            this.ClientSize  = new Size(1020, 620);
            this.MinimumSize = new Size(860, 500);

            ConstruirInterfaz();
            this.Load += (s, e) => CargarPedidos();
        }

        private void ConstruirInterfaz()
        {
            // ── Panel superior ────────────────────────────────────────────────
            Panel panelTop = new Panel
            {
                Dock = DockStyle.Top, Height = 56,
                BackColor = Color.FromArgb(225, 235, 245),
                Padding = new Padding(8, 8, 8, 4)
            };

            panelTop.Controls.Add(new Label
            {
                Text = "Estado:", Left = 8, Top = 18, Width = 50,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            });

            cmbFiltroEstado = new ComboBox
            {
                Left = 60, Top = 15, Width = 140,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbFiltroEstado.Items.AddRange(new object[]
                { "Todos", "Pendiente", "Despachado", "Entregado", "Cancelado" });
            cmbFiltroEstado.SelectedIndex = 0;
            cmbFiltroEstado.SelectedIndexChanged += (s, e) => AplicarFiltro();
            panelTop.Controls.Add(cmbFiltroEstado);

            btnDespachar = new Button
            {
                Text = "📦 Despachar", Left = 214, Top = 13,
                Width = 130, Height = 28,
                BackColor = Color.FromArgb(30, 110, 180), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Enabled = false
            };
            btnDespachar.FlatAppearance.BorderSize = 0;
            btnDespachar.Click += BtnDespachar_Click;
            panelTop.Controls.Add(btnDespachar);

            btnEntregado = new Button
            {
                Text = "✓ Marcar Entregado", Left = 352, Top = 13,
                Width = 160, Height = 28,
                BackColor = Color.FromArgb(40, 140, 60), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Enabled = false
            };
            btnEntregado.FlatAppearance.BorderSize = 0;
            btnEntregado.Click += BtnEntregado_Click;
            panelTop.Controls.Add(btnEntregado);

            btnVerNotificacion = new Button
            {
                Text = "✉ Ver Notificación", Left = 520, Top = 13,
                Width = 150, Height = 28,
                FlatStyle = FlatStyle.Flat, Enabled = false
            };
            btnVerNotificacion.Click += BtnVerNotificacion_Click;
            panelTop.Controls.Add(btnVerNotificacion);

            btnRefrescar = new Button
            {
                Text = "↻", Left = 678, Top = 13,
                Width = 32, Height = 28, FlatStyle = FlatStyle.Flat
            };
            btnRefrescar.Click += (s, e) => CargarPedidos();
            panelTop.Controls.Add(btnRefrescar);

            lblConteo = new Label
            {
                Left = 718, Top = 18, Width = 260,
                ForeColor = Color.DimGray, Font = new Font("Segoe UI", 8.5f)
            };
            panelTop.Controls.Add(lblConteo);

            // ── Panel inferior: detalle prendas ───────────────────────────────
            Panel panelDetalle = new Panel
            {
                Dock = DockStyle.Bottom, Height = 190,
                BackColor = Color.FromArgb(248, 248, 252),
                Padding = new Padding(8)
            };

            lblDetalleTitulo = new Label
            {
                Text = "Detalle del pedido seleccionado",
                Dock = DockStyle.Top, Height = 22,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Padding = new Padding(4, 2, 0, 0),
                BackColor = Color.FromArgb(210, 220, 235)
            };

            dgvDetalle = new DataGridView
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

            panelDetalle.Controls.Add(dgvDetalle);
            panelDetalle.Controls.Add(lblDetalleTitulo);

            // ── Status bar ────────────────────────────────────────────────────
            Panel panelStatus = new Panel
            {
                Dock = DockStyle.Bottom, Height = 26,
                BackColor = Color.FromArgb(225, 235, 245),
                Padding = new Padding(8, 4, 8, 4)
            };
            lblMensaje = new Label { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 8.5f) };
            panelStatus.Controls.Add(lblMensaje);

            // ── DataGridView principal ────────────────────────────────────────
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
                    BackColor = Color.FromArgb(246, 248, 255)
                }
            };
            dgvPedidos.SelectionChanged += DgvPedidos_SelectionChanged;

            this.Controls.Add(dgvPedidos);
            this.Controls.Add(panelDetalle);
            this.Controls.Add(panelStatus);
            this.Controls.Add(panelTop);
        }

        // ── Carga y filtrado ──────────────────────────────────────────────────

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
            int idx = cmbFiltroEstado.SelectedIndex;

            var lista = _pedidos.FindAll(p =>
                idx == 0 ||
                (idx == 1 && p.Estado == BE.EstadoPedido.Pendiente)  ||
                (idx == 2 && p.Estado == BE.EstadoPedido.Despachado) ||
                (idx == 3 && p.Estado == BE.EstadoPedido.Entregado)  ||
                (idx == 4 && p.Estado == BE.EstadoPedido.Cancelado));

            var tabla = new DataTable();
            tabla.Columns.Add("ID",        typeof(int));
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

            lblConteo.Text = $"Mostrando {lista.Count} de {_pedidos.Count}";
            LimpiarDetalle();
        }

        private void ColorearFilas()
        {
            foreach (DataGridViewRow row in dgvPedidos.Rows)
            {
                string estado = row.Cells["Estado"].Value?.ToString() ?? "";
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
