using Seguridad;
using Servicios;
using System;
using System.Collections.Generic;

namespace BLL
{
    /// <summary>Lógica de negocio para autenticación y gestión de usuarios.</summary>
    public class Usuario
    {
        private readonly DAL.Usuario        usuarioDAL = new DAL.Usuario();
        private readonly DAL.Permiso        permisoDAL = new DAL.Permiso();
        private readonly Servicios.Bitacora bitacora   = new Servicios.Bitacora();

        private const int    MaxIntentosFallidos = 3;
        private const string RolAdministrador    = "Administrador";

        /// <summary>Autentica al usuario y establece la sesión. Bloquea la cuenta tras 3 intentos fallidos.</summary>
        public bool Login(string modulo, string username, string contraseña)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(contraseña))
                throw new Exception("Usuario y contraseña son obligatorios.");

            if (ContadorSesion.GetInstance().LimiteAlcanzado)
                throw new Exception(
                    "Demasiados intentos fallidos en esta sesión.\n" +
                    "Reiniciá la aplicación para volver a intentarlo.");

            BE.Usuario usuario = usuarioDAL.ObtenerPorUsername(username);
            if (usuario == null) return false;

            if (usuario.Bloqueado)
                throw new Exception(
                    $"La cuenta '{username}' está bloqueada.\n" +
                    "Contactá al Administrador para que la reactive desde Administrar → Usuarios.");

            bool esValido = Encriptador.VerificarContrasena(contraseña, usuario.Contraseña);

            if (esValido)
            {
                ContadorSesion.GetInstance().Resetear();
                usuarioDAL.ResetearIntentosFallidos(username);
                usuario.Permisos = permisoDAL.ObtenerPorRol(usuario.Rol ?? usuario.Perfil);
                SessionManager.Login(usuario);
                bitacora.Registrar(modulo, "Inicio Sesion", BE.Criticidad.None);
            }
            else
            {
                ContadorSesion.GetInstance().RegistrarIntento();
                usuarioDAL.IncrementarIntentosFallidos(username);
                int intentos = usuario.IntentosFallidos + 1;

                RegistrarIntentoFallidoInterno(modulo, username, intentos, usuario.Id);

                if (intentos >= MaxIntentosFallidos)
                {
                    usuarioDAL.Bloquear(usuario.Id);
                    RegistrarBloqueo(modulo, username, usuario.Id);

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
        public void Logout(string modulo)
        {
            bitacora.Registrar(modulo, "Cierre Sesion", BE.Criticidad.None);
            usuarioDAL.Logout();
            SessionManager.Logout();
        }

        // Crea un nuevo usuario con rol y contraseña hasheada.
        // Acepta tanto el nombre visible del perfil ("Controlador de Stock")
        // como el código interno ("ControladorDeStock") — normaliza internamente.
        public void Alta(string modulo, string username, string contraseña, string perfil)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(contraseña))
                throw new Exception("Usuario y contraseña son obligatorios.");

            if (username.Trim().Length < 3)
                throw new Exception("El nombre de usuario debe tener al menos 3 caracteres.");

            if (string.IsNullOrWhiteSpace(perfil))
                throw new Exception("El perfil/rol es obligatorio.");

            perfil = NormalizarPerfil(perfil);

            var (valida, mensaje) = Encriptador.ValidarContrasena(contraseña);
            if (!valida) throw new Exception(mensaje);

            string claveHasheada = Encriptador.Hash(contraseña);
            usuarioDAL.Alta(username, claveHasheada, perfil);

            bitacora.Registrar(modulo,
                $"Alta Usuario: '{username}' [{perfil}]",
                BE.Criticidad.Media);
        }

        // Resetea la contraseña de un usuario. Solo Administrador.
        public void ResetearClave(string modulo, int idUsuario, string nuevaClave)
        {
            if (!SessionManager.IsLoggedIn)
                throw new Exception("No hay sesión activa.");

            string perfil = SessionManager.GetInstance.Usuario.Perfil ?? "";
            if (!perfil.Equals(RolAdministrador, StringComparison.OrdinalIgnoreCase))
                throw new Exception("Solo un Administrador puede resetear contraseñas.");

            var (valida, mensaje) = Encriptador.ValidarContrasena(nuevaClave);
            if (!valida) throw new Exception(mensaje);

            var admin = SessionManager.GetInstance.Usuario;

            new VersionUsuario().GrabarVersion(idUsuario, admin.Username,
                $"Snapshot antes de reset de contraseña por '{admin.Username}'.");

            string claveHasheada = Encriptador.Hash(nuevaClave);
            bitacora.RegistrarSinSesion(
                modulo:     modulo,
                actividad:  "Reset Contrasena",
                criticidad: BE.Criticidad.RecuperacionClave,
                idUsuario:  admin.Id,
                detalle:    $"Admin '{admin.Username}' (ID: {admin.Id}) reseteo la contrasena del usuario ID {idUsuario} a las {DateTime.Now:HH:mm:ss}."
            );
        }

