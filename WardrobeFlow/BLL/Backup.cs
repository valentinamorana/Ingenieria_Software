using System;
using System.IO;
using System.Text.RegularExpressions;

namespace BLL
{
    public class Backup
    {
        private readonly DAL.Backup       _dal      = new DAL.Backup();
        private readonly Servicios.Bitacora _bitacora = new Servicios.Bitacora();

        // Construye el nombre del archivo embebiendo el usuario activo.
        // Formato: WardrobeFlow_Backup_yyyyMMddHHmmss_USUARIO.bak
        // El usuario en el nombre permite saber quién hizo cada backup desde la lista.
        public string RealizarBackup(string modulo, string dirDestino)
        {
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

        public void RestaurarBackup(string modulo, string rutaArchivo)
        {
            _dal.RestaurarBackup(rutaArchivo);
            _bitacora.Registrar(modulo,
                $"Base de datos restaurada desde '{Path.GetFileName(rutaArchivo)}'",
                BE.Criticidad.Alta);
        }

        public void EliminarBackup(string modulo, string rutaArchivo)
        {
            if (!File.Exists(rutaArchivo))
                throw new FileNotFoundException("El archivo de backup no existe.", rutaArchivo);

            string filename = Path.GetFileName(rutaArchivo);
            File.Delete(rutaArchivo);
            _bitacora.Registrar(modulo,
                $"Backup eliminado: '{filename}'",
                BE.Criticidad.Alta);
        }
    }
}
