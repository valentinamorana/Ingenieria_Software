using System;
using System.Collections.Generic;
using System.Text;

namespace BLL
{
    // Resultado estructurado para RestauracionForm — permite formatear el mensaje en cualquier idioma.
    public class ResultadoIntegridad
    {
        public List<string> FilasCorruptas { get; set; } = new List<string>();
        public int? DvvAlmacenado  { get; set; }
        public int  DvvCalculado   { get; set; }
        public bool HayDvhInvalido { get; set; }
        public bool HayDvvInvalido { get; set; }
        public string ErrorTecnico { get; set; }

        // Fallback en español para la sobrecarga legacy (out string).
        public string MensajeES
        {
            get
            {
                if (ErrorTecnico != null) return $"Advertencia al verificar integridad DV:\n{ErrorTecnico}";
                var sb = new StringBuilder();
                sb.AppendLine("ALERTA DE INTEGRIDAD — Tabla Usuario");
                sb.AppendLine(new string('─', 50));
                sb.AppendLine();
                if (HayDvhInvalido)
                {
                    sb.AppendLine($"Se detectaron {FilasCorruptas.Count} fila(s) con DVH inválido:");
                    foreach (var f in FilasCorruptas) sb.AppendLine($"  • Usuario {f}");
                    sb.AppendLine();
                }
                if (HayDvvInvalido)
                {
                    sb.AppendLine("El DVV de la tabla no coincide con el valor almacenado.");
                    sb.AppendLine($"  Almacenado: {(DvvAlmacenado?.ToString() ?? "—")}  |  Calculado: {DvvCalculado}");
                    sb.AppendLine();
                }
                sb.AppendLine("Posibles causas: modificación directa en la base de datos,");
                sb.AppendLine("restauración parcial de backup o error en la migración.");
                sb.AppendLine();
                sb.AppendLine("Para restaurar la integridad, un Administrador debe:");
                sb.AppendLine("  1. Revisar los registros alterados en SQL Server.");
                sb.AppendLine("  2. Corregir los valores afectados manualmente.");
                sb.AppendLine("  3. Ejecutar el recálculo de DVH/DVV desde Administrar → Usuarios.");
                return sb.ToString();
            }
        }
    }

    // Resultado para diagnóstico granular (ObtenerDiagnostico / RepararFilas).
    public class ResultadoDiagnostico
    {
        public bool   Integro          { get; set; }
        public int?   DVVAlmacenado    { get; set; }
        public int    DVVCalculado     { get; set; }
        public List<DAL.FilaUsuarioDV> FilasRotas { get; set; } = new List<DAL.FilaUsuarioDV>();
    }

