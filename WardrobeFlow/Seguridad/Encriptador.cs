using System;
using System.Security.Cryptography;

// BUG FIX: namespace corregido de "Servicios" a "Seguridad" para reflejar
// correctamente que esta clase pertenece al proyecto Seguridad.
namespace Seguridad
{
    /// <summary>
    /// Módulo de Seguridad — Encriptador de Contraseñas.
    ///
    /// Implementa el algoritmo PBKDF2 (Password-Based Key Derivation Function 2)
    /// con SHA-1 para el hash seguro de contraseñas. Este algoritmo es el estándar
    /// recomendado para almacenamiento de contraseñas porque:
    ///   - Usa un Salt aleatorio único por contraseña (evita ataques de rainbow table)
    ///   - Aplica 100.000 iteraciones (hace brute-force costoso computacionalmente)
    ///   - Nunca almacena la contraseña en texto plano
    ///
    /// Formato de almacenamiento en BD: Base64(Salt[16 bytes] + Hash[20 bytes])
    /// </summary>
    public static class Encriptador
    {
        // Tamaño del Salt: 16 bytes = 128 bits de entropía aleatoria
        private const int SaltSize = 16;

        // Tamaño del Hash resultante: 20 bytes = 160 bits (salida de SHA-1)
        private const int HashSize = 20;

        // Número de iteraciones PBKDF2: cuanto mayor, más difícil de atacar por fuerza bruta
        // 100.000 iteraciones es el mínimo recomendado por OWASP (2023)
        private const int Iterations = 100000;

        /// <summary>
        /// Genera un hash seguro de la contraseña para almacenar en la base de datos.
        /// Cada llamada genera un Salt diferente, por lo que el mismo password
        /// producirá hashes distintos en cada invocación (esto es correcto y esperado).
        /// </summary>
        /// <param name="contraseña">Contraseña en texto plano ingresada por el usuario.</param>
        /// <returns>String Base64 con Salt + Hash combinados, listo para guardar en la BD.</returns>
        public static string Hash(string contraseña)
        {
            // Paso 1: Generar un Salt criptográficamente aleatorio
            // Se usa RandomNumberGenerator (CSPRNG) en lugar de Random para mayor seguridad
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Paso 2: Aplicar PBKDF2 con el salt generado y las iteraciones configuradas
            using (var pbkdf2 = new Rfc2898DeriveBytes(contraseña, salt, Iterations))
            {
                byte[] hash = pbkdf2.GetBytes(HashSize);

                // Paso 3: Concatenar Salt + Hash en un único arreglo de bytes
                // Estructura: [0..15] = Salt | [16..35] = Hash
                byte[] hashBytes = new byte[SaltSize + HashSize];
                Array.Copy(salt, 0, hashBytes, 0, SaltSize);
                Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

                // Paso 4: Convertir a Base64 para almacenamiento en columna VARCHAR de la BD
                return Convert.ToBase64String(hashBytes);
            }
        }

        /// <summary>
        /// Verifica si una contraseña ingresada coincide con el hash almacenado en la BD.
        /// Extrae el Salt original del hash almacenado y rehashea la contraseña ingresada
        /// para compararla byte a byte.
        /// </summary>
        /// <param name="contraseñaIngresada">Contraseña en texto plano que el usuario escribió en el login.</param>
        /// <param name="hashAlmacenado">String Base64 con Salt+Hash guardado en la base de datos.</param>
        /// <returns>true si la contraseña es correcta; false si no coincide.</returns>
        public static bool VerificarContraseña(string contraseñaIngresada, string hashAlmacenado)
        {
            // Paso 1: Decodificar el string Base64 a bytes
            byte[] hashBytes = Convert.FromBase64String(hashAlmacenado);

            // Paso 2: Extraer el Salt (primeros 16 bytes del arreglo)
            byte[] salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            // Paso 3: Rehashear la contraseña ingresada usando el MISMO Salt e iteraciones
            // Si la contraseña es correcta, el resultado será idéntico al hash almacenado
            using (var pbkdf2 = new Rfc2898DeriveBytes(contraseñaIngresada, salt, Iterations))
            {
                byte[] hashCalculado = pbkdf2.GetBytes(HashSize);

                // Paso 4: Comparación byte por byte de los 20 bytes del hash
                // Se compara la sección [16..35] del hashBytes almacenado contra el hashCalculado
                for (int i = 0; i < HashSize; i++)
                {
                    if (hashBytes[i + SaltSize] != hashCalculado[i])
                    {
                        return false;   // Cualquier byte diferente → contraseña incorrecta
                    }
                }
                return true;    // Todos los bytes coinciden → contraseña válida
            }
        }
    }
}
