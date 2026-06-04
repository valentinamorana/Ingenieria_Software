using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Servicios.Multiidioma;

namespace GUI
{
    /// <summary>
    /// Panel de Control — se abre automáticamente al iniciar sesión como hijo MDI.
    ///
    /// Muestra solo las métricas a las que el usuario tiene permiso:
    ///   · Prendas disponibles  (requiere mnuPrendas)
    ///   · Clientes registrados (requiere mnuClientes)
    ///   · Pedidos pendientes   (requiere mnuPedidosVenta o mnuPedidosRealizados)
    ///   · Días sin backup      (requiere mnuUsuarios — solo Administrador)
    ///
    /// La tarjeta de Backup cambia de color según la antigüedad y muestra un aviso
    /// cuando se supera el umbral configurado (recordatorio.cfg en carpeta Backups).
    /// El botón ⚙ permite configurar el intervalo de recordatorio.
    ///
    /// Implementa IIdiomaObserver: las etiquetas se traducen al cambiar el idioma.
    /// </summary>
    public class DashboardForm : Form, IIdiomaObserver
    {
        private static readonly string DirBackups =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");

        // ── Dependencias BLL ──────────────────────────────────────────────────
        private readonly BLL.Interfaces.IPrendaService  _bllPrenda   = new BLL.Prenda();
        private readonly BLL.Interfaces.IClienteService _bllCliente  = new BLL.Cliente();
        private readonly BLL.Interfaces.IPedidoService  _bllPedido   = new BLL.Pedido();
        private readonly BLL.Usuario  _bllUsuario  = new BLL.Usuario();
        private readonly BLL.Bitacora _bllBitacora = new BLL.Bitacora();

        // ── Visibilidad por rol ───────────────────────────────────────────────
        private readonly bool _verPrendas, _verClientes, _verPedidos, _verBackup, _verStock;

        // ── Controles de tarjetas (null si el rol no tiene permiso) ───────────
        private Label _numPrendas,  _txtPrendas;
        private Label _numClientes, _txtClientes;
        private Label _numPedidos,  _txtPedidos;
        private Label _numBackup,   _txtBackup;
        private Label _numOcupacion, _txtOcupacion;
        private Panel _cardBackupPanel;

        // ── Controles generales ───────────────────────────────────────────────
        private Label           _lblTitulo;
        private Label           _lblSesion;
        private Label           _lblAviso;
        private Button          _btnRefrescar;
        private FlowLayoutPanel _flowCards;
        private Panel           _panelHeader;
        private Panel           _panelActividad;
        private Panel           _panelMiniStats;
        private Panel           _panelSbar;
        private Label        _lblActTitulo;
        private Label        _lblStTitulo;
        private DataGridView _dgvActividad;

        // ── Panel de Tareas Pendientes ────────────────────────────────────────
        private Panel        _panelTareas;
        private DataGridView _dgvTareas;
        private Label        _lblTareasTitulo;

        public DashboardForm(List<BE.Permiso> permisos)
        {
            var nombres = new HashSet<string>();
            if (permisos != null)
                foreach (var p in permisos)
                    if (p.NombreMenu != null) nombres.Add(p.NombreMenu);

            _verPrendas  = nombres.Contains("mnuPrendas");
            _verClientes = nombres.Contains("mnuClientes");
            _verPedidos  = nombres.Contains("mnuPedidosVenta") || nombres.Contains("mnuPedidosRealizados");
            _verBackup   = nombres.Contains("mnuUsuarios");
            _verStock    = nombres.Contains("mnuStock");

            this.Text            = "Panel de Control";
            this.Size            = new Size(870, 570);
            this.MinimumSize     = new Size(600, 400);
            this.BackColor       = Color.FromArgb(240, 240, 245);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.StartPosition   = FormStartPosition.Manual;
            this.Location        = new Point(10, 10);

            ConstruirUI();
        }

        // ── Ciclo de vida ─────────────────────────────────────────────────────

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try { string ico = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico"); if (File.Exists(ico)) this.Icon = new System.Drawing.Icon(ico); } catch { }
            GestorIdioma.SuscribirObservador(this);
            Traducir(GestorIdioma.IdiomaActual);
            ActualizarMetricas();
            CargarActividadReciente();
            CargarMiniStats();
            CargarTareasPendientes();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            GestorIdioma.DesuscribirObservador(this);
            base.OnFormClosing(e);
        }

