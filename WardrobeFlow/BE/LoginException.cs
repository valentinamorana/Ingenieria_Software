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

        public TipoError Tipo             { get; }
        public int?     IntentosRestantes { get; }

        public LoginException(TipoError tipo, string mensaje, int? intentosRestantes = null)
            : base(mensaje)
        {
            Tipo              = tipo;
            IntentosRestantes = intentosRestantes;
        }
    }
}
