using System;
using System.Drawing;
using System.Windows.Forms;
using Servicios.Multiidioma;

namespace GUI
{
    /// <summary>
    /// "Mi Perfil" — preferencias del usuario en sesión (RF-22/23 + personalización de UI).
    /// El usuario ve sus datos y configura: idioma, tipografía, tamaño de letra, tema
    /// (claro/oscuro), formato de fecha y notificaciones. Todo se guarda en la BD
    /// (Usuario.IdIdioma + tabla Preferencia) y se aplica al instante (idioma por Observer;
    /// fuente/tema re-aplicados a los formularios abiertos).
    /// </summary>
    public class MiPerfilForm : Form, IIdiomaObserver
    {
        private readonly BE.Usuario  _usuario;
        private readonly BLL.Usuario _usuarioBLL = new BLL.Usuario();
        private BE.Preferencia       _pref;

        private Label    _lblTitulo, _lblUsuarioCap, _lblUsuarioVal, _lblPerfilCap, _lblPerfilVal, _lblSeccion;
        private Label    _lblIdiomaCap, _lblFuenteCap, _lblTamanoCap, _lblTemaCap, _lblFechaCap;
        private ComboBox _cmbIdioma, _cmbFuente, _cmbTamano, _cmbTema, _cmbFecha;
        private CheckBox _chkNotif;
        private Button   _btnGuardar, _btnDefault;
        private Label    _lblEstado;

        public MiPerfilForm(BE.Usuario usuario)
        {
            _usuario = usuario ?? new BLL.Usuario().ObtenerUsuarioActivo();
            try { _pref = new BLL.Preferencia().Obtener(_usuario?.Id ?? 0); }
            catch { _pref = new BE.Preferencia(); }
            BuildUI();
        }

        private string T(string k, string fb)
        {
            var t = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
            return t.ContainsKey(k) ? t[k].Texto : fb;
        }

        private void BuildUI()
        {
            this.Text            = T("perfil.frm.titulo", "Mi Perfil");
            this.ClientSize      = new Size(440, 474);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.Font            = new Font("Segoe UI", 9f);
            this.BackColor       = Color.White;

            _lblTitulo = new Label { Text = T("perfil.frm.titulo", "Mi Perfil"), Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = Color.FromArgb(176, 62, 96), Location = new Point(20, 14), AutoSize = true };

            _lblUsuarioCap = new Label { Text = T("perfil.usuario", "Usuario:"), Location = new Point(24, 52), AutoSize = true, ForeColor = Color.FromArgb(90,90,100) };
            _lblUsuarioVal = new Label { Text = _usuario?.Username ?? "—", Location = new Point(170, 52), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };

            _lblPerfilCap = new Label { Text = T("perfil.perfil", "Perfil / Rol:"), Location = new Point(24, 76), AutoSize = true, ForeColor = Color.FromArgb(90,90,100) };
            _lblPerfilVal = new Label { Text = TraductorPerfil.Nombre(_usuario?.Perfil), Location = new Point(170, 76), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };

            _lblSeccion = new Label { Text = T("perfil.seccion", "Preferencias"), Font = new Font("Segoe UI", 10f, FontStyle.Bold), ForeColor = Color.FromArgb(176, 62, 96), Location = new Point(20, 108), AutoSize = true };

            int xCap = 24, xCtl = 170, w = 244, y = 138, dy = 32;

            _lblIdiomaCap = MkCap(T("perfil.idioma", "Idioma preferido:"), xCap, y);
            _cmbIdioma    = MkCombo(xCtl, y, w);

            y += dy; _lblFuenteCap = MkCap(T("perfil.fuente", "Tipografía:"), xCap, y);
            _cmbFuente = MkCombo(xCtl, y, w);
            _cmbFuente.Items.AddRange(new object[] { "Segoe UI", "Verdana", "Calibri", "Tahoma", "Arial" });

            y += dy; _lblTamanoCap = MkCap(T("perfil.tamano", "Tamaño de letra:"), xCap, y);
            _cmbTamano = MkCombo(xCtl, y, w);
            _cmbTamano.Items.AddRange(new object[] { "Chico", "Normal", "Grande" });

            y += dy; _lblTemaCap = MkCap(T("perfil.tema", "Tema:"), xCap, y);
            _cmbTema = MkCombo(xCtl, y, w);
            _cmbTema.Items.AddRange(new object[] { "Claro", "Oscuro" });

            y += dy; _lblFechaCap = MkCap(T("perfil.fecha", "Formato de fecha:"), xCap, y);
            _cmbFecha = MkCombo(xCtl, y, w);
            _cmbFecha.Items.AddRange(new object[] { "dd/MM/yyyy", "dd-MM-yyyy", "yyyy-MM-dd", "MM/dd/yyyy" });

            y += dy; _chkNotif = new CheckBox { Text = T("perfil.notif", "Recibir notificaciones"), Location = new Point(xCtl, y), AutoSize = true };

            _btnGuardar = new Button { Text = T("perfil.btn.guardar", "Guardar preferencias"), Location = new Point(xCtl, y + 36), Size = new Size(w, 34), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(176, 62, 96), ForeColor = Color.White, Cursor = Cursors.Hand };
            _btnGuardar.FlatAppearance.BorderSize = 0;
            _btnGuardar.Click += (s, e) => Guardar();

            // Volver a la apariencia "de fábrica" (Segoe UI / Normal / Claro / dd-MM-yyyy / notif. on).
            _btnDefault = new Button { Text = T("perfil.btn.default", "Restaurar valores de fábrica"), Location = new Point(xCtl, y + 74), Size = new Size(w, 30), FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Color.FromArgb(176, 62, 96), Cursor = Cursors.Hand };
            _btnDefault.FlatAppearance.BorderColor = Color.FromArgb(210, 180, 195);
            _btnDefault.Click += (s, e) => RestaurarDefault();

            _lblEstado = new Label { Location = new Point(24, y + 112), Size = new Size(392, 28), ForeColor = Color.FromArgb(40, 140, 60) };

            this.Controls.AddRange(new Control[]
            {
                _lblTitulo, _lblUsuarioCap, _lblUsuarioVal, _lblPerfilCap, _lblPerfilVal, _lblSeccion,
                _lblIdiomaCap, _cmbIdioma, _lblFuenteCap, _cmbFuente, _lblTamanoCap, _cmbTamano,
                _lblTemaCap, _cmbTema, _lblFechaCap, _cmbFecha, _chkNotif, _btnGuardar, _btnDefault, _lblEstado
            });
        }

