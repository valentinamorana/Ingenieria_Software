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
        private static readonly string RutaConfig =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups", "recordatorio.cfg");

        // ── Dependencias BLL ──────────────────────────────────────────────────
        private readonly BLL.Prenda  _bllPrenda  = new BLL.Prenda();
        private readonly BLL.Cliente _bllCliente = new BLL.Cliente();
        private readonly BLL.Pedido  _bllPedido  = new BLL.Pedido();
        private readonly BLL.Usuario _bllUsuario = new BLL.Usuario();

        // ── Visibilidad por rol ───────────────────────────────────────────────
        private readonly bool _verPrendas, _verClientes, _verPedidos, _verBackup;

        // ── Controles de tarjetas (null si el rol no tiene permiso) ───────────
        private Label _numPrendas,  _txtPrendas;
        private Label _numClientes, _txtClientes;
        private Label _numPedidos,  _txtPedidos;
        private Label _numBackup,   _txtBackup;
        private Panel _cardBackupPanel;

        // ── Controles generales ───────────────────────────────────────────────
        private Label           _lblTitulo;
        private Label           _lblSesion;
        private Label           _lblAviso;
        private Button          _btnRefrescar;
        private FlowLayoutPanel _flowCards;

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

            this.Text            = "Panel de Control";
            this.Size            = new Size(660, 340);
            this.MinimumSize     = new Size(380, 280);
            this.BackColor       = Color.FromArgb(250, 240, 246);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.StartPosition   = FormStartPosition.Manual;
            this.Location        = new Point(10, 10);

            ConstruirUI();
        }

        // ── Ciclo de vida ─────────────────────────────────────────────────────

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GestorIdioma.SuscribirObservador(this);
            Traducir(GestorIdioma.IdiomaActual);
            ActualizarMetricas();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            GestorIdioma.DesuscribirObservador(this);
            base.OnFormClosing(e);
        }

        // ── IIdiomaObserver ───────────────────────────────────────────────────

        public void UpdateLanguage(Idioma idioma) => Traducir(idioma);

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

            if (_txtPrendas  != null) _txtPrendas.Text  = T("dash.prendas",  "Prendas\ndisponibles");
            if (_txtClientes != null) _txtClientes.Text = T("dash.clientes", "Clientes\nregistrados");
            if (_txtPedidos  != null) _txtPedidos.Text  = T("dash.pedidos",  "Pedidos\npendientes");
            if (_txtBackup   != null) _txtBackup.Text   = T("dash.backup",   "días sin\nbackup");
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

                int umbral = LeerRecordatorioDias();

                if (ultimo == null)
                {
                    _numBackup.Text              = "!";
                    _numBackup.Font              = new Font("Segoe UI", 36f, FontStyle.Bold);
                    _cardBackupPanel.BackColor   = Color.FromArgb(255, 218, 218);
                    _numBackup.ForeColor         = Color.FromArgb(160, 20, 20);
                    _txtBackup.ForeColor         = Color.FromArgb(160, 20, 20);
                    _cardBackupPanel.Invalidate();
                    MostrarAviso("⚠  Sin backups generados. Generá uno desde Administrar → Backup.", Color.FromArgb(180, 30, 30));
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
                        $"⚠  Hace {dias} día(s) sin backup — recordatorio configurado cada {umbral} días.",
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
            _lblAviso.Visible   = true;
        }

        private void OcultarAviso() => _lblAviso.Visible = false;

        // ── Recordatorio: config en archivo ──────────────────────────────────

        private static int LeerRecordatorioDias()
        {
            try
            {
                if (File.Exists(RutaConfig) &&
                    int.TryParse(File.ReadAllText(RutaConfig).Trim(), out int d) && d > 0)
                    return d;
            }
            catch { }
            return 7;   // default: 1 semana
        }

        private static void GuardarRecordatorioDias(int dias)
        {
            try
            {
                if (!Directory.Exists(DirBackups)) Directory.CreateDirectory(DirBackups);
                File.WriteAllText(RutaConfig, dias.ToString());
            }
            catch { }
        }

        private void ConfigurarRecordatorio()
        {
            int actual = LeerRecordatorioDias();

            using (var dlg = new Form())
            {
                dlg.Text            = "Recordatorio de Backup";
                dlg.ClientSize      = new Size(300, 150);
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition   = FormStartPosition.CenterParent;
                dlg.MaximizeBox     = false;
                dlg.MinimizeBox     = false;
                dlg.BackColor       = Color.White;

                var lbl = new Label
                {
                    Text     = "Recordarme cada:",
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
                    Text = "días", Left = 104, Top = 52, Width = 60, Height = 20,
                    Font = new Font("Segoe UI", 9f)
                };

                var btnOk = new Button
                {
                    Text = "Guardar", Left = 80, Top = 104, Width = 90, Height = 30,
                    DialogResult = DialogResult.OK,
                    BackColor    = Color.FromArgb(146, 62, 96),
                    ForeColor    = Color.White, FlatStyle = FlatStyle.Flat
                };
                btnOk.FlatAppearance.BorderSize = 0;

                var btnCancelar = new Button
                {
                    Text = "Cancelar", Left = 184, Top = 104, Width = 100, Height = 30,
                    DialogResult = DialogResult.Cancel, FlatStyle = FlatStyle.Flat
                };

                dlg.AcceptButton = btnOk;
                dlg.CancelButton = btnCancelar;
                dlg.Controls.AddRange(new Control[] { lbl, spn, lblDias, btnOk, btnCancelar });

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    GuardarRecordatorioDias((int)spn.Value);
                    ActualizarMetricas();
                }
            }
        }

        // ── Construcción de UI ────────────────────────────────────────────────

        private void ConstruirUI()
        {
            _lblTitulo = new Label
            {
                Text      = "Panel de Control",
                Font      = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.FromArgb(80, 30, 55),
                AutoSize  = true,
                Location  = new Point(16, 14)
            };

            _btnRefrescar = new Button
            {
                Text      = "Actualizar",
                Size      = new Size(92, 28),
                Anchor    = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(146, 62, 96),
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 8.5f),
                Cursor    = Cursors.Hand
            };
            _btnRefrescar.FlatAppearance.BorderSize = 0;
            _btnRefrescar.Click   += (s, e) => ActualizarMetricas();
            _btnRefrescar.Location = new Point(this.ClientSize.Width - 104, 12);

            _flowCards = new FlowLayoutPanel
            {
                Location      = new Point(12, 52),
                Height        = 168,
                Anchor        = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor     = Color.Transparent,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents  = false
            };
            _flowCards.Width = this.ClientSize.Width - 24;

            // Agregar solo tarjetas permitidas por rol
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

                // Botón ⚙ para configurar recordatorio (superpuesto en esquina superior derecha)
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

            // Aviso de recordatorio vencido (oculto por defecto)
            _lblAviso = new Label
            {
                Text      = "",
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                AutoSize  = false,
                Height    = 22,
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Left      = 12,
                Top       = 228,
                Visible   = false
            };
            _lblAviso.Width = this.ClientSize.Width - 24;

            var sep = new Panel
            {
                BackColor = Color.FromArgb(220, 215, 225),
                Height    = 1,
                Anchor    = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Left      = 0,
                Top       = this.ClientSize.Height - 38
            };
            sep.Width = this.ClientSize.Width;

            _lblSesion = new Label
            {
                Text      = "",
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = Color.DimGray,
                AutoSize  = false,
                Height    = 20,
                Anchor    = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Left      = 16,
                Top       = this.ClientSize.Height - 30
            };
            _lblSesion.Width = this.ClientSize.Width - 32;

            this.Controls.Add(_lblTitulo);
            this.Controls.Add(_btnRefrescar);
            this.Controls.Add(_flowCards);
            this.Controls.Add(_lblAviso);
            this.Controls.Add(sep);
            this.Controls.Add(_lblSesion);

            this.Resize += (s, e) =>
            {
                _flowCards.Width   = this.ClientSize.Width - 24;
                _lblAviso.Width    = this.ClientSize.Width - 24;
                sep.Width          = this.ClientSize.Width;
                sep.Top            = this.ClientSize.Height - 38;
                _lblSesion.Width   = this.ClientSize.Width - 32;
                _lblSesion.Top     = this.ClientSize.Height - 30;
                _btnRefrescar.Left = this.ClientSize.Width - 104;

                int count = _flowCards.Controls.Count;
                if (count > 0)
                {
                    int cardW = Math.Max(100, (_flowCards.Width - count * 8) / count);
                    foreach (Control card in _flowCards.Controls)
                        card.Width = cardW;
                }
            };
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
