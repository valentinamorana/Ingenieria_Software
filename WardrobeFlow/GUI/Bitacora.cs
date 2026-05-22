using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using Servicios.Multiidioma;

namespace GUI
{
    /// <summary>
    /// Capa de Presentación — Módulo de Auditoría (Bitácora).
    ///
    /// Presenta dos pestañas:
    ///   Tab 1 — Sistema    : eventos de seguridad (login, logout, resets, intentos fallidos)
    ///   Tab 2 — Negocio    : eventos de negocio (ventas, despachos, stock, clientes)
    ///
    /// Filtro de fecha unificado: sólo "Últimos N días" (0 = sin filtro de fecha).
    /// Criticidad: "Todas" + valores reales 1-6, sin "None (0)".
    /// Exportación PDF: vía PrintPreviewDialog (imprimir → "Microsoft Print to PDF").
    ///
    /// Accesible para Administrador (mnuAuditoria) y Supervisor (mnuAuditoria).
    /// </summary>
    /// <summary>
    /// Hereda de <see cref="FormBase"/>:
    ///   - MostrarError() → heredado. Como este formulario no tiene lblMensaje,
    ///     MensajeLabel retorna null y FormBase usa MessageBox automáticamente.
    /// </summary>
    public partial class Bitacora : FormBase, IIdiomaObserver
    {
        private readonly BLL.Bitacora bllBitacora = new BLL.Bitacora();

        // ── Combo Tipo Evento (DB keys paralelas a los ítems del combo) ──────────
        private readonly List<string> _tipoEventoDB = new List<string>();

        // ── Estado para impresión paginada ────────────────────────────────────
        private DataTable   _tablaImpresion;
        private string      _tituloImpresion;
        private string[]    _headersImpresion;   // headers traducidos capturados del DataGridView
        private int         _paginaActual;
        private int         _filaImpresion;
        private Font        _fuenteHeader;
        private Font        _fuenteCelda;
        private Font        _fuenteTitulo;

        public Bitacora()
        {
            InitializeComponent();
        }

        // ── Observer de idioma ────────────────────────────────────────────────

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GestorIdioma.SuscribirObservador(this);
            Traducir(GestorIdioma.IdiomaActual);  // calls RellenarComboCriticidad internally
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            GestorIdioma.DesuscribirObservador(this);
            base.OnFormClosing(e);
        }

        public void UpdateLanguage(Idioma idioma)
        {
            Traducir(idioma);   // includes RellenarComboCriticidad
            // Refrescar headers de las grillas actualmente visibles
            TraducirHeadersGrilla(dgvSistema, idioma, esSistema: true);
            TraducirHeadersGrilla(dgvNegocio, idioma, esSistema: false);
        }

        private void Traducir(Idioma idioma)
        {
            var t = Traductor.ObtenerTraducciones(idioma);
            if (this.Tag != null && t.ContainsKey(this.Tag.ToString()))
                this.Text = t[this.Tag.ToString()].Texto;
            // Tabs
            if (t.ContainsKey("tab.sistema")) tabPageSistema.Text = t["tab.sistema"].Texto;
            if (t.ContainsKey("tab.negocio")) tabPageNegocio.Text = t["tab.negocio"].Texto;
            // Filtros sistema
            Aplicar(lblUltimosSistema,    t);
            Aplicar(lblDiasSistema,       t);
            Aplicar(btnUltimosDias,       t);
            Aplicar(lblUsuarioId,         t);
            Aplicar(lblActividadSistema,  t);
            Aplicar(lblCriticidadSistema, t);
            Aplicar(btnBuscar,            t);
            Aplicar(btnLimpiar,           t);
            // Filtros negocio
            Aplicar(lblUltimosNegocio,    t);
            Aplicar(lblDiasNegocio,       t);
            Aplicar(btnNegUltimosDias,    t);
            Aplicar(lblTipoEvento,        t);
            Aplicar(lblIdPedido,          t);
            Aplicar(lblIdCliente,         t);
            Aplicar(btnNegBuscar,         t);
            Aplicar(btnNegLimpiar,        t);
            // Botones Exportar PDF (sin Tag en Designer → texto directo)
            string exportPdf = t.ContainsKey("btn.exportar.pdf")
                ? t["btn.exportar.pdf"].Texto : "📄 Exportar PDF";
            btnExportSistema.Text  = exportPdf;
            btnExportNegocio.Text  = exportPdf;

            RellenarComboCriticidad(idioma);
            RellenarComboTipoEvento(idioma);
        }

