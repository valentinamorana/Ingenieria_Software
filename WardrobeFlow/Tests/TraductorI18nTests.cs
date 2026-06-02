using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Servicios.Multiidioma;

namespace Tests
{
    /// <summary>
    /// T05 — Invariantes de internacionalización (i18n).
    ///
    /// Garantiza que TODOS los idiomas soportados expongan exactamente el mismo
    /// conjunto de claves (sin faltantes ni sobrantes) y que ninguna traducción
    /// quede vacía. Bloquea la regresión típica: agregar una clave a un idioma
    /// y olvidarla en otro (o dejar PT incompleto).
    /// Usa los diccionarios HARDCODEADOS, así no requiere base de datos.
    /// </summary>
    [TestClass]
    public class TraductorI18nTests
    {
        [TestMethod]
        public void HayAlMenosCuatroIdiomas()
        {
            var idiomas = Traductor.ObtenerIdiomasHardcode();
            Assert.IsTrue(idiomas.Count >= 4, "Se esperaban al menos 4 idiomas (ES/EN/RU/PT).");
        }

        [TestMethod]
        public void TodosLosIdiomasTienenLasMismasClaves()
        {
            var idiomas   = Traductor.ObtenerIdiomasHardcode();
            var referencia = Traductor.ObtenerIdiomaDefault();
            var clavesRef = new HashSet<string>(Traductor.ObtenerTraduccionesHardcode(referencia).Keys);

            var errores = new StringBuilder();
            foreach (var idm in idiomas)
            {
                var claves = new HashSet<string>(Traductor.ObtenerTraduccionesHardcode(idm).Keys);

                var faltantes = clavesRef.Except(claves).OrderBy(k => k).ToList();
                var sobrantes = claves.Except(clavesRef).OrderBy(k => k).ToList();

                if (faltantes.Count > 0)
                    errores.AppendLine(idm.Id + " FALTAN (" + faltantes.Count + "): " + string.Join(", ", faltantes));
                if (sobrantes.Count > 0)
                    errores.AppendLine(idm.Id + " SOBRAN (" + sobrantes.Count + "): " + string.Join(", ", sobrantes));
            }

            Assert.AreEqual(0, errores.Length, "Desbalance de claves entre idiomas:\n" + errores);
        }

        [TestMethod]
        public void NingunaTraduccionEstaVacia()
        {
            var errores = new StringBuilder();
            foreach (var idm in Traductor.ObtenerIdiomasHardcode())
                foreach (var kv in Traductor.ObtenerTraduccionesHardcode(idm))
                    if (string.IsNullOrWhiteSpace(kv.Value != null ? kv.Value.Texto : null))
                        errores.AppendLine(idm.Id + " → clave sin texto: " + kv.Key);

            Assert.AreEqual(0, errores.Length, "Hay traducciones vacías:\n" + errores);
        }
    }
}
