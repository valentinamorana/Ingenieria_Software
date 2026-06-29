using System;
using System.Collections.Generic;

namespace BLL
{
    /// <summary>
    /// Capa de Lógica de Negocio — T04 Gestión de Perfiles de Usuario (Patrón Composite).
    ///
    /// Responsabilidades:
    ///   1. ObtenerArbol() / ObtenerArbolPorRol() — exponen el árbol Composite desde BD.
    ///   2. ObtenerPermisosEfectivos()            — resuelve recursivamente los permisos de un rol.
    ///   3. CRUD de componentes + AsignarPermiso() / QuitarPermiso() — operan sobre [PermisoRelacion],
    ///      el motor real de autorización.
    /// </summary>
    public class Familia
    {
        private readonly DAL.Interfaces.IPermisoDAL permisoDAL;
        // Bitácora perezosa: solo se instancia cuando una operación de escritura la usa.
        // Así los métodos de lectura/resolución se pueden testear sin tocar la BD.
        private Servicios.Bitacora _bitacoraLazy;
        private Servicios.Bitacora _bitacora => _bitacoraLazy ?? (_bitacoraLazy = new Servicios.Bitacora());

        // Inyección de dependencias: el constructor por defecto usa el DAL real;
        // el segundo permite inyectar un doble de prueba (tests unitarios sin BD).
        public Familia() : this(new DAL.Permiso()) { }
        public Familia(DAL.Interfaces.IPermisoDAL permisoDAL)
        {
            this.permisoDAL = permisoDAL;
        }

        // Patente que habilita la gestión de usuarios/permisos (acceso a esta misma pantalla).
        private const string MenuGestion = "mnuUsuarios";

        // ¿El usuario en sesión es Administrador? (acceso total por rol — bypass, decisión B1).
        private static bool EsAdminEnSesion()
        {
            return Seguridad.SessionManager.IsLoggedIn &&
                   Seguridad.SessionManager.GetInstance().Usuario.EsAdministrador;
        }

        // Re-validación en el BACKEND. Tras la Etapa 4, administrar permisos no es exclusivo del
        // Administrador: también puede hacerlo un rol que tenga la patente de Gestión de Usuarios
        // (el Administrador sigue con acceso total por bypass).
        private static void VerificarPuedeGestionar()
        {
            // Guard centralizado (DRY): Administrador (bypass) o rol con la patente mnuUsuarios.
            BLLHelper.ExigirGestionUsuarios();
        }

        // Anti-autobloqueo: simula los permisos efectivos que daría un conjunto de ids (resolviendo
        // sus subárboles en el árbol actual) y dice si CONSERVAN el acceso de gestión.
        private bool ConservaGestion(IEnumerable<int> ids)
        {
            var arbol = permisoDAL.ObtenerArbol();
            foreach (int id in ids)
            {
                var nodo = BuscarPorId(arbol, id, new HashSet<int>());
                if (nodo == null) continue;
                foreach (var pat in nodo.ObtenerPatentesEfectivas())
                    if (pat.NombreMenu == MenuGestion) return true;
            }
            return false;
        }

        // ── Guard SISTÉMICO de "último administrador" (idea tomada del proyecto Stach) ───────────
        // El anti-autobloqueo de arriba solo protege la PROPIA cuenta. Este guard protege al SISTEMA
        // ENTERO: ninguna operación destructiva (quitar una relación, vaciar o eliminar un rol) puede
        // dejar a TODOS los usuarios activos sin acceso de Gestión de Usuarios. Cubre el caso de roles
        // tipo "admin1 / admin2" (sin el perfil literal "Administrador") y la gestión heredada por
        // rol-dentro-de-rol, que la protección por nombre de perfil de BLL.Usuario no contempla.

