using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    /// <summary>
    /// T05 — Acceso a datos de la entidad CONTROL (textos traducibles del sistema).
    /// </summary>
    public class Control
    {
        private readonly Acceso acceso = Acceso.GetInstance();

        // Todos los controles traducibles, para el grid de Controles del FormIdiomas.
        public List<BE.Control> ObtenerTodos()
        {
            var lista = new List<BE.Control>();
            DataTable dt = acceso.Leer(
                "SELECT IdControl, Clave, Formulario FROM Control ORDER BY Formulario, Clave", null);
            if (dt == null) return lista;
            foreach (DataRow row in dt.Rows)
                lista.Add(new BE.Control
                {
                    IdControl  = Convert.ToInt32(row["IdControl"]),
                    Clave      = row["Clave"].ToString(),
                    Formulario = row["Formulario"].ToString()
                });
            return lista;
        }

        // Obtiene o crea un Control por su clave y devuelve su ID.
        public int ObtenerOCrear(string clave, string formulario)
        {
            DataTable dt = acceso.Leer(
                "SELECT IdControl FROM Control WHERE Clave = @Clave",
                new[] { new SqlParameter("@Clave", clave) });
            if (dt.Rows.Count > 0)
                return Convert.ToInt32(dt.Rows[0]["IdControl"]);

            acceso.Escribir(
                "INSERT INTO Control (Clave, Formulario) VALUES (@Clave, @Formulario)",
                new[]
                {
                    new SqlParameter("@Clave",      clave),
                    new SqlParameter("@Formulario", formulario ?? "General")
                });

            DataTable dtNew = acceso.Leer(
                "SELECT IdControl FROM Control WHERE Clave = @Clave",
                new[] { new SqlParameter("@Clave", clave) });
            return Convert.ToInt32(dtNew.Rows[0]["IdControl"]);
        }
    }
}
