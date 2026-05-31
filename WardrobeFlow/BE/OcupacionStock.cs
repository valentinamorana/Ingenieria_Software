namespace BE
{
    /// <summary>
    /// DTO con el resumen de ocupación del stock de prendas.
    /// Usado por el Dashboard para mostrar el KPI de ocupación global.
    /// </summary>
    public class OcupacionStock
    {
        public int Total       { get; set; }
        public int EnUso       { get; set; }
        public int EnLimpieza  { get; set; }
        public int Disponibles { get; set; }

        // Porcentaje de prendas actualmente en uso (0-100).
        public int PorcentajeOcupacion =>
            Total > 0 ? (int)((EnUso * 100.0) / Total) : 0;
    }
}
