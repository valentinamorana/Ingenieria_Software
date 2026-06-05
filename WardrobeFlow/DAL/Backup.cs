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

        private string ObtenerDirectorioTemp()
        {
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TempBackups");
            try
            {
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var dInfo    = new DirectoryInfo(dir);
                var dSecurity = dInfo.GetAccessControl();
                var everyone  = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                dSecurity.AddAccessRule(new FileSystemAccessRule(
                    everyone,
                    FileSystemRights.FullControl,
                    InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                    PropagationFlags.None,
                    AccessControlType.Allow));
                dInfo.SetAccessControl(dSecurity);
            }
            catch
            {
                dir = Path.Combine(Path.GetTempPath(), "WardrobeFlow_TempBackups");
                try { if (!Directory.Exists(dir)) Directory.CreateDirectory(dir); } catch { }
            }
            return dir;
        }

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

            // RNF-02 — No se restaura un backup corrupto: validar integridad del .bak primero.
            VerificarBackup(rutaOrigen);

            string dirTemp  = ObtenerDirectorioTemp();
            string rutaTemp = Path.Combine(dirTemp, $"WardrobeFlow_Restore_{Guid.NewGuid():N}.bak");

            try
            {
                File.Copy(rutaOrigen, rutaTemp, true);

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
