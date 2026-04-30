using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    /// <summary>
    /// Capa de Acceso a Datos — Prenda.
    /// Opera sobre la tabla [Prenda] de WardrobeFlowDB.
    /// </summary>
    /// <summary>
    /// Hereda de <see cref="BaseDAL{T}"/>:
    ///   - acceso  → Singleton de BD (heredado, no se redeclara)
    ///   - ObtenerTodos() y ObtenerPorId() → implementados con SQL de Prenda
    /// </summary>
    public class Prenda : BaseDAL<BE.Prenda>
    {

        // Devuelve todas las prendas con nombre del cliente si están en uso. 
        public override List<BE.Prenda> ObtenerTodos()
        {
            var lista = new List<BE.Prenda>();
            try
            {
                DataTable tabla = acceso.Leer(
                    "SELECT p.IdPrenda, p.Nombre, p.Descripcion, p.Talle, p.Color, " +
                    "       p.Categoria, p.Estado, p.IdClienteActual, p.FechaAlta, " +
                    "       c.Nombre + ' ' + c.Apellido AS NombreCliente " +
                    "FROM Prenda p " +
                    "LEFT JOIN Cliente c ON c.IdCliente = p.IdClienteActual " +
                    "ORDER BY p.Categoria, p.Nombre",
                    null);

                foreach (DataRow row in tabla.Rows)
                    lista.Add(Mapear(row));
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener la lista de prendas.", ex);
            }
            return lista;
        }

        // Devuelve solo las prendas con estado Disponible.
        public List<BE.Prenda> ObtenerDisponibles()
        {
            var lista = new List<BE.Prenda>();
            try
            {
                DataTable tabla = acceso.Leer(
                    "SELECT p.IdPrenda, p.Nombre, p.Descripcion, p.Talle, p.Color, " +
                    "       p.Categoria, p.Estado, p.IdClienteActual, p.FechaAlta, " +
                    "       NULL AS NombreCliente " +
                    "FROM Prenda p " +
                    "WHERE p.Estado = 0 " +  // EstadoPrenda.Disponible = 0
                    "ORDER BY p.Categoria, p.Nombre",
                    null);

                foreach (DataRow row in tabla.Rows)
                    lista.Add(Mapear(row));
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener prendas disponibles.", ex);
            }
            return lista;
        }

        // Obtiene una prenda por ID.
        public override BE.Prenda ObtenerPorId(int idPrenda)
        {
            SqlParameter[] p = { new SqlParameter("@IdPrenda", idPrenda) };
            try
            {
                DataTable tabla = acceso.Leer(
                    "SELECT p.IdPrenda, p.Nombre, p.Descripcion, p.Talle, p.Color, " +
                    "       p.Categoria, p.Estado, p.IdClienteActual, p.FechaAlta, " +
                    "       c.Nombre + ' ' + c.Apellido AS NombreCliente " +
                    "FROM Prenda p " +
                    "LEFT JOIN Cliente c ON c.IdCliente = p.IdClienteActual " +
                    "WHERE p.IdPrenda = @IdPrenda",
                    p);

                if (tabla == null || tabla.Rows.Count == 0) return null;
                return Mapear(tabla.Rows[0]);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener la prenda.", ex);
            }
        }

        // Devuelve las prendas actualmente asignadas a un cliente.
        public List<BE.Prenda> ObtenerPorCliente(int idCliente)
        {
            SqlParameter[] p = { new SqlParameter("@IdCliente", idCliente) };
            var lista = new List<BE.Prenda>();
            try
            {
                DataTable tabla = acceso.Leer(
                    "SELECT p.IdPrenda, p.Nombre, p.Descripcion, p.Talle, p.Color, " +
                    "       p.Categoria, p.Estado, p.IdClienteActual, p.FechaAlta, " +
                    "       c.Nombre + ' ' + c.Apellido AS NombreCliente " +
                    "FROM Prenda p " +
                    "LEFT JOIN Cliente c ON c.IdCliente = p.IdClienteActual " +
                    "WHERE p.IdClienteActual = @IdCliente AND p.Estado = 1",  // EnUso = 1
                    p);

                foreach (DataRow row in tabla.Rows)
                    lista.Add(Mapear(row));
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener prendas del cliente.", ex);
            }
            return lista;
        }

        // Inserta una nueva prenda. Devuelve el ID generado.
        public int Alta(BE.Prenda prenda)
        {
            SqlParameter[] p =
            {
                new SqlParameter("@Nombre", prenda.Nombre),
                new SqlParameter("@Descripcion", (object)prenda.Descripcion ?? DBNull.Value),
                new SqlParameter("@Talle", (object)prenda.Talle ?? DBNull.Value),
                new SqlParameter("@Color", (object)prenda.Color ?? DBNull.Value),
                new SqlParameter("@Categoria", (object)prenda.Categoria ?? DBNull.Value),
                new SqlParameter("@Estado", (int)prenda.Estado),
                new SqlParameter("@FechaAlta", prenda.FechaAlta)
            };

            DataTable tabla = acceso.Leer(
                "INSERT INTO Prenda (Nombre, Descripcion, Talle, Color, Categoria, Estado, FechaAlta) " +
                "VALUES (@Nombre, @Descripcion, @Talle, @Color, @Categoria, @Estado, @FechaAlta); " +
                "SELECT SCOPE_IDENTITY() AS IdNuevo",
                p);

            return tabla != null && tabla.Rows.Count > 0
                ? Convert.ToInt32(tabla.Rows[0]["IdNuevo"])
                : 0;
        }

        // Actualiza datos descriptivos de una prenda.
        public void Modificar(BE.Prenda prenda)
        {
            SqlParameter[] p =
            {
                new SqlParameter("@Nombre", prenda.Nombre),
                new SqlParameter("@Descripcion", (object)prenda.Descripcion ?? DBNull.Value),
                new SqlParameter("@Talle", (object)prenda.Talle ?? DBNull.Value),
                new SqlParameter("@Color", (object)prenda.Color ?? DBNull.Value),
                new SqlParameter("@Categoria", (object)prenda.Categoria ?? DBNull.Value),
                new SqlParameter("@IdPrenda", prenda.IdPrenda)
            };
            acceso.Escribir(
                "UPDATE Prenda SET Nombre=@Nombre, Descripcion=@Descripcion, " +
                "Talle=@Talle, Color=@Color, Categoria=@Categoria " +
                "WHERE IdPrenda=@IdPrenda",
                p);
        }

        // Cambia el estado de una prenda (disponible, en uso, limpieza, baja).
        public void CambiarEstado(int idPrenda, BE.EstadoPrenda nuevoEstado, int? idClienteActual = null)
        {
            SqlParameter[] p =
            {
                new SqlParameter("@Estado", (int)nuevoEstado),
                new SqlParameter("@IdClienteActual", (object)idClienteActual ?? DBNull.Value),
                new SqlParameter("@IdPrenda", idPrenda)
            };
            acceso.Escribir(
                "UPDATE Prenda SET Estado=@Estado, IdClienteActual=@IdClienteActual " +
                "WHERE IdPrenda=@IdPrenda",
                p);
        }

        private BE.Prenda Mapear(DataRow row)
        {
            return new BE.Prenda
            {
                IdPrenda = Convert.ToInt32(row["IdPrenda"]),
                Nombre = row["Nombre"].ToString(),
                Descripcion = row["Descripcion"] != DBNull.Value ? row["Descripcion"].ToString() : null,
                Talle = row["Talle"]  != DBNull.Value ? row["Talle"].ToString() : null,
                Color = row["Color"]  != DBNull.Value ? row["Color"].ToString() : null,
                Categoria = row["Categoria"] != DBNull.Value ? row["Categoria"].ToString() : null,
                Estado = (BE.EstadoPrenda)Convert.ToInt32(row["Estado"]),
                IdClienteActual = row["IdClienteActual"] != DBNull.Value ? (int?)Convert.ToInt32(row["IdClienteActual"]) : null,
                NombreCliente = row["NombreCliente"] != DBNull.Value ? row["NombreCliente"].ToString() : null,
                FechaAlta = Convert.ToDateTime(row["FechaAlta"])
            };
        }
    }
}
