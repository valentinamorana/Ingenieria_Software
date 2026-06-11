using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    /// <summary>
    /// Capa de Acceso a Datos — Etapa 4 (permisos a nivel de control).
    /// Lee/escribe la tabla [ControlMapeado] (patente ↔ control de un formulario).
    /// </summary>
    public class ControlMapeado : DAL.Interfaces.IControlMapeadoDAL
    {
        private readonly Acceso acceso = Acceso.GetInstance();

        public List<BE.ControlMapeado> ObtenerTodos()
        {
            var lista = new List<BE.ControlMapeado>();
            DataTable dt = acceso.Leer(
                "SELECT IdControlMapeado, IdPermiso, Formulario, NombreControl FROM ControlMapeado", null);
            if (dt == null) return lista;
            foreach (DataRow row in dt.Rows) lista.Add(Mapear(row));
            return lista;
        }

        public List<BE.ControlMapeado> ObtenerPorPermiso(int idPermiso)
        {
            var lista = new List<BE.ControlMapeado>();
            DataTable dt = acceso.Leer(
                "SELECT IdControlMapeado, IdPermiso, Formulario, NombreControl " +
                "FROM ControlMapeado WHERE IdPermiso = @id",
                new SqlParameter[] { new SqlParameter("@id", idPermiso) });
            if (dt == null) return lista;
            foreach (DataRow row in dt.Rows) lista.Add(Mapear(row));
            return lista;
        }

        // Reemplazo atómico: borra los mapeos previos de la patente e inserta el set nuevo.
        public void GuardarAsociados(int idPermiso, List<BE.ControlMapeado> controles)
        {
            acceso.EjecutarTransaccion((conn, tx) =>
            {
                using (var del = new SqlCommand("DELETE FROM ControlMapeado WHERE IdPermiso = @id", conn, tx))
                {
                    del.Parameters.AddWithValue("@id", idPermiso);
                    del.ExecuteNonQuery();
                }

                if (controles == null) return;
                foreach (var c in controles)
                {
                    using (var ins = new SqlCommand(
                        "INSERT INTO ControlMapeado (IdPermiso, Formulario, NombreControl) " +
                        "VALUES (@id, @form, @ctrl)", conn, tx))
                    {
                        ins.Parameters.AddWithValue("@id",   idPermiso);
                        ins.Parameters.AddWithValue("@form", c.Formulario ?? string.Empty);
                        ins.Parameters.AddWithValue("@ctrl", c.NombreControl ?? string.Empty);
                        ins.ExecuteNonQuery();
                    }
                }
            });
        }

        private static BE.ControlMapeado Mapear(DataRow row) => new BE.ControlMapeado
        {
            Id            = Convert.ToInt32(row["IdControlMapeado"]),
            IdPermiso     = Convert.ToInt32(row["IdPermiso"]),
            Formulario    = row["Formulario"].ToString(),
            NombreControl = row["NombreControl"].ToString()
        };
    }
}
