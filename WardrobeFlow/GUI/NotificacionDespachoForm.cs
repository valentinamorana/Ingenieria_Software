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

            this.Text = $"Notificaci\u00f3n \u2014 Pedido #{pedido.IdPedido}";

            bool entregado = pedido.Estado == BE.EstadoPedido.Entregado;
            panelHeader.BackColor = entregado
                ? Color.FromArgb(40, 140, 60)
                : Color.FromArgb(30, 110, 180);
            lblHeader.Text = entregado
                ? $"\u2713  Pedido #{pedido.IdPedido} \u2014 ENTREGADO"
                : $"\ud83d\udce6  Pedido #{pedido.IdPedido} \u2014 DESPACHADO";

            txtMensaje.Text = GenerarMensaje();
        }

        private void BtnCopiar_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(txtMensaje.Text);
            btnCopiar.Text      = "\u2713 Copiado";
            btnCopiar.BackColor = Color.FromArgb(40, 140, 60);
            btnCopiar.ForeColor = Color.White;
        }

        private void BtnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
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
