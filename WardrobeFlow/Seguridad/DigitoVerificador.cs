using System.Collections.Generic;

namespace Seguridad
{
    /// <summary>
    /// T07 Dígitos Verificadores — algoritmo genérico (DVH + DVV).
    ///
    /// Vive en Seguridad porque no depende de ninguna otra capa del sistema,
    /// lo que permite que tanto DAL como BLL la usen sin crear dependencias circulares.
    /// </summary>
    public class DigitoVerificador
    {
        private const int Modulo = 10;

        // Calcula el DVH para una fila a partir de sus valores de campo.
        // Cada carácter contribuye con: ASCII(char) × posición (1-indexed, acumulada entre campos).
        public int CalcularDVH(params string[] campos)
        {
            int suma     = 0;
            int posicion = 1;

            foreach (string campo in campos)
            {
                string valor = campo ?? string.Empty;
                foreach (char c in valor)
                {
                    suma += (int)c * posicion;
                    posicion++;
                }
            }

            return suma % Modulo;
        }

        // Calcula el DVV para una tabla a partir de la lista de DVH de sus filas.
        // Cada DVH contribuye con: DVH_i × posición_i (1-indexed).
        public int CalcularDVV(IList<int> dvhValues)
        {
            int suma = 0;
            for (int i = 0; i < dvhValues.Count; i++)
                suma += dvhValues[i] * (i + 1);
            return suma % Modulo;
        }
    }
}
