using System.Collections.Generic;

namespace BE
{
    /// <summary>
    /// Capa de Entidades — Usuario del sistema (empleado).
    /// Mapea la tabla [Usuario] de WardrobeFlowDB.
    ///
    /// Columnas:
    ///   Id         int          PK (columna IdUsuario en BD, alias "Id")
    ///   Username   varchar      nombre de acceso único
    ///   Contraseña varchar      hash PBKDF2-SHA256 — NUNCA texto plano
    ///   Perfil     varchar      rol del empleado (ej: "Administrador")
    ///
    /// La lista Permisos se carga en memoria tras el login (no persiste en esta tabla).
    /// </summary>
    public class Usuario
    {
        /// <summary>Identificador único del usuario (mapeado desde IdUsuario en BD).</summary>
        public int Id { get; set; }

        /// <summary>Nombre de usuario para el login. Debe ser único en el sistema.</summary>
        public string Username { get; set; }

        /// <summary>Hash PBKDF2-SHA256 de la contraseña. Nunca se almacena en texto plano.</summary>
        public string Contraseña { get; set; }

        /// <summary>
        /// Perfil/rol del empleado. Determina qué permisos tiene en el sistema.
        /// Valores válidos: "Administrador", "OperadorLogistico", "Supervisor".
        /// </summary>
        public string Perfil { get; set; }

        /// <summary>
        /// Lista de permisos habilitados para este usuario según su rol.
        /// Se carga desde la tabla RolPermiso al momento del login.
        /// Usada por el menú MDI para construir la navegación dinámicamente.
        /// </summary>
        public List<Permiso> Permisos { get; set; } = new List<Permiso>();
    }
}