        private Label MkCap(string txt, int x, int y) =>
            new Label { Text = txt, Location = new Point(x, y + 3), AutoSize = true, ForeColor = Color.FromArgb(90, 90, 100) };

        private ComboBox MkCombo(int x, int y, int w) =>
            new ComboBox { Location = new Point(x, y), Width = w, DropDownStyle = ComboBoxStyle.DropDownList };

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try { string ico = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico"); if (System.IO.File.Exists(ico)) this.Icon = new Icon(ico); } catch { }
            GestorIdioma.SuscribirObservador(this);
            CargarIdiomas();
            CargarPreferencias();
            try { PreferenciasUI.Aplicar(this); } catch { }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            GestorIdioma.DesuscribirObservador(this);
            base.OnFormClosing(e);
        }

        // Combo de idioma: cargado EN VIVO desde la tabla Idioma (un idioma nuevo aparece solo).
        private void CargarIdiomas()
        {
            System.Collections.Generic.IList<Idioma> idiomas;
            try { idiomas = new BLL.IdiomaService().ObtenerIdiomasActivosComoIdioma(); }
            catch { idiomas = Traductor.ObtenerIdiomas(); }

            _cmbIdioma.DisplayMember = "Nombre";
            _cmbIdioma.ValueMember   = "Id";
            _cmbIdioma.DataSource     = idiomas;

            string actual = _usuario?.IdIdioma ?? GestorIdioma.IdiomaActual?.Id ?? "ES";
            for (int i = 0; i < idiomas.Count; i++)
                if (string.Equals(idiomas[i].Id, actual, StringComparison.OrdinalIgnoreCase)) { _cmbIdioma.SelectedIndex = i; break; }
        }

        // Preselecciona los combos de preferencias con lo guardado en BD.
        private void CargarPreferencias()
        {
            SeleccionarOAgregar(_cmbFuente, _pref.FuenteFamilia, "Segoe UI");
            SeleccionarOAgregar(_cmbTamano, _pref.FuenteTamano,  "Normal");
            SeleccionarOAgregar(_cmbTema,   _pref.Tema,          "Claro");
            SeleccionarOAgregar(_cmbFecha,  _pref.FormatoFecha,  "dd/MM/yyyy");
            _chkNotif.Checked = _pref.Notificaciones;
        }

