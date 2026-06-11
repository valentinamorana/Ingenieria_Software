using System;
using System.IO;
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

                // Comparación en TIEMPO CONSTANTE: recorre siempre los 32 bytes y acumula
                // las diferencias con XOR, en vez de cortar en el primer byte distinto. Así
                // el tiempo de respuesta no filtra cuántos bytes coincidieron (canal lateral).
                int diferencia = 0;
                for (int i = 0; i < HashSize; i++)
                    diferencia |= hashBytes[i + SaltSize] ^ hashCalculado[i];
                return diferencia == 0;
            }
        }

        // Hash "señuelo" precalculado una sola vez. Se usa para verificar contra un hash
        // VÁLIDO cuando el usuario no existe, de modo que el login consuma el mismo tiempo
        // de PBKDF2 que con un usuario real y no se pueda enumerar usuarios por temporización.
        private static readonly string _hashSenuelo = Hash("\0senuelo-sin-usuario\0");

        /// <summary>
        /// Ejecuta una verificación PBKDF2 contra el hash señuelo. Siempre devuelve false;
        /// su único objetivo es igualar el costo temporal del camino "usuario inexistente".
        /// </summary>
        public static bool VerificacionSenuelo(string contrasena)
        {
            return VerificarContrasena(contrasena, _hashSenuelo);
        }

        // ── Validacion de requisitos de contraseña ────────────────────────────

        // Requisitos: minimo 8 caracteres, al menos 1 numero y 1 caracter especial.
        public static (bool valida, string mensaje) ValidarContrasena(string contrasena)
        {
            if (string.IsNullOrWhiteSpace(contrasena) || contrasena.Length < 8)
                return (false, "La contrasena debe tener al menos 8 caracteres.");

            bool tieneNumero   = false;
            bool tieneEspecial = false;
            const string especiales = "!@#$%^&*()_+-=[]{}|;:',.<>?/";

            foreach (char c in contrasena)
            {
                if (char.IsDigit(c))            tieneNumero   = true;
                if (especiales.IndexOf(c) >= 0) tieneEspecial = true;
            }

            if (!tieneNumero)
                return (false, "La contrasena debe contener al menos un numero.");
            if (!tieneEspecial)
                return (false, "La contrasena debe contener al menos un caracter especial (!@#$%...).");

            return (true, string.Empty);
        }

        // ── AES-128-CBC — cifrado reversible para datos sensibles (ej: DNI) ───

        // Clave AES cargada (o generada la primera vez) desde key.dat junto al ejecutable.
        // El archivo nunca se sube al repositorio (.gitignore).
        private static readonly byte[] _claveAES = CargarOCrearClave();

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
        /// Solo captura excepciones criptográficas y de formato, no errores del sistema.
        /// </summary>
        public static string TryDesencriptar(string valor)
        {
            try { return Desencriptar(valor); }
            catch (CryptographicException) { return valor; }
            catch (FormatException)        { return valor; }
            catch (ArgumentException)      { return valor; }
        }

        // Carga la clave AES desde key.dat, PROTEGIDA con DPAPI (ProtectedData, ámbito del
        // usuario actual). Migra automáticamente un key.dat legacy en texto plano sin cambiar
        // la clave (para no invalidar los datos ya cifrados).
        // ⚠ Eliminar key.dat hace que los DNI cifrados existentes sean irrecuperables.
        private static byte[] CargarOCrearClave()
        {
            string ruta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "key.dat");

            if (File.Exists(ruta))
            {
                byte[] bytes = null;
                try { bytes = Convert.FromBase64String(File.ReadAllText(ruta).Trim()); }
                catch { }

                if (bytes != null)
                {
                    // 1) Caso normal: el archivo es un blob DPAPI → desproteger.
                    try
                    {
                        byte[] clave = ProtectedData.Unprotect(bytes, null, DataProtectionScope.CurrentUser);
                        if (clave.Length == 16) return clave;
                    }
                    catch { }

                    // 2) Legacy: clave plana de 16 bytes → migrar a DPAPI conservando la MISMA clave.
                    if (bytes.Length == 16)
                    {
                        GuardarClaveProtegida(ruta, bytes);
                        return bytes;
                    }
                }
            }

            byte[] nueva = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(nueva);

            GuardarClaveProtegida(ruta, nueva);
            return nueva;
        }

        // Persiste la clave AES protegida con DPAPI (Base64 del blob protegido).
        private static void GuardarClaveProtegida(string ruta, byte[] clave)
        {
            byte[] protegida = ProtectedData.Protect(clave, null, DataProtectionScope.CurrentUser);
            File.WriteAllText(ruta, Convert.ToBase64String(protegida));
        }

        /// <summary>Convierte un array de bytes a Base64.</summary>
        public static string ConvertirBase64(byte[] data)
        {
            return Convert.ToBase64String(data);
        }
    }
}
