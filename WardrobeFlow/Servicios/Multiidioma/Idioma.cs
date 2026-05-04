namespace Servicios.Multiidioma
{
    /// <summary>
    /// Representa un idioma soportado por el sistema.
    ///
    /// Equivalente a la clase Idioma del ejemplo de cátedra.
    /// Se identifican con un código corto (ES, EN, RU) en lugar de Guid
    /// porque los idiomas son fijos y no vienen de base de datos.
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
