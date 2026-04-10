namespace BE
{
    // PATRON DECORATOR - Agrega ocasion a la descripcion
    public class DecoradorOcasion : DecoradorPrenda
    {
        private string ocasion;
        public DecoradorOcasion(IDescripcionPrenda p, string ocasion) : base(p) { this.ocasion = ocasion; }
        public override string ObtenerDescripcion() { return prendaDecorada.ObtenerDescripcion() + " [" + ocasion + "]"; }
        public override string ObtenerEtiqueta() { return prendaDecorada.ObtenerEtiqueta() + " | " + ocasion; }
    }
}
