using System;
using System.Collections.Generic;

namespace BLL
{
    /// <summary>
    /// Capa de Lógica de Negocio — Gestión de Planes de Suscripción.
    /// Valida datos antes de persistir y centraliza reglas de negocio.
    /// </summary>
    public class PlanSuscripcion
    {
        private readonly DAL.PlanSuscripcion dalPlan = new DAL.PlanSuscripcion();

        // Devuelve todos los planes activos (para combos/selección).
        public List<BE.PlanSuscripcion> ObtenerActivos()
        {
            return dalPlan.ObtenerActivos();
        }

        // Devuelve todos los planes (activos e inactivos) para administración.
        public List<BE.PlanSuscripcion> ObtenerTodos()
        {
            return dalPlan.ObtenerTodos();
        }

        // Obtiene un plan por ID. Devuelve null si no existe.
        public BE.PlanSuscripcion ObtenerPorId(int idPlan)
        {
            return dalPlan.ObtenerPorId(idPlan);
        }

        // Crea un nuevo plan de suscripción.
        // Valida que nombre no esté vacío, límite > 0 y precio >= 0.
        public void Alta(BE.PlanSuscripcion plan)
        {
            Validar(plan);
            plan.Estado = true;
            dalPlan.Alta(plan);
        }

        // Modifica un plan existente.
        public void Modificar(BE.PlanSuscripcion plan)
        {
            Validar(plan);
            dalPlan.Modificar(plan);
        }

        // Desactiva (baja lógica) un plan.
        public void Desactivar(int idPlan)
        {
            dalPlan.Desactivar(idPlan);
        }

        // Reactiva un plan previamente desactivado.
        public void Activar(int idPlan)
        {
            dalPlan.Activar(idPlan);
        }

        // Validaciones
        private void Validar(BE.PlanSuscripcion plan)
        {
            if (plan == null)
                throw new ArgumentNullException(nameof(plan));

            if (string.IsNullOrWhiteSpace(plan.Nombre))
                throw new Exception("El nombre del plan es obligatorio.");

            if (plan.LimitePrendas <= 0)
                throw new Exception("El límite de prendas debe ser mayor que cero.");

            if (plan.Precio < 0)
                throw new Exception("El precio no puede ser negativo.");
        }
    }
}
