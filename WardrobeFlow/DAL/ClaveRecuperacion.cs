using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    /// <summary>
    /// Capa de Acceso a Datos — Claves de emergencia (tabla ClaveRecuperacion).
    /// Las claves se guardan HASHEADAS (PBKDF2). Esta capa nunca ve la clave en texto plano:
    /// recibe/almacena hashes y controla el consumo de uso único.
    /// </summary>
    public class ClaveRecuperacion : Interfaces.IClaveRecuperacionDAL
    {
        private readonly Acceso acceso = Acceso.GetInstance();

        // Inserta una clave (ya hasheada por la capa de negocio).
        public void Insertar(string claveHash)
        {
            acceso.Escribir(
                "INSERT INTO ClaveRecuperacion (ClaveHash, Usada, FechaCreacion) VALUES (@h, 0, GETDATE())",
                new SqlParameter[] { new SqlParameter("@h", claveHash) });
        }

        // Total de claves cargadas (usadas + disponibles).
        public int ContarTotal()
        {
            DataTable t = acceso.Leer("SELECT COUNT(*) AS N FROM ClaveRecuperacion", null);
            return t.Rows.Count == 0 ? 0 : Convert.ToInt32(t.Rows[0]["N"]);
        }

        // Claves todavía disponibles (no consumidas).
        public int ContarDisponibles()
        {
            DataTable t = acceso.Leer("SELECT COUNT(*) AS N FROM ClaveRecuperacion WHERE Usada = 0", null);
            return t.Rows.Count == 0 ? 0 : Convert.ToInt32(t.Rows[0]["N"]);
        }

        // (IdClave, ClaveHash) de las claves NO usadas, para verificar contra la clave ingresada.
        public List<KeyValuePair<int, string>> ObtenerDisponibles()
        {
            var lista = new List<KeyValuePair<int, string>>();
            DataTable t = acceso.Leer(
                "SELECT IdClave, ClaveHash FROM ClaveRecuperacion WHERE Usada = 0 ORDER BY IdClave", null);
            foreach (DataRow row in t.Rows)
                lista.Add(new KeyValuePair<int, string>(
                    Convert.ToInt32(row["IdClave"]), row["ClaveHash"].ToString()));
            return lista;
        }

        // Marca una clave como usada (uso único). La condición Usada=0 en el WHERE evita el
        // doble consumo aunque dos intentos lleguen casi simultáneos. Devuelve true si la consumió.
        public bool MarcarUsada(int idClave, string username)
        {
            int filas = acceso.Escribir(
                "UPDATE ClaveRecuperacion SET Usada = 1, UsadaPor = @u, FechaUso = GETDATE() " +
                "WHERE IdClave = @id AND Usada = 0",
                new SqlParameter[]
                {
                    new SqlParameter("@id", idClave),
                    new SqlParameter("@u",  (object)username ?? DBNull.Value)
                });
            return filas > 0;
        }

        // Borra todo el set (para regenerar claves desde cero).
        public void EliminarTodas()
        {
            acceso.Escribir("DELETE FROM ClaveRecuperacion", null);
        }
    }
}
