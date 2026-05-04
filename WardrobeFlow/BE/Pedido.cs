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
        public int IdPedido { get; set; }

        public int IdCliente { get; set; }

        public int IdEmpleado { get; set; }

        public EstadoPedido Estado { get; set; } = EstadoPedido.Pendiente;

        public DateTime FechaPedido { get; set; }
        public DateTime? FechaDespacho { get; set; }

        public DateTime? FechaEntrega { get; set; }

        public string MotivoCancelacion { get; set; }

        public string NombreCliente { get; set; }

        public string NombreEmpleado { get; set; }

        // Prendas asociadas al pedido (cargadas desde PedidoPrenda).
        public List<Prenda> Prendas { get; set; } = new List<Prenda>();

        public int CantidadPrendas => Prendas?.Count ?? 0;

        // Resumen para mostrar en grillas.
        public string Resumen =>
            $"Pedido #{IdPedido} — {NombreCliente ?? $"Cliente {IdCliente}"} — {Estado}";

        // Comportamiento
        // El pedido puede cancelarse solo si está Pendiente.
        public bool PuedeCancelarse() => Estado == EstadoPedido.Pendiente;

        // El pedido puede despacharse solo si está Pendiente.
        public bool PuedeDespachar() => Estado == EstadoPedido.Pendiente;

        // El pedido puede marcarse como entregado solo si está Despachado.
        public bool PuedeEntregarse() => Estado == EstadoPedido.Despachado;

        // El pedido puede des-cancelarse solo si está Cancelado.
        public bool PuedeDesCancelarse() => Estado == EstadoPedido.Cancelado;

        /// <summary>
        /// Valida que la transición de estado sea permitida según el flujo definido:
        ///   Pendiente → Despachado → Entregado
        ///   Pendiente → Cancelado  → Pendiente (des-cancelar)
        /// Impide cualquier retroceso o salto de estado no contemplado.
        /// </summary>
        public bool TransicionValida(EstadoPedido destino)
        {
            switch (Estado)
            {
                case EstadoPedido.Pendiente:
                    return destino == EstadoPedido.Despachado
                        || destino == EstadoPedido.Cancelado;

                case EstadoPedido.Despachado:
                    return destino == EstadoPedido.Entregado;

                case EstadoPedido.Entregado:
                    return false; // Estado final — no admite más transiciones

                case EstadoPedido.Cancelado:
                    return destino == EstadoPedido.Pendiente; // Des-cancelar

                default:
                    return false;
            }
        }
    }
}
