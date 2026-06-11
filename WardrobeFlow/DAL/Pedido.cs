using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

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
                        "WHERE IdPrenda=@IdPrenda AND Estado=@EstadoDisponible",
                        conexion, tx))
                    {
                        cmdPr.Parameters.AddWithValue("@Estado",           (int)BE.EstadoPrenda.EnUso);
                        cmdPr.Parameters.AddWithValue("@IdCliente",        pedido.IdCliente);
                        cmdPr.Parameters.AddWithValue("@IdPrenda",         prenda.IdPrenda);
                        cmdPr.Parameters.AddWithValue("@EstadoDisponible", (int)BE.EstadoPrenda.Disponible);
                        // Control de concurrencia (anti-TOCTOU): si otra operación tomó la prenda
                        // entre la validación y este UPDATE, no se afecta ninguna fila → se aborta
                        // la transacción (rollback en EjecutarTransaccion) en vez de pisar el estado.
                        if (cmdPr.ExecuteNonQuery() == 0)
                            throw new BE.AppException("err.dal.pedido.prenda_tomada",
                                "La prenda '{0}' ya no está disponible. Actualizá la selección e intentá de nuevo.",
                                prenda.Nombre);
                    }
                }
            });

            RecalcularDV();   // T07: DV multi-tabla (pedido + líneas)
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
            RecalcularDV();   // T07
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
            RecalcularDV();   // T07
        }

        // Pasa a EnLimpieza SOLO las prendas del pedido que siguen EnUso por este cliente.
        // Es idempotente: una segunda devolución del mismo pedido no afecta filas (las prendas
        // ya no están EnUso) y no pisa prendas que ya hayan vuelto a circular en otro pedido.
        // Devuelve la cantidad de prendas efectivamente devueltas.
        public int RegistrarDevolucion(int idPedido, int idCliente)
        {
            int afectadas = 0;
            acceso.EjecutarTransaccion((conexion, tx) =>
            {
                using (var cmd = new SqlCommand(
                    "UPDATE Prenda SET Estado=@Estado, IdClienteActual=NULL " +
                    "WHERE Estado=@EstadoEnUso AND IdClienteActual=@IdCliente AND IdPrenda IN " +
                    "  (SELECT IdPrenda FROM PedidoPrenda WHERE IdPedido=@IdPedido)",
                    conexion, tx))
                {
                    cmd.Parameters.AddWithValue("@Estado",      (int)BE.EstadoPrenda.EnLimpieza);
                    cmd.Parameters.AddWithValue("@EstadoEnUso", (int)BE.EstadoPrenda.EnUso);
                    cmd.Parameters.AddWithValue("@IdCliente",   idCliente);
                    cmd.Parameters.AddWithValue("@IdPedido",    idPedido);
                    afectadas = cmd.ExecuteNonQuery();
                }
            });
            if (afectadas > 0) RecalcularDV();   // T07 — mantener el DV del pedido consistente
            return afectadas;
        }

        // Reconcilia el estado de las prendas del pedido con el estado ACTUAL del pedido.
        // Se usa tras restaurar un pedido desde el historial: si quedó en un estado activo
        // (Pendiente/Despachado/Entregado) sus prendas deben estar EnUso del cliente; si quedó
        // Cancelado, deben estar Disponibles. Evita dejar el stock inconsistente.
        public void ReconciliarPrendasConEstado(int idPedido)
        {
            acceso.EjecutarTransaccion((conexion, tx) => ReconciliarEnTx(conexion, tx, idPedido));
        }

        // Núcleo de la reconciliación, sobre una transacción YA abierta. Reutilizable por la
        // restauración atómica (RestaurarOperacionAtomica) para no abrir una segunda transacción.
        private void ReconciliarEnTx(SqlConnection conexion, SqlTransaction tx, int idPedido)
        {
            int estado, idCliente;
            using (var cmd = new SqlCommand(
                "SELECT Estado, IdCliente FROM Pedido WHERE IdPedido=@IdPedido", conexion, tx))
            {
                cmd.Parameters.AddWithValue("@IdPedido", idPedido);
                using (var rd = cmd.ExecuteReader())
                {
                    if (!rd.Read()) return;
                    estado    = Convert.ToInt32(rd["Estado"]);
                    idCliente = Convert.ToInt32(rd["IdCliente"]);
                }
            }

            bool cancelado = estado == (int)BE.EstadoPedido.Cancelado;

            using (var cmd = new SqlCommand(
                "UPDATE Prenda SET Estado=@Estado, IdClienteActual=@IdCliente " +
                "WHERE IdPrenda IN (SELECT IdPrenda FROM PedidoPrenda WHERE IdPedido=@IdPedido)",
                conexion, tx))
            {
                cmd.Parameters.AddWithValue("@Estado",
                    cancelado ? (int)BE.EstadoPrenda.Disponible : (int)BE.EstadoPrenda.EnUso);
                cmd.Parameters.AddWithValue("@IdCliente",
                    cancelado ? (object)DBNull.Value : idCliente);
                cmd.Parameters.AddWithValue("@IdPedido", idPedido);
                cmd.ExecuteNonQuery();
            }
        }

        // ── T06b — Restauración ATÓMICA desde el historial ──────────────────────
        // Revierte TODOS los campos del pedido a sus valores anteriores Y reconcilia el estado
        // de sus prendas dentro de UNA ÚNICA transacción: si cualquier paso falla, se revierte
        // todo (EjecutarTransaccion hace Rollback), evitando que el pedido quede con un estado y
        // las prendas con otro. El DV se recalcula aparte (es recomputable y no debe abortar el rollback).
        public void RestaurarOperacionAtomica(int idPedido, IList<(string Campo, string ValorAnterior)> campos)
        {
            acceso.EjecutarTransaccion((conexion, tx) =>
            {
                foreach (var c in campos)
                    RestaurarCampoEnTx(conexion, tx, idPedido, c.Campo, c.ValorAnterior);
                ReconciliarEnTx(conexion, tx, idPedido);
            });
        }

        // Restaura un campo de [Pedido] a su valor anterior, sobre una transacción YA abierta.
        // Campos soportados: Estado | FechaDespacho | FechaEntrega | MotivoCancelacion.
        // "Prendas" (informativo: el estado de las prendas se reconcilia aparte) y "FechaPedido"
        // (inmutable una vez creado el pedido) se omiten silenciosamente.
        private void RestaurarCampoEnTx(SqlConnection conexion, SqlTransaction tx,
                                        int idPedido, string campo, string valorAnterior)
        {
            string sql;
            SqlParameter[] parametros;

            switch (campo)
            {
                case "Estado":
                    if (!Enum.TryParse(valorAnterior, out BE.EstadoPedido estado))
                        throw new Exception($"Valor de estado inválido para restaurar: '{valorAnterior}'.");
                    sql = "UPDATE Pedido SET Estado = @Valor WHERE IdPedido = @IdPedido";
                    parametros = new[]
                    {
                        new SqlParameter("@Valor",    (int)estado),
                        new SqlParameter("@IdPedido", idPedido)
                    };
                    break;

                case "FechaDespacho":
                    sql = "UPDATE Pedido SET FechaDespacho = @Valor WHERE IdPedido = @IdPedido";
                    parametros = new[]
                    {
                        new SqlParameter("@Valor", string.IsNullOrEmpty(valorAnterior)
                            ? (object)DBNull.Value
                            : DateTime.ParseExact(valorAnterior, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)),
                        new SqlParameter("@IdPedido", idPedido)
                    };
                    break;

                case "FechaEntrega":
                    sql = "UPDATE Pedido SET FechaEntrega = @Valor WHERE IdPedido = @IdPedido";
                    parametros = new[]
                    {
                        new SqlParameter("@Valor", string.IsNullOrEmpty(valorAnterior)
                            ? (object)DBNull.Value
                            : DateTime.ParseExact(valorAnterior, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)),
                        new SqlParameter("@IdPedido", idPedido)
                    };
                    break;

                case "MotivoCancelacion":
                    sql = "UPDATE Pedido SET MotivoCancelacion = @Valor WHERE IdPedido = @IdPedido";
                    parametros = new[]
                    {
                        new SqlParameter("@Valor", string.IsNullOrEmpty(valorAnterior)
                            ? (object)DBNull.Value : valorAnterior),
                        new SqlParameter("@IdPedido", idPedido)
                    };
                    break;

                case "Prendas":      // informativo — el estado de las prendas se reconcilia aparte
                case "FechaPedido":  // inmutable una vez creado el pedido
                    return;

                default:
                    throw new Exception($"Campo '{campo}' no es restaurable desde el historial.");
            }

            using (var cmd = new SqlCommand(sql, conexion, tx))
            {
                cmd.Parameters.AddRange(parametros);
                cmd.ExecuteNonQuery();
            }
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
            RecalcularDV();   // T07
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

            if (puedeReactivar) RecalcularDV();   // T07
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

        // ── T07 — DV MULTI-TABLA del Pedido ─────────────────────────────────────
        // El estado de un Pedido se compone de su fila MÁS sus líneas (PedidoPrenda).
        // El DVH incorpora un digest de las líneas: así, agregar / quitar / intercambiar
        // prendas del pedido por fuera del sistema cambia el DVH y se detecta.
        public const string DV_Tabla = "Pedido";

        public List<BE.FilaDV> ObtenerFilasDV()
        {
            var lista = new List<BE.FilaDV>();
            DataTable dt = acceso.Leer(
                "SELECT IdPedido, IdCliente, IdEmpleado, Estado, DVH FROM Pedido ORDER BY IdPedido", null);
            if (dt == null) return lista;
            foreach (DataRow row in dt.Rows)
            {
                int id = Convert.ToInt32(row["IdPedido"]);
                lista.Add(new BE.FilaDV
                {
                    Id = id,
                    Campos = new[]
                    {
                        id.ToString(),
                        row["IdCliente"].ToString(),
                        row["IdEmpleado"].ToString(),
                        row["Estado"].ToString(),
                        DigestLineas(id)
                    },
                    DVHAlmacenado = row["DVH"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["DVH"]),
                    Descripcion = "Pedido #" + id
                });
            }
            return lista;
        }

        // Huella de las líneas del pedido: IdPrenda concatenados y ordenados.
        private string DigestLineas(int idPedido)
        {
            DataTable dt = acceso.Leer(
                "SELECT IdPrenda FROM PedidoPrenda WHERE IdPedido = @id ORDER BY IdPrenda",
                new SqlParameter[] { new SqlParameter("@id", idPedido) });
            var ids = new List<string>();
            if (dt != null)
                foreach (DataRow r in dt.Rows) ids.Add(r["IdPrenda"].ToString());
            return string.Join(",", ids);
        }

        // Recalcula el DVH de cada Pedido (con sus líneas) y el DVV de la tabla.
        // Se invoca tras cada operación que modifica un pedido. No propaga errores
        // (la falla del DV no debe abortar la operación de negocio).
        public void RecalcularDV()
        {
            try
            {
                var svc   = Seguridad.CalculadorDV.Crear();
                var dvDAL = new DigitoVerificador();
                var dvhs  = new List<int>();
                foreach (var f in ObtenerFilasDV())
                {
                    int dvh = svc.CalcularDVH(f.Campos);
                    acceso.Escribir("UPDATE Pedido SET DVH=@dvh WHERE IdPedido=@id",
                        new SqlParameter[] { new SqlParameter("@dvh", dvh), new SqlParameter("@id", f.Id) });
                    dvhs.Add(dvh);
                }
                dvDAL.GuardarDVV(DV_Tabla, svc.CalcularDVV(dvhs));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[DAL.Pedido.RecalcularDV] " + ex.Message);
            }
        }
    }
}
