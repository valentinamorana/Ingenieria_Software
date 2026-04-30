namespace BE
{
    /// <summary>
    /// Tipos de evento registrables en la BitacoraNegocio.
    /// Separa los eventos de negocio de los eventos de seguridad (Bitacora del sistema).
    /// </summary>
    public enum TipoEventoNegocio
    {
        Venta = 0,

        Cancelacion = 1,

        Despacho = 2,

        Entrega = 3,

        AltaPrenda = 4,

        ModificacionPrenda = 5,

        CambioEstadoPrenda = 6,

        AltaCliente = 7,

        ModificacionCliente = 8,

        BajaCliente = 9,

        Devolucion = 10,

        Reactivacion = 11
    }
}
