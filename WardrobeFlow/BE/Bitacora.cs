using System;

namespace BE
{
    /// <summary>
    /// Capa de Entidades — Entidad Bitácora (Registro de Auditoría).
    /// Mapea la tabla [Bitacora] de WardrobeFlowDB.
    ///
    /// Columnas:
    ///   Id          int          PK identity
    ///   Fecha       datetime
    ///   IdUsuario   int          FK → Usuario.Id
    ///   Modulo      varchar      formulario/sección del sistema
    ///   Actividad   varchar      acción realizada
    ///   Detalle     varchar      descripción completa
    ///   Criticidad  int          0=None, 1=Baja, 2=Media, 3=Alta
    /// </summary>
    public class Bitacora
    {
        /// <summary>Clave primaria identity.</summary>
        public int Id { get; set; }

        /// <summary>Fecha y hora del evento.</summary>
        public DateTime Fecha { get; set; }

        /// <summary>ID del usuario que generó el evento. NULL para eventos pre-sesión (login fallido, forgot password).</summary>
        public int? IdUsuario { get; set; }

        /// <summary>Módulo o formulario donde ocurrió la actividad.</summary>
        public string Modulo { get; set; }

        /// <summary>Acción realizada (ej: "Inicio Sesion", "Alta Usuario").</summary>
        public string Actividad { get; set; }

        /// <summary>Descripción detallada del evento para trazabilidad.</summary>
        public string Detalle { get; set; }

        /// <summary>Nivel de importancia: None=0, Baja=1, Media=2, Alta=3.</summary>
        public Criticidad Criticidad { get; set; }

        /// <summary>Dirección IP del equipo desde donde se originó la acción.</summary>
        public string IP { get; set; }
    }
}
