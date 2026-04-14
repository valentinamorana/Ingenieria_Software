namespace GUI
{
    partial class frmGestorPermisos
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
            this.cboUsuarios = new System.Windows.Forms.ComboBox();
            this.treeView1   = new System.Windows.Forms.TreeView();
            this.lblUsuario  = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblUsuario
            // 
            this.lblUsuario.AutoSize = true;
            this.lblUsuario.Location = new System.Drawing.Point(12, 15);
            this.lblUsuario.Name     = "lblUsuario";
            this.lblUsuario.Text     = "Usuario:";
            // 
            // cboUsuarios
            // 
            this.cboUsuarios.DropDownStyle         = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboUsuarios.FormattingEnabled      = true;
            this.cboUsuarios.Location               = new System.Drawing.Point(75, 12);
            this.cboUsuarios.Name                   = "cboUsuarios";
            this.cboUsuarios.Size                   = new System.Drawing.Size(280, 24);
            this.cboUsuarios.TabIndex               = 0;
            this.cboUsuarios.SelectedIndexChanged  += new System.EventHandler(this.cboUsuarios_SelectedIndexChanged);
            // 
            // treeView1
            // 
            this.treeView1.Location = new System.Drawing.Point(12, 50);
            this.treeView1.Name     = "treeView1";
            this.treeView1.Size     = new System.Drawing.Size(370, 380);
            this.treeView1.TabIndex = 1;
            // 
            // frmGestorPermisos
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(400, 460);
            this.Controls.Add(this.lblUsuario);
            this.Controls.Add(this.cboUsuarios);
            this.Controls.Add(this.treeView1);
            this.Name = "frmGestorPermisos";
            this.Text = "Gestor de Permisos";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label    lblUsuario;
        private System.Windows.Forms.ComboBox cboUsuarios;
        private System.Windows.Forms.TreeView treeView1;
    }
}
