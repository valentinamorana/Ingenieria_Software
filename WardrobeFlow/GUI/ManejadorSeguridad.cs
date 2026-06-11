using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace GUI
{
    /// <summary>
    /// Etapa 4 — Aplica la seguridad a nivel de CONTROL (patrón ManejadorSeguridad de Stach).
    /// Lee los mapeos [ControlMapeado] del formulario y muestra/oculta cada control según las
    /// patentes efectivas del usuario en sesión.
    ///
    /// Decisiones adoptadas: A1 (aditivo — convive con la visibilidad de menú por NombreMenu),
    /// B1 (el Administrador ve todo: bypass), C1 (la lista de controles se arma en runtime).
    ///
    /// Vive en la capa GUI porque manipula controles WinForms; evita acoplar Servicios a WinForms.
    /// </summary>
    internal static class ManejadorSeguridad
    {
        private static readonly BLL.ControlMapeado _bll = new BLL.ControlMapeado();

        // Caché en memoria de TODOS los mapeos (se recarga sólo cuando cambian, vía InvalidarCache).
        // Evita leer la tabla completa en cada OnLoad de formulario.
        private static List<BE.ControlMapeado> _cacheMapeos;

        // Controles que ESTE manejador ocultó (clave "Form.Control"), para poder volver a mostrarlos
        // si la patente cambia o el control deja de estar mapeado (no pisar controles ajenos).
        private static readonly HashSet<string> _ocultadosPorNosotros =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // El Menú principal NO se gobierna por control: tiene su propia lógica por NombreMenu
        // (Menu.AplicarPermisos). Se excluye acá para no aplicar dos mecanismos sobre los mismos ítems.
        private const string FormularioMenu = "Menu";

        private static List<BE.ControlMapeado> Mapeos()
        {
            if (_cacheMapeos == null) _cacheMapeos = _bll.ObtenerTodos();
            return _cacheMapeos;
        }

        /// <summary>Invalida la caché de mapeos (llamar tras editar el mapeo de controles).</summary>
        public static void InvalidarCache() { _cacheMapeos = null; }

        /// <summary>Aplica la seguridad de permisos a los controles mapeados de un formulario.</summary>
        public static void AplicarSeguridad(Form form, BE.Usuario usuario)
        {
            if (form == null || usuario == null) return;
            // El Menú tiene su propio gateo por NombreMenu; no se le aplican mapeos de control.
            if (string.Equals(form.Name, FormularioMenu, StringComparison.OrdinalIgnoreCase)) return;
            try
            {
                List<BE.ControlMapeado> mapeos = Mapeos()
                    .Where(c => string.Equals(c.Formulario, form.Name, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                // B1 — el Administrador ve todo.
                bool esAdmin = usuario.EsAdministrador;

                var patentesUsuario = new HashSet<int>();
                if (usuario.Permisos != null)
                    foreach (var p in usuario.Permisos) patentesUsuario.Add(p.Id);

                var mapeadosAhora = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var m in mapeos)
                {
                    object control = EncontrarControl(form, m.NombreControl);
                    if (control == null) continue;

                    string key = form.Name + "." + m.NombreControl;
                    mapeadosAhora.Add(key);

                    bool tieneAcceso = esAdmin || patentesUsuario.Contains(m.IdPermiso);
                    SetVisible(control, tieneAcceso);
                    if (tieneAcceso) _ocultadosPorNosotros.Remove(key);
                    else             _ocultadosPorNosotros.Add(key);
                }

                // Re-mostrar lo que NOSOTROS habíamos ocultado en este form y ya no está mapeado
                // (p. ej. se quitó el mapeo): así no queda algo oculto para siempre.
                string prefijo = form.Name + ".";
                foreach (string key in _ocultadosPorNosotros.ToList())
                {
                    if (!key.StartsWith(prefijo, StringComparison.OrdinalIgnoreCase)) continue;
                    if (mapeadosAhora.Contains(key)) continue;
                    string ctrlName = key.Substring(prefijo.Length);
                    object control = EncontrarControl(form, ctrlName);
                    if (control != null) SetVisible(control, true);
                    _ocultadosPorNosotros.Remove(key);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[ManejadorSeguridad.AplicarSeguridad] " + ex.Message);
            }
        }

        /// <summary>Re-aplica la seguridad a TODOS los formularios abiertos (tras un cambio de permisos).</summary>
        public static void ActualizarSeguridadFormulariosAbiertos(BE.Usuario usuario)
        {
            if (usuario == null) return;
            foreach (Form f in Application.OpenForms.Cast<Form>().ToList())
                AplicarSeguridad(f, usuario);
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static void SetVisible(object control, bool visible)
        {
            if (control is Control c)               c.Visible = visible;
            else if (control is ToolStripItem tsi)  tsi.Visible = visible;
        }

        // Busca un control por nombre: primero entre los Controls (recursivo), luego dentro de los
        // ToolStrip (menús) y sus submenús.
        private static object EncontrarControl(Form form, string nombre)
        {
            Control[] m = form.Controls.Find(nombre, true);
            if (m.Length > 0) return m[0];

            foreach (Control c in form.Controls)
                if (c is ToolStrip ts)
                {
                    object found = BuscarEnToolStrip(ts.Items, nombre);
                    if (found != null) return found;
                }
            return null;
        }

        private static object BuscarEnToolStrip(ToolStripItemCollection items, string nombre)
        {
            foreach (ToolStripItem item in items)
            {
                if (string.Equals(item.Name, nombre, StringComparison.OrdinalIgnoreCase)) return item;
                if (item is ToolStripDropDownItem dd)
                {
                    object found = BuscarEnToolStrip(dd.DropDownItems, nombre);
                    if (found != null) return found;
                }
            }
            return null;
        }
    }
}
