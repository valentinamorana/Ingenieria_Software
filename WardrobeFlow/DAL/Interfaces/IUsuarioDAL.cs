using System;
using System.Collections.Generic;

namespace DAL.Interfaces
{
    /// <summary>Contrato del acceso a datos de Usuario (permite inyección y dobles de prueba).</summary>
    public interface IUsuarioDAL
    {
        List<BE.Usuario> ObtenerTodos();
        List<BE.Usuario> ObtenerArchivados();
        BE.Usuario       ObtenerPorUsername(string username);
        void             Alta(string username, string clave, string perfil);
        // ABM — modificación de datos administrativos NO sensibles y cambio de rol.
        void             Modificar(int idUsuario, string nombre, string apellido, string username, DateTime? fechaNacimiento, string email);
        void             CambiarRol(int idUsuario, string rol);
        void             Bloquear(int idUsuario);
        // Bloqueo progresivo: marca el bloqueo con timestamp e incrementa la escala / auto-desbloqueo al expirar.
        void             BloquearConTiempo(int idUsuario);
        void             AutoDesbloquear(int idUsuario);
        void             Desbloquear(int idUsuario);
        void             IncrementarIntentosFallidos(string username);
        void             ResetearIntentosFallidos(string username);
        void             ResetearTodasLasClaves(string claveHasheada);
        void             ResetearClave(int idUsuario, string claveHasheada);
        // Cambio de clave por el propio usuario (baja el flag RequiereCambioClave).
        void             CambiarClave(int idUsuario, string claveHasheada);
        void             GuardarIdioma(int idUsuario, string idIdioma);
        // RF-10 — baja lógica (archivar) y purga física diferida.
        void             BajaLogica(int idUsuario);
        void             EliminarFisico(int idUsuario);
        int              ContarAdministradoresActivos();
        List<BE.Usuario> ObtenerArchivadosParaPurga(int diasRetencion);
    }
}
