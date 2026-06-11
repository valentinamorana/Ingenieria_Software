using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    /// <summary>
    /// Acceso a datos para la tabla [PedidoHistorial].
    /// Responsabilidades:
    ///   - Insertar lotes de cambios (todos los campos de un evento van en una transacción).
    ///   - Consultar el historial de un pedido con filtros opcionales.
    ///   - Obtener el próximo IdOperacion para agrupar los campos de un mismo evento.
    /// </summary>
    public class PedidoHistorial
    {
        private readonly Acceso acceso = Acceso.GetInstance();

        // ── Escritura ─────────────────────────────────────────────────────────

        /// <summary>
        /// Inserta una lista de registros de historial en una sola transacción.
        /// Todos los registros de un evento comparten el mismo IdOperacion.
        /// </summary>
        public void RegistrarCambios(List<BE.PedidoHistorial> cambios)
        {
            if (cambios == null || cambios.Count == 0) return;

            acceso.EjecutarTransaccion((conn, tx) =>
            {
                foreach (var c in cambios)
                {
                    using (var cmd = new SqlCommand(
                        "INSERT INTO PedidoHistorial " +
                        "(IdPedido, IdOperacion, Fecha, IdUsuario, NombreUsuario, Accion, Campo, ValorAnterior, ValorNuevo) " +
                        "VALUES (@IdPedido, @IdOperacion, @Fecha, @IdUsuario, @NombreUsuario, @Accion, @Campo, @ValorAnterior, @ValorNuevo)",
                        conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@IdPedido",      c.IdPedido);
                        cmd.Parameters.AddWithValue("@IdOperacion",   c.IdOperacion);
                        cmd.Parameters.AddWithValue("@Fecha",         c.Fecha);
                        cmd.Parameters.AddWithValue("@IdUsuario",     (object)c.IdUsuario     ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@NombreUsuario", (object)c.NombreUsuario ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Accion",        c.Accion);
                        cmd.Parameters.AddWithValue("@Campo",         c.Campo);
                        cmd.Parameters.AddWithValue("@ValorAnterior", (object)c.ValorAnterior ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ValorNuevo",    (object)c.ValorNuevo    ?? DBNull.Value);
                        cmd.ExecuteNonQuery();
                    }
                }
            });
        }

        /// <summary>
        /// Devuelve el próximo IdOperacion para el pedido indicado.
        /// Permite agrupar todos los campos de un mismo evento de negocio.
        /// </summary>
        public int ObtenerSiguienteIdOperacion(int idPedido)
        {
            try
            {
                var dt = acceso.Leer(
                    "SELECT ISNULL(MAX(IdOperacion), 0) + 1 AS Siguiente " +
                    "FROM PedidoHistorial WHERE IdPedido = @IdPedido",
                    new[] { new SqlParameter("@IdPedido", idPedido) });

                return dt.Rows.Count > 0 ? Convert.ToInt32(dt.Rows[0]["Siguiente"]) : 1;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener el siguiente IdOperacion del historial de pedidos.", ex);
            }
        }

        // ── Consulta ──────────────────────────────────────────────────────────

        /// <summary>
        /// Devuelve el historial de cambios de un pedido, con filtros opcionales.
        /// Ordenado por Fecha DESC, IdHistorial DESC (más reciente primero).
        /// </summary>
        public DataTable ObtenerPorPedido(int idPedido, string accion = null,
                                          DateTime? desde = null, DateTime? hasta = null)
        {
            var condiciones = new System.Text.StringBuilder(
                "WHERE IdPedido = @IdPedido");
            var parametros = new List<SqlParameter>
            {
                new SqlParameter("@IdPedido", idPedido)
            };

            if (!string.IsNullOrEmpty(accion))
            {
                condiciones.Append(" AND Accion = @Accion");
                parametros.Add(new SqlParameter("@Accion", accion));
            }
            if (desde.HasValue)
            {
                condiciones.Append(" AND Fecha >= @Desde");
                parametros.Add(new SqlParameter("@Desde", desde.Value));
            }
            if (hasta.HasValue)
            {
                condiciones.Append(" AND Fecha <= @Hasta");
                parametros.Add(new SqlParameter("@Hasta", hasta.Value.AddDays(1)));
            }

            try
            {
                return acceso.Leer(
                    "SELECT IdHistorial, IdOperacion, Fecha, NombreUsuario, " +
                    "       Accion, Campo, ValorAnterior, ValorNuevo " +
                    "FROM PedidoHistorial " +
                    condiciones +
                    " ORDER BY Fecha DESC, IdHistorial DESC",
                    parametros.ToArray());
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener el historial del pedido.", ex);
            }
        }

        /// <summary>
        /// Devuelve todos los registros de una misma operación (mismo IdOperacion + IdPedido).
        /// Se usa para restaurar de forma atómica todos los campos del evento.
        /// </summary>
        public List<BE.PedidoHistorial> ObtenerPorOperacion(int idPedido, int idOperacion)
        {
            var lista = new List<BE.PedidoHistorial>();
            try
            {
                var dt = acceso.Leer(
                    "SELECT IdHistorial, IdPedido, IdOperacion, Fecha, IdUsuario, " +
                    "       NombreUsuario, Accion, Campo, ValorAnterior, ValorNuevo " +
                    "FROM PedidoHistorial " +
                    "WHERE IdPedido = @IdPedido AND IdOperacion = @IdOperacion",
                    new[]
                    {
                        new SqlParameter("@IdPedido",    idPedido),
                        new SqlParameter("@IdOperacion", idOperacion)
                    });

                foreach (DataRow row in dt.Rows)
                    lista.Add(Mapear(row));
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener los cambios de la operación.", ex);
            }
            return lista;
        }

        // La restauración de los campos del Pedido se realiza de forma ATÓMICA en
        // DAL.Pedido.RestaurarOperacionAtomica (campos + reconciliación de prendas en una
        // única transacción). Esta clase solo registra y consulta el historial.

        // ── Helper ────────────────────────────────────────────────────────────

        private BE.PedidoHistorial Mapear(DataRow row)
        {
            return new BE.PedidoHistorial
            {
                IdHistorial   = Convert.ToInt32(row["IdHistorial"]),
                IdPedido      = Convert.ToInt32(row["IdPedido"]),
                IdOperacion   = Convert.ToInt32(row["IdOperacion"]),
                Fecha         = Convert.ToDateTime(row["Fecha"]),
                IdUsuario     = row["IdUsuario"]     != DBNull.Value ? (int?)Convert.ToInt32(row["IdUsuario"]) : null,
                NombreUsuario = row["NombreUsuario"] != DBNull.Value ? row["NombreUsuario"].ToString() : null,
                Accion        = row["Accion"].ToString(),
                Campo         = row["Campo"].ToString(),
                ValorAnterior = row["ValorAnterior"] != DBNull.Value ? row["ValorAnterior"].ToString() : null,
                ValorNuevo    = row["ValorNuevo"]    != DBNull.Value ? row["ValorNuevo"].ToString()    : null,
            };
        }
    }
}
