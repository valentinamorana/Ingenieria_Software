namespace GUI
{
    partial class CambioEstadoDialog
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
            this.lblPrendaInfo = new System.Windows.Forms.Label();
            this.lblNuevoEstado = new System.Windows.Forms.Label();
            this.cmbOpciones = new System.Windows.Forms.ComboBox();
            this.lblMensaje = new System.Windows.Forms.Label();
            this.btnConfirmar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblPrendaInfo
            // 
            this.lblPrendaInfo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblPrendaInfo.Location = new System.Drawing.Point(16, 16);
            this.lblPrendaInfo.Name = "lblPrendaInfo";
            this.lblPrendaInfo.Size = new System.Drawing.Size(330, 23);
            this.lblPrendaInfo.TabIndex = 0;
            this.lblPrendaInfo.Text = "Prenda: —";
            // 
            // lblNuevoEstado
            // 
            this.lblNuevoEstado.Location = new System.Drawing.Point(16, 50);
            this.lblNuevoEstado.Name = "lblNuevoEstado";
            this.lblNuevoEstado.Size = new System.Drawing.Size(120, 15);
            this.lblNuevoEstado.TabIndex = 1;
            this.lblNuevoEstado.Text = "Nuevo estado:";
            // 
            // cmbOpciones
            // 
            this.cmbOpciones.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbOpciones.Location = new System.Drawing.Point(16, 68);
            this.cmbOpciones.Name = "cmbOpciones";
            this.cmbOpciones.Size = new System.Drawing.Size(330, 21);
            this.cmbOpciones.TabIndex = 2;
            // 
            // lblMensaje
            // 
            this.lblMensaje.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblMensaje.ForeColor = System.Drawing.Color.DarkRed;
            this.lblMensaje.Location = new System.Drawing.Point(16, 102);
            this.lblMensaje.Name = "lblMensaje";
            this.lblMensaje.Size = new System.Drawing.Size(330, 24);
            this.lblMensaje.TabIndex = 3;
            // 
            // btnConfirmar
            // 
            this.btnConfirmar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(100)))), ((int)(((byte)(135)))));
            this.btnConfirmar.FlatAppearance.BorderSize = 0;
            this.btnConfirmar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConfirmar.ForeColor = System.Drawing.Color.White;
            this.btnConfirmar.Location = new System.Drawing.Point(16, 130);
            this.btnConfirmar.Name = "btnConfirmar";
            this.btnConfirmar.Size = new System.Drawing.Size(160, 32);
            this.btnConfirmar.TabIndex = 4;
            this.btnConfirmar.Text = "Confirmar Cambio";
            this.btnConfirmar.UseVisualStyleBackColor = false;
            this.btnConfirmar.Click += new System.EventHandler(this.BtnConfirmar_Click);
            // 
            // btnCancelar
            // 
            this.btnCancelar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancelar.Location = new System.Drawing.Point(184, 130);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(100, 32);
            this.btnCancelar.TabIndex = 5;
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.Click += new System.EventHandler(this.BtnCancelar_Click);
            // 
            // CambioEstadoDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(380, 180);
            this.Controls.Add(this.lblPrendaInfo);
            this.Controls.Add(this.lblNuevoEstado);
            this.Controls.Add(this.cmbOpciones);
            this.Controls.Add(this.lblMensaje);
            this.Controls.Add(this.btnConfirmar);
            this.Controls.Add(this.btnCancelar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CambioEstadoDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Cambiar Estado de Prenda";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label    lblPrendaInfo;
        private System.Windows.Forms.Label    lblNuevoEstado;
        private System.Windows.Forms.ComboBox cmbOpciones;
        private System.Windows.Forms.Label    lblMensaje;
        private System.Windows.Forms.Button   btnConfirmar;
        private System.Windows.Forms.Button   btnCancelar;
    }
}
