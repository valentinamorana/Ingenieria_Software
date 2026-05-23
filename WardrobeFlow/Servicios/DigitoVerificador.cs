using System.Collections.Generic;

namespace Servicios
{
    /// <summary>
    /// Capa de Servicios — T07 Dígitos Verificadores.
    ///
    /// Algoritmo genérico aplicable a cualquier entidad:
    ///   DVH (Horizontal): detecta alteraciones dentro de una fila y permutas de posición.
    ///   DVV (Vertical):   detecta inserciones, eliminaciones o permutas de filas.
    ///
    /// La suma ponderada por posición hace que mover un carácter (o una fila)
    /// a otra posición cambie el resultado — diferenciando errores de orden
    /// de errores de valor.
    /// </summary>
    public class DigitoVerificador
    {
        private const int Modulo = 10;

        // Calcula el DVH para una fila a partir de sus valores de campo.
        // Cada carácter contribuye con: ASCII(char) × posición (1-indexed, acumulada entre campos).
        // Génerico: acepta cualquier cantidad de campos de cualquier entidad.
        public int CalcularDVH(params string[] campos)
        {
            int suma = 0;
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
        // Detección: inserción, eliminación o permuta de filas cambia el resultado.
        public int CalcularDVV(IList<int> dvhValues)
        {
            int suma = 0;
            for (int i = 0; i < dvhValues.Count; i++)
                suma += dvhValues[i] * (i + 1);
            return suma % Modulo;
        }
    }
}
