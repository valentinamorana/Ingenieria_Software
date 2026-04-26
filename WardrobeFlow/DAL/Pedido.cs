using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    /// <summary>
    /// Capa de Acceso a Datos — Pedido.
    /// Opera sobre las tablas [Pedido] y [PedidoPrenda] de WardrobeFlowDB.
    /// </summary>
    public class Pedido
    {
        private readonly Acceso acceso = Acceso.GetInstance();

        /// <summary>
        /// Devuelve todos los pedidos con nombre de cliente y empleado.
        /// Las prendas de cada pedido NO se cargan aquí — usar ObtenerConPrendas() si es necesario.
        /// </summary>
        public List<BE.Pedido> ObtenerTodos()
        {
            var lista = new List<BE.Pedido>();
            try
            {
                DataTable tabla = acceso.Leer(
                    "SELECT ped.IdPedido, ped.IdCliente, ped.IdEmpleado, ped.Estado, " +
                    "       ped.FechaPedido, ped.FechaDespacho, ped.FechaEntrega, " +
                    "       ped.MotivoCancelacion, " +
                    "       cli.Nombre + ' ' + cli.Apellido AS NombreCliente, " +
                    "       emp.Nombre + ' ' + emp.Apellido AS NombreEmpleado " +
                    "FROM Pedido ped " +
                    "INNER JOIN Cliente cli ON cli.IdCliente = ped.IdCliente " +
                    "INNER JOIN Empleado emp ON emp.IdEmpleado = ped.IdEmpleado " +
                    "ORDER BY ped.FechaPedido DESC",
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

        /// <summary>Devuelve los pedidos pendientes (para el módulo de Despacho).</summary>
        public List<BE.Pedido> ObtenerPendientes()
        {
            var lista = new List<BE.Pedido>();
            try
            {
                DataTable tabla = acceso.Leer(
                    "SELECT ped.IdPedido, ped.IdCliente, ped.IdEmpleado, ped.Estado, " +
                    "       ped.FechaPedido, ped.FechaDespacho, ped.FechaEntrega, " +
                    "       ped.MotivoCancelacion, " +
                    "       cli.Nombre + ' ' + cli.Apellido AS NombreCliente, " +
                    "       emp.Nombre + ' ' + emp.Apellido AS NombreEmpleado " +
                    "FROM Pedido ped " +
                    "INNER JOIN Cliente cli ON cli.IdCliente = ped.IdCliente " +
                    "INNER JOIN Empleado emp ON emp.IdEmpleado = ped.IdEmpleado " +
                    "WHERE ped.Estado = 0 " +   // EstadoPedido.Pendiente = 0
                    "ORDER BY ped.FechaPedido",
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

        /// <summary>Obtiene un pedido por ID incluyendo sus prendas.</summary>
        public BE.Pedido ObtenerPorId(int idPedido)
        {
            SqlParameter[] p = { new SqlParameter("@IdPedido", idPedido) };
            try
            {
                DataTable tabla = acceso.Leer(
                    "SELECT ped.IdPedido, ped.IdCliente, ped.IdEmpleado, ped.Estado, " +
                    "       ped.FechaPedido, ped.FechaDespacho, ped.FechaEntrega, " +
                    "       ped.MotivoCancelacion, " +
                    "       cli.Nombre + ' ' + cli.Apellido AS NombreCliente, " +
                    "       emp.Nombre + ' ' + emp.Apellido AS NombreEmpleado " +
                    "FROM Pedido ped " +
                    "INNER JOIN Cliente cli ON cli.IdCliente = ped.IdCliente " +
                    "INNER JOIN Empleado emp ON emp.IdEmpleado = ped.IdEmpleado " +
                    "WHERE ped.IdPedido = @IdPedido",
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

        /// <summary>
        /// Crea un nuevo pedido con sus prendas en una transacción atómica.
        /// Devuelve el ID del pedido generado.
        /// </summary>
        public int Alta(BE.Pedido pedido)
        {
            // Paso 1: insertar cabecera del pedido
            SqlParameter[] p =
            {
                new SqlParameter("@IdCliente",  pedido.IdCliente),
                new SqlParameter("@IdEmpleado", pedido.IdEmpleado),
                new SqlParameter("@Estado",     (int)pedido.Estado),
                new SqlParameter("@FechaPedido", pedido.FechaPedido)
            };

            DataTable tabla = acceso.Leer(
                "INSERT INTO Pedido (IdCliente, IdEmpleado, Estado, FechaPedido) " +
                "VALUES (@IdCliente, @IdEmpleado, @Estado, @FechaPedido); " +
                "SELECT SCOPE_IDENTITY() AS IdNuevo",
                p);

            if (tabla == null || tabla.Rows.Count == 0)
                throw new Exception("No se pudo insertar el pedido.");

            int idNuevo = Convert.ToInt32(tabla.Rows[0]["IdNuevo"]);

            // Paso 2: insertar prendas en PedidoPrenda + marcar prendas como EnUso
            foreach (var prenda in pedido.Prendas)
            {
                SqlParameter[] pp =
                {
                    new SqlParameter("@IdPedido", idNuevo),
                    new SqlParameter("@IdPrenda", prenda.IdPrenda)
                };
                acceso.Escribir(
                    "INSERT INTO PedidoPrenda (IdPedido, IdPrenda) VALUES (@IdPedido, @IdPrenda)",
                    pp);

                // Marcar prenda como EnUso (Estado=1) con el cliente del pedido
                SqlParameter[] pe =
                {
                    new SqlParameter("@Estado",     1),   // EnUso
                    new SqlParameter("@IdCliente",  pedido.IdCliente),
                    new SqlParameter("@IdPrenda",   prenda.IdPrenda)
                };
                acceso.Escribir(
                    "UPDATE Prenda SET Estado=@Estado, IdClienteActual=@IdCliente " +
                    "WHERE IdPrenda=@IdPrenda",
                    pe);
            }

            return idNuevo;
        }

        /// <summary>Marca un pedido como Despachado y registra la fecha.</summary>
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

        /// <summary>Marca un pedido como Entregado y registra la fecha.</summary>
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

        /// <summary>
        /// Cancela un pedido, guarda el motivo y libera las prendas (vuelven a Disponible).
        /// Solo aplica si el pedido está en estado Pendiente.
        /// </summary>
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

        /// <summary>
        /// Des-cancela un pedido cancelado y vuelve a marcar sus prendas como EnUso.
        /// Solo es posible si TODAS las prendas del pedido siguen Disponibles.
        /// Devuelve false si alguna prenda ya fue asignada a otro cliente.
        /// </summary>
        public bool DesCancelar(int idPedido, int idCliente)
        {
            // Verificar que todas las prendas del pedido estén disponibles (Estado=0)
            SqlParameter[] checkP = { new SqlParameter("@IdPedido", idPedido) };
            DataTable chk = acceso.Leer(
                "SELECT COUNT(*) AS Ocupadas " +
                "FROM PedidoPrenda pp " +
                "INNER JOIN Prenda pr ON pr.IdPrenda = pp.IdPrenda " +
                "WHERE pp.IdPedido = @IdPedido AND pr.Estado <> 0",
                checkP);

            if (chk == null || Convert.ToInt32(chk.Rows[0]["Ocupadas"]) > 0)
                return false;   // Hay prendas que ya no están disponibles

            // Reactivar pedido
            acceso.Escribir(
                "UPDATE Pedido SET Estado=0, MotivoCancelacion=NULL " +
                "WHERE IdPedido=@IdPedido",
                new SqlParameter[] { new SqlParameter("@IdPedido", idPedido) });

            // Volver a marcar las prendas como EnUso
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

        // ── Helpers privados ─────────────────────────────────────────────────

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
