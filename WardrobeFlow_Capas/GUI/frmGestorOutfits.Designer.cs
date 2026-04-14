namespace GUI
{
    partial class frmGestorOutfits
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
            this.dgvOutfits     = new System.Windows.Forms.DataGridView();
            this.lblNombre      = new System.Windows.Forms.Label();
            this.txtNombre      = new System.Windows.Forms.TextBox();
            this.lblDescripcion = new System.Windows.Forms.Label();
            this.txtDescripcion = new System.Windows.Forms.TextBox();
            this.lblOcasion     = new System.Windows.Forms.Label();
            this.cboOcasion     = new System.Windows.Forms.ComboBox();
            this.lblTemporada   = new System.Windows.Forms.Label();
            this.cboTemporada   = new System.Windows.Forms.ComboBox();
            this.lblResumen     = new System.Windows.Forms.Label();
            this.txtResumen     = new System.Windows.Forms.TextBox();
            this.btnNuevo       = new System.Windows.Forms.Button();
            this.btnGuardar     = new System.Windows.Forms.Button();
            this.btnEliminar    = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvOutfits)).BeginInit();
            this.SuspendLayout();
            // dgvOutfits
            this.dgvOutfits.AllowUserToAddRows    = false;
            this.dgvOutfits.AllowUserToDeleteRows = false;
            this.dgvOutfits.Location              = new System.Drawing.Point(12, 12);
            this.dgvOutfits.MultiSelect           = false;
            this.dgvOutfits.Name                  = "dgvOutfits";
            this.dgvOutfits.ReadOnly              = true;
            this.dgvOutfits.SelectionMode         = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvOutfits.Size                  = new System.Drawing.Size(660, 160);
            this.dgvOutfits.SelectionChanged     += new System.EventHandler(this.dgvOutfits_SelectionChanged);
            // lblNombre
            this.lblNombre.AutoSize = true;
            this.lblNombre.Location = new System.Drawing.Point(12, 190);
            this.lblNombre.Text     = "Nombre:";
            this.txtNombre.Location = new System.Drawing.Point(110, 187);
            this.txtNombre.Name     = "txtNombre";
            this.txtNombre.Size     = new System.Drawing.Size(250, 22);
            // lblDescripcion
            this.lblDescripcion.AutoSize = true;
            this.lblDescripcion.Location = new System.Drawing.Point(12, 225);
            this.lblDescripcion.Text     = "Descripcion:";
            this.txtDescripcion.Location = new System.Drawing.Point(110, 222);
            this.txtDescripcion.Name     = "txtDescripcion";
            this.txtDescripcion.Size     = new System.Drawing.Size(250, 22);
            // lblOcasion
            this.lblOcasion.AutoSize = true;
            this.lblOcasion.Location = new System.Drawing.Point(380, 190);
            this.lblOcasion.Text     = "Ocasion:";
            this.cboOcasion.Location = new System.Drawing.Point(450, 187);
            this.cboOcasion.Name     = "cboOcasion";
            this.cboOcasion.Size     = new System.Drawing.Size(150, 24);
            this.cboOcasion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            // lblTemporada
            this.lblTemporada.AutoSize = true;
            this.lblTemporada.Location = new System.Drawing.Point(380, 225);
            this.lblTemporada.Text     = "Temporada:";
            this.cboTemporada.Location = new System.Drawing.Point(465, 222);
            this.cboTemporada.Name     = "cboTemporada";
            this.cboTemporada.Size     = new System.Drawing.Size(130, 24);
            this.cboTemporada.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            // lblResumen (resultado del Decorator)
            this.lblResumen.AutoSize = true;
            this.lblResumen.Location = new System.Drawing.Point(12, 260);
            this.lblResumen.Text     = "Resumen (Decorator):";
            this.lblResumen.ForeColor = System.Drawing.Color.DarkGreen;
            this.txtResumen.Location  = new System.Drawing.Point(12, 280);
            this.txtResumen.Multiline = true;
            this.txtResumen.Name      = "txtResumen";
            this.txtResumen.ReadOnly  = true;
            this.txtResumen.Size      = new System.Drawing.Size(660, 100);
            this.txtResumen.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtResumen.BackColor  = System.Drawing.Color.LightYellow;
            // botones
            this.btnNuevo.Location    = new System.Drawing.Point(110, 395);
            this.btnNuevo.Name        = "btnNuevo";
            this.btnNuevo.Size        = new System.Drawing.Size(100, 30);
            this.btnNuevo.Text        = "Nuevo";
            this.btnNuevo.Click      += new System.EventHandler(this.btnNuevo_Click);
            this.btnGuardar.Location  = new System.Drawing.Point(230, 395);
            this.btnGuardar.Name      = "btnGuardar";
            this.btnGuardar.Size      = new System.Drawing.Size(100, 30);
            this.btnGuardar.Text      = "Guardar";
            this.btnGuardar.Click    += new System.EventHandler(this.btnGuardar_Click);
            this.btnEliminar.Location = new System.Drawing.Point(350, 395);
            this.btnEliminar.Name     = "btnEliminar";
            this.btnEliminar.Size     = new System.Drawing.Size(100, 30);
            this.btnEliminar.Text     = "Eliminar";
            this.btnEliminar.Click   += new System.EventHandler(this.btnEliminar_Click);
            // frmGestorOutfits
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(690, 445);
            this.Controls.Add(this.dgvOutfits);
            this.Controls.Add(this.lblNombre);
            this.Controls.Add(this.txtNombre);
            this.Controls.Add(this.lblDescripcion);
            this.Controls.Add(this.txtDescripcion);
            this.Controls.Add(this.lblOcasion);
            this.Controls.Add(this.cboOcasion);
            this.Controls.Add(this.lblTemporada);
            this.Controls.Add(this.cboTemporada);
            this.Controls.Add(this.lblResumen);
            this.Controls.Add(this.txtResumen);
            this.Controls.Add(this.btnNuevo);
            this.Controls.Add(this.btnGuardar);
            this.Controls.Add(this.btnEliminar);
            this.Name = "frmGestorOutfits";
            this.Text = "Gestor de Outfits";
            ((System.ComponentModel.ISupportInitialize)(this.dgvOutfits)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.DataGridView dgvOutfits;
        private System.Windows.Forms.Label        lblNombre;
        private System.Windows.Forms.TextBox      txtNombre;
        private System.Windows.Forms.Label        lblDescripcion;
        private System.Windows.Forms.TextBox      txtDescripcion;
        private System.Windows.Forms.Label        lblOcasion;
        private System.Windows.Forms.ComboBox     cboOcasion;
        private System.Windows.Forms.Label        lblTemporada;
        private System.Windows.Forms.ComboBox     cboTemporada;
        private System.Windows.Forms.Label        lblResumen;
        private System.Windows.Forms.TextBox      txtResumen;
        private System.Windows.Forms.Button       btnNuevo;
        private System.Windows.Forms.Button       btnGuardar;
        private System.Windows.Forms.Button       btnEliminar;
    }
}
