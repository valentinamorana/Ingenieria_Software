using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace BLL
{
    public class Backup
    {
        private readonly DAL.Interfaces.IBackupDAL _dal;
        // Bitácora perezosa: solo se instancia cuando una operación de escritura la usa, para que
        // construir BLL.Backup (y testear el preview RF-08) no toque la BD ni el App.config.
        private Servicios.Bitacora _bitacoraLazy;
        private Servicios.Bitacora _bitacora => _bitacoraLazy ?? (_bitacoraLazy = new Servicios.Bitacora());

        // DI: el constructor por defecto usa el DAL real; el otro permite inyectar un doble
        // de prueba (tests del preview de pérdida RF-08 sin tocar SQL Server).
        public Backup() : this(new DAL.Backup()) { }
        public Backup(DAL.Interfaces.IBackupDAL dal)
        {
            _dal = dal;
        }

        // Extensión de los backups CIFRADOS (con contraseña). Los .bak planos legacy se siguen
        // pudiendo restaurar para no romper copias anteriores.
        public const string ExtensionCifrada = ".wfbak";

        private static void ValidarAdministrador()
        {
            BLLHelper.ExigirAdministrador("err.bll.backup.sin_permiso",
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
        public string RealizarBackup(string modulo, string dirDestino, string claveCifrado)
        {
            ValidarAdministrador();
            VerificarIntegridadAntesDeRespaldar();   // RF-04

            string usuario = Seguridad.SessionManager.IsLoggedIn
                ? Seguridad.SessionManager.GetInstance().Usuario.Username
                : "sistema";

            string usuarioSanitizado = Regex.Replace(usuario, @"[^\w]", "_");
            string filename = $"WardrobeFlow_Backup_{DateTime.Now:yyyyMMddHHmmss}_{usuarioSanitizado}{ExtensionCifrada}";

            GenerarBackupCifrado(Path.Combine(dirDestino, filename), claveCifrado);
            _bitacora.Registrar(modulo,
                $"Backup cifrado generado: '{filename}'",
                BE.Criticidad.Alta);

            return filename;
        }

        // Genera el .bak con SQL en un temporal plano y lo CIFRA hacia la ruta final (.wfbak).
        // El temporal plano se borra siempre (finally) para no dejar datos sin cifrar en disco.
        private void GenerarBackupCifrado(string rutaFinal, string claveCifrado)
        {
            if (string.IsNullOrEmpty(claveCifrado))
                throw new BE.AppException("err.bll.backup.clave_requerida",
                    "Debés ingresar una contraseña para cifrar el backup.");

            string tempPlano = Path.Combine(Path.GetTempPath(), $"WF_bak_{Guid.NewGuid():N}.bak");
            try
            {
                _dal.RealizarBackup(tempPlano);
                Seguridad.CifradorArchivos.Cifrar(tempPlano, rutaFinal, claveCifrado);
            }
            finally
            {
                if (File.Exists(tempPlano)) try { File.Delete(tempPlano); } catch { }
            }
        }

        // RF-06 — Genera un backup de INSTALACIÓN LIMPIA (estado inicial del sistema).
        // Se nombra con el marcador "Inicial" para distinguirlo en la lista y poder volver
        // siempre al punto de partida conocido. También valida integridad antes de generarlo.
        // Formato: WardrobeFlow_Inicial_yyyyMMddHHmmss_USUARIO.bak
        public string RealizarBackupInicial(string modulo, string dirDestino, string claveCifrado)
        {
            ValidarAdministrador();
            VerificarIntegridadAntesDeRespaldar();   // RF-04

            string usuario = Seguridad.SessionManager.IsLoggedIn
                ? Seguridad.SessionManager.GetInstance().Usuario.Username
                : "sistema";

            string usuarioSanitizado = Regex.Replace(usuario, @"[^\w]", "_");
            string filename = $"WardrobeFlow_Inicial_{DateTime.Now:yyyyMMddHHmmss}_{usuarioSanitizado}{ExtensionCifrada}";

            GenerarBackupCifrado(Path.Combine(dirDestino, filename), claveCifrado);
            _bitacora.Registrar(modulo,
                $"Backup de instalación limpia (cifrado) generado: '{filename}'",
                BE.Criticidad.Alta);

            return filename;
        }

        // RF-08 — Fecha en que se completó un backup (para informar el alcance de la pérdida
        // antes de restaurar). Devuelve null si no puede leerse.
        public DateTime? ObtenerFechaBackup(string rutaArchivo)
        {
            return _dal.ObtenerFechaBackup(rutaArchivo);
        }

        // RF-08 — Preview de pérdida a nivel REGISTRO: dada la fecha del backup, devuelve cuántos
        // registros de la base actual son posteriores a esa fecha (los que se perderían al
        // restaurar), desglosados por entidad. Solo incluye entidades con al menos 1 registro.
        // Es de SOLO LECTURA (no abre sesión ni modifica nada): se puede usar también en el
        // flujo de recuperación de arranque, donde todavía no hay sesión iniciada.
        // Devuelve lista vacía si la fecha es null o no hay cambios posteriores.
        public List<BE.CambioPosterior> ObtenerCambiosDesde(DateTime? fechaBackup)
        {
            if (!fechaBackup.HasValue) return new List<BE.CambioPosterior>();
            return _dal.ContarCambiosPosterioresA(fechaBackup.Value);
        }

        // Indica si un archivo es un backup CIFRADO (por su extensión .wfbak).
        public static bool EsCifrado(string rutaArchivo)
        {
            return !string.IsNullOrEmpty(rutaArchivo) &&
                   rutaArchivo.EndsWith(ExtensionCifrada, StringComparison.OrdinalIgnoreCase);
        }

        // Restaura un backup. Si es .wfbak (cifrado) lo descifra con la clave a un temporal y
        // restaura desde ahí; si es .bak plano (legacy) restaura directo. claveCifrado solo se
        // usa para los cifrados; una clave incorrecta da un mensaje claro.
        public void RestaurarBackup(string modulo, string rutaArchivo, string claveCifrado = null)
        {
            ValidarAdministrador();

            if (EsCifrado(rutaArchivo))
            {
                // Descifrar a una carpeta que la cuenta de SQL Server pueda LEER (no al %TEMP% del
                // usuario, que da Access denied al restaurar).
                string tempPlano = Path.Combine(DAL.Backup.DirectorioTempSeguro(), $"WF_restore_{Guid.NewGuid():N}.bak");
                try
                {
                    try
                    {
                        Seguridad.CifradorArchivos.Descifrar(rutaArchivo, tempPlano, claveCifrado);
                    }
                    catch (CryptographicException)
                    {
                        throw new BE.AppException("err.bll.backup.clave_invalida",
                            "La contraseña del backup es incorrecta o el archivo está dañado.");
                    }
                    _dal.RestaurarBackup(tempPlano);
                }
                finally
                {
                    if (File.Exists(tempPlano)) try { File.Delete(tempPlano); } catch { }
                }
            }
            else
            {
                _dal.RestaurarBackup(rutaArchivo);   // .bak plano (legacy)
            }

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
