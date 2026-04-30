using System;
using System.Security.Cryptography;
using System.Text;

namespace Seguridad
{
    /// <summary>
    /// Centraliza la criptografia del sistema.
    /// PBKDF2-SHA256 para contrasenas (unidireccional) y AES-128-CBC para datos sensibles (reversible).
    /// </summary>
    public static class Encriptador
    {
        // ── PBKDF2-SHA256 — hash unidireccional para contrasenas ──────────────

        private const int SaltSize   = 16;
        private const int HashSize   = 32;
        private const int Iterations = 100000;

        /// <summary>
        /// Genera un hash de la contrasena para guardar en BD.
        /// Formato: Base64( Salt[16] + Hash[32] ).
        /// </summary>
        public static string Hash(string contrasena)
        {
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(salt);

            using (var pbkdf2 = new Rfc2898DeriveBytes(
                contrasena, salt, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(HashSize);

                byte[] hashBytes = new byte[SaltSize + HashSize];
                Array.Copy(salt, 0, hashBytes, 0,        SaltSize);
                Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

                return ConvertirBase64(hashBytes);
            }
        }

        /// <summary>
        /// Verifica si la contrasena ingresada coincide con el hash almacenado en BD.
        /// Extrae el Salt, rehashea y compara byte a byte.
        /// </summary>
        public static bool VerificarContrasena(string contrasenaIngresada, string hashAlmacenado)
        {
            byte[] hashBytes = Convert.FromBase64String(hashAlmacenado);

            byte[] salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            using (var pbkdf2 = new Rfc2898DeriveBytes(
                contrasenaIngresada, salt, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] hashCalculado = pbkdf2.GetBytes(HashSize);

                for (int i = 0; i < HashSize; i++)
                    if (hashBytes[i + SaltSize] != hashCalculado[i])
                        return false;
                return true;
            }
        }

        // ── AES-128-CBC — cifrado reversible para datos sensibles (ej: DNI) ───

        // Clave fija de 16 bytes derivada de una semilla. En produccion deberia
        // venir de un almacen seguro (Windows DPAPI / Azure Key Vault).
        private static readonly byte[] _claveAES = GenerarClave();

        /// <summary>
        /// Cifra un texto con AES-128-CBC usando un IV aleatorio por operacion.
        /// Formato: Base64( IV[16] + CipherText ).
        /// </summary>
        public static string Encriptar(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return texto;

            using (var aes = Aes.Create())
            {
                aes.KeySize = 128;
                aes.Mode    = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key     = _claveAES;
                aes.GenerateIV();
                byte[] iv = aes.IV;

                using (var encryptor = aes.CreateEncryptor())
                {
                    byte[] textoBytes = Encoding.UTF8.GetBytes(texto);
                    byte[] cifrado    = encryptor.TransformFinalBlock(textoBytes, 0, textoBytes.Length);

                    byte[] resultado = new byte[iv.Length + cifrado.Length];
                    Array.Copy(iv,      0, resultado, 0,         iv.Length);
                    Array.Copy(cifrado, 0, resultado, iv.Length, cifrado.Length);

                    return ConvertirBase64(resultado);
                }
            }
        }

        /// <summary>Descifra un valor cifrado con AES-128-CBC.</summary>
        public static string Desencriptar(string cifrado)
        {
            if (string.IsNullOrEmpty(cifrado)) return cifrado;

            byte[] datos = Convert.FromBase64String(cifrado);

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
        /// Intenta desencriptar. Si falla (dato en texto plano o formato invalido),
        /// devuelve el valor original sin modificar.
        /// </summary>
        public static string TryDesencriptar(string valor)
        {
            try   { return Desencriptar(valor); }
            catch { return valor; }
        }

        /// <summary>Genera la clave AES de 16 bytes a partir de una semilla fija.</summary>
        public static byte[] GenerarClave()
        {
            const string semilla = "WardrobeFlow2026";
            byte[] semillaBytes  = Encoding.UTF8.GetBytes(semilla);
            byte[] clave         = new byte[16];
            Array.Copy(semillaBytes, clave, Math.Min(semillaBytes.Length, 16));
            return clave;
        }

        /// <summary>Convierte un array de bytes a Base64.</summary>
        public static string ConvertirBase64(byte[] data)
        {
            return Convert.ToBase64String(data);
        }
    }
}
