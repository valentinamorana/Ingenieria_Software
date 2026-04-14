namespace GUI
{
    partial class frmGestorBitacora
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
            this.dgvBitacora  = new System.Windows.Forms.DataGridView();
            this.colFecha     = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colUsuario   = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colOperacion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colModulo    = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDescripcion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colExitoso   = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnActualizar = new System.Windows.Forms.Button();
            this.btnCerrar    = new System.Windows.Forms.Button();
            this.lblTotal     = new System.Windows.Forms.Label();
            this.lblFiltro    = new System.Windows.Forms.Label();
            this.txtFiltro    = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBitacora)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvBitacora
            // 
            this.dgvBitacora.AllowUserToAddRows    = false;
            this.dgvBitacora.AllowUserToDeleteRows = false;
            this.dgvBitacora.ReadOnly              = true;
            this.dgvBitacora.AutoSizeColumnsMode   = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvBitacora.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvBitacora.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                this.colFecha,
                this.colUsuario,
                this.colOperacion,
                this.colModulo,
                this.colDescripcion,
                this.colExitoso });
            this.dgvBitacora.Location = new System.Drawing.Point(12, 50);
            this.dgvBitacora.Name     = "dgvBitacora";
            this.dgvBitacora.Size     = new System.Drawing.Size(860, 420);
            this.dgvBitacora.TabIndex = 0;
            // 
            // colFecha
            // 
            this.colFecha.HeaderText = "Fecha / Hora";
            this.colFecha.Name       = "colFecha";
            // 
            // colUsuario
            // 
            this.colUsuario.HeaderText = "Usuario";
            this.colUsuario.Name       = "colUsuario";
            // 
            // colOperacion
            // 
            this.colOperacion.HeaderText = "Operacion";
            this.colOperacion.Name       = "colOperacion";
            // 
            // colModulo
            // 
            this.colModulo.HeaderText = "Modulo";
            this.colModulo.Name       = "colModulo";
            // 
            // colDescripcion
            // 
            this.colDescripcion.HeaderText = "Descripcion";
            this.colDescripcion.Name       = "colDescripcion";
            // 
            // colExitoso
            // 
            this.colExitoso.HeaderText = "Exitoso";
            this.colExitoso.Name       = "colExitoso";
            this.colExitoso.Width      = 60;
            // 
            // lblFiltro
            // 
            this.lblFiltro.AutoSize = true;
            this.lblFiltro.Location = new System.Drawing.Point(12, 18);
            this.lblFiltro.Name     = "lblFiltro";
            this.lblFiltro.Text     = "Filtrar:";
            // 
            // txtFiltro
            // 
            this.txtFiltro.Location     = new System.Drawing.Point(60, 15);
            this.txtFiltro.Name         = "txtFiltro";
            this.txtFiltro.Size         = new System.Drawing.Size(250, 22);
            this.txtFiltro.TabIndex     = 1;
            this.txtFiltro.TextChanged += new System.EventHandler(this.txtFiltro_TextChanged);
            // 
            // lblTotal
            // 
            this.lblTotal.AutoSize = true;
            this.lblTotal.Location = new System.Drawing.Point(12, 480);
            this.lblTotal.Name     = "lblTotal";
            this.lblTotal.Text     = "Total de eventos: 0";
            // 
            // btnActualizar
            // 
            this.btnActualizar.Location = new System.Drawing.Point(700, 478);
            this.btnActualizar.Name     = "btnActualizar";
            this.btnActualizar.Size     = new System.Drawing.Size(85, 30);
            this.btnActualizar.TabIndex = 2;
            this.btnActualizar.Text     = "Actualizar";
            this.btnActualizar.Click   += new System.EventHandler(this.btnActualizar_Click);
            // 
            // btnCerrar
            // 
            this.btnCerrar.Location = new System.Drawing.Point(795, 478);
            this.btnCerrar.Name     = "btnCerrar";
            this.btnCerrar.Size     = new System.Drawing.Size(77, 30);
            this.btnCerrar.TabIndex = 3;
            this.btnCerrar.Text     = "Cerrar";
            this.btnCerrar.Click   += new System.EventHandler(this.btnCerrar_Click);
            // 
            // frmGestorBitacora
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(884, 521);
            this.Controls.Add(this.lblFiltro);
            this.Controls.Add(this.txtFiltro);
            this.Controls.Add(this.dgvBitacora);
            this.Controls.Add(this.lblTotal);
            this.Controls.Add(this.btnActualizar);
            this.Controls.Add(this.btnCerrar);
            this.Name            = "frmGestorBitacora";
            this.Text            = "Bitacora de Eventos";
            ((System.ComponentModel.ISupportInitialize)(this.dgvBitacora)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.DataGridView              dgvBitacora;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFecha;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUsuario;
        private System.Windows.Forms.DataGridViewTextBoxColumn colOperacion;
        private System.Windows.Forms.DataGridViewTextBoxColumn colModulo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDescripcion;
        private System.Windows.Forms.DataGridViewTextBoxColumn colExitoso;
        private System.Windows.Forms.Button                    btnActualizar;
        private System.Windows.Forms.Button                    btnCerrar;
        private System.Windows.Forms.Label                     lblTotal;
        private System.Windows.Forms.Label                     lblFiltro;
        private System.Windows.Forms.TextBox                   txtFiltro;
    }
}
