using System;
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
            if (c == null) return;

            if (c == this)
                throw new InvalidOperationException(
                    $"Referencia circular directa: '{Nombre}' no puede ser hijo de sí mismo.");

            // Verifica que 'this' no sea ya un descendiente de 'c'.
            // Si lo fuera, agregar 'c' como hijo de 'this' crearía un ciclo.
            if (ContieneDescendiente(c, this))
                throw new InvalidOperationException(
                    $"Referencia circular: agregar '{c.Nombre}' como hijo de '{Nombre}' " +
                    $"crearía un ciclo porque '{Nombre}' ya es descendiente de '{c.Nombre}'.");

            if (!_hijos.Contains(c))
                _hijos.Add(c);
        }

        // Retorna true si 'buscado' aparece en el subárbol de 'nodo'.
        private static bool ContieneDescendiente(Componente nodo, Componente buscado)
        {
            foreach (var hijo in nodo.Hijos)
            {
                if (hijo == buscado) return true;
                if (ContieneDescendiente(hijo, buscado)) return true;
            }
            return false;
        }

        // Elimina un hijo específico del nodo compuesto.
        // No lanza si el hijo no existe — comportamiento silencioso como en Stach.
        public override void QuitarHijo(Componente c)
        {
            if (c != null)
                _hijos.Remove(c);
        }

        public override void VaciarHijos()
        {
            _hijos.Clear();
        }
    }
}
