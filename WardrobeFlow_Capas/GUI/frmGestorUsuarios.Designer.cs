namespace GUI
{
    partial class frmGestorUsuarios
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
            this.dgvUsuarios = new System.Windows.Forms.DataGridView();
            this.pnlCampos = new System.Windows.Forms.Panel();
            this.lblCamposTitle = new System.Windows.Forms.Label();
            this.lblNombre = new System.Windows.Forms.Label();
            this.txtNombre = new System.Windows.Forms.TextBox();
            this.lblDocumento = new System.Windows.Forms.Label();
            this.txtDocumento = new System.Windows.Forms.TextBox();
            this.lblCorreo = new System.Windows.Forms.Label();
            this.txtCorreo = new System.Windows.Forms.TextBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.lblRol = new System.Windows.Forms.Label();
            this.cboRol = new System.Windows.Forms.ComboBox();
            this.btnNuevo = new System.Windows.Forms.Button();
            this.btnEditar = new System.Windows.Forms.Button();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.btnEliminar = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUsuarios)).BeginInit();
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
            this.lblTitulo.Text = "Gestor de Usuarios";
            // pnlContent
            this.pnlContent.BackColor = System.Drawing.Color.FromArgb(255, 240, 248);
            this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.Padding = new System.Windows.Forms.Padding(14);
            this.pnlContent.Controls.Add(this.dgvUsuarios);
            this.pnlContent.Controls.Add(this.pnlCampos);
            // dgvUsuarios
            this.dgvUsuarios.AllowUserToAddRows = false;
            this.dgvUsuarios.AllowUserToDeleteRows = false;
            this.dgvUsuarios.MultiSelect = false;
            this.dgvUsuarios.ReadOnly = true;
            this.dgvUsuarios.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvUsuarios.Location = new System.Drawing.Point(14, 14);
            this.dgvUsuarios.Name = "dgvUsuarios";
            this.dgvUsuarios.Size = new System.Drawing.Size(540, 450);
            this.dgvUsuarios.TabIndex = 0;
            this.dgvUsuarios.BackgroundColor = System.Drawing.Color.White;
            this.dgvUsuarios.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvUsuarios.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.dgvUsuarios.GridColor = System.Drawing.Color.FromArgb(220, 170, 195);
            this.dgvUsuarios.EnableHeadersVisualStyles = false;
            this.dgvUsuarios.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dgvUsuarios.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(180, 35, 75);
            this.dgvUsuarios.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            this.dgvUsuarios.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.dgvUsuarios.ColumnHeadersDefaultCellStyle.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.dgvUsuarios.ColumnHeadersHeight = 36;
            this.dgvUsuarios.DefaultCellStyle.BackColor = System.Drawing.Color.White;
            this.dgvUsuarios.DefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(35, 20, 30);
            this.dgvUsuarios.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.dgvUsuarios.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(232, 120, 160);
            this.dgvUsuarios.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.White;
            this.dgvUsuarios.DefaultCellStyle.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.dgvUsuarios.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(255, 225, 238);
            this.dgvUsuarios.RowHeadersVisible = false;
            this.dgvUsuarios.RowTemplate.Height = 28;
            this.dgvUsuarios.SelectionChanged += new System.EventHandler(this.dgvUsuarios_SelectionChanged);
            // pnlCampos
            this.pnlCampos.BackColor = System.Drawing.Color.White;
            this.pnlCampos.Location = new System.Drawing.Point(570, 14);
            this.pnlCampos.Name = "pnlCampos";
            this.pnlCampos.Size = new System.Drawing.Size(230, 460);
            this.pnlCampos.Controls.Add(this.lblCamposTitle);
            this.pnlCampos.Controls.Add(this.lblNombre);
            this.pnlCampos.Controls.Add(this.txtNombre);
            this.pnlCampos.Controls.Add(this.lblDocumento);
            this.pnlCampos.Controls.Add(this.txtDocumento);
            this.pnlCampos.Controls.Add(this.lblCorreo);
            this.pnlCampos.Controls.Add(this.txtCorreo);
            this.pnlCampos.Controls.Add(this.lblPassword);
            this.pnlCampos.Controls.Add(this.txtPassword);
            this.pnlCampos.Controls.Add(this.lblRol);
            this.pnlCampos.Controls.Add(this.cboRol);
            this.pnlCampos.Controls.Add(this.btnNuevo);
            this.pnlCampos.Controls.Add(this.btnEditar);
            this.pnlCampos.Controls.Add(this.btnGuardar);
            this.pnlCampos.Controls.Add(this.btnEliminar);
            // lblCamposTitle
            this.lblCamposTitle.AutoSize = true;
            this.lblCamposTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblCamposTitle.ForeColor = System.Drawing.Color.FromArgb(180, 35, 75);
            this.lblCamposTitle.Location = new System.Drawing.Point(15, 10);
            this.lblCamposTitle.Name = "lblCamposTitle";
            this.lblCamposTitle.Text = "Datos de Usuario";
            // lblNombre
            this.lblNombre.AutoSize = true;
            this.lblNombre.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblNombre.ForeColor = System.Drawing.Color.FromArgb(150, 80, 105);
            this.lblNombre.Location = new System.Drawing.Point(15, 38);
            this.lblNombre.Name = "lblNombre";
            this.lblNombre.Text = "NOMBRE COMPLETO";
            // txtNombre
            this.txtNombre.BackColor = System.Drawing.Color.FromArgb(255, 240, 248);
            this.txtNombre.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtNombre.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtNombre.Location = new System.Drawing.Point(15, 56);
            this.txtNombre.Name = "txtNombre";
            this.txtNombre.Size = new System.Drawing.Size(200, 26);
            this.txtNombre.TabIndex = 1;
            // lblDocumento
            this.lblDocumento.AutoSize = true;
            this.lblDocumento.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblDocumento.ForeColor = System.Drawing.Color.FromArgb(150, 80, 105);
            this.lblDocumento.Location = new System.Drawing.Point(15, 94);
            this.lblDocumento.Name = "lblDocumento";
            this.lblDocumento.Text = "DOCUMENTO";
            // txtDocumento
            this.txtDocumento.BackColor = System.Drawing.Color.FromArgb(255, 240, 248);
            this.txtDocumento.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtDocumento.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtDocumento.Location = new System.Drawing.Point(15, 112);
            this.txtDocumento.Name = "txtDocumento";
            this.txtDocumento.Size = new System.Drawing.Size(200, 26);
            this.txtDocumento.TabIndex = 2;
            // lblCorreo
            this.lblCorreo.AutoSize = true;
            this.lblCorreo.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblCorreo.ForeColor = System.Drawing.Color.FromArgb(150, 80, 105);
            this.lblCorreo.Location = new System.Drawing.Point(15, 150);
            this.lblCorreo.Name = "lblCorreo";
            this.lblCorreo.Text = "CORREO";
            // txtCorreo
            this.txtCorreo.BackColor = System.Drawing.Color.FromArgb(255, 240, 248);
            this.txtCorreo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtCorreo.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtCorreo.Location = new System.Drawing.Point(15, 168);
            this.txtCorreo.Name = "txtCorreo";
            this.txtCorreo.Size = new System.Drawing.Size(200, 26);
            this.txtCorreo.TabIndex = 3;
            // lblPassword
            this.lblPassword.AutoSize = true;
            this.lblPassword.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblPassword.ForeColor = System.Drawing.Color.FromArgb(150, 80, 105);
            this.lblPassword.Location = new System.Drawing.Point(15, 206);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Text = "CONTRASEÑA (vacío = no cambia)";
            // txtPassword
            this.txtPassword.BackColor = System.Drawing.Color.FromArgb(255, 240, 248);
            this.txtPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPassword.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtPassword.Location = new System.Drawing.Point(15, 224);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(200, 26);
            this.txtPassword.TabIndex = 4;
            // lblRol
            this.lblRol.AutoSize = true;
            this.lblRol.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblRol.ForeColor = System.Drawing.Color.FromArgb(150, 80, 105);
            this.lblRol.Location = new System.Drawing.Point(15, 262);
            this.lblRol.Name = "lblRol";
            this.lblRol.Text = "ROL";
            // cboRol
            this.cboRol.BackColor = System.Drawing.Color.FromArgb(255, 240, 248);
            this.cboRol.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboRol.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.cboRol.Location = new System.Drawing.Point(15, 280);
            this.cboRol.Name = "cboRol";
            this.cboRol.Size = new System.Drawing.Size(200, 26);
            this.cboRol.TabIndex = 5;
            // btnNuevo
            this.btnNuevo.BackColor = System.Drawing.Color.White;
            this.btnNuevo.ForeColor = System.Drawing.Color.FromArgb(180, 35, 75);
            this.btnNuevo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNuevo.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(180, 35, 75);
            this.btnNuevo.FlatAppearance.BorderSize = 1;
            this.btnNuevo.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnNuevo.Location = new System.Drawing.Point(15, 318);
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
            this.btnEditar.Location = new System.Drawing.Point(15, 354);
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
            this.btnGuardar.Location = new System.Drawing.Point(15, 390);
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
            this.btnEliminar.Location = new System.Drawing.Point(15, 426);
            this.btnEliminar.Name = "btnEliminar";
            this.btnEliminar.Size = new System.Drawing.Size(200, 30);
            this.btnEliminar.TabIndex = 9;
            this.btnEliminar.Text = "Eliminar";
            this.btnEliminar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnEliminar.Click += new System.EventHandler(this.btnEliminar_Click);
            // frmGestorUsuarios
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(255, 240, 248);
            this.ClientSize = new System.Drawing.Size(818, 536);
            this.Controls.Add(this.pnlContent);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.Name = "frmGestorUsuarios";
            this.Text = "WardrobeFlow - Usuarios";
            ((System.ComponentModel.ISupportInitialize)(this.dgvUsuarios)).EndInit();
            this.pnlContent.ResumeLayout(false);
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
        private System.Windows.Forms.DataGridView dgvUsuarios;
        private System.Windows.Forms.Label lblNombre;
        private System.Windows.Forms.TextBox txtNombre;
        private System.Windows.Forms.Label lblDocumento;
        private System.Windows.Forms.TextBox txtDocumento;
        private System.Windows.Forms.Label lblCorreo;
        private System.Windows.Forms.TextBox txtCorreo;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label lblRol;
        private System.Windows.Forms.ComboBox cboRol;
        private System.Windows.Forms.Button btnNuevo;
        private System.Windows.Forms.Button btnEditar;
        private System.Windows.Forms.Button btnGuardar;
        private System.Windows.Forms.Button btnEliminar;
    }
}
