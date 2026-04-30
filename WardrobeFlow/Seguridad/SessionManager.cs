using System;
using BE;

namespace Seguridad
{
    /// <summary>Singleton que gestiona la sesión del usuario autenticado.</summary>
    public class SessionManager
    {
        private static object _lock    = new object();
        private static SessionManager _session;

        // Usuario actualmente en sesión y fecha/hora de inicio. 
        public Usuario Usuario    { get; set; }

        // Marca de tiempo del momento en que se inició la sesión.
        public DateTime FechaInicio { get; set; }

        // Retorna la sesión activa. Lanza excepción si no hay sesión iniciada.
        public static SessionManager GetInstance
        {
            get
            {
                if (_session == null)
                    throw new Exception("Sesión no iniciada. Debe hacer Login primero.");

                return _session;
            }
        }

        // Indica si hay una sesión activa sin lanzar excepción.
        public static bool IsLoggedIn => _session != null;

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
                    throw new Exception("Sesión ya iniciada.");
                }
            }
        }

        // Destruye la sesión activa.
        public static void Logout()
        {
            lock (_lock)
            {
                if (_session != null)
                {
                    _session = null;
                }
                else
                {
                    throw new Exception("No hay sesión activa para cerrar.");
                }
            }
        }

        private SessionManager() { }
    }
}
