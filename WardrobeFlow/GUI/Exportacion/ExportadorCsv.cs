using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Servicios.Multiidioma;

namespace GUI.Exportacion
{
    /// <summary>
    /// Patrón FACTORY METHOD — rol "ConcreteProduct" (nuevo, extiende el patrón sin tocar
    /// los productos existentes).
    ///
    /// Exporta un reporte a un archivo CSV. La LÓGICA de serialización vive en
    /// <see cref="Servicios.Exportacion.SerializadorCsv"/> (capa de Servicios); aquí solo
    /// hay presentación: el diálogo "Guardar como" y la escritura del archivo.
    /// Se escribe con BOM UTF-8 para que Excel abra bien los acentos.
    /// </summary>
    public class ExportadorCsv : Exportador
    {
        public ExportadorCsv(string origen)
        {
            _formato = "CSV";
            _origen  = origen;
        }

        public override string Exportar(ReporteExportable reporte, IWin32Window propietario)
        {
            var t = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
            string T(string k, string fb) => t.ContainsKey(k) ? t[k].Texto : fb;

            if (reporte.EsTabular && (reporte.Datos == null || reporte.Datos.Rows.Count == 0))
            {
                MessageBox.Show(
                    T("err.pdf.sinDatos", "No hay datos para exportar."),
                    T("rpt.menu.guardarcsv", "Guardar como .CSV"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }

            using (var dlg = new SaveFileDialog())
            {
                dlg.Title            = T("rpt.dlg.guardarcsv", "Guardar como CSV");
                dlg.Filter           = "CSV (*.csv)|*.csv";
                dlg.FileName         = $"{reporte.NombreArchivo}.csv";
                dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                if (dlg.ShowDialog(propietario) != DialogResult.OK) return null;

                string contenido = reporte.EsTabular
                    ? Servicios.Exportacion.SerializadorCsv.Generar(reporte.Encabezados, reporte.Datos)
                    : (reporte.TextoPlano ?? string.Empty);

                // UTF-8 con BOM → Excel reconoce la codificación y muestra acentos/cirílico correctos.
                File.WriteAllText(dlg.FileName, contenido, new UTF8Encoding(true));

                MessageBox.Show(
                    string.Format(T("rpt.dlg.exito.msg", "Archivo guardado:\n{0}"), dlg.FileName),
                    T("rpt.dlg.exito.titulo", "Éxito"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                return dlg.FileName;
            }
        }
    }
}
