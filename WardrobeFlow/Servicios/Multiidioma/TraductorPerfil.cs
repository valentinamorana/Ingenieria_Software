namespace Servicios.Multiidioma
{
    /// <summary>
    /// Traduce el código interno de un perfil/rol al nombre visible en el idioma activo.
    /// Centraliza el mapeo que antes estaba DUPLICADO en varios formularios (Usuarios, MiPerfil),
    /// eliminando la repetición (DRY) y dejando una única fuente de verdad.
    /// </summary>
    public static class TraductorPerfil
    {
        public static string Nombre(string perfil)
        {
            if (string.IsNullOrEmpty(perfil)) return "—";

            var t = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
            string Key(string k) => t.ContainsKey(k) ? t[k].Texto : null;

            switch (perfil)
            {
                case "Administrador":        return Key("perfil.administrador")     ?? perfil;
                case "Auditor":              return Key("perfil.auditor")           ?? perfil;
                case "GerenteComercial":     return Key("perfil.gerentecomercial")  ?? perfil;
                case "Vendedor":             return Key("perfil.vendedor")          ?? perfil;
                case "GerenteInventario":    return Key("perfil.gerenteinventario") ?? perfil;
                case "EncargadoDeStock":     return Key("perfil.encargadodestock")  ?? perfil;
                case "OperadorLogistico":    return Key("perfil.operadorlogistico") ?? perfil;
                case "Supervisor":           return Key("perfil.supervisor")        ?? perfil;
                case "ControladorDeStock":   return Key("perfil.stock")             ?? perfil;
                case "OperadorDeInventario": return Key("perfil.operador")          ?? perfil;
                default:                     return perfil;
            }
        }
    }
}
