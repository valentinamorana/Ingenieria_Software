using System;
using System.Drawing;
using System.Windows.Forms;

namespace GUI
{
    /// <summary>
    /// Diálogo modal para ingresar una nueva contraseña al resetear credenciales.
    ///
    /// Muestra el nombre de usuario objetivo, solicita la nueva contraseña dos veces
    /// (nueva + confirmación) y expone la clave validada en la propiedad NuevaClave.
    ///
    /// Uso:
    ///   using (var dialog = new ResetClaveDialog(username))
    ///   {
    ///       if (dialog.ShowDialog(this) == DialogResult.OK)
    ///           usuarioBLL.ResetearClave(this, idUsuario, dialog.NuevaClave);
    ///   }
    /// </summary>
    public class ResetClaveDialog : Form
    {
        // ── Controles ─────────────────────────────────────────────────────────
        private TextBox txtNuevaClave;
        private TextBox txtConfirmar;
        private Button  btnAceptar;
        private Button  btnCancelar;
        private Label   lblError;

        /// <summary>
        /// Nueva contraseña validada (disponible solo cuando DialogResult == OK).
        /// </summary>
        public string NuevaClave { get; private set; }

        /// <summary>
        /// Construye el diálogo para el usuario especificado.
        /// </summary>
        /// <param name="username">Nombre del usuario cuya contraseña se va a resetear.</param>
        public ResetClaveDialog(string username)
        {
            // ── Propiedades del formulario ────────────────────────────────────
            this.Text            = "Resetear Contraseña";
            this.ClientSize      = new Size(340, 260);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.BackColor       = Color.White;

            // ── Controles ─────────────────────────────────────────────────────
            var lblTitulo = new Label
            {
                Text      = "Resetear Contraseña",
                Font      = new Font("Segoe UI", 12, FontStyle.Bold),
                Left      = 20, Top    = 18,
                Width     = 300, Height = 24,
                ForeColor = Color.FromArgb(30, 30, 60)
            };

            // Muestra el nombre del usuario afectado para evitar confusiones
            var lblUsuario = new Label
            {
                Text      = $"Usuario: {username}",
                Left      = 20, Top    = 50,
                Width     = 300, Height = 20,
                ForeColor = Color.DimGray,
                Font      = new Font("Segoe UI", 9, FontStyle.Italic)
            };

            var lblNueva = new Label
            {
                Text  = "Nueva contraseña (mín. 6 caracteres):",
                Left  = 20, Top   = 82,
                Width = 300, Height = 18
            };

            txtNuevaClave = new TextBox
            {
                Left         = 20,  Top    = 102,
                Width        = 300, Height = 24,
                PasswordChar = '●'
            };

            var lblConfirmar = new Label
            {
                Text  = "Confirmar contraseña:",
                Left  = 20, Top   = 136,
                Width = 300, Height = 18
            };

            txtConfirmar = new TextBox
            {
                Left         = 20,  Top    = 156,
                Width        = 300, Height = 24,
                PasswordChar = '●'
            };

            // Label de error (oculto hasta que falle la validación)
            lblError = new Label
            {
                Left      = 20,  Top    = 186,
                Width     = 300, Height = 18,
                ForeColor = Color.Crimson,
                Font      = new Font("Segoe UI", 8.5f),
                Text      = string.Empty
            };

            btnAceptar = new Button
            {
                Text      = "Confirmar Reset",
                Left      = 20,  Top    = 214,
                Width     = 145, Height = 32,
                BackColor = Color.FromArgb(180, 100, 30),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.None   // lo manejamos manualmente
            };
            btnAceptar.FlatAppearance.BorderSize = 0;
            btnAceptar.Click += BtnAceptar_Click;

            btnCancelar = new Button
            {
                Text         = "Cancelar",
                Left         = 175, Top    = 214,
                Width        = 145, Height = 32,
                DialogResult = DialogResult.Cancel
            };

            // Enter activa el botón Confirmar
            this.AcceptButton = btnAceptar;
            this.CancelButton = btnCancelar;

            this.Controls.AddRange(new Control[]
            {
                lblTitulo, lblUsuario,
                lblNueva, txtNuevaClave,
                lblConfirmar, txtConfirmar,
                lblError, btnAceptar, btnCancelar
            });
        }

        /// <summary>
        /// Valida las contraseñas ingresadas y, si son válidas, cierra el diálogo con OK.
        /// </summary>
        private void BtnAceptar_Click(object sender, EventArgs e)
        {
            lblError.Text = string.Empty;

            string nueva     = txtNuevaClave.Text;
            string confirmar = txtConfirmar.Text;

            // Validar longitud mínima
            if (string.IsNullOrWhiteSpace(nueva) || nueva.Length < 6)
            {
                lblError.Text = "La contraseña debe tener al menos 6 caracteres.";
                txtNuevaClave.Focus();
                return;
            }

            // Validar que ambas coincidan
            if (nueva != confirmar)
            {
                lblError.Text = "Las contraseñas no coinciden.";
                txtConfirmar.Clear();
                txtConfirmar.Focus();
                return;
            }

            // Todo OK: exponer el valor y cerrar el diálogo
            NuevaClave         = nueva;
            this.DialogResult  = DialogResult.OK;
            this.Close();
        }
    }
}
