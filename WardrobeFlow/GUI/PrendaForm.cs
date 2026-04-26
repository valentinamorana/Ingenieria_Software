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

        private readonly bool      _esEdicion;
        private readonly BE.Prenda _original;

        private TextBox  txtNombre;
        private TextBox  txtDescripcion;
        private ComboBox cmbTalle;
        private TextBox  txtColor;
        private ComboBox cmbCategoria;
        private Button   btnGuardar;
        private Button   btnCancelar;
        private Label    lblMensaje;

        public PrendaForm() : this(null) { }

        public PrendaForm(BE.Prenda prenda)
        {
            InitializeComponent();
            _esEdicion = prenda != null;
            _original  = prenda;

            this.Text             = _esEdicion ? "Editar Prenda" : "Nueva Prenda";
            this.FormBorderStyle  = FormBorderStyle.FixedDialog;
            this.MaximizeBox      = false;
            this.MinimizeBox      = false;
            this.StartPosition    = FormStartPosition.CenterParent;

            ConstruirInterfaz();
            if (_esEdicion) CargarDatos();
        }

        private void ConstruirInterfaz()
        {
            const int lx = 16, cx = 16, cw = 360, gap = 54;
            int top = 16;

            // Nombre
            this.Controls.Add(new Label { Text = "Nombre *", Left = lx, Top = top, Width = cw });
            txtNombre = new TextBox { Left = cx, Top = top + 20, Width = cw };
            this.Controls.Add(txtNombre);

            // Descripción
            top += gap;
            this.Controls.Add(new Label { Text = "Descripción", Left = lx, Top = top, Width = cw });
            txtDescripcion = new TextBox
            {
                Left = cx, Top = top + 20, Width = cw, Height = 56,
                Multiline = true, ScrollBars = ScrollBars.Vertical
            };
            this.Controls.Add(txtDescripcion);

            // Talle + Color (en la misma fila)
            top += 80;
            this.Controls.Add(new Label { Text = "Talle *", Left = lx, Top = top, Width = 160 });
            cmbTalle = new ComboBox
            {
                Left = cx, Top = top + 20, Width = 160,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbTalle.Items.AddRange(new object[]
                { "XS", "S", "M", "L", "XL", "XXL", "34", "36", "38", "40", "42", "44", "46", "Único" });
            cmbTalle.SelectedIndex = 2;
            this.Controls.Add(cmbTalle);

            this.Controls.Add(new Label { Text = "Color", Left = cx + 180, Top = top, Width = 180 });
            txtColor = new TextBox { Left = cx + 180, Top = top + 20, Width = 180 };
            this.Controls.Add(txtColor);

            // Categoría — alineadas con las del sistema
            top += gap;
            this.Controls.Add(new Label { Text = "Categoría *", Left = lx, Top = top, Width = cw });
            cmbCategoria = new ComboBox
            {
                Left = cx, Top = top + 20, Width = cw,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbCategoria.Items.AddRange(new object[]
            {
                "Vestidos", "Faldas", "Pantalones", "Tops",
                "Blazers", "Abrigos", "Conjuntos",
                "Ropa Deportiva", "Accesorios", "Otro"
            });
            cmbCategoria.SelectedIndex = 0;
            this.Controls.Add(cmbCategoria);

            // Mensaje de error
            top += gap;
            lblMensaje = new Label
            {
                Left = lx, Top = top, Width = cw, Height = 34,
                ForeColor = Color.DarkRed, Font = new Font("Segoe UI", 8.5f)
            };
            this.Controls.Add(lblMensaje);

            // Botones
            top += 40;
            btnGuardar = new Button
            {
                Text = _esEdicion ? "Guardar Cambios" : "Agregar Prenda",
                Left = cx, Top = top, Width = 184, Height = 36,
                BackColor = Color.SteelBlue, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Click += BtnGuardar_Click;

            btnCancelar = new Button
            {
                Text = "Cancelar",
                Left = cx + 200, Top = top, Width = 120, Height = 36,
                FlatStyle = FlatStyle.Flat
            };
            btnCancelar.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.Add(btnGuardar);
            this.Controls.Add(btnCancelar);

            // Tamaño fijo con margen generoso para que nada quede cortado
            this.ClientSize = new Size(396, top + 58);
        }

        private void CargarDatos()
        {
            txtNombre.Text      = _original.Nombre;
            txtDescripcion.Text = _original.Descripcion ?? "";
            txtColor.Text       = _original.Color ?? "";

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
                    IdPrenda    = _esEdicion ? _original.IdPrenda : 0,
                    Nombre      = txtNombre.Text.Trim(),
                    Descripcion = string.IsNullOrWhiteSpace(txtDescripcion.Text) ? null : txtDescripcion.Text.Trim(),
                    Talle       = cmbTalle.SelectedItem?.ToString(),
                    Color       = string.IsNullOrWhiteSpace(txtColor.Text) ? null : txtColor.Text.Trim(),
                    Categoria   = cmbCategoria.SelectedItem?.ToString(),
                    Estado      = _esEdicion ? _original.Estado : BE.EstadoPrenda.Disponible,
                    FechaAlta   = _esEdicion ? _original.FechaAlta : DateTime.Now
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
