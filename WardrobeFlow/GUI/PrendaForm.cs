using System;
using System.Drawing;
using System.Windows.Forms;

namespace GUI
{
    /// <summary>
    /// Diálogo modal para dar de alta o editar una prenda del catálogo.
    /// Devuelve DialogResult.OK con PrendaEditada cargada si el usuario confirma.
    /// </summary>
    public partial class PrendaForm : Form
    {
        public BE.Prenda PrendaEditada { get; private set; }

        private readonly bool _esEdicion;
        private readonly BE.Prenda _original;

        public PrendaForm() : this(null) { }

        public PrendaForm(BE.Prenda prenda)
        {
            InitializeComponent();
            _esEdicion = prenda != null;
            _original  = prenda;

            this.Text         = _esEdicion ? "Editar Prenda" : "Nueva Prenda";
            btnGuardar.Text   = _esEdicion ? "Guardar Cambios" : "Agregar Prenda";

            if (_esEdicion) CargarDatos();
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void CargarDatos()
        {
            txtNombre.Text = _original.Nombre;
            txtDescripcion.Text = _original.Descripcion ?? "";
            txtColor.Text = _original.Color ?? "";

            int idxTalle = cmbTalle.Items.IndexOf(_original.Talle ?? "");
            cmbTalle.SelectedIndex = idxTalle >= 0 ? idxTalle : 2;

            int idxCat = cmbCategoria.Items.IndexOf(_original.Categoria ?? "");
            cmbCategoria.SelectedIndex = idxCat >= 0 ? idxCat : 0;
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            lblMensaje.Text = string.Empty;
            try
            {
                PrendaEditada = new BE.Prenda
                {
                    IdPrenda = _esEdicion ? _original.IdPrenda : 0,
                    Nombre = txtNombre.Text.Trim(),
                    Descripcion = string.IsNullOrWhiteSpace(txtDescripcion.Text) ? null : txtDescripcion.Text.Trim(),
                    Talle = cmbTalle.SelectedItem?.ToString(),
                    Color = string.IsNullOrWhiteSpace(txtColor.Text) ? null : txtColor.Text.Trim(),
                    Categoria = cmbCategoria.SelectedItem?.ToString(),
                    Estado = _esEdicion ? _original.Estado : BE.EstadoPrenda.Disponible,
                    FechaAlta = _esEdicion ? _original.FechaAlta : DateTime.Now
                };

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"✗ {ex.Message}";
            }
        }
    }
}
