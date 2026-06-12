using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Servicios.Multiidioma;

namespace Tests
{
    /// <summary>
    /// Casos de Prueba Especiales del documento "Requerimientos pre 2da entrega".
    /// Cada método mapea uno de los 8 escenarios solicitados.
    ///
    /// Los casos que dependen de SQL Server real (restaurar/verificar un .bak) se marcan
    /// con [Ignore] como pruebas de INTEGRACIÓN: documentan el comportamiento esperado y el
    /// punto exacto de la implementación, pero no corren en CI sin una base + backup reales.
    /// El resto son pruebas unitarias efectivas (sin BD ni sesión).
    /// </summary>
    [TestClass]
    public class CasosPruebaEspecialesTests
    {
        // ── Caso 1 — Intentar eliminar el último administrador (RF-10) ───────────

        [TestMethod]
        public void Caso1_NoSePuedeArchivarAlUltimoAdministrador()
        {
            // Un único Administrador activo → archivarlo debe cancelarse con AppException.
            try
            {
                BLL.Usuario.ValidarPuedeArchivar("Administrador", idObjetivo: 5, idEnSesion: 9, adminsActivos: 1);
                Assert.Fail("Debía impedir archivar al último Administrador activo.");
            }
            catch (BE.AppException ex)
            {
                Assert.AreEqual("err.bll.usuario.ultimo_admin", ex.Clave);
            }
        }

        [TestMethod]
        public void Caso1_SePuedeArchivarAdminSiHayOtrosActivos()
        {
            // 2 admins activos → archivar uno (distinto del de la sesión) está permitido (no lanza).
            BLL.Usuario.ValidarPuedeArchivar("Administrador", idObjetivo: 5, idEnSesion: 9, adminsActivos: 2);
        }

        [TestMethod]
        public void Caso1_NoSePuedeArchivarAlUsuarioEnSesion()
        {
            try
            {
                BLL.Usuario.ValidarPuedeArchivar("Vendedor", idObjetivo: 7, idEnSesion: 7, adminsActivos: 3);
                Assert.Fail("Debía impedir que un usuario se archive a sí mismo.");
            }
            catch (BE.AppException ex)
            {
                Assert.AreEqual("err.bll.usuario.autobaja", ex.Clave);
            }
        }

        // ── Caso 2 — Intentar restaurar un backup corrupto (RNF-02) ──────────────

        [TestMethod]
        [Ignore] // Integración: requiere SQL Server + un .bak corrupto.
                 // DAL.Backup.VerificarBackup ejecuta RESTORE VERIFYONLY y, ante un archivo
                 // inválido, lanza BE.AppException("err.dal.backup_corrupto"). RestaurarBackup
                 // llama a VerificarBackup ANTES de restaurar, por lo que un backup corrupto
                 // nunca llega a sobrescribir la base.
        public void Caso2_RestaurarBackupCorrupto_EsRechazado()
        {
            var dal = new DAL.Backup();
            try
            {
                dal.VerificarBackup("backup_corrupto.bak");
                Assert.Fail("Un backup corrupto debía ser rechazado.");
            }
            catch (BE.AppException ex)
            {
                Assert.AreEqual("err.dal.backup_corrupto", ex.Clave);
            }
        }

        // ── Caso 3 — Detectar corrupción mediante DVH/DVV (RF-01/02) ─────────────

        [TestMethod]
        public void Caso3_DVH_DetectaManipulacionDeFila()
        {
            var dv   = new Seguridad.DigitoVerificador();
            var fila = new BE.FilaUsuarioDV
            {
                Id = 1, Username = "admin", Clave = "hash",
                Rol = "Administrador", Perfil = "Administrador", Estado = "1", IntentosFallidos = "0"
            };
            int dvhOriginal = dv.CalcularDVH(fila.CamposParaDVH());

            // Manipulación directa en BD: alguien desbloqueó la cuenta por fuera de la app.
            fila.Estado = "0";
            int dvhManipulado = dv.CalcularDVH(fila.CamposParaDVH());

            Assert.AreNotEqual(dvhOriginal, dvhManipulado,
                "El DVH debe revelar cualquier manipulación de la fila.");
        }

        [TestMethod]
        public void Caso3_DVV_DetectaEliminacionDeFila()
        {
            var dv = new Seguridad.DigitoVerificador();
            int dvvIntegro = dv.CalcularDVV(new List<int> { 111, 222, 333 });
            // Alguien borró una fila directamente en la base:
            int dvvTrasBorrado = dv.CalcularDVV(new List<int> { 111, 333 });
            Assert.AreNotEqual(dvvIntegro, dvvTrasBorrado,
                "El DVV debe revelar la eliminación (o inserción) de una fila.");
        }

        // ── Caso 4 — Asignar roles generando dependencia circular (RF-11/12/13) ──

        [TestMethod]
        public void Caso4_JerarquiaDeRoles_RechazaDependenciaCircular()
        {
            // Jerarquía rol-dentro-de-rol: Gerente → Administrador.
            var gerente = new BE.Rol { Id = 1, Nombre = "Gerente" };
            var admin   = new BE.Rol { Id = 2, Nombre = "Administrador" };
            gerente.AgregarHijo(admin);

            // Intentar Administrador → Gerente cerraría el ciclo (Gerente → Admin → Gerente).
            try
            {
                admin.AgregarHijo(gerente);
                Assert.Fail("Debía rechazar la dependencia circular entre roles.");
            }
            catch (InvalidOperationException ex)
            {
                Assert.IsTrue(ex.Message.ToLower().Contains("circular"),
                    "El error debe indicar la referencia circular detectada.");
            }
        }

