using Seguridad;
using Servicios;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BLL
{
    /// <summary>Lógica de negocio para autenticación y gestión de usuarios.</summary>
    public class Usuario
    {
        private readonly DAL.Usuario        usuarioDAL = new DAL.Usuario();
        private readonly DAL.Permiso        permisoDAL = new DAL.Permiso();
        private readonly Servicios.Bitacora bitacora   = new Servicios.Bitacora();

        private const int MaxIntentosFallidos = 3;

        /// <summary>Autentica al usuario y establece la sesión. Bloquea la cuenta tras 3 intentos fallidos.</summary>
        public bool Login(Form formulario, string username, string contraseña)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(contraseña))
                throw new Exception("Usuario y contraseña son obligatorios.");

            BE.Usuario usuario = usuarioDAL.ObtenerPorUsername(username);
            if (usuario == null) return false;

            if (usuario.Bloqueado)
                throw new Exception(
                    $"La cuenta '{username}' está bloqueada.\n" +
                    "Contactá al Administrador para que la reactive desde Administrar → Usuarios.");

            bool esValido = Encriptador.VerificarContrasena(contraseña, usuario.Contraseña);

            if (esValido)
            {
                usuarioDAL.ResetearIntentosFallidos(username);
                usuario.Permisos = permisoDAL.ObtenerPorRol(usuario.Rol ?? usuario.Perfil);
                SessionManager.Login(usuario);
                bitacora.Registrar(formulario.Text, "Inicio Sesion", BE.Criticidad.None);
            }
            else
            {
                usuarioDAL.IncrementarIntentosFallidos(username);
                int intentos = usuario.IntentosFallidos + 1;

                RegistrarIntentoFallidoInterno(formulario.Text, username, intentos, usuario.Id);

                if (intentos >= MaxIntentosFallidos)
                {
                    usuarioDAL.Bloquear(usuario.Id);
                    RegistrarBloqueo(formulario.Text, username, usuario.Id);

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
       
        // Cierra la sesión: registra en bitácora y destruye la sesión Singleton.
        public void Logout(Form formulario)
        {
            bitacora.Registrar(formulario.Text, "Cierre Sesion", BE.Criticidad.None);
            usuarioDAL.Logout();
            SessionManager.Logout();
        }

        // Crea un nuevo usuario con rol y contraseña hasheada.
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

        // Resetea la contraseña de un usuario. Solo Administrador.
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

        // Desbloquea la cuenta de un usuario y resetea el contador de intentos. Solo Administrador.
        public void Desbloquear(Form formulario, int idUsuario, string usernameObjetivo)
        {
            if (!SessionManager.IsLoggedIn)
                throw new Exception("No hay sesión activa.");

            string perfil = SessionManager.GetInstance.Usuario.Perfil ?? "";
            if (!perfil.Equals("Administrador", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Solo un Administrador puede desbloquear cuentas.");

            usuarioDAL.Desbloquear(idUsuario);

            bitacora.Registrar(formulario.Text,
                $"Desbloqueo de Cuenta: '{usernameObjetivo}'",
                BE.Criticidad.Alta);
        }

        // Retorna el usuario en sesión (con sus permisos) desde el SessionManager.
        public BE.Usuario ObtenerUsuarioActivo()
        {
            if (!SessionManager.IsLoggedIn) return null;
            return SessionManager.GetInstance.Usuario;
        }

        // Lista todos los usuarios del sistema (sin contraseñas).
        public List<BE.Usuario> ObtenerTodos()
        {
            return usuarioDAL.ObtenerTodos();
        }

        // Verifica si un username existe en la base de datos.
        public bool ExisteUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return false;
            return usuarioDAL.ObtenerPorUsername(username) != null;
        }

        // Registra un intento de login fallido en bitácora.
        private void RegistrarIntentoFallidoInterno(string modulo, string username,
                                                     int numeroIntento, int? idUsuario = null)
        {
            bitacora.RegistrarSinSesion(
                modulo:      modulo ?? "Login",
                actividad:   "Intento Fallido Login",
                criticidad:  BE.Criticidad.IntentosLogin,
                idUsuario:   idUsuario,
                detalle:     $"Intento fallido #{numeroIntento}/{MaxIntentosFallidos} " +
                             $"para '{username}' (ID: {idUsuario?.ToString() ?? "?"}) " +
                             $"a las {DateTime.Now:HH:mm:ss}.");
        }

        // Registra el bloqueo de cuenta en bitácora.
        private void RegistrarBloqueo(string modulo, string username, int? idUsuario = null)
        {
            bitacora.RegistrarSinSesion(
                modulo:      modulo ?? "Login",
                actividad:   "Bloqueo de Cuenta",
                criticidad:  BE.Criticidad.BloqueosCuenta,
                idUsuario:   idUsuario,
                detalle:     $"Cuenta '{username}' (ID: {idUsuario?.ToString() ?? "?"}) " +
                             $"bloqueada automáticamente tras {MaxIntentosFallidos} " +
                             $"intentos fallidos consecutivos a las {DateTime.Now:HH:mm:ss}.");
        }
    }
}