        // ── IIdiomaObserver ───────────────────────────────────────────────────

        public void UpdateLanguage(Idioma idioma)
        {
            Traducir(idioma);
            ActualizarMetricas();
            CargarActividadReciente();
            CargarMiniStats();
            CargarTareasPendientes();
        }

        private string T(string key, string fallback)
        {
            var t = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
            return t.ContainsKey(key) ? t[key].Texto : fallback;
        }

        private void Traducir(Idioma idioma)
        {
            var t = Traductor.ObtenerTraducciones(idioma);
            string T(string k, string fb) => t.ContainsKey(k) ? t[k].Texto : fb;

            this.Text          = T("frm.dashboard",      "Panel de Control");
            _lblTitulo.Text    = T("frm.dashboard",      "Panel de Control");
            _btnRefrescar.Text = T("dash.btn.refrescar", "↻ Actualizar");

            if (_txtPrendas   != null) _txtPrendas.Text   = T("dash.prendas",    "Prendas\ndisponibles");
            if (_txtClientes  != null) _txtClientes.Text  = T("dash.clientes",   "Clientes\nregistrados");
            if (_txtPedidos   != null) _txtPedidos.Text   = T("dash.pedidos",    "Pedidos\npendientes");
            if (_txtBackup    != null) _txtBackup.Text    = T("dash.backup",     "días sin\nbackup");
            if (_txtOcupacion != null) _txtOcupacion.Text = T("dash.ocupacion",  "ocupación\ndel stock");

            if (_lblActTitulo != null) _lblActTitulo.Text = T("dash.actividad.titulo", "Actividad reciente");
            if (_lblStTitulo  != null) _lblStTitulo.Text  = T("dash.stats.titulo",     "Resumen de eventos");
            if (_dgvActividad != null && _dgvActividad.Columns.Count >= 3)
            {
                _dgvActividad.Columns["colFecha"].HeaderText = T("dash.col.fecha",   "Fecha");
                _dgvActividad.Columns["colTipo"].HeaderText  = T("dash.col.evento",  "Evento");
                _dgvActividad.Columns["colUser"].HeaderText  = T("dash.col.usuario", "Usuario");
            }
        }

        // ── Métricas ──────────────────────────────────────────────────────────

        private void ActualizarMetricas()
        {
            if (_verPrendas && _numPrendas != null)
            {
                try { _numPrendas.Text = _bllPrenda.ObtenerDisponibles().Count.ToString(); }
                catch { _numPrendas.Text = "—"; }
            }

            if (_verClientes && _numClientes != null)
            {
                try { _numClientes.Text = _bllCliente.ObtenerTodos().Count.ToString(); }
                catch { _numClientes.Text = "—"; }
            }

            if (_verPedidos && _numPedidos != null)
            {
                try { _numPedidos.Text = _bllPedido.ObtenerPendientes().Count.ToString(); }
                catch { _numPedidos.Text = "—"; }
            }

            if (_verBackup && _numBackup != null)
                ActualizarTarjetaBackup();

            if (_verPrendas && _numOcupacion != null)
                ActualizarTarjetaOcupacion();

            // Info de sesión
            try
            {
                var u    = _bllUsuario.ObtenerUsuarioActivo();
                var hora = _bllUsuario.ObtenerFechaInicioSesion();
                if (u != null)
                    _lblSesion.Text =
                        $"{u.Username}  ·  {u.Perfil ?? "—"}" +
                        (hora.HasValue ? $"  ·  {T("dash.sesion.iniciada", "Sesión iniciada:")} {hora.Value:HH:mm}" : "");
            }
            catch { _lblSesion.Text = ""; }
        }

