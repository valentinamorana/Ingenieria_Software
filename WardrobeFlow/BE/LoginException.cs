using System;

namespace BE
{
    public class LoginException : Exception
    {
        public enum TipoError
        {
            CamposVacios,
            LimiteAlcanzado,
            CuentaBloqueada,
            CredencialesInvalidas
        }

        public TipoError Tipo { get; }

        public LoginException(TipoError tipo, string mensaje) : base(mensaje)
        {
            Tipo = tipo;
        }
    }
}
