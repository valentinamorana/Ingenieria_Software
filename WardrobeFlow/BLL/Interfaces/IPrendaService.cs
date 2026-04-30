using System.Collections.Generic;
using System.Windows.Forms;

namespace BLL.Interfaces
{
    /// <summary>
    /// Gestión del catálogo de Prendas.
    /// </summary>
    public interface IPrendaService
    {
        // Devuelve todas las prendas con cliente actual (JOIN).
        List<BE.Prenda> ObtenerTodos();

        // Devuelve solo las prendas disponibles para asignar a pedidos.
        List<BE.Prenda> ObtenerDisponibles();

        // Devuelve las prendas actualmente asignadas a un cliente.
        List<BE.Prenda> ObtenerPorCliente(int idCliente);

        // Obtiene una prenda por ID.
        BE.Prenda ObtenerPorId(int idPrenda);

        // Da de alta una nueva prenda. Estado inicial siempre Disponible.
        void Alta(Form formulario, BE.Prenda prenda);

        // Modifica los datos descriptivos de una prenda (no afecta estado ni cliente).
        void Modificar(Form formulario, BE.Prenda prenda);

        // Cambia el estado de una prenda validando las transiciones permitidas por negocio.
        void CambiarEstado(Form formulario, BE.Prenda prenda, BE.EstadoPrenda nuevoEstado);
    }
}
