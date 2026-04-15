using System;
using System.Windows.Forms;
using BE;
using BLL;

namespace GUI
{
    // Gestor de prendas. Usa el patron DECORATOR para la descripcion enriquecida.
    public partial class frmGestorPrendas : Form
    {
        private readonly PrendaBLL _bllPrendas;
        private readonly CategoriaBLL _bllCategorias;
        private Prenda _prendaEditando = null;

        public frmGestorPrendas()
        {
            _bllCategorias = new CategoriaBLL();
            _bllPrendas = new PrendaBLL(_bllCategorias);
            InitializeComponent();
            cboCategorias.DataSource = _bllCategorias.GetAll();
            cboCategorias.DisplayMember = "Nombre";
            cboTemporada.Items.AddRange(new object[] { "Verano", "Invierno", "Otoño", "Primavera", "Todo el año" });
            cboTemporada.SelectedIndex = 0;
            CargarPrendas();
        }

        private void CargarPrendas()
        {
            dgvPrendas.DataSource = null;
            dgvPrendas.DataSource = _bllPrendas.GetAll();
            lblDescripcion.Text = string.Empty;
        }

        private void dgvPrendas_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvPrendas.CurrentRow == null) return;
            var p = (Prenda)dgvPrendas.CurrentRow.DataBoundItem;
            _prendaEditando = p;
            txtNombre.Text = p.Nombre;
            txtColor.Text = p.Color;
            txtTalla.Text = p.Talla;
            cboTemporada.Text = p.Temporada;
            foreach (Categoria cat in cboCategorias.Items)
            {
                if (cat.Id == p.OCategoria?.Id) { cboCategorias.SelectedItem = cat; break; }
            }
            // Mostrar descripcion con el DECORATOR
            string desc = _bllPrendas.ObtenerDescripcionDecorada(p, "Casual");
            lblDescripcion.Text = "Descripcion: " + desc;
        }

        // Carga la prenda seleccionada para edicion explicitamente
        private void btnEditar_Click(object sender, EventArgs e)
        {
            if (_prendaEditando == null)
            {
                MessageBox.Show("Seleccione una prenda de la lista para editar.",
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
            Prenda p = _prendaEditando ?? new Prenda();
            p.Nombre = txtNombre.Text.Trim();
            p.Color = txtColor.Text.Trim();
            p.Talla = txtTalla.Text.Trim();
            p.Temporada = cboTemporada.Text;
            p.OCategoria = (Categoria)cboCategorias.SelectedItem;
            p.Estado = true;
            _bllPrendas.Save(p);
            CargarPrendas();
            LimpiarCampos();
            MessageBox.Show("Prenda guardada.", "Exito",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (_prendaEditando == null)
            {
                MessageBox.Show("Seleccione una prenda para eliminar.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (MessageBox.Show("Eliminar '" + _prendaEditando.Nombre + "'?", "Confirmar",
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _bllPrendas.Delete(_prendaEditando);
                CargarPrendas();
                LimpiarCampos();
            }
        }

        private void btnNuevo_Click(object sender, EventArgs e) { LimpiarCampos(); }

        private void LimpiarCampos()
        {
            _prendaEditando = null;
            dgvPrendas.ClearSelection();
            txtNombre.Text = string.Empty;
            txtColor.Text = string.Empty;
            txtTalla.Text = string.Empty;
            lblDescripcion.Text = string.Empty;
            if (cboTemporada.Items.Count > 0) cboTemporada.SelectedIndex = 0;
            txtNombre.Focus();
        }
    }
}
