namespace GUI
{
    partial class Login
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        private void InitializeComponent()
        {
            this.txtUsuario    = new System.Windows.Forms.TextBox();
            this.txtContraseña = new System.Windows.Forms.TextBox();
            this.lblUsuario    = new System.Windows.Forms.Label();
            this.lblContraseña = new System.Windows.Forms.Label();
            this.btnIngresar   = new System.Windows.Forms.Button();
            this.btnSalir      = new System.Windows.Forms.Button();
            this.lblError      = new System.Windows.Forms.Label();
            this.pnlCard       = new System.Windows.Forms.Panel();
            this.lblTitle      = new System.Windows.Forms.Label();
            this.lblSubtitle   = new System.Windows.Forms.Label();
            this.lblAccent     = new System.Windows.Forms.Label();

            this.pnlCard.SuspendLayout();
            this.SuspendLayout();

            // ── Formulario ────────────────────────────────────────────────────
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor           = System.Drawing.Color.FromArgb(18, 18, 30);
            this.ClientSize          = new System.Drawing.Size(460, 500);
            this.FormBorderStyle     = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox         = false;
            this.MinimizeBox         = false;
            this.Name                = "Login";
            this.StartPosition       = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text                = "WardrobeFlow — Acceso";

            // ── Línea dorada decorativa (acento superior) ─────────────────────
            this.lblAccent.BackColor = System.Drawing.Color.FromArgb(196, 160, 100);
            this.lblAccent.Location  = new System.Drawing.Point(0, 0);
            this.lblAccent.Size      = new System.Drawing.Size(460, 5);
            this.lblAccent.Name      = "lblAccent";
            this.lblAccent.TabIndex  = 10;

            // ── Título principal "WardrobeFlow" ───────────────────────────────
            this.lblTitle.AutoSize  = false;
            this.lblTitle.Text      = "WardrobeFlow";
            this.lblTitle.Font      = new System.Drawing.Font("Segoe UI", 26F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblTitle.Location  = new System.Drawing.Point(0, 40);
            this.lblTitle.Size      = new System.Drawing.Size(460, 55);
            this.lblTitle.Name      = "lblTitle";
            this.lblTitle.TabIndex  = 11;

            // ── Subtítulo "Portal de Empleados" ──────────────────────────────
            this.lblSubtitle.AutoSize  = false;
            this.lblSubtitle.Text      = "PORTAL DE EMPLEADOS";
            this.lblSubtitle.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(196, 160, 100);
            this.lblSubtitle.BackColor = System.Drawing.Color.Transparent;
            this.lblSubtitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblSubtitle.Location  = new System.Drawing.Point(0, 94);
            this.lblSubtitle.Size      = new System.Drawing.Size(460, 22);
            this.lblSubtitle.Name      = "lblSubtitle";
            this.lblSubtitle.TabIndex  = 12;

            // ── Panel blanco (card central) ───────────────────────────────────
            this.pnlCard.BackColor    = System.Drawing.Color.White;
            this.pnlCard.Location     = new System.Drawing.Point(40, 145);
            this.pnlCard.Size         = new System.Drawing.Size(380, 310);
            this.pnlCard.Name         = "pnlCard";
            this.pnlCard.TabIndex     = 13;
            this.pnlCard.Padding      = new System.Windows.Forms.Padding(0);

            // ── Label "Usuario" ────────────────────────────────────────────────
            this.lblUsuario.AutoSize  = false;
            this.lblUsuario.Text      = "Usuario";
            this.lblUsuario.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblUsuario.ForeColor = System.Drawing.Color.FromArgb(80, 80, 80);
            this.lblUsuario.BackColor = System.Drawing.Color.White;
            this.lblUsuario.Location  = new System.Drawing.Point(30, 35);
            this.lblUsuario.Size      = new System.Drawing.Size(320, 18);
            this.lblUsuario.Name      = "lblUsuario";
            this.lblUsuario.TabIndex  = 0;

            // ── TextBox Usuario ────────────────────────────────────────────────
            this.txtUsuario.Font        = new System.Drawing.Font("Segoe UI", 10F);
            this.txtUsuario.Location    = new System.Drawing.Point(30, 56);
            this.txtUsuario.Size        = new System.Drawing.Size(320, 28);
            this.txtUsuario.Name        = "txtUsuario";
            this.txtUsuario.TabIndex    = 1;
            this.txtUsuario.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtUsuario.BackColor   = System.Drawing.Color.FromArgb(245, 245, 248);

            // ── Label "Contraseña" ─────────────────────────────────────────────
            this.lblContraseña.AutoSize  = false;
            this.lblContraseña.Text      = "Contraseña";
            this.lblContraseña.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblContraseña.ForeColor = System.Drawing.Color.FromArgb(80, 80, 80);
            this.lblContraseña.BackColor = System.Drawing.Color.White;
            this.lblContraseña.Location  = new System.Drawing.Point(30, 105);
            this.lblContraseña.Size      = new System.Drawing.Size(320, 18);
            this.lblContraseña.Name      = "lblContraseña";
            this.lblContraseña.TabIndex  = 2;

            // ── TextBox Contraseña ─────────────────────────────────────────────
            this.txtContraseña.Font         = new System.Drawing.Font("Segoe UI", 10F);
            this.txtContraseña.Location     = new System.Drawing.Point(30, 126);
            this.txtContraseña.Size         = new System.Drawing.Size(320, 28);
            this.txtContraseña.Name         = "txtContraseña";
            this.txtContraseña.TabIndex     = 3;
            this.txtContraseña.PasswordChar = '●';
            this.txtContraseña.BorderStyle  = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtContraseña.BackColor    = System.Drawing.Color.FromArgb(245, 245, 248);

            // ── Label Error ────────────────────────────────────────────────────
            this.lblError.AutoSize  = false;
            this.lblError.Text      = "";
            this.lblError.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblError.ForeColor = System.Drawing.Color.FromArgb(180, 50, 50);
            this.lblError.BackColor = System.Drawing.Color.White;
            this.lblError.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblError.Location  = new System.Drawing.Point(30, 168);
            this.lblError.Size      = new System.Drawing.Size(320, 20);
            this.lblError.Name      = "lblError";
            this.lblError.TabIndex  = 4;

            // ── Botón INGRESAR ─────────────────────────────────────────────────
            this.btnIngresar.Text      = "INGRESAR";
            this.btnIngresar.Font      = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnIngresar.ForeColor = System.Drawing.Color.White;
            this.btnIngresar.BackColor = System.Drawing.Color.FromArgb(18, 18, 30);
            this.btnIngresar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnIngresar.FlatAppearance.BorderColor     = System.Drawing.Color.FromArgb(196, 160, 100);
            this.btnIngresar.FlatAppearance.BorderSize      = 1;
            this.btnIngresar.FlatAppearance.MouseOverBackColor  = System.Drawing.Color.FromArgb(196, 160, 100);
            this.btnIngresar.FlatAppearance.MouseDownBackColor  = System.Drawing.Color.FromArgb(160, 120, 70);
            this.btnIngresar.Location  = new System.Drawing.Point(30, 205);
            this.btnIngresar.Size      = new System.Drawing.Size(320, 42);
            this.btnIngresar.Name      = "btnIngresar";
            this.btnIngresar.TabIndex  = 5;
            this.btnIngresar.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnIngresar.Click    += new System.EventHandler(this.btnIngresar_Click);

            // ── Botón SALIR ────────────────────────────────────────────────────
            this.btnSalir.Text      = "SALIR";
            this.btnSalir.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.btnSalir.ForeColor = System.Drawing.Color.FromArgb(130, 130, 130);
            this.btnSalir.BackColor = System.Drawing.Color.White;
            this.btnSalir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSalir.FlatAppearance.BorderColor    = System.Drawing.Color.FromArgb(200, 200, 200);
            this.btnSalir.FlatAppearance.BorderSize     = 1;
            this.btnSalir.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            this.btnSalir.Location  = new System.Drawing.Point(130, 260);
            this.btnSalir.Size      = new System.Drawing.Size(120, 30);
            this.btnSalir.Name      = "btnSalir";
            this.btnSalir.TabIndex  = 6;
            this.btnSalir.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnSalir.Click    += new System.EventHandler(this.btnSalir_Click);

            // ── Armar jerarquía de controles ──────────────────────────────────
            // Controles dentro del card blanco
            this.pnlCard.Controls.Add(this.lblUsuario);
            this.pnlCard.Controls.Add(this.txtUsuario);
            this.pnlCard.Controls.Add(this.lblContraseña);
            this.pnlCard.Controls.Add(this.txtContraseña);
            this.pnlCard.Controls.Add(this.lblError);
            this.pnlCard.Controls.Add(this.btnIngresar);
            this.pnlCard.Controls.Add(this.btnSalir);

            // Controles directamente en el formulario (sobre fondo oscuro)
            this.Controls.Add(this.lblAccent);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblSubtitle);
            this.Controls.Add(this.pnlCard);

            this.pnlCard.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox  txtUsuario;
        private System.Windows.Forms.TextBox  txtContraseña;
        private System.Windows.Forms.Label    lblUsuario;
        private System.Windows.Forms.Label    lblContraseña;
        private System.Windows.Forms.Button   btnIngresar;
        private System.Windows.Forms.Button   btnSalir;
        private System.Windows.Forms.Label    lblError;
        private System.Windows.Forms.Panel    pnlCard;
        private System.Windows.Forms.Label    lblTitle;
        private System.Windows.Forms.Label    lblSubtitle;
        private System.Windows.Forms.Label    lblAccent;
    }
}
