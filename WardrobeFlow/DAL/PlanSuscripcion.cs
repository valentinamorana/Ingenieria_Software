using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    /// <summary>
    /// Capa de Acceso a Datos — PlanSuscripcion.
    /// Opera sobre la tabla [PlanSuscripcion] de WardrobeFlowDB.
    /// </summary>
    public class PlanSuscripcion
    {
        private readonly Acceso acceso = Acceso.GetInstance();

        // Devuelve todos los planes activos. 
        public List<BE.PlanSuscripcion> ObtenerActivos()
        {
            var lista = new List<BE.PlanSuscripcion>();
            try
            {
                DataTable tabla = acceso.Leer(
                    "SELECT IdPlan, Nombre, LimitePrendas, Precio, Estado " +
                    "FROM PlanSuscripcion WHERE Estado = 1 ORDER BY Precio",
                    null);

                foreach (DataRow row in tabla.Rows)
                    lista.Add(Mapear(row));
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener planes de suscripción.", ex);
            }
            return lista;
        }

        // Devuelve todos los planes (activos e inactivos) — para administración.
        public List<BE.PlanSuscripcion> ObtenerTodos()
        {
            var lista = new List<BE.PlanSuscripcion>();
            try
            {
                DataTable tabla = acceso.Leer(
                    "SELECT IdPlan, Nombre, LimitePrendas, Precio, Estado " +
                    "FROM PlanSuscripcion ORDER BY Precio",
                    null);

                foreach (DataRow row in tabla.Rows)
                    lista.Add(Mapear(row));
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener planes de suscripción.", ex);
            }
            return lista;
        }

        // Obtiene un plan por su ID. Devuelve null si no existe.
        public BE.PlanSuscripcion ObtenerPorId(int idPlan)
        {
            SqlParameter[] p = { new SqlParameter("@IdPlan", idPlan) };
            try
            {
                DataTable tabla = acceso.Leer(
                    "SELECT IdPlan, Nombre, LimitePrendas, Precio, Estado " +
                    "FROM PlanSuscripcion WHERE IdPlan = @IdPlan",
                    p);

                if (tabla == null || tabla.Rows.Count == 0) return null;
                return Mapear(tabla.Rows[0]);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener el plan de suscripción.", ex);
            }
        }

        // Inserta un nuevo plan.
        public void Alta(BE.PlanSuscripcion plan)
        {
            SqlParameter[] p =
            {
                new SqlParameter("@Nombre",        plan.Nombre),
                new SqlParameter("@LimitePrendas", plan.LimitePrendas),
                new SqlParameter("@Precio",        plan.Precio),
                new SqlParameter("@Estado",        plan.Estado ? 1 : 0)
            };
            acceso.Escribir(
                "INSERT INTO PlanSuscripcion (Nombre, LimitePrendas, Precio, Estado) " +
                "VALUES (@Nombre, @LimitePrendas, @Precio, @Estado)",
                p);
        }

        // Actualiza datos de un plan existente.
        public void Modificar(BE.PlanSuscripcion plan)
        {
            SqlParameter[] p =
            {
                new SqlParameter("@Nombre",        plan.Nombre),
                new SqlParameter("@LimitePrendas", plan.LimitePrendas),
                new SqlParameter("@Precio",        plan.Precio),
                new SqlParameter("@Estado",        plan.Estado ? 1 : 0),
                new SqlParameter("@IdPlan",        plan.IdPlan)
            };
            acceso.Escribir(
                "UPDATE PlanSuscripcion SET Nombre=@Nombre, LimitePrendas=@LimitePrendas, " +
                "Precio=@Precio, Estado=@Estado WHERE IdPlan=@IdPlan",
                p);
        }

        // Desactiva un plan (baja lógica).
        public void Desactivar(int idPlan)
        {
            SqlParameter[] p = { new SqlParameter("@IdPlan", idPlan) };
            acceso.Escribir(
                "UPDATE PlanSuscripcion SET Estado=0 WHERE IdPlan=@IdPlan", p);
        }

        // Reactiva un plan previamente desactivado.
        public void Activar(int idPlan)
        {
            SqlParameter[] p = { new SqlParameter("@IdPlan", idPlan) };
            acceso.Escribir(
                "UPDATE PlanSuscripcion SET Estado=1 WHERE IdPlan=@IdPlan", p);
        }

        private BE.PlanSuscripcion Mapear(DataRow row)
        {
            return new BE.PlanSuscripcion
            {
                IdPlan        = Convert.ToInt32(row["IdPlan"]),
                Nombre        = row["Nombre"].ToString(),
                LimitePrendas = Convert.ToInt32(row["LimitePrendas"]),
                Precio        = Convert.ToDecimal(row["Precio"]),
                Estado        = Convert.ToBoolean(row["Estado"])
            };
        }
    }
}
