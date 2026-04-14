using System;

namespace Seguridad
{
    // Excepcion especifica del proceso de login.
    // Lleva el resultado del intento para que la GUI muestre el mensaje correcto.
    // Tomada del proyecto de referencia sin modificaciones.
    public class LoginException : Exception
    {
        // Resultado del login (InvalidUsername o InvalidPassword)
        public LoginResult Result;

        // Constructor: recibe el resultado que origino la excepcion
        public LoginException(LoginResult result)
        {
            Result = result;
        }
    }
}
