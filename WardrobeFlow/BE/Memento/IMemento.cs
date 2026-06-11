using System;

namespace BE.Memento
{
    /// <summary>
    /// Patrón MEMENTO — rol "Memento".
    ///
    /// Cápsula que guarda una "foto" del estado interno de un Originator.
    /// El estado concreto es opaco para el Caretaker: este solo accede a los
    /// METADATOS (Fecha, Actor, Detalle) para listar e identificar las versiones,
    /// pero NO conoce ni manipula el estado encapsulado (eso solo lo hace el
    /// Originator que lo creó y lo restaura).
    /// </summary>
    public interface IMemento
    {
        /// <summary>Momento en que se capturó el estado.</summary>
        DateTime Fecha { get; }

        /// <summary>Usuario que originó la captura del estado.</summary>
        string Actor { get; }

        /// <summary>Descripción legible de por qué/cuándo se tomó el snapshot.</summary>
        string Detalle { get; }
    }
}
