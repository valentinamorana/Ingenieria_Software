using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BE;

namespace DAL
{
    public class DAL_Prenda
    {
        public List<Prenda> ListarPrendas()
        {
            List<Prenda> lista = new List<Prenda>();
            SqlConnection con = DAL_Conexion.Instancia.ObtenerConexion();
            try
            {
                string sql = @"SELECT p.IdPrenda, p.Nombre, p.Color, p.Talla, p.Temporada,
                                      p.IdCategoria, p.Estado, p.FechaRegistro,
                                      c.Nombre AS NombreCategoria
                               FROM Prenda p
                               INNER JOIN Categoria c ON p.IdCategoria = c.IdCategoria";
                SqlCommand cmd = new SqlCommand(sql, con);
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    Prenda p = new Prenda();
                    p.IdPrenda    = (int)dr["IdPrenda"];
                    p.Nombre      = dr["Nombre"].ToString();
                    p.Color       = dr["Color"].ToString();
                    p.Talla       = dr["Talla"].ToString();
                    p.Temporada   = dr["Temporada"].ToString();
                    p.IdCategoria = (int)dr["IdCategoria"];
                    p.Estado      = (bool)dr["Estado"];
                    p.FechaRegistro = (System.DateTime)dr["FechaRegistro"];
                    Categoria cat = new Categoria();
                    cat.Nombre    = dr["NombreCategoria"].ToString();
                    p.OCategoria  = cat;
                    lista.Add(p);
                }
                dr.Close();
            }
            finally { DAL_Conexion.Instancia.CerrarConexion(); }
            return lista;
        }

        public string AgregarPrenda(Prenda p)
        {
            SqlConnection con = DAL_Conexion.Instancia.ObtenerConexion();
            try
            {
                SqlCommand cmd = new SqlCommand("SP_AgregarPrenda", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Nombre",      p.Nombre);
                cmd.Parameters.AddWithValue("@Color",       p.Color);
                cmd.Parameters.AddWithValue("@Talla",       p.Talla);
                cmd.Parameters.AddWithValue("@Temporada",   p.Temporada);
                cmd.Parameters.AddWithValue("@IdCategoria", p.IdCategoria);
                cmd.Parameters.Add("@Mensaje", SqlDbType.VarChar, 200).Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                return cmd.Parameters["@Mensaje"].Value.ToString();
            }
            finally { DAL_Conexion.Instancia.CerrarConexion(); }
        }

        public string EditarPrenda(Prenda p)
        {
            SqlConnection con = DAL_Conexion.Instancia.ObtenerConexion();
            try
            {
                SqlCommand cmd = new SqlCommand("SP_EditarPrenda", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdPrenda",    p.IdPrenda);
                cmd.Parameters.AddWithValue("@Nombre",      p.Nombre);
                cmd.Parameters.AddWithValue("@Color",       p.Color);
                cmd.Parameters.AddWithValue("@Talla",       p.Talla);
                cmd.Parameters.AddWithValue("@Temporada",   p.Temporada);
                cmd.Parameters.AddWithValue("@IdCategoria", p.IdCategoria);
                cmd.Parameters.AddWithValue("@Estado",      p.Estado);
                cmd.Parameters.Add("@Mensaje", SqlDbType.VarChar, 200).Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                return cmd.Parameters["@Mensaje"].Value.ToString();
            }
            finally { DAL_Conexion.Instancia.CerrarConexion(); }
        }

        public string EliminarPrenda(int id)
        {
            SqlConnection con = DAL_Conexion.Instancia.ObtenerConexion();
            try
            {
                SqlCommand cmd = new SqlCommand("SP_EliminarPrenda", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdPrenda", id);
                cmd.Parameters.Add("@Mensaje", SqlDbType.VarChar, 200).Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                return cmd.Parameters["@Mensaje"].Value.ToString();
            }
            finally { DAL_Conexion.Instancia.CerrarConexion(); }
        }
    }
}
