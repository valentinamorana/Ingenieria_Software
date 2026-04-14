using BE;
using BE.Composite;

namespace Seguridad
{
    // Clase estatica auxiliar para acceder rapidamente a la sesion actual.
    // Basada en Sesion.cs del ejemplo de referencia (Nacho/Codigo),
    // adaptada para WardrobeFlow. Delega todo a SessionManagerSL.
    // Uso en GUI: Sesion.ObtenerUsuarioActual(), Sesion.TieneSesionActiva(), etc.
    public static class Sesion
    {
        // Indica si hay una sesion activa en el sistema
        public static bool TieneSesionActiva()
        {
            return SessionManagerSL.Instancia.TieneSesionActiva();
        }

        // Devuelve el usuario actualmente logueado, o null si no hay sesion
        public static Usuario ObtenerUsuarioActual()
        {
            return SessionManagerSL.Instancia.ObtenerUsuarioActual();
        }

        // Verifica si el usuario actual tiene un permiso especifico del Composite
        public static bool IsInRole(TipoPermiso tipoPermiso)
        {
            return SessionManagerSL.Instancia.IsInRole(tipoPermiso);
        }

        // Devuelve el nombre del usuario actual para mostrar en la UI
        public static string ObtenerNombreUsuario()
        {
            var u = ObtenerUsuarioActual();
            if (u == null) return "[ Sesion no iniciada ]";
            return u.NombreCompleto + " [" + u.Rol + "]";
        }
    }
}
