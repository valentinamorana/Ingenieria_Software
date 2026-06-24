using System.Data;

namespace GUI.Exportacion
{
    /// <summary>
    /// Modelo neutro de un reporte a exportar (NO es un rol del patrón: es el dato
    /// que el cliente le pasa al Producto). Un mismo reporte puede ser:
    ///   • TABULAR  → trae <see cref="Encabezados"/> + <see cref="Datos"/> (ej: Bitácora)
    ///   • DE TEXTO → trae <see cref="TextoPlano"/> (ej: Reporte de Jornada)
    /// Así un único Exportador sabe representar cualquiera de los dos reportes.
    /// </summary>
    public class ReporteExportable
    {
        /// <summary>Título que encabeza el PDF / nombre lógico del reporte.</summary>
        public string Titulo { get; set; }

        /// <summary>Nombre base sugerido del archivo (sin extensión) para "Guardar como".</summary>
        public string NombreArchivo { get; set; }

        /// <summary>Encabezados de columna ya traducidos (null ⇒ el reporte es de texto).</summary>
        public string[] Encabezados { get; set; }

        /// <summary>Filas tabulares (null ⇒ el reporte es de texto).</summary>
        public DataTable Datos { get; set; }

        /// <summary>Cuerpo de texto plano (para reportes de texto: TXT y PDF).</summary>
        public string TextoPlano { get; set; }

        /// <summary>True si el reporte trae estructura tabular (columnas + filas).</summary>
        public bool EsTabular => Encabezados != null && Datos != null;
    }
}
