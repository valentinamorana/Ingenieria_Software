using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    /// <summary>
    /// Capa de Acceso a Datos — Usuario.
    /// Opera sobre la tabla [Usuario] de WardrobeFlowDB.
    ///
    /// Columnas de la tabla:
    ///   IdUsuario  int PK     → alias "Id" en consultas
    ///   Username   varchar    → nombre de acceso único
    ///   Clave      varchar    → hash PBKDF2-SHA256 (alias "Contraseña")
    ///   Rol        varchar    → rol técnico
    ///   Perfil     varchar    → nombre visible
    ///   Estado     bit        → 1=activo, 0=bloqueado (T02: bloqueo tras 3 intentos)
    /// </summary>
    public class Usuario
    {
        private readonly Acceso acceso = Acceso.GetInstance();

        /// <summary>
        /// Inserta un nuevo usuario con contraseña hasheada y rol asignado.
        /// Estado=1 (activo) por defecto al crear.
        /// </summary>
        public void Alta(string username, string clave, string perfil)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@username", username),
                new SqlParameter("@clave",    clave),
                new SqlParameter("@perfil",   perfil),
                new SqlParameter("@rol",      perfil)  // Rol = Perfil para empleados
            };
            acceso.Escribir(
                "INSERT INTO Usuario (Username, Clave, Rol, Estado, Perfil) " +
                "VALUES (@username, @clave, @rol, 1, @perfil)",
                parametros);
        }

        /// <summary>Cierra la conexión a la BD (se llama al hacer Logout).</summary>
        public void Logout()
        {
            acceso.CerrarConexion();
        }

        /// <summary>
        /// Busca un usuario por Username para el proceso de Login.
        /// Incluye Estado para detectar cuentas bloqueadas (T02).
        /// </summary>
        public BE.Usuario ObtenerPorUsername(string username)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@Username", username)
            };

            try
            {
                DataTable tabla = acceso.Leer(
                    "SELECT IdUsuario AS Id, Username, Clave AS Contraseña, Rol, Perfil, Estado " +
                    "FROM Usuario WHERE Username = @Username",
                    parametros);

                if (tabla == null || tabla.Rows.Count == 0) return null;

                DataRow row = tabla.Rows[0];
                return new BE.Usuario
                {
                    Id         = Convert.ToInt32(row["Id"]),
                    Username   = row["Username"].ToString(),
                    Contraseña = row["Contraseña"].ToString(),
                    Rol        = row["Rol"]    != DBNull.Value ? row["Rol"].ToString()    : null,
                    Perfil     = row["Perfil"] != DBNull.Value ? row["Perfil"].ToString() : null,
                    // Estado=0 → bloqueado; Estado=1 → activo
                    Bloqueado  = row["Estado"] != DBNull.Value && Convert.ToInt32(row["Estado"]) == 0
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener el usuario desde la base de datos.", ex);
            }
        }

        /// <summary>
        /// Bloquea la cuenta de un usuario (Estado=0).
        /// Se llama tras superar el máximo de intentos fallidos (T02).
        /// Solo un Administrador puede revertirlo con Desbloquear().
        /// </summary>
        public void Bloquear(int idUsuario)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@idUsuario", idUsuario)
            };
            acceso.Escribir(
                "UPDATE Usuario SET Estado = 0 WHERE IdUsuario = @idUsuario",
                parametros);
        }

        /// <summary>
        /// Desbloquea la cuenta de un usuario (Estado=1).
        /// Solo puede ejecutarlo un Administrador desde la GUI de Usuarios.
        /// </summary>
        public void Desbloquear(int idUsuario)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@idUsuario", idUsuario)
            };
            acceso.Escribir(
                "UPDATE Usuario SET Estado = 1 WHERE IdUsuario = @idUsuario",
                parametros);
        }

        /// <summary>
        /// Actualiza la contraseña de un usuario existente (ya hasheada por la BLL).
        /// Llamado únicamente desde BLL.Usuario.ResetearClave().
        /// </summary>
        public void ResetearClave(int idUsuario, string claveHasheada)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@clave",      claveHasheada),
                new SqlParameter("@idUsuario",  idUsuario)
            };
            acceso.Escribir(
                "UPDATE Usuario SET Clave = @clave WHERE IdUsuario = @idUsuario",
                parametros);
        }

        /// <summary>
        /// Lista todos los usuarios del sistema (sin contraseña por seguridad).
        /// Incluye Estado para mostrar cuentas bloqueadas en la GUI.
        /// </summary>
        public List<BE.Usuario> ObtenerTodos()
        {
            var lista = new List<BE.Usuario>();
            try
            {
                DataTable tabla = acceso.Leer(
                    "SELECT IdUsuario AS Id, Username, Perfil, Estado " +
                    "FROM Usuario ORDER BY Username",
                    null);

                foreach (DataRow row in tabla.Rows)
                {
                    lista.Add(new BE.Usuario
                    {
                        Id        = Convert.ToInt32(row["Id"]),
                        Username  = row["Username"].ToString(),
                        Perfil    = row["Perfil"] != DBNull.Value ? row["Perfil"].ToString() : null,
                        Bloqueado = row["Estado"] != DBNull.Value && Convert.ToInt32(row["Estado"]) == 0
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener la lista de usuarios.", ex);
            }
            return lista;
        }
    }
}
