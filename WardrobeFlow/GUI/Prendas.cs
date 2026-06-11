using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Servicios.Multiidioma;

namespace GUI
{
    /// <summary>
    /// Capa de Presentación — Módulo de Gestión de Prendas.
    ///
    /// Permite al ControladorDeStock administrar el catálogo de prendas:
    ///   ✓ Ver todas las prendas con estado, cliente actual y datos descriptivos
    ///   ✓ Filtrar por estado (Todos / Disponible / EnUso / EnLimpieza / Baja)
    ///   ✓ Filtrar por texto libre (nombre, categoría, color)
    ///   ✓ Agregar nueva prenda al catálogo
    ///   ✓ Editar datos descriptivos de una prenda
    ///   ✓ Cambiar estado (Disponible ↔ EnLimpieza, → Baja)
    ///   ✓ Ver detalle del cliente que tiene la prenda en uso
    ///
    /// El OperadorLogístico también accede (mnuPrendas) pero sin panel de acciones
    /// de stock (mnuStock). Los botones de cambio de estado están disponibles solo
    /// si el usuario tiene el permiso mnuStock.
    ///
    /// Accesible desde Menú → Inventario → Prendas.
    /// </summary>
    /// <summary>
    /// Hereda de <see cref="FormBase"/>:
    ///   - MostrarOk() y MostrarError() → heredados, no se redeclaran
    ///   - MensajeLabel → sobreescrito para devolver el lblMensaje de este formulario
    /// </summary>
    public partial class Prendas : FormBase, IIdiomaObserver
    {
        protected override Label MensajeLabel => lblMensaje;

        private readonly BLL.Interfaces.IPrendaService prendaBLL = new BLL.Prenda();

        // Determina si el usuario puede cambiar estados (ControladorDeStock)
        private readonly bool _tieneStock;

        // Idioma activo — sincronizado en Traducir() para usar en EstadoLabel y ColorearFilas
        private Idioma _idioma = GestorIdioma.IdiomaActual;

        private List<BE.Prenda> _prendas = new List<BE.Prenda>();

        public Prendas()
        {
            InitializeComponent();

            // Verificar si el usuario activo tiene permiso de stock
            var bllUsuario = new BLL.Usuario();
            var usuario    = bllUsuario.ObtenerUsuarioActivo();
            _tieneStock = usuario?.Permisos?.Exists(p => p.NombreMenu == "mnuStock") == true;

            // Ocultar botones de acción si el usuario no tiene permiso de stock
            btnNueva.Visible        = _tieneStock;
            btnEditar.Visible       = _tieneStock;
            btnCambiarEstado.Visible = _tieneStock;

            this.Load += new EventHandler(Prendas_Load);
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
            Traducir(idioma);    // actualiza _idioma, combo de estados y headers
            AplicarFiltro();     // reconstruye filas con EstadoLabel() en el nuevo idioma
        }

        private void Traducir(Idioma idioma)
        {
            _idioma = idioma;
            var t = Traductor.ObtenerTraducciones(idioma);
            if (this.Tag != null && t.ContainsKey(this.Tag.ToString()))
                this.Text = t[this.Tag.ToString()].Texto;
            Aplicar(lblEstado,        t);
            Aplicar(lblBuscar,        t);
            Aplicar(btnNueva,          t);
            Aplicar(btnEditar,         t);
            Aplicar(btnCambiarEstado,  t);
            Aplicar(btnMantenimiento,  t);
            Aplicar(lblDetalleTitulo,  t);
            RellenarComboEstado(idioma);
            TraducirHeadersGrilla();
        }

        /// <summary>Rellena el combo de estado con las opciones traducidas al idioma activo.</summary>
        private void RellenarComboEstado(Idioma idioma)
        {
            var t = Traductor.ObtenerTraducciones(idioma);
            string T(string key, string fallback) => t.ContainsKey(key) ? t[key].Texto : fallback;

            int prevIdx = cmbEstadoFiltro.SelectedIndex < 0 ? 0 : cmbEstadoFiltro.SelectedIndex;
            cmbEstadoFiltro.SelectedIndexChanged -= CmbEstadoFiltro_SelectedIndexChanged;
            cmbEstadoFiltro.Items.Clear();
            cmbEstadoFiltro.Items.Add(T("combo.prenda.todos",  "Todos"));
            cmbEstadoFiltro.Items.Add(T("prenda.disponible",   "Disponible"));
            cmbEstadoFiltro.Items.Add(T("prenda.enuso",        "En Uso"));
            cmbEstadoFiltro.Items.Add(T("prenda.enlimpieza",   "En Limpieza"));
            cmbEstadoFiltro.Items.Add(T("prenda.baja",         "Baja"));
            cmbEstadoFiltro.SelectedIndex = prevIdx < cmbEstadoFiltro.Items.Count ? prevIdx : 0;
            cmbEstadoFiltro.SelectedIndexChanged += CmbEstadoFiltro_SelectedIndexChanged;
        }

