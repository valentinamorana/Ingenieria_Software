using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using Servicios.Multiidioma;

namespace GUI
{
    public class DashboardOperador : Form, IIdiomaObserver
    {
        private readonly BLL.Interfaces.IPedidoService _bllPedido  = new BLL.Pedido();
        private readonly BLL.Usuario                   _bllUsuario = new BLL.Usuario();

        private System.Windows.Forms.Timer _timer;

        private Label           _lblTitulo;
        private Label           _lblSesion;
        private Button          _btnRefrescar;
        private Panel           _panelHeader;
        private Label           _numPend, _txtPend;
        private Label           _numDesp, _txtDesp;
        private Label           _numEntr, _txtEntr;
        private FlowLayoutPanel _flowCards;
        private FlowLayoutPanel _colPendiente, _colDespachado, _colEntregado;
        private Label           _lblColPend, _lblColDesp, _lblColEntr;

        public DashboardOperador()
        {
            this.Text            = "Panel de Operaciones";
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

            this.Text          = Tr("dash.operador.titulo", "Panel de Operaciones");
            _lblTitulo.Text    = Tr("dash.operador.titulo", "Panel de Operaciones");
            _btnRefrescar.Text = Tr("dash.btn.refrescar",   "↻ Actualizar");
            if (_txtPend != null) _txtPend.Text = Tr("dash.pedidos",   "Pedidos\npendientes");
            if (_txtDesp != null) _txtDesp.Text = Tr("dash.despachados", "Pedidos\ndespachados");
            if (_txtEntr != null) _txtEntr.Text = Tr("dash.entregados",  "Pedidos\nentregados");
            _lblColPend.Text = Tr("dash.kan.pendiente",  "Pendiente");
            _lblColDesp.Text = Tr("dash.kan.despachado", "Despachado");
            _lblColEntr.Text = Tr("dash.kan.entregado",  "Entregado");
        }

        private void CargarEnBackground()
        {
            Task.Run(() =>
            {
                try
                {
                    var pedidos = _bllPedido.ObtenerTodos();
                    this.BeginInvoke(new Action(() =>
                    {
                        if (IsDisposed) return;
                        ActualizarCards(pedidos);
                        ActualizarKanban(pedidos);
                        ActualizarSesion();
                    }));
                }
                catch { }
            });
        }

        private void ActualizarCards(List<BE.Pedido> pedidos)
        {
            int pend = 0, desp = 0, entr = 0;
            foreach (var p in pedidos)
            {
                if (p.Estado == BE.EstadoPedido.Pendiente)  pend++;
                else if (p.Estado == BE.EstadoPedido.Despachado) desp++;
                else if (p.Estado == BE.EstadoPedido.Entregado)  entr++;
            }
            if (_numPend != null) _numPend.Text = pend.ToString();
            if (_numDesp != null) _numDesp.Text = desp.ToString();
            if (_numEntr != null) _numEntr.Text = entr.ToString();
        }

