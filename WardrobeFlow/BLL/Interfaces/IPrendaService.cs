using System.Collections.Generic;
using System.Windows.Forms;

namespace BLL.Interfaces
{
    /// <summary>
    /// Contrato de negocio para la gestión del catálogo de Prendas.
    /// </summary>
    public interface IPrendaService
    {
        /// <summary>Devuelve todas las prendas con cliente actual (JOIN).</summary>
        List<BE.Prenda> ObtenerTodos();

        /// <summary>Devuelve solo las prendas disponibles para asignar a pedidos.</summary>
        List<BE.Prenda> ObtenerDisponibles();

        /// <summary>Devuelve las prendas actualmente asignadas a un cliente.</summary>
        List<BE.Prenda> ObtenerPorCliente(int idCliente);

        /// <summary>Obtiene una prenda por ID.</summary>
        BE.Prenda ObtenerPorId(int idPrenda);

        /// <summary>Da de alta una nueva prenda. Estado inicial siempre Disponible.</summary>
        void Alta(Form formulario, BE.Prenda prenda);

        /// <summary>Modifica los datos descriptivos de una prenda (no afecta estado ni cliente).</summary>
        void Modificar(Form formulario, BE.Prenda prenda);

        /// <summary>
        /// Cambia el estado de una prenda validando las transiciones permitidas por negocio.
        /// </summary>
        void CambiarEstado(Form formulario, BE.Prenda prenda, BE.EstadoPrenda nuevoEstado);
    }
}
