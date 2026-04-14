using System;

namespace BE
{
    // Clase base abstracta para todas las entidades del sistema.
    // Basada en el patron del proyecto de referencia: cada entidad
    // tiene un identificador unico global (Guid) generado automaticamente.
    public abstract class Entity
    {
        // Constructor: genera un nuevo Guid al crear cualquier entidad
        public Entity()
        {
            Id = Guid.NewGuid();
        }

        // Identificador unico de solo lectura (no se puede cambiar despues de creado)
        public Guid Id { get; }
    }
}
