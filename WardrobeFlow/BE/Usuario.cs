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
        public int Id { get; set; }

        public string Username { get; set; }

        public string Contraseña { get; set; }

        public string Perfil { get; set; }

        public string Rol { get; set; }

        public List<Permiso> Permisos { get; set; } = new List<Permiso>();

        public bool Bloqueado { get; set; }

        public int IntentosFallidos { get; set; }
    }
}
