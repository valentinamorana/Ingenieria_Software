using System.Drawing;
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
                MessageBox.Show($"Error: {msg}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            MensajeLabel.ForeColor = Color.DarkRed;
            MensajeLabel.Text      = $"✗ {msg}";
        }
    }
}
