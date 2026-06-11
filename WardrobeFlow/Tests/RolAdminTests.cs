using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    /// <summary>
    /// #3 de la auditoría — la identidad de Administrador se resuelve en UN solo lugar
    /// (BE.Usuario.EsAdministrador / BE.Roles.EsAdministrador), no con string.Equals disperso.
    /// Reglas: case-insensitive, null-safe, y considera tanto Perfil como Rol.
    /// </summary>
    [TestClass]
    public class RolAdminTests
    {
        [TestMethod]
        public void EsAdministrador_PorPerfil_True()
        {
            Assert.IsTrue(new BE.Usuario { Perfil = "Administrador" }.EsAdministrador);
        }

        [TestMethod]
        public void EsAdministrador_CaseInsensitive_True()
        {
            Assert.IsTrue(new BE.Usuario { Perfil = "administrador" }.EsAdministrador);
        }

        [TestMethod]
        public void EsAdministrador_PorRol_AunqueElPerfilNoLoSea_True()
        {
            Assert.IsTrue(new BE.Usuario { Perfil = "Vendedor", Rol = "Administrador" }.EsAdministrador);
        }

        [TestMethod]
        public void EsAdministrador_RolComun_False()
        {
            Assert.IsFalse(new BE.Usuario { Perfil = "Auditor", Rol = "Auditor" }.EsAdministrador);
        }

        [TestMethod]
        public void EsAdministrador_PerfilYRolNulos_False()
        {
            Assert.IsFalse(new BE.Usuario { Perfil = null, Rol = null }.EsAdministrador);
        }

        [TestMethod]
        public void Roles_EsAdministrador_Helper_NullSafe()
        {
            Assert.IsFalse(BE.Roles.EsAdministrador(null));
            Assert.IsTrue(BE.Roles.EsAdministrador("Administrador"));
        }
    }
}
