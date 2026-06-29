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
            this.lblTituloIdiomas = new System.Windows.Forms.Label();
            this.dgvIdiomas = new System.Windows.Forms.DataGridView();
            this.btnActivar = new System.Windows.Forms.Button();
            this.btnDesactivar = new System.Windows.Forms.Button();
            this.lblTituloTrad = new System.Windows.Forms.Label();
            this.dgvTraducciones = new System.Windows.Forms.DataGridView();
            this.lblTituloControles = new System.Windows.Forms.Label();
            this.dgvControles = new System.Windows.Forms.DataGridView();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.lblMensaje = new System.Windows.Forms.Label();
            this.panelIdiomas = new System.Windows.Forms.Panel();
            this.panelBotonesIdioma = new System.Windows.Forms.Panel();
            this.panelTrad = new System.Windows.Forms.Panel();
            this.panelBottom = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.dgvIdiomas)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTraducciones)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvControles)).BeginInit();
            this.panelIdiomas.SuspendLayout();
            this.panelBotonesIdioma.SuspendLayout();
            this.panelTrad.SuspendLayout();
            this.panelBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblTituloIdiomas
            // 
            this.lblTituloIdiomas.AutoSize = true;
            this.lblTituloIdiomas.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblTituloIdiomas.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(60)))), ((int)(((byte)(100)))));
            this.lblTituloIdiomas.Location = new System.Drawing.Point(8, 8);
            this.lblTituloIdiomas.Name = "lblTituloIdiomas";
            this.lblTituloIdiomas.Size = new System.Drawing.Size(141, 19);
            this.lblTituloIdiomas.TabIndex = 0;
            this.lblTituloIdiomas.Text = "Idiomas del sistema";
            // 
            // dgvIdiomas
            // 
            this.dgvIdiomas.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvIdiomas.BackgroundColor = System.Drawing.Color.White;
            this.dgvIdiomas.Location = new System.Drawing.Point(8, 35);
            this.dgvIdiomas.Name = "dgvIdiomas";
            this.dgvIdiomas.Size = new System.Drawing.Size(860, 120);
            this.dgvIdiomas.TabIndex = 0;
            this.dgvIdiomas.SelectionChanged += new System.EventHandler(this.DgvIdiomas_SelectionChanged);
            // 
            // btnActivar
            // 
            this.btnActivar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(80)))));
            this.btnActivar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnActivar.ForeColor = System.Drawing.Color.White;
            this.btnActivar.Location = new System.Drawing.Point(0, 4);
            this.btnActivar.Name = "btnActivar";
            this.btnActivar.Size = new System.Drawing.Size(130, 28);
            this.btnActivar.TabIndex = 1;
            this.btnActivar.Text = "✔ Activar";
            this.btnActivar.UseVisualStyleBackColor = false;
            this.btnActivar.Click += new System.EventHandler(this.BtnActivar_Click);
            // 
            // btnDesactivar
            // 
            this.btnDesactivar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.btnDesactivar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDesactivar.ForeColor = System.Drawing.Color.White;
            this.btnDesactivar.Location = new System.Drawing.Point(140, 4);
            this.btnDesactivar.Name = "btnDesactivar";
            this.btnDesactivar.Size = new System.Drawing.Size(130, 28);
            this.btnDesactivar.TabIndex = 2;
            this.btnDesactivar.Text = "✕ Desactivar";
            this.btnDesactivar.UseVisualStyleBackColor = false;
            this.btnDesactivar.Click += new System.EventHandler(this.BtnDesactivar_Click);
            // 
            // lblTituloTrad
            // 
            this.lblTituloTrad.AutoSize = true;
            this.lblTituloTrad.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblTituloTrad.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(60)))), ((int)(((byte)(100)))));
            this.lblTituloTrad.Location = new System.Drawing.Point(8, 8);
            this.lblTituloTrad.Name = "lblTituloTrad";
            this.lblTituloTrad.Size = new System.Drawing.Size(263, 19);
            this.lblTituloTrad.TabIndex = 0;
            this.lblTituloTrad.Text = "Traducciones del idioma seleccionado";
            // 
            // dgvTraducciones
            // 
            this.dgvTraducciones.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.dgvTraducciones.BackgroundColor = System.Drawing.Color.White;
            this.dgvTraducciones.Location = new System.Drawing.Point(8, 35);
            this.dgvTraducciones.Name = "dgvTraducciones";
            this.dgvTraducciones.Size = new System.Drawing.Size(860, 585);
            this.dgvTraducciones.TabIndex = 3;
            // 
            // lblTituloControles
            // 
            this.lblTituloControles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTituloControles.AutoSize = true;
            this.lblTituloControles.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblTituloControles.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(60)))), ((int)(((byte)(100)))));
            this.lblTituloControles.Location = new System.Drawing.Point(1256, 8);
            this.lblTituloControles.Name = "lblTituloControles";
            this.lblTituloControles.Size = new System.Drawing.Size(151, 19);
            this.lblTituloControles.TabIndex = 4;
            this.lblTituloControles.Text = "Controles traducibles";
            // 
            // dgvControles
            // 
            this.dgvControles.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvControles.BackgroundColor = System.Drawing.Color.White;
            this.dgvControles.Location = new System.Drawing.Point(1256, 35);
            this.dgvControles.Name = "dgvControles";
            this.dgvControles.Size = new System.Drawing.Size(292, 585);
            this.dgvControles.TabIndex = 6;
            // 
            // btnGuardar
            // 
            this.btnGuardar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnGuardar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(100)))), ((int)(((byte)(135)))));
            this.btnGuardar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGuardar.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnGuardar.ForeColor = System.Drawing.Color.White;
            this.btnGuardar.Location = new System.Drawing.Point(8, 627);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(138, 30);
            this.btnGuardar.TabIndex = 4;
            this.btnGuardar.Text = "💾 Guardar cambios";
            this.btnGuardar.UseVisualStyleBackColor = false;
            this.btnGuardar.Click += new System.EventHandler(this.BtnGuardar_Click);
            // 
            // lblMensaje
            // 
            this.lblMensaje.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMensaje.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblMensaje.Location = new System.Drawing.Point(8, 4);
            this.lblMensaje.Name = "lblMensaje";
            this.lblMensaje.Size = new System.Drawing.Size(864, 22);
            this.lblMensaje.TabIndex = 5;
            // 
            // panelIdiomas
            // 
            this.panelIdiomas.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(228)))), ((int)(((byte)(235)))));
            this.panelIdiomas.Controls.Add(this.lblTituloIdiomas);
            this.panelIdiomas.Controls.Add(this.dgvIdiomas);
            this.panelIdiomas.Controls.Add(this.panelBotonesIdioma);
            this.panelIdiomas.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelIdiomas.Location = new System.Drawing.Point(0, 0);
            this.panelIdiomas.Name = "panelIdiomas";
            this.panelIdiomas.Padding = new System.Windows.Forms.Padding(8);
            this.panelIdiomas.Size = new System.Drawing.Size(880, 210);
            this.panelIdiomas.TabIndex = 0;
            // 
            // panelBotonesIdioma
            // 
            this.panelBotonesIdioma.Controls.Add(this.btnActivar);
            this.panelBotonesIdioma.Controls.Add(this.btnDesactivar);
            this.panelBotonesIdioma.Location = new System.Drawing.Point(8, 163);
            this.panelBotonesIdioma.Name = "panelBotonesIdioma";
            this.panelBotonesIdioma.Size = new System.Drawing.Size(560, 36);
            this.panelBotonesIdioma.TabIndex = 1;
            // 
            // panelTrad
            // 
            this.panelTrad.Controls.Add(this.lblTituloTrad);
            this.panelTrad.Controls.Add(this.dgvTraducciones);
            this.panelTrad.Controls.Add(this.lblTituloControles);
            this.panelTrad.Controls.Add(this.dgvControles);
            this.panelTrad.Controls.Add(this.btnGuardar);
            this.panelTrad.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelTrad.Location = new System.Drawing.Point(0, 210);
            this.panelTrad.Name = "panelTrad";
            this.panelTrad.Padding = new System.Windows.Forms.Padding(8);
            this.panelTrad.Size = new System.Drawing.Size(880, 380);
            this.panelTrad.TabIndex = 1;
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.lblMensaje);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 590);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);
            this.panelBottom.Size = new System.Drawing.Size(880, 30);
            this.panelBottom.TabIndex = 2;
            // 
            // FormIdiomas
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(880, 620);
            this.Controls.Add(this.panelTrad);
            this.Controls.Add(this.panelBottom);
            this.Controls.Add(this.panelIdiomas);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.Name = "FormIdiomas";
            this.Tag = "mnu.idiomas";
            this.Text = "Gestión de Idiomas";
            ((System.ComponentModel.ISupportInitialize)(this.dgvIdiomas)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTraducciones)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvControles)).EndInit();
            this.panelIdiomas.ResumeLayout(false);
            this.panelIdiomas.PerformLayout();
            this.panelBotonesIdioma.ResumeLayout(false);
            this.panelTrad.ResumeLayout(false);
            this.panelTrad.PerformLayout();
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
