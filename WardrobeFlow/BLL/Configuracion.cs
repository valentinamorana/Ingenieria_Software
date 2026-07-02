using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BLL
{
    // Resultado estructurado de la verificación de integridad — permite formatear el mensaje en cualquier idioma.
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

    // Resultado para diagnóstico granular (ObtenerDiagnostico).
    public class ResultadoDiagnostico
    {
        public bool   Integro          { get; set; }
        public int?   DVVAlmacenado    { get; set; }
        public int    DVVCalculado     { get; set; }
        public List<BE.FilaUsuarioDV> FilasRotas { get; set; } = new List<BE.FilaUsuarioDV>();
        // Tablas adicionales protegidas (Cliente, Empleado, Pedido) cuyo DV no coincide.
        // Antes el diagnóstico solo miraba Usuario, así que una corrupción en Cliente
        // nunca aparecía ni habilitaba "Recalcular Todo".
        public List<string> TablasAdicionalesCorruptas { get; set; } = new List<string>();
    }

    /// <summary>
    /// Capa de Lógica de Negocio — Configuración del Sistema.
    ///
    /// Responsabilidades de arranque (Program.Main):
    ///   1. VerificarConexionDAL()  — confirma que SQL Server responde antes del Login.
    ///   2. VerificarIntegridadDV() — T07: controla DVH/DVV de la tabla Usuario. Se ejecuta
    ///      ANTES de mostrar la ventana de Login (requisito de cátedra). Retorna false si detecta
    ///      manipulación externa; Program deja constancia en bitácora y, tras autenticarse,
    ///      reserva el detalle y la reparación al Administrador.
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

        // T07 — Versión del FORMATO del DVH de Usuario. Se incrementa cuando cambian los
        // campos que entran al cálculo (v2 = se agregó Rol). Se persiste como marcador en
        // DVVertical para poder migrar UNA vez (recalcular) en bases existentes sin bloquear.
        private const string FormatoDVUsuarioMarcador = "__FormatoDVUsuario__";
        private const int    FormatoDVUsuarioActual   = 2;

        /// <summary>
        /// T07 — Verifica la integridad de la tabla Usuario mediante DVH y DVV.
        /// Devuelve datos estructurados para que la consola de recuperación formatee el mensaje.
        /// </summary>
        public static bool VerificarIntegridadDV(out ResultadoIntegridad resultado)
        {
            resultado = null;
            try
            {
                var dvDAL = new DAL.DigitoVerificador();
                var svc   = Seguridad.CalculadorDV.Crear();
                var filas = dvDAL.ObtenerFilasUsuario();

                if (filas.Count == 0) return VerificarTablasAdicionales(out resultado);

                // Primer arranque sin DVH: todos null o cero + sin DVV → recalcular.
                bool todosEnCero = true;
                foreach (var f in filas)
                    if (f.DVHAlmacenado != null && f.DVHAlmacenado != 0) { todosEnCero = false; break; }

                int? dvvIni = dvDAL.ObtenerDVV("Usuario");
                if (todosEnCero && (dvvIni == null || dvvIni == 0))
                {
                    RecalcularTodoDV(dvDAL, svc, filas);
                    int? dvvInicializado = dvDAL.ObtenerDVV("Usuario");
                    LogearVerificacion("Usuario", dvvInicializado, dvvInicializado ?? 0, true, 0, "Arranque");
                    return VerificarTablasAdicionales(out resultado);
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
                    int? dvvMigAlg = dvDAL.ObtenerDVV("Usuario");
                    LogearVerificacion("Usuario", dvvMigAlg, dvvMigAlg ?? 0, true, 0, "Arranque");
                    return VerificarTablasAdicionales(out resultado);
                }

                // Migración de FORMATO del DVH (v2: el cálculo ahora incluye Rol). Si la base
                // trae un formato anterior — o sin marcador —, se recalcula UNA sola vez en
                // lugar de bloquear, y se sella el formato nuevo (RecalcularTodoDV graba el
                // marcador). Misma estrategia que la migración de algoritmo de arriba. A partir
                // de acá, manipular el Rol en BD queda detectado por la verificación de integridad.
                int? formatoDV = dvDAL.ObtenerDVV(FormatoDVUsuarioMarcador);
                if (formatoDV == null || formatoDV < FormatoDVUsuarioActual)
                {
                    System.Diagnostics.Trace.TraceInformation(
                        $"[Configuracion] Migrando formato del DVH de Usuario a v{FormatoDVUsuarioActual} " +
                        "(ahora incluye Rol). Recalculando una vez...");
                    RecalcularTodoDV(dvDAL, svc, filas);
                    int? dvvMigFmt = dvDAL.ObtenerDVV("Usuario");
                    LogearVerificacion("Usuario", dvvMigFmt, dvvMigFmt ?? 0, true, 0, "Arranque");
                    return VerificarTablasAdicionales(out resultado);
                }

                var dvhsRecalculados = new List<int>();
                var filasCorruptas   = new List<string>();

                foreach (var fila in filas)
                {
                    int dvhCalculado = svc.CalcularDVH(fila.CamposParaDVH());
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
                    // Base sana ya migrada pero sin espejo todavía → sembrarlo desde estas filas íntegras.
                    SeedEspejoSiVacio(filas);
                    return VerificarTablasAdicionales(out resultado);
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
                System.Diagnostics.Trace.TraceError($"[Configuracion.VerificarIntegridadDV] {ex.Message}");

                // Tolerancia: la columna DVH / la tabla DVVertical aún no existen (BD sin
                // migrar) → no hay integridad que verificar todavía, no se bloquea.
                string msg = (ex.Message ?? "") + " " + (ex.InnerException?.Message ?? "");
                if (msg.Contains("DVH") || msg.Contains("DVVertical"))
                    return true;

                // FAIL-SAFE: ante cualquier otro error NO se asume integridad. Se bloquea el
                // acceso y se informa al administrador (la consola de recuperación muestra el detalle).
                resultado = new ResultadoIntegridad
                {
                    HayDvhInvalido = true,
                    FilasCorruptas = new List<string> { "Error técnico al verificar la integridad: " + ex.Message },
                    DvvAlmacenado  = null,
                    DvvCalculado   = 0
                };
                return false;
            }
        }

        /// <summary>
        /// T07 — Asegura la integridad de la tabla Usuario ANTES de una operación sensible
        /// (alta/reset/desbloqueo de usuarios). Lanza AppException si la base fue manipulada,
        /// de modo que la operación no se ejecute sobre datos corruptos.
        /// </summary>
        public static void AsegurarIntegridadUsuarios()
        {
            if (!VerificarIntegridadDV(out ResultadoIntegridad _))
                throw new BE.AppException("err.bll.integridad",
                    "Operación cancelada: se detectó una posible manipulación de los datos de usuarios " +
                    "(dígito verificador inválido). Reiniciá el sistema para reparar la integridad antes de continuar.");
        }

        // Garantiza que exista al menos un segundo Administrador ("admin2") en la BD.
        // Se llama al arrancar la app, antes del Login, para que si admin1 queda bloqueado
        // siempre haya otro admin que pueda desbloquearlo.
        // Retorna la ruta del archivo de credenciales si admin2 se creó en esta ejecución,
        // o null si ya existía (no hace nada).
        public static string SeedAdminSecundario()
        {
            const string Username = "admin2";
            const string Perfil   = BE.Roles.Administrador;

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

        // RF-10 — Genera el set inicial de claves de emergencia (10) si todavía no existe ninguna.
        // Se llama al arrancar, antes del Login. Devuelve la ruta del .txt si se generaron en esta
        // ejecución, o null si ya existían (no hace nada) o si la tabla aún no está migrada.
        public static string SeedClavesEmergencia()
        {
            try
            {
                var dal = new DAL.ClaveRecuperacion();
                if (dal.ContarTotal() > 0) return null;   // ya hay un set cargado
                return RecuperacionAdmin.GenerarClavesEmergencia(10);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"[Configuracion.SeedClavesEmergencia] {ex.Message}");
                return null;
            }
        }

        // ── Métodos de diagnóstico y reparación granular ──────────────────────

        public static ResultadoDiagnostico ObtenerDiagnostico()
        {
            var dvDAL = new DAL.DigitoVerificador();
            var svc   = Seguridad.CalculadorDV.Crear();
            var filas = dvDAL.ObtenerFilasUsuario();

            var rotas      = new List<BE.FilaUsuarioDV>();
            var dvhsRecalc = new List<int>();

            foreach (var fila in filas)
            {
                int dvhCalc = svc.CalcularDVH(fila.CamposParaDVH());

                dvhsRecalc.Add(dvhCalc);

                if (fila.DVHAlmacenado == null || fila.DVHAlmacenado != dvhCalc)
                    rotas.Add(fila);
            }

            int  dvvCalculado  = svc.CalcularDVV(dvhsRecalc);
            int? dvvAlmacenado = dvDAL.ObtenerDVV("Usuario");

            // También diagnosticar Cliente/Empleado/Pedido (solo lectura, sin efectos),
            // para que una corrupción ahí marque el estado y habilite "Recalcular Todo".
            var adicionales = DiagnosticarTablasAdicionales();

            bool usuarioOk = rotas.Count == 0 && dvvAlmacenado != null && dvvAlmacenado == dvvCalculado;
            return new ResultadoDiagnostico
            {
                Integro       = usuarioOk && adicionales.Count == 0,
                DVVAlmacenado = dvvAlmacenado,
                DVVCalculado  = dvvCalculado,
                FilasRotas    = rotas,
                TablasAdicionalesCorruptas = adicionales
            };
        }

        // Verificación SOLO LECTURA (sin inicializar ni loguear) de las tablas adicionales
        // protegidas con DV. Devuelve los nombres de las que tienen DVH/DVV inválido.
        private static List<string> DiagnosticarTablasAdicionales()
        {
            var corruptas = new List<string>();
            var dvDAL = new DAL.DigitoVerificador();
            var svc   = Seguridad.CalculadorDV.Crear();
            var pedidoDAL = new DAL.Pedido();

            VerificarTablaSoloLectura(corruptas, dvDAL, svc, DAL.Cliente.DV_Tabla,
                () => dvDAL.ObtenerFilas(DAL.Cliente.DV_Tabla, DAL.Cliente.DV_Pk, DAL.Cliente.DV_Columnas));
            VerificarTablaSoloLectura(corruptas, dvDAL, svc, DAL.Empleado.DV_Tabla,
                () => dvDAL.ObtenerFilas(DAL.Empleado.DV_Tabla, DAL.Empleado.DV_Pk, DAL.Empleado.DV_Columnas));
            VerificarTablaSoloLectura(corruptas, dvDAL, svc, DAL.Pedido.DV_Tabla,
                () => pedidoDAL.ObtenerFilasDV());

            return corruptas;
        }

        private static void VerificarTablaSoloLectura(List<string> corruptas, DAL.DigitoVerificador dvDAL,
            Seguridad.ICalculadorDV svc, string tabla, Func<List<BE.FilaDV>> obtenerFilas)
        {
            List<BE.FilaDV> filas;
            try { filas = obtenerFilas(); } catch { return; }   // tabla sin migrar → no se evalúa
            if (filas.Count == 0) return;

            int? dvvAlm = dvDAL.ObtenerDVV(tabla);
            // Primer arranque sin DV (todo en null/0) → no es corrupción.
            bool todosNull = filas.TrueForAll(f => f.DVHAlmacenado == null || f.DVHAlmacenado == 0);
            if (todosNull && (dvvAlm == null || dvvAlm == 0)) return;

            var dvhs = new List<int>();
            bool rota = false;
            foreach (var f in filas)
            {
                int calc = svc.CalcularDVH(f.Campos);
                dvhs.Add(calc);
                if (f.DVHAlmacenado == null || f.DVHAlmacenado != calc) rota = true;
            }
            if (dvvAlm == null || dvvAlm != svc.CalcularDVV(dvhs)) rota = true;
            if (rota) corruptas.Add(tabla);
        }

        // #5 — Guard fail-closed para operaciones sobre Dígitos Verificadores.
        // Si hay una sesión iniciada, EXIGE que sea Administrador: un usuario autenticado sin
        // permiso queda BLOQUEADO, el intento se REGISTRA en bitácora (criticidad Alta, visible
        // para el administrador) y se lanza una AppException con mensaje genérico. Si NO hay sesión,
        // es el flujo de reparación de ARRANQUE (break-glass), ya autorizado por ConfirmarAdminForm
        // / Clave Maestra antes de llegar acá, por lo que se permite.
        private static void ExigirAdminParaDV(string operacion)
        {
            if (!Seguridad.SessionManager.IsLoggedIn) return;   // break-glass de arranque

            var u = Seguridad.SessionManager.GetInstance().Usuario;
            if (u != null && u.EsAdministrador) return;

            // Acceso no autorizado: registrar el evento para que el administrador lo vea.
            try
            {
                new Servicios.Bitacora().RegistrarSinSesion(
                    modulo:     "Integridad de Datos",
                    actividad:  "Acceso DENEGADO a Dígitos Verificadores",
                    criticidad: BE.Criticidad.Alta,
                    idUsuario:  u?.Id,
                    detalle:    $"El usuario '{u?.Username ?? "?"}' (rol '{u?.Perfil ?? "?"}') intentó ejecutar " +
                                $"'{operacion}' sin permiso de Administrador a las {DateTime.Now:HH:mm:ss}.");
            }
            catch { /* el fallo del log no debe ocultar el bloqueo */ }

            throw new BE.AppException("err.bll.dv.sin_permiso",
                "No tenés permiso para ejecutar operaciones sobre los Dígitos Verificadores. " +
                "Esta acción es exclusiva del Administrador y quedó registrada.");
        }

        // Recalcula y persiste DVH de cada fila de Usuario y el DVV de la tabla.
        // Llamado por el Administrador desde Diagnóstico de Integridad → "Recalcular DV".
        public static void RecalcularIntegridadDV()
        {
            ExigirAdminParaDV("Recalcular DV");
            var dvDAL = new DAL.DigitoVerificador();
            var svc   = Seguridad.CalculadorDV.Crear();
            var filas = dvDAL.ObtenerFilasUsuario();
            RecalcularTodoDV(dvDAL, svc, filas);

            // T07 — Tablas adicionales protegidas con DV.
            dvDAL.RecalcularTabla(DAL.Cliente.DV_Tabla,  DAL.Cliente.DV_Pk,  DAL.Cliente.DV_Columnas);
            dvDAL.RecalcularTabla(DAL.Empleado.DV_Tabla, DAL.Empleado.DV_Pk, DAL.Empleado.DV_Columnas);
            new DAL.Pedido().RecalcularDV();   // objeto multi-tabla (pedido + líneas)

            // Trazabilidad: dejar constancia en bitácora del recálculo de dígitos verificadores.
            // Se usa RegistrarSinSesion porque esta operación también puede dispararse desde el
            // form de integridad en el ARRANQUE, antes de que haya una sesión iniciada.
            int? idActor = Seguridad.SessionManager.IsLoggedIn
                           ? (int?)Seguridad.SessionManager.GetInstance().Usuario.Id : null;
            string actor = Seguridad.SessionManager.IsLoggedIn
                           ? Seguridad.SessionManager.GetInstance().Usuario.Username : "(arranque/sin sesión)";
            new Servicios.Bitacora().RegistrarSinSesion(
                modulo:     "Integridad de Datos",
                actividad:  "Recálculo de Dígitos Verificadores",
                criticidad: BE.Criticidad.Alta,
                idUsuario:  idActor,
                detalle:    $"{actor} ejecutó el recálculo de DVH/DVV (Usuario, Cliente, Empleado, Pedido) a las {DateTime.Now:HH:mm:ss}.");
        }

        // Helper compartido entre VerificarIntegridadDV (primer arranque) y RecalcularIntegridadDV.
        private static void RecalcularTodoDV(DAL.DigitoVerificador dvDAL,
                                              Seguridad.ICalculadorDV svc,
                                              List<BE.FilaUsuarioDV> filas)
        {
            var dvhValues = new List<int>();
            foreach (var fila in filas)
            {
                int dvh = svc.CalcularDVH(fila.CamposParaDVH());
                dvDAL.ActualizarDVH(fila.Id, dvh);
                fila.DVHAlmacenado = dvh;
                dvhValues.Add(dvh);
            }
            int dvv = svc.CalcularDVV(dvhValues);
            dvDAL.GuardarDVV("Usuario", dvv);
            // Sellar el formato vigente del DVH: marca que estas filas se calcularon con la
            // fórmula actual (incluye Rol), para que la migración no vuelva a dispararse.
            dvDAL.GuardarDVV(FormatoDVUsuarioMarcador, FormatoDVUsuarioActual);
            // T07 — Reconstruir el espejo de integridad para que refleje el estado recién
            // aceptado como legítimo (primer arranque, migración o "Asumir pérdida"/"Recalcular Todo").
            new DAL.EspejoUsuario().Reconstruir(filas);
        }

        // T07 — Recalcula DVH/DVV SOLO de la tabla Usuario y reconstruye su espejo de integridad.
        // Lo usa la recuperación asistida tras restaurar valores desde el espejo (no toca las demás
        // tablas protegidas, ya verificadas aparte). Exige permiso de Administrador.
        public static void RecalcularUsuario()
        {
            ExigirAdminParaDV("Recalcular Usuario (DV)");
            var dvDAL = new DAL.DigitoVerificador();
            var svc   = Seguridad.CalculadorDV.Crear();
            RecalcularTodoDV(dvDAL, svc, dvDAL.ObtenerFilasUsuario());
        }

        // Expone el guard de autorización de DV para la recuperación asistida (mismo fail-closed:
        // con sesión exige Administrador y registra el intento; sin sesión es break-glass de arranque).
        public static void ExigirAdminDV(string operacion) => ExigirAdminParaDV(operacion);

        // T07 — Siembra el espejo de integridad SOLO si está vacío y la tabla existe, a partir de
        // filas ya verificadas como íntegras. Permite que bases sanas ya migradas obtengan su espejo
        // sin forzar un recálculo. Nunca siembra desde datos corruptos (se llama solo en el camino OK).
        private static void SeedEspejoSiVacio(List<BE.FilaUsuarioDV> filas)
        {
            try
            {
                var esp = new DAL.EspejoUsuario();
                if (!esp.Existe()) return;
                if (filas != null && filas.Count > 0 && esp.ObtenerFilas().Count == 0)
                    esp.Reconstruir(filas);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"[Configuracion.SeedEspejoSiVacio] {ex.Message}");
            }
        }

        // ── DV en tablas ADICIONALES (Cliente, Empleado) — T07 ──────────────────

        // Verifica las tablas protegidas además de Usuario. Si alguna está corrupta,
        // arma el resultado y devuelve false; si están OK (o se inicializan en el primer
        // arranque), devuelve true. Las definiciones de columnas viven en cada DAL.
        private static bool VerificarTablasAdicionales(out ResultadoIntegridad resultado)
        {
            resultado = null;
            var dvDAL = new DAL.DigitoVerificador();
            var svc   = Seguridad.CalculadorDV.Crear();
            var corruptas = new List<string>();

            var pedidoDAL = new DAL.Pedido();
            VerificarUnaTabla(dvDAL, svc, DAL.Cliente.DV_Tabla,
                () => dvDAL.ObtenerFilas(DAL.Cliente.DV_Tabla, DAL.Cliente.DV_Pk, DAL.Cliente.DV_Columnas),
                () => dvDAL.RecalcularTabla(DAL.Cliente.DV_Tabla, DAL.Cliente.DV_Pk, DAL.Cliente.DV_Columnas), corruptas);
            VerificarUnaTabla(dvDAL, svc, DAL.Empleado.DV_Tabla,
                () => dvDAL.ObtenerFilas(DAL.Empleado.DV_Tabla, DAL.Empleado.DV_Pk, DAL.Empleado.DV_Columnas),
                () => dvDAL.RecalcularTabla(DAL.Empleado.DV_Tabla, DAL.Empleado.DV_Pk, DAL.Empleado.DV_Columnas), corruptas);
            // T07 — Pedido: objeto MULTI-TABLA (pedido + líneas PedidoPrenda).
            VerificarUnaTabla(dvDAL, svc, DAL.Pedido.DV_Tabla,
                () => pedidoDAL.ObtenerFilasDV(),
                () => pedidoDAL.RecalcularDV(), corruptas);

            if (corruptas.Count == 0) return true;

            resultado = new ResultadoIntegridad
            {
                HayDvhInvalido = true,
                FilasCorruptas = corruptas,
                DvvAlmacenado  = null,
                DvvCalculado   = 0
            };
            return false;
        }

        private static void VerificarUnaTabla(DAL.DigitoVerificador dvDAL, Seguridad.ICalculadorDV svc,
            string tabla, System.Func<List<BE.FilaDV>> obtenerFilas, System.Action recalcular, List<string> corruptas)
        {
            List<BE.FilaDV> filas;
            try { filas = obtenerFilas(); }
            catch { return; }   // tabla/columna DVH sin migrar → no se verifica
            if (filas.Count == 0) return;

            // Primer arranque: sin DVH ni DVV → inicializar (no es corrupción).
            bool todosNull = filas.TrueForAll(f => f.DVHAlmacenado == null || f.DVHAlmacenado == 0);
            int? dvvAlm = dvDAL.ObtenerDVV(tabla);
            if (todosNull && (dvvAlm == null || dvvAlm == 0))
            {
                recalcular();
                int? dvvNuevoTbl = dvDAL.ObtenerDVV(tabla);
                LogearVerificacion(tabla, dvvNuevoTbl, dvvNuevoTbl ?? 0, true, 0, "Arranque");
                return;
            }

            int antes = corruptas.Count;
            var dvhs  = new List<int>();
            foreach (var f in filas)
            {
                int calc = svc.CalcularDVH(f.Campos);
                dvhs.Add(calc);
                if (f.DVHAlmacenado == null || f.DVHAlmacenado != calc)
                    corruptas.Add(f.Descripcion + " (DVH)");
            }
            int dvvCalc = svc.CalcularDVV(dvhs);
            bool dvvOk  = dvvAlm != null && dvvAlm == dvvCalc;
            if (!dvvOk) corruptas.Add(tabla + " (DVV)");

            int rotasTabla = corruptas.Count - antes;
            LogearVerificacion(tabla, dvvAlm, dvvCalc, rotasTabla == 0, rotasTabla, "Arranque");
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

        // ── Recordatorio de backup ────────────────────────────────────────────

        private static readonly string RutaConfigRecordatorio =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups", "recordatorio.cfg");

        private const int DiasRecordatorioDefault = 7;

        public static int ObtenerDiasRecordatorio()
        {
            try
            {
                if (File.Exists(RutaConfigRecordatorio) &&
                    int.TryParse(File.ReadAllText(RutaConfigRecordatorio).Trim(), out int d) && d > 0)
                    return d;
            }
            catch (Exception ex) { System.Diagnostics.Trace.TraceError("[Configuracion.ObtenerDiasRecordatorio] " + ex.Message); }
            return DiasRecordatorioDefault;
        }

        public static void GuardarDiasRecordatorio(int dias)
        {
            try
            {
                string dir = Path.GetDirectoryName(RutaConfigRecordatorio);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(RutaConfigRecordatorio, dias.ToString());
            }
            catch (Exception ex) { System.Diagnostics.Trace.TraceError("[Configuracion.GuardarDiasRecordatorio] " + ex.Message); }
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
