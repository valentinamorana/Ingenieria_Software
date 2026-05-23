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

        // Obtiene todos los permisos existentes (activos e inactivos) para construir el árbol completo.
        public List<BE.Permiso> ObtenerTodos()
        {
            var lista = new List<BE.Permiso>();
            try
            {
                DataTable tabla = acceso.Leer(
                    "SELECT IdPermiso, Nombre, NombreMenu, TipoComponente, Estado " +
                    "FROM Permiso ORDER BY TipoComponente, Nombre",
                    null);

                if (tabla == null) return lista;

                foreach (DataRow row in tabla.Rows)
                {
                    lista.Add(new BE.Permiso
                    {
                        Id = Convert.ToInt32(row["IdPermiso"]),
                        Nombre = row["Nombre"].ToString(),
                        NombreMenu = row["NombreMenu"].ToString(),
                        TipoComponente = row["TipoComponente"].ToString(),
                        Estado = Convert.ToBoolean(row["Estado"])
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener todos los permisos.", ex);
            }
            return lista;
        }

        // Obtiene la lista de roles distintos registrados en RolPermiso.
        public List<string> ObtenerRoles()
        {
            var lista = new List<string>();
            try
            {
                DataTable tabla = acceso.Leer(
                    "SELECT DISTINCT Rol FROM RolPermiso ORDER BY Rol",
                    null);

                if (tabla == null) return lista;
                foreach (DataRow row in tabla.Rows)
                    lista.Add(row["Rol"].ToString());
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener la lista de roles.", ex);
            }
            return lista;
        }

        // Asigna un permiso a un rol (INSERT en RolPermiso). Ignora si ya existe.
        public void AsignarPermiso(string rol, int idPermiso)
        {
            try
            {
                acceso.Escribir(
                    "IF NOT EXISTS (SELECT 1 FROM RolPermiso WHERE Rol = @rol AND IdPermiso = @id) " +
                    "INSERT INTO RolPermiso (Rol, IdPermiso) VALUES (@rol, @id)",
                    new SqlParameter[]
                    {
                        new SqlParameter("@rol", rol),
                        new SqlParameter("@id",  idPermiso)
                    });
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al asignar permiso {idPermiso} al rol '{rol}'.", ex);
            }
        }

        // Quita un permiso de un rol (DELETE en RolPermiso).
        public void QuitarPermiso(string rol, int idPermiso)
        {
            try
            {
                acceso.Escribir(
                    "DELETE FROM RolPermiso WHERE Rol = @rol AND IdPermiso = @id",
                    new SqlParameter[]
                    {
                        new SqlParameter("@rol", rol),
                        new SqlParameter("@id",  idPermiso)
                    });
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al quitar permiso {idPermiso} del rol '{rol}'.", ex);
            }
        }

        // Obtiene la lista de permisos activos asignados a un rol.
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
                        Id = Convert.ToInt32(row["IdPermiso"]),
                        Nombre = row["Nombre"].ToString(),
                        NombreMenu = row["NombreMenu"].ToString(),
                        TipoComponente = row["TipoComponente"].ToString(),
                        Estado = Convert.ToBoolean(row["Estado"])
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
