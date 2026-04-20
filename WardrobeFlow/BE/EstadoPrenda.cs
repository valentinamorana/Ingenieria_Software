namespace BE
{
    /// <summary>
    /// Estado de una prenda en el catálogo de stock.
    /// </summary>
    public enum EstadoPrenda
    {
        /// <summary>Disponible para asignar a un cliente.</summary>
        Disponible  = 0,

        /// <summary>Asignada a un cliente actualmente.</summary>
        EnUso       = 1,

        /// <summary>En proceso de limpieza tras devolución.</summary>
        EnLimpieza  = 2,

        /// <summary>Dada de baja (daño irreparable o extravío).</summary>
        Baja        = 3
    }
}
