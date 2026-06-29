using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    /// <summary>
    /// Guard SISTÉMICO de "último administrador" (BLL.Familia.SistemaConservaGestion).
    /// Pruebas AISLADAS del núcleo PURO: no tocan BD ni sesión — se arma un árbol Composite y
    /// una lista de usuarios en memoria y se verifica la decisión, igual estilo que
    /// ValidarPuedeArchivar / ValidarPuedeCambiarRol.
    /// </summary>
    [TestClass]
    public class UltimoAdminGuardTests
    {
        private static BE.Patente Pat(int id, string nombre, string menu)
            => new BE.Patente { Id = id, Nombre = nombre, NombreMenu = menu };

        private static BE.Usuario Usr(string rol = null, string perfil = null)
            => new BE.Usuario { Rol = rol, Perfil = perfil };

        // Árbol de prueba:
        //   admin1   → 🔑 mnuUsuarios (gestión)
        //   vendedor → 🔑 mnuClientes
        //   super    → [Rol] admin1            (rol-dentro-de-rol: hereda la gestión)
        private static List<BE.Componente> ArbolBase()
        {
            var gestion = Pat(1, "Gestión de Usuarios", "mnuUsuarios");
            var otra    = Pat(2, "Clientes",            "mnuClientes");

            var admin1 = new BE.Rol { Id = 100, Nombre = "admin1" };
            admin1.AgregarHijo(gestion);

            var vendedor = new BE.Rol { Id = 200, Nombre = "vendedor" };
            vendedor.AgregarHijo(otra);

            var super = new BE.Rol { Id = 300, Nombre = "super" };
            super.AgregarHijo(admin1);

            return new List<BE.Componente> { admin1, vendedor, super };
        }

        [TestMethod]
        public void Conserva_CuandoUnRolResuelveLaGestion()
        {
            var usuarios = new[] { Usr(rol: "admin1") };
            Assert.IsTrue(BLL.Familia.SistemaConservaGestion(ArbolBase(), usuarios));
        }

        [TestMethod]
        public void NoConserva_CuandoNingunUsuarioTieneGestion()
        {
            var usuarios = new[] { Usr(rol: "vendedor") };
            Assert.IsFalse(BLL.Familia.SistemaConservaGestion(ArbolBase(), usuarios));
        }

        [TestMethod]
        public void Conserva_PorBypassDeAdministradorAunqueElArbolEsteVacio()
        {
            var usuarios = new[] { Usr(perfil: "Administrador") };
            Assert.IsTrue(BLL.Familia.SistemaConservaGestion(new List<BE.Componente>(), usuarios));
        }

        [TestMethod]
        public void Conserva_CuandoLaGestionSeHeredaPorRolDentroDeRol()
        {
            // 'super' no tiene la patente directa: la hereda de 'admin1' embebido (Composite recursivo).
            var usuarios = new[] { Usr(rol: "super") };
            Assert.IsTrue(BLL.Familia.SistemaConservaGestion(ArbolBase(), usuarios));
        }

        [TestMethod]
        public void NoBloquea_CuandoNoHayUsuariosQueVerificar()
        {
            Assert.IsTrue(BLL.Familia.SistemaConservaGestion(ArbolBase(), new BE.Usuario[0]));
        }
    }
}
