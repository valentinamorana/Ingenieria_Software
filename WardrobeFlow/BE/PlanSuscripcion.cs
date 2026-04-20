namespace BE
{
    /// <summary>
    /// Entidad — Plan de Suscripción.
    /// Define los tipos de planes disponibles con su límite de prendas y precio.
    /// Mapea la tabla [PlanSuscripcion].
    /// </summary>
    public class PlanSuscripcion
    {
        public int     IdPlan        { get; set; }
        public string  Nombre        { get; set; }

        /// <summary>Cantidad máxima de prendas que puede tener el cliente al mismo tiempo.</summary>
        public int     LimitePrendas { get; set; }

        public decimal Precio        { get; set; }
        public bool    Estado        { get; set; } = true;
    }
}
