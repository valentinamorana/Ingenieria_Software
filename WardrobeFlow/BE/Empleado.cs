using System;

namespace BE
{
    /// <summary>
    /// Entidad — Empleado.
    /// Datos personales del empleado, vinculado opcionalmente a un Usuario del sistema.
    /// Mapea la tabla [Empleado].
    /// </summary>
    public class Empleado
    {
        public int IdEmpleado { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string DNI { get; set; }
        public string Email { get; set; }
        public DateTime FechaIngreso { get; set; }
        public string Puesto { get; set; }
        public string Legajo { get; set; }

        // Null si el empleado no tiene acceso al sistema.
        public int? IdUsuario { get; set; }
        // Username del usuario asociado
        public string Username { get; set; }

        public string NombreCompleto => $"{Nombre} {Apellido}";
    }
}
