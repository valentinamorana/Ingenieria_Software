using Microsoft.VisualStudio.TestTools.UnitTesting;
using Seguridad;

namespace Tests
{
    /// <summary>
    /// RNF-04 — El SessionManager debe lanzar excepciones de DOMINIO tipadas
    /// (BE.SesionException, traducibles en la GUI) en vez de Exception genérica.
    /// Logout es idempotente, por lo que el setup/cleanup garantizan un estado limpio
    /// sin afectar a otras pruebas.
    /// </summary>
    [TestClass]
    public class SessionManagerTests
    {
        [TestInitialize] public void Setup()   => SessionManager.Logout();
        [TestCleanup]    public void Cleanup() => SessionManager.Logout();

        [TestMethod]
        public void GetInstance_SinSesion_LanzaSesionExceptionConClave()
        {
            try
            {
                SessionManager.GetInstance();
                Assert.Fail("Debía lanzar SesionException cuando no hay sesión.");
            }
            catch (BE.SesionException ex)
            {
                Assert.AreEqual("err.seg.sesion_no_iniciada", ex.Clave);
            }
        }

        [TestMethod]
        public void Login_Duplicado_LanzaSesionExceptionConClave()
        {
            SessionManager.Login(new BE.Usuario { Id = 1, Username = "tester", Perfil = "Auditor" });
            try
            {
                SessionManager.Login(new BE.Usuario { Id = 2, Username = "otro" });
                Assert.Fail("Debía lanzar SesionException por sesión ya iniciada.");
            }
            catch (BE.SesionException ex)
            {
                Assert.AreEqual("err.seg.sesion_ya_iniciada", ex.Clave);
            }
        }

        [TestMethod]
        public void SesionException_EsAppException_ParaTraduccionEnLaGUI()
        {
            // FormBase.MostrarError traduce cualquier AppException por su Clave;
            // SesionException debe serlo para mostrarse traducida al usuario.
            Assert.IsInstanceOfType(
                new BE.SesionException("err.seg.sesion_no_iniciada", "fallback"),
                typeof(BE.AppException));
        }
    }
}
