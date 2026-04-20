using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    /// <summary>
    /// Capa de Acceso a Datos — Permisos.
    /// Opera sobre las tablas [Permiso] y [RolPermiso] de WardrobeFlowDB.
    ///
    /// Permite cargar los permisos habilitados para un rol específico,
    /// usado por BLL.Usuario.Login() para enriquecer el objeto de sesión.
    /// </summary>
    public class Permiso
    {
        private readonly Acceso acceso = Acceso.GetInstance();

        /// <summary>
        /// Obtiene la lista de permisos activos asignados a un rol.
        /// Hace JOIN entre RolPermiso y Permiso para retornar solo los habilitados.
        /// </summary>
        /// <param name="rol">Nombre del rol (ej: "Administrador", "Supervisor").</param>
        /// <returns>Lista de BE.Permiso con los permisos del rol.</returns>
        public List<BE.Permiso> ObtenerPorRol(string rol)
        {
            var lista = new List<BE.Permiso>();

            if (string.IsNullOrWhiteSpace(rol)) return lista;

            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@rol", rol)
            };

            try
            {
                DataTable tabla = acceso.Leer(
                    "SELECT p.IdPermiso, p.Nombre, p.NombreMenu, p.TipoComponente, p.Estado " +
                    "FROM Permiso p " +
                    "INNER JOIN RolPermiso rp ON p.IdPermiso = rp.IdPermiso " +
                    "WHERE rp.Rol = @rol AND p.Estado = 1 " +
                    "ORDER BY p.TipoComponente, p.Nombre",
                    parametros);

                if (tabla == null) return lista;

                foreach (DataRow row in tabla.Rows)
                {
                    lista.Add(new BE.Permiso
                    {
                        Id             = Convert.ToInt32(row["IdPermiso"]),
                        Nombre         = row["Nombre"].ToString(),
                        NombreMenu     = row["NombreMenu"].ToString(),
                        TipoComponente = row["TipoComponente"].ToString(),
                        Estado         = Convert.ToBoolean(row["Estado"])
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener permisos para el rol '{rol}'.", ex);
            }

            return lista;
        }
    }
}
