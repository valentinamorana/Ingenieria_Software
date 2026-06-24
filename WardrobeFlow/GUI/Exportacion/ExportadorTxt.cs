using System;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Servicios.Multiidioma;

namespace GUI.Exportacion
{
    /// <summary>
    /// Patrón FACTORY METHOD — rol "ConcreteProduct".
    ///
    /// Exporta un reporte a un archivo de texto plano (.txt) mediante un diálogo
    /// "Guardar como". Si el reporte es de texto usa su cuerpo tal cual; si es
    /// tabular lo serializa con encabezados y filas separadas por tabulaciones.
    /// </summary>
    public class ExportadorTxt : Exportador
    {
        public ExportadorTxt(string origen)
        {
            _formato = "TXT";
            _origen  = origen;
        }

        public override string Exportar(ReporteExportable reporte, IWin32Window propietario)
        {
            var t = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
            string T(string k, string fb) => t.ContainsKey(k) ? t[k].Texto : fb;

            using (var dlg = new SaveFileDialog())
            {
                dlg.Title            = T("rpt.dlg.guardartxt", "Guardar como TXT");
                dlg.Filter           = "Archivo de texto (*.txt)|*.txt";
                dlg.FileName         = $"{reporte.NombreArchivo}.txt";
                dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                if (dlg.ShowDialog(propietario) != DialogResult.OK) return null;

                string contenido = reporte.EsTabular
                    ? SerializarTabular(reporte)
                    : (reporte.TextoPlano ?? string.Empty);

                File.WriteAllText(dlg.FileName, contenido, Encoding.UTF8);

                MessageBox.Show(
                    string.Format(T("rpt.dlg.exito.msg", "Archivo guardado:\n{0}"), dlg.FileName),
                    T("rpt.dlg.exito.titulo", "Éxito"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                return dlg.FileName;
            }
        }

        private static string SerializarTabular(ReporteExportable reporte)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(reporte.Titulo))
            {
                sb.AppendLine(reporte.Titulo);
                sb.AppendLine();
            }
            sb.AppendLine(string.Join("\t", reporte.Encabezados));

            DataTable tabla = reporte.Datos;
            foreach (DataRow fila in tabla.Rows)
            {
                var celdas = new string[tabla.Columns.Count];
                for (int col = 0; col < tabla.Columns.Count; col++)
                    celdas[col] = fila[col]?.ToString() ?? "";
                sb.AppendLine(string.Join("\t", celdas));
            }
            return sb.ToString();
        }
    }
}
