namespace GUI
{
    partial class NuevoPedidoForm
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
            this.panelHeader = new System.Windows.Forms.Panel();
            this.lblPaso = new System.Windows.Forms.Label();
            this.panelStatus = new System.Windows.Forms.Panel();
            this.lblMensaje = new System.Windows.Forms.Label();
            this.panelPaso1 = new System.Windows.Forms.Panel();
            this.lblSeleccionaCliente = new System.Windows.Forms.Label();
            this.cmbCliente = new System.Windows.Forms.ComboBox();
            this.lblInfoPlan = new System.Windows.Forms.Label();
            this.btnSiguiente = new System.Windows.Forms.Button();
            this.panelPaso2 = new System.Windows.Forms.Panel();
            this.lblInstruccion = new System.Windows.Forms.Label();
            this.dgvPrendas = new System.Windows.Forms.DataGridView();
            this.Sel = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Nombre = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Categoria = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Talle = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colColor = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lblResumen = new System.Windows.Forms.Label();
            this.btnVolver = new System.Windows.Forms.Button();
            this.btnConfirmar = new System.Windows.Forms.Button();
            this.panelHeader.SuspendLayout();
            this.panelStatus.SuspendLayout();
            this.panelPaso1.SuspendLayout();
            this.panelPaso2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPrendas)).BeginInit();
            this.SuspendLayout();
            // 
            // panelHeader
            // 
            this.panelHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(100)))), ((int)(((byte)(160)))));
            this.panelHeader.Controls.Add(this.lblPaso);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Location = new System.Drawing.Point(0, 0);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Size = new System.Drawing.Size(700, 40);
            this.panelHeader.TabIndex = 0;
            // 
            // lblPaso
            // 
            this.lblPaso.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(100)))), ((int)(((byte)(135)))));
            this.lblPaso.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPaso.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblPaso.ForeColor = System.Drawing.Color.White;
            this.lblPaso.Location = new System.Drawing.Point(0, 0);
            this.lblPaso.Name = "lblPaso";
            this.lblPaso.Padding = new System.Windows.Forms.Padding(12, 8, 0, 0);
            this.lblPaso.Size = new System.Drawing.Size(700, 40);
            this.lblPaso.TabIndex = 0;
            this.lblPaso.Text = "Paso 1 de 2 — Seleccionar Cliente";
            // 
            // panelStatus
            // 
            this.panelStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(240)))));
            this.panelStatus.Controls.Add(this.lblMensaje);
            this.panelStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelStatus.Location = new System.Drawing.Point(0, 494);
            this.panelStatus.Name = "panelStatus";
            this.panelStatus.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);
            this.panelStatus.Size = new System.Drawing.Size(700, 26);
            this.panelStatus.TabIndex = 3;
            // 
            // lblMensaje
            // 
            this.lblMensaje.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMensaje.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblMensaje.Location = new System.Drawing.Point(8, 4);
            this.lblMensaje.Name = "lblMensaje";
            this.lblMensaje.Size = new System.Drawing.Size(684, 18);
            this.lblMensaje.TabIndex = 0;
            // 
            // panelPaso1
            // 
            this.panelPaso1.Controls.Add(this.lblSeleccionaCliente);
            this.panelPaso1.Controls.Add(this.cmbCliente);
            this.panelPaso1.Controls.Add(this.lblInfoPlan);
            this.panelPaso1.Controls.Add(this.btnSiguiente);
            this.panelPaso1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelPaso1.Location = new System.Drawing.Point(0, 40);
            this.panelPaso1.Name = "panelPaso1";
            this.panelPaso1.Padding = new System.Windows.Forms.Padding(20);
            this.panelPaso1.Size = new System.Drawing.Size(700, 454);
            this.panelPaso1.TabIndex = 1;
            // 
            // lblSeleccionaCliente
            // 
            this.lblSeleccionaCliente.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblSeleccionaCliente.Location = new System.Drawing.Point(20, 20);
            this.lblSeleccionaCliente.Name = "lblSeleccionaCliente";
            this.lblSeleccionaCliente.Size = new System.Drawing.Size(600, 23);
            this.lblSeleccionaCliente.TabIndex = 0;
            this.lblSeleccionaCliente.Text = "Selecioná el cliente para este pedido:";
            // 
            // cmbCliente
            // 
            this.cmbCliente.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCliente.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbCliente.Location = new System.Drawing.Point(20, 50);
            this.cmbCliente.Name = "cmbCliente";
            this.cmbCliente.Size = new System.Drawing.Size(500, 25);
            this.cmbCliente.TabIndex = 1;
            this.cmbCliente.SelectedIndexChanged += new System.EventHandler(this.CmbCliente_SelectedIndexChanged);
            // 
            // lblInfoPlan
            // 
            this.lblInfoPlan.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            this.lblInfoPlan.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblInfoPlan.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblInfoPlan.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(80)))), ((int)(((byte)(140)))));
            this.lblInfoPlan.Location = new System.Drawing.Point(20, 90);
            this.lblInfoPlan.Name = "lblInfoPlan";
            this.lblInfoPlan.Padding = new System.Windows.Forms.Padding(10);
            this.lblInfoPlan.Size = new System.Drawing.Size(620, 120);
            this.lblInfoPlan.TabIndex = 2;
            this.lblInfoPlan.Visible = false;
            // 
            // btnSiguiente
            // 
            this.btnSiguiente.BackColor = System.Drawing.Color.SteelBlue;
            this.btnSiguiente.Enabled = false;
            this.btnSiguiente.FlatAppearance.BorderSize = 0;
            this.btnSiguiente.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSiguiente.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnSiguiente.ForeColor = System.Drawing.Color.White;
            this.btnSiguiente.Location = new System.Drawing.Point(20, 240);
            this.btnSiguiente.Name = "btnSiguiente";
            this.btnSiguiente.Size = new System.Drawing.Size(160, 36);
            this.btnSiguiente.TabIndex = 3;
            this.btnSiguiente.Text = "Siguiente →";
            this.btnSiguiente.UseVisualStyleBackColor = false;
            this.btnSiguiente.Click += new System.EventHandler(this.BtnSiguiente_Click);
            // 
            // panelPaso2
            // 
            this.panelPaso2.Controls.Add(this.lblInstruccion);
            this.panelPaso2.Controls.Add(this.dgvPrendas);
            this.panelPaso2.Controls.Add(this.lblResumen);
            this.panelPaso2.Controls.Add(this.btnVolver);
            this.panelPaso2.Controls.Add(this.btnConfirmar);
            this.panelPaso2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelPaso2.Location = new System.Drawing.Point(0, 40);
            this.panelPaso2.Name = "panelPaso2";
            this.panelPaso2.Padding = new System.Windows.Forms.Padding(10);
            this.panelPaso2.Size = new System.Drawing.Size(700, 454);
            this.panelPaso2.TabIndex = 2;
            this.panelPaso2.Visible = false;
            // 
            // lblInstruccion
            // 
            this.lblInstruccion.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblInstruccion.Location = new System.Drawing.Point(10, 4);
            this.lblInstruccion.Name = "lblInstruccion";
            this.lblInstruccion.Size = new System.Drawing.Size(660, 23);
            this.lblInstruccion.TabIndex = 0;
            this.lblInstruccion.Text = "Selecioná las prendas para incluir en el pedido (checkbox):";
            // 
            // dgvPrendas
            // 
            this.dgvPrendas.AllowUserToAddRows = false;
            this.dgvPrendas.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(248)))), ((int)(((byte)(252)))));
            this.dgvPrendas.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvPrendas.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvPrendas.BackgroundColor = System.Drawing.Color.White;
            this.dgvPrendas.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Sel,
            this.ID,
            this.Nombre,
            this.Categoria,
            this.Talle,
            this.colColor});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(182)))), ((int)(((byte)(193)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvPrendas.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvPrendas.Location = new System.Drawing.Point(10, 26);
            this.dgvPrendas.Name = "dgvPrendas";
            this.dgvPrendas.RowHeadersVisible = false;
            this.dgvPrendas.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvPrendas.Size = new System.Drawing.Size(660, 340);
            this.dgvPrendas.TabIndex = 1;
            this.dgvPrendas.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.DgvPrendas_CellValueChanged);
            this.dgvPrendas.CurrentCellDirtyStateChanged += new System.EventHandler(this.DgvPrendas_CurrentCellDirtyStateChanged);
            // 
            // Sel
            // 
            this.Sel.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.Sel.HeaderText = "";
            this.Sel.Name = "Sel";
            this.Sel.Width = 32;
            // 
            // ID
            // 
            this.ID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.ID.HeaderText = "ID";
            this.ID.Name = "ID";
            this.ID.ReadOnly = true;
            this.ID.Width = 40;
            // 
            // Nombre
            // 
            this.Nombre.HeaderText = "Nombre";
            this.Nombre.Name = "Nombre";
            this.Nombre.ReadOnly = true;
            // 
            // Categoria
            // 
            this.Categoria.HeaderText = "Categoría";
            this.Categoria.Name = "Categoria";
            this.Categoria.ReadOnly = true;
            // 
            // Talle
            // 
            this.Talle.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.Talle.HeaderText = "Talle";
            this.Talle.Name = "Talle";
            this.Talle.ReadOnly = true;
            this.Talle.Width = 60;
            // 
            // Color
            // 
            this.colColor.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colColor.HeaderText = "Color";
            this.colColor.Name = "Color";
            this.colColor.ReadOnly = true;
            this.colColor.Width = 90;
            // 
            // lblResumen
            // 
            this.lblResumen.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblResumen.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(80)))), ((int)(((byte)(140)))));
            this.lblResumen.Location = new System.Drawing.Point(10, 374);
            this.lblResumen.Name = "lblResumen";
            this.lblResumen.Size = new System.Drawing.Size(660, 40);
            this.lblResumen.TabIndex = 2;
            // 
            // btnVolver
            // 
            this.btnVolver.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVolver.Location = new System.Drawing.Point(10, 417);
            this.btnVolver.Name = "btnVolver";
            this.btnVolver.Size = new System.Drawing.Size(110, 34);
            this.btnVolver.TabIndex = 3;
            this.btnVolver.Text = "← Volver";
            this.btnVolver.Click += new System.EventHandler(this.BtnVolver_Click);
            // 
            // btnConfirmar
            // 
            this.btnConfirmar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(140)))), ((int)(((byte)(60)))));
            this.btnConfirmar.Enabled = false;
            this.btnConfirmar.FlatAppearance.BorderSize = 0;
            this.btnConfirmar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConfirmar.ForeColor = System.Drawing.Color.White;
            this.btnConfirmar.Location = new System.Drawing.Point(130, 417);
            this.btnConfirmar.Name = "btnConfirmar";
            this.btnConfirmar.Size = new System.Drawing.Size(190, 34);
            this.btnConfirmar.TabIndex = 4;
            this.btnConfirmar.Text = "✓ Confirmar Pedido";
            this.btnConfirmar.UseVisualStyleBackColor = false;
            this.btnConfirmar.Click += new System.EventHandler(this.BtnConfirmar_Click);
            // 
            // NuevoPedidoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(700, 520);
            this.Controls.Add(this.panelPaso2);
            this.Controls.Add(this.panelPaso1);
            this.Controls.Add(this.panelStatus);
            this.Controls.Add(this.panelHeader);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NuevoPedidoForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Nuevo Pedido de Venta";
            this.panelHeader.ResumeLayout(false);
            this.panelStatus.ResumeLayout(false);
            this.panelPaso1.ResumeLayout(false);
            this.panelPaso2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvPrendas)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel          panelHeader;
        private System.Windows.Forms.Label          lblPaso;
        private System.Windows.Forms.Panel          panelStatus;
        private System.Windows.Forms.Label          lblMensaje;
        private System.Windows.Forms.Panel          panelPaso1;
        private System.Windows.Forms.Label          lblSeleccionaCliente;
        private System.Windows.Forms.ComboBox       cmbCliente;
        private System.Windows.Forms.Label          lblInfoPlan;
        private System.Windows.Forms.Button         btnSiguiente;
        private System.Windows.Forms.Panel          panelPaso2;
        private System.Windows.Forms.Label          lblInstruccion;
        private System.Windows.Forms.DataGridView   dgvPrendas;
        private System.Windows.Forms.Label          lblResumen;
        private System.Windows.Forms.Button         btnVolver;
        private System.Windows.Forms.Button         btnConfirmar;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Sel;
        private System.Windows.Forms.DataGridViewTextBoxColumn  ID;
        private System.Windows.Forms.DataGridViewTextBoxColumn  Nombre;
        private System.Windows.Forms.DataGridViewTextBoxColumn  Categoria;
        private System.Windows.Forms.DataGridViewTextBoxColumn  Talle;
        private System.Windows.Forms.DataGridViewTextBoxColumn  colColor;
    }
}