        [TestMethod]
        public void Caso4_JerarquiaDeRoles_PermiteAnidamientoValido()
        {
            // Un rol puede contener otros roles / familias / patentes sin generar ciclos.
            var gerente = new BE.Rol { Id = 1, Nombre = "Gerente" };
            var admin   = new BE.Rol { Id = 2, Nombre = "Administrador" };
            var ventas  = new BE.Familia { Id = 3, Nombre = "Ventas" };

            admin.AgregarHijo(ventas);     // rol contiene una familia
            gerente.AgregarHijo(admin);    // rol contiene otro rol

            Assert.AreEqual(1, gerente.Hijos.Count);
            Assert.AreEqual(1, admin.Hijos.Count);
        }

        // ── Caso 5 — Realizar múltiples niveles de rollback (RF-16/17/18) ────────

        [TestMethod]
        public void Caso5_MultiplesNivelesDeRollback()
        {
            // Patrón Memento: se capturan varias versiones de los DATOS ADMINISTRATIVOS (no sensibles)
            // y se deshace nivel por nivel. El control de cambios NO versiona la contraseña, así que
            // el rollback opera sobre nombre/apellido/email (revisión 11/06).
            var u  = new BE.Usuario { Id = 1, Username = "u", Nombre = "v1", Apellido = "A1", Contraseña = "secreta" };
            var m1 = u.CrearMemento("admin", "estado v1");

            u.Nombre = "v2"; u.Apellido = "A2";
            var m2 = u.CrearMemento("admin", "estado v2");

            u.Nombre = "v3"; u.Apellido = "A3"; u.Contraseña = "otra";

            // Rollback nivel 1: v3 → v2
            u.RestaurarDesde(m2);
            Assert.AreEqual("v2", u.Nombre);
            Assert.AreEqual("A2", u.Apellido);

            // Rollback nivel 2: v2 → v1
            u.RestaurarDesde(m1);
            Assert.AreEqual("v1", u.Nombre);
            Assert.AreEqual("A1", u.Apellido);

            // La contraseña nunca se revierte por el rollback (no es un dato versionable).
            Assert.AreEqual("otra", u.Contraseña);
        }

        // ── Caso 6 — Cambiar idioma con múltiples usuarios conectados (RNF-05) ────

        [TestMethod]
        public void Caso6_PreferenciaDeIdiomaEsIndependientePorUsuario()
        {
            // Cada usuario persiste su propio idioma: cambiar el de uno no afecta al otro.
            var ana  = new BE.Usuario { Id = 1, Username = "ana",  IdIdioma = "ES" };
            var ivan = new BE.Usuario { Id = 2, Username = "ivan", IdIdioma = "RU" };

            ana.IdIdioma = "EN";   // Ana cambia su preferencia

            Assert.AreEqual("EN", ana.IdIdioma);
            Assert.AreEqual("RU", ivan.IdIdioma,
                "El cambio de idioma de un usuario no debe afectar la preferencia de otro.");
        }

        // ── Caso 7 — Cambiar idioma mientras la app está en uso (RF-19/20) ───────

        [TestMethod]
        public void Caso7_CambioDeIdiomaEnRuntime_NotificaObservers()
        {
            var espia  = new ObserverEspia();
            var previo = GestorIdioma.IdiomaActual;   // para restaurar al final

            GestorIdioma.SuscribirObservador(espia);
            try
            {
                GestorIdioma.CambiarIdioma(new Idioma { Id = "EN", Nombre = "English" });

                Assert.IsTrue(espia.Notificado,
                    "El Observer debe recibir UpdateLanguage al cambiar el idioma en tiempo de ejecución.");
                Assert.AreEqual("EN", espia.UltimoIdioma);
            }
            finally
            {
                GestorIdioma.DesuscribirObservador(espia);
                if (previo != null) GestorIdioma.CambiarIdioma(previo);   // no afectar otras pruebas
            }
        }

        // ── Caso 8 — Restaurar BD con info posterior al último backup (RF-08) ─────

        [TestMethod]
        [Ignore] // Integración: requiere SQL Server + un .bak real.
                 // BLL.Backup.ObtenerFechaBackup lee BackupFinishDate (RESTORE HEADERONLY);
                 // BackupForm informa "se perderán los cambios posteriores a {fecha}" antes
                 // de confirmar, de modo que el admin conoce el alcance de la pérdida.
        public void Caso8_RestaurarInformaAlcanceDePerdida()
        {
            var bll = new BLL.Backup();
            DateTime? fechaBackup = bll.ObtenerFechaBackup("backup_real.bak");
            Assert.IsNotNull(fechaBackup,
                "Debe poder leerse la fecha del backup para calcular el alcance de la pérdida.");
        }

        // Observer de prueba: registra si fue notificado y con qué idioma.
        private class ObserverEspia : IIdiomaObserver
        {
            public bool   Notificado   { get; private set; }
            public string UltimoIdioma { get; private set; }

            public void UpdateLanguage(Idioma idioma)
            {
                Notificado   = true;
                UltimoIdioma = idioma?.Id;
            }
        }
    }
}
