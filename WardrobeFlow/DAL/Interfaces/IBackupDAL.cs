using System;
using System.Collections.Generic;

namespace DAL.Interfaces
{
    /// <summary>
    /// Contrato del acceso a datos de Backup/Restauración.
    /// Permite que BLL.Backup dependa de una abstracción (DIP) y que las pruebas
    /// inyecten un doble sin tocar SQL Server (ej: el preview de pérdida RF-08).
    /// </summary>
    public interface IBackupDAL
    {
        // Genera el .bak físico de la base en la ruta indicada.
        void RealizarBackup(string rutaDestino);

        // Restaura la base desde el .bak indicado (valida integridad antes — RNF-02).
        void RestaurarBackup(string rutaOrigen);

        // RF-08 — Fecha de finalización del backup (header del .bak), o null si no se puede leer.
        DateTime? ObtenerFechaBackup(string rutaArchivo);

        // RF-08 — Registros de la base ACTUAL posteriores a 'fecha' (los que se perderían
        // al restaurar), desglosados por entidad.
        List<BE.CambioPosterior> ContarCambiosPosterioresA(DateTime fecha);
    }
}
