using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Servicios.Multiidioma;

namespace GUI
{
    /// <summary>
    /// ABM de traducciones e idiomas del sistema.
    ///
    /// Grilla superior: listado de idiomas (Activo/Inactivo).
    /// Grilla inferior: traducciones editables del idioma seleccionado.
    ///
    /// Implementa IIdiomaObserver para que sus propios controles se traduzcan
    /// al cambiar el idioma activo (título, botones, etiquetas de sección).
    /// </summary>
    public partial class FormIdiomas : FormBase, IIdiomaObserver
    {
        protected override Label MensajeLabel => lblMensaje;

        private readonly BLL.IdiomaService  _bllIdioma  = new BLL.IdiomaService();
        private List<BE.Idioma>      _idiomas     = new List<BE.Idioma>();
        private int                  _idIdiomaSeleccionado = 0;
        private Button               _btnNuevoIdioma, _btnRenombrarIdioma;

        public FormIdiomas()
        {
            InitializeComponent();
        }

        // Helper de traducción reutilizable (idioma activo).
        private string Tx(string key, string fallback)
        {
            var t = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
            return t.ContainsKey(key) ? t[key].Texto : fallback;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GestorIdioma.SuscribirObservador(this);
            Traducir(GestorIdioma.IdiomaActual);
            ConfigurarGrillas();
            CrearBotonesIdioma();
            CargarIdiomas();
            CargarControles();
        }

        // ── Alta / modificación de idiomas (T05) — botones creados por código ────
        private void CrearBotonesIdioma()
        {
            // Se agregan al MISMO panel que Activar/Desactivar (panelBotonesIdioma), en la misma
            // fila y con el mismo espaciado, para que no se superpongan (antes iban a panelIdiomas
            // con coordenadas absolutas y se pisaban con los otros botones al reescalar).
            //   Activar(0) · Desactivar(140) · Nuevo idioma(280) · Renombrar(420)
            _btnNuevoIdioma = new Button
            {
                Text = Tx("btn.idiomas.nuevo", "➕ Nuevo idioma"), Location = new System.Drawing.Point(280, 4),
                Size = new System.Drawing.Size(130, 28), FlatStyle = FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(210, 100, 135), ForeColor = System.Drawing.Color.White,
                Cursor = Cursors.Hand
            };
            _btnNuevoIdioma.FlatAppearance.BorderSize = 0;
            _btnNuevoIdioma.Click += BtnNuevoIdioma_Click;

            _btnRenombrarIdioma = new Button
            {
                Text = Tx("btn.idiomas.renombrar", "✏ Renombrar"), Location = new System.Drawing.Point(420, 4),
                Size = new System.Drawing.Size(120, 28), FlatStyle = FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(176, 62, 96), ForeColor = System.Drawing.Color.White,
                Cursor = Cursors.Hand
            };
            _btnRenombrarIdioma.FlatAppearance.BorderSize = 0;
            _btnRenombrarIdioma.Click += BtnRenombrarIdioma_Click;

            this.panelBotonesIdioma.Controls.Add(_btnNuevoIdioma);
            this.panelBotonesIdioma.Controls.Add(_btnRenombrarIdioma);
        }

        private void BtnNuevoIdioma_Click(object sender, EventArgs e)
        {
            string codigo = Pedir(Tx("idiomas.dlg.nuevo.t", "Nuevo idioma"), Tx("idiomas.dlg.nuevo.codigo", "Código (ej: FR, IT, PT) — máx. 5:"));
            if (string.IsNullOrWhiteSpace(codigo)) return;
            string nombre = Pedir(Tx("idiomas.dlg.nuevo.t", "Nuevo idioma"), Tx("idiomas.dlg.nuevo.nombre", "Nombre del idioma (ej: Français):"));
            if (string.IsNullOrWhiteSpace(nombre)) return;
            try
            {
                _bllIdioma.CrearIdioma(codigo, nombre);
                CargarIdiomas();
                MostrarOk(string.Format(Tx("idiomas.ok.creado", "Idioma '{0}' creado (inactivo — activalo cuando cargues sus traducciones)."), nombre.Trim()));
            }
            catch (Exception ex) { MostrarError(ex); }
        }

