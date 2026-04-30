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
        public int Id { get; set; }

        public string Nombre { get; set; }

        // Nombre del elemento de menú asociado en la GUI.
        public string NombreMenu { get; set; }

        public string TipoComponente { get; set; }

        public bool Estado { get; set; }
    }
}
