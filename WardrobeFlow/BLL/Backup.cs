using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace BLL
{
    public class Backup
    {
        private readonly DAL.Backup       _dal      = new DAL.Backup();
        private readonly Servicios.Bitacora _bitacora = new Servicios.Bitacora();

        private static void ValidarAdministrador()
        {
            if (!Seguridad.SessionManager.IsLoggedIn)
                throw new BE.AppException("err.bll.sesion_expirada",
                    "La sesión expiró. Volvé a iniciar sesión.");
            string perfil = Seguridad.SessionManager.GetInstance().Usuario.Perfil ?? "";
            if (!perfil.Equals(BE.Roles.Administrador, StringComparison.OrdinalIgnoreCase))
                throw new BE.AppException("err.bll.backup.sin_permiso",
                    "Solo un Administrador puede realizar operaciones de backup y restauración.");
        }

        // RF-04 — Antes de generar un backup se verifica la integridad (DVH/DVV) de la
        // información a respaldar. Si la base está comprometida, se cancela el backup para
        // no perpetuar datos corruptos en la copia. Lanza AppException con el detalle.
        private static void VerificarIntegridadAntesDeRespaldar()
        {
            if (!Configuracion.VerificarIntegridadDV(out ResultadoIntegridad r))
            {
                string detalle = r?.MensajeES ?? "Se detectaron inconsistencias en los dígitos verificadores.";
                throw new BE.AppException("err.bll.backup.integridad",
                    "No se puede generar el backup: la base tiene problemas de integridad.\n" +
                    "Reparalos desde Administrar → Diagnóstico de Integridad antes de respaldar.\n\n" + detalle);
            }
        }

        // Construye el nombre del archivo embebiendo el usuario activo.
        // Formato: WardrobeFlow_Backup_yyyyMMddHHmmss_USUARIO.bak
        // El usuario en el nombre permite saber quién hizo cada backup desde la lista.
        public string RealizarBackup(string modulo, string dirDestino)
        {
            ValidarAdministrador();
            VerificarIntegridadAntesDeRespaldar();   // RF-04

            string usuario = Seguridad.SessionManager.IsLoggedIn
                ? Seguridad.SessionManager.GetInstance().Usuario.Username
                : "sistema";

            string usuarioSanitizado = Regex.Replace(usuario, @"[^\w]", "_");
            string filename   = $"WardrobeFlow_Backup_{DateTime.Now:yyyyMMddHHmmss}_{usuarioSanitizado}.bak";
            string rutaArchivo = Path.Combine(dirDestino, filename);

            _dal.RealizarBackup(rutaArchivo);
            _bitacora.Registrar(modulo,
                $"Backup generado: '{filename}'",
                BE.Criticidad.Alta);

            return filename;
        }

        // RF-06 — Genera un backup de INSTALACIÓN LIMPIA (estado inicial del sistema).
        // Se nombra con el marcador "Inicial" para distinguirlo en la lista y poder volver
        // siempre al punto de partida conocido. También valida integridad antes de generarlo.
        // Formato: WardrobeFlow_Inicial_yyyyMMddHHmmss_USUARIO.bak
        public string RealizarBackupInicial(string modulo, string dirDestino)
        {
            ValidarAdministrador();
            VerificarIntegridadAntesDeRespaldar();   // RF-04

            string usuario = Seguridad.SessionManager.IsLoggedIn
                ? Seguridad.SessionManager.GetInstance().Usuario.Username
                : "sistema";

            string usuarioSanitizado = Regex.Replace(usuario, @"[^\w]", "_");
            string filename   = $"WardrobeFlow_Inicial_{DateTime.Now:yyyyMMddHHmmss}_{usuarioSanitizado}.bak";
            string rutaArchivo = Path.Combine(dirDestino, filename);

            _dal.RealizarBackup(rutaArchivo);
            _bitacora.Registrar(modulo,
                $"Backup de instalación limpia generado: '{filename}'",
                BE.Criticidad.Alta);

            return filename;
        }

        // RF-08 — Fecha en que se completó un backup (para informar el alcance de la pérdida
        // antes de restaurar). Devuelve null si no puede leerse.
        public DateTime? ObtenerFechaBackup(string rutaArchivo)
        {
            return _dal.ObtenerFechaBackup(rutaArchivo);
        }

        public void RestaurarBackup(string modulo, string rutaArchivo)
        {
            ValidarAdministrador();
            _dal.RestaurarBackup(rutaArchivo);
            _bitacora.Registrar(modulo,
                $"Base de datos restaurada desde '{Path.GetFileName(rutaArchivo)}'",
                BE.Criticidad.Alta);
        }

        public void EliminarBackup(string modulo, string rutaArchivo)
        {
            ValidarAdministrador();
            if (!File.Exists(rutaArchivo))
                throw new FileNotFoundException("El archivo de backup no existe.", rutaArchivo);

            string filename = Path.GetFileName(rutaArchivo);
            File.Delete(rutaArchivo);
            _bitacora.Registrar(modulo,
                $"Backup eliminado: '{filename}'",
                BE.Criticidad.Alta);
        }

        // Extrae el autor del nombre de un archivo de backup.
        // Formato: WardrobeFlow_Backup_yyyyMMddHHmmss_USUARIO.bak
        // Devuelve "—" si el nombre no sigue la convención.
        public string ExtraerAutorDeNombre(string nombreArchivo)
        {
            const string prefix = "WardrobeFlow_Backup_";
            if (!nombreArchivo.StartsWith(prefix) || nombreArchivo.Length <= prefix.Length + 15)
                return "—";

            string rest = nombreArchivo.Substring(prefix.Length);
            bool isNewFormat = rest.Length >= 15
                && rest.Substring(0, 14).All(char.IsDigit)
                && rest[14] == '_';

            if (!isNewFormat) return "—";

            return Path.GetFileNameWithoutExtension(rest.Substring(15));
        }
    }
}
