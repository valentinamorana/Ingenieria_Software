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

        private readonly string _tabInicial;
        private Panel _leyendaCriticidad;

        public Bitacora(string tabInicial = null)
        {
            InitializeComponent();
            _tabInicial = tabInicial;
        }

        public void SeleccionarTab(string nombre)
        {
            if (nombre == "negocio" && tabControl.TabPages.Contains(tabPageNegocio))
                tabControl.SelectedTab = tabPageNegocio;
            else if (nombre == "sistema" && tabControl.TabPages.Contains(tabPageSistema))
                tabControl.SelectedTab = tabPageSistema;
        }

        // ── Observer de idioma ────────────────────────────────────────────────

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GestorIdioma.SuscribirObservador(this);
            Traducir(GestorIdioma.IdiomaActual);  // calls RellenarComboCriticidad internally
            InicializarDisenoMockup();
        }

        private void InicializarDisenoMockup()
        {
            // ── Gradient en panelTop ──────────────────────────────────────────
            panelTop.Paint += (s, pe) =>
            {
                using (var br = new System.Drawing.Drawing2D.LinearGradientBrush(
                    panelTop.ClientRectangle,
                    Color.FromArgb(176, 62, 96),
                    Color.FromArgb(242, 114, 153),
                    System.Drawing.Drawing2D.LinearGradientMode.Horizontal))
                    pe.Graphics.FillRectangle(br, panelTop.ClientRectangle);
            };
            panelTop.Invalidate();

            // ── Colores de headers DGV ────────────────────────────────────────
            foreach (var dgv in new[] { dgvSistema, dgvNegocio })
            {
                dgv.EnableHeadersVisualStyles = false;
                dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(176, 62, 96);
                dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                dgv.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold);
                dgv.GridColor = Color.FromArgb(230, 210, 220);
            }

            // ── Leyenda de criticidad en tab Sistema ──────────────────────────
            var leyenda = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 28,
                BackColor = Color.FromArgb(245, 238, 242)
            };
            leyenda.Paint += (s, pe) =>
            {
                var g = pe.Graphics;
                int x = 10;
                var tr = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
                string Tl(string key, string fb) => tr.ContainsKey(key) ? tr[key].Texto : fb;
                // (trad-key, fallback, back, fore)
                var niveles = new (string key, string fb, Color back, Color fore)[]
                {
                    ("stat.ninguno",    "Ninguno",     Color.FromArgb(245,245,245), Color.Gray),
                    ("stat.baja",       "Baja",        Color.FromArgb(220,255,220), Color.DarkGreen),
                    ("stat.media",      "Media",       Color.FromArgb(255,255,200), Color.DarkGoldenrod),
                    ("stat.alta",       "Alta",        Color.FromArgb(255,220,170), Color.DarkOrange),
                    ("stat.intlogin",   "Int.Login",   Color.FromArgb(255,205,205), Color.DarkRed),
                    ("stat.recupclave", "Recup.Clave", Color.FromArgb(210,225,255), Color.DarkBlue),
                    ("stat.bloqueos",   "Bloqueos",    Color.FromArgb(200,0,20),    Color.White),
                };
                using (var fnt = new Font("Segoe UI", 7.5f))
                {
                    foreach (var n in niveles)
                    {
                        using (var br = new SolidBrush(n.back))
                            g.FillRectangle(br, x, 7, 10, 10);
                        g.DrawRectangle(Pens.Gray, x, 7, 10, 10);
                        x += 13;
                        string etiqueta = Tl(n.key, n.fb);
                        using (var br = new SolidBrush(Color.FromArgb(60,40,50)))
                            g.DrawString(etiqueta, fnt, br, x, 6);
                        x += (int)g.MeasureString(etiqueta, fnt).Width + 6;
                    }
                }
            };

            // Insertar leyenda DEBAJO del panelFiltrosSistema dentro de tabPageSistema
            _leyendaCriticidad = leyenda;
            tabPageSistema.Controls.Add(leyenda);
            tabPageSistema.Controls.SetChildIndex(leyenda, tabPageSistema.Controls.IndexOf(panelFiltrosSistema));

            // ── Row coloring para Negocio ─────────────────────────────────────
            dgvNegocio.DataBindingComplete += (s, e2) => ColorearPorTipoNegocio();

            // ── Status bar inferior ───────────────────────────────────────────
            var sbar = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 26,
                BackColor = Color.FromArgb(176, 62, 96)
            };
            var lblSb = new Label
            {
                Dock      = DockStyle.Fill,
                ForeColor = Color.FromArgb(244, 212, 226),
                Font      = new Font("Segoe UI", 8f),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(10, 0, 0, 0)
            };
            try
            {
                var u = new BLL.Usuario().ObtenerUsuarioActivo();
                lblSb.Text = u != null ? $"{u.Username}  ·  {u.Perfil ?? "—"}" : "";
            }
            catch { }
            sbar.Controls.Add(lblSb);
            this.Controls.Add(sbar);
        }

        private void ColorearPorTipoNegocio()
        {
            foreach (DataGridViewRow fila in dgvNegocio.Rows)
            {
                if (fila.IsNewRow) continue;
                string tipo = fila.Cells["Tipo"]?.Value?.ToString() ?? "";
                Color back, fore;
                switch (tipo)
                {
                    case "Venta":               back = Color.FromArgb(225, 240, 255); fore = Color.FromArgb(30,100,170);  break;
                    case "Despacho":            back = Color.FromArgb(225, 240, 255); fore = Color.FromArgb(30,100,170);  break;
                    case "Entrega":             back = Color.FromArgb(220, 248, 220); fore = Color.FromArgb(30,130,30);   break;
                    case "Cancelacion":         back = Color.FromArgb(255, 225, 225); fore = Color.FromArgb(160,50,50);   break;
                    case "AltaCliente":         back = Color.FromArgb(225, 248, 225); fore = Color.FromArgb(30,130,30);   break;
                    case "ModificacionCliente": back = Color.FromArgb(240, 232, 255); fore = Color.FromArgb(100,80,160);  break;
                    case "AltaPrenda":          back = Color.FromArgb(225, 248, 225); fore = Color.FromArgb(30,130,30);   break;
                    case "ModificacionPrenda":  back = Color.FromArgb(255, 240, 225); fore = Color.FromArgb(160,100,20);  break;
                    case "CambioEstadoPrenda":  back = Color.FromArgb(225, 240, 255); fore = Color.FromArgb(30,100,170);  break;
                    default: continue;
                }
                fila.DefaultCellStyle.BackColor = back;
                fila.DefaultCellStyle.ForeColor = fore;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            GestorIdioma.DesuscribirObservador(this);
            base.OnFormClosing(e);
        }

        public void UpdateLanguage(Idioma idioma)
        {
            Traducir(idioma);   // includes RellenarComboCriticidad
            TraducirHeadersGrilla(dgvSistema, idioma, esSistema: true);
            TraducirHeadersGrilla(dgvNegocio, idioma, esSistema: false);
            ActualizarLabelEstadisticas(dgvSistema, lblResultadosSistema);
            ActualizarLabelEstadisticas(dgvNegocio, lblResultadosNegocio);
            _leyendaCriticidad?.Invalidate();   // repinta leyenda con nuevo idioma
        }

        private void ActualizarLabelEstadisticas(DataGridView dgv, Label lbl)
        {
            var datos = dgv?.DataSource as DataTable;
            if (datos == null) return;
            var tR = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
            string linea1 = string.Format(
                tR.ContainsKey("msg.bit.registros") ? tR["msg.bit.registros"].Texto : "  {0} registro(s)",
                datos.Rows.Count);
            if (dgv == dgvSistema && datos.Columns.Contains("criticidad"))
            {
                lbl.Height = 44;
                lbl.Text   = linea1 + "\r\n  " + ComputarEstadisticasCriticidad(datos, GestorIdioma.IdiomaActual);
            }
            else if (dgv == dgvNegocio && datos.Columns.Contains("Tipo"))
            {
                lbl.Height = 44;
                string resumen = ComputarEstadisticasTipoEvento(datos, GestorIdioma.IdiomaActual);
                lbl.Text = string.IsNullOrEmpty(resumen) ? linea1 : linea1 + "\r\n  " + resumen;
            }
            else
            {
                lbl.Height = 44;
                lbl.Text   = linea1;
            }
        }

        private void Traducir(Idioma idioma)
        {
            var t = Traductor.ObtenerTraducciones(idioma);
            string Tv(string k, string fb) => t.ContainsKey(k) ? t[k].Texto : fb;

            if (this.Tag != null && t.ContainsKey(this.Tag.ToString()))
                this.Text = t[this.Tag.ToString()].Texto;
            lblBitTitulo.Text    = Tv("frm.bitacora",          "Auditoría — Bitácoras");
            lblBitSubtitulo.Text = Tv("frm.bitacora.subtitulo","Registro de eventos del sistema y operaciones de negocio");
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
            SeleccionarTab(_tabInicial);
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

        private string T(string key, string fallback)
        {
            var t = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
            return t.ContainsKey(key) ? t[key].Texto : fallback;
        }

        private void BtnExportSistema_Click(object sender, EventArgs e)
        {
            MostrarMenuExportar((Control)sender, dgvSistema,
                T("bit.pdf.titulosistema", "Bitácora del Sistema — WardrobeFlow"));
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
            MostrarMenuExportar((Control)sender, dgvNegocio,
                T("bit.pdf.titulonegocio", "Bitácora de Negocio — WardrobeFlow"));
        }

        // ── Carga ─────────────────────────────────────────────────────────────

        private void CargarSistema()
        {
            try
            {
                var dt = bllBitacora.ObtenerTodosSistema();
                MostrarEnGrilla(dgvSistema, lblResultadosSistema, dt);
            }
            catch (Exception ex) { MostrarError(ex); }
        }

        private void CargarNegocio()
        {
            try
            {
                var dt = bllBitacora.ObtenerTodosNegocio();
                MostrarEnGrilla(dgvNegocio, lblResultadosNegocio, dt);
            }
            catch (Exception ex) { MostrarError(ex); }
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
            catch (Exception ex) { MostrarError(ex); }
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
            catch (Exception ex) { MostrarError(ex); }
        }

        // ══════════════════════════════════════════════════════════════════════
        // EXPORTAR PDF — Patrón Factory Method (ver GUI/Exportacion)
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Despliega un menú con los formatos de exportación disponibles (PDF / CSV)
        /// junto al botón de exportar. Cada opción usa el mismo Factory Method.
        /// </summary>
        private void MostrarMenuExportar(Control ancla, DataGridView dgv, string titulo)
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add(T("bit.menu.exportarpdf", "Exportar a PDF"), null,
                (s, e) => ExportarReporte(dgv, titulo, "pdf"));
            menu.Items.Add(T("bit.menu.exportarcsv", "Exportar a CSV"), null,
                (s, e) => ExportarReporte(dgv, titulo, "csv"));
            menu.Show(ancla, new Point(0, ancla.Height));
        }

        /// <summary>
        /// Arma el reporte tabular (con los HeaderText ya traducidos del DataGridView)
        /// y delega la exportación en el producto que fabrica el GeneradorBitacora
        /// (Factory Method). El render del PDF/CSV vive en GUI.Exportacion — este
        /// formulario es solo el cliente del patrón.
        /// </summary>
        private void ExportarReporte(DataGridView dgv, string titulo, string formato)
        {
            var headers = new string[dgv.Columns.Count];
            for (int i = 0; i < dgv.Columns.Count; i++)
                headers[i] = dgv.Columns[i].HeaderText;

            var reporte = new Exportacion.ReporteExportable
            {
                Titulo        = titulo,
                NombreArchivo = titulo,
                Encabezados   = headers,
                Datos         = dgv.DataSource as DataTable
            };

            // Creator → Factory Method → Product (Exportador concreto según el formato)
            Exportacion.GeneradorReporte generador  = new Exportacion.GeneradorBitacora();
            Exportacion.Exportador       exportador = generador.CrearExportador(formato);
            exportador?.Exportar(reporte, this);
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
            else if (dgv == dgvNegocio && datos.Columns.Contains("Tipo"))
            {
                lbl.Height = 44;
                string resumen = ComputarEstadisticasTipoEvento(datos, GestorIdioma.IdiomaActual);
                lbl.Text = string.IsNullOrEmpty(resumen) ? linea1 : linea1 + "\r\n  " + resumen;
            }
            else
            {
                lbl.Height = 44;
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
                : (t.ContainsKey("stat.sindatos") ? t["stat.sindatos"].Texto : "Sin datos de criticidad");
        }

        private string ComputarEstadisticasTipoEvento(DataTable datos, Idioma idioma)
        {
            var conteos = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (DataRow row in datos.Rows)
            {
                string tipo = row["Tipo"]?.ToString() ?? "";
                if (string.IsNullOrEmpty(tipo)) continue;
                if (!conteos.ContainsKey(tipo)) conteos[tipo] = 0;
                conteos[tipo]++;
            }

            var t = Traductor.ObtenerTraducciones(idioma);
            var mapa = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Venta",               "tevt.venta"          },
                { "Cancelacion",         "tevt.cancelacion"    },
                { "Despacho",            "tevt.despacho"       },
                { "Entrega",             "tevt.entrega"        },
                { "AltaPrenda",          "tevt.altaprenda"     },
                { "ModificacionPrenda",  "tevt.modprenda"      },
                { "CambioEstadoPrenda",  "tevt.cambiostprenda" },
                { "AltaCliente",         "tevt.altacliente"    },
                { "ModificacionCliente", "tevt.modcliente"     },
                { "BajaCliente",         "tevt.bajacliente"    },
            };

            var partes = new List<string>();
            foreach (var kv in conteos)
            {
                if (mapa.TryGetValue(kv.Key, out string clave) && t.ContainsKey(clave))
                    partes.Add($"{t[clave].Texto}: {kv.Value}");
                else
                    partes.Add($"{kv.Key}: {kv.Value}");
            }
            return string.Join("   |   ", partes);
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
