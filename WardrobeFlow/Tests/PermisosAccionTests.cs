using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    /// <summary>
    /// Permisos granulares de acción ("Ver" vs "Configurar") — BLL.PermisosAccion.PermiteAccion.
    /// Tabla de verdad de la decisión PURA (sin BD ni sesión), incluida la retrocompatibilidad:
    /// si la patente de edición no está definida en el catálogo, se cae al permiso de VER.
    /// </summary>
    [TestClass]
    public class PermisosAccionTests
    {
        // esAdmin, editarDefinida, tieneEditar, tieneVer
        private static bool P(bool admin, bool def, bool ed, bool ver)
            => BLL.PermisosAccion.PermiteAccion(admin, def, ed, ver);

        [TestMethod]
        public void Admin_SiemprePuede_AunqueNoTengaNada()
        {
            Assert.IsTrue(P(admin: true, def: true,  ed: false, ver: false));
            Assert.IsTrue(P(admin: true, def: false, ed: false, ver: false));
        }

        [TestMethod]
        public void Migrado_ConPatenteDeEdicion_Puede()
        {
            // editar definida en el catálogo + el usuario la tiene → puede.
            Assert.IsTrue(P(admin: false, def: true, ed: true, ver: true));
            Assert.IsTrue(P(admin: false, def: true, ed: true, ver: false));
        }

        [TestMethod]
        public void Migrado_SoloVer_NoPuedeEditar()
        {
            // editar definida pero el usuario solo tiene VER → solo-lectura (el objetivo del Tier 2).
            Assert.IsFalse(P(admin: false, def: true, ed: false, ver: true));
            Assert.IsFalse(P(admin: false, def: true, ed: false, ver: false));
        }

        [TestMethod]
        public void SinMigrar_CaeAlPermisoDeVer_Retrocompatible()
        {
            // editar NO definida (base sin migrar) → se exige VER, igual que antes.
            Assert.IsTrue(P(admin: false, def: false, ed: false, ver: true));
            Assert.IsFalse(P(admin: false, def: false, ed: false, ver: false));
        }
    }
}
