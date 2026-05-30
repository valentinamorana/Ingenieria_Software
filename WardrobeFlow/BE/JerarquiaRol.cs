using System.Collections.Generic;

namespace BE
{
    /// <summary>
    /// T04 — Entidad que representa un nodo en la jerarquía de roles del sistema.
    ///
    /// La jerarquía de roles es independiente del árbol de permisos (Composite Familia/Patente).
    /// Mientras el Composite de permisos agrupa Patentes en Familias temáticas (Inventario, Ventas, Sistema),
    /// la jerarquía de roles organiza los roles según la estructura organizacional de la empresa:
    ///
    ///   Área Comercial
    ///     └── Gerente Comercial  → hereda visibilidad de todos los módulos comerciales
    ///         └── Vendedor       → acceso solo a operaciones de venta
    ///
    ///   Área Inventario
    ///     └── Gerente de Inventario → gestión completa del inventario
    ///         ├── Encargado de Stock
    ///         └── Operador Logístico
    ///
    /// Se almacena en la tabla [RolJerarquia] (RolHijo, RolPadre, Nivel, Area).
    /// Se reconstruye como árbol en BLL.Familia.ObtenerArbolJerarquiaRoles().
    /// </summary>
    public class JerarquiaRol
    {
        public string       Rol      { get; set; }
        public string       RolPadre { get; set; }
        public int          Nivel    { get; set; }
        public string       Area     { get; set; }
        public List<JerarquiaRol> Hijos { get; set; } = new List<JerarquiaRol>();

        public override string ToString() => Rol;
    }
}
