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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Login));
            this.pnlLeft       = new System.Windows.Forms.Panel();
            this.lblTitle      = new System.Windows.Forms.Label();
            this.lblSubtitulo  = new System.Windows.Forms.Label();
            this.pnlCard       = new System.Windows.Forms.Panel();
            this.lblAccent     = new System.Windows.Forms.Label();
            this.lblUsuario    = new System.Windows.Forms.Label();
            this.txtUsuario    = new System.Windows.Forms.TextBox();
            this.lblContraseña = new System.Windows.Forms.Label();
            this.txtContraseña = new System.Windows.Forms.TextBox();
            this.lblError      = new System.Windows.Forms.Label();
            this.btnIngresar   = new System.Windows.Forms.Button();
            this.lnkOlvidaste  = new System.Windows.Forms.LinkLabel();
            this.btnSalir      = new System.Windows.Forms.Button();
            this.pnlLeft.SuspendLayout();
            this.pnlCard.SuspendLayout();
            this.SuspendLayout();

            // ── pnlLeft — branding izquierdo (265 × 420) ────────────────
            this.pnlLeft.BackColor = System.Drawing.Color.FromArgb(146, 62, 96);
            this.pnlLeft.Controls.Add(this.lblTitle);
            this.pnlLeft.Controls.Add(this.lblSubtitulo);
            this.pnlLeft.Location = new System.Drawing.Point(0, 0);
            this.pnlLeft.Name = "pnlLeft";
            this.pnlLeft.Size = new System.Drawing.Size(265, 420);
            this.pnlLeft.TabIndex = 20;

            // ── lblTitle — centrado horizontalmente en el panel izquierdo
            // TextAlign = MiddleCenter + ancho completo del panel garantizan
            // que "WardrobeFlow" y "Portal de Empleados" compartan el mismo
            // eje central, sin depender del renderizado de cada tamaño de fuente.
            this.lblTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(0, 160);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(265, 38);
            this.lblTitle.TabIndex = 11;
            this.lblTitle.Text = "WardrobeFlow";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // ── lblSubtitulo — mismo eje central que lblTitle ────────────
            this.lblSubtitulo.BackColor = System.Drawing.Color.Transparent;
            this.lblSubtitulo.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblSubtitulo.ForeColor = System.Drawing.Color.FromArgb(215, 180, 196);
            this.lblSubtitulo.Location = new System.Drawing.Point(0, 200);
            this.lblSubtitulo.Name = "lblSubtitulo";
            this.lblSubtitulo.Size = new System.Drawing.Size(265, 22);
            this.lblSubtitulo.TabIndex = 12;
            this.lblSubtitulo.Text = "Portal de Empleados";
            this.lblSubtitulo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // ── pnlCard — panel derecho con el formulario (455 × 420) ────
            this.pnlCard.BackColor = System.Drawing.Color.FromArgb(252, 250, 252);
            this.pnlCard.Controls.Add(this.lblAccent);
            this.pnlCard.Controls.Add(this.lblUsuario);
            this.pnlCard.Controls.Add(this.txtUsuario);
            this.pnlCard.Controls.Add(this.lblContraseña);
            this.pnlCard.Controls.Add(this.txtContraseña);
            this.pnlCard.Controls.Add(this.lblError);
            this.pnlCard.Controls.Add(this.btnIngresar);
            this.pnlCard.Controls.Add(this.lnkOlvidaste);
            this.pnlCard.Controls.Add(this.btnSalir);
            this.pnlCard.Location = new System.Drawing.Point(265, 0);
            this.pnlCard.Name = "pnlCard";
            this.pnlCard.Size = new System.Drawing.Size(455, 420);
            this.pnlCard.TabIndex = 13;

            // Todos los controles del panel derecho comparten el mismo Left (32)
            // y el mismo Width (391), garantizando alineación vertical perfecta.
            // Espaciados calculados para que el bloque quede visualmente centrado:
            //   top padding ≈ 70px  |  separación título→campos: 30px
            //   separación entre grupos: 18px  |  separación campos→botón: 22px

            // ── lblAccent — "Iniciar sesión" ─────────────────────────────
            this.lblAccent.BackColor = System.Drawing.Color.Transparent;
            this.lblAccent.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
            this.lblAccent.ForeColor = System.Drawing.Color.FromArgb(40, 40, 60);
            this.lblAccent.Location = new System.Drawing.Point(22, 70);
            this.lblAccent.Name = "lblAccent";
            this.lblAccent.Size = new System.Drawing.Size(401, 30);
            this.lblAccent.TabIndex = 10;
            this.lblAccent.Tag = "lbl.iniciarsesion";
            this.lblAccent.Text = "Iniciar sesión";
            this.lblAccent.TextAlign = System.Drawing.ContentAlignment.TopLeft;

            // ── lblUsuario ───────────────────────────────────────────────
            this.lblUsuario.BackColor = System.Drawing.Color.Transparent;
            this.lblUsuario.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblUsuario.ForeColor = System.Drawing.Color.FromArgb(80, 80, 100);
            this.lblUsuario.Location = new System.Drawing.Point(32, 120);
            this.lblUsuario.Name = "lblUsuario";
            this.lblUsuario.Size = new System.Drawing.Size(391, 15);
            this.lblUsuario.TabIndex = 0;
            this.lblUsuario.Tag = "lbl.usuario";
            this.lblUsuario.Text = "Usuario";
            this.lblUsuario.TextAlign = System.Drawing.ContentAlignment.TopLeft;

            // ── txtUsuario ───────────────────────────────────────────────
            this.txtUsuario.BackColor = System.Drawing.Color.FromArgb(247, 247, 250);
            this.txtUsuario.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtUsuario.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtUsuario.Location = new System.Drawing.Point(32, 138);
            this.txtUsuario.MaxLength = 50;
            this.txtUsuario.Name = "txtUsuario";
            this.txtUsuario.Size = new System.Drawing.Size(391, 25);
            this.txtUsuario.TabIndex = 1;

            // ── lblContraseña ────────────────────────────────────────────
            this.lblContraseña.BackColor = System.Drawing.Color.Transparent;
            this.lblContraseña.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblContraseña.ForeColor = System.Drawing.Color.FromArgb(80, 80, 100);
            this.lblContraseña.Location = new System.Drawing.Point(32, 181);
            this.lblContraseña.Name = "lblContraseña";
            this.lblContraseña.Size = new System.Drawing.Size(391, 15);
            this.lblContraseña.TabIndex = 2;
            this.lblContraseña.Tag = "lbl.contrasena";
            this.lblContraseña.Text = "Contraseña";
            this.lblContraseña.TextAlign = System.Drawing.ContentAlignment.TopLeft;

            // ── txtContraseña ────────────────────────────────────────────
            this.txtContraseña.BackColor = System.Drawing.Color.FromArgb(247, 247, 250);
            this.txtContraseña.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtContraseña.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtContraseña.Location = new System.Drawing.Point(32, 199);
            this.txtContraseña.MaxLength = 100;
            this.txtContraseña.Name = "txtContraseña";
            this.txtContraseña.PasswordChar = '●';
            this.txtContraseña.Size = new System.Drawing.Size(391, 25);
            this.txtContraseña.TabIndex = 3;

            // ── lblError ─────────────────────────────────────────────────
            this.lblError.BackColor = System.Drawing.Color.Transparent;
            this.lblError.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblError.ForeColor = System.Drawing.Color.FromArgb(180, 50, 50);
            this.lblError.Location = new System.Drawing.Point(32, 236);
            this.lblError.Name = "lblError";
            this.lblError.Size = new System.Drawing.Size(391, 15);
            this.lblError.TabIndex = 4;
            this.lblError.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // ── btnIngresar ──────────────────────────────────────────────
            this.btnIngresar.BackColor = System.Drawing.Color.FromArgb(210, 100, 135);
            this.btnIngresar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnIngresar.FlatAppearance.BorderSize = 0;
            this.btnIngresar.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(160, 60, 90);
            this.btnIngresar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(196, 90, 125);
            this.btnIngresar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnIngresar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnIngresar.ForeColor = System.Drawing.Color.White;
            this.btnIngresar.Location = new System.Drawing.Point(32, 261);
            this.btnIngresar.Name = "btnIngresar";
            this.btnIngresar.Size = new System.Drawing.Size(391, 36);
            this.btnIngresar.TabIndex = 5;
            this.btnIngresar.Tag = "btn.ingresar";
            this.btnIngresar.Text = "INGRESAR";
            this.btnIngresar.UseVisualStyleBackColor = false;
            this.btnIngresar.Click += new System.EventHandler(this.btnIngresar_Click);

            // ── lnkOlvidaste ─────────────────────────────────────────────
            this.lnkOlvidaste.ActiveLinkColor = System.Drawing.Color.FromArgb(210, 100, 135);
            this.lnkOlvidaste.BackColor = System.Drawing.Color.Transparent;
            this.lnkOlvidaste.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lnkOlvidaste.LinkColor = System.Drawing.Color.FromArgb(146, 62, 96);
            this.lnkOlvidaste.Location = new System.Drawing.Point(32, 309);
            this.lnkOlvidaste.Name = "lnkOlvidaste";
            this.lnkOlvidaste.Size = new System.Drawing.Size(391, 18);
            this.lnkOlvidaste.TabIndex = 6;
            this.lnkOlvidaste.TabStop = true;
            this.lnkOlvidaste.Tag = "lnk.olvide";
            this.lnkOlvidaste.Text = "Olvidé mi contraseña";
            this.lnkOlvidaste.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lnkOlvidaste.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkOlvidaste_LinkClicked);

            // ── btnSalir — centrado en la parte inferior ─────────────────
            this.btnSalir.BackColor = System.Drawing.Color.FromArgb(252, 250, 252);
            this.btnSalir.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSalir.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(210, 210, 215);
            this.btnSalir.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(248, 243, 246);
            this.btnSalir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSalir.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnSalir.ForeColor = System.Drawing.Color.FromArgb(150, 150, 160);
            this.btnSalir.Location = new System.Drawing.Point(153, 378);
            this.btnSalir.Name = "btnSalir";
            this.btnSalir.Size = new System.Drawing.Size(150, 26);
            this.btnSalir.TabIndex = 7;
            this.btnSalir.Tag = "btn.salir";
            this.btnSalir.Text = "SALIR";
            this.btnSalir.UseVisualStyleBackColor = false;
            this.btnSalir.Click += new System.EventHandler(this.btnSalir_Click);

            // ── Login form ───────────────────────────────────────────────
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(720, 420);
            this.Controls.Add(this.pnlLeft);
            this.Controls.Add(this.pnlCard);
            this.ForeColor = System.Drawing.Color.Black;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Login";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Tag = "frm.login";
            this.Text = "WardrobeFlow";
            this.pnlLeft.ResumeLayout(false);
            this.pnlCard.ResumeLayout(false);
            this.pnlCard.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel      pnlLeft;
        private System.Windows.Forms.TextBox    txtUsuario;
        private System.Windows.Forms.TextBox    txtContraseña;
        private System.Windows.Forms.Label      lblUsuario;
        private System.Windows.Forms.Label      lblContraseña;
        private System.Windows.Forms.Button     btnIngresar;
        private System.Windows.Forms.Button     btnSalir;
        private System.Windows.Forms.Label      lblError;
        private System.Windows.Forms.LinkLabel  lnkOlvidaste;
        private System.Windows.Forms.Panel      pnlCard;
        private System.Windows.Forms.Label      lblTitle;
        private System.Windows.Forms.Label      lblSubtitulo;
        private System.Windows.Forms.Label      lblAccent;
    }
}
