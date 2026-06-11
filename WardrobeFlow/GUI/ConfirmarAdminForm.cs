using BLL;
using Servicios.Multiidioma;
using System;
using System.Windows.Forms;

namespace GUI
{
    public partial class ConfirmarAdminForm : Form
    {
        private readonly Usuario _usuarioBLL = new Usuario();
        private readonly RecuperacionAdmin _recuperacionBLL = new RecuperacionAdmin();

        public bool Autorizado { get; private set; }

        public ConfirmarAdminForm()
        {
            InitializeComponent();
            Autorizado = false;
            try { string ico = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico"); if (System.IO.File.Exists(ico)) this.Icon = new System.Drawing.Icon(ico); } catch { }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Traducir(GestorIdioma.IdiomaActual);
        }

        private void Traducir(Servicios.Multiidioma.Idioma idioma)
        {
            var t = Traductor.ObtenerTraducciones(idioma);
            void Apl(Control c)
            {
                if (c?.Tag != null && t.ContainsKey(c.Tag.ToString()))
                    c.Text = t[c.Tag.ToString()].Texto;
            }
            if (this.Tag != null && t.ContainsKey(this.Tag.ToString()))
                this.Text = t[this.Tag.ToString()].Texto;
            Apl(lblTitulo); Apl(lblSubtitulo);
            Apl(lblUsuario); Apl(lblClave);
            Apl(btnConfirmar); Apl(btnCancelar);
        }

        private void btnConfirmar_Click(object sender, EventArgs e)
        {
            lblError.Text = string.Empty;

            var t = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
            string T(string k, string fb) => t.ContainsKey(k) ? t[k].Texto : fb;

            if (string.IsNullOrWhiteSpace(txtClave.Text))
            {
                lblError.Text = T("msg.confirmar.vacio", "Ingrese usuario y contraseña.");
                return;
            }

            try
            {
                // Vía 1 — Clave Maestra de Recuperación. Permite autorizar SIN usuario, para
                // cuando ningún admin puede ingresar. La validación (lectura de config + cripto)
                // la resuelve la BLL; la GUI solo reacciona al booleano. (Patrón de Stach.)
                if (_recuperacionBLL.ValidarClaveMaestra(txtClave.Text))
                {
                    Autorizado = true;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                    return;
                }

                // Vía 2 — Credenciales de un Administrador.
                if (string.IsNullOrWhiteSpace(txtUsuario.Text))
                {
                    lblError.Text = T("msg.confirmar.vacio", "Ingrese usuario y contraseña.");
                    return;
                }
                if (!_usuarioBLL.ValidarCredencialesAdmin(txtUsuario.Text.Trim(), txtClave.Text))
                {
                    lblError.Text = T("msg.confirmar.invalido", "Credenciales inválidas o el usuario no es Administrador.");
                    txtClave.Clear();
                    txtClave.Focus();
                    return;
                }

                Autorizado = true;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                lblError.Text = ex.Message;
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
