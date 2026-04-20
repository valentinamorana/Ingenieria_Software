using System;
using System.Security.Cryptography;

namespace Seguridad
{
    /// <summary>
    /// Módulo de Seguridad — Encriptador de Contraseñas.
    ///
    /// Implementa PBKDF2 (Password-Based Key Derivation Function 2) con SHA-256
    /// para el hash seguro de contraseñas. SHA-256 reemplaza a SHA-1 que es legacy.
    ///
    /// Por qué PBKDF2-SHA256:
    ///   - Salt aleatorio único por contraseña (evita ataques de rainbow table)
    ///   - 100.000 iteraciones (hace brute-force costoso computacionalmente)
    ///   - SHA-256 produce 256 bits de salida (vs 160 bits de SHA-1) — mayor resistencia
    ///   - Nativo en .NET Framework 4.7.2, sin dependencias externas
    ///   - Nunca se almacena la contraseña en texto plano
    ///
    /// Formato de almacenamiento en BD: Base64( Salt[16 bytes] + Hash[32 bytes] ) = 64 chars
    /// </summary>
    public static class Encriptador
    {
        // Tamaño del Salt: 16 bytes = 128 bits de entropía aleatoria
        private const int SaltSize = 16;

        // Tamaño del Hash: 32 bytes = 256 bits (salida de SHA-256)
        private const int HashSize = 32;

        // Iteraciones PBKDF2: mínimo recomendado por OWASP para SHA-256 (2024)
        private const int Iterations = 100000;

        /// <summary>
        /// Genera un hash seguro de la contraseña para almacenar en la base de datos.
        /// Cada llamada genera un Salt diferente, por lo que el mismo password
        /// producirá hashes distintos en cada invocación (esto es correcto y esperado).
        /// </summary>
        /// <param name="contraseña">Contraseña en texto plano ingresada por el usuario.</param>
        /// <returns>String Base64 de 64 caracteres con Salt + Hash, listo para guardar en BD.</returns>
        public static string Hash(string contraseña)
        {
            // Paso 1: Generar Salt criptográficamente aleatorio con CSPRNG
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Paso 2: Aplicar PBKDF2-SHA256 con el salt generado
            using (var pbkdf2 = new Rfc2898DeriveBytes(
                contraseña, salt, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(HashSize);

                // Paso 3: Concatenar Salt + Hash
                // Estructura: [0..15] = Salt | [16..47] = Hash
                byte[] hashBytes = new byte[SaltSize + HashSize];
                Array.Copy(salt, 0, hashBytes, 0,        SaltSize);
                Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

                // Paso 4: Base64 para almacenamiento en VARCHAR de la BD (64 chars)
                return Convert.ToBase64String(hashBytes);
            }
        }

        /// <summary>
        /// Verifica si una contraseña ingresada coincide con el hash almacenado en la BD.
        /// Extrae el Salt del hash almacenado, rehashea con SHA-256 y compara byte a byte.
        /// </summary>
        /// <param name="contraseñaIngresada">Contraseña en texto plano del formulario de login.</param>
        /// <param name="hashAlmacenado">String Base64 con Salt+Hash guardado en la base de datos.</param>
        /// <returns>true si la contraseña es correcta; false si no coincide.</returns>
        public static bool VerificarContraseña(string contraseñaIngresada, string hashAlmacenado)
        {
            // Paso 1: Decodificar Base64 → bytes
            byte[] hashBytes = Convert.FromBase64String(hashAlmacenado);

            // Paso 2: Extraer el Salt (primeros 16 bytes)
            byte[] salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            // Paso 3: Rehashear con el MISMO Salt, iteraciones y algoritmo (SHA-256)
            using (var pbkdf2 = new Rfc2898DeriveBytes(
                contraseñaIngresada, salt, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] hashCalculado = pbkdf2.GetBytes(HashSize);

                // Paso 4: Comparación byte a byte de los 32 bytes del hash
                // Sección [16..47] del almacenado vs el recalculado
                for (int i = 0; i < HashSize; i++)
                {
                    if (hashBytes[i + SaltSize] != hashCalculado[i])
                        return false;   // Cualquier byte diferente → contraseña incorrecta
                }
                return true;            // Todos coinciden → contraseña válida
            }
        }
    }
}
