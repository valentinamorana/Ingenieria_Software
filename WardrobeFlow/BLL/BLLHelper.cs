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

        // Exige una sesión activa de ADMINISTRADOR (fail-closed). Centraliza el guard que antes
        // estaba duplicado en BLL.Usuario, BLL.Backup y BLL.RecuperacionAdmin. La clave/mensaje
        // permiten personalizar el error de "sin permiso" por módulo.
        internal static void ExigirAdministrador(string claveSinPermiso, string mensajeSinPermiso)
        {
            if (!Seguridad.SessionManager.IsLoggedIn)
                throw new BE.AppException("err.bll.sesion_expirada",
                    "La sesión expiró. Volvé a iniciar sesión.");
            if (!Seguridad.SessionManager.GetInstance().Usuario.EsAdministrador)
                throw new BE.AppException(claveSinPermiso, mensajeSinPermiso);
        }

        // Exige poder gestionar usuarios/permisos: Administrador (bypass) o un rol que tenga la
        // patente de Gestión de Usuarios (mnuUsuarios). Centraliza el guard duplicado en
        // BLL.Familia y BLL.ControlMapeado.
        internal static void ExigirGestionUsuarios()
        {
            if (!Seguridad.SessionManager.IsLoggedIn)
                throw new BE.AppException("err.bll.sesion_expirada",
                    "La sesión expiró. Volvé a iniciar sesión.");
            var u = Seguridad.SessionManager.GetInstance().Usuario;
            if (u.EsAdministrador) return;
            bool tieneGestion = u.Permisos != null && u.Permisos.Exists(p => p.NombreMenu == "mnuUsuarios");
            if (!tieneGestion)
                throw new BE.AppException("err.bll.familia.sin_permiso",
                    "No tenés permiso para gestionar usuarios y permisos.");
        }
    }
}
