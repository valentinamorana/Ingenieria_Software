using System;
using System.Drawing;
using System.Windows.Forms;
using Servicios.Multiidioma;

namespace GUI
{
    /// <summary>
    /// T07 — Consola de Recuperación de Integridad asistida por el ESPEJO (Usuario_Seguridad).
    ///
    /// Muestra el diagnóstico a nivel de CAMPO (qué cambió respecto del último estado legítimo) y
    /// ofrece tres opciones, habilitadas según el tipo de daño (inspirado en el proyecto Agus):
    ///   • Reparar desde Espejo  — restaura los valores legítimos guardados en el espejo.
    ///   • Asumir Pérdida        — acepta los datos actuales y recalcula todos los DV.
    ///   • Restaurar Backup      — cuando el espejo no alcanza (vaciado/eliminación física).
    ///
    /// Es la ÚNICA vía de recuperación del sistema. Se abre desde el Diagnóstico de Integridad
    /// (admin logueado) y desde el arranque (Program), cuando un Administrador autenticado entra
    /// con la base comprometida. Cada acción se confirma con ConfirmarAdminForm.
    /// </summary>
    public class RecuperacionEspejoForm : Form
    {
        private DataGridView _grid;
        private Label  _lblEstado;
        private TextBox _txtResumen;
        private Button _btnReparar;
        private Button _btnAsumir;
        private Button _btnBackup;
        private Button _btnCerrar;

        // True si se ejecutó una recuperación con éxito (el llamador puede reiniciar/refrescar).
        public bool RecuperadoExitosamente { get; private set; }

        public RecuperacionEspejoForm()
        {
            BuildUI();
        }

        private static string T(string key, string fallback)
        {
            var t = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
            return t.ContainsKey(key) ? t[key].Texto : fallback;
        }

        private void BuildUI()
        {
            this.Text          = T("rec.frm.titulo", "Recuperación de Integridad (Espejo)");
            this.Size          = new Size(860, 600);
            this.MinimumSize   = new Size(720, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font          = new Font("Segoe UI", 9f);

            var panelEstado = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = Color.FromArgb(40, 40, 55), Padding = new Padding(16, 0, 16, 0) };
            _lblEstado = new Label { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 12f, FontStyle.Bold), ForeColor = Color.White, TextAlign = ContentAlignment.MiddleLeft };
            panelEstado.Controls.Add(_lblEstado);

