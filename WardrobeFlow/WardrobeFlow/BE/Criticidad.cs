namespace BE
{
    /// <summary>
    /// Capa de Entidades — Enumeración de Niveles de Criticidad para Bitácora.
    ///
    ///   None  (0): Eventos de sesión (login/logout)
    ///   Baja  (1): Consultas, navegación, lectura
    ///   Media (2): Modificaciones de datos
    ///   Alta  (3): Eliminaciones, acceso privilegiado
    ///   IntentosLogin (4): Intentos fallidos de login — posible bloqueo de cuenta
    ///   RecuperacionClave (5): Recuperación / forgot password
    /// </summary>
    public enum Criticidad
    {
        /// <summary>Sin criticidad. Usado en Login/Logout exitosos.</summary>
        None             = 0,

        /// <summary>Actividad de baja importancia (consultas, navegación).</summary>
        Baja             = 1,

        /// <summary>Modificación de datos (actualizaciones, ediciones).</summary>
        Media            = 2,

        /// <summary>Acción de alto impacto (eliminaciones, cambios de configuración).</summary>
        Alta             = 3,

        /// <summary>Intento fallido de login. Acumular puede derivar en bloqueo de cuenta.</summary>
        IntentosLogin    = 4,

        /// <summary>Recuperación de contraseña (forgot password / reset por admin).</summary>
        RecuperacionClave = 5,

        /// <summary>Cuenta bloqueada tras acumulación de intentos fallidos de login.</summary>
        BloqueosCuenta = 6
    }
}
