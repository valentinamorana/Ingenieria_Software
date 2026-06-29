using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    /// <summary>
    /// Capa de Acceso a Datos — T07 Tabla ESPEJO de integridad (Usuario_Seguridad).
    ///
    /// Mantiene una copia sombra de los campos que entran al DVH de cada usuario, más su DVH.
    /// La app la sincroniza con cada escritura LEGÍTIMA (junto al recálculo del DVH), de modo que
    /// el espejo siempre refleja el "último estado válido conocido". Eso habilita dos cosas que el
    /// DVH solo no permite:
    ///   1. DIAGNÓSTICO a nivel de campo: comparar la fila actual contra el espejo para saber QUÉ
    ///      campo fue alterado (no solo "esta fila no coincide").
    ///   2. REPARACIÓN a valores legítimos: restaurar el dato bueno desde el espejo, sin necesitar
    ///      un backup completo de la base.
    ///
    /// Inspirado en la tabla 'Usuario_Seguridad' del proyecto de referencia (Agus).
    ///
    /// TOLERANCIA: si la tabla aún no fue creada (BD sin migrar — falta 02_Actualizar), todos los
    /// métodos fallan en silencio (no-op / lista vacía), igual que el resto de la capa DAL.
    /// </summary>
    public class EspejoUsuario
    {
        private readonly Acceso acceso = Acceso.GetInstance();

        // Indica si la tabla espejo existe (BD migrada). Se usa para no intentar operar sin migración.
        public bool Existe()
        {
            try
            {
                DataTable dt = acceso.Leer(
                    "SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Usuario_Seguridad'", null);
                return dt != null && dt.Rows.Count > 0;
            }
            catch { return false; }
        }

        // Lee todas las filas del espejo, ordenadas por IdUsuario. Reusa BE.FilaUsuarioDV para que
        // el DVH se calcule con CamposParaDVH() exactamente igual que sobre la tabla real.
        public List<BE.FilaUsuarioDV> ObtenerFilas()
        {
            var lista = new List<BE.FilaUsuarioDV>();
            try
            {
                DataTable dt = acceso.Leer(
                    "SELECT IdUsuario, Username, Clave, Rol, Perfil, Estado, IntentosFallidos, DVH " +
                    "FROM Usuario_Seguridad ORDER BY IdUsuario", null);
                if (dt == null) return lista;

                foreach (DataRow r in dt.Rows)
                {
                    lista.Add(new BE.FilaUsuarioDV
                    {
                        Id               = Convert.ToInt32(r["IdUsuario"]),
                        Username         = r["Username"].ToString(),
                        Clave            = r["Clave"].ToString(),
                        Rol              = r["Rol"]    != DBNull.Value ? r["Rol"].ToString()    : "",
                        Perfil           = r["Perfil"] != DBNull.Value ? r["Perfil"].ToString() : "",
                        Estado           = r["Estado"] != DBNull.Value ? Convert.ToInt32(r["Estado"]).ToString() : "0",
                        IntentosFallidos = r["IntentosFallidos"] != DBNull.Value ? Convert.ToInt32(r["IntentosFallidos"]).ToString() : "0",
                        DVHAlmacenado    = r["DVH"] != DBNull.Value ? (int?)Convert.ToInt32(r["DVH"]) : null
                    });
                }
            }
            catch { /* tabla sin migrar → espejo vacío */ }
            return lista;
        }

        // Inserta o actualiza (upsert) la fila del espejo para un usuario. La llama la app tras cada
        // escritura legítima sobre el usuario (cuando recalcula su DVH).
        public void Upsert(BE.FilaUsuarioDV fila)
        {
            if (fila == null) return;
            try
            {
                acceso.Escribir(
                    "IF EXISTS (SELECT 1 FROM Usuario_Seguridad WHERE IdUsuario = @id) " +
                    "    UPDATE Usuario_Seguridad SET Username=@u, Clave=@c, Rol=@r, Perfil=@p, " +
                    "        Estado=@e, IntentosFallidos=@i, DVH=@dvh, FechaActualizacion=GETDATE() " +
                    "    WHERE IdUsuario=@id " +
                    "ELSE " +
                    "    INSERT INTO Usuario_Seguridad (IdUsuario, Username, Clave, Rol, Perfil, Estado, " +
                    "        IntentosFallidos, DVH, FechaActualizacion) " +
                    "    VALUES (@id, @u, @c, @r, @p, @e, @i, @dvh, GETDATE())",
                    ParametrosFila(fila));
            }
            catch (SqlException ex) when (ex.Message.Contains("Usuario_Seguridad"))
            {
                System.Diagnostics.Trace.TraceWarning(
                    "[DAL.EspejoUsuario.Upsert] Tabla espejo ausente; ejecutá 02_Actualizar.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"[DAL.EspejoUsuario.Upsert] {ex.Message}");
            }
        }

        // Elimina una fila del espejo (cuando se da de baja FÍSICA un usuario legítimamente).
        public void Eliminar(int idUsuario)
        {
            try
            {
                acceso.Escribir("DELETE FROM Usuario_Seguridad WHERE IdUsuario = @id",
                    new SqlParameter[] { new SqlParameter("@id", idUsuario) });
            }
            catch { /* tabla sin migrar */ }
        }

        // Reconstruye TODO el espejo desde una lista de filas legítimas (wipe + reinsert), en una
        // transacción atómica. Se usa tras operaciones masivas y tras "Recalcular Todo"/"Asumir
        // pérdida", donde el espejo debe quedar idéntico al estado actual aceptado como válido.
        public void Reconstruir(List<BE.FilaUsuarioDV> filas)
        {
            if (filas == null) return;
            if (!Existe()) return;
            try
            {
                acceso.EjecutarTransaccion((conn, tx) =>
                {
                    using (var del = new SqlCommand("DELETE FROM Usuario_Seguridad", conn, tx))
                        del.ExecuteNonQuery();

                    foreach (var f in filas)
                    {
                        using (var cmd = new SqlCommand(
                            "INSERT INTO Usuario_Seguridad (IdUsuario, Username, Clave, Rol, Perfil, " +
                            "Estado, IntentosFallidos, DVH, FechaActualizacion) " +
                            "VALUES (@id, @u, @c, @r, @p, @e, @i, @dvh, GETDATE())", conn, tx))
                        {
                            cmd.Parameters.AddRange(ParametrosFila(f));
                            cmd.ExecuteNonQuery();
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"[DAL.EspejoUsuario.Reconstruir] {ex.Message}");
            }
        }

        // Arma los SqlParameter de una fila del espejo a partir del DTO de DVH.
        private static SqlParameter[] ParametrosFila(BE.FilaUsuarioDV f)
        {
            int estado = ParseEntero(f.Estado, 1);
            int intentos = ParseEntero(f.IntentosFallidos, 0);
            return new SqlParameter[]
            {
                new SqlParameter("@id",  f.Id),
                new SqlParameter("@u",   (object)f.Username ?? string.Empty),
                new SqlParameter("@c",   (object)f.Clave    ?? string.Empty),
                new SqlParameter("@r",   (object)f.Rol      ?? DBNull.Value),
                new SqlParameter("@p",   (object)f.Perfil   ?? DBNull.Value),
                new SqlParameter("@e",   estado),
                new SqlParameter("@i",   intentos),
                new SqlParameter("@dvh", (object)(f.DVHAlmacenado ?? 0))
            };
        }

        private static int ParseEntero(string valor, int porDefecto)
        {
            return int.TryParse(valor, out int v) ? v : porDefecto;
        }
    }
}