            _txtResumen = new TextBox
            {
                Dock = DockStyle.Top, Height = 110, Multiline = true, ReadOnly = true,
                ScrollBars = ScrollBars.Vertical, BackColor = Color.FromArgb(248, 244, 250),
                BorderStyle = BorderStyle.FixedSingle, Font = new Font("Consolas", 8.5f),
                ForeColor = Color.FromArgb(60, 30, 50)
            };

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 9f)
            };
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "cId",       HeaderText = T("rec.col.id",       "ID"),       FillWeight = 7 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "cUsuario",  HeaderText = T("rec.col.usuario",  "Usuario"),  FillWeight = 18 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "cTipo",     HeaderText = T("rec.col.tipo",     "Tipo"),     FillWeight = 18 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "cCampo",    HeaderText = T("rec.col.campo",    "Campo"),    FillWeight = 17 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "cActual",   HeaderText = T("rec.col.actual",   "Actual"),   FillWeight = 20 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "cEsperado", HeaderText = T("rec.col.esperado", "Esperado (espejo)"), FillWeight = 20 });

            var panelBotones = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom, Height = 52, FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(8, 8, 8, 8), BackColor = Color.FromArgb(245, 245, 250)
            };

            _btnCerrar = MkButton(T("rec.btn.cerrar", "Cerrar"), Color.FromArgb(120, 120, 135), 90);
            _btnCerrar.Click += (s, e) => this.Close();

            _btnBackup = MkButton(T("rec.btn.backup", "Restaurar Backup..."), Color.FromArgb(210, 100, 135), 160);
            _btnBackup.Click += BtnBackup_Click;

            _btnAsumir = MkButton(T("rec.btn.asumir", "Asumir Pérdida"), Color.FromArgb(180, 120, 60), 140);
            _btnAsumir.Click += BtnAsumir_Click;

            _btnReparar = MkButton(T("rec.btn.reparar", "Reparar desde Espejo"), Color.FromArgb(80, 150, 90), 170);
            _btnReparar.Click += BtnReparar_Click;

            panelBotones.Controls.AddRange(new Control[] { _btnCerrar, _btnBackup, _btnAsumir, _btnReparar });

            var contenedor = new Panel { Dock = DockStyle.Fill };
            contenedor.Controls.Add(_grid);
            contenedor.Controls.Add(_txtResumen);

            this.Controls.Add(contenedor);
            this.Controls.Add(panelBotones);
            this.Controls.Add(panelEstado);
        }

        private Button MkButton(string text, Color back, int width)
        {
            var b = new Button { Text = text, Width = width, Height = 34, FlatStyle = FlatStyle.Flat, BackColor = back, ForeColor = Color.White };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try { string ico = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico"); if (System.IO.File.Exists(ico)) this.Icon = new Icon(ico); } catch { }
            CargarDiagnostico();
        }

        private void CargarDiagnostico()
        {
            _grid.Rows.Clear();
            try
            {
                var d = BLL.RecuperacionIntegridad.Diagnosticar();

                _lblEstado.Text = d.Integro
                    ? T("rec.estado.integro", "Estado: ÍNTEGRO")
                    : T("rec.estado.comprometido", "Estado: COMPROMETIDO");

                _txtResumen.Text = string.Join("\r\n", d.Resumen.ToArray());
                if (d.DvvAlmacenado != null || d.DvvCalculado != 0)
                    _txtResumen.Text += "\r\n" + string.Format(
                        T("rec.dvv.detalle", "DVV almacenado: {0}  |  DVV calculado: {1}"),
                        d.DvvAlmacenado?.ToString() ?? "—", d.DvvCalculado);

                string tMod = T("rec.tipo.modificada",   "Modificada");
                string tDvh = T("rec.tipo.dvh",          "DVH corrupto");
                string tIns = T("rec.tipo.insertada",    "Inserción externa");
                string tDel = T("rec.tipo.eliminada",    "Eliminada (falta)");

                foreach (var a in d.Alteradas)
                {
                    string tipo = a.Tipo == BLL.TipoAlteracion.Modificada ? tMod
                                : a.Tipo == BLL.TipoAlteracion.DVHCorrupto ? tDvh : tIns;
                    if (a.Campos != null && a.Campos.Count > 0)
                        foreach (var c in a.Campos)
                            _grid.Rows.Add(a.Id, a.Username, tipo, c.Campo, c.ValorActual, c.ValorEsperado);
                    else
                        _grid.Rows.Add(a.Id, a.Username, tipo, "—", a.Descripcion ?? "", "—");
                }
                foreach (var f in d.Faltantes)
                    _grid.Rows.Add(f.Id, f.Username, tDel, "—",
                        T("rec.fila.ausente", "(ausente en la tabla actual)"), T("rec.fila.enespejo", "(presente en el espejo)"));

                _btnReparar.Enabled = d.PuedeReparar;
                _btnAsumir.Enabled  = d.PuedeAsumirPerdida;
                _btnBackup.Enabled  = d.PuedeRestaurarBackup;

                // Pista visual: si todo está íntegro, deshabilitar las acciones correctivas.
                if (d.Integro)
                {
                    _btnReparar.Enabled = false;
                    _btnAsumir.Enabled  = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(T("rec.err.cargar", "Error al diagnosticar: {0}"), ex.Message),
                    T("rec.err.titulo", "Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ConfirmarAdmin()
        {
            using (var admin = new ConfirmarAdminForm())
                return admin.ShowDialog(this) == DialogResult.OK && admin.Autorizado;
        }

        private void BtnReparar_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(
                    T("rec.conf.reparar", "¿Restaurar la tabla Usuario a los valores legítimos del espejo?\n\nSe revertirán las filas modificadas y se eliminarán las inserciones externas."),
                    T("rec.conf.reparar.titulo", "Confirmar Reparación"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            if (!ConfirmarAdmin()) return;

            try
            {
                BLL.RecuperacionIntegridad.RepararDesdeEspejo();
                RecuperadoExitosamente = true;
                MessageBox.Show(T("rec.msg.reparado", "Saneamiento completado: se restauraron los datos legítimos del espejo."),
                    T("rec.msg.exito.titulo", "Éxito"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarDiagnostico();
            }
            catch (Exception ex) { MostrarError(ex); }
        }

        private void BtnAsumir_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(
                    T("rec.conf.asumir", "ATENCIÓN: se aceptarán los datos ACTUALES como legítimos y se recalcularán todos los dígitos verificadores.\n\nSi hubo manipulación, quedará consolidada. ¿Continuar?"),
                    T("rec.conf.asumir.titulo", "Confirmar Asumir Pérdida"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.Yes) return;
            if (!ConfirmarAdmin()) return;

            try
            {
                BLL.RecuperacionIntegridad.AsumirPerdida();
                RecuperadoExitosamente = true;
                MessageBox.Show(T("rec.msg.asumido", "Dígitos verificadores recalculados sobre los datos actuales."),
                    T("rec.msg.exito.titulo", "Éxito"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarDiagnostico();
            }
            catch (Exception ex) { MostrarError(ex); }
        }

        private void BtnBackup_Click(object sender, EventArgs e)
        {
            if (!ConfirmarAdmin()) return;

            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Copias de Seguridad (*.wfbak;*.bak)|*.wfbak;*.bak";
                ofd.Title  = T("rec.backup.seleccionar", "Seleccionar Backup para Restaurar");
                if (ofd.ShowDialog() != DialogResult.OK) return;

                // RF-08 — Informar el ALCANCE de la pérdida (fecha del backup + registros actuales
                // posteriores que se perderían al sobrescribir) antes de confirmar la restauración.
                string alcance = ConstruirAlcancePerdida(ofd.FileName);

                if (MessageBox.Show(
                        T("conf.rest.sobreescribir", "¿Está seguro? Esta operación sobrescribirá todos los datos actuales y reiniciará la aplicación.") + alcance,
                        T("msg.backup.titulorestaura", "Confirmar Restauración"),
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

                string clave = null;
                if (BLL.Backup.EsCifrado(ofd.FileName))
                {
                    using (var dlg = new InputDialog(
                        T("dlg.backup.clave.titulo", "Contraseña del backup"),
                        T("dlg.backup.clave.ingresar", "Ingresá la contraseña con la que se cifró este backup:"),
                        esPassword: true))
                    {
                        if (dlg.ShowDialog(this) != DialogResult.OK) return;
                        clave = dlg.InputText;
                    }
                }

                try
                {
                    new BLL.Backup().RestaurarBackup("RecuperacionEspejo", ofd.FileName, clave);
                    MessageBox.Show(T("msg.backup.restauradaexito", "Base de datos restaurada con éxito.\nLa aplicación se reiniciará."),
                        T("rpt.dlg.exito.titulo", "Éxito"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    RecuperadoExitosamente = true;
                    Application.Restart();
                }
                catch (Exception ex) { MostrarError(ex); }
            }
        }

        // RF-08 — Arma el aviso de alcance de la pérdida: fecha del backup + desglose de los
        // registros de la base actual posteriores a esa fecha (los que se perderían). Tolerante a
        // fallos: si no puede leerse la fecha o contar, devuelve el aviso genérico o vacío.
        private string ConstruirAlcancePerdida(string ruta)
        {
            try
            {
                var bll = new BLL.Backup();
                DateTime? fecha = bll.ObtenerFechaBackup(ruta);
                if (!fecha.HasValue)
                    return T("msg.backup.alcance.desconocido",
                        "\n\nNo se pudo determinar la fecha del backup. Se perderán todos los cambios posteriores a su creación.");

                var sb = new System.Text.StringBuilder();
                sb.Append(string.Format(
                    T("msg.backup.alcance",
                      "\n\nEl backup es del {0} (hace {1} día(s)).\nSe PERDERÁN todos los cambios posteriores a esa fecha."),
                    fecha.Value.ToString("dd/MM/yyyy HH:mm"),
                    Math.Max(0, (int)(DateTime.Now - fecha.Value).TotalDays)));

                var cambios = bll.ObtenerCambiosDesde(fecha);
                if (cambios == null || cambios.Count == 0)
                    sb.Append(T("msg.backup.sinperdida",
                        "\n\nNo hay registros nuevos posteriores a esa fecha: no se perdería información reciente."));
                else
                {
                    sb.Append(T("msg.backup.perdida.titulo",
                        "\n\nSe perderán estos registros creados después del backup:"));
                    foreach (var c in cambios)
                        sb.Append($"\n  • {c.Entidad}: {c.Cantidad}");
                }
                return sb.ToString();
            }
            catch { return string.Empty; }
        }

        private void MostrarError(Exception ex)
        {
            MessageBox.Show(T("rec.err.titulo", "Error") + ":\n" + ex.Message,
                T("rec.err.titulo", "Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
