using System;

namespace BE
{
    /// <summary>
    /// Snapshot del estado de un Usuario en un instante dado.
    /// Persiste en la tabla HistorialUsuario.
    /// Se guarda automáticamente antes de cada operación destructiva
    /// (reset de clave, desbloqueo) para permitir reversión.
    /// </summary>
    public class VersionUsuario
    {
        public int      Id               { get; set; }
        public int      IdUsuario        { get; set; }
        public DateTime Fecha            { get; set; }
        public string   Actor            { get; set; }
        public string   Detalle          { get; set; }
        public string   UsernameSnapshot { get; set; }
        public string   ClaveSnapshot    { get; set; }
        public bool     EstadoSnapshot   { get; set; } // true = activo, false = bloqueado
        public int      IntentosSnapshot { get; set; }
    }
}
