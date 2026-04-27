using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace GUI
{
    /// <summary>
    /// Diálogo simple para seleccionar el nuevo estado de una prenda.
    /// Recibe las opciones válidas desde Prendas.cs y devuelve el estado elegido.
    /// </summary>
    public partial class CambioEstadoDialog : Form
    {
        public BE.EstadoPrenda EstadoSeleccionado { get; private set; }

        private readonly List<(string texto, BE.EstadoPrenda estado)> _opciones;

        public CambioEstadoDialog(BE.Prenda prenda,
            List<(string texto, BE.EstadoPrenda estado)> opciones)
        {
            InitializeComponent();
            _opciones = opciones;

            lblPrendaInfo.Text = $"Prenda: {prenda.Nombre}  —  Estado actual: {prenda.Estado}";

            foreach (var op in _opciones)
                cmbOpciones.Items.Add(op.texto);
            if (cmbOpciones.Items.Count > 0)
                cmbOpciones.SelectedIndex = 0;
        }

        private void BtnConfirmar_Click(object sender, EventArgs e)
        {
            int idx = cmbOpciones.SelectedIndex;
            if (idx < 0) { lblMensaje.Text = "Seleccioná una opción."; return; }

            EstadoSeleccionado = _opciones[idx].estado;

            // Confirmación extra para Baja (irreversible)
            if (EstadoSeleccionado == BE.EstadoPrenda.Baja)
            {
                var conf = MessageBox.Show(
                    "La baja es irreversible. ¿Confirmar?",
                    "Dar de Baja", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if (conf != DialogResult.Yes) return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
