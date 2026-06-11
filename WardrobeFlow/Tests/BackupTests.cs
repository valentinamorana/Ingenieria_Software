using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.Fakes;

namespace Tests
{
    /// <summary>
    /// RF-08 — Preview de pérdida antes de restaurar un backup.
    /// Verifica que BLL.Backup.ObtenerCambiosDesde delegue correctamente en el DAL
    /// (vía IBackupDAL inyectado) y aplique el corto-circuito cuando no hay fecha.
    /// Sin SQL Server: el acceso a datos se reemplaza por FakeBackupDAL.
    /// </summary>
    [TestClass]
    public class BackupTests
    {
        [TestMethod]
        public void ObtenerCambiosDesde_FechaNull_DevuelveVacio_SinConsultarDAL()
        {
            var fake = new FakeBackupDAL
            {
                // Aunque el DAL "tenga" cambios, no debe consultarse si no hay fecha.
                CambiosConfig = new List<BE.CambioPosterior>
                {
                    new BE.CambioPosterior { Entidad = "Pedidos", Cantidad = 5 }
                }
            };
            var bll = new BLL.Backup(fake);

            var resultado = bll.ObtenerCambiosDesde(null);

            Assert.IsNotNull(resultado);
            Assert.AreEqual(0, resultado.Count);
            Assert.AreEqual(0, fake.VecesContado, "No debe consultar el DAL cuando la fecha es null.");
        }

        [TestMethod]
        public void ObtenerCambiosDesde_ConFecha_DelegaYPropagaLaFechaExacta()
        {
            var fecha = new DateTime(2026, 6, 10, 13, 46, 0);
            var fake = new FakeBackupDAL
            {
                CambiosConfig = new List<BE.CambioPosterior>
                {
                    new BE.CambioPosterior { Entidad = "Pedidos",         Cantidad = 3 },
                    new BE.CambioPosterior { Entidad = "Clientes nuevos", Cantidad = 1 },
                }
            };
            var bll = new BLL.Backup(fake);

            var resultado = bll.ObtenerCambiosDesde(fecha);

            Assert.AreEqual(1, fake.VecesContado, "Debe consultar el DAL exactamente una vez.");
            Assert.AreEqual(fecha, fake.FechaRecibidaEnConteo, "Debe pasar la fecha del backup sin alterarla.");
            Assert.AreEqual(2, resultado.Count);
            Assert.AreEqual("Pedidos", resultado[0].Entidad);
            Assert.AreEqual(3,         resultado[0].Cantidad);
            Assert.AreEqual("Clientes nuevos", resultado[1].Entidad);
            Assert.AreEqual(1,                 resultado[1].Cantidad);
        }

        [TestMethod]
        public void ObtenerCambiosDesde_SinCambiosPosteriores_DevuelveVacioPeroConsultaElDAL()
        {
            var fake = new FakeBackupDAL { CambiosConfig = new List<BE.CambioPosterior>() };
            var bll  = new BLL.Backup(fake);

            var resultado = bll.ObtenerCambiosDesde(new DateTime(2026, 1, 1));

            Assert.AreEqual(0, resultado.Count);
            Assert.AreEqual(1, fake.VecesContado, "Con fecha presente, debe consultar el DAL aunque devuelva vacío.");
        }

        [TestMethod]
        public void ObtenerFechaBackup_DelegaEnElDAL()
        {
            var esperada = new DateTime(2026, 5, 1, 9, 0, 0);
            var fake = new FakeBackupDAL { FechaBackupConfig = esperada };
            var bll  = new BLL.Backup(fake);

            Assert.AreEqual(esperada, bll.ObtenerFechaBackup("cualquiera.wfbak"));
        }
    }
}
