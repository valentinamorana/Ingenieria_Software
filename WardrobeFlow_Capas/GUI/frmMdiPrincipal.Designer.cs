namespace GUI
{
    partial class frmMdiPrincipal
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
            this.components          = new System.ComponentModel.Container();
            this.menuStrip           = new System.Windows.Forms.MenuStrip();
            this.mnuSesion           = new System.Windows.Forms.ToolStripMenuItem();
            this.itemLogin           = new System.Windows.Forms.ToolStripMenuItem();
            this.itemLogout          = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuGestores         = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuGestorCategorias = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuGestorPrendas    = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuGestorOutfits    = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuGestorUsuarios   = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuGestorPermisos   = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuGestorBitacora   = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip         = new System.Windows.Forms.StatusStrip();
            this.toolStripLabel      = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripSesion     = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.mnuSesion,
                this.mnuGestores });
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name     = "menuStrip";
            this.menuStrip.Size     = new System.Drawing.Size(900, 28);
            this.menuStrip.Text     = "menuStrip";
            // 
            // mnuSesion
            // 
            this.mnuSesion.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.itemLogin,
                this.itemLogout });
            this.mnuSesion.Name = "mnuSesion";
            this.mnuSesion.Text = "Sesion";
            // 
            // itemLogin
            // 
            this.itemLogin.Name   = "itemLogin";
            this.itemLogin.Text   = "Iniciar Sesion";
            this.itemLogin.Click += new System.EventHandler(this.itemLogin_Click);
            // 
            // itemLogout
            // 
            this.itemLogout.Name   = "itemLogout";
            this.itemLogout.Text   = "Cerrar Sesion";
            this.itemLogout.Click += new System.EventHandler(this.itemLogout_Click);
            // 
            // mnuGestores
            // 
            this.mnuGestores.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.mnuGestorCategorias,
                this.mnuGestorPrendas,
                this.mnuGestorOutfits,
                this.mnuGestorUsuarios,
                this.mnuGestorPermisos,
                this.mnuGestorBitacora });
            this.mnuGestores.Name = "mnuGestores";
            this.mnuGestores.Text = "Gestores";
            // 
            // mnuGestorCategorias
            // 
            this.mnuGestorCategorias.Name   = "mnuGestorCategorias";
            this.mnuGestorCategorias.Text   = "Gestor de Categorias";
            this.mnuGestorCategorias.Click += new System.EventHandler(this.mnuGestorCategorias_Click);
            // 
            // mnuGestorPrendas
            // 
            this.mnuGestorPrendas.Name   = "mnuGestorPrendas";
            this.mnuGestorPrendas.Text   = "Gestor de Prendas";
            this.mnuGestorPrendas.Click += new System.EventHandler(this.mnuGestorPrendas_Click);
            // 
            // mnuGestorOutfits
            // 
            this.mnuGestorOutfits.Name   = "mnuGestorOutfits";
            this.mnuGestorOutfits.Text   = "Gestor de Outfits";
            this.mnuGestorOutfits.Click += new System.EventHandler(this.mnuGestorOutfits_Click);
            // 
            // mnuGestorUsuarios
            // 
            this.mnuGestorUsuarios.Name   = "mnuGestorUsuarios";
            this.mnuGestorUsuarios.Text   = "Gestor de Usuarios";
            this.mnuGestorUsuarios.Click += new System.EventHandler(this.mnuGestorUsuarios_Click);
            // 
            // mnuGestorPermisos
            // 
            this.mnuGestorPermisos.Name   = "mnuGestorPermisos";
            this.mnuGestorPermisos.Text   = "Gestor de Permisos";
            this.mnuGestorPermisos.Click += new System.EventHandler(this.mnuGestorPermisos_Click);
            // 
            // mnuGestorBitacora
            // 
            this.mnuGestorBitacora.Name   = "mnuGestorBitacora";
            this.mnuGestorBitacora.Text   = "Bitacora de Eventos";
            this.mnuGestorBitacora.Click += new System.EventHandler(this.mnuGestorBitacora_Click);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.toolStripLabel,
                this.toolStripSesion });
            this.statusStrip.Location = new System.Drawing.Point(0, 558);
            this.statusStrip.Name     = "statusStrip";
            this.statusStrip.Size     = new System.Drawing.Size(900, 26);
            // 
            // toolStripLabel
            // 
            this.toolStripLabel.Name = "toolStripLabel";
            this.toolStripLabel.Text = "Usuario:";
            // 
            // toolStripSesion
            // 
            this.toolStripSesion.Name = "toolStripSesion";
            this.toolStripSesion.Text = "[ Sesion no iniciada ]";
            // 
            // frmMdiPrincipal
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(900, 584);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.menuStrip);
            this.IsMdiContainer      = true;
            this.MainMenuStrip       = this.menuStrip;
            this.Name                = "frmMdiPrincipal";
            this.Text                = "WardrobeFlow";
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.MenuStrip              menuStrip;
        private System.Windows.Forms.StatusStrip             statusStrip;
        private System.Windows.Forms.ToolStripMenuItem       mnuSesion;
        private System.Windows.Forms.ToolStripMenuItem       itemLogin;
        private System.Windows.Forms.ToolStripMenuItem       itemLogout;
        private System.Windows.Forms.ToolStripMenuItem       mnuGestores;
        private System.Windows.Forms.ToolStripMenuItem       mnuGestorCategorias;
        private System.Windows.Forms.ToolStripMenuItem       mnuGestorPrendas;
        private System.Windows.Forms.ToolStripMenuItem       mnuGestorOutfits;
        private System.Windows.Forms.ToolStripMenuItem       mnuGestorUsuarios;
        private System.Windows.Forms.ToolStripMenuItem       mnuGestorPermisos;
        private System.Windows.Forms.ToolStripMenuItem       mnuGestorBitacora;
        private System.Windows.Forms.ToolStripStatusLabel    toolStripLabel;
        private System.Windows.Forms.ToolStripStatusLabel    toolStripSesion;
    }
}
