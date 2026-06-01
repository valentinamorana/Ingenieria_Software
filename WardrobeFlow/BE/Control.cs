namespace BE
{
    /// <summary>
    /// T05 — Multiidioma. Entidad CONTROL: representa cualquier texto visible del
    /// sistema que puede traducirse (etiqueta, botón, menú, columna, mensaje…).
    ///
    /// Mapea la tabla [Control]:
    ///   • IdControl  → Identificador (PK).
    ///   • Clave      → Nombre interno del control (ej: "btn.ingresar").
    ///   • Formulario → Formulario al que pertenece (ej: "Login").
    /// </summary>
    public class Control
    {
        public int    IdControl  { get; set; }
        public string Clave      { get; set; }
        public string Formulario { get; set; }

        public override string ToString() => Clave;
    }
}
