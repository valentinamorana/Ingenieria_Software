namespace GUI
{
    partial class frmGestorUsuarios
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
            this.dgvUsuarios  = new System.Windows.Forms.DataGridView();
            this.lblNombre    = new System.Windows.Forms.Label();
            this.txtNombre    = new System.Windows.Forms.TextBox();
            this.lblDocumento = new System.Windows.Forms.Label();
            this.txtDocumento = new System.Windows.Forms.TextBox();
            this.lblCorreo    = new System.Windows.Forms.Label();
            this.txtCorreo    = new System.Windows.Forms.TextBox();
            this.lblPassword  = new System.Windows.Forms.Label();
            this.txtPassword  = new System.Windows.Forms.TextBox();
            this.lblRol       = new System.Windows.Forms.Label();
            this.cboRol       = new System.Windows.Forms.ComboBox();
            this.btnNuevo     = new System.Windows.Forms.Button();
            this.btnGuardar   = new System.Windows.Forms.Button();
            this.btnEliminar  = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUsuarios)).BeginInit();
            this.SuspendLayout();
            // dgvUsuarios
            this.dgvUsuarios.AllowUserToAddRows    = false;
            this.dgvUsuarios.AllowUserToDeleteRows = false;
            this.dgvUsuarios.Location              = new System.Drawing.Point(12, 12);
            this.dgvUsuarios.MultiSelect           = false;
            this.dgvUsuarios.Name                  = "dgvUsuarios";
            this.dgvUsuarios.ReadOnly              = true;
            this.dgvUsuarios.SelectionMode         = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvUsuarios.Size                  = new System.Drawing.Size(560, 160);
            this.dgvUsuarios.SelectionChanged     += new System.EventHandler(this.dgvUsuarios_SelectionChanged);
            // campos
            this.lblNombre.AutoSize = true;
            this.lblNombre.Location = new System.Drawing.Point(12, 190);
            this.lblNombre.Text     = "Nombre:";
            this.txtNombre.Location = new System.Drawing.Point(120, 187);
            this.txtNombre.Name     = "txtNombre";
            this.txtNombre.Size     = new System.Drawing.Size(240, 22);
            this.lblDocumento.AutoSize = true;
            this.lblDocumento.Location = new System.Drawing.Point(12, 225);
            this.lblDocumento.Text     = "Documento:";
            this.txtDocumento.Location = new System.Drawing.Point(120, 222);
            this.txtDocumento.Name     = "txtDocumento";
            this.txtDocumento.Size     = new System.Drawing.Size(160, 22);
            this.lblCorreo.AutoSize = true;
            this.lblCorreo.Location = new System.Drawing.Point(12, 260);
            this.lblCorreo.Text     = "Correo:";
            this.txtCorreo.Location = new System.Drawing.Point(120, 257);
            this.txtCorreo.Name     = "txtCorreo";
            this.txtCorreo.Size     = new System.Drawing.Size(240, 22);
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(12, 295);
            this.lblPassword.Text     = "Contrasena:";
            this.txtPassword.Location     = new System.Drawing.Point(120, 292);
            this.txtPassword.Name         = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size         = new System.Drawing.Size(160, 22);
            this.lblRol.AutoSize = true;
            this.lblRol.Location = new System.Drawing.Point(300, 225);
            this.lblRol.Text     = "Rol:";
            this.cboRol.Location     = new System.Drawing.Point(340, 222);
            this.cboRol.Name         = "cboRol";
            this.cboRol.Size         = new System.Drawing.Size(150, 24);
            this.cboRol.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            // botones
            this.btnNuevo.Location    = new System.Drawing.Point(120, 330);
            this.btnNuevo.Name        = "btnNuevo";
            this.btnNuevo.Size        = new System.Drawing.Size(100, 30);
            this.btnNuevo.Text        = "Nuevo";
            this.btnNuevo.Click      += new System.EventHandler(this.btnNuevo_Click);
            this.btnGuardar.Location  = new System.Drawing.Point(240, 330);
            this.btnGuardar.Name      = "btnGuardar";
            this.btnGuardar.Size      = new System.Drawing.Size(100, 30);
            this.btnGuardar.Text      = "Guardar";
            this.btnGuardar.Click    += new System.EventHandler(this.btnGuardar_Click);
            this.btnEliminar.Location = new System.Drawing.Point(360, 330);
            this.btnEliminar.Name     = "btnEliminar";
            this.btnEliminar.Size     = new System.Drawing.Size(100, 30);
            this.btnEliminar.Text     = "Eliminar";
            this.btnEliminar.Click   += new System.EventHandler(this.btnEliminar_Click);
            // form
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(590, 385);
            this.Controls.Add(this.dgvUsuarios);
            this.Controls.Add(this.lblNombre);
            this.Controls.Add(this.txtNombre);
            this.Controls.Add(this.lblDocumento);
            this.Controls.Add(this.txtDocumento);
            this.Controls.Add(this.lblCorreo);
            this.Controls.Add(this.txtCorreo);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.lblRol);
            this.Controls.Add(this.cboRol);
            this.Controls.Add(this.btnNuevo);
            this.Controls.Add(this.btnGuardar);
            this.Controls.Add(this.btnEliminar);
            this.Name = "frmGestorUsuarios";
            this.Text = "Gestor de Usuarios";
            ((System.ComponentModel.ISupportInitialize)(this.dgvUsuarios)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.DataGridView dgvUsuarios;
        private System.Windows.Forms.Label        lblNombre;
        private System.Windows.Forms.TextBox      txtNombre;
        private System.Windows.Forms.Label        lblDocumento;
        private System.Windows.Forms.TextBox      txtDocumento;
        private System.Windows.Forms.Label        lblCorreo;
        private System.Windows.Forms.TextBox      txtCorreo;
        private System.Windows.Forms.Label        lblPassword;
        private System.Windows.Forms.TextBox      txtPassword;
        private System.Windows.Forms.Label        lblRol;
        private System.Windows.Forms.ComboBox     cboRol;
        private System.Windows.Forms.Button       btnNuevo;
        private System.Windows.Forms.Button       btnGuardar;
        private System.Windows.Forms.Button       btnEliminar;
    }
}
