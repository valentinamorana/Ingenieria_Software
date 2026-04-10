using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BE;

namespace DAL
{
    public class DAL_Categoria
    {
        public List<Categoria> ListarCategorias()
        {
            List<Categoria> lista = new List<Categoria>();
            SqlConnection con = DAL_Conexion.Instancia.ObtenerConexion();
            try
            {
                SqlCommand cmd = new SqlCommand("SELECT IdCategoria, Nombre, Descripcion, Estado FROM Categoria", con);
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    Categoria c = new Categoria();
                    c.IdCategoria  = (int)dr["IdCategoria"];
                    c.Nombre       = dr["Nombre"].ToString();
                    c.Descripcion  = dr["Descripcion"].ToString();
                    c.Estado       = (bool)dr["Estado"];
                    lista.Add(c);
                }
                dr.Close();
            }
            finally { DAL_Conexion.Instancia.CerrarConexion(); }
            return lista;
        }

        public string AgregarCategoria(Categoria c)
        {
            SqlConnection con = DAL_Conexion.Instancia.ObtenerConexion();
            try
            {
                SqlCommand cmd = new SqlCommand("SP_AgregarCategoria", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Nombre",      c.Nombre);
                cmd.Parameters.AddWithValue("@Descripcion", c.Descripcion);
                cmd.Parameters.Add("@Mensaje", SqlDbType.VarChar, 200).Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                return cmd.Parameters["@Mensaje"].Value.ToString();
            }
            finally { DAL_Conexion.Instancia.CerrarConexion(); }
        }

        public string EditarCategoria(Categoria c)
        {
            SqlConnection con = DAL_Conexion.Instancia.ObtenerConexion();
            try
            {
                SqlCommand cmd = new SqlCommand("SP_EditarCategoria", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdCategoria", c.IdCategoria);
                cmd.Parameters.AddWithValue("@Nombre",      c.Nombre);
                cmd.Parameters.AddWithValue("@Descripcion", c.Descripcion);
                cmd.Parameters.AddWithValue("@Estado",      c.Estado);
                cmd.Parameters.Add("@Mensaje", SqlDbType.VarChar, 200).Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                return cmd.Parameters["@Mensaje"].Value.ToString();
            }
            finally { DAL_Conexion.Instancia.CerrarConexion(); }
        }

        public string EliminarCategoria(int id)
        {
            SqlConnection con = DAL_Conexion.Instancia.ObtenerConexion();
            try
            {
                SqlCommand cmd = new SqlCommand("SP_EliminarCategoria", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdCategoria", id);
                cmd.Parameters.Add("@Mensaje", SqlDbType.VarChar, 200).Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                return cmd.Parameters["@Mensaje"].Value.ToString();
            }
            finally { DAL_Conexion.Instancia.CerrarConexion(); }
        }
    }
}
