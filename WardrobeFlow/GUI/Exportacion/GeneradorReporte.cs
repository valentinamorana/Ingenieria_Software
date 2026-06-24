namespace GUI.Exportacion
{
    /// <summary>
    /// Patrón FACTORY METHOD — rol "Creator" (Creador abstracto).
    ///
    /// Declara el método fábrica <see cref="CrearExportador"/>, que recibe el formato
    /// pedido ("pdf" / "txt") y devuelve el <see cref="Exportador"/> concreto. Las
    /// SUBCLASES (un creador por reporte) deciden con qué origen se etiqueta el
    /// producto, igual que Logistica/Pizzeria en el ejercicio de la cátedra.
    ///
    /// El cliente (los formularios) usa esta abstracción y no instancia productos
    /// concretos: pide el exportador y lo usa polimórficamente.
    /// </summary>
    public abstract class GeneradorReporte
    {
        /// <summary>
        /// Método fábrica. Devuelve el exportador concreto para el formato dado,
        /// o null si el formato no se reconoce.
        /// </summary>
        public abstract Exportador CrearExportador(string formato);
    }
}
