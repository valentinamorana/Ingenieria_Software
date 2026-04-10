using System;
using System.Collections.Generic;
using BE;

namespace Seguridad
{
    public class BLL_Permiso
    {
        private DAL_Permiso _dal = new DAL_Permiso();

        public List<Permiso> ListarPermisos()
        {
            try { return _dal.ListarPermisos(); }
            catch (Exception ex) { throw new Exception("Error al listar permisos: " + ex.Message); }
        }

        public List<Permiso> ListarPermisosPorUsuario(int idUsuario)
        {
            try { return _dal.ListarPermisosPorUsuario(idUsuario); }
            catch (Exception ex) { throw new Exception("Error al obtener permisos: " + ex.Message); }
        }
    }
}
