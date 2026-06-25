using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Servicios.Exportacion;

namespace Tests
{
    /// <summary>Pruebas del serializador CSV (RFC 4180) — núcleo de la exportación CSV.</summary>
    [TestClass]
    public class SerializadorCsvTests
    {
        private static string[] Lineas(string csv) =>
            csv.Replace("\r\n", "\n").TrimEnd('\n').Split('\n');

        private static DataTable Tabla(string[] columnas, params object[][] filas)
        {
            var dt = new DataTable();
            foreach (var c in columnas) dt.Columns.Add(c);
            foreach (var f in filas) dt.Rows.Add(f);
            return dt;
        }

        [TestMethod]
        public void Csv_EncabezadosYFilas_SeparadosPorPuntoYComa()
        {
            var dt  = Tabla(new[] { "c1", "c2" }, new object[] { "a", "b" }, new object[] { "c", "d" });
            var csv = SerializadorCsv.Generar(new[] { "Col 1", "Col 2" }, dt);

            var l = Lineas(csv);
            Assert.AreEqual(3, l.Length);          // encabezado + 2 filas
            Assert.AreEqual("Col 1;Col 2", l[0]);  // usa los encabezados pasados, no los de la tabla
            Assert.AreEqual("a;b", l[1]);
            Assert.AreEqual("c;d", l[2]);
        }

        [TestMethod]
        public void Csv_CampoConSeparador_SeEntrecomilla()
        {
            var dt  = Tabla(new[] { "c1" }, new object[] { "uno;dos" });
            var csv = SerializadorCsv.Generar(new[] { "H" }, dt);
            Assert.AreEqual("\"uno;dos\"", Lineas(csv)[1]);
        }

        [TestMethod]
        public void Csv_CampoConComillas_DuplicaComillasYEntrecomilla()
        {
            var dt  = Tabla(new[] { "c1" }, new object[] { "he dijo \"hola\"" });
            var csv = SerializadorCsv.Generar(new[] { "H" }, dt);
            Assert.AreEqual("\"he dijo \"\"hola\"\"\"", Lineas(csv)[1]);
        }

        [TestMethod]
        public void Csv_CampoConSaltoDeLinea_SeEntrecomilla()
        {
            var dt  = Tabla(new[] { "c1" }, new object[] { "linea1\nlinea2" });
            var csv = SerializadorCsv.Generar(new[] { "H" }, dt);
            Assert.IsTrue(csv.Contains("\"linea1\nlinea2\""));
        }

        [TestMethod]
        public void Csv_CampoSimple_NoSeEntrecomilla()
        {
            var dt  = Tabla(new[] { "c1" }, new object[] { "simple" });
            var csv = SerializadorCsv.Generar(new[] { "H" }, dt);
            Assert.AreEqual("simple", Lineas(csv)[1]);
        }

        [TestMethod]
        public void Csv_TablaVacia_SoloEncabezado()
        {
            var dt  = Tabla(new[] { "c1", "c2" });
            var csv = SerializadorCsv.Generar(new[] { "A", "B" }, dt);
            Assert.AreEqual(1, Lineas(csv).Length);
            Assert.AreEqual("A;B", Lineas(csv)[0]);
        }

        [TestMethod]
        public void Csv_CeldaNula_QuedaVacia()
        {
            var dt = new DataTable();
            dt.Columns.Add("c1");
            dt.Columns.Add("c2");
            dt.Rows.Add("x", System.DBNull.Value);
            var csv = SerializadorCsv.Generar(new[] { "A", "B" }, dt);
            Assert.AreEqual("x;", Lineas(csv)[1]);
        }
    }
}
