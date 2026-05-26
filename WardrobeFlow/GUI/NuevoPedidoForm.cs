using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Servicios.Multiidioma;

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
    public partial class NuevoPedidoForm : FormBase, IIdiomaObserver
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
        private Idioma           _idioma      = GestorIdioma.IdiomaActual;

        public NuevoPedidoForm()
        {
            InitializeComponent();
            AplicarIdioma(GestorIdioma.IdiomaActual);
            this.Load += new EventHandler(NuevoPedidoForm_Load);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GestorIdioma.SuscribirObservador(this);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            GestorIdioma.DesuscribirObservador(this);
            base.OnFormClosing(e);
        }

        // ── IIdiomaObserver ───────────────────────────────────────────────────

        public void UpdateLanguage(Idioma idioma)
        {
            AplicarIdioma(idioma);
            if (dgvPrendas.Columns.Count > 0)
                TraducirHeadersGrilla();
        }

        // ── Traducción ────────────────────────────────────────────────────────

        private void AplicarIdioma(Idioma idioma)
        {
            _idioma = idioma;
            var t = Traductor.ObtenerTraducciones(idioma);
            string T(string key, string fallback) =>
                t.ContainsKey(key) ? t[key].Texto : fallback;

            this.Text                 = T("frm.nuevopedido",     "Nuevo Pedido de Venta");
            lblPaso.Text              = T("paso1.texto",          "Paso 1 de 2 — Seleccionar Cliente");
            lblSeleccionaCliente.Text = T("lbl.ped.selcliente",   "Seleccioná el cliente para este pedido:");
            lblInstruccion.Text       = T("lbl.ped.selprendas",   "Seleccioná las prendas para incluir en el pedido (checkbox):");
            btnSiguiente.Text         = T("btn.siguiente",         "Siguiente →");
            btnVolver.Text            = T("btn.volver",            "← Volver");
            btnConfirmar.Text         = T("btn.confirmar.pedido",  "✓ Confirmar Pedido");
        }

        private void TraducirHeadersGrilla()
        {
            var t = Traductor.ObtenerTraducciones(_idioma);
            void RH(string col, string clave, string fallback)
            {
                if (dgvPrendas.Columns.Contains(col) && t.ContainsKey(clave))
                    dgvPrendas.Columns[col].HeaderText = t[clave].Texto;
                else if (dgvPrendas.Columns.Contains(col))
                    dgvPrendas.Columns[col].HeaderText = fallback;
            }
            RH("Nombre",   "col.prenda.nombre",    "Nombre");
            RH("Categoria","col.prenda.categoria",  "Categoría");
            RH("Talle",    "col.prenda.talle",      "Talle");
            RH("Color",    "col.prenda.color",      "Color");
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
                var t = Traductor.ObtenerTraducciones(_idioma);
                string placeholder = t.ContainsKey("combo.ped.placeholder")
                    ? t["combo.ped.placeholder"].Texto
                    : "— Seleccioná un cliente —";

                _clientes = clienteBLL.ObtenerTodos();
                cmbCliente.Items.Clear();
                cmbCliente.Items.Add(placeholder);
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

                TraducirHeadersGrilla();
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
            var t = Traductor.ObtenerTraducciones(_idioma);
            string paso1txt = t.ContainsKey("paso1.texto") ? t["paso1.texto"].Texto : "Paso 1 de 2 — Seleccionar Cliente";
            string paso2txt = t.ContainsKey("paso2.texto") ? t["paso2.texto"].Texto : "Paso 2 de 2 — Seleccionar Prendas";
            lblPaso.Text = paso == 1
                ? paso1txt
                : $"{paso2txt}  ({_clienteSel?.NombreCompleto})";
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
            var tI = Traductor.ObtenerTraducciones(_idioma);
            string T_i(string k, string fb) => tI.ContainsKey(k) ? tI[k].Texto : fb;

            // Bloquear si suscripción vencida
            if (_clienteSel.TienePlan() && !_clienteSel.SuscripcionVigente())
            {
                string msgVenc = string.Format(
                    T_i("err.ped.suscvencida",
                        "⚠ La suscripción de {0} venció el {1}.\nRenovar en el módulo de Clientes."),
                    _clienteSel.NombreCompleto,
                    _clienteSel.FechaVencimiento.Value.ToString("dd/MM/yyyy"));
                lblInfoPlan.Text      = msgVenc;
                lblInfoPlan.ForeColor = Color.DarkRed;
                lblInfoPlan.Visible   = true;
                btnSiguiente.Enabled  = false;
                return;
            }

            if (_clienteSel.IdPlan.HasValue)
            {
                lblInfoPlan.Text = string.Format(
                    T_i("lbl.ped.infoplan",
                        "Cliente: {0}\nPlan: {1}\nPrendas en uso actualmente: {2}\nMétodo de pago: {3}\nAlta: {4}"),
                    _clienteSel.NombreCompleto,
                    _clienteSel.NombrePlan ?? "—",
                    _clienteSel.StockUtilizado,
                    _clienteSel.MetodoPago,
                    _clienteSel.FechaAlta.ToString("dd/MM/yyyy"));
            }
            else
            {
                lblInfoPlan.Text = string.Format(
                    T_i("err.ped.sinplan",
                        "⚠ {0} no tiene plan asignado.\nAsigná un plan en el módulo de Clientes antes de crear un pedido."),
                    _clienteSel.NombreCompleto);
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
            int disponibles   = _clienteSel?.PrendasDisponiblesEnPlan() ?? 0;
            int total         = enUso + seleccionadas;

            var tR = Traductor.ObtenerTraducciones(_idioma);
            string TR(string k, string fb) => tR.ContainsKey(k) ? tR[k].Texto : fb;

            string linea1 = string.Format(
                TR("lbl.ped.res.linea1", "Seleccionadas: {0}  |  Ya en uso: {1}  |  Total: {2}"),
                seleccionadas, enUso, total);
            string linea2 = "";

            if (limite > 0)
            {
                if (!_clienteSel.PuedeSolicitarPrendas(seleccionadas))
                {
                    linea2 = string.Format(
                        TR("lbl.ped.res.limite",
                           "✗ El plan '{0}' permite {1} prenda(s). Estás superando el límite por {2}."),
                        _clienteSel?.NombrePlan, limite, total - limite);
                    lblResumen.ForeColor = Color.DarkRed;
                    btnConfirmar.Enabled = false;
                }
                else if (seleccionadas == 0)
                {
                    linea2 = string.Format(
                        TR("lbl.ped.res.vacio", "Podés agregar hasta {0} prenda(s) (plan {1})."),
                        disponibles, _clienteSel?.NombrePlan);
                    lblResumen.ForeColor = Color.DimGray;
                    btnConfirmar.Enabled = false;
                }
                else if (seleccionadas < disponibles)
                {
                    linea2 = string.Format(
                        TR("lbl.ped.res.parcial",
                           "ℹ El plan '{0}' permite {1}. Estás eligiendo {2} de {3} posibles — podés agregar más."),
                        _clienteSel?.NombrePlan, limite, seleccionadas, disponibles);
                    lblResumen.ForeColor = Color.FromArgb(140, 100, 0);
                    btnConfirmar.Enabled = true;
                }
                else
                {
                    linea2 = string.Format(
                        TR("lbl.ped.res.lleno",
                           "✓ Alcanzás el máximo del plan '{0}' ({1} prendas)."),
                        _clienteSel?.NombrePlan, limite);
                    lblResumen.ForeColor = Color.DarkGreen;
                    btnConfirmar.Enabled = true;
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
                var tSP = Traductor.ObtenerTraducciones(_idioma);
                MostrarError(tSP.ContainsKey("err.ped.sinprendas") ? tSP["err.ped.sinprendas"].Texto : "Seleccioná al menos una prenda.");
                return;
            }

            // Confirmación final
            string detallesPrendas = string.Join("\n  • ",
                prendas.ConvertAll(p => $"{p.Nombre} ({p.Talle} — {p.Color})"));

            var tConf = Traductor.ObtenerTraducciones(_idioma);
            string TC(string k, string fb) => tConf.ContainsKey(k) ? tConf[k].Texto : fb;

            var confirmar = MessageBox.Show(
                string.Format(
                    TC("conf.ped.msg",
                       "Confirmar pedido para {0}:\n\n  • {1}\n\nTotal: {2} prenda(s)\nMétodo de pago: {3}"),
                    _clienteSel.NombreCompleto,
                    detallesPrendas,
                    prendas.Count,
                    _clienteSel.MetodoPago),
                TC("conf.ped.titulo", "Confirmar Pedido"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);

            if (confirmar != DialogResult.Yes) return;

            try
            {
                btnConfirmar.Enabled = false;
                var tp = Traductor.ObtenerTraducciones(_idioma);
                btnConfirmar.Text = tp.ContainsKey("btn.procesando") ? tp["btn.procesando"].Texto : "Procesando...";

                IdPedidoCreado = pedidoBLL.CrearPedido(this.Text, _clienteSel.IdCliente, prendas);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                btnConfirmar.Enabled = true;
                var te = Traductor.ObtenerTraducciones(_idioma);
                btnConfirmar.Text = te.ContainsKey("btn.confirmar.pedido") ? te["btn.confirmar.pedido"].Texto : "✓ Confirmar Pedido";
                MostrarError(ex.Message);
            }
        }

    }
}