        /// <summary>Traduce los HeaderText de la grilla de prendas según el idioma activo.</summary>
        private void TraducirHeadersGrilla()
        {
            var t = Traductor.ObtenerTraducciones(_idioma);
            void RH(string col, string key, string fallback)
            {
                if (dgvPrendas.Columns.Contains(col) && t.ContainsKey(key))
                    dgvPrendas.Columns[col].HeaderText = t[key].Texto;
                else if (dgvPrendas.Columns.Contains(col))
                    dgvPrendas.Columns[col].HeaderText = fallback;
            }
            RH("Nombre",    "col.prenda.nombre",    "Nombre");
            RH("Categoría", "col.prenda.categoria",  "Categoría");
            RH("Talle",     "col.prenda.talle",      "Talle");
            RH("Color",     "col.prenda.color",      "Color");
            RH("Estado",    "col.prenda.estado",     "Estado");
            RH("Cliente",   "col.prenda.cliente",    "Cliente");
            RH("Alta",      "col.prenda.alta",       "Alta");
        }

        private static void Aplicar(Control c, IDictionary<string, Traduccion> t)
        {
            if (c?.Tag != null && t.ContainsKey(c.Tag.ToString()))
                c.Text = t[c.Tag.ToString()].Texto;
        }

        // ── Eventos del Designer ──────────────────────────────────────────────

        private void Prendas_Load(object sender, EventArgs e)
        {
            CargarPrendas();
        }

        private void CmbEstadoFiltro_SelectedIndexChanged(object sender, EventArgs e)
        {
            AplicarFiltro();
        }

        private void TxtFiltro_TextChanged(object sender, EventArgs e)
        {
            AplicarFiltro();
        }

        private void BtnRefrescar_Click(object sender, EventArgs e)
        {
            CargarPrendas();
        }

        private void DgvPrendas_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (_tieneStock) BtnEditar_Click(sender, e);
        }

        // ── Carga y filtrado ──────────────────────────────────────────────────

