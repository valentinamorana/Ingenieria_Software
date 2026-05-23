using System;
using System.Collections.Generic;

namespace BLL
{
    /// <summary>
    /// Capa de Lógica de Negocio — T04 Gestión de Perfiles de Usuario (Patrón Composite).
    ///
    /// Construye el árbol de permisos en memoria a partir de los datos en BD.
    /// Estructura del árbol:
    ///   BE.Familia (raíz — nombre del rol)
    ///     BE.Familia (nodo — grupo TipoComponente: "Inventario", "Ventas", ...)
    ///       BE.Patente (hoja — permiso individual, con Asignado=true si está en el rol)
    ///
    /// La GUI usa esta BLL exclusivamente; nunca toca DAL.Permiso directamente.
    /// </summary>
    public class Familia
    {
        private readonly DAL.Permiso permisoDAL = new DAL.Permiso();

        // Retorna la lista de roles disponibles en el sistema.
        public List<string> ObtenerRoles()
        {
            return permisoDAL.ObtenerRoles();
        }

        // Construye el árbol Composite completo para el rol dado.
        // Raíz = Familia con el nombre del rol.
        // Hijos de la raíz = una Familia por cada TipoComponente.
        // Hojas = Patente por cada permiso, con Asignado marcado según RolPermiso.
        public BE.Familia ObtenerArbolPorRol(string rol)
        {
            if (string.IsNullOrWhiteSpace(rol))
                throw new ArgumentException("El rol no puede estar vacío.");

            List<BE.Permiso> todos     = permisoDAL.ObtenerTodos();
            List<BE.Permiso> asignados = permisoDAL.ObtenerPorRol(rol);

            var idsAsignados = new HashSet<int>();
            foreach (var p in asignados)
                idsAsignados.Add(p.Id);

            var raiz = new BE.Familia { Id = 0, Nombre = rol };

            // Agrupar por TipoComponente preservando el orden de aparición
            var grupos = new Dictionary<string, BE.Familia>();
            var ordenGrupos = new List<string>();

            foreach (var perm in todos)
            {
                string grupo = string.IsNullOrWhiteSpace(perm.TipoComponente) ? "General" : perm.TipoComponente;

                if (!grupos.ContainsKey(grupo))
                {
                    grupos[grupo] = new BE.Familia { Nombre = grupo };
                    ordenGrupos.Add(grupo);
                }

                grupos[grupo].AgregarHijo(new BE.Patente
                {
                    Id        = perm.Id,
                    Nombre    = perm.Nombre,
                    NombreMenu = perm.NombreMenu,
                    Asignado  = idsAsignados.Contains(perm.Id)
                });
            }

            foreach (string g in ordenGrupos)
                raiz.AgregarHijo(grupos[g]);

            return raiz;
        }

        // Asigna un permiso a un rol. Solo Administrador (validación en GUI).
        public void AsignarPermiso(string rol, int idPermiso)
        {
            permisoDAL.AsignarPermiso(rol, idPermiso);
        }

        // Quita un permiso de un rol. Solo Administrador (validación en GUI).
        public void QuitarPermiso(string rol, int idPermiso)
        {
            permisoDAL.QuitarPermiso(rol, idPermiso);
        }
    }
}