        private void ActualizarTarjetaBackup()
        {
            try
            {
                FileInfo ultimo = null;
                if (Directory.Exists(DirBackups))
                    ultimo = new DirectoryInfo(DirBackups)
                        .GetFiles("*.bak")
                        .OrderByDescending(f => f.LastWriteTime)
                        .FirstOrDefault();

                int umbral = BLL.Configuracion.ObtenerDiasRecordatorio();

                if (ultimo == null)
                {
                    _numBackup.Text              = "!";
                    _numBackup.Font              = new Font("Segoe UI", 36f, FontStyle.Bold);
                    _cardBackupPanel.BackColor   = Color.FromArgb(255, 218, 218);
                    _numBackup.ForeColor         = Color.FromArgb(160, 20, 20);
                    _txtBackup.ForeColor         = Color.FromArgb(160, 20, 20);
                    _cardBackupPanel.Invalidate();
                    MostrarAviso(T("dash.aviso.sinbackup", "⚠  Sin backups. Generá uno desde Administrar → Backup."), Color.FromArgb(180, 30, 30));
                    return;
                }

                int dias = (int)(DateTime.Now - ultimo.LastWriteTime).TotalDays;

                // Número grande: días transcurridos (o "Hoy")
                if (dias == 0)
                {
                    _numBackup.Text = T("dash.backup.hoy", "Hoy");
                    _numBackup.Font = new Font("Segoe UI", 20f, FontStyle.Bold);
                }
                else
                {
                    _numBackup.Text = dias.ToString();
                    _numBackup.Font = new Font("Segoe UI", 36f, FontStyle.Bold);
                }

                // Código de color: verde → amarillo → rojo según antigüedad vs umbral
                Color fondo, tinta;
                if (dias <= umbral / 2)
                {
                    fondo = Color.FromArgb(215, 240, 220);   // verde
                    tinta = Color.FromArgb(15, 85, 35);
                }
                else if (dias <= umbral)
                {
                    fondo = Color.FromArgb(255, 248, 210);   // amarillo
                    tinta = Color.FromArgb(120, 90, 0);
                }
                else
                {
                    fondo = Color.FromArgb(255, 218, 218);   // rojo
                    tinta = Color.FromArgb(160, 20, 20);
                }

                _cardBackupPanel.BackColor = fondo;
                _numBackup.ForeColor       = tinta;
                _txtBackup.ForeColor       = tinta;
                _cardBackupPanel.Invalidate();

                if (dias > umbral)
                    MostrarAviso(
                        string.Format(T("dash.aviso.vencido", "⚠  Hace {0} día(s) sin backup — recordatorio cada {1} días."), dias, umbral),
                        Color.FromArgb(160, 60, 0));
                else
                    OcultarAviso();
            }
            catch { if (_numBackup != null) _numBackup.Text = "—"; }
        }

        private void MostrarAviso(string msg, Color color)
        {
            _lblAviso.Text      = msg;
            _lblAviso.ForeColor = color;
            _lblAviso.Height    = 24;
            _lblAviso.Visible   = true;
        }

        private void OcultarAviso()
        {
            _lblAviso.Visible = false;
            _lblAviso.Height  = 0;
        }

        // ── Ocupación del stock ───────────────────────────────────────────────

        private void ActualizarTarjetaOcupacion()
        {
            try
            {
                var oc = _bllPrenda.ObtenerOcupacion();
                if (_numOcupacion == null) return;

                _numOcupacion.Text = $"{oc.PorcentajeOcupacion}%";
                _numOcupacion.Font = new System.Drawing.Font("Segoe UI", 28f, System.Drawing.FontStyle.Bold);

                // Código de color: verde (< 70%) → amarillo (70-90%) → rojo (> 90%)
                System.Drawing.Color fondo, tinta;
                if (oc.PorcentajeOcupacion < 70)
                {
                    fondo = System.Drawing.Color.FromArgb(215, 240, 220);
                    tinta = System.Drawing.Color.FromArgb(15, 85, 35);
                }
                else if (oc.PorcentajeOcupacion <= 90)
                {
                    fondo = System.Drawing.Color.FromArgb(255, 248, 210);
                    tinta = System.Drawing.Color.FromArgb(120, 90, 0);
                }
                else
                {
                    fondo = System.Drawing.Color.FromArgb(255, 218, 218);
                    tinta = System.Drawing.Color.FromArgb(160, 20, 20);
                }

                if (_txtOcupacion != null)
                {
                    _txtOcupacion.Text = string.Format(
                        T("dash.ocupacion.detalle", "{0} en uso · {1} libres"),
                        oc.EnUso, oc.Disponibles);
                    _txtOcupacion.ForeColor = tinta;
                }
                _numOcupacion.ForeColor = tinta;
                // Actualizar fondo del card buscando el panel padre
                var card = _numOcupacion.Parent;
                if (card != null) card.BackColor = fondo;
            }
            catch { if (_numOcupacion != null) _numOcupacion.Text = "—"; }
        }

