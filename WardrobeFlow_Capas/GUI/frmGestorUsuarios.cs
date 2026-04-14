using System;
using System.Windows.Forms;
using BE;
using BLL;
using Seguridad;

namespace GUI
{
    // Gestor de usuarios del sistema WardrobeFlow.
    // Solo visible para usuarios con permiso GestorUsuarios.
    public partial class frmGestorUsuarios : Form
    {
        // BLL de usuarios para operaciones CRUD
        private readonly UsuarioBLL _bllUsuarios;

        // Constructor: carga la lista de usuarios
        public frmGestorUsuarios()
        {
            _bllUsuarios = new UsuarioBLL();
            InitializeComponent();
            // Cargar roles disponibles en el combo
            cboRol.Items.AddRange(new object[] { "Administrador", "Empleado", "Usuario" });
            cboRol.SelectedIndex = 1;
            CargarUsuarios();
        }

        // Recarga el grid con todos los usuarios
        private void CargarUsuarios()
        {
            dgvUsuarios.DataSource = null;
            dgvUsuarios.DataSource = _bllUsuarios.GetAll();
        }

        // Al seleccionar un usuario en el grid, llena los campos
        private void dgvUsuarios_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvUsuarios.CurrentRow == null) return;
            var u = (Usuario)dgvUsuarios.CurrentRow.DataBoundItem;
            txtNombre.Text    = u.NombreCompleto;
            txtDocumento.Text = u.Documento;
            txtCorreo.Text    = u.Correo;
            cboRol.Text       = u.Rol;
            // No mostrar la clave por seguridad
            txtPassword.Text  = string.Empty;
        }

        // Guarda un usuario nuevo o actualiza el existente
        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text) ||
                string.IsNullOrWhiteSpace(txtDocumento.Text))
            {
                MessageBox.Show("Nombre y Documento son obligatorios.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Usuario u;
            if (dgvUsuarios.CurrentRow != null &&
                dgvUsuarios.CurrentRow.DataBoundItem is Usuario uSel &&
                string.IsNullOrEmpty(txtPassword.Text))
            {
                // Edicion sin cambiar clave
                u = uSel;
            }
            else
            {
                // Nuevo usuario o cambio de clave
                u = new Usuario();
                if (!string.IsNullOrEmpty(txtPassword.Text))
                    u.Password = Encriptador.Hash(txtPassword.Text);
                else
                    u.Password = Encriptador.Hash("1234"); // clave por defecto
            }

            u.NombreCompleto = txtNombre.Text.Trim();
            u.Documento      = txtDocumento.Text.Trim();
            u.Correo         = txtCorreo.Text.Trim();
            u.Rol            = cboRol.Text;
            u.Estado         = true;

            _bllUsuarios.Save(u);
            CargarUsuarios();
            LimpiarCampos();
            MessageBox.Show("Usuario guardado.", "Exito",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Elimina el usuario seleccionado
        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (dgvUsuarios.CurrentRow == null) return;
            var u = (Usuario)dgvUsuarios.CurrentRow.DataBoundItem;
            if (MessageBox.Show("Eliminar al usuario '" + u.NombreCompleto + "'?",
                    "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _bllUsuarios.Delete(u);
                CargarUsuarios();
                LimpiarCampos();
            }
        }

        private void btnNuevo_Click(object sender, EventArgs e) { LimpiarCampos(); }

        private void LimpiarCampos()
        {
            txtNombre.Text    = string.Empty;
            txtDocumento.Text = string.Empty;
            txtCorreo.Text    = string.Empty;
            txtPassword.Text  = string.Empty;
        }
    }
}