        /// <summary>
        /// Rellena el combo de criticidad con los 8 ítems traducidos al idioma dado,
        /// respetando el índice previamente seleccionado (para que el cambio de idioma
        /// no pierda la selección del usuario).
        /// </summary>
        private void RellenarComboCriticidad(Idioma idioma)
        {
            int idx = cmbCriticidad.SelectedIndex;
            if (idx < 0) idx = 0;
            cmbCriticidad.Items.Clear();
            var t = Traductor.ObtenerTraducciones(idioma);
            string T(string key, string fallback) =>
                t.ContainsKey(key) ? t[key].Texto : fallback;
            cmbCriticidad.Items.Add(T("crit.todas",      "Todas"));
            cmbCriticidad.Items.Add(T("crit.ninguno",    "Ninguno (0)"));
            cmbCriticidad.Items.Add(T("crit.baja",       "Baja (1)"));
            cmbCriticidad.Items.Add(T("crit.media",      "Media (2)"));
            cmbCriticidad.Items.Add(T("crit.alta",       "Alta (3)"));
            cmbCriticidad.Items.Add(T("crit.intlogin",   "Intentos Login (4)"));
            cmbCriticidad.Items.Add(T("crit.recupclave", "Recuperacion Clave (5)"));
            cmbCriticidad.Items.Add(T("crit.bloqueos",   "Bloqueos Cuenta (6)"));
            cmbCriticidad.SelectedIndex =
                (idx >= 0 && idx < cmbCriticidad.Items.Count) ? idx : 0;
        }

        /// <summary>
        /// Rellena el combo de tipo de evento de negocio con ítems traducidos.
        /// Usa _tipoEventoDB como lista paralela de claves reales de BD,
        /// para que el filtro pueda usar el valor correcto aunque el idioma cambie.
        /// </summary>
        private void RellenarComboTipoEvento(Idioma idioma)
        {
            int idx = cmbTipoEvento.SelectedIndex;
            if (idx < 0) idx = 0;
            cmbTipoEvento.Items.Clear();
            _tipoEventoDB.Clear();
            var t = Traductor.ObtenerTraducciones(idioma);
            string T(string key, string fb) => t.ContainsKey(key) ? t[key].Texto : fb;

            void Add(string dbVal, string key, string fb)
            {
                cmbTipoEvento.Items.Add(T(key, fb));
                _tipoEventoDB.Add(dbVal);
            }

            Add("",                   "tevt.todos",          "Todos");
            Add("Venta",              "tevt.venta",          "Venta");
            Add("Cancelacion",        "tevt.cancelacion",    "Cancelación");
            Add("Despacho",           "tevt.despacho",       "Despacho");
            Add("Entrega",            "tevt.entrega",        "Entrega");
            Add("AltaPrenda",         "tevt.altaprenda",     "Alta Prenda");
            Add("ModificacionPrenda", "tevt.modprenda",      "Modificación Prenda");
            Add("CambioEstadoPrenda", "tevt.cambiostprenda", "Cambio Estado Prenda");
            Add("AltaCliente",        "tevt.altacliente",    "Alta Cliente");
            Add("ModificacionCliente","tevt.modcliente",     "Modificación Cliente");
            Add("BajaCliente",        "tevt.bajacliente",    "Baja Cliente");

            cmbTipoEvento.SelectedIndex =
                (idx >= 0 && idx < cmbTipoEvento.Items.Count) ? idx : 0;
        }

        private static void Aplicar(Control c, IDictionary<string, Traduccion> t)
        {
            if (c?.Tag != null && t.ContainsKey(c.Tag.ToString()))
                c.Text = t[c.Tag.ToString()].Texto;
        }

        private void Bitacora_Load(object sender, EventArgs e)
        {
            if (!bllBitacora.UsuarioPuedeVerSistema())
                tabControl.TabPages.Remove(tabPageSistema);
            else
                CargarSistema();

            CargarNegocio();
        }

