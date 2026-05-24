namespace Seguridad
{
    /// <summary>
    /// Singleton thread-safe que cuenta intentos fallidos de login en la sesión actual.
    /// Primera línea de defensa en memoria: evita consultas a BD cuando el límite ya se alcanzó.
    /// Complementa el contador persistente de DAL.Usuario (que bloquea la cuenta en BD).
    /// </summary>
    public sealed class ContadorSesion
    {
        private static volatile ContadorSesion _instance;
        private static readonly object _lock = new object();

        private int _intentosFallidos = 0;
        private const int MaxIntentos = 3;

        private ContadorSesion() { }

        public static ContadorSesion GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new ContadorSesion();
                }
            }
            return _instance;
        }

        public bool LimiteAlcanzado => _intentosFallidos >= MaxIntentos;

        public void RegistrarIntento()
        {
            _intentosFallidos++;
        }

        public void Resetear()
        {
            _intentosFallidos = 0;
        }
    }
}
