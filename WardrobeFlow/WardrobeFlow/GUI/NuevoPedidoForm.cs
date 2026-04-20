using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace GUI
{
    /// <summary>
    /// Formulario de creación de Pedido de Venta.
    ///
    /// Flujo en 2 pasos visuales dentro del mismo form:
    ///   PASO 1 — Seleccionar cliente
    ///   PASO 2 — Seleccionar prendas disponibles (respeta límite del plan)
    ///
    /// Devuelve DialogResult.OK cuando el pedido fue creado exitosamente.
    /// El ID del pedido creado queda en IdPedidoCreado.
    /// </summary>
    public partial class NuevoPedidoForm : Form
    {
        public int IdPedidoCreado { get; private set; }

        // ── BLL ───────────────────────────────────────────────────────────────
        private readonly BLL.Cliente  clienteBLL = new BLL.Cliente();
        private readonly BLL.Prenda   prendaBLL  = new BLL.Prenda();
        private readonly BLL.Pedido   pedidoBLL  = new BLL.Pedido();

        // ── Estado interno ────────────────────────────────────────────────────
        private List<BE.Cliente> _clientes  = new List<BE.Cliente>();
        private List<BE.Prenda>  _disponibles = new List<BE.Prenda>();
        private BE.Cliente       _clienteSel  = null;

        // ── Controles ─────────────────────────────────────────────────────────
        // Paso 1
        private Panel        panelPaso1;
        private ComboBox     cmbCliente;
        private Label        lblInfoPlan;
        private Button       btnSiguiente;

        // Paso 2
        private Panel        panelPaso2;
        private DataGridView dgvPrendas;
        private Label        lblResumen;
        private Button       btnConfirmar;
        private Button       btnVolver;

        // Compartidos
        private Label        lblPaso;
        private Label        lblMensaje;

        public NuevoPedidoForm()
        {
            InitializeComponent();
            this.Text            = "Nuevo Pedido de Venta";
            this.ClientSize      = new Size(700, 520);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.StartPosition   = FormStartPosition.CenterParent;

            ConstruirInterfaz();
            this.Load += (s, e) => CargarDatosIniciales();
        }

        private void ConstruirInterfaz()
        {
            // ── Header ────────────────────────────────────────────────────────
            var panelHeader = new Panel
            {
                Dock = DockStyle.Top, Height = 40,
                BackColor = Color.FromArgb(60, 100, 160)
            };
            lblPaso = new Label
            {
                Text = "Paso 1 de 2 — Seleccionar Cliente",
                Dock = DockStyle.Fill,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Padding = new Padding(12, 8, 0, 0)
            };
            panelHeader.Controls.Add(lblPaso);

            // ── Status bar ────────────────────────────────────────────────────
            var panelStatus = new Panel
            {
                Dock = DockStyle.Bottom, Height = 26,
                BackColor = Color.FromArgb(230, 230, 240),
                Padding = new Padding(8, 4, 8, 4)
            };
            lblMensaje = new Label { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 8.5f) };
            panelStatus.Controls.Add(lblMensaje);

            // ── PASO 1: Seleccionar cliente ───────────────────────────────────
            panelPaso1 = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            panelPaso1.Controls.Add(new Label
            {
                Text = "Seleccioná el cliente para este pedido:",
                Left = 20, Top = 20, Width = 600,
                Font = new Font("Segoe UI", 10)
            });

            cmbCliente = new ComboBox
            {
                Left = 20, Top = 50, Width = 500,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            cmbCliente.SelectedIndexChanged += CmbCliente_SelectedIndexChanged;
            panelPaso1.Controls.Add(cmbCliente);

            lblInfoPlan = new Label
            {
                Left = 20, Top = 90, Width = 620, Height = 120,
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = Color.FromArgb(40, 80, 140),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(240, 246, 255),
                Padding = new Padding(10),
                Visible = false
            };
            panelPaso1.Controls.Add(lblInfoPlan);

            btnSiguiente = new Button
            {
                Text = "Siguiente →",
                Left = 20, Top = 240,
                Width = 160, Height = 36,
                BackColor = Color.SteelBlue, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Enabled = false,
                Font = new Font("Segoe UI", 10)
            };
            btnSiguiente.FlatAppearance.BorderSize = 0;
            btnSiguiente.Click += BtnSiguiente_Click;
            panelPaso1.Controls.Add(btnSiguiente);

            // ── PASO 2: Seleccionar prendas ───────────────────────────────────
            panelPaso2 = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10), Visible = false };

            var lblInstruccion = new Label
            {
                Text = "Seleccioná las prendas para incluir en el pedido (checkbox):",
                Left = 10, Top = 4, Width = 660,
                Font = new Font("Segoe UI", 9.5f)
            };
            panelPaso2.Controls.Add(lblInstruccion);

            dgvPrendas = new DataGridView
            {
                Left = 10, Top = 26, Width = 660, Height = 340,
                ReadOnly = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.FixedSingle,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(248, 248, 255)
                }
            };

            // Columna checkbox
            var colCheck = new DataGridViewCheckBoxColumn
            {
                Name = "Sel", HeaderText = "", Width = 32,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            };
            var colId       = new DataGridViewTextBoxColumn { Name = "ID",       HeaderText = "ID",        ReadOnly = true, Width = 40, AutoSizeMode = DataGridViewAutoSizeColumnMode.None };
            var colNombre   = new DataGridViewTextBoxColumn { Name = "Nombre",   HeaderText = "Nombre",    ReadOnly = true };
            var colCateg    = new DataGridViewTextBoxColumn { Name = "Categoria",HeaderText = "Categoría", ReadOnly = true };
            var colTalle    = new DataGridViewTextBoxColumn { Name = "Talle",    HeaderText = "Talle",     ReadOnly = true, Width = 60,  AutoSizeMode = DataGridViewAutoSizeColumnMode.None };
            var colColor    = new DataGridViewTextBoxColumn { Name = "Color",    HeaderText = "Color",     ReadOnly = true, Width = 90,  AutoSizeMode = DataGridViewAutoSizeColumnMode.None };

            dgvPrendas.Columns.AddRange(colCheck, colId, colNombre, colCateg, colTalle, colColor);
            dgvPrendas.CellValueChanged      += DgvPrendas_CellValueChanged;
            dgvPrendas.CurrentCellDirtyStateChanged += (s, e) =>
            {
                if (dgvPrendas.IsCurrentCellDirty) dgvPrendas.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
            panelPaso2.Controls.Add(dgvPrendas);

            lblResumen = new Label
            {
                Left = 10, Top = 374, Width = 500, Height = 40,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 80, 140)
            };
            panelPaso2.Controls.Add(lblResumen);

            btnVolver = new Button
            {
                Text = "← Volver",
                Left = 10, Top = 420, Width = 110, Height = 34,
                FlatStyle = FlatStyle.Flat
            };
            btnVolver.Click += (s, e) => MostrarPaso(1);
            panelPaso2.Controls.Add(btnVolver);

            btnConfirmar = new Button
            {
                Text = "✓ Confirmar Pedido",
                Left = 130, Top = 420, Width = 180, Height = 34,
                BackColor = Color.FromArgb(60, 140, 60), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Enabled = false
            };
            btnConfirmar.FlatAppearance.BorderSize = 0;
            btnConfirmar.Click += BtnConfirmar_Click;
            panelPaso2.Controls.Add(btnConfirmar);

            // Agregar paneles al form
            this.Controls.Add(panelPaso2);
            this.Controls.Add(panelPaso1);
            this.Controls.Add(panelStatus);
            this.Controls.Add(panelHeader);
        }

        // ── Carga de datos ────────────────────────────────────────────────────

        private void CargarDatosIniciales()
        {
            try
            {
                _clientes = clienteBLL.ObtenerTodos();
                cmbCliente.Items.Clear();
                cmbCliente.Items.Add("— Seleccioná un cliente —");
                foreach (var c in _clientes)
                    cmbCliente.Items.Add($"{c.NombreCompleto}  (DNI {c.DNI})");
                cmbCliente.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MostrarError($"Error al cargar clientes: {ex.Message}");
            }
        }

        private void CargarPrendasDisponibles()
        {
            try
            {
                _disponibles = prendaBLL.ObtenerDisponibles();
                dgvPrendas.Rows.Clear();

                foreach (var p in _disponibles)
                {
                    dgvPrendas.Rows.Add(false, p.IdPrenda, p.Nombre,
                        p.Categoria ?? "—", p.Talle ?? "—", p.Color ?? "—");
                }

                ActualizarResumen();
            }
            catch (Exception ex)
            {
                MostrarError($"Error al cargar prendas: {ex.Message}");
            }
        }

        // ── Navegación entre pasos ────────────────────────────────────────────

        private void MostrarPaso(int paso)
        {
            panelPaso1.Visible = paso == 1;
            panelPaso2.Visible = paso == 2;
            lblPaso.Text = paso == 1
                ? "Paso 1 de 2 — Seleccionar Cliente"
                : $"Paso 2 de 2 — Seleccionar Prendas  ({_clienteSel?.NombreCompleto})";
            lblMensaje.Text = string.Empty;

            if (paso == 2) CargarPrendasDisponibles();
        }

        // ── Eventos ───────────────────────────────────────────────────────────

        private void CmbCliente_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = cmbCliente.SelectedIndex - 1;
            if (idx < 0)
            {
                lblInfoPlan.Visible  = false;
                btnSiguiente.Enabled = false;
                _clienteSel          = null;
                return;
            }

            _clienteSel = _clientes[idx];
            btnSiguiente.Enabled = true;

            // Mostrar info del plan
            if (_clienteSel.IdPlan.HasValue)
            {
                int disponibles = _clienteSel.IdPlan.HasValue
                    ? 99  // se calcula abajo
                    : 0;

                lblInfoPlan.Text =
                    $"Cliente: {_clienteSel.NombreCompleto}\n" +
                    $"Plan: {_clienteSel.NombrePlan ?? "—"}\n" +
                    $"Prendas en uso actualmente: {_clienteSel.StockUtilizado}\n" +
                    $"Método de pago: {_clienteSel.MetodoPago}\n" +
                    $"Alta: {_clienteSel.FechaAlta:dd/MM/yyyy}";
            }
            else
            {
                lblInfoPlan.Text = $"⚠ {_clienteSel.NombreCompleto} no tiene plan asignado.\n" +
                                   "Asigná un plan en el módulo de Clientes antes de crear un pedido.";
                lblInfoPlan.ForeColor = Color.DarkRed;
                btnSiguiente.Enabled  = false;
            }

            lblInfoPlan.ForeColor = _clienteSel.IdPlan.HasValue
                ? Color.FromArgb(40, 80, 140) : Color.DarkRed;
            lblInfoPlan.Visible = true;
        }

        private void BtnSiguiente_Click(object sender, EventArgs e)
        {
            if (_clienteSel == null) return;
            MostrarPaso(2);
        }

        private void DgvPrendas_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dgvPrendas.Columns["Sel"].Index)
                ActualizarResumen();
        }

        private void ActualizarResumen()
        {
            int seleccionadas = ContarSeleccionadas();
            int enUso         = _clienteSel?.StockUtilizado ?? 0;
            int total         = enUso + seleccionadas;

            string limite = _clienteSel?.IdPlan.HasValue == true ? "—" : "—";

            // Intentar obtener límite del plan
            lblResumen.Text = $"Seleccionadas: {seleccionadas}  |  Ya en uso: {enUso}  |  Total: {total}";

            btnConfirmar.Enabled = seleccionadas > 0;

            // Colorear aviso si hay muchas seleccionadas
            lblResumen.ForeColor = seleccionadas > 0
                ? Color.FromArgb(40, 80, 140)
                : Color.DimGray;
        }

        private int ContarSeleccionadas()
        {
            int count = 0;
            foreach (DataGridViewRow row in dgvPrendas.Rows)
            {
                if (row.Cells["Sel"].Value is bool sel && sel) count++;
            }
            return count;
        }

        private List<BE.Prenda> ObtenerPrendasSeleccionadas()
        {
            var lista = new List<BE.Prenda>();
            foreach (DataGridViewRow row in dgvPrendas.Rows)
            {
                if (row.Cells["Sel"].Value is bool sel && sel)
                {
                    int id = Convert.ToInt32(row.Cells["ID"].Value);
                    var p = _disponibles.Find(pr => pr.IdPrenda == id);
                    if (p != null) lista.Add(p);
                }
            }
            return lista;
        }

        private void BtnConfirmar_Click(object sender, EventArgs e)
        {
            var prendas = ObtenerPrendasSeleccionadas();
            if (prendas.Count == 0)
            {
                MostrarError("Seleccioná al menos una prenda.");
                return;
            }

            // Confirmación final
            string detallesPrendas = string.Join("\n  • ",
                prendas.ConvertAll(p => $"{p.Nombre} ({p.Talle} — {p.Color})"));

            var confirmar = MessageBox.Show(
                $"Confirmar pedido para {_clienteSel.NombreCompleto}:\n\n" +
                $"  • {detallesPrendas}\n\n" +
                $"Total: {prendas.Count} prenda(s)\n" +
                $"Método de pago: {_clienteSel.MetodoPago}",
                "Confirmar Pedido",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);

            if (confirmar != DialogResult.Yes) return;

            try
            {
                btnConfirmar.Enabled = false;
                btnConfirmar.Text    = "Procesando...";

                IdPedidoCreado = pedidoBLL.Alta(this, _clienteSel.IdCliente, prendas);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                btnConfirmar.Enabled = true;
                btnConfirmar.Text    = "✓ Confirmar Pedido";
                MostrarError(ex.Message);
            }
        }

        private void MostrarError(string msg)
        {
            lblMensaje.ForeColor = Color.DarkRed;
            lblMensaje.Text      = $"✗ {msg}";
        }
    }
}
