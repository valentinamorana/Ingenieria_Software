namespace BE
{
    /// <summary>
    /// Nombres de roles del sistema. Centraliza los literales que antes estaban
    /// dispersos (evita errores de tipeo y facilita el mantenimiento — criterio de reuso).
    /// </summary>
    public static class Roles
    {
        public const string Administrador = "Administrador";

        // Único punto que compara un nombre de rol/perfil contra el literal del Administrador
        // (case-insensitive y null-safe). Centraliza la decisión para no repetir string.Equals
        // por las capas. Lo usan BE.Usuario.EsAdministrador y las reglas que solo tienen el string.
        public static bool EsAdministrador(string rolOPerfil) =>
            string.Equals(rolOPerfil, Administrador, System.StringComparison.OrdinalIgnoreCase);
    }
}
