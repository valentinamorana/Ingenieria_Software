namespace BE
{
    // Enumeracion con los tipos de operacion que registra la Bitacora.
    // Basado en el ejemplo de referencia (Nacho/Codigo), adaptado a WardrobeFlow.
    public enum TipoOperacion
    {
        // Autenticacion
        LOGIN,
        LOGOUT,
        LOGIN_FALLIDO,

        // Operaciones sobre prendas
        ALTA_PRENDA,
        BAJA_PRENDA,
        MODIFICACION_PRENDA,

        // Operaciones sobre categorias
        ALTA_CATEGORIA,
        BAJA_CATEGORIA,
        MODIFICACION_CATEGORIA,

        // Operaciones sobre outfits
        ALTA_OUTFIT,
        BAJA_OUTFIT,
        MODIFICACION_OUTFIT,

        // Operaciones sobre usuarios
        ALTA_USUARIO,
        BAJA_USUARIO,
        MODIFICACION_USUARIO,

        // Seguridad
        ACCESO_DENEGADO,
        ASIGNACION_PERMISOS
    }
}
