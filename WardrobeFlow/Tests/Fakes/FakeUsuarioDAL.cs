using System.Collections.Generic;
using DAL.Interfaces;

namespace Tests.Fakes
{
    /// <summary>
    /// Doble de prueba de IUsuarioDAL (sin SQL Server). Implementa todos los miembros del
    /// contrato con cuerpos mínimos y deja espías sobre CambiarClave para verificar el cambio
    /// de clave obligatorio (clave nueva persistida + flag bajado).
    /// </summary>
    public class FakeUsuarioDAL : IUsuarioDAL
    {
        // Espías de CambiarClave.
        public int     CambiarClaveVeces  { get; private set; }
        public int     CambiarClaveIdUsuario { get; private set; }
        public string  CambiarClaveHash   { get; private set; }

        public void CambiarClave(int idUsuario, string claveHasheada)
        {
            CambiarClaveVeces++;
            CambiarClaveIdUsuario = idUsuario;
            CambiarClaveHash      = claveHasheada;
        }

        // Resto del contrato: sin efecto (no se usan en estas pruebas).
        public List<BE.Usuario> ObtenerTodos()                       => new List<BE.Usuario>();
        public List<BE.Usuario> ObtenerArchivados()                  => new List<BE.Usuario>();
        public BE.Usuario       ObtenerPorUsername(string username)  => null;
        public void             Alta(string u, string c, string p)   { }
        public void             Bloquear(int id)                     { }
        public void             BloquearConTiempo(int id)            { }
        public void             AutoDesbloquear(int id)              { }
        public void             Desbloquear(int id)                  { }
        public void             IncrementarIntentosFallidos(string u){ }
        public void             ResetearIntentosFallidos(string u)   { }
        public void             ResetearTodasLasClaves(string hash)  { }
        public void             ResetearClave(int id, string hash)   { }
        public void             GuardarIdioma(int id, string idi)    { }
        public void             BajaLogica(int id)                   { }
        public void             EliminarFisico(int id)               { }
        public int              ContarAdministradoresActivos()       => 1;
        public List<BE.Usuario> ObtenerArchivadosParaPurga(int dias) => new List<BE.Usuario>();
    }
}
