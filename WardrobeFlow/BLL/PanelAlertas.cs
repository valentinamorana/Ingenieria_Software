using System.Collections.Generic;
using System.Linq;

namespace BLL
{
    /// <summary>
    /// Reúne en un solo lugar las alertas operativas del sistema: suscripciones por
    /// vencer/vencidas, antigüedad del último backup, prendas trabadas en limpieza e
    /// integridad de datos (DV). Toda la lógica de detección vive acá (capa BLL); la
    /// GUI solo lista las <see cref="BE.Alerta"/> resultantes y las traduce.
    ///
    /// Cada chequeo está aislado en su try: si una fuente falla, no tumba al resto.
    /// </summary>
    public class PanelAlertas
    {
        private readonly Cliente        _cliente = new Cliente();
        private readonly Prenda         _prenda  = new Prenda();
        private readonly ReporteJornada _reporte = new ReporteJornada();

        public List<BE.Alerta> ObtenerAlertas()
        {
            // Recolección de métricas desde las fuentes (cada una aislada en su try para
            // que una caída no tumbe al resto). Desconocido = métrica no disponible (se ignora).
            int vencidas = Desconocido, porVencer = Desconocido, diasSinBackup = Desconocido,
                enLimpieza = Desconocido, dvRotas = Desconocido;

            try
            {
                var clientes = _cliente.ObtenerTodos();
                porVencer = clientes.Count(c => c.SuscripcionProximaAVencer(7));
                vencidas  = clientes.Count(c => c.VencimientoExpirado);
            }
            catch { }

            try { diasSinBackup = _reporte.ObtenerDiasSinBackup(); } catch { }

            try
            {
                enLimpieza = _prenda.ObtenerTodos()
                    .Count(p => p.Estado == BE.EstadoPrenda.EnLimpieza);
            }
            catch { }

            try
            {
                var diag = Configuracion.ObtenerDiagnostico();
                dvRotas = diag.Integro ? 0 : diag.FilasRotas.Count;
            }
            catch { }

            return EvaluarAlertas(vencidas, porVencer, diasSinBackup, enLimpieza, dvRotas);
        }

        /// <summary>Centinela: la métrica no pudo obtenerse (fuente caída) → se ignora.</summary>
        public const int Desconocido = int.MinValue;

        /// <summary>
        /// NÚCLEO PURO de las reglas de alerta: dadas las métricas ya recolectadas, decide
        /// qué alertas emitir y con qué severidad. Sin acceso a datos → 100% testeable.
        /// Una métrica en <see cref="Desconocido"/> se ignora. Para el backup, un valor
        /// negativo (pero conocido) significa "no hay backups registrados".
        /// </summary>
        public static List<BE.Alerta> EvaluarAlertas(int vencidas, int porVencer,
            int diasSinBackup, int enLimpieza, int dvRotas)
        {
            var alertas = new List<BE.Alerta>();

            // 1) Suscripciones vencidas / por vencer
            if (vencidas > 0)
                alertas.Add(new BE.Alerta(BE.NivelAlerta.Critica, "alert.subs.vencidas",
                    "{0} suscripción(es) vencida(s).", vencidas, vencidas));
            if (porVencer > 0)
                alertas.Add(new BE.Alerta(BE.NivelAlerta.Advertencia, "alert.subs.porvencer",
                    "{0} suscripción(es) vence(n) en los próximos 7 días.", porVencer, porVencer));

            // 2) Backup: Desconocido = se ignora; negativo conocido = no hay backups; >=7 = aviso
            if (diasSinBackup != Desconocido)
            {
                if (diasSinBackup < 0)
                    alertas.Add(new BE.Alerta(BE.NivelAlerta.Critica, "alert.backup.nunca",
                        "No hay backups registrados.", 0));
                else if (diasSinBackup >= 7)
                    alertas.Add(new BE.Alerta(BE.NivelAlerta.Advertencia, "alert.backup.dias",
                        "Hace {0} día(s) que no se realiza un backup.", diasSinBackup, diasSinBackup));
            }

            // 3) Prendas trabadas en limpieza
            if (enLimpieza > 0)
                alertas.Add(new BE.Alerta(BE.NivelAlerta.Info, "alert.prendas.limpieza",
                    "{0} prenda(s) en limpieza.", enLimpieza, enLimpieza));

            // 4) Integridad de datos
            if (dvRotas > 0)
                alertas.Add(new BE.Alerta(BE.NivelAlerta.Critica, "alert.dv.corruptos",
                    "Integridad comprometida: {0} fila(s) con DV inválido.", dvRotas, dvRotas));

            return alertas;
        }

        /// <summary>Cantidad total de alertas activas (para el badge del menú).</summary>
        public int Contar()
        {
            return ObtenerAlertas().Count;
        }
    }
}
