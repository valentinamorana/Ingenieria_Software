namespace BE
{
    /// <summary>Severidad de una alerta del panel.</summary>
    public enum NivelAlerta
    {
        Info        = 0,
        Advertencia = 1,
        Critica     = 2
    }

    /// <summary>
    /// Alerta operativa del sistema (vencimientos, backups, stock, integridad).
    ///
    /// Sigue la misma convención que <see cref="AppException"/>: lleva una CLAVE de
    /// traducción + un texto de respaldo en español + parámetros, de modo que el
    /// MENSAJE se arma/traduce en la GUI sin que la BLL hardcodee texto de UI.
    /// La DECISIÓN de qué es una alerta vive en BLL.PanelAlertas.
    /// </summary>
    public class Alerta
    {
        public NivelAlerta Nivel           { get; set; }
        public string      ClaveI18n       { get; set; }
        public string      MensajeFallback { get; set; }
        public object[]    Parametros      { get; set; }
        public int         Cantidad        { get; set; }

        public Alerta() { }

        public Alerta(NivelAlerta nivel, string claveI18n, string mensajeFallback, int cantidad, params object[] parametros)
        {
            Nivel           = nivel;
            ClaveI18n       = claveI18n;
            MensajeFallback = mensajeFallback;
            Cantidad        = cantidad;
            Parametros      = parametros;
        }
    }
}
