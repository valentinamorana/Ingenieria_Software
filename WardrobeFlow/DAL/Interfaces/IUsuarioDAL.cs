using System.Collections.Generic;

namespace DAL.Interfaces
{
    /// <summary>Contrato del acceso a datos de Usuario (permite inyección y dobles de prueba).</summary>
    public interface IUsuarioDAL
    {
        List<BE.Usuario> ObtenerTodos();
        BE.Usuario       ObtenerPorUsername(string username);
        void             Alta(string username, string clave, string perfil);
        void             Bloquear(int idUsuario);
        void             Desbloquear(int idUsuario);
        void             IncrementarIntentosFallidos(string username);
        void             ResetearIntentosFallidos(string username);
        void             ResetearTodasLasClaves(string claveHasheada);
        void             ResetearClave(int idUsuario, string claveHasheada);
        void             GuardarIdioma(int idUsuario, string idIdioma);
    }
}
