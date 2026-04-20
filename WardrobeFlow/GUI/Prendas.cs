using System.Drawing;
using System.Windows.Forms;

namespace GUI
{
    /// <summary>
    /// Capa de Presentación — Módulo de Prendas.
    /// Accesible para Administrador y OperadorLogístico (permiso mnuPrendas).
    /// </summary>
    public partial class Prendas : Form
    {
        public Prendas()
        {
            InitializeComponent();
            this.Text       = "Prendas";
            this.ClientSize = new Size(800, 500);

            var lbl = new Label
            {
                Text      = "Módulo Prendas — en desarrollo",
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
