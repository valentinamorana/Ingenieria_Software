using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace BLL
{
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
        /// Si falla, muestra un mensaje de error y termina el proceso.
        /// Se invoca desde Program.Main() antes de mostrar cualquier formulario.
        /// </summary>
        public static void VerificarConexionDAL()
        {
            try
            {
                bool ok = DAL.Acceso.GetInstance().VerificarConexion();

                if (!ok)
                {
                    MessageBox.Show(
                        "No se pudo conectar a la base de datos.\nVerifique que SQL Server esté en ejecución.",
                        "Error de Conexión",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    Environment.Exit(1);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al inicializar la conexión:\n{ex.Message}\n\nVerifique la cadena de conexión en App.config.",
                    "Error de Conexión",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// T07 — Verifica la integridad de la tabla Usuario mediante DVH y DVV.
        ///
        /// Algoritmo:
        ///   1. Lee todas las filas de Usuario con sus DVH almacenados.
        ///   2. Recalcula el DVH de cada fila y compara con el almacenado.
        ///      → Detecta alteraciones de campos individuales o permutas de valores.
        ///   3. Calcula el DVV sobre los DVH recalculados y compara con el DVV en DVVertical.
        ///      → Detecta inserciones, eliminaciones o permutas de filas completas.
        ///   4. Si hay discrepancias, muestra alerta y retorna false (el Login no se abre).
        ///
        /// Retorna true si la integridad es correcta.
        /// </summary>
        public static bool VerificarIntegridadDV()
        {
            try
            {
                var dvDAL = new DAL.DigitoVerificador();
                var svc   = new Servicios.DigitoVerificador();
                var filas = dvDAL.ObtenerFilasUsuario();

                if (filas.Count == 0) return true;  // tabla vacía — sin datos que verificar

                // Primer arranque tras la migración: todos los DVH están en 0.
                // Recalcular automáticamente y continuar.
                bool todosEnCero = true;
                foreach (var f in filas)
                    if (f.DVHAlmacenado != null && f.DVHAlmacenado != 0) { todosEnCero = false; break; }

                int? dvvIni = dvDAL.ObtenerDVV("Usuario");
                if (todosEnCero && (dvvIni == null || dvvIni == 0))
                {
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
                        filasCorruptas.Add($"  • Usuario '{fila.Username}' (ID {fila.Id})");
                }

                int dvvCalculado  = svc.CalcularDVV(dvhsRecalculados);
                int? dvvAlmacenado = dvDAL.ObtenerDVV("Usuario");

                bool dvhOk = filasCorruptas.Count == 0;
                bool dvvOk = dvvAlmacenado != null && dvvAlmacenado == dvvCalculado;

                if (dvhOk && dvvOk) return true;

                // Construir mensaje de alerta detallado
                var sb = new StringBuilder();
                sb.AppendLine("ALERTA DE INTEGRIDAD — Tabla Usuario");
                sb.AppendLine(new string('─', 50));
                sb.AppendLine();

                if (!dvhOk)
                {
                    sb.AppendLine($"Se detectaron {filasCorruptas.Count} fila(s) con DVH inválido:");
                    foreach (string linea in filasCorruptas)
                        sb.AppendLine(linea);
                    sb.AppendLine();
                }

                if (!dvvOk)
                {
                    sb.AppendLine("El DVV de la tabla no coincide con el valor almacenado.");
                    sb.AppendLine($"  Almacenado: {(dvvAlmacenado?.ToString() ?? "—")}  |  Calculado: {dvvCalculado}");
                    sb.AppendLine();
                }

                sb.AppendLine("Posibles causas: modificación directa en la base de datos,");
                sb.AppendLine("restauración parcial de backup o error en la migración.");
                sb.AppendLine();
                sb.AppendLine("Para restaurar la integridad, un Administrador debe:");
                sb.AppendLine("  1. Revisar los registros alterados en SQL Server.");
                sb.AppendLine("  2. Corregir los valores afectados manualmente.");
                sb.AppendLine("  3. Ejecutar el recálculo de DVH/DVV desde Administrar → Usuarios.");

                MessageBox.Show(
                    sb.ToString(),
                    "Integridad comprometida — Acceso bloqueado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return false;
            }
            catch (Exception ex)
            {
                // Si la columna DVH no existe aún (antes de aplicar el script v3.0), ignorar silenciosamente
                if (ex.Message.Contains("DVH") || ex.InnerException?.Message.Contains("DVH") == true)
                    return true;

                MessageBox.Show(
                    $"Error al verificar integridad DV:\n{ex.Message}",
                    "Error de Integridad",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return true;  // Error en el check: no bloquear (puede ser primer arranque)
            }
        }

        // Recalcula y persiste DVH de cada fila de Usuario y el DVV de la tabla.
        // Llamado por el Administrador desde Usuarios → "Recalcular DV".
        public static void RecalcularIntegridadDV()
        {
            var dvDAL = new DAL.DigitoVerificador();
            var svc   = new Servicios.DigitoVerificador();
            var filas = dvDAL.ObtenerFilasUsuario();
            RecalcularTodoDV(dvDAL, svc, filas);
        }

        // Helper compartido entre VerificarIntegridadDV (primer arranque) y RecalcularIntegridadDV.
        private static void RecalcularTodoDV(DAL.DigitoVerificador dvDAL,
                                              Servicios.DigitoVerificador svc,
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
    }
}
