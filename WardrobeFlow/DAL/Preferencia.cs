using System;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    /// <summary>
    /// Capa de Acceso a Datos — Preferencias de UI por usuario (tabla Preferencia).
    /// Si no hay fila (o la tabla aún no está migrada), devuelve los valores por defecto.
    /// </summary>
    public class Preferencia : Interfaces.IPreferenciaDAL
    {
        private readonly Acceso acceso = Acceso.GetInstance();

        public BE.Preferencia Obtener(int idUsuario)
        {
            var pref = new BE.Preferencia { IdUsuario = idUsuario };
            try
            {
                DataTable t = acceso.Leer(
                    "SELECT FuenteFamilia, FuenteTamano, Tema, FormatoFecha, Notificaciones " +
                    "FROM Preferencia WHERE IdUsuario = @id",
                    new SqlParameter[] { new SqlParameter("@id", idUsuario) });

                if (t != null && t.Rows.Count > 0)
                {
                    DataRow r = t.Rows[0];
                    if (r["FuenteFamilia"]  != DBNull.Value) pref.FuenteFamilia  = r["FuenteFamilia"].ToString();
                    if (r["FuenteTamano"]   != DBNull.Value) pref.FuenteTamano   = r["FuenteTamano"].ToString();
                    if (r["Tema"]           != DBNull.Value) pref.Tema           = r["Tema"].ToString();
                    if (r["FormatoFecha"]   != DBNull.Value) pref.FormatoFecha   = r["FormatoFecha"].ToString();
                    if (r["Notificaciones"] != DBNull.Value) pref.Notificaciones = Convert.ToBoolean(r["Notificaciones"]);
                }
            }
            catch (Exception ex)
            {
                // Tabla sin migrar o error: se devuelven los defaults (no es crítico).
                System.Diagnostics.Trace.TraceWarning("[DAL.Preferencia.Obtener] " + ex.Message);
            }
            return pref;
        }

        public void Guardar(BE.Preferencia p)
        {
            acceso.Escribir(
                "IF EXISTS (SELECT 1 FROM Preferencia WHERE IdUsuario = @id) " +
                "    UPDATE Preferencia SET FuenteFamilia=@ff, FuenteTamano=@ft, Tema=@tm, " +
                "           FormatoFecha=@fd, Notificaciones=@nt WHERE IdUsuario=@id " +
                "ELSE " +
                "    INSERT INTO Preferencia (IdUsuario, FuenteFamilia, FuenteTamano, Tema, FormatoFecha, Notificaciones) " +
                "    VALUES (@id, @ff, @ft, @tm, @fd, @nt)",
                new SqlParameter[]
                {
                    new SqlParameter("@id", p.IdUsuario),
                    new SqlParameter("@ff", (object)p.FuenteFamilia ?? DBNull.Value),
                    new SqlParameter("@ft", (object)p.FuenteTamano  ?? DBNull.Value),
                    new SqlParameter("@tm", (object)p.Tema          ?? DBNull.Value),
                    new SqlParameter("@fd", (object)p.FormatoFecha  ?? DBNull.Value),
                    new SqlParameter("@nt", p.Notificaciones)
                });
        }
    }
}
