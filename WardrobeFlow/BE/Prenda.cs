using System;

namespace BE
{
    /// <summary>Entidad Prenda. Mapea la tabla [Prenda].</summary>
    public class Prenda
    {
        public int          IdPrenda        { get; set; }
        public DateTime     FechaAlta       { get; set; }
        public string       Nombre          { get; set; }
        public string       Descripcion     { get; set; }
        public string       Talle           { get; set; }
        public string       Color           { get; set; }
        public string       Categoria       { get; set; }
        public EstadoPrenda Estado          { get; set; } = EstadoPrenda.Disponible;

        /// <summary>FK → Cliente. Solo aplica cuando Estado = EnUso.</summary>
        public int?         IdClienteActual { get; set; }

        /// <summary>Nombre del cliente que la tiene (cargado por JOIN, no persiste).</summary>
        public string       NombreCliente   { get; set; }

        /// <summary>Descripción para mostrar en grillas: "Vestido azul – en uso"</summary>
        public string ResumenEstado =>
            Estado == EstadoPrenda.EnUso && !string.IsNullOrEmpty(NombreCliente)
                ? $"{Nombre} — en uso ({NombreCliente})"
                : $"{Nombre} — {Estado}";

        // ── Comportamiento ────────────────────────────────────────────────────

        /// <summary>Indica si la prenda puede ser asignada a un pedido.</summary>
        public bool EstaDisponible() => Estado == EstadoPrenda.Disponible;

        /// <summary>
        /// Indica si el cambio al nuevo estado está permitido.
        /// Transiciones válidas (solo desde el módulo de Prendas):
        ///   Disponible  → EnLimpieza | Baja
        ///   EnLimpieza  → Disponible | Baja
        ///   EnUso       → (ninguna: solo via pedido)
        ///   Baja        → (ninguna: estado final)
        /// La transición Disponible/EnLimpieza → EnUso solo ocurre
        /// internamente al crear o des-cancelar un pedido.
        /// </summary>
        public bool TransicionPermitida(EstadoPrenda nuevo)
        {
            if (Estado == nuevo) return true;

            switch (Estado)
            {
                case EstadoPrenda.Disponible:
                    return nuevo == EstadoPrenda.EnLimpieza || nuevo == EstadoPrenda.Baja;
                case EstadoPrenda.EnLimpieza:
                    return nuevo == EstadoPrenda.Disponible || nuevo == EstadoPrenda.Baja;
                case EstadoPrenda.EnUso:
                case EstadoPrenda.Baja:
                default:
                    return false;
            }
        }

        /// <summary>
        /// Devuelve el motivo por el que la transición no está permitida,
        /// o null si la transición es válida.
        /// </summary>
        public string MotivoTransicionNoPermitida(EstadoPrenda nuevo)
        {
            if (TransicionPermitida(nuevo)) return null;
            switch (Estado)
            {
                case EstadoPrenda.Baja:
                    return "Una prenda dada de baja no puede cambiar de estado.";
                case EstadoPrenda.EnUso:
                    return "No se puede cambiar manualmente el estado de una prenda en uso.\n" +
                           "El estado se actualiza automáticamente al procesar pedidos.";
                default:
                    return $"La transición de '{Estado}' a '{nuevo}' no está permitida.";
            }
        }
    }
}
