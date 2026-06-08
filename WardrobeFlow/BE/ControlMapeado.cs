namespace BE
{
    /// <summary>
    /// Capa de Entidades — Etapa 4 (permisos a nivel de control).
    /// Asocia una patente (permiso) con un control concreto de un formulario, identificado por
    /// el nombre del formulario y el nombre del control. El ManejadorSeguridad usa estos mapeos
    /// para mostrar/ocultar el control según las patentes efectivas del usuario en sesión.
    ///
    ///   Formulario    → Form.Name      (ej. "Prendas")
    ///   NombreControl → Control.Name   (ej. "btnAgregar", "prendasToolStripMenuItem")
    /// </summary>
    public class ControlMapeado
    {
        public int    Id            { get; set; }
        public int    IdPermiso     { get; set; }
        public string Formulario    { get; set; }
        public string NombreControl { get; set; }
    }
}
