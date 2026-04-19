namespace BE
{
    /// <summary>
    /// Capa de Entidades (Business Entities) — Entidad Usuario.
    /// Representa a un usuario del sistema con sus datos de identificación,
    /// credenciales de acceso (hasheadas) y el perfil/rol asignado.
    ///
    /// Esta entidad viaja entre todas las capas de la arquitectura:
    /// DAL la construye desde la BD, BLL la valida, Seguridad la almacena en sesión,
    /// y GUI la usa para personalizar la interfaz.
    ///
    /// NOTA: La contraseña NUNCA se almacena en texto plano.
    /// El campo Contraseña contiene el hash PBKDF2 generado por Seguridad.Encriptador.
    /// </summary>
    public class Usuario
    {
        /// <summary>
        /// Clave primaria de la tabla Usuario (columna: IdUsuario).
        /// Se mantiene como "Id" en la entidad para consistencia interna del código.
        /// </summary>
        public int Id { get; set; }

        /// <summary>Nombre de usuario para el inicio de sesión. Único en el sistema.</summary>
        public string Username { get; set; }

        /// <summary>
        /// Hash PBKDF2 de la contraseña en formato Base64 (Salt + Hash).
        /// NUNCA contiene la contraseña en texto plano.
        /// </summary>
        public string Contraseña { get; set; }

        /// <summary>
        /// FK hacia la tabla Persona (IdPersona).
        /// Permite obtener el nombre completo, correo y documento del empleado.
        /// </summary>
        public int? IdPersona { get; set; }

        /// <summary>
        /// Nombre completo del empleado, obtenido haciendo JOIN con la tabla Persona.
        /// No se persiste directamente en Usuario — es solo para mostrar en la UI.
        /// </summary>
        public string NombreCompleto { get; set; }
    }
}
