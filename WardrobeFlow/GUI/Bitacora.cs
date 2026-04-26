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
    public partial class Bitacora : Form
    {
        private readonly BLL.Bitacora        bllSistema = new BLL.Bitacora();
        private readonly BLL.BitacoraNegocio bllNegocio = new BLL.BitacoraNegocio();

        // ── Controles compartidos ─────────────────────────────────────────────
        private TabControl tabControl;

        // ── Tab Sistema ───────────────────────────────────────────────────────
        private DataGridView  dgvSistema;
        private NumericUpDown nudDias;
        private Button        btnUltimosDias;
        private TextBox       txtUsuario, txtActividad;
        private ComboBox      cmbCriticidad;
        private Button        btnBuscar, btnLimpiar;
        private Label         lblResultadosSistema;

        // ── Tab Negocio ───────────────────────────────────────────────────────
        private DataGridView  dgvNegocio;
        private NumericUpDown nudNegDias;
        private Button        btnNegUltimosDias;
        private ComboBox      cmbTipoEvento;
        private TextBox       txtNegPedido, txtNegCliente;
        private Button        btnNegBuscar, btnNegLimpiar;
        private Label         lblResultadosNegocio;

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
            this.Text        = "Auditoría — Bitácoras del Sistema";
            this.ClientSize  = new Size(1020, 640);
            this.MinimumSize = new Size(860, 540);

            ConstruirInterfaz();
            this.Load += (s, e) =>
            {
                CargarSistema();
                CargarNegocio();
            };
        }

        private void ConstruirInterfaz()
        {
            tabControl = new TabControl { Dock = DockStyle.Fill };
            tabControl.TabPages.Add(ConstruirTabSistema());
            tabControl.TabPages.Add(ConstruirTabNegocio());
            this.Controls.Add(tabControl);
        }

        // ══════════════════════════════════════════════════════════════════════
        // TAB SISTEMA
        // ══════════════════════════════════════════════════════════════════════

        private TabPage ConstruirTabSistema()
        {
            var tab = new TabPage("🔐  Bitácora del Sistema");

            // ── Panel de filtros (2 filas) ─────────────────────────────────────
            Panel panelFiltros = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 90,
                BackColor = Color.FromArgb(240, 240, 240),
                Padding   = new Padding(8)
            };

            // ── Fila 1: días + usuario ─────────────────────────────────────────
            panelFiltros.Controls.Add(new Label
                { Text = "Últimos", Left = 10, Top = 14, Width = 55 });

            nudDias = new NumericUpDown
                { Left = 68, Top = 10, Width = 55, Minimum = 0, Maximum = 365, Value = 7 };
            panelFiltros.Controls.Add(nudDias);

            panelFiltros.Controls.Add(new Label
                { Text = "días  (0 = todos)", Left = 128, Top = 14, Width = 110 });

            btnUltimosDias = new Button
            {
                Text      = "Ver",
                Left      = 242, Top = 9, Width = 50, Height = 26,
                BackColor = Color.FromArgb(210, 100, 135), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnUltimosDias.FlatAppearance.BorderSize = 0;
            btnUltimosDias.Click += (s, e) =>
            {
                DataTable dt;
                int dias = (int)nudDias.Value;
                if (dias == 0)
                    dt = bllSistema.ObtenerTodos();
                else
                    dt = bllSistema.ObtenerUltimosNDias(dias);
                MostrarEnGrilla(dgvSistema, lblResultadosSistema, dt,
                    dias > 0 ? $"últimos {dias} días" : "todos los registros");
            };
            panelFiltros.Controls.Add(btnUltimosDias);

            panelFiltros.Controls.Add(new Label
                { Text = "Usuario ID:", Left = 312, Top = 14, Width = 75 });
            txtUsuario = new TextBox { Left = 390, Top = 10, Width = 55, Text = "0" };
            panelFiltros.Controls.Add(txtUsuario);

            // ── Fila 2: actividad + criticidad + botones + exportar ────────────
            panelFiltros.Controls.Add(new Label
                { Text = "Actividad:", Left = 10, Top = 54, Width = 70 });
            txtActividad = new TextBox { Left = 82, Top = 50, Width = 200 };
            panelFiltros.Controls.Add(txtActividad);

            panelFiltros.Controls.Add(new Label
                { Text = "Criticidad:", Left = 296, Top = 54, Width = 70 });
            cmbCriticidad = new ComboBox
            {
                Left = 368, Top = 50, Width = 170, DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbCriticidad.Items.AddRange(new object[]
            {
                "Todas",
                "Baja (1)",
                "Media (2)",
                "Alta (3)",
                "Intentos Login (4)",
                "Recuperacion Clave (5)",
                "Bloqueos Cuenta (6)"
            });
            cmbCriticidad.SelectedIndex = 0;
            panelFiltros.Controls.Add(cmbCriticidad);

            btnBuscar = new Button
            {
                Text      = "Buscar",
                Left      = 548, Top = 48, Width = 80,
                BackColor = Color.FromArgb(210, 100, 135), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnBuscar.FlatAppearance.BorderSize = 0;
            btnBuscar.Click += BtnBuscarSistema_Click;
            panelFiltros.Controls.Add(btnBuscar);

            btnLimpiar = new Button
                { Text = "Limpiar", Left = 636, Top = 48, Width = 80 };
            btnLimpiar.Click += (s, e) =>
            {
                txtUsuario.Text = "0";
                txtActividad.Clear();
                cmbCriticidad.SelectedIndex = 0;
                nudDias.Value = 7;
                CargarSistema();
            };
            panelFiltros.Controls.Add(btnLimpiar);

            // Botón exportar PDF — derecha del panel
            var btnExportSistema = new Button
            {
                Text      = "📄 Exportar PDF",
                Left      = 726, Top = 9, Width = 130, Height = 66,
                BackColor = Color.FromArgb(70, 100, 160), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9f)
            };
            btnExportSistema.FlatAppearance.BorderSize = 0;
            btnExportSistema.Click += (s, e) =>
                ExportarPdf(dgvSistema, "Bitácora del Sistema — WardrobeFlow");
            panelFiltros.Controls.Add(btnExportSistema);

            // ── Grilla ─────────────────────────────────────────────────────────
            dgvSistema = CrearDgv();
            dgvSistema.DataBindingComplete += (s, e) => ColorearPorCriticidad(dgvSistema);

            lblResultadosSistema = new Label
            {
                Dock      = DockStyle.Bottom,
                Height    = 44,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Padding   = new Padding(6, 2, 0, 2),
                BackColor = Color.FromArgb(230, 230, 240),
                Font      = new Font("Segoe UI", 8.5f)
            };

            tab.Controls.Add(dgvSistema);
            tab.Controls.Add(lblResultadosSistema);
            tab.Controls.Add(panelFiltros);
            return tab;
        }

        // ══════════════════════════════════════════════════════════════════════
        // TAB NEGOCIO
        // ══════════════════════════════════════════════════════════════════════

        private TabPage ConstruirTabNegocio()
        {
            var tab = new TabPage("📦  Bitácora de Negocio");

            Panel panelFiltros = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 90,
                BackColor = Color.FromArgb(238, 245, 238),
                Padding   = new Padding(8)
            };

            // ── Fila 1: días + tipo + IDs ──────────────────────────────────────
            panelFiltros.Controls.Add(new Label
                { Text = "Últimos", Left = 8, Top = 14, Width = 55 });

            nudNegDias = new NumericUpDown
                { Left = 66, Top = 10, Width = 55, Minimum = 0, Maximum = 365, Value = 7 };
            panelFiltros.Controls.Add(nudNegDias);

            panelFiltros.Controls.Add(new Label
                { Text = "días  (0 = todos)", Left = 126, Top = 14, Width = 110 });

            btnNegUltimosDias = new Button
            {
                Text      = "Ver",
                Left      = 240, Top = 9, Width = 50, Height = 26,
                BackColor = Color.FromArgb(210, 100, 135), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnNegUltimosDias.FlatAppearance.BorderSize = 0;
            btnNegUltimosDias.Click += (s, e) =>
            {
                int dias = (int)nudNegDias.Value;
                DateTime? desde = dias > 0 ? DateTime.Now.AddDays(-dias) : (DateTime?)null;
                var dt = bllNegocio.BuscarPorFiltros(desde, null, null, null, null);
                MostrarEnGrilla(dgvNegocio, lblResultadosNegocio, dt,
                    dias > 0 ? $"últimos {dias} días" : "todos los registros");
            };
            panelFiltros.Controls.Add(btnNegUltimosDias);

            panelFiltros.Controls.Add(new Label
                { Text = "Tipo:", Left = 304, Top = 14, Width = 40 });
            cmbTipoEvento = new ComboBox
            {
                Left = 346, Top = 9, Width = 160, DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbTipoEvento.Items.AddRange(new object[]
            {
                "Todos",
                "Venta", "Cancelacion", "Despacho", "Entrega",
                "AltaPrenda", "ModificacionPrenda", "CambioEstadoPrenda",
                "AltaCliente", "ModificacionCliente", "BajaCliente"
            });
            cmbTipoEvento.SelectedIndex = 0;
            panelFiltros.Controls.Add(cmbTipoEvento);

            panelFiltros.Controls.Add(new Label
                { Text = "ID Pedido:", Left = 518, Top = 14, Width = 70 });
            txtNegPedido = new TextBox { Left = 590, Top = 10, Width = 60, Text = "0" };
            panelFiltros.Controls.Add(txtNegPedido);

            panelFiltros.Controls.Add(new Label
                { Text = "ID Cliente:", Left = 660, Top = 14, Width = 70 });
            txtNegCliente = new TextBox { Left = 732, Top = 10, Width = 60, Text = "0" };
            panelFiltros.Controls.Add(txtNegCliente);

            // ── Fila 2: botones + exportar ─────────────────────────────────────
            btnNegBuscar = new Button
            {
                Text      = "Buscar",
                Left      = 8, Top = 50, Width = 80, Height = 28,
                BackColor = Color.FromArgb(210, 100, 135), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnNegBuscar.FlatAppearance.BorderSize = 0;
            btnNegBuscar.Click += BtnBuscarNegocio_Click;
            panelFiltros.Controls.Add(btnNegBuscar);

            btnNegLimpiar = new Button
                { Text = "Limpiar", Left = 96, Top = 50, Width = 80, Height = 28 };
            btnNegLimpiar.Click += (s, e) =>
            {
                cmbTipoEvento.SelectedIndex = 0;
                txtNegPedido.Text  = "0";
                txtNegCliente.Text = "0";
                nudNegDias.Value   = 7;
                CargarNegocio();
            };
            panelFiltros.Controls.Add(btnNegLimpiar);

            // Botón exportar PDF — derecha del panel
            var btnExportNegocio = new Button
            {
                Text      = "📄 Exportar PDF",
                Left      = 726, Top = 9, Width = 130, Height = 66,
                BackColor = Color.FromArgb(70, 100, 160), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9f)
            };
            btnExportNegocio.FlatAppearance.BorderSize = 0;
            btnExportNegocio.Click += (s, e) =>
                ExportarPdf(dgvNegocio, "Bitácora de Negocio — WardrobeFlow");
            panelFiltros.Controls.Add(btnExportNegocio);

            dgvNegocio = CrearDgv();

            lblResultadosNegocio = new Label
            {
                Dock      = DockStyle.Bottom,
                Height    = 24,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Padding   = new Padding(4, 0, 0, 0),
                BackColor = Color.FromArgb(220, 235, 220)
            };

            tab.Controls.Add(dgvNegocio);
            tab.Controls.Add(lblResultadosNegocio);
            tab.Controls.Add(panelFiltros);
            return tab;
        }

        // ── Carga ─────────────────────────────────────────────────────────────

        private void CargarSistema()
        {
            try
            {
                var dt = bllSistema.ObtenerTodos();
                MostrarEnGrilla(dgvSistema, lblResultadosSistema, dt);
            }
            catch (Exception ex) { MostrarError(ex.Message); }
        }

        private void CargarNegocio()
        {
            try
            {
                var dt = bllNegocio.ObtenerTodos();
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

                var dt = bllSistema.BuscarPorFiltros(desde, null, uid, activ, criticidad);
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

                var dt = bllNegocio.BuscarPorFiltros(desde, null, tipo, idCliente, idPedido);
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

        private DataGridView CrearDgv()
        {
            return new DataGridView
            {
                Dock                  = DockStyle.Fill,
                ReadOnly              = true,
                AllowUserToAddRows    = false,
                AllowUserToDeleteRows = false,
                SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor       = Color.White,
                RowHeadersVisible     = false,
                BorderStyle           = BorderStyle.None,
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(248, 248, 255)
                },
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    SelectionBackColor = Color.FromArgb(255, 182, 193),
                    SelectionForeColor = Color.Black
                }
            };
        }

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

        private void MostrarError(string msg)
        {
            MessageBox.Show($"Error: {msg}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
