namespace GUI
{
    partial class frmGestorPrendas
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
            this.dgvPrendas     = new System.Windows.Forms.DataGridView();
            this.lblNombre      = new System.Windows.Forms.Label();
            this.txtNombre      = new System.Windows.Forms.TextBox();
            this.lblColor       = new System.Windows.Forms.Label();
            this.txtColor       = new System.Windows.Forms.TextBox();
            this.lblTalla       = new System.Windows.Forms.Label();
            this.txtTalla       = new System.Windows.Forms.TextBox();
            this.lblTemporada   = new System.Windows.Forms.Label();
            this.cboTemporada   = new System.Windows.Forms.ComboBox();
            this.lblCategoria   = new System.Windows.Forms.Label();
            this.cboCategorias  = new System.Windows.Forms.ComboBox();
            this.lblDescripcion = new System.Windows.Forms.Label();
            this.btnNuevo       = new System.Windows.Forms.Button();
            this.btnGuardar     = new System.Windows.Forms.Button();
            this.btnEliminar    = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPrendas)).BeginInit();
            this.SuspendLayout();
            // dgvPrendas
            this.dgvPrendas.AllowUserToAddRows    = false;
            this.dgvPrendas.AllowUserToDeleteRows = false;
            this.dgvPrendas.Location              = new System.Drawing.Point(12, 12);
            this.dgvPrendas.MultiSelect           = false;
            this.dgvPrendas.Name                  = "dgvPrendas";
            this.dgvPrendas.ReadOnly              = true;
            this.dgvPrendas.SelectionMode         = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvPrendas.Size                  = new System.Drawing.Size(660, 180);
            this.dgvPrendas.SelectionChanged     += new System.EventHandler(this.dgvPrendas_SelectionChanged);
            // lblNombre
            this.lblNombre.AutoSize = true;
            this.lblNombre.Location = new System.Drawing.Point(12, 210);
            this.lblNombre.Text     = "Nombre:";
            this.txtNombre.Location = new System.Drawing.Point(110, 207);
            this.txtNombre.Name     = "txtNombre";
            this.txtNombre.Size     = new System.Drawing.Size(250, 22);
            // lblColor
            this.lblColor.AutoSize = true;
            this.lblColor.Location = new System.Drawing.Point(380, 210);
            this.lblColor.Text     = "Color:";
            this.txtColor.Location = new System.Drawing.Point(430, 207);
            this.txtColor.Name     = "txtColor";
            this.txtColor.Size     = new System.Drawing.Size(130, 22);
            // lblTalla
            this.lblTalla.AutoSize = true;
            this.lblTalla.Location = new System.Drawing.Point(12, 245);
            this.lblTalla.Text     = "Talla:";
            this.txtTalla.Location = new System.Drawing.Point(110, 242);
            this.txtTalla.Name     = "txtTalla";
            this.txtTalla.Size     = new System.Drawing.Size(80, 22);
            // lblTemporada
            this.lblTemporada.AutoSize = true;
            this.lblTemporada.Location = new System.Drawing.Point(210, 245);
            this.lblTemporada.Text     = "Temporada:";
            this.cboTemporada.Location = new System.Drawing.Point(300, 242);
            this.cboTemporada.Name     = "cboTemporada";
            this.cboTemporada.Size     = new System.Drawing.Size(130, 24);
            this.cboTemporada.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            // lblCategoria
            this.lblCategoria.AutoSize = true;
            this.lblCategoria.Location = new System.Drawing.Point(12, 280);
            this.lblCategoria.Text     = "Categoria:";
            this.cboCategorias.Location = new System.Drawing.Point(110, 277);
            this.cboCategorias.Name     = "cboCategorias";
            this.cboCategorias.Size     = new System.Drawing.Size(200, 24);
            this.cboCategorias.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            // lblDescripcion (muestra el resultado del Decorator)
            this.lblDescripcion.AutoSize = true;
            this.lblDescripcion.Location = new System.Drawing.Point(12, 315);
            this.lblDescripcion.Name     = "lblDescripcion";
            this.lblDescripcion.Size     = new System.Drawing.Size(640, 17);
            this.lblDescripcion.ForeColor = System.Drawing.Color.DarkBlue;
            this.lblDescripcion.Text     = string.Empty;
            // botones
            this.btnNuevo.Location    = new System.Drawing.Point(110, 350);
            this.btnNuevo.Name        = "btnNuevo";
            this.btnNuevo.Size        = new System.Drawing.Size(100, 30);
            this.btnNuevo.Text        = "Nuevo";
            this.btnNuevo.Click      += new System.EventHandler(this.btnNuevo_Click);
            this.btnGuardar.Location  = new System.Drawing.Point(230, 350);
            this.btnGuardar.Name      = "btnGuardar";
            this.btnGuardar.Size      = new System.Drawing.Size(100, 30);
            this.btnGuardar.Text      = "Guardar";
            this.btnGuardar.Click    += new System.EventHandler(this.btnGuardar_Click);
            this.btnEliminar.Location = new System.Drawing.Point(350, 350);
            this.btnEliminar.Name     = "btnEliminar";
            this.btnEliminar.Size     = new System.Drawing.Size(100, 30);
            this.btnEliminar.Text     = "Eliminar";
            this.btnEliminar.Click   += new System.EventHandler(this.btnEliminar_Click);
            // frmGestorPrendas
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(690, 400);
            this.Controls.Add(this.dgvPrendas);
            this.Controls.Add(this.lblNombre);
            this.Controls.Add(this.txtNombre);
            this.Controls.Add(this.lblColor);
            this.Controls.Add(this.txtColor);
            this.Controls.Add(this.lblTalla);
            this.Controls.Add(this.txtTalla);
            this.Controls.Add(this.lblTemporada);
            this.Controls.Add(this.cboTemporada);
            this.Controls.Add(this.lblCategoria);
            this.Controls.Add(this.cboCategorias);
            this.Controls.Add(this.lblDescripcion);
            this.Controls.Add(this.btnNuevo);
            this.Controls.Add(this.btnGuardar);
            this.Controls.Add(this.btnEliminar);
            this.Name = "frmGestorPrendas";
            this.Text = "Gestor de Prendas";
            ((System.ComponentModel.ISupportInitialize)(this.dgvPrendas)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.DataGridView dgvPrendas;
        private System.Windows.Forms.Label        lblNombre;
        private System.Windows.Forms.TextBox      txtNombre;
        private System.Windows.Forms.Label        lblColor;
        private System.Windows.Forms.TextBox      txtColor;
        private System.Windows.Forms.Label        lblTalla;
        private System.Windows.Forms.TextBox      txtTalla;
        private System.Windows.Forms.Label        lblTemporada;
        private System.Windows.Forms.ComboBox     cboTemporada;
        private System.Windows.Forms.Label        lblCategoria;
        private System.Windows.Forms.ComboBox     cboCategorias;
        private System.Windows.Forms.Label        lblDescripcion;
        private System.Windows.Forms.Button       btnNuevo;
        private System.Windows.Forms.Button       btnGuardar;
        private System.Windows.Forms.Button       btnEliminar;
    }
}
