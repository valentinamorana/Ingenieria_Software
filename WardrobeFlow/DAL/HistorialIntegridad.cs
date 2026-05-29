using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class HistorialIntegridad
    {
        private readonly Acceso _acceso = Acceso.GetInstance();

        public void Insertar(BE.HistorialIntegridad entrada)
        {
            try
            {
                _acceso.Escribir(
                    "INSERT INTO HistorialIntegridad " +
                    "(NombreTabla, DVVAlmacenado, DVVCalculado, Resultado, FilasCorruptas, FechaVerificacion, DisparadoPor) " +
                    "VALUES (@tabla, @dvvAlm, @dvvCal, @resultado, @filas, GETDATE(), @disparador)",
                    new SqlParameter[]
                    {
                        new SqlParameter("@tabla",      entrada.NombreTabla),
                        new SqlParameter("@dvvAlm",     entrada.DVVAlmacenado.HasValue ? (object)entrada.DVVAlmacenado.Value : DBNull.Value),
                        new SqlParameter("@dvvCal",     entrada.DVVCalculado),
                        new SqlParameter("@resultado",  entrada.Resultado),
                        new SqlParameter("@filas",      entrada.FilasCorruptas),
                        new SqlParameter("@disparador", entrada.DisparadoPor)
                    });
            }
            catch (Exception ex)
            {
                throw new Exception("Error al insertar en HistorialIntegridad.", ex);
            }
        }

        public List<BE.HistorialIntegridad> ObtenerUltimos(int cantidad = 100)
        {
            var lista = new List<BE.HistorialIntegridad>();
            try
            {
                DataTable tabla = _acceso.Leer(
                    "SELECT TOP (@n) Id, NombreTabla, DVVAlmacenado, DVVCalculado, " +
                    "Resultado, FilasCorruptas, FechaVerificacion, DisparadoPor " +
                    "FROM HistorialIntegridad ORDER BY FechaVerificacion DESC",
                    new SqlParameter[] { new SqlParameter("@n", cantidad) });

                if (tabla == null) return lista;

                foreach (DataRow row in tabla.Rows)
                {
                    lista.Add(new BE.HistorialIntegridad
                    {
                        Id                = Convert.ToInt32(row["Id"]),
                        NombreTabla       = row["NombreTabla"].ToString(),
                        DVVAlmacenado     = row["DVVAlmacenado"] != DBNull.Value ? (int?)Convert.ToInt32(row["DVVAlmacenado"]) : null,
                        DVVCalculado      = Convert.ToInt32(row["DVVCalculado"]),
                        Resultado         = Convert.ToBoolean(row["Resultado"]),
                        FilasCorruptas    = Convert.ToInt32(row["FilasCorruptas"]),
                        FechaVerificacion = Convert.ToDateTime(row["FechaVerificacion"]),
                        DisparadoPor      = row["DisparadoPor"].ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener historial de integridad.", ex);
            }
            return lista;
        }
    }
}
