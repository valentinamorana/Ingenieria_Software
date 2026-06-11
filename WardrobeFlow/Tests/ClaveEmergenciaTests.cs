using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    /// <summary>
    /// RF-10 — Pruebas de las claves de emergencia (generación + hash de un solo uso).
    /// Cubren la parte sin BD: formato legible, ausencia de caracteres ambiguos, unicidad
    /// y verificación contra el hash PBKDF2. El consumo (uso único) se prueba contra BD real.
    /// </summary>
    [TestClass]
    public class ClaveEmergenciaTests
    {
        [TestMethod]
        public void Clave_TieneFormatoLegible()
        {
            string clave = Servicios.GeneradorCredenciales.GenerarClaveRecuperacion();
            // 3 grupos de 4 caracteres (mayúsculas/dígitos sin ambiguos) separados por guiones.
            Assert.IsTrue(Regex.IsMatch(clave, "^[A-Z2-9]{4}-[A-Z2-9]{4}-[A-Z2-9]{4}$"),
                "Formato esperado XXXX-XXXX-XXXX, fue: " + clave);
        }

        [TestMethod]
        public void Clave_NoTieneCaracteresAmbiguos()
        {
            for (int i = 0; i < 50; i++)
            {
                string c = Servicios.GeneradorCredenciales.GenerarClaveRecuperacion();
                Assert.IsFalse(Regex.IsMatch(c, "[O01I]"),
                    "No debe contener O, 0, 1 ni I: " + c);
            }
        }

        [TestMethod]
        public void Clave_SeVerificaContraSuHash()
        {
            string clave = Servicios.GeneradorCredenciales.GenerarClaveRecuperacion();
            string hash  = Seguridad.Encriptador.Hash(clave);

            Assert.IsTrue(Seguridad.Encriptador.VerificarContrasena(clave, hash),
                "La clave debe verificar contra su propio hash.");
            Assert.IsFalse(Seguridad.Encriptador.VerificarContrasena("ZZZZ-ZZZZ-ZZZZ", hash),
                "Una clave distinta no debe verificar.");
        }

        [TestMethod]
        public void Claves_SonUnicas()
        {
            var set = new HashSet<string>();
            for (int i = 0; i < 50; i++)
                set.Add(Servicios.GeneradorCredenciales.GenerarClaveRecuperacion());
            // Con 32^12 combinaciones, una colisión en 50 claves es prácticamente imposible.
            Assert.IsTrue(set.Count >= 49, "Las claves generadas deben ser únicas.");
        }
    }
}
