using Servicios.Multiidioma;
using System;
using System.IO;
using System.Windows.Forms;

namespace GUI
{
    public partial class BackupForm : Form, IIdiomaObserver
    {
        private readonly BLL.Backup _bll = new BLL.Backup();

        public BackupForm()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GestorIdioma.SuscribirObservador(this);
            Traducir(GestorIdioma.IdiomaActual);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            GestorIdioma.DesuscribirObservador(this);
            base.OnFormClosing(e);
        }

        public void UpdateLanguage(Idioma idioma) => Traducir(idioma);

        private void Traducir(Idioma idioma)
        {
            var t = Traductor.ObtenerTraducciones(idioma);
            string T(string key, string fallback) => t.ContainsKey(key) ? t[key].Texto : fallback;

            this.Text       = T("frm.backup",           "Backup y Restauración");
            lblTitulo.Text  = T("frm.backup",           "Backup y Restauración");
            btnCrear.Text   = T("btn.backup.crear",     "Generar Copia de Seguridad (.bak)");
            btnRestore.Text = T("btn.backup.restaurar", "Restaurar Copia de Seguridad (.bak)");
            lblInfo.Text    = T("lbl.backup.info",      "Nota: la restauración cierra las conexiones activas y reinicia la aplicación.");
        }

        private void btnCrear_Click(object sender, EventArgs e)
        {
            try
            {
                string dirBackups = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");
                if (!Directory.Exists(dirBackups))
                    Directory.CreateDirectory(dirBackups);

                string filename = $"WardrobeFlow_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
                string fullPath = Path.Combine(dirBackups, filename);

                _bll.RealizarBackup(this.Text, fullPath);
                MessageBox.Show(
                    $"Copia de seguridad generada con éxito en:\n{fullPath}",
                    "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al generar copia de seguridad:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRestore_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Copia de Seguridad SQL (*.bak)|*.bak";
                ofd.Title  = "Seleccionar Copia de Seguridad para Restaurar";

                string dirBackups = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");
                if (Directory.Exists(dirBackups))
                    ofd.InitialDirectory = dirBackups;

                if (ofd.ShowDialog() != DialogResult.OK) return;

                string msg =
                    "¿Está seguro de restaurar la base de datos?\n\n" +
                    "Esta operación sobrescribirá todos los datos actuales\n" +
                    "y reiniciará la aplicación.";

                if (MessageBox.Show(msg, "Confirmar Restauración",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                    return;

                try
                {
                    _bll.RestaurarBackup(this.Text, ofd.FileName);
                    MessageBox.Show(
                        "Base de datos restaurada con éxito.\nLa aplicación se reiniciará.",
                        "Restauración Exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Application.Restart();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Error al restaurar la base de datos:\n{ex.Message}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
