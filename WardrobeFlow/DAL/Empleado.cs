using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    /// <summary>
    /// Capa de Acceso a Datos — Empleado.
    /// Opera sobre la tabla [Empleado] de WardrobeFlowDB.
    /// Incluye JOIN opcional con [Usuario] para mostrar el username asociado.
    /// </summary>
    public class Empleado
    {
        private readonly Acceso acceso = Acceso.GetInstance();

        // Devuelve todos los empleados con su username (si tienen usuario).
        public List<BE.Empleado> ObtenerTodos()
        {
            var lista = new List<BE.Empleado>();
            try
            {
                DataTable tabla = acceso.Leer(
                    "SELECT e.IdEmpleado, e.Nombre, e.Apellido, e.DNI, e.Email, " +
                    "       e.FechaIngreso, e.Puesto, e.Legajo, e.IdUsuario, " +
                    "       u.Username " +
                    "FROM Empleado e " +
                    "LEFT JOIN Usuario u ON u.IdUsuario = e.IdUsuario " +
                    "ORDER BY e.Apellido, e.Nombre",
                    null);

                foreach (DataRow row in tabla.Rows)
                    lista.Add(Mapear(row));
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener la lista de empleados.", ex);
            }
            return lista;
        }

        // Obtiene un empleado por ID.
        public BE.Empleado ObtenerPorId(int idEmpleado)
        {
            SqlParameter[] p = { new SqlParameter("@IdEmpleado", idEmpleado) };
            try
            {
                DataTable tabla = acceso.Leer(
                    "SELECT e.IdEmpleado, e.Nombre, e.Apellido, e.DNI, e.Email, " +
                    "       e.FechaIngreso, e.Puesto, e.Legajo, e.IdUsuario, " +
                    "       u.Username " +
                    "FROM Empleado e " +
                    "LEFT JOIN Usuario u ON u.IdUsuario = e.IdUsuario " +
                    "WHERE e.IdEmpleado = @IdEmpleado",
                    p);

                if (tabla == null || tabla.Rows.Count == 0) return null;
                return Mapear(tabla.Rows[0]);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener el empleado.", ex);
            }
        }

        // Obtiene el empleado vinculado a un usuario del sistema.
        public BE.Empleado ObtenerPorUsuario(int idUsuario)
        {
            SqlParameter[] p = { new SqlParameter("@IdUsuario", idUsuario) };
            try
            {
                DataTable tabla = acceso.Leer(
                    "SELECT e.IdEmpleado, e.Nombre, e.Apellido, e.DNI, e.Email, " +
                    "       e.FechaIngreso, e.Puesto, e.Legajo, e.IdUsuario, " +
                    "       u.Username " +
                    "FROM Empleado e " +
                    "LEFT JOIN Usuario u ON u.IdUsuario = e.IdUsuario " +
                    "WHERE e.IdUsuario = @IdUsuario",
                    p);

                if (tabla == null || tabla.Rows.Count == 0) return null;
                return Mapear(tabla.Rows[0]);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener el empleado por usuario.", ex);
            }
        }

        // Verifica si ya existe un empleado con ese DNI.
        public bool ExisteDNI(string dni)
        {
            SqlParameter[] p = { new SqlParameter("@DNI", dni) };
            DataTable tabla = acceso.Leer(
                "SELECT IdEmpleado FROM Empleado WHERE DNI = @DNI", p);
            return tabla != null && tabla.Rows.Count > 0;
        }

        // Inserta un nuevo empleado. Devuelve el ID generado.
        public int Alta(BE.Empleado empleado)
        {
            SqlParameter[] p =
            {
                new SqlParameter("@Nombre", empleado.Nombre),
                new SqlParameter("@Apellido", empleado.Apellido),
                new SqlParameter("@DNI", empleado.DNI),
                new SqlParameter("@Email", (object)empleado.Email ?? DBNull.Value),
                new SqlParameter("@FechaIngreso", empleado.FechaIngreso),
                new SqlParameter("@Puesto", (object)empleado.Puesto ?? DBNull.Value),
                new SqlParameter("@Legajo", (object)empleado.Legajo ?? DBNull.Value),
                new SqlParameter("@IdUsuario", (object)empleado.IdUsuario ?? DBNull.Value)
            };

            DataTable tabla = acceso.Leer(
                "INSERT INTO Empleado (Nombre, Apellido, DNI, Email, FechaIngreso, Puesto, Legajo, IdUsuario) " +
                "VALUES (@Nombre, @Apellido, @DNI, @Email, @FechaIngreso, @Puesto, @Legajo, @IdUsuario); " +
                "SELECT SCOPE_IDENTITY() AS IdNuevo",
                p);

            return tabla != null && tabla.Rows.Count > 0
                ? Convert.ToInt32(tabla.Rows[0]["IdNuevo"])
                : 0;
        }

        // Actualiza los datos de un empleado existente.
        public void Modificar(BE.Empleado empleado)
        {
            SqlParameter[] p =
            {
                new SqlParameter("@Nombre", empleado.Nombre),
                new SqlParameter("@Apellido", empleado.Apellido),
                new SqlParameter("@DNI", empleado.DNI),
                new SqlParameter("@Email", (object)empleado.Email ?? DBNull.Value),
                new SqlParameter("@FechaIngreso", empleado.FechaIngreso),
                new SqlParameter("@Puesto", (object)empleado.Puesto ?? DBNull.Value),
                new SqlParameter("@Legajo", (object)empleado.Legajo ?? DBNull.Value),
                new SqlParameter("@IdUsuario", (object)empleado.IdUsuario ?? DBNull.Value),
                new SqlParameter("@IdEmpleado", empleado.IdEmpleado)
            };
            acceso.Escribir(
                "UPDATE Empleado SET Nombre=@Nombre, Apellido=@Apellido, DNI=@DNI, " +
                "Email=@Email, FechaIngreso=@FechaIngreso, Puesto=@Puesto, Legajo=@Legajo, " +
                "IdUsuario=@IdUsuario WHERE IdEmpleado=@IdEmpleado",
                p);
        }

        private BE.Empleado Mapear(DataRow row)
        {
            return new BE.Empleado
            {
                IdEmpleado = Convert.ToInt32(row["IdEmpleado"]),
                Nombre = row["Nombre"].ToString(),
                Apellido = row["Apellido"].ToString(),
                DNI = row["DNI"].ToString(),
                Email = row["Email"] != DBNull.Value ? row["Email"].ToString() : null,
                FechaIngreso = Convert.ToDateTime(row["FechaIngreso"]),
                Puesto = row["Puesto"] != DBNull.Value ? row["Puesto"].ToString() : null,
                Legajo = row["Legajo"] != DBNull.Value ? row["Legajo"].ToString() : null,
                IdUsuario = row["IdUsuario"] != DBNull.Value ? (int?)Convert.ToInt32(row["IdUsuario"]) : null,
                Username = row["Username"] != DBNull.Value ? row["Username"].ToString() : null
            };
        }
    }
}
