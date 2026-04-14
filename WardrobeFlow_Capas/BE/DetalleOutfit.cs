namespace BE
{
    // Entidad que representa la relacion entre un Outfit y una Prenda.
    // Un Outfit esta compuesto por varios DetalleOutfit (cada uno referencia una prenda).
    public class DetalleOutfit : Entity
    {
        // Prenda que forma parte de este detalle del outfit
        public Prenda OPrenda { get; set; }

        // Para mostrar en listas y controles visuales
        public override string ToString()
        {
            // Si hay prenda asignada, muestra su nombre; de lo contrario "Sin prenda"
            return OPrenda != null ? OPrenda.Nombre : "Sin prenda";
        }
    }
}
