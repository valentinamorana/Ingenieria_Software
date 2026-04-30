using System.Collections.Generic;
using System.Windows.Forms;

namespace BLL.Interfaces
{
    /// <summary>
    /// Gestión de Clientes.
    /// Define las operaciones que la capa de presentación puede invocar
    /// sin conocer los detalles de implementación.
    /// </summary>
    public interface IClienteService
    {
        // Devuelve todos los clientes con plan y stock utilizado.
        List<BE.Cliente> ObtenerTodos();

        // Obtiene un cliente por su ID.
        BE.Cliente ObtenerPorId(int idCliente);

        // Registra un nuevo cliente validando datos y unicidad de DNI.
        void Alta(Form formulario, BE.Cliente cliente);

        // Modifica los datos de un cliente existente.
        void Modificar(Form formulario, BE.Cliente cliente);

        // Elimina un cliente si no tiene prendas en uso.
        void Baja(Form formulario, BE.Cliente cliente);
    }
}
