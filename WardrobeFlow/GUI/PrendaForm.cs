using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Servicios.Multiidioma;

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
            try { string ico = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico"); if (System.IO.File.Exists(ico)) this.Icon = new System.Drawing.Icon(ico); } catch { }
            _esEdicion = prenda != null;
            _original  = prenda;

            AplicarIdioma(GestorIdioma.IdiomaActual);

            if (_esEdicion) CargarDatos();
        }

        // ── Traducción ────────────────────────────────────────────────────────

        /// <summary>
        /// Aplica el idioma activo al abrir el diálogo.
        /// PrendaForm es modal → lee el idioma actual en construcción, sin Observer completo.
        /// </summary>
        private void AplicarIdioma(Idioma idioma)
        {
            var t = Traductor.ObtenerTraducciones(idioma);
            string T(string key, string fallback) => t.ContainsKey(key) ? t[key].Texto : fallback;

            this.Text = _esEdicion
                ? T("frm.editarprenda", "Editar Prenda")
                : T("frm.nuevaprenda",  "Nueva Prenda");

            // El botón guardar tiene dos estados: alta vs edición
            btnGuardar.Text = _esEdicion
                ? T("btn.guardar.cambios", "Guardar Cambios")
                : T("btn.agregar.prenda",  "Agregar Prenda");

            Aplicar(lblNombre,     t);
            Aplicar(lblDescripcion,t);
            Aplicar(lblTalle,      t);
            Aplicar(lblColor,      t);
            Aplicar(lblCategoria,  t);
            Aplicar(btnCancelar,   t);
        }

        private static void Aplicar(Control c, IDictionary<string, Traduccion> t)
        {
            if (c?.Tag != null && t.ContainsKey(c.Tag.ToString()))
                c.Text = t[c.Tag.ToString()].Texto;
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
