using BE;
using System;
using System.Windows.Forms;

namespace Servicios
{
    /// <summary>
    /// Capa de Servicios — Gestión de Bitácora.
    /// Responsable de registrar automáticamente las actividades del usuario en el sistema.
    /// Actúa como intermediario entre la capa GUI/BLL y la capa DAL para el log de auditoría.
    /// </summary>
    public class Bitacora
    {
        // BUG FIX: se instancia correctamente para evitar NullReferenceException
        private readonly DAL.Bitacora bitacoraDAL = new DAL.Bitacora();

        /// <summary>
        /// Registra una actividad del usuario actualmente en sesión en la bitácora del sistema.
        /// Captura automáticamente: fecha/hora, usuario, módulo (nombre del formulario), actividad y criticidad.
        /// </summary>
        /// <param name="formulario">Formulario desde donde se origina la actividad (se usa como nombre de módulo).</param>
        /// <param name="actividad">Descripción breve de la acción realizada (e.g. "Inicio Sesion").</param>
        /// <param name="criticidad">Nivel de importancia del evento: Baja, Media o Alta.</param>
        public void Registrar(Form formulario, string actividad, Criticidad criticidad)
        {
            // Verificar si hay sesión activa usando IsLoggedIn (no GetInstance, que lanza excepción).
            // Esto es necesario porque el Login se registra DESPUÉS de crear la sesión,
            // pero el Logout se registra ANTES de destruirla — ambos casos son válidos.
            if (!Seguridad.SessionManager.IsLoggedIn) return;

            var sesion = Seguridad.SessionManager.GetInstance;

            BE.Bitacora registro = new BE.Bitacora
            {
                Fecha      = DateTime.Now,
                IdUsuario  = sesion.Usuario.Id,
                Modulo     = formulario.Text,
                Actividad  = actividad,
                Criticidad = criticidad,
                Detalle    = $"Usuario '{sesion.Usuario.Username}' (ID: {sesion.Usuario.Id}) " +
                             $"realizó '{actividad}' en el módulo '{formulario.Text}' " +
                             $"[Criticidad: {criticidad}] a las {DateTime.Now:HH:mm:ss}."
            };

            bitacoraDAL.Registrar(registro);
        }
    }
}
