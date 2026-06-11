namespace GUI
{
    partial class FormIdiomas
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
            this.lblTituloIdiomas     = new System.Windows.Forms.Label();
            this.dgvIdiomas           = new System.Windows.Forms.DataGridView();
            this.btnActivar           = new System.Windows.Forms.Button();
            this.btnDesactivar        = new System.Windows.Forms.Button();
            this.lblTituloTrad        = new System.Windows.Forms.Label();
            this.dgvTraducciones      = new System.Windows.Forms.DataGridView();
            this.lblTituloControles   = new System.Windows.Forms.Label();
            this.dgvControles         = new System.Windows.Forms.DataGridView();
            this.btnGuardar           = new System.Windows.Forms.Button();
            this.lblMensaje           = new System.Windows.Forms.Label();
            this.panelIdiomas         = new System.Windows.Forms.Panel();
            this.panelTrad            = new System.Windows.Forms.Panel();
            this.panelBotonesIdioma   = new System.Windows.Forms.Panel();
            this.panelBottom          = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.dgvIdiomas)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTraducciones)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvControles)).BeginInit();
            this.panelIdiomas.SuspendLayout();
            this.panelTrad.SuspendLayout();
            this.panelBotonesIdioma.SuspendLayout();
            this.panelBottom.SuspendLayout();
            this.SuspendLayout();
            //
            // lblTituloIdiomas
            //
            this.lblTituloIdiomas.AutoSize  = true;
            this.lblTituloIdiomas.Font      = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblTituloIdiomas.ForeColor = System.Drawing.Color.FromArgb(180, 60, 100);
            this.lblTituloIdiomas.Location  = new System.Drawing.Point(8, 8);
            this.lblTituloIdiomas.Name      = "lblTituloIdiomas";
            this.lblTituloIdiomas.Size      = new System.Drawing.Size(130, 19);
            this.lblTituloIdiomas.Text      = "Idiomas del sistema";
            //
            // dgvIdiomas
            //
            this.dgvIdiomas.Anchor          = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvIdiomas.BackgroundColor = System.Drawing.Color.White;
            this.dgvIdiomas.BorderStyle     = System.Windows.Forms.BorderStyle.FixedSingle;
            this.dgvIdiomas.Location        = new System.Drawing.Point(8, 35);
            this.dgvIdiomas.Name            = "dgvIdiomas";
            this.dgvIdiomas.Size            = new System.Drawing.Size(860, 120);
            this.dgvIdiomas.TabIndex        = 0;
            this.dgvIdiomas.SelectionChanged += new System.EventHandler(this.DgvIdiomas_SelectionChanged);
            //
            // panelBotonesIdioma
            //
            this.panelBotonesIdioma.Controls.Add(this.btnActivar);
            this.panelBotonesIdioma.Controls.Add(this.btnDesactivar);
            this.panelBotonesIdioma.Dock     = System.Windows.Forms.DockStyle.None;
            this.panelBotonesIdioma.Location = new System.Drawing.Point(8, 163);
            this.panelBotonesIdioma.Name     = "panelBotonesIdioma";
            // Ancho para 4 botones en fila: Activar(0) Desactivar(140) Nuevo idioma(280) Renombrar(420..540)
            this.panelBotonesIdioma.Size     = new System.Drawing.Size(560, 36);
            //
            // btnActivar
            //
            this.btnActivar.BackColor = System.Drawing.Color.FromArgb(80, 160, 80);
            this.btnActivar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnActivar.ForeColor = System.Drawing.Color.White;
            this.btnActivar.Location  = new System.Drawing.Point(0, 4);
            this.btnActivar.Name      = "btnActivar";
            this.btnActivar.Size      = new System.Drawing.Size(130, 28);
            this.btnActivar.TabIndex  = 1;
            this.btnActivar.Text      = "✔ Activar";
            this.btnActivar.Click    += new System.EventHandler(this.BtnActivar_Click);
            //
            // btnDesactivar
            //
            this.btnDesactivar.BackColor = System.Drawing.Color.FromArgb(200, 80, 80);
            this.btnDesactivar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDesactivar.ForeColor = System.Drawing.Color.White;
            this.btnDesactivar.Location  = new System.Drawing.Point(140, 4);
            this.btnDesactivar.Name      = "btnDesactivar";
            this.btnDesactivar.Size      = new System.Drawing.Size(130, 28);
            this.btnDesactivar.TabIndex  = 2;
            this.btnDesactivar.Text      = "✕ Desactivar";
            this.btnDesactivar.Click    += new System.EventHandler(this.BtnDesactivar_Click);
            //
            // panelIdiomas
            //
            this.panelIdiomas.BackColor = System.Drawing.Color.FromArgb(252, 228, 235);
            this.panelIdiomas.Controls.Add(this.lblTituloIdiomas);
            this.panelIdiomas.Controls.Add(this.dgvIdiomas);
            this.panelIdiomas.Controls.Add(this.panelBotonesIdioma);
            this.panelIdiomas.Dock      = System.Windows.Forms.DockStyle.Top;
            this.panelIdiomas.Name      = "panelIdiomas";
            this.panelIdiomas.Padding   = new System.Windows.Forms.Padding(8);
            this.panelIdiomas.Size      = new System.Drawing.Size(880, 210);
            this.panelIdiomas.TabIndex  = 0;
            //
            // lblTituloTrad
            //
            this.lblTituloTrad.AutoSize  = true;
            this.lblTituloTrad.Font      = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblTituloTrad.ForeColor = System.Drawing.Color.FromArgb(180, 60, 100);
            this.lblTituloTrad.Location  = new System.Drawing.Point(8, 8);
            this.lblTituloTrad.Name      = "lblTituloTrad";
            this.lblTituloTrad.Size      = new System.Drawing.Size(200, 19);
            this.lblTituloTrad.Text      = "Traducciones del idioma seleccionado";
            //
            // dgvTraducciones
            //
            this.dgvTraducciones.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left)));
            this.dgvTraducciones.BackgroundColor = System.Drawing.Color.White;
            this.dgvTraducciones.BorderStyle     = System.Windows.Forms.BorderStyle.FixedSingle;
            this.dgvTraducciones.Location        = new System.Drawing.Point(8, 35);
            this.dgvTraducciones.Name            = "dgvTraducciones";
            this.dgvTraducciones.Size            = new System.Drawing.Size(560, 305);
            this.dgvTraducciones.TabIndex        = 3;
            //
            // lblTituloControles
            //
            this.lblTituloControles.Anchor    = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.lblTituloControles.AutoSize  = true;
            this.lblTituloControles.Font      = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblTituloControles.ForeColor = System.Drawing.Color.FromArgb(180, 60, 100);
            this.lblTituloControles.Location  = new System.Drawing.Point(576, 8);
            this.lblTituloControles.Name      = "lblTituloControles";
            this.lblTituloControles.Size      = new System.Drawing.Size(160, 19);
            this.lblTituloControles.Text      = "Controles traducibles";
            //
            // dgvControles
            //
            this.dgvControles.Anchor          = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvControles.BackgroundColor = System.Drawing.Color.White;
            this.dgvControles.BorderStyle     = System.Windows.Forms.BorderStyle.FixedSingle;
            this.dgvControles.Location        = new System.Drawing.Point(576, 35);
            this.dgvControles.Name            = "dgvControles";
            this.dgvControles.Size            = new System.Drawing.Size(292, 305);
            this.dgvControles.TabIndex        = 6;
            //
            // btnGuardar
            //
            this.btnGuardar.Anchor    = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            this.btnGuardar.BackColor = System.Drawing.Color.FromArgb(210, 100, 135);
            this.btnGuardar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGuardar.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnGuardar.ForeColor = System.Drawing.Color.White;
            this.btnGuardar.Location  = new System.Drawing.Point(8, 347);
            this.btnGuardar.Name      = "btnGuardar";
            this.btnGuardar.Size      = new System.Drawing.Size(138, 30);
            this.btnGuardar.TabIndex  = 4;
            this.btnGuardar.Text      = "💾 Guardar cambios";
            this.btnGuardar.Click    += new System.EventHandler(this.BtnGuardar_Click);
            //
            // panelTrad
            //
            this.panelTrad.Controls.Add(this.lblTituloTrad);
            this.panelTrad.Controls.Add(this.dgvTraducciones);
            this.panelTrad.Controls.Add(this.lblTituloControles);
            this.panelTrad.Controls.Add(this.dgvControles);
            this.panelTrad.Controls.Add(this.btnGuardar);
            this.panelTrad.Dock     = System.Windows.Forms.DockStyle.Fill;
            this.panelTrad.Name     = "panelTrad";
            this.panelTrad.Padding  = new System.Windows.Forms.Padding(8);
            this.panelTrad.TabIndex = 1;
            //
            // lblMensaje
            //
            this.lblMensaje.AutoSize  = false;
            this.lblMensaje.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.lblMensaje.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lblMensaje.Location  = new System.Drawing.Point(8, 4);
            this.lblMensaje.Name      = "lblMensaje";
            this.lblMensaje.Size      = new System.Drawing.Size(860, 22);
            this.lblMensaje.TabIndex  = 5;
            //
            // panelBottom
            //
            this.panelBottom.Controls.Add(this.lblMensaje);
            this.panelBottom.Dock     = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Name     = "panelBottom";
            this.panelBottom.Padding  = new System.Windows.Forms.Padding(8, 4, 8, 4);
            this.panelBottom.Size     = new System.Drawing.Size(880, 30);
            this.panelBottom.TabIndex = 2;
            //
            // FormIdiomas
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor           = System.Drawing.Color.White;
            this.ClientSize          = new System.Drawing.Size(880, 620);
            this.Controls.Add(this.panelTrad);
            this.Controls.Add(this.panelBottom);
            this.Controls.Add(this.panelIdiomas);
            this.Name = "FormIdiomas";
            this.Tag  = "mnu.idiomas";
            this.Text = "Gestión de Idiomas";
            ((System.ComponentModel.ISupportInitialize)(this.dgvIdiomas)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTraducciones)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvControles)).EndInit();
            this.panelIdiomas.ResumeLayout(false);
            this.panelIdiomas.PerformLayout();
            this.panelTrad.ResumeLayout(false);
            this.panelTrad.PerformLayout();
            this.panelBotonesIdioma.ResumeLayout(false);
            this.panelBottom.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label              lblTituloIdiomas;
        private System.Windows.Forms.DataGridView       dgvIdiomas;
        private System.Windows.Forms.Button             btnActivar;
        private System.Windows.Forms.Button             btnDesactivar;
        private System.Windows.Forms.Label              lblTituloTrad;
        private System.Windows.Forms.DataGridView       dgvTraducciones;
        private System.Windows.Forms.Label              lblTituloControles;
        private System.Windows.Forms.DataGridView       dgvControles;
        private System.Windows.Forms.Button             btnGuardar;
        private System.Windows.Forms.Label              lblMensaje;
        private System.Windows.Forms.Panel              panelIdiomas;
        private System.Windows.Forms.Panel              panelTrad;
        private System.Windows.Forms.Panel              panelBotonesIdioma;
        private System.Windows.Forms.Panel              panelBottom;
    }
}
