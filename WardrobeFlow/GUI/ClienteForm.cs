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

        // ── Controles ─────────────────────────────────────────────────────────
        private TextBox  txtNombre;
        private TextBox  txtApellido;
        private TextBox  txtDNI;
        private TextBox  txtEmail;
        private ComboBox cmbMetodoPago;
        private ComboBox cmbPlan;
        private Button   btnGuardar;
        private Button   btnCancelar;
        private Label    lblMensaje;

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

            this.Text        = _esEdicion ? "Editar Cliente" : "Nuevo Cliente";
            this.ClientSize  = new Size(380, 460);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            ConstruirInterfaz();
            CargarPlanes();

            if (_esEdicion) CargarDatosExistentes();
        }

        private void ConstruirInterfaz()
        {
            int labelX = 16, controlX = 16, controlW = 340, top = 20, gap = 52;

            // ── Nombre ────────────────────────────────────────────────────────
            this.Controls.Add(new Label { Text = "Nombre *", Left = labelX, Top = top, Width = controlW });
            txtNombre = new TextBox { Left = controlX, Top = top + 18, Width = controlW };
            this.Controls.Add(txtNombre);

            // ── Apellido ──────────────────────────────────────────────────────
            top += gap;
            this.Controls.Add(new Label { Text = "Apellido *", Left = labelX, Top = top, Width = controlW });
            txtApellido = new TextBox { Left = controlX, Top = top + 18, Width = controlW };
            this.Controls.Add(txtApellido);

            // ── DNI ───────────────────────────────────────────────────────────
            top += gap;
            this.Controls.Add(new Label { Text = "DNI * (7-8 dígitos)", Left = labelX, Top = top, Width = controlW });
            txtDNI = new TextBox { Left = controlX, Top = top + 18, Width = controlW };
            this.Controls.Add(txtDNI);

            // ── Email ─────────────────────────────────────────────────────────
            top += gap;
            this.Controls.Add(new Label { Text = "Email", Left = labelX, Top = top, Width = controlW });
            txtEmail = new TextBox { Left = controlX, Top = top + 18, Width = controlW };
            this.Controls.Add(txtEmail);

            // ── Método de Pago ────────────────────────────────────────────────
            top += gap;
            this.Controls.Add(new Label { Text = "Método de Pago *", Left = labelX, Top = top, Width = controlW });
            cmbMetodoPago = new ComboBox
            {
                Left = controlX, Top = top + 18, Width = controlW,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbMetodoPago.Items.AddRange(new object[] { "Efectivo", "Débito", "Crédito", "Transferencia" });
            cmbMetodoPago.SelectedIndex = 0;
            this.Controls.Add(cmbMetodoPago);

            // ── Plan ──────────────────────────────────────────────────────────
            top += gap;
            this.Controls.Add(new Label { Text = "Plan de Suscripción", Left = labelX, Top = top, Width = controlW });
            cmbPlan = new ComboBox
            {
                Left = controlX, Top = top + 18, Width = controlW,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            this.Controls.Add(cmbPlan);

            // ── Mensaje de error ──────────────────────────────────────────────
            top += gap;
            lblMensaje = new Label
            {
                Left = labelX, Top = top, Width = controlW, Height = 36,
                ForeColor = Color.DarkRed,
                Font = new Font("Segoe UI", 8.5f)
            };
            this.Controls.Add(lblMensaje);

            // ── Botones ───────────────────────────────────────────────────────
            top += 44;
            btnGuardar = new Button
            {
                Text = _esEdicion ? "Guardar Cambios" : "Registrar Cliente",
                Left = controlX, Top = top, Width = 160, Height = 34,
                BackColor = Color.SteelBlue, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Click += BtnGuardar_Click;

            btnCancelar = new Button
            {
                Text = "Cancelar",
                Left = controlX + 176, Top = top, Width = 100, Height = 34,
                FlatStyle = FlatStyle.Flat
            };
            btnCancelar.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.Add(btnGuardar);
            this.Controls.Add(btnCancelar);

            // Ajustar ClientSize al contenido
            this.ClientSize = new Size(380, top + 52);
        }

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
