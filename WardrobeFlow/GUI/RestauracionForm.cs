using BLL;
using System;
using System.Windows.Forms;

namespace GUI
{
    public partial class RestauracionForm : Form
    {
        private readonly string _detalle;

        public bool RestauradoExitosamente { get; private set; }

        public RestauracionForm(string detalle)
        {
            InitializeComponent();
            _detalle = detalle;
            RestauradoExitosamente = false;
        }

        private void RestauracionForm_Load(object sender, EventArgs e)
        {
            txtDetalle.Text = _detalle;
        }

        private void btnRecalcular_Click(object sender, EventArgs e)
        {
            using (var admin = new ConfirmarAdminForm())
            {
                if (admin.ShowDialog(this) != DialogResult.OK || !admin.Autorizado) return;

                try
                {
                    Configuracion.RecalcularIntegridadDV();
                    MessageBox.Show(
                        "Dígitos verificadores recalculados con éxito.\nYa puede ingresar al sistema.",
                        "Integridad restaurada",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    RestauradoExitosamente = true;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al recalcular: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnRestaurarBackup_Click(object sender, EventArgs e)
        {
            using (var admin = new ConfirmarAdminForm())
            {
                if (admin.ShowDialog(this) != DialogResult.OK || !admin.Autorizado) return;

                using (var ofd = new OpenFileDialog())
                {
                    ofd.Filter = "Copia de Seguridad SQL (*.bak)|*.bak";
                    ofd.Title  = "Seleccionar Backup para Restaurar";

                    if (ofd.ShowDialog() != DialogResult.OK) return;

                    if (MessageBox.Show(
                        "¿Está seguro? Esta operación sobrescribirá todos los datos actuales y reiniciará la aplicación.",
                        "Confirmar Restauración",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning) != DialogResult.Yes) return;

                    try
                    {
                        new Backup().RestaurarBackup("RestauracionIntegridad", ofd.FileName);
                        MessageBox.Show("Base de datos restaurada. La aplicación se reiniciará.",
                            "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Application.Restart();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al restaurar: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
