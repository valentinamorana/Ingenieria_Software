using System;
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
    /// Muestra un resumen en tiempo real del estado del sistema:
    ///   · Prendas disponibles  · Clientes registrados
    ///   · Pedidos pendientes   · Último backup generado
    /// También muestra la info de la sesión activa (usuario, rol, hora de inicio).
    ///
    /// Implementa IIdiomaObserver: las etiquetas de las tarjetas se traducen
    /// automáticamente cuando el usuario cambia el idioma desde el Menú.
    /// </summary>
    public class DashboardForm : Form, IIdiomaObserver
    {
        private static readonly string DirBackups =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");

        // ── Dependencias BLL ──────────────────────────────────────────────────
        private readonly BLL.Prenda  _bllPrenda  = new BLL.Prenda();
        private readonly BLL.Cliente _bllCliente = new BLL.Cliente();
        private readonly BLL.Pedido  _bllPedido  = new BLL.Pedido();
        private readonly BLL.Usuario _bllUsuario = new BLL.Usuario();

        // ── Controles de tarjetas ─────────────────────────────────────────────
        private Label _numPrendas,  _txtPrendas;
        private Label _numClientes, _txtClientes;
        private Label _numPedidos,  _txtPedidos;
        private Label _numBackup,   _txtBackup;

        // ── Controles generales ───────────────────────────────────────────────
        private Label  _lblTitulo;
        private Label  _lblSesion;
        private Button _btnRefrescar;
        private TableLayoutPanel _tblCards;

        public DashboardForm()
        {
            this.Text            = "Panel de Control";
            this.Size            = new Size(660, 320);
            this.MinimumSize     = new Size(520, 260);
            this.BackColor       = Color.White;
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

        private void Traducir(Idioma idioma)
        {
            var t = Traductor.ObtenerTraducciones(idioma);
            string T(string k, string fb) => t.ContainsKey(k) ? t[k].Texto : fb;

            this.Text           = T("frm.dashboard",  "Panel de Control");
            _lblTitulo.Text     = T("frm.dashboard",  "Panel de Control");
            _txtPrendas.Text    = T("dash.prendas",   "Prendas\ndisponibles");
            _txtClientes.Text   = T("dash.clientes",  "Clientes\nregistrados");
            _txtPedidos.Text    = T("dash.pedidos",   "Pedidos\npendientes");
            _txtBackup.Text     = T("dash.backup",    "Último\nbackup");
            _btnRefrescar.Text  = T("btn.refrescar",  "Actualizar");
        }

        // ── Métricas ──────────────────────────────────────────────────────────

        private void ActualizarMetricas()
        {
            // Prendas disponibles
            try { _numPrendas.Text = _bllPrenda.ObtenerDisponibles().Count.ToString(); }
            catch { _numPrendas.Text = "—"; }

            // Clientes
            try { _numClientes.Text = _bllCliente.ObtenerTodos().Count.ToString(); }
            catch { _numClientes.Text = "—"; }

            // Pedidos pendientes
            try { _numPedidos.Text = _bllPedido.ObtenerPendientes().Count.ToString(); }
            catch { _numPedidos.Text = "—"; }

            // Último backup
            try
            {
                if (Directory.Exists(DirBackups))
                {
                    var ultimo = new DirectoryInfo(DirBackups)
                        .GetFiles("*.bak")
                        .OrderByDescending(f => f.LastWriteTime)
                        .FirstOrDefault();
                    _numBackup.Text = ultimo != null
                        ? ultimo.LastWriteTime.ToString("dd/MM\nHH:mm")
                        : "—";
                }
                else { _numBackup.Text = "—"; }
            }
            catch { _numBackup.Text = "—"; }

            // Info de sesión
            try
            {
                var u    = _bllUsuario.ObtenerUsuarioActivo();
                var hora = _bllUsuario.ObtenerFechaInicioSesion();
                if (u != null)
                    _lblSesion.Text =
                        $"{u.Username}  ·  {u.Perfil ?? "—"}" +
                        (hora.HasValue ? $"  ·  Sesión iniciada: {hora.Value:HH:mm}" : "");
            }
            catch { _lblSesion.Text = ""; }
        }

        // ── Construcción de UI ────────────────────────────────────────────────

        private void ConstruirUI()
        {
            // Título
            _lblTitulo = new Label
            {
                Text      = "Panel de Control",
                Font      = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.FromArgb(80, 30, 55),
                AutoSize  = true,
                Location  = new Point(16, 14)
            };

            // Botón Actualizar
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
            _btnRefrescar.Click += (s, e) => ActualizarMetricas();
            _btnRefrescar.Location = new Point(this.ClientSize.Width - _btnRefrescar.Width - 12, 12);

            // TableLayoutPanel 4 columnas — se estira con la ventana
            _tblCards = new TableLayoutPanel
            {
                ColumnCount  = 4,
                RowCount     = 1,
                Location     = new Point(12, 52),
                Height       = 168,
                Anchor       = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor    = Color.Transparent,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                Padding      = new Padding(0)
            };
            _tblCards.Width = this.ClientSize.Width - 24;
            _tblCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            _tblCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            _tblCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            _tblCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            _tblCards.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            // Tarjetas
            _tblCards.Controls.Add(CrearTarjeta(
                Color.FromArgb(238, 228, 248), Color.FromArgb(70, 20, 100),
                out _numPrendas, out _txtPrendas), 0, 0);

            _tblCards.Controls.Add(CrearTarjeta(
                Color.FromArgb(220, 238, 255), Color.FromArgb(15, 55, 115),
                out _numClientes, out _txtClientes), 1, 0);

            _tblCards.Controls.Add(CrearTarjeta(
                Color.FromArgb(255, 235, 215), Color.FromArgb(130, 60, 10),
                out _numPedidos, out _txtPedidos), 2, 0);

            _tblCards.Controls.Add(CrearTarjeta(
                Color.FromArgb(215, 240, 220), Color.FromArgb(15, 85, 35),
                out _numBackup, out _txtBackup), 3, 0);

            // Separador inferior
            var sep = new Panel
            {
                BackColor = Color.FromArgb(220, 215, 225),
                Height    = 1,
                Anchor    = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Left      = 0,
                Top       = this.ClientSize.Height - 38
            };
            sep.Width = this.ClientSize.Width;

            // Label de sesión
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
            this.Controls.Add(_tblCards);
            this.Controls.Add(sep);
            this.Controls.Add(_lblSesion);

            // Ajustar ancho del table al redimensionar
            this.Resize += (s, e) =>
            {
                _tblCards.Width  = this.ClientSize.Width - 24;
                sep.Width        = this.ClientSize.Width;
                sep.Top          = this.ClientSize.Height - 38;
                _lblSesion.Width = this.ClientSize.Width - 32;
                _lblSesion.Top   = this.ClientSize.Height - 30;
                _btnRefrescar.Left = this.ClientSize.Width - _btnRefrescar.Width - 12;
            };
        }

        // Crea una tarjeta de métrica con fondo de color, número grande y etiqueta.
        private static Panel CrearTarjeta(Color fondo, Color tinta,
            out Label lblNum, out Label lblTxt)
        {
            var card = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = fondo,
                Margin    = new Padding(4)
            };

            // Esquinas redondeadas dibujadas en Paint
            card.Paint += (s, pe) =>
            {
                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RoundedRect(new Rectangle(0, 0, card.Width - 1, card.Height - 1), 10))
                using (var br   = new SolidBrush(fondo))
                    pe.Graphics.FillPath(br, path);
            };

            var num = new Label
            {
                Text      = "…",
                Font      = new Font("Segoe UI", 30f, FontStyle.Bold),
                ForeColor = tinta,
                Dock      = DockStyle.None,
                AutoSize  = false,
                TextAlign = ContentAlignment.BottomCenter,
                BackColor = Color.Transparent,
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Location  = new Point(0, 18),
                Height    = 72
            };

            var txt = new Label
            {
                Text      = "",
                Font      = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(Math.Min(tinta.R + 40, 255),
                                           Math.Min(tinta.G + 40, 255),
                                           Math.Min(tinta.B + 40, 255)),
                Dock      = DockStyle.None,
                AutoSize  = false,
                TextAlign = ContentAlignment.TopCenter,
                BackColor = Color.Transparent,
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Location  = new Point(0, 94),
                Height    = 44
            };

            // Ajustar anchos cuando el card se redimensiona
            card.Resize += (s, e) =>
            {
                num.Width = card.Width;
                txt.Width = card.Width;
            };

            card.Controls.Add(num);
            card.Controls.Add(txt);

            lblNum = num;
            lblTxt = txt;
            return card;
        }

        private static GraphicsPath RoundedRect(Rectangle b, int r)
        {
            int d    = r * 2;
            var path = new GraphicsPath();
            path.AddArc(b.X,           b.Y,            d, d, 180, 90);
            path.AddArc(b.Right - d,   b.Y,            d, d, 270, 90);
            path.AddArc(b.Right - d,   b.Bottom - d,   d, d, 0,   90);
            path.AddArc(b.X,           b.Bottom - d,   d, d, 90,  90);
            path.CloseFigure();
            return path;
        }
    }
}
