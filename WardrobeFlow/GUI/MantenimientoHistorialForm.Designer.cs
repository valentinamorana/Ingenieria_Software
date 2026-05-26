namespace GUI
{
    partial class MantenimientoHistorialForm
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
            this.lblTitulo      = new System.Windows.Forms.Label();
            this.dgvHistorial   = new System.Windows.Forms.DataGridView();
            this.lblSinRegistros = new System.Windows.Forms.Label();
            this.btnCerrar      = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvHistorial)).BeginInit();
            this.SuspendLayout();

            // lblTitulo
            this.lblTitulo.Dock      = System.Windows.Forms.DockStyle.Top;
            this.lblTitulo.Font      = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.FromArgb(178, 62, 96);
            this.lblTitulo.Height    = 40;
            this.lblTitulo.Padding   = new System.Windows.Forms.Padding(10, 10, 0, 0);
            this.lblTitulo.Name      = "lblTitulo";
            this.lblTitulo.TabIndex  = 0;
            this.lblTitulo.Text      = "Historial de Mantenimiento";

            // dgvHistorial
            this.dgvHistorial.Anchor           = System.Windows.Forms.AnchorStyles.Top
                                               | System.Windows.Forms.AnchorStyles.Bottom
                                               | System.Windows.Forms.AnchorStyles.Left
                                               | System.Windows.Forms.AnchorStyles.Right;
            this.dgvHistorial.Location         = new System.Drawing.Point(12, 48);
            this.dgvHistorial.Size             = new System.Drawing.Size(760, 320);
            this.dgvHistorial.ReadOnly         = true;
            this.dgvHistorial.AllowUserToAddRows    = false;
            this.dgvHistorial.AllowUserToDeleteRows = false;
            this.dgvHistorial.SelectionMode    = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvHistorial.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvHistorial.BackgroundColor  = System.Drawing.Color.White;
            this.dgvHistorial.RowHeadersVisible = false;
            this.dgvHistorial.BorderStyle      = System.Windows.Forms.BorderStyle.None;
            this.dgvHistorial.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(255, 248, 252);
            this.dgvHistorial.DefaultCellStyle.SelectionBackColor       = System.Drawing.Color.FromArgb(255, 182, 193);
            this.dgvHistorial.DefaultCellStyle.SelectionForeColor       = System.Drawing.Color.Black;
            this.dgvHistorial.Name             = "dgvHistorial";
            this.dgvHistorial.TabIndex         = 1;

            // lblSinRegistros
            this.lblSinRegistros.Anchor    = System.Windows.Forms.AnchorStyles.Top
                                           | System.Windows.Forms.AnchorStyles.Left
                                           | System.Windows.Forms.AnchorStyles.Right;
            this.lblSinRegistros.Location  = new System.Drawing.Point(12, 100);
            this.lblSinRegistros.Size      = new System.Drawing.Size(760, 40);
            this.lblSinRegistros.Font      = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblSinRegistros.ForeColor = System.Drawing.Color.DimGray;
            this.lblSinRegistros.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblSinRegistros.Name      = "lblSinRegistros";
            this.lblSinRegistros.TabIndex  = 2;
            this.lblSinRegistros.Visible   = false;

            // btnCerrar
            this.btnCerrar.Anchor      = System.Windows.Forms.AnchorStyles.Bottom
                                       | System.Windows.Forms.AnchorStyles.Right;
            this.btnCerrar.Location    = new System.Drawing.Point(652, 380);
            this.btnCerrar.Size        = new System.Drawing.Size(120, 32);
            this.btnCerrar.FlatStyle   = System.Windows.Forms.FlatStyle.Flat;
            this.btnCerrar.Name        = "btnCerrar";
            this.btnCerrar.TabIndex    = 3;
            this.btnCerrar.Text        = "Cerrar";
            this.btnCerrar.Click      += new System.EventHandler(this.BtnCerrar_Click);

            // MantenimientoHistorialForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(784, 424);
            this.MinimumSize         = new System.Drawing.Size(600, 380);
            this.FormBorderStyle     = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MaximizeBox         = false;
            this.Name                = "MantenimientoHistorialForm";
            this.StartPosition       = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text                = "Historial de Mantenimiento";
            this.Controls.Add(this.lblTitulo);
            this.Controls.Add(this.dgvHistorial);
            this.Controls.Add(this.lblSinRegistros);
            this.Controls.Add(this.btnCerrar);

            ((System.ComponentModel.ISupportInitialize)(this.dgvHistorial)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label         lblTitulo;
        private System.Windows.Forms.DataGridView  dgvHistorial;
        private System.Windows.Forms.Label         lblSinRegistros;
        private System.Windows.Forms.Button        btnCerrar;
    }
}