        // Aplica una mutación SIMULADA sobre una copia fresca del árbol y, si el resultado dejaría al
        // sistema sin ningún usuario capaz de gestionar, aborta la operación con un error de negocio.
        // Esta parte hace I/O (lee árbol + usuarios); la DECISIÓN vive en el método puro testeable.
        private void ExigirSistemaConservaGestion(Action<List<BE.Componente>> mutarArbolSimulado)
        {
            List<BE.Componente> arbol;
            try { arbol = permisoDAL.ObtenerArbol(); }
            catch { return; }   // si no se puede reconstruir el árbol, no bloqueamos por un fallo lateral
            try { mutarArbolSimulado(arbol); }
            catch { /* la mutación simulada nunca debe romper la operación real */ }

            List<BE.Usuario> usuarios;
            try { usuarios = new Usuario().ObtenerTodos(); }
            catch { return; }   // sin poder enumerar usuarios → fail-open controlado (no romper por un fallo de BD)

            if (!SistemaConservaGestion(arbol, usuarios))
                throw new BE.AppException("err.bll.familia.sistema_sin_gestion",
                    "Esta acción dejaría al sistema SIN NINGÚN usuario con acceso de Gestión de Usuarios. " +
                    "Asigná la gestión a otro rol o usuario antes de continuar.");
        }

        // Núcleo PURO y testeable (mismo estilo que ValidarPuedeArchivar/ValidarPuedeCambiarRol):
        // dado un árbol de permisos y una lista de usuarios, ¿al menos uno conserva acceso de gestión?
        // Un usuario lo conserva si es Administrador por su perfil (bypass) o si su rol resuelve la
        // patente de gestión (mnuUsuarios), incluso heredada por rol-dentro-de-rol. Sin usuarios que
        // verificar, devuelve true (no bloquea). No toca BD ni sesión: se puede testear de forma aislada.
        public static bool SistemaConservaGestion(IList<BE.Componente> arbol, IEnumerable<BE.Usuario> usuarios)
        {
            if (usuarios == null) return true;
            var raices = arbol ?? new List<BE.Componente>();

            bool hayUsuarios = false;
            foreach (var u in usuarios)
            {
                if (u == null) continue;
                hayUsuarios = true;
                if (u.EsAdministrador) return true;                       // bypass por perfil Administrador
                string rol = u.Rol ?? u.Perfil;
                if (string.IsNullOrWhiteSpace(rol)) continue;
                var nodo = BuscarRol(raices, rol, new HashSet<int>());
                if (nodo == null) continue;
                foreach (var pat in nodo.ObtenerPatentesEfectivas())
                    if (pat.NombreMenu == MenuGestion) return true;
            }
            return !hayUsuarios;   // lista vacía/solo-nulos → no bloquear
        }

        // Todas las Familias/Roles del árbol (para desanclar un nodo de todos sus padres al simular
        // una eliminación). Deduplicado por Id.
        private static IEnumerable<BE.Familia> TodasLasFamilias(IList<BE.Componente> nodos)
        {
            var acc = new List<BE.Familia>();
            var vis = new HashSet<int>();
            void Rec(IList<BE.Componente> ns)
            {
                foreach (var n in ns)
                {
                    if (n.Id != 0 && !vis.Add(n.Id)) continue;
                    if (n is BE.Familia f) { acc.Add(f); Rec(f.Hijos); }
                }
            }
            Rec(nodos);
            return acc;
        }

        // Retorna la lista de roles disponibles en el sistema.
        public List<string> ObtenerRoles()
        {
            return permisoDAL.ObtenerRoles();
        }

        // ── T04 — MOTOR REAL de autorización (Composite + recursión) ────────────

        // Devuelve el árbol Composite COMPLETO desde BD (Familias, Patentes y Roles
        // ya estructurados vía PermisoRelacion). Para visualización en TreeView.
        public List<BE.Componente> ObtenerArbol()
        {
            return permisoDAL.ObtenerArbol();
        }

