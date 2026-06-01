namespace BE
{
    /// <summary>
    /// T05 — Multiidioma. Entidad TRADUCCIÓN: tabla intermedia entre Idioma y Control.
    ///
    /// Mapea la tabla [Traduccion]:
    ///   • IdControl → Control asociado.
    ///   • IdIdioma  → Idioma asociado.
    ///   • Texto     → Traducción del texto en ese idioma.
    ///
    /// Nota de diseño: la identidad de la fila es la CLAVE COMPUESTA (IdControl, IdIdioma)
    /// — modelado normalizado (3FN) de la relación N:M Idioma↔Control. El "Id" del apunte
    /// se materializa como esta clave compuesta (no hay surrogate key redundante).
    /// </summary>
    public class Traduccion
    {
        public int    IdControl { get; set; }
        public int    IdIdioma  { get; set; }
        public string Texto     { get; set; }
    }
}
