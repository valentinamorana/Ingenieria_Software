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
                    " WHERE ped.Estado = @Estado" +
                    " ORDER BY ped.FechaPedido",
                    new[] { new SqlParameter("@Estado", (int)BE.EstadoPedido.Pendiente) });

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
                    cmd.Parameters.AddWithValue("@IdCliente", pedido.IdCliente);
                    cmd.Parameters.AddWithValue("@IdEmpleado", pedido.IdEmpleado);
                    cmd.Parameters.AddWithValue("@Estado", (int)pedido.Estado);
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
                        "UPDATE Prenda SET Estado=@Estado, IdClienteActual=@IdCliente " +
                        "WHERE IdPrenda=@IdPrenda",
                        conexion, tx))
                    {
                        cmdPr.Parameters.AddWithValue("@Estado",    (int)BE.EstadoPrenda.EnUso);
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
            try
            {
                acceso.Escribir(
                    "UPDATE Pedido SET Estado=@Estado, FechaDespacho=@FechaDespacho " +
                    "WHERE IdPedido=@IdPedido",
                    new SqlParameter[]
                    {
                        new SqlParameter("@Estado",        (int)BE.EstadoPedido.Despachado),
                        new SqlParameter("@FechaDespacho", DateTime.Now),
                        new SqlParameter("@IdPedido",      idPedido)
                    });
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al despachar el pedido ID {idPedido}.", ex);
            }
        }

        // Marca un pedido como Entregado y registra la fecha.
        public void MarcarEntregado(int idPedido)
        {
            try
            {
                acceso.Escribir(
                    "UPDATE Pedido SET Estado=@Estado, FechaEntrega=@FechaEntrega " +
                    "WHERE IdPedido=@IdPedido",
                    new SqlParameter[]
                    {
                        new SqlParameter("@Estado",       (int)BE.EstadoPedido.Entregado),
                        new SqlParameter("@FechaEntrega", DateTime.Now),
                        new SqlParameter("@IdPedido",     idPedido)
                    });
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al marcar como entregado el pedido ID {idPedido}.", ex);
            }
        }

        // Pasa las prendas del pedido a EnLimpieza y limpia IdClienteActual.
        public void RegistrarDevolucion(int idPedido)
        {
            acceso.EjecutarTransaccion((conexion, tx) =>
            {
                // Prendas del pedido → EnLimpieza, sin cliente asignado
                using (var cmd = new SqlCommand(
                    "UPDATE Prenda SET Estado=@Estado, IdClienteActual=NULL " +
                    "WHERE IdPrenda IN " +
                    "  (SELECT IdPrenda FROM PedidoPrenda WHERE IdPedido=@IdPedido)",
                    conexion, tx))
                {
                    cmd.Parameters.AddWithValue("@Estado",   (int)BE.EstadoPrenda.EnLimpieza);
                    cmd.Parameters.AddWithValue("@IdPedido", idPedido);
                    cmd.ExecuteNonQuery();
                }
            });
        }

        // Cancela el pedido, guarda el motivo y libera las prendas a Disponible.
        // Ambas operaciones se ejecutan en una única transacción: si falla alguna,
        // ningún cambio queda aplicado (integridad transaccional).
        public void Cancelar(int idPedido, string motivo)
        {
            acceso.EjecutarTransaccion((conexion, tx) =>
            {
                using (var cmdPedido = new SqlCommand(
                    "UPDATE Pedido SET Estado=@Estado, MotivoCancelacion=@Motivo " +
                    "WHERE IdPedido=@IdPedido",
                    conexion, tx))
                {
                    cmdPedido.Parameters.AddWithValue("@Estado",   (int)BE.EstadoPedido.Cancelado);
                    cmdPedido.Parameters.AddWithValue("@Motivo",   (object)motivo ?? DBNull.Value);
                    cmdPedido.Parameters.AddWithValue("@IdPedido", idPedido);
                    cmdPedido.ExecuteNonQuery();
                }

                // Liberar prendas del pedido → Disponible
                using (var cmdPrendas = new SqlCommand(
                    "UPDATE Prenda SET Estado=@Estado, IdClienteActual=NULL " +
                    "WHERE IdPrenda IN (SELECT IdPrenda FROM PedidoPrenda WHERE IdPedido=@IdPedido)",
                    conexion, tx))
                {
                    cmdPrendas.Parameters.AddWithValue("@Estado",   (int)BE.EstadoPrenda.Disponible);
                    cmdPrendas.Parameters.AddWithValue("@IdPedido", idPedido);
                    cmdPrendas.ExecuteNonQuery();
                }
            });
        }

        // Revierte la cancelación. Devuelve false si alguna prenda ya no está Disponible.
        // La verificación de disponibilidad se ejecuta DENTRO de la transacción para
        // evitar race conditions: si otra operación cambia el estado de una prenda entre
        // la verificación y el UPDATE, la transacción lo detecta con bloqueo consistente.
        public bool DesCancelar(int idPedido, int idCliente)
        {
            bool puedeReactivar = true;

            acceso.EjecutarTransaccion((conexion, tx) =>
            {
                // Verificar disponibilidad DENTRO de la transacción (con bloqueo compartido)
                using (var cmdCheck = new SqlCommand(
                    "SELECT COUNT(*) AS Ocupadas " +
                    "FROM PedidoPrenda pp " +
                    "INNER JOIN Prenda pr ON pr.IdPrenda = pp.IdPrenda " +
                    "WHERE pp.IdPedido = @IdPedido AND pr.Estado <> @Estado",
                    conexion, tx))
                {
                    cmdCheck.Parameters.AddWithValue("@Estado",   (int)BE.EstadoPrenda.Disponible);
                    cmdCheck.Parameters.AddWithValue("@IdPedido", idPedido);
                    int ocupadas = Convert.ToInt32(cmdCheck.ExecuteScalar());
                    if (ocupadas > 0)
                    {
                        puedeReactivar = false;
                        return;  // salir del lambda; la transacción se revierte en EjecutarTransaccion
                    }
                }

                using (var cmdPedido = new SqlCommand(
                    "UPDATE Pedido SET Estado=@Estado, MotivoCancelacion=NULL " +
                    "WHERE IdPedido=@IdPedido",
                    conexion, tx))
                {
                    cmdPedido.Parameters.AddWithValue("@Estado",   (int)BE.EstadoPedido.Pendiente);
                    cmdPedido.Parameters.AddWithValue("@IdPedido", idPedido);
                    cmdPedido.ExecuteNonQuery();
                }

                using (var cmdPrendas = new SqlCommand(
                    "UPDATE Prenda SET Estado=@Estado, IdClienteActual=@IdCliente " +
                    "WHERE IdPrenda IN (SELECT IdPrenda FROM PedidoPrenda WHERE IdPedido=@IdPedido)",
                    conexion, tx))
                {
                    cmdPrendas.Parameters.AddWithValue("@Estado",    (int)BE.EstadoPrenda.EnUso);
                    cmdPrendas.Parameters.AddWithValue("@IdCliente", idCliente);
                    cmdPrendas.Parameters.AddWithValue("@IdPedido",  idPedido);
                    cmdPrendas.ExecuteNonQuery();
                }
            });

            return puedeReactivar;
        }

        private List<BE.Prenda> ObtenerPrendasDePedido(int idPedido)
        {
            var lista = new List<BE.Prenda>();
            try
            {
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
                    IdPrenda = Convert.ToInt32(row["IdPrenda"]),
                    Nombre = row["Nombre"].ToString(),
                    Descripcion = row["Descripcion"] != DBNull.Value ? row["Descripcion"].ToString() : null,
                    Talle = row["Talle"] != DBNull.Value ? row["Talle"].ToString() : null,
                    Color = row["Color"] != DBNull.Value ? row["Color"].ToString() : null,
                    Categoria = row["Categoria"] != DBNull.Value ? row["Categoria"].ToString() : null,
                    Estado = (BE.EstadoPrenda)Convert.ToInt32(row["Estado"]),
                    FechaAlta = Convert.ToDateTime(row["FechaAlta"])
                });
            }

            return lista;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener las prendas del pedido ID {idPedido}.", ex);
            }
        }

        private BE.Pedido MapearCabecera(DataRow row)
        {
            return new BE.Pedido
            {
                IdPedido = Convert.ToInt32(row["IdPedido"]),
                IdCliente = Convert.ToInt32(row["IdCliente"]),
                IdEmpleado = Convert.ToInt32(row["IdEmpleado"]),
                Estado = (BE.EstadoPedido)Convert.ToInt32(row["Estado"]),
                FechaPedido = Convert.ToDateTime(row["FechaPedido"]),
                FechaDespacho = row["FechaDespacho"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["FechaDespacho"]) : null,
                FechaEntrega = row["FechaEntrega"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["FechaEntrega"]) : null,
                MotivoCancelacion = row.Table.Columns.Contains("MotivoCancelacion") && row["MotivoCancelacion"] != DBNull.Value
                                        ? row["MotivoCancelacion"].ToString() : null,
                NombreCliente = row["NombreCliente"].ToString(),
                NombreEmpleado = row["NombreEmpleado"].ToString()
            };
        }
    }
}
