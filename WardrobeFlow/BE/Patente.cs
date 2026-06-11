using System;
using System.Collections.Generic;

namespace BE
{
    /// <summary>
    /// Patrón Composite — T04 Gestión de Perfiles de Usuario.
    /// Nodo hoja: representa un permiso individual (ej: "Ver Usuarios").
    /// No puede tener hijos. Indica si el permiso está asignado al rol activo.
    /// </summary>
    public class Patente : Componente
    {
        // Nombre del elemento de menú en la GUI (eg: "mnuUsuarios")
        public string NombreMenu { get; set; }

        // true si el permiso está asignado al rol seleccionado
        public bool Asignado { get; set; }

        // Hoja — nunca tiene hijos
        public override IList<Componente> Hijos => new List<Componente>(0);

        public override void AgregarHijo(Componente c)
        {
            throw new InvalidOperationException(
                "Un permiso simple (Patente) no puede contener hijos. Solo las Familias admiten hijos.");
        }

        // Hoja — no tiene hijos que quitar. Equivalente a Quitar() de Stach en la clase Patente.
        public override void QuitarHijo(Componente c)
        {
            throw new InvalidOperationException(
                "Un permiso simple (Patente) no tiene hijos que quitar.");
        }

        public override void VaciarHijos() { }

        // Caso BASE de la recursión: una hoja se aporta a sí misma (sin duplicar).
        // Equivalente a CalcularPrecioBase() de ArticuloSimple, que devuelve su valor propio.
        internal override void RecolectarPatentes(IDictionary<int, Patente> acumulador, HashSet<int> visitados)
        {
            if (!acumulador.ContainsKey(Id)) acumulador[Id] = this;
        }

        public override string ObtenerDescripcion(int nivel = 0)
        {
            string sangria = new string(' ', nivel * 2);
            string menu    = string.IsNullOrEmpty(NombreMenu) ? string.Empty : $" ({NombreMenu})";
            return $"{sangria}- [Patente] {Nombre}{menu}";
        }
    }
}
