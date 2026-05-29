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

        // ── Tab historial ─────────────────────────────────────────────────────
        private DataGridView _gridHistorial;
        private Button       _btnActualizarHist;

        public DiagnosticoIntegridadForm()
        {
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text            = "Diagnóstico de Integridad";
            this.Size            = new Size(820, 560);
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.MinimumSize     = new Size(700, 460);
            this.Font            = new Font("Segoe UI", 9f);

            var tabs = new TabControl { Dock = DockStyle.Fill };

            tabs.TabPages.Add(BuildTabDiagnostico());
            tabs.TabPages.Add(BuildTabHistorial());

            this.Controls.Add(tabs);
        }

        private TabPage BuildTabDiagnostico()
        {
            var tab = new TabPage("Diagnóstico");

            // ── Panel de estado DVV ───────────────────────────────────────────
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

            // ── Grilla de filas rotas ─────────────────────────────────────────
            var lblRotas = new Label
            {
                Text     = "Filas con DVH inválido:",
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
            _gridRotas.Columns.Add(new DataGridViewTextBoxColumn { Name = "colId",       HeaderText = "ID",              FillWeight = 8  });
            _gridRotas.Columns.Add(new DataGridViewTextBoxColumn { Name = "colUsuario",   HeaderText = "Usuario",         FillWeight = 25 });
            _gridRotas.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDVHAlm",    HeaderText = "DVH Almacenado",  FillWeight = 20 });
            _gridRotas.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDVHCalc",   HeaderText = "DVH Calculado",   FillWeight = 20 });
            _gridRotas.Columns.Add(new DataGridViewTextBoxColumn { Name = "colEstado",    HeaderText = "Estado",          FillWeight = 27 });

            // ── Barra de botones ──────────────────────────────────────────────
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
                Text      = "Recalcular Todo",
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
                Text      = "Reparar Seleccionadas...",
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
                Text      = "Actualizar",
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
            contenedor.Controls.Add(lblRotas);

            tab.Controls.Add(contenedor);
            tab.Controls.Add(panelBotones);
            tab.Controls.Add(panelEstado);

            return tab;
        }

        private TabPage BuildTabHistorial()
        {
            var tab = new TabPage("Historial de Verificaciones");

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
            _gridHistorial.Columns.Add(new DataGridViewTextBoxColumn { Name = "hFecha",    HeaderText = "Fecha",            FillWeight = 22 });
            _gridHistorial.Columns.Add(new DataGridViewTextBoxColumn { Name = "hTabla",    HeaderText = "Tabla",            FillWeight = 15 });
            _gridHistorial.Columns.Add(new DataGridViewTextBoxColumn { Name = "hDVVAlm",   HeaderText = "DVV Almacenado",   FillWeight = 16 });
            _gridHistorial.Columns.Add(new DataGridViewTextBoxColumn { Name = "hDVVCalc",  HeaderText = "DVV Calculado",    FillWeight = 16 });
            _gridHistorial.Columns.Add(new DataGridViewTextBoxColumn { Name = "hRotas",    HeaderText = "Filas Corruptas",  FillWeight = 14 });
            _gridHistorial.Columns.Add(new DataGridViewTextBoxColumn { Name = "hResultado",HeaderText = "Resultado",        FillWeight = 10 });
            _gridHistorial.Columns.Add(new DataGridViewTextBoxColumn { Name = "hOrigen",   HeaderText = "Disparado por",    FillWeight = 12 });

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
                Text      = "Actualizar",
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
            try { string ico = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico"); if (System.IO.File.Exists(ico)) this.Icon = new System.Drawing.Icon(ico); } catch { }
            GestorIdioma.SuscribirObservador(this);
            CargarDiagnostico();
            CargarHistorial();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            GestorIdioma.DesuscribirObservador(this);
            base.OnFormClosing(e);
        }

        public void UpdateLanguage(Idioma idioma)
        {
            // Los textos son fijos en esta versión — se puede extender con Traductor si se necesita
        }

        // ── Carga de datos ────────────────────────────────────────────────────

        private void CargarDiagnostico()
        {
            _btnActualizar.Enabled = false;
            try
            {
                var diag = BLL.Configuracion.ObtenerDiagnostico();

                _lblEstadoDVV.Text      = diag.Integro ? "Estado: INTEGRO" : "Estado: COMPROMETIDO";
                _lblEstadoDVV.ForeColor = diag.Integro ? Color.FromArgb(40, 140, 60) : Color.FromArgb(180, 50, 50);

                _lblDVVDetalle.Text = $"DVV almacenado: {(diag.DVVAlmacenado?.ToString() ?? "—")}   |   " +
                                      $"DVV calculado: {diag.DVVCalculado}   |   " +
                                      $"Filas con DVH inválido: {diag.FilasRotas.Count}";

                _gridRotas.Rows.Clear();
                foreach (var fila in diag.FilasRotas)
                {
                    string estadoFila = fila.DVHAlmacenado == null ? "Sin DVH" : "DVH no coincide";
                    _gridRotas.Rows.Add(fila.Id, fila.Username, fila.DVHAlmacenado?.ToString() ?? "—", "Calculado en runtime", estadoFila);
                }

                _btnReparar.Enabled        = diag.FilasRotas.Count > 0;
                _btnRecalcularTodo.Enabled = !diag.Integro;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar diagnóstico:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                var lista = new DAL.HistorialIntegridad().ObtenerUltimos(150);
                foreach (var h in lista)
                {
                    _gridHistorial.Rows.Add(
                        h.FechaVerificacion.ToString("dd/MM/yyyy HH:mm:ss"),
                        h.NombreTabla,
                        h.DVVAlmacenado?.ToString() ?? "—",
                        h.DVVCalculado.ToString(),
                        h.FilasCorruptas.ToString(),
                        h.Resultado ? "OK" : "FALLO",
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

            string val = e.Value?.ToString() ?? "";
            e.CellStyle.ForeColor = val == "OK"
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
                MessageBox.Show("No hay filas con DVH inválido.", "Sin problemas", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                    "¿Recalcular todos los DVH y el DVV de la tabla Usuario?\n\nEsta operación sobreescribirá todos los dígitos verificadores almacenados.",
                    "Confirmar Recálculo Total",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            using (var admin = new ConfirmarAdminForm())
            {
                if (admin.ShowDialog(this) != DialogResult.OK || !admin.Autorizado) return;

                try
                {
                    BLL.Configuracion.RecalcularIntegridadDV();
                    MessageBox.Show("Dígitos verificadores recalculados con éxito.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al recalcular:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            CargarDiagnostico();
            CargarHistorial();
        }
    }
}
