using System;

namespace BE
{
    /// <summary>
    /// Excepción de dominio que lleva una clave de traducción.
    /// La GUI la detecta en FormBase.MostrarError(Exception) y traduce el mensaje
    /// al idioma activo antes de mostrarlo al usuario.
    /// </summary>
    public class AppException : Exception
    {
        public string   Clave { get; }
        public object[] Args  { get; }

        public AppException(string clave, string mensajeFallback, params object[] args)
            : base(args != null && args.Length > 0
                   ? string.Format(mensajeFallback, args)
                   : mensajeFallback)
        {
            Clave = clave;
            Args  = args;
        }
    }
}
