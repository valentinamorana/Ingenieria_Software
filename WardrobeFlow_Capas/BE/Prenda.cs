using System;

namespace BE
{
    // Entidad que representa una prenda del guardarropa.
    // Extiende Entity para tener Id unico automatico.
    public class Prenda : Entity
    {
        // Nombre de la prenda (ej: "Remera azul manga corta")
        public string Nombre { get; set; }

        // Color de la prenda
        public string Color { get; set; }

        // Talla (ej: S, M, L, XL)
        public string Talla { get; set; }

        // Temporada a la que pertenece (Verano, Invierno, etc.)
        public string Temporada { get; set; }

        // Categoria a la que pertenece esta prenda
        public Categoria OCategoria { get; set; }

        // Estado activo o inactivo
        public bool Estado { get; set; } = true;

        // Fecha en que se registro la prenda en el sistema
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        // Para mostrar en DataGridView y CheckedListBox
        public override string ToString()
        {
            return Nombre;
        }
    }
}
