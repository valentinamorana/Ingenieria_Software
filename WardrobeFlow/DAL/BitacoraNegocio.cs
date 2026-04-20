using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    /// <summary>
    /// Capa de Acceso a Datos — BitacoraNegocio.
    /// Opera sobre la tabla [BitacoraNegocio] de WardrobeFlowDB.
    /// </summary>
    public class BitacoraNegocio
    {
        private readonly Acceso acceso = Acceso.GetInstance();

        /// <summary>Registra un evento de negocio en la bitácora.</summary>
        public void Registrar(BE.BitacoraNegocio evento)
        {
            SqlParameter[] p =
            {
                new SqlParameter("@Fecha",       evento.Fecha),
                new SqlParameter("@Tipo",        evento.Tipo.ToString()),
                new SqlParameter("@IdUsuario",   (object)evento.IdUsuario ?? DBNull.Value),
                new SqlParameter("@IdPedido",    (object)evento.IdPedido  ?? DBNull.Value),
                new SqlParameter("@IdPrenda",    (object)evento.IdPrenda  ?? DBNull.Value),
                new SqlParameter("@IdCliente",   (object)evento.IdCliente ?? DBNull.Value),
                new SqlParameter("@Descripcion", evento.Descripcion)
            };
            acceso.Escribir(
                "INSERT INTO BitacoraNegocio (Fecha, Tipo, IdUsuario, IdPedido, IdPrenda, IdCliente, Descripcion) " +
                "VALUES (@Fecha, @Tipo, @IdUsuario, @IdPedido, @IdPrenda, @IdCliente, @Descripcion)",
                p);
        }

        /// <summary>Devuelve todos los eventos ordenados por fecha descendente.</summary>
        public DataTable ObtenerTodos()
        {
            return acceso.Leer(
                "SELECT bn.IdEvento, bn.Fecha, bn.Tipo, " +
                "       u.Username AS UsernameUsuario, " +
                "       c.Nombre + ' ' + c.Apellido AS NombreCliente, " +
                "       bn.IdPedido, bn.IdPrenda, bn.IdCliente, bn.Descripcion " +
                "FROM BitacoraNegocio bn " +
                "LEFT JOIN Usuario u ON u.IdUsuario = bn.IdUsuario " +
                "LEFT JOIN Cliente c ON c.IdCliente = bn.IdCliente " +
                "ORDER BY bn.Fecha DESC",
                null);
        }

        /// <summary>Devuelve eventos filtrados por tipo, rango de fechas y entidades opcionales.</summary>
        public DataTable BuscarPorFiltros(
            DateTime? desde, DateTime? hasta,
            string tipo, int? idCliente, int? idPedido)
        {
            var condiciones = new System.Text.StringBuilder("WHERE 1=1");
            var parametros  = new List<SqlParameter>();

            if (desde.HasValue)
            {
                condiciones.Append(" AND bn.Fecha >= @Desde");
                parametros.Add(new SqlParameter("@Desde", desde.Value));
            }
            if (hasta.HasValue)
            {
                condiciones.Append(" AND bn.Fecha <= @Hasta");
                parametros.Add(new SqlParameter("@Hasta", hasta.Value.AddDays(1)));
            }
            if (!string.IsNullOrEmpty(tipo))
            {
                condiciones.Append(" AND bn.Tipo = @Tipo");
                parametros.Add(new SqlParameter("@Tipo", tipo));
            }
            if (idCliente.HasValue && idCliente.Value > 0)
            {
                condiciones.Append(" AND bn.IdCliente = @IdCliente");
                parametros.Add(new SqlParameter("@IdCliente", idCliente.Value));
            }
            if (idPedido.HasValue && idPedido.Value > 0)
            {
                condiciones.Append(" AND bn.IdPedido = @IdPedido");
                parametros.Add(new SqlParameter("@IdPedido", idPedido.Value));
            }

            string sql =
                "SELECT bn.IdEvento, bn.Fecha, bn.Tipo, " +
                "       u.Username AS UsernameUsuario, " +
                "       c.Nombre + ' ' + c.Apellido AS NombreCliente, " +
                "       bn.IdPedido, bn.IdPrenda, bn.IdCliente, bn.Descripcion " +
                "FROM BitacoraNegocio bn " +
                "LEFT JOIN Usuario u ON u.IdUsuario = bn.IdUsuario " +
                "LEFT JOIN Cliente c ON c.IdCliente = bn.IdCliente " +
                condiciones + " ORDER BY bn.Fecha DESC";

            return acceso.Leer(sql, parametros.ToArray());
        }
    }
}
