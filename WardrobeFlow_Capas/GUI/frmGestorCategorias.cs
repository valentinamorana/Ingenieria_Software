using System;
using System.Windows.Forms;
using BE;
using BLL;

namespace GUI
{
    // Gestor de categorias de prendas.
    // Permite ver, agregar, editar y eliminar categorias.
    public partial class frmGestorCategorias : Form
    {
        // BLL de categorias 
        private readonly CategoriaBLL _bllCategorias;

        // Constructor: carga la lista de categorias al iniciar
        public frmGestorCategorias()
        {
            _bllCategorias = new CategoriaBLL();
            InitializeComponent();
            CargarCategorias();
        }

        // Recarga el DataGridView con todas las categorias activas
        private void CargarCategorias()
        {
            dgvCategorias.DataSource = null;
            dgvCategorias.DataSource = _bllCategorias.GetAll();
        }

        // Cuando se selecciona una fila en el grid, carga los datos en los campos
        private void dgvCategorias_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvCategorias.CurrentRow == null) return;
            var cat = (Categoria)dgvCategorias.CurrentRow.DataBoundItem;
            txtNombre.Text      = cat.Nombre;
            txtDescripcion.Text = cat.Descripcion;
        }

        // Guarda una nueva categoria o actualiza la existente
        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre es obligatorio.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Categoria cat;
            // Si hay una fila seleccionada, editar esa categoria
            if (dgvCategorias.CurrentRow != null &&
                dgvCategorias.CurrentRow.DataBoundItem is Categoria catSel)
            {
                cat = catSel;
            }
            else
            {
                // Nueva categoria
                cat = new Categoria();
            }

            cat.Nombre      = txtNombre.Text.Trim();
            cat.Descripcion = txtDescripcion.Text.Trim();
            cat.Estado      = true;

            // Save: si es nueva la agrega, si ya existe la actualiza (por referencia)
            _bllCategorias.Save(cat);

            CargarCategorias();
            LimpiarCampos();
            MessageBox.Show("Categoria guardada correctamente.", "Exito",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Elimina la categoria seleccionada
        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (dgvCategorias.CurrentRow == null) return;
            var cat = (Categoria)dgvCategorias.CurrentRow.DataBoundItem;

            if (MessageBox.Show("Eliminar la categoria '" + cat.Nombre + "'?", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _bllCategorias.Delete(cat);
                CargarCategorias();
                LimpiarCampos();
            }
        }

        // Limpia los campos del formulario
        private void btnNuevo_Click(object sender, EventArgs e)
        {
            LimpiarCampos();
        }

        // Auxiliar: vacia los campos de texto
        private void LimpiarCampos()
        {
            txtNombre.Text      = string.Empty;
            txtDescripcion.Text = string.Empty;
        }
    }
}
