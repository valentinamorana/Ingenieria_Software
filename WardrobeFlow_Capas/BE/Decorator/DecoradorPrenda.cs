namespace BE.Decorator
{
    // PATRON DECORATOR - Decorador abstracto.
    // Contiene una referencia al componente decorado y delega las llamadas.
    // Las subclases (DecoradorTemporada, DecoradorOcasion) extienden el comportamiento.
    public abstract class DecoradorPrenda : IDescripcionPrenda
    {
        // Referencia al componente que se esta decorando (puede ser base u otro decorador)
        protected readonly IDescripcionPrenda _prendaDecorada;

        // Constructor: recibe el componente a envolver
        protected DecoradorPrenda(IDescripcionPrenda prendaDecorada)
        {
            _prendaDecorada = prendaDecorada;
        }

        // Por defecto delega al componente decorado (las subclases pueden sobreescribir)
        public virtual string ObtenerDescripcion()
        {
            return _prendaDecorada.ObtenerDescripcion();
        }

        // Por defecto delega al componente decorado
        public virtual string ObtenerEtiqueta()
        {
            return _prendaDecorada.ObtenerEtiqueta();
        }
    }
}