        // ── Recordatorio: config en archivo ──────────────────────────────────

        private void ConfigurarRecordatorio()
        {
            int actual = BLL.Configuracion.ObtenerDiasRecordatorio();

            using (var dlg = new Form())
            {
                dlg.Text            = T("dash.cfg.titulo", "Recordatorio de Backup");
                dlg.ClientSize      = new Size(300, 150);
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition   = FormStartPosition.CenterParent;
                dlg.MaximizeBox     = false;
                dlg.MinimizeBox     = false;
                dlg.BackColor       = Color.White;

                var lbl = new Label
                {
                    Text     = T("dash.cfg.recada", "Recordarme cada:"),
                    Left     = 16, Top = 22, Width = 268, Height = 20,
                    Font     = new Font("Segoe UI", 9f)
                };

                var spn = new NumericUpDown
                {
                    Left    = 16, Top = 48, Width = 80, Height = 28,
                    Minimum = 1, Maximum = 365, Value = actual,
                    Font    = new Font("Segoe UI", 10f)
                };

                var lblDias = new Label
                {
                    Text = T("dash.cfg.dias", "días"), Left = 104, Top = 52, Width = 60, Height = 20,
                    Font = new Font("Segoe UI", 9f)
                };

                var btnOk = new Button
                {
                    Text = T("dash.cfg.guardar", "Guardar"), Left = 80, Top = 104, Width = 90, Height = 30,
                    DialogResult = DialogResult.OK,
                    BackColor    = Color.FromArgb(146, 62, 96),
                    ForeColor    = Color.White, FlatStyle = FlatStyle.Flat
                };
                btnOk.FlatAppearance.BorderSize = 0;

                var btnCancelar = new Button
                {
                    Text = T("btn.cancelar", "Cancelar"), Left = 184, Top = 104, Width = 100, Height = 30,
                    DialogResult = DialogResult.Cancel, FlatStyle = FlatStyle.Flat
                };

                dlg.AcceptButton = btnOk;
                dlg.CancelButton = btnCancelar;
                dlg.Controls.AddRange(new Control[] { lbl, spn, lblDias, btnOk, btnCancelar });

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    BLL.Configuracion.GuardarDiasRecordatorio((int)spn.Value);
                    ActualizarMetricas();
                }
            }
        }

        // ── Construcción de UI ────────────────────────────────────────────────

        private void ConstruirUI()
        {
            // ── Header con gradiente ──────────────────────────────────────────
            _panelHeader = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 62,
                BackColor = Color.FromArgb(176, 62, 96)
            };
            _panelHeader.Paint += (s, pe) =>
            {
                using (var br = new System.Drawing.Drawing2D.LinearGradientBrush(
                    _panelHeader.ClientRectangle,
                    Color.FromArgb(64, 0, 64),
                    Color.FromArgb(176, 62, 96),
                    System.Drawing.Drawing2D.LinearGradientMode.Horizontal))
                    pe.Graphics.FillRectangle(br, _panelHeader.ClientRectangle);
            };

            _lblTitulo = new Label
            {
                Text      = "Panel de Control",
                Font      = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize  = true,
                Location  = new Point(14, 8),
                BackColor = Color.Transparent
            };

