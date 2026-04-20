using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GUI
{
    /// <summary>
    /// Diálogo que muestra la notificación de despacho que se enviaría al cliente.
    /// Presenta un resumen del pedido en formato de mensaje, listo para copiar
    /// o para conectar a un servicio SMTP en una versión futura.
    /// </summary>
    public partial class NotificacionDespachoForm : Form
    {
        private readonly BE.Pedido _pedido;

        public NotificacionDespachoForm(BE.Pedido pedido)
        {
            InitializeComponent();
            _pedido = pedido;

            this.Text            = $"Notificación — Pedido #{pedido.IdPedido}";
            this.ClientSize      = new Size(560, 500);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.StartPosition   = FormStartPosition.CenterParent;

            ConstruirInterfaz();
        }

        private void ConstruirInterfaz()
        {
            // ── Header con estado ─────────────────────────────────────────────
            bool entregado = _pedido.Estado == BE.EstadoPedido.Entregado;
            Color headerColor = entregado
                ? Color.FromArgb(40, 140, 60)
                : Color.FromArgb(30, 110, 180);

            var panelHeader = new Panel
            {
                Dock = DockStyle.Top, Height = 48,
                BackColor = headerColor
            };
            panelHeader.Controls.Add(new Label
            {
                Text = entregado
                    ? $"✓  Pedido #{_pedido.IdPedido} — ENTREGADO"
                    : $"📦  Pedido #{_pedido.IdPedido} — DESPACHADO",
                Dock = DockStyle.Fill,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Padding = new Padding(14, 10, 0, 0)
            });

            // ── Cuerpo: texto del mensaje ─────────────────────────────────────
            var txtMensaje = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Color.White,
                Font = new Font("Consolas", 9.5f),
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                Padding = new Padding(10)
            };
            txtMensaje.Text = GenerarMensaje();

            // ── Botones ───────────────────────────────────────────────────────
            var panelBottom = new Panel
            {
                Dock = DockStyle.Bottom, Height = 48,
                BackColor = Color.FromArgb(240, 240, 245),
                Padding = new Padding(10, 8, 10, 8)
            };

            var btnCopiar = new Button
            {
                Text = "Copiar al portapapeles",
                Left = 10, Top = 8, Width = 180, Height = 30,
                FlatStyle = FlatStyle.Flat
            };
            btnCopiar.Click += (s, e) =>
            {
                Clipboard.SetText(txtMensaje.Text);
                btnCopiar.Text = "✓ Copiado";
                btnCopiar.BackColor = Color.FromArgb(40, 140, 60);
                btnCopiar.ForeColor = Color.White;
            };

            var btnCerrar = new Button
            {
                Text = "Cerrar",
                Left = 198, Top = 8, Width = 80, Height = 30,
                FlatStyle = FlatStyle.Flat
            };
            btnCerrar.Click += (s, e) => this.Close();

            var lblNota = new Label
            {
                Text = "ℹ Este mensaje puede enviarse por email o WhatsApp al cliente.",
                Left = 290, Top = 14, Width = 260,
                ForeColor = Color.DimGray,
                Font = new Font("Segoe UI", 8f)
            };

            panelBottom.Controls.AddRange(new Control[] { btnCopiar, btnCerrar, lblNota });

            this.Controls.Add(txtMensaje);
            this.Controls.Add(panelBottom);
            this.Controls.Add(panelHeader);
        }

        private string GenerarMensaje()
        {
            bool entregado = _pedido.Estado == BE.EstadoPedido.Entregado;
            var sb = new StringBuilder();

            sb.AppendLine("══════════════════════════════════════════════");
            sb.AppendLine("         WARDROBEFLOW — NOTIFICACIÓN");
            sb.AppendLine("══════════════════════════════════════════════");
            sb.AppendLine();
            sb.AppendLine($"Estimado/a {_pedido.NombreCliente},");
            sb.AppendLine();

            if (entregado)
            {
                sb.AppendLine("Queremos confirmarle que su pedido fue entregado.");
                sb.AppendLine("Gracias por elegir WardrobeFlow.");
            }
            else
            {
                sb.AppendLine("Le informamos que su pedido ha sido despachado y");
                sb.AppendLine("estará llegando a su domicilio en breve.");
            }

            sb.AppendLine();
            sb.AppendLine("──────────────────────────────────────────────");
            sb.AppendLine($"  Número de pedido : #{_pedido.IdPedido}");
            sb.AppendLine($"  Fecha de pedido  : {_pedido.FechaPedido:dd/MM/yyyy HH:mm}");

            if (_pedido.FechaDespacho.HasValue)
                sb.AppendLine($"  Fecha despacho   : {_pedido.FechaDespacho.Value:dd/MM/yyyy HH:mm}");

            if (_pedido.FechaEntrega.HasValue)
                sb.AppendLine($"  Fecha entrega    : {_pedido.FechaEntrega.Value:dd/MM/yyyy HH:mm}");

            sb.AppendLine($"  Estado           : {_pedido.Estado}");
            sb.AppendLine("──────────────────────────────────────────────");
            sb.AppendLine();
            sb.AppendLine($"  Prendas incluidas ({_pedido.Prendas.Count}):");
            sb.AppendLine();

            int i = 1;
            foreach (var p in _pedido.Prendas)
            {
                sb.AppendLine($"    {i++}. {p.Nombre}");
                sb.AppendLine($"       Talle: {p.Talle ?? "—"}   Color: {p.Color ?? "—"}");
                sb.AppendLine($"       Categoría: {p.Categoria ?? "—"}");
                sb.AppendLine();
            }

            sb.AppendLine("──────────────────────────────────────────────");
            sb.AppendLine();
            sb.AppendLine("Ante cualquier consulta, comuníquese con nosotros.");
            sb.AppendLine();
            sb.AppendLine("WardrobeFlow — Tu guardarropa, sin límites.");
            sb.AppendLine("══════════════════════════════════════════════");

            return sb.ToString();
        }
    }
}
