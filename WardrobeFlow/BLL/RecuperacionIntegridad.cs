using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace BLL
{
    // Tipo de alteración detectada al comparar una fila actual contra el espejo de integridad.
    public enum TipoAlteracion
    {
        Modificada,        // uno o más campos del DVH difieren del espejo
        DVHCorrupto,       // los datos coinciden con el espejo pero el DVH almacenado fue cambiado a mano
        InsertadaExterna   // la fila no existe en el espejo (alta hecha directo en la BD)
    }

    // Diferencia concreta de un campo entre la fila actual y la del espejo.
    public class CampoAlterado
    {
        public string Campo         { get; set; }
        public string ValorActual   { get; set; }
        public string ValorEsperado { get; set; }
    }

    // Una fila de Usuario cuya integridad no coincide, con el detalle de qué cambió.
    public class FilaAlterada
    {
        public int            Id          { get; set; }
        public string         Username    { get; set; }
        public TipoAlteracion Tipo        { get; set; }
        public List<CampoAlterado> Campos { get; set; } = new List<CampoAlterado>();
        public string         Descripcion { get; set; }
    }

    /// <summary>
    /// Resultado del diagnóstico basado en el ESPEJO de integridad (Usuario_Seguridad).
    /// A diferencia del diagnóstico clásico (solo "esta fila no coincide"), identifica QUÉ campo
    /// cambió, CLASIFICA el tipo de daño y dice QUÉ opciones de recuperación son válidas.
    /// </summary>
    public class DiagnosticoEspejo
    {
        public bool EspejoDisponible { get; set; }   // hay tabla espejo con datos → se puede diagnosticar por campo
        public bool TablaVaciada     { get; set; }   // Usuario quedó sin filas pero el espejo tenía → vaciado total
        public bool DvvOk            { get; set; }
        public int? DvvAlmacenado    { get; set; }
        public int  DvvCalculado     { get; set; }

        public List<FilaAlterada>      Alteradas { get; set; } = new List<FilaAlterada>();
        // Filas presentes en el espejo pero ausentes en la tabla actual → eliminación física.
        public List<BE.FilaUsuarioDV>  Faltantes { get; set; } = new List<BE.FilaUsuarioDV>();
        // Líneas legibles que resumen/clasifican el daño detectado.
        public List<string>            Resumen   { get; set; } = new List<string>();

        public bool Integro => !TablaVaciada && DvvOk && Alteradas.Count == 0 && Faltantes.Count == 0;

        // Habilitación de cada opción de recuperación según el tipo de daño.
        public bool PuedeReparar        { get; set; }   // restaurar valores legítimos desde el espejo
        public bool PuedeAsumirPerdida  { get; set; }   // aceptar los datos actuales y recalcular DV
        public bool PuedeRestaurarBackup { get; set; }
    }

    /// <summary>
    /// T07 — Recuperación de integridad asistida por la tabla ESPEJO (Usuario_Seguridad).
    ///
    /// Tres opciones, al estilo del proyecto de referencia (Agus):
    ///   • Reparar desde Espejo  → restaura los valores legítimos guardados en el espejo
    ///                             (borra inserciones externas y revierte campos modificados).
    ///   • Asumir Pérdida        → acepta los datos actuales como legítimos y recalcula todos los DV
    ///                             (delega en Configuracion.RecalcularIntegridadDV, que cubre todas
    ///                             las tablas protegidas y reconstruye el espejo).
    ///   • Restaurar Backup      → lo maneja la GUI con BLL.Backup (cuando el espejo no alcanza).
    /// </summary>
    public static class RecuperacionIntegridad
    {
        // Diagnóstico granular comparando la tabla Usuario actual contra el espejo.
        public static DiagnosticoEspejo Diagnosticar()
        {
            var d     = new DiagnosticoEspejo();
            var dvDAL = new DAL.DigitoVerificador();
            var esp   = new DAL.EspejoUsuario();
            var svc   = Seguridad.CalculadorDV.Crear();

            var actuales = dvDAL.ObtenerFilasUsuario();
            var espejo   = esp.Existe() ? esp.ObtenerFilas() : new List<BE.FilaUsuarioDV>();

            d.EspejoDisponible = espejo.Count > 0;

            // DVV: comparar el almacenado contra el recalculado sobre los datos actuales.
            var dvhsActuales = new List<int>();
            foreach (var f in actuales) dvhsActuales.Add(svc.CalcularDVH(f.CamposParaDVH()));
            d.DvvCalculado  = svc.CalcularDVV(dvhsActuales);
            d.DvvAlmacenado = dvDAL.ObtenerDVV("Usuario");
            d.DvvOk         = d.DvvAlmacenado != null && d.DvvAlmacenado == d.DvvCalculado;

            // Sin espejo no se puede diagnosticar por campo ni reparar: solo recalcular o backup.
            if (!d.EspejoDisponible)
            {
                d.Resumen.Add("No hay un espejo de integridad disponible (BD recién migrada o sin sembrar). " +
                              "No se puede reparar a valores legítimos; sí recalcular o restaurar un backup.");
                d.PuedeReparar         = false;
                d.PuedeAsumirPerdida   = true;
                d.PuedeRestaurarBackup = true;
                return d;
            }

            // Tabla vaciada por completo (pero el espejo tenía registros): solo backup salva.
            if (actuales.Count == 0)
            {
                d.TablaVaciada = true;
                d.Resumen.Add("ALERTA CRÍTICA: la tabla Usuario fue vaciada por completo. " +
                              "Solo se puede recuperar desde un backup.");
                d.PuedeReparar         = false;
                d.PuedeAsumirPerdida   = false;   // recalcular sobre 0 filas sellaría la pérdida total
                d.PuedeRestaurarBackup = true;
                return d;
            }

            var espejoById   = espejo.ToDictionary(x => x.Id);
            var actualesById = new HashSet<int>(actuales.Select(x => x.Id));

            foreach (var act in actuales)
            {
                int  dvhCalc    = svc.CalcularDVH(act.CamposParaDVH());
                bool dvhStoreOk = act.DVHAlmacenado != null && act.DVHAlmacenado == dvhCalc;

                if (!espejoById.TryGetValue(act.Id, out var esp1))
                {
                    d.Alteradas.Add(new FilaAlterada
                    {
                        Id = act.Id, Username = act.Username, Tipo = TipoAlteracion.InsertadaExterna,
                        Descripcion = "Registro inexistente en el espejo (inserción externa detectada)."
                    });
                    continue;
                }

                var diffs = CompararCampos(act, esp1);
                if (diffs.Count > 0)
                    d.Alteradas.Add(new FilaAlterada
                    {
                        Id = act.Id, Username = act.Username, Tipo = TipoAlteracion.Modificada, Campos = diffs
                    });
                else if (!dvhStoreOk)
                    d.Alteradas.Add(new FilaAlterada
                    {
                        Id = act.Id, Username = act.Username, Tipo = TipoAlteracion.DVHCorrupto,
                        Descripcion = "Los datos coinciden con el espejo, pero el DVH almacenado fue corrompido manualmente."
                    });
            }

            // Filas del espejo ausentes hoy → eliminación física.
            foreach (var e in espejo)
                if (!actualesById.Contains(e.Id)) d.Faltantes.Add(e);

            // Clasificación legible.
            if (d.Faltantes.Count > 0)
                d.Resumen.Add($"Se detectó la ELIMINACIÓN física de {d.Faltantes.Count} registro(s) (presentes en el espejo, ausentes hoy).");
            foreach (var a in d.Alteradas)
            {
                if (a.Tipo == TipoAlteracion.InsertadaExterna)
                    d.Resumen.Add($"Inserción externa: usuario '{a.Username}' (ID {a.Id}).");
                else if (a.Tipo == TipoAlteracion.DVHCorrupto)
                    d.Resumen.Add($"DVH corrompido a mano: usuario '{a.Username}' (ID {a.Id}).");
                else
                    d.Resumen.Add($"Modificación externa: usuario '{a.Username}' (ID {a.Id}) — {a.Campos.Count} campo(s) alterado(s).");
            }
            if (!d.DvvOk && d.Alteradas.Count == 0 && d.Faltantes.Count == 0)
                d.Resumen.Add("El DVV de la tabla no coincide (posible reordenamiento de filas).");
            if (d.Resumen.Count == 0)
                d.Resumen.Add("Integridad verificada: los datos coinciden con el espejo.");

            // Reparar desde espejo es seguro cuando NO faltan filas (no reinsertamos por IDENTITY/FKs):
            // cubre modificaciones, inserciones externas, DVH corrupto y reordenamiento.
            d.PuedeReparar         = d.Faltantes.Count == 0 && (d.Alteradas.Count > 0 || !d.DvvOk);
            d.PuedeAsumirPerdida   = true;
            d.PuedeRestaurarBackup = true;
            return d;
        }

        // Compara los campos que entran al DVH entre la fila actual y la del espejo.
        // La Clave (hash) se compara por valor real pero se MUESTRA enmascarada (no se filtra el hash).
        private static List<CampoAlterado> CompararCampos(BE.FilaUsuarioDV a, BE.FilaUsuarioDV e)
        {
            var l = new List<CampoAlterado>();
            void Cmp(string nombre, string va, string ve)
            {
                if ((va ?? "") != (ve ?? ""))
                    l.Add(new CampoAlterado { Campo = nombre, ValorActual = va, ValorEsperado = ve });
            }
            Cmp("Username",         a.Username,         e.Username);
            if ((a.Clave ?? "") != (e.Clave ?? ""))
                l.Add(new CampoAlterado { Campo = "Clave (hash)", ValorActual = "•••(alterada)", ValorEsperado = "•••(original)" });
            Cmp("Rol",              a.Rol,              e.Rol);
            Cmp("Perfil",           a.Perfil,           e.Perfil);
            Cmp("Estado",           a.Estado,           e.Estado);
            Cmp("IntentosFallidos", a.IntentosFallidos, e.IntentosFallidos);
            return l;
        }

        /// <summary>
        /// Repara la tabla Usuario restaurando los valores legítimos del espejo:
        ///   1. Borra las filas que NO existen en el espejo (inserciones externas).
        ///   2. Reescribe cada fila actual con los valores del espejo (revierte modificaciones).
        /// Luego recalcula DVH/DVV de Usuario (el espejo queda idéntico). Atómico y exclusivo de Admin.
        /// NO reinserta filas eliminadas (eso requiere backup): el diagnóstico deshabilita esta opción
        /// cuando hay eliminación física.
        /// </summary>
        public static void RepararDesdeEspejo()
        {
            Configuracion.ExigirAdminDV("Reparar desde espejo (DV)");

            var d = Diagnosticar();
            if (!d.EspejoDisponible)
                throw new BE.AppException("err.bll.espejo.no",
                    "No hay un espejo de integridad disponible para reparar. Usá «Recalcular» o restaurá un backup.");
            if (!d.PuedeReparar)
                throw new BE.AppException("err.bll.espejo.norep",
                    "El daño detectado no se puede reparar desde el espejo (hay registros eliminados físicamente). " +
                    "Restaurá un backup para recuperarlos.");

            var espejo = new DAL.EspejoUsuario().ObtenerFilas();
            var espById = espejo.ToDictionary(x => x.Id);
            var espIds  = new HashSet<int>(espejo.Select(x => x.Id));
            var actuales = new DAL.DigitoVerificador().ObtenerFilasUsuario();

            DAL.Acceso.GetInstance().EjecutarTransaccion((conn, tx) =>
            {
                // 1. Eliminar inserciones externas (filas no presentes en el espejo).
                foreach (var act in actuales)
                {
                    if (espIds.Contains(act.Id)) continue;
                    using (var cmd = new SqlCommand("DELETE FROM Usuario WHERE IdUsuario = @id", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@id", act.Id);
                        cmd.ExecuteNonQuery();
                    }
                }
                // 2. Revertir cada fila existente a los valores del espejo.
                foreach (var act in actuales)
                {
                    if (!espById.TryGetValue(act.Id, out var e)) continue;
                    using (var cmd = new SqlCommand(
                        "UPDATE Usuario SET Username=@u, Clave=@c, Rol=@r, Perfil=@p, Estado=@e, " +
                        "IntentosFallidos=@i WHERE IdUsuario=@id", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@u",  (object)e.Username ?? string.Empty);
                        cmd.Parameters.AddWithValue("@c",  (object)e.Clave    ?? string.Empty);
                        cmd.Parameters.AddWithValue("@r",  (object)e.Rol      ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@p",  (object)e.Perfil   ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@e",  int.TryParse(e.Estado, out int est) ? est : 1);
                        cmd.Parameters.AddWithValue("@i",  int.TryParse(e.IntentosFallidos, out int it) ? it : 0);
                        cmd.Parameters.AddWithValue("@id", e.Id);
                        cmd.ExecuteNonQuery();
                    }
                }
            });

            // Datos ya saneados → recalcular DVH/DVV de Usuario y reconstruir el espejo (queda igual).
            Configuracion.RecalcularUsuario();

            int? idActor = Seguridad.SessionManager.IsLoggedIn
                           ? (int?)Seguridad.SessionManager.GetInstance().Usuario.Id : null;
            string actor = Seguridad.SessionManager.IsLoggedIn
                           ? Seguridad.SessionManager.GetInstance().Usuario.Username : "(arranque/sin sesión)";
            new Servicios.Bitacora().RegistrarSinSesion(
                modulo:     "Integridad de Datos",
                actividad:  "Reparación desde Espejo de Integridad",
                criticidad: BE.Criticidad.Alta,
                idUsuario:  idActor,
                detalle:    $"{actor} restauró la tabla Usuario a los valores legítimos del espejo " +
                            $"({d.Alteradas.Count} fila(s) alterada(s)) a las {DateTime.Now:HH:mm:ss}.");
        }

        /// <summary>
        /// Asume la pérdida: acepta los datos ACTUALES como legítimos y recalcula todos los dígitos
        /// verificadores (todas las tablas protegidas), reconstruyendo el espejo. Delega en
        /// Configuracion.RecalcularIntegridadDV y deja constancia diferenciada en bitácora.
        /// </summary>
        public static void AsumirPerdida()
        {
            Configuracion.RecalcularIntegridadDV();

            int? idActor = Seguridad.SessionManager.IsLoggedIn
                           ? (int?)Seguridad.SessionManager.GetInstance().Usuario.Id : null;
            string actor = Seguridad.SessionManager.IsLoggedIn
                           ? Seguridad.SessionManager.GetInstance().Usuario.Username : "(arranque/sin sesión)";
            new Servicios.Bitacora().RegistrarSinSesion(
                modulo:     "Integridad de Datos",
                actividad:  "Asumir pérdida (recálculo de DV sobre datos actuales)",
                criticidad: BE.Criticidad.Alta,
                idUsuario:  idActor,
                detalle:    $"{actor} aceptó los datos actuales como legítimos y recalculó los dígitos " +
                            $"verificadores a las {DateTime.Now:HH:mm:ss}.");
        }
    }
}
