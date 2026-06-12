using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    /// <summary>T06 — Pruebas del patrón Memento (Originator / Memento).</summary>
    [TestClass]
    public class MementoTests
    {
        // El Memento versiona los datos administrativos NO sensibles (username/nombre/apellido/
        // fecha nac./email) y el rollback los restaura.
        [TestMethod]
        public void Memento_RoundTrip_RestauraDatosAdministrativos()
        {
            var u = new BE.Usuario
            {
                Id = 1, Username = "jperez", Nombre = "Juan", Apellido = "Pérez",
                Email = "juan@x.com", FechaNacimiento = new DateTime(1990, 5, 1)
            };
            BE.Memento.IMemento m = u.CrearMemento("tester", "snap");

            // Se cambian los datos administrativos…
            u.Username = "jp2"; u.Nombre = "Juana"; u.Apellido = "Gómez";
            u.Email = "otro@x.com"; u.FechaNacimiento = new DateTime(2000, 1, 1);
            u.RestaurarDesde(m);

            // …y el rollback los devuelve a su estado original.
            Assert.AreEqual("jperez", u.Username);
            Assert.AreEqual("Juan",   u.Nombre);
            Assert.AreEqual("Pérez",  u.Apellido);
            Assert.AreEqual("juan@x.com", u.Email);
            Assert.AreEqual(new DateTime(1990, 5, 1), u.FechaNacimiento);
        }

        // REQUISITO: el control de cambios NO debe permitir rollback de contraseñas.
        // RestaurarDesde nunca revierte la clave (ni el estado de bloqueo).
        [TestMethod]
        public void Memento_NoRevierteContrasenaNiBloqueo()
        {
            var u = new BE.Usuario { Id = 1, Username = "x", Contraseña = "ORIG", Bloqueado = false, IntentosFallidos = 2 };
            BE.Memento.IMemento m = u.CrearMemento("tester", "snap");

            u.Contraseña = "NUEVA"; u.Bloqueado = true; u.IntentosFallidos = 9;
            u.RestaurarDesde(m);

            // La contraseña y el estado de seguridad NO se revierten (solo datos administrativos).
            Assert.AreEqual("NUEVA", u.Contraseña);
            Assert.IsTrue(u.Bloqueado);
            Assert.AreEqual(9, u.IntentosFallidos);
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
