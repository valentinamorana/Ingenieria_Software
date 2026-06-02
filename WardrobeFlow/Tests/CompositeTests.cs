using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    /// <summary>T04 — Pruebas del patrón Composite (hojas, nodos, roles y anti-ciclos).</summary>
    [TestClass]
    public class CompositeTests
    {
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Patente_NoAdmiteHijos()
        {
            // Una patente es una HOJA: no puede contener otros componentes.
            new BE.Patente { Id = 1, Nombre = "P" }.AgregarHijo(new BE.Patente { Id = 2, Nombre = "Q" });
        }

        [TestMethod]
        public void Familia_AgregaHijos()
        {
            var f = new BE.Familia { Id = 1, Nombre = "F" };
            f.AgregarHijo(new BE.Patente { Id = 2, Nombre = "P" });
            Assert.AreEqual(1, f.Hijos.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Familia_RechazaCicloDirecto()
        {
            var f = new BE.Familia { Id = 1, Nombre = "F" };
            f.AgregarHijo(f);   // F → F
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Familia_RechazaCicloIndirecto()
        {
            var a = new BE.Familia { Id = 1, Nombre = "A" };
            var b = new BE.Familia { Id = 2, Nombre = "B" };
            a.AgregarHijo(b);
            b.AgregarHijo(a);   // A → B → A
        }

        [TestMethod]
        public void Rol_PuedeContenerFamiliasYPatentes()
        {
            var rol = new BE.Rol { Id = 1, Nombre = "Admin" };
            rol.AgregarHijo(new BE.Familia { Id = 2, Nombre = "F" });
            rol.AgregarHijo(new BE.Patente { Id = 3, Nombre = "P" });
            Assert.AreEqual(2, rol.Hijos.Count);
        }

        [TestMethod]
        public void Rol_NoDuplicaElMismoHijo()
        {
            var rol = new BE.Rol { Id = 1, Nombre = "Admin" };
            var p = new BE.Patente { Id = 2, Nombre = "P" };
            rol.AgregarHijo(p);
            rol.AgregarHijo(p);
            Assert.AreEqual(1, rol.Hijos.Count);
        }
    }
}