        private void CargarPrendas()
        {
            try
            {
                _prendas = prendaBLL.ObtenerTodos();
                AplicarFiltro();
                var t = Traductor.ObtenerTraducciones(_idioma);
                string fmt = t.ContainsKey("msg.prenda.cargadas") ? t["msg.prenda.cargadas"].Texto : "{0} prenda(s) en el catálogo.";
                MostrarOk(string.Format(fmt, _prendas.Count));
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void AplicarFiltro()
        {
            string texto = txtFiltro.Text.Trim().ToLower();
            int    idx   = cmbEstadoFiltro.SelectedIndex;  // 0=Todos 1=Disp 2=EnUso 3=Limpieza 4=Baja

            var lista = _prendas.FindAll(p =>
            {
                // Filtro estado
                bool pasaEstado = idx == 0
                    || (idx == 1 && p.Estado == BE.EstadoPrenda.Disponible)
                    || (idx == 2 && p.Estado == BE.EstadoPrenda.EnUso)
                    || (idx == 3 && p.Estado == BE.EstadoPrenda.EnLimpieza)
                    || (idx == 4 && p.Estado == BE.EstadoPrenda.Baja);

                // Filtro texto
                bool pasaTexto = string.IsNullOrEmpty(texto)
                    || p.Nombre.ToLower().Contains(texto)
                    || (p.Categoria ?? "").ToLower().Contains(texto)
                    || (p.Color ?? "").ToLower().Contains(texto)
                    || (p.Talle ?? "").ToLower().Contains(texto);

                return pasaEstado && pasaTexto;
            });

            var tabla = new DataTable();
            tabla.Columns.Add("ID",        typeof(int));
            tabla.Columns.Add("Nombre",    typeof(string));
            tabla.Columns.Add("Categoría", typeof(string));
            tabla.Columns.Add("Talle",     typeof(string));
            tabla.Columns.Add("Color",     typeof(string));
            tabla.Columns.Add("Estado",    typeof(string));
            tabla.Columns.Add("Cliente",   typeof(string));
            tabla.Columns.Add("Alta",      typeof(string));
            // Columna interna para colorear sin depender del texto visible
            tabla.Columns.Add("_EstadoKey", typeof(int));

            foreach (var p in lista)
            {
                tabla.Rows.Add(
                    p.IdPrenda,
                    p.Nombre,
                    p.Categoria ?? "—",
                    p.Talle ?? "—",
                    p.Color ?? "—",
                    EstadoLabel(p.Estado),
                    p.NombreCliente ?? "—",
                    p.FechaAlta.ToString("dd/MM/yyyy"),
                    (int)p.Estado);
            }

            dgvPrendas.DataSource = tabla;

            // Ocultar columna interna y colorear filas según estado
            if (dgvPrendas.Columns.Contains("_EstadoKey"))
                dgvPrendas.Columns["_EstadoKey"].Visible = false;

            ColorearFilas();
            TraducirHeadersGrilla();

            if (dgvPrendas.Columns.Contains("ID"))
                dgvPrendas.Columns["ID"].Width = 40;

            var t = Traductor.ObtenerTraducciones(_idioma);
            string fmt = t.ContainsKey("msg.prenda.conteo") ? t["msg.prenda.conteo"].Texto : "Mostrando {0} de {1}";
            lblConteo.Text = string.Format(fmt, lista.Count, _prendas.Count);
            panelDetalle.Visible = false;
        }

        private void ColorearFilas()
        {
            if (dgvPrendas.DataSource == null) return;
            if (!dgvPrendas.Columns.Contains("_EstadoKey")) return;
            foreach (DataGridViewRow row in dgvPrendas.Rows)
            {
                if (!int.TryParse(row.Cells["_EstadoKey"].Value?.ToString(), out int key)) continue;
                row.DefaultCellStyle.ForeColor = key switch
                {
                    (int)BE.EstadoPrenda.EnUso       => Color.FromArgb(30, 100, 170),
                    (int)BE.EstadoPrenda.EnLimpieza  => Color.FromArgb(160, 100, 0),
                    (int)BE.EstadoPrenda.Baja        => Color.FromArgb(160, 50, 50),
                    _                                => Color.Black
                };
            }
        }

        private void DgvPrendas_SelectionChanged(object sender, EventArgs e)
        {
            bool hay = dgvPrendas.SelectedRows.Count > 0;
            if (_tieneStock)
            {
                btnEditar.Enabled        = hay;
                btnCambiarEstado.Enabled = hay;
            }
            btnMantenimiento.Enabled = hay;

            // Mostrar panel detalle si la prenda está en uso
            panelDetalle.Visible = false;
            if (!hay) return;

            var prenda = ObtenerPrendaSeleccionada();
            if (prenda == null) return;

            if (prenda.Estado == BE.EstadoPrenda.EnUso && !string.IsNullOrEmpty(prenda.NombreCliente))
            {
                lblDetalleContenido.Text =
                    $"{prenda.NombreCliente}   |   " +
                    $"Prenda: {prenda.Nombre}  —  {prenda.Talle}  —  {prenda.Color}";
                panelDetalle.Visible = true;
            }
        }

        // ── Eventos de botones ────────────────────────────────────────────────

        private void BtnNueva_Click(object sender, EventArgs e)
        {
            using (var form = new PrendaForm())
            {
                if (form.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    prendaBLL.Alta(this.Text, form.PrendaEditada);
                    var tAlt = Traductor.ObtenerTraducciones(_idioma);
                    string fmtAlt = tAlt.ContainsKey("msg.prenda.agregada") ? tAlt["msg.prenda.agregada"].Texto : "Prenda '{0}' agregada al catálogo.";
                    MostrarOk(string.Format(fmtAlt, form.PrendaEditada.Nombre));
                    CargarPrendas();
                }
                catch (Exception ex) { MostrarError(ex); }
            }
        }

        private void BtnEditar_Click(object sender, EventArgs e)
        {
            var prenda = ObtenerPrendaSeleccionada();
            if (prenda == null) return;

            using (var form = new PrendaForm(prenda))
            {
                if (form.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    prendaBLL.Modificar(this.Text, form.PrendaEditada);
                    var tMod = Traductor.ObtenerTraducciones(_idioma);
                    string fmtMod = tMod.ContainsKey("msg.prenda.actualizada") ? tMod["msg.prenda.actualizada"].Texto : "Prenda '{0}' actualizada.";
                    MostrarOk(string.Format(fmtMod, form.PrendaEditada.Nombre));
                    CargarPrendas();
                }
                catch (Exception ex) { MostrarError(ex); }
            }
        }

        private void BtnCambiarEstado_Click(object sender, EventArgs e)
        {
            var prenda = ObtenerPrendaSeleccionada();
            if (prenda == null) return;

            // Construir opciones de transición válidas usando la lógica de BE
            var tEst = Traductor.ObtenerTraducciones(_idioma);
            string T_est(string k, string fb) => tEst.ContainsKey(k) ? tEst[k].Texto : fb;

            var opciones = new List<(string texto, BE.EstadoPrenda estado)>();

            var candidatos = new (BE.EstadoPrenda estado, string clave, string fb)[]
            {
                (BE.EstadoPrenda.Disponible,  "opt.marcardisp",     "Marcar Disponible"),
                (BE.EstadoPrenda.EnLimpieza,  "opt.enviarlimpieza", "Enviar a Limpieza"),
                (BE.EstadoPrenda.Baja,        "opt.darbaja",        "Dar de Baja"),
            };

            foreach (var cand in candidatos)
            {
                if (cand.estado != prenda.Estado && prenda.TransicionPermitida(cand.estado))
                    opciones.Add((T_est(cand.clave, cand.fb), cand.estado));
            }

            if (opciones.Count == 0)
            {
                string errKey = prenda.Estado == BE.EstadoPrenda.EnUso ? "err.prenda.enuso" : "err.prenda.baja";
                string errFb  = prenda.Estado == BE.EstadoPrenda.EnUso
                    ? "No se puede cambiar el estado: la prenda está en uso por un cliente."
                    : "La prenda está dada de baja y no puede ser reactivada.";
                MostrarError(T_est(errKey, errFb));
                return;
            }

            // Mostrar diálogo de selección de nuevo estado
            string estadoTraducido = EstadoLabel(prenda.Estado);
            using (var dlg = new CambioEstadoDialog(prenda, opciones, estadoTraducido))
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    string actor = Seguridad.SessionManager.IsLoggedIn
                        ? Seguridad.SessionManager.GetInstance().Usuario.Username
                        : null;
                    prendaBLL.CambiarEstado(this.Text, prenda, dlg.EstadoSeleccionado, actor);
                    string fmtEstAct = T_est("msg.prenda.estadoact", "Estado de '{0}' actualizado a {1}.");
                    MostrarOk(string.Format(fmtEstAct, prenda.Nombre, EstadoLabel(dlg.EstadoSeleccionado)));
                    CargarPrendas();
                }
                catch (Exception ex) { MostrarError(ex); }
            }
        }

