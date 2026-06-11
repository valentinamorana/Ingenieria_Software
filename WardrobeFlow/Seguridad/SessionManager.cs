using System;
using BE;

namespace Seguridad
{
    /// <summary>Singleton que gestiona la sesión del usuario autenticado.</summary>
    public sealed class SessionManager
    {
        private static object _lock    = new object();
        private static SessionManager _session;

        // Usuario actualmente en sesión y fecha/hora de inicio. 
        public Usuario Usuario    { get; set; }

        // Marca de tiempo del momento en que se inició la sesión.
        public DateTime FechaInicio { get; set; }

        // Retorna la sesión activa. Lanza SesionException (traducible) si no hay sesión iniciada.
        public static SessionManager GetInstance()
        {
            if (_session == null)
                throw new BE.SesionException("err.seg.sesion_no_iniciada",
                    "La sesión no está iniciada. Iniciá sesión primero.");

            return _session;
        }

        // Indica si hay una sesión activa sin lanzar excepción.
        public static bool IsLoggedIn => _session != null;

        // T04 — Re-validación de permisos en el BACKEND.
        // Devuelve true si el usuario en sesión posee la patente indicada.
        // El Administrador tiene acceso total. Si no hay usuario, no tiene permiso.
        // Captura _session en variable local para evitar race condition con Logout().
        public bool TienePermiso(string nombreMenu)
        {
            var s = _session;
            if (s?.Usuario == null) return false;
            if (s.Usuario.EsAdministrador) return true;
            return s.Usuario.Permisos != null &&
                   s.Usuario.Permisos.Exists(p => p.NombreMenu == nombreMenu);
        }

        // Crea la sesión para el usuario autenticado.
        public static void Login(Usuario usuario)
        {
            lock (_lock)
            {
                if (_session == null)
                {
                    _session             = new SessionManager();
                    _session.Usuario     = usuario;
                    _session.FechaInicio = DateTime.Now;
                }
                else
                {
                    throw new BE.SesionException("err.seg.sesion_ya_iniciada",
                        "Ya hay una sesión iniciada. Cerrá la sesión actual antes de iniciar otra.");
                }
            }
        }

        // Destruye la sesión activa. Idempotente: si ya no hay sesión, no hace nada.
        public static void Logout()
        {
            lock (_lock) { _session = null; }
        }

        private SessionManager() { }
    }
}
