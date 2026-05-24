namespace GUI
{
    partial class BackupForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlMain    = new System.Windows.Forms.Panel();
            this.lblTitulo  = new System.Windows.Forms.Label();
            this.btnCrear   = new System.Windows.Forms.Button();
            this.btnRestore = new System.Windows.Forms.Button();
            this.lblInfo    = new System.Windows.Forms.Label();
            this.pnlMain.SuspendLayout();
            this.SuspendLayout();

            // ── pnlMain ─────────────────────────────────────────────────────
            this.pnlMain.BackColor = System.Drawing.Color.FromArgb(252, 250, 252);
            this.pnlMain.Controls.Add(this.lblTitulo);
            this.pnlMain.Controls.Add(this.btnCrear);
            this.pnlMain.Controls.Add(this.btnRestore);
            this.pnlMain.Controls.Add(this.lblInfo);
            this.pnlMain.Dock     = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Padding  = new System.Windows.Forms.Padding(24);
            this.pnlMain.Name     = "pnlMain";
            this.pnlMain.TabIndex = 0;

            // ── lblTitulo ────────────────────────────────────────────────────
            this.lblTitulo.Font      = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.FromArgb(146, 62, 96);
            this.lblTitulo.Location  = new System.Drawing.Point(24, 22);
            this.lblTitulo.Name      = "lblTitulo";
            this.lblTitulo.Size      = new System.Drawing.Size(372, 28);
            this.lblTitulo.TabIndex  = 0;
            this.lblTitulo.Text      = "Backup y Restauración";

            // ── btnCrear ─────────────────────────────────────────────────────
            this.btnCrear.BackColor                        = System.Drawing.Color.FromArgb(146, 62, 96);
            this.btnCrear.FlatStyle                        = System.Windows.Forms.FlatStyle.Flat;
            this.btnCrear.FlatAppearance.BorderSize        = 0;
            this.btnCrear.FlatAppearance.MouseOverBackColor= System.Drawing.Color.FromArgb(120, 50, 78);
            this.btnCrear.Font      = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnCrear.ForeColor = System.Drawing.Color.White;
            this.btnCrear.Location  = new System.Drawing.Point(24, 68);
            this.btnCrear.Name      = "btnCrear";
            this.btnCrear.Size      = new System.Drawing.Size(372, 44);
            this.btnCrear.TabIndex  = 1;
            this.btnCrear.Text      = "Generar Copia de Seguridad (.bak)";
            this.btnCrear.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnCrear.Click    += new System.EventHandler(this.btnCrear_Click);

            // ── btnRestore ───────────────────────────────────────────────────
            this.btnRestore.BackColor                        = System.Drawing.Color.FromArgb(238, 228, 248);
            this.btnRestore.FlatStyle                        = System.Windows.Forms.FlatStyle.Flat;
            this.btnRestore.FlatAppearance.BorderSize        = 1;
            this.btnRestore.FlatAppearance.BorderColor       = System.Drawing.Color.FromArgb(180, 140, 180);
            this.btnRestore.FlatAppearance.MouseOverBackColor= System.Drawing.Color.FromArgb(220, 208, 235);
            this.btnRestore.Font      = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnRestore.ForeColor = System.Drawing.Color.FromArgb(100, 40, 80);
            this.btnRestore.Location  = new System.Drawing.Point(24, 128);
            this.btnRestore.Name      = "btnRestore";
            this.btnRestore.Size      = new System.Drawing.Size(372, 44);
            this.btnRestore.TabIndex  = 2;
            this.btnRestore.Text      = "Restaurar Copia de Seguridad (.bak)";
            this.btnRestore.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnRestore.Click    += new System.EventHandler(this.btnRestore_Click);

            // ── lblInfo ──────────────────────────────────────────────────────
            this.lblInfo.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblInfo.ForeColor = System.Drawing.Color.FromArgb(120, 90, 110);
            this.lblInfo.Location  = new System.Drawing.Point(24, 186);
            this.lblInfo.Name      = "lblInfo";
            this.lblInfo.Size      = new System.Drawing.Size(372, 50);
            this.lblInfo.TabIndex  = 3;
            this.lblInfo.Text      = "Nota: la restauración cierra las conexiones activas y reinicia la aplicación.";
            this.lblInfo.TextAlign = System.Drawing.ContentAlignment.TopLeft;

            // ── BackupForm ───────────────────────────────────────────────────
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor           = System.Drawing.Color.FromArgb(252, 250, 252);
            this.ClientSize          = new System.Drawing.Size(420, 256);
            this.Controls.Add(this.pnlMain);
            this.FormBorderStyle     = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox         = false;
            this.MinimizeBox         = false;
            this.Name                = "BackupForm";
            this.StartPosition       = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text                = "Backup y Restauración";
            this.pnlMain.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel  pnlMain;
        private System.Windows.Forms.Label  lblTitulo;
        private System.Windows.Forms.Button btnCrear;
        private System.Windows.Forms.Button btnRestore;
        private System.Windows.Forms.Label  lblInfo;
    }
}
