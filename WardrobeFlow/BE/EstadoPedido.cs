namespace BE
{
    /// <summary>
    /// Estado de un pedido generado por el Vendedor.
    /// </summary>
    public enum EstadoPedido
    {
        /// <summary>Pedido generado, esperando despacho.</summary>
        Pendiente   = 0,

        /// <summary>Despachado por el Operador de Inventario.</summary>
        Despachado  = 1,

        /// <summary>Entregado al cliente.</summary>
        Entregado   = 2,

        /// <summary>Cancelado antes del despacho.</summary>
        Cancelado   = 3
    }
}