    /// <summary>
    /// Capa de Lógica de Negocio — Configuración del Sistema.
    ///
    /// Responsabilidades de arranque (Program.Main):
    ///   1. VerificarConexionDAL()  — confirma que SQL Server responde antes del Login.
    ///   2. VerificarIntegridadDV() — T07: controla DVH/DVV de la tabla Usuario.
    ///      Retorna false si detecta manipulación externa; Program bloquea el Login.
    /// </summary>
    public class Configuracion
    {
        /// <summary>
        /// Verifica la conexión a SQL Server usando DAL.Acceso.VerificarConexion().
        /// Retorna false y un mensaje de error si la conexión falla.
        /// Se invoca desde Program.Main() antes de mostrar cualquier formulario.
        /// </summary>
        public static bool VerificarConexionDAL(out string mensajeError)
        {
            mensajeError = null;
            try
            {
                bool ok = DAL.Acceso.GetInstance().VerificarConexion();

                if (!ok)
                {
                    mensajeError = "No se pudo conectar a la base de datos.\nVerifique que SQL Server esté en ejecución.";
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                mensajeError = $"Error al inicializar la conexión:\n{ex.Message}\n\nVerifique la cadena de conexión en App.config.";
                return false;
            }
        }

        /// <summary>
        /// T07 — Verifica la integridad de la tabla Usuario mediante DVH y DVV.
        /// Sobrecarga legacy: devuelve el mensaje de error en español para compatibilidad.
        /// </summary>
        public static bool VerificarIntegridadDV(out string mensajeError)
        {
            bool ok = VerificarIntegridadDV(out ResultadoIntegridad r);
            mensajeError = r?.MensajeES;
            return ok;
        }

        /// <summary>
        /// T07 — Verifica la integridad de la tabla Usuario mediante DVH y DVV.
        /// Devuelve datos estructurados para que RestauracionForm formatee el mensaje en el idioma activo.
        /// </summary>
        public static bool VerificarIntegridadDV(out ResultadoIntegridad resultado)
        {
            resultado = null;
            try
            {
                var dvDAL = new DAL.DigitoVerificador();
                var svc   = new Seguridad.DigitoVerificador();
                var filas = dvDAL.ObtenerFilasUsuario();

                if (filas.Count == 0) return true;

                // Primer arranque sin DVH: todos null o cero + sin DVV → recalcular.
                bool todosEnCero = true;
                foreach (var f in filas)
                    if (f.DVHAlmacenado != null && f.DVHAlmacenado != 0) { todosEnCero = false; break; }

                int? dvvIni = dvDAL.ObtenerDVV("Usuario");
                if (todosEnCero && (dvvIni == null || dvvIni == 0))
                {
                    RecalcularTodoDV(dvDAL, svc, filas);
                    return true;
                }

                // Migración de algoritmo: si todos los DVH almacenados son < 10
                // (valores del algoritmo anterior mod 10), recalcular automáticamente
                // con el nuevo algoritmo en lugar de bloquear el login.
                bool todosConAlgoritmoAntiguo = true;
                foreach (var f in filas)
                {
                    if (f.DVHAlmacenado == null || f.DVHAlmacenado >= 10)
                    {
                        todosConAlgoritmoAntiguo = false;
                        break;
                    }
                }
                if (todosConAlgoritmoAntiguo)
                {
                    System.Diagnostics.Trace.TraceInformation(
                        "[Configuracion] Detectados DVH del algoritmo anterior (mod 10). " +
                        "Recalculando con nuevo algoritmo (mod 999.983)...");
                    RecalcularTodoDV(dvDAL, svc, filas);
                    return true;
                }

                var dvhsRecalculados = new List<int>();
                var filasCorruptas   = new List<string>();

                foreach (var fila in filas)
                {
                    int dvhCalculado = svc.CalcularDVH(
                        fila.Id.ToString(), fila.Username, fila.Clave,
                        fila.Perfil, fila.Estado, fila.IntentosFallidos);
                    dvhsRecalculados.Add(dvhCalculado);
                    if (fila.DVHAlmacenado == null || fila.DVHAlmacenado != dvhCalculado)
                        filasCorruptas.Add($"'{fila.Username}' (ID {fila.Id})");
                }

                int  dvvCalculado  = svc.CalcularDVV(dvhsRecalculados);
                int? dvvAlmacenado = dvDAL.ObtenerDVV("Usuario");

                bool dvhOk = filasCorruptas.Count == 0;
                bool dvvOk = dvvAlmacenado != null && dvvAlmacenado == dvvCalculado;

                if (dvhOk && dvvOk)
                {
                    LogearVerificacion("Usuario", dvvAlmacenado, dvvCalculado, true, 0, "Arranque");
                    return true;
                }

                resultado = new ResultadoIntegridad
                {
                    FilasCorruptas = filasCorruptas,
                    DvvAlmacenado  = dvvAlmacenado,
                    DvvCalculado   = dvvCalculado,
                    HayDvhInvalido = !dvhOk,
                    HayDvvInvalido = !dvvOk
                };
                LogearVerificacion("Usuario", dvvAlmacenado, dvvCalculado, false, filasCorruptas.Count, "Arranque");
                return false;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("DVH") || ex.InnerException?.Message.Contains("DVH") == true)
                    return true;
                resultado = new ResultadoIntegridad { ErrorTecnico = ex.Message };
                return true;
            }
        }

        // Garantiza que exista al menos un segundo Administrador ("admin2") en la BD.
        // Se llama al arrancar la app, antes del Login, para que si admin1 queda bloqueado
        // siempre haya otro admin que pueda desbloquearlo.
        // Retorna la ruta del archivo de credenciales si admin2 se creó en esta ejecución,
        // o null si ya existía (no hace nada).
        public static string SeedAdminSecundario()
        {
            const string Username = "admin2";
            const string Perfil   = "Administrador";

            try
            {
                var usuarioDAL = new DAL.Usuario();
                if (usuarioDAL.ObtenerPorUsername(Username) != null)
                    return null;

                string contrasena    = Servicios.GeneradorCredenciales.GenerarContrasena();
                string claveHasheada = Seguridad.Encriptador.Hash(contrasena);
                usuarioDAL.Alta(Username, claveHasheada, Perfil);

                return Servicios.GeneradorCredenciales.ExportarCredenciales(Username, contrasena);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"[Configuracion.SeedAdminSecundario] {ex.Message}");
                return null;
            }
        }

        // ── Métodos de diagnóstico y reparación granular ──────────────────────

