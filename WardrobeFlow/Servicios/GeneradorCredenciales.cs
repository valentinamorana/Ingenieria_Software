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

        // Alfabeto SIN caracteres ambiguos (sin O/0, I/1, etc.) para las claves de emergencia.
        private static readonly char[] CaracteresClave = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();

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

        // Genera una clave de recuperación legible: 3 grupos de 4 (ej: "A7K9-3MQP-XR2T").
        // Sin caracteres ambiguos para que el admin la transcriba sin errores.
        public static string GenerarClaveRecuperacion()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var grupos = new string[3];
                for (int g = 0; g < grupos.Length; g++)
                {
                    char[] buf = new char[4];
                    for (int i = 0; i < buf.Length; i++)
                        buf[i] = ElegirAleatorio(rng, CaracteresClave);
                    grupos[g] = new string(buf);
                }
                return string.Join("-", grupos);
            }
        }

        // Exporta el set de claves de emergencia a un .txt en CredencialesGeneradas/.
        // Devuelve la ruta del archivo generado. Es la copia física del admin (texto plano);
        // en la BD las claves se guardan hasheadas.
        public static string ExportarClavesRecuperacion(System.Collections.Generic.IList<string> claves)
        {
            string carpeta = ObtenerCarpeta();
            string nombre  = string.Format("ClavesEmergencia_{0:yyyyMMdd_HHmm}.txt", DateTime.Now);
            string ruta    = Path.Combine(carpeta, nombre);

            var sb = new StringBuilder();
            sb.AppendLine("=== CLAVES DE EMERGENCIA — WardrobeFlow ===");
            sb.AppendLine("Sirven para DESBLOQUEAR una cuenta de Administrador bloqueada.");
            sb.AppendLine("Cada clave es de UN SOLO USO. Guardá este archivo en un lugar seguro.");
            sb.AppendLine("Generadas: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
            sb.AppendLine(new string('-', 44));
            int n = 1;
            foreach (var c in claves)
                sb.AppendLine(string.Format("  {0,2}. {1}", n++, c));
            sb.AppendLine(new string('-', 44));
            sb.AppendLine("Uso: en el Login → \"¿Cuenta bloqueada? Usar clave de emergencia\".");

            File.WriteAllText(ruta, sb.ToString(), Encoding.UTF8);
            return ruta;
        }

        // Carpeta de salida de credenciales/claves: Documentos\WardrobeFlow\CredencialesGeneradas.
        // Antes se usaba la carpeta del ejecutable (bin\Debug al correr desde Visual Studio), poco
        // visible y que se pierde al recompilar/limpiar. Documentos es persistente y fácil de hallar.
        // Fallback a la carpeta del ejecutable si Documentos no estuviera disponible.
        private static string ObtenerCarpeta()
        {
            string baseDir;
            try
            {
                baseDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (string.IsNullOrEmpty(baseDir)) baseDir = AppDomain.CurrentDomain.BaseDirectory;
                else baseDir = Path.Combine(baseDir, "WardrobeFlow");
            }
            catch { baseDir = AppDomain.CurrentDomain.BaseDirectory; }

            string ruta = Path.Combine(baseDir, CarpetaCredenciales);
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
