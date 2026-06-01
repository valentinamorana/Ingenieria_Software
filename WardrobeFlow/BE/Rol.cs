namespace BE
{
    /// <summary>
    /// Patrón Composite — T04 Gestión de Perfiles de Usuario.
    ///
    /// Un Rol es un nodo COMPUESTO (hereda de Familia) que representa un perfil
    /// asignable a un Usuario. Al heredar de Familia puede contener:
    ///   • otras Patentes (permisos simples),
    ///   • Familias (grupos de permisos),
    ///   • y otros Roles (composición de roles — "embeber un rol dentro de otro").
    ///
    /// La validación anti-ciclos se hereda de Familia.AgregarHijo, de modo que
    /// no es posible crear una referencia circular (Rol A → Rol B → Rol A).
    ///
    /// En la BD un Rol es una fila de Permiso con EsFamilia=1 y EsRol=1.
    /// </summary>
    public class Rol : Familia
    {
        /// <summary>
        /// Indica si el rol es fijo según la definición del sistema
        /// (precargado, no eliminable libremente).
        /// </summary>
        public bool EsFijo { get; set; }
    }
}
