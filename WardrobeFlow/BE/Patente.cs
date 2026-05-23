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

        public override void AgregarHijo(Componente c) { }

        public override void VaciarHijos() { }
    }
}
