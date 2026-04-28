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
    /// <summary>
    /// Hereda de <see cref="FormBase"/>:
    ///   - MostrarError() → heredado, no se redeclara
    ///   - MensajeLabel → sobreescrito para devolver el lblMensaje de este formulario
    /// </summary>
    public partial class NuevoPedidoForm : FormBase
    {
        protected override Label MensajeLabel => lblMensaje;

        public int IdPedidoCreado { get; private set; }

        // ── BLL ───────────────────────────────────────────────────────────────
        private readonly BLL.Cliente  clienteBLL = new BLL.Cliente();
        private readonly BLL.Prenda   prendaBLL  = new BLL.Prenda();
        private readonly BLL.Pedido   pedidoBLL  = new BLL.Pedido();

        // ── Estado interno ────────────────────────────────────────────────────
        private List<BE.Cliente> _clientes    = new List<BE.Cliente>();
        private List<BE.Prenda>  _disponibles = new List<BE.Prenda>();
        private BE.Cliente       _clienteSel  = null;

        public NuevoPedidoForm()
        {
            InitializeComponent();
            this.Load += new EventHandler(NuevoPedidoForm_Load);
        }

        private void NuevoPedidoForm_Load(object sender, EventArgs e)
        {
            CargarDatosIniciales();
        }

        private void BtnVolver_Click(object sender, EventArgs e)
        {
            MostrarPaso(1);
        }

        private void DgvPrendas_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgvPrendas.IsCurrentCellDirty)
                dgvPrendas.CommitEdit(DataGridViewDataErrorContexts.Commit);
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
            int limite        = _clienteSel?.LimitePrendas  ?? 0;
            int total         = enUso + seleccionadas;
            int disponibles   = Math.Max(0, limite - enUso); // cuántas puede agregar

            string linea1 = $"Seleccionadas: {seleccionadas}  |  Ya en uso: {enUso}  |  Total: {total}";
            string linea2 = "";

            if (limite > 0)
            {
                if (total > limite)
                {
                    linea2 = $"✗ El plan '{_clienteSel?.NombrePlan}' permite {limite} prenda(s). " +
                             $"Estás superando el límite por {total - limite}.";
                    lblResumen.ForeColor  = Color.DarkRed;
                    btnConfirmar.Enabled  = false;
                }
                else if (seleccionadas == 0)
                {
                    linea2 = $"Podés agregar hasta {disponibles} prenda(s) (plan {_clienteSel?.NombrePlan}).";
                    lblResumen.ForeColor  = Color.DimGray;
                    btnConfirmar.Enabled  = false;
                }
                else if (seleccionadas < disponibles)
                {
                    linea2 = $"ℹ El plan '{_clienteSel?.NombrePlan}' permite {limite}. " +
                             $"Estás eligiendo {seleccionadas} de {disponibles} posibles — podés agregar más.";
                    lblResumen.ForeColor  = Color.FromArgb(140, 100, 0);
                    btnConfirmar.Enabled  = true;
                }
                else
                {
                    // seleccionadas == disponibles (límite exacto)
                    linea2 = $"✓ Alcanzás el máximo del plan '{_clienteSel?.NombrePlan}' ({limite} prendas).";
                    lblResumen.ForeColor  = Color.DarkGreen;
                    btnConfirmar.Enabled  = true;
                }
            }
            else
            {
                btnConfirmar.Enabled = seleccionadas > 0;
                lblResumen.ForeColor = Color.FromArgb(40, 80, 140);
            }

            lblResumen.Text = string.IsNullOrEmpty(linea2)
                ? linea1
                : linea1 + "\r\n" + linea2;
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

                IdPedidoCreado = pedidoBLL.CrearPedido(this, _clienteSel.IdCliente, prendas);

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

    }
}
