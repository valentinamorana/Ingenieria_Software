namespace GUI
{
    partial class BackupForm
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
            this.pnlMain      = new System.Windows.Forms.Panel();
            this.lblTitulo    = new System.Windows.Forms.Label();
            this.lblRutaLabel = new System.Windows.Forms.Label();
            this.lblRuta      = new System.Windows.Forms.Label();
            this.lstBackups   = new System.Windows.Forms.ListView();
            this.colArchivo   = new System.Windows.Forms.ColumnHeader();
            this.colFecha     = new System.Windows.Forms.ColumnHeader();
            this.colAutor     = new System.Windows.Forms.ColumnHeader();
            this.colTamanio   = new System.Windows.Forms.ColumnHeader();
            this.lblConteo    = new System.Windows.Forms.Label();
            this.btnCrear     = new System.Windows.Forms.Button();
            this.btnRestaurar = new System.Windows.Forms.Button();
            this.btnEliminar  = new System.Windows.Forms.Button();
            this.btnExterno   = new System.Windows.Forms.Button();
            this.lblInfo      = new System.Windows.Forms.Label();
            this.pnlMain.SuspendLayout();
            this.SuspendLayout();

            // ── pnlMain ──────────────────────────────────────────────────────
            this.pnlMain.BackColor = System.Drawing.Color.FromArgb(252, 250, 252);
            this.pnlMain.Controls.Add(this.lblTitulo);
            this.pnlMain.Controls.Add(this.lblRutaLabel);
            this.pnlMain.Controls.Add(this.lblRuta);
            this.pnlMain.Controls.Add(this.lstBackups);
            this.pnlMain.Controls.Add(this.lblConteo);
            this.pnlMain.Controls.Add(this.btnCrear);
            this.pnlMain.Controls.Add(this.btnRestaurar);
            this.pnlMain.Controls.Add(this.btnEliminar);
            this.pnlMain.Controls.Add(this.btnExterno);
            this.pnlMain.Controls.Add(this.lblInfo);
            this.pnlMain.Dock     = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Name     = "pnlMain";
            this.pnlMain.TabIndex = 0;

            // ── lblTitulo ────────────────────────────────────────────────────
            this.lblTitulo.Font      = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.FromArgb(176, 62, 96);
            this.lblTitulo.Location  = new System.Drawing.Point(20, 16);
            this.lblTitulo.Name      = "lblTitulo";
            this.lblTitulo.Size      = new System.Drawing.Size(460, 28);
            this.lblTitulo.TabIndex  = 0;
            this.lblTitulo.Text      = "Backup y Restauración";

            // ── lblRutaLabel ─────────────────────────────────────────────────
            this.lblRutaLabel.Font      = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblRutaLabel.ForeColor = System.Drawing.Color.FromArgb(100, 80, 100);
            this.lblRutaLabel.Location  = new System.Drawing.Point(20, 50);
            this.lblRutaLabel.Name      = "lblRutaLabel";
            this.lblRutaLabel.Size      = new System.Drawing.Size(130, 16);
            this.lblRutaLabel.TabIndex  = 1;
            this.lblRutaLabel.Text      = "Ubicación de copias:";

            // ── lblRuta ──────────────────────────────────────────────────────
            this.lblRuta.AutoEllipsis = true;
            this.lblRuta.Font         = new System.Drawing.Font("Consolas", 7.5F);
            this.lblRuta.ForeColor    = System.Drawing.Color.FromArgb(120, 100, 120);
            this.lblRuta.Location     = new System.Drawing.Point(20, 68);
            this.lblRuta.Name         = "lblRuta";
            this.lblRuta.Size         = new System.Drawing.Size(460, 16);
            this.lblRuta.TabIndex     = 2;
            this.lblRuta.Text         = "...";

            // ── lstBackups ───────────────────────────────────────────────────
            this.lstBackups.Columns.AddRange(new System.Windows.Forms.ColumnHeader[]
                { this.colArchivo, this.colFecha, this.colAutor, this.colTamanio });
            this.lstBackups.FullRowSelect = true;
            this.lstBackups.GridLines     = true;
            this.lstBackups.HideSelection = false;
            this.lstBackups.Location      = new System.Drawing.Point(20, 92);
            this.lstBackups.MultiSelect   = false;
            this.lstBackups.Name          = "lstBackups";
            this.lstBackups.Size          = new System.Drawing.Size(460, 200);
            this.lstBackups.TabIndex      = 3;
            this.lstBackups.View          = System.Windows.Forms.View.Details;
            this.lstBackups.Font          = new System.Drawing.Font("Segoe UI", 9F);
            this.lstBackups.SelectedIndexChanged += new System.EventHandler(this.lstBackups_SelectedIndexChanged);

            // ── colArchivo ───────────────────────────────────────────────────
            this.colArchivo.Text  = "Archivo";
            this.colArchivo.Width = 170;

            // ── colFecha ─────────────────────────────────────────────────────
            this.colFecha.Text  = "Fecha";
            this.colFecha.Width = 108;

            // ── colAutor ─────────────────────────────────────────────────────
            this.colAutor.Text  = "Autor";
            this.colAutor.Width = 96;

            // ── colTamanio ───────────────────────────────────────────────────
            this.colTamanio.Text      = "Tamaño";
            this.colTamanio.Width     = 82;
            this.colTamanio.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;

            // ── lblConteo ────────────────────────────────────────────────────
            this.lblConteo.Font      = new System.Drawing.Font("Segoe UI", 8F);
            this.lblConteo.ForeColor = System.Drawing.Color.FromArgb(120, 100, 120);
            this.lblConteo.Location  = new System.Drawing.Point(20, 298);
            this.lblConteo.Name      = "lblConteo";
            this.lblConteo.Size      = new System.Drawing.Size(460, 16);
            this.lblConteo.TabIndex  = 4;
            this.lblConteo.Text      = "";

            // ── btnCrear ─────────────────────────────────────────────────────
            this.btnCrear.BackColor                         = System.Drawing.Color.FromArgb(176, 62, 96);
            this.btnCrear.FlatStyle                         = System.Windows.Forms.FlatStyle.Flat;
            this.btnCrear.FlatAppearance.BorderSize         = 0;
            this.btnCrear.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(120, 50, 78);
            this.btnCrear.Font      = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnCrear.ForeColor = System.Drawing.Color.White;
            this.btnCrear.Location  = new System.Drawing.Point(20, 322);
            this.btnCrear.Name      = "btnCrear";
            this.btnCrear.Size      = new System.Drawing.Size(460, 38);
            this.btnCrear.TabIndex  = 5;
            this.btnCrear.Text      = "Generar Copia de Seguridad";
            this.btnCrear.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnCrear.Click    += new System.EventHandler(this.btnCrear_Click);

            // ── btnRestaurar ─────────────────────────────────────────────────
            this.btnRestaurar.BackColor                         = System.Drawing.Color.FromArgb(252, 228, 235);
            this.btnRestaurar.Enabled                           = false;
            this.btnRestaurar.FlatStyle                         = System.Windows.Forms.FlatStyle.Flat;
            this.btnRestaurar.FlatAppearance.BorderSize         = 1;
            this.btnRestaurar.FlatAppearance.BorderColor        = System.Drawing.Color.FromArgb(210, 100, 135);
            this.btnRestaurar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(244, 212, 226);
            this.btnRestaurar.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnRestaurar.ForeColor = System.Drawing.Color.FromArgb(100, 40, 80);
            this.btnRestaurar.Location  = new System.Drawing.Point(20, 370);
            this.btnRestaurar.Name      = "btnRestaurar";
            this.btnRestaurar.Size      = new System.Drawing.Size(200, 32);
            this.btnRestaurar.TabIndex  = 6;
            this.btnRestaurar.Text      = "Restaurar seleccionado";
            this.btnRestaurar.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnRestaurar.Click    += new System.EventHandler(this.btnRestaurar_Click);

            // ── btnEliminar ──────────────────────────────────────────────────
            this.btnEliminar.BackColor                         = System.Drawing.Color.FromArgb(250, 235, 235);
            this.btnEliminar.Enabled                           = false;
            this.btnEliminar.FlatStyle                         = System.Windows.Forms.FlatStyle.Flat;
            this.btnEliminar.FlatAppearance.BorderSize         = 1;
            this.btnEliminar.FlatAppearance.BorderColor        = System.Drawing.Color.FromArgb(200, 150, 150);
            this.btnEliminar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(240, 210, 210);
            this.btnEliminar.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.btnEliminar.ForeColor = System.Drawing.Color.FromArgb(160, 40, 40);
            this.btnEliminar.Location  = new System.Drawing.Point(228, 370);
            this.btnEliminar.Name      = "btnEliminar";
            this.btnEliminar.Size      = new System.Drawing.Size(108, 32);
            this.btnEliminar.TabIndex  = 7;
            this.btnEliminar.Text      = "Eliminar";
            this.btnEliminar.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnEliminar.Click    += new System.EventHandler(this.btnEliminar_Click);

            // ── btnExterno ───────────────────────────────────────────────────
            this.btnExterno.BackColor                         = System.Drawing.Color.FromArgb(230, 225, 230);
            this.btnExterno.FlatStyle                         = System.Windows.Forms.FlatStyle.Flat;
            this.btnExterno.FlatAppearance.BorderSize         = 0;
            this.btnExterno.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(210, 205, 215);
            this.btnExterno.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnExterno.ForeColor = System.Drawing.Color.FromArgb(80, 60, 80);
            this.btnExterno.Location  = new System.Drawing.Point(344, 370);
            this.btnExterno.Name      = "btnExterno";
            this.btnExterno.Size      = new System.Drawing.Size(136, 32);
            this.btnExterno.TabIndex  = 8;
            this.btnExterno.Text      = "Desde archivo...";
            this.btnExterno.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnExterno.Click    += new System.EventHandler(this.btnExterno_Click);

            // ── lblInfo ──────────────────────────────────────────────────────
            this.lblInfo.Font      = new System.Drawing.Font("Segoe UI", 8F);
            this.lblInfo.ForeColor = System.Drawing.Color.FromArgb(140, 110, 130);
            this.lblInfo.Location  = new System.Drawing.Point(20, 412);
            this.lblInfo.Name      = "lblInfo";
            this.lblInfo.Size      = new System.Drawing.Size(460, 32);
            this.lblInfo.TabIndex  = 9;
            this.lblInfo.Text      = "Nota: la restauración cierra las conexiones activas y reinicia la aplicación.";

            // ── BackupForm ───────────────────────────────────────────────────
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor           = System.Drawing.Color.FromArgb(252, 250, 252);
            this.ClientSize          = new System.Drawing.Size(500, 458);
            this.Controls.Add(this.pnlMain);
            this.FormBorderStyle     = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox         = false;
            this.MinimizeBox         = false;
            this.Name                = "BackupForm";
            this.StartPosition       = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text                = "Backup y Restauración";
            this.pnlMain.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel        pnlMain;
        private System.Windows.Forms.Label        lblTitulo;
        private System.Windows.Forms.Label        lblRutaLabel;
        private System.Windows.Forms.Label        lblRuta;
        private System.Windows.Forms.ListView     lstBackups;
        private System.Windows.Forms.ColumnHeader colArchivo;
        private System.Windows.Forms.ColumnHeader colFecha;
        private System.Windows.Forms.ColumnHeader colAutor;
        private System.Windows.Forms.ColumnHeader colTamanio;
        private System.Windows.Forms.Label        lblConteo;
        private System.Windows.Forms.Button       btnCrear;
        private System.Windows.Forms.Button       btnRestaurar;
        private System.Windows.Forms.Button       btnEliminar;
        private System.Windows.Forms.Button       btnExterno;
        private System.Windows.Forms.Label        lblInfo;
    }
}
