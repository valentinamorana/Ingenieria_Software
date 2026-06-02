using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Tests
{
    /// <summary>T07 — Pruebas del algoritmo de Dígito Verificador (DVH / DVV).</summary>
    [TestClass]
    public class DigitoVerificadorTests
    {
        private readonly Seguridad.DigitoVerificador dv = new Seguridad.DigitoVerificador();

        [TestMethod]
        public void DVH_EsDeterminista()
        {
            int a = dv.CalcularDVH("1", "admin", "hash");
            int b = dv.CalcularDVH("1", "admin", "hash");
            Assert.AreEqual(a, b);
        }

        [TestMethod]
        public void DVH_DetectaCambioDeCampo()
        {
            int a = dv.CalcularDVH("1", "admin", "hash");
            int b = dv.CalcularDVH("1", "admin", "HASH");
            Assert.AreNotEqual(a, b);
        }

        [TestMethod]
        public void DVH_DetectaIntercambioEntreCampos()
        {
            int a = dv.CalcularDVH("1", "AB", "CD");
            int b = dv.CalcularDVH("1", "CD", "AB");
            Assert.AreNotEqual(a, b, "El DVH debe detectar el intercambio de valores entre campos.");
        }

        [TestMethod]
        public void DVV_DetectaReordenamientoDeFilas()
        {
            int a = dv.CalcularDVV(new List<int> { 10, 20, 30 });
            int b = dv.CalcularDVV(new List<int> { 30, 20, 10 });
            Assert.AreNotEqual(a, b);
        }

        [TestMethod]
        public void DVV_DetectaInsercionDeFila()
        {
            int a = dv.CalcularDVV(new List<int> { 10, 20 });
            int b = dv.CalcularDVV(new List<int> { 10, 20, 5 });
            Assert.AreNotEqual(a, b);
        }
    }
}
