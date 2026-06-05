namespace BE
{
    /// <summary>
    /// Preferencias de interfaz por usuario (personalización). Una fila por usuario en la
    /// tabla Preferencia. Los valores por defecto representan la apariencia estándar.
    /// El idioma NO vive acá: se mantiene en Usuario.IdIdioma (que ya funciona); en "Mi Perfil"
    /// se muestran juntos.
    /// </summary>
    public class Preferencia
    {
        public int    IdUsuario      { get; set; }
        public string FuenteFamilia  { get; set; } = "Segoe UI";
        public string FuenteTamano   { get; set; } = "Normal";      // Chico / Normal / Grande
        public string Tema           { get; set; } = "Claro";       // Claro / Oscuro
        public string FormatoFecha   { get; set; } = "dd/MM/yyyy";
        public bool   Notificaciones { get; set; } = true;          // placeholder (se guarda; sin efecto aún)

        // Tamaño en puntos según la escala elegida.
        public float TamanoEnPuntos()
        {
            switch ((FuenteTamano ?? "Normal").Trim().ToLowerInvariant())
            {
                case "chico":  return 8f;
                case "grande": return 11f;
                default:       return 9f;   // Normal
            }
        }

        public bool EsOscuro =>
            string.Equals(Tema, "Oscuro", System.StringComparison.OrdinalIgnoreCase);
    }
}
