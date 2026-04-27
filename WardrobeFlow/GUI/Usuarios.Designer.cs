namespace GUI
{
    partial class Usuarios
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.panelAlta = new System.Windows.Forms.Panel();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.lblUser = new System.Windows.Forms.Label();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.lblPass = new System.Windows.Forms.Label();
            this.txtContraseña = new System.Windows.Forms.TextBox();
            this.lblPerfil = new System.Windows.Forms.Label();
            this.cmbPerfil = new System.Windows.Forms.ComboBox();
            this.btnAgregar = new System.Windows.Forms.Button();
            this.btnRefrescar = new System.Windows.Forms.Button();
            this.separador1 = new System.Windows.Forms.Label();
            this.lblResetTitulo = new System.Windows.Forms.Label();
            this.lblResetInfo = new System.Windows.Forms.Label();
            this.btnResetearClave = new System.Windows.Forms.Button();
            this.separador2 = new System.Windows.Forms.Label();
            this.lblDesbloquearTitulo = new System.Windows.Forms.Label();
            this.lblDesbloquearInfo = new System.Windows.Forms.Label();
            this.btnDesbloquear = new System.Windows.Forms.Button();
            this.lblMensaje = new System.Windows.Forms.Label();
            this.dgvUsuarios = new System.Windows.Forms.DataGridView();
            this.lblListaTitulo = new System.Windows.Forms.Label();
            this.panelAlta.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUsuarios)).BeginInit();
            this.SuspendLayout();
            // 
            // panelAlta
            // 
            this.panelAlta.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(245)))), ((int)(((byte)(250)))));
            this.panelAlta.Controls.Add(this.lblTitulo);
            this.panelAlta.Controls.Add(this.lblUser);
            this.panelAlta.Controls.Add(this.txtUsername);
            this.panelAlta.Controls.Add(this.lblPass);
            this.panelAlta.Controls.Add(this.txtContraseña);
            this.panelAlta.Controls.Add(this.lblPerfil);
            this.panelAlta.Controls.Add(this.cmbPerfil);
            this.panelAlta.Controls.Add(this.btnAgregar);
            this.panelAlta.Controls.Add(this.btnRefrescar);
            this.panelAlta.Controls.Add(this.separador1);
            this.panelAlta.Controls.Add(this.lblResetTitulo);
            this.panelAlta.Controls.Add(this.lblResetInfo);
            this.panelAlta.Controls.Add(this.btnResetearClave);
            this.panelAlta.Controls.Add(this.separador2);
            this.panelAlta.Controls.Add(this.lblDesbloquearTitulo);
            this.panelAlta.Controls.Add(this.lblDesbloquearInfo);
            this.panelAlta.Controls.Add(this.btnDesbloquear);
            this.panelAlta.Controls.Add(this.lblMensaje);
            this.panelAlta.Dock = System.Windows.Forms.DockStyle.Right;
            this.panelAlta.Location = new System.Drawing.Point(620, 0);
            this.panelAlta.Name = "panelAlta";
            this.panelAlta.Padding = new System.Windows.Forms.Padding(12);
            this.panelAlta.Size = new System.Drawing.Size(240, 660);
            this.panelAlta.TabIndex = 0;
            // 
            // lblTitulo
            // 
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.Location = new System.Drawing.Point(12, 12);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(210, 23);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "Nuevo Usuario";
            // 
            // lblUser
            // 
            this.lblUser.Location = new System.Drawing.Point(12, 50);
            this.lblUser.Name = "lblUser";
            this.lblUser.Size = new System.Drawing.Size(200, 15);
            this.lblUser.TabIndex = 1;
            this.lblUser.Text = "Nombre de usuario:";
            // 
            // txtUsername
            // 
            this.txtUsername.Location = new System.Drawing.Point(12, 68);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(210, 20);
            this.txtUsername.TabIndex = 2;
            // 
            // lblPass
            // 
            this.lblPass.Location = new System.Drawing.Point(12, 100);
            this.lblPass.Name = "lblPass";
            this.lblPass.Size = new System.Drawing.Size(200, 15);
            this.lblPass.TabIndex = 3;
            this.lblPass.Text = "Contraseña:";
            // 
            // txtContraseña
            // 
            this.txtContraseña.Location = new System.Drawing.Point(12, 118);
            this.txtContraseña.Name = "txtContraseña";
            this.txtContraseña.PasswordChar = '●';
            this.txtContraseña.Size = new System.Drawing.Size(210, 20);
            this.txtContraseña.TabIndex = 4;
            // 
            // lblPerfil
            // 
            this.lblPerfil.Location = new System.Drawing.Point(12, 150);
            this.lblPerfil.Name = "lblPerfil";
            this.lblPerfil.Size = new System.Drawing.Size(200, 15);
            this.lblPerfil.TabIndex = 5;
            this.lblPerfil.Text = "Perfil (rol):";
            // 
            // cmbPerfil
            // 
            this.cmbPerfil.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPerfil.Items.AddRange(new object[] {
            "Administrador",
            "Supervisor",
            "Vendedor",
            "Controlador de Stock",
            "Operador de Inventario"});
            this.cmbPerfil.Location = new System.Drawing.Point(12, 168);
            this.cmbPerfil.Name = "cmbPerfil";
            this.cmbPerfil.Size = new System.Drawing.Size(210, 21);
            this.cmbPerfil.TabIndex = 6;
            // 
            // btnAgregar
            // 
            this.btnAgregar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(100)))), ((int)(((byte)(135)))));
            this.btnAgregar.FlatAppearance.BorderSize = 0;
            this.btnAgregar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAgregar.ForeColor = System.Drawing.Color.White;
            this.btnAgregar.Location = new System.Drawing.Point(12, 210);
            this.btnAgregar.Name = "btnAgregar";
            this.btnAgregar.Size = new System.Drawing.Size(210, 34);
            this.btnAgregar.TabIndex = 7;
            this.btnAgregar.Text = "Agregar Usuario";
            this.btnAgregar.UseVisualStyleBackColor = false;
            this.btnAgregar.Click += new System.EventHandler(this.BtnAgregar_Click);
            // 
            // btnRefrescar
            // 
            this.btnRefrescar.Location = new System.Drawing.Point(12, 253);
            this.btnRefrescar.Name = "btnRefrescar";
            this.btnRefrescar.Size = new System.Drawing.Size(210, 28);
            this.btnRefrescar.TabIndex = 8;
            this.btnRefrescar.Text = "↻ Refrescar Lista";
            this.btnRefrescar.Click += new System.EventHandler(this.BtnRefrescar_Click);
            // 
            // separador1
            // 
            this.separador1.BackColor = System.Drawing.Color.Silver;
            this.separador1.Location = new System.Drawing.Point(12, 292);
            this.separador1.Name = "separador1";
            this.separador1.Size = new System.Drawing.Size(210, 1);
            this.separador1.TabIndex = 9;
            // 
            // lblResetTitulo
            // 
            this.lblResetTitulo.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblResetTitulo.Location = new System.Drawing.Point(12, 302);
            this.lblResetTitulo.Name = "lblResetTitulo";
            this.lblResetTitulo.Size = new System.Drawing.Size(210, 23);
            this.lblResetTitulo.TabIndex = 10;
            this.lblResetTitulo.Text = "Resetear Contraseña";
            // 
            // lblResetInfo
            // 
            this.lblResetInfo.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblResetInfo.ForeColor = System.Drawing.Color.DimGray;
            this.lblResetInfo.Location = new System.Drawing.Point(12, 325);
            this.lblResetInfo.Name = "lblResetInfo";
            this.lblResetInfo.Size = new System.Drawing.Size(210, 36);
            this.lblResetInfo.TabIndex = 11;
            this.lblResetInfo.Text = "Selecioná un usuario\nen la lista y presioná:";
            // 
            // btnResetearClave
            // 
            this.btnResetearClave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(100)))), ((int)(((byte)(30)))));
            this.btnResetearClave.Enabled = false;
            this.btnResetearClave.FlatAppearance.BorderSize = 0;
            this.btnResetearClave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnResetearClave.ForeColor = System.Drawing.Color.White;
            this.btnResetearClave.Location = new System.Drawing.Point(12, 366);
            this.btnResetearClave.Name = "btnResetearClave";
            this.btnResetearClave.Size = new System.Drawing.Size(210, 34);
            this.btnResetearClave.TabIndex = 12;
            this.btnResetearClave.Text = "Resetear Contraseña";
            this.btnResetearClave.UseVisualStyleBackColor = false;
            this.btnResetearClave.Click += new System.EventHandler(this.BtnResetearClave_Click);
            // 
            // separador2
            // 
            this.separador2.BackColor = System.Drawing.Color.Silver;
            this.separador2.Location = new System.Drawing.Point(12, 408);
            this.separador2.Name = "separador2";
            this.separador2.Size = new System.Drawing.Size(210, 1);
            this.separador2.TabIndex = 13;
            // 
            // lblDesbloquearTitulo
            // 
            this.lblDesbloquearTitulo.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblDesbloquearTitulo.Location = new System.Drawing.Point(12, 418);
            this.lblDesbloquearTitulo.Name = "lblDesbloquearTitulo";
            this.lblDesbloquearTitulo.Size = new System.Drawing.Size(210, 23);
            this.lblDesbloquearTitulo.TabIndex = 14;
            this.lblDesbloquearTitulo.Text = "Desbloquear Cuenta";
            // 
            // lblDesbloquearInfo
            // 
            this.lblDesbloquearInfo.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblDesbloquearInfo.ForeColor = System.Drawing.Color.DimGray;
            this.lblDesbloquearInfo.Location = new System.Drawing.Point(12, 441);
            this.lblDesbloquearInfo.Name = "lblDesbloquearInfo";
            this.lblDesbloquearInfo.Size = new System.Drawing.Size(210, 36);
            this.lblDesbloquearInfo.TabIndex = 15;
            this.lblDesbloquearInfo.Text = "Selecioná un usuario\nbloqueado y presioná:";
            // 
            // btnDesbloquear
            // 
            this.btnDesbloquear.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(130)))), ((int)(((byte)(76)))));
            this.btnDesbloquear.Enabled = false;
            this.btnDesbloquear.FlatAppearance.BorderSize = 0;
            this.btnDesbloquear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDesbloquear.ForeColor = System.Drawing.Color.White;
            this.btnDesbloquear.Location = new System.Drawing.Point(12, 482);
            this.btnDesbloquear.Name = "btnDesbloquear";
            this.btnDesbloquear.Size = new System.Drawing.Size(210, 34);
            this.btnDesbloquear.TabIndex = 16;
            this.btnDesbloquear.Text = "Desbloquear Cuenta";
            this.btnDesbloquear.UseVisualStyleBackColor = false;
            this.btnDesbloquear.Click += new System.EventHandler(this.BtnDesbloquear_Click);
            // 
            // lblMensaje
            // 
            this.lblMensaje.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblMensaje.ForeColor = System.Drawing.Color.DarkGreen;
            this.lblMensaje.Location = new System.Drawing.Point(12, 526);
            this.lblMensaje.Name = "lblMensaje";
            this.lblMensaje.Size = new System.Drawing.Size(210, 80);
            this.lblMensaje.TabIndex = 17;
            // 
            // dgvUsuarios
            // 
            this.dgvUsuarios.AllowUserToAddRows = false;
            this.dgvUsuarios.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(248)))), ((int)(((byte)(252)))));
            this.dgvUsuarios.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvUsuarios.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvUsuarios.BackgroundColor = System.Drawing.Color.White;
            this.dgvUsuarios.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(182)))), ((int)(((byte)(193)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvUsuarios.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvUsuarios.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvUsuarios.Location = new System.Drawing.Point(0, 28);
            this.dgvUsuarios.Name = "dgvUsuarios";
            this.dgvUsuarios.ReadOnly = true;
            this.dgvUsuarios.RowHeadersVisible = false;
            this.dgvUsuarios.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvUsuarios.Size = new System.Drawing.Size(620, 632);
            this.dgvUsuarios.TabIndex = 1;
            this.dgvUsuarios.SelectionChanged += new System.EventHandler(this.DgvUsuarios_SelectionChanged);
            // 
            // lblListaTitulo
            // 
            this.lblListaTitulo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(240)))));
            this.lblListaTitulo.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblListaTitulo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblListaTitulo.Location = new System.Drawing.Point(0, 0);
            this.lblListaTitulo.Name = "lblListaTitulo";
            this.lblListaTitulo.Padding = new System.Windows.Forms.Padding(6, 6, 0, 0);
            this.lblListaTitulo.Size = new System.Drawing.Size(620, 28);
            this.lblListaTitulo.TabIndex = 2;
            this.lblListaTitulo.Text = "Usuarios registrados en el sistema";
            // 
            // Usuarios
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(860, 660);
            this.Controls.Add(this.dgvUsuarios);
            this.Controls.Add(this.lblListaTitulo);
            this.Controls.Add(this.panelAlta);
            this.MinimumSize = new System.Drawing.Size(760, 580);
            this.Name = "Usuarios";
            this.Text = "Gestión de Usuarios";
            this.panelAlta.ResumeLayout(false);
            this.panelAlta.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUsuarios)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel        panelAlta;
        private System.Windows.Forms.Label        lblTitulo;
        private System.Windows.Forms.Label        lblUser;
        private System.Windows.Forms.TextBox      txtUsername;
        private System.Windows.Forms.Label        lblPass;
        private System.Windows.Forms.TextBox      txtContraseña;
        private System.Windows.Forms.Label        lblPerfil;
        private System.Windows.Forms.ComboBox     cmbPerfil;
        private System.Windows.Forms.Button       btnAgregar;
        private System.Windows.Forms.Button       btnRefrescar;
        private System.Windows.Forms.Label        separador1;
        private System.Windows.Forms.Label        lblResetTitulo;
        private System.Windows.Forms.Label        lblResetInfo;
        private System.Windows.Forms.Button       btnResetearClave;
        private System.Windows.Forms.Label        separador2;
        private System.Windows.Forms.Label        lblDesbloquearTitulo;
        private System.Windows.Forms.Label        lblDesbloquearInfo;
        private System.Windows.Forms.Button       btnDesbloquear;
        private System.Windows.Forms.Label        lblMensaje;
        private System.Windows.Forms.DataGridView dgvUsuarios;
        private System.Windows.Forms.Label        lblListaTitulo;
    }
}
