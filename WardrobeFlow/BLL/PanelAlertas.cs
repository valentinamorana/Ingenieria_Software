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
            var alertas = new List<BE.Alerta>();

            // 1) Suscripciones próximas a vencer / vencidas
            try
            {
                var clientes  = _cliente.ObtenerTodos();
                int porVencer = clientes.Count(c => c.SuscripcionProximaAVencer(7));
                int vencidas  = clientes.Count(c => c.VencimientoExpirado);

                if (vencidas > 0)
                    alertas.Add(new BE.Alerta(BE.NivelAlerta.Critica, "alert.subs.vencidas",
                        "{0} suscripción(es) vencida(s).", vencidas, vencidas));

                if (porVencer > 0)
                    alertas.Add(new BE.Alerta(BE.NivelAlerta.Advertencia, "alert.subs.porvencer",
                        "{0} suscripción(es) vence(n) en los próximos 7 días.", porVencer, porVencer));
            }
            catch { /* fuente no disponible — se omite esta alerta */ }

            // 2) Antigüedad del último backup
            try
            {
                int dias = _reporte.ObtenerDiasSinBackup();
                if (dias < 0)
                    alertas.Add(new BE.Alerta(BE.NivelAlerta.Critica, "alert.backup.nunca",
                        "No hay backups registrados.", 0));
                else if (dias >= 7)
                    alertas.Add(new BE.Alerta(BE.NivelAlerta.Advertencia, "alert.backup.dias",
                        "Hace {0} día(s) que no se realiza un backup.", dias, dias));
            }
            catch { }

            // 3) Prendas trabadas en limpieza
            try
            {
                int enLimpieza = _prenda.ObtenerTodos()
                    .Count(p => p.Estado == BE.EstadoPrenda.EnLimpieza);
                if (enLimpieza > 0)
                    alertas.Add(new BE.Alerta(BE.NivelAlerta.Info, "alert.prendas.limpieza",
                        "{0} prenda(s) en limpieza.", enLimpieza, enLimpieza));
            }
            catch { }

            // 4) Integridad de datos (dígitos verificadores)
            try
            {
                var diag = Configuracion.ObtenerDiagnostico();
                if (!diag.Integro)
                    alertas.Add(new BE.Alerta(BE.NivelAlerta.Critica, "alert.dv.corruptos",
                        "Integridad comprometida: {0} fila(s) con DV inválido.",
                        diag.FilasRotas.Count, diag.FilasRotas.Count));
            }
            catch { }

            return alertas;
        }

        /// <summary>Cantidad total de alertas activas (para el badge del menú).</summary>
        public int Contar()
        {
            return ObtenerAlertas().Count;
        }
    }
}
