namespace Servicios.Multiidioma
{
    /// <summary>
    /// Tipo de RUNTIME del idioma para el patrón Observer (Subject GestorIdioma e
    /// IIdiomaObserver.UpdateLanguage). Es liviano: se identifica por código corto
    /// (ES, EN, RU). Su contraparte de PERSISTENCIA es <see cref="BE.Idioma"/>
    /// (la conversión está centralizada en BLL.IdiomaService).
    ///
    /// La lista de idiomas activos se carga desde la tabla Idioma en BD;
    /// el hardcode en Traductor actúa solo como fallback de primer arranque.
    /// </summary>
    public class Idioma
    {
        /// <summary>Código corto del idioma: "ES", "EN", "RU".</summary>
        public string Id       { get; set; }

        /// <summary>Nombre visible en la interfaz: "Español", "English", "Русский".</summary>
        public string Nombre   { get; set; }

        /// <summary>Indica si este idioma es el predeterminado al iniciar la aplicación.</summary>
        public bool   EsDefault { get; set; }
    }
}
