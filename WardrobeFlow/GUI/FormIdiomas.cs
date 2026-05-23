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

        public FormIdiomas()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GestorIdioma.SuscribirObservador(this);
            Traducir(GestorIdioma.IdiomaActual);
            ConfigurarGrillas();
            CargarIdiomas();
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
            btnActivar.Text         = T("btn.idiomas.activar",    "✔ Activar");
            btnDesactivar.Text      = T("btn.idiomas.desactivar", "✕ Desactivar");
            btnGuardar.Text         = T("btn.idiomas.guardar",    "💾 Guardar cambios");
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
                MostrarError($"Error al cargar idiomas: {ex.Message}");
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
                var colTexto     = new DataGridViewTextBoxColumn { Name = "colTexto",     HeaderText = "Texto",      ReadOnly = false };

                dgvTraducciones.Columns.AddRange(colIdControl, colClave, colFormulario, colTexto);

                foreach (var f in filas)
                    dgvTraducciones.Rows.Add(f.IdControl, f.Clave, f.Formulario, f.Texto);

                MostrarOk($"{filas.Count} traducción(es) cargadas.");
            }
            catch (Exception ex)
            {
                MostrarError($"Error al cargar traducciones: {ex.Message}");
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
                _bllIdioma.ActivarIdioma(_idIdiomaSeleccionado);
                MostrarOk("Idioma activado.");
                CargarIdiomas();
            }
            catch (Exception ex) { MostrarError(ex.Message); }
        }

        private void BtnDesactivar_Click(object sender, EventArgs e)
        {
            if (_idIdiomaSeleccionado == 0) return;
            var confirm = MessageBox.Show(
                "¿Desactivar este idioma? Los usuarios no podrán seleccionarlo.",
                "Confirmar",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);
            if (confirm != DialogResult.Yes) return;
            try
            {
                _bllIdioma.DesactivarIdioma(_idIdiomaSeleccionado);
                MostrarOk("Idioma desactivado.");
                CargarIdiomas();
            }
            catch (Exception ex) { MostrarError(ex.Message); }
        }

        // ── Guardar traducción editada ────────────────────────────────────────

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (_idIdiomaSeleccionado == 0)
            {
                MostrarError("Seleccioná un idioma primero.");
                return;
            }

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
                MostrarOk($"{guardadas} traducción(es) guardadas correctamente.");
            else
                MostrarError($"Se guardaron {guardadas} y fallaron {errores}.");
        }
    }
}
