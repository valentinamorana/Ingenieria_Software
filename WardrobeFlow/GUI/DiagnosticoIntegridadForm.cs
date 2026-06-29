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
        private Button      _btnRecalcularTodo;
        private Button      _btnEspejo;
        private Label       _lblFilasRotas;
        private Label       _lblGridVacio;   // cartel "Todo íntegro" sobre la grilla (estado vacío)

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

            _btnEspejo = new Button
            {
                Text      = T("diag.btn.espejo", "Recuperación (Espejo)..."),
                Width     = 180,
                Height    = 32,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(80, 150, 90),
                ForeColor = Color.White
            };
            _btnEspejo.FlatAppearance.BorderSize = 0;
            _btnEspejo.Click += BtnEspejo_Click;

            panelBotones.Controls.AddRange(new Control[] { _btnRecalcularTodo, _btnEspejo, _btnActualizar });

            // Cartel de estado vacío ("Todo íntegro"): se muestra SOBRE la grilla en vez de
            // inyectar una fila falsa (que antes caía en la columna Usuario y confundía).
            _lblGridVacio = new Label
            {
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 140, 60),
                BackColor = Color.White,
                Visible   = false,
                Text      = T("diag.sinfilas", "✓ Todo íntegro — no hay filas con problemas de integridad.")
            };

            var contenedor = new Panel { Dock = DockStyle.Fill };
            contenedor.Controls.Add(_gridRotas);
            contenedor.Controls.Add(_lblGridVacio);
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
            _btnRecalcularTodo.Text     = T("diag.btn.recalcular",     "Recalcular Todo");
            _btnEspejo.Text             = T("diag.btn.espejo",         "Recuperación (Espejo)...");
            // (botón "Reparar Seleccionadas" eliminado: lo reemplaza la consola del Espejo)
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

                if (diag.TablasAdicionalesCorruptas.Count > 0)
                    _lblDVVDetalle.Text += "   |   " + string.Format(
                        T("diag.tablas.corruptas", "Tablas con DV inválido: {0}"),
                        string.Join(", ", diag.TablasAdicionalesCorruptas));

                _gridRotas.Rows.Clear();
                string sinDVH      = T("diag.fila.sinDVH",     "Sin DVH");
                string noCoincide  = T("diag.fila.nocoincide", "DVH no coincide");
                string dvhRuntime  = T("diag.fila.recalculado", "Recalculado en vivo");
                foreach (var fila in diag.FilasRotas)
                {
                    string estadoFila = fila.DVHAlmacenado == null ? sinDVH : noCoincide;
                    _gridRotas.Rows.Add(fila.Id, fila.Username,
                        fila.DVHAlmacenado?.ToString() ?? "—",
                        dvhRuntime,
                        estadoFila);
                }

                // Tablas adicionales (Cliente / Empleado / Pedido) con DV inválido: se listan
                // como filas informativas para que el problema sea visible y se sepa que hay que
                // recalcular (antes una corrupción en Cliente no aparecía por ningún lado).
                foreach (var tablaCorrupta in diag.TablasAdicionalesCorruptas)
                {
                    int tIdx = _gridRotas.Rows.Add("—",
                        string.Format(T("diag.tabla.corrupta", "Tabla '{0}'"), tablaCorrupta),
                        "—", "—",
                        T("diag.tabla.dvinvalido", "DV inválido — usá «Recalcular Todo»"));
                    _gridRotas.Rows[tIdx].DefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(180, 50, 50);
                    _gridRotas.Rows[tIdx].ReadOnly = true;
                }

                // Si no hay NINGÚN problema (ni en Usuario ni en las tablas adicionales),
                // mostrar el cartel "Todo íntegro" SOBRE la grilla (no como fila en Usuario).
                bool gridVacio = diag.FilasRotas.Count == 0 && diag.TablasAdicionalesCorruptas.Count == 0;
                _lblGridVacio.Text    = T("diag.sinfilas", "✓ Todo íntegro — no hay filas con problemas de integridad.");
                _lblGridVacio.Visible = gridVacio;
                if (gridVacio) _lblGridVacio.BringToFront(); else _lblGridVacio.SendToBack();

                // Habilitar el recálculo total si CUALQUIER tabla protegida está comprometida
                // (incluye Cliente/Empleado/Pedido, no solo Usuario).
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
                    int idx = _gridHistorial.Rows.Add(
                        h.FechaVerificacion.ToString("dd/MM/yyyy HH:mm:ss"),
                        h.NombreTabla,
                        h.DVVAlmacenado?.ToString() ?? "—",
                        h.DVVCalculado.ToString(),
                        h.FilasCorruptas.ToString(),
                        h.Resultado ? ok : fallo,
                        h.DisparadoPor);

                    // Corrección pedida: al pasar el mouse sobre una verificación, explicar
                    // POR QUÉ falló (o por qué está OK). El tooltip se fija en toda la fila.
                    string tip = ConstruirTooltipHistorial(h);
                    foreach (DataGridViewCell celda in _gridHistorial.Rows[idx].Cells)
                        celda.ToolTipText = tip;
                }
            }
            catch
            {
                // Si la tabla aún no existe, mostrar vacío silenciosamente
            }
        }

        // Arma el texto del tooltip de una verificación del historial: si FALLÓ, detalla la causa
        // (DVV que no coincide y/o filas con DVH inválido); si está OK, lo confirma.
        private string ConstruirTooltipHistorial(BE.HistorialIntegridad h)
        {
            if (h.Resultado)
                return T("diag.tip.ok", "Verificación correcta: los dígitos verificadores coinciden con los datos.");

            var sb = new System.Text.StringBuilder();
            sb.AppendLine(string.Format(
                T("diag.tip.fallo.titulo", "Verificación FALLIDA — tabla {0}"), h.NombreTabla));

            // DVV no coincide → se agregaron/quitaron/reordenaron filas.
            if (h.DVVAlmacenado == null || h.DVVAlmacenado != h.DVVCalculado)
                sb.AppendLine(string.Format(
                    T("diag.tip.dvv.mismatch",
                      "• El DVV no coincide: almacenado {0} ≠ calculado {1} (se insertaron, eliminaron o reordenaron filas)."),
                    h.DVVAlmacenado?.ToString() ?? "—", h.DVVCalculado));

            // Filas con DVH inválido → se modificó el contenido de esas filas.
            if (h.FilasCorruptas > 0)
                sb.AppendLine(string.Format(
                    T("diag.tip.filas.corruptas",
                      "• {0} fila(s) con DVH inválido: el contenido de esas filas fue modificado directamente en la base."),
                    h.FilasCorruptas));

            sb.Append(T("diag.tip.causa",
                "Posible manipulación directa de la base de datos. Reparar desde la pestaña Diagnóstico o restaurar un backup."));
            return sb.ToString();
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

        private void BtnEspejo_Click(object sender, EventArgs e)
        {
            using (var rec = new RecuperacionEspejoForm())
            {
                rec.ShowDialog(this);
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
