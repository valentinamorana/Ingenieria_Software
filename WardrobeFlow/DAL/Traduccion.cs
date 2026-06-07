using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class Traduccion
    {
        private readonly Acceso acceso = Acceso.GetInstance();

        // Devuelve todas las traducciones de un idioma como diccionario clave→texto.
        // Usado por BLL para cargar el cache en memoria antes de notificar observers.
        public Dictionary<string, string> ObtenerDiccionario(string codigoIdioma)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var p = new SqlParameter[] { new SqlParameter("@Codigo", codigoIdioma) };

            DataTable dt = acceso.Leer(
                "SELECT c.Clave, t.Texto " +
                "FROM Traduccion t " +
                "JOIN Control c ON t.IdControl = c.IdControl " +
                "JOIN Idioma  i ON t.IdIdioma  = i.IdIdioma " +
                "WHERE i.Codigo = @Codigo", p);

            foreach (DataRow row in dt.Rows)
                dict[row["Clave"].ToString()] = row["Texto"].ToString();

            return dict;
        }

        // Devuelve filas completas para mostrar en el FormIdiomas (DataGridView).
        // LEFT JOIN desde Control: aparecen TODAS las claves del sistema; las que aún no tienen
        // traducción en este idioma vienen con Texto vacío. Así un idioma recién creado muestra
        // toda la grilla (ID/Clave/Formulario) lista para que un traductor complete los textos a mano.
        public List<BE.FilaTraduccion> ObtenerPorIdioma(int idIdioma)
        {
            var lista = new List<BE.FilaTraduccion>();
            var p = new SqlParameter[] { new SqlParameter("@IdIdioma", idIdioma) };

            DataTable dt = acceso.Leer(
                "SELECT c.IdControl, @IdIdioma AS IdIdioma, c.Clave, c.Formulario, " +
                "       ISNULL(t.Texto, '') AS Texto " +
                "FROM Control c " +
                "LEFT JOIN Traduccion t ON t.IdControl = c.IdControl AND t.IdIdioma = @IdIdioma " +
                "ORDER BY c.Formulario, c.Clave", p);

            foreach (DataRow row in dt.Rows)
                lista.Add(new BE.FilaTraduccion
                {
                    IdControl  = Convert.ToInt32(row["IdControl"]),
                    IdIdioma   = Convert.ToInt32(row["IdIdioma"]),
                    Clave      = row["Clave"].ToString(),
                    Formulario = row["Formulario"].ToString(),
                    Texto      = row["Texto"].ToString()
                });

            return lista;
        }

        // Guarda (UPSERT) el texto de una traducción. Si la fila no existía aún para ese
        // idioma (caso típico de un idioma nuevo que se está traduciendo por primera vez),
        // la inserta; si existía, la actualiza.
        public void GuardarTraduccion(int idControl, int idIdioma, string texto)
        {
            var p = new SqlParameter[]
            {
                new SqlParameter("@IdControl", idControl),
                new SqlParameter("@IdIdioma",  idIdioma),
                new SqlParameter("@Texto",     texto ?? "")
            };
            acceso.Escribir(
                "IF EXISTS (SELECT 1 FROM Traduccion WHERE IdControl = @IdControl AND IdIdioma = @IdIdioma) " +
                "    UPDATE Traduccion SET Texto = @Texto WHERE IdControl = @IdControl AND IdIdioma = @IdIdioma " +
                "ELSE " +
                "    INSERT INTO Traduccion (IdControl, IdIdioma, Texto) VALUES (@IdControl, @IdIdioma, @Texto)", p);
        }

        // Obtiene o crea un Control por su clave y devuelve su ID.
        public int ObtenerOCrearControl(string clave, string formulario)
        {
            var p = new SqlParameter[] { new SqlParameter("@Clave", clave) };
            DataTable dt = acceso.Leer(
                "SELECT IdControl FROM Control WHERE Clave = @Clave", p);

            if (dt.Rows.Count > 0)
                return Convert.ToInt32(dt.Rows[0]["IdControl"]);

            var pIns = new SqlParameter[]
            {
                new SqlParameter("@Clave",      clave),
                new SqlParameter("@Formulario", formulario ?? "General")
            };
            acceso.Escribir(
                "INSERT INTO Control (Clave, Formulario) VALUES (@Clave, @Formulario)",
                pIns);

            DataTable dtNew = acceso.Leer(
                "SELECT IdControl FROM Control WHERE Clave = @Clave",
                new SqlParameter[] { new SqlParameter("@Clave", clave) });
            return Convert.ToInt32(dtNew.Rows[0]["IdControl"]);
        }

        // Inserta una traducción si no existe (idempotente).
        public void InsertarSiNoExiste(int idControl, int idIdioma, string texto)
        {
            var p = new SqlParameter[]
            {
                new SqlParameter("@IdControl", idControl),
                new SqlParameter("@IdIdioma",  idIdioma),
                new SqlParameter("@Texto",     texto ?? "")
            };
            acceso.Escribir(
                "IF NOT EXISTS (SELECT 1 FROM Traduccion WHERE IdControl = @IdControl AND IdIdioma = @IdIdioma) " +
                "INSERT INTO Traduccion (IdControl, IdIdioma, Texto) VALUES (@IdControl, @IdIdioma, @Texto)",
                p);
        }

        public bool HayTraducciones(string codigoIdioma)
        {
            var p = new SqlParameter[] { new SqlParameter("@Codigo", codigoIdioma) };
            DataTable dt = acceso.Leer(
                "SELECT TOP 1 t.IdControl FROM Traduccion t " +
                "JOIN Idioma i ON t.IdIdioma = i.IdIdioma " +
                "WHERE i.Codigo = @Codigo", p);
            return dt.Rows.Count > 0;
        }
    }
}
