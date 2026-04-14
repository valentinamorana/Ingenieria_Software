namespace BE.Decorator
{
    // PATRON DECORATOR - Decorador concreto.
    // Agrega informacion de temporada a la descripcion de la prenda.
    // Ejemplo: "Remera azul [Temporada: Verano]"
    public class DecoradorTemporada : DecoradorPrenda
    {
        // Temporada que se va a agregar a la descripcion
        private readonly string _temporada;

        // Constructor: recibe el componente a decorar y la temporada
        public DecoradorTemporada(IDescripcionPrenda prendaDecorada, string temporada)
            : base(prendaDecorada)
        {
            _temporada = temporada;
        }

        // Agrega la temporada a la descripcion del componente decorado
        public override string ObtenerDescripcion()
        {
            return _prendaDecorada.ObtenerDescripcion() + " [Temporada: " + _temporada + "]";
        }

        // Agrega la temporada a la etiqueta del componente decorado
        public override string ObtenerEtiqueta()
        {
            return _prendaDecorada.ObtenerEtiqueta() + " | " + _temporada;
        }
    }
}
