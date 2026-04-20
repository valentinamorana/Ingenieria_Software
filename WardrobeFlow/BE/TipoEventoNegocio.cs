namespace BE
{
    /// <summary>
    /// Tipos de evento registrables en la BitacoraNegocio.
    /// Separa los eventos de negocio de los eventos de seguridad (Bitacora del sistema).
    /// </summary>
    public enum TipoEventoNegocio
    {
        /// <summary>Nuevo pedido de venta creado por el Vendedor.</summary>
        Venta              = 0,

        /// <summary>Pedido cancelado antes del despacho.</summary>
        Cancelacion        = 1,

        /// <summary>Pedido despachado por el OperadorDeInventario.</summary>
        Despacho           = 2,

        /// <summary>Pedido marcado como entregado al cliente.</summary>
        Entrega            = 3,

        /// <summary>Nueva prenda dada de alta en el catálogo.</summary>
        AltaPrenda         = 4,

        /// <summary>Datos descriptivos de una prenda modificados.</summary>
        ModificacionPrenda = 5,

        /// <summary>Estado de una prenda cambiado (Disponible/EnLimpieza/Baja).</summary>
        CambioEstadoPrenda = 6,

        /// <summary>Nuevo cliente registrado en el sistema.</summary>
        AltaCliente        = 7,

        /// <summary>Datos de un cliente modificados.</summary>
        ModificacionCliente = 8,

        /// <summary>Cliente dado de baja del sistema.</summary>
        BajaCliente        = 9
    }
}
