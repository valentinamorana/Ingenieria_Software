using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class Idioma
    {
        private readonly Acceso acceso = Acceso.GetInstance();

        public List<BE.Idioma> ObtenerActivos()
        {
            var lista = new List<BE.Idioma>();
            DataTable dt = acceso.Leer(
                "SELECT IdIdioma, Codigo, Nombre, Activo, EsDefault " +
                "FROM Idioma WHERE Activo = 1 ORDER BY EsDefault DESC, Nombre", null);

            foreach (DataRow row in dt.Rows)
                lista.Add(MapearFila(row));

            return lista;
        }

        public List<BE.Idioma> ObtenerTodos()
        {
            var lista = new List<BE.Idioma>();
            DataTable dt = acceso.Leer(
                "SELECT IdIdioma, Codigo, Nombre, Activo, EsDefault " +
                "FROM Idioma ORDER BY EsDefault DESC, Nombre", null);

            foreach (DataRow row in dt.Rows)
                lista.Add(MapearFila(row));

            return lista;
        }

        public int ObtenerOCrearPorCodigo(string codigo, string nombre)
        {
            var p = new SqlParameter[] { new SqlParameter("@Codigo", codigo) };
            DataTable dt = acceso.Leer(
                "SELECT IdIdioma FROM Idioma WHERE Codigo = @Codigo", p);

            if (dt.Rows.Count > 0)
                return Convert.ToInt32(dt.Rows[0]["IdIdioma"]);

            // No debería ocurrir si el script SQL ya insertar los 3 idiomas
            var pIns = new SqlParameter[]
            {
                new SqlParameter("@Codigo",    codigo),
                new SqlParameter("@Nombre",    nombre),
                new SqlParameter("@Activo",    true),
                new SqlParameter("@EsDefault", false)
            };
            acceso.Escribir(
                "INSERT INTO Idioma (Codigo, Nombre, Activo, EsDefault) " +
                "VALUES (@Codigo, @Nombre, @Activo, @EsDefault)", pIns);

            DataTable dtNew = acceso.Leer(
                "SELECT IdIdioma FROM Idioma WHERE Codigo = @Codigo",
                new SqlParameter[] { new SqlParameter("@Codigo", codigo) });

            return Convert.ToInt32(dtNew.Rows[0]["IdIdioma"]);
        }

        public void Activar(int idIdioma)
        {
            acceso.Escribir("UPDATE Idioma SET Activo = 1 WHERE IdIdioma = @id",
                new SqlParameter[] { new SqlParameter("@id", idIdioma) });
        }

        public void Desactivar(int idIdioma)
        {
            // No desactivar el idioma por defecto
            acceso.Escribir(
                "UPDATE Idioma SET Activo = 0 WHERE IdIdioma = @id AND EsDefault = 0",
                new SqlParameter[] { new SqlParameter("@id", idIdioma) });
        }

        private static BE.Idioma MapearFila(DataRow row) => new BE.Idioma
        {
            IdIdioma  = Convert.ToInt32(row["IdIdioma"]),
            Codigo    = row["Codigo"].ToString(),
            Nombre    = row["Nombre"].ToString(),
            Activo    = Convert.ToBoolean(row["Activo"]),
            EsDefault = Convert.ToBoolean(row["EsDefault"])
        };
    }
}
