using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    /// <summary>Pruebas del núcleo de reglas del Panel de Alertas (BLL.PanelAlertas.EvaluarAlertas).</summary>
    [TestClass]
    public class PanelAlertasTests
    {
        private const int X = BLL.PanelAlertas.Desconocido;
        // EvaluarAlertas es estática y pura: no se instancia PanelAlertas (su ctor toca la BD).
        private static System.Collections.Generic.List<BE.Alerta> Evaluar(
            int venc, int porVenc, int backup, int limp, int dv) =>
            BLL.PanelAlertas.EvaluarAlertas(venc, porVenc, backup, limp, dv);

        [TestMethod]
        public void Evaluar_TodoEnOrden_SinAlertas()
        {
            // 0 vencidas, 0 por vencer, backup reciente (2 días), 0 en limpieza, 0 DV rotas
            var alertas = Evaluar(0, 0, 2, 0, 0);
            Assert.AreEqual(0, alertas.Count);
        }

        [TestMethod]
        public void Evaluar_SuscripcionesVencidas_AlertaCritica()
        {
            var a = Evaluar(3, 0, 2, 0, 0).Single();
            Assert.AreEqual(BE.NivelAlerta.Critica, a.Nivel);
            Assert.AreEqual("alert.subs.vencidas", a.ClaveI18n);
            Assert.AreEqual(3, a.Cantidad);
        }

        [TestMethod]
        public void Evaluar_SuscripcionesPorVencer_AlertaAdvertencia()
        {
            var a = Evaluar(0, 5, 2, 0, 0).Single();
            Assert.AreEqual(BE.NivelAlerta.Advertencia, a.Nivel);
            Assert.AreEqual("alert.subs.porvencer", a.ClaveI18n);
            Assert.AreEqual(5, a.Cantidad);
        }

        [TestMethod]
        public void Evaluar_SinBackups_AlertaCriticaNunca()
        {
            // diasSinBackup negativo CONOCIDO (-1) = no hay backups
            var a = Evaluar(0, 0, -1, 0, 0).Single();
            Assert.AreEqual(BE.NivelAlerta.Critica, a.Nivel);
            Assert.AreEqual("alert.backup.nunca", a.ClaveI18n);
        }

        [TestMethod]
        public void Evaluar_BackupViejo_AlertaAdvertencia()
        {
            var a = Evaluar(0, 0, 10, 0, 0).Single();
            Assert.AreEqual(BE.NivelAlerta.Advertencia, a.Nivel);
            Assert.AreEqual("alert.backup.dias", a.ClaveI18n);
            Assert.AreEqual(10, a.Cantidad);
        }

        [TestMethod]
        public void Evaluar_BackupReciente_NoAlerta()
        {
            // 6 días < umbral de 7 → no debe alertar
            Assert.AreEqual(0, Evaluar(0, 0, 6, 0, 0).Count);
        }

        [TestMethod]
        public void Evaluar_MetricaDesconocida_SeIgnora()
        {
            // Todas las fuentes caídas → ninguna alerta (ni siquiera "no hay backups")
            Assert.AreEqual(0, Evaluar(X, X, X, X, X).Count);
        }

        [TestMethod]
        public void Evaluar_PrendasEnLimpieza_AlertaInfo()
        {
            var a = Evaluar(0, 0, 2, 4, 0).Single();
            Assert.AreEqual(BE.NivelAlerta.Info, a.Nivel);
            Assert.AreEqual("alert.prendas.limpieza", a.ClaveI18n);
            Assert.AreEqual(4, a.Cantidad);
        }

        [TestMethod]
        public void Evaluar_IntegridadComprometida_AlertaCritica()
        {
            var a = Evaluar(0, 0, 2, 0, 7).Single();
            Assert.AreEqual(BE.NivelAlerta.Critica, a.Nivel);
            Assert.AreEqual("alert.dv.corruptos", a.ClaveI18n);
            Assert.AreEqual(7, a.Cantidad);
        }

        [TestMethod]
        public void Evaluar_VariasFuentes_AcumulaTodasLasAlertas()
        {
            var alertas = Evaluar(2, 3, -1, 1, 5);
            Assert.AreEqual(5, alertas.Count); // vencidas + porvencer + backup.nunca + limpieza + dv
            Assert.AreEqual(3, alertas.Count(x => x.Nivel == BE.NivelAlerta.Critica)); // vencidas, backup.nunca, dv
        }
    }
}
