using BLL;
using Servicios.Multiidioma;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GUI
{
    public partial class RestauracionForm : Form, IIdiomaObserver
    {
        private readonly BLL.ResultadoIntegridad _resultado;
        private readonly Dictionary<string, Button> _btnsIdioma = new Dictionary<string, Button>();

        public bool RestauradoExitosamente { get; private set; }

        // Constructor legacy (string) — mantiene compatibilidad si algo lo llama directamente
        public RestauracionForm(string detalle) : this(new BLL.ResultadoIntegridad
            { HayDvhInvalido = true, FilasCorruptas = new List<string> { detalle } }) { }

        public RestauracionForm(BLL.ResultadoIntegridad resultado)
        {
            InitializeComponent();
            _resultado = resultado;
            RestauradoExitosamente = false;
        }

        private void RestauracionForm_Load(object sender, EventArgs e)
        {
            try { string ico = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico"); if (System.IO.File.Exists(ico)) this.Icon = new System.Drawing.Icon(ico); } catch { }
            GestorIdioma.SuscribirObservador(this);
            AgregarBotonesIdioma();
            Traducir(GestorIdioma.IdiomaActual);
        }

        private void AgregarBotonesIdioma()
        {
            IList<Idioma> idiomas;
            try { idiomas = new BLL.IdiomaService().ObtenerIdiomasActivosComoIdioma(); }
            catch { idiomas = Traductor.ObtenerIdiomas(); }

            // Botones en la esquina superior-derecha del header (y=8, no superpone el subtítulo)
            int btnW = 40, gap = 6;
            int x = pnlHeader.Width - 16;
            foreach (var idioma in idiomas)
            {
                x -= (btnW + gap);
                string cod = idioma.Id;
                var btn = new Button
                {
                    Text      = cod,
                    Size      = new Size(btnW, 22),
                    Location  = new Point(x, 8),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(176, 136, 152),
                    Font      = new Font("Segoe UI", 8f),
                    Cursor    = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize  = 1;
                btn.FlatAppearance.BorderColor = Color.FromArgb(224, 200, 216);
                btn.Click += (s, ev) =>
                {
                    try
                    {
                        var dict = new BLL.IdiomaService().CargarTraducciones(cod);
                        GestorIdioma.CambiarIdioma(idioma, dict);
                    }
                    catch { GestorIdioma.CambiarIdioma(idioma); }
                };
                pnlHeader.Controls.Add(btn);
                btn.BringToFront();
                _btnsIdioma[cod] = btn;
            }

            // Limitar ancho del subtítulo para que no llegue hasta los botones
            int areaBoton = idiomas.Count * (btnW + gap) + 20;
            lblSubtitulo.MaximumSize = new Size(pnlHeader.Width - areaBoton - 24, 0);

            MarcarActivo(GestorIdioma.IdiomaActual?.Id ?? "ES");
        }

        private void MarcarActivo(string cod)
        {
            foreach (var kv in _btnsIdioma)
            {
                bool activo = kv.Key == cod;
                kv.Value.Font      = new Font("Segoe UI", 8f, activo ? FontStyle.Bold : FontStyle.Regular);
                kv.Value.ForeColor = activo ? Color.FromArgb(146, 62, 96) : Color.FromArgb(176, 136, 152);
                kv.Value.BackColor = activo ? Color.FromArgb(243, 234, 240) : Color.White;
                kv.Value.FlatAppearance.BorderColor = activo
                    ? Color.FromArgb(201, 160, 186)
                    : Color.FromArgb(224, 200, 216);
            }
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

            this.Text               = T("frm.restauracion",   "Integridad del Sistema");
            lblTitulo.Text          = T("lbl.rest.titulo",    "Integridad del Sistema Comprometida");
            lblSubtitulo.Text       = T("lbl.rest.subtitulo", "Se detectaron discrepancias en los dígitos verificadores. El acceso está bloqueado.");
            lblDetalle.Text         = T("lbl.rest.detalle",   "Detalle del error:");
            btnRecalcular.Text      = T("btn.rest.recalcular","Recalcular Dígitos Verificadores");
            btnRestaurarBackup.Text = T("btn.rest.backup",    "Restaurar desde Backup");
            btnSalir.Text           = T("btn.rest.salir",     "Salir");

            // Regenerar el mensaje de detalle en el idioma activo
            txtDetalle.Text = ConstruirMensaje(t);
            MarcarActivo(idioma?.Id ?? "ES");
        }

        private string ConstruirMensaje(IDictionary<string, Traduccion> t)
        {
            if (_resultado == null) return string.Empty;
            string T(string k, string fb) => t.ContainsKey(k) ? t[k].Texto : fb;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine(T("dv.alerta.titulo", "ALERTA DE INTEGRIDAD — Tabla Usuario"));
            sb.AppendLine(new string('─', 50));
            sb.AppendLine();

            if (_resultado.HayDvhInvalido && _resultado.FilasCorruptas.Count > 0)
            {
                sb.AppendLine(string.Format(T("dv.dvh.intro", "Se detectaron {0} fila(s) con DVH inválido:"), _resultado.FilasCorruptas.Count));
                foreach (var f in _resultado.FilasCorruptas)
                    sb.AppendLine($"  • {T("lbl.confirmaradmin.usuario", "Usuario")} {f}");
                sb.AppendLine();
            }

            if (_resultado.HayDvvInvalido)
            {
                sb.AppendLine(T("dv.dvv.intro", "El DVV de la tabla no coincide con el valor almacenado."));
                sb.AppendLine(string.Format(T("dv.dvv.valores", "  Almacenado: {0}  |  Calculado: {1}"),
                    _resultado.DvvAlmacenado?.ToString() ?? "—", _resultado.DvvCalculado));
                sb.AppendLine();
            }

            sb.AppendLine(T("dv.causas", "Posibles causas: modificación directa en la base de datos,\nrestauración parcial de backup o error en la migración."));
            sb.AppendLine();
            sb.AppendLine(T("dv.pasos.titulo", "Para restaurar la integridad, un Administrador debe:"));
            sb.AppendLine(T("dv.pasos.1", "  1. Revisar los registros alterados en SQL Server."));
            sb.AppendLine(T("dv.pasos.2", "  2. Corregir los valores afectados manualmente."));
            sb.AppendLine(T("dv.pasos.3", "  3. Ejecutar el recálculo de DVH/DVV desde Administrar → Usuarios."));
            return sb.ToString();
        }

        private void btnRecalcular_Click(object sender, EventArgs e)
        {
            using (var admin = new ConfirmarAdminForm())
            {
                if (admin.ShowDialog(this) != DialogResult.OK || !admin.Autorizado) return;

                try
                {
                    Configuracion.RecalcularIntegridadDV();
                    var tR = Servicios.Multiidioma.Traductor.ObtenerTraducciones(Servicios.Multiidioma.GestorIdioma.IdiomaActual);
                    string TR(string k, string fb) => tR.ContainsKey(k) ? tR[k].Texto : fb;
                    MessageBox.Show(
                        TR("msg.rest.dvexito",  "Dígitos verificadores recalculados con éxito.\nYa puede ingresar al sistema."),
                        TR("msg.rest.dvtitulo", "Integridad restaurada"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    RestauradoExitosamente = true;
                    this.Close();
                }
                catch (Exception ex)
                {
                    var tE = Servicios.Multiidioma.Traductor.ObtenerTraducciones(Servicios.Multiidioma.GestorIdioma.IdiomaActual);
                    string titulo = tE.ContainsKey("msg.error.titulo") ? tE["msg.error.titulo"].Texto : "Error";
                    string fmt    = tE.ContainsKey("msg.rest.errorrecalcular") ? tE["msg.rest.errorrecalcular"].Texto : "Error al recalcular: {0}";
                    MessageBox.Show(string.Format(fmt, ex.Message), titulo, MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                    var tC = Servicios.Multiidioma.Traductor.ObtenerTraducciones(Servicios.Multiidioma.GestorIdioma.IdiomaActual);
                    string TC(string k, string fb) => tC.ContainsKey(k) ? tC[k].Texto : fb;
                    if (MessageBox.Show(
                        TC("conf.rest.sobreescribir", "¿Está seguro? Esta operación sobrescribirá todos los datos actuales y reiniciará la aplicación."),
                        TC("msg.backup.titulorestaura", "Confirmar Restauración"),
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
                        var tEx = Servicios.Multiidioma.Traductor.ObtenerTraducciones(Servicios.Multiidioma.GestorIdioma.IdiomaActual);
                        string titulo2 = tEx.ContainsKey("msg.error.titulo") ? tEx["msg.error.titulo"].Texto : "Error";
                        string fmt2    = tEx.ContainsKey("msg.rest.errorrestaurar") ? tEx["msg.rest.errorrestaurar"].Texto : "Error al restaurar: {0}";
                        MessageBox.Show(string.Format(fmt2, ex.Message), titulo2, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
