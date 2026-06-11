namespace BE
{
    /// <summary>
    /// T07 — Fila genérica para el cálculo del Dígito Verificador.
    /// Reutilizable por cualquier tabla protegida (Usuario, Cliente, Empleado, …):
    /// agrupa la PK, los valores de campo que entran al DVH y el DVH almacenado.
    /// </summary>
    public class FilaDV
    {
        public int      Id            { get; set; }
        /// <summary>Valores que se ponderan para el DVH (incluye la PK como primer campo).</summary>
        public string[] Campos        { get; set; }
        public int?     DVHAlmacenado { get; set; }
        /// <summary>Etiqueta legible para reportes, p. ej. "Cliente #5".</summary>
        public string   Descripcion   { get; set; }
    }
}
