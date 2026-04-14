using System;
using System.Windows.Forms;
using BE;
using BLL;

namespace GUI
{
    // Gestor de outfits. Muestra los outfits existentes y sus prendas.
    // Usa el patron DECORATOR a traves de OutfitBLL.ObtenerResumenOutfit().
    public partial class frmGestorOutfits : Form
    {
        // BLL de outfits (usa el Decorator internamente)
        private readonly OutfitBLL    _bllOutfits;
        // BLL de prendas (necesaria para inicializar OutfitBLL)
        private readonly PrendaBLL    _bllPrendas;
        // BLL de categorias (necesaria para inicializar PrendaBLL)
        private readonly CategoriaBLL _bllCategorias;

        // Constructor: inicializa BLLs en cadena y carga datos
        public frmGestorOutfits()
        {
            _bllCategorias = new CategoriaBLL();
            _bllPrendas    = new PrendaBLL(_bllCategorias);
            _bllOutfits    = new OutfitBLL(_bllPrendas);
            InitializeComponent();

            // Cargar ocasiones disponibles
            cboOcasion.Items.AddRange(new object[] { "Casual", "Formal", "Deportivo", "Fiesta", "Trabajo" });
            cboOcasion.SelectedIndex = 0;

            // Cargar temporadas disponibles
            cboTemporada.Items.AddRange(new object[] { "Verano", "Invierno", "Otono", "Primavera", "Todo el ano" });
            cboTemporada.SelectedIndex = 0;

            CargarOutfits();
        }

        // Recarga el grid con todos los outfits
        private void CargarOutfits()
        {
            dgvOutfits.DataSource   = null;
            dgvOutfits.DataSource   = _bllOutfits.GetAll();
            txtResumen.Text         = string.Empty;
        }

        // Al seleccionar un outfit, muestra el resumen con el DECORATOR
        private void dgvOutfits_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvOutfits.CurrentRow == null) return;
            var outfit = (Outfit)dgvOutfits.CurrentRow.DataBoundItem;

            txtNombre.Text    = outfit.Nombre;
            txtDescripcion.Text = outfit.Descripcion;
            cboOcasion.Text   = outfit.Ocasion;
            cboTemporada.Text = outfit.Temporada;

            // Mostrar el resumen enriquecido usando el patron DECORATOR
            txtResumen.Text = _bllOutfits.ObtenerResumenOutfit(outfit);
        }

        // Guarda un outfit nuevo o actualiza el existente
        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre es obligatorio.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Outfit outfit;
            if (dgvOutfits.CurrentRow != null &&
                dgvOutfits.CurrentRow.DataBoundItem is Outfit oSel)
            {
                outfit = oSel;
            }
            else
            {
                outfit = new Outfit();
            }

            outfit.Nombre      = txtNombre.Text.Trim();
            outfit.Descripcion = txtDescripcion.Text.Trim();
            outfit.Ocasion     = cboOcasion.Text;
            outfit.Temporada   = cboTemporada.Text;
            outfit.Estado      = true;

            _bllOutfits.Save(outfit);
            CargarOutfits();
            LimpiarCampos();
            MessageBox.Show("Outfit guardado.", "Exito",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Elimina el outfit seleccionado
        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (dgvOutfits.CurrentRow == null) return;
            var outfit = (Outfit)dgvOutfits.CurrentRow.DataBoundItem;

            if (MessageBox.Show("Eliminar '" + outfit.Nombre + "'?", "Confirmar",
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _bllOutfits.Delete(outfit);
                CargarOutfits();
                LimpiarCampos();
            }
        }

        private void btnNuevo_Click(object sender, EventArgs e) { LimpiarCampos(); }

        private void LimpiarCampos()
        {
            txtNombre.Text      = string.Empty;
            txtDescripcion.Text = string.Empty;
            txtResumen.Text     = string.Empty;
        }
    }
}
