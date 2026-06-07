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
            this.lblLoginSub   = new System.Windows.Forms.Label();
            this.lblUsuario    = new System.Windows.Forms.Label();
            this.txtUsuario    = new System.Windows.Forms.TextBox();
            this.lblContraseña = new System.Windows.Forms.Label();
            this.txtContraseña = new System.Windows.Forms.TextBox();
            this.lnkOlvidaste  = new System.Windows.Forms.LinkLabel();
            this.lblError      = new System.Windows.Forms.Label();
            this.btnIngresar   = new System.Windows.Forms.Button();
            this.lblDivider    = new System.Windows.Forms.Label();
            this.btnSalir      = new System.Windows.Forms.Button();
            this.pnlLeft.SuspendLayout();
            this.pnlCard.SuspendLayout();
            this.SuspendLayout();

            // ── pnlLeft — panel de branding izquierdo (279 × 520) ──────────
            // Fondo blanco; los círculos decorativos se pintan vía Paint event en Login.cs.
            this.pnlLeft.BackColor = System.Drawing.Color.White;
            this.pnlLeft.Controls.Add(this.lblTitle);
            this.pnlLeft.Controls.Add(this.lblSubtitulo);
            this.pnlLeft.Location = new System.Drawing.Point(0, 0);
            this.pnlLeft.Name     = "pnlLeft";
            this.pnlLeft.Size     = new System.Drawing.Size(279, 520);
            this.pnlLeft.TabIndex = 20;

            // ── lblTitle — nombre de marca junto al ícono ─────────────────
            this.lblTitle.AutoSize  = false;
            this.lblTitle.BackColor = System.Drawing.Color.White;
            this.lblTitle.Font      = new System.Drawing.Font("Segoe UI", 11.5F);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(44, 26, 36);
            this.lblTitle.Location  = new System.Drawing.Point(64, 30);
            this.lblTitle.Name      = "lblTitle";
            this.lblTitle.Size      = new System.Drawing.Size(190, 22);
            this.lblTitle.TabIndex  = 11;
            this.lblTitle.Text      = "WardrobeFlow";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // ── lblSubtitulo — "PORTAL DE EMPLEADOS" ─────────────────────
            this.lblSubtitulo.AutoSize  = false;
            this.lblSubtitulo.BackColor = System.Drawing.Color.White;
            this.lblSubtitulo.Font      = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular);
            this.lblSubtitulo.ForeColor = System.Drawing.Color.FromArgb(201, 160, 186);
            this.lblSubtitulo.Location  = new System.Drawing.Point(20, 182);
            this.lblSubtitulo.Name      = "lblSubtitulo";
            this.lblSubtitulo.Size      = new System.Drawing.Size(240, 16);
            this.lblSubtitulo.TabIndex  = 12;
            this.lblSubtitulo.Tag       = "lbl.subtitulo";
            this.lblSubtitulo.Text      = "PORTAL DE EMPLEADOS";
            this.lblSubtitulo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // ── pnlCard — panel derecho con el formulario (440 × 520) ──────
            this.pnlCard.BackColor = System.Drawing.Color.FromArgb(251, 240, 246);
            this.pnlCard.Controls.Add(this.lblAccent);
            this.pnlCard.Controls.Add(this.lblLoginSub);
            this.pnlCard.Controls.Add(this.lblUsuario);
            this.pnlCard.Controls.Add(this.txtUsuario);
            this.pnlCard.Controls.Add(this.lblContraseña);
            this.pnlCard.Controls.Add(this.txtContraseña);
            this.pnlCard.Controls.Add(this.lnkOlvidaste);
            this.pnlCard.Controls.Add(this.lblError);
            this.pnlCard.Controls.Add(this.btnIngresar);
            this.pnlCard.Controls.Add(this.lblDivider);
            this.pnlCard.Controls.Add(this.btnSalir);
            this.pnlCard.Location = new System.Drawing.Point(280, 0);
            this.pnlCard.Name     = "pnlCard";
            this.pnlCard.Size     = new System.Drawing.Size(440, 520);
            this.pnlCard.TabIndex = 13;

            // ── lblAccent — "Bienvenido de nuevo" ─────────────────────────
            this.lblAccent.AutoSize  = false;
            this.lblAccent.BackColor = System.Drawing.Color.Transparent;
            this.lblAccent.Font      = new System.Drawing.Font("Segoe UI", 18F);
            this.lblAccent.ForeColor = System.Drawing.Color.FromArgb(44, 26, 36);
            this.lblAccent.Location  = new System.Drawing.Point(32, 76);
            this.lblAccent.Name      = "lblAccent";
            this.lblAccent.Size      = new System.Drawing.Size(376, 32);
            this.lblAccent.TabIndex  = 10;
            this.lblAccent.Tag       = "lbl.bienvenido";
            this.lblAccent.Text      = "Bienvenido de nuevo";
            this.lblAccent.TextAlign = System.Drawing.ContentAlignment.TopLeft;

            // ── lblLoginSub — subtítulo "Ingresá tus credenciales..." ──────
            this.lblLoginSub.AutoSize  = false;
            this.lblLoginSub.BackColor = System.Drawing.Color.Transparent;
            this.lblLoginSub.Font      = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblLoginSub.ForeColor = System.Drawing.Color.FromArgb(160, 136, 152);
            this.lblLoginSub.Location  = new System.Drawing.Point(32, 112);
            this.lblLoginSub.Name      = "lblLoginSub";
            this.lblLoginSub.Size      = new System.Drawing.Size(376, 18);
            this.lblLoginSub.TabIndex  = 15;
            this.lblLoginSub.Tag       = "lbl.credenciales";
            this.lblLoginSub.Text      = "Ingresá tus credenciales para continuar";

            // ── lblUsuario — etiqueta "USUARIO" (uppercase small) ──────────
            this.lblUsuario.AutoSize  = false;
            this.lblUsuario.BackColor = System.Drawing.Color.Transparent;
            this.lblUsuario.Font      = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblUsuario.ForeColor = System.Drawing.Color.FromArgb(160, 136, 152);
            this.lblUsuario.Location  = new System.Drawing.Point(32, 156);
            this.lblUsuario.Name      = "lblUsuario";
            this.lblUsuario.Size      = new System.Drawing.Size(376, 16);
            this.lblUsuario.TabIndex  = 0;
            this.lblUsuario.Tag       = "lbl.usuario";
            this.lblUsuario.Text      = "USUARIO";
            this.lblUsuario.TextAlign = System.Drawing.ContentAlignment.TopLeft;

            // ── txtUsuario ─────────────────────────────────────────────────
            this.txtUsuario.BackColor   = System.Drawing.Color.White;
            this.txtUsuario.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtUsuario.Font        = new System.Drawing.Font("Segoe UI", 10F);
            this.txtUsuario.Location    = new System.Drawing.Point(32, 174);
            this.txtUsuario.MaxLength   = 50;
            this.txtUsuario.Name        = "txtUsuario";
            this.txtUsuario.Size        = new System.Drawing.Size(376, 25);
            this.txtUsuario.TabIndex    = 1;

            // ── lblContraseña — etiqueta "CONTRASEÑA" ─────────────────────
            this.lblContraseña.AutoSize  = false;
            this.lblContraseña.BackColor = System.Drawing.Color.Transparent;
            this.lblContraseña.Font      = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblContraseña.ForeColor = System.Drawing.Color.FromArgb(160, 136, 152);
            this.lblContraseña.Location  = new System.Drawing.Point(32, 218);
            this.lblContraseña.Name      = "lblContraseña";
            this.lblContraseña.Size      = new System.Drawing.Size(376, 16);
            this.lblContraseña.TabIndex  = 2;
            this.lblContraseña.Tag       = "lbl.contrasena";
            this.lblContraseña.Text      = "CONTRASEÑA";
            this.lblContraseña.TextAlign = System.Drawing.ContentAlignment.TopLeft;

            // ── txtContraseña ──────────────────────────────────────────────
            this.txtContraseña.BackColor   = System.Drawing.Color.White;
            this.txtContraseña.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtContraseña.Font        = new System.Drawing.Font("Segoe UI", 10F);
            this.txtContraseña.Location    = new System.Drawing.Point(32, 236);
            this.txtContraseña.MaxLength   = 100;
            this.txtContraseña.Name        = "txtContraseña";
            this.txtContraseña.PasswordChar = '●';
            this.txtContraseña.Size        = new System.Drawing.Size(376, 25);
            this.txtContraseña.TabIndex    = 3;

            // ── lnkOlvidaste — link alineado a la derecha bajo contraseña ─
            this.lnkOlvidaste.ActiveLinkColor = System.Drawing.Color.FromArgb(176, 62, 96);
            this.lnkOlvidaste.AutoSize        = false;
            this.lnkOlvidaste.BackColor       = System.Drawing.Color.Transparent;
            this.lnkOlvidaste.Font            = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lnkOlvidaste.LinkColor       = System.Drawing.Color.FromArgb(176, 62, 96);
            this.lnkOlvidaste.Location        = new System.Drawing.Point(32, 271);
            this.lnkOlvidaste.Name            = "lnkOlvidaste";
            this.lnkOlvidaste.Size            = new System.Drawing.Size(376, 16);
            this.lnkOlvidaste.TabIndex        = 6;
            this.lnkOlvidaste.TabStop         = true;
            this.lnkOlvidaste.Tag             = "lnk.olvide";
            this.lnkOlvidaste.Text            = "¿Olvidaste tu contraseña?";
            this.lnkOlvidaste.TextAlign       = System.Drawing.ContentAlignment.MiddleRight;
            this.lnkOlvidaste.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkOlvidaste_LinkClicked);

            // ── lblError ───────────────────────────────────────────────────
            this.lblError.AutoSize    = false;
            this.lblError.BackColor   = System.Drawing.Color.FromArgb(251, 240, 246);
            this.lblError.Font        = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblError.ForeColor   = System.Drawing.Color.FromArgb(180, 50, 50);
            this.lblError.Location    = new System.Drawing.Point(32, 311);
            this.lblError.Name        = "lblError";
            this.lblError.Size        = new System.Drawing.Size(376, 34);
            this.lblError.TabIndex    = 4;
            this.lblError.TextAlign   = System.Drawing.ContentAlignment.MiddleCenter;

            // ── btnIngresar — botón primario ancho completo (vino) ─────────
            this.btnIngresar.BackColor                         = System.Drawing.Color.FromArgb(176, 62, 96);
            this.btnIngresar.Cursor                            = System.Windows.Forms.Cursors.Hand;
            this.btnIngresar.FlatAppearance.BorderSize         = 0;
            this.btnIngresar.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(118, 48, 76);
            this.btnIngresar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(162, 76, 108);
            this.btnIngresar.FlatStyle                         = System.Windows.Forms.FlatStyle.Flat;
            this.btnIngresar.Font                              = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnIngresar.ForeColor                         = System.Drawing.Color.White;
            this.btnIngresar.Location                          = new System.Drawing.Point(32, 352);
            this.btnIngresar.Name                              = "btnIngresar";
            this.btnIngresar.Size                              = new System.Drawing.Size(376, 44);
            this.btnIngresar.TabIndex                          = 5;
            this.btnIngresar.Tag                               = "btn.ingresar";
            this.btnIngresar.Text                              = "INGRESAR";
            this.btnIngresar.UseVisualStyleBackColor           = false;
            this.btnIngresar.Click += new System.EventHandler(this.btnIngresar_Click);

            // ── lblDivider — separador "o" con líneas a los lados ─────────
            // Las líneas horizontales se dibujan vía Paint event en Login.cs.
            this.lblDivider.AutoSize  = false;
            this.lblDivider.BackColor = System.Drawing.Color.Transparent;
            this.lblDivider.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblDivider.ForeColor = System.Drawing.Color.FromArgb(192, 168, 180);
            this.lblDivider.Location  = new System.Drawing.Point(32, 406);
            this.lblDivider.Name      = "lblDivider";
            this.lblDivider.Size      = new System.Drawing.Size(376, 22);
            this.lblDivider.TabIndex  = 16;
            this.lblDivider.Tag       = "lbl.divider";
            this.lblDivider.Text      = "o";
            this.lblDivider.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // ── btnSalir — botón secundario ancho completo (blanco bordeado) ─
            this.btnSalir.BackColor                         = System.Drawing.Color.White;
            this.btnSalir.Cursor                            = System.Windows.Forms.Cursors.Hand;
            this.btnSalir.FlatAppearance.BorderColor        = System.Drawing.Color.FromArgb(224, 200, 216);
            this.btnSalir.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(248, 240, 244);
            this.btnSalir.FlatStyle                         = System.Windows.Forms.FlatStyle.Flat;
            this.btnSalir.Font                              = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnSalir.ForeColor                         = System.Drawing.Color.FromArgb(160, 136, 152);
            this.btnSalir.Location                          = new System.Drawing.Point(32, 434);
            this.btnSalir.Name                              = "btnSalir";
            this.btnSalir.Size                              = new System.Drawing.Size(376, 40);
            this.btnSalir.TabIndex                          = 7;
            this.btnSalir.Tag                               = "btn.salir";
            this.btnSalir.Text                              = "Salir";
            this.btnSalir.UseVisualStyleBackColor           = false;
            this.btnSalir.Click += new System.EventHandler(this.btnSalir_Click);

            // ── Login form ─────────────────────────────────────────────────
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor           = System.Drawing.Color.FromArgb(240, 224, 234);
            this.ClientSize          = new System.Drawing.Size(720, 520);
            this.Controls.Add(this.pnlLeft);
            this.Controls.Add(this.pnlCard);
            this.ForeColor           = System.Drawing.Color.Black;
            this.FormBorderStyle     = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon                = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox         = false;
            this.MinimizeBox         = false;
            this.Name                = "Login";
            this.StartPosition       = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Tag                 = "frm.login";
            this.Text                = "WardrobeFlow";
            this.pnlLeft.ResumeLayout(false);
            this.pnlCard.ResumeLayout(false);
            this.pnlCard.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel      pnlLeft;
        private System.Windows.Forms.Label      lblTitle;
        private System.Windows.Forms.Label      lblSubtitulo;
        private System.Windows.Forms.Panel      pnlCard;
        private System.Windows.Forms.Label      lblAccent;
        private System.Windows.Forms.Label      lblLoginSub;
        private System.Windows.Forms.Label      lblDivider;
        private System.Windows.Forms.TextBox    txtUsuario;
        private System.Windows.Forms.TextBox    txtContraseña;
        private System.Windows.Forms.Label      lblUsuario;
        private System.Windows.Forms.Label      lblContraseña;
        private System.Windows.Forms.Button     btnIngresar;
        private System.Windows.Forms.Button     btnSalir;
        private System.Windows.Forms.Label      lblError;
        private System.Windows.Forms.LinkLabel  lnkOlvidaste;
    }
}
