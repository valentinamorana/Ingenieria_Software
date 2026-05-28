using Servicios.Multiidioma;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace GUI
{
    /// <summary>
    /// Formulario base de WardrobeFlow.
    ///
    /// PATRÓN HERENCIA (igual al ejemplo Vehículo/Auto/Moto de la cátedra):
    ///   Esta clase es como "Vehículo": define atributos y métodos comunes
    ///   que todos los formularios hijos heredan automáticamente.
    ///
    ///   Jerarquía:
    ///     FormBase : Form          ← como Vehiculo
    ///       ├── Clientes           ← como Auto
    ///       ├── Prendas            ← como Auto
    ///       ├── Planes             ← como Auto
    ///       ├── PedidosVenta       ← como Moto
    ///       ├── PedidosRealizados  ← como Moto
    ///       ├── NuevoPedidoForm    ← como Moto
    ///       └── Bitacora           ← como Moto
    ///
    /// MÉTODOS HEREDADOS (equivalentes a "acelerar()" en Vehículo):
    ///   MostrarOk(msg)    → feedback verde en el label del formulario
    ///   MostrarError(msg) → feedback rojo (o MessageBox si no hay label)
    ///
    /// PROPIEDAD VIRTUAL (equivalente a una propiedad sobreescribible):
    ///   MensajeLabel → cada hijo sobreescribe para devolver su lblMensaje.
    ///   Si no sobreescribe (como Bitacora), MostrarError usa MessageBox.
    /// </summary>
    public class FormBase : Form
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                string ico = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico");
                if (File.Exists(ico))
                    this.Icon = new Icon(ico);
            }
            catch { }
        }

        /// <summary>
        /// Label donde se muestra el feedback al usuario.
        /// Cada formulario hijo sobreescribe esta propiedad devolviendo
        /// su propio control lblMensaje declarado en el Designer.
        ///
        /// Ejemplo en cada hijo:
        ///   protected override Label MensajeLabel => lblMensaje;
        ///
        /// Si un formulario no tiene lblMensaje (como Bitácora),
        /// MostrarError usa MessageBox como fallback automático.
        /// </summary>
        protected virtual Label MensajeLabel => null;

        /// <summary>
        /// Muestra un mensaje de operación exitosa (✓ en verde).
        /// Heredado por todos los formularios hijos — no necesitan redefinirlo.
        /// </summary>
        protected void MostrarOk(string msg)
        {
            if (MensajeLabel == null) return;
            MensajeLabel.ForeColor = Color.DarkGreen;
            MensajeLabel.Text      = $"✓ {msg}";
        }

        /// <summary>
        /// Muestra un mensaje de error (✗ en rojo).
        /// Si el formulario no tiene lblMensaje, usa MessageBox como fallback.
        /// Heredado por todos los formularios hijos — no necesitan redefinirlo.
        /// </summary>
        protected void MostrarError(string msg)
        {
            if (MensajeLabel == null)
            {
                var t = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
                string titulo = t.ContainsKey("msg.error.titulo") ? t["msg.error.titulo"].Texto : "Error";
                MessageBox.Show(msg, titulo, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            MensajeLabel.ForeColor = Color.DarkRed;
            MensajeLabel.Text      = $"✗ {msg}";
        }

        /// <summary>
        /// Sobrecarga que traduce AppException al idioma activo antes de mostrar.
        /// Para otras excepciones muestra ex.Message directamente.
        /// </summary>
        protected void MostrarError(Exception ex)
        {
            if (ex is BE.AppException appEx)
            {
                var t = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
                if (t.ContainsKey(appEx.Clave))
                {
                    string texto = t[appEx.Clave].Texto;
                    string msg   = (appEx.Args != null && appEx.Args.Length > 0)
                                   ? string.Format(texto, appEx.Args)
                                   : texto;
                    MostrarError(msg);
                    return;
                }
            }
            MostrarError(ex.Message);
        }
    }
}
