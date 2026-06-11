using System;
using System.Collections.Generic;
using DAL.Interfaces;

namespace Tests.Fakes
{
    /// <summary>
    /// Doble de prueba de IBackupDAL (sin SQL Server). Permite configurar la fecha del
    /// backup y la lista de cambios posteriores, y registra qué fecha recibió el conteo
    /// y cuántas veces se invocó — para verificar la delegación de BLL.Backup (RF-08).
    /// </summary>
    public class FakeBackupDAL : IBackupDAL
    {
        // Valores que devolverá el doble.
        public DateTime? FechaBackupConfig { get; set; }
        public List<BE.CambioPosterior> CambiosConfig { get; set; } = new List<BE.CambioPosterior>();

        // Espías para las aserciones.
        public DateTime? FechaRecibidaEnConteo { get; private set; }
        public int VecesContado { get; private set; }

        public void RealizarBackup(string rutaDestino) { }
        public void RestaurarBackup(string rutaOrigen) { }

        public DateTime? ObtenerFechaBackup(string rutaArchivo) => FechaBackupConfig;

        public List<BE.CambioPosterior> ContarCambiosPosterioresA(DateTime fecha)
        {
            VecesContado++;
            FechaRecibidaEnConteo = fecha;
            return CambiosConfig;
        }
    }
}
