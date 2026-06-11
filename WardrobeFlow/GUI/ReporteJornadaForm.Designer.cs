namespace GUI
{
    partial class ReporteJornadaForm
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
            this.panelTop       = new System.Windows.Forms.Panel();
            this.lblTitulo      = new System.Windows.Forms.Label();
            this.lblSubtitulo   = new System.Windows.Forms.Label();
            this.panelControles = new System.Windows.Forms.Panel();
            this.lblJornada     = new System.Windows.Forms.Label();
            this.dtpJornada     = new System.Windows.Forms.DateTimePicker();
            this.btnGenerar     = new System.Windows.Forms.Button();
            this.btnExportar    = new System.Windows.Forms.Button();
            this.lblComparar    = new System.Windows.Forms.Label();
            this.dtpJornada2    = new System.Windows.Forms.DateTimePicker();
            this.btnComparar    = new System.Windows.Forms.Button();
            this.btnExportarComp = new System.Windows.Forms.Button();
            this.btnLimpiar     = new System.Windows.Forms.Button();
            this.rtbReporte     = new System.Windows.Forms.RichTextBox();
            this.panelStatus    = new System.Windows.Forms.Panel();
            this.lblStatus      = new System.Windows.Forms.Label();
            this.panelTop.SuspendLayout();
            this.panelControles.SuspendLayout();
            this.panelStatus.SuspendLayout();
            this.SuspendLayout();
            //
            // panelTop
            //
            this.panelTop.BackColor = System.Drawing.Color.FromArgb(176, 62, 96);
            this.panelTop.Controls.Add(this.lblTitulo);
            this.panelTop.Controls.Add(this.lblSubtitulo);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(960, 56);
            this.panelTop.TabIndex = 0;
            //
            // lblTitulo
            //
            this.lblTitulo.BackColor = System.Drawing.Color.Transparent;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.White;
            this.lblTitulo.Location = new System.Drawing.Point(12, 6);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(700, 26);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "Reporte de Jornada";
            //
            // lblSubtitulo
            //
            this.lblSubtitulo.BackColor = System.Drawing.Color.Transparent;
            this.lblSubtitulo.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Italic);
            this.lblSubtitulo.ForeColor = System.Drawing.Color.FromArgb(255, 200, 218);
            this.lblSubtitulo.Location = new System.Drawing.Point(12, 34);
            this.lblSubtitulo.Name = "lblSubtitulo";
            this.lblSubtitulo.Size = new System.Drawing.Size(900, 16);
            this.lblSubtitulo.TabIndex = 1;
            this.lblSubtitulo.Text = "Eventos de negocio por jornada con exportación a TXT";
            //
            // panelControles
            //
            this.panelControles.BackColor = System.Drawing.Color.FromArgb(240, 220, 230);
            this.panelControles.Controls.Add(this.lblJornada);
            this.panelControles.Controls.Add(this.dtpJornada);
            this.panelControles.Controls.Add(this.btnGenerar);
            this.panelControles.Controls.Add(this.btnExportar);
            this.panelControles.Controls.Add(this.lblComparar);
            this.panelControles.Controls.Add(this.dtpJornada2);
            this.panelControles.Controls.Add(this.btnComparar);
            this.panelControles.Controls.Add(this.btnExportarComp);
            this.panelControles.Controls.Add(this.btnLimpiar);
            this.panelControles.Location = new System.Drawing.Point(10, 66);
            this.panelControles.Name = "panelControles";
            this.panelControles.Size = new System.Drawing.Size(940, 90);
            this.panelControles.TabIndex = 1;
            //
            // lblJornada
            //
            this.lblJornada.AutoSize = true;
            this.lblJornada.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblJornada.ForeColor = System.Drawing.Color.FromArgb(64, 20, 42);
            this.lblJornada.Location = new System.Drawing.Point(10, 16);
            this.lblJornada.Name = "lblJornada";
            this.lblJornada.TabIndex = 0;
            this.lblJornada.Text = "Jornada:";
            //
            // dtpJornada
            //
            this.dtpJornada.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.dtpJornada.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpJornada.CustomFormat = "dd'/'MM'/'yyyy";
            this.dtpJornada.Location = new System.Drawing.Point(78, 13);
            this.dtpJornada.Name = "dtpJornada";
            this.dtpJornada.Size = new System.Drawing.Size(120, 23);
            this.dtpJornada.TabIndex = 1;
            this.dtpJornada.ValueChanged += new System.EventHandler(this.dtpJornada_ValueChanged);
            //
            // btnGenerar
            //
            this.btnGenerar.BackColor = System.Drawing.Color.FromArgb(176, 62, 96);
            this.btnGenerar.FlatAppearance.BorderSize = 0;
            this.btnGenerar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGenerar.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnGenerar.ForeColor = System.Drawing.Color.White;
            this.btnGenerar.Location = new System.Drawing.Point(212, 12);
            this.btnGenerar.Name = "btnGenerar";
            this.btnGenerar.Size = new System.Drawing.Size(145, 28);
            this.btnGenerar.TabIndex = 2;
            this.btnGenerar.Text = "↻  Generar";
            this.btnGenerar.UseVisualStyleBackColor = false;
            this.btnGenerar.Click += new System.EventHandler(this.btnGenerar_Click);
            //
            // btnExportar
            //
            this.btnExportar.BackColor = System.Drawing.Color.FromArgb(110, 40, 70);
            this.btnExportar.FlatAppearance.BorderSize = 0;
            this.btnExportar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExportar.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnExportar.ForeColor = System.Drawing.Color.White;
            this.btnExportar.Location = new System.Drawing.Point(368, 12);
            this.btnExportar.Name = "btnExportar";
            this.btnExportar.Size = new System.Drawing.Size(165, 28);
            this.btnExportar.TabIndex = 3;
            this.btnExportar.Text = "⬇  Exportar reporte...";
            this.btnExportar.UseVisualStyleBackColor = false;
            this.btnExportar.Click += new System.EventHandler(this.btnExportar_Click);
            //
            // lblComparar
            //
            this.lblComparar.AutoSize = true;
            this.lblComparar.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblComparar.ForeColor = System.Drawing.Color.FromArgb(64, 20, 42);
            this.lblComparar.Location = new System.Drawing.Point(10, 57);
            this.lblComparar.Name = "lblComparar";
            this.lblComparar.TabIndex = 4;
            this.lblComparar.Text = "Comparar con:";
            //
            // dtpJornada2
            //
            this.dtpJornada2.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.dtpJornada2.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpJornada2.CustomFormat = "dd'/'MM'/'yyyy";
            this.dtpJornada2.Location = new System.Drawing.Point(112, 53);
            this.dtpJornada2.Name = "dtpJornada2";
            this.dtpJornada2.Size = new System.Drawing.Size(120, 23);
            this.dtpJornada2.TabIndex = 5;
            //
            // btnComparar
            //
            this.btnComparar.BackColor = System.Drawing.Color.FromArgb(170, 85, 120);
            this.btnComparar.FlatAppearance.BorderSize = 0;
            this.btnComparar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnComparar.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnComparar.ForeColor = System.Drawing.Color.White;
            this.btnComparar.Location = new System.Drawing.Point(246, 51);
            this.btnComparar.Name = "btnComparar";
            this.btnComparar.Size = new System.Drawing.Size(185, 28);
            this.btnComparar.TabIndex = 6;
            this.btnComparar.Text = "⚖  Comparar Jornadas";
            this.btnComparar.UseVisualStyleBackColor = false;
            this.btnComparar.Click += new System.EventHandler(this.btnComparar_Click);
            //
            // btnExportarComp
            //
            this.btnExportarComp.BackColor = System.Drawing.Color.FromArgb(110, 40, 70);
            this.btnExportarComp.FlatAppearance.BorderSize = 0;
            this.btnExportarComp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExportarComp.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnExportarComp.ForeColor = System.Drawing.Color.White;
            this.btnExportarComp.Location = new System.Drawing.Point(442, 51);
            this.btnExportarComp.Name = "btnExportarComp";
            this.btnExportarComp.Size = new System.Drawing.Size(185, 28);
            this.btnExportarComp.TabIndex = 7;
            this.btnExportarComp.Text = "⬇  Exportar comparación...";
            this.btnExportarComp.UseVisualStyleBackColor = false;
            this.btnExportarComp.Click += new System.EventHandler(this.btnExportarComp_Click);
            //
            // btnLimpiar
            //
            this.btnLimpiar.BackColor = System.Drawing.Color.FromArgb(215, 185, 200);
            this.btnLimpiar.FlatAppearance.BorderSize = 0;
            this.btnLimpiar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLimpiar.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnLimpiar.ForeColor = System.Drawing.Color.FromArgb(72, 28, 50);
            this.btnLimpiar.Location = new System.Drawing.Point(640, 51);
            this.btnLimpiar.Name = "btnLimpiar";
            this.btnLimpiar.Size = new System.Drawing.Size(175, 28);
            this.btnLimpiar.TabIndex = 8;
            this.btnLimpiar.Text = "↩  Volver al reporte";
            this.btnLimpiar.UseVisualStyleBackColor = false;
            this.btnLimpiar.Click += new System.EventHandler(this.btnLimpiar_Click);
            //
            // rtbReporte
            //
            this.rtbReporte.BackColor = System.Drawing.Color.FromArgb(255, 250, 253);
            this.rtbReporte.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbReporte.Font = new System.Drawing.Font("Consolas", 9.5F);
            this.rtbReporte.ForeColor = System.Drawing.Color.FromArgb(40, 15, 28);
            this.rtbReporte.Anchor = ((System.Windows.Forms.AnchorStyles)(
                System.Windows.Forms.AnchorStyles.Top    |
                System.Windows.Forms.AnchorStyles.Bottom |
                System.Windows.Forms.AnchorStyles.Left   |
                System.Windows.Forms.AnchorStyles.Right));
            this.rtbReporte.Location = new System.Drawing.Point(10, 168);
            this.rtbReporte.Name = "rtbReporte";
            this.rtbReporte.ReadOnly = true;
            this.rtbReporte.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.rtbReporte.Size = new System.Drawing.Size(940, 390);
            this.rtbReporte.TabIndex = 0;
            this.rtbReporte.Text = "";
            //
            // panelStatus
            //
            this.panelStatus.BackColor = System.Drawing.Color.FromArgb(235, 220, 228);
            this.panelStatus.Controls.Add(this.lblStatus);
            this.panelStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelStatus.Location = new System.Drawing.Point(0, 564);
            this.panelStatus.Name = "panelStatus";
            this.panelStatus.Size = new System.Drawing.Size(960, 28);
            this.panelStatus.TabIndex = 2;
            //
            // lblStatus
            //
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(100, 45, 68);
            this.lblStatus.Location = new System.Drawing.Point(8, 6);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.TabIndex = 0;
            this.lblStatus.Text = "Listo.";
            //
            // ReporteJornadaForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(250, 240, 246);
            this.ClientSize = new System.Drawing.Size(960, 592);
            this.Controls.Add(this.rtbReporte);
            this.Controls.Add(this.panelControles);
            this.Controls.Add(this.panelStatus);
            this.Controls.Add(this.panelTop);
            this.Name = "ReporteJornadaForm";
            this.Text = "Reporte de Jornada";
            this.Load += new System.EventHandler(this.ReporteJornadaForm_Load);
            this.panelTop.ResumeLayout(false);
            this.panelControles.ResumeLayout(false);
            this.panelControles.PerformLayout();
            this.panelStatus.ResumeLayout(false);
            this.panelStatus.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel          panelTop;
        private System.Windows.Forms.Label          lblTitulo;
        private System.Windows.Forms.Label          lblSubtitulo;
        private System.Windows.Forms.Panel          panelControles;
        private System.Windows.Forms.Label          lblJornada;
        private System.Windows.Forms.DateTimePicker dtpJornada;
        private System.Windows.Forms.Button         btnGenerar;
        private System.Windows.Forms.Button         btnExportar;
        private System.Windows.Forms.Label          lblComparar;
        private System.Windows.Forms.DateTimePicker dtpJornada2;
        private System.Windows.Forms.Button         btnComparar;
        private System.Windows.Forms.Button         btnExportarComp;
        private System.Windows.Forms.Button         btnLimpiar;
        private System.Windows.Forms.RichTextBox    rtbReporte;
        private System.Windows.Forms.Panel          panelStatus;
        private System.Windows.Forms.Label          lblStatus;
    }
}
