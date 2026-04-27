namespace GUI
{
    partial class Bitacora
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageSistema = new System.Windows.Forms.TabPage();
            this.dgvSistema = new System.Windows.Forms.DataGridView();
            this.lblResultadosSistema = new System.Windows.Forms.Label();
            this.panelFiltrosSistema = new System.Windows.Forms.Panel();
            this.lblUltimosSistema = new System.Windows.Forms.Label();
            this.nudDias = new System.Windows.Forms.NumericUpDown();
            this.lblDiasSistema = new System.Windows.Forms.Label();
            this.btnUltimosDias = new System.Windows.Forms.Button();
            this.lblUsuarioId = new System.Windows.Forms.Label();
            this.txtUsuario = new System.Windows.Forms.TextBox();
            this.lblActividadSistema = new System.Windows.Forms.Label();
            this.txtActividad = new System.Windows.Forms.TextBox();
            this.lblCriticidadSistema = new System.Windows.Forms.Label();
            this.cmbCriticidad = new System.Windows.Forms.ComboBox();
            this.btnBuscar = new System.Windows.Forms.Button();
            this.btnLimpiar = new System.Windows.Forms.Button();
            this.btnExportSistema = new System.Windows.Forms.Button();
            this.tabPageNegocio = new System.Windows.Forms.TabPage();
            this.dgvNegocio = new System.Windows.Forms.DataGridView();
            this.lblResultadosNegocio = new System.Windows.Forms.Label();
            this.panelFiltrosNegocio = new System.Windows.Forms.Panel();
            this.lblUltimosNegocio = new System.Windows.Forms.Label();
            this.nudNegDias = new System.Windows.Forms.NumericUpDown();
            this.lblDiasNegocio = new System.Windows.Forms.Label();
            this.btnNegUltimosDias = new System.Windows.Forms.Button();
            this.lblTipoEvento = new System.Windows.Forms.Label();
            this.cmbTipoEvento = new System.Windows.Forms.ComboBox();
            this.lblIdPedido = new System.Windows.Forms.Label();
            this.txtNegPedido = new System.Windows.Forms.TextBox();
            this.lblIdCliente = new System.Windows.Forms.Label();
            this.txtNegCliente = new System.Windows.Forms.TextBox();
            this.btnNegBuscar = new System.Windows.Forms.Button();
            this.btnNegLimpiar = new System.Windows.Forms.Button();
            this.btnExportNegocio = new System.Windows.Forms.Button();
            this.tabControl.SuspendLayout();
            this.tabPageSistema.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSistema)).BeginInit();
            this.panelFiltrosSistema.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudDias)).BeginInit();
            this.tabPageNegocio.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvNegocio)).BeginInit();
            this.panelFiltrosNegocio.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudNegDias)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPageSistema);
            this.tabControl.Controls.Add(this.tabPageNegocio);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(1020, 640);
            this.tabControl.TabIndex = 0;
            // 
            // tabPageSistema
            // 
            this.tabPageSistema.Controls.Add(this.dgvSistema);
            this.tabPageSistema.Controls.Add(this.lblResultadosSistema);
            this.tabPageSistema.Controls.Add(this.panelFiltrosSistema);
            this.tabPageSistema.Location = new System.Drawing.Point(4, 22);
            this.tabPageSistema.Name = "tabPageSistema";
            this.tabPageSistema.Size = new System.Drawing.Size(1012, 614);
            this.tabPageSistema.TabIndex = 0;
            this.tabPageSistema.Text = "🔐  Bitácora del Sistema";
            // 
            // dgvSistema
            // 
            this.dgvSistema.AllowUserToAddRows = false;
            this.dgvSistema.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(255)))));
            this.dgvSistema.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvSistema.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvSistema.BackgroundColor = System.Drawing.Color.White;
            this.dgvSistema.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(182)))), ((int)(((byte)(193)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvSistema.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvSistema.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvSistema.Location = new System.Drawing.Point(0, 90);
            this.dgvSistema.Name = "dgvSistema";
            this.dgvSistema.ReadOnly = true;
            this.dgvSistema.RowHeadersVisible = false;
            this.dgvSistema.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvSistema.Size = new System.Drawing.Size(1012, 480);
            this.dgvSistema.TabIndex = 1;
            this.dgvSistema.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.DgvSistema_DataBindingComplete);
            // 
            // lblResultadosSistema
            // 
            this.lblResultadosSistema.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(240)))));
            this.lblResultadosSistema.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblResultadosSistema.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblResultadosSistema.Location = new System.Drawing.Point(0, 570);
            this.lblResultadosSistema.Name = "lblResultadosSistema";
            this.lblResultadosSistema.Padding = new System.Windows.Forms.Padding(6, 2, 0, 2);
            this.lblResultadosSistema.Size = new System.Drawing.Size(1012, 44);
            this.lblResultadosSistema.TabIndex = 2;
            this.lblResultadosSistema.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panelFiltrosSistema
            // 
            this.panelFiltrosSistema.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.panelFiltrosSistema.Controls.Add(this.lblUltimosSistema);
            this.panelFiltrosSistema.Controls.Add(this.nudDias);
            this.panelFiltrosSistema.Controls.Add(this.lblDiasSistema);
            this.panelFiltrosSistema.Controls.Add(this.btnUltimosDias);
            this.panelFiltrosSistema.Controls.Add(this.lblUsuarioId);
            this.panelFiltrosSistema.Controls.Add(this.txtUsuario);
            this.panelFiltrosSistema.Controls.Add(this.lblActividadSistema);
            this.panelFiltrosSistema.Controls.Add(this.txtActividad);
            this.panelFiltrosSistema.Controls.Add(this.lblCriticidadSistema);
            this.panelFiltrosSistema.Controls.Add(this.cmbCriticidad);
            this.panelFiltrosSistema.Controls.Add(this.btnBuscar);
            this.panelFiltrosSistema.Controls.Add(this.btnLimpiar);
            this.panelFiltrosSistema.Controls.Add(this.btnExportSistema);
            this.panelFiltrosSistema.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelFiltrosSistema.Location = new System.Drawing.Point(0, 0);
            this.panelFiltrosSistema.Name = "panelFiltrosSistema";
            this.panelFiltrosSistema.Padding = new System.Windows.Forms.Padding(8);
            this.panelFiltrosSistema.Size = new System.Drawing.Size(1012, 90);
            this.panelFiltrosSistema.TabIndex = 0;
            // 
            // lblUltimosSistema
            // 
            this.lblUltimosSistema.Location = new System.Drawing.Point(10, 14);
            this.lblUltimosSistema.Name = "lblUltimosSistema";
            this.lblUltimosSistema.Size = new System.Drawing.Size(55, 23);
            this.lblUltimosSistema.TabIndex = 0;
            this.lblUltimosSistema.Text = "Últimos";
            // 
            // nudDias
            // 
            this.nudDias.Location = new System.Drawing.Point(68, 10);
            this.nudDias.Maximum = new decimal(new int[] {
            365,
            0,
            0,
            0});
            this.nudDias.Name = "nudDias";
            this.nudDias.Size = new System.Drawing.Size(55, 20);
            this.nudDias.TabIndex = 1;
            this.nudDias.Value = new decimal(new int[] {
            7,
            0,
            0,
            0});
            // 
            // lblDiasSistema
            // 
            this.lblDiasSistema.Location = new System.Drawing.Point(128, 14);
            this.lblDiasSistema.Name = "lblDiasSistema";
            this.lblDiasSistema.Size = new System.Drawing.Size(110, 23);
            this.lblDiasSistema.TabIndex = 2;
            this.lblDiasSistema.Text = "días  (0 = todos)";
            // 
            // btnUltimosDias
            // 
            this.btnUltimosDias.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(100)))), ((int)(((byte)(135)))));
            this.btnUltimosDias.FlatAppearance.BorderSize = 0;
            this.btnUltimosDias.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUltimosDias.ForeColor = System.Drawing.Color.White;
            this.btnUltimosDias.Location = new System.Drawing.Point(242, 9);
            this.btnUltimosDias.Name = "btnUltimosDias";
            this.btnUltimosDias.Size = new System.Drawing.Size(50, 26);
            this.btnUltimosDias.TabIndex = 3;
            this.btnUltimosDias.Text = "Ver";
            this.btnUltimosDias.UseVisualStyleBackColor = false;
            this.btnUltimosDias.Click += new System.EventHandler(this.BtnUltimosDias_Click);
            // 
            // lblUsuarioId
            // 
            this.lblUsuarioId.Location = new System.Drawing.Point(312, 14);
            this.lblUsuarioId.Name = "lblUsuarioId";
            this.lblUsuarioId.Size = new System.Drawing.Size(75, 23);
            this.lblUsuarioId.TabIndex = 4;
            this.lblUsuarioId.Text = "Usuario ID:";
            // 
            // txtUsuario
            // 
            this.txtUsuario.Location = new System.Drawing.Point(390, 10);
            this.txtUsuario.Name = "txtUsuario";
            this.txtUsuario.Size = new System.Drawing.Size(55, 20);
            this.txtUsuario.TabIndex = 5;
            this.txtUsuario.Text = "0";
            // 
            // lblActividadSistema
            // 
            this.lblActividadSistema.Location = new System.Drawing.Point(10, 54);
            this.lblActividadSistema.Name = "lblActividadSistema";
            this.lblActividadSistema.Size = new System.Drawing.Size(70, 23);
            this.lblActividadSistema.TabIndex = 6;
            this.lblActividadSistema.Text = "Actividad:";
            // 
            // txtActividad
            // 
            this.txtActividad.Location = new System.Drawing.Point(82, 50);
            this.txtActividad.Name = "txtActividad";
            this.txtActividad.Size = new System.Drawing.Size(200, 20);
            this.txtActividad.TabIndex = 7;
            // 
            // lblCriticidadSistema
            // 
            this.lblCriticidadSistema.Location = new System.Drawing.Point(296, 54);
            this.lblCriticidadSistema.Name = "lblCriticidadSistema";
            this.lblCriticidadSistema.Size = new System.Drawing.Size(70, 23);
            this.lblCriticidadSistema.TabIndex = 8;
            this.lblCriticidadSistema.Text = "Criticidad:";
            // 
            // cmbCriticidad
            // 
            this.cmbCriticidad.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCriticidad.Items.AddRange(new object[] {
            "Todas",
            "Baja (1)",
            "Media (2)",
            "Alta (3)",
            "Intentos Login (4)",
            "Recuperacion Clave (5)",
            "Bloqueos Cuenta (6)"});
            this.cmbCriticidad.Location = new System.Drawing.Point(368, 50);
            this.cmbCriticidad.Name = "cmbCriticidad";
            this.cmbCriticidad.Size = new System.Drawing.Size(170, 21);
            this.cmbCriticidad.TabIndex = 9;
            // 
            // btnBuscar
            // 
            this.btnBuscar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(100)))), ((int)(((byte)(135)))));
            this.btnBuscar.FlatAppearance.BorderSize = 0;
            this.btnBuscar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBuscar.ForeColor = System.Drawing.Color.White;
            this.btnBuscar.Location = new System.Drawing.Point(548, 48);
            this.btnBuscar.Name = "btnBuscar";
            this.btnBuscar.Size = new System.Drawing.Size(80, 26);
            this.btnBuscar.TabIndex = 10;
            this.btnBuscar.Text = "Buscar";
            this.btnBuscar.UseVisualStyleBackColor = false;
            this.btnBuscar.Click += new System.EventHandler(this.BtnBuscarSistema_Click);
            // 
            // btnLimpiar
            // 
            this.btnLimpiar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLimpiar.Location = new System.Drawing.Point(636, 48);
            this.btnLimpiar.Name = "btnLimpiar";
            this.btnLimpiar.Size = new System.Drawing.Size(80, 26);
            this.btnLimpiar.TabIndex = 11;
            this.btnLimpiar.Text = "Limpiar";
            this.btnLimpiar.Click += new System.EventHandler(this.BtnLimpiar_Click);
            // 
            // btnExportSistema
            // 
            this.btnExportSistema.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(100)))), ((int)(((byte)(135)))));
            this.btnExportSistema.FlatAppearance.BorderSize = 0;
            this.btnExportSistema.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExportSistema.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnExportSistema.ForeColor = System.Drawing.Color.White;
            this.btnExportSistema.Location = new System.Drawing.Point(874, 11);
            this.btnExportSistema.Name = "btnExportSistema";
            this.btnExportSistema.Size = new System.Drawing.Size(130, 66);
            this.btnExportSistema.TabIndex = 12;
            this.btnExportSistema.Text = "📄 Exportar PDF";
            this.btnExportSistema.UseVisualStyleBackColor = false;
            this.btnExportSistema.Click += new System.EventHandler(this.BtnExportSistema_Click);
            // 
            // tabPageNegocio
            // 
            this.tabPageNegocio.Controls.Add(this.dgvNegocio);
            this.tabPageNegocio.Controls.Add(this.lblResultadosNegocio);
            this.tabPageNegocio.Controls.Add(this.panelFiltrosNegocio);
            this.tabPageNegocio.Location = new System.Drawing.Point(4, 22);
            this.tabPageNegocio.Name = "tabPageNegocio";
            this.tabPageNegocio.Size = new System.Drawing.Size(1012, 614);
            this.tabPageNegocio.TabIndex = 1;
            this.tabPageNegocio.Text = "📦  Bitácora de Negocio";
            // 
            // dgvNegocio
            // 
            this.dgvNegocio.AllowUserToAddRows = false;
            this.dgvNegocio.AllowUserToDeleteRows = false;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(255)))));
            this.dgvNegocio.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle3;
            this.dgvNegocio.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvNegocio.BackgroundColor = System.Drawing.Color.White;
            this.dgvNegocio.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(182)))), ((int)(((byte)(193)))));
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvNegocio.DefaultCellStyle = dataGridViewCellStyle4;
            this.dgvNegocio.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvNegocio.Location = new System.Drawing.Point(0, 90);
            this.dgvNegocio.Name = "dgvNegocio";
            this.dgvNegocio.ReadOnly = true;
            this.dgvNegocio.RowHeadersVisible = false;
            this.dgvNegocio.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvNegocio.Size = new System.Drawing.Size(1012, 500);
            this.dgvNegocio.TabIndex = 1;
            // 
            // lblResultadosNegocio
            // 
            this.lblResultadosNegocio.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(235)))), ((int)(((byte)(220)))));
            this.lblResultadosNegocio.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblResultadosNegocio.Location = new System.Drawing.Point(0, 590);
            this.lblResultadosNegocio.Name = "lblResultadosNegocio";
            this.lblResultadosNegocio.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.lblResultadosNegocio.Size = new System.Drawing.Size(1012, 24);
            this.lblResultadosNegocio.TabIndex = 2;
            this.lblResultadosNegocio.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panelFiltrosNegocio
            // 
            this.panelFiltrosNegocio.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(245)))), ((int)(((byte)(238)))));
            this.panelFiltrosNegocio.Controls.Add(this.lblUltimosNegocio);
            this.panelFiltrosNegocio.Controls.Add(this.nudNegDias);
            this.panelFiltrosNegocio.Controls.Add(this.lblDiasNegocio);
            this.panelFiltrosNegocio.Controls.Add(this.btnNegUltimosDias);
            this.panelFiltrosNegocio.Controls.Add(this.lblTipoEvento);
            this.panelFiltrosNegocio.Controls.Add(this.cmbTipoEvento);
            this.panelFiltrosNegocio.Controls.Add(this.lblIdPedido);
            this.panelFiltrosNegocio.Controls.Add(this.txtNegPedido);
            this.panelFiltrosNegocio.Controls.Add(this.lblIdCliente);
            this.panelFiltrosNegocio.Controls.Add(this.txtNegCliente);
            this.panelFiltrosNegocio.Controls.Add(this.btnNegBuscar);
            this.panelFiltrosNegocio.Controls.Add(this.btnNegLimpiar);
            this.panelFiltrosNegocio.Controls.Add(this.btnExportNegocio);
            this.panelFiltrosNegocio.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelFiltrosNegocio.Location = new System.Drawing.Point(0, 0);
            this.panelFiltrosNegocio.Name = "panelFiltrosNegocio";
            this.panelFiltrosNegocio.Padding = new System.Windows.Forms.Padding(8);
            this.panelFiltrosNegocio.Size = new System.Drawing.Size(1012, 90);
            this.panelFiltrosNegocio.TabIndex = 0;
            // 
            // lblUltimosNegocio
            // 
            this.lblUltimosNegocio.Location = new System.Drawing.Point(8, 14);
            this.lblUltimosNegocio.Name = "lblUltimosNegocio";
            this.lblUltimosNegocio.Size = new System.Drawing.Size(55, 23);
            this.lblUltimosNegocio.TabIndex = 0;
            this.lblUltimosNegocio.Text = "Últimos";
            // 
            // nudNegDias
            // 
            this.nudNegDias.Location = new System.Drawing.Point(66, 10);
            this.nudNegDias.Maximum = new decimal(new int[] {
            365,
            0,
            0,
            0});
            this.nudNegDias.Name = "nudNegDias";
            this.nudNegDias.Size = new System.Drawing.Size(55, 20);
            this.nudNegDias.TabIndex = 1;
            this.nudNegDias.Value = new decimal(new int[] {
            7,
            0,
            0,
            0});
            // 
            // lblDiasNegocio
            // 
            this.lblDiasNegocio.Location = new System.Drawing.Point(126, 14);
            this.lblDiasNegocio.Name = "lblDiasNegocio";
            this.lblDiasNegocio.Size = new System.Drawing.Size(110, 23);
            this.lblDiasNegocio.TabIndex = 2;
            this.lblDiasNegocio.Text = "días  (0 = todos)";
            // 
            // btnNegUltimosDias
            // 
            this.btnNegUltimosDias.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(100)))), ((int)(((byte)(135)))));
            this.btnNegUltimosDias.FlatAppearance.BorderSize = 0;
            this.btnNegUltimosDias.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNegUltimosDias.ForeColor = System.Drawing.Color.White;
            this.btnNegUltimosDias.Location = new System.Drawing.Point(240, 9);
            this.btnNegUltimosDias.Name = "btnNegUltimosDias";
            this.btnNegUltimosDias.Size = new System.Drawing.Size(50, 26);
            this.btnNegUltimosDias.TabIndex = 3;
            this.btnNegUltimosDias.Text = "Ver";
            this.btnNegUltimosDias.UseVisualStyleBackColor = false;
            this.btnNegUltimosDias.Click += new System.EventHandler(this.BtnNegUltimosDias_Click);
            // 
            // lblTipoEvento
            // 
            this.lblTipoEvento.Location = new System.Drawing.Point(304, 14);
            this.lblTipoEvento.Name = "lblTipoEvento";
            this.lblTipoEvento.Size = new System.Drawing.Size(40, 23);
            this.lblTipoEvento.TabIndex = 4;
            this.lblTipoEvento.Text = "Tipo:";
            // 
            // cmbTipoEvento
            // 
            this.cmbTipoEvento.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTipoEvento.Items.AddRange(new object[] {
            "Todos",
            "Venta",
            "Cancelacion",
            "Despacho",
            "Entrega",
            "AltaPrenda",
            "ModificacionPrenda",
            "CambioEstadoPrenda",
            "AltaCliente",
            "ModificacionCliente",
            "BajaCliente"});
            this.cmbTipoEvento.Location = new System.Drawing.Point(346, 9);
            this.cmbTipoEvento.Name = "cmbTipoEvento";
            this.cmbTipoEvento.Size = new System.Drawing.Size(160, 21);
            this.cmbTipoEvento.TabIndex = 5;
            // 
            // lblIdPedido
            // 
            this.lblIdPedido.Location = new System.Drawing.Point(518, 14);
            this.lblIdPedido.Name = "lblIdPedido";
            this.lblIdPedido.Size = new System.Drawing.Size(70, 23);
            this.lblIdPedido.TabIndex = 6;
            this.lblIdPedido.Text = "ID Pedido:";
            // 
            // txtNegPedido
            // 
            this.txtNegPedido.Location = new System.Drawing.Point(590, 10);
            this.txtNegPedido.Name = "txtNegPedido";
            this.txtNegPedido.Size = new System.Drawing.Size(60, 20);
            this.txtNegPedido.TabIndex = 7;
            this.txtNegPedido.Text = "0";
            // 
            // lblIdCliente
            // 
            this.lblIdCliente.Location = new System.Drawing.Point(660, 14);
            this.lblIdCliente.Name = "lblIdCliente";
            this.lblIdCliente.Size = new System.Drawing.Size(70, 23);
            this.lblIdCliente.TabIndex = 8;
            this.lblIdCliente.Text = "ID Cliente:";
            // 
            // txtNegCliente
            // 
            this.txtNegCliente.Location = new System.Drawing.Point(732, 10);
            this.txtNegCliente.Name = "txtNegCliente";
            this.txtNegCliente.Size = new System.Drawing.Size(60, 20);
            this.txtNegCliente.TabIndex = 9;
            this.txtNegCliente.Text = "0";
            // 
            // btnNegBuscar
            // 
            this.btnNegBuscar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(100)))), ((int)(((byte)(135)))));
            this.btnNegBuscar.FlatAppearance.BorderSize = 0;
            this.btnNegBuscar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNegBuscar.ForeColor = System.Drawing.Color.White;
            this.btnNegBuscar.Location = new System.Drawing.Point(8, 50);
            this.btnNegBuscar.Name = "btnNegBuscar";
            this.btnNegBuscar.Size = new System.Drawing.Size(80, 28);
            this.btnNegBuscar.TabIndex = 10;
            this.btnNegBuscar.Text = "Buscar";
            this.btnNegBuscar.UseVisualStyleBackColor = false;
            this.btnNegBuscar.Click += new System.EventHandler(this.BtnBuscarNegocio_Click);
            // 
            // btnNegLimpiar
            // 
            this.btnNegLimpiar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNegLimpiar.Location = new System.Drawing.Point(96, 50);
            this.btnNegLimpiar.Name = "btnNegLimpiar";
            this.btnNegLimpiar.Size = new System.Drawing.Size(80, 28);
            this.btnNegLimpiar.TabIndex = 11;
            this.btnNegLimpiar.Text = "Limpiar";
            this.btnNegLimpiar.Click += new System.EventHandler(this.BtnNegLimpiar_Click);
            // 
            // btnExportNegocio
            // 
            this.btnExportNegocio.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(100)))), ((int)(((byte)(135)))));
            this.btnExportNegocio.FlatAppearance.BorderSize = 0;
            this.btnExportNegocio.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExportNegocio.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnExportNegocio.ForeColor = System.Drawing.Color.White;
            this.btnExportNegocio.Location = new System.Drawing.Point(871, 14);
            this.btnExportNegocio.Name = "btnExportNegocio";
            this.btnExportNegocio.Size = new System.Drawing.Size(130, 66);
            this.btnExportNegocio.TabIndex = 12;
            this.btnExportNegocio.Text = "📄 Exportar PDF";
            this.btnExportNegocio.UseVisualStyleBackColor = false;
            this.btnExportNegocio.Click += new System.EventHandler(this.BtnExportNegocio_Click);
            // 
            // Bitacora
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1020, 640);
            this.Controls.Add(this.tabControl);
            this.MinimumSize = new System.Drawing.Size(860, 540);
            this.Name = "Bitacora";
            this.Text = "Auditoría — Bitácoras del Sistema";
            this.Load += new System.EventHandler(this.Bitacora_Load);
            this.tabControl.ResumeLayout(false);
            this.tabPageSistema.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvSistema)).EndInit();
            this.panelFiltrosSistema.ResumeLayout(false);
            this.panelFiltrosSistema.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudDias)).EndInit();
            this.tabPageNegocio.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvNegocio)).EndInit();
            this.panelFiltrosNegocio.ResumeLayout(false);
            this.panelFiltrosNegocio.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudNegDias)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl    tabControl;
        private System.Windows.Forms.TabPage       tabPageSistema;
        private System.Windows.Forms.Panel         panelFiltrosSistema;
        private System.Windows.Forms.Label         lblUltimosSistema;
        private System.Windows.Forms.NumericUpDown nudDias;
        private System.Windows.Forms.Label         lblDiasSistema;
        private System.Windows.Forms.Button        btnUltimosDias;
        private System.Windows.Forms.Label         lblUsuarioId;
        private System.Windows.Forms.TextBox       txtUsuario;
        private System.Windows.Forms.Label         lblActividadSistema;
        private System.Windows.Forms.TextBox       txtActividad;
        private System.Windows.Forms.Label         lblCriticidadSistema;
        private System.Windows.Forms.ComboBox      cmbCriticidad;
        private System.Windows.Forms.Button        btnBuscar;
        private System.Windows.Forms.Button        btnLimpiar;
        private System.Windows.Forms.Button        btnExportSistema;
        private System.Windows.Forms.DataGridView  dgvSistema;
        private System.Windows.Forms.Label         lblResultadosSistema;
        private System.Windows.Forms.TabPage       tabPageNegocio;
        private System.Windows.Forms.Panel         panelFiltrosNegocio;
        private System.Windows.Forms.Label         lblUltimosNegocio;
        private System.Windows.Forms.NumericUpDown nudNegDias;
        private System.Windows.Forms.Label         lblDiasNegocio;
        private System.Windows.Forms.Button        btnNegUltimosDias;
        private System.Windows.Forms.Label         lblTipoEvento;
        private System.Windows.Forms.ComboBox      cmbTipoEvento;
        private System.Windows.Forms.Label         lblIdPedido;
        private System.Windows.Forms.TextBox       txtNegPedido;
        private System.Windows.Forms.Label         lblIdCliente;
        private System.Windows.Forms.TextBox       txtNegCliente;
        private System.Windows.Forms.Button        btnNegBuscar;
        private System.Windows.Forms.Button        btnNegLimpiar;
        private System.Windows.Forms.Button        btnExportNegocio;
        private System.Windows.Forms.DataGridView  dgvNegocio;
        private System.Windows.Forms.Label         lblResultadosNegocio;
    }
}
