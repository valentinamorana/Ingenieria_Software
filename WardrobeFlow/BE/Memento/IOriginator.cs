namespace BE.Memento
{
    /// <summary>
    /// Patrón MEMENTO — rol "Originator".
    ///
    /// Objeto cuyo estado puede guardarse y restaurarse. Es el ÚNICO que conoce
    /// la estructura interna del Memento: lo crea capturando su estado actual y
    /// lo usa para volver a un estado anterior.
    /// </summary>
    public interface IOriginator
    {
        /// <summary>Captura el estado actual del objeto en un Memento.</summary>
        IMemento CrearMemento(string actor, string detalle);

        /// <summary>Restaura el estado del objeto a partir de un Memento previo.</summary>
        void RestaurarDesde(IMemento memento);
    }
}
