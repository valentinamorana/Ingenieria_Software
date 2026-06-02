using System.Collections.Generic;

namespace DAL.Interfaces
{
    /// <summary>
    /// Contrato del acceso a datos de permisos/roles (T04 Composite).
    /// Permite inyectar el DAL en la BLL y reemplazarlo por un doble de prueba,
    /// habilitando tests unitarios de la lógica de negocio sin base de datos.
    /// </summary>
    public interface IPermisoDAL
    {
        List<BE.Componente> ObtenerArbol();
        List<string>        ObtenerRoles();
        List<BE.Permiso>    ObtenerPorRol(string rol);
        int                 ObtenerIdRol(string rolNombre);
        List<int>           ObtenerIdsHijos(int idPadre);
        void                AgregarRelacion(int idPadre, int idHijo);
        void                QuitarRelacion(int idPadre, int idHijo);
        int                 AltaComponente(string nombre, string nombreMenu, bool esFamilia, bool esRol, string tipoComponente);
        void                ModificarComponente(int idPermiso, string nombre, string nombreMenu);
        void                BajaComponente(int idPermiso);
        int                 ContarUsuariosPorRol(string rol);
        List<string>        ObtenerUsuariosPorRol(string rol);
    }
}
