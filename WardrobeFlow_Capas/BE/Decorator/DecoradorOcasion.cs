namespace BE.Decorator
{
    // PATRON DECORATOR - Decorador concreto.
    // Agrega informacion de ocasion a la descripcion de la prenda.
    // Ejemplo: "Remera azul [Temporada: Verano] [Ocasion: Casual]"
    public class DecoradorOcasion : DecoradorPrenda
    {
        // Ocasion que se va a agregar a la descripcion
        private readonly string _ocasion;

        // Constructor: recibe el componente a decorar y la ocasion
        public DecoradorOcasion(IDescripcionPrenda prendaDecorada, string ocasion)
            : base(prendaDecorada)
        {
            _ocasion = ocasion;
        }

        // Agrega la ocasion a la descripcion del componente decorado
        public override string ObtenerDescripcion()
        {
            return _prendaDecorada.ObtenerDescripcion() + " [Ocasion: " + _ocasion + "]";
        }

        // Agrega la ocasion a la etiqueta del componente decorado
        public override string ObtenerEtiqueta()
        {
            return _prendaDecorada.ObtenerEtiqueta() + " | " + _ocasion;
        }
    }
}
