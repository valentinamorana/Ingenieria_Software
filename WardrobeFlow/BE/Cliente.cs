using System;

namespace BE
{
    /// <summary>Entidad Cliente. Mapea la tabla [Cliente].</summary>
    public class Cliente : Entidad
    {
        public int IdCliente { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string DNI { get; set; }
        public string Email { get; set; }
        public string MetodoPago { get; set; } = "Efectivo";

        // Null si no tiene plan asignado.
        public int? IdPlan { get; set; }

        // Nombre del plan (cargado por JOIN, no persiste) La tabla no guarda NombrePlan, ya está en la tabla PlanSuscripcion
        public string NombrePlan { get; set; }

        // Cantidad de prendas actualmente en uso por este cliente.
        public int StockUtilizado { get; set; }

        // Límite de prendas del plan (cargado por JOIN, no persiste).
        public int LimitePrendas { get; set; }

        // Nombre completo para mostrar en grillas.
        public string NombreCompleto => $"{Nombre} {Apellido}";

        public override int GetId() => IdCliente;

        // Comportamiento 
        // Indica si el cliente tiene un plan de suscripción asignado.
        public bool TienePlan() => IdPlan.HasValue;

        // Indica si el cliente puede solicitar la cantidad de nuevas prendas indicada sin superar el límite de su plan.
        public bool PuedeSolicitarPrendas(int cantidadNuevas)
            => TienePlan() && (StockUtilizado + cantidadNuevas) <= LimitePrendas;

        // Cantidad de prendas adicionales que el plan aún permite.
        public int PrendasDisponiblesEnPlan()
            => TienePlan() ? Math.Max(0, LimitePrendas - StockUtilizado) : 0;
    }
}
