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

        [TestMethod]
        public void DVH_IncluyeRol_DetectaManipulacionDeRol()
        {
            // A4: el Rol participa del DVH (formato v2) para que una manipulación del rol
            // —del que dependen los permisos efectivos— sea detectada por la verificación
            // de integridad. Antes solo se protegía el Perfil.
            var filaBase = new BE.FilaUsuarioDV
            {
                Id = 1, Username = "u", Clave = "hash",
                Rol = "Vendedor", Perfil = "Vendedor", Estado = "1", IntentosFallidos = "0"
            };
            var filaRolAlterado = new BE.FilaUsuarioDV
            {
                Id = 1, Username = "u", Clave = "hash",
                Rol = "Administrador", Perfil = "Vendedor", Estado = "1", IntentosFallidos = "0"
            };

            int dvhBase     = dv.CalcularDVH(filaBase.CamposParaDVH());
            int dvhAlterado = dv.CalcularDVH(filaRolAlterado.CamposParaDVH());

            Assert.AreNotEqual(dvhBase, dvhAlterado,
                "Cambiar solo el Rol debe cambiar el DVH; si no, una escalada por BD pasaría inadvertida.");
        }

        [TestMethod]
        public void CamposParaDVH_IncluyeRolEnElOrdenEsperado()
        {
            var fila = new BE.FilaUsuarioDV
            {
                Id = 7, Username = "u", Clave = "h",
                Rol = "Supervisor", Perfil = "Supervisor", Estado = "1", IntentosFallidos = "2"
            };
            CollectionAssert.AreEqual(
                new[] { "7", "u", "h", "Supervisor", "Supervisor", "1", "2" },
                fila.CamposParaDVH());
        }
    }
}
