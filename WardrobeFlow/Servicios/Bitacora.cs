using BE;
using System;
using System.Data;
using System.Net;
using System.Net.Sockets;

namespace Servicios
{
    /// <summary>
    /// Capa de Servicios — Bitácora del Sistema.
    ///
    /// Responsabilidades únicas:
    ///   - ESCRITURA: Registrar() y RegistrarSinSesion() persisten eventos en BD.
    ///   - LECTURA:   ObtenerTodos(), ObtenerUltimosNDias(), BuscarPorFiltros()
    ///                para que la GUI consulte la bitácora sin pasar por BLL.
    ///
    /// BLL decide CUÁNDO registrar; Servicios sabe CÓMO persistir y CÓMO consultar.
    /// No existe BLL.Bitacora: la GUI usa Servicios.Bitacora directamente para queries.
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
        /// Registra un evento de seguridad sin requerir sesión activa.
        /// Usado para eventos pre-login como intentos fallidos y bloqueos de cuenta.
        /// A diferencia de <see cref="Registrar"/>, este método no verifica SessionManager.
        /// </summary>
        /// <param name="modulo">Nombre del módulo/formulario de origen.</param>
        /// <param name="actividad">Descripción breve del evento (e.g. "Intento Fallido Login").</param>
        /// <param name="criticidad">Nivel de criticidad del evento.</param>
        /// <param name="idUsuario">ID del usuario afectado (puede ser null si se desconoce).</param>
        /// <param name="detalle">Detalle adicional del evento. Si null, se genera uno automático.</param>
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

        // ── Métodos de consulta (para GUI, sin pasar por BLL) ─────────────────

        /// <summary>
        /// Devuelve todos los registros de la bitácora, ordenados por fecha descendente.
        /// </summary>
        public DataTable ObtenerTodos()
        {
            return bitacoraDAL.ObtenerTodos();
        }

        /// <summary>
        /// Devuelve los registros de los últimos <paramref name="dias"/> días.
        /// Normaliza valores menores a 1.
        /// </summary>
        public DataTable ObtenerUltimosNDias(int dias)
        {
            if (dias < 1) dias = 1;
            return bitacoraDAL.ObtenerUltimosNDias(dias);
        }

        /// <summary>
        /// Búsqueda combinada: rango de fechas, usuario, actividad y criticidad.
        /// Cualquier parámetro nulo/vacío/-1 se ignora en el filtro.
        /// </summary>
        public DataTable BuscarPorFiltros(
            DateTime? desde, DateTime? hasta,
            int idUsuario, string actividad, int criticidad)
        {
            return bitacoraDAL.BuscarPorFiltros(desde, hasta, idUsuario, actividad, criticidad);
        }

        // ── Helper de red ─────────────────────────────────────────────────────

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
