using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace GUI
{
    /// <summary>
    /// Capa de Presentación — Formulario de Gestión de Usuarios.
    ///
    /// Permite administrar los usuarios del sistema: ver la lista completa,
    /// crear nuevos usuarios y resetear contraseñas (solo Administrador).
    ///
    /// Se abre como formulario hijo MDI desde el Menú → Administrar → Usuarios.
    ///
    /// CUMPLE REQUISITOS T02:
    ///   ✓ Gestión de usuarios del sistema
    ///   ✓ Las contraseñas se hashean con PBKDF2 antes de guardarse
    ///   ✓ Se registra la actividad en la bitácora (via BLL)
    ///   ✓ Solo un Administrador puede resetear contraseñas ajenas
    /// </summary>
    public partial class Usuarios : Form
    {
        // BLL de usuarios para operaciones de negocio
        private readonly BLL.Usuario usuarioBLL = new BLL.Usuario();

        // ── Controles del formulario ──────────────────────────────────────────
        private DataGridView dgvUsuarios;
        private TextBox txtUsername;
        private TextBox txtContraseña;
        private ComboBox cmbPerfil;
        private Button btnAgregar;
        private Button btnRefrescar;
        private Button btnResetearClave;
        private Label lblMensaje;

        /// <summary>
        /// Constructor: inicializa el formulario y construye la interfaz de gestión de usuarios.
        /// </summary>
        public Usuarios()
        {
            InitializeComponent();
            this.Text        = "Gestión de Usuarios";
            this.ClientSize  = new Size(820, 540);
            this.MinimumSize = new Size(720, 460);

            ConstruirInterfaz();

            // Cargar lista de usuarios al abrir el formulario
            this.Load += (s, e) => CargarUsuarios();
        }

        /// <summary>
        /// Construye la interfaz del formulario programáticamente.
        /// Panel lateral derecho para operaciones, grilla central para la lista.
        /// </summary>
        private void ConstruirInterfaz()
        {
            // ── Panel lateral derecho ─────────────────────────────────────────
            Panel panelAlta = new Panel
            {
                Dock      = DockStyle.Right,
                Width     = 240,
                Padding   = new Padding(12),
                BackColor = Color.FromArgb(245, 245, 250)
            };

            // ── Sección: Nuevo Usuario ────────────────────────────────────────
            var lblTitulo = new Label
            {
                Text = "Nuevo Usuario",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Left = 12, Top = 12, Width = 210
            };

            var lblUser = new Label { Text = "Nombre de usuario:", Left = 12, Top = 50, Width = 200 };
            txtUsername = new TextBox { Left = 12, Top = 68, Width = 210 };

            var lblPass = new Label { Text = "Contraseña:", Left = 12, Top = 100, Width = 200 };
            txtContraseña = new TextBox
            {
                Left         = 12,
                Top          = 118,
                Width        = 210,
                PasswordChar = '●'
            };

            var lblPerfil = new Label { Text = "Perfil (rol):", Left = 12, Top = 150, Width = 200 };
            cmbPerfil = new ComboBox
            {
                Left          = 12, Top = 168, Width = 210,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbPerfil.Items.AddRange(new object[]
            {
                "Administrador",
                "OperadorLogistico",
                "Supervisor"
            });
            cmbPerfil.SelectedIndex = 1; // OperadorLogistico por defecto

            btnAgregar = new Button
            {
                Text      = "Agregar Usuario",
                Left      = 12,  Top    = 210,
                Width     = 210, Height = 34,
                BackColor = Color.SteelBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAgregar.FlatAppearance.BorderSize = 0;
            btnAgregar.Click += BtnAgregar_Click;

            btnRefrescar = new Button
            {
                Text   = "↻ Refrescar Lista",
                Left   = 12,  Top    = 253,
                Width  = 210, Height = 28
            };
            btnRefrescar.Click += (s, e) => CargarUsuarios();

            // ── Separador visual ──────────────────────────────────────────────
            var separador = new Label
            {
                Left      = 12,  Top    = 292,
                Width     = 210, Height = 1,
                BackColor = Color.Silver
            };

            // ── Sección: Resetear Contraseña ──────────────────────────────────
            var lblResetTitulo = new Label
            {
                Text = "Resetear Contraseña",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Left = 12, Top = 302, Width = 210
            };

            var lblResetInfo = new Label
            {
                Text      = "Seleccioná un usuario\nen la lista y presioná:",
                Left      = 12, Top    = 325,
                Width     = 210, Height = 36,
                ForeColor = Color.DimGray,
                Font      = new Font("Segoe UI", 8.5f)
            };

            btnResetearClave = new Button
            {
                Text      = "Resetear Contrasena",
                Left      = 12,  Top    = 366,
                Width     = 210, Height = 34,
                BackColor = Color.FromArgb(180, 100, 30),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled   = false   // se habilita cuando hay una fila seleccionada
            };
            btnResetearClave.FlatAppearance.BorderSize = 0;
            btnResetearClave.Click += BtnResetearClave_Click;

            // Mensaje de éxito o error
            lblMensaje = new Label
            {
                Left      = 12,  Top    = 410,
                Width     = 210, Height = 80,
                ForeColor = Color.DarkGreen,
                Font      = new Font("Segoe UI", 8.5f)
            };

            panelAlta.Controls.AddRange(new Control[]
            {
                lblTitulo, lblUser, txtUsername, lblPass, txtContraseña,
                lblPerfil, cmbPerfil, btnAgregar, btnRefrescar,
                separador, lblResetTitulo, lblResetInfo, btnResetearClave,
                lblMensaje
            });

            // ── DataGridView — lista de usuarios ──────────────────────────────
            dgvUsuarios = new DataGridView
            {
                Dock                            = DockStyle.Fill,
                ReadOnly                        = true,
                AllowUserToAddRows              = false,
                AllowUserToDeleteRows           = false,
                SelectionMode                   = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode             = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor                 = Color.White,
                RowHeadersVisible               = false,
                BorderStyle                     = BorderStyle.None,
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(248, 248, 255)
                }
            };

            // Habilitar el botón Resetear solo cuando hay selección en la grilla
            dgvUsuarios.SelectionChanged += (s, e) =>
            {
                btnResetearClave.Enabled = dgvUsuarios.SelectedRows.Count > 0;
            };

            // Label de título de la grilla
            var lblListaTitulo = new Label
            {
                Text      = "Usuarios registrados en el sistema",
                Dock      = DockStyle.Top,
                Height    = 28,
                Font      = new Font("Segoe UI", 9, FontStyle.Bold),
                Padding   = new Padding(6, 6, 0, 0),
                BackColor = Color.FromArgb(230, 230, 240)
            };

            // Agregar controles al formulario
            this.Controls.Add(dgvUsuarios);
            this.Controls.Add(lblListaTitulo);
            this.Controls.Add(panelAlta);
        }

        /// <summary>
        /// Carga la lista de usuarios desde la BLL → DAL y la muestra en la grilla.
        /// Las contraseñas NO se muestran por seguridad.
        /// </summary>
        private void CargarUsuarios()
        {
            try
            {
                List<BE.Usuario> usuarios = usuarioBLL.ObtenerTodos();

                var tabla = new DataTable();
                tabla.Columns.Add("ID",       typeof(int));
                tabla.Columns.Add("Username", typeof(string));
                tabla.Columns.Add("Perfil",   typeof(string));

                foreach (var u in usuarios)
                    tabla.Rows.Add(u.Id, u.Username, u.Perfil ?? "—");

                dgvUsuarios.DataSource = tabla;
                lblMensaje.ForeColor   = Color.DarkGreen;
                lblMensaje.Text        = $"{usuarios.Count} usuario(s) registrado(s).";
            }
            catch (Exception ex)
            {
                MostrarError($"Error al cargar: {ex.Message}");
            }
        }

        /// <summary>
        /// Evento del botón Agregar: valida, crea el usuario a través de BLL
        /// (que hashea la contraseña) y refresca la lista.
        /// </summary>
        private void BtnAgregar_Click(object sender, EventArgs e)
        {
            lblMensaje.Text = string.Empty;

            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MostrarError("El nombre de usuario es obligatorio.");
                return;
            }
            if (string.IsNullOrWhiteSpace(txtContraseña.Text) || txtContraseña.Text.Length < 6)
            {
                MostrarError("La contraseña debe tener al menos 6 caracteres.");
                return;
            }

            if (cmbPerfil.SelectedIndex < 0)
            {
                MostrarError("Seleccioná un perfil/rol.");
                return;
            }

            try
            {
                string perfil = cmbPerfil.SelectedItem.ToString();
                usuarioBLL.Alta(txtUsername.Text.Trim(), txtContraseña.Text, perfil);

                lblMensaje.ForeColor = Color.DarkGreen;
                lblMensaje.Text      = $"Usuario '{txtUsername.Text}' [{perfil}] creado.";
                txtUsername.Clear();
                txtContraseña.Clear();
                cmbPerfil.SelectedIndex = 1;

                CargarUsuarios();
            }
            catch (Exception ex)
            {
                MostrarError(ex.Message);
            }
        }

        /// <summary>
        /// Evento del botón Resetear Contraseña.
        /// Obtiene el usuario seleccionado, abre un diálogo para ingresar la nueva clave
        /// y delega el reseteo a BLL (que verifica permisos, hashea y persiste).
        /// </summary>
        private void BtnResetearClave_Click(object sender, EventArgs e)
        {
            if (dgvUsuarios.SelectedRows.Count == 0) return;

            // Leer el usuario seleccionado en la grilla
            DataGridViewRow fila      = dgvUsuarios.SelectedRows[0];
            int             idUsuario = Convert.ToInt32(fila.Cells["ID"].Value);
            string          username  = fila.Cells["Username"].Value?.ToString() ?? "";

            // Confirmación previa — acción irreversible
            var confirmar = MessageBox.Show(
                $"¿Está seguro que desea resetear la contraseña de '{username}'?\n\nEsta acción no se puede deshacer.",
                "Confirmar Reset",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);   // "No" es el botón por defecto

            if (confirmar != DialogResult.Yes) return;

            // Abrir diálogo para ingresar la nueva contraseña
            using (var dialog = new ResetClaveDialog(username))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;

                try
                {
                    usuarioBLL.ResetearClave(this, idUsuario, dialog.NuevaClave);

                    lblMensaje.ForeColor = Color.DarkGreen;
                    lblMensaje.Text      = $"Contrasena de '{username}' reseteada correctamente.";
                }
                catch (Exception ex)
                {
                    MostrarError(ex.Message);
                }
            }
        }

        /// <summary>Muestra un mensaje de error en el label de feedback.</summary>
        private void MostrarError(string mensaje)
        {
            lblMensaje.ForeColor = Color.DarkRed;
            lblMensaje.Text      = $"✗ {mensaje}";
        }
    }
}
