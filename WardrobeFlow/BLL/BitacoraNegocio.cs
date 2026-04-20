using System;
using System.Data;

namespace BLL
{
    /// <summary>
    /// Capa de Lógica de Negocio — Consultas de BitacoraNegocio.
    /// Provee a la GUI los métodos para consultar y filtrar eventos de negocio
    /// (ventas, despachos, stock, clientes) separados de la bitácora de seguridad.
    /// </summary>
    public class BitacoraNegocio
    {
        private readonly DAL.BitacoraNegocio dal = new DAL.BitacoraNegocio();

        /// <summary>Devuelve todos los eventos de negocio ordenados por fecha descendente.</summary>
        public DataTable ObtenerTodos()
        {
            return dal.ObtenerTodos();
        }

        /// <summary>Búsqueda combinada: fechas, tipo de evento, cliente y pedido.</summary>
        public DataTable BuscarPorFiltros(
            DateTime? desde, DateTime? hasta,
            string tipo, int? idCliente, int? idPedido)
        {
            return dal.BuscarPorFiltros(desde, hasta, tipo, idCliente, idPedido);
        }
    }
}
