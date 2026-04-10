namespace BE
{
    // PATRON DECORATOR - Decorador abstracto
    public abstract class DecoradorPrenda : IDescripcionPrenda
    {
        protected IDescripcionPrenda prendaDecorada;
        public DecoradorPrenda(IDescripcionPrenda prendaDecorada) { this.prendaDecorada = prendaDecorada; }
        public virtual string ObtenerDescripcion() { return prendaDecorada.ObtenerDescripcion(); }
        public virtual string ObtenerEtiqueta() { return prendaDecorada.ObtenerEtiqueta(); }
    }
}
