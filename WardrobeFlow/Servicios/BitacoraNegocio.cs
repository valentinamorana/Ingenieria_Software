using BE;
using System;
using System.Data;

namespace Servicios
{
    /// <summary>
    /// Capa de Servicios — BitacoraNegocio.
    ///
    /// Responsabilidades únicas:
    ///   - ESCRITURA: Registrar() persiste eventos de negocio en [BitacoraNegocio].
    ///   - LECTURA:   ObtenerTodos() y BuscarPorFiltros() para consultas desde la GUI.
    ///
    /// No existe BLL.BitacoraNegocio: la GUI usa Servicios.BitacoraNegocio directamente.
    /// Los eventos de negocio están separados de la tabla [Bitacora] (seguridad/sistema).
    /// </summary>
    public class BitacoraNegocio
    {
        private readonly DAL.BitacoraNegocio dal = new DAL.BitacoraNegocio();

        /// <summary>
        /// Registra un evento de negocio.
        /// IdUsuario se resuelve automáticamente desde SessionManager si hay sesión activa.
        /// </summary>
        public void Registrar(
            TipoEventoNegocio tipo,
            string descripcion,
            int? idPedido  = null,
            int? idPrenda  = null,
            int? idCliente = null)
        {
            try
            {
                int? idUsuario = null;
                if (Seguridad.SessionManager.IsLoggedIn)
                    idUsuario = Seguridad.SessionManager.GetInstance.Usuario.Id;

                var evento = new BE.BitacoraNegocio
                {
                    Fecha       = DateTime.Now,
                    Tipo        = tipo,
                    IdUsuario   = idUsuario,
                    IdPedido    = idPedido,
                    IdPrenda    = idPrenda,
                    IdCliente   = idCliente,
                    Descripcion = descripcion
                };

                dal.Registrar(evento);
            }
            catch
            {
                // No interrumpir el flujo de negocio por error de bitácora
            }
        }

        // ── Métodos de consulta (para GUI, sin pasar por BLL) ─────────────────

        /// <summary>Devuelve todos los eventos de negocio ordenados por fecha descendente.</summary>
        public DataTable ObtenerTodos()
        {
            return dal.ObtenerTodos();
        }

        /// <summary>
        /// Búsqueda combinada: rango de fechas, tipo de evento, cliente y pedido.
        /// Cualquier parámetro nulo/vacío se ignora en el filtro.
        /// </summary>
        public DataTable BuscarPorFiltros(
            DateTime? desde, DateTime? hasta,
            string tipo, int? idCliente, int? idPedido)
        {
            return dal.BuscarPorFiltros(desde, hasta, tipo, idCliente, idPedido);
        }
    }
}
