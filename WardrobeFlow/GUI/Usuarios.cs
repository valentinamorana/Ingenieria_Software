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
    /// Permite administrar los usuarios del sistema: ver la lista completa
    /// y crear nuevos usuarios con contraseñas hasheadas.
    ///
    /// Se abre como formulario hijo MDI desde el Menú → Administrar → Usuarios.
    ///
    /// CUMPLE REQUISITOS T02:
    ///   ✓ Gestión de usuarios del sistema
    ///   ✓ Las contraseñas se hashean con PBKDF2 antes de guardarse
    ///   ✓ Se registra la actividad en la bitácora (via BLL)
    /// </summary>
    public partial class Usuarios : Form
    {
        // BLL de usuarios para operaciones de negocio
        private readonly BLL.Usuario usuarioBLL = new BLL.Usuario();

        // ── Controles del formulario ──────────────────────────────────────────
        private DataGridView dgvUsuarios;
        private TextBox txtUsername;
        private TextBox txtContraseña;
        private TextBox txtPerfil;
        private Button btnAgregar;
        private Button btnRefrescar;
        private Label lblMensaje;

        /// <summary>
        /// Constructor: inicializa el formulario y construye la interfaz de gestión de usuarios.
        /// </summary>
        public Usuarios()
        {
            InitializeComponent();
            this.Text        = "Gestión de Usuarios";
            this.ClientSize  = new Size(800, 520);
            this.MinimumSize = new Size(700, 450);

            ConstruirInterfaz();

            // Cargar lista de usuarios al abrir el formulario
            this.Load += (s, e) => CargarUsuarios();
        }

        /// <summary>
        /// Construye la interfaz del formulario programáticamente.
        /// Incluye un panel lateral para agregar usuarios y una grilla para listarlos.
        /// </summary>
        private void ConstruirInterfaz()
        {
            // ── Panel lateral derecho — formulario de alta ────────────────────
            Panel panelAlta = new Panel
            {
                Dock      = DockStyle.Right,
                Width     = 240,
                Padding   = new Padding(12),
                BackColor = Color.FromArgb(245, 245, 250)
            };

            var lblTitulo = new Label
            {
                Text     = "Nuevo Usuario",
                Font     = new Font("Segoe UI", 11, FontStyle.Bold),
                Left     = 12, Top = 12, Width = 210
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
            txtPerfil = new TextBox
            {
                Left  = 12,
                Top   = 168,
                Width = 210
            };

            btnAgregar = new Button
            {
                Text      = "Agregar Usuario",
                Left      = 12,
                Top       = 210,
                Width     = 210,
                Height    = 34,
                BackColor = Color.SteelBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAgregar.Click += BtnAgregar_Click;

            btnRefrescar = new Button
            {
                Text  = "↻ Refrescar Lista",
                Left  = 12,
                Top   = 255,
                Width = 210,
                Height = 30
            };
            btnRefrescar.Click += (s, e) => CargarUsuarios();

            // Mensaje de éxito o error
            lblMensaje = new Label
            {
                Left      = 12,
                Top       = 295,
                Width     = 210,
                Height    = 60,
                ForeColor = Color.DarkGreen,
                Font      = new Font("Segoe UI", 8.5f)
            };

            panelAlta.Controls.AddRange(new Control[]
            {
                lblTitulo, lblUser, txtUsername, lblPass, txtContraseña,
                lblPerfil, txtPerfil, btnAgregar, btnRefrescar, lblMensaje
            });

            // ── DataGridView — lista de usuarios ──────────────────────────────
            dgvUsuarios = new DataGridView
            {
                Dock                     = DockStyle.Fill,
                ReadOnly                 = true,
                AllowUserToAddRows       = false,
                AllowUserToDeleteRows    = false,
                SelectionMode            = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode      = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor          = Color.White,
                RowHeadersVisible        = false,
                BorderStyle              = BorderStyle.None,
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(248, 248, 255) }
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
                // Acceder via BLL (GUI no referencia DAL directamente — respeta arquitectura en capas)
                List<BE.Usuario> usuarios = usuarioBLL.ObtenerTodos();

                // Construir un DataTable simple para la grilla (sin campo Contraseña)
                var tabla = new System.Data.DataTable();
                tabla.Columns.Add("ID",       typeof(int));
                tabla.Columns.Add("Username", typeof(string));
                tabla.Columns.Add("Perfil",   typeof(string));

                foreach (var u in usuarios)
                {
                    tabla.Rows.Add(u.Id, u.Username, u.Perfil ?? "—");
                }

                dgvUsuarios.DataSource = tabla;
                lblMensaje.ForeColor   = Color.DarkGreen;
                lblMensaje.Text        = $"{usuarios.Count} usuario(s) registrado(s).";
            }
            catch (Exception ex)
            {
                lblMensaje.ForeColor = Color.DarkRed;
                lblMensaje.Text      = $"Error al cargar: {ex.Message}";
            }
        }

        /// <summary>
        /// Evento del botón Agregar: valida los datos ingresados, crea el usuario
        /// a través de BLL (que hashea la contraseña) y refresca la lista.
        /// </summary>
        private void BtnAgregar_Click(object sender, EventArgs e)
        {
            lblMensaje.Text = string.Empty;

            // Validaciones básicas en la GUI (BLL también valida)
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

            try
            {
                // Delegar la creación a BLL, que hashea la contraseña antes de ir a DAL
                usuarioBLL.Alta(txtUsername.Text.Trim(), txtContraseña.Text);

                // Mostrar confirmación y limpiar el formulario
                lblMensaje.ForeColor = Color.DarkGreen;
                lblMensaje.Text      = $"✓ Usuario '{txtUsername.Text}' creado correctamente.";
                txtUsername.Clear();
                txtContraseña.Clear();
                txtPerfil.Clear();

                // Refrescar la lista de usuarios
                CargarUsuarios();
            }
            catch (Exception ex)
            {
                MostrarError(ex.Message);
            }
        }

        /// <summary>
        /// Muestra un mensaje de error en el label de feedback del formulario.
        /// </summary>
        private void MostrarError(string mensaje)
        {
            lblMensaje.ForeColor = Color.DarkRed;
            lblMensaje.Text      = $"✗ {mensaje}";
        }
    }
}
