using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using BE;
using BLL;

namespace GUI.Modales
{
    // Formulario de gestion de Outfits.
    // Permite crear, editar y eliminar outfits, asignando prendas
    // desde el inventario. Usa BLL_Outfit.ObtenerResumenOutfit()
    // que aplica el PATRON DECORATOR sobre las prendas del outfit.
    public partial class frmOutfit : Form
    {
        private BLL_Outfit    _bll        = new BLL_Outfit();
        private BLL_Prenda    _bllPrenda  = new BLL_Prenda();
        private List<Outfit>  _lista;
        private List<Prenda>  _todasPrendas;
        private Outfit        _seleccionado;
        private Usuario       _usuarioActual;

        public frmOutfit(Usuario usuario)
        {
            InitializeComponent();
            _usuarioActual = usuario;
        }

        // ── Carga inicial ──────────────────────────────────────────────────
        private void frmOutfit_Load(object sender, EventArgs e)
        {
            // Combos de opciones fijas
            cboOcasion.Items.AddRange(new[] { "Casual", "Formal", "Deportivo", "Fiesta", "Trabajo" });
            cboTemporada.Items.AddRange(new[] { "Verano", "Otoño", "Invierno", "Primavera" });

            // Lista de prendas disponibles
            _todasPrendas = _bllPrenda.ListarPrendas();
            clbPrendas.DataSource    = _todasPrendas;
            clbPrendas.DisplayMember = "Nombre";

            CargarGrilla();
        }

        // ── Grilla de outfits ──────────────────────────────────────────────
        private void CargarGrilla()
        {
            _lista = _bll.ListarOutfits();
            dgvOutfits.DataSource = null;
            dgvOutfits.DataSource = _lista;
        }

        // ── Guardar (Agregar o Editar) ─────────────────────────────────────
        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                Outfit o = ObtenerDesdeFormulario();
                string msg = _seleccionado == null
                    ? _bll.AgregarOutfit(o)
                    : _bll.EditarOutfit(o);
                MessageBox.Show(msg, "Exito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Limpiar();
                CargarGrilla();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Eliminar ───────────────────────────────────────────────────────
        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (_seleccionado == null)
            {
                MessageBox.Show("Seleccione un outfit de la lista.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("¿Eliminar el outfit \"" + _seleccionado.Nombre + "\"?",
                "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    MessageBox.Show(_bll.EliminarOutfit(_seleccionado.IdOutfit));
                    Limpiar();
                    CargarGrilla();
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            }
        }

        // ── Ver resumen con Decorator ──────────────────────────────────────
        private void btnVerResumen_Click(object sender, EventArgs e)
        {
            if (_seleccionado == null)
            {
                MessageBox.Show("Seleccione un outfit de la lista.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // Cargar detalles y aplicar Decorator para generar descripcion enriquecida
            _seleccionado.Detalles = _bll.ListarDetallesPorOutfit(_seleccionado.IdOutfit);
            string resumen = _bll.ObtenerResumenOutfit(_seleccionado);
            MessageBox.Show(resumen, "Resumen del Outfit", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ── Limpiar formulario ─────────────────────────────────────────────
        private void btnLimpiar_Click(object sender, EventArgs e) { Limpiar(); }

        // ── Seleccion en grilla ────────────────────────────────────────────
        private void dgvOutfits_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            _seleccionado       = _lista[e.RowIndex];
            txtNombre.Text      = _seleccionado.Nombre;
            txtDescripcion.Text = _seleccionado.Descripcion;
            cboOcasion.Text     = _seleccionado.Ocasion;
            cboTemporada.Text   = _seleccionado.Temporada;
            chkEstado.Checked   = _seleccionado.Estado;

            // Marcar en el CheckedListBox las prendas que ya pertenecen al outfit
            List<DetalleOutfit> detalles = _bll.ListarDetallesPorOutfit(_seleccionado.IdOutfit);
            _seleccionado.Detalles = detalles;

            for (int i = 0; i < clbPrendas.Items.Count; i++)
            {
                Prenda p = (Prenda)clbPrendas.Items[i];
                clbPrendas.SetItemChecked(i, detalles.Any(d => d.IdPrenda == p.IdPrenda));
            }
        }

        // ── Helpers ────────────────────────────────────────────────────────
        private Outfit ObtenerDesdeFormulario()
        {
            Outfit o = _seleccionado ?? new Outfit();
            o.Nombre       = txtNombre.Text.Trim();
            o.Descripcion  = txtDescripcion.Text.Trim();
            o.Ocasion      = cboOcasion.Text;
            o.Temporada    = cboTemporada.Text;
            o.Estado       = chkEstado.Checked;
            o.IdUsuario    = _usuarioActual.IdUsuario;

            // Construir lista de detalles con las prendas seleccionadas
            o.Detalles = new List<DetalleOutfit>();
            foreach (Prenda p in clbPrendas.CheckedItems)
            {
                DetalleOutfit det = new DetalleOutfit();
                det.IdPrenda = p.IdPrenda;
                det.OPrenda  = p;
                o.Detalles.Add(det);
            }
            return o;
        }

        private void Limpiar()
        {
            _seleccionado       = null;
            txtNombre.Text      = string.Empty;
            txtDescripcion.Text = string.Empty;
            cboOcasion.SelectedIndex  = -1;
            cboTemporada.SelectedIndex = -1;
            chkEstado.Checked   = true;
            for (int i = 0; i < clbPrendas.Items.Count; i++)
                clbPrendas.SetItemChecked(i, false);
        }
    }
}
