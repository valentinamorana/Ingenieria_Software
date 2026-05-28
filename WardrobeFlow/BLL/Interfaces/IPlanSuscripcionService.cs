using System.Collections.Generic;

namespace BLL.Interfaces
{
    /// <summary>
    /// Gestión de Planes de Suscripción.
    /// Define las operaciones que la capa de presentación puede invocar.
    /// </summary>
    public interface IPlanSuscripcionService
    {
        List<BE.PlanSuscripcion> ObtenerActivos();
        List<BE.PlanSuscripcion> ObtenerTodos();
        BE.PlanSuscripcion ObtenerPorId(int idPlan);
        void Alta(BE.PlanSuscripcion plan);
        void Modificar(BE.PlanSuscripcion plan);
        void Desactivar(int idPlan);
        void Activar(int idPlan);
    }
}
