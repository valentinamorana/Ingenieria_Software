using System;

namespace BE
{
    /// <summary>
    /// Capa de Entidades — Entidad Bitácora / Auditoría de Sesión.
    /// Mapea exactamente la tabla [AuditoriaSesion] de WardrobeFlowDB.
    ///
    /// Columnas reales de la tabla (verificadas en SSMS):
    ///   IdAuditoria          int         PK identity
    ///   IdUsuario            int         FK → Usuario.IdUsuario
    ///   DescripcionAuditoria varchar     descripción completa del evento
    ///   FechaHora            datetime    marca de tiempo
    ///
    /// NOTA: La tabla NO tiene columnas separadas para módulo, actividad ni criticidad.
    /// Todo se consolida en DescripcionAuditoria como texto enriquecido.
    /// </summary>
    public class Bitacora
    {
        /// <summary>Clave primaria identity de AuditoriaSesion.</summary>
        public int IdAuditoria { get; set; }

        /// <summary>ID del usuario que generó el evento (FK → Usuario.IdUsuario).</summary>
        public int IdUsuario { get; set; }

        /// <summary>
        /// Texto completo del evento: incluye módulo, actividad, criticidad y detalle.
        /// Ejemplo: "[Login] Inicio Sesion — Usuario: jlopez [Criticidad: None] 14:32:01"
        /// </summary>
        public string DescripcionAuditoria { get; set; }

        /// <summary>Fecha y hora exacta del evento (DateTime.Now al momento de registrar).</summary>
        public DateTime FechaHora { get; set; }
    }
}
