using System;
using System.Windows.Forms;
using BE;
using BLL;
using Seguridad;

namespace GUI
{
    // Gestor de usuarios. Solo visible para usuarios con permiso GestorUsuarios.
    public partial class frmGestorUsuarios : Form
    {
        private readonly UsuarioBLL _bllUsuarios;
        private Usuario _usuarioEditando = null;

        public frmGestorUsuarios()
        {
            _bllUsuarios = new UsuarioBLL();
            InitializeComponent();
            cboRol.Items.AddRange(new object[]
            {
                "Administrador", "Supervisor", "Operador Logistico", "Empleado", "Cliente"
            });
            cboRol.SelectedIndex = 3;
            CargarUsuarios();
        }

        private void CargarUsuarios()
        {
            dgvUsuarios.DataSource = null;
            dgvUsuarios.DataSource = _bllUsuarios.GetAll();
        }

        private void dgvUsuarios_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvUsuarios.CurrentRow == null) return;
            var u = (Usuario)dgvUsuarios.CurrentRow.DataBoundItem;
            _usuarioEditando = u;
            txtNombre.Text = u.NombreCompleto;
            txtDocumento.Text = u.Documento;
            txtCorreo.Text = u.Correo;
            cboRol.Text = u.Rol;
            txtPassword.Text = string.Empty; // no mostrar la clave
        }

        // Carga el usuario seleccionado para edicion explicitamente
        private void btnEditar_Click(object sender, EventArgs e)
        {
            if (_usuarioEditando == null)
            {
                MessageBox.Show("Seleccione un usuario de la lista para editar.",
                    "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            txtNombre.Focus();
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text) ||
                string.IsNullOrWhiteSpace(txtDocumento.Text))
            {
                MessageBox.Show("Nombre y Documento son obligatorios.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Usuario u = _usuarioEditando ?? new Usuario();
            u.NombreCompleto = txtNombre.Text.Trim();
            u.Documento = txtDocumento.Text.Trim();
            u.Correo = txtCorreo.Text.Trim();
            u.Rol = cboRol.Text;
            u.Estado = true;
            // Solo re-hashear si se ingreso una nueva clave
            if (!string.IsNullOrWhiteSpace(txtPassword.Text))
                u.Password = Encriptador.Hash(txtPassword.Text.Trim());
            else if (_usuarioEditando == null)
                u.Password = Encriptador.Hash("1234"); // default para usuarios nuevos
            _bllUsuarios.Save(u);
            CargarUsuarios();
            LimpiarCampos();
            MessageBox.Show("Usuario guardado.", "Exito",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (_usuarioEditando == null)
            {
                MessageBox.Show("Seleccione un usuario para eliminar.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (MessageBox.Show("Eliminar al usuario '" + _usuarioEditando.NombreCompleto + "'?",
                    "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _bllUsuarios.Delete(_usuarioEditando);
                CargarUsuarios();
                LimpiarCampos();
            }
        }

        private void btnNuevo_Click(object sender, EventArgs e) { LimpiarCampos(); }

        private void LimpiarCampos()
        {
            _usuarioEditando = null;
            dgvUsuarios.ClearSelection();
            txtNombre.Text = string.Empty;
            txtDocumento.Text = string.Empty;
            txtCorreo.Text = string.Empty;
            txtPassword.Text = string.Empty;
            if (cboRol.Items.Count > 3) cboRol.SelectedIndex = 3;
            txtNombre.Focus();
        }
    }
}
