using Servicios.Multiidioma;
using System;
using System.IO;
using System.Windows.Forms;

namespace GUI
{
    public partial class BackupForm : Form, IIdiomaObserver
    {
        private readonly BLL.Backup _bll = new BLL.Backup();

        // RF-06 — botón de backup de instalación limpia (creado dinámicamente)
        private Button _btnInicial;

        private static readonly string DirBackups =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");

        public BackupForm()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try { string ico = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico"); if (System.IO.File.Exists(ico)) this.Icon = new System.Drawing.Icon(ico); } catch { }
            GestorIdioma.SuscribirObservador(this);

            // RF-06 — Botón "Backup de instalación limpia". Se ubica debajo de la fila de
            // restauración y se agranda el formulario para no solapar la nota informativa.
            this.ClientSize = new System.Drawing.Size(this.ClientSize.Width, this.ClientSize.Height + 44);
            lblInfo.Top += 44;
            _btnInicial = new Button
            {
                Tag       = "btn.backup.inicial",
                Text      = "Backup de instalación limpia",
                Size      = new System.Drawing.Size(btnCrear.Width, 32),
                Location  = new System.Drawing.Point(btnCrear.Left, btnExterno.Bottom + 8),
                FlatStyle = btnCrear.FlatStyle,
                BackColor = System.Drawing.Color.FromArgb(70, 110, 90),
                ForeColor = System.Drawing.Color.White,
                Font      = btnCrear.Font,
                Cursor    = Cursors.Hand
            };
            _btnInicial.FlatAppearance.BorderSize = 0;
            _btnInicial.Click += btnInicial_Click;
            (btnCrear.Parent ?? this).Controls.Add(_btnInicial);
            _btnInicial.BringToFront();

            Traducir(GestorIdioma.IdiomaActual);
            lblRuta.Text = DirBackups;
            CargarLista();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            GestorIdioma.DesuscribirObservador(this);
            base.OnFormClosing(e);
        }

        public void UpdateLanguage(Idioma idioma)
        {
            Traducir(idioma);
            CargarLista();
        }

        private string T(string key, string fallback)
        {
            var t = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
            return t.ContainsKey(key) ? t[key].Texto : fallback;
        }

        private void Traducir(Idioma idioma)
        {
            this.Text         = T("frm.backup",           "Backup y Restauración");
            lblTitulo.Text    = T("frm.backup",           "Backup y Restauración");
            lblRutaLabel.Text = T("lbl.backup.ubicacion", "Ubicación de copias:");
            btnCrear.Text     = T("btn.backup.crear",     "Generar Copia de Seguridad");
            btnRestaurar.Text = T("btn.backup.restaurar", "Restaurar seleccionado");
            btnEliminar.Text  = T("btn.backup.eliminar",  "Eliminar");
            btnExterno.Text   = T("btn.backup.externo",   "Desde archivo...");
            lblInfo.Text      = T("lbl.backup.info",      "Nota: la restauración cierra las conexiones activas y reinicia la aplicación.");
            colArchivo.Text   = T("col.backup.archivo",   "Archivo");
            colFecha.Text     = T("col.backup.fecha",     "Fecha");
            colAutor.Text     = T("col.backup.autor",     "Autor");
            colTamanio.Text   = T("col.backup.tamanio",   "Tamaño");
            if (_btnInicial != null)
                _btnInicial.Text = T("btn.backup.inicial", "Backup de instalación limpia");
        }

        // Carga los .bak de la carpeta Backups/ ordenados por fecha descendente (más reciente primero).
        private void CargarLista()
        {
            lstBackups.Items.Clear();
            btnRestaurar.Enabled = false;
            btnEliminar.Enabled  = false;

            if (!Directory.Exists(DirBackups))
            {
                lblConteo.Text = T("lbl.backup.sincopias", "Sin copias de seguridad generadas aún.");
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
                item.SubItems.Add(_bll.ExtraerAutorDeNombre(fi.Name));
                item.SubItems.Add(tamanio);
                lstBackups.Items.Add(item);
            }

            lblConteo.Text = archivos.Length == 0
                ? T("lbl.backup.sincopias", "Sin copias de seguridad generadas aún.")
                : string.Format(T("lbl.backup.conteo", "{0} copia(s) disponible(s). La más reciente: {1}"),
                    archivos.Length, archivos[0].LastWriteTime.ToString("dd/MM/yyyy HH:mm"));
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
                    string.Format(T("msg.backup.creadoexito", "Copia de seguridad generada con éxito:\n{0}"), filename),
                    T("rpt.dlg.exito.titulo", "Éxito"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarLista();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(T("msg.backup.errorgenerar", "Error al generar copia de seguridad:\n{0}"), ex.Message),
                    T("msg.error.titulo", "Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnInicial_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Directory.Exists(DirBackups))
                    Directory.CreateDirectory(DirBackups);

                string filename = _bll.RealizarBackupInicial(this.Text, DirBackups);
                MessageBox.Show(
                    string.Format(T("msg.backup.inicialexito", "Backup de instalación limpia generado:\n{0}"), filename),
                    T("rpt.dlg.exito.titulo", "Éxito"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarLista();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(T("msg.backup.errorgenerar", "Error al generar copia de seguridad:\n{0}"), ex.Message),
                    T("msg.error.titulo", "Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    string.Format(T("msg.backup.confirmeliminar",
                        "¿Eliminar la copia de seguridad?\n\"{0}\"\n\nEsta acción no se puede deshacer."), filename),
                    T("msg.backup.tituloeliminar", "Confirmar Eliminación"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                _bll.EliminarBackup(this.Text, ruta);
                CargarLista();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(T("msg.backup.erroreliminar", "Error al eliminar:\n{0}"), ex.Message),
                    T("msg.error.titulo", "Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            // RF-08 — Informar el ALCANCE de la pérdida: todo lo creado/modificado después de la
            // fecha del backup se perderá. Se lee la fecha real del header del .bak.
            string alcance;
            DateTime? fechaBackup = null;
            try { fechaBackup = _bll.ObtenerFechaBackup(ruta); } catch { /* header ilegible */ }
            if (fechaBackup.HasValue)
            {
                var antiguedad = DateTime.Now - fechaBackup.Value;
                alcance = string.Format(
                    T("msg.backup.alcance",
                      "\n\nEl backup es del {0} (hace {1} día(s)).\nSe PERDERÁN todos los cambios posteriores a esa fecha."),
                    fechaBackup.Value.ToString("dd/MM/yyyy HH:mm"),
                    Math.Max(0, (int)antiguedad.TotalDays));
            }
            else
            {
                alcance = T("msg.backup.alcance.desconocido",
                    "\n\nNo se pudo determinar la fecha del backup. Se perderán todos los cambios posteriores a su creación.");
            }

            string msg = string.Format(
                T("msg.backup.confirmrestaura",
                    "¿Restaurar la base de datos desde:\n\"{0}\"?\n\nEsta operación sobrescribirá todos los datos actuales\ny reiniciará la aplicación."),
                Path.GetFileName(ruta)) + alcance;

            if (MessageBox.Show(msg, T("msg.backup.titulorestaura", "Confirmar Restauración"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                _bll.RestaurarBackup(this.Text, ruta);
                MessageBox.Show(
                    T("msg.backup.restauradaexito", "Base de datos restaurada con éxito.\nLa aplicación se reiniciará."),
                    T("msg.backup.restauradatitulo", "Restauración Exitosa"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                Application.Restart();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(T("msg.backup.errorrestaurar", "Error al restaurar:\n{0}"), ex.Message),
                    T("msg.error.titulo", "Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
