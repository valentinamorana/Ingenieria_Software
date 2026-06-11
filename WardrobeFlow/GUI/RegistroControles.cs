using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace GUI
{
    /// <summary>
    /// Etapa 4 (opción C1) — Registro EN RUNTIME de los controles mapeables de cada formulario.
    /// A medida que se abren los formularios durante el uso normal, se anotan sus controles
    /// (botones e ítems de menú con nombre). La pantalla de mapeo ofrece esos formularios, sin
    /// instanciar nada por reflexión a ciegas (que en WardrobeFlow falla por los ctores con parámetros).
    /// </summary>
    internal static class RegistroControles
    {
        public class ControlInfo
        {
            public string Nombre { get; set; }
            public string Texto  { get; set; }
            public string Display => string.IsNullOrWhiteSpace(Texto) ? Nombre : $"{Nombre}  —  {Texto}";
        }

        // Form.Name → controles mapeables (por nombre).
        private static readonly Dictionary<string, Dictionary<string, ControlInfo>> _registro =
            new Dictionary<string, Dictionary<string, ControlInfo>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Anota los controles mapeables de un formulario (idempotente).</summary>
        public static void Registrar(Form form)
        {
            if (form == null || string.IsNullOrEmpty(form.Name)) return;
            try
            {
                if (!_registro.TryGetValue(form.Name, out var ctrls))
                {
                    ctrls = new Dictionary<string, ControlInfo>(StringComparer.OrdinalIgnoreCase);
                    _registro[form.Name] = ctrls;
                }
                RecorrerControles(form.Controls, ctrls);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[RegistroControles.Registrar] " + ex.Message);
            }
        }

        /// <summary>Formularios registrados hasta ahora (ordenados).</summary>
        public static List<string> Formularios() => _registro.Keys.OrderBy(k => k).ToList();

        /// <summary>Controles mapeables de un formulario registrado.</summary>
        public static List<ControlInfo> Controles(string formulario)
        {
            if (formulario != null && _registro.TryGetValue(formulario, out var ctrls))
                return ctrls.Values.OrderBy(c => c.Nombre).ToList();
            return new List<ControlInfo>();
        }

        // Recolecta Botones (recursivo) e ítems de ToolStrip/menús. Son los controles que tiene
        // sentido proteger por permiso (acciones), igual que en el PermisosForm de Stach.
        private static void RecorrerControles(Control.ControlCollection controls, Dictionary<string, ControlInfo> acc)
        {
            foreach (Control c in controls)
            {
                if (c is Button && !string.IsNullOrEmpty(c.Name))
                    acc[c.Name] = new ControlInfo { Nombre = c.Name, Texto = c.Text };
                else if (c is ToolStrip ts)
                    RecorrerToolStrip(ts.Items, acc);

                if (c.Controls.Count > 0) RecorrerControles(c.Controls, acc);
            }
        }

        private static void RecorrerToolStrip(ToolStripItemCollection items, Dictionary<string, ControlInfo> acc)
        {
            foreach (ToolStripItem item in items)
            {
                if (!string.IsNullOrEmpty(item.Name))
                    acc[item.Name] = new ControlInfo { Nombre = item.Name, Texto = item.Text };
                if (item is ToolStripDropDownItem dd)
                    RecorrerToolStrip(dd.DropDownItems, acc);
            }
        }
    }
}
