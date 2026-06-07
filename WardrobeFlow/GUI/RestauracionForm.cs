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
        // Selector de idioma como dropdown (reemplaza los botones). Soporta idiomas dinámicos de BD.
        private ComboBox _cmbIdioma;
        private bool _suprimirIdiomaChange = false;

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
            AgregarComboIdioma();
            Traducir(GestorIdioma.IdiomaActual);
        }

        private void AgregarComboIdioma()
        {
            IList<Idioma> idiomas;
            try { idiomas = new BLL.IdiomaService().ObtenerIdiomasActivosComoIdioma(); }
            catch { idiomas = Traductor.ObtenerIdiomas(); }

            // Combo en la esquina superior-derecha del header.
            int w = 130;
            _cmbIdioma = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Size          = new Size(w, 22),
                Location      = new Point(pnlHeader.Width - w - 16, 8),
                Anchor        = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle     = FlatStyle.Flat,
                Font          = new Font("Segoe UI", 8.5f),
                ForeColor     = Color.FromArgb(146, 62, 96),
                BackColor     = Color.White,
                DisplayMember = "Nombre",
                ValueMember   = "Id"
            };
            _suprimirIdiomaChange = true;
            _cmbIdioma.DataSource = new List<Idioma>(idiomas);
            _suprimirIdiomaChange = false;
            _cmbIdioma.SelectedIndexChanged += CmbIdioma_Changed;
            pnlHeader.Controls.Add(_cmbIdioma);
            _cmbIdioma.BringToFront();

            // El subtítulo va DEBAJO del combo, así que puede usar el ancho completo del header
            // (antes se le restaba el ancho del combo y el texto se cortaba).
            lblSubtitulo.Size = new Size(pnlHeader.Width - 40, lblSubtitulo.Height);

            SeleccionarIdioma(GestorIdioma.IdiomaActual?.Id ?? "ES");
        }

        // Selecciona en el combo el idioma indicado SIN disparar el cambio (uso interno).
        private void SeleccionarIdioma(string cod)
        {
            if (_cmbIdioma?.Items == null) return;
            for (int i = 0; i < _cmbIdioma.Items.Count; i++)
                if (_cmbIdioma.Items[i] is Idioma idm &&
                    string.Equals(idm.Id, cod, StringComparison.OrdinalIgnoreCase))
                {
                    if (_cmbIdioma.SelectedIndex != i)
                    {
                        bool prev = _suprimirIdiomaChange;
                        _suprimirIdiomaChange = true;
                        _cmbIdioma.SelectedIndex = i;
                        _suprimirIdiomaChange = prev;
                    }
                    break;
                }
        }

        private void CmbIdioma_Changed(object sender, EventArgs e)
        {
            if (_suprimirIdiomaChange) return;
            var idioma = _cmbIdioma.SelectedItem as Idioma;
            if (idioma == null) return;
            try
            {
                var dict = new BLL.IdiomaService().CargarTraducciones(idioma.Id);
                GestorIdioma.CambiarIdioma(idioma, dict);
            }
            catch { GestorIdioma.CambiarIdioma(idioma); }
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
            btnRecalcular.Text      = T("btn.rest.recalcular","Recalcular Dígitos");
            btnRestaurarBackup.Text = T("btn.rest.backup",    "Restaurar desde Backup");
            btnSalir.Text           = T("btn.rest.salir",     "Salir");

            // Regenerar el mensaje de detalle en el idioma activo
            txtDetalle.Text = ConstruirMensaje(t);
            SeleccionarIdioma(idioma?.Id ?? "ES");
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
