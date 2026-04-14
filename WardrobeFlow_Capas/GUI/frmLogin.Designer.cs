namespace GUI
{
    partial class frmLogin
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
            this.lblDocumento   = new System.Windows.Forms.Label();
            this.lblPassword    = new System.Windows.Forms.Label();
            this.txtDocumento   = new System.Windows.Forms.TextBox();
            this.txtPassword    = new System.Windows.Forms.TextBox();
            this.btnIngresar    = new System.Windows.Forms.Button();
            this.btnSalir       = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblDocumento
            // 
            this.lblDocumento.AutoSize = true;
            this.lblDocumento.Location = new System.Drawing.Point(50, 50);
            this.lblDocumento.Name     = "lblDocumento";
            this.lblDocumento.Size     = new System.Drawing.Size(70, 17);
            this.lblDocumento.Text     = "Documento:";
            // 
            // txtDocumento
            // 
            this.txtDocumento.Location = new System.Drawing.Point(150, 47);
            this.txtDocumento.Name     = "txtDocumento";
            this.txtDocumento.Size     = new System.Drawing.Size(200, 22);
            this.txtDocumento.TabIndex = 0;
            this.txtDocumento.Text     = "admin";
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(50, 90);
            this.lblPassword.Name     = "lblPassword";
            this.lblPassword.Size     = new System.Drawing.Size(70, 17);
            this.lblPassword.Text     = "Contrasena:";
            // 
            // txtPassword
            // 
            this.txtPassword.Location     = new System.Drawing.Point(150, 87);
            this.txtPassword.Name         = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size         = new System.Drawing.Size(200, 22);
            this.txtPassword.TabIndex     = 1;
            this.txtPassword.Text         = "1234";
            // 
            // btnIngresar
            // 
            this.btnIngresar.Location = new System.Drawing.Point(100, 135);
            this.btnIngresar.Name     = "btnIngresar";
            this.btnIngresar.Size     = new System.Drawing.Size(110, 30);
            this.btnIngresar.TabIndex = 2;
            this.btnIngresar.Text     = "Ingresar";
            this.btnIngresar.Click   += new System.EventHandler(this.btnIngresar_Click);
            // 
            // btnSalir
            // 
            this.btnSalir.Location = new System.Drawing.Point(230, 135);
            this.btnSalir.Name     = "btnSalir";
            this.btnSalir.Size     = new System.Drawing.Size(110, 30);
            this.btnSalir.TabIndex = 3;
            this.btnSalir.Text     = "Salir";
            this.btnSalir.Click   += new System.EventHandler(this.btnSalir_Click);
            // 
            // frmLogin
            // 
            this.AcceptButton          = this.btnIngresar;
            this.AutoScaleDimensions   = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode         = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize            = new System.Drawing.Size(450, 200);
            this.ControlBox            = false;
            this.Controls.Add(this.lblDocumento);
            this.Controls.Add(this.txtDocumento);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.btnIngresar);
            this.Controls.Add(this.btnSalir);
            this.Name                  = "frmLogin";
            this.StartPosition         = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text                  = "WardrobeFlow - Iniciar Sesion";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label   lblDocumento;
        private System.Windows.Forms.Label   lblPassword;
        private System.Windows.Forms.TextBox txtDocumento;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Button  btnIngresar;
        private System.Windows.Forms.Button  btnSalir;
    }
}
