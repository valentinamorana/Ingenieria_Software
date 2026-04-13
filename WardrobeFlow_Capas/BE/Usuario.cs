using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace BE
{
    public class Usuario : Persona
    {
        #region Atributos
        private int idUsuario;
        private string clave;
        private bool estado;
        private string rol; // "Administrador", "Empleado", "Usuario"
        private List<Permiso> permisos;
        #endregion

        #region Propiedades
        public int IdUsuario { get { return idUsuario; } set { idUsuario = value; } }
        public string Rol { get { return rol; } set { rol = value; } }
        public bool Estado { get { return estado; } set { estado = value; } }
        public void SetClave(string clave) { this.clave = clave; }
        public string GetClave() { return this.clave; }
        public void SetPermisos(List<Permiso> permisos) { this.permisos = permisos; }
        public List<Permiso> GetPermisos() { return this.permisos; }
        #endregion

        #region Metodos - T03 Encriptado (SHA256 + Salt)
        public static string GenerarClaveHash(string clave)
        {
            string salto = GenerarSaltoAleatorio();
            string claveSalto = clave + salto;
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(claveSalto));
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                    sb.Append(bytes[i].ToString("x2"));
                return sb.ToString() + "|" + salto;
            }
        }

        public bool VerificarClave(string claveIngresada)
        {
            string[] partes = this.clave.Split('|');
            if (partes.Length \!= 2) return false;
            string hashGuardado = partes[0];
            string salto = partes[1];
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(claveIngresada + salto));
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                    sb.Append(bytes[i].ToString("x2"));
                return sb.ToString() == hashGuardado;
            }
        }

        private static string GenerarSaltoAleatorio()
        {
            string salto = string.Empty;
            Random r = new Random();
            int longitud = r.Next(10, 20);
            char c;
            for (int i = 0; i < longitud; i++)
            {
                do { c = (char)r.Next(33, 126); } while (c == '|');
                salto += c;
            }
            return salto;
        }
        #endregion
    }
}
