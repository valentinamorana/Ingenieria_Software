namespace GUI
{
    partial class PrendaForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.lblNombre = new System.Windows.Forms.Label();
            this.txtNombre = new System.Windows.Forms.TextBox();
            this.lblDescripcion = new System.Windows.Forms.Label();
            this.txtDescripcion = new System.Windows.Forms.TextBox();
            this.lblTalle = new System.Windows.Forms.Label();
            this.cmbTalle = new System.Windows.Forms.ComboBox();
            this.lblColor = new System.Windows.Forms.Label();
            this.txtColor = new System.Windows.Forms.TextBox();
            this.lblCategoria = new System.Windows.Forms.Label();
            this.cmbCategoria = new System.Windows.Forms.ComboBox();
            this.lblMensaje = new System.Windows.Forms.Label();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblNombre
            // 
            this.lblNombre.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblNombre.Location = new System.Drawing.Point(16, 16);
            this.lblNombre.Name = "lblNombre";
            this.lblNombre.Size = new System.Drawing.Size(360, 18);
            this.lblNombre.TabIndex = 0;
            this.lblNombre.Text = "Nombre *";
            // 
            // txtNombre
            // 
            this.txtNombre.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtNombre.Location = new System.Drawing.Point(16, 36);
            this.txtNombre.Name = "txtNombre";
            this.txtNombre.Size = new System.Drawing.Size(360, 24);
            this.txtNombre.TabIndex = 1;
            // 
            // lblDescripcion
            // 
            this.lblDescripcion.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblDescripcion.Location = new System.Drawing.Point(16, 70);
            this.lblDescripcion.Name = "lblDescripcion";
            this.lblDescripcion.Size = new System.Drawing.Size(360, 18);
            this.lblDescripcion.TabIndex = 2;
            this.lblDescripcion.Text = "Descripción";
            // 
            // txtDescripcion
            // 
            this.txtDescripcion.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtDescripcion.Location = new System.Drawing.Point(16, 90);
            this.txtDescripcion.Multiline = true;
            this.txtDescripcion.Name = "txtDescripcion";
            this.txtDescripcion.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDescripcion.Size = new System.Drawing.Size(360, 56);
            this.txtDescripcion.TabIndex = 3;
            // 
            // lblTalle
            // 
            this.lblTalle.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTalle.Location = new System.Drawing.Point(16, 150);
            this.lblTalle.Name = "lblTalle";
            this.lblTalle.Size = new System.Drawing.Size(160, 18);
            this.lblTalle.TabIndex = 4;
            this.lblTalle.Text = "Talle *";
            // 
            // cmbTalle
            // 
            this.cmbTalle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTalle.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.cmbTalle.Items.AddRange(new object[] {
            "XS",
            "S",
            "M",
            "L",
            "XL",
            "XXL",
            "34",
            "36",
            "38",
            "40",
            "42",
            "44",
            "46",
            "Único"});
            this.cmbTalle.Location = new System.Drawing.Point(16, 170);
            this.cmbTalle.Name = "cmbTalle";
            this.cmbTalle.Size = new System.Drawing.Size(160, 25);
            this.cmbTalle.TabIndex = 5;
            // 
            // lblColor
            // 
            this.lblColor.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblColor.Location = new System.Drawing.Point(196, 150);
            this.lblColor.Name = "lblColor";
            this.lblColor.Size = new System.Drawing.Size(180, 18);
            this.lblColor.TabIndex = 6;
            this.lblColor.Text = "Color";
            // 
            // txtColor
            // 
            this.txtColor.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtColor.Location = new System.Drawing.Point(196, 170);
            this.txtColor.Name = "txtColor";
            this.txtColor.Size = new System.Drawing.Size(180, 24);
            this.txtColor.TabIndex = 7;
            // 
            // lblCategoria
            // 
            this.lblCategoria.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblCategoria.Location = new System.Drawing.Point(16, 204);
            this.lblCategoria.Name = "lblCategoria";
            this.lblCategoria.Size = new System.Drawing.Size(360, 18);
            this.lblCategoria.TabIndex = 8;
            this.lblCategoria.Text = "Categoría *";
            // 
            // cmbCategoria
            // 
            this.cmbCategoria.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCategoria.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.cmbCategoria.Items.AddRange(new object[] {
            "Vestidos",
            "Faldas",
            "Pantalones",
            "Tops",
            "Blazers",
            "Abrigos",
            "Conjuntos",
            "Ropa Deportiva",
            "Accesorios",
            "Otro"});
            this.cmbCategoria.Location = new System.Drawing.Point(16, 224);
            this.cmbCategoria.Name = "cmbCategoria";
            this.cmbCategoria.Size = new System.Drawing.Size(360, 25);
            this.cmbCategoria.TabIndex = 9;
            // 
            // lblMensaje
            // 
            this.lblMensaje.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblMensaje.ForeColor = System.Drawing.Color.DarkRed;
            this.lblMensaje.Location = new System.Drawing.Point(16, 258);
            this.lblMensaje.Name = "lblMensaje";
            this.lblMensaje.Size = new System.Drawing.Size(360, 34);
            this.lblMensaje.TabIndex = 10;
            // 
            // btnGuardar
            // 
            this.btnGuardar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(100)))), ((int)(((byte)(135)))));
            this.btnGuardar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnGuardar.FlatAppearance.BorderSize = 0;
            this.btnGuardar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGuardar.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnGuardar.ForeColor = System.Drawing.Color.White;
            this.btnGuardar.Location = new System.Drawing.Point(16, 298);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(184, 36);
            this.btnGuardar.TabIndex = 11;
            this.btnGuardar.Text = "Guardar Cambios";
            this.btnGuardar.UseVisualStyleBackColor = false;
            this.btnGuardar.Click += new System.EventHandler(this.BtnGuardar_Click);
            // 
            // btnCancelar
            // 
            this.btnCancelar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCancelar.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancelar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancelar.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnCancelar.Location = new System.Drawing.Point(216, 298);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(120, 36);
            this.btnCancelar.TabIndex = 12;
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.Click += new System.EventHandler(this.BtnCancelar_Click);
            // 
            // PrendaForm
            // 
            this.AcceptButton = this.btnGuardar;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.CancelButton = this.btnCancelar;
            this.ClientSize = new System.Drawing.Size(396, 356);
            this.Controls.Add(this.lblNombre);
            this.Controls.Add(this.txtNombre);
            this.Controls.Add(this.lblDescripcion);
            this.Controls.Add(this.txtDescripcion);
            this.Controls.Add(this.lblTalle);
            this.Controls.Add(this.cmbTalle);
            this.Controls.Add(this.lblColor);
            this.Controls.Add(this.txtColor);
            this.Controls.Add(this.lblCategoria);
            this.Controls.Add(this.cmbCategoria);
            this.Controls.Add(this.lblMensaje);
            this.Controls.Add(this.btnGuardar);
            this.Controls.Add(this.btnCancelar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PrendaForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Prenda";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label    lblNombre;
        private System.Windows.Forms.TextBox  txtNombre;
        private System.Windows.Forms.Label    lblDescripcion;
        private System.Windows.Forms.TextBox  txtDescripcion;
        private System.Windows.Forms.Label    lblTalle;
        private System.Windows.Forms.ComboBox cmbTalle;
        private System.Windows.Forms.Label    lblColor;
        private System.Windows.Forms.TextBox  txtColor;
        private System.Windows.Forms.Label    lblCategoria;
        private System.Windows.Forms.ComboBox cmbCategoria;
        private System.Windows.Forms.Label    lblMensaje;
        private System.Windows.Forms.Button   btnGuardar;
        private System.Windows.Forms.Button   btnCancelar;
    }
}
