namespace BE
{
    /// <summary>
    /// T05 — Entidad de PERSISTENCIA del idioma (capa BE), mapea la tabla [Idioma].
    /// El sistema de Observer/Traductor usa su contraparte de runtime
    /// <see cref="Servicios.Multiidioma.Idioma"/> (más liviana, identificada por código);
    /// la conversión BE.Idioma → runtime está centralizada en
    /// BLL.IdiomaService.ObtenerIdiomasActivosComoIdioma (Codigo → Id).
    /// </summary>
    public class Idioma
    {
        public int    IdIdioma  { get; set; }
        public string Codigo    { get; set; }
        public string Nombre    { get; set; }
        public bool   Activo    { get; set; }
        public bool   EsDefault { get; set; }
    }
}