        // Desbloquea la cuenta de un usuario y resetea el contador de intentos. Solo Administrador.
        public void Desbloquear(string modulo, int idUsuario, string usernameObjetivo)
        {
            if (!SessionManager.IsLoggedIn)
                throw new Exception("No hay sesión activa.");

            string perfil = SessionManager.GetInstance.Usuario.Perfil ?? "";
            if (!perfil.Equals(RolAdministrador, StringComparison.OrdinalIgnoreCase))
                throw new Exception("Solo un Administrador puede desbloquear cuentas.");

            new VersionUsuario().GrabarVersion(idUsuario,
                SessionManager.GetInstance.Usuario.Username,
                $"Snapshot antes de desbloqueo por '{SessionManager.GetInstance.Usuario.Username}'.");

            usuarioDAL.Desbloquear(idUsuario);

            bitacora.Registrar(modulo,
                $"Desbloqueo de Cuenta: '{usernameObjetivo}'",
                BE.Criticidad.Alta);
        }

        // Resetea la contraseña de TODOS los usuarios a una clave temporal. Solo Administrador.
        public void ResetearTodasLasClaves(string modulo, string claveTemporal)
        {
            if (!SessionManager.IsLoggedIn)
                throw new Exception("No hay sesión activa.");

            string perfil = SessionManager.GetInstance.Usuario.Perfil ?? "";
            if (!perfil.Equals(RolAdministrador, StringComparison.OrdinalIgnoreCase))
                throw new Exception("Solo un Administrador puede realizar esta operación.");

            var (valida, mensaje) = Encriptador.ValidarContrasena(claveTemporal);
            if (!valida) throw new Exception(mensaje);

            string hash = Encriptador.Hash(claveTemporal);
            usuarioDAL.ResetearTodasLasClaves(hash);

            var admin = SessionManager.GetInstance.Usuario;
            bitacora.RegistrarSinSesion(
                modulo:     modulo,
                actividad:  "Reset Masivo Contrasenas",
                criticidad: BE.Criticidad.Alta,
                idUsuario:  admin.Id,
                detalle:    $"Admin '{admin.Username}' (ID: {admin.Id}) reseteo todas las contrasenas a clave temporal a las {DateTime.Now:HH:mm:ss}."
            );
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

        // Registra una solicitud de recuperación de clave en la bitácora.
        // Retorna true si el usuario existe, false si no se encontró.
        // Lanza excepción solo si ocurre un error inesperado en BD.
        public bool SolicitarRecuperacionClave(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return false;

            bool existe = usuarioDAL.ObtenerPorUsername(username) != null;
            if (!existe) return false;

            bitacora.RegistrarSinSesion(
                modulo:     "Recuperar Contrasena",
                actividad:  "Solicitud Recuperacion Clave",
                criticidad: BE.Criticidad.RecuperacionClave,
                detalle:    $"Solicitud de recuperacion de clave para '{username}' a las {DateTime.Now:HH:mm:ss}."
            );

            return true;
        }

        // Persiste la preferencia de idioma del usuario activo.
        // También actualiza el objeto en sesión para que las consultas inmediatas reflejen el cambio.
        public void GuardarPreferenciaIdioma(int idUsuario, string idIdioma)
        {
            usuarioDAL.GuardarIdioma(idUsuario, idIdioma);
            if (Seguridad.SessionManager.IsLoggedIn)
                Seguridad.SessionManager.GetInstance.Usuario.IdIdioma = idIdioma;
        }

        // Expone la validación de contraseña para que la GUI pueda dar feedback
        // temprano sin acceder directamente a la capa Seguridad.
        public (bool valida, string mensaje) ValidarContrasena(string contrasena)
        {
            return Encriptador.ValidarContrasena(contrasena);
        }

        // Convierte el nombre visible del perfil al código interno usado en BD.
        private static string NormalizarPerfil(string perfil)
        {
            switch (perfil.Trim())
            {
                case "Controlador de Stock":   return "ControladorDeStock";
                case "Operador de Inventario": return "OperadorDeInventario";
                default:                       return perfil.Trim();
            }
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
