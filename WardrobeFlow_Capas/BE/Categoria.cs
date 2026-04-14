namespace BE
{
    // Entidad que representa una categoria de prendas (ej: Camisas, Pantalones).
    // Extiende Entity para tener Id unico automatico.
    public class Categoria : Entity
    {
        // Nombre de la categoria
        public string Nombre { get; set; }

        // Descripcion opcional
        public string Descripcion { get; set; }

        // Estado activo o inactivo
        public bool Estado { get; set; } = true;

        // Para mostrar en DataGridView y ComboBox
        public override string ToString()
        {
            return Nombre;
        }
    }
}