        private void ActualizarKanban(List<BE.Pedido> pedidos)
        {
            _colPendiente.Controls.Clear();
            _colDespachado.Controls.Clear();
            _colEntregado.Controls.Clear();

            foreach (var p in pedidos)
            {
                int    dias = (int)(DateTime.Today - p.FechaPedido.Date).TotalDays;
                string tit  = $"Pedido #{p.IdPedido}";
                string sub  = p.NombreCliente ?? $"Cliente {p.IdCliente}";

                switch (p.Estado)
                {
                    case BE.EstadoPedido.Pendiente:
                        _colPendiente.Controls.Add(CrearCard(tit, sub, dias,
                            dias >= 2 ? Color.FromArgb(255, 205, 200) : Color.FromArgb(255, 242, 200)));
                        break;
                    case BE.EstadoPedido.Despachado:
                        _colDespachado.Controls.Add(CrearCard(tit, sub, dias, Color.FromArgb(205, 225, 255)));
                        break;
                    case BE.EstadoPedido.Entregado:
                        _colEntregado.Controls.Add(CrearCard(tit, sub, dias, Color.FromArgb(210, 240, 220)));
                        break;
                }
            }

            if (_colPendiente.Controls.Count  == 0) _colPendiente.Controls.Add(CrearVacio());
            if (_colDespachado.Controls.Count == 0) _colDespachado.Controls.Add(CrearVacio());
            if (_colEntregado.Controls.Count  == 0) _colEntregado.Controls.Add(CrearVacio());
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
            _lblTitulo = new Label { Text = "Panel de Operaciones", Font = new Font("Segoe UI", 14f, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(14, 8), BackColor = Color.Transparent };
            _btnRefrescar = new Button { Text = "↻  Actualizar", Size = new Size(100, 28), Anchor = AnchorStyles.Top | AnchorStyles.Right, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(210, 100, 135), ForeColor = Color.White, Font = new Font("Segoe UI", 8.5f), Cursor = Cursors.Hand, Location = new Point(756, 17) };
            _btnRefrescar.FlatAppearance.BorderColor = Color.FromArgb(180, 230, 140, 170);
            _btnRefrescar.FlatAppearance.BorderSize  = 1;
            _btnRefrescar.Click += (s, e) => CargarEnBackground();
            _panelHeader.Resize += (s, e) => _btnRefrescar.Left = _panelHeader.Width - 112;
            var lblSub = new Label { Text = "WardrobeFlow  —  Operaciones", Font = new Font("Segoe UI", 8f, FontStyle.Italic), ForeColor = Color.FromArgb(200, 255, 200, 220), AutoSize = true, Location = new Point(14, 36), BackColor = Color.Transparent };
            _panelHeader.Controls.AddRange(new Control[] { _lblTitulo, lblSub, _btnRefrescar });

            var panelSbar = new Panel { Dock = DockStyle.Bottom, Height = 26, BackColor = Color.FromArgb(176, 62, 96) };
            _lblSesion = new Label { Dock = DockStyle.Fill, ForeColor = Color.FromArgb(244, 212, 226), Font = new Font("Segoe UI", 8f), TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(10, 0, 0, 0) };
            panelSbar.Controls.Add(_lblSesion);

            _flowCards = new FlowLayoutPanel { Height = 168, Dock = DockStyle.Top, Padding = new Padding(10, 10, 10, 4), BackColor = Color.FromArgb(240, 240, 245), FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
            _flowCards.Controls.Add(CrearTarjeta(Color.FromArgb(255, 242, 200), Color.FromArgb(120, 80, 0),  out _numPend, out _txtPend));
            _flowCards.Controls.Add(CrearTarjeta(Color.FromArgb(205, 225, 255), Color.FromArgb(20, 60, 130), out _numDesp, out _txtDesp));
            _flowCards.Controls.Add(CrearTarjeta(Color.FromArgb(210, 240, 220), Color.FromArgb(15, 85, 35),  out _numEntr, out _txtEntr));
            _flowCards.Resize += (s, e) =>
            {
                int cnt = _flowCards.Controls.Count;
                if (cnt == 0) return;
                int w = Math.Max(100, (_flowCards.ClientSize.Width - _flowCards.Padding.Horizontal - cnt * 8) / cnt);
                foreach (Control c in _flowCards.Controls) c.Width = w;
            };

            _lblColPend = new Label { Text = "Pendiente",  Dock = DockStyle.Top, Height = 30, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(120, 80, 0),   BackColor = Color.FromArgb(255, 248, 210), Padding = new Padding(8, 6, 0, 0) };
            _lblColDesp = new Label { Text = "Despachado", Dock = DockStyle.Top, Height = 30, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(20, 60, 130),   BackColor = Color.FromArgb(220, 230, 252), Padding = new Padding(8, 6, 0, 0) };
            _lblColEntr = new Label { Text = "Entregado",  Dock = DockStyle.Top, Height = 30, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(15, 85, 35),    BackColor = Color.FromArgb(215, 240, 220), Padding = new Padding(8, 6, 0, 0) };

            _colPendiente  = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, BackColor = Color.FromArgb(252, 250, 242), Padding = new Padding(6) };
            _colDespachado = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, BackColor = Color.FromArgb(242, 246, 252), Padding = new Padding(6) };
            _colEntregado  = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, BackColor = Color.FromArgb(244, 252, 246), Padding = new Padding(6) };

            AjustarAnchosCards(_colPendiente);
            AjustarAnchosCards(_colDespachado);
            AjustarAnchosCards(_colEntregado);

            var col1 = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 3, 0) };
            col1.Controls.Add(_colPendiente);  col1.Controls.Add(_lblColPend);
            var col2 = new Panel { Dock = DockStyle.Fill, Margin = new Padding(3, 0, 3, 0) };
            col2.Controls.Add(_colDespachado); col2.Controls.Add(_lblColDesp);
            var col3 = new Panel { Dock = DockStyle.Fill, Margin = new Padding(3, 0, 0, 0) };
            col3.Controls.Add(_colEntregado);  col3.Controls.Add(_lblColEntr);

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

        private static Panel CrearTarjeta(Color fondo, Color tinta, out Label lblNum, out Label lblTxt)
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
            lblNum = num; lblTxt = txt;
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
            card.Controls.Add(new Label { Text = dStr, Font = new Font("Segoe UI", 7f, FontStyle.Italic), AutoSize = true, Location = new Point(8, 44), BackColor = Color.Transparent, ForeColor = Color.FromArgb(110, 100, 100) });
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
