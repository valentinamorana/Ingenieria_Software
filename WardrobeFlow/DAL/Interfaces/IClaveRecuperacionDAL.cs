using System.Collections.Generic;

namespace DAL.Interfaces
{
    /// <summary>Contrato del acceso a datos de las claves de emergencia (permite inyección y dobles).</summary>
    public interface IClaveRecuperacionDAL
    {
        void Insertar(string claveHash);
        int  ContarTotal();
        int  ContarDisponibles();
        List<KeyValuePair<int, string>> ObtenerDisponibles();
        bool MarcarUsada(int idClave, string username);
        void EliminarTodas();
    }
}
