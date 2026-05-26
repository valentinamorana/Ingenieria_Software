using System;

namespace BE
{
    public class MantenimientoPrenda
    {
        public int       IdMantenimiento { get; set; }
        public int       IdPrenda        { get; set; }
        public string    NombrePrenda    { get; set; }
        public DateTime  FechaEntrada    { get; set; }
        public DateTime? FechaSalida     { get; set; }
        public string    Actor           { get; set; }

        public bool EstaAbierto => !FechaSalida.HasValue;

        public int? DuracionDias => FechaSalida.HasValue
            ? (int?)(FechaSalida.Value.Date - FechaEntrada.Date).TotalDays
            : null;
    }
}
