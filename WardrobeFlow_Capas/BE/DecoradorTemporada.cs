namespace BE
{
    // PATRON DECORATOR - Agrega temporada a la descripcion
    public class DecoradorTemporada : DecoradorPrenda
    {
        private string temporada;
        public DecoradorTemporada(IDescripcionPrenda p, string temporada) : base(p) { this.temporada = temporada; }
        public override string ObtenerDescripcion() { return prendaDecorada.ObtenerDescripcion() + " [" + temporada + "]"; }
        public override string ObtenerEtiqueta() { return prendaDecorada.ObtenerEtiqueta() + " | " + temporada; }
    }
}
