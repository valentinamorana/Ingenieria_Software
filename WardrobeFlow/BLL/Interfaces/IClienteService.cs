using System.Collections.Generic;
using System.Windows.Forms;

namespace BLL.Interfaces
{
    /// <summary>
    /// Contrato de negocio para la gestión de Clientes.
    /// Define las operaciones que la capa de presentación puede invocar
    /// sin conocer los detalles de implementación.
    /// </summary>
    public interface IClienteService
    {
        /// <summary>Devuelve todos los clientes con plan y stock utilizado.</summary>
        List<BE.Cliente> ObtenerTodos();

        /// <summary>Obtiene un cliente por su ID.</summary>
        BE.Cliente ObtenerPorId(int idCliente);

        /// <summary>Registra un nuevo cliente validando datos y unicidad de DNI.</summary>
        void Alta(Form formulario, BE.Cliente cliente);

        /// <summary>Modifica los datos de un cliente existente.</summary>
        void Modificar(Form formulario, BE.Cliente cliente);

        /// <summary>Elimina un cliente si no tiene prendas en uso.</summary>
        void Baja(Form formulario, BE.Cliente cliente);
    }
}
