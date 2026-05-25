using System.Collections.Generic;

namespace BE
{
    /// <summary>
    /// Patrón Composite — T04 Gestión de Perfiles de Usuario.
    /// Nodo compuesto: representa un grupo de permisos (ej: "Inventario", "Ventas").
    /// Puede contener tanto Familia como Patente como hijos.
    /// </summary>
    public class Familia : Componente
    {
        private readonly List<Componente> _hijos = new List<Componente>();

        public override IList<Componente> Hijos => _hijos.ToArray();

        public override void AgregarHijo(Componente c)
        {
            if (c != null && !_hijos.Contains(c))
                _hijos.Add(c);
        }

        public override void VaciarHijos()
        {
            _hijos.Clear();
        }
    }
}