            _btnRefrescar = new Button
            {
                Text      = "↻  Actualizar",
                Size      = new Size(100, 28),
                Anchor    = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(210, 100, 135),
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 8.5f),
                Cursor    = Cursors.Hand,
                Location  = new Point(_panelHeader.Width - 112, 17)
            };
            _btnRefrescar.FlatAppearance.BorderColor = Color.FromArgb(180, 230, 140, 170);
            _btnRefrescar.FlatAppearance.BorderSize  = 1;
            _btnRefrescar.Click += (s, e) => { ActualizarMetricas(); CargarActividadReciente(); CargarMiniStats(); };

            var lblSub = new Label
            {
                Text      = "WardrobeFlow",
                Font      = new Font("Segoe UI", 8f, FontStyle.Italic),
                ForeColor = Color.FromArgb(200, 255, 200, 220),
                AutoSize  = true,
                Location  = new Point(14, 36),
                BackColor = Color.Transparent
            };

            _panelHeader.Controls.Add(_lblTitulo);
            _panelHeader.Controls.Add(lblSub);
            _panelHeader.Controls.Add(_btnRefrescar);
            _panelHeader.Resize += (s, e) => _btnRefrescar.Left = _panelHeader.Width - 112;

            // ── Cards area (scroll de tarjetas KPI) ──────────────────────────
            _flowCards = new FlowLayoutPanel
            {
                Height        = 168,
                Dock          = DockStyle.Top,
                Padding       = new Padding(10, 10, 10, 4),
                BackColor     = Color.FromArgb(240, 240, 245),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents  = false
            };

            if (_verPrendas)
                _flowCards.Controls.Add(CrearTarjeta(
                    Color.FromArgb(252, 228, 235), Color.FromArgb(80, 28, 52),
                    out _numPrendas, out _txtPrendas, out _));

            if (_verClientes)
                _flowCards.Controls.Add(CrearTarjeta(
                    Color.FromArgb(244, 212, 226), Color.FromArgb(110, 42, 74),
                    out _numClientes, out _txtClientes, out _));

            if (_verPedidos)
                _flowCards.Controls.Add(CrearTarjeta(
                    Color.FromArgb(236, 196, 215), Color.FromArgb(146, 62, 96),
                    out _numPedidos, out _txtPedidos, out _));

            if (_verBackup)
            {
                var tarjeta = CrearTarjeta(
                    Color.FromArgb(215, 240, 220), Color.FromArgb(15, 85, 35),
                    out _numBackup, out _txtBackup, out _cardBackupPanel);
                var btnConfig = new Button
                {
                    Text      = "⚙",
                    Font      = new Font("Segoe UI", 9f),
                    Size      = new Size(22, 22),
                    Location  = new Point(tarjeta.Width - 26, 4),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.Transparent,
                    ForeColor = Color.FromArgb(50, 100, 55),
                    Cursor    = Cursors.Hand,
                    TabStop   = false,
                    Anchor    = AnchorStyles.Top | AnchorStyles.Right
                };
                btnConfig.FlatAppearance.BorderSize = 0;
                btnConfig.Click += (s, e) => ConfigurarRecordatorio();
                tarjeta.Controls.Add(btnConfig);
                btnConfig.BringToFront();
                _flowCards.Controls.Add(tarjeta);
            }

            // ── Tarjeta de ocupación del stock ────────────────────────────────
            if (_verPrendas)
                _flowCards.Controls.Add(CrearTarjeta(
                    Color.FromArgb(215, 240, 220), Color.FromArgb(15, 85, 35),
                    out _numOcupacion, out _txtOcupacion, out _));

            // ── Aviso de backup vencido ───────────────────────────────────────
            _lblAviso = new Label
            {
                Text      = "",
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                AutoSize  = false,
                Dock      = DockStyle.Top,
                Height    = 0,
                Visible   = false,
                Padding   = new Padding(12, 0, 0, 0)
            };

            // ── Panel Actividad Reciente ──────────────────────────────────────
            _panelActividad = new Panel
            {
                Dock      = DockStyle.Left,
                Width     = 0,      // se calcula en Resize
                BackColor = Color.White,
                Padding   = new Padding(0)
            };

            _lblActTitulo = new Label
            {
                Text      = "Actividad reciente",
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(146, 62, 96),
                Dock      = DockStyle.Top,
                Height    = 28,
                Padding   = new Padding(10, 6, 0, 0),
                BackColor = Color.FromArgb(252, 240, 248)
            };

            _dgvActividad = new DataGridView
            {
                Name                        = "dgvActividad",
                Dock                        = DockStyle.Fill,
                BackgroundColor             = Color.White,
                BorderStyle                 = BorderStyle.None,
                RowHeadersVisible           = false,
                AllowUserToAddRows          = false,
                AllowUserToResizeRows       = false,
                AllowUserToResizeColumns    = false,
                ReadOnly                    = true,
                SelectionMode               = DataGridViewSelectionMode.FullRowSelect,
                EnableHeadersVisualStyles   = false,
                Font                        = new Font("Segoe UI", 8f),
                AutoSizeColumnsMode         = DataGridViewAutoSizeColumnsMode.Fill,
                CellBorderStyle             = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor                   = Color.FromArgb(235, 225, 232)
            };
            _dgvActividad.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(176, 62, 96);
            _dgvActividad.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _dgvActividad.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 8f, FontStyle.Bold);
            _dgvActividad.Columns.Add(new DataGridViewTextBoxColumn { Name = "colFecha", HeaderText = "Fecha",   FillWeight = 28 });
            _dgvActividad.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTipo",  HeaderText = "Evento",  FillWeight = 36 });
            _dgvActividad.Columns.Add(new DataGridViewTextBoxColumn { Name = "colUser",  HeaderText = "Usuario", FillWeight = 36 });

            _panelActividad.Controls.Add(_dgvActividad);
            _panelActividad.Controls.Add(_lblActTitulo);
            _panelActividad.Tag = _dgvActividad;

            // ── Panel Mini-Stats ──────────────────────────────────────────────
            _panelMiniStats = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Color.FromArgb(248, 244, 250),
                Padding   = new Padding(8)
            };

            _lblStTitulo = new Label
            {
                Text      = "Resumen de eventos",
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(64, 0, 64),
                Dock      = DockStyle.Top,
                Height    = 28,
                Padding   = new Padding(4, 6, 0, 0),
                BackColor = Color.FromArgb(240, 232, 248)
            };

            var flStats = new FlowLayoutPanel
            {
                Name          = "flStats",
                Dock          = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents  = false,
                BackColor     = Color.Transparent,
                Padding       = new Padding(6, 4, 6, 4)
            };

            _panelMiniStats.Controls.Add(flStats);
            _panelMiniStats.Controls.Add(_lblStTitulo);
            _panelMiniStats.Tag = flStats;

            // ── Session bar ───────────────────────────────────────────────────
            _panelSbar = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 26,
                BackColor = Color.FromArgb(64, 0, 64)
            };
            _lblSesion = new Label
            {
                Dock      = DockStyle.Fill,
                ForeColor = Color.FromArgb(200, 180, 210),
                Font      = new Font("Segoe UI", 8f),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(10, 0, 0, 0)
            };
            _panelSbar.Controls.Add(_lblSesion);

            // ── Área central (actividad + mini-stats) ─────────────────────────
            var panelCentro = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(240, 240, 245) };
            panelCentro.Controls.Add(_panelMiniStats);
            panelCentro.Controls.Add(_panelActividad);

            // Ajustar anchuras al arrancar y en resize
            void AjustarAnchuras()
            {
                int w = panelCentro.ClientSize.Width;
                _panelActividad.Width = (int)(w * 0.55);
            }
            panelCentro.Resize += (s, e) => AjustarAnchuras();

            // ── Panel Tareas Pendientes ───────────────────────────────────────
            bool hayTareas = _verPedidos || _verStock;
            _panelTareas = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = hayTareas ? 140 : 0,
                Visible   = hayTareas,
                BackColor = Color.White,
                Padding   = new Padding(0)
            };

            _lblTareasTitulo = new Label
            {
                Text      = "Mis Tareas Pendientes",
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(146, 62, 96),
                Dock      = DockStyle.Top,
                Height    = 26,
                Padding   = new Padding(10, 5, 0, 0),
                BackColor = Color.FromArgb(252, 240, 248)
            };

            _dgvTareas = new DataGridView
            {
                Dock                      = DockStyle.Fill,
                BackgroundColor           = Color.White,
                BorderStyle               = BorderStyle.None,
                RowHeadersVisible         = false,
                AllowUserToAddRows        = false,
                AllowUserToResizeRows     = false,
                ReadOnly                  = true,
                SelectionMode             = DataGridViewSelectionMode.FullRowSelect,
                EnableHeadersVisualStyles = false,
                Font                      = new Font("Segoe UI", 8f),
                AutoSizeColumnsMode       = DataGridViewAutoSizeColumnsMode.Fill,
                CellBorderStyle           = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor                 = Color.FromArgb(235, 225, 232)
            };
            _dgvTareas.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(176, 62, 96);
            _dgvTareas.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _dgvTareas.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 8f, FontStyle.Bold);
            _dgvTareas.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTipo",  HeaderText = "Tipo",        FillWeight = 22 });
            _dgvTareas.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDesc",  HeaderText = "Descripción", FillWeight = 56 });
            _dgvTareas.Columns.Add(new DataGridViewTextBoxColumn { Name = "colFecha", HeaderText = "Desde",       FillWeight = 22 });

            _panelTareas.Controls.Add(_dgvTareas);
            _panelTareas.Controls.Add(_lblTareasTitulo);

            // ── Ensamblar controles al formulario (orden: Dock procesa al revés)
            // Bottom → Top → Fill
            this.Controls.Add(panelCentro);
            this.Controls.Add(_panelTareas);
            this.Controls.Add(_lblAviso);
            this.Controls.Add(_flowCards);
            this.Controls.Add(_panelHeader);
            this.Controls.Add(_panelSbar);

            // Recalcular tarjetas al resize
            _flowCards.Resize += (s, e) =>
            {
                int count = _flowCards.Controls.Count;
                if (count > 0)
                {
                    int avail = _flowCards.ClientSize.Width - _flowCards.Padding.Horizontal - count * 8;
                    int cardW = Math.Max(100, avail / count);
                    foreach (Control card in _flowCards.Controls)
                        card.Width = cardW;
                }
            };
        }

        private void CargarTareasPendientes()
        {
            if (_dgvTareas == null || !_panelTareas.Visible) return;
            _dgvTareas.Rows.Clear();

            string tipoMant   = T("dash.tarea.mantenimiento", "Mantenimiento");
            string tipoPedido = T("dash.tarea.pedido",        "Pedido");

            try
            {
                if (_verStock)
                {
                    var enMant = _bllPrenda.ObtenerEnMantenimiento();
                    foreach (var m in enMant)
                    {
                        int dias = (int)(DateTime.Today - m.FechaEntrada.Date).TotalDays;
                        string desde = dias == 0
                            ? T("dash.hoy", "hoy")
                            : string.Format(T("dash.hace_dias", "hace {0}d"), dias);
                        var fila = _dgvTareas.Rows[_dgvTareas.Rows.Add(tipoMant, m.NombrePrenda, desde)];
                        fila.DefaultCellStyle.ForeColor = dias >= 3
                            ? Color.FromArgb(160, 60, 0)
                            : Color.FromArgb(60, 100, 60);
                    }
                }

                if (_verPedidos)
                {
                    var pendientes = _bllPedido.ObtenerPendientes();
                    foreach (var p in pendientes)
                    {
                        int dias = (int)(DateTime.Today - p.FechaPedido.Date).TotalDays;
                        string desde = dias == 0
                            ? T("dash.hoy", "hoy")
                            : string.Format(T("dash.hace_dias", "hace {0}d"), dias);
                        string desc = $"#{p.IdPedido} — {p.NombreCliente ?? $"Cliente {p.IdCliente}"}";
                        var fila = _dgvTareas.Rows[_dgvTareas.Rows.Add(tipoPedido, desc, desde)];
                        fila.DefaultCellStyle.ForeColor = dias >= 2
                            ? Color.FromArgb(160, 40, 40)
                            : Color.FromArgb(40, 80, 140);
                    }
                }

                if (_dgvTareas.Rows.Count == 0)
                {
                    _dgvTareas.Rows.Add("—", T("dash.tareas.sinpendientes", "Sin tareas pendientes"), "—");
                    _dgvTareas.Rows[0].DefaultCellStyle.ForeColor = Color.Gray;
                }

                _lblTareasTitulo.Text = string.Format(
                    T("dash.tareas.titulo", "Mis Tareas Pendientes ({0})"),
                    _dgvTareas.Rows.Count == 1 && _dgvTareas.Rows[0].Cells["colTipo"].Value?.ToString() == "—"
                        ? 0
                        : _dgvTareas.Rows.Count);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[DashboardForm.CargarTareasPendientes] " + ex.Message);
            }
        }

        private void CargarActividadReciente()
        {
            var dgv = _dgvActividad;
            if (dgv == null) return;
            dgv.Rows.Clear();
            try
            {
                var dt = _bllBitacora.ObtenerUltimosNDiasSistema(7);
                if (dt == null) return;
                int n = 0;
                foreach (System.Data.DataRow row in dt.Rows)
                {
                    if (n >= 8) break;
                    string fecha = row["fecha"]?.ToString() ?? "";
                    string activ = row["actividad"]?.ToString() ?? "";
                    string user  = row["usuario"]?.ToString() ?? "";
                    dgv.Rows.Add(fecha, activ, user);
                    n++;
                }
            }
            catch (Exception ex)
            {
                // Widget no crítico: no interrumpe el Dashboard, pero deja traza para diagnóstico.
                System.Diagnostics.Trace.TraceError("[DashboardForm.CargarActividadReciente] " + ex.Message);
            }
        }

        private void CargarMiniStats()
        {
            var fl = _panelMiniStats?.Tag as FlowLayoutPanel;
            if (fl == null) return;
            fl.Controls.Clear();
            try
            {
                // Negocio: últimos 30 días
                var dtN = _bllBitacora.ObtenerUltimosNDiasSistema(30);
                int totalSistema = dtN?.Rows.Count ?? 0;

                var statsItems = new[]
                {
                    ("Sistema (30d)", totalSistema.ToString(), Color.FromArgb(64, 0, 64)),
                };

                foreach (var (etiqueta, valor, color) in statsItems)
                    fl.Controls.Add(CrearMiniStatRow(etiqueta, valor, color));
            }
            catch { }

            try
            {
                var dtNeg = _bllBitacora.ObtenerTodosNegocio();
                if (dtNeg != null)
                {
                    var conteos = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                    foreach (System.Data.DataRow r in dtNeg.Rows)
                    {
                        string tipo = r["Tipo"]?.ToString() ?? "";
                        if (string.IsNullOrEmpty(tipo)) continue;
                        if (!conteos.ContainsKey(tipo)) conteos[tipo] = 0;
                        conteos[tipo]++;
                    }
                    foreach (var kv in conteos)
                        fl.Controls.Add(CrearMiniStatRow(kv.Key, kv.Value.ToString(), Color.FromArgb(146, 62, 96)));
                }
            }
            catch { }
        }

        private static Panel CrearMiniStatRow(string label, string valor, Color color)
        {
            var row = new Panel { Height = 26, Dock = DockStyle.Top, BackColor = Color.Transparent, Width = 200 };
            var lv = new Label { Text = valor, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = color, AutoSize = true, Location = new Point(0, 4) };
            var ll = new Label { Text = label, Font = new Font("Segoe UI", 8f), ForeColor = Color.FromArgb(100, 80, 100), AutoSize = true, Location = new Point(34, 6) };
            row.Controls.Add(lv);
            row.Controls.Add(ll);
            return row;
        }

        private static Panel CrearTarjeta(Color fondo, Color tinta,
            out Label lblNum, out Label lblTxt, out Panel cardRef)
        {
            var card = new Panel
            {
                Width     = 148,
                Height    = 160,
                BackColor = fondo,
                Margin    = new Padding(0, 0, 8, 0)
            };

            card.Paint += (s, pe) =>
            {
                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RoundedRect(new Rectangle(0, 0, card.Width - 1, card.Height - 1), 10))
                using (var br   = new SolidBrush(card.BackColor))
                    pe.Graphics.FillPath(br, path);
            };

            var num = new Label
            {
                Text      = "…",
                Font      = new Font("Segoe UI", 30f, FontStyle.Bold),
                ForeColor = tinta,
                AutoSize  = false,
                TextAlign = ContentAlignment.BottomCenter,
                BackColor = Color.Transparent,
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Location  = new Point(0, 20),
                Height    = 78,
                Width     = card.Width
            };

            var txt = new Label
            {
                Text      = "",
                Font      = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(
                    Math.Min(tinta.R + 50, 255),
                    Math.Min(tinta.G + 50, 255),
                    Math.Min(tinta.B + 50, 255)),
                AutoSize  = false,
                TextAlign = ContentAlignment.TopCenter,
                BackColor = Color.Transparent,
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Location  = new Point(0, 102),
                Height    = 44,
                Width     = card.Width
            };

            card.Resize += (s, e) => { num.Width = card.Width; txt.Width = card.Width; };
            card.Controls.Add(num);
            card.Controls.Add(txt);

            lblNum  = num;
            lblTxt  = txt;
            cardRef = card;
            return card;
        }

        private static GraphicsPath RoundedRect(Rectangle b, int r)
        {
            int d    = r * 2;
            var path = new GraphicsPath();
            path.AddArc(b.X,         b.Y,          d, d, 180, 90);
            path.AddArc(b.Right - d, b.Y,          d, d, 270, 90);
            path.AddArc(b.Right - d, b.Bottom - d, d, d,   0, 90);
            path.AddArc(b.X,         b.Bottom - d, d, d,  90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
