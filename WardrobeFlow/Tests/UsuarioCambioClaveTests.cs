using Microsoft.VisualStudio.TestTools.UnitTesting;
using Seguridad;
using Tests.Fakes;

namespace Tests
{
    /// <summary>
    /// Cambio de clave OBLIGATORIO (#2 de la auditoría). Cubre las reglas de rechazo de
    /// BLL.Usuario.CambiarClavePropia, que se evalúan ANTES de tocar la BD (DAL/bitácora):
    /// requiere sesión, exige una clave que cumpla los requisitos y que difiera de la actual.
    /// El camino feliz (persiste + baja el flag + escribe bitácora) es integración con BD.
    /// </summary>
    [TestClass]
    public class UsuarioCambioClaveTests
    {
        [TestInitialize] public void Setup()   => SessionManager.Logout();
        [TestCleanup]    public void Cleanup() => SessionManager.Logout();

        private static void LoginCon(string claveActualPlano)
        {
            SessionManager.Login(new BE.Usuario
            {
                Id                  = 5,
                Username            = "tester",
                Perfil              = "Auditor",
                Contraseña          = Encriptador.Hash(claveActualPlano),
                RequiereCambioClave = true
            });
        }

        [TestMethod]
        public void CambiarClavePropia_SinSesion_LanzaSesionException()
        {
            var fake = new FakeUsuarioDAL();
            var bll  = new BLL.Usuario(fake);
            try
            {
                bll.CambiarClavePropia("Test", "NuevaClave1!");
                Assert.Fail("Debía exigir sesión iniciada.");
            }
            catch (BE.SesionException ex)
            {
                Assert.AreEqual("err.seg.sesion_no_iniciada", ex.Clave);
            }
            Assert.AreEqual(0, fake.CambiarClaveVeces, "Sin sesión no debe tocar el DAL.");
        }

        [TestMethod]
        public void CambiarClavePropia_ClaveDebil_LanzaClaveInvalida_SinTocarDAL()
        {
            LoginCon("Actual1!");
            var fake = new FakeUsuarioDAL();
            var bll  = new BLL.Usuario(fake);
            try
            {
                bll.CambiarClavePropia("Test", "abc");   // no cumple requisitos
                Assert.Fail("Debía rechazar una clave débil.");
            }
            catch (BE.AppException ex)
            {
                Assert.AreEqual("err.bll.usuario.clave_invalida", ex.Clave);
            }
            Assert.AreEqual(0, fake.CambiarClaveVeces);
        }

        [TestMethod]
        public void CambiarClavePropia_IgualALaActual_LanzaClaveIgual_SinTocarDAL()
        {
            LoginCon("Actual1!");
            var fake = new FakeUsuarioDAL();
            var bll  = new BLL.Usuario(fake);
            try
            {
                bll.CambiarClavePropia("Test", "Actual1!");  // misma que la actual
                Assert.Fail("Debía rechazar reutilizar la clave actual.");
            }
            catch (BE.AppException ex)
            {
                Assert.AreEqual("err.bll.usuario.clave_igual_actual", ex.Clave);
            }
            Assert.AreEqual(0, fake.CambiarClaveVeces);
        }
    }
}
