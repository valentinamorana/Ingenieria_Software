namespace GUI
{
    partial class frmGestorCategorias
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }
        private void InitializeComponent()
        {
            this.pnlHeader      = new System.Windows.Forms.Panel();
            this.lblTitulo      = new System.Windows.Forms.Label();
            this.pnlContent     = new System.Windows.Forms.Panel();
            this.dgvCategorias  = new System.Windows.Forms.DataGridView();
            this.pnlCampos      = new System.Windows.Forms.Panel();
            this.lblCamposTitle = new System.Windows.Forms.Label();
            this.lblNombre      = new System.Windows.Forms.Label();
            this.txtNombre      = new System.Windows.Forms.TextBox();
            this.lblDescripcion = new System.Windows.Forms.Label();
            this.txtDescripcion = new System.Windows.Forms.TextBox();
            this.btnNuevo       = new System.Windows.Forms.Button();
            this.btnGuardar     = new System.Windows.Forms.Button();
            this.btnEliminar    = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCategorias)).BeginInit();
            this.pnlContent.SuspendLayout();
            this.pnlHeader.SuspendLayout();
            this.pnlCampos.SuspendLayout();
            this.SuspendLayout();
            // pnlHeader
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(180, 35, 75);
            this.pnlHeader.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Height    = 58;
            this.pnlHeader.Name      = "pnlHeader";
            this.pnlHeader.Controls.Add(this.lblTitulo);
            // lblTitulo
            this.lblTitulo.AutoSize  = true;
            this.lblTitulo.Font      = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.White;
            this.lblTitulo.Location  = new System.Drawing.Point(18, 18);
            this.lblTitulo.Name      = "lblTitulo";
            this.lblTitulo.Text      = "Gestor de Categorias";
            // pnlContent
            this.pnlContent.BackColor = System.Drawing.Color.FromArgb(255, 240, 248);
            this.pnlContent.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Name      = "pnlContent";
            this.pnlContent.Padding   = new System.Windows.Forms.Padding(14);
            this.pnlContent.Controls.Add(this.dgvCategorias);
            this.pnlContent.Controls.Add(this.pnlCampos);
            // dgvCategorias
            this.dgvCategorias.AllowUserToAddRows    = false;
            this.dgvCategorias.AllowUserToDeleteRows = false;
            this.dgvCategorias.MultiSelect           = false;
            this.dgvCategorias.ReadOnly              = true;
            this.dgvCategorias.SelectionMode         = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvCategorias.Location              = new System.Drawing.Point(14, 14);
            this.dgvCategorias.Name                  = "dgvCategorias";
            this.dgvCategorias.Size                  = new System.Drawing.Size(540, 450);
            this.dgvCategorias.TabIndex              = 0;
            this.dgvCategorias.BackgroundColor                     = System.Drawing.Color.White;
            this.dgvCategorias.BorderStyle                         = System.Windows.Forms.BorderStyle.None;
            this.dgvCategorias.CellBorderStyle                     = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.dgvCategorias.GridColor                           = System.Drawing.Color.FromArgb(220, 170, 195);
            this.dgvCategorias.EnableHeadersVisualStyles           = false;
            this.dgvCategorias.ColumnHeadersBorderStyle            = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dgvCategorias.ColumnHeadersDefaultCellStyle.BackColor  = System.Drawing.Color.FromArgb(180, 35, 75);
            this.dgvCategorias.ColumnHeadersDefaultCellStyle.ForeColor  = System.Drawing.Color.White;
            this.dgvCategorias.ColumnHeadersDefaultCellStyle.Font       = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.dgvCategorias.ColumnHeadersDefaultCellStyle.Padding    = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.dgvCategorias.ColumnHeadersHeight                 = 36;
            this.dgvCategorias.DefaultCellStyle.BackColor          = System.Drawing.Color.White;
            this.dgvCategorias.DefaultCellStyle.ForeColor          = System.Drawing.Color.FromArgb(35, 20, 30);
            this.dgvCategorias.DefaultCellStyle.Font               = new System.Drawing.Font("Segoe UI", 9.5F);
            this.dgvCategorias.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(232, 120, 160);
            this.dgvCategorias.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.White;
            this.dgvCategorias.DefaultCellStyle.Padding            = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.dgvCategorias.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(255, 225, 238);
            this.dgvCategorias.RowHeadersVisible                   = false;
            this.dgvCategorias.RowTemplate.Height                  = 28;
            this.dgvCategorias.SelectionChanged += new System.EventHandler(this.dgvCategorias_SelectionChanged);
            // pnlCampos
            this.pnlCampos.BackColor = System.Drawing.Color.White;
            this.pnlCampos.Location  = new System.Drawing.Point(570, 14);
            this.pnlCampos.Name      = "pnlCampos";
            this.pnlCampos.Size      = new System.Drawing.Size(230, 450);
            this.pnlCampos.Controls.Add(this.lblCamposTitle);
            this.pnlCampos.Controls.Add(this.lblNombre);
            this.pnlCampos.Controls.Add(this.txtNombre);
            this.pnlCampos.Controls.Add(this.lblDescripcion);
            this.pnlCampos.Controls.Add(this.txtDescripcion);
            this.pnlCampos.Controls.Add(this.btnNuevo);
            this.pnlCampos.Controls.Add(this.btnGuardar);
            this.pnlCampos.Controls.Add(this.btnEliminar);
            // lblCamposTitle
            this.lblCamposTitle.AutoSize  = true;
            this.lblCamposTitle.Font      = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblCamposTitle.ForeColor = System.Drawing.Color.FromArgb(180, 35, 75);
            this.lblCamposTitle.Location  = new System.Drawing.Point(15, 18);
            this.lblCamposTitle.Name      = "lblCamposTitle";
            this.lblCamposTitle.Text      = "Datos de Categoria";
            // lblNombre
            this.lblNombre.AutoSize  = true;
            this.lblNombre.Font      = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblNombre.ForeColor = System.Drawing.Color.FromArgb(150, 80, 105);
            this.lblNombre.Location  = new System.Drawing.Point(15, 58);
            this.lblNombre.Name      = "lblNombre";
            this.lblNombre.Text      = "NOMBRE";
            // txtNombre
            this.txtNombre.BackColor    = System.Drawing.Color.FromArgb(255, 240, 248);
            this.txtNombre.BorderStyle  = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtNombre.Font         = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtNombre.ForeColor    = System.Drawing.Color.FromArgb(35, 20, 30);
            this.txtNombre.Location     = new System.Drawing.Point(15, 78);
            this.txtNombre.Name         = "txtNombre";
            this.txtNombre.Size         = new System.Drawing.Size(200, 30);
            this.txtNombre.TabIndex     = 1;
            // lblDescripcion
            this.lblDescripcion.AutoSize  = true;
            this.lblDescripcion.Font      = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblDescripcion.ForeColor = System.Drawing.Color.FromArgb(150, 80, 105);
            this.lblDescripcion.Location  = new System.Drawing.Point(15, 122);
            this.lblDescripcion.Name      = "lblDescripcion";
            this.lblDescripcion.Text      = "DESCRIPCION";
            // txtDescripcion
            this.txtDescripcion.BackColor    = System.Drawing.Color.FromArgb(255, 240, 248);
            this.txtDescripcion.BorderStyle  = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtDescripcion.Font         = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtDescripcion.ForeColor    = System.Drawing.Color.FromArgb(35, 20, 30);
            this.txtDescripcion.Location     = new System.Drawing.Point(15, 142);
            this.txtDescripcion.Name         = "txtDescripcion";
            this.txtDescripcion.Size         = new System.Drawing.Size(200, 30);
            this.txtDescripcion.TabIndex     = 2;
            // btnNuevo
            this.btnNuevo.BackColor    = System.Drawing.Color.White;
            this.btnNuevo.ForeColor    = System.Drawing.Color.FromArgb(180, 35, 75);
            this.btnNuevo.FlatStyle    = System.Windows.Forms.FlatStyle.Flat;
            this.btnNuevo.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(180, 35, 75);
            this.btnNuevo.FlatAppearance.BorderSize  = 1;
            this.btnNuevo.Font         = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnNuevo.Location     = new System.Drawing.Point(15, 195);
            this.btnNuevo.Name         = "btnNuevo";
            this.btnNuevo.Size         = new System.Drawing.Size(200, 36);
            this.btnNuevo.TabIndex     = 3;
            this.btnNuevo.Text         = "Nuevo";
            this.btnNuevo.Cursor       = System.Windows.Forms.Cursors.Hand;
            this.btnNuevo.Click       += new System.EventHandler(this.btnNuevo_Click);
            // btnGuardar
            this.btnGuardar.BackColor    = System.Drawing.Color.FromArgb(180, 35, 75);
            this.btnGuardar.ForeColor    = System.Drawing.Color.White;
            this.btnGuardar.FlatStyle    = System.Windows.Forms.FlatStyle.Flat;
            this.btnGuardar.FlatAppearance.BorderSize = 0;
            this.btnGuardar.Font         = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnGuardar.Location     = new System.Drawing.Point(15, 241);
            this.btnGuardar.Name         = "btnGuardar";
            this.btnGuardar.Size         = new System.Drawing.Size(200, 36);
            this.btnGuardar.TabIndex     = 4;
            this.btnGuardar.Text         = "Guardar";
            this.btnGuardar.Cursor       = System.Windows.Forms.Cursors.Hand;
            this.btnGuardar.Click       += new System.EventHandler(this.btnGuardar_Click);
            // btnEliminar
            this.btnEliminar.BackColor    = System.Drawing.Color.FromArgb(90, 70, 80);
            this.btnEliminar.ForeColor    = System.Drawing.Color.White;
            this.btnEliminar.FlatStyle    = System.Windows.Forms.FlatStyle.Flat;
            this.btnEliminar.FlatAppearance.BorderSize = 0;
            this.btnEliminar.Font         = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnEliminar.Location     = new System.Drawing.Point(15, 287);
            this.btnEliminar.Name         = "btnEliminar";
            this.btnEliminar.Size         = new System.Drawing.Size(200, 36);
            this.btnEliminar.TabIndex     = 5;
            this.btnEliminar.Text         = "Eliminar";
            this.btnEliminar.Cursor       = System.Windows.Forms.Cursors.Hand;
            this.btnEliminar.Click       += new System.EventHandler(this.btnEliminar_Click);
            // frmGestorCategorias
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor           = System.Drawing.Color.FromArgb(255, 240, 248);
            this.ClientSize          = new System.Drawing.Size(818, 536);
            this.Controls.Add(this.pnlContent);
            this.Controls.Add(this.pnlHeader);
            this.Font                = new System.Drawing.Font("Segoe UI", 9.5F);
            this.Name                = "frmGestorCategorias";
            this.Text                = "WardrobeFlow - Categorias";
            ((System.ComponentModel.ISupportInitialize)(this.dgvCategorias)).EndInit();
            this.pnlContent.ResumeLayout(false);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.pnlCampos.ResumeLayout(false);
            this.pnlCampos.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel       pnlHeader;
        private System.Windows.Forms.Panel       pnlContent;
        private System.Windows.Forms.Panel       pnlCampos;
        private System.Windows.Forms.Label       lblTitulo;
        private System.Windows.Forms.Label       lblCamposTitle;
        private System.Windows.Forms.DataGridView dgvCategorias;
        private System.Windows.Forms.Label       lblNombre;
        private System.Windows.Forms.TextBox     txtNombre;
        private System.Windows.Forms.Label       lblDescripcion;
        private System.Windows.Forms.TextBox     txtDescripcion;
        private System.Windows.Forms.Button      btnNuevo;
        private System.Windows.Forms.Button      btnGuardar;
        private System.Windows.Forms.Button      btnEliminar;
    }
}
