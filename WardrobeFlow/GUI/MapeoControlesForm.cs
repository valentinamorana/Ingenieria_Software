using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Servicios.Multiidioma;

namespace GUI
{
    /// <summary>
    /// Etapa 4.3 — Pantalla de MAPEO de controles a patentes. El admin elige una patente y un
    /// formulario, y asocia/desasocia los controles (botones, ítems de menú) que esa patente
    /// gobierna. Los formularios y controles disponibles provienen del registro en runtime (C1):
    /// aparecen los que se abrieron al menos una vez en esta ejecución.
    /// </summary>
    public class MapeoControlesForm : Form
    {
        private readonly BLL.ControlMapeado _bll     = new BLL.ControlMapeado();
        private readonly BLL.Familia        _famBLL  = new BLL.Familia();

        private ComboBox _cmbPatente, _cmbFormulario;
        private ListBox  _lstDisponibles, _lstAsociados;
        private Button   _btnAgregar, _btnQuitar, _btnGuardar, _btnCerrar;
        private Label    _lblPat, _lblForm, _lblDisp, _lblAsoc, _lblEstado;

        // Conjunto COMPLETO de mapeos de la patente seleccionada (todos los forms): se edita el
        // subconjunto del form actual y al guardar se persiste el conjunto completo (preserva otros forms).
        private List<BE.ControlMapeado> _mapeosPatente = new List<BE.ControlMapeado>();
        // Todos los mapeos del sistema, para no ofrecer un control ya tomado por OTRA patente
        // (la tabla tiene UNIQUE(Formulario, NombreControl): un control pertenece a una sola patente).
        private List<BE.ControlMapeado> _todosLosMapeos = new List<BE.ControlMapeado>();

        private int    PatenteId   => (_cmbPatente.SelectedItem as ItemPatente)?.Id ?? 0;
        private string FormSel     => _cmbFormulario.SelectedItem as string;

        private class ItemPatente { public int Id; public string Nombre; public override string ToString() => Nombre; }

        public MapeoControlesForm() { BuildUI(); }

        private string T(string k, string fb)
        {
            var t = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
            return t.ContainsKey(k) ? t[k].Texto : fb;
        }

        private void BuildUI()
        {
            this.Text            = T("frm.mapeocontroles", "Mapeo de controles por permiso");
            this.ClientSize      = new Size(580, 470);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.BackColor       = Color.White;
            this.Font            = new Font("Segoe UI", 9f);
            try { string ico = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico"); if (System.IO.File.Exists(ico)) this.Icon = new Icon(ico); } catch { }

            _lblPat  = new Label { Text = T("map.patente", "Patente:"),    Location = new Point(16, 18), AutoSize = true };
            _cmbPatente = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(110, 14), Size = new Size(454, 24) };
            _cmbPatente.SelectedIndexChanged += (s, e) => { CargarMapeosPatente(); RefrescarListas(); };

            _lblForm = new Label { Text = T("map.formulario", "Formulario:"), Location = new Point(16, 50), AutoSize = true };
            _cmbFormulario = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(110, 46), Size = new Size(454, 24) };
            _cmbFormulario.SelectedIndexChanged += (s, e) => RefrescarListas();

            _lblDisp = new Label { Text = T("map.disponibles", "Controles disponibles"), Location = new Point(16, 84), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            _lstDisponibles = new ListBox { Location = new Point(16, 104), Size = new Size(230, 280), DisplayMember = "Display", HorizontalScrollbar = true };

            _lblAsoc = new Label { Text = T("map.asociados", "Asociados a la patente"), Location = new Point(334, 84), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(176, 62, 96) };
            _lstAsociados = new ListBox { Location = new Point(334, 104), Size = new Size(230, 280), DisplayMember = "Display", HorizontalScrollbar = true };

            _btnAgregar = Btn("Asociar ➜", new Point(258, 170), 64);
            _btnAgregar.Click += (s, e) => Asociar();
            _btnQuitar  = Btn("⬅ Quitar", new Point(258, 210), 64);
            _btnQuitar.Click += (s, e) => Quitar();

            _btnGuardar = new Button { Text = T("map.guardar", "💾 Guardar"), Location = new Point(334, 400), Size = new Size(140, 32), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(60, 140, 80), ForeColor = Color.White, Cursor = Cursors.Hand };
            _btnGuardar.FlatAppearance.BorderSize = 0;
            _btnGuardar.Click += (s, e) => Guardar();
            _btnCerrar = new Button { Text = T("btn.cerrar", "Cerrar"), Location = new Point(484, 400), Size = new Size(80, 32), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(220, 215, 225), ForeColor = Color.Black, Cursor = Cursors.Hand };
            _btnCerrar.FlatAppearance.BorderSize = 0;
            _btnCerrar.Click += (s, e) => this.Close();

            _lblEstado = new Label { Location = new Point(16, 405), Size = new Size(300, 40), ForeColor = Color.FromArgb(40, 140, 60) };

            this.Controls.AddRange(new Control[]
            {
                _lblPat, _cmbPatente, _lblForm, _cmbFormulario,
                _lblDisp, _lstDisponibles, _lblAsoc, _lstAsociados,
                _btnAgregar, _btnQuitar, _btnGuardar, _btnCerrar, _lblEstado
            });
        }

