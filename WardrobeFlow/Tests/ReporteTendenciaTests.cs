using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    /// <summary>Pruebas del núcleo de agregación de tendencia (BLL.ReporteJornada.CalcularTendencia).</summary>
    [TestClass]
    public class ReporteTendenciaTests
    {
        private static KeyValuePair<DateTime, int> Dia(int dia, int eventos) =>
            new KeyValuePair<DateTime, int>(new DateTime(2026, 6, dia), eventos);

        [TestMethod]
        public void Calcular_ListaVacia_DevuelveCeros()
        {
            var est = BLL.ReporteJornada.CalcularTendencia(new List<KeyValuePair<DateTime, int>>());
            Assert.AreEqual(0, est.DiasAnalizados);
            Assert.AreEqual(0, est.TotalEventos);
            Assert.AreEqual(0d, est.PromedioDiario);
        }

        [TestMethod]
        public void Calcular_Null_DevuelveCeros()
        {
            var est = BLL.ReporteJornada.CalcularTendencia(null);
            Assert.AreEqual(0, est.DiasAnalizados);
            Assert.AreEqual(0, est.TotalEventos);
        }

        [TestMethod]
        public void Calcular_VariosDias_TotalPromedioYExtremos()
        {
            var serie = new List<KeyValuePair<DateTime, int>>
            {
                Dia(1, 2), Dia(2, 10), Dia(3, 0), Dia(4, 8)
            };

            var est = BLL.ReporteJornada.CalcularTendencia(serie);

            Assert.AreEqual(4,  est.DiasAnalizados);
            Assert.AreEqual(20, est.TotalEventos);
            Assert.AreEqual(5d, est.PromedioDiario);            // 20 / 4
            Assert.AreEqual(new DateTime(2026, 6, 2), est.DiaPico);
            Assert.AreEqual(10, est.MaxEventos);
            Assert.AreEqual(new DateTime(2026, 6, 3), est.DiaValle);
            Assert.AreEqual(0,  est.MinEventos);
        }

        [TestMethod]
        public void Calcular_UnSoloDia_PicoYValleSonElMismo()
        {
            var est = BLL.ReporteJornada.CalcularTendencia(new List<KeyValuePair<DateTime, int>> { Dia(5, 7) });
            Assert.AreEqual(1, est.DiasAnalizados);
            Assert.AreEqual(7, est.TotalEventos);
            Assert.AreEqual(7d, est.PromedioDiario);
            Assert.AreEqual(est.DiaPico, est.DiaValle);
            Assert.AreEqual(7, est.MaxEventos);
            Assert.AreEqual(7, est.MinEventos);
        }

        [TestMethod]
        public void Calcular_TodosLosDiasEnCero_SinDivisionRara()
        {
            var serie = new List<KeyValuePair<DateTime, int>> { Dia(1, 0), Dia(2, 0), Dia(3, 0) };
            var est   = BLL.ReporteJornada.CalcularTendencia(serie);
            Assert.AreEqual(0,  est.TotalEventos);
            Assert.AreEqual(0d, est.PromedioDiario);
            Assert.AreEqual(0,  est.MaxEventos);
            Assert.AreEqual(0,  est.MinEventos);
        }
    }
}
