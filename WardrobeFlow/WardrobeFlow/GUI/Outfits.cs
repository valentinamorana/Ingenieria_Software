using System.Drawing;
using System.Windows.Forms;

namespace GUI
{
    /// <summary>
    /// Capa de Presentación — Módulo de Outfits.
    /// Accesible para Administrador y OperadorLogístico (permiso mnuOutfits).
    /// </summary>
    public partial class Outfits : Form
    {
        public Outfits()
        {
            InitializeComponent();
            this.Text       = "Outfits";
            this.ClientSize = new Size(800, 500);

            var lbl = new Label
            {
                Text      = "Módulo Outfits — en desarrollo",
                Font      = new Font("Segoe UI", 14, FontStyle.Regular),
                ForeColor = Color.Gray,
                AutoSize  = false,
                Dock      = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lbl);
        }
    }
}
