using System.Collections.Generic;

namespace DAL.Interfaces
{
    /// <summary>Contrato del acceso a datos de los mapeos control↔permiso (Etapa 4).</summary>
    public interface IControlMapeadoDAL
    {
        // Todos los mapeos del sistema (los carga el ManejadorSeguridad para aplicar seguridad).
        List<BE.ControlMapeado> ObtenerTodos();

        // Mapeos de una patente puntual (para la UI de mapeo en Gestión de Permisos).
        List<BE.ControlMapeado> ObtenerPorPermiso(int idPermiso);

        // Reemplaza el conjunto de controles asociados a una patente (borra los previos e inserta los nuevos).
        void GuardarAsociados(int idPermiso, List<BE.ControlMapeado> controles);
    }
}
