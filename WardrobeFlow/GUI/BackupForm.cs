using Servicios.Multiidioma;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace GUI
{
    public partial class BackupForm : Form, IIdiomaObserver
    {
        private readonly BLL.Backup _bll = new BLL.Backup();

        private static readonly string DirBackups =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");

        public BackupForm()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GestorIdioma.SuscribirObservador(this);
            Traducir(GestorIdioma.IdiomaActual);
            lblRuta.Text = DirBackups;
            CargarLista();
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

            this.Text         = T("frm.backup",           "Backup y Restauración");
            lblTitulo.Text    = T("frm.backup",           "Backup y Restauración");
            btnCrear.Text     = T("btn.backup.crear",     "Generar Copia de Seguridad");
            btnRestaurar.Text = T("btn.backup.restaurar", "Restaurar seleccionado");
            btnEliminar.Text  = T("btn.backup.eliminar",  "Eliminar");
            btnExterno.Text   = T("btn.backup.externo",   "Desde archivo...");
            lblInfo.Text      = T("lbl.backup.info",      "Nota: la restauración cierra las conexiones activas y reinicia la aplicación.");
            colArchivo.Text   = T("col.backup.archivo",   "Archivo");
            colFecha.Text     = T("col.backup.fecha",     "Fecha");
            colAutor.Text     = T("col.backup.autor",     "Autor");
            colTamanio.Text   = T("col.backup.tamanio",   "Tamaño");
        }

        // Carga los .bak de la carpeta Backups/ ordenados por fecha descendente (más reciente primero).
        private void CargarLista()
        {
            lstBackups.Items.Clear();
            btnRestaurar.Enabled = false;
            btnEliminar.Enabled  = false;

            if (!Directory.Exists(DirBackups))
            {
                lblConteo.Text = "Sin copias de seguridad generadas aún.";
                return;
            }

            var archivos = new DirectoryInfo(DirBackups).GetFiles("*.bak");
            Array.Sort(archivos, (a, b) => b.LastWriteTime.CompareTo(a.LastWriteTime));

            foreach (var fi in archivos)
            {
                string tamanio = fi.Length >= 1_048_576
                    ? $"{fi.Length / 1_048_576.0:F1} MB"
                    : $"{fi.Length / 1024.0:F0} KB";

                var item = new ListViewItem(fi.Name) { Tag = fi.FullName };
                item.SubItems.Add(fi.LastWriteTime.ToString("dd/MM/yyyy HH:mm"));
                item.SubItems.Add(ExtraerAutor(fi.Name));
                item.SubItems.Add(tamanio);
                lstBackups.Items.Add(item);
            }

            lblConteo.Text = archivos.Length == 0
                ? "Sin copias de seguridad generadas aún."
                : $"{archivos.Length} copia(s) disponible(s). La más reciente: {archivos[0].LastWriteTime:dd/MM/yyyy HH:mm}";
        }

        // Extrae el autor del nombre del archivo.
        // Formato nuevo: WardrobeFlow_Backup_yyyyMMddHHmmss_USUARIO.bak
        // Formato viejo (sin autor): muestra "—"
        private static string ExtraerAutor(string nombreArchivo)
        {
            const string prefix = "WardrobeFlow_Backup_";
            if (!nombreArchivo.StartsWith(prefix) || nombreArchivo.Length <= prefix.Length + 15)
                return "—";

            string rest = nombreArchivo.Substring(prefix.Length);
            bool isNewFormat = rest.Length >= 15
                && rest.Substring(0, 14).All(char.IsDigit)
                && rest[14] == '_';

            if (!isNewFormat) return "—";

            return Path.GetFileNameWithoutExtension(rest.Substring(15));
        }

        private void lstBackups_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool seleccionado    = lstBackups.SelectedItems.Count > 0;
            btnRestaurar.Enabled = seleccionado;
            btnEliminar.Enabled  = seleccionado;
        }

        private void btnCrear_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Directory.Exists(DirBackups))
                    Directory.CreateDirectory(DirBackups);

                string filename = _bll.RealizarBackup(this.Text, DirBackups);
                MessageBox.Show(
                    $"Copia de seguridad generada con éxito:\n{filename}",
                    "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarLista();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al generar copia de seguridad:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRestaurar_Click(object sender, EventArgs e)
        {
            if (lstBackups.SelectedItems.Count == 0) return;
            Restaurar(lstBackups.SelectedItems[0].Tag as string);
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (lstBackups.SelectedItems.Count == 0) return;

            string ruta     = lstBackups.SelectedItems[0].Tag as string;
            string filename = Path.GetFileName(ruta);

            if (MessageBox.Show(
                    $"¿Eliminar la copia de seguridad?\n\"{filename}\"\n\nEsta acción no se puede deshacer.",
                    "Confirmar Eliminación",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                _bll.EliminarBackup(this.Text, ruta);
                CargarLista();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Restaura un archivo elegido manualmente (útil para backups en USB u otra ubicación).
        private void btnExterno_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Copia de Seguridad SQL (*.bak)|*.bak";
                ofd.Title  = "Seleccionar Copia de Seguridad para Restaurar";
                if (Directory.Exists(DirBackups))
                    ofd.InitialDirectory = DirBackups;

                if (ofd.ShowDialog() != DialogResult.OK) return;
                Restaurar(ofd.FileName);
            }
        }

        private void Restaurar(string ruta)
        {
            if (string.IsNullOrEmpty(ruta)) return;

            string msg =
                $"¿Restaurar la base de datos desde:\n\"{Path.GetFileName(ruta)}\"?\n\n" +
                "Esta operación sobrescribirá todos los datos actuales\n" +
                "y reiniciará la aplicación.";

            if (MessageBox.Show(msg, "Confirmar Restauración",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                _bll.RestaurarBackup(this.Text, ruta);
                MessageBox.Show(
                    "Base de datos restaurada con éxito.\nLa aplicación se reiniciará.",
                    "Restauración Exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Application.Restart();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al restaurar:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
