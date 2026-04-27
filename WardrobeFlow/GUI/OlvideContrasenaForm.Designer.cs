namespace GUI
{
    partial class OlvideContrasenaForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.lblAccent = new System.Windows.Forms.Label();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.lblDesc = new System.Windows.Forms.Label();
            this.lblUser = new System.Windows.Forms.Label();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.lblMensaje = new System.Windows.Forms.Label();
            this.btnEnviar = new System.Windows.Forms.Button();
            this.btnCerrar = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblAccent
            // 
            this.lblAccent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(100)))), ((int)(((byte)(135)))));
            this.lblAccent.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblAccent.Location = new System.Drawing.Point(0, 0);
            this.lblAccent.Name = "lblAccent";
            this.lblAccent.Size = new System.Drawing.Size(380, 4);
            this.lblAccent.TabIndex = 0;
            // 
            // lblTitulo
            // 
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(60)))));
            this.lblTitulo.Location = new System.Drawing.Point(24, 22);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(332, 28);
            this.lblTitulo.TabIndex = 1;
            this.lblTitulo.Text = "Recuperar Contraseña";
            // 
            // lblDesc
            // 
            this.lblDesc.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblDesc.ForeColor = System.Drawing.Color.DimGray;
            this.lblDesc.Location = new System.Drawing.Point(24, 58);
            this.lblDesc.Name = "lblDesc";
            this.lblDesc.Size = new System.Drawing.Size(332, 40);
            this.lblDesc.TabIndex = 2;
            this.lblDesc.Text = "Ingresá tu nombre de usuario. Un administrador\npodrá resetear tu contraseña desde" +
    " el sistema.";
            // 
            // lblUser
            // 
            this.lblUser.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblUser.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.lblUser.Location = new System.Drawing.Point(24, 108);
            this.lblUser.Name = "lblUser";
            this.lblUser.Size = new System.Drawing.Size(332, 18);
            this.lblUser.TabIndex = 3;
            this.lblUser.Text = "Nombre de usuario:";
            // 
            // txtUsername
            // 
            this.txtUsername.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(245)))), ((int)(((byte)(248)))));
            this.txtUsername.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtUsername.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtUsername.Location = new System.Drawing.Point(24, 128);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(332, 25);
            this.txtUsername.TabIndex = 4;
            // 
            // lblMensaje
            // 
            this.lblMensaje.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblMensaje.ForeColor = System.Drawing.Color.DarkGreen;
            this.lblMensaje.Location = new System.Drawing.Point(24, 164);
            this.lblMensaje.Name = "lblMensaje";
            this.lblMensaje.Size = new System.Drawing.Size(332, 52);
            this.lblMensaje.TabIndex = 5;
            // 
            // btnEnviar
            // 
            this.btnEnviar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(100)))), ((int)(((byte)(135)))));
            this.btnEnviar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnEnviar.FlatAppearance.BorderSize = 0;
            this.btnEnviar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEnviar.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnEnviar.ForeColor = System.Drawing.Color.White;
            this.btnEnviar.Location = new System.Drawing.Point(24, 224);
            this.btnEnviar.Name = "btnEnviar";
            this.btnEnviar.Size = new System.Drawing.Size(160, 34);
            this.btnEnviar.TabIndex = 6;
            this.btnEnviar.Text = "Enviar solicitud";
            this.btnEnviar.UseVisualStyleBackColor = false;
            this.btnEnviar.Click += new System.EventHandler(this.BtnEnviar_Click);
            // 
            // btnCerrar
            // 
            this.btnCerrar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.btnCerrar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCerrar.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCerrar.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.btnCerrar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCerrar.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnCerrar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.btnCerrar.Location = new System.Drawing.Point(196, 224);
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.Size = new System.Drawing.Size(160, 34);
            this.btnCerrar.TabIndex = 7;
            this.btnCerrar.Text = "Cerrar";
            this.btnCerrar.UseVisualStyleBackColor = false;
            this.btnCerrar.Click += new System.EventHandler(this.BtnCerrar_Click);
            // 
            // OlvideContrasenaForm
            // 
            this.AcceptButton = this.btnEnviar;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.CancelButton = this.btnCerrar;
            this.ClientSize = new System.Drawing.Size(380, 300);
            this.Controls.Add(this.lblAccent);
            this.Controls.Add(this.lblTitulo);
            this.Controls.Add(this.lblDesc);
            this.Controls.Add(this.lblUser);
            this.Controls.Add(this.txtUsername);
            this.Controls.Add(this.lblMensaje);
            this.Controls.Add(this.btnEnviar);
            this.Controls.Add(this.btnCerrar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OlvideContrasenaForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Recuperar Contraseña";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label   lblAccent;
        private System.Windows.Forms.Label   lblTitulo;
        private System.Windows.Forms.Label   lblDesc;
        private System.Windows.Forms.Label   lblUser;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.Label   lblMensaje;
        private System.Windows.Forms.Button  btnEnviar;
        private System.Windows.Forms.Button  btnCerrar;
    }
}