        private static void SeleccionarOAgregar(ComboBox cmb, string valor, string fallback)
        {
            string v = string.IsNullOrEmpty(valor) ? fallback : valor;
            int idx = cmb.Items.IndexOf(v);
            if (idx < 0) idx = cmb.Items.Add(v);
            cmb.SelectedIndex = idx;
        }

        private void Guardar()
        {
            try
            {
                // 1) Preferencias de UI → BD (tabla Preferencia).
                var pref = new BE.Preferencia
                {
                    IdUsuario      = _usuario.Id,
                    FuenteFamilia  = _cmbFuente.SelectedItem?.ToString() ?? "Segoe UI",
                    FuenteTamano   = _cmbTamano.SelectedItem?.ToString() ?? "Normal",
                    Tema           = _cmbTema.SelectedItem?.ToString()   ?? "Claro",
                    FormatoFecha   = _cmbFecha.SelectedItem?.ToString()  ?? "dd/MM/yyyy",
                    Notificaciones = _chkNotif.Checked
                };
                new BLL.Preferencia().Guardar(pref);
                _pref = pref;
                PreferenciasUI.Set(pref);

                // 2) Idioma → Usuario.IdIdioma + aplicar por Observer.
                var idioma = _cmbIdioma.SelectedItem as Idioma;
                if (idioma != null)
                {
                    _usuarioBLL.GuardarPreferenciaIdioma(_usuario.Id, idioma.Id);
                    if (_usuario != null) _usuario.IdIdioma = idioma.Id;
                    try { GestorIdioma.CambiarIdioma(idioma, new BLL.IdiomaService().CargarTraducciones(idioma.Id)); }
                    catch { GestorIdioma.CambiarIdioma(idioma); }
                }

                // 3) Aplicar fuente/tema en vivo a todos los formularios abiertos.
                PreferenciasUI.ReaplicarTodo();

                _lblEstado.ForeColor = Color.FromArgb(40, 140, 60);
                _lblEstado.Text = "✓ " + T("perfil.guardado", "Preferencias guardadas.");
            }
            catch (Exception ex)
            {
                _lblEstado.ForeColor = Color.FromArgb(180, 50, 50);
                _lblEstado.Text = "✗ " + ex.Message;
            }
        }

        public void UpdateLanguage(Idioma idioma)
        {
            this.Text           = T("perfil.frm.titulo", "Mi Perfil");
            _lblTitulo.Text     = T("perfil.frm.titulo", "Mi Perfil");
            _lblUsuarioCap.Text = T("perfil.usuario", "Usuario:");
            _lblPerfilCap.Text  = T("perfil.perfil", "Perfil / Rol:");
            _lblPerfilVal.Text  = TraductorPerfil.Nombre(_usuario?.Perfil);
            _lblSeccion.Text    = T("perfil.seccion", "Preferencias");
            _lblIdiomaCap.Text  = T("perfil.idioma", "Idioma preferido:");
            _lblFuenteCap.Text  = T("perfil.fuente", "Tipografía:");
            _lblTamanoCap.Text  = T("perfil.tamano", "Tamaño de letra:");
            _lblTemaCap.Text    = T("perfil.tema", "Tema:");
            _lblFechaCap.Text   = T("perfil.fecha", "Formato de fecha:");
            _chkNotif.Text      = T("perfil.notif", "Recibir notificaciones");
            _btnGuardar.Text    = T("perfil.btn.guardar", "Guardar preferencias");
            _btnDefault.Text    = T("perfil.btn.default", "Restaurar valores de fábrica");
        }

        // Restaura las preferencias de UI a los valores por defecto y las guarda/aplica.
        // (El idioma no se toca: es una preferencia aparte.)
        private void RestaurarDefault()
        {
            SeleccionarOAgregar(_cmbFuente, "Segoe UI",   "Segoe UI");
            SeleccionarOAgregar(_cmbTamano, "Normal",     "Normal");
            SeleccionarOAgregar(_cmbTema,   "Claro",      "Claro");
            SeleccionarOAgregar(_cmbFecha,  "dd/MM/yyyy", "dd/MM/yyyy");
            _chkNotif.Checked = true;
            Guardar();
        }

    }
}
