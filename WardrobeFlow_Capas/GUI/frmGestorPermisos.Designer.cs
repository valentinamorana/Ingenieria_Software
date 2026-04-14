namespace GUI
{
    partial class frmGestorPermisos
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }
        private void InitializeComponent()
        {
            this.pnlHeader   = new System.Windows.Forms.Panel();
            this.lblTitulo   = new System.Windows.Forms.Label();
            this.pnlContent  = new System.Windows.Forms.Panel();
            this.lblUsuario  = new System.Windows.Forms.Label();
            this.cboUsuarios = new System.Windows.Forms.ComboBox();
            this.treeView1   = new System.Windows.Forms.TreeView();
            this.pnlHeader.SuspendLayout();
            this.pnlContent.SuspendLayout();
            this.SuspendLayout();
            // pnlHeader
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(180, 35, 75);
            this.pnlHeader.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Height    = 58;
            this.pnlHeader.Name      = "pnlHeader";
            this.pnlHeader.Controls.Add(this.lblTitulo);
            this.lblTitulo.AutoSize  = true;
            this.lblTitulo.Font      = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.White;
            this.lblTitulo.Location  = new System.Drawing.Point(18, 18);
            this.lblTitulo.Name      = "lblTitulo";
            this.lblTitulo.Text      = "Gestor de Permisos";
            // pnlContent
            this.pnlContent.BackColor = System.Drawing.Color.FromArgb(255, 240, 248);
            this.pnlContent.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Padding   = new System.Windows.Forms.Padding(14);
            this.pnlContent.Name      = "pnlContent";
            this.pnlContent.Controls.Add(this.lblUsuario);
            this.pnlContent.Controls.Add(this.cboUsuarios);
            this.pnlContent.Controls.Add(this.treeView1);
            // lblUsuario
            this.lblUsuario.AutoSize  = true;
            this.lblUsuario.Font      = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblUsuario.ForeColor = System.Drawing.Color.FromArgb(150, 80, 105);
            this.lblUsuario.Location  = new System.Drawing.Point(14, 18);
            this.lblUsuario.Name      = "lblUsuario";
            this.lblUsuario.Text      = "USUARIO:";
            // cboUsuarios
            this.cboUsuarios.BackColor    = System.Drawing.Color.FromArgb(255, 240, 248);
            this.cboUsuarios.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboUsuarios.FlatStyle    = System.Windows.Forms.FlatStyle.Flat;
            this.cboUsuarios.Font         = new System.Drawing.Font("Segoe UI", 9.5F);
            this.cboUsuarios.ForeColor    = System.Drawing.Color.FromArgb(35, 20, 30);
            this.cboUsuarios.FormattingEnabled = true;
            this.cboUsuarios.Location     = new System.Drawing.Point(100, 14);
            this.cboUsuarios.Name         = "cboUsuarios";
            this.cboUsuarios.Size         = new System.Drawing.Size(280, 28);
            this.cboUsuarios.TabIndex     = 0;
            this.cboUsuarios.SelectedIndexChanged += new System.EventHandler(this.cboUsuarios_SelectedIndexChanged);
            // treeView1
            this.treeView1.BackColor  = System.Drawing.Color.White;
            this.treeView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.treeView1.Font       = new System.Drawing.Font("Segoe UI", 9.5F);
            this.treeView1.ForeColor  = System.Drawing.Color.FromArgb(35, 20, 30);
            this.treeView1.LineColor  = System.Drawing.Color.FromArgb(180, 35, 75);
            this.treeView1.Location   = new System.Drawing.Point(14, 54);
            this.treeView1.Name       = "treeView1";
            this.treeView1.Size       = new System.Drawing.Size(370, 400);
            this.treeView1.TabIndex   = 1;
            // form
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor           = System.Drawing.Color.FromArgb(255, 240, 248);
            this.ClientSize          = new System.Drawing.Size(420, 536);
            this.Controls.Add(this.pnlContent);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9.5F); this.Name = "frmGestorPermisos"; this.Text = "WardrobeFlow - Permisos";
            this.pnlContent.ResumeLayout(false); this.pnlContent.PerformLayout();
            this.pnlHeader.ResumeLayout(false); this.pnlHeader.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel    pnlHeader;
        private System.Windows.Forms.Panel    pnlContent;
        private System.Windows.Forms.Label    lblTitulo;
        private System.Windows.Forms.Label    lblUsuario;
        private System.Windows.Forms.ComboBox cboUsuarios;
        private System.Windows.Forms.TreeView treeView1;
    }
}
