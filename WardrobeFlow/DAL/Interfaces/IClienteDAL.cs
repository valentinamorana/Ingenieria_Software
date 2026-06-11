using System.Collections.Generic;

namespace DAL.Interfaces
{
    /// <summary>Contrato del acceso a datos de Cliente (permite inyección y dobles de prueba).</summary>
    public interface IClienteDAL
    {
        List<BE.Cliente> ObtenerTodos();
        BE.Cliente       ObtenerPorId(int idCliente);
        int              Alta(BE.Cliente cliente);
        void             Modificar(BE.Cliente cliente);
        void             Baja(int idCliente);
        bool             ExisteDNI(string dni);
        bool             ExisteDNIParaOtro(string dni, int idExcluir);
    }
}
