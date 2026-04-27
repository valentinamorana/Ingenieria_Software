namespace GUI
{
    partial class Login
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Login));
            this.txtUsuario = new System.Windows.Forms.TextBox();
            this.txtContraseña = new System.Windows.Forms.TextBox();
            this.lblUsuario = new System.Windows.Forms.Label();
            this.lblContraseña = new System.Windows.Forms.Label();
            this.btnIngresar = new System.Windows.Forms.Button();
            this.btnSalir = new System.Windows.Forms.Button();
            this.lblError = new System.Windows.Forms.Label();
            this.lnkOlvidaste = new System.Windows.Forms.LinkLabel();
            this.pnlCard = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSubtitulo = new System.Windows.Forms.Label();
            this.lblAccent = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.pnlCard.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtUsuario
            // 
            this.txtUsuario.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(245)))), ((int)(((byte)(248)))));
            this.txtUsuario.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtUsuario.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtUsuario.Location = new System.Drawing.Point(22, 46);
            this.txtUsuario.Margin = new System.Windows.Forms.Padding(2);
            this.txtUsuario.Name = "txtUsuario";
            this.txtUsuario.Size = new System.Drawing.Size(240, 25);
            this.txtUsuario.TabIndex = 1;
            // 
            // txtContraseña
            // 
            this.txtContraseña.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(245)))), ((int)(((byte)(248)))));
            this.txtContraseña.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtContraseña.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtContraseña.Location = new System.Drawing.Point(22, 102);
            this.txtContraseña.Margin = new System.Windows.Forms.Padding(2);
            this.txtContraseña.Name = "txtContraseña";
            this.txtContraseña.PasswordChar = '●';
            this.txtContraseña.Size = new System.Drawing.Size(240, 25);
            this.txtContraseña.TabIndex = 3;
            // 
            // lblUsuario
            // 
            this.lblUsuario.BackColor = System.Drawing.Color.White;
            this.lblUsuario.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblUsuario.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblUsuario.Location = new System.Drawing.Point(22, 28);
            this.lblUsuario.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblUsuario.Name = "lblUsuario";
            this.lblUsuario.Size = new System.Drawing.Size(240, 15);
            this.lblUsuario.TabIndex = 0;
            this.lblUsuario.Text = "Usuario";
            // 
            // lblContraseña
            // 
            this.lblContraseña.BackColor = System.Drawing.Color.White;
            this.lblContraseña.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblContraseña.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblContraseña.Location = new System.Drawing.Point(22, 85);
            this.lblContraseña.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblContraseña.Name = "lblContraseña";
            this.lblContraseña.Size = new System.Drawing.Size(240, 15);
            this.lblContraseña.TabIndex = 2;
            this.lblContraseña.Text = "Contraseña";
            // 
            // btnIngresar
            // 
            this.btnIngresar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(100)))), ((int)(((byte)(135)))));
            this.btnIngresar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnIngresar.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(160)))), ((int)(((byte)(100)))));
            this.btnIngresar.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(120)))), ((int)(((byte)(70)))));
            this.btnIngresar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(160)))), ((int)(((byte)(100)))));
            this.btnIngresar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnIngresar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnIngresar.ForeColor = System.Drawing.Color.White;
            this.btnIngresar.Location = new System.Drawing.Point(22, 153);
            this.btnIngresar.Margin = new System.Windows.Forms.Padding(2);
            this.btnIngresar.Name = "btnIngresar";
            this.btnIngresar.Size = new System.Drawing.Size(240, 34);
            this.btnIngresar.TabIndex = 5;
            this.btnIngresar.Text = "INGRESAR";
            this.btnIngresar.UseVisualStyleBackColor = false;
            this.btnIngresar.Click += new System.EventHandler(this.btnIngresar_Click);
            // 
            // btnSalir
            // 
            this.btnSalir.BackColor = System.Drawing.Color.White;
            this.btnSalir.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSalir.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.btnSalir.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.btnSalir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSalir.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnSalir.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(130)))), ((int)(((byte)(130)))), ((int)(((byte)(130)))));
            this.btnSalir.Location = new System.Drawing.Point(98, 238);
            this.btnSalir.Margin = new System.Windows.Forms.Padding(2);
            this.btnSalir.Name = "btnSalir";
            this.btnSalir.Size = new System.Drawing.Size(90, 24);
            this.btnSalir.TabIndex = 7;
            this.btnSalir.Text = "SALIR";
            this.btnSalir.UseVisualStyleBackColor = false;
            this.btnSalir.Click += new System.EventHandler(this.btnSalir_Click);
            // 
            // lblError
            // 
            this.lblError.BackColor = System.Drawing.Color.White;
            this.lblError.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblError.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.lblError.Location = new System.Drawing.Point(22, 136);
            this.lblError.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblError.Name = "lblError";
            this.lblError.Size = new System.Drawing.Size(240, 16);
            this.lblError.TabIndex = 4;
            this.lblError.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lnkOlvidaste
            // 
            this.lnkOlvidaste.ActiveLinkColor = System.Drawing.Color.DeepPink;
            this.lnkOlvidaste.BackColor = System.Drawing.Color.White;
            this.lnkOlvidaste.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lnkOlvidaste.LinkColor = System.Drawing.Color.Purple;
            this.lnkOlvidaste.Location = new System.Drawing.Point(22, 210);
            this.lnkOlvidaste.Name = "lnkOlvidaste";
            this.lnkOlvidaste.Size = new System.Drawing.Size(240, 18);
            this.lnkOlvidaste.TabIndex = 6;
            this.lnkOlvidaste.TabStop = true;
            this.lnkOlvidaste.Text = "Olvidé mi contraseña";
            this.lnkOlvidaste.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lnkOlvidaste.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkOlvidaste_LinkClicked);
            // 
            // pnlCard
            // 
            this.pnlCard.BackColor = System.Drawing.Color.White;
            this.pnlCard.Controls.Add(this.lblUsuario);
            this.pnlCard.Controls.Add(this.txtUsuario);
            this.pnlCard.Controls.Add(this.lblContraseña);
            this.pnlCard.Controls.Add(this.txtContraseña);
            this.pnlCard.Controls.Add(this.lblError);
            this.pnlCard.Controls.Add(this.btnIngresar);
            this.pnlCard.Controls.Add(this.lnkOlvidaste);
            this.pnlCard.Controls.Add(this.btnSalir);
            this.pnlCard.Location = new System.Drawing.Point(233, 125);
            this.pnlCard.Margin = new System.Windows.Forms.Padding(2);
            this.pnlCard.Name = "pnlCard";
            this.pnlCard.Size = new System.Drawing.Size(285, 278);
            this.pnlCard.TabIndex = 13;
            // 
            // lblTitle
            // 
            this.lblTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 26F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(3, 31);
            this.lblTitle.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(742, 45);
            this.lblTitle.TabIndex = 11;
            this.lblTitle.Text = "WardrobeFlow";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblSubtitulo
            // 
            this.lblSubtitulo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(100)))), ((int)(((byte)(135)))));
            this.lblSubtitulo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSubtitulo.ForeColor = System.Drawing.Color.White;
            this.lblSubtitulo.Location = new System.Drawing.Point(233, 84);
            this.lblSubtitulo.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblSubtitulo.Name = "lblSubtitulo";
            this.lblSubtitulo.Size = new System.Drawing.Size(285, 18);
            this.lblSubtitulo.TabIndex = 12;
            this.lblSubtitulo.Text = "PORTAL DE EMPLEADOS";
            this.lblSubtitulo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblAccent
            // 
            this.lblAccent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(100)))), ((int)(((byte)(135)))));
            this.lblAccent.Location = new System.Drawing.Point(0, -4);
            this.lblAccent.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblAccent.Name = "lblAccent";
            this.lblAccent.Size = new System.Drawing.Size(746, 10);
            this.lblAccent.TabIndex = 10;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(100)))), ((int)(((byte)(135)))));
            this.label1.Location = new System.Drawing.Point(0, 430);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(746, 10);
            this.label1.TabIndex = 14;
            // 
            // Login
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(146)))), ((int)(((byte)(62)))), ((int)(((byte)(96)))));
            this.ClientSize = new System.Drawing.Size(744, 437);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblAccent);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblSubtitulo);
            this.Controls.Add(this.pnlCard);
            this.ForeColor = System.Drawing.Color.Black;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Login";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "WardrobeFlow";
            this.pnlCard.ResumeLayout(false);
            this.pnlCard.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox     txtUsuario;
        private System.Windows.Forms.TextBox     txtContraseña;
        private System.Windows.Forms.Label       lblUsuario;
        private System.Windows.Forms.Label       lblContraseña;
        private System.Windows.Forms.Button      btnIngresar;
        private System.Windows.Forms.Button      btnSalir;
        private System.Windows.Forms.Label       lblError;
        private System.Windows.Forms.LinkLabel   lnkOlvidaste;
        private System.Windows.Forms.Panel       pnlCard;
        private System.Windows.Forms.Label       lblTitle;
        private System.Windows.Forms.Label       lblSubtitulo;
        private System.Windows.Forms.Label       lblAccent;
        private System.Windows.Forms.Label label1;
    }
}
