using System;
using System.Collections.Generic;
using System.Linq;
using BE;

namespace Seguridad
{
    public class BLL_Permiso
    {
        private DAL_Permiso _dal = new DAL_Permiso();

        // ── Listar permisos planos ─────────────────────────────────────────
        public List<Permiso> ListarPermisos()
        {
            try { return _dal.ListarPermisos(); }
            catch (Exception ex) { throw new Exception("Error al listar permisos: " + ex.Message); }
        }

        // ── Permisos asignados a un usuario ────────────────────────────────
        public List<Permiso> ListarPermisosPorUsuario(int idUsuario)
        {
            try { return _dal.ListarPermisosPorUsuario(idUsuario); }
            catch (Exception ex) { throw new Exception("Error al obtener permisos: " + ex.Message); }
        }

        // ── Guardar permisos seleccionados para un usuario ─────────────────
        public string GuardarPermisosUsuario(int idUsuario, List<int> idsPermisos)
        {
            try { return _dal.GuardarPermisosUsuario(idUsuario, idsPermisos); }
            catch (Exception ex) { throw new Exception("Error al guardar permisos: " + ex.Message); }
        }

        // ── PATRON COMPOSITE — construir arbol de permisos agrupados ───────
        // Agrupa los permisos por su TipoComponente, devolviendo un arbol
        // donde cada GrupoPermiso (nodo) contiene sus Permiso hojas.
        public List<GrupoPermiso> ObtenerArbolPermisos()
        {
            try
            {
                List<Permiso> todos = _dal.ListarPermisos();
                List<GrupoPermiso> arbol = new List<GrupoPermiso>();

                // Agrupar por modulo (TipoComponente)
                var grupos = todos.GroupBy(p => p.TipoComponente);
                foreach (var grupo in grupos)
                {
                    GrupoPermiso gp = new GrupoPermiso();
                    gp.NombreGrupo    = grupo.Key;
                    gp.Nombre         = grupo.Key;
                    gp.TipoComponente = "Grupo";
                    gp.Estado         = true;

                    // Agregar cada permiso como hoja del grupo
                    foreach (Permiso p in grupo)
                        gp.AgregarHijo(p);

                    arbol.Add(gp);
                }
                return arbol;
            }
            catch (Exception ex) { throw new Exception("Error al construir arbol de permisos: " + ex.Message); }
        }
    }
}
