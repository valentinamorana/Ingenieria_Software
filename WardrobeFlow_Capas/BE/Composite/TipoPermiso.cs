namespace BE.Composite
{
    // Enumeracion que define todos los permisos posibles en WardrobeFlow.
    // Cada valor representa una accion que puede estar habilitada o no para un usuario.
    // Se usa junto al patron Composite (Patente = hoja del arbol de permisos).
    public enum TipoPermiso
    {
        GestorCategorias,   // Puede gestionar categorias de prendas
        GestorPrendas,      // Puede gestionar el inventario de prendas
        GestorOutfits,      // Puede crear y editar outfits
        GestorUsuarios,     // Puede administrar usuarios del sistema
        GestorPermisos,     // Puede asignar permisos a usuarios
    }
}
