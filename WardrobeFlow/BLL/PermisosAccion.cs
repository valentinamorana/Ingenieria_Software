using System;
using System.Collections.Generic;

namespace BLL
{
    /// <summary>
    /// Permisos granulares de ACCIÓN — separan "Ver" de "Configurar" (idea tomada de los
    /// proyectos Agus / tp-ing-soft, adaptada al modelo de patentes de WardrobeFlow).
    ///
    /// • VER un módulo  → patente de menú (ej. mnuClientes): muestra la pantalla y permite leer.
    /// • CONFIGURARLO   → patente de edición (ej. mnuClientesEditar): alta / modificación / baja.
    ///
    /// RETROCOMPATIBLE (decisión de despliegue seguro): si la patente de edición todavía NO
    /// existe en el catálogo (base sin migrar), se exige la patente de VER — exactamente el
    /// comportamiento anterior, así desplegar este código NO rompe nada. Una vez creada la
    /// patente de edición (script BD/03_Permisos_Granulares.sql) pasa a exigirse esa, y el
    /// Administrador puede quitarla a los roles que deban quedar de solo-lectura.
    /// El Administrador siempre tiene acceso (bypass por perfil).
    /// </summary>
    public static class PermisosAccion
    {
        // ── Decisión PURA (testeable sin BD ni sesión) ──────────────────────────────────
        // ¿El usuario puede ejecutar la acción de escritura?
        //   esAdmin        → bypass total.
        //   editarDefinida → la patente de edición existe en el catálogo (sistema migrado).
        //   tieneEditar    → el usuario posee la patente de edición.
        //   tieneVer       → el usuario posee la patente de ver (fallback legacy).
        public static bool PermiteAccion(bool esAdmin, bool editarDefinida, bool tieneEditar, bool tieneVer)
        {
            if (esAdmin) return true;
            return editarDefinida ? tieneEditar : tieneVer;
        }

        // ── Catálogo de patentes definidas (NombreMenu) — cacheado en memoria ───────────
        // Saber qué patentes de acción existen es lo que habilita el modo granular. El
        // catálogo es un conjunto fijo del sistema (no se crean patentes desde la app), así
        // que se lee una sola vez. Si no se puede leer, queda vacío → comportamiento legacy.
        private static HashSet<string> _catalogoLazy;
        private static readonly object _lock = new object();

        internal static ISet<string> Catalogo()
        {
            if (_catalogoLazy != null) return _catalogoLazy;
            lock (_lock)
            {
                if (_catalogoLazy == null)
                {
                    var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    try
                    {
                        foreach (var nm in new DAL.Permiso().ObtenerNombresMenuPatentes())
                            if (!string.IsNullOrEmpty(nm)) set.Add(nm);
                    }
                    catch { /* sin catálogo accesible → set vacío → fallback legacy */ }
                    _catalogoLazy = set;
                }
            }
            return _catalogoLazy;
        }

        /// <summary>Invalida la caché del catálogo (llamar tras crear/eliminar patentes).</summary>
        public static void InvalidarCache() { _catalogoLazy = null; }

        // ── Guard de negocio (fail-closed) ──────────────────────────────────────────────
        // Exige permiso para CONFIGURAR el módulo. 'patenteVer' es el fallback retrocompatible.
        public static void Exigir(string patenteEditar, string patenteVer)
        {
            if (!Seguridad.SessionManager.IsLoggedIn)
                throw new BE.AppException("err.bll.sesion_expirada",
                    "La sesión expiró. Volvé a iniciar sesión.");

            var sm = Seguridad.SessionManager.GetInstance();
            bool editarDefinida = Catalogo().Contains(patenteEditar);

            bool permite = PermiteAccion(
                sm.Usuario.EsAdministrador,
                editarDefinida,
                sm.TienePermiso(patenteEditar),
                sm.TienePermiso(patenteVer));

            if (!permite)
                throw new BE.AppException("err.bll.sin_permiso",
                    "No tiene permiso para ejecutar esta operación ('{0}').",
                    editarDefinida ? patenteEditar : patenteVer);
        }
    }
}
