namespace BE
{
    // PATRON DECORATOR - Componente concreto base
    public class PrendaDescripcionBase : IDescripcionPrenda
    {
        private Prenda prenda;
        public PrendaDescripcionBase(Prenda prenda) { this.prenda = prenda; }
        public string ObtenerDescripcion() { return prenda.Nombre; }
        public string ObtenerEtiqueta() { return prenda.Color + " | " + prenda.Talla; }
    }
}
