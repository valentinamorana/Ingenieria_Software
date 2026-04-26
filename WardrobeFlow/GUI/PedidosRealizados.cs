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

        // ── Controles ─────────────────────────────────────────────────────────
        private DataGridView dgvPedidos;
        private DataGridView dgvDetalle;
        private ComboBox      cmbFiltroEstado;
        private NumericUpDown nudDiasFiltro;
        private Button        btnDespachar;
        private Button        btnEntregado;
        private Button        btnVerNotificacion;
        private Button        btnRefrescar;
        private Label         lblMensaje;
        private Label         lblConteo;
        private Label         lblDetalleTitulo;

        private List<BE.Pedido> _pedidos = new List<BE.Pedido>();

        public PedidosRealizados()
        {
            InitializeComponent();
            this.Text        = "Despacho de Pedidos";
            this.ClientSize  = new Size(1060, 660);
            this.MinimumSize = new Size(880, 520);

            ConstruirInterfaz();
            this.Load += (s, e) => CargarPedidos();
        }

        private void ConstruirInterfaz()
        {
            // ── Panel superior (2 filas: filtros + acciones) ──────────────────
            Panel panelTop = new Panel
            {
                Dock = DockStyle.Top, Height = 90,
                BackColor = Color.FromArgb(225, 235, 245),
                Padding = new Padding(8, 6, 8, 4)
            };

            // Fila 1: Filtros
            panelTop.Controls.Add(new Label
            {
                Text = "Estado:", Left = 8, Top = 10, Width = 50,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            });

            cmbFiltroEstado = new ComboBox
            {
                Left = 60, Top = 8, Width = 140,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbFiltroEstado.Items.AddRange(new object[]
                { "Todos", "Pendiente", "Despachado", "Entregado", "Cancelado" });
            cmbFiltroEstado.SelectedIndex = 0;
            cmbFiltroEstado.SelectedIndexChanged += (s, e) => AplicarFiltro();
            panelTop.Controls.Add(cmbFiltroEstado);

            panelTop.Controls.Add(new Label
            {
                Text = "Últimos:", Left = 212, Top = 10, Width = 56,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            });

            nudDiasFiltro = new NumericUpDown
            {
                Left = 270, Top = 8, Width = 60,
                Minimum = 0, Maximum = 365, Value = 0
            };
            nudDiasFiltro.ValueChanged += (s, e) => AplicarFiltro();
            panelTop.Controls.Add(nudDiasFiltro);

            panelTop.Controls.Add(new Label
            {
                Text = "días (0 = todos)", Left = 334, Top = 10, Width = 120,
                ForeColor = Color.DimGray, Font = new Font("Segoe UI", 8f)
            });

            lblConteo = new Label
            {
                Left = 460, Top = 10, Width = 380,
                ForeColor = Color.DimGray, Font = new Font("Segoe UI", 8.5f),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            panelTop.Controls.Add(lblConteo);

            // Fila 2: Acciones
            btnDespachar = new Button
            {
                Text = "📦 Despachar", Left = 8, Top = 50,
                Width = 130, Height = 28,
                BackColor = Color.FromArgb(210, 100, 135), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Enabled = false
            };
            btnDespachar.FlatAppearance.BorderSize = 0;
            btnDespachar.Click += BtnDespachar_Click;
            panelTop.Controls.Add(btnDespachar);

            btnEntregado = new Button
            {
                Text = "✓ Marcar Entregado", Left = 146, Top = 50,
                Width = 160, Height = 28,
                BackColor = Color.FromArgb(40, 140, 60), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Enabled = false
            };
            btnEntregado.FlatAppearance.BorderSize = 0;
            btnEntregado.Click += BtnEntregado_Click;
            panelTop.Controls.Add(btnEntregado);

            btnVerNotificacion = new Button
            {
                Text = "✉ Ver Notificación", Left = 314, Top = 50,
                Width = 150, Height = 28,
                FlatStyle = FlatStyle.Flat, Enabled = false
            };
            btnVerNotificacion.Click += BtnVerNotificacion_Click;
            panelTop.Controls.Add(btnVerNotificacion);

            btnRefrescar = new Button
            {
                Text = "↻", Left = 472, Top = 50,
                Width = 32, Height = 28, FlatStyle = FlatStyle.Flat
            };
            btnRefrescar.Click += (s, e) => CargarPedidos();
            panelTop.Controls.Add(btnRefrescar);

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
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(255, 248, 252)
                },
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    SelectionBackColor = Color.FromArgb(255, 182, 193),
                    SelectionForeColor = Color.Black
                }
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
                    BackColor = Color.FromArgb(255, 248, 252)
                },
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    SelectionBackColor = Color.FromArgb(255, 182, 193),
                    SelectionForeColor = Color.Black
                }
            };
            dgvPedidos.SelectionChanged += DgvPedidos_SelectionChanged;

            this.Controls.Add(dgvPedidos);
            this.Controls.Add(panelDetalle);
            this.Controls.Add(panelStatus);
            this.Controls.Add(panelTop);
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