        private void BtnRenombrarIdioma_Click(object sender, EventArgs e)
        {
            if (_idIdiomaSeleccionado == 0) { MostrarError(Tx("idiomas.msg.selecc", "Seleccioná un idioma de la grilla.")); return; }
            string nombre = Pedir(Tx("idiomas.dlg.renombrar.t", "Renombrar idioma"), Tx("idiomas.dlg.renombrar.p", "Nuevo nombre:"));
            if (string.IsNullOrWhiteSpace(nombre)) return;
            try
            {
                _bllIdioma.ModificarIdioma(_idIdiomaSeleccionado, nombre);
                CargarIdiomas();
                MostrarOk(Tx("idiomas.ok.renombrado", "Idioma renombrado."));
            }
            catch (Exception ex) { MostrarError(ex); }
        }

        // Actualiza la lista GLOBAL de idiomas disponibles y notifica a los observers (Menu, etc.)
        // para que sus dropdowns se reconstruyan al instante tras activar/desactivar/crear un idioma.
        private void RefrescarIdiomasGlobales()
        {
            try
            {
                var activos = _bllIdioma.ObtenerIdiomasActivosComoIdioma();
                if (activos != null && activos.Count > 0)
                {
                    GestorIdioma.SetIdiomasDisponibles(activos);
                    GestorIdioma.CambiarIdioma(GestorIdioma.IdiomaActual); // re-notifica sin cambiar el idioma activo
                }
            }
            catch { /* si falla el refresco no debe romper la operación principal */ }
        }

