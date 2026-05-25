using BLL;
using System;
using System.Windows.Forms;

namespace GUI
{
    public partial class ConfirmarAdminForm : Form
    {
        private readonly Usuario _usuarioBLL = new Usuario();

        public bool Autorizado { get; private set; }

        public ConfirmarAdminForm()
        {
            InitializeComponent();
            Autorizado = false;
        }

        private void btnConfirmar_Click(object sender, EventArgs e)
        {
            lblError.Text = string.Empty;

            if (string.IsNullOrWhiteSpace(txtUsuario.Text) || string.IsNullOrWhiteSpace(txtClave.Text))
            {
                lblError.Text = "Ingrese usuario y contraseña.";
                return;
            }

            try
            {
                if (!_usuarioBLL.ValidarCredencialesAdmin(txtUsuario.Text.Trim(), txtClave.Text))
                {
                    lblError.Text = "Credenciales inválidas o el usuario no es Administrador.";
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
