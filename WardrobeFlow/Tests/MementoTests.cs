using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    /// <summary>T06 — Pruebas del patrón Memento (Originator / Memento).</summary>
    [TestClass]
    public class MementoTests
    {
        [TestMethod]
        public void Memento_RoundTrip()
        {
            var u = new BE.Usuario { Id = 1, Username = "x", Contraseña = "ORIG", Bloqueado = false, IntentosFallidos = 2 };
            BE.Memento.IMemento m = u.CrearMemento("tester", "snap");

            u.Contraseña = "NUEVA"; u.Bloqueado = true; u.IntentosFallidos = 9;
            u.RestaurarDesde(m);

            Assert.AreEqual("ORIG", u.Contraseña);
            Assert.IsFalse(u.Bloqueado);
            Assert.AreEqual(2, u.IntentosFallidos);
        }

        [TestMethod]
        public void Memento_ExponeMetadatos()
        {
            BE.Memento.IMemento m = new BE.Usuario { Id = 1 }.CrearMemento("ana", "reset");
            Assert.AreEqual("ana", m.Actor);
            Assert.AreEqual("reset", m.Detalle);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Memento_RechazaIncompatible()
        {
            new BE.Usuario().RestaurarDesde(new MementoFalso());
        }

        // Memento de otro tipo: el Originator debe rechazarlo.
        private class MementoFalso : BE.Memento.IMemento
        {
            public DateTime Fecha   { get { return DateTime.Now; } }
            public string   Actor   { get { return ""; } }
            public string   Detalle { get { return ""; } }
        }
    }
}
