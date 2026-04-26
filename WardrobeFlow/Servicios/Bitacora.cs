using BE;
using System;
using System.Net;
using System.Net.Sockets;

namespace Servicios
{
    /// <summary>
    /// Capa de Servicios — Gestión de Bitácora del Sistema.
    ///
    /// Registra automáticamente las actividades del usuario en sesión.
    /// Recibe el nombre del módulo como string (no como Form) para mantener
    /// Servicios desacoplado de System.Windows.Forms.
    /// Captura la IP local del equipo para trazabilidad de auditoría.
    /// </summary>
    public class Bitacora
    {
        private readonly DAL.Bitacora bitacoraDAL = new DAL.Bitacora();

        /// <summary>
        /// Registra una actividad del usuario en sesión en la bitácora del sistema.
        /// Captura automáticamente: fecha/hora, usuario, módulo, actividad, criticidad e IP.
        /// No lanza excepciones: si no hay sesión activa, retorna silenciosamente.
        /// </summary>
        /// <param name="modulo">Nombre del módulo/formulario desde donde se origina la actividad.</param>
        /// <param name="actividad">Descripción breve de la acción (e.g. "Inicio Sesion").</param>
        /// <param name="criticidad">Nivel de importancia del evento.</param>
        public void Registrar(string modulo, string actividad, Criticidad criticidad)
        {
            // IsLoggedIn evita excepción si se llama antes del Login o después del Logout
            if (!Seguridad.SessionManager.IsLoggedIn) return;

            var sesion = Seguridad.SessionManager.GetInstance;
            string ip  = ObtenerIPLocal();

            BE.Bitacora registro = new BE.Bitacora
            {
                Fecha      = DateTime.Now,
                IdUsuario  = (int?)sesion.Usuario.Id,
                Modulo     = modulo ?? string.Empty,
                Actividad  = actividad,
                Criticidad = criticidad,
                IP         = ip,
                Detalle    = $"Usuario '{sesion.Usuario.Username}' (ID: {sesion.Usuario.Id}) " +
                             $"realizó '{actividad}' en '{modulo}' " +
                             $"[Criticidad: {criticidad}] desde {ip} a las {DateTime.Now:HH:mm:ss}."
            };

            bitacoraDAL.Registrar(registro);
        }

        /// <summary>
        /// Obtiene la dirección IPv4 local del equipo.
        /// Usado para registrar desde qué máquina se realizó cada acción.
        /// </summary>
        public static string ObtenerIPLocal()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                        return ip.ToString();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Servicios.Bitacora.ObtenerIPLocal] {ex.Message}");
            }
            return "IP desconocida";
        }
    }
}
