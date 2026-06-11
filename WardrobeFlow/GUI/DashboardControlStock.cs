using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using Servicios.Multiidioma;

namespace GUI
{
    public class DashboardControlStock : Form, IIdiomaObserver
    {
        private readonly BLL.Interfaces.IPrendaService _bllPrenda  = new BLL.Prenda();
        private readonly BLL.Usuario                   _bllUsuario = new BLL.Usuario();

        private System.Windows.Forms.Timer _timer;

        private Label           _lblTitulo;
        private Label           _lblSesion;
        private Button          _btnRefrescar;
        private Panel           _panelHeader;
        private Label           _numDisp, _txtDisp;
        private Label           _numMant, _txtMant;
        private Label           _numOcup, _txtOcup;
        private Panel           _cardMantPanel;
        private FlowLayoutPanel _flowCards;
        private FlowLayoutPanel _colReciente, _colEnCurso, _colUrgente;
        private Label           _lblColRec, _lblColCur, _lblColUrg;

        public DashboardControlStock()
        {
            this.Text            = "Panel de Stock";
            this.Size            = new Size(870, 570);
            this.MinimumSize     = new Size(600, 400);
            this.BackColor       = Color.FromArgb(240, 240, 245);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.StartPosition   = FormStartPosition.Manual;
            this.Location        = new Point(10, 10);
            ConstruirUI();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try { string ico = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico"); if (System.IO.File.Exists(ico)) this.Icon = new System.Drawing.Icon(ico); } catch { }
            GestorIdioma.SuscribirObservador(this);
            Traducir(GestorIdioma.IdiomaActual);
            CargarEnBackground();
            _timer = new System.Windows.Forms.Timer { Interval = 2 * 60 * 1000 };
            _timer.Tick += (s, ev) => CargarEnBackground();
            _timer.Start();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _timer?.Stop();
            _timer?.Dispose();
            GestorIdioma.DesuscribirObservador(this);
            base.OnFormClosing(e);
        }

        public void UpdateLanguage(Idioma idioma)
        {
            Traducir(idioma);
            CargarEnBackground();
        }

        private string T(string k, string fb)
        {
            var t = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
            return t.ContainsKey(k) ? t[k].Texto : fb;
        }

        private void Traducir(Idioma idioma)
        {
            var t = Traductor.ObtenerTraducciones(idioma);
            string Tr(string k, string fb) => t.ContainsKey(k) ? t[k].Texto : fb;

            this.Text          = Tr("dash.stock.titulo",   "Panel de Stock");
            _lblTitulo.Text    = Tr("dash.stock.titulo",   "Panel de Stock");
            _btnRefrescar.Text = Tr("dash.btn.refrescar",  "↻ Actualizar");
            if (_txtDisp != null) _txtDisp.Text = Tr("dash.prendas",    "Prendas\ndisponibles");
            if (_txtMant != null) _txtMant.Text = Tr("dash.mant.activo", "En\nmantenimiento");
            if (_txtOcup != null) _txtOcup.Text = Tr("dash.ocupacion",  "ocupación\ndel stock");
            _lblColRec.Text = Tr("dash.mant.reciente", "Reciente (< 2d)");
            _lblColCur.Text = Tr("dash.mant.encurso",  "En curso (2-7d)");
            _lblColUrg.Text = Tr("dash.mant.urgente",  "Urgente (> 7d)");
        }

        private void CargarEnBackground()
        {
            Task.Run(() =>
            {
                try
                {
                    var disponibles  = _bllPrenda.ObtenerDisponibles();
                    var enMant       = _bllPrenda.ObtenerEnMantenimiento();
                    var ocupacion    = _bllPrenda.ObtenerOcupacion();
                    this.BeginInvoke(new Action(() =>
                    {
                        if (IsDisposed) return;
                        ActualizarCards(disponibles, enMant, ocupacion);
                        ActualizarKanban(enMant);
                        ActualizarSesion();
                    }));
                }
                catch { }
            });
        }

        private void ActualizarCards(List<BE.Prenda> disponibles, List<BE.MantenimientoPrenda> enMant, BE.OcupacionStock ocup)
        {
            if (_numDisp != null) _numDisp.Text = disponibles.Count.ToString();
            if (_numMant != null)
            {
                _numMant.Text = enMant.Count.ToString();
                Color fondo = enMant.Count == 0
                    ? Color.FromArgb(215, 240, 220)
                    : enMant.Count > 5 ? Color.FromArgb(255, 218, 218) : Color.FromArgb(255, 248, 210);
                if (_cardMantPanel != null) _cardMantPanel.BackColor = fondo;
            }
            if (_numOcup != null && ocup != null)
            {
                _numOcup.Text = $"{ocup.PorcentajeOcupacion}%";
                _numOcup.Font = new Font("Segoe UI", 24f, FontStyle.Bold);
                if (_txtOcup != null)
                    _txtOcup.Text = $"{ocup.EnUso} en uso · {ocup.Disponibles} libres";
            }
        }

        private void ActualizarKanban(List<BE.MantenimientoPrenda> enMant)
        {
            _colReciente.Controls.Clear();
            _colEnCurso.Controls.Clear();
            _colUrgente.Controls.Clear();

            foreach (var m in enMant)
            {
                int    dias  = (int)(DateTime.Today - m.FechaEntrada.Date).TotalDays;
                string tit   = m.NombrePrenda ?? $"Prenda #{m.IdPrenda}";
                string sub   = $"Entrada: {m.FechaEntrada:dd/MM/yyyy}";

                if (dias < 2)
                    _colReciente.Controls.Add(CrearCard(tit, sub, dias, Color.FromArgb(210, 240, 220)));
                else if (dias <= 7)
                    _colEnCurso.Controls.Add(CrearCard(tit, sub, dias, Color.FromArgb(255, 248, 210)));
                else
                    _colUrgente.Controls.Add(CrearCard(tit, sub, dias, Color.FromArgb(255, 205, 200)));
            }

            if (_colReciente.Controls.Count == 0) _colReciente.Controls.Add(CrearVacio());
            if (_colEnCurso.Controls.Count  == 0) _colEnCurso.Controls.Add(CrearVacio());
            if (_colUrgente.Controls.Count  == 0) _colUrgente.Controls.Add(CrearVacio());
        }

        private void ActualizarSesion()
        {
            try
            {
                var u = _bllUsuario.ObtenerUsuarioActivo();
                var h = _bllUsuario.ObtenerFechaInicioSesion();
                if (u != null && _lblSesion != null)
                    _lblSesion.Text = $"{u.Username}  ·  {u.Perfil ?? "—"}" + (h.HasValue ? $"  ·  {h.Value:HH:mm}" : "");
            }
            catch { }
        }

        private void ConstruirUI()
        {
            _panelHeader = new Panel { Dock = DockStyle.Top, Height = 62, BackColor = Color.FromArgb(176, 62, 96) };
            _panelHeader.Paint += (s, pe) =>
            {
                using (var br = new LinearGradientBrush(_panelHeader.ClientRectangle,
                    Color.FromArgb(210, 100, 135), Color.FromArgb(176, 62, 96), LinearGradientMode.Horizontal))
                    pe.Graphics.FillRectangle(br, _panelHeader.ClientRectangle);
            };
            _lblTitulo = new Label { Text = "Panel de Stock", Font = new Font("Segoe UI", 14f, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(14, 8), BackColor = Color.Transparent };
            _btnRefrescar = new Button { Text = "↻  Actualizar", Size = new Size(100, 28), Anchor = AnchorStyles.Top | AnchorStyles.Right, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(210, 100, 135), ForeColor = Color.White, Font = new Font("Segoe UI", 8.5f), Cursor = Cursors.Hand, Location = new Point(756, 17) };
            _btnRefrescar.FlatAppearance.BorderColor = Color.FromArgb(180, 230, 140, 170);
            _btnRefrescar.FlatAppearance.BorderSize  = 1;
            _btnRefrescar.Click += (s, e) => CargarEnBackground();
            _panelHeader.Resize += (s, e) => _btnRefrescar.Left = _panelHeader.Width - 112;
            var lblSub = new Label { Text = "WardrobeFlow  —  Stock", Font = new Font("Segoe UI", 8f, FontStyle.Italic), ForeColor = Color.FromArgb(200, 255, 200, 220), AutoSize = true, Location = new Point(14, 36), BackColor = Color.Transparent };
            _panelHeader.Controls.AddRange(new Control[] { _lblTitulo, lblSub, _btnRefrescar });

            var panelSbar = new Panel { Dock = DockStyle.Bottom, Height = 26, BackColor = Color.FromArgb(176, 62, 96) };
            _lblSesion = new Label { Dock = DockStyle.Fill, ForeColor = Color.FromArgb(244, 212, 226), Font = new Font("Segoe UI", 8f), TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(10, 0, 0, 0) };
            panelSbar.Controls.Add(_lblSesion);

            _flowCards = new FlowLayoutPanel { Height = 168, Dock = DockStyle.Top, Padding = new Padding(10, 10, 10, 4), BackColor = Color.FromArgb(240, 240, 245), FlowDirection = FlowDirection.LeftToRight, WrapContents = false };

            var cardDisp = CrearTarjeta(Color.FromArgb(215, 240, 220), Color.FromArgb(15, 85, 35),  out _numDisp, out _txtDisp, out _);
            var cardMant = CrearTarjeta(Color.FromArgb(215, 240, 220), Color.FromArgb(15, 85, 35),  out _numMant, out _txtMant, out _cardMantPanel);
            var cardOcup = CrearTarjeta(Color.FromArgb(215, 240, 220), Color.FromArgb(15, 85, 35),  out _numOcup, out _txtOcup, out _);

            _flowCards.Controls.Add(cardDisp);
            _flowCards.Controls.Add(cardMant);
            _flowCards.Controls.Add(cardOcup);
            _flowCards.Resize += (s, e) =>
            {
                int cnt = _flowCards.Controls.Count;
                if (cnt == 0) return;
                int w = Math.Max(100, (_flowCards.ClientSize.Width - _flowCards.Padding.Horizontal - cnt * 8) / cnt);
                foreach (Control c in _flowCards.Controls) c.Width = w;
            };

            _lblColRec = new Label { Text = "Reciente (< 2d)", Dock = DockStyle.Top, Height = 30, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(15, 85, 35),   BackColor = Color.FromArgb(215, 240, 220), Padding = new Padding(8, 6, 0, 0) };
            _lblColCur = new Label { Text = "En curso (2-7d)", Dock = DockStyle.Top, Height = 30, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(120, 90, 0),   BackColor = Color.FromArgb(255, 248, 210), Padding = new Padding(8, 6, 0, 0) };
            _lblColUrg = new Label { Text = "Urgente (> 7d)",  Dock = DockStyle.Top, Height = 30, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(160, 20, 20),  BackColor = Color.FromArgb(255, 218, 218), Padding = new Padding(8, 6, 0, 0) };

            _colReciente = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, BackColor = Color.FromArgb(244, 252, 246), Padding = new Padding(6) };
            _colEnCurso  = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, BackColor = Color.FromArgb(252, 250, 240), Padding = new Padding(6) };
            _colUrgente  = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, BackColor = Color.FromArgb(252, 244, 244), Padding = new Padding(6) };

