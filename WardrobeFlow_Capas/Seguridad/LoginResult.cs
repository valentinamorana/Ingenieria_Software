namespace Seguridad
{
    // Enumeracion con los posibles resultados de un intento de login.
    // Tomada del proyecto de referencia sin modificaciones.
    public enum LoginResult
    {
        // El documento ingresado no existe en el sistema
        InvalidUsername,

        // El documento existe pero la clave es incorrecta
        InvalidPassword,

        // Credenciales correctas: sesion iniciada exitosamente
        ValidUser
    }
}
