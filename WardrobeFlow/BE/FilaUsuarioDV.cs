namespace BE
{
    /// <summary>
    /// DTO de transporte para el cálculo de Dígitos Verificadores (T07).
    ///
    /// Representa una fila de la tabla Usuario con solo los campos que participan
    /// en el cálculo del DVH, más el DVH almacenado. Es un DTO liviano — NO expone
    /// la entidad BE.Usuario completa (la contraseña hasheada se trata como un campo
    /// más del cálculo, no como dato de negocio).
    ///
    /// Vive en BE para que pueda cruzar todas las capas (DAL → BLL → GUI) sin forzar
    /// que la GUI dependa de DAL. Lo producen DAL.DigitoVerificador.ObtenerFilasUsuario(),
    /// lo consumen BLL.Configuracion y los formularios de diagnóstico/reparación.
    /// </summary>
    public class FilaUsuarioDV
    {
        public int    Id               { get; set; }
        public string Username         { get; set; }
        public string Clave            { get; set; }
        public string Perfil           { get; set; }
        public string Estado           { get; set; }
        public string IntentosFallidos { get; set; }
        public int?   DVHAlmacenado    { get; set; }
    }
}