        private void BtnUltimosDias_Click(object sender, EventArgs e)
        {
            int dias = (int)nudDias.Value;
            DataTable dt = dias == 0
                ? bllBitacora.ObtenerTodosSistema()
                : bllBitacora.ObtenerUltimosNDiasSistema(dias);
            var tU = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
            string contexto = dias > 0
                ? string.Format(tU.ContainsKey("msg.bit.ultimos") ? tU["msg.bit.ultimos"].Texto : "últimos {0} días", dias)
                : (tU.ContainsKey("msg.bit.todos") ? tU["msg.bit.todos"].Texto : "todos los registros");
            MostrarEnGrilla(dgvSistema, lblResultadosSistema, dt, contexto);
        }

        private void BtnLimpiar_Click(object sender, EventArgs e)
        {
            txtUsuario.Text = "0";
            txtActividad.Clear();
            nudDias.Value   = 7;
            // Refill combo first (keeps index 0), then explicitly reset to 0
            RellenarComboCriticidad(GestorIdioma.IdiomaActual);
            cmbCriticidad.SelectedIndex = 0;
            CargarSistema();
        }

        private void BtnExportSistema_Click(object sender, EventArgs e)
        {
            ExportarPdf(dgvSistema, "Bitácora del Sistema — WardrobeFlow");
        }

        private void DgvSistema_DataBindingComplete(object sender,
            System.Windows.Forms.DataGridViewBindingCompleteEventArgs e)
        {
            ColorearPorCriticidad(dgvSistema);
        }

        private void BtnNegUltimosDias_Click(object sender, EventArgs e)
        {
            int dias = (int)nudNegDias.Value;
            DateTime? desde = dias > 0 ? DateTime.Now.AddDays(-dias) : (DateTime?)null;
            var dt = bllBitacora.BuscarPorFiltrosNegocio(desde, null, null, null, null);
            var tUN = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
            string contexto = dias > 0
                ? string.Format(tUN.ContainsKey("msg.bit.ultimos") ? tUN["msg.bit.ultimos"].Texto : "últimos {0} días", dias)
                : (tUN.ContainsKey("msg.bit.todos") ? tUN["msg.bit.todos"].Texto : "todos los registros");
            MostrarEnGrilla(dgvNegocio, lblResultadosNegocio, dt, contexto);
        }

        private void BtnNegLimpiar_Click(object sender, EventArgs e)
        {
            cmbTipoEvento.SelectedIndex = 0;
            txtNegPedido.Text           = "0";
            txtNegCliente.Text          = "0";
            nudNegDias.Value            = 7;
            CargarNegocio();
        }

        private void BtnExportNegocio_Click(object sender, EventArgs e)
        {
            ExportarPdf(dgvNegocio, "Bitácora de Negocio — WardrobeFlow");
        }

        // ── Carga ─────────────────────────────────────────────────────────────

        private void CargarSistema()
        {
            try
            {
                var dt = bllBitacora.ObtenerTodosSistema();
                MostrarEnGrilla(dgvSistema, lblResultadosSistema, dt);
            }
            catch (Exception ex) { MostrarError(ex.Message); }
        }

        private void CargarNegocio()
        {
            try
            {
                var dt = bllBitacora.ObtenerTodosNegocio();
                MostrarEnGrilla(dgvNegocio, lblResultadosNegocio, dt);
            }
            catch (Exception ex) { MostrarError(ex.Message); }
        }

        // ── Eventos ───────────────────────────────────────────────────────────

        private void BtnBuscarSistema_Click(object sender, EventArgs e)
        {
            try
            {
                int dias        = (int)nudDias.Value;
                DateTime? desde = dias > 0 ? DateTime.Now.AddDays(-dias) : (DateTime?)null;
                int uid         = int.TryParse(txtUsuario.Text, out int u) ? u : 0;
                string activ    = txtActividad.Text.Trim();

                int[] criticidadMap = { -1, 0, 1, 2, 3, 4, 5, 6 };
                int criticidad = criticidadMap[cmbCriticidad.SelectedIndex];

                var dt = bllBitacora.BuscarPorFiltrosSistema(desde, null, uid, activ, criticidad);
                MostrarEnGrilla(dgvSistema, lblResultadosSistema, dt);
            }
            catch (Exception ex) { MostrarError(ex.Message); }
        }

