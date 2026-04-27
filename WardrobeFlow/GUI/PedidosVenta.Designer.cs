namespace GUI
{
    partial class PedidosVenta
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.panelTop = new System.Windows.Forms.Panel();
            this.btnNuevoPedido = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.btnDesCancelar = new System.Windows.Forms.Button();
            this.btnRefrescar = new System.Windows.Forms.Button();
            this.lblConteo = new System.Windows.Forms.Label();
            this.panelDetalle = new System.Windows.Forms.Panel();
            this.dgvDetallePrendas = new System.Windows.Forms.DataGridView();
            this.lblDetalleTitulo = new System.Windows.Forms.Label();
            this.panelStatus = new System.Windows.Forms.Panel();
            this.lblMensaje = new System.Windows.Forms.Label();
            this.dgvPedidos = new System.Windows.Forms.DataGridView();
            this.panelTop.SuspendLayout();
            this.panelDetalle.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDetallePrendas)).BeginInit();
            this.panelStatus.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPedidos)).BeginInit();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(240)))));
            this.panelTop.Controls.Add(this.btnNuevoPedido);
            this.panelTop.Controls.Add(this.btnCancelar);
            this.panelTop.Controls.Add(this.btnDesCancelar);
            this.panelTop.Controls.Add(this.btnRefrescar);
            this.panelTop.Controls.Add(this.lblConteo);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Padding = new System.Windows.Forms.Padding(8, 8, 8, 4);
            this.panelTop.Size = new System.Drawing.Size(1000, 52);
            this.panelTop.TabIndex = 0;
            // 
            // btnNuevoPedido
            // 
            this.btnNuevoPedido.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(100)))), ((int)(((byte)(135)))));
            this.btnNuevoPedido.FlatAppearance.BorderSize = 0;
            this.btnNuevoPedido.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNuevoPedido.ForeColor = System.Drawing.Color.White;
            this.btnNuevoPedido.Location = new System.Drawing.Point(8, 11);
            this.btnNuevoPedido.Name = "btnNuevoPedido";
            this.btnNuevoPedido.Size = new System.Drawing.Size(140, 28);
            this.btnNuevoPedido.TabIndex = 0;
            this.btnNuevoPedido.Text = "+ Nuevo Pedido";
            this.btnNuevoPedido.UseVisualStyleBackColor = false;
            this.btnNuevoPedido.Click += new System.EventHandler(this.BtnNuevoPedido_Click);
            // 
            // btnCancelar
            // 
            this.btnCancelar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.btnCancelar.Enabled = false;
            this.btnCancelar.FlatAppearance.BorderSize = 0;
            this.btnCancelar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancelar.ForeColor = System.Drawing.Color.White;
            this.btnCancelar.Location = new System.Drawing.Point(156, 11);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(110, 28);
            this.btnCancelar.TabIndex = 1;
            this.btnCancelar.Text = "✕ Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = false;
            this.btnCancelar.Click += new System.EventHandler(this.BtnCancelarPedido_Click);
            // 
            // btnDesCancelar
            // 
            this.btnDesCancelar.BackColor = System.Drawing.Color.Purple;
            this.btnDesCancelar.Enabled = false;
            this.btnDesCancelar.FlatAppearance.BorderSize = 0;
            this.btnDesCancelar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDesCancelar.ForeColor = System.Drawing.Color.White;
            this.btnDesCancelar.Location = new System.Drawing.Point(274, 11);
            this.btnDesCancelar.Name = "btnDesCancelar";
            this.btnDesCancelar.Size = new System.Drawing.Size(130, 28);
            this.btnDesCancelar.TabIndex = 2;
            this.btnDesCancelar.Text = "↩ Des-cancelar";
            this.btnDesCancelar.UseVisualStyleBackColor = false;
            this.btnDesCancelar.Click += new System.EventHandler(this.BtnDesCancelarPedido_Click);
            // 
            // btnRefrescar
            // 
            this.btnRefrescar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefrescar.Location = new System.Drawing.Point(412, 11);
            this.btnRefrescar.Name = "btnRefrescar";
            this.btnRefrescar.Size = new System.Drawing.Size(32, 28);
            this.btnRefrescar.TabIndex = 3;
            this.btnRefrescar.Text = "↻";
            this.btnRefrescar.Click += new System.EventHandler(this.BtnRefrescar_Click);
            // 
            // lblConteo
            // 
            this.lblConteo.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblConteo.ForeColor = System.Drawing.Color.DimGray;
            this.lblConteo.Location = new System.Drawing.Point(452, 16);
            this.lblConteo.Name = "lblConteo";
            this.lblConteo.Size = new System.Drawing.Size(300, 23);
            this.lblConteo.TabIndex = 4;
            // 
            // panelDetalle
            // 
            this.panelDetalle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(245)))), ((int)(((byte)(250)))));
            this.panelDetalle.Controls.Add(this.dgvDetallePrendas);
            this.panelDetalle.Controls.Add(this.lblDetalleTitulo);
            this.panelDetalle.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelDetalle.Location = new System.Drawing.Point(0, 394);
            this.panelDetalle.Name = "panelDetalle";
            this.panelDetalle.Padding = new System.Windows.Forms.Padding(8);
            this.panelDetalle.Size = new System.Drawing.Size(1000, 180);
            this.panelDetalle.TabIndex = 3;
            // 
            // dgvDetallePrendas
            // 
            this.dgvDetallePrendas.AllowUserToAddRows = false;
            this.dgvDetallePrendas.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(248)))), ((int)(((byte)(252)))));
            this.dgvDetallePrendas.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvDetallePrendas.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvDetallePrendas.BackgroundColor = System.Drawing.Color.White;
            this.dgvDetallePrendas.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(182)))), ((int)(((byte)(193)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvDetallePrendas.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvDetallePrendas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvDetallePrendas.Location = new System.Drawing.Point(8, 30);
            this.dgvDetallePrendas.Name = "dgvDetallePrendas";
            this.dgvDetallePrendas.ReadOnly = true;
            this.dgvDetallePrendas.RowHeadersVisible = false;
            this.dgvDetallePrendas.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvDetallePrendas.Size = new System.Drawing.Size(984, 142);
            this.dgvDetallePrendas.TabIndex = 1;
            // 
            // lblDetalleTitulo
            // 
            this.lblDetalleTitulo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.lblDetalleTitulo.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblDetalleTitulo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblDetalleTitulo.Location = new System.Drawing.Point(8, 8);
            this.lblDetalleTitulo.Name = "lblDetalleTitulo";
            this.lblDetalleTitulo.Padding = new System.Windows.Forms.Padding(4, 2, 0, 0);
            this.lblDetalleTitulo.Size = new System.Drawing.Size(984, 22);
            this.lblDetalleTitulo.TabIndex = 0;
            this.lblDetalleTitulo.Text = "Prendas del pedido seleccionado";
            // 
            // panelStatus
            // 
            this.panelStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(240)))));
            this.panelStatus.Controls.Add(this.lblMensaje);
            this.panelStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelStatus.Location = new System.Drawing.Point(0, 574);
            this.panelStatus.Name = "panelStatus";
            this.panelStatus.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);
            this.panelStatus.Size = new System.Drawing.Size(1000, 26);
            this.panelStatus.TabIndex = 2;
            // 
            // lblMensaje
            // 
            this.lblMensaje.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMensaje.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblMensaje.Location = new System.Drawing.Point(8, 4);
            this.lblMensaje.Name = "lblMensaje";
            this.lblMensaje.Size = new System.Drawing.Size(984, 18);
            this.lblMensaje.TabIndex = 0;
            // 
            // dgvPedidos
            // 
            this.dgvPedidos.AllowUserToAddRows = false;
            this.dgvPedidos.AllowUserToDeleteRows = false;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(248)))), ((int)(((byte)(252)))));
            this.dgvPedidos.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle3;
            this.dgvPedidos.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvPedidos.BackgroundColor = System.Drawing.Color.White;
            this.dgvPedidos.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(182)))), ((int)(((byte)(193)))));
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvPedidos.DefaultCellStyle = dataGridViewCellStyle4;
            this.dgvPedidos.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvPedidos.Location = new System.Drawing.Point(0, 52);
            this.dgvPedidos.Name = "dgvPedidos";
            this.dgvPedidos.ReadOnly = true;
            this.dgvPedidos.RowHeadersVisible = false;
            this.dgvPedidos.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvPedidos.Size = new System.Drawing.Size(1000, 342);
            this.dgvPedidos.TabIndex = 1;
            this.dgvPedidos.SelectionChanged += new System.EventHandler(this.DgvPedidos_SelectionChanged);
            // 
            // PedidosVenta
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 600);
            this.Controls.Add(this.dgvPedidos);
            this.Controls.Add(this.panelDetalle);
            this.Controls.Add(this.panelStatus);
            this.Controls.Add(this.panelTop);
            this.MinimumSize = new System.Drawing.Size(820, 480);
            this.Name = "PedidosVenta";
            this.Text = "Pedidos de Venta";
            this.panelTop.ResumeLayout(false);
            this.panelDetalle.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDetallePrendas)).EndInit();
            this.panelStatus.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvPedidos)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel        panelTop;
        private System.Windows.Forms.Button       btnNuevoPedido;
        private System.Windows.Forms.Button       btnCancelar;
        private System.Windows.Forms.Button       btnDesCancelar;
        private System.Windows.Forms.Button       btnRefrescar;
        private System.Windows.Forms.Label        lblConteo;
        private System.Windows.Forms.Panel        panelDetalle;
        private System.Windows.Forms.Label        lblDetalleTitulo;
        private System.Windows.Forms.DataGridView dgvDetallePrendas;
        private System.Windows.Forms.Panel        panelStatus;
        private System.Windows.Forms.Label        lblMensaje;
        private System.Windows.Forms.DataGridView dgvPedidos;
    }
}
