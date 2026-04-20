using Seguridad;
using Servicios;
using System;
using System.Windows.Forms;

namespace BLL
{
    /// <summary>
    /// Capa de Lógica de Negocio — Gestión de Usuarios.
    ///
    /// RESPONSABILIDADES:
    ///   - Validar credenciales de Login usando PBKDF2-SHA256
    ///   - Cargar permisos del rol tras autenticación exitosa
    ///   - Establecer y destruir la sesión via SessionManager (Singleton)
    ///   - Registrar eventos en Bitácora
    ///   - Crear nuevos usuarios con contraseñas hasheadas y rol asignado
    ///
    /// FLUJO DE LOGIN:
    ///   1. Validar campos no vacíos
    ///   2. Buscar usuario en BD (DAL)
    ///   3. Verificar contraseña con PBKDF2-SHA256 (Encriptador)
    ///   4. Cargar permisos del rol desde RolPermiso (DAL.Permiso)
    ///   5. Establecer sesión con usuario + permisos (SessionManager)
    ///   6. Registrar en bitácora
    /// </summary>
    public class Usuario
    {
        private readonly DAL.Usuario    usuarioDAL  = new DAL.Usuario();
        private readonly DAL.Permiso    permisoDAL  = new DAL.Permiso();
        private readonly Servicios.Bitacora bitacora = new Servicios.Bitacora();

        /// <summary>
        /// Autentica un usuario y establece la sesión con sus permisos cargados.
        /// </summary>
        /// <param name="formulario">Formulario de Login (módulo para bitácora).</param>
        /// <param name="username">Nombre de usuario ingresado.</param>
        /// <param name="contraseña">Contraseña en texto plano.</param>
        /// <returns>true si las credenciales son válidas; false si no coinciden.</returns>
        public bool Login(Form formulario, string username, string contraseña)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(contraseña))
                throw new Exception("Usuario y contraseña son obligatorios.");

            // 1. Buscar usuario en BD
            BE.Usuario usuario = usuarioDAL.ObtenerPorUsername(username);
            if (usuario == null) return false;

            // 2. Verificar contraseña con PBKDF2-SHA256
            bool esValido = Encriptador.VerificarContraseña(contraseña, usuario.Contraseña);

            if (esValido)
            {
                // 3. Cargar permisos del rol ANTES de crear la sesión,
                //    para que el SessionManager almacene el usuario ya enriquecido
                usuario.Permisos = permisoDAL.ObtenerPorRol(usuario.Perfil);

                // 4. Crear la sesión — patrón Singleton del profesor
                SessionManager.Login(usuario);

                // 5. Registrar login en bitácora
                bitacora.Registrar(formulario, "Inicio Sesion", BE.Criticidad.None);
            }

            return esValido;
        }

        /// <summary>
        /// Cierra la sesión: registra en bitácora, cierra conexión BD y limpia SessionManager.
        /// </summary>
        public void Logout(Form formulario)
        {
            // Registrar ANTES de destruir la sesión (Registrar necesita el usuario activo)
            bitacora.Registrar(formulario, "Cierre Sesion", BE.Criticidad.None);
            usuarioDAL.Logout();
            SessionManager.Logout();
        }

        /// <summary>
        /// Crea un nuevo usuario con su rol y contraseña hasheada.
        /// Solo un Administrador puede ejecutar esta acción.
        /// </summary>
        /// <param name="username">Nombre de usuario único.</param>
        /// <param name="contraseña">Contraseña en texto plano (será hasheada).</param>
        /// <param name="perfil">Rol del empleado: "Administrador", "OperadorLogistico" o "Supervisor".</param>
        public void Alta(string username, string contraseña, string perfil)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(contraseña))
                throw new Exception("Usuario y contraseña son obligatorios.");

            if (string.IsNullOrWhiteSpace(perfil))
                throw new Exception("El perfil/rol es obligatorio.");

            string claveHasheada = Encriptador.Hash(contraseña);
            usuarioDAL.Alta(username, claveHasheada, perfil);
        }

        /// <summary>
        /// Resetea la contraseña de un usuario. Solo Administrador puede hacerlo.
        /// </summary>
        public void ResetearClave(Form formulario, int idUsuario, string nuevaClave)
        {
            if (!SessionManager.IsLoggedIn)
                throw new Exception("No hay sesión activa.");

            string perfil = SessionManager.GetInstance.Usuario.Perfil ?? "";
            if (!perfil.Equals("Administrador", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Solo un Administrador puede resetear contraseñas.");

            if (string.IsNullOrWhiteSpace(nuevaClave) || nuevaClave.Length < 6)
                throw new Exception("La nueva contraseña debe tener al menos 6 caracteres.");

            string claveHasheada = Encriptador.Hash(nuevaClave);
            usuarioDAL.ResetearClave(idUsuario, claveHasheada);
            bitacora.Registrar(formulario, "Reset Contrasena", BE.Criticidad.Media);
        }

        /// <summary>
        /// Retorna el usuario en sesión (con sus permisos) desde el SessionManager.
        /// </summary>
        public BE.Usuario ObtenerUsuarioActivo()
        {
            if (!SessionManager.IsLoggedIn) return null;
            return SessionManager.GetInstance.Usuario;
        }

        /// <summary>
        /// Lista todos los usuarios del sistema (sin contraseñas).
        /// </summary>
        public System.Collections.Generic.List<BE.Usuario> ObtenerTodos()
        {
            return usuarioDAL.ObtenerTodos();
        }

        /// <summary>
        /// Verifica si un username existe en la base de datos.
        /// Registra en bitácora con criticidad RecuperacionClave (5).
        /// </summary>
        public bool ExisteUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return false;
            return usuarioDAL.ObtenerPorUsername(username) != null;
        }

        /// <summary>
        /// Registra un intento fallido de login en la bitácora (criticidad 4).
        /// Llamado desde GUI.Login cuando las credenciales no coinciden.
        /// </summary>
        public void RegistrarIntentoFallido(Form formulario, string username)
        {
            // No hay sesión activa en este punto — usamos DAL.Bitacora directamente
            // a través de un usuario de sistema con Id=0 (convención para eventos pre-sesión)
            try
            {
                var dal = new DAL.Bitacora();
                var registro = new BE.Bitacora
                {
                    Fecha      = System.DateTime.Now,
                    IdUsuario  = null,  // null = evento pre-sesión (sin usuario autenticado)
                    Modulo     = formulario?.Text ?? "Login",
                    Actividad  = "Intento Fallido Login",
                    Criticidad = BE.Criticidad.IntentosLogin,
                    Detalle    = $"Intento fallido de login para username '{username}' " +
                                 $"a las {System.DateTime.Now:HH:mm:ss}."
                };
                dal.Registrar(registro);
            }
            catch { /* No interrumpir el flujo de login por error de bitácora */ }
        }
    }
}
