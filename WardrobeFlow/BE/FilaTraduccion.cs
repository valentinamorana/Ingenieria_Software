namespace BE
{
    /// <summary>
    /// T05 — Entidad de PERSISTENCIA de una traducción (capa BE).
    /// Representa una fila de la tabla [Traduccion] (IdControl, IdIdioma, Texto)
    /// JUNTO con los datos de su [Control] asociado (Clave, Formulario) que se
    /// obtienen por JOIN. Es la única representación de "traducción" en la capa BE.
    ///
    /// Modelo de traducción por capa:
    ///   • Persistencia (BE)  → esta clase (BE.FilaTraduccion) + BE.Control + BE.Idioma.
    ///   • Runtime/Observer    → Servicios.Multiidioma.Traduccion (par clave→texto).
    /// </summary>
    public class FilaTraduccion
    {
        public int    IdControl  { get; set; }
        public int    IdIdioma   { get; set; }
        public string Clave      { get; set; }
        public string Formulario { get; set; }
        public string Texto      { get; set; }
    }
}
