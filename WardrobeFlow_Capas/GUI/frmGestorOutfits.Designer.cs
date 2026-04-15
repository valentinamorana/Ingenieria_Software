namespace GUI
{
    partial class frmGestorOutfits
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }
        private void InitializeComponent()
        {
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.pnlContent = new System.Windows.Forms.Panel();
            this.dgvOutfits = new System.Windows.Forms.DataGridView();
            this.pnlCampos = new System.Windows.Forms.Panel();
            this.lblCamposTitle = new System.Windows.Forms.Label();
            this.lblNombre = new System.Windows.Forms.Label();
            this.txtNombre = new System.Windows.Forms.TextBox();
            this.lblDescripcion = new System.Windows.Forms.Label();
            this.txtDescripcion = new System.Windows.Forms.TextBox();
            this.lblOcasion = new System.Windows.Forms.Label();
            this.cboOcasion = new System.Windows.Forms.ComboBox();
            this.lblTemporada = new System.Windows.Forms.Label();
            this.cboTemporada = new System.Windows.Forms.ComboBox();
            this.lblResumen = new System.Windows.Forms.Label();
            this.txtResumen = new System.Windows.Forms.TextBox();
            this.btnNuevo = new System.Windows.Forms.Button();
            this.btnEditar = new System.Windows.Forms.Button();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.btnEliminar = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvOutfits)).BeginInit();
            this.pnlContent.SuspendLayout();
            this.pnlHeader.SuspendLayout();
            this.pnlCampos.SuspendLayout();
            this.SuspendLayout();
            // pnlHeader
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(180, 35, 75);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Height = 58;
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Controls.Add(this.lblTitulo);
            // lblTitulo
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.White;
            this.lblTitulo.Location = new System.Drawing.Point(18, 18);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Text = "Gestor de Outfits";
            // pnlContent
            this.pnlContent.BackColor = System.Drawing.Color.FromArgb(255, 240, 248);
            this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.Padding = new System.Windows.Forms.Padding(14);
            this.pnlContent.Controls.Add(this.dgvOutfits);
            this.pnlContent.Controls.Add(this.lblResumen);
            this.pnlContent.Controls.Add(this.txtResumen);
            this.pnlContent.Controls.Add(this.pnlCampos);
            // dgvOutfits
            this.dgvOutfits.AllowUserToAddRows = false;
            this.dgvOutfits.AllowUserToDeleteRows = false;
            this.dgvOutfits.MultiSelect = false;
            this.dgvOutfits.ReadOnly = true;
            this.dgvOutfits.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvOutfits.Location = new System.Drawing.Point(14, 14);
            this.dgvOutfits.Name = "dgvOutfits";
            this.dgvOutfits.Size = new System.Drawing.Size(540, 270);
            this.dgvOutfits.TabIndex = 0;
            this.dgvOutfits.BackgroundColor = System.Drawing.Color.White;
            this.dgvOutfits.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvOutfits.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.dgvOutfits.GridColor = System.Drawing.Color.FromArgb(220, 170, 195);
            this.dgvOutfits.EnableHeadersVisualStyles = false;
            this.dgvOutfits.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dgvOutfits.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(180, 35, 75);
            this.dgvOutfits.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            this.dgvOutfits.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.dgvOutfits.ColumnHeadersDefaultCellStyle.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.dgvOutfits.ColumnHeadersHeight = 36;
            this.dgvOutfits.DefaultCellStyle.BackColor = System.Drawing.Color.White;
            this.dgvOutfits.DefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(35, 20, 30);
            this.dgvOutfits.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.dgvOutfits.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(232, 120, 160);
            this.dgvOutfits.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.White;
            this.dgvOutfits.DefaultCellStyle.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.dgvOutfits.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(255, 225, 238);
            this.dgvOutfits.RowHeadersVisible = false;
            this.dgvOutfits.RowTemplate.Height = 28;
            this.dgvOutfits.SelectionChanged += new System.EventHandler(this.dgvOutfits_SelectionChanged);
            // lblResumen
            this.lblResumen.AutoSize = true;
            this.lblResumen.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblResumen.ForeColor = System.Drawing.Color.FromArgb(180, 35, 75);
            this.lblResumen.Location = new System.Drawing.Point(14, 294);
            this.lblResumen.Name = "lblResumen";
            this.lblResumen.Text = "Descripcion enriquecida (Decorator):";
            // txtResumen
            this.txtResumen.BackColor = System.Drawing.Color.White;
            this.txtResumen.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtResumen.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtResumen.ForeColor = System.Drawing.Color.FromArgb(35, 20, 30);
            this.txtResumen.Location = new System.Drawing.Point(14, 315);
            this.txtResumen.Multiline = true;
            this.txtResumen.Name = "txtResumen";
            this.txtResumen.ReadOnly = true;
            this.txtResumen.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtResumen.Size = new System.Drawing.Size(540, 115);
            this.txtResumen.TabIndex = 1;
            // pnlCampos
            this.pnlCampos.BackColor = System.Drawing.Color.White;
            this.pnlCampos.Location = new System.Drawing.Point(570, 14);
            this.pnlCampos.Name = "pnlCampos";
            this.pnlCampos.Size = new System.Drawing.Size(230, 450);
            this.pnlCampos.Controls.Add(this.lblCamposTitle);
            this.pnlCampos.Controls.Add(this.lblNombre);
            this.pnlCampos.Controls.Add(this.txtNombre);
            this.pnlCampos.Controls.Add(this.lblDescripcion);
            this.pnlCampos.Controls.Add(this.txtDescripcion);
            this.pnlCampos.Controls.Add(this.lblOcasion);
            this.pnlCampos.Controls.Add(this.cboOcasion);
            this.pnlCampos.Controls.Add(this.lblTemporada);
            this.pnlCampos.Controls.Add(this.cboTemporada);
            this.pnlCampos.Controls.Add(this.btnNuevo);
            this.pnlCampos.Controls.Add(this.btnEditar);
            this.pnlCampos.Controls.Add(this.btnGuardar);
            this.pnlCampos.Controls.Add(this.btnEliminar);
            // lblCamposTitle
            this.lblCamposTitle.AutoSize = true;
            this.lblCamposTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblCamposTitle.ForeColor = System.Drawing.Color.FromArgb(180, 35, 75);
            this.lblCamposTitle.Location = new System.Drawing.Point(15, 12);
            this.lblCamposTitle.Name = "lblCamposTitle";
            this.lblCamposTitle.Text = "Datos de Outfit";
            // lblNombre
            this.lblNombre.AutoSize = true;
            this.lblNombre.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblNombre.ForeColor = System.Drawing.Color.FromArgb(150, 80, 105);
            this.lblNombre.Location = new System.Drawing.Point(15, 44);
            this.lblNombre.Name = "lblNombre";
            this.lblNombre.Text = "NOMBRE";
            // txtNombre
            this.txtNombre.BackColor = System.Drawing.Color.FromArgb(255, 240, 248);
            this.txtNombre.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtNombre.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtNombre.Location = new System.Drawing.Point(15, 62);
            this.txtNombre.Name = "txtNombre";
            this.txtNombre.Size = new System.Drawing.Size(200, 26);
            this.txtNombre.TabIndex = 2;
            // lblDescripcion
            this.lblDescripcion.AutoSize = true;
            this.lblDescripcion.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblDescripcion.ForeColor = System.Drawing.Color.FromArgb(150, 80, 105);
            this.lblDescripcion.Location = new System.Drawing.Point(15, 100);
            this.lblDescripcion.Name = "lblDescripcion";
            this.lblDescripcion.Text = "DESCRIPCION";
            // txtDescripcion
            this.txtDescripcion.BackColor = System.Drawing.Color.FromArgb(255, 240, 248);
            this.txtDescripcion.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtDescripcion.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtDescripcion.Location = new System.Drawing.Point(15, 118);
            this.txtDescripcion.Name = "txtDescripcion";
            this.txtDescripcion.Size = new System.Drawing.Size(200, 26);
            this.txtDescripcion.TabIndex = 3;
            // lblOcasion
            this.lblOcasion.AutoSize = true;
            this.lblOcasion.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblOcasion.ForeColor = System.Drawing.Color.FromArgb(150, 80, 105);
            this.lblOcasion.Location = new System.Drawing.Point(15, 156);
            this.lblOcasion.Name = "lblOcasion";
            this.lblOcasion.Text = "OCASION";
            // cboOcasion
            this.cboOcasion.BackColor = System.Drawing.Color.FromArgb(255, 240, 248);
            this.cboOcasion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboOcasion.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.cboOcasion.Location = new System.Drawing.Point(15, 174);
            this.cboOcasion.Name = "cboOcasion";
            this.cboOcasion.Size = new System.Drawing.Size(200, 26);
            this.cboOcasion.TabIndex = 4;
            // lblTemporada
            this.lblTemporada.AutoSize = true;
            this.lblTemporada.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblTemporada.ForeColor = System.Drawing.Color.FromArgb(150, 80, 105);
            this.lblTemporada.Location = new System.Drawing.Point(15, 212);
            this.lblTemporada.Name = "lblTemporada";
            this.lblTemporada.Text = "TEMPORADA";
            // cboTemporada
            this.cboTemporada.BackColor = System.Drawing.Color.FromArgb(255, 240, 248);
            this.cboTemporada.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTemporada.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.cboTemporada.Location = new System.Drawing.Point(15, 230);
            this.cboTemporada.Name = "cboTemporada";
            this.cboTemporada.Size = new System.Drawing.Size(200, 26);
            this.cboTemporada.TabIndex = 5;
            // btnNuevo
            this.btnNuevo.BackColor = System.Drawing.Color.White;
            this.btnNuevo.ForeColor = System.Drawing.Color.FromArgb(180, 35, 75);
            this.btnNuevo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNuevo.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(180, 35, 75);
            this.btnNuevo.FlatAppearance.BorderSize = 1;
            this.btnNuevo.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnNuevo.Location = new System.Drawing.Point(15, 270);
            this.btnNuevo.Name = "btnNuevo";
            this.btnNuevo.Size = new System.Drawing.Size(200, 30);
            this.btnNuevo.TabIndex = 6;
            this.btnNuevo.Text = "Nuevo";
            this.btnNuevo.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNuevo.Click += new System.EventHandler(this.btnNuevo_Click);
            // btnEditar
            this.btnEditar.BackColor = System.Drawing.Color.FromArgb(130, 80, 100);
            this.btnEditar.ForeColor = System.Drawing.Color.White;
            this.btnEditar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEditar.FlatAppearance.BorderSize = 0;
            this.btnEditar.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnEditar.Location = new System.Drawing.Point(15, 306);
            this.btnEditar.Name = "btnEditar";
            this.btnEditar.Size = new System.Drawing.Size(200, 30);
            this.btnEditar.TabIndex = 7;
            this.btnEditar.Text = "Editar";
            this.btnEditar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnEditar.Click += new System.EventHandler(this.btnEditar_Click);
            // btnGuardar
            this.btnGuardar.BackColor = System.Drawing.Color.FromArgb(180, 35, 75);
            this.btnGuardar.ForeColor = System.Drawing.Color.White;
            this.btnGuardar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGuardar.FlatAppearance.BorderSize = 0;
            this.btnGuardar.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnGuardar.Location = new System.Drawing.Point(15, 342);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(200, 30);
            this.btnGuardar.TabIndex = 8;
            this.btnGuardar.Text = "Guardar";
            this.btnGuardar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);
            // btnEliminar
            this.btnEliminar.BackColor = System.Drawing.Color.FromArgb(90, 70, 80);
            this.btnEliminar.ForeColor = System.Drawing.Color.White;
            this.btnEliminar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEliminar.FlatAppearance.BorderSize = 0;
            this.btnEliminar.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnEliminar.Location = new System.Drawing.Point(15, 378);
            this.btnEliminar.Name = "btnEliminar";
            this.btnEliminar.Size = new System.Drawing.Size(200, 30);
            this.btnEliminar.TabIndex = 9;
            this.btnEliminar.Text = "Eliminar";
            this.btnEliminar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnEliminar.Click += new System.EventHandler(this.btnEliminar_Click);
            // frmGestorOutfits
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(255, 240, 248);
            this.ClientSize = new System.Drawing.Size(818, 536);
            this.Controls.Add(this.pnlContent);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.Name = "frmGestorOutfits";
            this.Text = "WardrobeFlow - Outfits";
            ((System.ComponentModel.ISupportInitialize)(this.dgvOutfits)).EndInit();
            this.pnlContent.ResumeLayout(false);
            this.pnlContent.PerformLayout();
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.pnlCampos.ResumeLayout(false);
            this.pnlCampos.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Panel pnlContent;
        private System.Windows.Forms.Panel pnlCampos;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblCamposTitle;
        private System.Windows.Forms.DataGridView dgvOutfits;
        private System.Windows.Forms.Label lblNombre;
        private System.Windows.Forms.TextBox txtNombre;
        private System.Windows.Forms.Label lblDescripcion;
        private System.Windows.Forms.TextBox txtDescripcion;
        private System.Windows.Forms.Label lblOcasion;
        private System.Windows.Forms.ComboBox cboOcasion;
        private System.Windows.Forms.Label lblTemporada;
        private System.Windows.Forms.ComboBox cboTemporada;
        private System.Windows.Forms.Label lblResumen;
        private System.Windows.Forms.TextBox txtResumen;
        private System.Windows.Forms.Button btnNuevo;
        private System.Windows.Forms.Button btnEditar;
        private System.Windows.Forms.Button btnGuardar;
        private System.Windows.Forms.Button btnEliminar;
    }
}
