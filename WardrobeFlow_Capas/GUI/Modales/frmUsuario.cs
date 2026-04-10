using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BE;
using BLL;

namespace GUI.Modales
{
    public partial class frmUsuario : Form
    {
        private BLL_Usuario _bll = new BLL_Usuario();
        private List<Usuario> _lista;
        private Usuario _seleccionado;

        public frmUsuario() { InitializeComponent(); }

        private void frmUsuario_Load(object sender, EventArgs e)
        {
            cboRol.Items.AddRange(new[] { "Administrador", "Empleado", "Usuario" });
            CargarGrilla();
        }

        private void CargarGrilla()
        {
            _lista = _bll.ListarUsuarios();
            dgvUsuarios.DataSource = null;
            dgvUsuarios.DataSource = _lista;
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                Usuario u = ObtenerDesdeFormulario();
                string msg = _seleccionado == null
                    ? _bll.AgregarUsuario(u)
                    : _bll.EditarUsuario(u);
                MessageBox.Show(msg);
                Limpiar();
                CargarGrilla();
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (_seleccionado == null) return;
            if (MessageBox.Show("¿Eliminar usuario?", "Confirmar",
                MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                MessageBox.Show(_bll.EliminarUsuario(_seleccionado.IdUsuario));
                Limpiar();
                CargarGrilla();
            }
        }

        private void btnLimpiar_Click(object sender, EventArgs e) { Limpiar(); }

        private void dgvUsuarios_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            _seleccionado = _lista[e.RowIndex];
            txtNombre.Text   = _seleccionado.NombreCompleto;
            txtCorreo.Text   = _seleccionado.Correo;
            txtDocumento.Text = _seleccionado.Documento;
            cboRol.Text      = _seleccionado.Rol;
            chkEstado.Checked = _seleccionado.Estado;
            txtClave.Text    = string.Empty;
        }

        private Usuario ObtenerDesdeFormulario()
        {
            Usuario u = _seleccionado ?? new Usuario();
            u.NombreCompleto = txtNombre.Text.Trim();
            u.Correo         = txtCorreo.Text.Trim();
            u.Documento      = txtDocumento.Text.Trim();
            u.Rol            = cboRol.Text;
            u.Estado         = chkEstado.Checked;
            if (\!string.IsNullOrWhiteSpace(txtClave.Text))
                u.SetClave(txtClave.Text);
            return u;
        }

        private void Limpiar()
        {
            _seleccionado = null;
            txtNombre.Text = txtCorreo.Text = txtDocumento.Text = txtClave.Text = string.Empty;
            cboRol.SelectedIndex = -1;
            chkEstado.Checked = true;
        }
    }
}
