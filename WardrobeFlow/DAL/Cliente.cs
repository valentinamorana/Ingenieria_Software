using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    /// <summary>Acceso a datos de la tabla [Cliente].</summary>
    public class Cliente : BaseDAL<BE.Cliente>, Interfaces.IClienteDAL
    {
        // T07 — Definición del Dígito Verificador de esta tabla (fuente única, usada
        // tanto para recalcular tras escrituras como para verificar al arrancar).
        public const  string   DV_Tabla    = "Cliente";
        public const  string   DV_Pk       = "IdCliente";
        public static readonly string[] DV_Columnas = { "Nombre", "Apellido", "DNI", "Email", "MetodoPago" };

        // Recalcula DVH de cada fila + DVV de la tabla. Se llama tras Alta/Modificar/Baja.
        // No propaga errores: la falla del DV no debe abortar la operación de negocio
        // (la verificación de integridad al arranque la detectaría igual).
        private void RecalcularDV()
        {
            try { new DigitoVerificador().RecalcularTabla(DV_Tabla, DV_Pk, DV_Columnas); }
            catch (Exception ex) { System.Diagnostics.Trace.TraceError("[DAL.Cliente.RecalcularDV] " + ex.Message); }
        }

        // Devuelve todos los clientes activos con el nombre de su plan (JOIN).
        public override List<BE.Cliente> ObtenerTodos()
        {
            var lista = new List<BE.Cliente>();
            try
            {
                SqlParameter[] p = { new SqlParameter("@EstadoEnUso", (int)BE.EstadoPrenda.EnUso) };
                DataTable tabla = acceso.Leer(
                    "SELECT c.IdCliente, c.Nombre, c.Apellido, c.DNI, c.Email, " +
                    "       c.MetodoPago, c.IdPlan, c.FechaAlta, c.FechaVencimiento, c.FechaNacimiento, " +
                    "       p.Nombre AS NombrePlan, " +
                    "       ISNULL(p.LimitePrendas, 0) AS LimitePrendas, " +
                    "       ISNULL(stock.StockUtilizado, 0) AS StockUtilizado " +
                    "FROM Cliente c " +
                    "LEFT JOIN PlanSuscripcion p ON p.IdPlan = c.IdPlan " +
                    "LEFT JOIN ( " +
                    "    SELECT IdClienteActual, COUNT(*) AS StockUtilizado " +
                    "    FROM Prenda " +
                    "    WHERE Estado = @EstadoEnUso " +
                    "    GROUP BY IdClienteActual " +
                    ") stock ON stock.IdClienteActual = c.IdCliente " +
                    "WHERE c.Activo = 1 " +
                    "ORDER BY c.Apellido, c.Nombre",
                    p);

                foreach (DataRow row in tabla.Rows)
                    lista.Add(Mapear(row));
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener la lista de clientes.", ex);
            }
            return lista;
        }

        // Obtiene un cliente por ID con plan y stock actual.
        public override BE.Cliente ObtenerPorId(int idCliente)
        {
            SqlParameter[] p =
            {
                new SqlParameter("@IdCliente",   idCliente),
                new SqlParameter("@EstadoEnUso", (int)BE.EstadoPrenda.EnUso)
            };
            try
            {
                DataTable tabla = acceso.Leer(
                    "SELECT c.IdCliente, c.Nombre, c.Apellido, c.DNI, c.Email, " +
                    "       c.MetodoPago, c.IdPlan, c.FechaAlta, c.FechaVencimiento, c.FechaNacimiento, " +
                    "       p.Nombre AS NombrePlan, " +
                    "       ISNULL(p.LimitePrendas, 0) AS LimitePrendas, " +
                    "       (SELECT COUNT(*) FROM Prenda pr WHERE pr.IdClienteActual = c.IdCliente " +
                    "        AND pr.Estado = @EstadoEnUso) AS StockUtilizado " +
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

        // Devuelve la cantidad de clientes activos asignados al plan indicado.
        public int ContarClientesActivosPorPlan(int idPlan)
        {
            var dt = acceso.Leer(
                "SELECT COUNT(*) AS Total FROM Cliente WHERE IdPlan = @IdPlan AND Activo = 1",
                new[] { new SqlParameter("@IdPlan", idPlan) });
            return dt != null && dt.Rows.Count > 0 ? Convert.ToInt32(dt.Rows[0]["Total"]) : 0;
        }

        // Verifica si ya existe un cliente activo con ese DNI.
        public bool ExisteDNI(string dni)
        {
            return BuscarIdPorDni(dni, -1) != 0;
        }

        // Verifica si existe otro cliente activo con ese DNI (excluyendo el ID indicado).
        // Usado al modificar para detectar colisiones.
        public bool ExisteDNIParaOtro(string dni, int idExcluir)
        {
            return BuscarIdPorDni(dni, idExcluir) != 0;
        }

        // T03 — El DNI se almacena CIFRADO (AES con IV aleatorio), por lo que no se puede
        // comparar por igualdad en SQL. Se trae el DNI de cada cliente activo, se descifra
        // (TryDesencriptar tolera registros legacy en texto plano) y se compara en memoria.
        private int BuscarIdPorDni(string dni, int idExcluir)
        {
            DataTable tabla = acceso.Leer(
                "SELECT IdCliente, DNI FROM Cliente WHERE Activo = 1", null);
            if (tabla == null) return 0;
            foreach (DataRow row in tabla.Rows)
            {
                int id = Convert.ToInt32(row["IdCliente"]);
                if (id == idExcluir) continue;
                string dniGuardado = Seguridad.Encriptador.TryDesencriptar(row["DNI"].ToString());
                if (string.Equals(dniGuardado, dni, StringComparison.Ordinal)) return id;
            }
            return 0;
        }

        // Inserta un nuevo cliente. Devuelve el ID generado.
        public int Alta(BE.Cliente cliente)
        {
            SqlParameter[] p =
            {
                new SqlParameter("@Nombre",            cliente.Nombre),
                new SqlParameter("@Apellido",          cliente.Apellido),
                new SqlParameter("@DNI",               Seguridad.Encriptador.Encriptar(cliente.DNI)),
                new SqlParameter("@Email",             (object)cliente.Email ?? DBNull.Value),
                new SqlParameter("@MetodoPago",        cliente.MetodoPago),
                new SqlParameter("@IdPlan",            (object)cliente.IdPlan ?? DBNull.Value),
                new SqlParameter("@FechaAlta",         cliente.FechaAlta),
                new SqlParameter("@FechaVencimiento",  (object)cliente.FechaVencimiento  ?? DBNull.Value),
                new SqlParameter("@FechaNacimiento",   (object)cliente.FechaNacimiento   ?? DBNull.Value)
            };

            DataTable tabla = acceso.Leer(
                "INSERT INTO Cliente (Nombre, Apellido, DNI, Email, MetodoPago, IdPlan, FechaAlta, FechaVencimiento, FechaNacimiento) " +
                "VALUES (@Nombre, @Apellido, @DNI, @Email, @MetodoPago, @IdPlan, @FechaAlta, @FechaVencimiento, @FechaNacimiento); " +
                "SELECT SCOPE_IDENTITY() AS IdNuevo",
                p);

            int idNuevo = tabla != null && tabla.Rows.Count > 0
                ? Convert.ToInt32(tabla.Rows[0]["IdNuevo"])
                : 0;
            RecalcularDV();   // T07
            return idNuevo;
        }

        // Actualiza los datos de un cliente existente.
        public void Modificar(BE.Cliente cliente)
        {
            SqlParameter[] p =
            {
                new SqlParameter("@Nombre",           cliente.Nombre),
                new SqlParameter("@Apellido",         cliente.Apellido),
                new SqlParameter("@DNI",              Seguridad.Encriptador.Encriptar(cliente.DNI)),
                new SqlParameter("@Email",            (object)cliente.Email ?? DBNull.Value),
                new SqlParameter("@MetodoPago",       cliente.MetodoPago),
                new SqlParameter("@IdPlan",           (object)cliente.IdPlan ?? DBNull.Value),
                new SqlParameter("@FechaVencimiento", (object)cliente.FechaVencimiento ?? DBNull.Value),
                new SqlParameter("@FechaNacimiento",  (object)cliente.FechaNacimiento  ?? DBNull.Value),
                new SqlParameter("@IdCliente",        cliente.IdCliente)
            };
            acceso.Escribir(
                "UPDATE Cliente SET Nombre=@Nombre, Apellido=@Apellido, DNI=@DNI, " +
                "Email=@Email, MetodoPago=@MetodoPago, IdPlan=@IdPlan, " +
                "FechaVencimiento=@FechaVencimiento, FechaNacimiento=@FechaNacimiento " +
                "WHERE IdCliente=@IdCliente",
                p);
            RecalcularDV();   // T07
        }

        // Baja lógica del cliente (Activo=0).
        public void Baja(int idCliente)
        {
            SqlParameter[] p = { new SqlParameter("@IdCliente", idCliente) };
            acceso.Escribir(
                "UPDATE Cliente SET Activo = 0 WHERE IdCliente = @IdCliente", p);
            RecalcularDV();   // T07
        }

        private BE.Cliente Mapear(DataRow row)
        {
            return new BE.Cliente
            {
                IdCliente      = Convert.ToInt32(row["IdCliente"]),
                Nombre         = row["Nombre"].ToString(),
                Apellido       = row["Apellido"].ToString(),
                DNI            = Seguridad.Encriptador.TryDesencriptar(row["DNI"].ToString()),
                Email          = row["Email"] != DBNull.Value ? row["Email"].ToString() : null,
                MetodoPago     = row["MetodoPago"].ToString(),
                IdPlan         = row["IdPlan"] != DBNull.Value ? (int?)Convert.ToInt32(row["IdPlan"]) : null,
                NombrePlan     = row["NombrePlan"] != DBNull.Value ? row["NombrePlan"].ToString() : null,
                LimitePrendas  = row.Table.Columns.Contains("LimitePrendas")
                                    ? Convert.ToInt32(row["LimitePrendas"])
                                    : 0,
                FechaAlta        = Convert.ToDateTime(row["FechaAlta"]),
                FechaVencimiento = row.Table.Columns.Contains("FechaVencimiento") && row["FechaVencimiento"] != DBNull.Value
                                      ? (DateTime?)Convert.ToDateTime(row["FechaVencimiento"])
                                      : null,
                FechaNacimiento  = row.Table.Columns.Contains("FechaNacimiento") && row["FechaNacimiento"] != DBNull.Value
                                      ? (DateTime?)Convert.ToDateTime(row["FechaNacimiento"])
                                      : null,
                StockUtilizado = Convert.ToInt32(row["StockUtilizado"])
            };
        }
    }
}
