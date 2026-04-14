namespace BE.Decorator
{
    // PATRON DECORATOR - Componente concreto base.
    // Envuelve una Prenda y expone su informacion basica.
    // Es el punto de partida para apilar decoradores.
    public class PrendaDescripcionBase : IDescripcionPrenda
    {
        // Prenda que se va a describir
        private readonly Prenda _prenda;

        // Constructor: recibe la prenda a decorar
        public PrendaDescripcionBase(Prenda prenda)
        {
            _prenda = prenda;
        }

        // Devuelve el nombre de la prenda como descripcion base
        public string ObtenerDescripcion()
        {
            return _prenda.Nombre;
        }

        // Devuelve color y talla como etiqueta base
        public string ObtenerEtiqueta()
        {
            return _prenda.Color + " | " + _prenda.Talla;
        }
    }
}
