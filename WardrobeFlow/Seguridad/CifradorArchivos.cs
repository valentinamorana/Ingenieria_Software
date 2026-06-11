using System.IO;
using System.Security.Cryptography;

namespace Seguridad
{
    /// <summary>
    /// Cifrado de ARCHIVOS con contraseña, para backups portables (patrón tomado del BackupService
    /// de Stach). Deriva una clave AES-128 de la contraseña con PBKDF2-SHA256 y cifra en AES-CBC.
    ///
    /// Formato del archivo de salida:  [ salt(16) ][ IV(16) ][ ciphertext ]
    ///
    /// A diferencia de <see cref="Encriptador"/> (que usa la clave de máquina en key.dat, no
    /// portable), acá la clave depende SOLO de la contraseña, de modo que el backup se puede
    /// restaurar en otra máquina conociendo la contraseña. Una contraseña incorrecta produce
    /// una CryptographicException al descifrar (padding inválido), que la capa superior traduce
    /// a un mensaje claro para el usuario.
    /// </summary>
    public static class CifradorArchivos
    {
        private const int SaltSize   = 16;
        private const int IvSize     = 16;
        private const int KeySize    = 16;      // AES-128
        private const int Iterations = 100000;

        private static byte[] DerivarClave(string password, byte[] salt)
        {
            using (var kdf = new Rfc2898DeriveBytes(password ?? string.Empty, salt, Iterations, HashAlgorithmName.SHA256))
                return kdf.GetBytes(KeySize);
        }

        /// <summary>Cifra rutaOrigen → rutaDestino con la contraseña dada (streaming, soporta archivos grandes).</summary>
        public static void Cifrar(string rutaOrigen, string rutaDestino, string password)
        {
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(salt);

            using (var aes = Aes.Create())
            {
                aes.KeySize = 128;
                aes.Mode    = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key     = DerivarClave(password, salt);
                aes.GenerateIV();

                using (var fsOut = new FileStream(rutaDestino, FileMode.Create, FileAccess.Write))
                {
                    fsOut.Write(salt,   0, salt.Length);
                    fsOut.Write(aes.IV, 0, aes.IV.Length);
                    using (var encryptor = aes.CreateEncryptor())
                    using (var cs = new CryptoStream(fsOut, encryptor, CryptoStreamMode.Write))
                    using (var fsIn = new FileStream(rutaOrigen, FileMode.Open, FileAccess.Read))
                        fsIn.CopyTo(cs);
                }
            }
        }

        /// <summary>
        /// Descifra rutaOrigen → rutaDestino. Lanza CryptographicException si la contraseña es
        /// incorrecta o el archivo está dañado.
        /// </summary>
        public static void Descifrar(string rutaOrigen, string rutaDestino, string password)
        {
            using (var fsIn = new FileStream(rutaOrigen, FileMode.Open, FileAccess.Read))
            {
                byte[] salt = LeerExacto(fsIn, SaltSize);
                byte[] iv   = LeerExacto(fsIn, IvSize);

                using (var aes = Aes.Create())
                {
                    aes.KeySize = 128;
                    aes.Mode    = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Key     = DerivarClave(password, salt);
                    aes.IV      = iv;

                    using (var decryptor = aes.CreateDecryptor())
                    using (var cs = new CryptoStream(fsIn, decryptor, CryptoStreamMode.Read))
                    using (var fsOut = new FileStream(rutaDestino, FileMode.Create, FileAccess.Write))
                        cs.CopyTo(fsOut);   // contraseña incorrecta → CryptographicException acá
                }
            }
        }

        private static byte[] LeerExacto(Stream s, int n)
        {
            byte[] buf = new byte[n];
            int off = 0;
            while (off < n)
            {
                int r = s.Read(buf, off, n - off);
                if (r == 0) throw new CryptographicException("El archivo de backup es inválido o está dañado.");
                off += r;
            }
            return buf;
        }
    }
}
