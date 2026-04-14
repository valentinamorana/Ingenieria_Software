using System;
using System.Collections.Generic;

namespace BE
{
    // Entidad que representa un conjunto/outfit armado por el usuario.
    // Contiene una lista de DetalleOutfit (las prendas que lo componen).
    public class Outfit : Entity
    {
        // Lista de prendas que conforman el outfit
        private IList<DetalleOutfit> _detalles;

        public Outfit()
        {
            _detalles = new List<DetalleOutfit>();
        }

        // Nombre del outfit (ej: "Look casual verano")
        public string Nombre { get; set; }

        // Descripcion general del conjunto
        public string Descripcion { get; set; }

        // Ocasion para la que sirve (Casual, Formal, Deportivo, Fiesta, Trabajo)
        public string Ocasion { get; set; }

        // Temporada recomendada (Verano, Invierno, etc.)
        public string Temporada { get; set; }

        // Estado activo o inactivo
        public bool Estado { get; set; } = true;

        // Fecha en que se creo el outfit
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Usuario propietario del outfit
        public Usuario OUsuario { get; set; }

        // Lista de prendas del outfit (solo lectura desde afuera)
        public IList<DetalleOutfit> Detalles
        {
            get { return _detalles; }
        }

        // Para mostrar en DataGridView
        public override string ToString()
        {
            return Nombre;
        }
    }
}
