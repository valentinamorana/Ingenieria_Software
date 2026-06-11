namespace BLL
{
    /// <summary>
    /// Helpers compartidos por todas las clases BLL.
    /// Evita duplicar la misma lógica de validación en cada clase de negocio.
    /// </summary>
    internal static class BLLHelper
    {
        // T04 — Re-validación de permisos en el BACKEND (fail-closed).
        // El Administrador siempre pasa. Sin sesión activa se rechaza la operación.
        // Centralizado acá para no repetir el mismo bloque en Pedido, Cliente, Prenda y PlanSuscripcion.
        internal static void ValidarPermiso(string nombrePatente)
        {
            if (!Seguridad.SessionManager.IsLoggedIn)
                throw new BE.AppException("err.bll.sesion_expirada",
                    "La sesión expiró. Volvé a iniciar sesión.");
            if (!Seguridad.SessionManager.GetInstance().TienePermiso(nombrePatente))
                throw new BE.AppException("err.bll.sin_permiso",
                    "No tiene permiso para ejecutar esta operación ('{0}').", nombrePatente);
        }
    }
}
