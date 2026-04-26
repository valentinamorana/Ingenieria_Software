using System;
using System.Collections.Generic;

namespace BE
{
    /// <summary>
    /// Entidad — Pedido de prendas generado por el Vendedor para un Cliente.
    /// Mapea la tabla [Pedido]. Las prendas del pedido se cargan por separado
    /// desde la tabla intermedia [PedidoPrenda].
    /// </summary>
    public class Pedido
    {
        public int          IdPedido      { get; set; }

        /// <summary>FK → Cliente.</summary>
        public int          IdCliente     { get; set; }

        /// <summary>FK → Empleado (Vendedor que generó el pedido).</summary>
        public int          IdEmpleado    { get; set; }

        public EstadoPedido Estado        { get; set; } = EstadoPedido.Pendiente;

        public DateTime     FechaPedido   { get; set; }

        /// <summary>Fecha en que el OperadorDeInventario despachó el pedido. Null si aún no.</summary>
        public DateTime?    FechaDespacho { get; set; }

        /// <summary>Fecha de entrega al cliente. Null si aún no entregado.</summary>
        public DateTime?    FechaEntrega  { get; set; }

        /// <summary>Motivo de cancelación. Null si no fue cancelado.</summary>
        public string       MotivoCancelacion { get; set; }

        // ── Campos cargados por JOIN (no persisten) ─────────────────────────

        /// <summary>Nombre completo del cliente (JOIN con Cliente).</summary>
        public string       NombreCliente  { get; set; }

        /// <summary>Nombre completo del Vendedor (JOIN con Empleado).</summary>
        public string       NombreEmpleado { get; set; }

        /// <summary>Prendas asociadas al pedido (cargadas desde PedidoPrenda).</summary>
        public List<Prenda> Prendas        { get; set; } = new List<Prenda>();

        /// <summary>Cantidad de prendas del pedido.</summary>
        public int CantidadPrendas => Prendas?.Count ?? 0;

        /// <summary>Resumen para mostrar en grillas.</summary>
        public string Resumen =>
            $"Pedido #{IdPedido} — {NombreCliente ?? $"Cliente {IdCliente}"} — {Estado}";
    }
}
