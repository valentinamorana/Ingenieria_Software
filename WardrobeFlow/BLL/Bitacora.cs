using System;
using System.Data;

namespace BLL
{
    /// <summary>
    /// Lógica de negocio para consultas de auditoría (Bitácora del sistema y de negocio).
    /// La GUI no accede directamente a Servicios.Bitacora ni a Servicios.BitacoraNegocio.
    /// </summary>
    public class Bitacora
    {
        private readonly Servicios.Bitacora        srvSistema = new Servicios.Bitacora();
        private readonly Servicios.BitacoraNegocio srvNegocio = new Servicios.BitacoraNegocio();

        // ── Sistema ───────────────────────────────────────────────────────────

        public DataTable ObtenerTodosSistema()
            => srvSistema.ObtenerTodos();

        public DataTable ObtenerUltimosNDiasSistema(int dias)
            => srvSistema.ObtenerUltimosNDias(dias);

        public DataTable BuscarPorFiltrosSistema(
            DateTime? desde, DateTime? hasta,
            int idUsuario, string actividad, int criticidad)
            => srvSistema.BuscarPorFiltros(desde, hasta, idUsuario, actividad, criticidad);

        // ── Negocio ───────────────────────────────────────────────────────────

        public DataTable ObtenerTodosNegocio()
            => srvNegocio.ObtenerTodos();

        public DataTable BuscarPorFiltrosNegocio(
            DateTime? desde, DateTime? hasta,
            string tipo, int? idCliente, int? idPedido)
            => srvNegocio.BuscarPorFiltros(desde, hasta, tipo, idCliente, idPedido);

        // ── Acceso por rol ────────────────────────────────────────────────────

        /// <summary>
        /// Determina si el usuario activo puede ver la tab de Bitácora del Sistema.
        /// El Supervisor solo accede a la bitácora de negocio; el resto (Administrador) ve ambas.
        /// </summary>
        public bool UsuarioPuedeVerSistema()
        {
            if (!Seguridad.SessionManager.IsLoggedIn) return false;
            string perfil = Seguridad.SessionManager.GetInstance().Usuario.Perfil ?? "";
            return !perfil.Equals("Supervisor", StringComparison.OrdinalIgnoreCase);
        }
    }
}
