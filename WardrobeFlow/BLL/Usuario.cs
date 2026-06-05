using Seguridad;
using Servicios;
using System;
using System.Collections.Generic;

namespace BLL
{
    /// <summary>Lógica de negocio para autenticación y gestión de usuarios.</summary>
    public class Usuario
    {
        private readonly DAL.Interfaces.IUsuarioDAL usuarioDAL;
        private readonly BLL.Familia        perfilesBLL = new BLL.Familia();
        private readonly Servicios.Bitacora bitacora   = new Servicios.Bitacora();

        // DI: el constructor por defecto usa el DAL real; el otro permite inyectar un doble.
        public Usuario() : this(new DAL.Usuario()) { }
        public Usuario(DAL.Interfaces.IUsuarioDAL usuarioDAL)
        {
            this.usuarioDAL = usuarioDAL;
        }

        private const int    MaxIntentosFallidos  = 3;
        private const string RolAdministrador    = BE.Roles.Administrador;
        private const string ClaveTemporalDefault = "Wardrobe1!";

        // RF-10 — Días de retención antes de habilitar la purga física de un usuario archivado.
        // Como en una empresa real: el ex-empleado queda "archivado" 1 año (no contamina la
        // operación ni las métricas) y recién después puede eliminarse definitivamente.
        public const int DiasRetencionPurga = 365;

        // Re-validación en el BACKEND: la gestión de usuarios es una operación EXCLUSIVA del
        // Administrador. Se verifica el rol en sesión por Perfil, de forma consistente con
        // SessionManager.TienePermiso (que también identifica al admin por su Perfil).
        private static void ValidarEsAdministrador()
        {
            // Fail-closed: sin sesión NO se permite la operación.
            if (!SessionManager.IsLoggedIn)
                throw new BE.AppException("err.bll.sesion_expirada",
                    "La sesión expiró. Volvé a iniciar sesión.");
            string perfil = SessionManager.GetInstance().Usuario.Perfil ?? "";
            if (!perfil.Equals(RolAdministrador, StringComparison.OrdinalIgnoreCase))
                throw new BE.AppException("err.bll.usuario.sin_permiso",
                    "Solo un Administrador puede gestionar usuarios.");
        }

        /// <summary>Autentica al usuario y establece la sesión. Bloquea la cuenta tras 3 intentos fallidos.</summary>
        public bool Login(string modulo, string username, string contraseña)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(contraseña))
                throw new BE.LoginException(BE.LoginException.TipoError.CamposVacios,
                    "Usuario y contraseña son obligatorios.");

            if (ContadorSesion.GetInstance().LimiteAlcanzado)
                throw new BE.LoginException(BE.LoginException.TipoError.LimiteAlcanzado,
                    "Demasiados intentos fallidos en esta sesión.\n" +
                    "Reiniciá la aplicación para volver a intentarlo.");

            BE.Usuario usuario = usuarioDAL.ObtenerPorUsername(username);
            if (usuario == null)
            {
                // Anti-enumeración: igualar el costo temporal de un usuario real (corre PBKDF2
                // contra un hash señuelo), contar el intento en la sesión y registrarlo. Se
                // devuelve false y la GUI muestra el MISMO mensaje genérico que para
                // credenciales inválidas, sin revelar si el usuario existe.
                Encriptador.VerificacionSenuelo(contraseña);
                ContadorSesion.GetInstance().RegistrarIntento();
                bitacora.RegistrarSinSesion(
                    modulo:     modulo ?? "Login",
                    actividad:  "Intento Fallido Login",
                    criticidad: BE.Criticidad.IntentosLogin,
                    detalle:    $"Intento de login para usuario inexistente '{username}' a las {DateTime.Now:HH:mm:ss}.");
                return false;
            }

            if (usuario.Bloqueado)
                throw new BE.LoginException(BE.LoginException.TipoError.CuentaBloqueada,
                    $"La cuenta '{username}' está bloqueada.\n" +
                    "Contactá al Administrador para que la reactive desde Administrar → Usuarios.");

            bool esValido = Encriptador.VerificarContrasena(contraseña, usuario.Contraseña);