        // Permisos EFECTIVOS de un rol — recorre RECURSIVAMENTE toda la jerarquía
        // (rol → roles/familias → ... → patentes) y devuelve las hojas (Patentes) sin
        // duplicados. Un permiso que aparece por varios caminos se cuenta una sola vez.
        // Esta es la lista que se carga en la sesión del usuario (BLL.Usuario.Login).
        public List<BE.Permiso> ObtenerPermisosEfectivos(string rol)
        {
            var resultado = new Dictionary<int, BE.Permiso>();
            if (string.IsNullOrWhiteSpace(rol)) return new List<BE.Permiso>();

            List<BE.Componente> arbol = permisoDAL.ObtenerArbol();
            BE.Componente nodoRol = BuscarRol(arbol, rol, new HashSet<int>());

            if (nodoRol != null)
            {
                // La recursión vive en el Composite: el propio nodo-rol resuelve sus hojas
                // (patentes) atravesando familias y roles anidados. Acá sólo se mapea a BE.Permiso.
                foreach (var pat in nodoRol.ObtenerPatentesEfectivas())
                    if (!resultado.ContainsKey(pat.Id))
                        resultado[pat.Id] = new BE.Permiso
                        {
                            Id = pat.Id, Nombre = pat.Nombre, NombreMenu = pat.NombreMenu, Estado = true
                        };
            }
            else
            {
                // Fallback resiliente: BD sin migrar al Composite → asignación plana.
                foreach (var p in permisoDAL.ObtenerPorRol(rol))
                    if (!resultado.ContainsKey(p.Id)) resultado[p.Id] = p;
            }
            return new List<BE.Permiso>(resultado.Values);
        }

        // Subárbol del rol (BE.Rol con sus descendientes) para visualización en vivo.
        // Marca como Asignado=true toda Patente alcanzable (son permisos efectivos del rol).
        public BE.Familia ObtenerArbolPorRol(string rol)
        {
            if (string.IsNullOrWhiteSpace(rol))
                throw new ArgumentException("El rol no puede estar vacío.");

            List<BE.Componente> arbol = permisoDAL.ObtenerArbol();
            BE.Componente nodoRol = BuscarRol(arbol, rol, new HashSet<int>());

            if (nodoRol is BE.Familia fam)
            {
                MarcarTodasAsignadas(fam, new HashSet<int>());
                return fam;
            }

            // Rol sin nodo (BD sin migrar): construir desde asignación plana.
            var raiz = new BE.Rol { Id = 0, Nombre = rol };
            foreach (var p in permisoDAL.ObtenerPorRol(rol))
                raiz.AgregarHijo(new BE.Patente { Id = p.Id, Nombre = p.Nombre, NombreMenu = p.NombreMenu, Asignado = true });
            return raiz;
        }

        // Familias disponibles (compuestos que NO son roles) — para la Lista de Familias.
        public List<BE.Familia> ObtenerFamiliasDisponibles()
        {
            var lista = new List<BE.Familia>();
            RecolectarPorTipo(permisoDAL.ObtenerArbol(), lista, null, new HashSet<int>());
            return lista;
        }

        // Patentes disponibles (permisos simples) — para la Lista de Patentes.
        public List<BE.Patente> ObtenerPatentesDisponibles()
        {
            var lista = new List<BE.Patente>();
            RecolectarPorTipo(permisoDAL.ObtenerArbol(), null, lista, new HashSet<int>());
            return lista;
        }

        // Ids de los componentes asignados DIRECTAMENTE al rol (hijos directos del nodo-rol).
        public HashSet<int> ObtenerIdsDirectosDelRol(string rol)
        {
            int idRol = permisoDAL.ObtenerIdRol(rol);
            var set = new HashSet<int>();
            if (idRol == 0) return set;
            foreach (int id in permisoDAL.ObtenerIdsHijos(idRol)) set.Add(id);
            return set;
        }

