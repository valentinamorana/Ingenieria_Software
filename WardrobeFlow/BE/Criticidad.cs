namespace BE
{
    /// <summary>
    /// Capa de Entidades — Enumeración de Niveles de Criticidad para Bitácora.
    ///   None  (0): Eventos de sesión (login/logout)
    ///   Baja  (1): Consultas, navegación, lectura
    ///   Media (2): Modificaciones de datos
    ///   Alta  (3): Eliminaciones, acceso privilegiado
    ///   IntentosLogin (4): Intentos fallidos de login — posible bloqueo de cuenta
    ///   RecuperacionClave (5): Recuperación / forgot password
    /// </summary>
    public enum Criticidad
    {
        None = 0,

        Baja = 1,

        Media = 2,

        Alta = 3,

        IntentosLogin = 4,

        RecuperacionClave = 5,

        BloqueosCuenta = 6
    }
}
