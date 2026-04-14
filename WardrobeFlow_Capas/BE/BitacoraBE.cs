using System;

namespace BE
{
    // Clase que representa un evento registrado en la bitacora del sistema.
    // Basada en BitacoraBE del ejemplo de referencia (Nacho/Codigo),
    // adaptada para storage en memoria (no SQL) con Guid para IdUsuario.
    public class BitacoraBE
    {
        // Identificador unico del evento (generado automaticamente)
        public Guid IdBitacora { get; set; } = Guid.NewGuid();

        // Fecha y hora exacta del evento
        public DateTime FechaHora { get; set; }

        // Nombre del usuario que genero el evento
        public string NombreUsuario { get; set; }

        // Tipo de operacion realizada (ver TipoOperacion enum)
        public TipoOperacion TipoOperacion { get; set; }

        // Modulo del sistema donde ocurrio el evento (ver Modulo enum)
        public Modulo Modulo { get; set; }

        // Descripcion legible del evento
        public string Descripcion { get; set; }

        // Indica si la operacion fue exitosa o no
        public bool Exitoso { get; set; } = true;

        // Constructor: establece la fecha y hora actual al crear el evento
        public BitacoraBE()
        {
            FechaHora = DateTime.Now;
            Exitoso   = true;
        }

        // Valida que el evento tenga los datos minimos requeridos
        public bool EsValido()
        {
            return !string.IsNullOrWhiteSpace(NombreUsuario)
                && !string.IsNullOrWhiteSpace(Descripcion)
                && Descripcion.Length <= 500;
        }

        // Representacion textual para mostrar en grillas y listas
        public override string ToString()
        {
            string estado = Exitoso ? "OK" : "ERROR";
            return string.Format("{0:dd/MM/yyyy HH:mm} | {1} | {2} | {3} | {4}",
                FechaHora, NombreUsuario, Modulo, TipoOperacion, estado);
        }
    }
}
