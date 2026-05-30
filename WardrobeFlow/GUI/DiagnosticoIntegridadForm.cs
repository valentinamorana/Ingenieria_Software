using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Servicios.Multiidioma;

namespace GUI
{
    public class DiagnosticoIntegridadForm : Form, IIdiomaObserver
    {
        // ── Tab diagnóstico ───────────────────────────────────────────────────
        private Label       _lblEstadoDVV;
        private Label       _lblDVVDetalle;
        private DataGridView _gridRotas;
        private Button      _btnActualizar;
        private Button      _btnReparar;
        private Button      _btnRecalcularTodo;
        private Label       _lblFilasRotas;

        // ── Tab historial ─────────────────────────────────────────────────────
        private DataGridView _gridHistorial;
        private Button       _btnActualizarHist;

        // ── Control raíz ─────────────────────────────────────────────────────
        private TabControl _tabs;

        public DiagnosticoIntegridadForm()
        {
            BuildUI();
        }

        // Helper de traducción
        private string T(string key, string fallback)
        {
            var t = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
            return t.ContainsKey(key) ? t[key].Texto : fallback;
        }

        private void BuildUI()
        {
            this.Text            = T("diag.frm.titulo", "Diagnóstico de Integridad");
            this.Size            = new Size(820, 560);
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.MinimumSize     = new Size(700, 460);
            this.Font            = new Font("Segoe UI", 9f);

            _tabs = new TabControl { Dock = DockStyle.Fill };
            _tabs.TabPages.Add(BuildTabDiagnostico());
            _tabs.TabPages.Add(BuildTabHistorial());

            this.Controls.Add(_tabs);
        }

        private TabPage BuildTabDiagnostico()
        {
            var tab = new TabPage(T("diag.tab.diagnostico", "Diagnóstico"));

            var panelEstado = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 90,
                BackColor = Color.FromArgb(245, 245, 250),
                Padding   = new Padding(12, 8, 12, 8)
            };

            _lblEstadoDVV = new Label
            {
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                Location  = new Point(12, 10),
                AutoSize  = true
            };

            _lblDVVDetalle = new Label
            {
                Font     = new Font("Segoe UI", 9f),
                Location = new Point(12, 38),
                AutoSize = true,
                ForeColor = Color.FromArgb(80, 80, 100)
            };

            panelEstado.Controls.AddRange(new Control[] { _lblEstadoDVV, _lblDVVDetalle });

            _lblFilasRotas = new Label
            {
                Text     = T("diag.lbl.filasrotas", "Filas con DVH inválido:"),
                Font     = new Font("Segoe UI", 9f, FontStyle.Bold),
                Dock     = DockStyle.Top,
                Height   = 22,
                Padding  = new Padding(4, 2, 0, 0)
            };

            _gridRotas = new DataGridView
            {
                Dock                  = DockStyle.Fill,
                ReadOnly              = true,
                AllowUserToAddRows    = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible     = false,
                AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor       = Color.White,
                BorderStyle           = BorderStyle.None,
                Font                  = new Font("Segoe UI", 9f)
            };
            _gridRotas.Columns.Add(new DataGridViewTextBoxColumn { Name = "colId",       HeaderText = T("diag.col.id",       "ID"),              FillWeight = 8  });
            _gridRotas.Columns.Add(new DataGridViewTextBoxColumn { Name = "colUsuario",   HeaderText = T("diag.col.usuario",  "Usuario"),         FillWeight = 25 });
            _gridRotas.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDVHAlm",    HeaderText = T("diag.col.dvh.alm",  "DVH Almacenado"),  FillWeight = 20 });
            _gridRotas.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDVHCalc",   HeaderText = T("diag.col.dvh.calc", "DVH Calculado"),   FillWeight = 20 });
            _gridRotas.Columns.Add(new DataGridViewTextBoxColumn { Name = "colEstado",    HeaderText = T("diag.col.estado",   "Estado"),          FillWeight = 27 });

            var panelBotones = new FlowLayoutPanel
            {
                Dock          = DockStyle.Bottom,
                Height        = 44,
                FlowDirection = FlowDirection.RightToLeft,
                Padding       = new Padding(4),
                BackColor     = Color.FromArgb(245, 245, 250)
            };

