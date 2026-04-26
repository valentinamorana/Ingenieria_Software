using Seguridad;
using Servicios;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BLL
{
    /// <summary>
    /// Capa de Lógica de Negocio — Gestión de Usuarios.
    ///
    /// RESPONSABILIDADES:
    ///   - Validar credenciales de Login usando PBKDF2-SHA256 (Encriptador)
    ///   - Controlar bloqueo automático tras 3 intentos fallidos consecutivos (T02)
    ///   - Cargar permisos del rol tras autenticación exitosa
    ///   - Establecer y destruir la sesión via SessionManager (Singleton)
    ///   - Registrar todos los eventos relevantes en Bitácora
    ///   - Crear/resetear usuarios; desbloquear cuentas (solo Administrador)
    ///
    /// FLUJO DE LOGIN (T02 §CU001):
    ///   1. Validar campos no vacíos
    ///   2. Buscar usuario en BD (DAL)
    ///   3. Verificar si la cuenta está bloqueada → lanzar excepción si es así
    ///   4. Verificar contraseña con PBKDF2-SHA256
    ///   5a. Si OK: cargar permisos + crear sesión Singleton + registrar en bitácora
    ///   5b. Si falla: incrementar contador en memoria; si ≥ 3 → bloquear en BD + bitácora
    ///
    /// BLOQUEO DE CUENTA:
    ///   El contador de intentos fallidos es en memoria (se resetea al reiniciar la app).
    ///   El estado BLOQUEADO se persiste en BD (Estado=0) hasta que un Administrador
    ///   lo desbloquee explícitamente desde Administrar → Usuarios.
    /// </summary>
    public class Usuario
    {
        private readonly DAL.Usuario        usuarioDAL = new DAL.Usuario();
        private readonly DAL.Permiso        permisoDAL = new DAL.Permiso();
        private readonly Servicios.Bitacora bitacora   = new Servicios.Bitacora();

        // ── Control de intentos fallidos (T02 §3.3) ──────────────────────────
        // Contador por username en memoria. Persiste mientras la app esté abierta.
        // Al cerrar y reabrir la app, el contador se resetea, pero el bloqueo
        // en BD permanece hasta que un Administrador lo desbloquee manualmente.
        private static readonly Dictionary<string, int> _intentosFallidos =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        private const int MaxIntentosFallidos = 3;

        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Autentica un usuario y establece la sesión con sus permisos cargados.
        /// Controla automáticamente el bloqueo de cuenta tras 3 intentos fallidos.
        /// </summary>
        /// <param name="formulario">Formulario de Login (módulo para bitácora).</param>
        /// <param name="username">Nombre de usuario ingresado.</param>
        /// <param name="contraseña">Contraseña en texto plano.</param>
        /// <returns>true si las credenciales son válidas y la sesión fue creada.</returns>
        /// <exception cref="Exception">Si la cuenta está bloqueada o la cuenta se bloqueó ahora.</exception>
        public bool Login(Form formulario, string username, string contraseña)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(contraseña))
                throw new Exception("Usuario y contraseña son obligatorios.");

            // 1. Buscar usuario en BD
            BE.Usuario usuario = usuarioDAL.ObtenerPorUsername(username);
            if (usuario == null) return false;

            // 2. Verificar si la cuenta ya está bloqueada en BD (T02 §3.2)
            if (usuario.Bloqueado)
                throw new Exception(
                    $"La cuenta '{username}' está bloqueada.\n" +
                    "Contactá al Administrador para que la reactive desde Administrar → Usuarios.");

            // 3. Verificar contraseña con PBKDF2-SHA256
            bool esValido = Encriptador.VerificarContraseña(contraseña, usuario.Contraseña);

            if (esValido)
            {
                // Credenciales correctas → limpiar contador de intentos fallidos
                _intentosFallidos.Remove(username);

                // 4. Cargar permisos del rol (Composite de permisos)
                usuario.Permisos = permisoDAL.ObtenerPorRol(usuario.Rol ?? usuario.Perfil);

                // 5. Crear sesión — patrón Singleton (exactamente como el ejemplo del profesor)
                SessionManager.Login(usuario);

                // 6. Registrar login exitoso en bitácora
                bitacora.Registrar(formulario, "Inicio Sesion", BE.Criticidad.None);
            }
            else
            {
                // Credenciales incorrectas → incrementar contador e intentar bloqueo
                if (!_intentosFallidos.ContainsKey(username))
                    _intentosFallidos[username] = 0;

                _intentosFallidos[username]++;
                int intentos = _intentosFallidos[username];

                // Registrar intento fallido en bitácora (siempre)
                RegistrarIntentoFallidoInterno(formulario, username, intentos);

                // Si alcanzó el límite → bloquear cuenta en BD (T02 §3.3)
                if (intentos >= MaxIntentosFallidos)
                {
                    _intentosFallidos.Remove(username);  // limpiar contador
                    usuarioDAL.Bloquear(usuario.Id);

                    // Registrar bloqueo en bitácora con criticidad BloqueosCuenta
                    RegistrarBloqueo(formulario, username);

                    throw new Exception(
                        $"La cuenta '{username}' ha sido bloqueada tras {MaxIntentosFallidos} " +
                        "intentos fallidos consecutivos.\n" +
                        "Contactá al Administrador para reactivarla.");
                }

                int restantes = MaxIntentosFallidos - intentos;
                throw new Exception(
                    $"Usuario o contraseña incorrectos.\n" +
                    $"Intentos restantes antes del bloqueo: {restantes}.");
            }

            return esValido;
        }

        /// <summary>
        /// Cierra la sesión: registra en bitácora, cierra conexión BD y destruye la sesión Singleton.
        /// </summary>
        public void Logout(Form formulario)
        {
            // Registrar ANTES de destruir la sesión (bitácora necesita el usuario activo)
            bitacora.Registrar(formulario, "Cierre Sesion", BE.Criticidad.None);
            usuarioDAL.Logout();
            SessionManager.Logout();
        }

        /// <summary>
        /// Crea un nuevo usuario con su rol y contraseña hasheada con PBKDF2-SHA256.
        /// Solo un Administrador debería ejecutar esta acción.
        /// Registra el alta en bitácora con criticidad Media.
        /// </summary>
        public void Alta(Form formulario, string username, string contraseña, string perfil)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(contraseña))
                throw new Exception("Usuario y contraseña son obligatorios.");

            if (string.IsNullOrWhiteSpace(perfil))
                throw new Exception("El perfil/rol es obligatorio.");

            string claveHasheada = Encriptador.Hash(contraseña);
            usuarioDAL.Alta(username, claveHasheada, perfil);

            // Registrar en bitácora — gestión de usuarios (T02)
            bitacora.Registrar(formulario,
                $"Alta Usuario: '{username}' [{perfil}]",
                BE.Criticidad.Media);
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
            bitacora.Registrar(formulario, "Reset Contrasena", BE.Criticidad.RecuperacionClave);
        }

        /// <summary>
        /// Desbloquea la cuenta de un usuario. Solo Administrador puede hacerlo.
        /// Reactiva el Estado=1 en BD y registra la acción en bitácora.
        /// </summary>
        public void Desbloquear(Form formulario, int idUsuario, string usernameObjetivo)
        {
            if (!SessionManager.IsLoggedIn)
                throw new Exception("No hay sesión activa.");

            string perfil = SessionManager.GetInstance.Usuario.Perfil ?? "";
            if (!perfil.Equals("Administrador", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Solo un Administrador puede desbloquear cuentas.");

            usuarioDAL.Desbloquear(idUsuario);

            // Limpiar contador en memoria por si quedó algún residuo
            _intentosFallidos.Remove(usernameObjetivo);

            bitacora.Registrar(formulario,
                $"Desbloqueo de Cuenta: '{usernameObjetivo}'",
                BE.Criticidad.Alta);
        }

        /// <summary>Retorna el usuario en sesión (con sus permisos) desde el SessionManager.</summary>
        public BE.Usuario ObtenerUsuarioActivo()
        {
            if (!SessionManager.IsLoggedIn) return null;
            return SessionManager.GetInstance.Usuario;
        }

        /// <summary>Lista todos los usuarios del sistema (sin contraseñas).</summary>
        public List<BE.Usuario> ObtenerTodos()
        {
            return usuarioDAL.ObtenerTodos();
        }

        /// <summary>
        /// Verifica si un username existe en la base de datos.
        /// Usado por el formulario de recuperación de contraseña.
        /// </summary>
        public bool ExisteUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return false;
            return usuarioDAL.ObtenerPorUsername(username) != null;
        }

        // ── Helpers privados para bitácora de seguridad ───────────────────────

        private void RegistrarIntentoFallidoInterno(Form formulario, string username, int numeroIntento)
        {
            try
            {
                var dal = new DAL.Bitacora();
                dal.Registrar(new BE.Bitacora
                {
                    Fecha      = DateTime.Now,
                    IdUsuario  = null,
                    Modulo     = formulario?.Text ?? "Login",
                    Actividad  = "Intento Fallido Login",
                    Criticidad = BE.Criticidad.IntentosLogin,
                    Detalle    = $"Intento fallido #{numeroIntento}/{MaxIntentosFallidos} " +
                                 $"para '{username}' a las {DateTime.Now:HH:mm:ss}."
                });
            }
            catch { /* No interrumpir el flujo de login por error de bitácora */ }
        }

        private void RegistrarBloqueo(Form formulario, string username)
        {
            try
            {
                var dal = new DAL.Bitacora();
                dal.Registrar(new BE.Bitacora
                {
                    Fecha      = DateTime.Now,
                    IdUsuario  = null,
                    Modulo     = formulario?.Text ?? "Login",
                    Actividad  = "Bloqueo de Cuenta",
                    Criticidad = BE.Criticidad.BloqueosCuenta,
                    Detalle    = $"Cuenta '{username}' bloqueada automáticamente tras " +
                                 $"{MaxIntentosFallidos} intentos fallidos consecutivos " +
                                 $"a las {DateTime.Now:HH:mm:ss}."
                });
            }
            catch { }
        }
    }
}
