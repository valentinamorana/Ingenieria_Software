namespace GUI
{
    partial class frmGestorBitacora
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }
        private void InitializeComponent()
        {
            this.pnlHeader    = new System.Windows.Forms.Panel();
            this.lblTitulo    = new System.Windows.Forms.Label();
            this.pnlContent   = new System.Windows.Forms.Panel();
            this.pnlToolbar   = new System.Windows.Forms.Panel();
            this.lblFiltro    = new System.Windows.Forms.Label();
            this.txtFiltro    = new System.Windows.Forms.TextBox();
            this.btnActualizar = new System.Windows.Forms.Button();
            this.btnCerrar    = new System.Windows.Forms.Button();
            this.dgvBitacora  = new System.Windows.Forms.DataGridView();
            this.colFecha     = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colUsuario   = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colOperacion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colModulo    = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDescripcion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colExitoso   = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lblTotal     = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBitacora)).BeginInit();
            this.pnlHeader.SuspendLayout();
            this.pnlContent.SuspendLayout();
            this.pnlToolbar.SuspendLayout();
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
            this.lblTitulo.Text      = "Bitacora de Eventos";
            // pnlContent
            this.pnlContent.BackColor = System.Drawing.Color.FromArgb(255, 240, 248);
            this.pnlContent.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Padding   = new System.Windows.Forms.Padding(14);
            this.pnlContent.Name      = "pnlContent";
            this.pnlContent.Controls.Add(this.pnlToolbar);
            this.pnlContent.Controls.Add(this.dgvBitacora);
            this.pnlContent.Controls.Add(this.lblTotal);
            // pnlToolbar
            this.pnlToolbar.BackColor = System.Drawing.Color.White;
            this.pnlToolbar.Location  = new System.Drawing.Point(14, 14);
            this.pnlToolbar.Name      = "pnlToolbar";
            this.pnlToolbar.Size      = new System.Drawing.Size(852, 48);
            this.pnlToolbar.Controls.Add(this.lblFiltro);
            this.pnlToolbar.Controls.Add(this.txtFiltro);
            this.pnlToolbar.Controls.Add(this.btnActualizar);
            this.pnlToolbar.Controls.Add(this.btnCerrar);
            this.lblFiltro.AutoSize  = true;
            this.lblFiltro.Font      = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblFiltro.ForeColor = System.Drawing.Color.FromArgb(150, 80, 105);
            this.lblFiltro.Location  = new System.Drawing.Point(12, 14);
            this.lblFiltro.Name      = "lblFiltro";
            this.lblFiltro.Text      = "Buscar:";
            this.txtFiltro.BackColor    = System.Drawing.Color.FromArgb(255, 240, 248);
            this.txtFiltro.BorderStyle  = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtFiltro.Font         = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtFiltro.ForeColor    = System.Drawing.Color.FromArgb(35, 20, 30);
            this.txtFiltro.Location     = new System.Drawing.Point(74, 10);
            this.txtFiltro.Name         = "txtFiltro";
            this.txtFiltro.Size         = new System.Drawing.Size(300, 28);
            this.txtFiltro.TabIndex     = 0;
            this.txtFiltro.TextChanged += new System.EventHandler(this.txtFiltro_TextChanged);
            this.btnActualizar.BackColor = System.Drawing.Color.FromArgb(180, 35, 75);
            this.btnActualizar.ForeColor = System.Drawing.Color.White;
            this.btnActualizar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnActualizar.FlatAppearance.BorderSize = 0;
            this.btnActualizar.Font      = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnActualizar.Location  = new System.Drawing.Point(660, 8);
            this.btnActualizar.Name      = "btnActualizar";
            this.btnActualizar.Size      = new System.Drawing.Size(88, 32);
            this.btnActualizar.TabIndex  = 1;
            this.btnActualizar.Text      = "Actualizar";
            this.btnActualizar.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnActualizar.Click    += new System.EventHandler(this.btnActualizar_Click);
            this.btnCerrar.BackColor = System.Drawing.Color.White;
            this.btnCerrar.ForeColor = System.Drawing.Color.FromArgb(180, 35, 75);
            this.btnCerrar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCerrar.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(180, 35, 75);
            this.btnCerrar.FlatAppearance.BorderSize  = 1;
            this.btnCerrar.Font      = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnCerrar.Location  = new System.Drawing.Point(758, 8);
            this.btnCerrar.Name      = "btnCerrar";
            this.btnCerrar.Size      = new System.Drawing.Size(80, 32);
            this.btnCerrar.TabIndex  = 2;
            this.btnCerrar.Text      = "Cerrar";
            this.btnCerrar.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnCerrar.Click    += new System.EventHandler(this.btnCerrar_Click);
            // dgvBitacora
            this.dgvBitacora.AllowUserToAddRows    = false;
            this.dgvBitacora.AllowUserToDeleteRows = false;
            this.dgvBitacora.ReadOnly              = true;
            this.dgvBitacora.AutoSizeColumnsMode   = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvBitacora.Location              = new System.Drawing.Point(14, 72);
            this.dgvBitacora.Name                  = "dgvBitacora";
            this.dgvBitacora.Size                  = new System.Drawing.Size(852, 400);
            this.dgvBitacora.TabIndex              = 0;
            this.dgvBitacora.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                this.colFecha, this.colUsuario, this.colOperacion,
                this.colModulo, this.colDescripcion, this.colExitoso });
            
            this.dgvBitacora.BackgroundColor                     = System.Drawing.Color.White;
            this.dgvBitacora.BorderStyle                         = System.Windows.Forms.BorderStyle.None;
            this.dgvBitacora.CellBorderStyle                     = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.dgvBitacora.GridColor                           = System.Drawing.Color.FromArgb(220, 170, 195);
            this.dgvBitacora.EnableHeadersVisualStyles           = false;
            this.dgvBitacora.ColumnHeadersBorderStyle            = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dgvBitacora.ColumnHeadersDefaultCellStyle.BackColor  = System.Drawing.Color.FromArgb(180, 35, 75);
            this.dgvBitacora.ColumnHeadersDefaultCellStyle.ForeColor  = System.Drawing.Color.White;
            this.dgvBitacora.ColumnHeadersDefaultCellStyle.Font       = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.dgvBitacora.ColumnHeadersDefaultCellStyle.Padding    = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.dgvBitacora.ColumnHeadersHeight                 = 36;
            this.dgvBitacora.DefaultCellStyle.BackColor          = System.Drawing.Color.White;
            this.dgvBitacora.DefaultCellStyle.ForeColor          = System.Drawing.Color.FromArgb(35, 20, 30);
            this.dgvBitacora.DefaultCellStyle.Font               = new System.Drawing.Font("Segoe UI", 9.5F);
            this.dgvBitacora.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(232, 120, 160);
            this.dgvBitacora.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.White;
            this.dgvBitacora.DefaultCellStyle.Padding            = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.dgvBitacora.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(255, 225, 238);
            this.dgvBitacora.RowHeadersVisible                   = false;
            this.dgvBitacora.RowTemplate.Height                  = 28;
            this.colFecha.HeaderText = "Fecha / Hora";    this.colFecha.Name = "colFecha";
            this.colUsuario.HeaderText = "Usuario";       this.colUsuario.Name = "colUsuario";
            this.colOperacion.HeaderText = "Operacion";   this.colOperacion.Name = "colOperacion";
            this.colModulo.HeaderText = "Modulo";         this.colModulo.Name = "colModulo";
            this.colDescripcion.HeaderText = "Descripcion"; this.colDescripcion.Name = "colDescripcion";
            this.colExitoso.HeaderText = "OK";            this.colExitoso.Name = "colExitoso"; this.colExitoso.FillWeight = 40;
            // lblTotal
            this.lblTotal.AutoSize  = true;
            this.lblTotal.Font      = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblTotal.ForeColor = System.Drawing.Color.FromArgb(150, 80, 105);
            this.lblTotal.Location  = new System.Drawing.Point(14, 482);
            this.lblTotal.Name      = "lblTotal";
            this.lblTotal.Text      = "Total de eventos: 0";
            // form
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor           = System.Drawing.Color.FromArgb(255, 240, 248);
            this.ClientSize          = new System.Drawing.Size(880, 560);
            this.Controls.Add(this.pnlContent);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9.5F); this.Name = "frmGestorBitacora"; this.Text = "WardrobeFlow - Bitacora";
            ((System.ComponentModel.ISupportInitialize)(this.dgvBitacora)).EndInit();
            this.pnlContent.ResumeLayout(false); this.pnlContent.PerformLayout();
            this.pnlHeader.ResumeLayout(false); this.pnlHeader.PerformLayout();
            this.pnlToolbar.ResumeLayout(false); this.pnlToolbar.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel        pnlHeader;
        private System.Windows.Forms.Panel        pnlContent;
        private System.Windows.Forms.Panel        pnlToolbar;
        private System.Windows.Forms.Label        lblTitulo;
        private System.Windows.Forms.Label        lblFiltro;
        private System.Windows.Forms.TextBox      txtFiltro;
        private System.Windows.Forms.Button       btnActualizar;
        private System.Windows.Forms.Button       btnCerrar;
        private System.Windows.Forms.DataGridView dgvBitacora;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFecha;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUsuario;
        private System.Windows.Forms.DataGridViewTextBoxColumn colOperacion;
        private System.Windows.Forms.DataGridViewTextBoxColumn colModulo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDescripcion;
        private System.Windows.Forms.DataGridViewTextBoxColumn colExitoso;
        private System.Windows.Forms.Label        lblTotal;
    }
}
