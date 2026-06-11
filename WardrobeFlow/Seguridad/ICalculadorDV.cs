using System.Collections.Generic;

namespace Seguridad
{
    /// <summary>
    /// T07 — Contrato del algoritmo de Dígito Verificador.
    /// Permite intercambiar la implementación del cálculo (p. ej. la suma ponderada
    /// posicional actual, o una basada en hash) sin tocar el resto del sistema:
    /// los consumidores piden la implementación a <see cref="CalculadorDV.Crear"/>.
    /// </summary>
    public interface ICalculadorDV
    {
        /// <summary>DVH (horizontal) de una fila a partir de sus campos.</summary>
        int CalcularDVH(params string[] campos);

        /// <summary>DVV (vertical) de una tabla a partir de los DVH de sus filas.</summary>
        int CalcularDVV(IList<int> dvhValues);
    }
}
