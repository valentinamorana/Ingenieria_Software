namespace GUI
{
    partial class VersionHistorialForm
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
            this.pnlTop      = new System.Windows.Forms.Panel();
            this.lblTitulo   = new System.Windows.Forms.Label();
            this.lblUsuario  = new System.Windows.Forms.Label();
            this.cboUsuario  = new System.Windows.Forms.ComboBox();
            this.btnCargar   = new System.Windows.Forms.Button();
            this.dgv         = new System.Windows.Forms.DataGridView();
            this.colId       = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRegistro = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colFecha    = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colActor    = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDetalle  = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnRestaurar= new System.Windows.Forms.Button();
            this.pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).BeginInit();
            this.SuspendLayout();

            // ── pnlTop ───────────────────────────────────────────────────────
            this.pnlTop.BackColor = System.Drawing.Color.FromArgb(252, 250, 252);
            this.pnlTop.Controls.Add(this.lblTitulo);
            this.pnlTop.Controls.Add(this.lblUsuario);
            this.pnlTop.Controls.Add(this.cboUsuario);
            this.pnlTop.Controls.Add(this.btnCargar);
            this.pnlTop.Dock     = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Height   = 80;
            this.pnlTop.Name     = "pnlTop";
            this.pnlTop.Padding  = new System.Windows.Forms.Padding(12, 10, 12, 8);
            this.pnlTop.TabIndex = 0;

            // ── lblTitulo ────────────────────────────────────────────────────
            this.lblTitulo.Font      = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.FromArgb(176, 62, 96);
            this.lblTitulo.Location  = new System.Drawing.Point(12, 10);
            this.lblTitulo.Name      = "lblTitulo";
            this.lblTitulo.Size      = new System.Drawing.Size(760, 26);
            this.lblTitulo.TabIndex  = 0;
            this.lblTitulo.Text      = "Historial de Cambios de Usuarios";

            // ── lblUsuario ───────────────────────────────────────────────────
            this.lblUsuario.Font     = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblUsuario.Location = new System.Drawing.Point(12, 46);
            this.lblUsuario.Name     = "lblUsuario";
            this.lblUsuario.Size     = new System.Drawing.Size(70, 22);
            this.lblUsuario.TabIndex = 1;
            this.lblUsuario.Text     = "Usuario:";
            this.lblUsuario.TextAlign= System.Drawing.ContentAlignment.MiddleLeft;

            // ── cboUsuario ───────────────────────────────────────────────────
            this.cboUsuario.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboUsuario.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.cboUsuario.Location  = new System.Drawing.Point(88, 46);
            this.cboUsuario.Name      = "cboUsuario";
            this.cboUsuario.Size      = new System.Drawing.Size(220, 23);
            this.cboUsuario.TabIndex  = 2;

            // ── btnCargar ────────────────────────────────────────────────────
            this.btnCargar.BackColor                  = System.Drawing.Color.FromArgb(176, 62, 96);
            this.btnCargar.FlatStyle                  = System.Windows.Forms.FlatStyle.Flat;
            this.btnCargar.FlatAppearance.BorderSize  = 0;
            this.btnCargar.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnCargar.ForeColor = System.Drawing.Color.White;
            this.btnCargar.Location  = new System.Drawing.Point(316, 44);
            this.btnCargar.Name      = "btnCargar";
            this.btnCargar.Size      = new System.Drawing.Size(80, 26);
            this.btnCargar.TabIndex  = 3;
            this.btnCargar.Text      = "Cargar";
            this.btnCargar.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnCargar.Click    += new System.EventHandler(this.btnCargar_Click);

            // ── dgv ──────────────────────────────────────────────────────────
            this.dgv.AllowUserToAddRows    = false;
            this.dgv.AllowUserToDeleteRows = false;
            this.dgv.ReadOnly              = true;
            this.dgv.SelectionMode         = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgv.MultiSelect           = false;
            this.dgv.AutoSizeColumnsMode   = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgv.BackgroundColor       = System.Drawing.Color.FromArgb(252, 250, 252);
            this.dgv.BorderStyle           = System.Windows.Forms.BorderStyle.None;
            this.dgv.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(176, 62, 96);
            this.dgv.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            this.dgv.ColumnHeadersDefaultCellStyle.Font      = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.dgv.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dgv.EnableHeadersVisualStyles= false;
            this.dgv.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.dgv.Name      = "dgv";
            this.dgv.TabIndex  = 4;
            this.dgv.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[]
            {
                this.colId, this.colRegistro, this.colFecha, this.colActor, this.colDetalle
            });

            // ── colId (oculta: referencia a la versión para restaurar) ────────
            this.colId.Name        = "colId";
            this.colId.HeaderText  = "ID";
            this.colId.Visible     = false;

            // ── colRegistro (identificador del registro afectado: usuario + ID) ──
            this.colRegistro.Name        = "colRegistro";
            this.colRegistro.HeaderText  = "Usuario (ID)";
            this.colRegistro.FillWeight  = 18;
            this.colRegistro.MinimumWidth= 110;

            // ── colFecha ─────────────────────────────────────────────────────
            this.colFecha.Name        = "colFecha";
            this.colFecha.HeaderText  = "Fecha";
            this.colFecha.FillWeight  = 20;
            this.colFecha.MinimumWidth= 130;

            // ── colActor ─────────────────────────────────────────────────────
            this.colActor.Name        = "colActor";
            this.colActor.HeaderText  = "Modificado por";
            this.colActor.FillWeight  = 16;
            this.colActor.MinimumWidth= 100;

            // ── colDetalle (cambios realizados en ese guardado: "campo: 'a' → 'b'; ...") ──
            this.colDetalle.Name        = "colDetalle";
            this.colDetalle.HeaderText  = "Cambios realizados";
            this.colDetalle.FillWeight  = 46;
            this.colDetalle.MinimumWidth= 220;

            // ── btnRestaurar ─────────────────────────────────────────────────
            this.btnRestaurar.BackColor                  = System.Drawing.Color.FromArgb(210, 100, 135);
            this.btnRestaurar.FlatStyle                  = System.Windows.Forms.FlatStyle.Flat;
            this.btnRestaurar.FlatAppearance.BorderSize  = 0;
            this.btnRestaurar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(190, 80, 115);
            this.btnRestaurar.Dock      = System.Windows.Forms.DockStyle.Bottom;
            this.btnRestaurar.Font      = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnRestaurar.ForeColor = System.Drawing.Color.White;
            this.btnRestaurar.Height    = 36;
            this.btnRestaurar.Name      = "btnRestaurar";
            this.btnRestaurar.TabIndex  = 5;
            this.btnRestaurar.Text      = "Restaurar Versión Seleccionada";
            this.btnRestaurar.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnRestaurar.Click    += new System.EventHandler(this.btnRestaurar_Click);

            // ── VersionHistorialForm ─────────────────────────────────────────
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor           = System.Drawing.Color.FromArgb(252, 250, 252);
            this.ClientSize          = new System.Drawing.Size(800, 500);
            this.Controls.Add(this.dgv);
            this.Controls.Add(this.btnRestaurar);
            this.Controls.Add(this.pnlTop);
            this.Name          = "VersionHistorialForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text          = "Historial de Cambios de Usuarios";
            this.pnlTop.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel                    pnlTop;
        private System.Windows.Forms.Label                    lblTitulo;
        private System.Windows.Forms.Label                    lblUsuario;
        private System.Windows.Forms.ComboBox                 cboUsuario;
        private System.Windows.Forms.Button                   btnCargar;
        private System.Windows.Forms.DataGridView             dgv;
        private System.Windows.Forms.DataGridViewTextBoxColumn colId;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRegistro;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFecha;
        private System.Windows.Forms.DataGridViewTextBoxColumn colActor;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDetalle;
        private System.Windows.Forms.Button                   btnRestaurar;
    }
}
