using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    /// <summary>
    /// Capa de Acceso a Datos — Cliente.
    /// Opera sobre la tabla [Cliente] de WardrobeFlowDB.
    /// Los clientes son suscriptores del servicio, NO son usuarios del sistema.
    /// </summary>
    public class Cliente
    {
        private readonly Acceso acceso = Acceso.GetInstance();

        /// <summary>Devuelve todos los clientes activos con el nombre de su plan (JOIN).</summary>
        public List<BE.Cliente> ObtenerTodos()
        {
            var lista = new List<BE.Cliente>();
            try
            {
                DataTable tabla = acceso.Leer(
                    "SELECT c.IdCliente, c.Nombre, c.Apellido, c.DNI, c.Email, " +
                    "       c.MetodoPago, c.IdPlan, c.FechaAlta, " +
                    "       p.Nombre AS NombrePlan, " +
                    "       ISNULL(p.LimitePrendas, 0) AS LimitePrendas, " +
                    "       (SELECT COUNT(*) FROM Prenda pr WHERE pr.IdClienteActual = c.IdCliente " +
                    "        AND pr.Estado = 1) AS StockUtilizado " +
                    "FROM Cliente c " +
                    "LEFT JOIN PlanSuscripcion p ON p.IdPlan = c.IdPlan " +
                    "WHERE c.Activo = 1 " +
                    "ORDER BY c.Apellido, c.Nombre",
                    null);

                foreach (DataRow row in tabla.Rows)
                    lista.Add(Mapear(row));
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener la lista de clientes.", ex);
            }
            return lista;
        }

        /// <summary>Obtiene un cliente por ID con plan y stock actual.</summary>
        public BE.Cliente ObtenerPorId(int idCliente)
        {
            SqlParameter[] p = { new SqlParameter("@IdCliente", idCliente) };
            try
            {
                DataTable tabla = acceso.Leer(
                    "SELECT c.IdCliente, c.Nombre, c.Apellido, c.DNI, c.Email, " +
                    "       c.MetodoPago, c.IdPlan, c.FechaAlta, " +
                    "       p.Nombre AS NombrePlan, " +
                    "       ISNULL(p.LimitePrendas, 0) AS LimitePrendas, " +
                    "       (SELECT COUNT(*) FROM Prenda pr WHERE pr.IdClienteActual = c.IdCliente " +
                    "        AND pr.Estado = 1) AS StockUtilizado " +
                    "FROM Cliente c " +
                    "LEFT JOIN PlanSuscripcion p ON p.IdPlan = c.IdPlan " +
                    "WHERE c.IdCliente = @IdCliente AND c.Activo = 1",
                    p);

                if (tabla == null || tabla.Rows.Count == 0) return null;
                return Mapear(tabla.Rows[0]);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener el cliente.", ex);
            }
        }

        /// <summary>Verifica si ya existe un cliente activo con ese DNI.</summary>
        public bool ExisteDNI(string dni)
        {
            SqlParameter[] p = { new SqlParameter("@DNI", dni) };
            DataTable tabla = acceso.Leer(
                "SELECT IdCliente FROM Cliente WHERE DNI = @DNI AND Activo = 1", p);
            return tabla != null && tabla.Rows.Count > 0;
        }

        /// <summary>Inserta un nuevo cliente. Devuelve el ID generado.</summary>
        public int Alta(BE.Cliente cliente)
        {
            SqlParameter[] p =
            {
                new SqlParameter("@Nombre",     cliente.Nombre),
                new SqlParameter("@Apellido",   cliente.Apellido),
                new SqlParameter("@DNI",        cliente.DNI),
                new SqlParameter("@Email",      (object)cliente.Email      ?? DBNull.Value),
                new SqlParameter("@MetodoPago", cliente.MetodoPago),
                new SqlParameter("@IdPlan",     (object)cliente.IdPlan     ?? DBNull.Value),
                new SqlParameter("@FechaAlta",  cliente.FechaAlta)
            };

            DataTable tabla = acceso.Leer(
                "INSERT INTO Cliente (Nombre, Apellido, DNI, Email, MetodoPago, IdPlan, FechaAlta) " +
                "VALUES (@Nombre, @Apellido, @DNI, @Email, @MetodoPago, @IdPlan, @FechaAlta); " +
                "SELECT SCOPE_IDENTITY() AS IdNuevo",
                p);

            return tabla != null && tabla.Rows.Count > 0
                ? Convert.ToInt32(tabla.Rows[0]["IdNuevo"])
                : 0;
        }

        /// <summary>Actualiza los datos de un cliente existente.</summary>
        public void Modificar(BE.Cliente cliente)
        {
            SqlParameter[] p =
            {
                new SqlParameter("@Nombre",     cliente.Nombre),
                new SqlParameter("@Apellido",   cliente.Apellido),
                new SqlParameter("@DNI",        cliente.DNI),
                new SqlParameter("@Email",      (object)cliente.Email  ?? DBNull.Value),
                new SqlParameter("@MetodoPago", cliente.MetodoPago),
                new SqlParameter("@IdPlan",     (object)cliente.IdPlan ?? DBNull.Value),
                new SqlParameter("@IdCliente",  cliente.IdCliente)
            };
            acceso.Escribir(
                "UPDATE Cliente SET Nombre=@Nombre, Apellido=@Apellido, DNI=@DNI, " +
                "Email=@Email, MetodoPago=@MetodoPago, IdPlan=@IdPlan " +
                "WHERE IdCliente=@IdCliente",
                p);
        }

        /// <summary>
        /// Baja lógica de un cliente (soft delete — Activo=0).
        /// No elimina el registro físicamente para preservar la integridad referencial
        /// con Pedido y BitacoraNegocio.
        /// </summary>
        public void Baja(int idCliente)
        {
            SqlParameter[] p = { new SqlParameter("@IdCliente", idCliente) };
            acceso.Escribir(
                "UPDATE Cliente SET Activo = 0 WHERE IdCliente = @IdCliente", p);
        }

        // ── Mapeo privado ────────────────────────────────────────────────────

        private BE.Cliente Mapear(DataRow row)
        {
            return new BE.Cliente
            {
                IdCliente      = Convert.ToInt32(row["IdCliente"]),
                Nombre         = row["Nombre"].ToString(),
                Apellido       = row["Apellido"].ToString(),
                DNI            = row["DNI"].ToString(),
                Email          = row["Email"] != DBNull.Value ? row["Email"].ToString() : null,
                MetodoPago     = row["MetodoPago"].ToString(),
                IdPlan         = row["IdPlan"] != DBNull.Value ? (int?)Convert.ToInt32(row["IdPlan"]) : null,
                NombrePlan     = row["NombrePlan"] != DBNull.Value ? row["NombrePlan"].ToString() : null,
                LimitePrendas  = row.Table.Columns.Contains("LimitePrendas")
                                    ? Convert.ToInt32(row["LimitePrendas"])
                                    : 0,
                FechaAlta      = Convert.ToDateTime(row["FechaAlta"]),
                StockUtilizado = Convert.ToInt32(row["StockUtilizado"])
            };
        }
    }
}