        // Guarda la asignación de componentes a un rol: compara los seleccionados con
        // los actuales y aplica altas/bajas de relaciones. Valida ciclos antes de agregar.
        // Devuelve (agregados, quitados).
        public (int Agregados, int Quitados) GuardarAsignacionRol(string rol, IEnumerable<int> idsSeleccionados)
        {
            VerificarPuedeGestionar();
            int idRol = permisoDAL.ObtenerIdRol(rol);
            if (idRol == 0)
                throw new BE.AppException("err.bll.rol_inexistente",
                    "El rol '{0}' no existe como nodo del árbol de permisos. Volvé a abrir la pantalla.", rol);

            var seleccion = new HashSet<int>(idsSeleccionados ?? new List<int>());
            var actuales  = ObtenerIdsDirectosDelRol(rol);

            // Anti-autobloqueo: un usuario NO administrador no puede quitarse a sí mismo el acceso de
            // gestión editando su PROPIO rol. (El Administrador tiene bypass y no se ve afectado.)
            if (!EsAdminEnSesion())
            {
                var u = Seguridad.SessionManager.GetInstance().Usuario;
                string rolPropio = u.Rol ?? u.Perfil;
                if (string.Equals(rol, rolPropio, StringComparison.OrdinalIgnoreCase) && !ConservaGestion(seleccion))
                    throw new BE.AppException("err.bll.familia.autobloqueo",
                        "No podés quitarte a vos mismo el acceso de Gestión de Usuarios de tu propio rol.");
            }

            // Guard sistémico: simular el rol con EXACTAMENTE los hijos seleccionados y verificar que
            // el sistema entero conserve al menos un usuario con acceso de gestión.
            ExigirSistemaConservaGestion(arbol =>
            {
                var rolNodo = BuscarPorId(arbol, idRol, new HashSet<int>()) as BE.Familia;
                if (rolNodo == null) return;
                rolNodo.VaciarHijos();
                foreach (int id in seleccion)
                {
                    var h = BuscarPorId(arbol, id, new HashSet<int>());
                    if (h != null) { try { rolNodo.AgregarHijo(h); } catch { } }
                }
            });

            int agregados = 0, quitados = 0;

            // Altas: en selección pero no actuales.
            foreach (int idHijo in seleccion)
            {
                if (actuales.Contains(idHijo)) continue;
                ValidarSinCiclo(idRol, idHijo);
                permisoDAL.AgregarRelacion(idRol, idHijo);
                agregados++;
            }
            // Bajas: actuales pero no en selección.
            foreach (int idHijo in actuales)
            {
                if (seleccion.Contains(idHijo)) continue;
                permisoDAL.QuitarRelacion(idRol, idHijo);
                quitados++;
            }

            _bitacora.Registrar("Gestión de Perfiles",
                $"Asignación del rol '{rol}' actualizada: +{agregados} / -{quitados}",
                BE.Criticidad.Alta);

            if (agregados > 0 || quitados > 0)
                GrabarVersionesUsuariosConRol(rol,
                    Seguridad.SessionManager.GetInstance().Usuario.Username,
                    $"Permisos del rol '{rol}' modificados: +{agregados} / -{quitados}");

            return (agregados, quitados);
        }

        // ── T04 — CRUD de componentes (Patentes / Familias / Roles) ─────────────

        [Obsolete("Los permisos (patentes) son un catálogo fijo del sistema; no se crean desde la app (revisión 11/06). Se conserva por compatibilidad.")]
        public int CrearPatente(string nombre, string nombreMenu)
        {
            VerificarPuedeGestionar();
            ValidarNombre(nombre);
            int id = permisoDAL.AltaComponente(nombre, nombreMenu, esFamilia: false, esRol: false, tipoComponente: null);
            _bitacora.Registrar("Gestión de Perfiles", $"Patente creada: '{nombre}'", BE.Criticidad.Alta);
            return id;
        }

        [Obsolete("Las Familias se retiraron del modelo (revisión 11/06): el árbol es Rol→Patente. Se conserva por compatibilidad.")]
        public int CrearFamilia(string nombre)
        {
            VerificarPuedeGestionar();
            ValidarNombre(nombre);
            int id = permisoDAL.AltaComponente(nombre, null, esFamilia: true, esRol: false, tipoComponente: "Familia");
            _bitacora.Registrar("Gestión de Perfiles", $"Familia creada: '{nombre}'", BE.Criticidad.Alta);
            return id;
        }

        public int CrearRol(string nombre)
        {
            VerificarPuedeGestionar();
            ValidarNombre(nombre);
            if (permisoDAL.ObtenerIdRol(nombre) != 0)
                throw new BE.AppException("err.bll.rol_duplicado", "Ya existe un rol llamado '{0}'.", nombre);
            int id = permisoDAL.AltaComponente(nombre, null, esFamilia: true, esRol: true, tipoComponente: "Rol");
            _bitacora.Registrar("Gestión de Perfiles", $"Rol creado: '{nombre}'", BE.Criticidad.Alta);
            return id;
        }

