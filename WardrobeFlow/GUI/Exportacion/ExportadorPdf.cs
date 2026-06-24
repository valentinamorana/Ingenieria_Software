using System;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using Servicios.Multiidioma;

namespace GUI.Exportacion
{
    /// <summary>
    /// Patrón FACTORY METHOD — rol "ConcreteProduct".
    ///
    /// Exporta un reporte a PDF mediante <see cref="PrintDocument"/> + vista previa
    /// (desde la cual el usuario imprime o elige "Microsoft Print to PDF"). Sabe
    /// representar tanto reportes TABULARES (grilla, ej. Bitácora) como de TEXTO
    /// (líneas monoespaciadas, ej. Reporte de Jornada). El encabezado, el pie
    /// "Página N" y la vista previa —antes duplicados en cada formulario— viven
    /// ahora en un solo lugar.
    /// </summary>
    public class ExportadorPdf : Exportador
    {
        // Paleta vino del sistema (idéntica a la que usaban los formularios).
        private static readonly Color VinoOscuro = Color.FromArgb(176, 62, 96);
        private static readonly Color VinoClaro  = Color.FromArgb(252, 228, 235);
        private static readonly Color VinoMedio  = Color.FromArgb(110, 40, 70);

        // Estado de paginación (instancia nueva por exportación → sin estado compartido).
        private ReporteExportable _reporte;
        private int _pagina;
        private int _fila;        // próxima fila tabular a imprimir
        private string[] _lineas; // líneas de texto (modo texto)
        private int _linea;       // próxima línea de texto a imprimir

        public ExportadorPdf(string origen)
        {
            _formato = "PDF";
            _origen  = origen;
        }

