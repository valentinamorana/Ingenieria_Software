using System;
using System.Collections.Generic;
using DAL.Interfaces;

namespace Tests.Fakes
{
    /// <summary>
    /// Doble de prueba de IUsuarioDAL (sin SQL Server). Implementa todos los miembros del
    /// contrato con cuerpos mínimos y deja espías sobre CambiarClave/Modificar/CambiarRol.
    /// Se puede sembrar una lista de usuarios (Seed) para probar lectura/búsqueda.
    /// </summary>
    public class FakeUsuarioDAL : IUsuarioDAL
    {
        // Usuarios sembrados (vacío por defecto → comportamiento previo intacto).
        public readonly List<BE.Usuario> Usuarios = new List<BE.Usuario>();
        public int AdministradoresActivos { get; set; } = 1;

        // Espías de CambiarClave.
        public int     CambiarClaveVeces  { get; private set; }
        public int     CambiarClaveIdUsuario { get; private set; }
        public string  CambiarClaveHash   { get; private set; }

        // Espías de Modificar / CambiarRol.
        public int     ModificarVeces { get; private set; }
        public int     CambiarRolVeces { get; private set; }
        public string  CambiarRolValor { get; private set; }

        public void CambiarClave(int idUsuario, string claveHasheada)
        {
            CambiarClaveVeces++;
            CambiarClaveIdUsuario = idUsuario;
            CambiarClaveHash      = claveHasheada;
        }

        public void Modificar(int idUsuario, string nombre, string apellido, string username, DateTime? fechaNacimiento, string email)
        {
            ModificarVeces++;
            var u = Usuarios.Find(x => x.Id == idUsuario);
            if (u != null)
            {
                u.Nombre = nombre; u.Apellido = apellido; u.Username = username;
                u.FechaNacimiento = fechaNacimiento; u.Email = email;
            }
        }

        public void CambiarRol(int idUsuario, string rol)
        {
            CambiarRolVeces++;
            CambiarRolValor = rol;
            var u = Usuarios.Find(x => x.Id == idUsuario);
            if (u != null) { u.Perfil = rol; u.Rol = rol; }
        }

        // Resto del contrato: lee de la lista sembrada (vacía por defecto).
        public List<BE.Usuario> ObtenerTodos()                       => new List<BE.Usuario>(Usuarios);
        public List<BE.Usuario> ObtenerArchivados()                  => new List<BE.Usuario>();
        public BE.Usuario       ObtenerPorUsername(string username)  => Usuarios.Find(x => string.Equals(x.Username, username, StringComparison.OrdinalIgnoreCase));
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
        public int              ContarAdministradoresActivos()       => AdministradoresActivos;
        public List<BE.Usuario> ObtenerArchivadosParaPurga(int dias) => new List<BE.Usuario>();
    }
}
