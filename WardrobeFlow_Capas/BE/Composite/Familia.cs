using System.Collections.Generic;
using System.Linq;

namespace BE.Composite
{
    // PATRON COMPOSITE — Nodo contenedor (rama del arbol).
    // Una Familia puede contener Patentes u otras Familias.
    // Ejemplo: "Administradores" contiene "Gestores de prendas" y "Gestores de outfits".
    // Tomado del proyecto de referencia sin modificaciones.
    public class Familia : PermisoCompuesto
    {
        // Lista interna de permisos hijos
        private IList<PermisoCompuesto> _hijos;

        public Familia()
        {
            _hijos = new List<PermisoCompuesto>();
        }

        // Agrega un hijo si no estaba ya incluido (evita duplicados)
        public override void AgregarPermiso(PermisoCompuesto p)
        {
            if (!_hijos.Contains(p))
                _hijos.Add(p);
        }

        // Devuelve un array de los hijos (copia para no exponer la lista interna)
        public override IList<PermisoCompuesto> ObtenerHijos()
        {
            return _hijos.ToArray();
        }

        // Quita un hijo si existe en la lista
        public override void QuitarPermiso(PermisoCompuesto p)
        {
            if (_hijos.Contains(p))
                _hijos.Remove(p);
        }
    }
}