        // Mini cuadro de entrada de texto (sin dependencias externas).
        private static string Pedir(string titulo, string prompt)
        {
            var tr = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
            string txtOk = tr.ContainsKey("btn.aceptar")  ? tr["btn.aceptar"].Texto  : "Aceptar";
            string txtCa = tr.ContainsKey("btn.cancelar") ? tr["btn.cancelar"].Texto : "Cancelar";
            using (var f = new Form())
            {
                f.Text = titulo; f.Size = new System.Drawing.Size(420, 160);
                f.StartPosition = FormStartPosition.CenterParent;
                f.FormBorderStyle = FormBorderStyle.FixedDialog; f.MinimizeBox = false; f.MaximizeBox = false;
                var lbl = new Label { Text = prompt, Location = new System.Drawing.Point(12, 15), AutoSize = true };
                var txt = new TextBox { Location = new System.Drawing.Point(15, 45), Size = new System.Drawing.Size(380, 24) };
                var ok = new Button { Text = txtOk, DialogResult = DialogResult.OK, Location = new System.Drawing.Point(225, 80), Size = new System.Drawing.Size(80, 30) };
                var ca = new Button { Text = txtCa, DialogResult = DialogResult.Cancel, Location = new System.Drawing.Point(315, 80), Size = new System.Drawing.Size(80, 30) };
                f.Controls.AddRange(new Control[] { lbl, txt, ok, ca });
                f.AcceptButton = ok; f.CancelButton = ca;
                return f.ShowDialog() == DialogResult.OK ? txt.Text : null;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            GestorIdioma.DesuscribirObservador(this);
            base.OnFormClosing(e);
        }

        // ── IIdiomaObserver ───────────────────────────────────────────────────

        public void UpdateLanguage(Idioma idioma) => Traducir(idioma);

        private void Traducir(Idioma idioma)
        {
            var t = Traductor.ObtenerTraducciones(idioma);
            string T(string k, string fb) => t.ContainsKey(k) ? t[k].Texto : fb;

            this.Text               = T("frm.idiomas",            "Gestión de Idiomas");
            lblTituloIdiomas.Text   = T("lbl.idiomas.titulo",     "Idiomas del sistema");
            lblTituloTrad.Text      = T("lbl.idiomas.trad",       "Traducciones del idioma seleccionado");
            lblTituloControles.Text = T("lbl.idiomas.controles",  "Controles traducibles");
            btnActivar.Text         = T("btn.idiomas.activar",    "✔ Activar");
            btnDesactivar.Text      = T("btn.idiomas.desactivar", "✕ Desactivar");
            btnGuardar.Text         = T("btn.idiomas.guardar",    "💾 Guardar cambios");
            // Botones creados por código (pueden no existir aún en el primer Traducir de OnLoad).
            if (_btnNuevoIdioma     != null) _btnNuevoIdioma.Text     = T("btn.idiomas.nuevo",     "➕ Nuevo idioma");
            if (_btnRenombrarIdioma != null) _btnRenombrarIdioma.Text = T("btn.idiomas.renombrar", "✏ Renombrar");
        }

        // ── Configuración inicial de grillas ─────────────────────────────────

        private void ConfigurarGrillas()
        {
            // dgvIdiomas — solo lectura, una fila a la vez
            dgvIdiomas.SelectionMode           = DataGridViewSelectionMode.FullRowSelect;
            dgvIdiomas.MultiSelect             = false;
            dgvIdiomas.ReadOnly                = true;
            dgvIdiomas.AllowUserToAddRows      = false;
            dgvIdiomas.AllowUserToDeleteRows   = false;
            dgvIdiomas.AutoSizeColumnsMode     = DataGridViewAutoSizeColumnsMode.Fill;
            dgvIdiomas.RowHeadersVisible       = false;

            // dgvTraducciones — Clave y Formulario son de solo lectura; Texto es editable
            dgvTraducciones.SelectionMode          = DataGridViewSelectionMode.FullRowSelect;
            dgvTraducciones.MultiSelect            = false;
            dgvTraducciones.AllowUserToAddRows     = false;
            dgvTraducciones.AllowUserToDeleteRows  = false;
            dgvTraducciones.AutoSizeColumnsMode    = DataGridViewAutoSizeColumnsMode.Fill;
            dgvTraducciones.RowHeadersVisible      = false;
            // Guardado automático: al confirmar la edición de una celda (Enter o salir de
            // la celda) se persiste la traducción en BD, sin tener que apretar "Guardar".
            dgvTraducciones.CellEndEdit           -= DgvTraducciones_CellEndEdit;
            dgvTraducciones.CellEndEdit           += DgvTraducciones_CellEndEdit;

            // dgvControles — listado de controles traducibles del sistema (solo lectura)
            dgvControles.SelectionMode          = DataGridViewSelectionMode.FullRowSelect;
            dgvControles.MultiSelect            = false;
            dgvControles.ReadOnly               = true;
            dgvControles.AllowUserToAddRows     = false;
            dgvControles.AllowUserToDeleteRows  = false;
            dgvControles.AutoSizeColumnsMode    = DataGridViewAutoSizeColumnsMode.Fill;
            dgvControles.RowHeadersVisible      = false;
        }

        // ── Grid de Controles (textos traducibles del sistema) — T05 ─────────────
        private void CargarControles()
        {
            try
            {
                dgvControles.Rows.Clear();
                dgvControles.Columns.Clear();
                dgvControles.Columns.Add("colCtrlId",   "ID");
                dgvControles.Columns.Add("colCtrlClave","Clave");
                dgvControles.Columns.Add("colCtrlForm", "Formulario");
                dgvControles.Columns["colCtrlId"].Width = 40;

                foreach (var c in _bllIdioma.ObtenerControles())
                    dgvControles.Rows.Add(c.IdControl, c.Clave, c.Formulario);
            }
            catch (Exception ex)
            {
                MostrarError(string.Format(Tx("err.generico.cargar", "Error al cargar: {0}"), ex.Message));
            }
        }

        // ── Carga de idiomas ─────────────────────────────────────────────────

        private void CargarIdiomas()
        {
            try
            {
                // Guardar el ID seleccionado antes de limpiar la grilla
                int prevSeleccionado = _idIdiomaSeleccionado;

                _idiomas = _bllIdioma.ObtenerTodosLosIdiomas();

                // Suspender eventos para que el Columns.Clear no dispare SelectionChanged
                dgvIdiomas.SelectionChanged -= DgvIdiomas_SelectionChanged;
                dgvIdiomas.Rows.Clear();
                dgvIdiomas.Columns.Clear();

                dgvIdiomas.Columns.Add("colId",     "ID");
                dgvIdiomas.Columns.Add("colCodigo", "Código");
                dgvIdiomas.Columns.Add("colNombre", "Nombre");
                dgvIdiomas.Columns.Add("colActivo", "Activo");
                dgvIdiomas.Columns.Add("colDefault","Default");

                dgvIdiomas.Columns["colId"].Width      = 40;
                dgvIdiomas.Columns["colCodigo"].Width  = 60;
                dgvIdiomas.Columns["colActivo"].Width  = 60;
                dgvIdiomas.Columns["colDefault"].Width = 65;

                foreach (var idm in _idiomas)
                    dgvIdiomas.Rows.Add(
                        idm.IdIdioma,
                        idm.Codigo,
                        idm.Nombre,
                        idm.Activo   ? "Sí" : "No",
                        idm.EsDefault? "Sí" : "No");

                dgvIdiomas.SelectionChanged += DgvIdiomas_SelectionChanged;

                // Restaurar la selección previa; si no existe, seleccionar la primera fila
                bool restaurado = false;
                if (prevSeleccionado != 0)
                {
                    foreach (DataGridViewRow row in dgvIdiomas.Rows)
                    {
                        if (Convert.ToInt32(row.Cells["colId"].Value) == prevSeleccionado)
                        {
                            row.Selected = true;
                            _idIdiomaSeleccionado = prevSeleccionado;
                            restaurado = true;
                            break;
                        }
                    }
                }
                if (!restaurado && dgvIdiomas.Rows.Count > 0)
                    dgvIdiomas.Rows[0].Selected = true;

                ActualizarBotonesIdioma();
            }
            catch (Exception ex)
            {
                MostrarError(string.Format(Tx("err.generico.cargar", "Error al cargar: {0}"), ex.Message));
            }
        }

        // ── Carga de traducciones del idioma seleccionado ────────────────────

        private void CargarTraducciones(int idIdioma)
        {
            try
            {
                var filas = _bllIdioma.ObtenerTraduccionesPorIdioma(idIdioma);

                dgvTraducciones.Rows.Clear();
                dgvTraducciones.Columns.Clear();

                var colIdControl = new DataGridViewTextBoxColumn { Name = "colIdControl", HeaderText = "ID", Width = 40, ReadOnly = true };
                var colClave     = new DataGridViewTextBoxColumn { Name = "colClave",     HeaderText = "Clave",      ReadOnly = true };
                var colFormulario= new DataGridViewTextBoxColumn { Name = "colFormulario",HeaderText = "Formulario", ReadOnly = true, Width = 120 };
                // Referencia: el texto del idioma por defecto (completo), para que un traductor vea
                // el original mientras completa el idioma destino. Solo lectura.
                var colReferencia= new DataGridViewTextBoxColumn { Name = "colReferencia",HeaderText = "Referencia (por defecto)", ReadOnly = true };
                colReferencia.DefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(110, 110, 120);
                var colTexto     = new DataGridViewTextBoxColumn { Name = "colTexto",     HeaderText = "Texto / Traducción", ReadOnly = false };

                dgvTraducciones.Columns.AddRange(colIdControl, colClave, colFormulario, colReferencia, colTexto);

                // Diccionario del idioma por defecto (ES), keyed por clave → texto de referencia.
                var refDict = Traductor.ObtenerTraduccionesHardcode(Traductor.ObtenerIdiomaDefault());

                foreach (var f in filas)
                {
                    string referencia = refDict != null && refDict.ContainsKey(f.Clave) ? refDict[f.Clave].Texto : "";
                    dgvTraducciones.Rows.Add(f.IdControl, f.Clave, f.Formulario, referencia, f.Texto);
                }

                var tOk = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
                string fmtOk = tOk.ContainsKey("msg.idiomas.cargadas") ? tOk["msg.idiomas.cargadas"].Texto : "{0} traducción(es) cargadas.";
                MostrarOk(string.Format(fmtOk, filas.Count));
            }
            catch (Exception ex)
            {
                MostrarError(string.Format(Tx("err.generico.cargar", "Error al cargar: {0}"), ex.Message));
            }
        }

        // ── Eventos de la grilla de idiomas ──────────────────────────────────

        private void DgvIdiomas_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvIdiomas.SelectedRows.Count == 0) return;

