using System.Collections.Generic;

namespace Seguridad
{
    /// <summary>
    /// T07 Dígitos Verificadores — algoritmo robusto (DVH + DVV).
    ///
    /// Vive en Seguridad porque no depende de ninguna otra capa del sistema,
    /// lo que permite que tanto DAL como BLL la usen sin crear dependencias circulares.
    ///
    /// ALGORITMO (alineado con referencia Stach):
    ///   DVH: suma ponderada por posición-en-campo × índice-de-campo, módulo primo grande.
    ///        Detecta modificaciones en cualquier campo de la fila.
    ///   DVV: suma ponderada de DVH × posición-de-fila, módulo primo grande.
    ///        Detecta inserción, eliminación o reordenamiento de filas.
    ///
    /// MÓDULO: 999_983 (primo) — ~100.000× más resistente a colisiones que mod 10.
    ///   Con mod 10 → 10 posibles DVH.
    ///   Con mod 999.983 → hasta 999.983 posibles DVH.
    ///
    /// NOTA DE MIGRACIÓN: al desplegar esta versión, los DVH/DVV almacenados con el
    /// algoritmo anterior (mod 10) quedan invalidados. El sistema detectará la
    /// discrepancia al arrancar y bloqueará el login hasta que un Administrador
    /// ejecute "Recalcular Todo" desde Administrar → Diagnóstico de Integridad.
    /// </summary>
    public class DigitoVerificador : ICalculadorDV
    {
        // Primo grande que permite ~100.000x más valores distintos que el mod 10 anterior.
        // Cabe en INT de SQL Server (max 2.147.483.647).
        private const int Modulo = 999_983;

        /// <summary>
        /// Calcula el DVH para una fila a partir de sus valores de campo.
        ///
        /// Fórmula (igual que Stach):
        ///   suma += ASCII(c) × posición_en_campo × índice_de_campo
        ///
        /// La doble ponderación (por carácter Y por campo) hace que:
        ///   - Modificar el mismo carácter en campos distintos produce DVH distintos.
        ///   - Intercambiar valores entre campos produce DVH distintos.
        /// </summary>
        public int CalcularDVH(params string[] campos)
        {
            long suma      = 0;
            int campoIdx   = 1;

            foreach (string campo in campos)
            {
                string valor  = campo ?? string.Empty;
                int charPos   = 1;

                foreach (char c in valor)
                {
                    suma += (long)(int)c * charPos * campoIdx;
                    charPos++;
                }

                campoIdx++;
            }

            return (int)(suma % Modulo);
        }

        /// <summary>
        /// Calcula el DVV para una tabla a partir de la lista de DVH de sus filas.
        ///
        /// Fórmula: suma += DVH_i × (i + 1)
        ///
        /// La ponderación posicional detecta:
        ///   - Eliminación de una fila (suma cambia).
        ///   - Inserción de una fila (suma cambia).
        ///   - Reordenamiento de filas (suma cambia).
        ///
        /// Usa long para el acumulador para evitar overflow con muchos usuarios.
        /// </summary>
        public int CalcularDVV(IList<int> dvhValues)
        {
            long suma = 0;
            for (int i = 0; i < dvhValues.Count; i++)
                suma += (long)dvhValues[i] * (i + 1);
            return (int)(suma % Modulo);
        }
    }
}
