using System;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    /// <summary>
    /// Capa de Acceso a Datos — Bitácora.
    /// Maneja todas las operaciones de persistencia y consulta de registros de auditoría
    /// en la tabla "Bitacora" de la base de datos SQL Server.
    /// </summary>
    public class Bitacora
    {
        // Referencia al Singleton de conexión compartido en toda la capa DAL
        private readonly Acceso acceso = Acceso.GetInstance();

        /// <summary>
        /// Inserta un nuevo registro de actividad en la tabla Bitacora.
        /// Usa parámetros SQL para prevenir inyección SQL.
        /// </summary>
        /// <param name="registro">Entidad BE.Bitacora con todos los datos del evento a registrar.</param>
        public void Registrar(BE.Bitacora registro)
        {
            // BUG FIX: el parámetro "@criticidad" necesita el prefijo "@" para coincidir con la consulta SQL
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@fecha",      registro.Fecha),
                new SqlParameter("@usuario",    registro.IdUsuario),
                new SqlParameter("@modulo",     registro.Modulo),
                new SqlParameter("@actividad",  registro.Actividad),
                new SqlParameter("@detalle",    registro.Detalle),
                new SqlParameter("@criticidad", (int)registro.Criticidad)   // Se convierte a int para guardar el valor numérico
            };

            try
            {
                acceso.Escribir(
                    "INSERT INTO AuditoriaSesion (fecha, usuario, modulo, actividad, detalle, criticidad) " +
                    "VALUES (@fecha, @usuario, @modulo, @actividad, @detalle, @criticidad)",
                    parametros);
            }
            catch (Exception ex)
            {
                // Log de error en consola: no se relanza para no interrumpir el flujo de la aplicación
                Console.Error.WriteLine($"[DAL.Bitacora] Error al insertar registro de bitácora: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene todos los registros de la bitácora ordenados por fecha descendente (más recientes primero).
        /// Usado por la capa BLL para mostrar el historial completo de actividades.
        /// </summary>
        /// <returns>DataTable con todos los registros de la tabla Bitacora.</returns>
        public DataTable ObtenerTodos()
        {
            try
            {
                return acceso.Leer(
                    "SELECT fecha, usuario, modulo, actividad, detalle, criticidad " +
                    "FROM AuditoriaSesion ORDER BY fecha DESC",
                    null);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[DAL.Bitacora] Error al obtener registros: {ex.Message}");
                return new DataTable();
            }
        }

        /// <summary>
        /// Busca registros en la bitácora aplicando filtros combinados.
        /// Todos los parámetros son opcionales; si son null/vacíos no se aplican como filtro.
        /// Cumple con el requisito de "búsquedas combinadas" del módulo de Bitácora.
        /// </summary>
        /// <param name="desde">Fecha de inicio del rango de búsqueda (inclusiva).</param>
        /// <param name="hasta">Fecha de fin del rango de búsqueda (inclusiva).</param>
        /// <param name="usuario">ID del usuario a filtrar (0 = todos).</param>
        /// <param name="actividad">Texto parcial de la actividad a buscar (usa LIKE).</param>
        /// <param name="criticidad">Nivel de criticidad a filtrar (0 = todos).</param>
        /// <returns>DataTable con los registros que coinciden con los filtros aplicados.</returns>
        public DataTable BuscarPorFiltros(DateTime? desde, DateTime? hasta, int usuario, string actividad, int criticidad)
        {
            // Construimos la consulta dinámicamente según los filtros activos
            string consulta = "SELECT fecha, usuario, modulo, actividad, detalle, criticidad FROM AuditoriaSesion WHERE 1=1";
            var parametros = new System.Collections.Generic.List<SqlParameter>();

            // Filtro por rango de fechas
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

            // Filtro por usuario específico (ID > 0 significa que se seleccionó uno)
            if (usuario > 0)
            {
                consulta += " AND usuario = @usuario";
                parametros.Add(new SqlParameter("@usuario", usuario));
            }

            // Filtro por actividad con búsqueda parcial (LIKE)
            if (!string.IsNullOrWhiteSpace(actividad))
            {
                consulta += " AND actividad LIKE @actividad";
                parametros.Add(new SqlParameter("@actividad", $"%{actividad}%"));
            }

            // Filtro por criticidad (valor > 0 significa que se seleccionó una)
            if (criticidad > 0)
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
                Console.Error.WriteLine($"[DAL.Bitacora] Error en búsqueda filtrada: {ex.Message}");
                return new DataTable();
            }
        }
    }
}
