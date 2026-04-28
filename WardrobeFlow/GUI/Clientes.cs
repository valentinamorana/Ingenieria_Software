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
    /// <summary>
    /// Hereda de <see cref="FormBase"/>:
    ///   - MostrarOk() y MostrarError() → heredados, no se redeclaran
    ///   - MensajeLabel → sobreescrito para devolver el lblMensaje de este formulario
    /// </summary>
    public partial class Clientes : FormBase
    {
        protected override Label MensajeLabel => lblMensaje;

        private readonly BLL.Cliente clienteBLL = new BLL.Cliente();

        // Cache de la lista actual
        private List<BE.Cliente> _clientes = new List<BE.Cliente>();

        public Clientes()
        {
            InitializeComponent();
            this.Load += new EventHandler(Clientes_Load);
        }

        // ── Eventos de carga ──────────────────────────────────────────────────

        private void Clientes_Load(object sender, EventArgs e)
        {
            CargarClientes();
        }

        // ── Eventos del Designer ──────────────────────────────────────────────

        private void TxtFiltro_TextChanged(object sender, EventArgs e)
        {
            AplicarFiltro();
        }

        private void BtnRefrescar_Click(object sender, EventArgs e)
        {
            CargarClientes();
        }

        private void DgvClientes_SelectionChanged(object sender, EventArgs e)
        {
            bool hay = dgvClientes.SelectedRows.Count > 0;
            btnEditar.Enabled = hay;
            btnBaja.Enabled   = hay;
        }

        private void DgvClientes_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            BtnEditar_Click(sender, e);
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

    }
}
