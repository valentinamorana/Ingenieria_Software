using Seguridad;
using Servicios;
using System;
using System.Collections.Generic;

namespace BLL
{
    /// <summary>
    /// Recuperación de cuentas de Administrador mediante CLAVES DE EMERGENCIA de un solo uso
    /// (tipo códigos de respaldo de Steam / 2FA).
    ///
    /// Se EXTRAJO de BLL.Usuario para respetar SRP (Single Responsibility): el ciclo de vida de
    /// los usuarios y la recuperación de acceso son responsabilidades distintas. Usa inyección de
    /// dependencias por interfaces (DIP) para poder testearse con dobles sin tocar la base de datos.
    /// </summary>
    public class RecuperacionAdmin
    {
        private readonly DAL.Interfaces.IUsuarioDAL           usuarioDAL;
        private readonly DAL.Interfaces.IClaveRecuperacionDAL claveDAL;
        private readonly Servicios.Bitacora bitacora = new Servicios.Bitacora();

        // DI: el constructor por defecto usa los DAL reales; el otro permite inyectar dobles.
        public RecuperacionAdmin() : this(new DAL.Usuario(), new DAL.ClaveRecuperacion()) { }
        public RecuperacionAdmin(DAL.Interfaces.IUsuarioDAL usuarioDAL,
                                 DAL.Interfaces.IClaveRecuperacionDAL claveDAL)
        {
            this.usuarioDAL = usuarioDAL;
            this.claveDAL   = claveDAL;
        }

        // Genera N claves de emergencia: las hashea, persiste y exporta el .txt (copia del admin).
        // Estático para poder seedearlo al instalar (sin sesión). Devuelve la ruta del .txt.
        public static string GenerarClavesEmergencia(int cantidad)
        {
            var dal    = new DAL.ClaveRecuperacion();
            var planas = new List<string>();
            for (int i = 0; i < cantidad; i++)
            {
                string clave = GeneradorCredenciales.GenerarClaveRecuperacion();
                planas.Add(clave);
                dal.Insertar(Encriptador.Hash(clave));
            }
            return GeneradorCredenciales.ExportarClavesRecuperacion(planas);
        }

        // Regenera el set completo (admin): borra las anteriores y crea N nuevas. Devuelve la ruta del .txt.
        public string RegenerarClaves(string modulo, int cantidad = 10)
        {
            ValidarEsAdministrador();
            claveDAL.EliminarTodas();
            string ruta = GenerarClavesEmergencia(cantidad);
            bitacora.Registrar(modulo, $"Regeneración de {cantidad} claves de emergencia", BE.Criticidad.Alta);
            return ruta;
        }

        // Claves de emergencia todavía disponibles.
        public int ContarClavesDisponibles()
        {
            return claveDAL.ContarDisponibles();
        }

        // Valida la CLAVE MAESTRA DE RECUPERACIÓN contra su hash PBKDF2 en App.config
        // (AppSettings["MasterRecoveryKeyHash"]). Es el "break glass" para autorizar operaciones
        // críticas SIN usuario cuando ningún Administrador puede ingresar (BD corrupta).
        // Devuelve false si la clave maestra está desactivada (hash vacío) o no coincide.
        // La lectura de configuración y la verificación criptográfica viven acá, no en la GUI.
        public bool ValidarClaveMaestra(string clave)
        {
            if (string.IsNullOrEmpty(clave)) return false;
            string hashMaestra =
                System.Configuration.ConfigurationManager.AppSettings["MasterRecoveryKeyHash"];
            return !string.IsNullOrEmpty(hashMaestra) &&
                   Encriptador.VerificarContrasena(clave, hashMaestra);
        }

        // Autodesbloqueo de un Administrador bloqueado mediante una clave de emergencia de un solo uso.
        // Valida: usuario existe, es Administrador y está bloqueado; la clave es válida y no usada.
        // Si todo OK: consume la clave (uso único), desbloquea la cuenta y registra en bitácora.
        public bool DesbloquearConClave(string modulo, string username, string clavePlana)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(clavePlana))
                throw new BE.AppException("err.bll.emergencia.campos",
                    "Ingresá tu usuario y una clave de emergencia.");

            string user  = username.Trim();
            string clave = clavePlana.Trim().ToUpperInvariant();   // las claves son en mayúsculas

            var usuario = usuarioDAL.ObtenerPorUsername(user);
            if (usuario == null)
                throw new BE.AppException("err.bll.emergencia.invalida",
                    "Usuario o clave de emergencia inválidos.");

            // Solo Administradores pueden autodesbloquearse con clave de emergencia.
            if (!usuario.EsAdministrador)
                throw new BE.AppException("err.bll.emergencia.solo_admin",
                    "Las claves de emergencia solo desbloquean cuentas de Administrador.");

            if (!usuario.Bloqueado)
                throw new BE.AppException("err.bll.emergencia.no_bloqueada",
                    "La cuenta no está bloqueada: podés iniciar sesión normalmente.");

            var disponibles = claveDAL.ObtenerDisponibles();
            if (disponibles.Count == 0)
                throw new BE.AppException("err.bll.emergencia.sin_claves",
                    "No quedan claves de emergencia. Pedile a otro Administrador que genere un nuevo set.");

            foreach (var kv in disponibles)
            {
                if (!Encriptador.VerificarContrasena(clave, kv.Value)) continue;

                // Consumir la clave (uso único). Si otro la consumió en paralelo, abortar.
                if (!claveDAL.MarcarUsada(kv.Key, user))
                    break;

                usuarioDAL.Desbloquear(usuario.Id);

                // Resetear el contador de intentos EN MEMORIA de esta ejecución de la app.
                // La cuenta ya quedó desbloqueada en BD (Desbloquear pone IntentosFallidos=0),
                // pero ContadorSesion es un singleton por ejecución: si quedó en el límite, el
                // próximo login dispararía "demasiados intentos, la app se cerrará" aunque la
                // cuenta ya esté desbloqueada. Reseteamos para permitir reingresar de inmediato.
                ContadorSesion.GetInstance().Resetear();

                bitacora.RegistrarSinSesion(
                    modulo:     modulo ?? "Login",
                    actividad:  "Desbloqueo con Clave de Emergencia",
                    criticidad: BE.Criticidad.Alta,
                    idUsuario:  usuario.Id,
                    detalle:    $"La cuenta '{user}' se autodesbloqueó con una clave de emergencia a las {DateTime.Now:HH:mm:ss}. Claves restantes: {claveDAL.ContarDisponibles()}.");
                return true;
            }

            throw new BE.AppException("err.bll.emergencia.invalida",
                "Usuario o clave de emergencia inválidos.");
        }

        // Fail-closed: solo un Administrador con sesión puede gestionar las claves.
        private static void ValidarEsAdministrador()
        {
            BLLHelper.ExigirAdministrador("err.bll.usuario.sin_permiso",
                "Solo un Administrador puede gestionar las claves de emergencia.");
        }
    }
}