            var row = dgvIdiomas.SelectedRows[0];
            _idIdiomaSeleccionado = Convert.ToInt32(row.Cells["colId"].Value);

            ActualizarBotonesIdioma();
            CargarTraducciones(_idIdiomaSeleccionado);
        }

        private void ActualizarBotonesIdioma()
        {
            if (dgvIdiomas.SelectedRows.Count == 0 || _idiomas.Count == 0)
            {
                btnActivar.Enabled   = false;
                btnDesactivar.Enabled = false;
                return;
            }

            var row   = dgvIdiomas.SelectedRows[0];
            int id    = Convert.ToInt32(row.Cells["colId"].Value);
            var idm   = _idiomas.Find(i => i.IdIdioma == id);
            if (idm == null) return;

            btnActivar.Enabled    = !idm.Activo;
            btnDesactivar.Enabled =  idm.Activo && !idm.EsDefault;
        }

        // ── Botones de idioma ─────────────────────────────────────────────────

        private void BtnActivar_Click(object sender, EventArgs e)
        {
            if (_idIdiomaSeleccionado == 0) return;
            try
            {
                // T05 — Advertir si el idioma tiene traducciones incompletas.
                // Política elegida: permitir la activación, usando los textos por defecto
                // para las claves faltantes (fallback por-clave en Traductor).
                int faltantes = _bllIdioma.ContarTraduccionesFaltantes(_idIdiomaSeleccionado);
                if (faltantes > 0)
                {
                    var confirm = MessageBox.Show(
                        string.Format(Tx("idiomas.conf.incompleto",
                            "Este idioma tiene {0} control(es) sin traducir.\nSi lo activás, esos textos se mostrarán en el idioma por defecto.\n\n¿Activar de todos modos?"), faltantes),
                        Tx("idiomas.conf.incompleto.t", "Traducciones incompletas"),
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                        MessageBoxDefaultButton.Button2);
                    if (confirm != DialogResult.Yes) return;
                }

                _bllIdioma.ActivarIdioma(_idIdiomaSeleccionado);
                var t = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
                MostrarOk(t.ContainsKey("msg.idiomas.activado") ? t["msg.idiomas.activado"].Texto : "Idioma activado.");
                CargarIdiomas();
                RefrescarIdiomasGlobales();   // que el nuevo idioma aparezca al instante en los dropdowns
            }
            catch (Exception ex) { MostrarError(ex); }
        }

