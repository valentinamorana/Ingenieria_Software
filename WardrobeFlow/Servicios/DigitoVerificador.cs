using System.Collections.Generic;

namespace Servicios
{
    /// <summary>
    /// OBSOLETO — duplicado de Seguridad.DigitoVerificador.
    /// Delega todas las operaciones a la implementación canónica en Seguridad.
    /// Use Seguridad.DigitoVerificador directamente en código nuevo.
    /// </summary>
    [System.Obsolete("Use Seguridad.DigitoVerificador directly.")]
    public class DigitoVerificador
    {
        private readonly Seguridad.DigitoVerificador _inner = new Seguridad.DigitoVerificador();

        public int CalcularDVH(params string[] campos)           => _inner.CalcularDVH(campos);
        public int CalcularDVV(IList<int> dvhValues)             => _inner.CalcularDVV(dvhValues);
    }
}
