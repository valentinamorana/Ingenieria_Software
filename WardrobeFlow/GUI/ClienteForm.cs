using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GUI
{
    /// <summary>
    /// Diálogo modal para dar de alta o modificar un cliente.
    /// Se abre desde el formulario Clientes con ShowDialog().
    /// Devuelve DialogResult.OK con ClienteEditado cargado si el usuario confirma.
    /// </summary>
    public partial class ClienteForm : Form
    {
        // ── Resultado del diálogo ─────────────────────────────────────────────
        public BE.Cliente ClienteEditado { get; private set; }

        // ── Modo del formulario ───────────────────────────────────────────────
        private readonly bool _esEdicion;
        private readonly BE.Cliente _clienteOriginal;

        private List<BE.PlanSuscripcion> _planes;

        /// <summary>Constructor para ALTA (cliente nuevo).</summary>
        public ClienteForm() : this(null) { }

        /// <summary>
        /// Constructor para ALTA o EDICIÓN.
        /// Si cliente es null, modo alta; si tiene datos, modo edición.
        /// </summary>
        public ClienteForm(BE.Cliente cliente)
        {
            InitializeComponent();

            _esEdicion       = cliente != null;
            _clienteOriginal = cliente;

            this.Text = _esEdicion ? "Editar Cliente" : "Nuevo Cliente";
            btnGuardar.Text = _esEdicion ? "Guardar Cambios" : "Registrar Cliente";

            CargarPlanes();

            if (_esEdicion) CargarDatosExistentes();
        }

        // ── Eventos del Designer ──────────────────────────────────────────────

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // ── Carga de datos ────────────────────────────────────────────────────

        private void CargarPlanes()
        {
            try
            {
                var bllPlan = new BLL.PlanSuscripcion();
                _planes = bllPlan.ObtenerActivos();

                cmbPlan.Items.Clear();
                cmbPlan.Items.Add("— Sin plan —");
                foreach (var p in _planes)
                    cmbPlan.Items.Add($"{p.Nombre}  ({p.LimitePrendas} prendas — ${p.Precio:N2})");

                cmbPlan.SelectedIndex = 0;
            }
            catch
            {
                cmbPlan.Items.Add("— Error al cargar planes —");
                cmbPlan.SelectedIndex = 0;
            }
        }

        private void CargarDatosExistentes()
        {
            txtNombre.Text   = _clienteOriginal.Nombre;
            txtApellido.Text = _clienteOriginal.Apellido;
            txtDNI.Text      = _clienteOriginal.DNI;
            txtEmail.Text    = _clienteOriginal.Email ?? "";

            int idxPago = cmbMetodoPago.Items.IndexOf(_clienteOriginal.MetodoPago);
            cmbMetodoPago.SelectedIndex = idxPago >= 0 ? idxPago : 0;

            // Seleccionar plan actual
            if (_clienteOriginal.IdPlan.HasValue && _planes != null)
            {
                int planIdx = _planes.FindIndex(p => p.IdPlan == _clienteOriginal.IdPlan.Value);
                cmbPlan.SelectedIndex = planIdx >= 0 ? planIdx + 1 : 0;  // +1 por "Sin plan"
            }
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            lblMensaje.Text = string.Empty;

            // Validación de DNI en la UI antes de llamar a BLL
            string dniInput = txtDNI.Text.Trim();
            if (!string.IsNullOrWhiteSpace(dniInput))
            {
                foreach (char c in dniInput)
                {
                    if (!char.IsDigit(c))
                    {
                        lblMensaje.Text = "✗ El DNI solo puede contener números.";
                        txtDNI.Focus();
                        return;
                    }
                }
            }

            try
            {
                // Determinar plan seleccionado
                int? idPlan = null;
                if (cmbPlan.SelectedIndex > 0 && _planes != null && _planes.Count >= cmbPlan.SelectedIndex)
                    idPlan = _planes[cmbPlan.SelectedIndex - 1].IdPlan;

                ClienteEditado = new BE.Cliente
                {
                    IdCliente  = _esEdicion ? _clienteOriginal.IdCliente : 0,
                    Nombre     = txtNombre.Text.Trim(),
                    Apellido   = txtApellido.Text.Trim(),
                    DNI        = txtDNI.Text.Trim(),
                    Email      = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim(),
                    MetodoPago = cmbMetodoPago.SelectedItem?.ToString() ?? "Efectivo",
                    IdPlan     = idPlan,
                    FechaAlta  = _esEdicion ? _clienteOriginal.FechaAlta : DateTime.Now
                };

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"✗ {ex.Message}";
            }
        }
    }
}
