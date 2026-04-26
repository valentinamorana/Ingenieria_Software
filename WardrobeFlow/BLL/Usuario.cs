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
    ///   2. Buscar usuario en BD (incluye IntentosFallidos actual)
    ///   3. Verificar si la cuenta está bloqueada → lanzar excepción si es así
    ///   4. Verificar contraseña con PBKDF2-SHA256
    ///   5a. Si OK: resetear IntentosFallidos en BD + cargar permisos + sesión + bitácora
    ///   5b. Si falla: incrementar IntentosFallidos en BD; si ≥ 3 → bloquear + bitácora
    ///
    /// BLOQUEO DE CUENTA:
    ///   El contador IntentosFallidos persiste en BD — sobrevive reinicios de la app.
    ///   El estado BLOQUEADO (Estado=0) persiste en BD hasta que un Administrador
    ///   lo desbloquee desde Administrar → Usuarios (también resetea el contador).
    /// </summary>
    public class Usuario
    {
        private readonly DAL.Usuario        usuarioDAL = new DAL.Usuario();
        private readonly DAL.Permiso        permisoDAL = new DAL.Permiso();
        private readonly Servicios.Bitacora bitacora   = new Servicios.Bitacora();

        private const int MaxIntentosFallidos = 3;

        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Autentica un usuario y establece la sesión con sus permisos cargados.
        /// Controla automáticamente el bloqueo de cuenta tras 3 intentos fallidos.
        /// El contador de intentos se persiste en BD (no en memoria).
        /// </summary>
        public bool Login(Form formulario, string username, string contraseña)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(contraseña))
                throw new Exception("Usuario y contraseña son obligatorios.");

            // 1. Buscar usuario en BD (incluye IntentosFallidos actual)
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
                // Credenciales correctas → resetear contador en BD
                usuarioDAL.ResetearIntentosFallidos(username);

                // 4. Cargar permisos del rol
                usuario.Permisos = permisoDAL.ObtenerPorRol(usuario.Rol ?? usuario.Perfil);

                // 5. Crear sesión Singleton
                SessionManager.Login(usuario);

                // 6. Registrar en bitácora
                bitacora.Registrar(formulario.Text, "Inicio Sesion", BE.Criticidad.None);
            }
            else
            {
                // Credenciales incorrectas → incrementar contador en BD
                usuarioDAL.IncrementarIntentosFallidos(username);
                int intentos = usuario.IntentosFallidos + 1;  // valor actualizado localmente

                // Registrar intento fallido en bitácora
                RegistrarIntentoFallidoInterno(formulario.Text, username, intentos);

                // Si alcanzó el límite → bloquear cuenta en BD
                if (intentos >= MaxIntentosFallidos)
                {
                    usuarioDAL.Bloquear(usuario.Id);
                    RegistrarBloqueo(formulario.Text, username);

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
        /// Cierra la sesión: registra en bitácora y destruye la sesión Singleton.
        /// </summary>
        public void Logout(Form formulario)
        {
            bitacora.Registrar(formulario.Text, "Cierre Sesion", BE.Criticidad.None);
            usuarioDAL.Logout();
            SessionManager.Logout();
        }

        /// <summary>
        /// Crea un nuevo usuario con su rol y contraseña hasheada con PBKDF2-SHA256.
        /// Solo un Administrador debería ejecutar esta acción.
        /// </summary>
        public void Alta(Form formulario, string username, string contraseña, string perfil)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(contraseña))
                throw new Exception("Usuario y contraseña son obligatorios.");

            if (string.IsNullOrWhiteSpace(perfil))
                throw new Exception("El perfil/rol es obligatorio.");

            string claveHasheada = Encriptador.Hash(contraseña);
            usuarioDAL.Alta(username, claveHasheada, perfil);

            bitacora.Registrar(formulario.Text,
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
            bitacora.Registrar(formulario.Text, "Reset Contrasena", BE.Criticidad.RecuperacionClave);
        }

        /// <summary>
        /// Desbloquea la cuenta de un usuario. Solo Administrador puede hacerlo.
        /// También resetea el contador IntentosFallidos en BD.
        /// </summary>
        public void Desbloquear(Form formulario, int idUsuario, string usernameObjetivo)
        {
            if (!SessionManager.IsLoggedIn)
                throw new Exception("No hay sesión activa.");

            string perfil = SessionManager.GetInstance.Usuario.Perfil ?? "";
            if (!perfil.Equals("Administrador", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Solo un Administrador puede desbloquear cuentas.");

            // Desbloquear en BD y resetear contador (ambos en un solo UPDATE en DAL)
            usuarioDAL.Desbloquear(idUsuario);

            bitacora.Registrar(formulario.Text,
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

        // ── Helpers privados de bitácora de seguridad ─────────────────────────
        // Registran directamente en DAL porque no hay sesión activa al momento del
        // intento fallido — Servicios.Bitacora requiere sesión activa para registrar.

        private void RegistrarIntentoFallidoInterno(string modulo, string username, int numeroIntento)
        {
            try
            {
                new DAL.Bitacora().Registrar(new BE.Bitacora
                {
                    Fecha      = DateTime.Now,
                    IdUsuario  = null,
                    Modulo     = modulo ?? "Login",
                    Actividad  = "Intento Fallido Login",
                    Criticidad = BE.Criticidad.IntentosLogin,
                    IP         = Servicios.Bitacora.ObtenerIPLocal(),
                    Detalle    = $"Intento fallido #{numeroIntento}/{MaxIntentosFallidos} " +
                                 $"para '{username}' a las {DateTime.Now:HH:mm:ss}."
                });
            }
            catch { /* No interrumpir el flujo de login por error de bitácora */ }
        }

        private void RegistrarBloqueo(string modulo, string username)
        {
            try
            {
                new DAL.Bitacora().Registrar(new BE.Bitacora
                {
                    Fecha      = DateTime.Now,
                    IdUsuario  = null,
                    Modulo     = modulo ?? "Login",
                    Actividad  = "Bloqueo de Cuenta",
                    Criticidad = BE.Criticidad.BloqueosCuenta,
                    IP         = Servicios.Bitacora.ObtenerIPLocal(),
                    Detalle    = $"Cuenta '{username}' bloqueada automáticamente tras " +
                                 $"{MaxIntentosFallidos} intentos fallidos consecutivos " +
                                 $"a las {DateTime.Now:HH:mm:ss}."
                });
            }
            catch { }
        }
    }
}
