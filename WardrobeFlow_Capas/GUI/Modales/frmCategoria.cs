using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BE;
using BLL;

namespace GUI.Modales
{
    public partial class frmCategoria : Form
    {
        private BLL_Categoria _bll = new BLL_Categoria();
        private List<Categoria> _lista;
        private Categoria _seleccionada;

        public frmCategoria() { InitializeComponent(); }

        private void frmCategoria_Load(object sender, EventArgs e) { CargarGrilla(); }

        private void CargarGrilla()
        {
            _lista = _bll.ListarCategorias();
            dgvCategorias.DataSource = null;
            dgvCategorias.DataSource = _lista;
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                Categoria c = ObtenerDesdeFormulario();
                string msg = _seleccionada == null
                    ? _bll.AgregarCategoria(c)
                    : _bll.EditarCategoria(c);
                MessageBox.Show(msg);
                Limpiar();
                CargarGrilla();
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (_seleccionada == null) return;
            if (MessageBox.Show("¿Eliminar categoria?", "Confirmar",
                MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                MessageBox.Show(_bll.EliminarCategoria(_seleccionada.IdCategoria));
                Limpiar();
                CargarGrilla();
            }
        }

        private void btnLimpiar_Click(object sender, EventArgs e) { Limpiar(); }

        private void dgvCategorias_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            _seleccionada = _lista[e.RowIndex];
            txtNombre.Text      = _seleccionada.Nombre;
            txtDescripcion.Text = _seleccionada.Descripcion;
            chkEstado.Checked   = _seleccionada.Estado;
        }

        private Categoria ObtenerDesdeFormulario()
        {
            Categoria c = _seleccionada ?? new Categoria();
            c.Nombre      = txtNombre.Text.Trim();
            c.Descripcion = txtDescripcion.Text.Trim();
            c.Estado      = chkEstado.Checked;
            return c;
        }

        private void Limpiar()
        {
            _seleccionada = null;
            txtNombre.Text = txtDescripcion.Text = string.Empty;
            chkEstado.Checked = true;
        }
    }
}
