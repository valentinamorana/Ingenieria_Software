namespace BE
{
    /// <summary>
    /// Capa de Entidades — Permiso del sistema.
    /// Mapea la tabla [Permiso] de WardrobeFlowDB.
    ///
    /// Cada permiso representa una funcionalidad habilitada para un rol.
    /// El campo NombreMenu identifica el ToolStripMenuItem en la GUI
    /// para construir el menú dinámico al iniciar sesión.
    ///
    /// Columnas:
    ///   IdPermiso       int          PK
    ///   Nombre          varchar      descripción legible (ej: "Ver Usuarios")
    ///   NombreMenu      varchar      nombre del control en GUI (ej: "mnuUsuarios")
    ///   TipoComponente  varchar      categoría del módulo (ej: "Sistema", "Inventario")
    ///   Estado          bit          1 = activo, 0 = deshabilitado
    /// </summary>
    public class Permiso
    {
        /// <summary>Identificador único del permiso.</summary>
        public int Id { get; set; }

        /// <summary>Nombre descriptivo del permiso (ej: "Ver Usuarios").</summary>
        public string Nombre { get; set; }

        /// <summary>
        /// Nombre del elemento de menú asociado en la GUI.
        /// Se usa para mostrar/ocultar ToolStripMenuItems dinámicamente.
        /// Valores posibles: mnuPrendas, mnuOutfits, mnuCategorias,
        ///                   mnuUsuarios, mnuPermisos, mnuAuditoria.
        /// </summary>
        public string NombreMenu { get; set; }

        /// <summary>Categoría del módulo: "Inventario" o "Sistema".</summary>
        public string TipoComponente { get; set; }

        /// <summary>true si el permiso está activo y puede asignarse.</summary>
        public bool Estado { get; set; }
    }
}
