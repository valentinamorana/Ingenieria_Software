using System;
using System.Windows.Forms;
using BE;
using BLL;
using Seguridad;

namespace GUI
{
    public partial class InicioDeSesion : Form
    {
        public InicioDeSesion()
        {
            InitializeComponent();
        }

        private void btnIngresar_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtDocumento.Text) ||
                    string.IsNullOrWhiteSpace(txtClave.Text))
                {
                    MessageBox.Show("Complete todos los campos.", "Aviso",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                BLL_Usuario bllUsu = new BLL_Usuario();
                Usuario usuario = bllUsu.ObtenerPorDocumento(txtDocumento.Text.Trim());

                if (usuario == null || \!usuario.VerificarClave(txtClave.Text))
                {
                    MessageBox.Show("Documento o clave incorrectos.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (\!usuario.Estado)
                {
                    MessageBox.Show("Su cuenta está inactiva.", "Aviso",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Cargar permisos
                BLL_Permiso bllPerm = new BLL_Permiso();
                usuario.SetPermisos(bllPerm.ListarPermisosPorUsuario(usuario.IdUsuario));

                // Registrar auditoría — T06a
                BLL_AuditoriaSesion bllAud = new BLL_AuditoriaSesion();
                AuditoriaSesion aud = new AuditoriaSesion();
                aud.OUsuario             = usuario;
                aud.DescripcionAuditoria = "Inicio de sesion";
                bllAud.RegistrarAuditoriaSesion(aud);

                Inicio frmInicio = new Inicio(usuario);
                frmInicio.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
