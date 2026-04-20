using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace GUI
{
    /// <summary>
    /// Capa de Presentación — Módulo de Gestión de Clientes.
    ///
    /// Permite al Vendedor administrar los clientes suscriptores del servicio:
    ///   ✓ Ver listado completo con plan y stock en uso
    ///   ✓ Registrar nuevo cliente
    ///   ✓ Editar datos de cliente existente
    ///   ✓ Dar de baja (solo si no tiene prendas en uso)
    ///   ✓ Filtrar por nombre/apellido/DNI
    ///
    /// Accesible desde Menú → Ventas → Clientes (permiso mnuClientes).
    /// </summary>
    public partial class Clientes : Form
    {
        private readonly BLL.Cliente clienteBLL = new BLL.Cliente();

        // ── Controles ─────────────────────────────────────────────────────────
        private DataGridView dgvClientes;
        private TextBox      txtFiltro;
        private Button       btnNuevo;
        private Button       btnEditar;
        private Button       btnBaja;
        private Button       btnRefrescar;
        private Label        lblMensaje;
        private Label        lblConteo;

        // Cache de la lista actual
        private List<BE.Cliente> _clientes = new List<BE.Cliente>();

        public Clientes()
        {
            InitializeComponent();
            this.Text        = "Gestión de Clientes";
            this.ClientSize  = new Size(920, 560);
            this.MinimumSize = new Size(780, 460);

            ConstruirInterfaz();
            this.Load += (s, e) => CargarClientes();
        }

        private void ConstruirInterfaz()
        {
            // ── Panel superior: filtro + acciones ─────────────────────────────
            Panel panelTop = new Panel
            {
                Dock = DockStyle.Top, Height = 52,
                Padding = new Padding(8, 8, 8, 4),
                BackColor = Color.FromArgb(230, 230, 240)
            };

            var lblFiltro = new Label
            {
                Text = "Buscar:", Left = 8, Top = 16,
                Width = 48, TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            txtFiltro = new TextBox { Left = 58, Top = 13, Width = 220 };
            txtFiltro.TextChanged += (s, e) => AplicarFiltro();

            btnNuevo = new Button
            {
                Text = "+ Nuevo Cliente", Left = 300, Top = 11,
                Width = 130, Height = 28,
                BackColor = Color.SteelBlue, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnNuevo.FlatAppearance.BorderSize = 0;
            btnNuevo.Click += BtnNuevo_Click;

            btnEditar = new Button
            {
                Text = "✎ Editar", Left = 438, Top = 11,
                Width = 90, Height = 28,
                FlatStyle = FlatStyle.Flat, Enabled = false
            };
            btnEditar.Click += BtnEditar_Click;

            btnBaja = new Button
            {
                Text = "✕ Dar de Baja", Left = 536, Top = 11,
                Width = 110, Height = 28,
                BackColor = Color.FromArgb(200, 60, 60), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Enabled = false
            };
            btnBaja.FlatAppearance.BorderSize = 0;
            btnBaja.Click += BtnBaja_Click;

            btnRefrescar = new Button
            {
                Text = "↻", Left = 654, Top = 11,
                Width = 32, Height = 28,
                FlatStyle = FlatStyle.Flat
            };
            btnRefrescar.Click += (s, e) => CargarClientes();

            lblConteo = new Label
            {
                Left = 696, Top = 16, Width = 200,
                ForeColor = Color.DimGray, Font = new Font("Segoe UI", 8.5f)
            };

            panelTop.Controls.AddRange(new Control[]
            {
                lblFiltro, txtFiltro, btnNuevo, btnEditar, btnBaja, btnRefrescar, lblConteo
            });

            // ── DataGridView ──────────────────────────────────────────────────
            dgvClientes = new DataGridView
            {
                Dock                            = DockStyle.Fill,
                ReadOnly                        = true,
                AllowUserToAddRows              = false,
                AllowUserToDeleteRows           = false,
                SelectionMode                   = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode             = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor                 = Color.White,
                RowHeadersVisible               = false,
                BorderStyle                     = BorderStyle.None,
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(248, 248, 255)
                }
            };
            dgvClientes.SelectionChanged += (s, e) =>
            {
                bool hay = dgvClientes.SelectedRows.Count > 0;
                btnEditar.Enabled = hay;
                btnBaja.Enabled   = hay;
            };
            dgvClientes.CellDoubleClick += (s, e) => BtnEditar_Click(s, e);

            // ── Barra de estado inferior ──────────────────────────────────────
            Panel panelBottom = new Panel
            {
                Dock = DockStyle.Bottom, Height = 28,
                BackColor = Color.FromArgb(230, 230, 240),
                Padding = new Padding(8, 4, 8, 4)
            };
            lblMensaje = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 8.5f)
            };
            panelBottom.Controls.Add(lblMensaje);

            this.Controls.Add(dgvClientes);
            this.Controls.Add(panelTop);
            this.Controls.Add(panelBottom);
        }

        // ── Carga y filtrado ──────────────────────────────────────────────────

        private void CargarClientes()
        {
            try
            {
                _clientes = clienteBLL.ObtenerTodos();
                AplicarFiltro();
                MostrarOk($"{_clientes.Count} cliente(s) registrado(s).");
            }
            catch (Exception ex)
            {
                MostrarError($"Error al cargar clientes: {ex.Message}");
            }
        }

        private void AplicarFiltro()
        {
            string filtro = txtFiltro.Text.Trim().ToLower();
            var lista = string.IsNullOrEmpty(filtro)
                ? _clientes
                : _clientes.FindAll(c =>
                    c.NombreCompleto.ToLower().Contains(filtro) ||
                    c.DNI.Contains(filtro) ||
                    (c.Email ?? "").ToLower().Contains(filtro));

            var tabla = new DataTable();
            tabla.Columns.Add("ID",         typeof(int));
            tabla.Columns.Add("Nombre",     typeof(string));
            tabla.Columns.Add("Apellido",   typeof(string));
            tabla.Columns.Add("DNI",        typeof(string));
            tabla.Columns.Add("Email",      typeof(string));
            tabla.Columns.Add("Plan",       typeof(string));
            tabla.Columns.Add("Prendas",    typeof(int));
            tabla.Columns.Add("MetodoPago", typeof(string));
            tabla.Columns.Add("Alta",       typeof(string));

            foreach (var c in lista)
            {
                tabla.Rows.Add(
                    c.IdCliente,
                    c.Nombre,
                    c.Apellido,
                    c.DNI,
                    c.Email ?? "—",
                    c.NombrePlan ?? "Sin plan",
                    c.StockUtilizado,
                    c.MetodoPago,
                    c.FechaAlta.ToString("dd/MM/yyyy"));
            }

            dgvClientes.DataSource = tabla;

            // Ajustar columnas
            if (dgvClientes.Columns.Contains("ID"))
                dgvClientes.Columns["ID"].Width = 40;

            lblConteo.Text = $"Mostrando {lista.Count} de {_clientes.Count}";
        }

        // ── Eventos de botones ────────────────────────────────────────────────

        private void BtnNuevo_Click(object sender, EventArgs e)
        {
            using (var form = new ClienteForm())
            {
                if (form.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    clienteBLL.Alta(this, form.ClienteEditado);
                    MostrarOk($"Cliente '{form.ClienteEditado.NombreCompleto}' registrado correctamente.");
                    CargarClientes();
                }
                catch (Exception ex)
                {
                    MostrarError(ex.Message);
                }
            }
        }

        private void BtnEditar_Click(object sender, EventArgs e)
        {
            var cliente = ObtenerClienteSeleccionado();
            if (cliente == null) return;

            using (var form = new ClienteForm(cliente))
            {
                if (form.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    // Preservar stock utilizado (no viene del form)
                    form.ClienteEditado.StockUtilizado = cliente.StockUtilizado;

                    clienteBLL.Modificar(this, form.ClienteEditado);
                    MostrarOk($"Cliente '{form.ClienteEditado.NombreCompleto}' actualizado.");
                    CargarClientes();
                }
                catch (Exception ex)
                {
                    MostrarError(ex.Message);
                }
            }
        }

        private void BtnBaja_Click(object sender, EventArgs e)
        {
            var cliente = ObtenerClienteSeleccionado();
            if (cliente == null) return;

            var confirmacion = MessageBox.Show(
                $"¿Dar de baja a {cliente.NombreCompleto} (DNI {cliente.DNI})?\n\n" +
                "Esta acción no se puede deshacer.",
                "Confirmar Baja",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (confirmacion != DialogResult.Yes) return;

            try
            {
                clienteBLL.Baja(this, cliente);
                MostrarOk($"Cliente '{cliente.NombreCompleto}' eliminado.");
                CargarClientes();
            }
            catch (Exception ex)
            {
                MostrarError(ex.Message);
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private BE.Cliente ObtenerClienteSeleccionado()
        {
            if (dgvClientes.SelectedRows.Count == 0) return null;
            int id = Convert.ToInt32(dgvClientes.SelectedRows[0].Cells["ID"].Value);
            return _clientes.Find(c => c.IdCliente == id);
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
