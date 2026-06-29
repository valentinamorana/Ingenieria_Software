namespace GUI
{
    partial class RestauracionForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlHeader         = new System.Windows.Forms.Panel();
            this.lblSubtitulo      = new System.Windows.Forms.Label();
            this.lblTitulo         = new System.Windows.Forms.Label();
            this.pnlBody           = new System.Windows.Forms.Panel();
            this.lblDetalle        = new System.Windows.Forms.Label();
            this.txtDetalle        = new System.Windows.Forms.TextBox();
            this.pnlBotones        = new System.Windows.Forms.Panel();
            this.btnSalir          = new System.Windows.Forms.Button();
            this.btnRecuperacion   = new System.Windows.Forms.Button();
            this.btnRestaurarBackup = new System.Windows.Forms.Button();
            this.btnRecalcular     = new System.Windows.Forms.Button();
            this.pnlHeader.SuspendLayout();
            this.pnlBody.SuspendLayout();
            this.pnlBotones.SuspendLayout();
            this.SuspendLayout();

            // pnlHeader
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(120, 40, 70);
            this.pnlHeader.Controls.Add(this.lblSubtitulo);
            this.pnlHeader.Controls.Add(this.lblTitulo);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Height = 104;
            this.pnlHeader.Padding = new System.Windows.Forms.Padding(20, 12, 20, 10);

            // lblTitulo
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 13f, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.White;
            this.lblTitulo.Location = new System.Drawing.Point(20, 12);
            this.lblTitulo.Text = "Integridad del Sistema Comprometida";

            // lblSubtitulo — tamaño fijo (multilínea) para que NO se corte; va debajo del combo.
            this.lblSubtitulo.AutoSize = false;
            this.lblSubtitulo.Font = new System.Drawing.Font("Segoe UI", 9f);
            this.lblSubtitulo.ForeColor = System.Drawing.Color.FromArgb(220, 190, 205);
            this.lblSubtitulo.Location = new System.Drawing.Point(21, 42);
            this.lblSubtitulo.Size = new System.Drawing.Size(478, 52);
            this.lblSubtitulo.Text = "Se detectaron discrepancias en los dígitos verificadores. El acceso está bloqueado.";

            // pnlBody
            this.pnlBody.BackColor = System.Drawing.Color.FromArgb(252, 250, 252);
            this.pnlBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBody.Padding = new System.Windows.Forms.Padding(20, 14, 20, 0);
            this.pnlBody.Controls.Add(this.txtDetalle);
            this.pnlBody.Controls.Add(this.lblDetalle);

            // lblDetalle
            this.lblDetalle.AutoSize = true;
            this.lblDetalle.Font = new System.Drawing.Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Bold);
            this.lblDetalle.ForeColor = System.Drawing.Color.FromArgb(80, 40, 60);
            this.lblDetalle.Location = new System.Drawing.Point(20, 14);
            this.lblDetalle.Text = "Detalle del error:";

            // txtDetalle
            this.txtDetalle.BackColor = System.Drawing.Color.FromArgb(248, 244, 250);
            this.txtDetalle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtDetalle.Font = new System.Drawing.Font("Consolas", 8.5f);
            this.txtDetalle.ForeColor = System.Drawing.Color.FromArgb(60, 30, 50);
            this.txtDetalle.Location = new System.Drawing.Point(20, 36);
            this.txtDetalle.Multiline = true;
            this.txtDetalle.ReadOnly = true;
            this.txtDetalle.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDetalle.Size = new System.Drawing.Size(480, 220);
            this.txtDetalle.TabStop = false;

            // pnlBotones
            this.pnlBotones.BackColor = System.Drawing.Color.FromArgb(245, 240, 248);
            this.pnlBotones.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBotones.Height = 62;
            this.pnlBotones.Padding = new System.Windows.Forms.Padding(16, 12, 16, 12);
            this.pnlBotones.Controls.Add(this.btnSalir);
            this.pnlBotones.Controls.Add(this.btnRecuperacion);
            this.pnlBotones.Controls.Add(this.btnRestaurarBackup);
            this.pnlBotones.Controls.Add(this.btnRecalcular);

            // btnRecalcular
            this.btnRecalcular.BackColor = System.Drawing.Color.FromArgb(176, 62, 96);
            this.btnRecalcular.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRecalcular.FlatAppearance.BorderSize = 0;
            this.btnRecalcular.Font = new System.Drawing.Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Bold);
            this.btnRecalcular.ForeColor = System.Drawing.Color.White;
            this.btnRecalcular.Location = new System.Drawing.Point(16, 12);
            this.btnRecalcular.Size = new System.Drawing.Size(176, 36);
            this.btnRecalcular.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnRecalcular.Text = "Recalcular Dígitos";
            this.btnRecalcular.Click += new System.EventHandler(this.btnRecalcular_Click);

            // btnRestaurarBackup
            this.btnRestaurarBackup.BackColor = System.Drawing.Color.FromArgb(252, 228, 235);
            this.btnRestaurarBackup.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRestaurarBackup.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(210, 100, 135);
            this.btnRestaurarBackup.FlatAppearance.BorderSize = 1;
            this.btnRestaurarBackup.Font = new System.Drawing.Font("Segoe UI", 8.5f);
            this.btnRestaurarBackup.ForeColor = System.Drawing.Color.FromArgb(100, 40, 80);
            this.btnRestaurarBackup.Location = new System.Drawing.Point(200, 12);
            this.btnRestaurarBackup.Size = new System.Drawing.Size(168, 36);
            this.btnRestaurarBackup.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnRestaurarBackup.Text = "Restaurar desde Backup";
            this.btnRestaurarBackup.Click += new System.EventHandler(this.btnRestaurarBackup_Click);

            // btnRecuperacion — abre la consola de recuperación asistida por el espejo de integridad.
            this.btnRecuperacion.BackColor = System.Drawing.Color.FromArgb(80, 150, 90);
            this.btnRecuperacion.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRecuperacion.FlatAppearance.BorderSize = 0;
            this.btnRecuperacion.Font = new System.Drawing.Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Bold);
            this.btnRecuperacion.ForeColor = System.Drawing.Color.White;
            this.btnRecuperacion.Location = new System.Drawing.Point(376, 12);
            this.btnRecuperacion.Size = new System.Drawing.Size(184, 36);
            this.btnRecuperacion.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnRecuperacion.Text = "Recuperación Asistida...";
            this.btnRecuperacion.Click += new System.EventHandler(this.btnRecuperacion_Click);

            // btnSalir
            this.btnSalir.BackColor = System.Drawing.Color.FromArgb(220, 215, 225);
            this.btnSalir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSalir.FlatAppearance.BorderSize = 0;
            this.btnSalir.Font = new System.Drawing.Font("Segoe UI", 8.5f);
            this.btnSalir.ForeColor = System.Drawing.Color.FromArgb(80, 70, 80);
            this.btnSalir.Location = new System.Drawing.Point(568, 12);
            this.btnSalir.Size = new System.Drawing.Size(80, 36);
            this.btnSalir.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnSalir.Text = "Salir";
            this.btnSalir.Click += new System.EventHandler(this.btnSalir_Click);

            // RestauracionForm
            this.BackColor = System.Drawing.Color.FromArgb(252, 250, 252);
            this.ClientSize = new System.Drawing.Size(664, 398);
            this.Controls.Add(this.pnlBody);
            this.Controls.Add(this.pnlBotones);
            this.Controls.Add(this.pnlHeader);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Integridad del Sistema";
            this.Load += new System.EventHandler(this.RestauracionForm_Load);

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
        private System.Windows.Forms.Label    lblDetalle;
        private System.Windows.Forms.TextBox  txtDetalle;
        private System.Windows.Forms.Panel    pnlBotones;
        private System.Windows.Forms.Button   btnRecalcular;
        private System.Windows.Forms.Button   btnRestaurarBackup;
        private System.Windows.Forms.Button   btnRecuperacion;
        private System.Windows.Forms.Button   btnSalir;
    }
}
