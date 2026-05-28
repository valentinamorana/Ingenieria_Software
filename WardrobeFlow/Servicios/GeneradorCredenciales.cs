using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Servicios
{
    public static class GeneradorCredenciales
    {
        private const string CarpetaCredenciales = "CredencialesGeneradas";

        private static readonly char[] Mayusculas = "ABCDEFGHJKLMNPQRSTUVWXYZ".ToCharArray();
        private static readonly char[] Minusculas = "abcdefghjkmnpqrstuvwxyz".ToCharArray();
        private static readonly char[] Numeros    = "23456789".ToCharArray();
        private static readonly char[] Simbolos   = "!@#$%&*?".ToCharArray();
        private static readonly char[] Todos;

        static GeneradorCredenciales()
        {
            var lista = new System.Collections.Generic.List<char>();
            lista.AddRange(Mayusculas);
            lista.AddRange(Minusculas);
            lista.AddRange(Numeros);
            lista.AddRange(Simbolos);
            Todos = lista.ToArray();
        }

        // Genera una contraseña aleatoria de 10 caracteres con al menos
        // 1 mayúscula, 1 minúscula, 1 número y 1 símbolo.
        public static string GenerarContrasena()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                char[] buf = new char[10];
                buf[0] = ElegirAleatorio(rng, Mayusculas);
                buf[1] = ElegirAleatorio(rng, Minusculas);
                buf[2] = ElegirAleatorio(rng, Numeros);
                buf[3] = ElegirAleatorio(rng, Simbolos);
                for (int i = 4; i < buf.Length; i++)
                    buf[i] = ElegirAleatorio(rng, Todos);

                // Fisher-Yates shuffle para evitar posiciones predecibles
                for (int i = buf.Length - 1; i > 0; i--)
                {
                    int j = ObtenerIndice(rng, i + 1);
                    char tmp = buf[i]; buf[i] = buf[j]; buf[j] = tmp;
                }

                return new string(buf);
            }
        }

        // Exporta las credenciales a un archivo .txt en la carpeta CredencialesGeneradas.
        // Devuelve la ruta completa del archivo generado.
        public static string ExportarCredenciales(string username, string contrasena)
        {
            string carpeta = ObtenerCarpeta();
            string nombre  = string.Format("credenciales_{0}_{1:yyyyMMdd_HHmm}.txt", username, DateTime.Now);
            string ruta    = Path.Combine(carpeta, nombre);

            var sb = new StringBuilder();
            sb.AppendLine("Usuario: "    + username);
            sb.AppendLine("Contraseña: " + contrasena);
            sb.AppendLine("Fecha: "      + DateTime.Now.ToString("dd/MM/yyyy HH:mm"));

            File.WriteAllText(ruta, sb.ToString(), Encoding.UTF8);
            return ruta;
        }

        private static string ObtenerCarpeta()
        {
            string ruta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CarpetaCredenciales);
            if (!Directory.Exists(ruta))
                Directory.CreateDirectory(ruta);
            return ruta;
        }

        private static char ElegirAleatorio(RandomNumberGenerator rng, char[] chars)
        {
            return chars[ObtenerIndice(rng, chars.Length)];
        }

        private static int ObtenerIndice(RandomNumberGenerator rng, int max)
        {
            byte[] buf = new byte[4];
            rng.GetBytes(buf);
            return (int)(BitConverter.ToUInt32(buf, 0) % (uint)max);
        }
    }
}
