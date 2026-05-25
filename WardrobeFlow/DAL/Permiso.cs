/*
 * SCRIPT SQL — ejecutar en WardrobeFlowDB para migrar al Composite relacional:
 *
 *   -- 1. Agregar discriminador EsFamilia
 *   ALTER TABLE Permiso ADD EsFamilia BIT NOT NULL DEFAULT 0;
 *
 *   -- 2. Crear tabla de relaciones padre-hijo
 *   CREATE TABLE PermisoRelacion (
 *       IdPadre INT NOT NULL REFERENCES Permiso(IdPermiso),
 *       IdHijo  INT NOT NULL REFERENCES Permiso(IdPermiso),
 *       PRIMARY KEY (IdPadre, IdHijo)
 *   );
 *
 *   -- 3. Crear nodos Familia por cada grupo TipoComponente existente
 *   DECLARE @mapa TABLE (Grupo NVARCHAR(100), IdFamilia INT);
 *
 *   INSERT INTO Permiso (Nombre, NombreMenu, TipoComponente, Estado, EsFamilia)
 *   OUTPUT INSERTED.Nombre, INSERTED.IdPermiso INTO @mapa (Grupo, IdFamilia)
 *   SELECT DISTINCT TipoComponente, TipoComponente, TipoComponente, 1, 1
 *   FROM   Permiso
 *   WHERE  TipoComponente IS NOT NULL AND LTRIM(RTRIM(TipoComponente)) <> ''
 *   AND    EsFamilia = 0;
 *
 *   -- 4. Vincular cada Patente con su Familia
 *   INSERT INTO PermisoRelacion (IdPadre, IdHijo)
 *   SELECT m.IdFamilia, p.IdPermiso
 *   FROM   Permiso p
 *   INNER JOIN @mapa m ON p.TipoComponente = m.Grupo
 *   WHERE  p.EsFamilia = 0;
 */
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class Permiso
    {
        private readonly Acceso acceso = Acceso.GetInstance();

        // Construye el árbol Composite completo desde BD.
        // Lee Permiso (EsFamilia discrimina tipo) y PermisoRelacion (padre→hijo).
        // Retorna los nodos raíz (Familias sin padre) listas para que BLL las envuelva.
        public List<BE.Componente> ObtenerArbol()
        {
            var dt = acceso.Leer(
                "SELECT IdPermiso, Nombre, NombreMenu, EsFamilia " +
                "FROM Permiso WHERE Estado = 1 ORDER BY EsFamilia DESC, Nombre",
                null);

            var nodos = new Dictionary<int, BE.Componente>();

            foreach (DataRow row in dt.Rows)
            {
                int  id        = Convert.ToInt32(row["IdPermiso"]);
                bool esFamilia = row["EsFamilia"] != DBNull.Value && Convert.ToBoolean(row["EsFamilia"]);

                if (esFamilia)
                {
                    nodos[id] = new BE.Familia { Id = id, Nombre = row["Nombre"].ToString() };
                }
                else
                {
                    nodos[id] = new BE.Patente
                    {
                        Id         = id,
                        Nombre     = row["Nombre"].ToString(),
                        NombreMenu = row["NombreMenu"] != DBNull.Value ? row["NombreMenu"].ToString() : string.Empty
                    };
                }
            }

            var rels = acceso.Leer("SELECT IdPadre, IdHijo FROM PermisoRelacion", null);
            var conPadre = new HashSet<int>();

            foreach (DataRow row in rels.Rows)
            {
                int idPadre = Convert.ToInt32(row["IdPadre"]);
                int idHijo  = Convert.ToInt32(row["IdHijo"]);

                if (nodos.TryGetValue(idPadre, out BE.Componente nodoPadre) &&
                    nodos.TryGetValue(idHijo,  out BE.Componente nodoHijo)  &&
                    nodoPadre is BE.Familia familia)
                {
                    familia.AgregarHijo(nodoHijo);
                    conPadre.Add(idHijo);
                }
            }

            var raices = new List<BE.Componente>();
            foreach (var kvp in nodos)
                if (!conPadre.Contains(kvp.Key))
                    raices.Add(kvp.Value);

            return raices;
        }

        // Solo Patentes (EsFamilia=0) — usado por Login para poblar la sesión.
        public List<BE.Permiso> ObtenerTodos()
        {
            var lista = new List<BE.Permiso>();
            try
            {
                DataTable tabla = acceso.Leer(
                    "SELECT IdPermiso, Nombre, NombreMenu, TipoComponente, Estado " +
                    "FROM Permiso WHERE ISNULL(EsFamilia,0) = 0 ORDER BY TipoComponente, Nombre",
                    null);

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
                throw new Exception("Error al obtener permisos.", ex);
            }
            return lista;
        }

        // Roles disponibles en el sistema.
        public List<string> ObtenerRoles()
        {
            var lista = new List<string>();
            try
            {
                DataTable tabla = acceso.Leer(
                    "SELECT DISTINCT Rol FROM RolPermiso ORDER BY Rol", null);
                if (tabla == null) return lista;
                foreach (DataRow row in tabla.Rows)
                    lista.Add(row["Rol"].ToString());
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener roles.", ex);
            }
            return lista;
        }

        // Patentes activas asignadas a un rol — para sesión y para marcar Asignado en el árbol.
        public List<BE.Permiso> ObtenerPorRol(string rol)
        {
            var lista = new List<BE.Permiso>();
            if (string.IsNullOrWhiteSpace(rol)) return lista;

            try
            {
                DataTable tabla = acceso.Leer(
                    "SELECT p.IdPermiso, p.Nombre, p.NombreMenu, p.TipoComponente, p.Estado " +
                    "FROM Permiso p " +
                    "INNER JOIN RolPermiso rp ON p.IdPermiso = rp.IdPermiso " +
                    "WHERE rp.Rol = @rol AND p.Estado = 1 " +
                    "ORDER BY p.TipoComponente, p.Nombre",
                    new[] { new SqlParameter("@rol", rol) });

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

        public void AsignarPermiso(string rol, int idPermiso)
        {
            try
            {
                acceso.Escribir(
                    "IF NOT EXISTS (SELECT 1 FROM RolPermiso WHERE Rol = @rol AND IdPermiso = @id) " +
                    "INSERT INTO RolPermiso (Rol, IdPermiso) VALUES (@rol, @id)",
                    new SqlParameter[] { new SqlParameter("@rol", rol), new SqlParameter("@id", idPermiso) });
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al asignar permiso {idPermiso} al rol '{rol}'.", ex);
            }
        }

        public void QuitarPermiso(string rol, int idPermiso)
        {
            try
            {
                acceso.Escribir(
                    "DELETE FROM RolPermiso WHERE Rol = @rol AND IdPermiso = @id",
                    new SqlParameter[] { new SqlParameter("@rol", rol), new SqlParameter("@id", idPermiso) });
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al quitar permiso {idPermiso} del rol '{rol}'.", ex);
            }
        }
    }
}
