using System;
using System.Collections.Generic;
using System.Drawing;
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
        private ComboBox cmbOpciones;
        private Label    lblMensaje;

        public CambioEstadoDialog(BE.Prenda prenda,
            List<(string texto, BE.EstadoPrenda estado)> opciones)
        {
            InitializeComponent();
            _opciones = opciones;

            this.Text            = "Cambiar Estado de Prenda";
            this.ClientSize      = new Size(360, 180);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.StartPosition   = FormStartPosition.CenterParent;

            ConstruirInterfaz(prenda);
        }

        private void ConstruirInterfaz(BE.Prenda prenda)
        {
            this.Controls.Add(new Label
            {
                Text = $"Prenda: {prenda.Nombre}  —  Estado actual: {prenda.Estado}",
                Left = 16, Top = 16, Width = 330,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            });

            this.Controls.Add(new Label
            {
                Text = "Nuevo estado:", Left = 16, Top = 50, Width = 120
            });

            cmbOpciones = new ComboBox
            {
                Left = 16, Top = 68, Width = 330,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            foreach (var op in _opciones)
                cmbOpciones.Items.Add(op.texto);
            cmbOpciones.SelectedIndex = 0;
            this.Controls.Add(cmbOpciones);

            lblMensaje = new Label
            {
                Left = 16, Top = 102, Width = 330, Height = 24,
                ForeColor = Color.DarkRed, Font = new Font("Segoe UI", 8.5f)
            };
            this.Controls.Add(lblMensaje);

            var btnConfirmar = new Button
            {
                Text = "Confirmar Cambio",
                Left = 16, Top = 130, Width = 160, Height = 32,
                BackColor = Color.SteelBlue, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnConfirmar.FlatAppearance.BorderSize = 0;
            btnConfirmar.Click += (s, e) =>
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
            };

            var btnCancelar = new Button
            {
                Text = "Cancelar",
                Left = 184, Top = 130, Width = 100, Height = 32,
                FlatStyle = FlatStyle.Flat
            };
            btnCancelar.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.Add(btnConfirmar);
            this.Controls.Add(btnCancelar);
        }
    }
}
