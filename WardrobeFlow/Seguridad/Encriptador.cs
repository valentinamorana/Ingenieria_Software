using System;
using System.Security.Cryptography;
using System.Text;

namespace Seguridad
{
    /// <summary>
    /// Módulo de Seguridad — Encriptador centralizado (T03).
    ///
    /// Centraliza dos mecanismos criptográficos distintos según la naturaleza
    /// del dato a proteger. Cumple el principio Open/Closed de SOLID: toda la
    /// lógica criptográfica reside aquí; cambiar un algoritmo no impacta a los
    /// módulos consumidores (BLL.Usuario, DAL.Cliente, etc.).
    ///
    /// ── PBKDF2-SHA256  (contraseñas) ─────────────────────────────────────────
    ///   Hash unidireccional. Salt aleatorio 16 bytes + 100.000 iteraciones.
    ///   Formato BD: Base64( Salt[16] + Hash[32] ) = 64 caracteres.
    ///   Métodos: Hash(contraseña)  |  VerificarContraseña(ingresada, almacenada)
    ///
    /// ── AES-128-CBC  (datos sensibles) ───────────────────────────────────────
    ///   Cifrado simétrico reversible. IV aleatorio por operación (16 bytes).
    ///   Formato BD: Base64( IV[16] + CipherText[variable] )
    ///   Métodos: Encriptar(texto)  |  Desencriptar(cifrado)  |  GenerarClave()
    ///
    /// Flujo de aplicación por caso de uso (T03 §"Flujo de aplicación"):
    ///   Login / cambio clave   → Hash / VerificarContraseña  (SHA-256)
    ///   Guardar DNI cliente    → Encriptar                   (AES-128)
    ///   Leer DNI cliente       → Desencriptar / TryDesencriptar
    /// </summary>
    public static class Encriptador
    {
        // ══════════════════════════════════════════════════════════════════════
        // PBKDF2-SHA256 — Hash unidireccional para contraseñas
        // ══════════════════════════════════════════════════════════════════════

        private const int SaltSize   = 16;      // 128 bits de entropía
        private const int HashSize   = 32;      // 256 bits de salida SHA-256
        private const int Iterations = 100000;  // Costo computacional OWASP 2024

