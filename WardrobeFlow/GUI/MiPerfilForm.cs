using System;
using System.Drawing;
using System.Windows.Forms;
using Servicios.Multiidioma;

namespace GUI
{
    /// <summary>
    /// "Mi Perfil" — preferencias del usuario en sesión (RF-22/23).
    /// Permite ver sus datos y elegir su IDIOMA preferido desde un combo cargado en vivo
    /// desde la BD (tabla Idioma): agregar un idioma nuevo en BD lo hace aparecer acá sin tocar
    /// código. La preferencia se guarda en Usuario.IdIdioma y se aplica al instante por Observer.
    /// </summary>
    public class MiPerfilForm : Form, IIdiomaObserver
    {
        private readonly BE.Usuario _usuario;
        private readonly BLL.Usuario _usuarioBLL = new BLL.Usuario();

        private Label    _lblTitulo;
        private Label    _lblUsuarioCap, _lblUsuarioVal;
        private Label    _lblPerfilCap,  _lblPerfilVal;
        private Label    _lblIdiomaCap;
        private ComboBox _cmbIdioma;
        private Button   _btnGuardar;
        private Label    _lblEstado;

        public MiPerfilForm(BE.Usuario usuario)
        {
            _usuario = usuario ?? new BLL.Usuario().ObtenerUsuarioActivo();
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
            this.ClientSize      = new Size(420, 250);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.Font            = new Font("Segoe UI", 9f);
            this.BackColor       = Color.White;

            _lblTitulo = new Label
            {
                Text = T("perfil.frm.titulo", "Mi Perfil"),
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.FromArgb(146, 62, 96),
                Location = new Point(20, 16), AutoSize = true
            };

            _lblUsuarioCap = new Label { Text = T("perfil.usuario", "Usuario:"), Location = new Point(24, 64), AutoSize = true, ForeColor = Color.FromArgb(90,90,100) };
            _lblUsuarioVal = new Label { Text = _usuario?.Username ?? "—", Location = new Point(150, 64), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };

            _lblPerfilCap = new Label { Text = T("perfil.perfil", "Perfil / Rol:"), Location = new Point(24, 92), AutoSize = true, ForeColor = Color.FromArgb(90,90,100) };
            _lblPerfilVal = new Label { Text = TraducirPerfil(_usuario?.Perfil), Location = new Point(150, 92), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };

            _lblIdiomaCap = new Label { Text = T("perfil.idioma", "Idioma preferido:"), Location = new Point(24, 128), AutoSize = true, ForeColor = Color.FromArgb(90,90,100) };
            _cmbIdioma = new ComboBox
            {
                Location = new Point(150, 124),
                Width = 240,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            _btnGuardar = new Button
            {
                Text = T("perfil.btn.guardar", "Guardar preferencias"),
                Location = new Point(150, 168), Size = new Size(240, 34),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(146, 62, 96), ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            _btnGuardar.FlatAppearance.BorderSize = 0;
            _btnGuardar.Click += (s, e) => Guardar();

            _lblEstado = new Label { Location = new Point(24, 212), Size = new Size(372, 28), ForeColor = Color.FromArgb(40, 140, 60) };

            this.Controls.AddRange(new Control[]
            {
                _lblTitulo, _lblUsuarioCap, _lblUsuarioVal, _lblPerfilCap, _lblPerfilVal,
                _lblIdiomaCap, _cmbIdioma, _btnGuardar, _lblEstado
            });
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try { string ico = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico"); if (System.IO.File.Exists(ico)) this.Icon = new Icon(ico); } catch { }
            GestorIdioma.SuscribirObservador(this);
            CargarIdiomas();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            GestorIdioma.DesuscribirObservador(this);
            base.OnFormClosing(e);
        }

        // Carga el combo con los idiomas ACTIVOS desde BD (escalable: nuevos idiomas aparecen solos).
        private void CargarIdiomas()
        {
            System.Collections.Generic.IList<Idioma> idiomas;
            try { idiomas = new BLL.IdiomaService().ObtenerIdiomasActivosComoIdioma(); }
            catch { idiomas = Traductor.ObtenerIdiomas(); }

            _cmbIdioma.DisplayMember = "Nombre";
            _cmbIdioma.ValueMember   = "Id";
            _cmbIdioma.DataSource    = idiomas;

            // Preseleccionar el idioma guardado del usuario.
            string actual = _usuario?.IdIdioma ?? GestorIdioma.IdiomaActual?.Id ?? "ES";
            for (int i = 0; i < idiomas.Count; i++)
                if (string.Equals(idiomas[i].Id, actual, StringComparison.OrdinalIgnoreCase)) { _cmbIdioma.SelectedIndex = i; break; }
        }

        private void Guardar()
        {
            var idioma = _cmbIdioma.SelectedItem as Idioma;
            if (idioma == null) return;

            try
            {
                // 1) Persistir la preferencia en BD (Usuario.IdIdioma).
                _usuarioBLL.GuardarPreferenciaIdioma(_usuario.Id, idioma.Id);
                if (_usuario != null) _usuario.IdIdioma = idioma.Id;

                // 2) Aplicar al instante en toda la app (Observer notifica a todos los formularios).
                try
                {
                    var dict = new BLL.IdiomaService().CargarTraducciones(idioma.Id);
                    GestorIdioma.CambiarIdioma(idioma, dict);
                }
                catch { GestorIdioma.CambiarIdioma(idioma); }

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
            this.Text            = T("perfil.frm.titulo", "Mi Perfil");
            _lblTitulo.Text      = T("perfil.frm.titulo", "Mi Perfil");
            _lblUsuarioCap.Text  = T("perfil.usuario", "Usuario:");
            _lblPerfilCap.Text   = T("perfil.perfil", "Perfil / Rol:");
            _lblPerfilVal.Text   = TraducirPerfil(_usuario?.Perfil);
            _lblIdiomaCap.Text   = T("perfil.idioma", "Idioma preferido:");
            _btnGuardar.Text     = T("perfil.btn.guardar", "Guardar preferencias");
        }

        // Traduce el código de perfil al nombre visible en el idioma activo.
        private string TraducirPerfil(string perfil)
        {
            if (string.IsNullOrEmpty(perfil)) return "—";
            var t = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
            string Key(string k) => t.ContainsKey(k) ? t[k].Texto : null;
            switch (perfil)
            {
                case "Administrador":        return Key("perfil.administrador")    ?? perfil;
                case "Auditor":              return Key("perfil.auditor")          ?? perfil;
                case "GerenteComercial":     return Key("perfil.gerentecomercial") ?? perfil;
                case "Vendedor":             return Key("perfil.vendedor")         ?? perfil;
                case "GerenteInventario":    return Key("perfil.gerenteinventario")?? perfil;
                case "EncargadoDeStock":     return Key("perfil.encargadodestock") ?? perfil;
                case "OperadorLogistico":    return Key("perfil.operadorlogistico")?? perfil;
                case "Supervisor":           return Key("perfil.supervisor")       ?? perfil;
                case "ControladorDeStock":   return Key("perfil.stock")            ?? perfil;
                case "OperadorDeInventario": return Key("perfil.operador")         ?? perfil;
                default:                     return perfil;
            }
        }
    }
}