        public static ResultadoDiagnostico ObtenerDiagnostico()
        {
            var dvDAL = new DAL.DigitoVerificador();
            var svc   = new Seguridad.DigitoVerificador();
            var filas = dvDAL.ObtenerFilasUsuario();

            var rotas      = new List<DAL.FilaUsuarioDV>();
            var dvhsRecalc = new List<int>();

            foreach (var fila in filas)
            {
                int dvhCalc = svc.CalcularDVH(
                    fila.Id.ToString(), fila.Username, fila.Clave,
                    fila.Perfil, fila.Estado, fila.IntentosFallidos);

                dvhsRecalc.Add(dvhCalc);

                if (fila.DVHAlmacenado == null || fila.DVHAlmacenado != dvhCalc)
                    rotas.Add(fila);
            }

            int  dvvCalculado  = svc.CalcularDVV(dvhsRecalc);
            int? dvvAlmacenado = dvDAL.ObtenerDVV("Usuario");

            return new ResultadoDiagnostico
            {
                Integro       = rotas.Count == 0 && dvvAlmacenado != null && dvvAlmacenado == dvvCalculado,
                DVVAlmacenado = dvvAlmacenado,
                DVVCalculado  = dvvCalculado,
                FilasRotas    = rotas
            };
        }

        public static void RepararFilas(IEnumerable<int> ids)
        {
            var dvDAL  = new DAL.DigitoVerificador();
            var svc    = new Seguridad.DigitoVerificador();
            var todas  = dvDAL.ObtenerFilasUsuario();
            var idsSet = new HashSet<int>(ids);

            foreach (var fila in todas)
            {
                if (!idsSet.Contains(fila.Id)) continue;
                int dvh = svc.CalcularDVH(
                    fila.Id.ToString(), fila.Username, fila.Clave,
                    fila.Perfil, fila.Estado, fila.IntentosFallidos);
                dvDAL.ActualizarDVH(fila.Id, dvh);
            }

            var todasActualizadas = dvDAL.ObtenerFilasUsuario();
            var dvhValues = new List<int>();
            foreach (var f in todasActualizadas)
                dvhValues.Add(svc.CalcularDVH(f.Id.ToString(), f.Username, f.Clave, f.Perfil, f.Estado, f.IntentosFallidos));
            dvDAL.GuardarDVV("Usuario", svc.CalcularDVV(dvhValues));
        }

        // Recalcula y persiste DVH de cada fila de Usuario y el DVV de la tabla.
        // Llamado por el Administrador desde Usuarios → "Recalcular DV".
        public static void RecalcularIntegridadDV()
        {
            var dvDAL = new DAL.DigitoVerificador();
            var svc   = new Seguridad.DigitoVerificador();
            var filas = dvDAL.ObtenerFilasUsuario();
            RecalcularTodoDV(dvDAL, svc, filas);
        }

        // Helper compartido entre VerificarIntegridadDV (primer arranque) y RecalcularIntegridadDV.
        private static void RecalcularTodoDV(DAL.DigitoVerificador dvDAL,
                                              Seguridad.DigitoVerificador svc,
                                              List<DAL.FilaUsuarioDV> filas)
        {
            var dvhValues = new List<int>();
            foreach (var fila in filas)
            {
                int dvh = svc.CalcularDVH(
                    fila.Id.ToString(), fila.Username, fila.Clave,
                    fila.Perfil, fila.Estado, fila.IntentosFallidos);
                dvDAL.ActualizarDVH(fila.Id, dvh);
                dvhValues.Add(dvh);
            }
            int dvv = svc.CalcularDVV(dvhValues);
            dvDAL.GuardarDVV("Usuario", dvv);
        }

        // Devuelve los últimos N registros del historial de verificaciones DV.
        // Encapsula el acceso a DAL para que la GUI no dependa de DAL.HistorialIntegridad.
        public static List<BE.HistorialIntegridad> ObtenerHistorialIntegridad(int n)
        {
            return new DAL.HistorialIntegridad().ObtenerUltimos(n);
        }

        // Registra una verificación periódica (Timer del Menu) en el historial.
        // Centraliza el acceso a DAL para que Menu.cs no dependa de DAL directamente.
        public static void RegistrarVerificacionPeriodica(ResultadoDiagnostico diag)
        {
            try
            {
                new DAL.HistorialIntegridad().Insertar(new BE.HistorialIntegridad
                {
                    NombreTabla    = "Usuario",
                    DVVAlmacenado  = diag.DVVAlmacenado,
                    DVVCalculado   = diag.DVVCalculado,
                    Resultado      = diag.Integro,
                    FilasCorruptas = diag.FilasRotas.Count,
                    DisparadoPor   = "Timer"
                });
            }
            catch { /* tabla aún no existe */ }
        }

        // Registra silenciosamente cada verificación en HistorialIntegridad.
        // Falla silenciosamente si la tabla aún no existe (antes de la migración).
        private static void LogearVerificacion(string tabla, int? dvvAlm, int dvvCalc, bool resultado, int filasRotas, string origen)
        {
            try
            {
                new DAL.HistorialIntegridad().Insertar(new BE.HistorialIntegridad
                {
                    NombreTabla    = tabla,
                    DVVAlmacenado  = dvvAlm,
                    DVVCalculado   = dvvCalc,
                    Resultado      = resultado,
                    FilasCorruptas = filasRotas,
                    DisparadoPor   = origen
                });
            }
            catch { /* tabla aún no existe — ignorar */ }
        }
    }
}
