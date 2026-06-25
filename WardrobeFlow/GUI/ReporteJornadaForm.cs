using BLL;
using Servicios.Multiidioma;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace GUI
{
    public partial class ReporteJornadaForm : Form, IIdiomaObserver
    {
        private readonly ReporteJornada _servicio = new ReporteJornada();

        // KPI banner value labels (created in code, like the primer parcial)
        private Label _kpiPrendasVal, _kpiClientesVal, _kpiEventosVal, _kpiBackupVal;
        private Label _kpiPrendasLbl, _kpiClientesLbl, _kpiEventosLbl, _kpiBackupLbl;

        // Context menus for export buttons
        private ContextMenuStrip _menuExportar;
        private ContextMenuStrip _menuExportarComp;

        // Botón "Tendencia" creado por código (no se toca el Designer).
        private Button _btnTendencia;

        private bool _esComparacion = false;

        public ReporteJornadaForm(List<BE.Permiso> permisos)
        {
            InitializeComponent();
        }

        private void ReporteJornadaForm_Load(object sender, EventArgs e)
        {
            try { string ico = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico"); if (System.IO.File.Exists(ico)) this.Icon = new System.Drawing.Icon(ico); } catch { }
            // Gradient on header panel
            panelTop.Paint += (s, pe) =>
            {
                using (var br = new System.Drawing.Drawing2D.LinearGradientBrush(
                    panelTop.ClientRectangle,
                    System.Drawing.Color.FromArgb(176, 62, 96),
                    System.Drawing.Color.FromArgb(242, 114, 153),
                    System.Drawing.Drawing2D.LinearGradientMode.Horizontal))
                    pe.Graphics.FillRectangle(br, panelTop.ClientRectangle);
            };
            panelTop.Invalidate();
            GestorIdioma.SuscribirObservador(this);
            Traducir(GestorIdioma.IdiomaActual);

            dtpJornada.Value  = DateTime.Today;
            dtpJornada2.Value = DateTime.Today.AddDays(-1);

            btnExportarComp.Visible = false;
            CrearBannerKPIs();
            ConfigurarMenusExportar();
            CrearBotonTendencia();
            GenerarReporte();
        }

        // Crea el botón "Tendencia" por código y lo ubica en el espacio libre de la
        // primera fila del panel (a la derecha de "Exportar"). No se modifica el Designer.
        private void CrearBotonTendencia()
        {
            var t = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
            string Tv(string k, string fb) => t.ContainsKey(k) ? t[k].Texto : fb;

            _btnTendencia = new Button
            {
                Text      = "📈  " + Tv("rpt.tendencia", "Tendencia (rango)"),
                Location  = new Point(545, 12),
                Size      = new Size(175, 28),
                BackColor = Color.FromArgb(150, 70, 105),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9F, FontStyle.Bold),
                UseVisualStyleBackColor = false
            };
            _btnTendencia.FlatAppearance.BorderSize = 0;
            _btnTendencia.Click += (s, e) => MostrarTendencia();
            panelControles.Controls.Add(_btnTendencia);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            GestorIdioma.DesuscribirObservador(this);
            base.OnFormClosing(e);
        }

        // ── IIdiomaObserver ───────────────────────────────────────────────────

        public void UpdateLanguage(Idioma idioma)
        {
            Traducir(idioma);
            if (_esComparacion)
                btnComparar_Click(null, EventArgs.Empty);
            else
                GenerarReporte();
        }

        private void Traducir(Idioma idioma)
        {
            var t = Traductor.ObtenerTraducciones(idioma);
            string T(string k, string fb) => t.ContainsKey(k) ? t[k].Texto : fb;

            this.Text          = T("frm.reportejornada", "Reporte de Jornada — WardrobeFlow");
            lblTitulo.Text     = T("frm.reportejornada", "Reporte de Jornada — WardrobeFlow");
            lblSubtitulo.Text  = T("rpt.subtitulo",      "Eventos de negocio por jornada con exportación a TXT");
            lblJornada.Text    = T("rpt.fecha",          "Jornada:");
            lblComparar.Text   = T("rpt.fecha2",         "Comparar con:");
            btnGenerar.Text    = "↻  " + T("rpt.generar",     "Generar");
            btnComparar.Text   = "⚖  " + T("rpt.comparar",    "Comparar jornadas");
            btnExportar.Text   = "⬇  " + T("rpt.exportartxt", "Exportar TXT") + "...";
            btnExportarComp.Text = "⬇  " + T("rpt.exportartxt", "Exportar TXT") + "...";
            btnLimpiar.Text    = "↩  " + T("rpt.limpiar",     "Limpiar");
            if (_btnTendencia != null) _btnTendencia.Text = "📈  " + T("rpt.tendencia", "Tendencia (rango)");

            if (_kpiPrendasLbl != null) _kpiPrendasLbl.Text = T("rpt.kpi.prendas",  "Prendas disponibles");
            if (_kpiClientesLbl != null) _kpiClientesLbl.Text = T("rpt.kpi.clientes", "Clientes registrados");
            if (_kpiEventosLbl != null) _kpiEventosLbl.Text = T("rpt.kpi.eventos",  "Eventos del día");
            if (_kpiBackupLbl != null) _kpiBackupLbl.Text = T("rpt.kpi.backup",   "días sin backup");

            if (_menuExportar != null && _menuExportar.Items.Count >= 3)
            {
                _menuExportar.Items[0].Text = T("rpt.menu.guardartxt",  "Guardar como .TXT");
                _menuExportar.Items[1].Text = T("rpt.menu.imprimir",    "Imprimir / Exportar PDF");
                _menuExportar.Items[2].Text = T("rpt.menu.guardarcsv",  "Guardar eventos como .CSV");
            }
            if (_menuExportarComp != null && _menuExportarComp.Items.Count >= 2)
            {
                _menuExportarComp.Items[0].Text = T("rpt.menu.guardarcmp", "Guardar comparación como .TXT");
                _menuExportarComp.Items[1].Text = T("rpt.menu.imprimir",   "Imprimir / Exportar PDF");
            }
        }

        // ── KPI Banner ────────────────────────────────────────────────────────

        private void CrearBannerKPIs()
        {
            const int panelH = 66;
            const int n      = 4;
            int cellW = panelControles.Width / n;

            var banner = new Panel
            {
                Location  = new Point(panelControles.Left, panelControles.Bottom + 4),
                Size      = new Size(panelControles.Width, panelH),
                BackColor = Color.FromArgb(250, 236, 244)
            };

            banner.Paint += (s, pe) =>
            {
                using (var br = new SolidBrush(Color.FromArgb(176, 62, 96)))
                    pe.Graphics.FillRectangle(br, 0, panelH - 3, banner.Width, 3);
                using (var pen = new Pen(Color.FromArgb(220, 180, 200), 1))
                    for (int i = 1; i < n; i++)
                        pe.Graphics.DrawLine(pen, i * cellW, 8, i * cellW, panelH - 10);
            };

            var valLabels = new Label[n];
            var titLabels = new Label[n];

            for (int i = 0; i < n; i++)
            {
                int x = i * cellW;

                var lblVal = new Label
                {
                    Text      = "—",
                    Location  = new Point(x, 5),
                    Size      = new Size(cellW, 34),
                    Font      = new Font("Segoe UI", 15F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(80, 28, 52),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Transparent
                };
                var lblTit = new Label
                {
                    Text      = "",
                    Location  = new Point(x, 39),
                    Size      = new Size(cellW, 20),
                    Font      = new Font("Segoe UI", 7.5F, FontStyle.Regular),
                    ForeColor = Color.FromArgb(176, 62, 96),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Transparent
                };

                banner.Controls.Add(lblVal);
                banner.Controls.Add(lblTit);
                valLabels[i] = lblVal;
                titLabels[i] = lblTit;
            }

            _kpiPrendasVal  = valLabels[0];  _kpiPrendasLbl  = titLabels[0];
            _kpiClientesVal = valLabels[1];  _kpiClientesLbl = titLabels[1];
            _kpiEventosVal  = valLabels[2];  _kpiEventosLbl  = titLabels[2];
            _kpiBackupVal   = valLabels[3];  _kpiBackupLbl   = titLabels[3];

            _kpiPrendasLbl.Text  = "Prendas disponibles";
            _kpiClientesLbl.Text = "Clientes registrados";
            _kpiEventosLbl.Text  = "Eventos del día";
            _kpiBackupLbl.Text   = "días sin backup";

            this.Controls.Add(banner);
            banner.BringToFront();

            // Push rtbReporte down to make room for the banner
            rtbReporte.Top    = banner.Bottom + 4;
            rtbReporte.Height = panelStatus.Top - rtbReporte.Top - 4;
        }

        private void ActualizarKPIs(DateTime fecha)
        {
            try
            {
                _kpiPrendasVal.Text  = _servicio.ContarPrendasDisponibles().ToString();
                _kpiClientesVal.Text = _servicio.ContarClientes().ToString();
                _kpiEventosVal.Text  = _servicio.ContarEventosDia(fecha).ToString();

                int dias = _servicio.ObtenerDiasSinBackup();
                _kpiBackupVal.Text = dias < 0 ? "!" : dias.ToString();
            }
            catch { /* no interrumpir el reporte */ }
        }

        // ── Menús de exportación ──────────────────────────────────────────────

        private void ConfigurarMenusExportar()
        {
            var t = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
            string Tv(string k, string fb) => t.ContainsKey(k) ? t[k].Texto : fb;

            _menuExportar = new ContextMenuStrip();
            _menuExportar.Items.Add(Tv("rpt.menu.guardartxt", "Guardar como .TXT"), null,
                (s, e) => ExportarContenido(esComparacion: false));
            _menuExportar.Items.Add(Tv("rpt.menu.imprimir", "Imprimir / Exportar PDF"), null,
                (s, e) => ImprimirReporte());
            _menuExportar.Items.Add(Tv("rpt.menu.guardarcsv", "Guardar eventos como .CSV"), null,
                (s, e) => ExportarEventosCsv());

            _menuExportarComp = new ContextMenuStrip();
            _menuExportarComp.Items.Add(Tv("rpt.menu.guardarcmp", "Guardar comparación como .TXT"), null,
                (s, e) => ExportarContenido(esComparacion: true));
            _menuExportarComp.Items.Add(Tv("rpt.menu.imprimir", "Imprimir / Exportar PDF"), null,
                (s, e) => ImprimirReporte());
        }

        private void ExportarContenido(bool esComparacion)
        {
            try
            {
                var lbl = ConstruirLblReporte();
                string nombreBase = esComparacion
                    ? $"Comparacion_{dtpJornada.Value:yyyyMMdd}_vs_{dtpJornada2.Value:yyyyMMdd}"
                    : $"ReporteJornada_{dtpJornada.Value:yyyyMMdd}";

                var t = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
                string Tk(string k, string fb) => t.ContainsKey(k) ? t[k].Texto : fb;

                var reporte = new Exportacion.ReporteExportable
                {
                    Titulo        = $"{Tk("frm.reportejornada", "Reporte de Jornada")} — {dtpJornada.Value:dd/MM/yyyy}",
                    NombreArchivo = nombreBase,
                    TextoPlano    = rtbReporte.Text
                };

                // Creator → Factory Method → Product (Exportador concreto a .TXT)
                Exportacion.GeneradorReporte generador  = new Exportacion.GeneradorJornada();
                Exportacion.Exportador       exportador = generador.CrearExportador("txt");
                string ruta = exportador.Exportar(reporte, this);

                if (ruta != null)
                    lblStatus.Text = $"{lbl["rptoegenerado"]}: {Path.GetFileName(ruta)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error al exportar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ImprimirReporte()
        {
            try
            {
                var t = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
                string Tk(string k, string fb) => t.ContainsKey(k) ? t[k].Texto : fb;

                var reporte = new Exportacion.ReporteExportable
                {
                    Titulo        = $"{Tk("frm.reportejornada", "Reporte de Jornada")} — {dtpJornada.Value:dd/MM/yyyy}",
                    NombreArchivo = $"ReporteJornada_{dtpJornada.Value:yyyyMMdd}",
                    TextoPlano    = rtbReporte.Text
                };

                // Creator → Factory Method → Product (Exportador concreto a PDF)
                Exportacion.GeneradorReporte generador  = new Exportacion.GeneradorJornada();
                Exportacion.Exportador       exportador = generador.CrearExportador("pdf");
                exportador.Exportar(reporte, this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error al imprimir", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Genera la TENDENCIA de actividad en el rango [Comparar con … Jornada].
        // La agregación por día vive en BLL.ReporteJornada; la GUI solo la muestra.
        private void MostrarTendencia()
        {
            try
            {
                var lbl = ConstruirLblReporte();
                rtbReporte.Text = _servicio.GenerarTendencia(
                    dtpJornada2.Value.Date, dtpJornada.Value.Date, lbl);
                lblStatus.Text  = $"{lbl["rptoegenerado"]} — {DateTime.Now:HH:mm:ss}";
                _esComparacion  = false;
                btnExportarComp.Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Exporta los eventos de negocio de la jornada como CSV tabular (Factory Method).
        // Los datos los provee BLL.ReporteJornada; acá solo se arma el reporte y se exporta.
        private void ExportarEventosCsv()
        {
            try
            {
                var t = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
                string Tk(string k, string fb) => t.ContainsKey(k) ? t[k].Texto : fb;

                DateTime fecha    = dtpJornada.Value.Date;
                DataTable eventos = _servicio.ObtenerEventosDelDia(fecha);

                if (eventos == null || eventos.Rows.Count == 0)
                {
                    MessageBox.Show(
                        Tk("err.pdf.sinDatos", "No hay datos para exportar."),
                        Tk("rpt.menu.guardarcsv", "Guardar eventos como .CSV"),
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var headers = new string[eventos.Columns.Count];
                for (int i = 0; i < eventos.Columns.Count; i++)
                    headers[i] = eventos.Columns[i].ColumnName;

                var reporte = new Exportacion.ReporteExportable
                {
                    Titulo        = $"{Tk("frm.reportejornada", "Reporte de Jornada")} — {fecha:dd/MM/yyyy}",
                    NombreArchivo = $"EventosJornada_{fecha:yyyyMMdd}",
                    Encabezados   = headers,
                    Datos         = eventos
                };

                Exportacion.GeneradorReporte generador  = new Exportacion.GeneradorJornada();
                Exportacion.Exportador       exportador = generador.CrearExportador("csv");
                exportador?.Exportar(reporte, this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error al exportar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Handlers ─────────────────────────────────────────────────────────

        private void btnGenerar_Click(object sender, EventArgs e) => GenerarReporte();

        private void dtpJornada_ValueChanged(object sender, EventArgs e) => GenerarReporte();

        private void btnExportar_Click(object sender, EventArgs e)
        {
            _menuExportar?.Show(btnExportar, new Point(0, btnExportar.Height));
        }

        private void btnExportarComp_Click(object sender, EventArgs e)
        {
            _menuExportarComp?.Show(btnExportarComp, new Point(0, btnExportarComp.Height));
        }

        private void btnLimpiar_Click(object sender, EventArgs e) => GenerarReporte();

        private void btnComparar_Click(object sender, EventArgs e)
        {
            try
            {
                var lbl = ConstruirLblReporte();
                string texto = _servicio.GenerarComparacion(
                    dtpJornada.Value.Date, dtpJornada2.Value.Date, lbl);
                rtbReporte.Text = texto;
                lblStatus.Text  = $"{lbl["compgenerada"]} — {DateTime.Now:HH:mm:ss}";
                _esComparacion = true;
                btnExportarComp.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private IDictionary<string, string> ConstruirLblReporte()
        {
            var t = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
            string Tv(string key, string fb) => t.ContainsKey(key) ? t[key].Texto : fb;
            return new Dictionary<string, string>
            {
                { "titulo",        Tv("rpt.txt.titulo",       "REPORTE DE JORNADA")                                    },
                { "resumen",       Tv("rpt.txt.resumen",      "RESUMEN DEL SISTEMA")                                   },
                { "prendas",       Tv("rpt.txt.prendas",      "Prendas disponibles")                                   },
                { "clientes",      Tv("rpt.txt.clientes",     "Clientes registrados")                                  },
                { "diassinbkp",    Tv("rpt.txt.diassinbkp",   "Días sin backup")                                       },
                { "sinbackups",    Tv("rpt.txt.sinbackups",   "Sin backups")                                           },
                { "eventos",       Tv("rpt.txt.eventos",      "EVENTOS DE NEGOCIO DEL DÍA")                            },
                { "sinevt",        Tv("rpt.txt.sinevt",       "(sin eventos registrados para esta jornada)")            },
                { "usuario",       Tv("rpt.txt.usuario",      "Usuario")                                               },
                { "cliente",       Tv("rpt.txt.cliente",      "Cliente")                                               },
                { "totalevt",      Tv("rpt.txt.totalevt",     "TOTAL EVENTOS")                                         },
                { "generado",      Tv("rpt.txt.generado",     "Generado")                                              },
                { "comparacion",   Tv("rpt.txt.comparacion",  "COMPARACIÓN DE JORNADAS")                               },
                { "jornada",       Tv("rpt.txt.jornada",      "JORNADA")                                               },
                { "sinevtjorn",    Tv("rpt.txt.sinevtjorn",   "(sin eventos registrados en esta jornada)")              },
                { "comparfinal",   Tv("rpt.txt.comparfinal",  "COMPARATIVO FINAL")                                     },
                { "fecha",         Tv("rpt.txt.fecha",        "Fecha")                                                 },
                { "eventostot",    Tv("rpt.txt.eventostot",   "Eventos totales")                                       },
                { "masmasa",       Tv("rpt.txt.masmasa",      "tuvo más actividad")                                    },
                { "ninguna",       Tv("rpt.txt.ninguna",      "Ninguna jornada tuvo eventos registrados.")              },
                { "iguales",       Tv("rpt.txt.iguales",      "Ambas jornadas tuvieron la misma cantidad de eventos.") },
                { "rptoegenerado", Tv("rpt.txt.rptoegenerado","Reporte generado")                                      },
                { "compgenerada",  Tv("rpt.txt.compgenerada", "Comparación generada")                                  },
                { "impresionenv",  Tv("rpt.txt.impresionenv", "Impresión enviada")                                     },
                { "tend.titulo",   Tv("rpt.txt.tend.titulo",  "TENDENCIA DE ACTIVIDAD")                                },
                { "tend.dias",     Tv("rpt.txt.tend.dias",    "Días analizados")                                       },
                { "tend.total",    Tv("rpt.txt.tend.total",   "Total de eventos")                                      },
                { "tend.promedio", Tv("rpt.txt.tend.promedio","Promedio diario")                                       },
                { "tend.diapico",  Tv("rpt.txt.tend.diapico", "Día de mayor actividad")                                },
                { "tend.diavalle", Tv("rpt.txt.tend.diavalle","Día de menor actividad")                                },
                { "tend.detalle",  Tv("rpt.txt.tend.detalle", "DETALLE POR DÍA")                                       },
            };
        }

        private void GenerarReporte()
        {
            try
            {
                DateTime fecha = dtpJornada.Value.Date;
                var lbl = ConstruirLblReporte();
                rtbReporte.Text = _servicio.Generar(fecha, lbl);
                lblStatus.Text  = $"{lbl["rptoegenerado"]} — {DateTime.Now:HH:mm:ss}";
                _esComparacion = false;
                btnExportarComp.Visible = false;
                if (_kpiPrendasVal != null) ActualizarKPIs(fecha);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
