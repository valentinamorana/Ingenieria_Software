namespace Servicios.Multiidioma
{
    /// <summary>
    /// Tipo de RUNTIME de una traducción (par clave → texto) para un idioma concreto.
    /// Cada entrada del diccionario que devuelve <see cref="Traductor.ObtenerTraducciones"/>
    /// es una instancia de esta clase. Su contraparte de PERSISTENCIA es
    /// <see cref="BE.FilaTraduccion"/> (fila de la tabla Traduccion + Control).
    /// </summary>
    public class Traduccion
    {
        /// <summary>La clave que identifica el texto en la interfaz.</summary>
        public Etiqueta Etiqueta { get; set; }

        /// <summary>La clave string que identifica el texto (usada por Traductor.Construir).</summary>
        public string   Clave    { get; set; }

        /// <summary>El texto traducido al idioma correspondiente.</summary>
        public string   Texto    { get; set; }
    }
}
