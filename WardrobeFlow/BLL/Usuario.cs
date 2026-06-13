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
        // perfilesBLL y bitacora son PEREZOSOS: solo se instancian cuando una operación los usa
        // (Login resuelve permisos; las escrituras registran bitácora). Así construir BLL.Usuario
        // —y testear con un IUsuarioDAL falso— no toca la BD a través de sus DAL internos.
        private BLL.Familia _perfilesLazy;
        private BLL.Familia perfilesBLL => _perfilesLazy ?? (_perfilesLazy = new BLL.Familia());
        private Servicios.Bitacora _bitacoraLazy;
        private Servicios.Bitacora bitacora => _bitacoraLazy ?? (_bitacoraLazy = new Servicios.Bitacora());

        // DI: el constructor por defecto usa el DAL real; el otro permite inyectar un doble.
        public Usuario() : this(new DAL.Usuario()) { }
        public Usuario(DAL.Interfaces.IUsuarioDAL usuarioDAL)
        {
            this.usuarioDAL = usuarioDAL;
        }

        private const int    MaxIntentosFallidos  = 3;

        // Clave temporal por defecto para el reset masivo. Configurable en App.config
        // (appSettings["ClaveTemporalDefault"]); si falta o está vacía, usa un fallback válido.
        // Antes estaba hardcodeada; sacarla a config evita exponer la clave en el binario.
        private static readonly string ClaveTemporalDefault = LeerClaveTemporalDefault();

        private static string LeerClaveTemporalDefault()
        {
            string v = System.Configuration.ConfigurationManager.AppSettings["ClaveTemporalDefault"];
            return string.IsNullOrWhiteSpace(v) ? "Wardrobe1!" : v;
        }

        // Bloqueo PROGRESIVO: duración (en minutos) según cuántas veces ya se bloqueó la cuenta.
        // 1er bloqueo → 1 min, 2do → 5, 3ro → 15, 4to → 60; superada la escala, queda permanente.
        private static readonly int[] _minutosBloqueo = { 1, 5, 15, 60 };

        // Evalúa una cuenta bloqueada. Devuelve:
        //   expirado    = el bloqueo TEMPORAL ya venció → se puede reactivar y continuar.
        //   permanente  = no auto-expira (bloqueo manual del admin, sin fecha, o escala agotada).
        //   minutosRest = minutos que faltan si todavía no expiró.
        private static (bool expirado, bool permanente, int minutosRestantes) EvaluarBloqueo(BE.Usuario u)
        {
            // Sin fecha de bloqueo (bloqueo manual del admin o BD sin migrar) → no auto-expira.
            if (!u.FechaBloqueo.HasValue) return (false, true, 0);
            // Escala agotada → bloqueo permanente.
            if (u.CantidadBloqueos <= 0 || u.CantidadBloqueos > _minutosBloqueo.Length)
                return (false, true, 0);

            int minutos = _minutosBloqueo[u.CantidadBloqueos - 1];
            double transcurridos = (DateTime.Now - u.FechaBloqueo.Value).TotalMinutes;
            if (transcurridos >= minutos) return (true, false, 0);
            return (false, false, (int)Math.Ceiling(minutos - transcurridos));
        }

        // RF-10 — Días de retención antes de habilitar la purga física de un usuario archivado.
        // Como en una empresa real: el ex-empleado queda "archivado" 1 año (no contamina la
        // operación ni las métricas) y recién después puede eliminarse definitivamente.
        public const int DiasRetencionPurga = 365;

        // Re-validación en el BACKEND: la gestión de usuarios es una operación EXCLUSIVA del
        // Administrador. Se verifica el rol en sesión por Perfil, de forma consistente con
        // SessionManager.TienePermiso (que también identifica al admin por su Perfil).
        private static void ValidarEsAdministrador()
        {
            // Guard centralizado (DRY): sesión activa + rol Administrador.
            BLLHelper.ExigirAdministrador("err.bll.usuario.sin_permiso",
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
                // lanza EXACTAMENTE la misma excepción, mensaje y contador (de sesión) que para
                // una contraseña incorrecta, de modo que el atacante no pueda distinguir si el
                // usuario existe — ni por el texto, ni por la presencia del contador, ni por el tiempo.
                Encriptador.VerificacionSenuelo(contraseña);
                ContadorSesion.GetInstance().RegistrarIntento();
                bitacora.RegistrarSinSesion(
                    modulo:     modulo ?? "Login",
                    actividad:  "Intento Fallido Login",
                    criticidad: BE.Criticidad.IntentosLogin,
                    detalle:    $"Intento de login para usuario inexistente '{username}' a las {DateTime.Now:HH:mm:ss}.");
                throw new BE.LoginException(BE.LoginException.TipoError.CredencialesInvalidas,
                    "Usuario o contraseña incorrectos.",
                    intentosRestantes: ContadorSesion.GetInstance().IntentosRestantes);
            }

            if (usuario.Bloqueado)
            {
                var (expirado, permanente, minutos) = EvaluarBloqueo(usuario);
                if (permanente)
                    throw new BE.LoginException(BE.LoginException.TipoError.CuentaBloqueada,
                        $"La cuenta '{username}' está bloqueada.\n" +
                        "Contactá al Administrador (o usá una clave de emergencia) para reactivarla.");
                if (!expirado)
                    throw new BE.LoginException(BE.LoginException.TipoError.CuentaBloqueada,
                        $"La cuenta '{username}' está bloqueada temporalmente.\n" +
                        $"Reintentá en {minutos} minuto(s) o usá una clave de emergencia.");

                // El bloqueo temporal EXPIRÓ → se reactiva sola y el login continúa normalmente.
                usuarioDAL.AutoDesbloquear(usuario.Id);
                usuario.Bloqueado        = false;
                usuario.IntentosFallidos = 0;
            }

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
                    // Bloqueo PROGRESIVO: cada bloqueo dura más (1/5/15/60 min) y tras agotar la
                    // escala queda permanente (requiere admin / clave de emergencia).
                    usuarioDAL.BloquearConTiempo(usuario.Id);
                    RegistrarBloqueo(modulo, username, usuario.Id);

                    int nuevaCantidad = usuario.CantidadBloqueos + 1;
                    string msgBloqueo = nuevaCantidad > _minutosBloqueo.Length
                        ? $"La cuenta '{username}' fue bloqueada permanentemente tras varios bloqueos.\n" +
                          "Contactá al Administrador (o usá una clave de emergencia) para reactivarla."
                        : $"La cuenta '{username}' fue bloqueada por {_minutosBloqueo[nuevaCantidad - 1]} " +
                          $"minuto(s) tras {MaxIntentosFallidos} intentos fallidos.\n" +
                          "Reintentá más tarde o usá una clave de emergencia.";

                    throw new BE.LoginException(BE.LoginException.TipoError.CuentaBloqueada, msgBloqueo);
                }

                // Mismo mensaje y mismo contador (de sesión) que el caso "usuario inexistente":
                // indistinguibles entre sí (anti-enumeración). El bloqueo de la CUENTA ya se
                // resolvió arriba; acá solo se informa el intento fallido genérico.
                throw new BE.LoginException(BE.LoginException.TipoError.CredencialesInvalidas,
                    "Usuario o contraseña incorrectos.",
                    intentosRestantes: ContadorSesion.GetInstance().IntentosRestantes);
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
        // Sobrecarga simple (sin datos de perfil) — usada por callers heredados.
        public string Alta(string modulo, string username, string perfil)
        {
            return Alta(modulo, username, perfil, null, null, null, null);
        }

        // Crea un nuevo usuario con rol, datos administrativos (no sensibles) y contraseña
        // generada automáticamente. La contraseña NO la ingresa el administrador — se genera
        // aquí y se exporta a un .txt en CredencialesGeneradas/. Devuelve la ruta del archivo.
        // Graba una versión base (Memento) para que el primer cambio tenga "valor anterior".
        public string Alta(string modulo, string username, string perfil,
                           string nombre, string apellido, DateTime? fechaNacimiento, string email)
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

            // Validaciones puras de datos administrativos (email, fecha, username único).
            ValidarDatosAdministrativos(username, email, fechaNacimiento, ObtenerUsernamesExcepto(0));

            perfil = NormalizarPerfil(perfil);

            string contrasena    = GeneradorCredenciales.GenerarContrasena();
            string claveHasheada = Encriptador.Hash(contrasena);
            usuarioDAL.Alta(username.Trim(), claveHasheada, perfil);

            // Persistir los datos administrativos del alta (si se ingresaron) y grabar la versión base.
            var nuevo = usuarioDAL.ObtenerPorUsername(username.Trim());
            if (nuevo != null)
            {
                bool hayPerfil = !string.IsNullOrWhiteSpace(nombre) || !string.IsNullOrWhiteSpace(apellido)
                                 || !string.IsNullOrWhiteSpace(email) || fechaNacimiento.HasValue;
                if (hayPerfil)
                    usuarioDAL.Modificar(nuevo.Id, Limpiar(nombre), Limpiar(apellido), username.Trim(),
                                         fechaNacimiento, Limpiar(email));

                try
                {
                    new VersionUsuario().GrabarVersion(nuevo.Id,
                        SessionManager.GetInstance().Usuario.Username,
                        "Alta de usuario (estado administrativo inicial).");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError("[BLL.Usuario.Alta] snapshot base: " + ex.Message);
                }
            }

            string rutaArchivo = GeneradorCredenciales.ExportarCredenciales(username.Trim(), contrasena);

            bitacora.Registrar(modulo,
                "Alta Usuario: '" + username.Trim() + "' [" + perfil + "]",
                BE.Criticidad.Media);

            return rutaArchivo;
        }

        // ── ABM — Modificación de datos administrativos NO sensibles ─────────────
        // Edita nombre, apellido, username, fecha de nacimiento y email. NUNCA toca la
        // contraseña. Graba una versión (Memento) con el NUEVO estado, de modo que el
        // Historial de Cambios pueda mostrar campo/valor anterior/valor nuevo (diff entre
        // versiones consecutivas) y se registre en bitácora para auditoría.
        public void Modificar(string modulo, int idUsuario, string nombre, string apellido,
                              string username, DateTime? fechaNacimiento, string email)
        {
            ValidarEsAdministrador();
            Configuracion.AsegurarIntegridadUsuarios();

            BE.Usuario antes = ObtenerActivoPorId(idUsuario);
            if (antes == null)
                throw new BE.AppException("err.bll.usuario.no_existe",
                    "El usuario a modificar no existe o está archivado.");

            ValidarDatosAdministrativos(username, email, fechaNacimiento, ObtenerUsernamesExcepto(idUsuario));

            string nuevoUsername = username.Trim();
            string nuevoNombre   = Limpiar(nombre);
            string nuevoApellido = Limpiar(apellido);
            string nuevoEmail    = Limpiar(email);

            string detalle = DescribirCambios(antes, nuevoNombre, nuevoApellido, nuevoUsername, fechaNacimiento, nuevoEmail);
            if (detalle == null)
                throw new BE.AppException("err.bll.usuario.sin_cambios",
                    "No hay cambios para guardar.");

            string actor = SessionManager.GetInstance().Usuario.Username;

            // Si el usuario NO tiene historial previo (p. ej. usuarios semilla creados por script),
            // se graba un snapshot BASELINE del estado ACTUAL (antes del cambio) para que el Historial
            // de Cambios tenga "valor anterior" desde la primera modificación (diff entre versiones).
            var versionBLL = new VersionUsuario();
            if (versionBLL.ObtenerPorUsuario(idUsuario).Count == 0)
                versionBLL.GrabarVersion(idUsuario, actor, "Estado administrativo inicial.");

            usuarioDAL.Modificar(idUsuario, nuevoNombre, nuevoApellido, nuevoUsername, fechaNacimiento, nuevoEmail);

            // Snapshot del NUEVO estado (después del UPDATE) → alimenta el Historial de Cambios.
            versionBLL.GrabarVersion(idUsuario, actor, detalle);

            bitacora.RegistrarSinSesion(
                modulo:     modulo,
                actividad:  "Modificación de Usuario",
                criticidad: BE.Criticidad.Media,
                idUsuario:  SessionManager.GetInstance().Usuario.Id,
                detalle:    $"Usuario ID {idUsuario}: {detalle}");
        }

        // ── Cambio de ROL de un usuario (procedimiento documentado) ──────────────
        // Solo Administrador. Protege contra dejar al sistema sin Administradores activos.
        // Graba snapshot y registra en bitácora.
        public void CambiarRol(string modulo, int idUsuario, string nuevoPerfil)
        {
            ValidarEsAdministrador();
            Configuracion.AsegurarIntegridadUsuarios();

            BE.Usuario antes = ObtenerActivoPorId(idUsuario);
            if (antes == null)
                throw new BE.AppException("err.bll.usuario.no_existe",
                    "El usuario a modificar no existe o está archivado.");

            if (string.IsNullOrWhiteSpace(nuevoPerfil))
                throw new BE.AppException("err.bll.usuario.perfil_requerido", "El perfil/rol es obligatorio.");

            string perfilNorm = NormalizarPerfil(nuevoPerfil);

            // Validación pura (testeable): no quitar el rol al último Administrador activo.
            ValidarPuedeCambiarRol(antes.Perfil, perfilNorm, usuarioDAL.ContarAdministradoresActivos());

            if (string.Equals(antes.Perfil, perfilNorm, StringComparison.OrdinalIgnoreCase))
                throw new BE.AppException("err.bll.usuario.sin_cambios", "No hay cambios para guardar.");

            // Snapshot del estado actual antes del cambio (trazabilidad del cambio de rol).
            new VersionUsuario().GrabarVersion(idUsuario,
                SessionManager.GetInstance().Usuario.Username,
                $"Cambio de rol: '{antes.Perfil}' → '{perfilNorm}'.");

            usuarioDAL.CambiarRol(idUsuario, perfilNorm);

            bitacora.RegistrarSinSesion(
                modulo:     modulo,
                actividad:  "Cambio de Rol de Usuario",
                criticidad: BE.Criticidad.Alta,
                idUsuario:  SessionManager.GetInstance().Usuario.Id,
                detalle:    $"Usuario ID {idUsuario} ('{antes.Username}'): rol '{antes.Perfil}' → '{perfilNorm}'.");
        }

        // Búsqueda por datos NO sensibles (nombre, apellido, email, username). Filtra en
        // memoria sobre los usuarios activos (dataset chico; respeta protección de datos).
        public List<BE.Usuario> Buscar(string filtro)
        {
            var todos = usuarioDAL.ObtenerTodos();
            if (string.IsNullOrWhiteSpace(filtro)) return todos;
            string f = filtro.Trim().ToLowerInvariant();
            bool Coincide(string s) => !string.IsNullOrEmpty(s) && s.ToLowerInvariant().Contains(f);
            return todos.FindAll(u => Coincide(u.Nombre) || Coincide(u.Apellido)
                                      || Coincide(u.Email) || Coincide(u.Username));
        }

        // ── Validaciones PURAS (testeables sin sesión ni BD) ─────────────────────

        // Valida los datos administrativos de un usuario: username (no vacío, ≥3, único entre
        // 'otrosUsernames'), email (formato si se ingresó) y fecha de nacimiento (no futura,
        // mayoría de edad). Reutilizable por Alta y Modificar.
        public static void ValidarDatosAdministrativos(string username, string email,
                                                        DateTime? fechaNacimiento, IEnumerable<string> otrosUsernames)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new BE.AppException("err.bll.usuario.username_requerido", "El nombre de usuario es obligatorio.");
            if (username.Trim().Length < 3)
                throw new BE.AppException("err.bll.usuario.username_corto",
                    "El nombre de usuario debe tener al menos 3 caracteres.");
            if (otrosUsernames != null)
                foreach (var u in otrosUsernames)
                    if (string.Equals(u, username.Trim(), StringComparison.OrdinalIgnoreCase))
                        throw new BE.AppException("err.bll.usuario.username_duplicado",
                            "Ya existe un usuario con el nombre '{0}'.", username.Trim());

            if (!string.IsNullOrWhiteSpace(email) && !EsEmailValido(email.Trim()))
                throw new BE.AppException("err.bll.usuario.email_invalido",
                    "El correo electrónico no tiene un formato válido.");

            if (fechaNacimiento.HasValue)
            {
                if (fechaNacimiento.Value.Date > DateTime.Today)
                    throw new BE.AppException("err.bll.usuario.fechanac_futura",
                        "La fecha de nacimiento no puede ser futura.");
                if (fechaNacimiento.Value.Date > DateTime.Today.AddYears(-18))
                    throw new BE.AppException("err.bll.usuario.fechanac_menor",
                        "El usuario debe ser mayor de edad (18 años).");
            }
        }

        // No se puede quitar el rol de Administrador al ÚLTIMO Administrador activo.
        public static void ValidarPuedeCambiarRol(string perfilActual, string nuevoPerfil, int adminsActivos)
        {
            if (BE.Roles.EsAdministrador(perfilActual)
                && !BE.Roles.EsAdministrador(nuevoPerfil)
                && adminsActivos <= 1)
                throw new BE.AppException("err.bll.usuario.ultimo_admin_rol",
                    "No se puede quitar el rol de Administrador al último administrador activo. " +
                    "Asigná el rol Administrador a otro usuario antes de cambiar este.");
        }

        // Validación de email simple y robusta (sin regex frágil): un '@' con texto a ambos
        // lados y un '.' en el dominio.
        private static bool EsEmailValido(string email)
        {
            int at = email.IndexOf('@');
            if (at <= 0 || at != email.LastIndexOf('@') || at == email.Length - 1) return false;
            string dominio = email.Substring(at + 1);
            int punto = dominio.IndexOf('.');
            return punto > 0 && punto < dominio.Length - 1 && !email.Contains(" ");
        }

        // Devuelve un resumen "Campo: 'anterior' → 'nuevo'; ..." de los campos que cambian,
        // o null si no hay cambios. Compara solo datos administrativos NO sensibles.
        private static string DescribirCambios(BE.Usuario antes, string nombre, string apellido,
                                               string username, DateTime? fechaNac, string email)
        {
            var partes = new List<string>();
            void Cmp(string campo, string viejo, string nuevo)
            {
                if (!string.Equals(viejo ?? "", nuevo ?? "", StringComparison.Ordinal))
                    partes.Add($"{campo}: '{viejo ?? ""}' → '{nuevo ?? ""}'");
            }
            Cmp("Usuario",  antes.Username, username);
            Cmp("Nombre",   antes.Nombre,   nombre);
            Cmp("Apellido", antes.Apellido, apellido);
            Cmp("Email",    antes.Email,    email);
            string fechaVieja = antes.FechaNacimiento?.ToString("yyyy-MM-dd") ?? "";
            string fechaNueva = fechaNac?.ToString("yyyy-MM-dd") ?? "";
            if (!string.Equals(fechaVieja, fechaNueva, StringComparison.Ordinal))
                partes.Add($"Fecha nac.: '{fechaVieja}' → '{fechaNueva}'");
            return partes.Count == 0 ? null : string.Join("; ", partes);
        }

        private static string Limpiar(string s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

        // Usuario activo por Id (desde la lista de activos; la edición ABM opera sobre activos).
        private BE.Usuario ObtenerActivoPorId(int idUsuario)
        {
            return usuarioDAL.ObtenerTodos().Find(u => u.Id == idUsuario);
        }

        // Usernames de los demás usuarios (para validar unicidad excluyendo 'idExcluir').
        // Incluye ACTIVOS y ARCHIVADOS: el UNIQUE(Username) es global, así que un username de un
        // usuario archivado igual está tomado (evita un error de BD poco claro al chocar con la UQ).
        private List<string> ObtenerUsernamesExcepto(int idExcluir)
        {
            var lista = new List<string>();
            foreach (var u in usuarioDAL.ObtenerTodos())
                if (u.Id != idExcluir) lista.Add(u.Username);
            try
            {
                foreach (var u in usuarioDAL.ObtenerArchivados())
                    if (u.Id != idExcluir) lista.Add(u.Username);
            }
            catch { /* BD sin migrar (sin archivados): se valida solo contra activos */ }
            return lista;
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

            if (BE.Roles.EsAdministrador(perfilObjetivo)
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

        // Las claves de emergencia (autodesbloqueo de Admin) viven en BLL.RecuperacionAdmin (SRP).

        // Cambio de clave por el PROPIO usuario en sesión. Lo usa el cambio OBLIGATORIO posterior
        // al login (cuando RequiereCambioClave=1) y también puede usarlo "Mi Perfil".
        // Valida la clave nueva, exige que difiera de la actual, persiste, baja el flag y
        // actualiza la sesión. No requiere ser administrador: cada uno cambia SU propia clave.
        public void CambiarClavePropia(string modulo, string claveNueva)
        {
            if (!SessionManager.IsLoggedIn)
                throw new BE.SesionException("err.seg.sesion_no_iniciada",
                    "La sesión no está iniciada. Iniciá sesión primero.");

            var u = SessionManager.GetInstance().Usuario;

            var (valida, mensaje) = Encriptador.ValidarContrasena(claveNueva);
            if (!valida)
                throw new BE.AppException("err.bll.usuario.clave_invalida", mensaje);

            // La clave nueva no puede ser la misma que la actual (evita "cambiarla" por la temporal).
            if (Encriptador.VerificarContrasena(claveNueva, u.Contraseña))
                throw new BE.AppException("err.bll.usuario.clave_igual_actual",
                    "La nueva contraseña no puede ser igual a la actual.");

            string hash = Encriptador.Hash(claveNueva);
            usuarioDAL.CambiarClave(u.Id, hash);

            // Reflejar el cambio en la sesión para que no se vuelva a pedir.
            u.Contraseña          = hash;
            u.RequiereCambioClave = false;

            bitacora.Registrar(modulo, "Cambio de Contrasena Propia", BE.Criticidad.Media);
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

            return usuario.EsAdministrador;
        }

        // Convierte el nombre visible del perfil al código interno usado en BD.
        private static string NormalizarPerfil(string perfil)
        {
            switch (perfil.Trim())
            {
                // Jerarquía consolidada (2da entrega)
                case "Operador de Inventario":  return "OperadorDeInventario"; // mantenimiento de prendas
                case "Operador Logístico":      return "OperadorLogistico";    // pedidos / despacho
                case "Gerente Comercial":       return "GerenteComercial";
                case "Gerente de Inventario":   return "GerenteInventario";
                case "Auditor":                 return "Auditor";
                // Roles retirados → se mapean a su reemplazo (por si llega una etiqueta vieja)
                case "Controlador de Stock":    return "OperadorDeInventario";
                case "Encargado de Stock":      return "OperadorDeInventario";
                case "Supervisor":              return "GerenteComercial";
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
