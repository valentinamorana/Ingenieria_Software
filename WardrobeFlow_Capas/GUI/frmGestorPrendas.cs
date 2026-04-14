using System;
using System.Windows.Forms;
using BE;
using BLL;

namespace GUI
{
    // Gestor de prendas del guardarropa.
    // Permite agregar, editar y eliminar prendas.
    // Muestra la descripcion enriquecida usando el patron DECORATOR.
    public partial class frmGestorPrendas : Form
    {
        // BLL de prendas (incluye el Decorator)
        private readonly PrendaBLL    _bllPrendas;
        // BLL de categorias para el ComboBox
        private readonly CategoriaBLL _bllCategorias;

        // Constructor: inicializa BLLs y carga datos
        public frmGestorPrendas()
        {
            _bllCategorias = new CategoriaBLL();
            _bllPrendas    = new PrendaBLL(_bllCategorias);
            InitializeComponent();

            // Cargar categorias en el combo
            cboCategorias.DataSource    = _bllCategorias.GetAll();
            cboCategorias.DisplayMember = "Nombre";

            // Cargar las temporadas disponibles
            cboTemporada.Items.AddRange(new object[] { "Verano", "Invierno", "Otono", "Primavera", "Todo el ano" });
            cboTemporada.SelectedIndex = 0;

            CargarPrendas();
        }

        // Recarga el grid con todas las prendas
        private void CargarPrendas()
        {
            dgvPrendas.DataSource = null;
            dgvPrendas.DataSource = _bllPrendas.GetAll();
            lblDescripcion.Text   = string.Empty;
        }

        // Al seleccionar una prenda en el grid, llena los campos
        // y muestra la descripcion decorada (patron DECORATOR)
        private void dgvPrendas_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvPrendas.CurrentRow == null) return;
            var p = (Prenda)dgvPrendas.CurrentRow.DataBoundItem;

            txtNombre.Text      = p.Nombre;
            txtColor.Text       = p.Color;
            txtTalla.Text       = p.Talla;
            cboTemporada.Text   = p.Temporada;

            // Seleccionar la categoria correspondiente en el combo
            foreach (Categoria cat in cboCategorias.Items)
            {
                if (cat.Id == p.OCategoria?.Id)
                {
                    cboCategorias.SelectedItem = cat;
                    break;
                }
            }

            // Mostrar descripcion con el DECORATOR (base + temporada + ocasion casual)
            string desc = _bllPrendas.ObtenerDescripcionDecorada(p, "Casual");
            lblDescripcion.Text = "Descripcion: " + desc;
        }

        // Guarda una prenda nueva o actualiza la existente
        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre es obligatorio.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Prenda p;
            if (dgvPrendas.CurrentRow != null &&
                dgvPrendas.CurrentRow.DataBoundItem is Prenda pSel)
            {
                p = pSel; // Edicion
            }
            else
            {
                p = new Prenda(); // Nueva prenda
            }

            p.Nombre     = txtNombre.Text.Trim();
            p.Color      = txtColor.Text.Trim();
            p.Talla      = txtTalla.Text.Trim();
            p.Temporada  = cboTemporada.Text;
            p.OCategoria = (Categoria)cboCategorias.SelectedItem;
            p.Estado     = true;

            _bllPrendas.Save(p);
            CargarPrendas();
            LimpiarCampos();
            MessageBox.Show("Prenda guardada.", "Exito",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Elimina la prenda seleccionada
        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (dgvPrendas.CurrentRow == null) return;
            var p = (Prenda)dgvPrendas.CurrentRow.DataBoundItem;

            if (MessageBox.Show("Eliminar '" + p.Nombre + "'?", "Confirmar",
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _bllPrendas.Delete(p);
                CargarPrendas();
                LimpiarCampos();
            }
        }

        // Limpia los campos para ingresar una prenda nueva
        private void btnNuevo_Click(object sender, EventArgs e) { LimpiarCampos(); }

        private void LimpiarCampos()
        {
            txtNombre.Text    = string.Empty;
            txtColor.Text     = string.Empty;
            txtTalla.Text     = string.Empty;
            lblDescripcion.Text = string.Empty;
        }
    }
}
