namespace GUI
{
    partial class frmLogin
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlHeader      = new System.Windows.Forms.Panel();
            this.lblBrand       = new System.Windows.Forms.Label();
            this.lblSubtitle    = new System.Windows.Forms.Label();
            this.pnlCard        = new System.Windows.Forms.Panel();
            this.lblDocLabel    = new System.Windows.Forms.Label();
            this.txtDocumento   = new System.Windows.Forms.TextBox();
            this.lblPassLabel   = new System.Windows.Forms.Label();
            this.txtPassword    = new System.Windows.Forms.TextBox();
            this.btnIngresar    = new System.Windows.Forms.Button();
            this.btnSalir       = new System.Windows.Forms.Button();
            this.pnlCard.SuspendLayout();
            this.pnlHeader.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlHeader
            //
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(180, 35, 75);
            this.pnlHeader.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Height    = 100;
            this.pnlHeader.Name      = "pnlHeader";
            this.pnlHeader.Controls.Add(this.lblBrand);
            this.pnlHeader.Controls.Add(this.lblSubtitle);
            //
            // lblBrand
            //
            this.lblBrand.AutoSize  = true;
            this.lblBrand.Font      = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblBrand.ForeColor = System.Drawing.Color.White;
            this.lblBrand.Location  = new System.Drawing.Point(20, 16);
            this.lblBrand.Name      = "lblBrand";
            this.lblBrand.Text      = "WardrobeFlow";
            //
            // lblSubtitle
            //
            this.lblSubtitle.AutoSize  = true;
            this.lblSubtitle.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(255, 200, 220);
            this.lblSubtitle.Location  = new System.Drawing.Point(22, 65);
            this.lblSubtitle.Name      = "lblSubtitle";
            this.lblSubtitle.Text      = "Sistema de gestion de guardarropa";
            //
            // pnlCard
            //
            this.pnlCard.BackColor = System.Drawing.Color.White;
            this.pnlCard.Location  = new System.Drawing.Point(28, 118);
            this.pnlCard.Name      = "pnlCard";
            this.pnlCard.Size      = new System.Drawing.Size(414, 248);
            this.pnlCard.Controls.Add(this.lblDocLabel);
            this.pnlCard.Controls.Add(this.txtDocumento);
            this.pnlCard.Controls.Add(this.lblPassLabel);
            this.pnlCard.Controls.Add(this.txtPassword);
            this.pnlCard.Controls.Add(this.btnIngresar);
            this.pnlCard.Controls.Add(this.btnSalir);
            //
            // lblDocLabel
            //
            this.lblDocLabel.AutoSize  = true;
            this.lblDocLabel.Font      = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblDocLabel.ForeColor = System.Drawing.Color.FromArgb(150, 80, 105);
            this.lblDocLabel.Location  = new System.Drawing.Point(22, 22);
            this.lblDocLabel.Name      = "lblDocLabel";
            this.lblDocLabel.Text      = "DOCUMENTO";
            //
            // txtDocumento
            //
            this.txtDocumento.BackColor    = System.Drawing.Color.FromArgb(255, 240, 248);
            this.txtDocumento.BorderStyle  = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtDocumento.Font         = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtDocumento.ForeColor    = System.Drawing.Color.FromArgb(35, 20, 30);
            this.txtDocumento.Location     = new System.Drawing.Point(22, 44);
            this.txtDocumento.Name         = "txtDocumento";
            this.txtDocumento.Size         = new System.Drawing.Size(370, 32);
            this.txtDocumento.TabIndex     = 0;
            this.txtDocumento.Text         = "admin";
            //
            // lblPassLabel
            //
            this.lblPassLabel.AutoSize  = true;
            this.lblPassLabel.Font      = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblPassLabel.ForeColor = System.Drawing.Color.FromArgb(150, 80, 105);
            this.lblPassLabel.Location  = new System.Drawing.Point(22, 92);
            this.lblPassLabel.Name      = "lblPassLabel";
            this.lblPassLabel.Text      = "CONTRASENA";
            //
            // txtPassword
            //
            this.txtPassword.BackColor    = System.Drawing.Color.FromArgb(255, 240, 248);
            this.txtPassword.BorderStyle  = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPassword.Font         = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtPassword.ForeColor    = System.Drawing.Color.FromArgb(35, 20, 30);
            this.txtPassword.Location     = new System.Drawing.Point(22, 114);
            this.txtPassword.Name         = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size         = new System.Drawing.Size(370, 32);
            this.txtPassword.TabIndex     = 1;
            this.txtPassword.Text         = "1234";
            //
            // btnIngresar
            //
            this.btnIngresar.BackColor    = System.Drawing.Color.FromArgb(180, 35, 75);
            this.btnIngresar.ForeColor    = System.Drawing.Color.White;
            this.btnIngresar.FlatStyle    = System.Windows.Forms.FlatStyle.Flat;
            this.btnIngresar.FlatAppearance.BorderSize = 0;
            this.btnIngresar.Font         = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnIngresar.Location     = new System.Drawing.Point(22, 168);
            this.btnIngresar.Name         = "btnIngresar";
            this.btnIngresar.Size         = new System.Drawing.Size(180, 40);
            this.btnIngresar.TabIndex     = 2;
            this.btnIngresar.Text         = "INGRESAR";
            this.btnIngresar.Cursor       = System.Windows.Forms.Cursors.Hand;
            this.btnIngresar.Click       += new System.EventHandler(this.btnIngresar_Click);
            //
            // btnSalir
            //
            this.btnSalir.BackColor    = System.Drawing.Color.White;
            this.btnSalir.ForeColor    = System.Drawing.Color.FromArgb(180, 35, 75);
            this.btnSalir.FlatStyle    = System.Windows.Forms.FlatStyle.Flat;
            this.btnSalir.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(180, 35, 75);
            this.btnSalir.FlatAppearance.BorderSize  = 1;
            this.btnSalir.Font         = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnSalir.Location     = new System.Drawing.Point(212, 168);
            this.btnSalir.Name         = "btnSalir";
            this.btnSalir.Size         = new System.Drawing.Size(180, 40);
            this.btnSalir.TabIndex     = 3;
            this.btnSalir.Text         = "Salir";
            this.btnSalir.Cursor       = System.Windows.Forms.Cursors.Hand;
            this.btnSalir.Click       += new System.EventHandler(this.btnSalir_Click);
            //
            // frmLogin
            //
            this.AcceptButton          = this.btnIngresar;
            this.AutoScaleDimensions   = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode         = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor             = System.Drawing.Color.FromArgb(255, 240, 248);
            this.ClientSize            = new System.Drawing.Size(470, 390);
            this.ControlBox            = false;
            this.Controls.Add(this.pnlCard);
            this.Controls.Add(this.pnlHeader);
            this.Font                  = new System.Drawing.Font("Segoe UI", 9.5F);
            this.Name                  = "frmLogin";
            this.StartPosition         = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text                  = "WardrobeFlow - Iniciar Sesion";
            this.pnlCard.ResumeLayout(false);
            this.pnlCard.PerformLayout();
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel    pnlHeader;
        private System.Windows.Forms.Panel    pnlCard;
        private System.Windows.Forms.Label    lblBrand;
        private System.Windows.Forms.Label    lblSubtitle;
        private System.Windows.Forms.Label    lblDocLabel;
        private System.Windows.Forms.Label    lblPassLabel;
        private System.Windows.Forms.TextBox  txtDocumento;
        private System.Windows.Forms.TextBox  txtPassword;
        private System.Windows.Forms.Button   btnIngresar;
        private System.Windows.Forms.Button   btnSalir;
    }
}
