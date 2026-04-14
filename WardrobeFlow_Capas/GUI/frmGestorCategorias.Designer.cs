namespace GUI
{
    partial class frmGestorCategorias
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
            this.dgvCategorias  = new System.Windows.Forms.DataGridView();
            this.lblNombre      = new System.Windows.Forms.Label();
            this.txtNombre      = new System.Windows.Forms.TextBox();
            this.lblDescripcion = new System.Windows.Forms.Label();
            this.txtDescripcion = new System.Windows.Forms.TextBox();
            this.btnNuevo       = new System.Windows.Forms.Button();
            this.btnGuardar     = new System.Windows.Forms.Button();
            this.btnEliminar    = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCategorias)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvCategorias
            // 
            this.dgvCategorias.AllowUserToAddRows    = false;
            this.dgvCategorias.AllowUserToDeleteRows = false;
            this.dgvCategorias.Location              = new System.Drawing.Point(12, 12);
            this.dgvCategorias.MultiSelect           = false;
            this.dgvCategorias.Name                  = "dgvCategorias";
            this.dgvCategorias.ReadOnly              = true;
            this.dgvCategorias.SelectionMode         = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvCategorias.Size                  = new System.Drawing.Size(560, 200);
            this.dgvCategorias.TabIndex              = 0;
            this.dgvCategorias.SelectionChanged     += new System.EventHandler(this.dgvCategorias_SelectionChanged);
            // 
            // lblNombre
            // 
            this.lblNombre.AutoSize = true;
            this.lblNombre.Location = new System.Drawing.Point(12, 230);
            this.lblNombre.Name     = "lblNombre";
            this.lblNombre.Text     = "Nombre:";
            // 
            // txtNombre
            // 
            this.txtNombre.Location = new System.Drawing.Point(110, 227);
            this.txtNombre.Name     = "txtNombre";
            this.txtNombre.Size     = new System.Drawing.Size(460, 22);
            this.txtNombre.TabIndex = 1;
            // 
            // lblDescripcion
            // 
            this.lblDescripcion.AutoSize = true;
            this.lblDescripcion.Location = new System.Drawing.Point(12, 265);
            this.lblDescripcion.Name     = "lblDescripcion";
            this.lblDescripcion.Text     = "Descripcion:";
            // 
            // txtDescripcion
            // 
            this.txtDescripcion.Location = new System.Drawing.Point(110, 262);
            this.txtDescripcion.Name     = "txtDescripcion";
            this.txtDescripcion.Size     = new System.Drawing.Size(460, 22);
            this.txtDescripcion.TabIndex = 2;
            // 
            // btnNuevo
            // 
            this.btnNuevo.Location = new System.Drawing.Point(110, 305);
            this.btnNuevo.Name     = "btnNuevo";
            this.btnNuevo.Size     = new System.Drawing.Size(100, 30);
            this.btnNuevo.Text     = "Nuevo";
            this.btnNuevo.Click   += new System.EventHandler(this.btnNuevo_Click);
            // 
            // btnGuardar
            // 
            this.btnGuardar.Location = new System.Drawing.Point(230, 305);
            this.btnGuardar.Name     = "btnGuardar";
            this.btnGuardar.Size     = new System.Drawing.Size(100, 30);
            this.btnGuardar.Text     = "Guardar";
            this.btnGuardar.Click   += new System.EventHandler(this.btnGuardar_Click);
            // 
            // btnEliminar
            // 
            this.btnEliminar.Location = new System.Drawing.Point(350, 305);
            this.btnEliminar.Name     = "btnEliminar";
            this.btnEliminar.Size     = new System.Drawing.Size(100, 30);
            this.btnEliminar.Text     = "Eliminar";
            this.btnEliminar.Click   += new System.EventHandler(this.btnEliminar_Click);
            // 
            // frmGestorCategorias
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(590, 360);
            this.Controls.Add(this.dgvCategorias);
            this.Controls.Add(this.lblNombre);
            this.Controls.Add(this.txtNombre);
            this.Controls.Add(this.lblDescripcion);
            this.Controls.Add(this.txtDescripcion);
            this.Controls.Add(this.btnNuevo);
            this.Controls.Add(this.btnGuardar);
            this.Controls.Add(this.btnEliminar);
            this.Name = "frmGestorCategorias";
            this.Text = "Gestor de Categorias";
            ((System.ComponentModel.ISupportInitialize)(this.dgvCategorias)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.DataGridView dgvCategorias;
        private System.Windows.Forms.Label        lblNombre;
        private System.Windows.Forms.TextBox      txtNombre;
        private System.Windows.Forms.Label        lblDescripcion;
        private System.Windows.Forms.TextBox      txtDescripcion;
        private System.Windows.Forms.Button       btnNuevo;
        private System.Windows.Forms.Button       btnGuardar;
        private System.Windows.Forms.Button       btnEliminar;
    }
}
