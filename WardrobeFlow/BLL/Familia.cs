using System;
using System.Collections.Generic;

namespace BLL
{
    /// <summary>
    /// Capa de Lógica de Negocio — T04 Gestión de Perfiles de Usuario (Patrón Composite).
    ///
    /// Responsabilidades:
    ///   1. ObtenerArbolPorRol()           — construye el árbol Familia→Patente para GestorPermisos.
    ///   2. ConstruirArbolOrganizacional()  — construye en memoria la jerarquía de la empresa
    ///                                        (WardrobeFlow → Área → Rol → Patentes) para el
    ///                                        ExploradorCompositeForm (demostración académica).
    ///   3. AsignarPermiso() / QuitarPermiso() — modifican [RolPermiso] para el mecanismo real de auth.
    /// </summary>
    public class Familia
    {
        private readonly DAL.Permiso       permisoDAL = new DAL.Permiso();
        private readonly Servicios.Bitacora _bitacora  = new Servicios.Bitacora();

        private const string RolAdministrador = "Administrador";

        private static void VerificarAdmin()
        {
            if (!Seguridad.SessionManager.IsLoggedIn)
                throw new BE.AppException("err.bll.sesion_expirada",
                    "La sesión expiró. Volvé a iniciar sesión.");
            string perfil = Seguridad.SessionManager.GetInstance().Usuario.Perfil ?? "";
            if (!perfil.Equals(RolAdministrador, StringComparison.OrdinalIgnoreCase))
                throw new BE.AppException("err.bll.familia.sin_permiso",
                    "Solo un Administrador puede modificar permisos de roles.");
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
            foreach (var p in asignados) idsAsignados.Add(p.Id);

            MarcarAsignados(arbol, idsAsignados);

            var raiz = new BE.Familia { Id = 0, Nombre = rol };
            foreach (var nodo in arbol) raiz.AgregarHijo(nodo);

            VerificarSinCiclos(raiz);
            return raiz;
        }

        // T04 — Construye en memoria la jerarquía organizacional de la empresa como árbol Composite.
        //
        // Estructura generada:
        //   BE.Familia("WardrobeFlow")            ← raíz
        //     BE.Familia("Administración")         ← área
        //       BE.Familia("Administrador")        ← rol (con sus Patentes)
        //         BE.Patente("Gestionar Usuarios") ← permiso atómico
        //         ...
        //       BE.Familia("Auditor")
        //         BE.Patente("Ver Auditoría")
        //     BE.Familia("Comercial")
        //       ...
        //     BE.Familia("Inventario y Logística")
        //       ...
        //
        // Usado exclusivamente por ExploradorCompositeForm (visualización académica).
        // No modifica [RolPermiso] ni afecta la autorización.
        // Los permisos de cada rol se leen de [RolPermiso] vía DAL.Permiso.ObtenerPorRol().
        public BE.Familia ConstruirArbolOrganizacional()
        {
            // Mapa: rol técnico → (área, nombre de display).
            // Los nombres de display son iguales a los usados en GestorPermisos (sin sufijos "(legacy)").
            var mapa = new List<(string Area, string Rol, string RolDisplay)>
            {
                // Administración
                ("Administración",        "Administrador",        "Administrador"),
                ("Administración",        "Auditor",              "Auditor"),
                ("Administración",        "Supervisor",           "Supervisor"),
                // Comercial
                ("Comercial",             "GerenteComercial",     "Gerente Comercial"),
                ("Comercial",             "Vendedor",             "Vendedor"),
                // Inventario y Logística
                ("Inventario y Logística","GerenteInventario",    "Gerente de Inventario"),
                ("Inventario y Logística","EncargadoDeStock",     "Encargado de Stock"),
                ("Inventario y Logística","ControladorDeStock",   "Controlador de Stock"),
                ("Inventario y Logística","OperadorLogistico",    "Operador Logístico"),
                ("Inventario y Logística","OperadorDeInventario", "Operador de Inventario"),
            };

            // Paso 1: construir nodos de rol con sus Patentes.
            // Agrupar por área solo los roles que tienen permisos en [RolPermiso].
            var rolesPorArea = new Dictionary<string, List<BE.Familia>>(StringComparer.OrdinalIgnoreCase);
            var rolesAgregados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var (area, rol, rolDisplay) in mapa)
            {
                if (rolesAgregados.Contains(rol)) continue;
                rolesAgregados.Add(rol);

                List<BE.Permiso> permsRol;
                try { permsRol = permisoDAL.ObtenerPorRol(rol); }
                catch { permsRol = new List<BE.Permiso>(); }

                if (permsRol.Count == 0) continue; // omitir roles sin permisos (área no se agrega si quedaría vacía)

                var nodoRol = new BE.Familia { Id = 0, Nombre = rolDisplay };
                foreach (var perm in permsRol)
                {
                    nodoRol.AgregarHijo(new BE.Patente
                    {
                        Id         = perm.Id,
                        Nombre     = perm.Nombre,
                        NombreMenu = perm.NombreMenu,
                        Asignado   = true
                    });
                }

                if (!rolesPorArea.ContainsKey(area))
                    rolesPorArea[area] = new List<BE.Familia>();
                rolesPorArea[area].Add(nodoRol);
            }

            // Paso 2: construir el árbol — solo agrega áreas con al menos un rol activo.
            var empresa = new BE.Familia { Id = 0, Nombre = "WardrobeFlow" };
            foreach (var kvp in rolesPorArea)
            {
                var nodoArea = new BE.Familia { Id = 0, Nombre = kvp.Key };
                foreach (var nodoRol in kvp.Value)
                    nodoArea.AgregarHijo(nodoRol);
                empresa.AgregarHijo(nodoArea);
            }

            return empresa;
        }

        // T04 — DFS con HashSet para detectar ciclos en el árbol cargado.
        public void VerificarSinCiclos(BE.Familia raiz)
        {
            var enCamino = new HashSet<int>();
            var camino   = new Stack<string>();
            VerificarRecursivo(raiz, enCamino, camino);
        }

        private void VerificarRecursivo(BE.Componente nodo, HashSet<int> enCamino, Stack<string> camino)
        {
            if (nodo.Id != 0)
            {
                if (enCamino.Contains(nodo.Id))
                {
                    var ruta = new List<string>(camino) { nodo.Nombre };
                    ruta.Reverse();
                    throw new InvalidOperationException(
                        "Referencia circular detectada en el árbol de permisos: " +
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
