using System;

namespace BE
{
    /// <summary>
    /// Entidad — Cliente (suscriptor del servicio).
    /// Los clientes NO son usuarios del sistema: no tienen login.
    /// Son registros en la BD que el Vendedor crea y gestiona.
    /// Mapea la tabla [Cliente].
    /// </summary>
    /// <summary>
    /// Hereda de <see cref="Entidad"/>:
    ///   - FechaAlta  → fecha de registro del suscriptor (heredada)
    ///   - GetId()    → retorna IdCliente (implementación concreta)
    /// </summary>
    public class Cliente : Entidad
    {
        public int              IdCliente      { get; set; }
        public string           Nombre         { get; set; }
        public string           Apellido       { get; set; }
        public string           DNI            { get; set; }
        public string           Email          { get; set; }
        public string           MetodoPago     { get; set; } = "Efectivo";

        /// <summary>FK → PlanSuscripcion. Null si aún no tiene plan asignado.</summary>
        public int?             IdPlan         { get; set; }

        /// <summary>Nombre del plan (cargado por JOIN, no persiste).</summary>
        public string           NombrePlan     { get; set; }

        /// <summary>Cantidad de prendas actualmente en uso por este cliente.</summary>
        public int              StockUtilizado { get; set; }

        /// <summary>Límite de prendas del plan (cargado por JOIN, no persiste).</summary>
        public int              LimitePrendas  { get; set; }

        /// <summary>Nombre completo para mostrar en grillas.</summary>
        public string NombreCompleto => $"{Nombre} {Apellido}";

        /// <summary>Implementación de Entidad.GetId() → retorna la clave primaria de Cliente.</summary>
        public override int GetId() => IdCliente;

        // ── Comportamiento ────────────────────────────────────────────────────

        /// <summary>Indica si el cliente tiene un plan de suscripción asignado.</summary>
        public bool TienePlan() => IdPlan.HasValue;

        /// <summary>
        /// Indica si el cliente puede solicitar la cantidad de nuevas prendas indicada
        /// sin superar el límite de su plan.
        /// </summary>
        public bool PuedeSolicitarPrendas(int cantidadNuevas)
            => TienePlan() && (StockUtilizado + cantidadNuevas) <= LimitePrendas;

        /// <summary>Cantidad de prendas adicionales que el plan aún permite.</summary>
        public int PrendasDisponiblesEnPlan()
            => TienePlan() ? Math.Max(0, LimitePrendas - StockUtilizado) : 0;
    }
}
