using System;
using BE;

namespace Seguridad
{
    /// <summary>
    /// Módulo de Seguridad — Gestor de Sesión (Patrón Singleton).
    ///
    /// Implementado según el patrón de referencia de la cátedra.
    ///
    /// ESTRUCTURA DEL PATRÓN (según diagrama de clase del profesor):
    ///   - _session       : instancia única privada (equivale a -instance: SessionManager)
    ///   - _lock          : objeto para thread-safety
    ///   - GetInstance    : propiedad estática pública que retorna la sesión activa
    ///                      (lanza excepción si no hay sesión iniciada)
    ///   - Login(usuario) : método estático que crea la sesión (lanza si ya existe)
    ///   - Logout()       : método estático que destruye la sesión (lanza si no existe)
    ///   - new()          : constructor privado (impide instanciación externa)
    ///
    /// DIFERENCIA CLAVE con otros Singleton:
    ///   Aquí GetInstance NO crea la instancia — solo la retorna.
    ///   La creación es exclusiva de Login(). Esto modela que no puede haber
    ///   "acceso a sesión" sin haber hecho Login primero.
    ///
    /// USO CORRECTO:
    ///   SessionManager.Login(usuario);          // Crea la sesión
    ///   var u = SessionManager.GetInstance.Usuario;  // Accede a la sesión activa
    ///   SessionManager.Logout();                // Destruye la sesión
    /// </summary>
    public class SessionManager
    {
        // Objeto de sincronización para garantizar thread-safety en Login/Logout
        private static object _lock = new object();

        // Instancia única de la sesión — null cuando no hay usuario logueado
        private static SessionManager _session;

        /// <summary>Usuario actualmente en sesión y fecha/hora de inicio.</summary>
        public Usuario Usuario    { get; set; }

        /// <summary>Marca de tiempo del momento en que se inició la sesión.</summary>
        public DateTime FechaInicio { get; set; }

        /// <summary>
        /// Propiedad estática que retorna la sesión activa.
        /// LANZA excepción si se accede sin haber hecho Login previamente.
        /// No crear la instancia aquí es intencional: fuerza el uso de Login().
        /// </summary>
        public static SessionManager GetInstance
        {
            get
            {
                if (_session == null)
                    throw new Exception("Sesión no iniciada. Debe hacer Login primero.");

                return _session;
            }
        }

        /// <summary>
        /// Indica si hay una sesión activa sin lanzar excepción.
        /// Usar antes de GetInstance para evitar excepciones al verificar estado.
        /// </summary>
        public static bool IsLoggedIn => _session != null;

        /// <summary>
        /// Inicia una nueva sesión para el usuario dado.
        /// Thread-safe: usa lock para evitar condiciones de carrera.
        /// LANZA excepción si ya existe una sesión activa.
        /// </summary>
        /// <param name="usuario">Usuario autenticado por BLL.</param>
        /// <exception cref="Exception">Si ya hay una sesión iniciada.</exception>
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

        /// <summary>
        /// Cierra la sesión activa destruyendo la instancia Singleton.
        /// Thread-safe: usa lock para evitar condiciones de carrera.
        /// LANZA excepción si no hay sesión activa para cerrar.
        /// </summary>
        /// <exception cref="Exception">Si no hay sesión activa.</exception>
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

        /// <summary>
        /// Constructor privado: impide la creación de instancias desde fuera.
        /// Solo Login() puede crear el SessionManager.
        /// </summary>
        private SessionManager() { }
    }
}
