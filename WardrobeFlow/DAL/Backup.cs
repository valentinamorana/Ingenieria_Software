using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace DAL
{
    public class Backup
    {
        private readonly string _cadenaConexionMaster;

        public Backup()
        {
            var entrada = ConfigurationManager.ConnectionStrings["WardrobeFlowDB"];
            if (entrada != null)
            {
                var builder = new SqlConnectionStringBuilder(entrada.ConnectionString);
                builder.InitialCatalog = "master";
                _cadenaConexionMaster = builder.ConnectionString;
            }
        }

        // Directorio temporal que la CUENTA DE SERVICIO de SQL Server pueda LEER (para BACKUP/RESTORE
        // y RESTORE VERIFYONLY). C:\Users\Public es legible por todos los servicios; el %TEMP% del
        // usuario (AppData\Local\Temp) NO lo es → causaba "Operating system error 5 (Access is denied)"
        // al restaurar. Por eso se usa Public como ubicación principal.
        public static string DirectorioTempSeguro()
        {
            string publico = Environment.GetEnvironmentVariable("PUBLIC");
            if (string.IsNullOrEmpty(publico)) publico = @"C:\Users\Public";
            string dir = Path.Combine(publico, "WardrobeFlow_Temp");
            try { if (!Directory.Exists(dir)) Directory.CreateDirectory(dir); return dir; }
            catch { }

            // Fallback: carpeta junto al ejecutable con permiso de lectura para Todos.
            string alt = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TempBackups");
            try
            {
                if (!Directory.Exists(alt)) Directory.CreateDirectory(alt);
                var dInfo = new DirectoryInfo(alt);
                var sec   = dInfo.GetAccessControl();
                sec.AddAccessRule(new FileSystemAccessRule(
                    new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                    FileSystemRights.FullControl,
                    InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                    PropagationFlags.None, AccessControlType.Allow));
                dInfo.SetAccessControl(sec);
            }
            catch { }
            return alt;
        }

        private string ObtenerDirectorioTemp() => DirectorioTempSeguro();

        public void RealizarBackup(string rutaDestino)
        {
            if (string.IsNullOrEmpty(_cadenaConexionMaster))
                throw new InvalidOperationException("Cadena de conexión no configurada.");

            string dirTemp  = ObtenerDirectorioTemp();
            string rutaTemp = Path.Combine(dirTemp, $"WardrobeFlow_Temp_{Guid.NewGuid():N}.bak");

            try
            {
                using (var conn = new SqlConnection(_cadenaConexionMaster))
                using (var cmd  = new SqlCommand(
                    "BACKUP DATABASE WardrobeFlowDB TO DISK = @Ruta WITH FORMAT, INIT, NAME = 'WardrobeFlowBackup';",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@Ruta", rutaTemp);
                    cmd.CommandTimeout = 120;
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                if (File.Exists(rutaDestino)) File.Delete(rutaDestino);
                File.Copy(rutaTemp, rutaDestino);
            }
            finally
            {
                if (File.Exists(rutaTemp))
                    try { File.Delete(rutaTemp); } catch { }
            }
        }

        // RNF-02 — Verifica que un archivo .bak sea un backup VÁLIDO y no esté corrupto,
        // usando RESTORE VERIFYONLY (chequea integridad del conjunto de backup sin restaurar).
        // Lanza excepción si el backup es ilegible/corrupto. Se llama ANTES de cualquier restauración.
        public void VerificarBackup(string rutaArchivo)
        {
            if (string.IsNullOrEmpty(_cadenaConexionMaster))
                throw new InvalidOperationException("Cadena de conexión no configurada.");
            if (!File.Exists(rutaArchivo))
                throw new FileNotFoundException("El archivo de backup no existe.", rutaArchivo);

            try
            {
                using (var conn = new SqlConnection(_cadenaConexionMaster))
                using (var cmd  = new SqlCommand("RESTORE VERIFYONLY FROM DISK = @Ruta;", conn))
                {
                    cmd.Parameters.AddWithValue("@Ruta", rutaArchivo);
                    cmd.CommandTimeout = 120;
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                // VERIFYONLY falla cuando el .bak está corrupto, truncado o no es un backup válido.
                throw new BE.AppException("err.dal.backup_corrupto",
                    "El archivo de backup es inválido o está corrupto y no puede restaurarse.\n\nDetalle: " + ex.Message);
            }
        }

        // RF-08 — Devuelve la fecha en que se completó el backup (BackupFinishDate del header),
        // para informar al administrador el alcance de la pérdida antes de restaurar.
        // Retorna null si no puede leerse el header.
        public DateTime? ObtenerFechaBackup(string rutaArchivo)
        {
            if (string.IsNullOrEmpty(_cadenaConexionMaster) || !File.Exists(rutaArchivo))
                return null;
            try
            {
                using (var conn = new SqlConnection(_cadenaConexionMaster))
                using (var cmd  = new SqlCommand("RESTORE HEADERONLY FROM DISK = @Ruta;", conn))
                {
                    cmd.Parameters.AddWithValue("@Ruta", rutaArchivo);
                    cmd.CommandTimeout = 120;
                    conn.Open();
                    using (var rd = cmd.ExecuteReader())
                    {
                        if (rd.Read())
                        {
                            int col = rd.GetOrdinal("BackupFinishDate");
                            if (!rd.IsDBNull(col)) return rd.GetDateTime(col);
                        }
                    }
                }
            }
            catch { /* header ilegible → null (la verificación de corrupción se hace en VerificarBackup) */ }
            return null;
        }

        public void RestaurarBackup(string rutaOrigen)
        {
            if (string.IsNullOrEmpty(_cadenaConexionMaster))
                throw new InvalidOperationException("Cadena de conexión no configurada.");

            string dirTemp  = ObtenerDirectorioTemp();
            string rutaTemp = Path.Combine(dirTemp, $"WardrobeFlow_Restore_{Guid.NewGuid():N}.bak");

            try
            {
                // Copiar PRIMERO a una ubicación legible por la cuenta de SQL Server, y recién ahí
                // verificar/restaurar. (La app puede leer el origen aunque SQL no — ej. %TEMP% del
                // usuario o Documentos. Antes se hacía VERIFYONLY sobre el origen → Access denied.)
                File.Copy(rutaOrigen, rutaTemp, true);

                // RNF-02 — No se restaura un backup corrupto: validar integridad de la copia accesible.
                VerificarBackup(rutaTemp);

                const string sql =
                    "ALTER DATABASE WardrobeFlowDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE; " +
                    "RESTORE DATABASE WardrobeFlowDB FROM DISK = @Ruta WITH REPLACE; " +
                    "ALTER DATABASE WardrobeFlowDB SET MULTI_USER;";

                using (var conn = new SqlConnection(_cadenaConexionMaster))
                using (var cmd  = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Ruta", rutaTemp);
                    cmd.CommandTimeout = 240;
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            finally
            {
                if (File.Exists(rutaTemp))
                    try { File.Delete(rutaTemp); } catch { }
            }
        }
    }
}
