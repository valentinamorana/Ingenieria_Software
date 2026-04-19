namespace BE
{
    /// <summary>
    /// Capa de Entidades — Enumeración de Niveles de Criticidad.
    /// Define los posibles niveles de importancia para los registros de la Bitácora.
    /// Se usa para clasificar qué tan relevante o grave es una actividad registrada.
    ///
    /// Uso en Bitácora:
    ///   - None  (0): Sin criticidad definida (eventos de sistema como login/logout)
    ///   - Baja  (1): Consultas, navegación, lectura de datos
    ///   - Media (2): Modificaciones, actualizaciones de datos
    ///   - Alta  (3): Eliminaciones, cambios de configuración, accesos privilegiados
    /// </summary>
    public enum Criticidad
    {
        /// <summary>Sin nivel de criticidad asignado. Usado en eventos de sesión (login/logout).</summary>
        None  = 0,

        /// <summary>Actividad de baja importancia (e.g., consultas, navegación).</summary>
        Baja  = 1,

        /// <summary>Actividad de impacto moderado (e.g., modificación de registros).</summary>
        Media = 2,

        /// <summary>Actividad crítica o de alto impacto (e.g., eliminaciones, acceso privilegiado).</summary>
        Alta  = 3
    }
}
