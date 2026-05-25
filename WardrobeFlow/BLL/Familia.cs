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
    ///     BE.Familia (nodo — grupo/familia de permisos, cargado desde BD con EsFamilia=1)
    ///       BE.Patente (hoja — permiso individual, Asignado=true si está en RolPermiso)
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
        // El árbol de Familias y Patentes viene ya estructurado desde DAL (vía PermisoRelacion).
        // MarcarAsignados recorre el árbol recursivamente y marca Patente.Asignado según RolPermiso.
        public BE.Familia ObtenerArbolPorRol(string rol)
        {
            if (string.IsNullOrWhiteSpace(rol))
                throw new ArgumentException("El rol no puede estar vacío.");

            List<BE.Componente> arbol    = permisoDAL.ObtenerArbol();
            List<BE.Permiso>    asignados = permisoDAL.ObtenerPorRol(rol);

            var idsAsignados = new HashSet<int>();
            foreach (var p in asignados)
                idsAsignados.Add(p.Id);

            MarcarAsignados(arbol, idsAsignados);

            var raiz = new BE.Familia { Id = 0, Nombre = rol };
            foreach (var nodo in arbol)
                raiz.AgregarHijo(nodo);

            return raiz;
        }

        private void MarcarAsignados(IList<BE.Componente> nodos, HashSet<int> ids)
        {
            foreach (var nodo in nodos)
            {
                if (nodo is BE.Patente patente)
                    patente.Asignado = ids.Contains(patente.Id);
                else if (nodo is BE.Familia familia)
                    MarcarAsignados(familia.Hijos, ids);
            }
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
