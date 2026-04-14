using System.Collections.Generic;
using BE.Composite;

namespace BE
{
    // Entidad Usuario del sistema WardrobeFlow.
    // Extiende Entity (tiene Guid Id automatico).
    // Basado en el Usuario del proyecto de referencia, adaptado para WardrobeFlow.
    public class Usuario : Entity
    {
        // Lista de permisos asignados al usuario (puede ser Patente o Familia)
        private IList<PermisoCompuesto> _permisos;

        public Usuario()
        {
            _permisos = new List<PermisoCompuesto>();
        }

        // Nombre completo del usuario
        public string NombreCompleto { get; set; }

        // Documento de identidad (usado como nombre de usuario para el login)
        public string Documento { get; set; }

        // Correo electronico
        public string Correo { get; set; }

        // Clave encriptada con MD5 (misma tecnica que el proyecto de referencia)
        public string Password { get; set; }

        // Rol del usuario: "Administrador", "Empleado", "Usuario"
        public string Rol { get; set; }

        // Estado activo/inactivo
        public bool Estado { get; set; } = true;

        // Lista de permisos (solo lectura desde afuera, se modifica via metodos BLL)
        public IList<PermisoCompuesto> Permisos
        {
            get { return _permisos; }
        }

        // Para mostrar en ComboBox y otros controles
        public override string ToString()
        {
            return NombreCompleto;
        }
    }
}
