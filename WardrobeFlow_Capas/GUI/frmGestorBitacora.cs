using System;
using System.Windows.Forms;
using BE;
using Seguridad;

namespace GUI
{
    // Formulario que muestra el historial de eventos registrados por BitacoraSL.
    // Permite visualizar los ultimos N eventos con filtrado y recarga dinamica.
    // Patron: consume el Singleton BitacoraSL directamente desde la GUI.
    public partial class frmGestorBitacora : Form
    {
        // Constructor: inicializa controles y carga los datos de la bitacora
        public frmGestorBitacora()
        {
            InitializeComponent();
            CargarBitacora();
        }

        // Carga el historial de BitacoraSL en el DataGridView
        private void CargarBitacora()
        {
            // Obtener todos los eventos registrados desde el Singleton de auditoria
            var historial = BitacoraSL.Instancia.ObtenerHistorial();

            // Limpiar y recargar la grilla
            dgvBitacora.Rows.Clear();
            foreach (var registro in historial)
            {
                dgvBitacora.Rows.Add(
                    registro.FechaHora.ToString("yyyy-MM-dd HH:mm:ss"),
                    registro.NombreUsuario,
                    registro.TipoOperacion.ToString(),
                    registro.Modulo.ToString(),
                    registro.Descripcion,
                    registro.Exitoso ? "Si" : "No"
                );
            }

            // Mostrar total de registros en el label
            lblTotal.Text = "Total de eventos: " + historial.Count;
        }

        // Boton Actualizar: recarga los datos desde la bitacora en memoria
        private void btnActualizar_Click(object sender, EventArgs e)
        {
            CargarBitacora();
        }

        // Boton Cerrar: cierra el formulario
        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // Filtra los registros mostrados segun el texto ingresado en el buscador
        private void txtFiltro_TextChanged(object sender, EventArgs e)
        {
            string filtro = txtFiltro.Text.ToLower();
            foreach (DataGridViewRow row in dgvBitacora.Rows)
            {
                // Mostrar la fila si alguna celda contiene el texto del filtro
                bool visible = false;
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.Value != null && cell.Value.ToString().ToLower().Contains(filtro))
                    {
                        visible = true;
                        break;
                    }
                }
                row.Visible = visible;
            }
        }
    }
}
