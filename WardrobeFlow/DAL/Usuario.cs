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
    ///   IdUsuario         int PK     → alias "Id" en consultas
    ///   Username          varchar    → nombre de acceso único
    ///   Clave             varchar    → hash PBKDF2-SHA256 (alias "Contraseña")
    ///   Rol               varchar    → rol técnico
    ///   Perfil            varchar    → nombre visible
    ///   Estado            bit        → 1=activo, 0=bloqueado (T02)
    ///   IntentosFallidos  int        → contador persistente de intentos fallidos
    /// </summary>
    /// <summary>
    /// Hereda de <see cref="BaseDAL{T}"/>:
    ///   - acceso  → Singleton de BD (heredado, no se redeclara)
    ///   - ObtenerTodos() y ObtenerPorId() → implementados con SQL de Usuario
    /// </summary>
    public class Usuario : BaseDAL<BE.Usuario>
    {
        /// <summary>
        /// Inserta un nuevo usuario con contraseña hasheada y rol asignado.
        /// Estado=1 (activo) e IntentosFallidos=0 por defecto al crear.
        /// </summary>
        public void Alta(string username, string clave, string perfil)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@username", username),
                new SqlParameter("@clave",    clave),
                new SqlParameter("@perfil",   perfil),
                new SqlParameter("@rol",      perfil)
            };
            acceso.Escribir(
                "INSERT INTO Usuario (Username, Clave, Rol, Estado, Perfil, IntentosFallidos) " +
                "VALUES (@username, @clave, @rol, 1, @perfil, 0)",
                parametros);
        }

        /// <summary>No-op — conservado por compatibilidad. La conexión ahora es por operación.</summary>
        public void Logout() { /* no-op */ }

        /// <summary>
        /// Busca un usuario por Username para el proceso de Login.
        /// Incluye Estado e IntentosFallidos para el control de bloqueo (T02).
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
                    "SELECT IdUsuario AS Id, Username, Clave AS Contraseña, Rol, Perfil, " +
                    "       Estado, IntentosFallidos " +
                    "FROM Usuario WHERE Username = @Username",
                    parametros);

                if (tabla == null || tabla.Rows.Count == 0) return null;

                DataRow row = tabla.Rows[0];
                return new BE.Usuario
                {
                    Id               = Convert.ToInt32(row["Id"]),
                    Username         = row["Username"].ToString(),
                    Contraseña       = row["Contraseña"].ToString(),
                    Rol              = row["Rol"]    != DBNull.Value ? row["Rol"].ToString()    : null,
                    Perfil           = row["Perfil"] != DBNull.Value ? row["Perfil"].ToString() : null,
                    Bloqueado        = row["Estado"] != DBNull.Value && Convert.ToInt32(row["Estado"]) == 0,
                    IntentosFallidos = row["IntentosFallidos"] != DBNull.Value
                                          ? Convert.ToInt32(row["IntentosFallidos"]) : 0
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
        /// Desbloquea la cuenta de un usuario (Estado=1) y resetea el contador de intentos.
        /// Solo puede ejecutarlo un Administrador desde la GUI de Usuarios.
        /// </summary>
        public void Desbloquear(int idUsuario)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@idUsuario", idUsuario)
            };
            acceso.Escribir(
                "UPDATE Usuario SET Estado = 1, IntentosFallidos = 0 WHERE IdUsuario = @idUsuario",
                parametros);
        }

        /// <summary>
        /// Incrementa en 1 el contador de intentos fallidos para el username dado.
        /// El contador persiste en BD: sobrevive reinicios de la aplicación.
        /// </summary>
        public void IncrementarIntentosFallidos(string username)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@username", username)
            };
            try
            {
                acceso.Escribir(
                    "UPDATE Usuario SET IntentosFallidos = ISNULL(IntentosFallidos, 0) + 1 " +
                    "WHERE Username = @username",
                    parametros);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DAL.Usuario.IncrementarIntentosFallidos] {ex.Message}");
            }
        }

        /// <summary>
        /// Resetea a 0 el contador de intentos fallidos para el username dado.
        /// Se llama tras un login exitoso.
        /// </summary>
        public void ResetearIntentosFallidos(string username)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@username", username)
            };
            try
            {
                acceso.Escribir(
                    "UPDATE Usuario SET IntentosFallidos = 0 WHERE Username = @username",
                    parametros);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DAL.Usuario.ResetearIntentosFallidos] {ex.Message}");
            }
        }

        /// <summary>
        /// Actualiza la contraseña de un usuario existente (ya hasheada por la BLL).
        /// </summary>
        public void ResetearClave(int idUsuario, string claveHasheada)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@clave",     claveHasheada),
                new SqlParameter("@idUsuario", idUsuario)
            };
            acceso.Escribir(
                "UPDATE Usuario SET Clave = @clave WHERE IdUsuario = @idUsuario",
                parametros);
        }

        /// <summary>
        /// Obtiene un usuario por su clave primaria (IdUsuario).
        /// Incluye Estado e IntentosFallidos para el control de bloqueo.
        /// </summary>
        public override BE.Usuario ObtenerPorId(int idUsuario)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@IdUsuario", idUsuario)
            };
            try
            {
                DataTable tabla = acceso.Leer(
                    "SELECT IdUsuario AS Id, Username, Clave AS Contraseña, Rol, Perfil, " +
                    "       Estado, IntentosFallidos " +
                    "FROM Usuario WHERE IdUsuario = @IdUsuario",
                    parametros);

                if (tabla == null || tabla.Rows.Count == 0) return null;

                DataRow row = tabla.Rows[0];
                return new BE.Usuario
                {
                    Id               = Convert.ToInt32(row["Id"]),
                    Username         = row["Username"].ToString(),
                    Contraseña       = row["Contraseña"].ToString(),
                    Rol              = row["Rol"]    != DBNull.Value ? row["Rol"].ToString()    : null,
                    Perfil           = row["Perfil"] != DBNull.Value ? row["Perfil"].ToString() : null,
                    Bloqueado        = row["Estado"] != DBNull.Value && Convert.ToInt32(row["Estado"]) == 0,
                    IntentosFallidos = row["IntentosFallidos"] != DBNull.Value
                                          ? Convert.ToInt32(row["IntentosFallidos"]) : 0
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener el usuario por ID.", ex);
            }
        }

        /// <summary>
        /// Lista todos los usuarios del sistema (sin contraseña).
        /// Incluye Estado e IntentosFallidos para la vista de administración.
        /// </summary>
        public override List<BE.Usuario> ObtenerTodos()
        {
            var lista = new List<BE.Usuario>();
            try
            {
                DataTable tabla = acceso.Leer(
                    "SELECT IdUsuario AS Id, Username, Perfil, Estado, IntentosFallidos " +
                    "FROM Usuario ORDER BY Username",
                    null);

                foreach (DataRow row in tabla.Rows)
                {
                    lista.Add(new BE.Usuario
                    {
                        Id               = Convert.ToInt32(row["Id"]),
                        Username         = row["Username"].ToString(),
                        Perfil           = row["Perfil"] != DBNull.Value ? row["Perfil"].ToString() : null,
                        Bloqueado        = row["Estado"] != DBNull.Value && Convert.ToInt32(row["Estado"]) == 0,
                        IntentosFallidos = row["IntentosFallidos"] != DBNull.Value
                                              ? Convert.ToInt32(row["IntentosFallidos"]) : 0
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