        private static Button Btn(string text, Point loc, int width) => new Button
        {
            Text = text, Location = loc, Size = new Size(width, 30), FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(210, 100, 135), ForeColor = Color.White, Font = new Font("Segoe UI", 8f), Cursor = Cursors.Hand
        };

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                _todosLosMapeos = _bll.ObtenerTodos();

                _cmbPatente.Items.Clear();
                foreach (var p in _famBLL.ObtenerPatentesDisponibles().OrderBy(p => p.Nombre))
                    _cmbPatente.Items.Add(new ItemPatente { Id = p.Id, Nombre = p.Nombre });

                _cmbFormulario.Items.Clear();
                foreach (string f in RegistroControles.Formularios())
                    if (!string.Equals(f, "Menu", StringComparison.OrdinalIgnoreCase))
                        _cmbFormulario.Items.Add(f);   // el Menú se gobierna por NombreMenu, no por mapeo de control

                if (_cmbFormulario.Items.Count == 0)
                    _lblEstado.Text = T("map.sinforms", "Abrí los formularios que quieras mapear y volvé a entrar acá.");

                if (_cmbPatente.Items.Count    > 0) _cmbPatente.SelectedIndex    = 0;
                if (_cmbFormulario.Items.Count > 0) _cmbFormulario.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, T("msg.error.titulo", "Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarMapeosPatente()
        {
            _mapeosPatente = PatenteId == 0 ? new List<BE.ControlMapeado>() : _bll.ObtenerPorPermiso(PatenteId);
        }

        private void RefrescarListas()
        {
            _lstDisponibles.Items.Clear();
            _lstAsociados.Items.Clear();
            if (PatenteId == 0 || FormSel == null) return;

            var controles = RegistroControles.Controles(FormSel);

            // Asociados a ESTA patente en ESTE form.
            var asociadosNombres = _mapeosPatente
                .Where(m => string.Equals(m.Formulario, FormSel, StringComparison.OrdinalIgnoreCase))
                .Select(m => m.NombreControl).ToList();

            // Controles tomados por OTRA patente (no se pueden re-asignar: UNIQUE por control).
            var tomadosPorOtra = new HashSet<string>(
                _todosLosMapeos
                    .Where(m => string.Equals(m.Formulario, FormSel, StringComparison.OrdinalIgnoreCase)
                                && m.IdPermiso != PatenteId)
                    .Select(m => m.NombreControl),
                StringComparer.OrdinalIgnoreCase);

            foreach (var c in controles)
            {
                if (asociadosNombres.Contains(c.Nombre, StringComparer.OrdinalIgnoreCase))
                    _lstAsociados.Items.Add(c);
                else if (!tomadosPorOtra.Contains(c.Nombre))
                    _lstDisponibles.Items.Add(c);
            }
        }

        private void Asociar()
        {
            if (PatenteId == 0 || FormSel == null) return;
            if (!(_lstDisponibles.SelectedItem is RegistroControles.ControlInfo ci)) return;
            _mapeosPatente.Add(new BE.ControlMapeado { IdPermiso = PatenteId, Formulario = FormSel, NombreControl = ci.Nombre });
            RefrescarListas();
        }

        private void Quitar()
        {
            if (!(_lstAsociados.SelectedItem is RegistroControles.ControlInfo ci)) return;
            _mapeosPatente.RemoveAll(m => string.Equals(m.Formulario, FormSel, StringComparison.OrdinalIgnoreCase)
                                       && string.Equals(m.NombreControl, ci.Nombre, StringComparison.OrdinalIgnoreCase));
            RefrescarListas();
        }

        private void Guardar()
        {
            if (PatenteId == 0) return;
            try
            {
                _bll.GuardarAsociados(PatenteId, _mapeosPatente);
                _todosLosMapeos = _bll.ObtenerTodos();
                ManejadorSeguridad.InvalidarCache();   // cambiaron los mapeos → refrescar la caché
                // Re-aplica en vivo a los forms abiertos.
                if (Seguridad.SessionManager.IsLoggedIn)
                    ManejadorSeguridad.ActualizarSeguridadFormulariosAbiertos(Seguridad.SessionManager.GetInstance().Usuario);
                _lblEstado.ForeColor = Color.FromArgb(40, 140, 60);
                _lblEstado.Text = "✓ " + T("map.guardado", "Mapeo guardado.");
            }
            catch (Exception ex)
            {
                _lblEstado.ForeColor = Color.FromArgb(180, 50, 50);
                _lblEstado.Text = "✗ " + ex.Message;
            }
        }
    }
}
