using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    /// <summary>
    /// Capa de Acceso a Datos — Usuario.
    /// Opera sobre la tabla [Usuario] de WardrobeFlowDB.
    /// Nota: la PK en la BD es IdUsuario — se selecciona con alias "Id" para
    /// coincidir con la propiedad BE.Usuario.Id sin renombrar la columna en la BD.
    /// </summary>
    public class Usuario
    {
        private readonly Acceso acceso = Acceso.GetInstance();

        /// <summary>
        /// Inserta un nuevo usuario con contraseña ya hasheada (PBKDF2).
        /// </summary>
        public void Alta(string username, string clave)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@username", username),
                new SqlParameter("@clave",    clave)
            };
            acceso.Escribir(
                "INSERT INTO Usuario (Username, Clave, Estado) VALUES (@username, @clave, 1)",
                parametros);
        }

        /// <summary>Cierra la conexión a la BD (se llama al hacer Logout).</summary>
        public void Logout()
        {
            acceso.CerrarConexion();
        }

        /// <summary>
        /// Busca un usuario por Username para el proceso de Login.
        /// Usa alias "Id" sobre IdUsuario para mapear a BE.Usuario.Id.
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
                    "SELECT IdUsuario AS Id, Username, Clave AS Contraseña, Perfil " +
                    "FROM Usuario WHERE Username = @Username",
                    parametros);

                if (tabla == null || tabla.Rows.Count == 0) return null;

                DataRow row = tabla.Rows[0];
                return new BE.Usuario
                {
                    Id         = Convert.ToInt32(row["Id"]),
                    Username   = row["Username"].ToString(),
                    Contraseña = row["Contraseña"].ToString(),   // alias Clave → Contraseña
                    Perfil     = row["Perfil"] != DBNull.Value ? row["Perfil"].ToString() : null
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener el usuario desde la base de datos.", ex);
            }
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
        /// </summary>
        public List<BE.Usuario> ObtenerTodos()
        {
            var lista = new List<BE.Usuario>();
            try
            {
                DataTable tabla = acceso.Leer(
                    "SELECT IdUsuario AS Id, Username, Perfil FROM Usuario ORDER BY Username",
                    null);

                foreach (DataRow row in tabla.Rows)
                {
                    lista.Add(new BE.Usuario
                    {
                        Id       = Convert.ToInt32(row["Id"]),
                        Username = row["Username"].ToString(),
                        Perfil   = row["Perfil"] != DBNull.Value ? row["Perfil"].ToString() : null
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