        private void BtnDesactivar_Click(object sender, EventArgs e)
        {
            if (_idIdiomaSeleccionado == 0) return;
            var t = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
            string T(string k, string fb) => t.ContainsKey(k) ? t[k].Texto : fb;

            var confirm = MessageBox.Show(
                T("conf.idiomas.desactivar", "¿Desactivar este idioma? Los usuarios no podrán seleccionarlo."),
                T("conf.idiomas.titulo", "Confirmar"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);
            if (confirm != DialogResult.Yes) return;
            try
            {
                _bllIdioma.DesactivarIdioma(_idIdiomaSeleccionado);
                var t2 = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
                MostrarOk(t2.ContainsKey("msg.idiomas.desactivado") ? t2["msg.idiomas.desactivado"].Texto : "Idioma desactivado.");
                CargarIdiomas();
                RefrescarIdiomasGlobales();   // que el dropdown deje de ofrecerlo al instante
            }
            catch (Exception ex) { MostrarError(ex); }
        }

        // ── Guardado automático al confirmar una celda (Enter / salir de la celda) ──
        private void DgvTraducciones_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (_idIdiomaSeleccionado == 0 || e.RowIndex < 0) return;
            if (!dgvTraducciones.Columns.Contains("colTexto")) return;
            if (dgvTraducciones.Columns[e.ColumnIndex].Name != "colTexto") return;

            var row = dgvTraducciones.Rows[e.RowIndex];
            if (row.Cells["colIdControl"].Value == null) return;

            int    idControl = Convert.ToInt32(row.Cells["colIdControl"].Value);
            string texto     = row.Cells["colTexto"].Value?.ToString() ?? string.Empty;

            try
            {
                _bllIdioma.GuardarTraduccion(idControl, _idIdiomaSeleccionado, texto);
                MostrarOk(Tx("msg.idiomas.guardada.una", "Traducción guardada."));
                RefrescarTraduccionesEnVivoSiActivo();   // impacta en el sistema al instante
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        // Si el idioma editado es el ACTIVO, recarga el diccionario desde BD y notifica a
        // todos los observers (patrón Observer) para que los forms reflejen el cambio en vivo.
        private void RefrescarTraduccionesEnVivoSiActivo()
        {
            var idiomaActual  = GestorIdioma.IdiomaActual;
            var idiomaEditado = _idiomas.Find(i => i.IdIdioma == _idIdiomaSeleccionado);
            if (idiomaActual != null && idiomaEditado != null
                && idiomaEditado.Codigo == idiomaActual.Id)
            {
                var dictActualizado = _bllIdioma.CargarTraducciones(idiomaActual.Id);
                GestorIdioma.CambiarIdioma(idiomaActual, dictActualizado);
            }
        }

        // ── Guardar traducción editada ────────────────────────────────────────

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (_idIdiomaSeleccionado == 0)
            {
                var tSel = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
                MostrarError(tSel.ContainsKey("msg.idiomas.seleccionar") ? tSel["msg.idiomas.seleccionar"].Texto : "Seleccioná un idioma primero.");
                return;
            }

            // Confirmar la edición en curso ANTES de leer las celdas: si el usuario tipeó una
            // traducción y clickeó "Guardar" sin salir de la celda, el valor todavía no estaba
            // volcado a Cells[].Value y se guardaba el texto viejo (o vacío). Esto causaba el
            // "no se guarda la traducción" reportado.
            dgvTraducciones.EndEdit();

            int guardadas = 0;
            int errores   = 0;

            foreach (DataGridViewRow row in dgvTraducciones.Rows)
            {
                if (row.IsNewRow) continue;
                if (row.Cells["colTexto"].Value == null) continue;

                int    idControl = Convert.ToInt32(row.Cells["colIdControl"].Value);
                string texto     = row.Cells["colTexto"].Value?.ToString() ?? string.Empty;

                try
                {
                    _bllIdioma.GuardarTraduccion(idControl, _idIdiomaSeleccionado, texto);
                    guardadas++;
                }
                catch
                {
                    errores++;
                }
            }

            if (errores == 0)
            {
                var tG = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
                string fmtG = tG.ContainsKey("msg.idiomas.guardadas") ? tG["msg.idiomas.guardadas"].Texto : "{0} traducción(es) guardadas correctamente.";
                MostrarOk(string.Format(fmtG, guardadas));

                // Si el idioma editado es el activo, refrescar en vivo (Observer).
                RefrescarTraduccionesEnVivoSiActivo();
            }
            else
            {
                var tGE = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
                string fmtGE = tGE.ContainsKey("msg.idiomas.guardadas.error") ? tGE["msg.idiomas.guardadas.error"].Texto : "Se guardaron {0} y fallaron {1}.";
                MostrarError(string.Format(fmtGE, guardadas, errores));
            }
        }
    }
}
