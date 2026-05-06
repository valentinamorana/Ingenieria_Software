namespace Servicios.Multiidioma
{
    /// <summary>
    /// Par (clave → texto traducido) para un idioma concreto.
    ///
    /// Equivalente a la clase Traduccion del ejemplo de cátedra.
    /// Cada entrada del diccionario que devuelve <see cref="Traductor.ObtenerTraducciones"/>
    /// es una instancia de esta clase.
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
