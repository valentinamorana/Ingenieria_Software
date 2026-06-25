using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace BLL
{
    public class ReporteJornada
    {
        private readonly Bitacora _bllBitacora = new Bitacora();
        private readonly Prenda   _bllPrenda   = new Prenda();
        private readonly Cliente  _bllCliente  = new Cliente();

        private static readonly string DirBackups =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");

        // ── KPI helpers ───────────────────────────────────────────────────────

        public int ContarPrendasDisponibles()
        {
            try { return _bllPrenda.ObtenerDisponibles().Count; }
            catch { return 0; }
        }

        public int ContarClientes()
        {
            try { return _bllCliente.ObtenerTodos().Count; }
            catch { return 0; }
        }

        public int ContarEventosDia(DateTime fecha)
        {
            try
            {
                DataTable dt = _bllBitacora.BuscarPorFiltrosNegocio(
                    fecha.Date, fecha.Date, null, null, null);
                return dt.Rows.Count;
            }
            catch { return 0; }
        }

        public int ObtenerDiasSinBackup()
        {
            try
            {
                if (!Directory.Exists(DirBackups)) return -1;
                FileInfo ultimo = new DirectoryInfo(DirBackups)
                    .GetFiles("*.bak")
                    .OrderByDescending(f => f.LastWriteTime)
                    .FirstOrDefault();
                return ultimo == null ? -1 : (int)(DateTime.Now - ultimo.LastWriteTime).TotalDays;
            }
            catch { return -1; }
        }

        // ── Reporte de un día ─────────────────────────────────────────────────

        public string Generar(DateTime fecha, IDictionary<string, string> lbl = null)
        {
            string L(string k, string fb) => (lbl != null && lbl.ContainsKey(k)) ? lbl[k] : fb;

            DataTable eventos = _bllBitacora.BuscarPorFiltrosNegocio(
                fecha.Date, fecha.Date, null, null, null);
            int prendas  = ContarPrendasDisponibles();
            int clientes = ContarClientes();
            int dias     = ObtenerDiasSinBackup();

            var sb = new StringBuilder();
            sb.AppendLine("═══════════════════════════════════════════════════════");
            sb.AppendLine($"   {L("titulo", "REPORTE DE JORNADA")} — {fecha:dd'/'MM'/'yyyy}");
            sb.AppendLine("   WardrobeFlow");
            sb.AppendLine("═══════════════════════════════════════════════════════");
            sb.AppendLine();

            sb.AppendLine($"▌ {L("resumen", "RESUMEN DEL SISTEMA")}");
            sb.AppendLine(new string('─', 55));
            sb.AppendLine($"  {L("prendas",    "Prendas disponibles"),-22}: {prendas}");
            sb.AppendLine($"  {L("clientes",   "Clientes registrados"),-22}: {clientes}");
            sb.AppendLine($"  {L("diassinbkp", "Días sin backup"),-22}: {(dias < 0 ? L("sinbackups", "Sin backups") : dias.ToString())}");
            sb.AppendLine();

            sb.AppendLine($"▌ {L("eventos", "EVENTOS DE NEGOCIO DEL DÍA")}");
            sb.AppendLine(new string('─', 55));

            if (eventos.Rows.Count == 0)
            {
                sb.AppendLine($"  {L("sinevt", "(sin eventos registrados para esta jornada)")}");
            }
            else
            {
                int n = 0;
                foreach (DataRow row in eventos.Rows)
                {
                    n++;
                    string tipo = row["Tipo"]?.ToString() ?? "—";
                    string det  = row["Descripcion"]?.ToString() ?? "—";
                    string hora = row["Fecha"] is DateTime dt ? dt.ToString("HH:mm:ss") : "—";
                    string usr  = row["UsernameUsuario"]?.ToString();
                    string cli  = row["NombreCliente"]?.ToString();

                    sb.AppendLine($"  {n,3}. [{hora}] {tipo}");
                    if (!string.IsNullOrEmpty(usr))
                        sb.AppendLine($"       {L("usuario", "Usuario")} : {usr}");
                    if (!string.IsNullOrEmpty(cli))
                        sb.AppendLine($"       {L("cliente", "Cliente")} : {cli}");
                    sb.AppendLine($"       {det}");
                }
                sb.AppendLine(new string('─', 55));
                sb.AppendLine($"  {L("totalevt", "TOTAL EVENTOS")} : {n}");
            }

            sb.AppendLine();
            sb.AppendLine($"  {L("generado", "Generado")}: {DateTime.Now:dd'/'MM'/'yyyy HH:mm:ss}");
            sb.AppendLine(new string('═', 55));
            return sb.ToString();
        }

        // ── Eventos crudos de una jornada (para exportar a CSV tabular) ───────
        // Devuelve el DataTable de eventos de negocio del día. La GUI lo usa para
        // exportar a CSV; toda la consulta vive acá (la GUI no arma SQL ni filtra).
        public DataTable ObtenerEventosDelDia(DateTime fecha)
        {
            return _bllBitacora.BuscarPorFiltrosNegocio(fecha.Date, fecha.Date, null, null, null);
        }

        // ── Tendencia de actividad en un rango de fechas ──────────────────────
        // Agrega los eventos de negocio por día entre 'desde' y 'hasta' (inclusive) y
        // arma un resumen: total, promedio diario, día pico y día valle, con el detalle
        // por jornada. Toda la lógica de agregación vive en BLL (la GUI solo lo muestra).
        public string GenerarTendencia(DateTime desde, DateTime hasta, IDictionary<string, string> lbl = null)
        {
            string L(string k, string fb) => (lbl != null && lbl.ContainsKey(k)) ? lbl[k] : fb;

            if (hasta.Date < desde.Date)
            {
                DateTime tmp = desde; desde = hasta; hasta = tmp;
            }

            // Recolección de datos: una consulta por día del rango.
            var conteos = new List<KeyValuePair<DateTime, int>>();
            for (DateTime d = desde.Date; d <= hasta.Date; d = d.AddDays(1))
            {
                int c;
                try { c = _bllBitacora.BuscarPorFiltrosNegocio(d, d, null, null, null).Rows.Count; }
                catch { c = 0; }
                conteos.Add(new KeyValuePair<DateTime, int>(d, c));
            }

            // Agregación PURA (sin BD → testeable de forma determinista).
            EstadisticaTendencia est = CalcularTendencia(conteos);

            var sb = new StringBuilder();
            sb.AppendLine("═══════════════════════════════════════════════════════════");
            sb.AppendLine($"   {L("tend.titulo", "TENDENCIA DE ACTIVIDAD")}");
            sb.AppendLine($"   {desde:dd'/'MM'/'yyyy}  →  {hasta:dd'/'MM'/'yyyy}");
            sb.AppendLine("   WardrobeFlow");
            sb.AppendLine("═══════════════════════════════════════════════════════════");
            sb.AppendLine();

            sb.AppendLine($"▌ {L("resumen", "RESUMEN DEL SISTEMA")}");
            sb.AppendLine(new string('─', 59));
            sb.AppendLine($"  {L("tend.dias",     "Días analizados"),-26}: {est.DiasAnalizados}");
            sb.AppendLine($"  {L("tend.total",    "Total de eventos"),-26}: {est.TotalEventos}");
            sb.AppendLine($"  {L("tend.promedio", "Promedio diario"),-26}: {est.PromedioDiario:0.0}");
            if (est.TotalEventos > 0)
            {
                sb.AppendLine($"  {L("tend.diapico",  "Día de mayor actividad"),-26}: {est.DiaPico:dd'/'MM'/'yyyy} ({est.MaxEventos})");
                sb.AppendLine($"  {L("tend.diavalle", "Día de menor actividad"),-26}: {est.DiaValle:dd'/'MM'/'yyyy} ({est.MinEventos})");
            }
            sb.AppendLine();

            sb.AppendLine($"▌ {L("tend.detalle", "DETALLE POR DÍA")}");
            sb.AppendLine(new string('─', 59));
            foreach (var kv in conteos)
            {
                int barras = kv.Value > 0 && est.MaxEventos > 0 ? (int)System.Math.Round(20.0 * kv.Value / est.MaxEventos) : 0;
                sb.AppendLine($"  {kv.Key:dd'/'MM'/'yyyy}  {new string('█', barras),-20}  {kv.Value}");
            }
            sb.AppendLine(new string('─', 59));

            sb.AppendLine();
            sb.AppendLine($"  {L("generado", "Generado")}: {DateTime.Now:dd'/'MM'/'yyyy HH:mm:ss}");
            sb.AppendLine(new string('═', 59));
            return sb.ToString();
        }

        /// <summary>
        /// NÚCLEO PURO de la tendencia: dada la serie de (día → cantidad de eventos),
        /// calcula total, promedio diario y los días pico/valle. Sin acceso a datos →
        /// 100% testeable de forma determinista.
        /// </summary>
        public static EstadisticaTendencia CalcularTendencia(IList<KeyValuePair<DateTime, int>> conteos)
        {
            var est = new EstadisticaTendencia();
            if (conteos == null || conteos.Count == 0) return est;

            est.DiasAnalizados = conteos.Count;
            int maxC = -1, minC = int.MaxValue, total = 0;
            DateTime diaPico = conteos[0].Key, diaValle = conteos[0].Key;

            foreach (var kv in conteos)
            {
                total += kv.Value;
                if (kv.Value > maxC) { maxC = kv.Value; diaPico  = kv.Key; }
                if (kv.Value < minC) { minC = kv.Value; diaValle = kv.Key; }
            }

            est.TotalEventos   = total;
            est.PromedioDiario = (double)total / conteos.Count;
            est.DiaPico        = diaPico;
            est.MaxEventos     = maxC < 0 ? 0 : maxC;
            est.DiaValle       = diaValle;
            est.MinEventos     = minC == int.MaxValue ? 0 : minC;
            return est;
        }

        // ── Comparación de dos jornadas ───────────────────────────────────────

        public string GenerarComparacion(DateTime fecha1, DateTime fecha2, IDictionary<string, string> lbl = null)
        {
            string L(string k, string fb) => (lbl != null && lbl.ContainsKey(k)) ? lbl[k] : fb;

            DataTable ev1 = _bllBitacora.BuscarPorFiltrosNegocio(
                fecha1.Date, fecha1.Date, null, null, null);
            DataTable ev2 = _bllBitacora.BuscarPorFiltrosNegocio(
                fecha2.Date, fecha2.Date, null, null, null);

            string f1 = fecha1.ToString("dd'/'MM'/'yyyy");
            string f2 = fecha2.ToString("dd'/'MM'/'yyyy");

            var sb = new StringBuilder();
            sb.AppendLine("═══════════════════════════════════════════════════════════");
            sb.AppendLine($"   {L("comparacion", "COMPARACIÓN DE JORNADAS")}");
            sb.AppendLine($"   {f1}  vs  {f2}");
            sb.AppendLine("   WardrobeFlow");
            sb.AppendLine("═══════════════════════════════════════════════════════════");
            sb.AppendLine();

            AppendDetalleJornada(sb, ev1, fecha1, "A", lbl);
            sb.AppendLine();
            AppendDetalleJornada(sb, ev2, fecha2, "B", lbl);
            sb.AppendLine();

            sb.AppendLine(new string('═', 57));
            sb.AppendLine($"  {L("comparfinal", "COMPARATIVO FINAL")}:");
            sb.AppendLine(new string('─', 57));
            sb.AppendLine($"  {"",20}  {$"{L("jornada","JORNADA")} A",14}  {$"{L("jornada","JORNADA")} B",14}");
            sb.AppendLine($"  {L("fecha","Fecha"),-20}  {f1,14}  {f2,14}");
            sb.AppendLine($"  {L("eventostot","Eventos totales"),-20}  {ev1.Rows.Count,14}  {ev2.Rows.Count,14}");
            sb.AppendLine(new string('─', 57));

            if (ev1.Rows.Count > ev2.Rows.Count)
                sb.AppendLine($"  {L("jornada","JORNADA")} A ({f1}) {L("masmasa","tuvo más actividad")}: {ev1.Rows.Count} vs {ev2.Rows.Count}.");
            else if (ev2.Rows.Count > ev1.Rows.Count)
                sb.AppendLine($"  {L("jornada","JORNADA")} B ({f2}) {L("masmasa","tuvo más actividad")}: {ev2.Rows.Count} vs {ev1.Rows.Count}.");
            else if (ev1.Rows.Count == 0)
                sb.AppendLine($"  {L("ninguna", "Ninguna jornada tuvo eventos registrados.")}");
            else
                sb.AppendLine($"  {L("iguales", "Ambas jornadas tuvieron la misma cantidad de eventos.")}");

            sb.AppendLine($"  {L("generado","Generado")}: {DateTime.Now:dd'/'MM'/'yyyy HH:mm:ss}");
            sb.AppendLine(new string('═', 57));
            return sb.ToString();
        }

        private static void AppendDetalleJornada(StringBuilder sb, DataTable eventos,
            DateTime fecha, string letra, IDictionary<string, string> lbl)
        {
            string L(string k, string fb) => (lbl != null && lbl.ContainsKey(k)) ? lbl[k] : fb;

            sb.AppendLine($"──── {L("jornada","JORNADA")} {letra}  ({fecha:dd'/'MM'/'yyyy}) ──────────────────────────");
            if (eventos.Rows.Count == 0)
            {
                sb.AppendLine($"  {L("sinevtjorn","(sin eventos registrados en esta jornada)")}");
            }
            else
            {
                int n = 0;
                foreach (DataRow row in eventos.Rows)
                {
                    n++;
                    string tipo = row["Tipo"]?.ToString() ?? "—";
                    string det  = row["Descripcion"]?.ToString() ?? "—";
                    string hora = row["Fecha"] is DateTime dt ? dt.ToString("HH:mm:ss") : "—";
                    sb.AppendLine($"  {n,3}. [{hora}] {tipo}: {det}");
                }
                sb.AppendLine($"  ► {L("eventostot","Eventos totales")}: {n}");
            }
        }
    }

    /// <summary>Estadística agregada de una tendencia de actividad (resultado puro).</summary>
    public class EstadisticaTendencia
    {
        public int      DiasAnalizados { get; set; }
        public int      TotalEventos   { get; set; }
        public double   PromedioDiario { get; set; }
        public DateTime DiaPico        { get; set; }
        public int      MaxEventos     { get; set; }
        public DateTime DiaValle       { get; set; }
        public int      MinEventos     { get; set; }
    }
}
