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
    ///   Estado            bit        → 1=activo, 0=bloqueado
    ///   IntentosFallidos  int        → contador persistente de intentos fallidos
    /// </summary>
    /// <summary>
    /// Hereda de BaseDAL<BE.Usuario>
    ///   - acceso  → Singleton de BD (heredado, no se redeclara)
    ///   - ObtenerTodos() y ObtenerPorId() → implementados con SQL de Usuario
    /// </summary>
    public class Usuario : BaseDAL<BE.Usuario>
    {
        // Inserta un nuevo usuario con contraseña hasheada y rol asignado.
        // Estado=1 (activo) e IntentosFallidos=0 por defecto al crear.
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

        public void Logout() { /* no-op */ }

        // Busca un usuario por Username para el proceso de Login.
        // Incluye Estado e IntentosFallidos para el control de bloqueo.
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

        // Bloquea la cuenta de un usuario (Estado=0).
        // Se llama tras superar el máximo de intentos fallidos 
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

        // Desbloquea la cuenta de un usuario (Estado=1) y resetea el contador de intentos.
        // Solo puede ejecutarlo un Administrador desde la GUI de Usuarios.
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

        // Incrementa en 1 el contador de intentos fallidos para el username dado.
        // El contador persiste en BD: sobrevive reinicios de la aplicación.
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

        // Resetea a 0 el contador de intentos fallidos para el username dado.
        // Se llama tras un login exitoso.
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

        // Actualiza la contraseña de un usuario existente (ya hasheada por la BLL).
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

        // Obtiene un usuario por su clave primaria (IdUsuario).
        // Incluye Estado e IntentosFallidos para el control de bloqueo.
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

        // Lista todos los usuarios del sistema (sin contraseña).
        // Incluye Estado e IntentosFallidos para la vista de administración.
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
