using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace GUI
{
    /// <summary>
    /// Aplica las preferencias de UI del usuario en sesión (fuente, tamaño, tema) a los
    /// formularios. Mantiene en memoria la preferencia activa (cargada al ingresar) y la
    /// re-aplica en vivo cuando cambia desde "Mi Perfil".
    ///
    /// Tema CLARO: solo cambia la tipografía (respeta el diseño/colores originales).
    /// Tema OSCURO: además oscurece fondos y aclara textos de los controles estándar.
    /// </summary>
    internal static class PreferenciasUI
    {
        public static BE.Preferencia Actual { get; private set; } = new BE.Preferencia();

        // Punto de referencia (pt) del tamaño "Normal": la escala se calcula relativa a esto.
        private const float TamanoBase = 9f;

        // Recuerda la fuente de DISEÑO de cada control la primera vez que se lo toca. Así el tamaño
        // se escala SIEMPRE desde el original (no se va acumulando en cada re-aplicación) y se
        // preserva la jerarquía tipográfica (un título grande sigue siendo proporcionalmente grande).
        private static readonly ConditionalWeakTable<Control, Font> _fuenteOriginal =
            new ConditionalWeakTable<Control, Font>();

        // Carga la preferencia del usuario desde BD (defaults si no hay / tabla sin migrar).
        public static void Cargar(int idUsuario)
        {
            try { Actual = new BLL.Preferencia().Obtener(idUsuario) ?? new BE.Preferencia(); }
            catch { Actual = new BE.Preferencia(); }
        }

        public static void Set(BE.Preferencia p) { if (p != null) Actual = p; }

        // Formatea una fecha según el formato preferido del usuario.
        public static string Fecha(DateTime f)
        {
            string fmt = string.IsNullOrEmpty(Actual.FormatoFecha) ? "dd/MM/yyyy" : Actual.FormatoFecha;
            try { return f.ToString(fmt); } catch { return f.ToString("dd/MM/yyyy"); }
        }

        // Aplica fuente (y tema si es oscuro) a un formulario y todos sus controles.
        public static void Aplicar(Control root)
        {
            if (root == null) return;
            string familia = string.IsNullOrEmpty(Actual.FuenteFamilia) ? "Segoe UI" : Actual.FuenteFamilia;
            float  tam     = Actual.TamanoEnPuntos();
            bool   oscuro  = Actual.EsOscuro;
            AplicarRec(root, familia, tam, oscuro, esRaiz: true);
        }

        private static void AplicarRec(Control c, string familia, float tam, bool oscuro, bool esRaiz)
        {
            // Escalar la fuente DESDE el tamaño de diseño original de cada control, no a un tamaño
            // absoluto único (eso aplanaba toda la tipografía y rompía los layouts). Se conserva
            // el estilo (negrita/itálica) y la proporción entre títulos y textos.
            Font original = _fuenteOriginal.GetValue(c, ctrl => ctrl.Font);
            float factor  = tam / TamanoBase;
            try { c.Font = new Font(familia, original.Size * factor, original.Style); } catch { }

            if (oscuro)
            {
                if (esRaiz || c is Form || c is Panel || c is GroupBox || c is TabPage)
                    c.BackColor = Color.FromArgb(37, 37, 45);
                if (c is Label || c is CheckBox || c is RadioButton || c is LinkLabel)
                    c.ForeColor = Color.Gainsboro;
                if (c is TextBox || c is ComboBox || c is ListBox || c is ListView)
                {
                    c.BackColor = Color.FromArgb(50, 50, 60);
                    c.ForeColor = Color.Gainsboro;
                }
                if (c is DataGridView grid)
                {
                    grid.BackgroundColor = Color.FromArgb(45, 45, 55);
                    grid.DefaultCellStyle.BackColor = Color.FromArgb(50, 50, 60);
                    grid.DefaultCellStyle.ForeColor = Color.Gainsboro;
                    grid.EnableHeadersVisualStyles = false;
                    grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(60, 60, 72);
                    grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.Gainsboro;
                }
            }

            foreach (Control hijo in c.Controls)
                AplicarRec(hijo, familia, tam, oscuro, esRaiz: false);
        }

        // Re-aplica a TODOS los formularios abiertos (tras guardar en Mi Perfil).
        public static void ReaplicarTodo()
        {
            foreach (Form f in Application.OpenForms)
                Aplicar(f);
        }
    }
}
