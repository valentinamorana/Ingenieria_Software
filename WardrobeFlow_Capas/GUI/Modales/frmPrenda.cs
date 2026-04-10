using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BE;
using BLL;

namespace GUI.Modales
{
    public partial class frmPrenda : Form
    {
        private BLL_Prenda    _bll    = new BLL_Prenda();
        private BLL_Categoria _bllCat = new BLL_Categoria();
        private List<Prenda>  _lista;
        private Prenda        _seleccionada;
        private Usuario       _usuarioActual;

        public frmPrenda(Usuario usuario)
        {
            InitializeComponent();
            _usuarioActual = usuario;
        }

        private void frmPrenda_Load(object sender, EventArgs e)
        {
            cboTemporada.Items.AddRange(new[] { "Verano", "Otoño", "Invierno", "Primavera" });
            cboCategorias.DataSource    = _bllCat.ListarCategorias();
            cboCategorias.DisplayMember = "Nombre";
            cboCategorias.ValueMember   = "IdCategoria";
            CargarGrilla();
        }

        private void CargarGrilla()
        {
            _lista = _bll.ListarPrendas();
            dgvPrendas.DataSource = null;
            dgvPrendas.DataSource = _lista;
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                Prenda p = ObtenerDesdeFormulario();
                string msg = _seleccionada == null
                    ? _bll.AgregarPrenda(p)
                    : _bll.EditarPrenda(p);
                MessageBox.Show(msg);
                Limpiar();
                CargarGrilla();
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (_seleccionada == null) return;
            if (MessageBox.Show("¿Eliminar prenda?", "Confirmar",
                MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                MessageBox.Show(_bll.EliminarPrenda(_seleccionada.IdPrenda));
                Limpiar();
                CargarGrilla();
            }
        }

        private void btnLimpiar_Click(object sender, EventArgs e) { Limpiar(); }

        private void dgvPrendas_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            _seleccionada    = _lista[e.RowIndex];
            txtNombre.Text   = _seleccionada.Nombre;
            txtColor.Text    = _seleccionada.Color;
            txtTalla.Text    = _seleccionada.Talla;
            cboTemporada.Text = _seleccionada.Temporada;
            cboCategorias.SelectedValue = _seleccionada.IdCategoria;
            chkEstado.Checked = _seleccionada.Estado;
        }

        private Prenda ObtenerDesdeFormulario()
        {
            Prenda p = _seleccionada ?? new Prenda();
            p.Nombre      = txtNombre.Text.Trim();
            p.Color       = txtColor.Text.Trim();
            p.Talla       = txtTalla.Text.Trim();
            p.Temporada   = cboTemporada.Text;
            p.IdCategoria = (int)cboCategorias.SelectedValue;
            p.Estado      = chkEstado.Checked;
            return p;
        }

        private void Limpiar()
        {
            _seleccionada = null;
            txtNombre.Text = txtColor.Text = txtTalla.Text = string.Empty;
            cboTemporada.SelectedIndex = -1;
            chkEstado.Checked = true;
        }
    }
}
