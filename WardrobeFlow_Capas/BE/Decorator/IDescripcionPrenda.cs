namespace BE.Decorator
{
    // PATRON DECORATOR - Interfaz del componente.
    // Define las operaciones que tanto el componente base
    // como los decoradores deben implementar.
    public interface IDescripcionPrenda
    {
        // Devuelve la descripcion textual de la prenda
        string ObtenerDescripcion();

        // Devuelve una etiqueta resumida (color, talla, etc.)
        string ObtenerEtiqueta();
    }
}