        private void BtnMantenimiento_Click(object sender, EventArgs e)
        {
            var prenda = ObtenerPrendaSeleccionada();
            if (prenda == null) return;

            using (var form = new MantenimientoHistorialForm(prenda, prendaBLL))
                form.ShowDialog(this);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private BE.Prenda ObtenerPrendaSeleccionada()
        {
            if (dgvPrendas.SelectedRows.Count == 0) return null;
            int id = Convert.ToInt32(dgvPrendas.SelectedRows[0].Cells["ID"].Value);
            return _prendas.Find(p => p.IdPrenda == id);
        }

        private string EstadoLabel(BE.EstadoPrenda estado)
        {
            var t = Traductor.ObtenerTraducciones(_idioma);
            string T(string key, string fallback) => t.ContainsKey(key) ? t[key].Texto : fallback;
            switch (estado)
            {
                case BE.EstadoPrenda.Disponible:  return T("prenda.disponible",  "Disponible");
                case BE.EstadoPrenda.EnUso:       return T("prenda.enuso",       "En Uso");
                case BE.EstadoPrenda.EnLimpieza:  return T("prenda.enlimpieza",  "En Limpieza");
                case BE.EstadoPrenda.Baja:        return T("prenda.baja",        "Baja");
                default:                          return estado.ToString();
            }
        }

    }
}
