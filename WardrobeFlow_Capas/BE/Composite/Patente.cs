using System.Collections.Generic;

namespace BE.Composite
{
    // PATRON COMPOSITE — Nodo hoja (no puede tener hijos).
    // Una Patente representa un permiso individual concreto del sistema.
    // Ejemplo: "Puede gestionar prendas" con Tipo = GestorPrendas.
    // Tomado del proyecto de referencia, adaptado con TipoPermiso de WardrobeFlow.
    public class Patente : PermisoCompuesto
    {
        // Tipo de permiso especifico que representa esta hoja
        public TipoPermiso Tipo { get; set; }

        // Las hojas no pueden agregar hijos — metodo vacio intencional
        public override void AgregarPermiso(PermisoCompuesto p)
        {
            // Las patentes son hojas: no tienen hijos
        }

        // Las hojas siempre devuelven lista vacia
        public override IList<PermisoCompuesto> ObtenerHijos()
        {
            return new List<PermisoCompuesto>();
        }

        // Las hojas no pueden quitar hijos — metodo vacio intencional
        public override void QuitarPermiso(PermisoCompuesto p)
        {
            // Las patentes son hojas: no tienen hijos
        }
    }
}
