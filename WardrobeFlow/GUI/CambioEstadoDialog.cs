using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Servicios.Multiidioma;

namespace GUI
{
    /// <summary>
    /// Diálogo simple para seleccionar el nuevo estado de una prenda.
    /// Recibe las opciones válidas desde Prendas.cs y devuelve el estado elegido.
    /// </summary>
    public partial class CambioEstadoDialog : Form
    {
        public BE.EstadoPrenda EstadoSeleccionado { get; private set; }

        private readonly List<(string texto, BE.EstadoPrenda estado)> _opciones;

        public CambioEstadoDialog(BE.Prenda prenda,
            List<(string texto, BE.EstadoPrenda estado)> opciones,
            string estadoTraducido = null)
        {
            InitializeComponent();
            _opciones = opciones;
            try { string ico = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico"); if (System.IO.File.Exists(ico)) this.Icon = new System.Drawing.Icon(ico); } catch { }

            var t = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
            string T_ce(string k, string fb) => t.ContainsKey(k) ? t[k].Texto : fb;

            // Traducir controles del Designer
            this.Text              = T_ce("frm.cambioestado",     "Cambiar Estado de Prenda");
            lblNuevoEstado.Text    = T_ce("lbl.nuevoestado",      "Nuevo estado:");
            btnConfirmar.Text      = T_ce("btn.confirmar.cambio", "Confirmar Cambio");
            btnCancelar.Text       = T_ce("btn.cancelar",         "Cancelar");

            string fmtInfo = T_ce("lbl.cambioest.info", "Prenda: {0}  —  Estado actual: {1}");
            string estadoLabel = estadoTraducido ?? prenda.Estado.ToString();
            lblPrendaInfo.Text = string.Format(fmtInfo, prenda.Nombre, estadoLabel);

            foreach (var op in _opciones)
                cmbOpciones.Items.Add(op.texto);
            if (cmbOpciones.Items.Count > 0)
                cmbOpciones.SelectedIndex = 0;
        }

        private void BtnConfirmar_Click(object sender, EventArgs e)
        {
            var t = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
            string T_dlg(string k, string fb) => t.ContainsKey(k) ? t[k].Texto : fb;

            int idx = cmbOpciones.SelectedIndex;
            if (idx < 0) { lblMensaje.Text = T_dlg("msg.cambioest.selecciona", "Seleccioná una opción."); return; }

            EstadoSeleccionado = _opciones[idx].estado;

            // Confirmación extra para Baja (irreversible)
            if (EstadoSeleccionado == BE.EstadoPrenda.Baja)
            {
                var conf = MessageBox.Show(
                    T_dlg("msg.cambioest.bajairrev", "La baja es irreversible. ¿Confirmar?"),
                    T_dlg("conf.baja.titulo", "Dar de Baja"),
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if (conf != DialogResult.Yes) return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
