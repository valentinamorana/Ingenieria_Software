using System;
using System.Windows.Forms;
using BE;
using BLL;

namespace GUI
{
    // Gestor de outfits. Usa el patron DECORATOR en ObtenerResumenOutfit().
    public partial class frmGestorOutfits : Form
    {
        private readonly OutfitBLL _bllOutfits;
        private readonly PrendaBLL _bllPrendas;
        private readonly CategoriaBLL _bllCategorias;
        private Outfit _outfitEditando = null;

        public frmGestorOutfits()
        {
            _bllCategorias = new CategoriaBLL();
            _bllPrendas = new PrendaBLL(_bllCategorias);
            _bllOutfits = new OutfitBLL(_bllPrendas);
            InitializeComponent();
            cboOcasion.Items.AddRange(new object[] { "Casual", "Formal", "Deportivo", "Fiesta", "Trabajo" });
            cboOcasion.SelectedIndex = 0;
            cboTemporada.Items.AddRange(new object[] { "Verano", "Invierno", "Otoño", "Primavera", "Todo el año" });
            cboTemporada.SelectedIndex = 0;
            CargarOutfits();
        }

        private void CargarOutfits()
        {
            dgvOutfits.DataSource = null;
            dgvOutfits.DataSource = _bllOutfits.GetAll();
            txtResumen.Text = string.Empty;
        }

        private void dgvOutfits_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvOutfits.CurrentRow == null) return;
            var outfit = (Outfit)dgvOutfits.CurrentRow.DataBoundItem;
            _outfitEditando = outfit;
            txtNombre.Text = outfit.Nombre;
            txtDescripcion.Text = outfit.Descripcion;
            cboOcasion.Text = outfit.Ocasion;
            cboTemporada.Text = outfit.Temporada;
            // Mostrar resumen con el DECORATOR
            txtResumen.Text = _bllOutfits.ObtenerResumenOutfit(outfit);
        }

        // Carga el outfit seleccionado para edicion explicitamente
        private void btnEditar_Click(object sender, EventArgs e)
        {
            if (_outfitEditando == null)
            {
                MessageBox.Show("Seleccione un outfit de la lista para editar.",
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
            Outfit outfit = _outfitEditando ?? new Outfit();
            outfit.Nombre = txtNombre.Text.Trim();
            outfit.Descripcion = txtDescripcion.Text.Trim();
            outfit.Ocasion = cboOcasion.Text;
            outfit.Temporada = cboTemporada.Text;
            outfit.Estado = true;
            _bllOutfits.Save(outfit);
            CargarOutfits();
            LimpiarCampos();
            MessageBox.Show("Outfit guardado correctamente.", "Exito",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (_outfitEditando == null)
            {
                MessageBox.Show("Seleccione un outfit para eliminar.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (MessageBox.Show("Eliminar '" + _outfitEditando.Nombre + "'?", "Confirmar",
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _bllOutfits.Delete(_outfitEditando);
                CargarOutfits();
                LimpiarCampos();
            }
        }

        private void btnNuevo_Click(object sender, EventArgs e) { LimpiarCampos(); }

        private void LimpiarCampos()
        {
            _outfitEditando = null;
            dgvOutfits.ClearSelection();
            txtNombre.Text = string.Empty;
            txtDescripcion.Text = string.Empty;
            txtResumen.Text = string.Empty;
            if (cboOcasion.Items.Count > 0) cboOcasion.SelectedIndex = 0;
            if (cboTemporada.Items.Count > 0) cboTemporada.SelectedIndex = 0;
            txtNombre.Focus();
        }
    }
}
