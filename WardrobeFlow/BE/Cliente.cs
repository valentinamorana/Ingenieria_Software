using System;

namespace BE
{
    /// <summary>
    /// Entidad — Cliente (suscriptor del servicio).
    /// Los clientes NO son usuarios del sistema: no tienen login.
    /// Son registros en la BD que el Vendedor crea y gestiona.
    /// Mapea la tabla [Cliente].
    /// </summary>
    public class Cliente
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

        public DateTime         FechaAlta      { get; set; }

        /// <summary>Cantidad de prendas actualmente en uso por este cliente.</summary>
        public int              StockUtilizado { get; set; }

        /// <summary>Límite de prendas del plan (cargado por JOIN, no persiste).</summary>
        public int              LimitePrendas  { get; set; }

        /// <summary>Nombre completo para mostrar en grillas.</summary>
        public string NombreCompleto => $"{Nombre} {Apellido}";
    }
}
