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

        /// <summary>
        /// Constructor: inicializa el formulario y construye la interfaz de gestión de usuarios.
        /// </summary>
        public Usuarios()
        {
            InitializeComponent();
            this.Load += new EventHandler(Usuarios_Load);
        }

        // ── Eventos del Designer ──────────────────────────────────────────────

        private void Usuarios_Load(object sender, EventArgs e)
        {
            CargarUsuarios();
        }

        private void BtnRefrescar_Click(object sender, EventArgs e)
        {
            CargarUsuarios();
        }

        private void DgvUsuarios_SelectionChanged(object sender, EventArgs e)
        {
            bool haySeleccion = dgvUsuarios.SelectedRows.Count > 0;
            btnResetearClave.Enabled = haySeleccion;

            // Desbloquear solo se habilita si el usuario seleccionado está bloqueado
            if (haySeleccion)
            {
                var estadoCell = dgvUsuarios.SelectedRows[0].Cells["Estado"];
                btnDesbloquear.Enabled = estadoCell?.Value?.ToString() == "BLOQUEADA";
            }
            else
            {
                btnDesbloquear.Enabled = false;
            }
        }

        // ── Carga ─────────────────────────────────────────────────────────────

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
                tabla.Columns.Add("Estado",   typeof(string));

                foreach (var u in usuarios)
                    tabla.Rows.Add(u.Id, u.Username, u.Perfil ?? "—",
                        u.Bloqueado ? "BLOQUEADA" : "Activo");

                dgvUsuarios.DataSource = tabla;

                // Colorear filas bloqueadas en rojo claro para resaltar visualmente (T02)
                foreach (DataGridViewRow fila in dgvUsuarios.Rows)
                {
                    if (fila.Cells["Estado"].Value?.ToString() == "BLOQUEADA")
                    {
                        fila.DefaultCellStyle.BackColor = Color.FromArgb(255, 220, 220);
                        fila.DefaultCellStyle.ForeColor = Color.DarkRed;
                    }
                }

                lblMensaje.ForeColor = Color.DarkGreen;
                lblMensaje.Text      = $"{usuarios.Count} usuario(s) registrado(s).";
            }
            catch (Exception ex)
            {
                MostrarError($"Error al cargar: {ex.Message}");
            }
        }

        // ── Eventos de botones ────────────────────────────────────────────────

        /// <summary>
        /// Crea un nuevo usuario tras validar que el username sea numérico (DNI)
        /// y que la contraseña tenga al menos 6 caracteres.
        /// </summary>
        private void BtnAgregar_Click(object sender, EventArgs e)
        {
            string username  = txtUsername.Text.Trim();
            string password  = txtContraseña.Text;
            string perfil    = cmbPerfil.SelectedItem?.ToString() ?? "";

            // Validaciones de UI antes de llamar a BLL
            if (string.IsNullOrWhiteSpace(username))
            {
                MostrarError("El nombre de usuario es obligatorio.");
                return;
            }

            // El username debe ser numérico (representa el DNI del empleado)
            foreach (char c in username)
            {
                if (!char.IsDigit(c))
                {
                    MostrarError("El nombre de usuario solo puede contener números (DNI).");
                    txtUsername.Focus();
                    return;
                }
            }

            if (username.Length < 7 || username.Length > 8)
            {
                MostrarError("El DNI debe tener entre 7 y 8 dígitos.");
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            {
                MostrarError("La contraseña debe tener al menos 6 caracteres.");
                return;
            }

            if (string.IsNullOrWhiteSpace(perfil))
            {
                MostrarError("Seleccioná un perfil/rol.");
                return;
            }

            // Mapear nombre visible → código interno que usa RolPermiso en la BD
            string perfilInterno = perfil;
            if (perfil == "Controlador de Stock")    perfilInterno = "ControladorDeStock";
            if (perfil == "Operador de Inventario")  perfilInterno = "OperadorDeInventario";

            try
            {
                usuarioBLL.Alta(this, username, password, perfilInterno);

                // Limpiar campos y refrescar lista
                txtUsername.Clear();
                txtContraseña.Clear();
                cmbPerfil.SelectedIndex = 2;

                CargarUsuarios();
                MostrarOk($"Usuario '{username}' [{perfil}] creado correctamente.");
            }
            catch (Exception ex)
            {
                MostrarError(ex.Message);
            }
        }

        /// <summary>
        /// Resetea la contraseña del usuario seleccionado en la grilla.
        /// Pide la nueva contraseña mediante un diálogo.
        /// Solo funciona si el usuario en sesión es Administrador.
        /// </summary>
        private void BtnResetearClave_Click(object sender, EventArgs e)
        {
            if (dgvUsuarios.SelectedRows.Count == 0)
            {
                MostrarError("Seleccioná un usuario de la lista.");
                return;
            }

            int    idUsuario = Convert.ToInt32(dgvUsuarios.SelectedRows[0].Cells["ID"].Value);
            string username  = dgvUsuarios.SelectedRows[0].Cells["Username"].Value?.ToString() ?? "";

            string nuevaClave = PedirTexto(
                $"Nueva contraseña para '{username}' (mínimo 6 caracteres):",
                "Resetear Contraseña");

            if (string.IsNullOrWhiteSpace(nuevaClave)) return;

            try
            {
                usuarioBLL.ResetearClave(this, idUsuario, nuevaClave);
                MostrarOk($"Contraseña de '{username}' reseteada correctamente.");
            }
            catch (Exception ex)
            {
                MostrarError(ex.Message);
            }
        }

        /// <summary>
        /// Desbloquea la cuenta del usuario seleccionado en la grilla.
        /// Solo funciona si el usuario en sesión es Administrador.
        /// </summary>
        private void BtnDesbloquear_Click(object sender, EventArgs e)
        {
            if (dgvUsuarios.SelectedRows.Count == 0)
            {
                MostrarError("Seleccioná un usuario bloqueado de la lista.");
                return;
            }

            int    idUsuario = Convert.ToInt32(dgvUsuarios.SelectedRows[0].Cells["ID"].Value);
            string username  = dgvUsuarios.SelectedRows[0].Cells["Username"].Value?.ToString() ?? "";

            var confirm = MessageBox.Show(
                $"¿Desbloquear la cuenta de '{username}'?",
                "Confirmar Desbloqueo",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (confirm != DialogResult.Yes) return;

            try
            {
                usuarioBLL.Desbloquear(this, idUsuario, username);
                CargarUsuarios();
                MostrarOk($"Cuenta '{username}' desbloqueada correctamente.");
            }
            catch (Exception ex)
            {
                MostrarError(ex.Message);
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void MostrarError(string msg)
        {
            lblMensaje.ForeColor = Color.DarkRed;
            lblMensaje.Text      = msg;
        }

        private void MostrarOk(string msg)
        {
            lblMensaje.ForeColor = Color.DarkGreen;
            lblMensaje.Text      = msg;
        }

        /// <summary>
        /// Diálogo de entrada de texto simple — reemplaza Microsoft.VisualBasic.Interaction.InputBox
        /// para evitar dependencia externa en el proyecto GUI.
        /// </summary>
        private string PedirTexto(string mensaje, string titulo)
        {
            using (Form dlg = new Form())
            {
                dlg.Text            = titulo;
                dlg.ClientSize      = new System.Drawing.Size(360, 130);
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition  = FormStartPosition.CenterParent;
                dlg.MaximizeBox    = false;
                dlg.MinimizeBox    = false;

                var lbl = new Label
                {
                    Text     = mensaje,
                    Left     = 12, Top   = 12,
                    Width    = 336, Height = 32,
                    Font     = new Font("Segoe UI", 9f)
                };

                var txt = new TextBox
                {
                    Left         = 12,  Top   = 50,
                    Width        = 336, Height = 24,
                    PasswordChar = '●'
                };

                var btnOk = new Button
                {
                    Text         = "Aceptar",
                    Left         = 168, Top    = 88,
                    Width        = 80,  Height = 28,
                    DialogResult = DialogResult.OK
                };
                var btnCancelar = new Button
                {
                    Text         = "Cancelar",
                    Left         = 260, Top    = 88,
                    Width        = 88,  Height = 28,
                    DialogResult = DialogResult.Cancel
                };

                dlg.AcceptButton = btnOk;
                dlg.CancelButton = btnCancelar;
                dlg.Controls.AddRange(new Control[] { lbl, txt, btnOk, btnCancelar });

                return dlg.ShowDialog(this) == DialogResult.OK ? txt.Text : string.Empty;
            }
        }
    }
}
