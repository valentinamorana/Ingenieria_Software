using System;

namespace BE
{
    public class HistorialIntegridad
    {
        public int      Id                { get; set; }
        public string   NombreTabla       { get; set; }
        public int?     DVVAlmacenado     { get; set; }
        public int      DVVCalculado      { get; set; }
        public bool     Resultado         { get; set; }
        public int      FilasCorruptas    { get; set; }
        public DateTime FechaVerificacion { get; set; }
        public string   DisparadoPor      { get; set; }
    }
}
