using BLL;
using Servicios.Multiidioma;
using System;
using System.Windows.Forms;

namespace GUI
{
    public partial class RestauracionForm : Form, IIdiomaObserver
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
            try { string ico = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico"); if (System.IO.File.Exists(ico)) this.Icon = new System.Drawing.Icon(ico); } catch { }
            GestorIdioma.SuscribirObservador(this);
            Traducir(GestorIdioma.IdiomaActual);
            txtDetalle.Text = _detalle;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            GestorIdioma.DesuscribirObservador(this);
            base.OnFormClosing(e);
        }

        public void UpdateLanguage(Idioma idioma) => Traducir(idioma);

        private void Traducir(Idioma idioma)
        {
            var t = Servicios.Multiidioma.Traductor.ObtenerTraducciones(idioma);
            string T(string key, string fb) => t.ContainsKey(key) ? t[key].Texto : fb;

            this.Text              = T("frm.restauracion",   "Integridad del Sistema");
            lblTitulo.Text         = T("lbl.rest.titulo",    "Integridad del Sistema Comprometida");
            lblSubtitulo.Text      = T("lbl.rest.subtitulo", "Se detectaron discrepancias en los dígitos verificadores. El acceso está bloqueado.");
            lblDetalle.Text        = T("lbl.rest.detalle",   "Detalle del error:");
            btnRecalcular.Text     = T("btn.rest.recalcular","Recalcular Dígitos Verificadores");
            btnRestaurarBackup.Text= T("btn.rest.backup",    "Restaurar desde Backup");
            btnSalir.Text          = T("btn.rest.salir",     "Salir");
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
                        var t2 = Servicios.Multiidioma.Traductor.ObtenerTraducciones(Servicios.Multiidioma.GestorIdioma.IdiomaActual);
                        string T2(string k, string fb) => t2.ContainsKey(k) ? t2[k].Texto : fb;
                        MessageBox.Show(T2("msg.backup.restauradaexito", "Base de datos restaurada con éxito.\nLa aplicación se reiniciará."),
                            T2("rpt.dlg.exito.titulo", "Éxito"), MessageBoxButtons.OK, MessageBoxIcon.Information);
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
