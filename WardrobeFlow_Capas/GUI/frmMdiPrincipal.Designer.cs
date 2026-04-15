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
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.mnuSesion = new System.Windows.Forms.ToolStripMenuItem();
            this.itemLogin = new System.Windows.Forms.ToolStripMenuItem();
            this.itemLogout = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuGestores = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuGestorCategorias = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuGestorPrendas = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuGestorOutfits = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuGestorUsuarios = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuGestorPermisos = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuGestorBitacora = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripSesion = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblHint = new System.Windows.Forms.Label();
            this.pnlSeparador = new System.Windows.Forms.Panel();
            this.lblSubtitulo = new System.Windows.Forms.Label();
            this.lblTituloApp = new System.Windows.Forms.Label();
            this.pnlBienvenida = new System.Windows.Forms.Panel();
            this.menuStrip.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.pnlBienvenida.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuSesion,
            this.mnuGestores});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Padding = new System.Windows.Forms.Padding(4, 2, 0, 2);
            this.menuStrip.Size = new System.Drawing.Size(675, 24);
            this.menuStrip.TabIndex = 2;
            this.menuStrip.Text = "menuStrip";
            // 
            // mnuSesion
            // 
            this.mnuSesion.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.itemLogin,
            this.itemLogout});
            this.mnuSesion.Name = "mnuSesion";
            this.mnuSesion.Size = new System.Drawing.Size(53, 20);
            this.mnuSesion.Text = "Sesión";
            // 
            // itemLogin
            // 
            this.itemLogin.Name = "itemLogin";
            this.itemLogin.Size = new System.Drawing.Size(143, 22);
            this.itemLogin.Text = "Iniciar Sesión";
            this.itemLogin.Click += new System.EventHandler(this.itemLogin_Click);
            // 
            // itemLogout
            // 
            this.itemLogout.Name = "itemLogout";
            this.itemLogout.Size = new System.Drawing.Size(143, 22);
            this.itemLogout.Text = "Cerrar Sesión";
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
            this.mnuGestorBitacora});
            this.mnuGestores.Name = "mnuGestores";
            this.mnuGestores.Size = new System.Drawing.Size(64, 20);
            this.mnuGestores.Text = "Gestores";
            // 
            // mnuGestorCategorias
            // 
            this.mnuGestorCategorias.Name = "mnuGestorCategorias";
            this.mnuGestorCategorias.Size = new System.Drawing.Size(183, 22);
            this.mnuGestorCategorias.Text = "Gestor de Categorías";
            this.mnuGestorCategorias.Click += new System.EventHandler(this.mnuGestorCategorias_Click);
            // 
            // mnuGestorPrendas
            // 
            this.mnuGestorPrendas.Name = "mnuGestorPrendas";
            this.mnuGestorPrendas.Size = new System.Drawing.Size(183, 22);
            this.mnuGestorPrendas.Text = "Gestor de Prendas";
            this.mnuGestorPrendas.Click += new System.EventHandler(this.mnuGestorPrendas_Click);
            // 
            // mnuGestorOutfits
            // 
            this.mnuGestorOutfits.Name = "mnuGestorOutfits";
            this.mnuGestorOutfits.Size = new System.Drawing.Size(183, 22);
            this.mnuGestorOutfits.Text = "Gestor de Outfits";
            this.mnuGestorOutfits.Click += new System.EventHandler(this.mnuGestorOutfits_Click);
            // 
            // mnuGestorUsuarios
            // 
            this.mnuGestorUsuarios.Name = "mnuGestorUsuarios";
            this.mnuGestorUsuarios.Size = new System.Drawing.Size(183, 22);
            this.mnuGestorUsuarios.Text = "Gestor de Usuarios";
            this.mnuGestorUsuarios.Click += new System.EventHandler(this.mnuGestorUsuarios_Click);
            // 
            // mnuGestorPermisos
            // 
            this.mnuGestorPermisos.Name = "mnuGestorPermisos";
            this.mnuGestorPermisos.Size = new System.Drawing.Size(183, 22);
            this.mnuGestorPermisos.Text = "Gestor de Permisos";
            this.mnuGestorPermisos.Click += new System.EventHandler(this.mnuGestorPermisos_Click);
            // 
            // mnuGestorBitacora
            // 
            this.mnuGestorBitacora.Name = "mnuGestorBitacora";
            this.mnuGestorBitacora.Size = new System.Drawing.Size(183, 22);
            this.mnuGestorBitacora.Text = "Bitácora de Eventos";
            this.mnuGestorBitacora.Click += new System.EventHandler(this.mnuGestorBitacora_Click);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel,
            this.toolStripSesion});
            this.statusStrip.Location = new System.Drawing.Point(0, 452);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Padding = new System.Windows.Forms.Padding(1, 0, 10, 0);
            this.statusStrip.Size = new System.Drawing.Size(675, 22);
            this.statusStrip.TabIndex = 1;
            // 
            // toolStripLabel
            // 
            this.toolStripLabel.Name = "toolStripLabel";
            this.toolStripLabel.Size = new System.Drawing.Size(50, 17);
            this.toolStripLabel.Text = "Usuario:";
            // 
            // toolStripSesion
            // 
            this.toolStripSesion.Name = "toolStripSesion";
            this.toolStripSesion.Size = new System.Drawing.Size(116, 17);
            this.toolStripSesion.Text = "[ Sesión no iniciada ]";
            // 
            // lblHint
            // 
            this.lblHint.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(130)))), ((int)(((byte)(170)))));
            this.lblHint.Location = new System.Drawing.Point(0, 104);
            this.lblHint.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblHint.Name = "lblHint";
            this.lblHint.Size = new System.Drawing.Size(298, 32);
            this.lblHint.TabIndex = 3;
            this.lblHint.Text = "Sesión  →  Iniciar Sesión para comenzar";
            this.lblHint.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlSeparador
            // 
            this.pnlSeparador.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(180)))), ((int)(((byte)(220)))));
            this.pnlSeparador.Location = new System.Drawing.Point(60, 94);
            this.pnlSeparador.Margin = new System.Windows.Forms.Padding(2);
            this.pnlSeparador.Name = "pnlSeparador";
            this.pnlSeparador.Size = new System.Drawing.Size(180, 1);
            this.pnlSeparador.TabIndex = 2;
            // 
            // lblSubtitulo
            // 
            this.lblSubtitulo.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblSubtitulo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(80)))), ((int)(((byte)(120)))));
            this.lblSubtitulo.Location = new System.Drawing.Point(0, 65);
            this.lblSubtitulo.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblSubtitulo.Name = "lblSubtitulo";
            this.lblSubtitulo.Size = new System.Drawing.Size(298, 21);
            this.lblSubtitulo.TabIndex = 1;
            this.lblSubtitulo.Text = "Sistema de Gestión de Indumentaria";
            this.lblSubtitulo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblTituloApp
            // 
            this.lblTituloApp.Font = new System.Drawing.Font("Segoe UI", 22F, System.Drawing.FontStyle.Bold);
            this.lblTituloApp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(40)))), ((int)(((byte)(80)))));
            this.lblTituloApp.Location = new System.Drawing.Point(0, 23);
            this.lblTituloApp.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblTituloApp.Name = "lblTituloApp";
            this.lblTituloApp.Size = new System.Drawing.Size(298, 37);
            this.lblTituloApp.TabIndex = 0;
            this.lblTituloApp.Text = "WardrobeFlow";
            this.lblTituloApp.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlBienvenida
            // 
            this.pnlBienvenida.BackColor = System.Drawing.Color.White;
            this.pnlBienvenida.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlBienvenida.Controls.Add(this.lblTituloApp);
            this.pnlBienvenida.Controls.Add(this.lblSubtitulo);
            this.pnlBienvenida.Controls.Add(this.pnlSeparador);
            this.pnlBienvenida.Controls.Add(this.lblHint);
            this.pnlBienvenida.Location = new System.Drawing.Point(188, 156);
            this.pnlBienvenida.Margin = new System.Windows.Forms.Padding(2);
            this.pnlBienvenida.Name = "pnlBienvenida";
            this.pnlBienvenida.Size = new System.Drawing.Size(300, 163);
            this.pnlBienvenida.TabIndex = 0;
            // 
            // frmMdiPrincipal
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(675, 474);
            this.Controls.Add(this.pnlBienvenida);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.menuStrip);
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.menuStrip;
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "frmMdiPrincipal";
            this.Text = "WardrobeFlow";
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.pnlBienvenida.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripMenuItem mnuSesion;
        private System.Windows.Forms.ToolStripMenuItem itemLogin;
        private System.Windows.Forms.ToolStripMenuItem itemLogout;
        private System.Windows.Forms.ToolStripMenuItem mnuGestores;
        private System.Windows.Forms.ToolStripMenuItem mnuGestorCategorias;
        private System.Windows.Forms.ToolStripMenuItem mnuGestorPrendas;
        private System.Windows.Forms.ToolStripMenuItem mnuGestorOutfits;
        private System.Windows.Forms.ToolStripMenuItem mnuGestorUsuarios;
        private System.Windows.Forms.ToolStripMenuItem mnuGestorPermisos;
        private System.Windows.Forms.ToolStripMenuItem mnuGestorBitacora;
        private System.Windows.Forms.ToolStripStatusLabel toolStripLabel;
        private System.Windows.Forms.ToolStripStatusLabel toolStripSesion;
        private System.Windows.Forms.Label lblHint;
        private System.Windows.Forms.Panel pnlSeparador;
        private System.Windows.Forms.Label lblSubtitulo;
        private System.Windows.Forms.Label lblTituloApp;
        private System.Windows.Forms.Panel pnlBienvenida;
    }
}
