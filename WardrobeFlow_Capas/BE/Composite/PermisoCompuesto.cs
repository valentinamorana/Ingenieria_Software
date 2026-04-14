using System.Collections.Generic;

namespace BE.Composite
{
    // PATRON COMPOSITE — Componente abstracto.
    // Define la interfaz comun para nodos hoja (Patente) y
    // nodos contenedor (Familia) del arbol de permisos.
    // Tomado directamente del proyecto de referencia.
    public abstract class PermisoCompuesto : Entity
    {
        // Nombre descriptivo del permiso o grupo
        public string Nombre { get; set; }

        // Agrega un permiso hijo (solo tiene efecto real en Familia)
        public abstract void AgregarPermiso(PermisoCompuesto p);

        // Quita un permiso hijo
        public abstract void QuitarPermiso(PermisoCompuesto p);

        // Devuelve la lista de hijos (vacia en Patente, con elementos en Familia)
        public abstract IList<PermisoCompuesto> ObtenerHijos();

        // Para mostrar el nombre en controles como ComboBox o TreeView
        public override string ToString()
        {
            return this.Nombre;
        }
    }
}
