using System;
using System.Linq;
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

        // ── Operación recursiva del Composite (rol-dentro-de-rol, como lote-dentro-de-lote) ──

        [TestMethod]
        public void Composite_PatentesEfectivas_RecursivoEnRolesAnidados()
        {
            // 3 niveles de anidamiento: GerenteInventario ⊃ EncargadoDeStock ⊃ OperadorLogistico.
            var pOper  = new BE.Patente { Id = 1, Nombre = "Despacho",   NombreMenu = "mnuPedidosRealizados" };
            var pStock = new BE.Patente { Id = 2, Nombre = "Stock",      NombreMenu = "mnuStock" };
            var pCat   = new BE.Patente { Id = 3, Nombre = "Categorias", NombreMenu = "mnuCategorias" };

            var operador = new BE.Rol { Id = 10, Nombre = "OperadorLogistico" };
            operador.AgregarHijo(pOper);

            var encargado = new BE.Rol { Id = 11, Nombre = "EncargadoDeStock" };
            encargado.AgregarHijo(pStock);
            encargado.AgregarHijo(operador);    // rol dentro de rol

            var gerente = new BE.Rol { Id = 12, Nombre = "GerenteInventario" };
            gerente.AgregarHijo(pCat);
            gerente.AgregarHijo(encargado);     // rol dentro de rol dentro de rol

            var hojas = gerente.ObtenerPatentesEfectivas();
            CollectionAssert.AreEquivalent(
                new[] { "mnuCategorias", "mnuStock", "mnuPedidosRealizados" },
                hojas.Select(p => p.NombreMenu).ToArray());
        }

        [TestMethod]
        public void Composite_PatentesEfectivas_DeduplicaPorVariosCaminos()
        {
            var p   = new BE.Patente { Id = 1, Nombre = "X", NombreMenu = "mnuX" };
            var fam = new BE.Familia { Id = 10, Nombre = "F1" };
            fam.AgregarHijo(p);

            var rol = new BE.Rol { Id = 20, Nombre = "R" };
            rol.AgregarHijo(fam);
            rol.AgregarHijo(p);     // la misma patente llega por dos caminos

            Assert.AreEqual(1, rol.ObtenerPatentesEfectivas().Count);
        }

        [TestMethod]
        public void Composite_ObtenerDescripcion_RecorreElArbol()
        {
            var rol = new BE.Rol { Id = 1, Nombre = "Admin" };
            var fam = new BE.Familia { Id = 2, Nombre = "Ventas" };
            fam.AgregarHijo(new BE.Patente { Id = 3, Nombre = "Clientes", NombreMenu = "mnuClientes" });
            rol.AgregarHijo(fam);

            string desc = rol.ObtenerDescripcion();
            StringAssert.Contains(desc, "[Rol] Admin");
            StringAssert.Contains(desc, "[Familia] Ventas");
            StringAssert.Contains(desc, "[Patente] Clientes");
        }
    }
}
