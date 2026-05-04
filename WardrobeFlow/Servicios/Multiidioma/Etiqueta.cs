namespace Servicios.Multiidioma
{
    /// <summary>
    /// Clave de traducción — identifica un texto de la interfaz de forma independiente del idioma.
    ///
    /// Equivalente a la clase Etiqueta del ejemplo de cátedra.
    /// El Nombre actúa como clave del diccionario en <see cref="Traductor"/>.
    ///
    /// Ejemplos de claves: "lbl.usuario", "btn.ingresar", "mnu.inventario".
    /// </summary>
    public class Etiqueta
    {
        /// <summary>Clave única que identifica el texto en la interfaz.</summary>
        public string Nombre { get; set; }
    }
}
