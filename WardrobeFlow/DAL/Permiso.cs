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
    /// <summary>
    /// DAL del Composite de permisos (T04).
    ///
    /// FUENTE DE VERDAD EN RUNTIME: la tabla [PermisoRelacion] (aristas padre→hijo). Todo el CRUD
    /// de autorización lee/escribe ahí.
    ///
    /// [RolPermiso] es LEGACY y de SEED/BOOTSTRAP únicamente: los scripts SQL la usan para sembrar
    /// las asignaciones planas rol→patente y, desde ellas, generar los nodos-rol y las aristas de
    /// [PermisoRelacion]. En runtime NO se escribe nunca y solo se LEE en ramas de fallback, para
    /// bases todavía sin migrar al Composite (sin columna EsRol / sin PermisoRelacion poblada).
    /// En una base migrada esas ramas no se ejecutan.
    /// </summary>
    public class Permiso : Interfaces.IPermisoDAL
    {
        private readonly Acceso acceso = Acceso.GetInstance();

        // Construye el árbol Composite completo desde BD.
        // Lee Permiso (EsFamilia discrimina tipo) y PermisoRelacion (padre→hijo).
        // Retorna los nodos raíz (Familias sin padre) listas para que BLL las envuelva.
        public List<BE.Componente> ObtenerArbol()
        {
            // EsRol se lee de forma resiliente: si la columna aún no existe (BD sin migrar),
            // se trata todo EsFamilia como Familia común.
            DataTable dt;
            try
            {
                dt = acceso.Leer(
                    "SELECT IdPermiso, Nombre, NombreMenu, EsFamilia, " +
                    "ISNULL(EsRol,0) AS EsRol " +
                    "FROM Permiso WHERE Estado = 1 ORDER BY EsFamilia DESC, Nombre",
                    null);
            }
            catch (SqlException)
            {
                dt = acceso.Leer(
                    "SELECT IdPermiso, Nombre, NombreMenu, EsFamilia, " +
                    "CAST(0 AS BIT) AS EsRol " +
                    "FROM Permiso WHERE Estado = 1 ORDER BY EsFamilia DESC, Nombre",
                    null);
            }

            var nodos = new Dictionary<int, BE.Componente>();

            foreach (DataRow row in dt.Rows)
            {
                int  id        = Convert.ToInt32(row["IdPermiso"]);
                bool esFamilia = row["EsFamilia"] != DBNull.Value && Convert.ToBoolean(row["EsFamilia"]);
                bool esRol     = row["EsRol"] != DBNull.Value && Convert.ToBoolean(row["EsRol"]);

                if (esRol)
                {
                    nodos[id] = new BE.Rol { Id = id, Nombre = row["Nombre"].ToString(), EsFijo = true };
                }
                else if (esFamilia)
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

            DataTable rels;
            try { rels = acceso.Leer("SELECT IdPadre, IdHijo FROM PermisoRelacion", null); }
            catch { rels = new System.Data.DataTable(); }
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

        // Roles disponibles en el sistema.
        // T04: los roles son nodos del Composite (EsRol=1), de modo que un rol recién
        // creado aparece aunque todavía no tenga permisos asignados.
        // FALLBACK LEGACY: solo si la columna EsRol no existe (BD sin migrar) se cae a [RolPermiso].
        // En una BD migrada esta rama no se ejecuta (ver nota de la clase sobre RolPermiso).
        public List<string> ObtenerRoles()
        {
            var lista = new List<string>();
            try
            {
                DataTable tabla = acceso.Leer(
                    "SELECT Nombre FROM Permiso WHERE ISNULL(EsRol,0) = 1 AND Estado = 1 " +
                    "ORDER BY Nombre", null);
                if (tabla == null) return lista;
                foreach (DataRow row in tabla.Rows)
                    lista.Add(row["Nombre"].ToString());
            }
            catch (SqlException)
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

        // Patentes activas asignadas a un rol leídas desde [RolPermiso] (asignación PLANA, legacy).
        // FALLBACK LEGACY: BLL.Familia solo llama acá cuando el rol NO existe como nodo del Composite
        // (BD sin migrar). En una BD migrada la resolución es 100% por PermisoRelacion y este método
        // no se invoca. No deprecar la lectura sin migrar antes las bases viejas.
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

        // ── T04 — CRUD del Composite (Patentes / Familias / Roles) ──────────────

        // Devuelve el Id del nodo-rol cuyo Nombre coincide, o 0 si no existe.
        public int ObtenerIdRol(string rolNombre)
        {
            if (string.IsNullOrWhiteSpace(rolNombre)) return 0;
            DataTable t = acceso.Leer(
                "SELECT TOP 1 IdPermiso FROM Permiso WHERE Nombre = @n AND ISNULL(EsRol,0) = 1",
                new[] { new SqlParameter("@n", rolNombre) });
            if (t == null || t.Rows.Count == 0) return 0;
            return Convert.ToInt32(t.Rows[0]["IdPermiso"]);
        }

        // Alta de un componente (Patente, Familia o Rol). Devuelve el IdPermiso generado.
        public int AltaComponente(string nombre, string nombreMenu, bool esFamilia, bool esRol, string tipoComponente)
        {
            try
            {
                DataTable t = acceso.Leer(
                    "INSERT INTO Permiso (Nombre, NombreMenu, TipoComponente, Estado, EsFamilia, EsRol) " +
                    "VALUES (@nom, @menu, @tipo, 1, @fam, @rol); " +
                    "SELECT CAST(SCOPE_IDENTITY() AS INT) AS Id",
                    new[]
                    {
                        new SqlParameter("@nom",  nombre),
                        // NombreMenu puede ser NOT NULL en esquemas legacy: para familias/roles
                        // (sin menú propio) se usa el nombre como valor por defecto.
                        new SqlParameter("@menu", (object)(string.IsNullOrEmpty(nombreMenu) ? nombre : nombreMenu)),
                        // TipoComponente puede ser NOT NULL en esquemas legacy → valor por defecto.
                        new SqlParameter("@tipo", (object)(string.IsNullOrEmpty(tipoComponente) ? "General" : tipoComponente)),
                        new SqlParameter("@fam",  esFamilia || esRol),
                        new SqlParameter("@rol",  esRol)
                    });
                return (t != null && t.Rows.Count > 0) ? Convert.ToInt32(t.Rows[0]["Id"]) : 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al crear el componente '{nombre}'.", ex);
            }
        }

        // Modifica nombre y nombre de menú de un componente.
        public void ModificarComponente(int idPermiso, string nombre, string nombreMenu)
        {
            try
            {
                acceso.Escribir(
                    "UPDATE Permiso SET Nombre = @nom, NombreMenu = @menu WHERE IdPermiso = @id",
                    new[]
                    {
                        new SqlParameter("@nom",  nombre),
                        new SqlParameter("@menu", (object)nombreMenu ?? DBNull.Value),
                        new SqlParameter("@id",   idPermiso)
                    });
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al modificar el componente {idPermiso}.", ex);
            }
        }

        // Baja lógica de un componente (Estado=0) y limpieza de sus relaciones,
        // tanto como padre (sus hijos) como hijo (en otros nodos).
        public void BajaComponente(int idPermiso)
        {
            try
            {
                acceso.Escribir(
                    "DELETE FROM PermisoRelacion WHERE IdPadre = @id OR IdHijo = @id; " +
                    "UPDATE Permiso SET Estado = 0 WHERE IdPermiso = @id",
                    new[] { new SqlParameter("@id", idPermiso) });
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al dar de baja el componente {idPermiso}.", ex);
            }
        }

        // NombreMenu de todas las PATENTES (permisos simples) activas. Sirve para saber qué
        // patentes de acción granular ("…Editar") están DEFINIDAS en el catálogo, y así
        // BLL.PermisosAccion decide si exige el permiso de edición o cae al de ver (legacy).
        public List<string> ObtenerNombresMenuPatentes()
        {
            var lista = new List<string>();
            DataTable t;
            try
            {
                t = acceso.Leer(
                    "SELECT NombreMenu FROM Permiso " +
                    "WHERE ISNULL(EsFamilia,0) = 0 AND Estado = 1 AND NombreMenu IS NOT NULL", null);
            }
            catch { return lista; }   // BD sin columnas esperadas → vacío → fallback legacy
            if (t == null) return lista;
            foreach (DataRow row in t.Rows)
            {
                string nm = row["NombreMenu"]?.ToString();
                if (!string.IsNullOrEmpty(nm)) lista.Add(nm);
            }
            return lista;
        }

        // Ids de los hijos DIRECTOS de un nodo (un solo nivel).
        public List<int> ObtenerIdsHijos(int idPadre)
        {
            var lista = new List<int>();
            DataTable t = acceso.Leer(
                "SELECT IdHijo FROM PermisoRelacion WHERE IdPadre = @p",
                new[] { new SqlParameter("@p", idPadre) });
            if (t == null) return lista;
            foreach (DataRow row in t.Rows)
                lista.Add(Convert.ToInt32(row["IdHijo"]));
            return lista;
        }

        // Crea una arista padre→hijo en el árbol Composite (idempotente).
        public void AgregarRelacion(int idPadre, int idHijo)
        {
            try
            {
                acceso.Escribir(
                    "IF NOT EXISTS (SELECT 1 FROM PermisoRelacion WHERE IdPadre = @p AND IdHijo = @h) " +
                    "INSERT INTO PermisoRelacion (IdPadre, IdHijo) VALUES (@p, @h)",
                    new[] { new SqlParameter("@p", idPadre), new SqlParameter("@h", idHijo) });
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al relacionar componentes {idPadre}→{idHijo}.", ex);
            }
        }

        // Elimina una arista padre→hijo del árbol Composite.
        public void QuitarRelacion(int idPadre, int idHijo)
        {
            try
            {
                acceso.Escribir(
                    "DELETE FROM PermisoRelacion WHERE IdPadre = @p AND IdHijo = @h",
                    new[] { new SqlParameter("@p", idPadre), new SqlParameter("@h", idHijo) });
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al quitar relación {idPadre}→{idHijo}.", ex);
            }
        }

        // Cantidad de usuarios que tienen asignado el rol.
        // Cubre el mismo criterio que el login (Rol ?? Perfil).
        private const string FiltroUsuarioRol =
            "(Rol = @rol OR (Rol IS NULL AND Perfil = @rol))";

        public int ContarUsuariosPorRol(string rol)
        {
            DataTable t = acceso.Leer(
                "SELECT COUNT(*) AS Total FROM Usuario WHERE " + FiltroUsuarioRol,
                new[] { new SqlParameter("@rol", rol) });
            if (t == null || t.Rows.Count == 0) return 0;
            return Convert.ToInt32(t.Rows[0]["Total"]);
        }

        // Nombres de usuario que tienen asignado el rol (para advertir antes de eliminar).
        public List<string> ObtenerUsuariosPorRol(string rol)
        {
            var lista = new List<string>();
            DataTable t = acceso.Leer(
                "SELECT Username FROM Usuario WHERE " + FiltroUsuarioRol + " ORDER BY Username",
                new[] { new SqlParameter("@rol", rol) });
            if (t == null) return lista;
            foreach (DataRow row in t.Rows)
                lista.Add(row["Username"].ToString());
            return lista;
        }

    }
}
