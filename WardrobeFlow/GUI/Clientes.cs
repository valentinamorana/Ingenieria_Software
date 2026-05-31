using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Servicios.Multiidioma;

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
    public partial class Clientes : FormBase, IIdiomaObserver
    {
        protected override Label MensajeLabel => lblMensaje;

        private readonly BLL.Interfaces.IClienteService clienteBLL = new BLL.Cliente();

        // Cache de la lista actual
        private List<BE.Cliente> _clientes = new List<BE.Cliente>();

        // Idioma activo — sincronizado en Traducir() para usar en AplicarFiltro y CargarClientes
        private Idioma _idioma = GestorIdioma.IdiomaActual;

        public Clientes()
        {
            InitializeComponent();
            this.Load += new EventHandler(Clientes_Load);
        }

        // ── Observer de idioma ────────────────────────────────────────────────

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GestorIdioma.SuscribirObservador(this);
            Traducir(GestorIdioma.IdiomaActual);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            GestorIdioma.DesuscribirObservador(this);
            base.OnFormClosing(e);
        }

        public void UpdateLanguage(Idioma idioma)
        {
            Traducir(idioma);
            // Refrescar grilla para que los headers y "Sin plan" reflejen el nuevo idioma
            if (_clientes.Count > 0 || dgvClientes.Rows.Count > 0)
                AplicarFiltro();
        }

        private void Traducir(Idioma idioma)
        {
            _idioma = idioma;
            var t = Traductor.ObtenerTraducciones(idioma);
            if (this.Tag != null && t.ContainsKey(this.Tag.ToString()))
                this.Text = t[this.Tag.ToString()].Texto;
            Aplicar(lblFiltro,  t);
            Aplicar(btnNuevo,   t);
            Aplicar(btnEditar,  t);
            Aplicar(btnBaja,    t);
            TraducirHeadersGrilla();
        }

        /// <summary>Traduce los HeaderText de la grilla de clientes según el idioma activo.</summary>
        private void TraducirHeadersGrilla()
        {
            var t = Traductor.ObtenerTraducciones(_idioma);
            void RH(string col, string clave, string fallback)
            {
                if (dgvClientes.Columns.Contains(col) && t.ContainsKey(clave))
                    dgvClientes.Columns[col].HeaderText = t[clave].Texto;
                else if (dgvClientes.Columns.Contains(col))
                    dgvClientes.Columns[col].HeaderText = fallback;
            }
            RH("Nombre",     "col.cli.nombre",     "Nombre");
            RH("Apellido",   "col.cli.apellido",   "Apellido");
            RH("DNI",        "col.cli.dni",        "DNI");
            RH("Email",      "col.cli.email",      "Email");
            RH("Plan",       "col.cli.plan",       "Plan");
            RH("Prendas",    "col.cli.prendas",    "Uso / Plan");
            RH("MetodoPago", "col.cli.metodopago", "Método de Pago");
            RH("Alta",        "col.cli.alta",        "Alta");
            RH("Vencimiento", "col.cli.vencimiento", "Vencimiento");
        }

        private static void Aplicar(Control c, IDictionary<string, Traduccion> t)
        {
            if (c?.Tag != null && t.ContainsKey(c.Tag.ToString()))
                c.Text = t[c.Tag.ToString()].Texto;
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

                var t = Traductor.ObtenerTraducciones(_idioma);
                string fmt = t.ContainsKey("msg.cli.cargados")
                    ? t["msg.cli.cargados"].Texto
                    : "{0} cliente(s) registrado(s).";
                MostrarOk(string.Format(fmt, _clientes.Count));
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

            var t = Traductor.ObtenerTraducciones(_idioma);
            string sinPlan = t.ContainsKey("lbl.sinplan") ? t["lbl.sinplan"].Texto : "Sin plan";

            string sinVenc = t.ContainsKey("msg.cli.sinvenc") ? t["msg.cli.sinvenc"].Texto : "—";
            string vencido = t.ContainsKey("msg.cli.vencido") ? t["msg.cli.vencido"].Texto : "Vencido";

            string proxVencer = t.ContainsKey("msg.cli.proxvencer") ? t["msg.cli.proxvencer"].Texto : "Vence pronto";

            var tabla = new DataTable();
            tabla.Columns.Add("ID",          typeof(int));
            tabla.Columns.Add("Nombre",      typeof(string));
            tabla.Columns.Add("Apellido",    typeof(string));
            tabla.Columns.Add("DNI",         typeof(string));
            tabla.Columns.Add("Email",       typeof(string));
            tabla.Columns.Add("Plan",        typeof(string));
            tabla.Columns.Add("Prendas",     typeof(string));
            tabla.Columns.Add("MetodoPago",  typeof(string));
            tabla.Columns.Add("Alta",        typeof(string));
            tabla.Columns.Add("Vencimiento", typeof(string));
            tabla.Columns.Add("_Vencido",    typeof(bool));
            tabla.Columns.Add("_ProxVencer", typeof(bool));

            foreach (var c in lista)
            {
                bool expirado    = c.VencimientoExpirado;
                bool proxAVencer = !expirado && c.SuscripcionProximaAVencer();

                string vencStr = c.FechaVencimiento.HasValue
                    ? (expirado
                        ? $"⚠ {c.FechaVencimiento.Value:dd/MM/yyyy} ({vencido})"
                        : proxAVencer
                            ? $"⏰ {c.FechaVencimiento.Value:dd/MM/yyyy} ({proxVencer})"
                            : c.FechaVencimiento.Value.ToString("dd/MM/yyyy"))
                    : sinVenc;

                // Mostrar "StockUtilizado / LimitePrendas" si tiene plan con límite
                string capacidad = c.TienePlan() && c.LimitePrendas > 0
                    ? $"{c.StockUtilizado} / {c.LimitePrendas}"
                    : c.StockUtilizado.ToString();

                tabla.Rows.Add(
                    c.IdCliente,
                    c.Nombre,
                    c.Apellido,
                    c.DNI,
                    c.Email ?? "—",
                    c.NombrePlan ?? sinPlan,
                    capacidad,
                    c.MetodoPago,
                    c.FechaAlta.ToString("dd/MM/yyyy"),
                    vencStr,
                    expirado,
                    proxAVencer);
            }

            dgvClientes.DataSource = tabla;

            // Ocultar columnas auxiliares y colorear filas
            if (dgvClientes.Columns.Contains("_Vencido"))
                dgvClientes.Columns["_Vencido"].Visible = false;
            if (dgvClientes.Columns.Contains("_ProxVencer"))
                dgvClientes.Columns["_ProxVencer"].Visible = false;

            foreach (DataGridViewRow row in dgvClientes.Rows)
            {
                if (row.Cells["_Vencido"].Value is bool exp && exp)
                    row.DefaultCellStyle.ForeColor = Color.DarkRed;
                else if (row.Cells["_ProxVencer"].Value is bool prox && prox)
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(160, 100, 0);
            }

            // Ajustar columnas
            if (dgvClientes.Columns.Contains("ID"))
                dgvClientes.Columns["ID"].Width = 40;

            TraducirHeadersGrilla();

            string fmtConteo = t.ContainsKey("msg.cli.conteo")
                ? t["msg.cli.conteo"].Texto
                : "Mostrando {0} de {1}";
            lblConteo.Text = string.Format(fmtConteo, lista.Count, _clientes.Count);
        }

        // ── Eventos de botones ────────────────────────────────────────────────

        private void BtnNuevo_Click(object sender, EventArgs e)
        {
            using (var form = new ClienteForm())
            {
                if (form.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    clienteBLL.Alta(this.Text, form.ClienteEditado);
                    var t = Traductor.ObtenerTraducciones(_idioma);
                    string fmt = t.ContainsKey("msg.cli.registrado") ? t["msg.cli.registrado"].Texto : "Cliente '{0}' registrado correctamente.";
                    MostrarOk(string.Format(fmt, form.ClienteEditado.NombreCompleto));
                    CargarClientes();
                }
                catch (Exception ex)
                {
                    MostrarError(ex);
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

                    clienteBLL.Modificar(this.Text, form.ClienteEditado);
                    var t = Traductor.ObtenerTraducciones(_idioma);
                    string fmt = t.ContainsKey("msg.cli.actualizado") ? t["msg.cli.actualizado"].Texto : "Cliente '{0}' actualizado.";
                    MostrarOk(string.Format(fmt, form.ClienteEditado.NombreCompleto));
                    CargarClientes();
                }
                catch (Exception ex)
                {
                    MostrarError(ex);
                }
            }
        }

        private void BtnBaja_Click(object sender, EventArgs e)
        {
            var cliente = ObtenerClienteSeleccionado();
            if (cliente == null) return;

            var t = Traductor.ObtenerTraducciones(_idioma);
            string T(string k, string fb) => t.ContainsKey(k) ? t[k].Texto : fb;

            var confirmacion = MessageBox.Show(
                string.Format(T("conf.baja.cli.msg", "¿Dar de baja a {0} (DNI {1})?\n\nEsta acción no se puede deshacer."),
                    cliente.NombreCompleto, cliente.DNI),
                T("conf.baja.cli.titulo", "Confirmar Baja"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (confirmacion != DialogResult.Yes) return;

            try
            {
                clienteBLL.Baja(this.Text, cliente);
                string fmt = T("msg.cli.eliminado", "Cliente '{0}' eliminado.");
                MostrarOk(string.Format(fmt, cliente.NombreCompleto));
                CargarClientes();
            }
            catch (Exception ex)
            {
                MostrarError(ex);
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
