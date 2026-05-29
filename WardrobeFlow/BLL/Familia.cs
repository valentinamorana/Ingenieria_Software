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
        private readonly DAL.Permiso       permisoDAL = new DAL.Permiso();
        private readonly Servicios.Bitacora _bitacora  = new Servicios.Bitacora();

        private const string RolAdministrador = "Administrador";

        private static void VerificarAdmin()
        {
            if (!Seguridad.SessionManager.IsLoggedIn)
                throw new Exception("No hay sesión activa.");
            string perfil = Seguridad.SessionManager.GetInstance().Usuario.Perfil ?? "";
            if (!perfil.Equals(RolAdministrador, StringComparison.OrdinalIgnoreCase))
                throw new Exception("Solo un Administrador puede modificar permisos de roles.");
        }

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

            // T04 — verificar que el árbol cargado no tenga ciclos (datos corruptos en BD).
            VerificarSinCiclos(raiz);

            return raiz;
        }

        // T04 — Recorre el árbol completo buscando referencias circulares.
        // Usa DFS con un set de visitados en el camino actual.
        // Lanza InvalidOperationException describiendo el ciclo encontrado.
        public void VerificarSinCiclos(BE.Familia raiz)
        {
            var enCamino = new HashSet<int>();
            var camino   = new System.Collections.Generic.Stack<string>();
            VerificarRecursivo(raiz, enCamino, camino);
        }

        private void VerificarRecursivo(BE.Componente nodo,
                                        HashSet<int> enCamino,
                                        System.Collections.Generic.Stack<string> camino)
        {
            // Los nodos raíz virtuales (Id=0) no se registran para evitar falsos positivos.
            if (nodo.Id != 0)
            {
                if (enCamino.Contains(nodo.Id))
                {
                    var ruta = new System.Collections.Generic.List<string>(camino) { nodo.Nombre };
                    ruta.Reverse();
                    throw new InvalidOperationException(
                        $"Referencia circular detectada en el árbol de permisos: " +
                        string.Join(" → ", ruta));
                }
                enCamino.Add(nodo.Id);
                camino.Push(nodo.Nombre);
            }

            foreach (var hijo in nodo.Hijos)
                VerificarRecursivo(hijo, enCamino, camino);

            if (nodo.Id != 0)
            {
                enCamino.Remove(nodo.Id);
                camino.Pop();
            }
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

        // Asigna un permiso a un rol. Requiere sesión activa con rol Administrador.
        public void AsignarPermiso(string rol, int idPermiso)
        {
            VerificarAdmin();
            permisoDAL.AsignarPermiso(rol, idPermiso);
            _bitacora.Registrar("Gestión de Perfiles",
                $"Permiso {idPermiso} asignado al rol '{rol}'",
                BE.Criticidad.Alta);
        }

        // Quita un permiso de un rol. Requiere sesión activa con rol Administrador.
        public void QuitarPermiso(string rol, int idPermiso)
        {
            VerificarAdmin();
            permisoDAL.QuitarPermiso(rol, idPermiso);
            _bitacora.Registrar("Gestión de Perfiles",
                $"Permiso {idPermiso} quitado del rol '{rol}'",
                BE.Criticidad.Alta);
        }
    }
}
