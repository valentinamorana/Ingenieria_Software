namespace BE
{
    /// <summary>
    /// RF-08 — Una línea del "preview de pérdida" antes de restaurar un backup.
    /// Representa cuántos registros de una entidad existen en la base ACTUAL con
    /// fecha posterior a la del backup; es decir, los que se perderían al restaurar.
    /// </summary>
    public class CambioPosterior
    {
        // Nombre legible de la entidad (ej: "Pedidos", "Clientes nuevos").
        public string Entidad { get; set; }

        // Cantidad de registros posteriores a la fecha del backup.
        public int Cantidad { get; set; }
    }
}
