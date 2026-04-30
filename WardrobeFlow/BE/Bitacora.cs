using System;

namespace BE
{
    /// <summary>
    /// Capa de Entidades - Bitácora
    /// Mapea la tabla [Bitacora] de WardrobeFlowDB. Contenedor de datos que mapea la BD. 
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
        public int Id { get; set; }

        public DateTime Fecha { get; set; }

        public int? IdUsuario { get; set; }

        public string Modulo { get; set; }

        public string Actividad { get; set; }

        public string Detalle { get; set; }

        public Criticidad Criticidad { get; set; }

        public string IP { get; set; }
    }
}
