using System.Drawing;
using System.Windows.Forms;

namespace GUI
{
    /// <summary>
    /// Capa de Presentación — Módulo de Categorías.
    /// Accesible para Administrador y OperadorLogístico (permiso mnuCategorias).
    /// </summary>
    public partial class Categorias : Form
    {
        public Categorias()
        {
            InitializeComponent();
            this.Text       = "Categorias";
            this.ClientSize = new Size(800, 500);

            var lbl = new Label
            {
                Text      = "Módulo Categorias — en desarrollo",
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