            _btnRecalcularTodo = new Button
            {
                Text      = T("diag.btn.recalcular", "Recalcular Todo"),
                Width     = 130,
                Height    = 32,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(180, 80, 80),
                ForeColor = Color.White
            };
            _btnRecalcularTodo.FlatAppearance.BorderSize = 0;
            _btnRecalcularTodo.Click += BtnRecalcularTodo_Click;

            _btnReparar = new Button
            {
                Text      = T("diag.btn.reparar", "Reparar Seleccionadas..."),
                Width     = 170,
                Height    = 32,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(60, 120, 180),
                ForeColor = Color.White
            };
            _btnReparar.FlatAppearance.BorderSize = 0;
            _btnReparar.Click += BtnReparar_Click;

            _btnActualizar = new Button
            {
                Text      = T("diag.btn.actualizar", "Actualizar"),
                Width     = 100,
                Height    = 32,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(100, 160, 100),
                ForeColor = Color.White
            };
            _btnActualizar.FlatAppearance.BorderSize = 0;
            _btnActualizar.Click += (s, e) => CargarDiagnostico();

            panelBotones.Controls.AddRange(new Control[] { _btnRecalcularTodo, _btnReparar, _btnActualizar });

            var contenedor = new Panel { Dock = DockStyle.Fill };
            contenedor.Controls.Add(_gridRotas);
            contenedor.Controls.Add(_lblFilasRotas);

            tab.Controls.Add(contenedor);
            tab.Controls.Add(panelBotones);
            tab.Controls.Add(panelEstado);

            return tab;
        }

        private TabPage BuildTabHistorial()
        {
            var tab = new TabPage(T("diag.tab.historial", "Historial de Verificaciones"));

            _gridHistorial = new DataGridView
            {
                Dock                  = DockStyle.Fill,
                ReadOnly              = true,
                AllowUserToAddRows    = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible     = false,
                AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor       = Color.White,
                BorderStyle           = BorderStyle.None,
                Font                  = new Font("Segoe UI", 9f)
            };
            _gridHistorial.Columns.Add(new DataGridViewTextBoxColumn { Name = "hFecha",    HeaderText = T("diag.col.fecha",      "Fecha"),            FillWeight = 22 });
            _gridHistorial.Columns.Add(new DataGridViewTextBoxColumn { Name = "hTabla",    HeaderText = T("diag.col.tabla",      "Tabla"),            FillWeight = 15 });
            _gridHistorial.Columns.Add(new DataGridViewTextBoxColumn { Name = "hDVVAlm",   HeaderText = T("diag.col.dvv.alm",   "DVV Almacenado"),   FillWeight = 16 });
            _gridHistorial.Columns.Add(new DataGridViewTextBoxColumn { Name = "hDVVCalc",  HeaderText = T("diag.col.dvv.calc",  "DVV Calculado"),    FillWeight = 16 });
            _gridHistorial.Columns.Add(new DataGridViewTextBoxColumn { Name = "hRotas",    HeaderText = T("diag.col.filas.corr","Filas Corruptas"),  FillWeight = 14 });
            _gridHistorial.Columns.Add(new DataGridViewTextBoxColumn { Name = "hResultado",HeaderText = T("diag.col.resultado", "Resultado"),        FillWeight = 10 });
            _gridHistorial.Columns.Add(new DataGridViewTextBoxColumn { Name = "hOrigen",   HeaderText = T("diag.col.origen",    "Disparado por"),    FillWeight = 12 });

            _gridHistorial.CellFormatting += GridHistorial_CellFormatting;

            var panelBotones = new FlowLayoutPanel
            {
                Dock          = DockStyle.Bottom,
                Height        = 44,
                FlowDirection = FlowDirection.RightToLeft,
                Padding       = new Padding(4),
                BackColor     = Color.FromArgb(245, 245, 250)
            };

            _btnActualizarHist = new Button
            {
                Text      = T("diag.btn.actualizar", "Actualizar"),
                Width     = 100,
                Height    = 32,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(100, 160, 100),
                ForeColor = Color.White
            };
            _btnActualizarHist.FlatAppearance.BorderSize = 0;
            _btnActualizarHist.Click += (s, e) => CargarHistorial();

            panelBotones.Controls.Add(_btnActualizarHist);

            tab.Controls.Add(_gridHistorial);
            tab.Controls.Add(panelBotones);

            return tab;
        }

