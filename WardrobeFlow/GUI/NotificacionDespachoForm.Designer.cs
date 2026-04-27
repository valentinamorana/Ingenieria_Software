namespace GUI
{
    partial class NotificacionDespachoForm
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
            this.panelHeader = new System.Windows.Forms.Panel();
            this.lblHeader = new System.Windows.Forms.Label();
            this.txtMensaje = new System.Windows.Forms.RichTextBox();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.btnCopiar = new System.Windows.Forms.Button();
            this.btnCerrar = new System.Windows.Forms.Button();
            this.lblNota = new System.Windows.Forms.Label();
            this.panelHeader.SuspendLayout();
            this.panelBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelHeader
            // 
            this.panelHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(110)))), ((int)(((byte)(180)))));
            this.panelHeader.Controls.Add(this.lblHeader);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Location = new System.Drawing.Point(0, 0);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Size = new System.Drawing.Size(560, 48);
            this.panelHeader.TabIndex = 0;
            // 
            // lblHeader
            // 
            this.lblHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(100)))), ((int)(((byte)(135)))));
            this.lblHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHeader.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblHeader.ForeColor = System.Drawing.Color.White;
            this.lblHeader.Location = new System.Drawing.Point(0, 0);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Padding = new System.Windows.Forms.Padding(14, 10, 0, 0);
            this.lblHeader.Size = new System.Drawing.Size(560, 48);
            this.lblHeader.TabIndex = 0;
            this.lblHeader.Text = "Notificación de Despacho";
            // 
            // txtMensaje
            // 
            this.txtMensaje.BackColor = System.Drawing.Color.White;
            this.txtMensaje.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtMensaje.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtMensaje.Font = new System.Drawing.Font("Consolas", 9.5F);
            this.txtMensaje.Location = new System.Drawing.Point(0, 48);
            this.txtMensaje.Name = "txtMensaje";
            this.txtMensaje.ReadOnly = true;
            this.txtMensaje.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.txtMensaje.Size = new System.Drawing.Size(560, 404);
            this.txtMensaje.TabIndex = 1;
            this.txtMensaje.Text = "";
            // 
            // panelBottom
            // 
            this.panelBottom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(245)))));
            this.panelBottom.Controls.Add(this.btnCopiar);
            this.panelBottom.Controls.Add(this.btnCerrar);
            this.panelBottom.Controls.Add(this.lblNota);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 452);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Padding = new System.Windows.Forms.Padding(10, 8, 10, 8);
            this.panelBottom.Size = new System.Drawing.Size(560, 48);
            this.panelBottom.TabIndex = 2;
            // 
            // btnCopiar
            // 
            this.btnCopiar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCopiar.Location = new System.Drawing.Point(10, 8);
            this.btnCopiar.Name = "btnCopiar";
            this.btnCopiar.Size = new System.Drawing.Size(180, 30);
            this.btnCopiar.TabIndex = 0;
            this.btnCopiar.Text = "Copiar al portapapeles";
            this.btnCopiar.Click += new System.EventHandler(this.BtnCopiar_Click);
            // 
            // btnCerrar
            // 
            this.btnCerrar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCerrar.Location = new System.Drawing.Point(198, 8);
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.Size = new System.Drawing.Size(80, 30);
            this.btnCerrar.TabIndex = 1;
            this.btnCerrar.Text = "Cerrar";
            this.btnCerrar.Click += new System.EventHandler(this.BtnCerrar_Click);
            // 
            // lblNota
            // 
            this.lblNota.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblNota.ForeColor = System.Drawing.Color.DimGray;
            this.lblNota.Location = new System.Drawing.Point(290, 14);
            this.lblNota.Name = "lblNota";
            this.lblNota.Size = new System.Drawing.Size(260, 23);
            this.lblNota.TabIndex = 2;
            this.lblNota.Text = "ℹ Este mensaje puede enviarse por email o WhatsApp al cliente.";
            // 
            // NotificacionDespachoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(560, 500);
            this.Controls.Add(this.txtMensaje);
            this.Controls.Add(this.panelBottom);
            this.Controls.Add(this.panelHeader);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NotificacionDespachoForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Notificación de Despacho";
            this.panelHeader.ResumeLayout(false);
            this.panelBottom.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel       panelHeader;
        private System.Windows.Forms.Label       lblHeader;
        private System.Windows.Forms.RichTextBox txtMensaje;
        private System.Windows.Forms.Panel       panelBottom;
        private System.Windows.Forms.Button      btnCopiar;
        private System.Windows.Forms.Button      btnCerrar;
        private System.Windows.Forms.Label       lblNota;
    }
}