            AjustarAnchosCards(_colReciente);
            AjustarAnchosCards(_colEnCurso);
            AjustarAnchosCards(_colUrgente);

            var col1 = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 3, 0) };
            col1.Controls.Add(_colReciente); col1.Controls.Add(_lblColRec);
            var col2 = new Panel { Dock = DockStyle.Fill, Margin = new Padding(3, 0, 3, 0) };
            col2.Controls.Add(_colEnCurso);  col2.Controls.Add(_lblColCur);
            var col3 = new Panel { Dock = DockStyle.Fill, Margin = new Padding(3, 0, 0, 0) };
            col3.Controls.Add(_colUrgente);  col3.Controls.Add(_lblColUrg);

            var tbl = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1 };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34f));
            tbl.Controls.Add(col1, 0, 0);
            tbl.Controls.Add(col2, 1, 0);
            tbl.Controls.Add(col3, 2, 0);

            var wrapper = new Panel { Dock = DockStyle.Fill, Padding = new Padding(6), BackColor = Color.FromArgb(240, 240, 245) };
            wrapper.Controls.Add(tbl);

            this.Controls.Add(wrapper);
            this.Controls.Add(_flowCards);
            this.Controls.Add(_panelHeader);
            this.Controls.Add(panelSbar);
        }

        private static void AjustarAnchosCards(FlowLayoutPanel col)
        {
            col.Resize += (s, e) =>
            {
                int w = Math.Max(100, col.ClientSize.Width - col.Padding.Horizontal - 2);
                foreach (Control c in col.Controls) c.Width = w;
            };
        }

        private static Panel CrearTarjeta(Color fondo, Color tinta, out Label lblNum, out Label lblTxt, out Panel cardRef)
        {
            var card = new Panel { Width = 148, Height = 160, BackColor = fondo, Margin = new Padding(0, 0, 8, 0) };
            card.Paint += (s, pe) =>
            {
                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RoundedRect(new Rectangle(0, 0, card.Width - 1, card.Height - 1), 10))
                using (var br   = new SolidBrush(card.BackColor))
                    pe.Graphics.FillPath(br, path);
            };
            var num = new Label { Text = "…", Font = new Font("Segoe UI", 30f, FontStyle.Bold), ForeColor = tinta, AutoSize = false, TextAlign = ContentAlignment.BottomCenter, BackColor = Color.Transparent, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, Location = new Point(0, 20), Height = 78, Width = card.Width };
            var txt = new Label { Text = "", Font = new Font("Segoe UI", 8f), ForeColor = Color.FromArgb(Math.Min(tinta.R + 50, 255), Math.Min(tinta.G + 50, 255), Math.Min(tinta.B + 50, 255)), AutoSize = false, TextAlign = ContentAlignment.TopCenter, BackColor = Color.Transparent, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, Location = new Point(0, 102), Height = 44, Width = card.Width };
            card.Resize += (s, e) => { num.Width = card.Width; txt.Width = card.Width; };
            card.Controls.Add(num); card.Controls.Add(txt);
            lblNum = num; lblTxt = txt; cardRef = card;
            return card;
        }

        private static Panel CrearCard(string titulo, string sub, int dias, Color fondo)
        {
            var card = new Panel { Width = 180, Height = 64, BackColor = fondo, Margin = new Padding(0, 0, 0, 6) };
            card.Paint += (s, pe) =>
            {
                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RoundedRect(new Rectangle(0, 0, card.Width - 1, card.Height - 1), 6))
                using (var br   = new SolidBrush(card.BackColor))
                    pe.Graphics.FillPath(br, path);
            };
            card.Controls.Add(new Label { Text = titulo, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), AutoSize = true, Location = new Point(8, 6), BackColor = Color.Transparent });
            card.Controls.Add(new Label { Text = sub, Font = new Font("Segoe UI", 7.5f), AutoSize = false, Size = new Size(164, 16), Location = new Point(8, 24), BackColor = Color.Transparent, ForeColor = Color.FromArgb(70, 70, 80) });
            string dStr = dias == 0 ? "hoy" : $"hace {dias}d";
            card.Controls.Add(new Label { Text = dStr, Font = new Font("Segoe UI", 7f, FontStyle.Italic), AutoSize = true, Location = new Point(8, 44), BackColor = Color.Transparent, ForeColor = Color.FromArgb(110, 100, 80) });
            return card;
        }

        private static Panel CrearVacio()
        {
            var p = new Panel { Width = 180, Height = 28, BackColor = Color.Transparent };
            p.Controls.Add(new Label { Text = "— sin tareas —", Font = new Font("Segoe UI", 8f, FontStyle.Italic), ForeColor = Color.Silver, AutoSize = true, Location = new Point(6, 6), BackColor = Color.Transparent });
            return p;
        }

        private static GraphicsPath RoundedRect(Rectangle b, int r)
        {
            int d = r * 2;
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
