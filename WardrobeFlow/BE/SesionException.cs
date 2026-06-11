namespace BE
{
    /// <summary>
    /// Error del ciclo de vida de la sesión: no iniciada, ya iniciada, o sin sesión activa.
    /// Hereda de <see cref="AppException"/> para integrarse con la traducción de la GUI
    /// (FormBase.MostrarError la detecta como AppException y la traduce por su Clave),
    /// y a la vez permite capturarla de forma específica (catch BE.SesionException).
    /// Reemplaza los 'throw new Exception(...)' genéricos del SessionManager (RNF-04).
    /// </summary>
    public class SesionException : AppException
    {
        public SesionException(string clave, string mensajeFallback, params object[] args)
            : base(clave, mensajeFallback, args) { }
    }
}
