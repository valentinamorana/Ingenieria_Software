using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    /// <summary>Acceso a datos de las tablas [Pedido] y [PedidoPrenda].</summary>
    public class Pedido : BaseDAL<BE.Pedido>
    {

        // SELECT base compartido por todos los métodos de lectura
        private const string SELECT_BASE =
            "SELECT ped.IdPedido, ped.IdCliente, ped.IdEmpleado, ped.Estado, " +
            "       ped.FechaPedido, ped.FechaDespacho, ped.FechaEntrega, " +
            "       ped.MotivoCancelacion, " +
            "       cli.Nombre + ' ' + cli.Apellido AS NombreCliente, " +
            "       emp.Nombre + ' ' + emp.Apellido AS NombreEmpleado " +
            "FROM Pedido ped " +
            "INNER JOIN Cliente cli ON cli.IdCliente = ped.IdCliente " +
            "INNER JOIN Empleado emp ON emp.IdEmpleado = ped.IdEmpleado";

        // Devuelve todos los pedidos. Las prendas se cargan por separado en ObtenerPorId.
        public override List<BE.Pedido> ObtenerTodos()
        {
            var lista = new List<BE.Pedido>();
            try
            {
                DataTable tabla = acceso.Leer(
                    SELECT_BASE + " ORDER BY ped.FechaPedido DESC",
                    null);

                foreach (DataRow row in tabla.Rows)
                    lista.Add(MapearCabecera(row));
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener la lista de pedidos.", ex);
            }
            return lista;
        }

        // Devuelve los pedidos pendientes (para el módulo de Despacho).
        public List<BE.Pedido> ObtenerPendientes()
        {
            var lista = new List<BE.Pedido>();
            try
            {
                DataTable tabla = acceso.Leer(
                    SELECT_BASE +
                    " WHERE ped.Estado = 0" +   // EstadoPedido.Pendiente = 0
                    " ORDER BY ped.FechaPedido",
                    null);

                foreach (DataRow row in tabla.Rows)
                    lista.Add(MapearCabecera(row));
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener pedidos pendientes.", ex);
            }
            return lista;
        }

        // Obtiene un pedido por ID incluyendo sus prendas.
        public override BE.Pedido ObtenerPorId(int idPedido)
        {
            SqlParameter[] p = { new SqlParameter("@IdPedido", idPedido) };
            try
            {
                DataTable tabla = acceso.Leer(
                    SELECT_BASE + " WHERE ped.IdPedido = @IdPedido",
                    p);

                if (tabla == null || tabla.Rows.Count == 0) return null;

                var pedido = MapearCabecera(tabla.Rows[0]);
                pedido.Prendas = ObtenerPrendasDePedido(idPedido);
                return pedido;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener el pedido.", ex);
            }
        }

        // Inserta el pedido y sus prendas en una transacción. Devuelve el ID generado.
        public int Alta(BE.Pedido pedido)
        {
            int idNuevo = 0;

            acceso.EjecutarTransaccion((conexion, tx) =>
            {
                using (var cmd = new SqlCommand(
                    "INSERT INTO Pedido (IdCliente, IdEmpleado, Estado, FechaPedido) " +
                    "VALUES (@IdCliente, @IdEmpleado, @Estado, @FechaPedido); " +
                    "SELECT SCOPE_IDENTITY() AS IdNuevo",
                    conexion, tx))
                {
                    cmd.Parameters.AddWithValue("@IdCliente",   pedido.IdCliente);
                    cmd.Parameters.AddWithValue("@IdEmpleado",  pedido.IdEmpleado);
                    cmd.Parameters.AddWithValue("@Estado",      (int)pedido.Estado);
                    cmd.Parameters.AddWithValue("@FechaPedido", pedido.FechaPedido);

                    var resultado = cmd.ExecuteScalar();
                    if (resultado == null || resultado == DBNull.Value)
                        throw new Exception("No se pudo insertar el pedido en la base de datos.");

                    idNuevo = Convert.ToInt32(resultado);
                }

                foreach (var prenda in pedido.Prendas)
                {
                    using (var cmdPP = new SqlCommand(
                        "INSERT INTO PedidoPrenda (IdPedido, IdPrenda) VALUES (@IdPedido, @IdPrenda)",
                        conexion, tx))
                    {
                        cmdPP.Parameters.AddWithValue("@IdPedido", idNuevo);
                        cmdPP.Parameters.AddWithValue("@IdPrenda", prenda.IdPrenda);
                        cmdPP.ExecuteNonQuery();
                    }

                    using (var cmdPr = new SqlCommand(
                        "UPDATE Prenda SET Estado=1, IdClienteActual=@IdCliente " +
                        "WHERE IdPrenda=@IdPrenda",   // Estado 1 = EnUso
                        conexion, tx))
                    {
                        cmdPr.Parameters.AddWithValue("@IdCliente", pedido.IdCliente);
                        cmdPr.Parameters.AddWithValue("@IdPrenda",  prenda.IdPrenda);
                        cmdPr.ExecuteNonQuery();
                    }
                }
            });

            return idNuevo;
        }

        // Marca un pedido como Despachado y registra la fecha.
        public void Despachar(int idPedido)
        {
            SqlParameter[] p =
            {
                new SqlParameter("@FechaDespacho", DateTime.Now),
                new SqlParameter("@IdPedido",      idPedido)
            };
            acceso.Escribir(
                "UPDATE Pedido SET Estado=1, FechaDespacho=@FechaDespacho " +
                "WHERE IdPedido=@IdPedido",   // Estado 1 = Despachado
                p);
        }

        // Marca un pedido como Entregado y registra la fecha.
        public void MarcarEntregado(int idPedido)
        {
            SqlParameter[] p =
            {
                new SqlParameter("@FechaEntrega", DateTime.Now),
                new SqlParameter("@IdPedido",     idPedido)
            };
            acceso.Escribir(
                "UPDATE Pedido SET Estado=2, FechaEntrega=@FechaEntrega " +
                "WHERE IdPedido=@IdPedido",   // Estado 2 = Entregado
                p);
        }

        // Pasa las prendas del pedido a EnLimpieza y limpia IdClienteActual.
        public void RegistrarDevolucion(int idPedido)
        {
            acceso.EjecutarTransaccion((conexion, tx) =>
            {
                // Prendas del pedido → EnLimpieza (2), sin cliente asignado
                using (var cmd = new SqlCommand(
                    "UPDATE Prenda SET Estado=2, IdClienteActual=NULL " +
                    "WHERE IdPrenda IN " +
                    "  (SELECT IdPrenda FROM PedidoPrenda WHERE IdPedido=@IdPedido)",
                    conexion, tx))
                {
                    cmd.Parameters.AddWithValue("@IdPedido", idPedido);
                    cmd.ExecuteNonQuery();
                }
            });
        }

        // Cancela el pedido, guarda el motivo y libera las prendas a Disponible.
        public void Cancelar(int idPedido, string motivo)
        {
            SqlParameter[] p =
            {
                new SqlParameter("@IdPedido", idPedido),
                new SqlParameter("@Motivo",   (object)motivo ?? DBNull.Value)
            };
            acceso.Escribir(
                "UPDATE Pedido SET Estado=3, MotivoCancelacion=@Motivo " +
                "WHERE IdPedido=@IdPedido",
                p);

            // Liberar prendas del pedido → Disponible
            acceso.Escribir(
                "UPDATE Prenda SET Estado=0, IdClienteActual=NULL " +
                "WHERE IdPrenda IN (SELECT IdPrenda FROM PedidoPrenda WHERE IdPedido=@IdPedido)",
                new SqlParameter[] { new SqlParameter("@IdPedido", idPedido) });
        }

        // Revierte la cancelación. Devuelve false si alguna prenda ya no está Disponible.
        public bool DesCancelar(int idPedido, int idCliente)
        {
            SqlParameter[] checkP = { new SqlParameter("@IdPedido", idPedido) };
            DataTable chk = acceso.Leer(
                "SELECT COUNT(*) AS Ocupadas " +
                "FROM PedidoPrenda pp " +
                "INNER JOIN Prenda pr ON pr.IdPrenda = pp.IdPrenda " +
                "WHERE pp.IdPedido = @IdPedido AND pr.Estado <> 0",
                checkP);

            if (chk == null || Convert.ToInt32(chk.Rows[0]["Ocupadas"]) > 0)
                return false;

            acceso.Escribir(
                "UPDATE Pedido SET Estado=0, MotivoCancelacion=NULL " +
                "WHERE IdPedido=@IdPedido",
                new SqlParameter[] { new SqlParameter("@IdPedido", idPedido) });

            SqlParameter[] pp =
            {
                new SqlParameter("@IdCliente", idCliente),
                new SqlParameter("@IdPedido",  idPedido)
            };
            acceso.Escribir(
                "UPDATE Prenda SET Estado=1, IdClienteActual=@IdCliente " +
                "WHERE IdPrenda IN (SELECT IdPrenda FROM PedidoPrenda WHERE IdPedido=@IdPedido)",
                pp);

            return true;
        }

        private List<BE.Prenda> ObtenerPrendasDePedido(int idPedido)
        {
            var lista = new List<BE.Prenda>();
            SqlParameter[] p = { new SqlParameter("@IdPedido", idPedido) };

            DataTable tabla = acceso.Leer(
                "SELECT pr.IdPrenda, pr.Nombre, pr.Descripcion, pr.Talle, pr.Color, " +
                "       pr.Categoria, pr.Estado, pr.IdClienteActual, pr.FechaAlta, " +
                "       NULL AS NombreCliente " +
                "FROM PedidoPrenda pp " +
                "INNER JOIN Prenda pr ON pr.IdPrenda = pp.IdPrenda " +
                "WHERE pp.IdPedido = @IdPedido",
                p);

            foreach (DataRow row in tabla.Rows)
            {
                lista.Add(new BE.Prenda
                {
                    IdPrenda    = Convert.ToInt32(row["IdPrenda"]),
                    Nombre      = row["Nombre"].ToString(),
                    Descripcion = row["Descripcion"] != DBNull.Value ? row["Descripcion"].ToString() : null,
                    Talle       = row["Talle"]       != DBNull.Value ? row["Talle"].ToString()       : null,
                    Color       = row["Color"]       != DBNull.Value ? row["Color"].ToString()       : null,
                    Categoria   = row["Categoria"]   != DBNull.Value ? row["Categoria"].ToString()   : null,
                    Estado      = (BE.EstadoPrenda)Convert.ToInt32(row["Estado"]),
                    FechaAlta   = Convert.ToDateTime(row["FechaAlta"])
                });
            }

            return lista;
        }

        private BE.Pedido MapearCabecera(DataRow row)
        {
            return new BE.Pedido
            {
                IdPedido           = Convert.ToInt32(row["IdPedido"]),
                IdCliente          = Convert.ToInt32(row["IdCliente"]),
                IdEmpleado         = Convert.ToInt32(row["IdEmpleado"]),
                Estado             = (BE.EstadoPedido)Convert.ToInt32(row["Estado"]),
                FechaPedido        = Convert.ToDateTime(row["FechaPedido"]),
                FechaDespacho      = row["FechaDespacho"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["FechaDespacho"]) : null,
                FechaEntrega       = row["FechaEntrega"]  != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["FechaEntrega"])  : null,
                MotivoCancelacion  = row.Table.Columns.Contains("MotivoCancelacion") && row["MotivoCancelacion"] != DBNull.Value
                                        ? row["MotivoCancelacion"].ToString() : null,
                NombreCliente      = row["NombreCliente"].ToString(),
                NombreEmpleado     = row["NombreEmpleado"].ToString()
            };
        }
    }
}
