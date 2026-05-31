/*
 * SCRIPT SQL — ejecutar una vez en WardrobeFlowDB antes de usar este módulo:
 *
 *   CREATE TABLE HistorialUsuario (
 *       IdVersion       INT IDENTITY(1,1) PRIMARY KEY,
 *       IdUsuario       INT          NOT NULL REFERENCES Usuario(IdUsuario),
 *       Fecha           DATETIME     NOT NULL DEFAULT GETDATE(),
 *       Actor           NVARCHAR(100) NOT NULL,
 *       Detalle         NVARCHAR(500) NOT NULL,
 *       UsernameSnap    NVARCHAR(100) NOT NULL,
 *       ClaveSnap       NVARCHAR(500) NOT NULL,
 *       EstadoSnap      BIT          NOT NULL,
 *       IntentosSnap    INT          NOT NULL
 *   );
 */
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class VersionUsuario : BaseDAL<BE.VersionUsuario>
    {
        public override List<BE.VersionUsuario> ObtenerTodos()
        {
            var lista = new List<BE.VersionUsuario>();
            var dt = acceso.Leer(
                "SELECT IdVersion, IdUsuario, Fecha, Actor, Detalle, UsernameSnap, ClaveSnap, EstadoSnap, IntentosSnap FROM HistorialUsuario ORDER BY Fecha DESC", null);
            foreach (DataRow row in dt.Rows)
                lista.Add(Mapear(row));
            return lista;
        }

        public override BE.VersionUsuario ObtenerPorId(int id)
        {
            var dt = acceso.Leer(
                "SELECT IdVersion, IdUsuario, Fecha, Actor, Detalle, UsernameSnap, ClaveSnap, EstadoSnap, IntentosSnap FROM HistorialUsuario WHERE IdVersion = @Id",
                new[] { new SqlParameter("@Id", id) });
            return dt.Rows.Count == 0 ? null : Mapear(dt.Rows[0]);
        }

        public List<BE.VersionUsuario> ObtenerPorUsuario(int idUsuario)
        {
            var lista = new List<BE.VersionUsuario>();
            var dt = acceso.Leer(
                "SELECT IdVersion, IdUsuario, Fecha, Actor, Detalle, UsernameSnap, ClaveSnap, EstadoSnap, IntentosSnap FROM HistorialUsuario WHERE IdUsuario = @Id ORDER BY Fecha DESC",
                new[] { new SqlParameter("@Id", idUsuario) });
            foreach (DataRow row in dt.Rows)
                lista.Add(Mapear(row));
            return lista;
        }

        public void Insertar(BE.VersionUsuario v)
        {
            acceso.Escribir(
                "INSERT INTO HistorialUsuario " +
                "(IdUsuario, Fecha, Actor, Detalle, UsernameSnap, ClaveSnap, EstadoSnap, IntentosSnap) " +
                "VALUES (@IdUsuario, @Fecha, @Actor, @Detalle, @Username, @Clave, @Estado, @Intentos)",
                new SqlParameter[]
                {
                    new SqlParameter("@IdUsuario", v.IdUsuario),
                    new SqlParameter("@Fecha",     v.Fecha),
                    new SqlParameter("@Actor",     v.Actor),
                    new SqlParameter("@Detalle",   v.Detalle),
                    new SqlParameter("@Username",  v.UsernameSnapshot),
                    new SqlParameter("@Clave",     v.ClaveSnapshot),
                    new SqlParameter("@Estado",    v.EstadoSnapshot),
                    new SqlParameter("@Intentos",  v.IntentosSnapshot)
                });
        }

        private static BE.VersionUsuario Mapear(DataRow row)
        {
            return new BE.VersionUsuario
            {
                Id               = Convert.ToInt32(row["IdVersion"]),
                IdUsuario        = Convert.ToInt32(row["IdUsuario"]),
                Fecha            = Convert.ToDateTime(row["Fecha"]),
                Actor            = row["Actor"].ToString(),
                Detalle          = row["Detalle"].ToString(),
                UsernameSnapshot = row["UsernameSnap"].ToString(),
                ClaveSnapshot    = row["ClaveSnap"].ToString(),
                EstadoSnapshot   = Convert.ToBoolean(row["EstadoSnap"]),
                IntentosSnapshot = Convert.ToInt32(row["IntentosSnap"])
            };
        }
    }
}
