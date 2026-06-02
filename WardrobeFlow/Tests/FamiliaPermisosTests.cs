using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.Fakes;

namespace Tests
{
    /// <summary>
    /// T04 — Pruebas AISLADAS de la resolución de permisos (BLL.Familia) usando un DAL
    /// falso inyectado por constructor (D2: DI/interfaces). No tocan la base de datos.
    /// </summary>
    [TestClass]
    public class FamiliaPermisosTests
    {
        private BLL.Familia _bll;

        [TestInitialize]
        public void Setup()
        {
            _bll = new BLL.Familia(new FakePermisoDAL());   // inyección del doble
        }

        [TestMethod]
        public void PermisosEfectivos_RecorreFamiliasRecursivamente()
        {
            var perms = _bll.ObtenerPermisosEfectivos("Admin");
            // Admin → Ventas → {Clientes, Stock}  (+ Clientes directo)
            CollectionAssert.AreEquivalent(
                new[] { "mnuClientes", "mnuStock" },
                perms.Select(p => p.NombreMenu).ToArray());
        }

        [TestMethod]
        public void PermisosEfectivos_DeduplicaPermisosRepetidos()
        {
            var perms = _bll.ObtenerPermisosEfectivos("Admin");
            // 'Clientes' llega por dos caminos pero debe contarse una sola vez.
            Assert.AreEqual(2, perms.Count);
            Assert.AreEqual(1, perms.Count(p => p.NombreMenu == "mnuClientes"));
        }

        [TestMethod]
        public void PermisosEfectivos_ResuelveRolDentroDeRol()
        {
            // Gerente no tiene patentes propias: hereda todo de Admin (rol embebido).
            var perms = _bll.ObtenerPermisosEfectivos("Gerente");
            CollectionAssert.AreEquivalent(
                new[] { "mnuClientes", "mnuStock" },
                perms.Select(p => p.NombreMenu).ToArray());
        }

        [TestMethod]
        public void PermisosEfectivos_RolInexistente_DevuelveVacio()
        {
            Assert.AreEqual(0, _bll.ObtenerPermisosEfectivos("NoExiste").Count);
        }

        [TestMethod]
        public void FamiliasDisponibles_ExcluyeRoles()
        {
            var fams = _bll.ObtenerFamiliasDisponibles();
            // Solo 'Ventas' es familia compuesta (Admin y Gerente son roles).
            Assert.AreEqual(1, fams.Count);
            Assert.AreEqual("Ventas", fams[0].Nombre);
        }

        [TestMethod]
        public void PatentesDisponibles_SinDuplicados()
        {
            var pats = _bll.ObtenerPatentesDisponibles();
            Assert.AreEqual(2, pats.Count);
        }
    }
}
