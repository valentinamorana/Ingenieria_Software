using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    /// <summary>
    /// Mejoras revisión 11/06 — ABM de datos administrativos, cambio de rol y guard de DV.
    /// Se prueban las validaciones PURAS (sin sesión ni BD) y el guard fail-closed de los
    /// Dígitos Verificadores.
    /// </summary>
    [TestClass]
    public class UsuarioAbmTests
    {
        private static readonly List<string> SinOtros = new List<string>();

        // Ejecuta la acción y devuelve la AppException lanzada (o null si no lanzó).
        // (MSTest v1 no tiene Assert.ThrowsException.)
        private static BE.AppException Capturar(Action accion)
        {
            try { accion(); }
            catch (BE.AppException ex) { return ex; }
            return null;
        }

        // ── ValidarDatosAdministrativos ──────────────────────────────────────────
        [TestMethod]
        public void Validar_DatosCorrectos_NoLanza()
        {
            BLL.Usuario.ValidarDatosAdministrativos("jperez", "juan@mail.com",
                new DateTime(1990, 1, 1), SinOtros);
        }

        [TestMethod]
        public void Validar_UsernameCorto_Lanza()
        {
            var ex = Capturar(() => BLL.Usuario.ValidarDatosAdministrativos("ab", null, null, SinOtros));
            Assert.IsNotNull(ex);
            Assert.AreEqual("err.bll.usuario.username_corto", ex.Clave);
        }

        [TestMethod]
        public void Validar_UsernameDuplicado_Lanza()
        {
            var otros = new List<string> { "admin", "jperez" };
            var ex = Capturar(() => BLL.Usuario.ValidarDatosAdministrativos("JPEREZ", null, null, otros));
            Assert.IsNotNull(ex);
            Assert.AreEqual("err.bll.usuario.username_duplicado", ex.Clave);
        }

        [TestMethod]
        public void Validar_EmailInvalido_Lanza()
        {
            var ex = Capturar(() => BLL.Usuario.ValidarDatosAdministrativos("jperez", "correo-malo", null, SinOtros));
            Assert.IsNotNull(ex);
            Assert.AreEqual("err.bll.usuario.email_invalido", ex.Clave);
        }

        [TestMethod]
        public void Validar_EmailValido_NoLanza()
        {
            BLL.Usuario.ValidarDatosAdministrativos("jperez", "a.b@dominio.com.ar", null, SinOtros);
        }

        [TestMethod]
        public void Validar_FechaNacimientoFutura_Lanza()
        {
            var ex = Capturar(() => BLL.Usuario.ValidarDatosAdministrativos("jperez", null,
                    DateTime.Today.AddDays(1), SinOtros));
            Assert.IsNotNull(ex);
            Assert.AreEqual("err.bll.usuario.fechanac_futura", ex.Clave);
        }

        [TestMethod]
        public void Validar_MenorDeEdad_Lanza()
        {
            var ex = Capturar(() => BLL.Usuario.ValidarDatosAdministrativos("jperez", null,
                    DateTime.Today.AddYears(-10), SinOtros));
            Assert.IsNotNull(ex);
            Assert.AreEqual("err.bll.usuario.fechanac_menor", ex.Clave);
        }

        // ── ValidarPuedeCambiarRol ───────────────────────────────────────────────
        [TestMethod]
        public void CambiarRol_QuitarAlUltimoAdmin_Lanza()
        {
            var ex = Capturar(() => BLL.Usuario.ValidarPuedeCambiarRol("Administrador", "Vendedor", 1));
            Assert.IsNotNull(ex);
            Assert.AreEqual("err.bll.usuario.ultimo_admin_rol", ex.Clave);
        }

        [TestMethod]
        public void CambiarRol_HayOtroAdmin_NoLanza()
        {
            BLL.Usuario.ValidarPuedeCambiarRol("Administrador", "Vendedor", 2);
        }

        [TestMethod]
        public void CambiarRol_NoEraAdmin_NoLanza()
        {
            BLL.Usuario.ValidarPuedeCambiarRol("Vendedor", "GerenteComercial", 1);
        }

        [TestMethod]
        public void CambiarRol_SigueSiendoAdmin_NoLanza()
        {
            // Si mantiene Administrador no se le está quitando el rol → no aplica la protección.
            BLL.Usuario.ValidarPuedeCambiarRol("Administrador", "Administrador", 1);
        }

        // ── Guard fail-closed de Dígitos Verificadores (#5) ──────────────────────
        [TestMethod]
        public void RecalcularDV_UsuarioNoAdmin_Bloquea()
        {
            var noAdmin = new BE.Usuario { Id = 7, Username = "vendedor", Perfil = "Vendedor" };
            try
            {
                Seguridad.SessionManager.Logout();   // defensivo: el SessionManager es global
                Seguridad.SessionManager.Login(noAdmin);
                var ex = Capturar(() => BLL.Configuracion.RecalcularIntegridadDV());
                Assert.IsNotNull(ex);
                Assert.AreEqual("err.bll.dv.sin_permiso", ex.Clave);
            }
            finally
            {
                Seguridad.SessionManager.Logout();
            }
        }
    }
}
