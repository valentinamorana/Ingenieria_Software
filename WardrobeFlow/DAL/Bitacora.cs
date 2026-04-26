using System;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    /// <summary>
    /// Capa de Acceso a Datos — Bitácora.
    /// Opera sobre la tabla [Bitacora] de WardrobeFlowDB.
    /// </summary>
    public class Bitacora
    {
        private readonly Acceso acceso = Acceso.GetInstance();

        /// <summary>
        /// Inserta un registro de auditoría. Id es identity — lo genera la BD.
        /// </summary>
        public void Registrar(BE.Bitacora registro)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@fecha",      registro.Fecha),
                new SqlParameter("@usuario",    (object)registro.IdUsuario ?? DBNull.Value),
                new SqlParameter("@modulo",     registro.Modulo),
                new SqlParameter("@actividad",  registro.Actividad),
                new SqlParameter("@detalle",    registro.Detalle),
                new SqlParameter("@criticidad", (int)registro.Criticidad)
            };

            try
            {
                acceso.Escribir(
                    "INSERT INTO Bitacora (fecha, usuario, modulo, actividad, detalle, criticidad) " +
                    "VALUES (@fecha, @usuario, @modulo, @actividad, @detalle, @criticidad)",
                    parametros);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[DAL.Bitacora] Error al registrar: {ex.Message}");
            }
        }

        /// <summary>Obtiene todos los registros ordenados por fecha descendente.</summary>
        public DataTable ObtenerTodos()
        {
            try
            {
                return acceso.Leer(
                    "SELECT Id, fecha, usuario, modulo, actividad, detalle, criticidad " +
                    "FROM Bitacora ORDER BY fecha DESC",
                    null);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[DAL.Bitacora] Error al obtener: {ex.Message}");
                return new DataTable();
            }
        }

        /// <summary>
        /// Devuelve los registros de los últimos <paramref name="dias"/> días,
        /// calculando la fecha de corte como DateTime.Now menos los días indicados.
        /// Cumple el requisito de "traer los últimos N días definibles".
        /// </summary>
        /// <param name="dias">Cantidad de días hacia atrás desde hoy.</param>
        public DataTable ObtenerUltimosNDias(int dias)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@desde", DateTime.Now.AddDays(-dias))
            };

            try
            {
                return acceso.Leer(
                    "SELECT Id, fecha, usuario, modulo, actividad, detalle, criticidad " +
                    "FROM Bitacora WHERE fecha >= @desde ORDER BY fecha DESC",
                    parametros);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[DAL.Bitacora] Error al obtener últimos {dias} días: {ex.Message}");
                return new DataTable();
            }
        }

        /// <summary>
        /// Búsqueda combinada por rango de fechas, usuario, actividad y criticidad.
        /// Cumple T06a: búsquedas por datos almacenados de manera combinada.
        /// </summary>
        public DataTable BuscarPorFiltros(DateTime? desde, DateTime? hasta, int idUsuario, string actividad, int criticidad)
        {
            string consulta = "SELECT Id, fecha, usuario, modulo, actividad, detalle, criticidad " +
                              "FROM Bitacora WHERE 1=1";
            var parametros = new System.Collections.Generic.List<SqlParameter>();

            if (desde.HasValue)
            {
                consulta += " AND fecha >= @desde";
                parametros.Add(new SqlParameter("@desde", desde.Value.Date));
            }
            if (hasta.HasValue)
            {
                consulta += " AND fecha <= @hasta";
                parametros.Add(new SqlParameter("@hasta", hasta.Value.Date.AddDays(1).AddSeconds(-1)));
            }
            if (idUsuario > 0)
            {
                consulta += " AND usuario = @usuario";
                parametros.Add(new SqlParameter("@usuario", idUsuario));
            }
            if (!string.IsNullOrWhiteSpace(actividad))
            {
                consulta += " AND actividad LIKE @actividad";
                parametros.Add(new SqlParameter("@actividad", $"%{actividad}%"));
            }
            // criticidad == -1 significa "Todas" (sin filtro de criticidad)
            // criticidad >= 0 filtra por valor exacto del enum (incluye None=0)
            if (criticidad >= 0)
            {
                consulta += " AND criticidad = @criticidad";
                parametros.Add(new SqlParameter("@criticidad", criticidad));
            }

            consulta += " ORDER BY fecha DESC";

            try
            {
                return acceso.Leer(consulta, parametros.ToArray());
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[DAL.Bitacora] Error en búsqueda: {ex.Message}");
                return new DataTable();
            }
        }
    }
}