        public void RenombrarComponente(int idPermiso, string nombre, string nombreMenu)
        {
            VerificarPuedeGestionar();
            ValidarNombre(nombre);
            permisoDAL.ModificarComponente(idPermiso, nombre, nombreMenu);
            _bitacora.Registrar("Gestión de Perfiles", $"Componente {idPermiso} modificado a '{nombre}'", BE.Criticidad.Media);
        }

        // Embebe un componente hijo dentro de un nodo padre (familia o rol), validando ciclos.
        public void AgregarComponente(int idPadre, int idHijo)
        {
            VerificarPuedeGestionar();
            if (idPadre == idHijo)
                throw new BE.AppException("err.bll.ciclo", "Un componente no puede contenerse a sí mismo.");
            ValidarSinCiclo(idPadre, idHijo);
            permisoDAL.AgregarRelacion(idPadre, idHijo);
            _bitacora.Registrar("Gestión de Perfiles", $"Relación creada {idPadre}→{idHijo}", BE.Criticidad.Alta);
        }

        public void QuitarComponente(int idPadre, int idHijo)
        {
            VerificarPuedeGestionar();
            // Guard sistémico: quitar esta relación NO puede dejar al sistema sin acceso de gestión
            // (cubre quitar la patente de gestión de un rol, o sacar un rol-de-gestión embebido en otro).
            ExigirSistemaConservaGestion(arbol =>
            {
                var padre = BuscarPorId(arbol, idPadre, new HashSet<int>()) as BE.Familia;
                var hijo  = BuscarPorId(arbol, idHijo,  new HashSet<int>());
                if (padre != null && hijo != null) padre.QuitarHijo(hijo);
            });
            permisoDAL.QuitarRelacion(idPadre, idHijo);
            _bitacora.Registrar("Gestión de Perfiles", $"Relación quitada {idPadre}→{idHijo}", BE.Criticidad.Alta);
        }

        // Elimina (baja lógica) un rol. NO se permite si hay usuarios que lo tienen asignado.
        public void EliminarRol(string rol)
        {
            VerificarPuedeGestionar();
            int idRol = permisoDAL.ObtenerIdRol(rol);
            if (idRol == 0)
                throw new BE.AppException("err.bll.rol_inexistente", "El rol '{0}' no existe.", rol);

            int usuarios = permisoDAL.ContarUsuariosPorRol(rol);
            if (usuarios > 0)
            {
                var nombres = permisoDAL.ObtenerUsuariosPorRol(rol);
                throw new BE.AppException("err.bll.rol_en_uso",
                    "No se puede eliminar el rol '{0}': está asignado a {1} usuario(s) ({2}). " +
                    "Reasigná esos usuarios a otro rol antes de eliminarlo.",
                    rol, usuarios, string.Join(", ", nombres));
            }

            // Guard sistémico: aunque el rol no tenga usuarios DIRECTOS, puede estar embebido en otros
            // roles (rol-dentro-de-rol) que SÍ tienen usuarios. Simular su eliminación (desanclarlo de
            // todos sus padres y de las raíces) y verificar que el sistema conserve la gestión.
            ExigirSistemaConservaGestion(arbol =>
            {
                var objetivo = BuscarPorId(arbol, idRol, new HashSet<int>());
                if (objetivo == null) return;
                foreach (var fam in TodasLasFamilias(arbol)) fam.QuitarHijo(objetivo);
                for (int i = arbol.Count - 1; i >= 0; i--)
                    if (arbol[i].Id == idRol) arbol.RemoveAt(i);
            });

            permisoDAL.BajaComponente(idRol);
            _bitacora.Registrar("Gestión de Perfiles", $"Rol eliminado: '{rol}'", BE.Criticidad.Alta);
        }

        // Baja lógica de un componente que NO es rol (patente o familia).
        public void EliminarComponente(int idPermiso)
        {
            VerificarPuedeGestionar();
            permisoDAL.BajaComponente(idPermiso);
            _bitacora.Registrar("Gestión de Perfiles", $"Componente {idPermiso} eliminado", BE.Criticidad.Alta);
        }

