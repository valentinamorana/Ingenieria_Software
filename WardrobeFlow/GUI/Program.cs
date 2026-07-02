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

            // T07 — VERIFICACIÓN DE INTEGRIDAD ANTES DEL LOGIN (requisito de cátedra).
            // "Al iniciar la aplicación, y antes de dar acceso a la ventana de log-in, se debe
            //  realizar el proceso de verificación de integridad de la base de datos."
            // El proceso de cálculo/comparación de DVH+DVV se ejecuta ACÁ, previo a mostrar el Login.
            // Por seguridad, el DETALLE de las filas rotas y la REPARACIÓN siguen reservados a un
            // Administrador autenticado: si la verificación falla, se deja constancia en la bitácora
            // (informar al administrador) y, tras el login, se enruta según el rol (ver más abajo).
            bool integridadOk = BLL.Configuracion.VerificarIntegridadDV(out BLL.ResultadoIntegridad _);
            if (!integridadOk)
            {
                try
                {
                    new Servicios.Bitacora().RegistrarSinSesion(
                        modulo:     "Arranque",
                        actividad:  "Integridad DV inválida detectada antes del Login",
                        criticidad: BE.Criticidad.Alta,
                        detalle:    "La verificación de dígitos verificadores (DVH/DVV) previa al Login " +
                                    "detectó una posible manipulación externa de la base. Se requiere que un " +
                                    "Administrador inicie sesión para reparar o restaurar el sistema.");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError(
                        "[Program] No se pudo auditar la falla de integridad de arranque: " + ex.Message);
                }
            }

            using (var frmLogin = new Login())
            {
                if (frmLogin.ShowDialog() != DialogResult.OK)
                    return;   // login cancelado → fin

                // T07 — La verificación ya se ejecutó ANTES del Login (arriba). Con el usuario
                // autenticado solo se ENRUTA la respuesta: el detalle y la reparación son solo-admin.
                if (!integridadOk)
                {
                    var usuario = Seguridad.SessionManager.IsLoggedIn
                        ? Seguridad.SessionManager.GetInstance().Usuario : null;

                    // Solo un Administrador ve el detalle de los dígitos rotos y puede repararlos.
                    // A cualquier otro usuario se le bloquea el ingreso con un mensaje GENÉRICO de
                    // mantenimiento: NO se le revela que la base fue manipulada (no darle esa pista a
                    // un posible atacante). El detalle real solo lo ve el Administrador.
                    if (usuario == null || !usuario.EsAdministrador)
                    {
                        var t = Servicios.Multiidioma.Traductor.ObtenerTraducciones(
                                    Servicios.Multiidioma.GestorIdioma.IdiomaActual);
                        string Tx(string k, string fb) => t.ContainsKey(k) ? t[k].Texto : fb;
                        MessageBox.Show(
                            Tx("msg.mantenimiento.cuerpo",
                               "El sistema no está disponible en este momento por tareas de mantenimiento. Reintentá más tarde o contactá al Administrador."),
                            Tx("msg.mantenimiento.titulo", "Sistema en mantenimiento"),
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        new BLL.Usuario().Logout("Arranque");
                        return;
                    }

                    // Administrador: abrir la Consola de Recuperación (Reparar desde Espejo /
                    // Asumir Pérdida / Restaurar Backup). Única vía de recuperación del sistema.
                    using (var rec = new RecuperacionEspejoForm())
                    {
                        Application.Run(rec);
                        if (!rec.RecuperadoExitosamente)
                            return;   // no reparó → no se entra con la base comprometida
                    }
                }

                Application.Run(new Menu());
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

            // #6 — Ante un error NO controlado se muestra un mensaje GENÉRICO (no se expone
            // información técnica al usuario). El detalle técnico ya quedó en la bitácora.
            string titulo  = "Error inesperado";
            string mensaje = "Ha ocurrido un error inesperado. Por favor, contacte al administrador del sistema.";
            try
            {
                var t = Servicios.Multiidioma.Traductor.ObtenerTraducciones(
                            Servicios.Multiidioma.GestorIdioma.IdiomaActual);
                if (t.ContainsKey("msg.error.titulo"))     titulo  = t["msg.error.titulo"].Texto;
                if (t.ContainsKey("msg.error.inesperado"))  mensaje = t["msg.error.inesperado"].Texto;
            }
            catch { /* sin i18n disponible: se usa el texto por defecto */ }

            MessageBox.Show(mensaje, titulo, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
