namespace GUI.Exportacion
{
    /// <summary>
    /// Patrón FACTORY METHOD — rol "ConcreteCreator".
    ///
    /// Creador de exportadores para el Reporte de Jornada (reporte de texto). Decide
    /// qué producto concreto instanciar según el formato pedido y lo marca con su origen.
    /// </summary>
    public class GeneradorJornada : GeneradorReporte
    {
        private const string Origen = "Reporte de Jornada";

        public override Exportador CrearExportador(string formato)
        {
            if (formato == "pdf")
                return new ExportadorPdf(Origen);
            else if (formato == "txt")
                return new ExportadorTxt(Origen);
            else if (formato == "csv")
                return new ExportadorCsv(Origen);
            else
                return null;
        }
    }
}
