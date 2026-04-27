namespace GUI
{
    partial class ClienteForm
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
            this.lblNombre = new System.Windows.Forms.Label();
            this.txtNombre = new System.Windows.Forms.TextBox();
            this.lblApellido = new System.Windows.Forms.Label();
            this.txtApellido = new System.Windows.Forms.TextBox();
            this.lblDNI = new System.Windows.Forms.Label();
            this.txtDNI = new System.Windows.Forms.TextBox();
            this.lblEmail = new System.Windows.Forms.Label();
            this.txtEmail = new System.Windows.Forms.TextBox();
            this.lblMetodoPago = new System.Windows.Forms.Label();
            this.cmbMetodoPago = new System.Windows.Forms.ComboBox();
            this.lblPlan = new System.Windows.Forms.Label();
            this.cmbPlan = new System.Windows.Forms.ComboBox();
            this.lblMensaje = new System.Windows.Forms.Label();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblNombre
            // 
            this.lblNombre.Location = new System.Drawing.Point(16, 20);
            this.lblNombre.Name = "lblNombre";
            this.lblNombre.Size = new System.Drawing.Size(340, 15);
            this.lblNombre.TabIndex = 0;
            this.lblNombre.Text = "Nombre *";
            // 
            // txtNombre
            // 
            this.txtNombre.Location = new System.Drawing.Point(16, 38);
            this.txtNombre.Name = "txtNombre";
            this.txtNombre.Size = new System.Drawing.Size(340, 20);
            this.txtNombre.TabIndex = 1;
            // 
            // lblApellido
            // 
            this.lblApellido.Location = new System.Drawing.Point(16, 72);
            this.lblApellido.Name = "lblApellido";
            this.lblApellido.Size = new System.Drawing.Size(340, 15);
            this.lblApellido.TabIndex = 2;
            this.lblApellido.Text = "Apellido *";
            // 
            // txtApellido
            // 
            this.txtApellido.Location = new System.Drawing.Point(16, 90);
            this.txtApellido.Name = "txtApellido";
            this.txtApellido.Size = new System.Drawing.Size(340, 20);
            this.txtApellido.TabIndex = 3;
            // 
            // lblDNI
            // 
            this.lblDNI.Location = new System.Drawing.Point(16, 124);
            this.lblDNI.Name = "lblDNI";
            this.lblDNI.Size = new System.Drawing.Size(340, 15);
            this.lblDNI.TabIndex = 4;
            this.lblDNI.Text = "DNI * (7-8 dígitos)";
            // 
            // txtDNI
            // 
            this.txtDNI.Location = new System.Drawing.Point(16, 142);
            this.txtDNI.Name = "txtDNI";
            this.txtDNI.Size = new System.Drawing.Size(340, 20);
            this.txtDNI.TabIndex = 5;
            // 
            // lblEmail
            // 
            this.lblEmail.Location = new System.Drawing.Point(16, 176);
            this.lblEmail.Name = "lblEmail";
            this.lblEmail.Size = new System.Drawing.Size(340, 15);
            this.lblEmail.TabIndex = 6;
            this.lblEmail.Text = "Email";
            // 
            // txtEmail
            // 
            this.txtEmail.Location = new System.Drawing.Point(16, 194);
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.Size = new System.Drawing.Size(340, 20);
            this.txtEmail.TabIndex = 7;
            // 
            // lblMetodoPago
            // 
            this.lblMetodoPago.Location = new System.Drawing.Point(16, 228);
            this.lblMetodoPago.Name = "lblMetodoPago";
            this.lblMetodoPago.Size = new System.Drawing.Size(340, 15);
            this.lblMetodoPago.TabIndex = 8;
            this.lblMetodoPago.Text = "Método de Pago *";
            // 
            // cmbMetodoPago
            // 
            this.cmbMetodoPago.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMetodoPago.Items.AddRange(new object[] {
            "Efectivo",
            "Débito",
            "Crédito",
            "Transferencia"});
            this.cmbMetodoPago.Location = new System.Drawing.Point(16, 246);
            this.cmbMetodoPago.Name = "cmbMetodoPago";
            this.cmbMetodoPago.Size = new System.Drawing.Size(340, 21);
            this.cmbMetodoPago.TabIndex = 9;
            // 
            // lblPlan
            // 
            this.lblPlan.Location = new System.Drawing.Point(16, 280);
            this.lblPlan.Name = "lblPlan";
            this.lblPlan.Size = new System.Drawing.Size(340, 15);
            this.lblPlan.TabIndex = 10;
            this.lblPlan.Text = "Plan de Suscripción";
            // 
            // cmbPlan
            // 
            this.cmbPlan.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPlan.Location = new System.Drawing.Point(16, 298);
            this.cmbPlan.Name = "cmbPlan";
            this.cmbPlan.Size = new System.Drawing.Size(340, 21);
            this.cmbPlan.TabIndex = 11;
            // 
            // lblMensaje
            // 
            this.lblMensaje.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblMensaje.ForeColor = System.Drawing.Color.DarkRed;
            this.lblMensaje.Location = new System.Drawing.Point(16, 332);
            this.lblMensaje.Name = "lblMensaje";
            this.lblMensaje.Size = new System.Drawing.Size(340, 36);
            this.lblMensaje.TabIndex = 12;
            // 
            // btnGuardar
            // 
            this.btnGuardar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(100)))), ((int)(((byte)(135)))));
            this.btnGuardar.FlatAppearance.BorderSize = 0;
            this.btnGuardar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGuardar.ForeColor = System.Drawing.Color.White;
            this.btnGuardar.Location = new System.Drawing.Point(16, 376);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(160, 34);
            this.btnGuardar.TabIndex = 13;
            this.btnGuardar.Text = "Registrar Cliente";
            this.btnGuardar.UseVisualStyleBackColor = false;
            this.btnGuardar.Click += new System.EventHandler(this.BtnGuardar_Click);
            // 
            // btnCancelar
            // 
            this.btnCancelar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancelar.Location = new System.Drawing.Point(192, 376);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(100, 34);
            this.btnCancelar.TabIndex = 14;
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.Click += new System.EventHandler(this.BtnCancelar_Click);
            // 
            // ClienteForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(380, 428);
            this.Controls.Add(this.lblNombre);
            this.Controls.Add(this.txtNombre);
            this.Controls.Add(this.lblApellido);
            this.Controls.Add(this.txtApellido);
            this.Controls.Add(this.lblDNI);
            this.Controls.Add(this.txtDNI);
            this.Controls.Add(this.lblEmail);
            this.Controls.Add(this.txtEmail);
            this.Controls.Add(this.lblMetodoPago);
            this.Controls.Add(this.cmbMetodoPago);
            this.Controls.Add(this.lblPlan);
            this.Controls.Add(this.cmbPlan);
            this.Controls.Add(this.lblMensaje);
            this.Controls.Add(this.btnGuardar);
            this.Controls.Add(this.btnCancelar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ClienteForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Nuevo Cliente";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label    lblNombre;
        private System.Windows.Forms.TextBox  txtNombre;
        private System.Windows.Forms.Label    lblApellido;
        private System.Windows.Forms.TextBox  txtApellido;
        private System.Windows.Forms.Label    lblDNI;
        private System.Windows.Forms.TextBox  txtDNI;
        private System.Windows.Forms.Label    lblEmail;
        private System.Windows.Forms.TextBox  txtEmail;
        private System.Windows.Forms.Label    lblMetodoPago;
        private System.Windows.Forms.ComboBox cmbMetodoPago;
        private System.Windows.Forms.Label    lblPlan;
        private System.Windows.Forms.ComboBox cmbPlan;
        private System.Windows.Forms.Label    lblMensaje;
        private System.Windows.Forms.Button   btnGuardar;
        private System.Windows.Forms.Button   btnCancelar;
    }
}
