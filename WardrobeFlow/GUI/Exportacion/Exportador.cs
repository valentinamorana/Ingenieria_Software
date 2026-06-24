using System.Windows.Forms;

namespace GUI.Exportacion
{
    /// <summary>
    /// Patrón FACTORY METHOD — rol "Product" (Producto abstracto).
    ///
    /// Define la operación común a todos los exportadores de reportes. Cada producto
    /// concreto (<see cref="ExportadorPdf"/>, <see cref="ExportadorTxt"/>) sabe llevar
    /// un <see cref="ReporteExportable"/> a SU formato. El cliente trabaja contra esta
    /// abstracción y nunca instancia los concretos: los pide a un GeneradorReporte.
    ///
    /// Análogo al "Transporte" del ejercicio de la cátedra: guarda en campos protegidos
    /// el formato y el origen (qué reporte lo fabricó), igual que Camion guardaba _medio
    /// y _sucursal.
    /// </summary>
    public abstract class Exportador
    {
        protected string _formato;   // "PDF" / "TXT"  (como _medio en el parcial)
        protected string _origen;    // "Bitácora" / "Reporte de Jornada"  (como _sucursal)

        /// <summary>Formato del producto concreto (informativo).</summary>
        public string Formato => _formato;

        /// <summary>
        /// Exporta el reporte a este formato. Devuelve la ruta del archivo escrito,
        /// o null si no se generó archivo (vista previa, sin datos o cancelado).
        /// </summary>
        public abstract string Exportar(ReporteExportable reporte, IWin32Window propietario);
    }
}
