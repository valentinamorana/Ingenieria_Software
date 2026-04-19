namespace BE
{
    /// <summary>
    /// Capa de Entidades — Usuario del sistema.
    /// Mapea la tabla [Usuario] de WardrobeFlowDB.
    ///
    /// Columnas:
    ///   Id         int          PK (columna IdUsuario en BD, mapeada como Id)
    ///   Username   varchar      nombre de acceso, único
    ///   Contraseña varchar      hash PBKDF2 — NUNCA texto plano
    ///   Perfil     varchar      rol del empleado (ej: "Administrador", "Vendedor")
    /// </summary>
    public class Usuario
    {
        /// <summary>Identificador único del usuario (mapeado desde IdUsuario en la BD).</summary>
        public int Id { get; set; }

        /// <summary>Nombre de usuario para el login. Debe ser único en el sistema.</summary>
        public string Username { get; set; }

        /// <summary>Hash PBKDF2 de la contraseña. Nunca se almacena en texto plano.</summary>
        public string Contraseña { get; set; }

        /// <summary>Perfil/rol del empleado. Determina sus permisos en el sistema.</summary>
        public string Perfil { get; set; }
    }
}
