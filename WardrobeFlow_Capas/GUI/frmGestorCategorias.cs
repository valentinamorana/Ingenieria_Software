using System;
using System.Windows.Forms;
using BE;
using BLL;

namespace GUI
{
    public partial class frmGestorCategorias : Form
    {
        private readonly CategoriaBLL _bllCategorias;
        private Categoria _categoriaEditando = null;

        public frmGestorCategorias()
        {
            _bllCategorias = new CategoriaBLL();
            InitializeComponent();
            CargarCategorias();
        }

        private void CargarCategorias()
        {
            dgvCategorias.DataSource = null;
            dgvCategorias.DataSource = _bllCategorias.GetAll();
        }

        private void dgvCategorias_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvCategorias.CurrentRow == null) return;
            var cat = (Categoria)dgvCategorias.CurrentRow.DataBoundItem;
            _categoriaEditando = cat;
            txtNombre.Text = cat.Nombre;
            txtDescripcion.Text = cat.Descripcion;
        }

        // Carga la categoria seleccionada para edicion explicitamente
        private void btnEditar_Click(object sender, EventArgs e)
        {
            if (_categoriaEditando == null)
            {
                MessageBox.Show("Seleccione una categoria de la lista para editar.",
                    "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            txtNombre.Focus();
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre es obligatorio.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Categoria cat = _categoriaEditando ?? new Categoria();
            cat.Nombre = txtNombre.Text.Trim();
            cat.Descripcion = txtDescripcion.Text.Trim();
            cat.Estado = true;
            _bllCategorias.Save(cat);
            CargarCategorias();
            LimpiarCampos();
            MessageBox.Show("Categoria guardada correctamente.", "Exito",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (_categoriaEditando == null)
            {
                MessageBox.Show("Seleccione una categoria para eliminar.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (MessageBox.Show("Eliminar la categoria '" + _categoriaEditando.Nombre + "'?",
                    "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _bllCategorias.Delete(_categoriaEditando);
                CargarCategorias();
                LimpiarCampos();
            }
        }

        private void btnNuevo_Click(object sender, EventArgs e) { LimpiarCampos(); }

        private void LimpiarCampos()
        {
            _categoriaEditando = null;
            dgvCategorias.ClearSelection();
            txtNombre.Text = string.Empty;
            txtDescripcion.Text = string.Empty;
            txtNombre.Focus();
        }
    }
}
