using System;
using System.Security.Cryptography;
using System.Text;

namespace Seguridad
{
    // Utilidad estatica para encriptar contrasenas con SHA256.
    // Basada en EncriptadorSL del ejemplo de referencia (Nacho/Codigo).
    // Se cambio de MD5 a SHA256 por mayor seguridad.
    public static class Encriptador
    {
        // Genera el hash SHA256 de un texto plano.
        // Parametro: texto - la contrasena en texto plano
        // Retorna: el hash como cadena hexadecimal (64 caracteres)
        public static string Hash(string texto)
        {
            if (string.IsNullOrEmpty(texto))
                return string.Empty;

            // Crear el proveedor SHA256
            using (SHA256 sha256 = SHA256.Create())
            {
                // Convertir el texto a bytes y calcular el hash
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(texto));

                // Convertir los bytes del hash a cadena hexadecimal
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    sb.Append(bytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

        // Verifica si un texto plano coincide con un hash SHA256 almacenado.
        // Parametros: texto - texto a verificar, hash - hash almacenado
        // Retorna: true si coinciden, false si no
        public static bool Verificar(string texto, string hash)
        {
            if (string.IsNullOrEmpty(texto) || string.IsNullOrEmpty(hash))
                return false;

            // Recalcular el hash del texto y comparar
            string hashDelTexto = Hash(texto);
            return hashDelTexto.Equals(hash, StringComparison.OrdinalIgnoreCase);
        }
    }
}
