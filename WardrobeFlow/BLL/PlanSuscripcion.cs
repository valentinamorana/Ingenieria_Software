using System;
using System.Collections.Generic;

namespace BLL
{
    /// <summary>
    /// Capa de Lógica de Negocio — Gestión de Planes de Suscripción.
    /// Valida datos antes de persistir y centraliza reglas de negocio.
    /// </summary>
    public class PlanSuscripcion : Interfaces.IPlanSuscripcionService
    {
        private readonly DAL.PlanSuscripcion dalPlan   = new DAL.PlanSuscripcion();
        private readonly DAL.Cliente         dalCliente = new DAL.Cliente();

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
            PermisosAccion.Exigir(BE.Patentes.PlanSuscripcionesEditar, BE.Patentes.PlanSuscripciones);
            Validar(plan);
            plan.Estado = true;
            dalPlan.Alta(plan);
        }

        // Modifica un plan existente.
        public void Modificar(BE.PlanSuscripcion plan)
        {
            PermisosAccion.Exigir(BE.Patentes.PlanSuscripcionesEditar, BE.Patentes.PlanSuscripciones);
            Validar(plan);
            dalPlan.Modificar(plan);
        }

        // Desactiva (baja lógica) un plan.
        // Falla si hay clientes activos asignados a ese plan.
        public void Desactivar(int idPlan)
        {
            PermisosAccion.Exigir(BE.Patentes.PlanSuscripcionesEditar, BE.Patentes.PlanSuscripciones);
            int clientesActivos = dalCliente.ContarClientesActivosPorPlan(idPlan);
            if (clientesActivos > 0)
                throw new BE.AppException("err.bll.plan.tiene_clientes",
                    "No se puede desactivar el plan: tiene {0} cliente(s) activo(s) asignado(s). " +
                    "Reasignalos a otro plan antes de desactivarlo.",
                    clientesActivos);

            dalPlan.Desactivar(idPlan);
        }

        // Reactiva un plan previamente desactivado.
        public void Activar(int idPlan)
        {
            PermisosAccion.Exigir(BE.Patentes.PlanSuscripcionesEditar, BE.Patentes.PlanSuscripciones);
            dalPlan.Activar(idPlan);
        }

        // Validaciones
        private void Validar(BE.PlanSuscripcion plan)
        {
            if (plan == null)
                throw new ArgumentNullException(nameof(plan));

            if (string.IsNullOrWhiteSpace(plan.Nombre))
                throw new BE.AppException("err.bll.plan.nombre_requerido",
                    "El nombre del plan es obligatorio.");

            if (plan.LimitePrendas <= 0)
                throw new BE.AppException("err.bll.plan.limite_invalido",
                    "El límite de prendas debe ser mayor que cero.");

            foreach (char c in plan.Nombre)
                if (!char.IsLetter(c) && c != ' ')
                    throw new BE.AppException("err.bll.plan.nombre_letras",
                        "El nombre del plan solo puede contener letras y espacios.");

            if (plan.Precio <= 0)
                throw new BE.AppException("err.bll.plan.precio_cero",
                    "El precio debe ser mayor a cero.");
        }
    }
}
