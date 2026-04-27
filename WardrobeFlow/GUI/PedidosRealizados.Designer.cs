namespace GUI
{
    partial class PedidosRealizados
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle11 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle12 = new System.Windows.Forms.DataGridViewCellStyle();
            this.panelTop = new System.Windows.Forms.Panel();
            this.lblEstado = new System.Windows.Forms.Label();
            this.cmbFiltroEstado = new System.Windows.Forms.ComboBox();
            this.lblUltimos = new System.Windows.Forms.Label();
            this.nudDiasFiltro = new System.Windows.Forms.NumericUpDown();
            this.lblDias = new System.Windows.Forms.Label();
            this.lblConteo = new System.Windows.Forms.Label();
            this.btnDespachar = new System.Windows.Forms.Button();
            this.btnEntregado = new System.Windows.Forms.Button();
            this.btnVerNotificacion = new System.Windows.Forms.Button();
            this.btnRefrescar = new System.Windows.Forms.Button();
            this.panelDetalle = new System.Windows.Forms.Panel();
            this.dgvDetalle = new System.Windows.Forms.DataGridView();
            this.lblDetalleTitulo = new System.Windows.Forms.Label();
            this.panelStatus = new System.Windows.Forms.Panel();
            this.lblMensaje = new System.Windows.Forms.Label();
            this.dgvPedidos = new System.Windows.Forms.DataGridView();
            this.panelTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudDiasFiltro)).BeginInit();
            this.panelDetalle.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDetalle)).BeginInit();
            this.panelStatus.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPedidos)).BeginInit();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(235)))), ((int)(((byte)(245)))));
            this.panelTop.Controls.Add(this.lblEstado);
            this.panelTop.Controls.Add(this.cmbFiltroEstado);
            this.panelTop.Controls.Add(this.lblUltimos);
            this.panelTop.Controls.Add(this.nudDiasFiltro);
            this.panelTop.Controls.Add(this.lblDias);
            this.panelTop.Controls.Add(this.lblConteo);
            this.panelTop.Controls.Add(this.btnDespachar);
            this.panelTop.Controls.Add(this.btnEntregado);
            this.panelTop.Controls.Add(this.btnVerNotificacion);
            this.panelTop.Controls.Add(this.btnRefrescar);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Padding = new System.Windows.Forms.Padding(8, 6, 8, 4);
            this.panelTop.Size = new System.Drawing.Size(1060, 90);
            this.panelTop.TabIndex = 0;
            // 
            // lblEstado
            // 
            this.lblEstado.Location = new System.Drawing.Point(8, 10);
            this.lblEstado.Name = "lblEstado";
            this.lblEstado.Size = new System.Drawing.Size(50, 23);
            this.lblEstado.TabIndex = 0;
            this.lblEstado.Text = "Estado:";
            this.lblEstado.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cmbFiltroEstado
            // 
            this.cmbFiltroEstado.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFiltroEstado.Items.AddRange(new object[] {
            "Todos",
            "Pendiente",
            "Despachado",
            "Entregado",
            "Cancelado"});
            this.cmbFiltroEstado.Location = new System.Drawing.Point(60, 8);
            this.cmbFiltroEstado.Name = "cmbFiltroEstado";
            this.cmbFiltroEstado.Size = new System.Drawing.Size(140, 21);
            this.cmbFiltroEstado.TabIndex = 1;
            this.cmbFiltroEstado.SelectedIndexChanged += new System.EventHandler(this.CmbFiltroEstado_SelectedIndexChanged);
            // 
            // lblUltimos
            // 
            this.lblUltimos.Location = new System.Drawing.Point(212, 10);
            this.lblUltimos.Name = "lblUltimos";
            this.lblUltimos.Size = new System.Drawing.Size(56, 23);
            this.lblUltimos.TabIndex = 2;
            this.lblUltimos.Text = "Últimos:";
            this.lblUltimos.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // nudDiasFiltro
            // 
            this.nudDiasFiltro.Location = new System.Drawing.Point(270, 8);
            this.nudDiasFiltro.Maximum = new decimal(new int[] {
            365,
            0,
            0,
            0});
            this.nudDiasFiltro.Name = "nudDiasFiltro";
            this.nudDiasFiltro.Size = new System.Drawing.Size(60, 20);
            this.nudDiasFiltro.TabIndex = 3;
            this.nudDiasFiltro.ValueChanged += new System.EventHandler(this.NudDiasFiltro_ValueChanged);
            // 
            // lblDias
            // 
            this.lblDias.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblDias.ForeColor = System.Drawing.Color.DimGray;
            this.lblDias.Location = new System.Drawing.Point(334, 10);
            this.lblDias.Name = "lblDias";
            this.lblDias.Size = new System.Drawing.Size(120, 23);
            this.lblDias.TabIndex = 4;
            this.lblDias.Text = "días (0 = todos)";
            // 
            // lblConteo
            // 
            this.lblConteo.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblConteo.ForeColor = System.Drawing.Color.DimGray;
            this.lblConteo.Location = new System.Drawing.Point(460, 10);
            this.lblConteo.Name = "lblConteo";
            this.lblConteo.Size = new System.Drawing.Size(380, 23);
            this.lblConteo.TabIndex = 5;
            this.lblConteo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnDespachar
            // 
            this.btnDespachar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(100)))), ((int)(((byte)(135)))));
            this.btnDespachar.Enabled = false;
            this.btnDespachar.FlatAppearance.BorderSize = 0;
            this.btnDespachar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDespachar.ForeColor = System.Drawing.Color.White;
            this.btnDespachar.Location = new System.Drawing.Point(8, 50);
            this.btnDespachar.Name = "btnDespachar";
            this.btnDespachar.Size = new System.Drawing.Size(130, 28);
            this.btnDespachar.TabIndex = 6;
            this.btnDespachar.Text = "📦 Despachar";
            this.btnDespachar.UseVisualStyleBackColor = false;
            this.btnDespachar.Click += new System.EventHandler(this.BtnDespachar_Click);
            // 
            // btnEntregado
            // 
            this.btnEntregado.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(140)))), ((int)(((byte)(60)))));
            this.btnEntregado.Enabled = false;
            this.btnEntregado.FlatAppearance.BorderSize = 0;
            this.btnEntregado.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEntregado.ForeColor = System.Drawing.Color.White;
            this.btnEntregado.Location = new System.Drawing.Point(146, 50);
            this.btnEntregado.Name = "btnEntregado";
            this.btnEntregado.Size = new System.Drawing.Size(160, 28);
            this.btnEntregado.TabIndex = 7;
            this.btnEntregado.Text = "✓ Marcar Entregado";
            this.btnEntregado.UseVisualStyleBackColor = false;
            this.btnEntregado.Click += new System.EventHandler(this.BtnEntregado_Click);
            // 
            // btnVerNotificacion
            // 
            this.btnVerNotificacion.Enabled = false;
            this.btnVerNotificacion.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVerNotificacion.Location = new System.Drawing.Point(314, 50);
            this.btnVerNotificacion.Name = "btnVerNotificacion";
            this.btnVerNotificacion.Size = new System.Drawing.Size(150, 28);
            this.btnVerNotificacion.TabIndex = 8;
            this.btnVerNotificacion.Text = "✉ Ver Notificación";
            this.btnVerNotificacion.Click += new System.EventHandler(this.BtnVerNotificacion_Click);
            // 
            // btnRefrescar
            // 
            this.btnRefrescar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefrescar.Location = new System.Drawing.Point(472, 50);
            this.btnRefrescar.Name = "btnRefrescar";
            this.btnRefrescar.Size = new System.Drawing.Size(32, 28);
            this.btnRefrescar.TabIndex = 9;
            this.btnRefrescar.Text = "↻";
            this.btnRefrescar.Click += new System.EventHandler(this.BtnRefrescar_Click);
            // 
            // panelDetalle
            // 
            this.panelDetalle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(252)))));
            this.panelDetalle.Controls.Add(this.dgvDetalle);
            this.panelDetalle.Controls.Add(this.lblDetalleTitulo);
            this.panelDetalle.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelDetalle.Location = new System.Drawing.Point(0, 444);
            this.panelDetalle.Name = "panelDetalle";
            this.panelDetalle.Padding = new System.Windows.Forms.Padding(8);
            this.panelDetalle.Size = new System.Drawing.Size(1060, 190);
            this.panelDetalle.TabIndex = 3;
            // 
            // dgvDetalle
            // 
            this.dgvDetalle.AllowUserToAddRows = false;
            this.dgvDetalle.AllowUserToDeleteRows = false;
            dataGridViewCellStyle9.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(248)))), ((int)(((byte)(252)))));
            this.dgvDetalle.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle9;
            this.dgvDetalle.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvDetalle.BackgroundColor = System.Drawing.Color.White;
            this.dgvDetalle.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle10.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle10.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle10.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle10.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(182)))), ((int)(((byte)(193)))));
            dataGridViewCellStyle10.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle10.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvDetalle.DefaultCellStyle = dataGridViewCellStyle10;
            this.dgvDetalle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvDetalle.Location = new System.Drawing.Point(8, 30);
            this.dgvDetalle.Name = "dgvDetalle";
            this.dgvDetalle.ReadOnly = true;
            this.dgvDetalle.RowHeadersVisible = false;
            this.dgvDetalle.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvDetalle.Size = new System.Drawing.Size(1044, 152);
            this.dgvDetalle.TabIndex = 1;
            // 
            // lblDetalleTitulo
            // 
            this.lblDetalleTitulo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.lblDetalleTitulo.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblDetalleTitulo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblDetalleTitulo.Location = new System.Drawing.Point(8, 8);
            this.lblDetalleTitulo.Name = "lblDetalleTitulo";
            this.lblDetalleTitulo.Padding = new System.Windows.Forms.Padding(4, 2, 0, 0);
            this.lblDetalleTitulo.Size = new System.Drawing.Size(1044, 22);
            this.lblDetalleTitulo.TabIndex = 0;
            this.lblDetalleTitulo.Text = "Detalle del pedido seleccionado";
            // 
            // panelStatus
            // 
            this.panelStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(235)))), ((int)(((byte)(245)))));
            this.panelStatus.Controls.Add(this.lblMensaje);
            this.panelStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelStatus.Location = new System.Drawing.Point(0, 634);
            this.panelStatus.Name = "panelStatus";
            this.panelStatus.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);
            this.panelStatus.Size = new System.Drawing.Size(1060, 26);
            this.panelStatus.TabIndex = 2;
            // 
            // lblMensaje
            // 
            this.lblMensaje.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMensaje.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblMensaje.Location = new System.Drawing.Point(8, 4);
            this.lblMensaje.Name = "lblMensaje";
            this.lblMensaje.Size = new System.Drawing.Size(1044, 18);
            this.lblMensaje.TabIndex = 0;
            // 
            // dgvPedidos
            // 
            this.dgvPedidos.AllowUserToAddRows = false;
            this.dgvPedidos.AllowUserToDeleteRows = false;
            dataGridViewCellStyle11.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(248)))), ((int)(((byte)(252)))));
            this.dgvPedidos.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle11;
            this.dgvPedidos.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvPedidos.BackgroundColor = System.Drawing.Color.White;
            this.dgvPedidos.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle12.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle12.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle12.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle12.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle12.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(182)))), ((int)(((byte)(193)))));
            dataGridViewCellStyle12.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle12.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvPedidos.DefaultCellStyle = dataGridViewCellStyle12;
            this.dgvPedidos.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvPedidos.Location = new System.Drawing.Point(0, 90);
            this.dgvPedidos.Name = "dgvPedidos";
            this.dgvPedidos.ReadOnly = true;
            this.dgvPedidos.RowHeadersVisible = false;
            this.dgvPedidos.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvPedidos.Size = new System.Drawing.Size(1060, 354);
            this.dgvPedidos.TabIndex = 1;
            this.dgvPedidos.SelectionChanged += new System.EventHandler(this.DgvPedidos_SelectionChanged);
            // 
            // PedidosRealizados
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1060, 660);
            this.Controls.Add(this.dgvPedidos);
            this.Controls.Add(this.panelDetalle);
            this.Controls.Add(this.panelStatus);
            this.Controls.Add(this.panelTop);
            this.MinimumSize = new System.Drawing.Size(880, 520);
            this.Name = "PedidosRealizados";
            this.Text = "Despacho de Pedidos";
            this.panelTop.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.nudDiasFiltro)).EndInit();
            this.panelDetalle.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDetalle)).EndInit();
            this.panelStatus.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvPedidos)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel        panelTop;
        private System.Windows.Forms.Label        lblEstado;
        private System.Windows.Forms.ComboBox     cmbFiltroEstado;
        private System.Windows.Forms.Label        lblUltimos;
        private System.Windows.Forms.NumericUpDown nudDiasFiltro;
        private System.Windows.Forms.Label        lblDias;
        private System.Windows.Forms.Label        lblConteo;
        private System.Windows.Forms.Button       btnDespachar;
        private System.Windows.Forms.Button       btnEntregado;
        private System.Windows.Forms.Button       btnVerNotificacion;
        private System.Windows.Forms.Button       btnRefrescar;
        private System.Windows.Forms.Panel        panelDetalle;
        private System.Windows.Forms.Label        lblDetalleTitulo;
        private System.Windows.Forms.DataGridView dgvDetalle;
        private System.Windows.Forms.Panel        panelStatus;
        private System.Windows.Forms.Label        lblMensaje;
        private System.Windows.Forms.DataGridView dgvPedidos;
    }
}
