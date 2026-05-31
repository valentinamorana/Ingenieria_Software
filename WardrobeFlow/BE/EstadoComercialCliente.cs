namespace BE
{
    /// <summary>
    /// DTO que la BLL devuelve con el estado comercial de un cliente para crear pedidos.
    /// No contiene lógica — es solo transporte de datos hacia la GUI.
    /// </summary>
    public class EstadoComercialCliente
    {
        // true si el cliente puede avanzar a seleccionar prendas
        public bool PuedeProceder { get; set; }

        // Código de bloqueo: "SIN_PLAN" | "SUSCRIPCION_VENCIDA" | null si puede proceder
        public string MotivoBloqueo { get; set; }

        // Datos del plan para mostrar en el resumen de la GUI
        public string NombrePlan      { get; set; }
        public int    StockUtilizado  { get; set; }
        public int    LimitePrendas   { get; set; }
        public string MetodoPago      { get; set; }
        public System.DateTime FechaAlta { get; set; }
        public System.DateTime? FechaVencimiento { get; set; }

        // Resultado de la validación de cantidad de prendas solicitadas
        public bool SuperaLimite       { get; set; }
        public int  PrendasDisponibles { get; set; }
        public int  Exceso             { get; set; }

        // Alerta proactiva: suscripción vigente pero vence pronto
        public bool SuscripcionProximaAVencer { get; set; }
        public int  DiasHastaVencimiento      { get; set; }
    }
}
