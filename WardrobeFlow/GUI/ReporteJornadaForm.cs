using BLL;
using Servicios.Multiidioma;
using System;
using System.Collections.Generic;
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

        public ReporteJornadaForm(List<BE.Permiso> permisos)
        {
            InitializeComponent();
        }

        private void ReporteJornadaForm_Load(object sender, EventArgs e)
        {
            GestorIdioma.SuscribirObservador(this);
            Traducir(GestorIdioma.IdiomaActual);

            dtpJornada.Value  = DateTime.Today;
            dtpJornada2.Value = DateTime.Today.AddDays(-1);

            CrearBannerKPIs();
            ConfigurarMenusExportar();
            GenerarReporte();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            GestorIdioma.DesuscribirObservador(this);
            base.OnFormClosing(e);
        }

        // ── IIdiomaObserver ───────────────────────────────────────────────────

        public void UpdateLanguage(Idioma idioma) => Traducir(idioma);

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

            if (_kpiPrendasLbl != null) _kpiPrendasLbl.Text = T("rpt.kpi.prendas",  "Prendas disponibles");
            if (_kpiClientesLbl != null) _kpiClientesLbl.Text = T("rpt.kpi.clientes", "Clientes registrados");
            if (_kpiEventosLbl != null) _kpiEventosLbl.Text = T("rpt.kpi.eventos",  "Eventos del día");
            if (_kpiBackupLbl != null) _kpiBackupLbl.Text = T("rpt.kpi.backup",   "días sin backup");
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
                using (var br = new SolidBrush(Color.FromArgb(146, 62, 96)))
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
                    ForeColor = Color.FromArgb(146, 62, 96),
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
            _menuExportar = new ContextMenuStrip();
            _menuExportar.Items.Add("Guardar como .TXT", null,
                (s, e) => ExportarContenido(esComparacion: false));
            _menuExportar.Items.Add("Imprimir / Exportar PDF", null,
                (s, e) => ImprimirReporte());

            _menuExportarComp = new ContextMenuStrip();
            _menuExportarComp.Items.Add("Guardar comparación como .TXT", null,
                (s, e) => ExportarContenido(esComparacion: true));
            _menuExportarComp.Items.Add("Imprimir / Exportar PDF", null,
                (s, e) => ImprimirReporte());
        }

        private void ExportarContenido(bool esComparacion)
        {
            try
            {
                string nombreBase = esComparacion
                    ? $"Comparacion_{dtpJornada.Value:yyyyMMdd}_vs_{dtpJornada2.Value:yyyyMMdd}"
                    : $"ReporteJornada_{dtpJornada.Value:yyyyMMdd}";

                using (var dlg = new SaveFileDialog())
                {
                    dlg.Title            = "Guardar como TXT";
                    dlg.Filter           = "Archivo de texto (*.txt)|*.txt";
                    dlg.FileName         = $"{nombreBase}.txt";
                    dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                    if (dlg.ShowDialog() != DialogResult.OK) return;

                    File.WriteAllText(dlg.FileName, rtbReporte.Text, Encoding.UTF8);

                    lblStatus.Text = $"Exportado: {Path.GetFileName(dlg.FileName)}";
                    MessageBox.Show($"Archivo guardado:\n{dlg.FileName}", "Éxito",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
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
                string[] lines     = rtbReporte.Lines;
                int      lineIndex = 0;
                int      pageNum   = 0;

                var vinoOscuro = Color.FromArgb(146, 62, 96);
                var vinoMedio  = Color.FromArgb(110, 40, 70);

                using (var pd = new PrintDocument())
                {
                    pd.DocumentName = $"ReporteJornada_{dtpJornada.Value:yyyyMMdd}";
                    pd.DefaultPageSettings.Margins = new System.Drawing.Printing.Margins(40, 40, 40, 40);

                    pd.PrintPage += (s, e) =>
                    {
                        pageNum++;
                        var g      = e.Graphics;
                        var margen = e.MarginBounds;
                        float y    = margen.Top;

                        using (var fuenteTitulo  = new Font("Segoe UI", 13f, FontStyle.Bold))
                        using (var fuenteSub     = new Font("Segoe UI", 8f, FontStyle.Regular))
                        using (var fuenteCuerpo  = new Font("Consolas", 8.5f))
                        using (var brVino        = new SolidBrush(vinoOscuro))
                        using (var brMedio       = new SolidBrush(vinoMedio))
                        using (var brTexto       = new SolidBrush(Color.FromArgb(40, 15, 28)))
                        using (var penLinea      = new Pen(vinoOscuro, 1.5f))
                        using (var penPie        = new Pen(vinoOscuro, 1f))
                        {
                            if (pageNum == 1)
                            {
                                // Barra de título vino
                                float headerH = fuenteTitulo.GetHeight(g) + 14;
                                using (var brHeaderBg = new SolidBrush(vinoOscuro))
                                    g.FillRectangle(brHeaderBg, margen.Left, y, margen.Width, headerH);
                                g.DrawString(
                                    $"Reporte de Jornada — {dtpJornada.Value:dd/MM/yyyy}",
                                    fuenteTitulo, Brushes.White,
                                    margen.Left + 6, y + 5);
                                y += headerH + 4;

                                g.DrawString(
                                    $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}",
                                    fuenteSub, brMedio, margen.Left, y);
                                y += fuenteSub.GetHeight(g) + 4;
                                g.DrawLine(penLinea, margen.Left, y, margen.Right, y);
                                y += 6;
                            }

                            float lineH = fuenteCuerpo.GetHeight(g);
                            while (lineIndex < lines.Length)
                            {
                                if (y + lineH > margen.Bottom - 20) break;
                                g.DrawString(lines[lineIndex], fuenteCuerpo, brTexto, margen.Left, y);
                                lineIndex++;
                                y += lineH;
                            }

                            // Pie de página
                            g.DrawLine(penPie, margen.Left, margen.Bottom - 16, margen.Right, margen.Bottom - 16);
                            g.DrawString(
                                $"WardrobeFlow — Página {pageNum}",
                                fuenteSub, brMedio, margen.Left, margen.Bottom - 14);
                        }

                        e.HasMorePages = lineIndex < lines.Length;
                    };

                    using (var dlg = new PrintDialog { Document = pd, AllowSomePages = false })
                    {
                        if (dlg.ShowDialog(this) == DialogResult.OK)
                        {
                            lineIndex = 0;
                            pageNum   = 0;
                            pd.Print();
                            lblStatus.Text = "Impresión enviada.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error al imprimir", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                string texto = _servicio.GenerarComparacion(
                    dtpJornada.Value.Date, dtpJornada2.Value.Date);
                rtbReporte.Text = texto;
                lblStatus.Text  = $"Comparación generada — {DateTime.Now:HH:mm:ss}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GenerarReporte()
        {
            try
            {
                DateTime fecha = dtpJornada.Value.Date;
                rtbReporte.Text = _servicio.Generar(fecha);
                lblStatus.Text  = $"Reporte generado — {DateTime.Now:HH:mm:ss}";
                if (_kpiPrendasVal != null) ActualizarKPIs(fecha);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