        public override string Exportar(ReporteExportable reporte, IWin32Window propietario)
        {
            _reporte = reporte;

            var t = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
            string T(string k, string fb) => t.ContainsKey(k) ? t[k].Texto : fb;

            if (reporte.EsTabular && (reporte.Datos == null || reporte.Datos.Rows.Count == 0))
            {
                MessageBox.Show(
                    T("err.pdf.sinDatos", "No hay datos para exportar."),
                    T("lbl.exportarpdf",  "Exportar PDF"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }

            if (!reporte.EsTabular)
                _lineas = (reporte.TextoPlano ?? string.Empty)
                          .Replace("\r\n", "\n").Split('\n');

            using (var doc = new PrintDocument())
            {
                doc.DocumentName = reporte.NombreArchivo ?? reporte.Titulo ?? "Reporte";
                doc.DefaultPageSettings.Landscape = reporte.EsTabular; // tabular en horizontal
                doc.DefaultPageSettings.Margins   = new Margins(40, 40, 40, 40);
                doc.BeginPrint += (s, e) => { _pagina = 1; _fila = 0; _linea = 0; };
                doc.PrintPage  += (s, e) =>
                {
                    if (reporte.EsTabular) DibujarPaginaTabular(e, T);
                    else                   DibujarPaginaTexto(e, T);
                };

                using (var preview = new PrintPreviewDialog())
                {
                    preview.Document = doc;
                    preview.Width    = 1050;
                    preview.Height   = 780;
                    preview.Text     = $"{T("bit.pdf.vistaprevia", "Vista Previa")} — {reporte.Titulo}";
                    preview.ShowDialog(propietario);
                }
            }

            return null; // el PDF lo materializa el usuario desde la vista previa
        }

        // ── Modo TABULAR (grilla con encabezados + filas alternadas) ──────────────
        private void DibujarPaginaTabular(PrintPageEventArgs e, Func<string, string, string> T)
        {
            Graphics  g      = e.Graphics;
            Rectangle margen = e.MarginBounds;
            float y          = margen.Top;
            float xIzq       = margen.Left;
            float anchoTotal = margen.Width;

            DataTable tabla = _reporte.Datos;

            using (var fuenteTitulo = new Font("Segoe UI", 13, FontStyle.Bold))
            using (var fuenteHeader = new Font("Segoe UI", 8,  FontStyle.Bold))
            using (var fuenteCelda  = new Font("Segoe UI", 7.5f))
            {
                // Título (solo en la primera página)
                if (_pagina == 1)
                {
                    using (var brTitulo = new SolidBrush(VinoOscuro))
                        g.DrawString(_reporte.Titulo, fuenteTitulo, brTitulo, xIzq, y);
                    y += fuenteTitulo.GetHeight(g) + 4;

                    using (var brSub = new SolidBrush(VinoMedio))
                        g.DrawString(
                            $"{T("rpt.txt.generado", "Generado")}: {DateTime.Now:dd/MM/yyyy HH:mm}   |   " +
                            string.Format(T("msg.bit.registros", "{0} registro(s)"), tabla.Rows.Count),
                            fuenteCelda, brSub, xIzq, y);
                    y += fuenteCelda.GetHeight(g) + 6;

                    using (var penLinea = new Pen(VinoOscuro, 1.5f))
                        g.DrawLine(penLinea, xIzq, y, xIzq + anchoTotal, y);
                    y += 5;
                }

                int nCols      = tabla.Columns.Count;
                float colAncho = anchoTotal / nCols;

                // Encabezados
                float alturaHeader = fuenteHeader.GetHeight(g) + 8;
                using (var brushHeaderBg = new SolidBrush(VinoOscuro))
                using (var brushHeaderFg = new SolidBrush(Color.White))
                {
                    g.FillRectangle(brushHeaderBg, xIzq, y, anchoTotal, alturaHeader);
                    for (int col = 0; col < nCols; col++)
                    {
                        string nombre = (_reporte.Encabezados != null && col < _reporte.Encabezados.Length)
                            ? _reporte.Encabezados[col]
                            : tabla.Columns[col].ColumnName;
                        var rect = new RectangleF(xIzq + col * colAncho + 3, y + 3, colAncho - 6, alturaHeader - 6);
                        g.DrawString(nombre, fuenteHeader, brushHeaderFg, rect,
                            new StringFormat { Trimming = StringTrimming.EllipsisCharacter });
                    }
                }
                y += alturaHeader + 2;

                // Filas
                float alturaCelda = fuenteCelda.GetHeight(g) + 5;
                bool  alternar    = false;
                using (var brushAlternar = new SolidBrush(VinoClaro))
                using (var brushTexto    = new SolidBrush(Color.FromArgb(40, 15, 28)))
                using (var penSeparador  = new Pen(Color.FromArgb(220, 180, 200), 0.5f))
                {
                    while (_fila < tabla.Rows.Count)
                    {
                        if (y + alturaCelda > margen.Bottom - 20) break;
                        DataRow fila = tabla.Rows[_fila];
                        if (alternar)
                            g.FillRectangle(brushAlternar, xIzq, y, anchoTotal, alturaCelda);
                        for (int col = 0; col < nCols; col++)
                        {
                            string valor = fila[col]?.ToString() ?? "";
                            var rect = new RectangleF(xIzq + col * colAncho + 3, y + 1, colAncho - 6, alturaCelda - 2);
                            g.DrawString(valor, fuenteCelda, brushTexto, rect,
                                new StringFormat { Trimming = StringTrimming.EllipsisCharacter });
                        }
                        g.DrawLine(penSeparador, xIzq, y + alturaCelda, xIzq + anchoTotal, y + alturaCelda);
                        y += alturaCelda;
                        alternar = !alternar;
                        _fila++;
                    }
                }

                DibujarPie(g, margen, fuenteCelda, T);
            }

            e.HasMorePages = _fila < tabla.Rows.Count;
            if (e.HasMorePages) _pagina++;
        }

        // ── Modo TEXTO (líneas monoespaciadas con barra de título) ────────────────
        private void DibujarPaginaTexto(PrintPageEventArgs e, Func<string, string, string> T)
        {
            Graphics  g      = e.Graphics;
            Rectangle margen = e.MarginBounds;
            float y          = margen.Top;

            using (var fuenteTitulo = new Font("Segoe UI", 13f, FontStyle.Bold))
            using (var fuenteSub    = new Font("Segoe UI", 8f, FontStyle.Regular))
            using (var fuenteCuerpo = new Font("Consolas", 8.5f))
            using (var brMedio      = new SolidBrush(VinoMedio))
            using (var brTexto      = new SolidBrush(Color.FromArgb(40, 15, 28)))
            using (var penLinea     = new Pen(VinoOscuro, 1.5f))
            {
                if (_pagina == 1)
                {
                    float headerH = fuenteTitulo.GetHeight(g) + 14;
                    using (var brHeaderBg = new SolidBrush(VinoOscuro))
                        g.FillRectangle(brHeaderBg, margen.Left, y, margen.Width, headerH);
                    g.DrawString(_reporte.Titulo, fuenteTitulo, Brushes.White, margen.Left + 6, y + 5);
                    y += headerH + 4;

                    g.DrawString(
                        $"{T("rpt.txt.generado", "Generado")}: {DateTime.Now:dd/MM/yyyy HH:mm}",
                        fuenteSub, brMedio, margen.Left, y);
                    y += fuenteSub.GetHeight(g) + 4;
                    g.DrawLine(penLinea, margen.Left, y, margen.Right, y);
                    y += 6;
                }

                float lineH = fuenteCuerpo.GetHeight(g);
                while (_linea < _lineas.Length)
                {
                    if (y + lineH > margen.Bottom - 20) break;
                    g.DrawString(_lineas[_linea], fuenteCuerpo, brTexto, margen.Left, y);
                    _linea++;
                    y += lineH;
                }

                DibujarPie(g, margen, fuenteSub, T);
            }

            e.HasMorePages = _linea < _lineas.Length;
            if (e.HasMorePages) _pagina++;
        }

        // Pie de página común a ambos modos (antes duplicado en cada formulario).
        private void DibujarPie(Graphics g, Rectangle margen, Font fuente, Func<string, string, string> T)
        {
            using (var penPie = new Pen(VinoOscuro, 1f))
            using (var brPie  = new SolidBrush(VinoMedio))
            {
                g.DrawLine(penPie, margen.Left, margen.Bottom - 16, margen.Right, margen.Bottom - 16);
                g.DrawString(
                    string.Format(T("bit.pdf.pagina", "WardrobeFlow — Página {0}"), _pagina),
                    fuente, brPie, margen.Left, margen.Bottom - 14);
            }
        }
    }
}
