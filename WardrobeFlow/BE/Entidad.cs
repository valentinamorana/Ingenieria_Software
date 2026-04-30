using System;

namespace BE
{
    /// <summary>
    /// Clase abstracta base para todas las entidades del dominio.
    /// </summary>
    public abstract class Entidad
    {
        public DateTime FechaAlta { get; set; }

        // Retorna el identificador único de la entidad.
        public abstract int GetId();
    }
}
