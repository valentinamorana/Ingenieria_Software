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

        /// <summary>Devuelve todos los planes activos (para combos/selección).</summary>
        public List<BE.PlanSuscripcion> ObtenerActivos()
        {
            return dalPlan.ObtenerActivos();
        }

        /// <summary>Devuelve todos los planes (activos e inactivos) para administración.</summary>
        public List<BE.PlanSuscripcion> ObtenerTodos()
        {
            return dalPlan.ObtenerTodos();
        }

        /// <summary>Obtiene un plan por ID. Devuelve null si no existe.</summary>
        public BE.PlanSuscripcion ObtenerPorId(int idPlan)
        {
            return dalPlan.ObtenerPorId(idPlan);
        }

        /// <summary>
        /// Crea un nuevo plan de suscripción.
        /// Valida que nombre no esté vacío, límite > 0 y precio >= 0.
        /// </summary>
        public void Alta(BE.PlanSuscripcion plan)
        {
            Validar(plan);
            plan.Estado = true;
            dalPlan.Alta(plan);
        }

        /// <summary>
        /// Modifica un plan existente.
        /// </summary>
        public void Modificar(BE.PlanSuscripcion plan)
        {
            Validar(plan);
            dalPlan.Modificar(plan);
        }

        /// <summary>
        /// Desactiva (baja lógica) un plan.
        /// </summary>
        public void Desactivar(int idPlan)
        {
            dalPlan.Desactivar(idPlan);
        }

        // ── Validaciones ─────────────────────────────────────────────────────

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
