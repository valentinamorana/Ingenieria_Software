using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace GUI
{
    /// <summary>
    /// Capa de Presentación — Módulo de Auditoría (Bitácora).
    ///
    /// Presenta dos pestañas:
    ///   Tab 1 — Sistema    : eventos de seguridad (login, logout, resets, intentos fallidos)
    ///   Tab 2 — Negocio    : eventos de negocio (ventas, despachos, stock, clientes)
    ///
    /// Filtro de fecha unificado: sólo "Últimos N días" (0 = sin filtro de fecha).
    /// Criticidad: "Todas" + valores reales 1-6, sin "None (0)".
    /// Exportación PDF: vía PrintPreviewDialog (imprimir → "Microsoft Print to PDF").
    ///
    /// Accesible para Administrador (mnuAuditoria) y Supervisor (mnuAuditoria).
    /// </summary>
    /// <summary>
    /// Hereda de <see cref="FormBase"/>:
    ///   - MostrarError() → heredado. Como este formulario no tiene lblMensaje,
    ///     MensajeLabel retorna null y FormBase usa MessageBox automáticamente.
    /// </summary>
    public partial class Bitacora : FormBase
    {
        // La GUI accede a Servicios directamente para consultas de bitácora.
        // BLL decide CUÁNDO registrar; Servicios sabe CÓMO persistir Y CÓMO consultar.
        private readonly Servicios.Bitacora        srvSistema = new Servicios.Bitacora();
        private readonly Servicios.BitacoraNegocio srvNegocio = new Servicios.BitacoraNegocio();

        // ── Estado para impresión paginada ────────────────────────────────────
        private DataTable   _tablaImpresion;
        private string      _tituloImpresion;
        private int         _paginaActual;
        private int         _filaImpresion;
        private Font        _fuenteHeader;
        private Font        _fuenteCelda;
        private Font        _fuenteTitulo;

        public Bitacora()
        {
            InitializeComponent();
        }

        private void Bitacora_Load(object sender, EventArgs e)
        {
            // Supervisor solo accede a la bitácora de negocio (seguimiento de ventas/prendas).
            // Se elimina la tab de Sistema antes de mostrar el formulario.
            var usuario = new BLL.Usuario().ObtenerUsuarioActivo();
            string perfil = usuario?.Perfil ?? "";

            if (perfil.Equals("Supervisor", StringComparison.OrdinalIgnoreCase))
                tabControl.TabPages.Remove(tabPageSistema);
            else
                CargarSistema();

            CargarNegocio();
        }

        private void BtnUltimosDias_Click(object sender, EventArgs e)
        {
            int dias = (int)nudDias.Value;
            DataTable dt = dias == 0
                ? srvSistema.ObtenerTodos()
                : srvSistema.ObtenerUltimosNDias(dias);
            MostrarEnGrilla(dgvSistema, lblResultadosSistema, dt,
                dias > 0 ? $"últimos {dias} días" : "todos los registros");
        }

        private void BtnLimpiar_Click(object sender, EventArgs e)
        {
            txtUsuario.Text             = "0";
            txtActividad.Clear();
            cmbCriticidad.SelectedIndex = 0;
            nudDias.Value               = 7;
            CargarSistema();
        }

        private void BtnExportSistema_Click(object sender, EventArgs e)
        {
            ExportarPdf(dgvSistema, "Bitácora del Sistema — WardrobeFlow");
        }

        private void DgvSistema_DataBindingComplete(object sender,
            System.Windows.Forms.DataGridViewBindingCompleteEventArgs e)
        {
            ColorearPorCriticidad(dgvSistema);
        }

        private void BtnNegUltimosDias_Click(object sender, EventArgs e)
        {
            int dias = (int)nudNegDias.Value;
            DateTime? desde = dias > 0 ? DateTime.Now.AddDays(-dias) : (DateTime?)null;
            var dt = srvNegocio.BuscarPorFiltros(desde, null, null, null, null);
            MostrarEnGrilla(dgvNegocio, lblResultadosNegocio, dt,
                dias > 0 ? $"últimos {dias} días" : "todos los registros");
        }

        private void BtnNegLimpiar_Click(object sender, EventArgs e)
        {
            cmbTipoEvento.SelectedIndex = 0;
            txtNegPedido.Text           = "0";
            txtNegCliente.Text          = "0";
            nudNegDias.Value            = 7;
            CargarNegocio();
        }

        private void BtnExportNegocio_Click(object sender, EventArgs e)
        {
            ExportarPdf(dgvNegocio, "Bitácora de Negocio — WardrobeFlow");
        }

        // ── Carga ─────────────────────────────────────────────────────────────

        private void CargarSistema()
        {
            try
            {
                var dt = srvSistema.ObtenerTodos();
                MostrarEnGrilla(dgvSistema, lblResultadosSistema, dt);
            }
            catch (Exception ex) { MostrarError(ex.Message); }
        }

        private void CargarNegocio()
        {
            try
            {
                var dt = srvNegocio.ObtenerTodos();
                MostrarEnGrilla(dgvNegocio, lblResultadosNegocio, dt);
            }
            catch (Exception ex) { MostrarError(ex.Message); }
        }

        // ── Eventos ───────────────────────────────────────────────────────────

        private void BtnBuscarSistema_Click(object sender, EventArgs e)
        {
            try
            {
                int dias        = (int)nudDias.Value;
                DateTime? desde = dias > 0 ? DateTime.Now.AddDays(-dias) : (DateTime?)null;
                int uid         = int.TryParse(txtUsuario.Text, out int u) ? u : 0;
                string activ    = txtActividad.Text.Trim();

                int[] criticidadMap = { -1, 1, 2, 3, 4, 5, 6 };
                int criticidad = criticidadMap[cmbCriticidad.SelectedIndex];

                var dt = srvSistema.BuscarPorFiltros(desde, null, uid, activ, criticidad);
                MostrarEnGrilla(dgvSistema, lblResultadosSistema, dt);
            }
            catch (Exception ex) { MostrarError(ex.Message); }
        }

        private void BtnBuscarNegocio_Click(object sender, EventArgs e)
        {
            try
            {
                int dias        = (int)nudNegDias.Value;
                DateTime? desde = dias > 0 ? DateTime.Now.AddDays(-dias) : (DateTime?)null;
                string tipo     = cmbTipoEvento.SelectedIndex == 0
                                    ? null
                                    : cmbTipoEvento.SelectedItem.ToString();
                int? idPedido   = int.TryParse(txtNegPedido.Text,  out int p) && p > 0 ? (int?)p : null;
                int? idCliente  = int.TryParse(txtNegCliente.Text, out int c) && c > 0 ? (int?)c : null;

                var dt = srvNegocio.BuscarPorFiltros(desde, null, tipo, idCliente, idPedido);
                MostrarEnGrilla(dgvNegocio, lblResultadosNegocio, dt);
            }
            catch (Exception ex) { MostrarError(ex.Message); }
        }

        // ══════════════════════════════════════════════════════════════════════
        // EXPORTAR PDF — PrintDocument + PrintPreviewDialog
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Abre una vista previa de impresión del contenido actual de la grilla.
        /// Desde ahí el usuario puede imprimir directamente o seleccionar
        /// "Microsoft Print to PDF" para guardar como archivo PDF.
        /// </summary>
        private void ExportarPdf(DataGridView dgv, string titulo)
        {
            if (dgv.Rows.Count == 0)
            {
                MessageBox.Show("No hay datos para exportar.", "Exportar PDF",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Capturar datos en tabla independiente para la impresión
            _tablaImpresion  = (DataTable)dgv.DataSource;
            _tituloImpresion = titulo;
            _paginaActual    = 1;
            _filaImpresion   = 0;

            _fuenteTitulo = new Font("Segoe UI", 13, FontStyle.Bold);
            _fuenteHeader = new Font("Segoe UI", 8,  FontStyle.Bold);
            _fuenteCelda  = new Font("Segoe UI", 7.5f);

            var doc = new PrintDocument();
            doc.DefaultPageSettings.Landscape = true;
            doc.DefaultPageSettings.Margins   = new Margins(40, 40, 40, 40);
            doc.PrintPage += ImprimirPagina;

            using (var preview = new PrintPreviewDialog())
            {
                preview.Document = doc;
                preview.Width    = 1050;
                preview.Height   = 780;
                preview.Text     = $"Vista Previa — {titulo}";
                preview.ShowDialog(this);
            }
        }

        /// <summary>
        /// Renderiza una página del documento de impresión.
        /// Dibuja el título, encabezados de columna y filas de datos.
        /// Continúa en páginas adicionales si los datos no caben en una sola.
        /// </summary>
        private void ImprimirPagina(object sender, PrintPageEventArgs e)
        {
            Graphics  g      = e.Graphics;
            Rectangle margen = e.MarginBounds;

            float y          = margen.Top;
            float xIzq       = margen.Left;
            float anchoTotal = margen.Width;

            // ── Título (solo en la primera página) ───────────────────────────
            if (_paginaActual == 1)
            {
                g.DrawString(_tituloImpresion, _fuenteTitulo, Brushes.DarkSlateBlue,
                    xIzq, y);
                y += _fuenteTitulo.GetHeight(g) + 4;

                g.DrawString(
                    $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}   |   " +
                    $"{_tablaImpresion.Rows.Count} registro(s)",
                    _fuenteCelda, Brushes.Gray, xIzq, y);
                y += _fuenteCelda.GetHeight(g) + 8;

                // Línea separadora bajo el título
                g.DrawLine(Pens.DarkSlateBlue, xIzq, y, xIzq + anchoTotal, y);
                y += 4;
            }

            // ── Calcular anchos de columna ────────────────────────────────────
            int nCols     = _tablaImpresion.Columns.Count;
            float colAncho = anchoTotal / nCols;

            // ── Encabezados ───────────────────────────────────────────────────
            float alturaHeader = _fuenteHeader.GetHeight(g) + 6;

            using (var brushHeader = new SolidBrush(Color.FromArgb(60, 60, 120)))
            using (var brushHeaderBg = new SolidBrush(Color.FromArgb(220, 220, 240)))
            {
                g.FillRectangle(brushHeaderBg,
                    xIzq, y, anchoTotal, alturaHeader);

                for (int col = 0; col < nCols; col++)
                {
                    string nombre = _tablaImpresion.Columns[col].ColumnName;
                    var rect = new RectangleF(
                        xIzq + col * colAncho + 2, y + 2,
                        colAncho - 4, alturaHeader - 4);
                    g.DrawString(nombre, _fuenteHeader, brushHeader, rect,
                        new StringFormat { Trimming = StringTrimming.EllipsisCharacter });
                }
            }
            y += alturaHeader + 2;

            // ── Filas de datos ────────────────────────────────────────────────
            float alturaCelda = _fuenteCelda.GetHeight(g) + 5;
            bool  alternar    = false;

            while (_filaImpresion < _tablaImpresion.Rows.Count)
            {
                // Verificar si cabe otra fila en la página
                if (y + alturaCelda > margen.Bottom - 20) break;

                DataRow fila = _tablaImpresion.Rows[_filaImpresion];

                // Fondo alternado
                if (alternar)
                    g.FillRectangle(new SolidBrush(Color.FromArgb(248, 248, 255)),
                        xIzq, y, anchoTotal, alturaCelda);

                for (int col = 0; col < nCols; col++)
                {
                    string valor = fila[col]?.ToString() ?? "";
                    var rect = new RectangleF(
                        xIzq + col * colAncho + 2, y + 1,
                        colAncho - 4, alturaCelda - 2);
                    g.DrawString(valor, _fuenteCelda, Brushes.Black, rect,
                        new StringFormat { Trimming = StringTrimming.EllipsisCharacter });
                }

                // Línea de separación fina
                g.DrawLine(Pens.LightGray,
                    xIzq, y + alturaCelda, xIzq + anchoTotal, y + alturaCelda);

                y        += alturaCelda;
                alternar  = !alternar;
                _filaImpresion++;
            }

            // ── Pie de página ─────────────────────────────────────────────────
            g.DrawLine(Pens.DarkSlateBlue,
                xIzq, margen.Bottom - 14, xIzq + anchoTotal, margen.Bottom - 14);
            g.DrawString(
                $"WardrobeFlow — Página {_paginaActual}",
                _fuenteCelda, Brushes.Gray,
                xIzq, margen.Bottom - 12);

            // ¿Hay más filas? Entonces hay más páginas
            e.HasMorePages = _filaImpresion < _tablaImpresion.Rows.Count;
            if (e.HasMorePages) _paginaActual++;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void MostrarEnGrilla(DataGridView dgv, Label lbl, DataTable datos, string contexto = null)
        {
            dgv.DataSource = datos;

            string linea1 = $"  {datos.Rows.Count} registro(s)";
            if (!string.IsNullOrEmpty(contexto)) linea1 += $"  —  {contexto}";

            if (dgv == dgvSistema && datos.Columns.Contains("criticidad"))
            {
                lbl.Height = 44;
                lbl.Text   = linea1 + "\r\n  " + ComputarEstadisticasCriticidad(datos);
            }
            else
            {
                lbl.Height = 24;
                lbl.Text   = linea1;
            }
        }

        private string ComputarEstadisticasCriticidad(DataTable datos)
        {
            var conteos = new int[7];
            foreach (DataRow row in datos.Rows)
            {
                if (int.TryParse(row["criticidad"]?.ToString(), out int c) && c >= 0 && c < 7)
                    conteos[c]++;
            }

            string[] etiquetas = { "None", "Baja", "Media", "Alta", "Int.Login", "Recup.Clave", "Bloqueos" };
            var partes = new List<string>();
            for (int i = 0; i < 7; i++)
                if (conteos[i] > 0)
                    partes.Add($"{etiquetas[i]}: {conteos[i]}");

            return partes.Count > 0
                ? string.Join("   |   ", partes)
                : "Sin datos de criticidad";
        }

        private void ColorearPorCriticidad(DataGridView dgv)
        {
            if (!dgv.Columns.Contains("criticidad")) return;

            foreach (DataGridViewRow fila in dgv.Rows)
            {
                if (fila.IsNewRow) continue;
                if (!int.TryParse(fila.Cells["criticidad"].Value?.ToString(), out int crit)) continue;

                Color back, fore;
                switch (crit)
                {
                    case 0:  back = Color.FromArgb(245, 245, 245); fore = Color.Gray;           break;
                    case 1:  back = Color.FromArgb(220, 255, 220); fore = Color.DarkGreen;      break;
                    case 2:  back = Color.FromArgb(255, 255, 200); fore = Color.DarkGoldenrod;  break;
                    case 3:  back = Color.FromArgb(255, 220, 170); fore = Color.DarkOrange;     break;
                    case 4:  back = Color.FromArgb(255, 205, 205); fore = Color.DarkRed;        break;
                    case 5:  back = Color.FromArgb(210, 225, 255); fore = Color.DarkBlue;       break;
                    case 6:  back = Color.FromArgb(200, 0,   20);  fore = Color.White;          break;
                    default: continue;
                }
                fila.DefaultCellStyle.BackColor = back;
                fila.DefaultCellStyle.ForeColor = fore;
            }
        }

    }
}
