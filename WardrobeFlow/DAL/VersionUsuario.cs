/*
 * SCRIPT SQL — ver BD/01_Crear_BaseDeDatos.sql (tabla HistorialUsuario). Columnas:
 *   IdVersion, IdUsuario, Fecha, Actor, Detalle,
 *   UsernameSnap, NombreSnap, ApellidoSnap, FechaNacSnap, EmailSnap   (datos administrativos)
 *   ClaveSnap, EstadoSnap, IntentosSnap                               (trazabilidad interna)
 */
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class VersionUsuario : BaseDAL<BE.VersionUsuario>
    {
        // Todas las columnas (perfil + seguridad). Tras 02_Actualizar la tabla las tiene.
        private const string Cols =
            "IdVersion, IdUsuario, Fecha, Actor, Detalle, UsernameSnap, " +
            "NombreSnap, ApellidoSnap, FechaNacSnap, EmailSnap, ClaveSnap, EstadoSnap, IntentosSnap";
        // Subconjunto legacy (BD sin migrar a snapshots de perfil).
        private const string ColsLegacy =
            "IdVersion, IdUsuario, Fecha, Actor, Detalle, UsernameSnap, ClaveSnap, EstadoSnap, IntentosSnap";

        public override List<BE.VersionUsuario> ObtenerTodos()
            => LeerLista("ORDER BY Fecha DESC", null);

        public override BE.VersionUsuario ObtenerPorId(int id)
        {
            var lista = LeerLista("WHERE IdVersion = @Id", new[] { new SqlParameter("@Id", id) });
            return lista.Count == 0 ? null : lista[0];
        }

        public List<BE.VersionUsuario> ObtenerPorUsuario(int idUsuario)
            => LeerLista("WHERE IdUsuario = @Id ORDER BY Fecha DESC", new[] { new SqlParameter("@Id", idUsuario) });

        // Lector tolerante: intenta con las columnas de perfil; si la BD no está migrada, cae al legacy.
        private List<BE.VersionUsuario> LeerLista(string filtro, SqlParameter[] parametros)
        {
            var lista = new List<BE.VersionUsuario>();
            DataTable dt;
            try
            {
                dt = acceso.Leer($"SELECT {Cols} FROM HistorialUsuario {filtro}", parametros);
            }
            catch (System.Data.SqlClient.SqlException sqlEx)
                when (sqlEx.Message.Contains("NombreSnap") || sqlEx.Message.Contains("ApellidoSnap")
                      || sqlEx.Message.Contains("FechaNacSnap") || sqlEx.Message.Contains("EmailSnap"))
            {
                dt = acceso.Leer($"SELECT {ColsLegacy} FROM HistorialUsuario {filtro}",
                    parametros == null ? null : new[] { new SqlParameter(parametros[0].ParameterName, parametros[0].Value) });
            }
            foreach (DataRow row in dt.Rows)
                lista.Add(Mapear(row));
            return lista;
        }

        public void Insertar(BE.VersionUsuario v)
        {
            try
            {
                acceso.Escribir(
                    "INSERT INTO HistorialUsuario " +
                    "(IdUsuario, Fecha, Actor, Detalle, UsernameSnap, NombreSnap, ApellidoSnap, " +
                    " FechaNacSnap, EmailSnap, ClaveSnap, EstadoSnap, IntentosSnap) " +
                    "VALUES (@IdUsuario, @Fecha, @Actor, @Detalle, @Username, @Nombre, @Apellido, " +
                    " @FechaNac, @Email, @Clave, @Estado, @Intentos)",
                    Parametros(v, conPerfil: true));
            }
            catch (System.Data.SqlClient.SqlException sqlEx)
                when (sqlEx.Message.Contains("NombreSnap") || sqlEx.Message.Contains("ApellidoSnap")
                      || sqlEx.Message.Contains("FechaNacSnap") || sqlEx.Message.Contains("EmailSnap"))
            {
                // BD sin migrar a snapshots de perfil: insertar solo las columnas legacy.
                acceso.Escribir(
                    "INSERT INTO HistorialUsuario " +
                    "(IdUsuario, Fecha, Actor, Detalle, UsernameSnap, ClaveSnap, EstadoSnap, IntentosSnap) " +
                    "VALUES (@IdUsuario, @Fecha, @Actor, @Detalle, @Username, @Clave, @Estado, @Intentos)",
                    Parametros(v, conPerfil: false));
            }
        }

        private static SqlParameter[] Parametros(BE.VersionUsuario v, bool conPerfil)
        {
            var ps = new List<SqlParameter>
            {
                new SqlParameter("@IdUsuario", v.IdUsuario),
                new SqlParameter("@Fecha",     v.Fecha),
                new SqlParameter("@Actor",     (object)v.Actor   ?? DBNull.Value),
                new SqlParameter("@Detalle",   (object)v.Detalle ?? DBNull.Value),
                new SqlParameter("@Username",  (object)v.UsernameSnapshot ?? DBNull.Value),
                new SqlParameter("@Clave",     (object)v.ClaveSnapshot ?? DBNull.Value),
                new SqlParameter("@Estado",    v.EstadoSnapshot),
                new SqlParameter("@Intentos",  v.IntentosSnapshot)
            };
            if (conPerfil)
            {
                ps.Add(new SqlParameter("@Nombre",   (object)v.NombreSnapshot   ?? DBNull.Value));
                ps.Add(new SqlParameter("@Apellido", (object)v.ApellidoSnapshot ?? DBNull.Value));
                ps.Add(new SqlParameter("@FechaNac", (object)v.FechaNacSnapshot ?? DBNull.Value));
                ps.Add(new SqlParameter("@Email",    (object)v.EmailSnapshot    ?? DBNull.Value));
            }
            return ps.ToArray();
        }

        private static BE.VersionUsuario Mapear(DataRow row)
        {
            bool Has(string c) => row.Table.Columns.Contains(c);
            return new BE.VersionUsuario
            {
                Id               = Convert.ToInt32(row["IdVersion"]),
                IdUsuario        = Convert.ToInt32(row["IdUsuario"]),
                Fecha            = Convert.ToDateTime(row["Fecha"]),
                Actor            = row["Actor"].ToString(),
                Detalle          = row["Detalle"].ToString(),
                UsernameSnapshot = row["UsernameSnap"].ToString(),
                NombreSnapshot   = Has("NombreSnap")   && row["NombreSnap"]   != DBNull.Value ? row["NombreSnap"].ToString()   : null,
                ApellidoSnapshot = Has("ApellidoSnap") && row["ApellidoSnap"] != DBNull.Value ? row["ApellidoSnap"].ToString() : null,
                FechaNacSnapshot = Has("FechaNacSnap") && row["FechaNacSnap"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["FechaNacSnap"]) : null,
                EmailSnapshot    = Has("EmailSnap")    && row["EmailSnap"]    != DBNull.Value ? row["EmailSnap"].ToString()    : null,
                ClaveSnapshot    = row["ClaveSnap"].ToString(),
                EstadoSnapshot   = Convert.ToBoolean(row["EstadoSnap"]),
                IntentosSnapshot = Convert.ToInt32(row["IntentosSnap"])
            };
        }
    }
}
