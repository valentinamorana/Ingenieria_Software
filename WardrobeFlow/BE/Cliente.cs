using System;

namespace BE
{
    /// <summary>Entidad Cliente. Mapea la tabla [Cliente].</summary>
    public class Cliente
    {
        public int IdCliente { get; set; }
        public DateTime FechaAlta { get; set; }
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

        // ── Comportamiento ────────────────────────────────────────────────────
        // Indica si el cliente tiene un plan de suscripción asignado.
        public bool TienePlan() => IdPlan.HasValue;

        // Indica si el cliente puede solicitar la cantidad de nuevas prendas indicada sin superar el límite de su plan.
        public bool PuedeSolicitarPrendas(int cantidadNuevas)
            => TienePlan() && (StockUtilizado + cantidadNuevas) <= LimitePrendas;

        // Cantidad de prendas adicionales que el plan aún permite.
        public int PrendasDisponiblesEnPlan()
            => TienePlan() ? Math.Max(0, LimitePrendas - StockUtilizado) : 0;

        // Fecha de nacimiento del cliente (para validar mayoría de edad).
        public DateTime? FechaNacimiento { get; set; }

        // Fecha opcional de vencimiento de la suscripción (null = sin límite).
        public DateTime? FechaVencimiento { get; set; }

        // True si FechaVencimiento está establecida y ya pasó (independientemente del plan).
        public bool VencimientoExpirado =>
            FechaVencimiento.HasValue && FechaVencimiento.Value.Date < DateTime.Today;

        // True si el cliente tiene plan Y (sin fecha de vencimiento O la fecha no pasó).
        public bool SuscripcionVigente()
        {
            if (!TienePlan()) return false;
            if (!FechaVencimiento.HasValue) return true;
            return FechaVencimiento.Value.Date >= DateTime.Today;
        }

        // True si la suscripción vence dentro de los próximos diasAlerta días (inclusive hoy).
        public bool SuscripcionProximaAVencer(int diasAlerta = 7) =>
            TienePlan()
            && FechaVencimiento.HasValue
            && FechaVencimiento.Value.Date >= DateTime.Today
            && (FechaVencimiento.Value.Date - DateTime.Today).TotalDays <= diasAlerta;

        // Días que faltan para que venza la suscripción; int.MaxValue si no hay fecha.
        public int DiasHastaVencimiento() =>
            FechaVencimiento.HasValue
                ? Math.Max(0, (FechaVencimiento.Value.Date - DateTime.Today).Days)
                : int.MaxValue;
    }
}
