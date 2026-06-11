using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Servicios.Multiidioma;

namespace Tests
{
    /// <summary>
    /// #6 — El corpus de traducciones se externalizó a un recurso embebido (traducciones.tsv)
    /// y Traductor lo carga en runtime. Estas pruebas blindan la mudanza: que el recurso cargue,
    /// que tenga volumen razonable y que el round-trip del escapado (textos multilínea) sea correcto.
    /// </summary>
    [TestClass]
    public class TraductorCorpusTests
    {
        private static IDictionary<string, Traduccion> Es()
        {
            return Traductor.ObtenerTraduccionesHardcode(new Idioma { Id = "ES" });
        }

        [TestMethod]
        public void Corpus_CargaDesdeRecursoEmbebido_ConVolumen()
        {
            var es = Es();
            Assert.IsTrue(es.Count > 800,
                $"El corpus ES cargó {es.Count} claves; se esperaban >800 (¿se embebió traducciones.tsv?).");
        }

        [TestMethod]
        public void Corpus_RoundTripDeEscapado_TextoMultilinea()
        {
            var es = Es();
            Assert.IsTrue(es.ContainsKey("conf.usr.archivar.body"),
                "Falta una clave conocida con texto multilínea.");
            string texto = es["conf.usr.archivar.body"].Texto;
            Assert.IsTrue(texto.Contains("\n"),
                "El salto de línea escapado (\\n) no se restauró al cargar el .tsv.");
            Assert.IsFalse(texto.Contains("\\n"),
                "Quedó el literal '\\n' sin desescapar en el texto cargado.");
        }

        [TestMethod]
        public void Corpus_TodosLosIdiomasCargan()
        {
            foreach (var idioma in Traductor.ObtenerIdiomasHardcode())
            {
                var dict = Traductor.ObtenerTraduccionesHardcode(idioma);
                Assert.IsTrue(dict.Count > 800,
                    $"El idioma {idioma.Id} cargó {dict.Count} claves; se esperaban >800.");
            }
        }
    }
}