        private void GrabarVersionesUsuariosConRol(string rol, string actor, string detalle)
        {
            try
            {
                var bllUsr = new Usuario();
                var bllVer = new VersionUsuario();
                foreach (var u in bllUsr.ObtenerTodos())
                    if (string.Equals(u.Perfil, rol, StringComparison.OrdinalIgnoreCase))
                        bllVer.GrabarVersion(u.Id, actor, detalle);
            }
            catch (Exception ex)
            {
                // El fallo de los snapshots no debe abortar el cambio de permisos,
                // pero sí debe quedar registrado para diagnóstico posterior.
                _bitacora.Registrar("Gestión de Perfiles",
                    $"Error al grabar snapshots de usuarios con rol '{rol}': {ex.Message}",
                    BE.Criticidad.Alta);
            }
        }

        // ── Helpers privados de recorrido recursivo ─────────────────────────────

        private static void ValidarNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new BE.AppException("err.bll.nombre_vacio", "El nombre no puede estar vacío.");
        }

        // Valida que agregar idHijo bajo idPadre no genere un ciclo (directo o indirecto).
        // Reutiliza la lógica anti-ciclos de BE.Familia.AgregarHijo sobre el árbol real.
        private void ValidarSinCiclo(int idPadre, int idHijo)
        {
            List<BE.Componente> arbol = permisoDAL.ObtenerArbol();
            var padre = BuscarPorId(arbol, idPadre, new HashSet<int>()) as BE.Familia;
            var hijo  = BuscarPorId(arbol, idHijo,  new HashSet<int>());
            if (padre == null)
                throw new BE.AppException("err.bll.padre_invalido",
                    "El nodo padre no admite hijos (no es Familia ni Rol).");
            if (hijo == null) return; // hijo nuevo aún no en el árbol → no puede haber ciclo
            // AgregarHijo lanza InvalidOperationException si detecta referencia circular.
            try { padre.AgregarHijo(hijo); }
            catch (InvalidOperationException ex)
            {
                throw new BE.AppException("err.bll.ciclo", ex.Message);
            }
        }

        private static BE.Rol BuscarRol(IList<BE.Componente> nodos, string nombre, HashSet<int> visit)
        {
            foreach (var nodo in nodos)
            {
                if (nodo.Id != 0 && !visit.Add(nodo.Id)) continue;
                if (nodo is BE.Rol rol &&
                    string.Equals(rol.Nombre, nombre, StringComparison.OrdinalIgnoreCase))
                    return rol;
                if (nodo is BE.Familia fam)
                {
                    var encontrado = BuscarRol(fam.Hijos, nombre, visit);
                    if (encontrado != null) return encontrado;
                }
            }
            return null;
        }

        private static BE.Componente BuscarPorId(IList<BE.Componente> nodos, int id, HashSet<int> visit)
        {
            foreach (var nodo in nodos)
            {
                if (nodo.Id == id) return nodo;
                if (nodo.Id != 0 && !visit.Add(nodo.Id)) continue;
                if (nodo is BE.Familia fam)
                {
                    var encontrado = BuscarPorId(fam.Hijos, id, visit);
                    if (encontrado != null) return encontrado;
                }
            }
            return null;
        }

        // Recolecta familias (no-rol) y/o patentes del árbol, según las listas provistas.
        private static void RecolectarPorTipo(IList<BE.Componente> nodos,
            List<BE.Familia> familias, List<BE.Patente> patentes, HashSet<int> visit)
        {
            foreach (var nodo in nodos)
            {
                if (nodo.Id != 0 && !visit.Add(nodo.Id)) continue;
                if (nodo is BE.Patente pat)
                {
                    if (patentes != null && !patentes.Exists(x => x.Id == pat.Id)) patentes.Add(pat);
                }
                else if (nodo is BE.Familia fam)
                {
                    if (!(nodo is BE.Rol) && familias != null && !familias.Exists(x => x.Id == fam.Id))
                        familias.Add(fam);
                    RecolectarPorTipo(fam.Hijos, familias, patentes, visit);
                }
            }
        }

        private static void MarcarTodasAsignadas(BE.Familia fam, HashSet<int> visit)
        {
            if (fam.Id != 0 && !visit.Add(fam.Id)) return;
            foreach (var hijo in fam.Hijos)
            {
                if (hijo is BE.Patente pat) pat.Asignado = true;
                else if (hijo is BE.Familia sub) MarcarTodasAsignadas(sub, visit);
            }
        }

    }
}
