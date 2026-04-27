namespace GUI
{
    partial class Clientes
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
            this.panelTop     = new System.Windows.Forms.Panel();
            this.lblFiltro    = new System.Windows.Forms.Label();
            this.txtFiltro    = new System.Windows.Forms.TextBox();
            this.btnNuevo     = new System.Windows.Forms.Button();
            this.btnEditar    = new System.Windows.Forms.Button();
            this.btnBaja      = new System.Windows.Forms.Button();
            this.btnRefrescar = new System.Windows.Forms.Button();
            this.lblConteo    = new System.Windows.Forms.Label();
            this.dgvClientes  = new System.Windows.Forms.DataGridView();
            this.panelBottom  = new System.Windows.Forms.Panel();
            this.lblMensaje   = new System.Windows.Forms.Label();
            this.panelTop.SuspendLayout();
            this.panelBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvClientes)).BeginInit();
            this.SuspendLayout();

            // panelTop
            this.panelTop.BackColor = System.Drawing.Color.FromArgb(230, 230, 240);
            this.panelTop.Controls.Add(this.lblFiltro);
            this.panelTop.Controls.Add(this.txtFiltro);
            this.panelTop.Controls.Add(this.btnNuevo);
            this.panelTop.Controls.Add(this.btnEditar);
            this.panelTop.Controls.Add(this.btnBaja);
            this.panelTop.Controls.Add(this.btnRefrescar);
            this.panelTop.Controls.Add(this.lblConteo);
            this.panelTop.Dock    = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Height  = 52;
            this.panelTop.Padding = new System.Windows.Forms.Padding(8, 8, 8, 4);
            this.panelTop.Name    = "panelTop";
            this.panelTop.TabIndex = 0;

            // lblFiltro
            this.lblFiltro.Text      = "Buscar:";
            this.lblFiltro.Left      = 8;
            this.lblFiltro.Top       = 16;
            this.lblFiltro.Width     = 48;
            this.lblFiltro.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblFiltro.Name      = "lblFiltro";
            this.lblFiltro.TabIndex  = 0;

            // txtFiltro
            this.txtFiltro.Left     = 58;
            this.txtFiltro.Top      = 13;
            this.txtFiltro.Width    = 220;
            this.txtFiltro.Name     = "txtFiltro";
            this.txtFiltro.TabIndex = 1;
            this.txtFiltro.TextChanged += new System.EventHandler(this.TxtFiltro_TextChanged);

            // btnNuevo
            this.btnNuevo.Text      = "+ Nuevo Cliente";
            this.btnNuevo.Left      = 300;
            this.btnNuevo.Top       = 11;
            this.btnNuevo.Width     = 130;
            this.btnNuevo.Height    = 28;
            this.btnNuevo.BackColor = System.Drawing.Color.FromArgb(210, 100, 135);
            this.btnNuevo.ForeColor = System.Drawing.Color.White;
            this.btnNuevo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNuevo.FlatAppearance.BorderSize = 0;
            this.btnNuevo.Name      = "btnNuevo";
            this.btnNuevo.TabIndex  = 2;
            this.btnNuevo.Click    += new System.EventHandler(this.BtnNuevo_Click);

            // btnEditar
            this.btnEditar.Text      = "\u270e Editar";
            this.btnEditar.Left      = 438;
            this.btnEditar.Top       = 11;
            this.btnEditar.Width     = 90;
            this.btnEditar.Height    = 28;
            this.btnEditar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEditar.Enabled   = false;
            this.btnEditar.Name      = "btnEditar";
            this.btnEditar.TabIndex  = 3;
            this.btnEditar.Click    += new System.EventHandler(this.BtnEditar_Click);

            // btnBaja
            this.btnBaja.Text      = "\u2715 Dar de Baja";
            this.btnBaja.Left      = 536;
            this.btnBaja.Top       = 11;
            this.btnBaja.Width     = 110;
            this.btnBaja.Height    = 28;
            this.btnBaja.BackColor = System.Drawing.Color.FromArgb(200, 60, 60);
            this.btnBaja.ForeColor = System.Drawing.Color.White;
            this.btnBaja.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBaja.FlatAppearance.BorderSize = 0;
            this.btnBaja.Enabled   = false;
            this.btnBaja.Name      = "btnBaja";
            this.btnBaja.TabIndex  = 4;
            this.btnBaja.Click    += new System.EventHandler(this.BtnBaja_Click);

            // btnRefrescar
            this.btnRefrescar.Text      = "\u21bb";
            this.btnRefrescar.Left      = 654;
            this.btnRefrescar.Top       = 11;
            this.btnRefrescar.Width     = 32;
            this.btnRefrescar.Height    = 28;
            this.btnRefrescar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefrescar.Name      = "btnRefrescar";
            this.btnRefrescar.TabIndex  = 5;
            this.btnRefrescar.Click    += new System.EventHandler(this.BtnRefrescar_Click);

            // lblConteo
            this.lblConteo.Left      = 696;
            this.lblConteo.Top       = 16;
            this.lblConteo.Width     = 200;
            this.lblConteo.ForeColor = System.Drawing.Color.DimGray;
            this.lblConteo.Font      = new System.Drawing.Font("Segoe UI", 8.5f);
            this.lblConteo.Name      = "lblConteo";
            this.lblConteo.TabIndex  = 6;

            // dgvClientes
            this.dgvClientes.Dock                  = System.Windows.Forms.DockStyle.Fill;
            this.dgvClientes.ReadOnly               = true;
            this.dgvClientes.AllowUserToAddRows     = false;
            this.dgvClientes.AllowUserToDeleteRows  = false;
            this.dgvClientes.SelectionMode          = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvClientes.AutoSizeColumnsMode    = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvClientes.BackgroundColor        = System.Drawing.Color.White;
            this.dgvClientes.RowHeadersVisible      = false;
            this.dgvClientes.BorderStyle            = System.Windows.Forms.BorderStyle.None;
            this.dgvClientes.Name                   = "dgvClientes";
            this.dgvClientes.TabIndex               = 1;
            this.dgvClientes.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(255, 248, 252);
            this.dgvClientes.DefaultCellStyle.SelectionBackColor       = System.Drawing.Color.FromArgb(255, 182, 193);
            this.dgvClientes.DefaultCellStyle.SelectionForeColor       = System.Drawing.Color.Black;
            this.dgvClientes.SelectionChanged += new System.EventHandler(this.DgvClientes_SelectionChanged);
            this.dgvClientes.CellDoubleClick  += new System.Windows.Forms.DataGridViewCellEventHandler(this.DgvClientes_CellDoubleClick);

            // panelBottom
            this.panelBottom.BackColor = System.Drawing.Color.FromArgb(230, 230, 240);
            this.panelBottom.Dock      = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Height    = 28;
            this.panelBottom.Padding   = new System.Windows.Forms.Padding(8, 4, 8, 4);
            this.panelBottom.Controls.Add(this.lblMensaje);
            this.panelBottom.Name      = "panelBottom";
            this.panelBottom.TabIndex  = 2;

            // lblMensaje
            this.lblMensaje.Dock     = System.Windows.Forms.DockStyle.Fill;
            this.lblMensaje.Font     = new System.Drawing.Font("Segoe UI", 8.5f);
            this.lblMensaje.Name     = "lblMensaje";
            this.lblMensaje.TabIndex = 0;

            // Clientes
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(920, 560);
            this.MinimumSize         = new System.Drawing.Size(780, 460);
            this.Text                = "Gesti\u00f3n de Clientes";
            this.Name                = "Clientes";
            this.Controls.Add(this.dgvClientes);
            this.Controls.Add(this.panelTop);
            this.Controls.Add(this.panelBottom);

            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.panelBottom.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvClientes)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel         panelTop;
        private System.Windows.Forms.Label         lblFiltro;
        private System.Windows.Forms.TextBox       txtFiltro;
        private System.Windows.Forms.Button        btnNuevo;
        private System.Windows.Forms.Button        btnEditar;
        private System.Windows.Forms.Button        btnBaja;
        private System.Windows.Forms.Button        btnRefrescar;
        private System.Windows.Forms.Label         lblConteo;
        private System.Windows.Forms.DataGridView  dgvClientes;
        private System.Windows.Forms.Panel         panelBottom;
        private System.Windows.Forms.Label         lblMensaje;
    }
}