        /// <summary>
        /// Genera un hash seguro PBKDF2-SHA256 de la contraseña para almacenar en BD.
        /// Cada llamada produce un Salt distinto → hashes siempre diferentes (esperado).
        /// Método documentado como "HashSHA256" en T03.
        /// </summary>
        /// <param name="contraseña">Contraseña en texto plano ingresada por el usuario.</param>
        /// <returns>Base64 de 64 chars: Salt[16] + Hash[32].</returns>
        public static string Hash(string contraseña)
        {
            // Paso 1: Salt criptográficamente aleatorio (CSPRNG)
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(salt);

            // Paso 2: PBKDF2-SHA256 con 100.000 iteraciones
            using (var pbkdf2 = new Rfc2898DeriveBytes(
                contraseña, salt, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(HashSize);

                // Paso 3: Concatenar Salt[0..15] + Hash[16..47]
                byte[] hashBytes = new byte[SaltSize + HashSize];
                Array.Copy(salt, 0, hashBytes, 0,        SaltSize);
                Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

                // Paso 4: Base64 → VARCHAR(64) en BD
                return ConvertirBase64(hashBytes);
            }
        }

        /// <summary>
        /// Verifica si la contraseña ingresada coincide con el hash almacenado en BD.
        /// Extrae el Salt, rehashea con SHA-256 y compara byte a byte (tiempo constante).
        /// </summary>
        /// <param name="contraseñaIngresada">Texto plano del formulario de login.</param>
        /// <param name="hashAlmacenado">Base64 con Salt+Hash de la base de datos.</param>
        /// <returns>true si la contraseña es correcta; false si no coincide.</returns>
        public static bool VerificarContraseña(string contraseñaIngresada, string hashAlmacenado)
        {
            // Paso 1: Decodificar Base64 → bytes
            byte[] hashBytes = Convert.FromBase64String(hashAlmacenado);

            // Paso 2: Extraer Salt (primeros 16 bytes)
            byte[] salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            // Paso 3: Rehashear con el MISMO Salt, iteraciones y algoritmo
            using (var pbkdf2 = new Rfc2898DeriveBytes(
                contraseñaIngresada, salt, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] hashCalculado = pbkdf2.GetBytes(HashSize);

                // Paso 4: Comparación byte a byte — sección [16..47]
                for (int i = 0; i < HashSize; i++)
                    if (hashBytes[i + SaltSize] != hashCalculado[i])
                        return false;
                return true;
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // AES-128-CBC — Cifrado simétrico reversible para datos sensibles
        // ══════════════════════════════════════════════════════════════════════

        // Clave AES de 16 bytes (128 bits). En producción debe provenir de
        // un almacén de claves seguro (Windows DPAPI / Azure Key Vault).
        // Para ambiente académico se deriva de una semilla fija.
        private static readonly byte[] _claveAES = GenerarClave();

        /// <summary>
        /// Cifra un texto con AES-128-CBC usando un IV aleatorio por operación.
        /// Dos cifrados del mismo valor producen resultados distintos (IV único cada vez).
        /// Documentado como "Encriptar" en T03.
        /// </summary>
        /// <param name="texto">Texto en claro a cifrar (ej: número de DNI del cliente).</param>
        /// <returns>Base64( IV[16] + CipherText ) listo para almacenar en BD.</returns>
        public static string Encriptar(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return texto;

            using (var aes = Aes.Create())
            {
                aes.KeySize = 128;
                aes.Mode    = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key     = _claveAES;

                // IV aleatorio para esta operación (garantiza que dos cifrados del mismo
                // valor sean siempre distintos — protección contra análisis de patrones)
                aes.GenerateIV();
                byte[] iv = aes.IV;  // 16 bytes

                using (var encryptor = aes.CreateEncryptor())
                {
                    byte[] textoBytes = Encoding.UTF8.GetBytes(texto);
                    byte[] cifrado    = encryptor.TransformFinalBlock(textoBytes, 0, textoBytes.Length);

                    // Estructura: IV[16] + CipherText[variable]
                    byte[] resultado = new byte[iv.Length + cifrado.Length];
                    Array.Copy(iv,     0, resultado, 0,         iv.Length);
                    Array.Copy(cifrado, 0, resultado, iv.Length, cifrado.Length);

                    return ConvertirBase64(resultado);
                }
            }
        }

        /// <summary>
        /// Descifra un valor cifrado con AES-128-CBC.
        /// Extrae el IV de los primeros 16 bytes y aplica el descifrado inverso.
        /// Documentado como "Desencriptar" en T03.
        /// </summary>
        /// <param name="cifrado">Base64( IV[16] + CipherText ) almacenado en BD.</param>
        /// <returns>Texto en claro original.</returns>
        public static string Desencriptar(string cifrado)
        {
            if (string.IsNullOrEmpty(cifrado)) return cifrado;

            byte[] datos = Convert.FromBase64String(cifrado);

            // Extraer IV (primeros 16 bytes) y texto cifrado (resto)
            byte[] iv           = new byte[16];
            byte[] textoCifrado = new byte[datos.Length - 16];
            Array.Copy(datos, 0,  iv,           0, 16);
            Array.Copy(datos, 16, textoCifrado,  0, textoCifrado.Length);

            using (var aes = Aes.Create())
            {
                aes.KeySize = 128;
                aes.Mode    = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key     = _claveAES;
                aes.IV      = iv;

                using (var decryptor = aes.CreateDecryptor())
                {
                    byte[] textoBytes = decryptor.TransformFinalBlock(textoCifrado, 0, textoCifrado.Length);
                    return Encoding.UTF8.GetString(textoBytes);
                }
            }
        }

        /// <summary>
        /// Intenta desencriptar un valor. Si falla (dato en texto plano preexistente
        /// o formato inválido), retorna el valor original sin modificar.
        /// Permite compatibilidad hacia atrás con registros no cifrados.
        /// </summary>
        public static string TryDesencriptar(string valor)
        {
            try   { return Desencriptar(valor); }
            catch { return valor; }
        }

        /// <summary>
        /// Genera la clave AES de 16 bytes (128 bits).
        /// En producción: recuperar desde Windows DPAPI o Azure Key Vault.
        /// Documentado como "GenerarClave" en T03.
        /// </summary>
        /// <returns>Array de 16 bytes usado como clave AES-128.</returns>
        public static byte[] GenerarClave()
        {
            // Semilla fija para ambiente académico.
            // En producción: reemplazar por recuperación desde almacén externo.
            const string semilla = "WardrobeFlow2026";
            byte[] semillaBytes  = Encoding.UTF8.GetBytes(semilla);
            byte[] clave         = new byte[16];
            Array.Copy(semillaBytes, clave, Math.Min(semillaBytes.Length, 16));
            return clave;
        }

        /// <summary>
        /// Convierte un array de bytes a Base64.
        /// Auxiliar compartido por Hash y Encriptar para consistencia de formato.
        /// Documentado como "ConvertirBase64" en T03.
        /// </summary>
        /// <param name="data">Bytes a convertir.</param>
        /// <returns>Cadena Base64.</returns>
        public static string ConvertirBase64(byte[] data)
        {
            return Convert.ToBase64String(data);
        }
    }
}
