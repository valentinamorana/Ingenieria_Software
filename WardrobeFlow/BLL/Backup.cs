namespace BLL
{
    public class Backup
    {
        private readonly DAL.Backup      _dal      = new DAL.Backup();
        private readonly Servicios.Bitacora _bitacora = new Servicios.Bitacora();

        public void RealizarBackup(string modulo, string rutaArchivo)
        {
            _dal.RealizarBackup(rutaArchivo);
            _bitacora.Registrar(modulo,
                $"Backup generado en '{rutaArchivo}'",
                BE.Criticidad.Alta);
        }

        public void RestaurarBackup(string modulo, string rutaArchivo)
        {
            _dal.RestaurarBackup(rutaArchivo);
            _bitacora.Registrar(modulo,
                $"Base de datos restaurada desde '{rutaArchivo}'",
                BE.Criticidad.Alta);
        }
    }
}