        private void BtnBuscarNegocio_Click(object sender, EventArgs e)
        {
            try
            {
                int dias        = (int)nudNegDias.Value;
                DateTime? desde = dias > 0 ? DateTime.Now.AddDays(-dias) : (DateTime?)null;
                int tipoIdx     = cmbTipoEvento.SelectedIndex;
                string tipo     = (tipoIdx <= 0 || tipoIdx >= _tipoEventoDB.Count)
                                    ? null
                                    : _tipoEventoDB[tipoIdx];
                int? idPedido   = int.TryParse(txtNegPedido.Text,  out int p) && p > 0 ? (int?)p : null;
                int? idCliente  = int.TryParse(txtNegCliente.Text, out int c) && c > 0 ? (int?)c : null;

                var dt = bllBitacora.BuscarPorFiltrosNegocio(desde, null, tipo, idCliente, idPedido);
                MostrarEnGrilla(dgvNegocio, lblResultadosNegocio, dt);
            }
            catch (Exception ex) { MostrarError(ex.Message); }
        }

        // ══════════════════════════════════════════════════════════════════════
        // EXPORTAR PDF — PrintDocument + PrintPreviewDialog
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Abre una vista previa de impresión del contenido actual de la grilla.
        /// Desde ahí el usuario puede imprimir directamente o seleccionar
        /// "Microsoft Print to PDF" para guardar como archivo PDF.
        /// </summary>
        private void ExportarPdf(DataGridView dgv, string titulo)
        {
            if (dgv.Rows.Count == 0)
            {
                var tB = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
                string T_b(string k, string fb) => tB.ContainsKey(k) ? tB[k].Texto : fb;
                MessageBox.Show(
                    T_b("err.pdf.sinDatos",  "No hay datos para exportar."),
                    T_b("lbl.exportarpdf",   "Exportar PDF"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Capturar datos y headers traducidos para la impresión.
            // Se usan los HeaderText del DataGridView (ya traducidos) en vez de
            // los ColumnName del DataTable (nombres crudos de BD).
            _tablaImpresion  = (DataTable)dgv.DataSource;
            _tituloImpresion = titulo;
            _headersImpresion = new string[dgv.Columns.Count];
            for (int i = 0; i < dgv.Columns.Count; i++)
                _headersImpresion[i] = dgv.Columns[i].HeaderText;
            _paginaActual    = 1;
            _filaImpresion   = 0;

            _fuenteTitulo = new Font("Segoe UI", 13, FontStyle.Bold);
            _fuenteHeader = new Font("Segoe UI", 8,  FontStyle.Bold);
            _fuenteCelda  = new Font("Segoe UI", 7.5f);

            var doc = new PrintDocument();
            doc.DefaultPageSettings.Landscape = true;
            doc.DefaultPageSettings.Margins   = new Margins(40, 40, 40, 40);
            doc.PrintPage += ImprimirPagina;

            using (var preview = new PrintPreviewDialog())
            {
                preview.Document = doc;
                preview.Width    = 1050;
                preview.Height   = 780;
                preview.Text     = $"Vista Previa — {titulo}";
                preview.ShowDialog(this);
            }
        }

        /// <summary>
        /// Renderiza una página del documento de impresión.
        /// Dibuja el título, encabezados de columna y filas de datos.
        /// Continúa en páginas adicionales si los datos no caben en una sola.
        /// </summary>
        private void ImprimirPagina(object sender, PrintPageEventArgs e)
        {
            Graphics  g      = e.Graphics;
            Rectangle margen = e.MarginBounds;

            float y          = margen.Top;
            float xIzq       = margen.Left;
            float anchoTotal = margen.Width;

            // ── Título (solo en la primera página) ───────────────────────────
            if (_paginaActual == 1)
            {
                g.DrawString(_tituloImpresion, _fuenteTitulo, Brushes.DarkSlateBlue,
                    xIzq, y);
                y += _fuenteTitulo.GetHeight(g) + 4;

                g.DrawString(
                    $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}   |   " +
                    $"{_tablaImpresion.Rows.Count} registro(s)",
                    _fuenteCelda, Brushes.Gray, xIzq, y);
                y += _fuenteCelda.GetHeight(g) + 8;

                // Línea separadora bajo el título
                g.DrawLine(Pens.DarkSlateBlue, xIzq, y, xIzq + anchoTotal, y);
                y += 4;
            }

            // ── Calcular anchos de columna ────────────────────────────────────
            int nCols     = _tablaImpresion.Columns.Count;
            float colAncho = anchoTotal / nCols;

            // ── Encabezados ───────────────────────────────────────────────────
            float alturaHeader = _fuenteHeader.GetHeight(g) + 6;

            using (var brushHeader = new SolidBrush(Color.FromArgb(60, 60, 120)))
            using (var brushHeaderBg = new SolidBrush(Color.FromArgb(220, 220, 240)))
            {
                g.FillRectangle(brushHeaderBg,
                    xIzq, y, anchoTotal, alturaHeader);

                for (int col = 0; col < nCols; col++)
                {
                    // Usar el HeaderText traducido capturado del DataGridView
                    string nombre = (_headersImpresion != null && col < _headersImpresion.Length)
                        ? _headersImpresion[col]
                        : _tablaImpresion.Columns[col].ColumnName;
                    var rect = new RectangleF(
                        xIzq + col * colAncho + 2, y + 2,
                        colAncho - 4, alturaHeader - 4);
                    g.DrawString(nombre, _fuenteHeader, brushHeader, rect,
                        new StringFormat { Trimming = StringTrimming.EllipsisCharacter });
                }
            }
            y += alturaHeader + 2;

            // ── Filas de datos ────────────────────────────────────────────────
            float alturaCelda = _fuenteCelda.GetHeight(g) + 5;
            bool  alternar    = false;

            while (_filaImpresion < _tablaImpresion.Rows.Count)
            {
                // Verificar si cabe otra fila en la página
                if (y + alturaCelda > margen.Bottom - 20) break;

                DataRow fila = _tablaImpresion.Rows[_filaImpresion];

                // Fondo alternado
                if (alternar)
                    g.FillRectangle(new SolidBrush(Color.FromArgb(248, 248, 255)),
                        xIzq, y, anchoTotal, alturaCelda);

                for (int col = 0; col < nCols; col++)
                {
                    string valor = fila[col]?.ToString() ?? "";
                    var rect = new RectangleF(
                        xIzq + col * colAncho + 2, y + 1,
                        colAncho - 4, alturaCelda - 2);
                    g.DrawString(valor, _fuenteCelda, Brushes.Black, rect,
                        new StringFormat { Trimming = StringTrimming.EllipsisCharacter });
                }

                // Línea de separación fina
                g.DrawLine(Pens.LightGray,
                    xIzq, y + alturaCelda, xIzq + anchoTotal, y + alturaCelda);

                y        += alturaCelda;
                alternar  = !alternar;
                _filaImpresion++;
            }

            // ── Pie de página ─────────────────────────────────────────────────
            g.DrawLine(Pens.DarkSlateBlue,
                xIzq, margen.Bottom - 14, xIzq + anchoTotal, margen.Bottom - 14);
            g.DrawString(
                $"WardrobeFlow — Página {_paginaActual}",
                _fuenteCelda, Brushes.Gray,
                xIzq, margen.Bottom - 12);

            // ¿Hay más filas? Entonces hay más páginas
            e.HasMorePages = _filaImpresion < _tablaImpresion.Rows.Count;
            if (e.HasMorePages) _paginaActual++;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void MostrarEnGrilla(DataGridView dgv, Label lbl, DataTable datos, string contexto = null)
        {
            dgv.DataSource = datos;

            // Traducir los headers de columna al idioma activo
            bool esSistema = (dgv == dgvSistema);
            TraducirHeadersGrilla(dgv, GestorIdioma.IdiomaActual, esSistema);

            var tR = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
            string linea1 = string.Format(
                tR.ContainsKey("msg.bit.registros") ? tR["msg.bit.registros"].Texto : "  {0} registro(s)",
                datos.Rows.Count);
            if (!string.IsNullOrEmpty(contexto)) linea1 += $"  —  {contexto}";

            if (dgv == dgvSistema && datos.Columns.Contains("criticidad"))
            {
                lbl.Height = 44;
                lbl.Text   = linea1 + "\r\n  " + ComputarEstadisticasCriticidad(datos, GestorIdioma.IdiomaActual);
            }
            else
            {
                lbl.Height = 24;
                lbl.Text   = linea1;
            }
        }

        /// <summary>
        /// Renombra el HeaderText de las columnas de la grilla de bitácora
        /// según el idioma activo, sin cambiar los nombres internos del DataTable
        /// (ColorearPorCriticidad sigue usando el nombre "criticidad").
        /// </summary>
        private void TraducirHeadersGrilla(DataGridView dgv, Idioma idioma, bool esSistema)
        {
            if (dgv == null || dgv.Columns.Count == 0) return;
            var t = Traductor.ObtenerTraducciones(idioma);

            void RH(string col, string clave)
            {
                if (dgv.Columns.Contains(col) && t.ContainsKey(clave))
                    dgv.Columns[col].HeaderText = t[clave].Texto;
            }

            if (esSistema)
            {
                RH("Id",         "col.bit.id");
                RH("fecha",      "col.bit.fecha");
                RH("usuario",    "col.bit.usuario");
                RH("modulo",     "col.bit.modulo");
                RH("actividad",  "col.bit.actividad");
                RH("detalle",    "col.bit.detalle");
                RH("criticidad", "col.bit.criticidad");
                RH("ip",         "col.bit.ip");
            }
            else
            {
                RH("IdEvento",        "col.neg.idevento");
                RH("Fecha",           "col.neg.fecha");
                RH("Tipo",            "col.neg.tipo");
                RH("UsernameUsuario", "col.neg.usuario");
                RH("NombreCliente",   "col.neg.cliente");
                RH("IdPedido",        "col.neg.idpedido");
                RH("IdPrenda",        "col.neg.idprenda");
                RH("IdCliente",       "col.neg.idcliente");
                RH("Descripcion",     "col.neg.desc");
            }
        }

        private string ComputarEstadisticasCriticidad(DataTable datos, Idioma idioma = null)
        {
            var conteos = new int[7];
            foreach (DataRow row in datos.Rows)
            {
                if (int.TryParse(row["criticidad"]?.ToString(), out int c) && c >= 0 && c < 7)
                    conteos[c]++;
            }

            var t = Traductor.ObtenerTraducciones(idioma ?? GestorIdioma.IdiomaActual);
            string[] claves = { "stat.ninguno", "stat.baja", "stat.media", "stat.alta",
                                 "stat.intlogin", "stat.recupclave", "stat.bloqueos" };
            string[] fallback = { "Ninguno", "Baja", "Media", "Alta", "Int.Login", "Recup.Clave", "Bloqueos" };

            var partes = new List<string>();
            for (int i = 0; i < 7; i++)
                if (conteos[i] > 0)
                {
                    string etiqueta = t.ContainsKey(claves[i]) ? t[claves[i]].Texto : fallback[i];
                    partes.Add($"{etiqueta}: {conteos[i]}");
                }

            return partes.Count > 0
                ? string.Join("   |   ", partes)
                : "Sin datos de criticidad";
        }

        private void ColorearPorCriticidad(DataGridView dgv)
        {
            if (!dgv.Columns.Contains("criticidad")) return;

            foreach (DataGridViewRow fila in dgv.Rows)
            {
                if (fila.IsNewRow) continue;
                if (!int.TryParse(fila.Cells["criticidad"].Value?.ToString(), out int crit)) continue;

                Color back, fore;
                switch (crit)
                {
                    case 0:  back = Color.FromArgb(245, 245, 245); fore = Color.Gray;           break;
                    case 1:  back = Color.FromArgb(220, 255, 220); fore = Color.DarkGreen;      break;
                    case 2:  back = Color.FromArgb(255, 255, 200); fore = Color.DarkGoldenrod;  break;
                    case 3:  back = Color.FromArgb(255, 220, 170); fore = Color.DarkOrange;     break;
                    case 4:  back = Color.FromArgb(255, 205, 205); fore = Color.DarkRed;        break;
                    case 5:  back = Color.FromArgb(210, 225, 255); fore = Color.DarkBlue;       break;
                    case 6:  back = Color.FromArgb(200, 0,   20);  fore = Color.White;          break;
                    default: continue;
                }
                fila.DefaultCellStyle.BackColor = back;
                fila.DefaultCellStyle.ForeColor = fore;
            }
        }

    }
}
