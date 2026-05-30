using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Servicios.Multiidioma;

namespace GUI
{
    /// <summary>
    /// Muestra el historial de registros de mantenimiento/limpieza de una prenda.
    /// Solo lectura. Accesible desde el módulo Prendas (btnMantenimiento).
    /// </summary>
    public partial class MantenimientoHistorialForm : Form
    {
        private readonly BE.Prenda              _prenda;
        private readonly BLL.Interfaces.IPrendaService  _prendaBLL;
        private Idioma                          _idioma = GestorIdioma.IdiomaActual;

        public MantenimientoHistorialForm(BE.Prenda prenda, BLL.Interfaces.IPrendaService prendaBLL)
        {
            InitializeComponent();
            _prenda    = prenda;
            _prendaBLL = prendaBLL;

            try
            {
                string ico = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico");
                if (System.IO.File.Exists(ico)) this.Icon = new Icon(ico);
            }
            catch { }

            AplicarIdioma(_idioma);
            CargarHistorial();
        }

        private void AplicarIdioma(Idioma idioma)
        {
            _idioma = idioma;
            var t = Traductor.ObtenerTraducciones(idioma);
            string T(string k, string fb) => t.ContainsKey(k) ? t[k].Texto : fb;

            this.Text      = T("frm.mantenimiento", "Historial de Mantenimiento");
            lblTitulo.Text = $"{T("frm.mantenimiento", "Historial de Mantenimiento")} — {_prenda.Nombre}";
            btnCerrar.Text = T("btn.mant.cerrar", "Cerrar");

            TraducirHeadersGrilla();
        }

        private void TraducirHeadersGrilla()
        {
            var t = Traductor.ObtenerTraducciones(_idioma);
            void RH(string col, string key, string fb)
            {
                if (dgvHistorial.Columns.Contains(col))
                    dgvHistorial.Columns[col].HeaderText = t.ContainsKey(key) ? t[key].Texto : fb;
            }
            RH("Entrada",  "col.mant.entrada",  "Entrada");
            RH("Salida",   "col.mant.salida",   "Salida");
            RH("Duracion", "col.mant.duracion", "Duración (días)");
            RH("Actor",    "col.mant.actor",    "Responsable");
            RH("Estado",   "col.mant.estado",   "Estado");
        }

        private void CargarHistorial()
        {
            try
            {
                var t = Traductor.ObtenerTraducciones(_idioma);
                string T(string k, string fb) => t.ContainsKey(k) ? t[k].Texto : fb;

                var registros = _prendaBLL.ObtenerHistorialMantenimiento(_prenda.IdPrenda);

                if (registros == null || registros.Count == 0)
                {
                    lblSinRegistros.Text    = T("msg.mant.sinregistros", "Esta prenda no tiene historial de mantenimiento.");
                    lblSinRegistros.Visible = true;
                    dgvHistorial.Visible    = false;
                    return;
                }

                lblSinRegistros.Visible = false;
                dgvHistorial.Visible    = true;

                string abierto  = T("mant.abierto",  "En limpieza");
                string cerrado  = T("mant.cerrado",  "Finalizado");
                string sinFecha = "—";

                var tabla = new DataTable();
                tabla.Columns.Add("Entrada",  typeof(string));
                tabla.Columns.Add("Salida",   typeof(string));
                tabla.Columns.Add("Duracion", typeof(string));
                tabla.Columns.Add("Actor",    typeof(string));
                tabla.Columns.Add("Estado",   typeof(string));

                foreach (var r in registros)
                {
                    tabla.Rows.Add(
                        r.FechaEntrada.ToString("dd/MM/yyyy HH:mm"),
                        r.FechaSalida.HasValue ? r.FechaSalida.Value.ToString("dd/MM/yyyy HH:mm") : sinFecha,
                        r.DuracionDias.HasValue ? r.DuracionDias.Value.ToString() : "—",
                        r.Actor ?? "—",
                        r.EstaAbierto ? abierto : cerrado);
                }

                dgvHistorial.DataSource = tabla;
                TraducirHeadersGrilla();

                // Colorear filas abiertas
                foreach (DataGridViewRow row in dgvHistorial.Rows)
                {
                    if (row.Cells["Estado"].Value?.ToString() == abierto)
                        row.DefaultCellStyle.ForeColor = Color.FromArgb(160, 100, 0);
                }
            }
            catch (Exception ex)
            {
                var tErr = Traductor.ObtenerTraducciones(_idioma);
                string fmtErr = tErr.ContainsKey("err.mant.cargar")
                    ? tErr["err.mant.cargar"].Texto
                    : "Error al cargar historial: {0}";
                lblSinRegistros.Text    = string.Format(fmtErr, ex.Message);
                lblSinRegistros.Visible = true;
                dgvHistorial.Visible    = false;
            }
        }

        private void BtnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
