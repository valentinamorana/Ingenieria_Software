using System.Data;
using System.Text;

namespace Servicios.Exportacion
{
    /// <summary>
    /// LÓGICA de serialización a CSV (RFC 4180). Vive en la capa de Servicios — NO en la GUI:
    /// los exportadores de la presentación solo piden el texto y lo escriben a disco.
    ///
    /// Reglas de escapado: un campo se encierra entre comillas dobles si contiene el
    /// separador, comillas o saltos de línea; las comillas internas se duplican.
    /// </summary>
    public static class SerializadorCsv
    {
        private const char Separador = ';'; // ';' = el Excel en es-AR lo abre en columnas directo

        /// <summary>Serializa encabezados + filas de un DataTable a texto CSV.</summary>
        public static string Generar(string[] encabezados, DataTable datos)
        {
            var sb = new StringBuilder();

            if (encabezados != null && encabezados.Length > 0)
                sb.AppendLine(UnirFila(encabezados));

            if (datos != null)
            {
                foreach (DataRow fila in datos.Rows)
                {
                    var celdas = new string[datos.Columns.Count];
                    for (int col = 0; col < datos.Columns.Count; col++)
                        celdas[col] = fila[col]?.ToString() ?? string.Empty;
                    sb.AppendLine(UnirFila(celdas));
                }
            }

            return sb.ToString();
        }

        private static string UnirFila(string[] campos)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < campos.Length; i++)
            {
                if (i > 0) sb.Append(Separador);
                sb.Append(Escapar(campos[i] ?? string.Empty));
            }
            return sb.ToString();
        }

        private static string Escapar(string campo)
        {
            bool necesitaComillas =
                campo.IndexOf(Separador) >= 0 ||
                campo.IndexOf('"') >= 0 ||
                campo.IndexOf('\n') >= 0 ||
                campo.IndexOf('\r') >= 0;

            if (!necesitaComillas) return campo;
            return "\"" + campo.Replace("\"", "\"\"") + "\"";
        }
    }
}
