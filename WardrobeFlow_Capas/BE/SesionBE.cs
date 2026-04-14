using System;

namespace BE
{
    // Clase que representa una sesion activa del sistema.
    // Basada en SesionBE del ejemplo de referencia (Nacho/Codigo),
    // adaptada para usar la entidad Usuario de WardrobeFlow (Guid Id).
    public class SesionBE
    {
        // Usuario que inicio sesion
        public Usuario UsuarioActual { get; private set; }

        // Fecha y hora en que comenzo la sesion
        public DateTime FechaInicioSesion { get; private set; }

        // Fecha y hora en que se cerro la sesion (null si aun esta activa)
        public DateTime? FechaFinSesion { get; private set; }

        // Indica si la sesion sigue activa
        public bool SesionActiva { get; private set; }

        // Nombre del equipo desde el que se inicio sesion
        public string NombreMaquina { get; private set; }

        // Constructor: inicializa la sesion como inactiva
        public SesionBE()
        {
            SesionActiva  = false;
            NombreMaquina = Environment.MachineName;
        }

        // Inicia la sesion para el usuario indicado
        public void IniciarSesion(Usuario usuario)
        {
            if (usuario == null)
                throw new ArgumentNullException("usuario", "El usuario no puede ser nulo");

            UsuarioActual    = usuario;
            FechaInicioSesion = DateTime.Now;
            FechaFinSesion   = null;
            SesionActiva     = true;
        }

        // Cierra la sesion y registra la hora de fin
        public void CerrarSesion()
        {
            FechaFinSesion = DateTime.Now;
            SesionActiva   = false;
            UsuarioActual  = null;
        }

        // Calcula cuanto tiempo lleva activa la sesion
        public TimeSpan ObtenerDuracionSesion()
        {
            if (!SesionActiva)
                return TimeSpan.Zero;

            DateTime fechaFin = FechaFinSesion ?? DateTime.Now;
            return fechaFin.Subtract(FechaInicioSesion);
        }
    }
}
