namespace BE
{
    /// <summary>
    /// Entidad — Plan de Suscripción.
    /// Define los tipos de planes disponibles con su límite de prendas y precio.
    /// Mapea la tabla [PlanSuscripcion].
    /// </summary>
    public class PlanSuscripcion
    {
        public int IdPlan { get; set; }
        public string Nombre { get; set; }

        /// Cantidad máxima de prendas que puede tener el cliente al mismo tiempo.
        public int LimitePrendas { get; set; }

        public decimal Precio { get; set; }
        public bool Estado { get; set; } = true;

        // Comportamiento
        public bool PermiteAgregarPrendas(int enUso, int nuevas)
            => (enUso + nuevas) <= LimitePrendas;

        // Cantidad de prendas adicionales que el plan aún permite dado el uso actual.
        public int LugaresDisponibles(int enUso)
            => System.Math.Max(0, LimitePrendas - enUso);
    }
}
