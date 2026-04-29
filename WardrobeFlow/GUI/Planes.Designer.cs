namespace GUI
{
    partial class Planes
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
            this.panelForm = new System.Windows.Forms.Panel();
            this.lblFormTitulo = new System.Windows.Forms.Label();
            this.lblNombrePlan = new System.Windows.Forms.Label();
            this.txtNombre = new System.Windows.Forms.TextBox();
            this.lblLimite = new System.Windows.Forms.Label();
            this.nudLimite = new System.Windows.Forms.NumericUpDown();
            this.lblPrecio = new System.Windows.Forms.Label();
            this.nudPrecio = new System.Windows.Forms.NumericUpDown();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.btnNuevo = new System.Windows.Forms.Button();
            this.separador = new System.Windows.Forms.Label();
            this.lblAcciones = new System.Windows.Forms.Label();
            this.btnDesactivar = new System.Windows.Forms.Button();
            this.btnActivar = new System.Windows.Forms.Button();
            this.lblMensaje = new System.Windows.Forms.Label();
            this.dgvPlanes = new System.Windows.Forms.DataGridView();
            this.lblTituloGrilla = new System.Windows.Forms.Label();
            this.panelForm.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudLimite)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPrecio)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPlanes)).BeginInit();
            this.SuspendLayout();
            // 
            // panelForm
            // 
            this.panelForm.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(245)))), ((int)(((byte)(250)))));
            this.panelForm.Controls.Add(this.lblFormTitulo);
            this.panelForm.Controls.Add(this.lblNombrePlan);
            this.panelForm.Controls.Add(this.txtNombre);
            this.panelForm.Controls.Add(this.lblLimite);
            this.panelForm.Controls.Add(this.nudLimite);
            this.panelForm.Controls.Add(this.lblPrecio);
            this.panelForm.Controls.Add(this.nudPrecio);
            this.panelForm.Controls.Add(this.btnGuardar);
            this.panelForm.Controls.Add(this.btnNuevo);
            this.panelForm.Controls.Add(this.separador);
            this.panelForm.Controls.Add(this.lblAcciones);
            this.panelForm.Controls.Add(this.btnDesactivar);
            this.panelForm.Controls.Add(this.btnActivar);
            this.panelForm.Controls.Add(this.lblMensaje);
            this.panelForm.Dock = System.Windows.Forms.DockStyle.Right;
            this.panelForm.Location = new System.Drawing.Point(580, 0);
            this.panelForm.Name = "panelForm";
            this.panelForm.Padding = new System.Windows.Forms.Padding(14);
            this.panelForm.Size = new System.Drawing.Size(320, 560);
            this.panelForm.TabIndex = 0;
            // 
            // lblFormTitulo
            // 
            this.lblFormTitulo.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblFormTitulo.Location = new System.Drawing.Point(14, 12);
            this.lblFormTitulo.Name = "lblFormTitulo";
            this.lblFormTitulo.Size = new System.Drawing.Size(286, 24);
            this.lblFormTitulo.TabIndex = 0;
            this.lblFormTitulo.Text = "Nuevo Plan";
            // 
            // lblNombrePlan
            // 
            this.lblNombrePlan.Location = new System.Drawing.Point(14, 46);
            this.lblNombrePlan.Name = "lblNombrePlan";
            this.lblNombrePlan.Size = new System.Drawing.Size(286, 16);
            this.lblNombrePlan.TabIndex = 1;
            this.lblNombrePlan.Text = "Nombre del plan *";
            // 
            // txtNombre
            // 
            this.txtNombre.Location = new System.Drawing.Point(14, 65);
            this.txtNombre.Name = "txtNombre";
            this.txtNombre.Size = new System.Drawing.Size(286, 20);
            this.txtNombre.TabIndex = 2;
            // 
            // lblLimite
            // 
            this.lblLimite.Location = new System.Drawing.Point(14, 100);
            this.lblLimite.Name = "lblLimite";
            this.lblLimite.Size = new System.Drawing.Size(286, 17);
            this.lblLimite.TabIndex = 3;
            this.lblLimite.Text = "Límite de prendas *";
            // 
            // nudLimite
            // 
            this.nudLimite.Location = new System.Drawing.Point(14, 120);
            this.nudLimite.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.nudLimite.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudLimite.Name = "nudLimite";
            this.nudLimite.Size = new System.Drawing.Size(130, 20);
            this.nudLimite.TabIndex = 4;
            this.nudLimite.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // lblPrecio
            // 
            this.lblPrecio.Location = new System.Drawing.Point(14, 158);
            this.lblPrecio.Name = "lblPrecio";
            this.lblPrecio.Size = new System.Drawing.Size(286, 17);
            this.lblPrecio.TabIndex = 5;
            this.lblPrecio.Text = "Precio mensual ($) *";
            // 
            // nudPrecio
            // 
            this.nudPrecio.DecimalPlaces = 2;
            this.nudPrecio.Location = new System.Drawing.Point(14, 178);
            this.nudPrecio.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.nudPrecio.Name = "nudPrecio";
            this.nudPrecio.Size = new System.Drawing.Size(180, 20);
            this.nudPrecio.TabIndex = 6;
            // 
            // btnGuardar
            // 
            this.btnGuardar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(100)))), ((int)(((byte)(135)))));
            this.btnGuardar.FlatAppearance.BorderSize = 0;
            this.btnGuardar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGuardar.ForeColor = System.Drawing.Color.White;
            this.btnGuardar.Location = new System.Drawing.Point(14, 222);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(286, 36);
            this.btnGuardar.TabIndex = 7;
            this.btnGuardar.Text = "Guardar Plan";
            this.btnGuardar.UseVisualStyleBackColor = false;
            this.btnGuardar.Click += new System.EventHandler(this.BtnGuardar_Click);
            // 
            // btnNuevo
            // 
            this.btnNuevo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNuevo.Location = new System.Drawing.Point(14, 266);
            this.btnNuevo.Name = "btnNuevo";
            this.btnNuevo.Size = new System.Drawing.Size(286, 30);
            this.btnNuevo.TabIndex = 8;
            this.btnNuevo.Text = "Limpiar / Nuevo";
            this.btnNuevo.Click += new System.EventHandler(this.BtnNuevo_Click);
            // 
            // separador
            // 
            this.separador.BackColor = System.Drawing.Color.Silver;
            this.separador.Location = new System.Drawing.Point(14, 310);
            this.separador.Name = "separador";
            this.separador.Size = new System.Drawing.Size(286, 1);
            this.separador.TabIndex = 9;
            // 
            // lblAcciones
            // 
            this.lblAcciones.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblAcciones.ForeColor = System.Drawing.Color.DimGray;
            this.lblAcciones.Location = new System.Drawing.Point(14, 320);
            this.lblAcciones.Name = "lblAcciones";
            this.lblAcciones.Size = new System.Drawing.Size(286, 18);
            this.lblAcciones.TabIndex = 10;
            this.lblAcciones.Text = "Acciones sobre plan seleccionado";
            // 
            // btnDesactivar
            // 
            this.btnDesactivar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.btnDesactivar.Enabled = false;
            this.btnDesactivar.FlatAppearance.BorderSize = 0;
            this.btnDesactivar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDesactivar.ForeColor = System.Drawing.Color.White;
            this.btnDesactivar.Location = new System.Drawing.Point(14, 344);
            this.btnDesactivar.Name = "btnDesactivar";
            this.btnDesactivar.Size = new System.Drawing.Size(286, 34);
            this.btnDesactivar.TabIndex = 11;
            this.btnDesactivar.Text = "Desactivar Plan";
            this.btnDesactivar.UseVisualStyleBackColor = false;
            this.btnDesactivar.Click += new System.EventHandler(this.BtnDesactivar_Click);
            // 
            // btnActivar
            // 
            this.btnActivar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(140)))), ((int)(((byte)(80)))));
            this.btnActivar.Enabled = false;
            this.btnActivar.FlatAppearance.BorderSize = 0;
            this.btnActivar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnActivar.ForeColor = System.Drawing.Color.White;
            this.btnActivar.Location = new System.Drawing.Point(14, 386);
            this.btnActivar.Name = "btnActivar";
            this.btnActivar.Size = new System.Drawing.Size(286, 34);
            this.btnActivar.TabIndex = 12;
            this.btnActivar.Text = "Activar Plan";
            this.btnActivar.UseVisualStyleBackColor = false;
            this.btnActivar.Click += new System.EventHandler(this.BtnActivar_Click);
            // 
            // lblMensaje
            // 
            this.lblMensaje.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblMensaje.Location = new System.Drawing.Point(14, 432);
            this.lblMensaje.Name = "lblMensaje";
            this.lblMensaje.Size = new System.Drawing.Size(286, 90);
            this.lblMensaje.TabIndex = 13;
            // 
            // dgvPlanes
            // 
            this.dgvPlanes.AllowUserToAddRows = false;
            this.dgvPlanes.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(248)))), ((int)(((byte)(252)))));
            this.dgvPlanes.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvPlanes.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvPlanes.BackgroundColor = System.Drawing.Color.White;
            this.dgvPlanes.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(182)))), ((int)(((byte)(193)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvPlanes.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvPlanes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvPlanes.Location = new System.Drawing.Point(0, 28);
            this.dgvPlanes.Name = "dgvPlanes";
            this.dgvPlanes.ReadOnly = true;
            this.dgvPlanes.RowHeadersVisible = false;
            this.dgvPlanes.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvPlanes.Size = new System.Drawing.Size(580, 532);
            this.dgvPlanes.TabIndex = 1;
            this.dgvPlanes.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DgvPlanes_CellDoubleClick);
            this.dgvPlanes.SelectionChanged += new System.EventHandler(this.DgvPlanes_SelectionChanged);
            // 
            // lblTituloGrilla
            // 
            this.lblTituloGrilla.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(240)))));
            this.lblTituloGrilla.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTituloGrilla.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblTituloGrilla.Location = new System.Drawing.Point(0, 0);
            this.lblTituloGrilla.Name = "lblTituloGrilla";
            this.lblTituloGrilla.Padding = new System.Windows.Forms.Padding(6, 6, 0, 0);
            this.lblTituloGrilla.Size = new System.Drawing.Size(580, 28);
            this.lblTituloGrilla.TabIndex = 2;
            this.lblTituloGrilla.Text = "Planes registrados";
            // 
            // Planes
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(900, 560);
            this.Controls.Add(this.dgvPlanes);
            this.Controls.Add(this.lblTituloGrilla);
            this.Controls.Add(this.panelForm);
            this.MinimumSize = new System.Drawing.Size(780, 500);
            this.Name = "Planes";
            this.Text = "Planes de Suscripción";
            this.panelForm.ResumeLayout(false);
            this.panelForm.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudLimite)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPrecio)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPlanes)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel          panelForm;
        private System.Windows.Forms.Label          lblFormTitulo;
        private System.Windows.Forms.Label          lblNombrePlan;
        private System.Windows.Forms.TextBox        txtNombre;
        private System.Windows.Forms.Label          lblLimite;
        private System.Windows.Forms.NumericUpDown  nudLimite;
        private System.Windows.Forms.Label          lblPrecio;
        private System.Windows.Forms.NumericUpDown  nudPrecio;
        private System.Windows.Forms.Button         btnGuardar;
        private System.Windows.Forms.Button         btnNuevo;
        private System.Windows.Forms.Label          separador;
        private System.Windows.Forms.Label          lblAcciones;
        private System.Windows.Forms.Button         btnDesactivar;
        private System.Windows.Forms.Button         btnActivar;
        private System.Windows.Forms.Label          lblMensaje;
        private System.Windows.Forms.DataGridView   dgvPlanes;
        private System.Windows.Forms.Label          lblTituloGrilla;
    }
}
