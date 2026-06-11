using System;
using System.Drawing;
using System.Windows.Forms;

namespace GUI
{
    /// <summary>
    /// Diálogo simple de entrada de texto (con opción de máscara para contraseñas), reutilizable
    /// sin dependencias del Designer. Devuelve el texto en <see cref="InputText"/> y DialogResult.OK
    /// si el usuario confirma. (Equivalente al InputDialog de Stach.)
    /// </summary>
    public class InputDialog : Form
    {
        private readonly TextBox _txt;

        public string InputText => _txt.Text;

        public InputDialog(string titulo, string prompt, bool esPassword)
        {
            this.Text            = titulo;
            this.ClientSize      = new Size(420, 150);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.BackColor       = Color.White;
            this.Font            = new Font("Segoe UI", 9f);
            try { string ico = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico"); if (System.IO.File.Exists(ico)) this.Icon = new Icon(ico); } catch { }

            var lbl = new Label { Text = prompt, Location = new Point(16, 16), Size = new Size(388, 40), AutoSize = false };

            _txt = new TextBox { Location = new Point(18, 60), Size = new Size(384, 24) };
            if (esPassword) _txt.UseSystemPasswordChar = true;

            var ok = new Button
            {
                Text = "Aceptar", DialogResult = DialogResult.OK,
                Location = new Point(228, 100), Size = new Size(84, 30),
                FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(176, 62, 96),
                ForeColor = Color.White, Cursor = Cursors.Hand
            };
            ok.FlatAppearance.BorderSize = 0;

            var cancel = new Button
            {
                Text = "Cancelar", DialogResult = DialogResult.Cancel,
                Location = new Point(318, 100), Size = new Size(84, 30),
                FlatStyle = FlatStyle.Flat, BackColor = Color.White,
                ForeColor = Color.FromArgb(90, 90, 100), Cursor = Cursors.Hand
            };
            cancel.FlatAppearance.BorderColor = Color.FromArgb(210, 180, 195);

            this.Controls.AddRange(new Control[] { lbl, _txt, ok, cancel });
            this.AcceptButton = ok;
            this.CancelButton  = cancel;
        }
    }
}