        // ── Ciclo de vida ─────────────────────────────────────────────────────

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                string ico = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico");
                if (System.IO.File.Exists(ico)) this.Icon = new System.Drawing.Icon(ico);
            }
            catch { }
            GestorIdioma.SuscribirObservador(this);
            CargarDiagnostico();
            CargarHistorial();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            GestorIdioma.DesuscribirObservador(this);
            base.OnFormClosing(e);
        }

        // ── IIdiomaObserver ───────────────────────────────────────────────────

        public void UpdateLanguage(Idioma idioma)
        {
            this.Text                   = T("diag.frm.titulo",         "Diagnóstico de Integridad");
            _lblFilasRotas.Text         = T("diag.lbl.filasrotas",     "Filas con DVH inválido:");
            _btnActualizar.Text         = T("diag.btn.actualizar",     "Actualizar");
            _btnReparar.Text            = T("diag.btn.reparar",        "Reparar Seleccionadas...");
            _btnRecalcularTodo.Text     = T("diag.btn.recalcular",     "Recalcular Todo");
            _btnActualizarHist.Text     = T("diag.btn.actualizar",     "Actualizar");

            if (_tabs.TabPages.Count >= 1)
                _tabs.TabPages[0].Text = T("diag.tab.diagnostico",    "Diagnóstico");
            if (_tabs.TabPages.Count >= 2)
                _tabs.TabPages[1].Text = T("diag.tab.historial",      "Historial de Verificaciones");

            ActualizarHeadersGrilla();
            CargarDiagnostico();
            CargarHistorial();
        }

        private void ActualizarHeadersGrilla()
        {
            void SetH(DataGridView g, string col, string key, string fb)
            {
                if (g.Columns.Contains(col)) g.Columns[col].HeaderText = T(key, fb);
            }
            SetH(_gridRotas,    "colId",       "diag.col.id",       "ID");
            SetH(_gridRotas,    "colUsuario",   "diag.col.usuario",  "Usuario");
            SetH(_gridRotas,    "colDVHAlm",    "diag.col.dvh.alm",  "DVH Almacenado");
            SetH(_gridRotas,    "colDVHCalc",   "diag.col.dvh.calc", "DVH Calculado");
            SetH(_gridRotas,    "colEstado",    "diag.col.estado",   "Estado");
            SetH(_gridHistorial,"hFecha",       "diag.col.fecha",    "Fecha");
            SetH(_gridHistorial,"hTabla",       "diag.col.tabla",    "Tabla");
            SetH(_gridHistorial,"hDVVAlm",      "diag.col.dvv.alm",  "DVV Almacenado");
            SetH(_gridHistorial,"hDVVCalc",     "diag.col.dvv.calc", "DVV Calculado");
            SetH(_gridHistorial,"hRotas",       "diag.col.filas.corr","Filas Corruptas");
            SetH(_gridHistorial,"hResultado",   "diag.col.resultado","Resultado");
            SetH(_gridHistorial,"hOrigen",      "diag.col.origen",   "Disparado por");
        }

        // ── Carga de datos ────────────────────────────────────────────────────

        private void CargarDiagnostico()
        {
            _btnActualizar.Enabled = false;
            try
            {
                var diag = BLL.Configuracion.ObtenerDiagnostico();

                _lblEstadoDVV.Text = diag.Integro
                    ? T("diag.estado.integro",      "Estado: INTEGRO")
                    : T("diag.estado.comprometido", "Estado: COMPROMETIDO");
                _lblEstadoDVV.ForeColor = diag.Integro
                    ? Color.FromArgb(40, 140, 60)
                    : Color.FromArgb(180, 50, 50);

                _lblDVVDetalle.Text = string.Format(
                    T("diag.dvv.detalle", "DVV almacenado: {0}   |   DVV calculado: {1}   |   Filas con DVH inválido: {2}"),
                    diag.DVVAlmacenado?.ToString() ?? "—",
                    diag.DVVCalculado,
                    diag.FilasRotas.Count);

                _gridRotas.Rows.Clear();
                string sinDVH      = T("diag.fila.sinDVH",     "Sin DVH");
                string noCoincide  = T("diag.fila.nocoincide", "DVH no coincide");
                string dvhRuntime  = T("diag.col.dvh.calc",    "DVH Calculado") + " (runtime)";
                foreach (var fila in diag.FilasRotas)
                {
                    string estadoFila = fila.DVHAlmacenado == null ? sinDVH : noCoincide;
                    _gridRotas.Rows.Add(fila.Id, fila.Username,
                        fila.DVHAlmacenado?.ToString() ?? "—",
                        dvhRuntime,
                        estadoFila);
                }

                _btnReparar.Enabled        = diag.FilasRotas.Count > 0;
                _btnRecalcularTodo.Enabled = !diag.Integro;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(T("diag.err.cargar", "Error al cargar diagnóstico: {0}"), ex.Message),
                    T("diag.err.titulo", "Error"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _btnActualizar.Enabled = true;
            }
        }

        private void CargarHistorial()
        {
            _gridHistorial.Rows.Clear();
            try
            {
                string ok    = T("diag.hist.ok",    "OK");
                string fallo = T("diag.hist.fallo", "FALLO");
                var lista = BLL.Configuracion.ObtenerHistorialIntegridad(150);
                foreach (var h in lista)
                {
                    _gridHistorial.Rows.Add(
                        h.FechaVerificacion.ToString("dd/MM/yyyy HH:mm:ss"),
                        h.NombreTabla,
                        h.DVVAlmacenado?.ToString() ?? "—",
                        h.DVVCalculado.ToString(),
                        h.FilasCorruptas.ToString(),
                        h.Resultado ? ok : fallo,
                        h.DisparadoPor);
                }
            }
            catch
            {
                // Si la tabla aún no existe, mostrar vacío silenciosamente
            }
        }

        private void GridHistorial_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var col = _gridHistorial.Columns[e.ColumnIndex];
            if (col.Name != "hResultado") return;

            string ok  = T("diag.hist.ok", "OK");
            string val = e.Value?.ToString() ?? "";
            e.CellStyle.ForeColor = val == ok
                ? Color.FromArgb(30, 130, 50)
                : Color.FromArgb(180, 50, 50);
            e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
        }

        // ── Acciones ──────────────────────────────────────────────────────────

        private void BtnReparar_Click(object sender, EventArgs e)
        {
            var diag = BLL.Configuracion.ObtenerDiagnostico();
            if (diag.FilasRotas.Count == 0)
            {
                MessageBox.Show(
                    T("diag.msg.sinproblemas", "No hay filas con DVH inválido."),
                    T("diag.msg.sinprob.titulo", "Sin problemas"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var admin = new ConfirmarAdminForm())
            {
                if (admin.ShowDialog(this) != DialogResult.OK || !admin.Autorizado) return;

                using (var rep = new ReparacionAsistidaForm(diag.FilasRotas))
                {
                    rep.ShowDialog(this);
                }
            }

            CargarDiagnostico();
            CargarHistorial();
        }

        private void BtnRecalcularTodo_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(
                    T("diag.conf.recalcular",
                      "¿Recalcular todos los DVH y el DVV de la tabla Usuario?\n\nEsta operación sobreescribirá todos los dígitos verificadores almacenados."),
                    T("diag.conf.recalcular.titulo", "Confirmar Recálculo Total"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            using (var admin = new ConfirmarAdminForm())
            {
                if (admin.ShowDialog(this) != DialogResult.OK || !admin.Autorizado) return;

                try
                {
                    BLL.Configuracion.RecalcularIntegridadDV();
                    MessageBox.Show(
                        T("diag.msg.recalc.exito", "Dígitos verificadores recalculados con éxito."),
                        T("diag.msg.exito.titulo", "Éxito"),
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        string.Format(T("diag.err.recalcular", "Error al recalcular: {0}"), ex.Message),
                        T("diag.err.titulo", "Error"),
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            CargarDiagnostico();
            CargarHistorial();
        }
    }
}
