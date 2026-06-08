using System;
using System.Windows.Forms;

namespace GUI
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Handler GLOBAL de excepciones no controladas: las registra en la bitácora (criticidad
            // Alta) y muestra un aviso, en vez de cerrar la app de forma muda. (Patrón de Stach.)
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, a) => ManejarExcepcionGlobal(a.Exception);
            AppDomain.CurrentDomain.UnhandledException += (s, a) => ManejarExcepcionGlobal(a.ExceptionObject as Exception);

            if (!BLL.Configuracion.VerificarConexionDAL(out string errConexion))
            {
                MessageBox.Show(errConexion, "Error de Conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Etapa 5 — i18n 100% desde BD: sembrar (si falta) y cargar el idioma por defecto desde
            // la BD ANTES de mostrar cualquier pantalla, para que toda la app (Login y diálogos de
            // arranque incluidos) se renderice desde la base. Si la BD/i18n fallara, el Traductor
            // sigue cayendo a los diccionarios hardcodeados (fallback de seguridad).
            InicializarIdiomaDesdeBD();

            // T07 — Verificar integridad DVH/DVV antes de mostrar el Login.
            // Si se detecta manipulación, abre RestauracionForm para que el admin pueda reparar.
            if (!BLL.Configuracion.VerificarIntegridadDV(out BLL.ResultadoIntegridad resultadoDV))
            {
                using (var restForm = new RestauracionForm(resultadoDV))
                {
                    Application.Run(restForm);
                    if (!restForm.RestauradoExitosamente)
                        return;
                }
            }

            // Garantiza que exista admin2 (admin de respaldo para desbloquear al admin1 si se bloquea).
            string rutaAdmin2 = BLL.Configuracion.SeedAdminSecundario();
            if (rutaAdmin2 != null)
                MessageBox.Show(
                    "Se creó el usuario administrador de respaldo 'admin2'.\n" +
                    "Sus credenciales fueron guardadas en:\n\n" + rutaAdmin2 +
                    "\n\nGuardá ese archivo en un lugar seguro.",
                    "Administrador de respaldo creado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

            // RF-10 — Genera el set inicial de 10 claves de emergencia (1 solo uso) para que un
            // Administrador bloqueado pueda autodesbloquearse sin depender de otro admin.
            string rutaClaves = BLL.Configuracion.SeedClavesEmergencia();
            if (rutaClaves != null)
                MessageBox.Show(
                    "Se generaron 10 claves de emergencia de un solo uso.\n" +
                    "Sirven para desbloquear una cuenta de Administrador bloqueada.\n\n" +
                    "Se guardaron en:\n" + rutaClaves +
                    "\n\nGuardá ese archivo en un lugar seguro.",
                    "Claves de emergencia generadas",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

            using (var frmLogin = new Login())
            {
                if (frmLogin.ShowDialog() == DialogResult.OK)
                    Application.Run(new Menu());
                else
                    Application.Exit();
            }
        }

        // Etapa 5 — Deja la BD como fuente de las traducciones desde el arranque: dispara el seed
        // (idempotente) la primera vez y carga el diccionario del idioma por defecto en el
        // GestorIdioma, de modo que el Traductor sirva textos de BD ya en la primera pantalla.
        private static void InicializarIdiomaDesdeBD()
        {
            try
            {
                var svc = new BLL.IdiomaService();
                var def = Servicios.Multiidioma.Traductor.ObtenerIdiomaDefault();
                if (def == null) return;

                var dict = svc.CargarTraducciones(def.Id);   // seedea (1ª vez) + carga desde BD
                if (dict != null && dict.Count > 0)
                    Servicios.Multiidioma.GestorIdioma.CambiarIdioma(def, dict);

                var activos = svc.ObtenerIdiomasActivosComoIdioma();
                if (activos != null && activos.Count > 0)
                    Servicios.Multiidioma.GestorIdioma.SetIdiomasDisponibles(activos);
            }
            catch (Exception ex)
            {
                // Sin BD/i18n disponible: el Traductor usa el fallback hardcodeado. No es crítico.
                System.Diagnostics.Trace.TraceError("[Program.InicializarIdiomaDesdeBD] " + ex.Message);
            }
        }

        // Registra cualquier excepción no controlada en la bitácora y avisa al usuario.
        // Nunca relanza: el logueo no debe tapar el error original ni provocar un segundo crash.
        private static void ManejarExcepcionGlobal(Exception ex)
        {
            if (ex == null) return;
            try
            {
                int? idUsuario = Seguridad.SessionManager.IsLoggedIn
                    ? (int?)Seguridad.SessionManager.GetInstance().Usuario.Id : null;
                new Servicios.Bitacora().RegistrarSinSesion(
                    modulo:     "Aplicación",
                    actividad:  "Excepción no controlada: " + ex.GetType().Name,
                    criticidad: BE.Criticidad.Alta,
                    idUsuario:  idUsuario,
                    detalle:    ex.ToString());
            }
            catch { /* si falla el logueo, igual mostramos el error */ }

            MessageBox.Show(ex.Message, "Error inesperado", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