            if (esValido)
            {
                ContadorSesion.GetInstance().Resetear();
                usuarioDAL.ResetearIntentosFallidos(username);
                // T04 — Permisos EFECTIVOS resueltos recursivamente sobre el árbol Composite
                // (rol → roles/familias → patentes), con deduplicación de permisos repetidos.
                usuario.Permisos = perfilesBLL.ObtenerPermisosEfectivos(usuario.Rol ?? usuario.Perfil);
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

                    throw new BE.LoginException(BE.LoginException.TipoError.CuentaBloqueada,
                        $"La cuenta '{username}' ha sido bloqueada tras {MaxIntentosFallidos} " +
                        "intentos fallidos consecutivos.\n" +
                        "Contactá al Administrador para reactivarla.");
                }

                int restantes = MaxIntentosFallidos - intentos;
                throw new BE.LoginException(BE.LoginException.TipoError.CredencialesInvalidas,
                    $"Usuario o contraseña incorrectos.\n" +
                    $"Intentos restantes antes del bloqueo: {restantes}.",
                    intentosRestantes: restantes);
            }

            return esValido;
        }
       
        // Cierra la sesión: registra en bitácora y destruye la sesión Singleton.
        public void Logout(string modulo)
        {
            bitacora.Registrar(modulo, "Cierre Sesion", BE.Criticidad.None);
            SessionManager.Logout();
        }

        // Crea un nuevo usuario con rol y contraseña generada automáticamente.
        // La contraseña NO es ingresada por el administrador — se genera aquí y se
        // exporta a un archivo .txt en CredencialesGeneradas/.
        // Devuelve la ruta del archivo de credenciales generado.
        public string Alta(string modulo, string username, string perfil)
        {
            ValidarEsAdministrador();

            // T07 — Verificar integridad de la base ANTES de modificar usuarios.
            Configuracion.AsegurarIntegridadUsuarios();

            if (string.IsNullOrWhiteSpace(username))
                throw new BE.AppException("err.bll.usuario.username_requerido",
                    "El nombre de usuario es obligatorio.");

            if (username.Trim().Length < 3)
                throw new BE.AppException("err.bll.usuario.username_corto",
                    "El nombre de usuario debe tener al menos 3 caracteres.");

            if (string.IsNullOrWhiteSpace(perfil))
                throw new BE.AppException("err.bll.usuario.perfil_requerido",
                    "El perfil/rol es obligatorio.");

            perfil = NormalizarPerfil(perfil);

            string contrasena    = GeneradorCredenciales.GenerarContrasena();
            string claveHasheada = Encriptador.Hash(contrasena);
            usuarioDAL.Alta(username, claveHasheada, perfil);

            string rutaArchivo = GeneradorCredenciales.ExportarCredenciales(username, contrasena);

            bitacora.Registrar(modulo,
                "Alta Usuario: '" + username + "' [" + perfil + "]",
                BE.Criticidad.Media);

            return rutaArchivo;
        }

        // Resetea la contraseña de un usuario generando una nueva automáticamente.
        // El administrador NO ingresa la contraseña — se genera aquí y se exporta
        // a un archivo .txt en CredencialesGeneradas/.
        // Devuelve la ruta del archivo de credenciales generado.
        public string ResetearClave(string modulo, int idUsuario, string usernameObjetivo)
        {
            ValidarEsAdministrador();

            // T07 — Verificar integridad de la base ANTES de modificar usuarios.
            Configuracion.AsegurarIntegridadUsuarios();

            var admin = SessionManager.GetInstance().Usuario;

            new VersionUsuario().GrabarVersion(idUsuario, admin.Username,
                "Snapshot antes de reset de contraseña por '" + admin.Username + "'.");

            string contrasena    = GeneradorCredenciales.GenerarContrasena();
            string claveHasheada = Encriptador.Hash(contrasena);
            usuarioDAL.ResetearClave(idUsuario, claveHasheada);

            string rutaArchivo = GeneradorCredenciales.ExportarCredenciales(usernameObjetivo, contrasena);

            bitacora.RegistrarSinSesion(
                modulo:     modulo,
                actividad:  "Reset Contrasena",
                criticidad: BE.Criticidad.RecuperacionClave,
                idUsuario:  admin.Id,
                detalle:    "Admin '" + admin.Username + "' (ID: " + admin.Id + ") reseteo la contrasena del usuario ID " + idUsuario + " a las " + DateTime.Now.ToString("HH:mm:ss") + "."
            );

            return rutaArchivo;
        }

        // Desbloquea la cuenta de un usuario y resetea el contador de intentos. Solo Administrador.
        public void Desbloquear(string modulo, int idUsuario, string usernameObjetivo)
        {
            ValidarEsAdministrador();

            // T07 — Verificar integridad de la base ANTES de modificar usuarios.
            Configuracion.AsegurarIntegridadUsuarios();

            new VersionUsuario().GrabarVersion(idUsuario,
                SessionManager.GetInstance().Usuario.Username,
                $"Snapshot antes de desbloqueo por '{SessionManager.GetInstance().Usuario.Username}'.");

            usuarioDAL.Desbloquear(idUsuario);

            bitacora.Registrar(modulo,
                $"Desbloqueo de Cuenta: '{usernameObjetivo}'",
                BE.Criticidad.Alta);
        }

        // RF-10 — Baja LÓGICA (archivar) de un usuario. Solo Administrador.
        // Reglas de protección:
        //   • No se puede archivar al propio usuario en sesión.
        //   • No se puede archivar al ÚLTIMO Administrador activo del sistema.
        // Se graba un snapshot (Memento) antes para preservar trazabilidad (RF-14/18).
        public void Eliminar(string modulo, int idUsuario, string usernameObjetivo)
        {
            ValidarEsAdministrador();

            // T07 — Verificar integridad de la base ANTES de modificar usuarios.
            Configuracion.AsegurarIntegridadUsuarios();

            var admin = SessionManager.GetInstance().Usuario;

            // Determinar el perfil del usuario objetivo para la protección del último admin.
            var objetivo = usuarioDAL.ObtenerPorUsername(usernameObjetivo);
            string perfilObjetivo = objetivo?.Perfil ?? "";
            ValidarPuedeArchivar(perfilObjetivo, idUsuario, admin.Id,
                                 usuarioDAL.ContarAdministradoresActivos());

            // Snapshot del estado actual antes de archivar (control de cambios).
            new VersionUsuario().GrabarVersion(idUsuario, admin.Username,
                $"Snapshot antes de archivar (baja lógica) por '{admin.Username}'.");

            usuarioDAL.BajaLogica(idUsuario);

            bitacora.RegistrarSinSesion(
                modulo:     modulo,
                actividad:  "Baja Logica Usuario",
                criticidad: BE.Criticidad.Alta,
                idUsuario:  admin.Id,
                detalle:    $"Admin '{admin.Username}' archivó al usuario '{usernameObjetivo}' (ID {idUsuario}) a las {DateTime.Now:HH:mm:ss}.");
        }

        // RF-10 — Reglas PURAS de protección para archivar un usuario. Se extraen acá para poder
        // testearlas sin sesión ni base de datos (caso de prueba "eliminar el último admin"):
        //   • No se puede archivar al usuario que tiene la sesión abierta.
        //   • No se puede archivar al último Administrador activo del sistema.
        public static void ValidarPuedeArchivar(string perfilObjetivo, int idObjetivo,
                                                 int idEnSesion, int adminsActivos)
        {
            if (idEnSesion == idObjetivo)
                throw new BE.AppException("err.bll.usuario.autobaja",
                    "No podés archivar tu propio usuario mientras tenés la sesión abierta.");

            if ((perfilObjetivo ?? "").Equals(RolAdministrador, StringComparison.OrdinalIgnoreCase)
                && adminsActivos <= 1)
                throw new BE.AppException("err.bll.usuario.ultimo_admin",
                    "No se puede archivar al último Administrador activo del sistema. " +
                    "Creá o activá otro Administrador antes de archivar este.");
        }

        // RF-10 — Lista de usuarios archivados (Activo=0) para la vista de gestión.
        public List<BE.Usuario> ObtenerArchivados()
        {
            return usuarioDAL.ObtenerArchivados();
        }

        // RF-10 — Usuarios archivados elegibles para purga física (archivados hace más de 1 año).
        public List<BE.Usuario> ObtenerArchivadosParaPurga()
        {
            return usuarioDAL.ObtenerArchivadosParaPurga(DiasRetencionPurga);
        }

        // RF-10 — Purga FÍSICA de todos los usuarios archivados con más de DiasRetencionPurga
        // días de antigüedad. Solo Administrador. Devuelve cuántos se eliminaron definitivamente.
        public int PurgarArchivados(string modulo)
        {
            ValidarEsAdministrador();
            Configuracion.AsegurarIntegridadUsuarios();

            var purgables = usuarioDAL.ObtenerArchivadosParaPurga(DiasRetencionPurga);
            if (purgables.Count == 0) return 0;

            var admin = SessionManager.GetInstance().Usuario;
            int eliminados = 0;
            foreach (var u in purgables)
            {
                usuarioDAL.EliminarFisico(u.Id);
                eliminados++;
            }

            bitacora.RegistrarSinSesion(
                modulo:     modulo,
                actividad:  "Purga Usuarios Archivados",
                criticidad: BE.Criticidad.Alta,
                idUsuario:  admin.Id,
                detalle:    $"Admin '{admin.Username}' purgó definitivamente {eliminados} usuario(s) archivado(s) con más de {DiasRetencionPurga} días a las {DateTime.Now:HH:mm:ss}.");

            return eliminados;
        }

        // Resetea la contraseña de TODOS los usuarios a la clave temporal por defecto. Solo Administrador.
        // Devuelve la clave usada para que la GUI pueda informarla al usuario sin conocerla.
        public string ResetearTodasLasClaves(string modulo)
        {
            ResetearTodasLasClaves(modulo, ClaveTemporalDefault);
            return ClaveTemporalDefault;
        }

        // Resetea la contraseña de TODOS los usuarios a una clave temporal. Solo Administrador.
        public void ResetearTodasLasClaves(string modulo, string claveTemporal)
        {
            ValidarEsAdministrador();

            var (valida, mensaje) = Encriptador.ValidarContrasena(claveTemporal);
            if (!valida)
                throw new BE.AppException("err.bll.usuario.clave_invalida", mensaje);

            string hash = Encriptador.Hash(claveTemporal);
            usuarioDAL.ResetearTodasLasClaves(hash);

            var admin = SessionManager.GetInstance().Usuario;
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
            return SessionManager.GetInstance().Usuario;
        }

        // Retorna la fecha/hora de inicio de la sesión activa, o null si no hay sesión.
        public DateTime? ObtenerFechaInicioSesion()
        {
            if (!SessionManager.IsLoggedIn) return null;
            return SessionManager.GetInstance().FechaInicio;
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
                Seguridad.SessionManager.GetInstance().Usuario.IdIdioma = idIdioma;
        }

        // Expone la validación de contraseña para que la GUI pueda dar feedback
        // temprano sin acceder directamente a la capa Seguridad.
        public (bool valida, string mensaje) ValidarContrasena(string contrasena)
        {
            return Encriptador.ValidarContrasena(contrasena);
        }

        // Valida credenciales sin abrir sesión — para operaciones que requieren confirmación de admin.
        // Retorna true solo si el usuario existe, no está bloqueado, la clave es correcta y tiene rol Administrador.
        public bool ValidarCredencialesAdmin(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return false;

            var usuario = usuarioDAL.ObtenerPorUsername(username);
            if (usuario == null) return false;
            if (usuario.Bloqueado) return false;

            if (!Encriptador.VerificarContrasena(password, usuario.Contraseña)) return false;

            string perfil = usuario.Perfil ?? "";
            return perfil.Equals(RolAdministrador, StringComparison.OrdinalIgnoreCase);
        }

        // Convierte el nombre visible del perfil al código interno usado en BD.
        private static string NormalizarPerfil(string perfil)
        {
            switch (perfil.Trim())
            {
                // Roles legacy
                case "Controlador de Stock":    return "ControladorDeStock";
                case "Operador de Inventario":  return "OperadorDeInventario";
                case "Operador Logístico":      return "OperadorLogistico";
                // Nuevos roles (T04 jerarquía)
                case "Gerente Comercial":       return "GerenteComercial";
                case "Gerente de Inventario":   return "GerenteInventario";
                case "Encargado de Stock":      return "EncargadoDeStock";
                case "Auditor":                 return "Auditor";
                default:                        return perfil.Trim();
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
