namespace GUI
{
    partial class ConfirmarAdminForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlHeader   = new System.Windows.Forms.Panel();
            this.lblTitulo   = new System.Windows.Forms.Label();
            this.lblSubtitulo = new System.Windows.Forms.Label();
            this.pnlBody     = new System.Windows.Forms.Panel();
            this.lblUsuario  = new System.Windows.Forms.Label();
            this.txtUsuario  = new System.Windows.Forms.TextBox();
            this.lblClave    = new System.Windows.Forms.Label();
            this.txtClave    = new System.Windows.Forms.TextBox();
            this.lblError    = new System.Windows.Forms.Label();
            this.pnlBotones  = new System.Windows.Forms.Panel();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.btnConfirmar = new System.Windows.Forms.Button();
            this.pnlHeader.SuspendLayout();
            this.pnlBody.SuspendLayout();
            this.pnlBotones.SuspendLayout();
            this.SuspendLayout();

            // pnlHeader
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(176, 62, 96);
            this.pnlHeader.Controls.Add(this.lblSubtitulo);
            this.pnlHeader.Controls.Add(this.lblTitulo);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Height = 64;
            this.pnlHeader.Padding = new System.Windows.Forms.Padding(16, 10, 16, 8);

            // lblTitulo
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 11f, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.White;
            this.lblTitulo.Location = new System.Drawing.Point(16, 10);
            this.lblTitulo.Tag = "frm.confirmaradmin";
            this.lblTitulo.Text = "Confirmar Administrador";

            // lblSubtitulo
            this.lblSubtitulo.AutoSize = true;
            this.lblSubtitulo.Font = new System.Drawing.Font("Segoe UI", 8f);
            this.lblSubtitulo.ForeColor = System.Drawing.Color.FromArgb(220, 200, 210);
            this.lblSubtitulo.Location = new System.Drawing.Point(17, 36);
            this.lblSubtitulo.Tag = "lbl.confirmaradmin.subtitulo";
            this.lblSubtitulo.Text = "Solo un Administrador puede continuar";

            // pnlBody
            this.pnlBody.BackColor = System.Drawing.Color.FromArgb(252, 250, 252);
            this.pnlBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBody.Padding = new System.Windows.Forms.Padding(20, 14, 20, 0);
            this.pnlBody.Controls.Add(this.lblError);
            this.pnlBody.Controls.Add(this.txtClave);
            this.pnlBody.Controls.Add(this.lblClave);
            this.pnlBody.Controls.Add(this.txtUsuario);
            this.pnlBody.Controls.Add(this.lblUsuario);

            // lblUsuario
            this.lblUsuario.AutoSize = true;
            this.lblUsuario.Font = new System.Drawing.Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Bold);
            this.lblUsuario.ForeColor = System.Drawing.Color.FromArgb(80, 40, 60);
            this.lblUsuario.Location = new System.Drawing.Point(20, 18);
            this.lblUsuario.Tag = "lbl.confirmaradmin.usuario";
            this.lblUsuario.Text = "Usuario";

            // txtUsuario
            this.txtUsuario.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtUsuario.Font = new System.Drawing.Font("Segoe UI", 9.5f);
            this.txtUsuario.Location = new System.Drawing.Point(20, 36);
            this.txtUsuario.MaxLength = 100;
            this.txtUsuario.Size = new System.Drawing.Size(340, 26);

            // lblClave
            this.lblClave.AutoSize = true;
            this.lblClave.Font = new System.Drawing.Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Bold);
            this.lblClave.ForeColor = System.Drawing.Color.FromArgb(80, 40, 60);
            this.lblClave.Location = new System.Drawing.Point(20, 74);
            this.lblClave.Tag = "lbl.confirmaradmin.clave";
            this.lblClave.Text = "Contraseña";

            // txtClave
            this.txtClave.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtClave.Font = new System.Drawing.Font("Segoe UI", 9.5f);
            this.txtClave.Location = new System.Drawing.Point(20, 92);
            this.txtClave.MaxLength = 100;
            this.txtClave.PasswordChar = '●';
            this.txtClave.Size = new System.Drawing.Size(340, 26);

            // lblError
            this.lblError.AutoSize = false;
            this.lblError.Font = new System.Drawing.Font("Segoe UI", 8f);
            this.lblError.ForeColor = System.Drawing.Color.FromArgb(180, 40, 40);
            this.lblError.Location = new System.Drawing.Point(20, 128);
            this.lblError.Size = new System.Drawing.Size(340, 34);
            this.lblError.Text = string.Empty;

            // pnlBotones
            this.pnlBotones.BackColor = System.Drawing.Color.FromArgb(245, 240, 248);
            this.pnlBotones.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBotones.Height = 52;
            this.pnlBotones.Padding = new System.Windows.Forms.Padding(16, 10, 16, 10);
            this.pnlBotones.Controls.Add(this.btnCancelar);
            this.pnlBotones.Controls.Add(this.btnConfirmar);

            // btnConfirmar
            this.btnConfirmar.BackColor = System.Drawing.Color.FromArgb(176, 62, 96);
            this.btnConfirmar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConfirmar.FlatAppearance.BorderSize = 0;
            this.btnConfirmar.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
            this.btnConfirmar.ForeColor = System.Drawing.Color.White;
            this.btnConfirmar.Location = new System.Drawing.Point(204, 10);
            this.btnConfirmar.Size = new System.Drawing.Size(156, 32);
            this.btnConfirmar.Tag = "btn.confirmaradmin.ok";
            this.btnConfirmar.Text = "Confirmar";
            this.btnConfirmar.Click += new System.EventHandler(this.btnConfirmar_Click);

            // btnCancelar
            this.btnCancelar.BackColor = System.Drawing.Color.FromArgb(252, 228, 235);
            this.btnCancelar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancelar.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(210, 100, 135);
            this.btnCancelar.FlatAppearance.BorderSize = 1;
            this.btnCancelar.Font = new System.Drawing.Font("Segoe UI", 9f);
            this.btnCancelar.ForeColor = System.Drawing.Color.FromArgb(100, 40, 80);
            this.btnCancelar.Location = new System.Drawing.Point(40, 10);
            this.btnCancelar.Size = new System.Drawing.Size(120, 32);
            this.btnCancelar.Tag = "btn.confirmaradmin.cancelar";
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);

            // ConfirmarAdminForm
            this.AcceptButton = this.btnConfirmar;
            this.CancelButton = this.btnCancelar;
            this.BackColor = System.Drawing.Color.FromArgb(252, 250, 252);
            this.ClientSize = new System.Drawing.Size(380, 290);
            this.Tag = "frm.confirmaradmin";
            this.Controls.Add(this.pnlBody);
            this.Controls.Add(this.pnlBotones);
            this.Controls.Add(this.pnlHeader);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Confirmar Administrador";

            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.pnlBody.ResumeLayout(false);
            this.pnlBody.PerformLayout();
            this.pnlBotones.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel    pnlHeader;
        private System.Windows.Forms.Label    lblTitulo;
        private System.Windows.Forms.Label    lblSubtitulo;
        private System.Windows.Forms.Panel    pnlBody;
        private System.Windows.Forms.Label    lblUsuario;
        private System.Windows.Forms.TextBox  txtUsuario;
        private System.Windows.Forms.Label    lblClave;
        private System.Windows.Forms.TextBox  txtClave;
        private System.Windows.Forms.Label    lblError;
        private System.Windows.Forms.Panel    pnlBotones;
        private System.Windows.Forms.Button   btnConfirmar;
        private System.Windows.Forms.Button   btnCancelar;
    }
}
