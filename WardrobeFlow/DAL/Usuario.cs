using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    /// <summary>
    /// Capa de Acceso a Datos — Operaciones de Usuario.
    /// Contiene todas las consultas SQL relacionadas con la tabla "Usuario".
    /// Esta capa NO contiene lógica de negocio; sólo accede y mapea datos de la BD.
    /// </summary>
    public class Usuario
    {
        // Acceso a la BD a través del Singleton compartido
        private readonly Acceso acceso = Acceso.GetInstance();

        /// <summary>
        /// Inserta un nuevo usuario en la base de datos con su contraseña ya hasheada.
        /// La contraseña que llega aquí ya fue procesada por Seguridad.Encriptador.Hash()
        /// en la capa BLL, por lo que NUNCA se recibe texto plano aquí.
        /// </summary>
        /// <param name="username">Nombre de usuario único para el nuevo registro.</param>
        /// <param name="contraseña">Hash PBKDF2 de la contraseña (Base64).</param>
        public void Alta(string username, string contraseña)
        {
            // BUG FIX: parámetros con prefijo "@" para coincidir correctamente con la consulta SQL
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@username",  username),
                new SqlParameter("@contraseña", contraseña)
            };

            acceso.Escribir(
                "INSERT INTO Usuario (Username, Contraseña) VALUES (@username, @contraseña)",
                parametros);
        }

        /// <summary>
        /// Cierra la conexión a la base de datos. Se invoca al hacer Logout del sistema.
        /// </summary>
        public void Logout()
        {
            acceso.CerrarConexion();
        }

        /// <summary>
        /// Busca y retorna un usuario por su nombre de usuario.
        /// Usado durante el proceso de Login para verificar credenciales.
        /// Retorna null si el usuario no existe (nunca lanza excepción de "no encontrado").
        /// </summary>
        /// <param name="username">Nombre de usuario a buscar en la BD.</param>
        /// <returns>Entidad BE.Usuario si existe; null si no se encontró.</returns>
        /// <exception cref="Exception">Si ocurre un error de base de datos inesperado.</exception>
        public BE.Usuario ObtenerPorUsername(string username)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@Username", username)
            };

            try
            {
                // Consulta con parámetro para prevenir SQL Injection
                DataTable tabla = acceso.Leer(
                    "SELECT Id, Username, Contraseña, Perfil FROM Usuario WHERE Username = @Username",
                    parametros);

                // Si no se encontró el usuario, retornar null (sin lanzar excepción)
                if (tabla == null || tabla.Rows.Count == 0)
                    return null;

                // Mapear la primera fila al objeto de entidad BE.Usuario
                DataRow row = tabla.Rows[0];
                return new BE.Usuario
                {
                    Id         = Convert.ToInt32(row["Id"]),
                    Username   = row["Username"].ToString(),
                    Contraseña = row["Contraseña"].ToString(),
                    // Perfil puede ser null si la columna no existe o está vacía
                    Perfil     = row.Table.Columns.Contains("Perfil") ? row["Perfil"]?.ToString() : null
                };
            }
            catch (Exception ex)
            {
                // Relanzar como excepción más descriptiva para que BLL la maneje
                throw new Exception("Error al obtener el usuario desde la base de datos.", ex);
            }
        }

        /// <summary>
        /// Obtiene la lista completa de usuarios del sistema.
        /// Usado por el módulo de Gestión de Usuarios (GUI/Usuarios.cs).
        /// Las contraseñas NO se retornan por seguridad.
        /// </summary>
        /// <returns>Lista de entidades BE.Usuario sin datos de contraseña.</returns>
        public List<BE.Usuario> ObtenerTodos()
        {
            var lista = new List<BE.Usuario>();

            try
            {
                // No se selecciona la contraseña por seguridad (no es necesaria para listado)
                DataTable tabla = acceso.Leer(
                    "SELECT Id, Username, Perfil FROM Usuario ORDER BY Username",
                    null);

                // Mapear cada fila de la tabla a una entidad BE.Usuario
                foreach (DataRow row in tabla.Rows)
                {
                    lista.Add(new BE.Usuario
                    {
                        Id       = Convert.ToInt32(row["Id"]),
                        Username = row["Username"].ToString(),
                        Perfil   = row.Table.Columns.Contains("Perfil") ? row["Perfil"]?.ToString() : null
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
