using System;
using System.Collections.Generic;
using BE;
using DAL;

namespace BLL
{
    public class BLL_Usuario
    {
        private DAL_Usuario _dal = new DAL_Usuario();

        public List<Usuario> ListarUsuarios()
        {
            try { return _dal.ListarUsuarios(); }
            catch (Exception ex) { throw new Exception("Error al listar usuarios: " + ex.Message); }
        }

        public string AgregarUsuario(Usuario u)
        {
            try
            {
                u.SetClave(Usuario.GenerarClaveHash(u.GetClave()));
                return _dal.AgregarUsuario(u);
            }
            catch (Exception ex) { throw new Exception("Error al agregar usuario: " + ex.Message); }
        }

        public string EditarUsuario(Usuario u)
        {
            try { return _dal.EditarUsuario(u); }
            catch (Exception ex) { throw new Exception("Error al editar usuario: " + ex.Message); }
        }

        public string EliminarUsuario(int idUsuario)
        {
            try { return _dal.EliminarUsuario(idUsuario); }
            catch (Exception ex) { throw new Exception("Error al eliminar usuario: " + ex.Message); }
        }

        public Usuario ObtenerPorDocumento(string documento)
        {
            try { return _dal.ObtenerPorDocumento(documento); }
            catch (Exception ex) { throw new Exception("Error al obtener usuario: " + ex.Message); }
        }
    }
}
