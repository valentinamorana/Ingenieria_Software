namespace GUI
{
    partial class Prendas
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
            this.panelTop           = new System.Windows.Forms.Panel();
            this.lblEstado          = new System.Windows.Forms.Label();
            this.cmbEstadoFiltro    = new System.Windows.Forms.ComboBox();
            this.lblBuscar          = new System.Windows.Forms.Label();
            this.txtFiltro          = new System.Windows.Forms.TextBox();
            this.btnNueva           = new System.Windows.Forms.Button();
            this.btnEditar          = new System.Windows.Forms.Button();
            this.btnCambiarEstado   = new System.Windows.Forms.Button();
            this.btnRefrescar       = new System.Windows.Forms.Button();
            this.lblConteo          = new System.Windows.Forms.Label();
            this.panelDetalle       = new System.Windows.Forms.Panel();
            this.lblDetalleTitulo   = new System.Windows.Forms.Label();
            this.lblDetalleContenido = new System.Windows.Forms.Label();
            this.panelStatus        = new System.Windows.Forms.Panel();
            this.lblMensaje         = new System.Windows.Forms.Label();
            this.dgvPrendas         = new System.Windows.Forms.DataGridView();
            this.panelTop.SuspendLayout();
            this.panelDetalle.SuspendLayout();
            this.panelStatus.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPrendas)).BeginInit();
            this.SuspendLayout();

            // panelTop
            this.panelTop.BackColor = System.Drawing.Color.FromArgb(230, 230, 240);
            this.panelTop.Dock      = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Height    = 56;
            this.panelTop.Padding   = new System.Windows.Forms.Padding(8, 8, 8, 4);
            this.panelTop.Controls.Add(this.lblEstado);
            this.panelTop.Controls.Add(this.cmbEstadoFiltro);
            this.panelTop.Controls.Add(this.lblBuscar);
            this.panelTop.Controls.Add(this.txtFiltro);
            this.panelTop.Controls.Add(this.btnNueva);
            this.panelTop.Controls.Add(this.btnEditar);
            this.panelTop.Controls.Add(this.btnCambiarEstado);
            this.panelTop.Controls.Add(this.btnRefrescar);
            this.panelTop.Controls.Add(this.lblConteo);
            this.panelTop.Name      = "panelTop";
            this.panelTop.TabIndex  = 0;

            // lblEstado
            this.lblEstado.Text      = "Estado:";
            this.lblEstado.Left      = 8;
            this.lblEstado.Top       = 18;
            this.lblEstado.Width     = 48;
            this.lblEstado.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblEstado.Name      = "lblEstado";
            this.lblEstado.TabIndex  = 0;

            // cmbEstadoFiltro
            this.cmbEstadoFiltro.Left          = 58;
            this.cmbEstadoFiltro.Top           = 15;
            this.cmbEstadoFiltro.Width         = 130;
            this.cmbEstadoFiltro.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbEstadoFiltro.Items.AddRange(new object[] { "Todos", "Disponible", "En Uso", "En Limpieza", "Baja" });
            this.cmbEstadoFiltro.SelectedIndex  = 0;
            this.cmbEstadoFiltro.Name           = "cmbEstadoFiltro";
            this.cmbEstadoFiltro.TabIndex       = 1;
            this.cmbEstadoFiltro.SelectedIndexChanged += new System.EventHandler(this.CmbEstadoFiltro_SelectedIndexChanged);

            // lblBuscar
            this.lblBuscar.Text      = "Buscar:";
            this.lblBuscar.Left      = 200;
            this.lblBuscar.Top       = 18;
            this.lblBuscar.Width     = 50;
            this.lblBuscar.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblBuscar.Name      = "lblBuscar";
            this.lblBuscar.TabIndex  = 2;

            // txtFiltro
            this.txtFiltro.Left     = 252;
            this.txtFiltro.Top      = 15;
            this.txtFiltro.Width    = 200;
            this.txtFiltro.Name     = "txtFiltro";
            this.txtFiltro.TabIndex = 3;
            this.txtFiltro.TextChanged += new System.EventHandler(this.TxtFiltro_TextChanged);

            // btnNueva
            this.btnNueva.Text      = "+ Nueva Prenda";
            this.btnNueva.Left      = 470;
            this.btnNueva.Top       = 13;
            this.btnNueva.Width     = 130;
            this.btnNueva.Height    = 28;
            this.btnNueva.BackColor = System.Drawing.Color.FromArgb(210, 100, 135);
            this.btnNueva.ForeColor = System.Drawing.Color.White;
            this.btnNueva.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNueva.FlatAppearance.BorderSize = 0;
            this.btnNueva.Name      = "btnNueva";
            this.btnNueva.TabIndex  = 4;
            this.btnNueva.Click    += new System.EventHandler(this.BtnNueva_Click);

            // btnEditar
            this.btnEditar.Text      = "\u270e Editar";
            this.btnEditar.Left      = 610;
            this.btnEditar.Top       = 13;
            this.btnEditar.Width     = 80;
            this.btnEditar.Height    = 28;
            this.btnEditar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEditar.Enabled   = false;
            this.btnEditar.Name      = "btnEditar";
            this.btnEditar.TabIndex  = 5;
            this.btnEditar.Click    += new System.EventHandler(this.BtnEditar_Click);

            // btnCambiarEstado
            this.btnCambiarEstado.Text      = "\u21c4 Estado";
            this.btnCambiarEstado.Left      = 698;
            this.btnCambiarEstado.Top       = 13;
            this.btnCambiarEstado.Width     = 90;
            this.btnCambiarEstado.Height    = 28;
            this.btnCambiarEstado.BackColor = System.Drawing.Color.FromArgb(100, 140, 80);
            this.btnCambiarEstado.ForeColor = System.Drawing.Color.White;
            this.btnCambiarEstado.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCambiarEstado.FlatAppearance.BorderSize = 0;
            this.btnCambiarEstado.Enabled   = false;
            this.btnCambiarEstado.Name      = "btnCambiarEstado";
            this.btnCambiarEstado.TabIndex  = 6;
            this.btnCambiarEstado.Click    += new System.EventHandler(this.BtnCambiarEstado_Click);

            // btnRefrescar
            this.btnRefrescar.Text      = "\u21bb";
            this.btnRefrescar.Left      = 796;
            this.btnRefrescar.Top       = 13;
            this.btnRefrescar.Width     = 32;
            this.btnRefrescar.Height    = 28;
            this.btnRefrescar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefrescar.Name      = "btnRefrescar";
            this.btnRefrescar.TabIndex  = 7;
            this.btnRefrescar.Click    += new System.EventHandler(this.BtnRefrescar_Click);

            // lblConteo
            this.lblConteo.Left      = 836;
            this.lblConteo.Top       = 18;
            this.lblConteo.Width     = 200;
            this.lblConteo.ForeColor = System.Drawing.Color.DimGray;
            this.lblConteo.Font      = new System.Drawing.Font("Segoe UI", 8.5f);
            this.lblConteo.Name      = "lblConteo";
            this.lblConteo.TabIndex  = 8;

            // panelDetalle
            this.panelDetalle.BackColor = System.Drawing.Color.FromArgb(255, 252, 235);
            this.panelDetalle.Dock      = System.Windows.Forms.DockStyle.Bottom;
            this.panelDetalle.Height    = 70;
            this.panelDetalle.Padding   = new System.Windows.Forms.Padding(10, 6, 10, 6);
            this.panelDetalle.Visible   = false;
            this.panelDetalle.Controls.Add(this.lblDetalleTitulo);
            this.panelDetalle.Controls.Add(this.lblDetalleContenido);
            this.panelDetalle.Name      = "panelDetalle";
            this.panelDetalle.TabIndex  = 3;

            // lblDetalleTitulo
            this.lblDetalleTitulo.Text     = "Cliente en uso:";
            this.lblDetalleTitulo.Font     = new System.Drawing.Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Bold);
            this.lblDetalleTitulo.Left     = 10;
            this.lblDetalleTitulo.Top      = 8;
            this.lblDetalleTitulo.Width    = 120;
            this.lblDetalleTitulo.Name     = "lblDetalleTitulo";
            this.lblDetalleTitulo.TabIndex = 0;

            // lblDetalleContenido
            this.lblDetalleContenido.Left      = 130;
            this.lblDetalleContenido.Top       = 8;
            this.lblDetalleContenido.Width     = 800;
            this.lblDetalleContenido.Height    = 50;
            this.lblDetalleContenido.Font      = new System.Drawing.Font("Segoe UI", 8.5f);
            this.lblDetalleContenido.ForeColor = System.Drawing.Color.FromArgb(100, 60, 0);
            this.lblDetalleContenido.Name      = "lblDetalleContenido";
            this.lblDetalleContenido.TabIndex  = 1;

            // panelStatus
            this.panelStatus.BackColor = System.Drawing.Color.FromArgb(230, 230, 240);
            this.panelStatus.Dock      = System.Windows.Forms.DockStyle.Bottom;
            this.panelStatus.Height    = 26;
            this.panelStatus.Padding   = new System.Windows.Forms.Padding(8, 4, 8, 4);
            this.panelStatus.Controls.Add(this.lblMensaje);
            this.panelStatus.Name      = "panelStatus";
            this.panelStatus.TabIndex  = 2;

            // lblMensaje
            this.lblMensaje.Dock     = System.Windows.Forms.DockStyle.Fill;
            this.lblMensaje.Font     = new System.Drawing.Font("Segoe UI", 8.5f);
            this.lblMensaje.Name     = "lblMensaje";
            this.lblMensaje.TabIndex = 0;

            // dgvPrendas
            this.dgvPrendas.Dock               = System.Windows.Forms.DockStyle.Fill;
            this.dgvPrendas.ReadOnly            = true;
            this.dgvPrendas.AllowUserToAddRows  = false;
            this.dgvPrendas.AllowUserToDeleteRows = false;
            this.dgvPrendas.SelectionMode       = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvPrendas.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvPrendas.BackgroundColor     = System.Drawing.Color.White;
            this.dgvPrendas.RowHeadersVisible   = false;
            this.dgvPrendas.BorderStyle         = System.Windows.Forms.BorderStyle.None;
            this.dgvPrendas.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(255, 248, 252);
            this.dgvPrendas.DefaultCellStyle.SelectionBackColor       = System.Drawing.Color.FromArgb(255, 182, 193);
            this.dgvPrendas.DefaultCellStyle.SelectionForeColor       = System.Drawing.Color.Black;
            this.dgvPrendas.Name               = "dgvPrendas";
            this.dgvPrendas.TabIndex           = 1;
            this.dgvPrendas.SelectionChanged  += new System.EventHandler(this.DgvPrendas_SelectionChanged);
            this.dgvPrendas.CellDoubleClick   += new System.Windows.Forms.DataGridViewCellEventHandler(this.DgvPrendas_CellDoubleClick);

            // Prendas
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(1000, 580);
            this.MinimumSize         = new System.Drawing.Size(820, 460);
            this.Text                = "Cat\u00e1logo de Prendas";
            this.Name                = "Prendas";
            this.Controls.Add(this.dgvPrendas);
            this.Controls.Add(this.panelDetalle);
            this.Controls.Add(this.panelStatus);
            this.Controls.Add(this.panelTop);

            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.panelDetalle.ResumeLayout(false);
            this.panelStatus.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvPrendas)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel        panelTop;
        private System.Windows.Forms.Label        lblEstado;
        private System.Windows.Forms.ComboBox     cmbEstadoFiltro;
        private System.Windows.Forms.Label        lblBuscar;
        private System.Windows.Forms.TextBox      txtFiltro;
        private System.Windows.Forms.Button       btnNueva;
        private System.Windows.Forms.Button       btnEditar;
        private System.Windows.Forms.Button       btnCambiarEstado;
        private System.Windows.Forms.Button       btnRefrescar;
        private System.Windows.Forms.Label        lblConteo;
        private System.Windows.Forms.Panel        panelDetalle;
        private System.Windows.Forms.Label        lblDetalleTitulo;
        private System.Windows.Forms.Label        lblDetalleContenido;
        private System.Windows.Forms.Panel        panelStatus;
        private System.Windows.Forms.Label        lblMensaje;
        private System.Windows.Forms.DataGridView dgvPrendas;
    }
}
