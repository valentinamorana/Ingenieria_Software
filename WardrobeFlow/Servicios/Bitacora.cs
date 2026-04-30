using BE;
using System;
using System.Data;
using System.Net;
using System.Net.Sockets;

namespace Servicios
{
    /// <summary>Servicio para registrar y consultar la bitácora del sistema.</summary>
    public class Bitacora
    {
        private readonly DAL.Bitacora bitacoraDAL = new DAL.Bitacora();

        // >Registra una actividad del usuario en sesión. No lanza si no hay sesión activa.
        public void Registrar(string modulo, string actividad, Criticidad criticidad)
        {
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

        // Registra un evento sin requerir sesión activa (p. ej. intentos de login fallidos).
        public void RegistrarSinSesion(string modulo, string actividad, Criticidad criticidad,
                                        int? idUsuario = null, string detalle = null)
        {
            try
            {
                string ip = ObtenerIPLocal();
                bitacoraDAL.Registrar(new BE.Bitacora
                {
                    Fecha      = DateTime.Now,
                    IdUsuario  = idUsuario,
                    Modulo     = modulo ?? string.Empty,
                    Actividad  = actividad,
                    Criticidad = criticidad,
                    IP         = ip,
                    Detalle    = detalle ??
                                 $"Actividad '{actividad}' en '{modulo}' desde {ip} " +
                                 $"a las {DateTime.Now:HH:mm:ss}."
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[Servicios.Bitacora.RegistrarSinSesion] Error al registrar: {ex.Message}");
            }
        }

        // Consultas 
        // Devuelve todos los registros de la bitácora.
        public DataTable ObtenerTodos()
        {
            return bitacoraDAL.ObtenerTodos();
        }

        // Devuelve los registros de los últimos N días.
        public DataTable ObtenerUltimosNDias(int dias)
        {
            if (dias < 1) dias = 1;
            return bitacoraDAL.ObtenerUltimosNDias(dias);
        }

        // Búsqueda filtrada por fecha, usuario, actividad y criticidad.
        public DataTable BuscarPorFiltros(
            DateTime? desde, DateTime? hasta,
            int idUsuario, string actividad, int criticidad)
        {
            return bitacoraDAL.BuscarPorFiltros(desde, hasta, idUsuario, actividad, criticidad);
        }

        // Obtiene la IP local del equipo.
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
