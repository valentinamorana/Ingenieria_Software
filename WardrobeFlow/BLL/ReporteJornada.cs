using System;
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

        public string Generar(DateTime fecha)
        {
            DataTable eventos = _bllBitacora.BuscarPorFiltrosNegocio(
                fecha.Date, fecha.Date, null, null, null);
            int prendas  = ContarPrendasDisponibles();
            int clientes = ContarClientes();
            int dias     = ObtenerDiasSinBackup();

            var sb = new StringBuilder();
            sb.AppendLine("═══════════════════════════════════════════════════════");
            sb.AppendLine($"   REPORTE DE JORNADA — {fecha:dd'/'MM'/'yyyy}");
            sb.AppendLine("   WardrobeFlow");
            sb.AppendLine("═══════════════════════════════════════════════════════");
            sb.AppendLine();

            sb.AppendLine("▌ RESUMEN DEL SISTEMA");
            sb.AppendLine(new string('─', 55));
            sb.AppendLine($"  Prendas disponibles  : {prendas}");
            sb.AppendLine($"  Clientes registrados : {clientes}");
            sb.AppendLine($"  Días sin backup      : {(dias < 0 ? "Sin backups" : dias.ToString())}");
            sb.AppendLine();

            sb.AppendLine("▌ EVENTOS DE NEGOCIO DEL DÍA");
            sb.AppendLine(new string('─', 55));

            if (eventos.Rows.Count == 0)
            {
                sb.AppendLine("  (sin eventos registrados para esta jornada)");
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
                    if (!string.IsNullOrEmpty(usr)) sb.AppendLine($"       Usuario : {usr}");
                    if (!string.IsNullOrEmpty(cli)) sb.AppendLine($"       Cliente : {cli}");
                    sb.AppendLine($"       {det}");
                }
                sb.AppendLine(new string('─', 55));
                sb.AppendLine($"  TOTAL EVENTOS : {n}");
            }

            sb.AppendLine();
            sb.AppendLine($"  Generado: {DateTime.Now:dd'/'MM'/'yyyy HH:mm:ss}");
            sb.AppendLine(new string('═', 55));
            return sb.ToString();
        }

        // ── Comparación de dos jornadas ───────────────────────────────────────

        public string GenerarComparacion(DateTime fecha1, DateTime fecha2)
        {
            DataTable ev1 = _bllBitacora.BuscarPorFiltrosNegocio(
                fecha1.Date, fecha1.Date, null, null, null);
            DataTable ev2 = _bllBitacora.BuscarPorFiltrosNegocio(
                fecha2.Date, fecha2.Date, null, null, null);

            string f1 = fecha1.ToString("dd'/'MM'/'yyyy");
            string f2 = fecha2.ToString("dd'/'MM'/'yyyy");

            var sb = new StringBuilder();
            sb.AppendLine("═══════════════════════════════════════════════════════════");
            sb.AppendLine("   COMPARACIÓN DE JORNADAS");
            sb.AppendLine($"   {f1}  vs  {f2}");
            sb.AppendLine("   WardrobeFlow");
            sb.AppendLine("═══════════════════════════════════════════════════════════");
            sb.AppendLine();

            AppendDetalleJornada(sb, ev1, fecha1, "A");
            sb.AppendLine();
            AppendDetalleJornada(sb, ev2, fecha2, "B");
            sb.AppendLine();

            sb.AppendLine(new string('═', 57));
            sb.AppendLine("  COMPARATIVO FINAL:");
            sb.AppendLine(new string('─', 57));
            sb.AppendLine($"  {"",20}  {"JORNADA A",14}  {"JORNADA B",14}");
            sb.AppendLine($"  {"Fecha",-20}  {f1,14}  {f2,14}");
            sb.AppendLine($"  {"Eventos totales",-20}  {ev1.Rows.Count,14}  {ev2.Rows.Count,14}");
            sb.AppendLine(new string('─', 57));

            if (ev1.Rows.Count > ev2.Rows.Count)
                sb.AppendLine($"  JORNADA A ({f1}) tuvo más actividad: {ev1.Rows.Count} vs {ev2.Rows.Count} eventos.");
            else if (ev2.Rows.Count > ev1.Rows.Count)
                sb.AppendLine($"  JORNADA B ({f2}) tuvo más actividad: {ev2.Rows.Count} vs {ev1.Rows.Count} eventos.");
            else if (ev1.Rows.Count == 0)
                sb.AppendLine("  Ninguna jornada tuvo eventos registrados.");
            else
                sb.AppendLine("  Ambas jornadas tuvieron la misma cantidad de eventos.");

            sb.AppendLine($"  Generado: {DateTime.Now:dd'/'MM'/'yyyy HH:mm:ss}");
            sb.AppendLine(new string('═', 57));
            return sb.ToString();
        }

        private static void AppendDetalleJornada(StringBuilder sb, DataTable eventos, DateTime fecha, string letra)
        {
            sb.AppendLine($"──── JORNADA {letra}  ({fecha:dd'/'MM'/'yyyy}) ──────────────────────────");
            if (eventos.Rows.Count == 0)
            {
                sb.AppendLine("  (sin eventos registrados en esta jornada)");
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
                sb.AppendLine($"  ► Eventos: {n}");
            }
        }
    }
}
