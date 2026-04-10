using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BE;

namespace DAL
{
    public class DAL_Usuario
    {
        public List<Usuario> ListarUsuarios()
        {
            List<Usuario> lista = new List<Usuario>();
            SqlConnection con = DAL_Conexion.Instancia.ObtenerConexion();
            try
            {
                string sql = @"SELECT u.IdUsuario, u.Clave, u.Estado, u.Rol,
                                      p.IdPersona, p.NombreCompleto, p.Correo, p.Documento
                               FROM Usuario u
                               INNER JOIN Persona p ON u.IdPersona = p.IdPersona";
                SqlCommand cmd = new SqlCommand(sql, con);
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    Usuario u = new Usuario();
                    u.IdUsuario   = (int)dr["IdUsuario"];
                    u.SetClave(dr["Clave"].ToString());
                    u.Estado      = (bool)dr["Estado"];
                    u.Rol         = dr["Rol"].ToString();
                    u.IdPersona   = (int)dr["IdPersona"];
                    u.NombreCompleto = dr["NombreCompleto"].ToString();
                    u.Correo      = dr["Correo"].ToString();
                    u.Documento   = dr["Documento"].ToString();
                    lista.Add(u);
                }
                dr.Close();
            }
            finally { DAL_Conexion.Instancia.CerrarConexion(); }
            return lista;
        }

        public string AgregarUsuario(Usuario u)
        {
            SqlConnection con = DAL_Conexion.Instancia.ObtenerConexion();
            try
            {
                SqlCommand cmd = new SqlCommand("SP_RegistrarUsuario", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@NombreCompleto", u.NombreCompleto);
                cmd.Parameters.AddWithValue("@Correo",  u.Correo);
                cmd.Parameters.AddWithValue("@Documento", u.Documento);
                cmd.Parameters.AddWithValue("@Clave",  u.GetClave());
                cmd.Parameters.AddWithValue("@Rol",    u.Rol);
                cmd.Parameters.Add("@Mensaje",  SqlDbType.VarChar, 200).Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                return cmd.Parameters["@Mensaje"].Value.ToString();
            }
            finally { DAL_Conexion.Instancia.CerrarConexion(); }
        }

        public string EditarUsuario(Usuario u)
        {
            SqlConnection con = DAL_Conexion.Instancia.ObtenerConexion();
            try
            {
                SqlCommand cmd = new SqlCommand("SP_EditarUsuario", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdUsuario", u.IdUsuario);
                cmd.Parameters.AddWithValue("@NombreCompleto", u.NombreCompleto);
                cmd.Parameters.AddWithValue("@Correo",  u.Correo);
                cmd.Parameters.AddWithValue("@Rol",    u.Rol);
                cmd.Parameters.AddWithValue("@Estado", u.Estado);
                cmd.Parameters.Add("@Mensaje",  SqlDbType.VarChar, 200).Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                return cmd.Parameters["@Mensaje"].Value.ToString();
            }
            finally { DAL_Conexion.Instancia.CerrarConexion(); }
        }

        public string EliminarUsuario(int idUsuario)
        {
            SqlConnection con = DAL_Conexion.Instancia.ObtenerConexion();
            try
            {
                SqlCommand cmd = new SqlCommand("SP_EliminarUsuario", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                cmd.Parameters.Add("@Mensaje",  SqlDbType.VarChar, 200).Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                return cmd.Parameters["@Mensaje"].Value.ToString();
            }
            finally { DAL_Conexion.Instancia.CerrarConexion(); }
        }

        public Usuario ObtenerPorDocumento(string documento)
        {
            SqlConnection con = DAL_Conexion.Instancia.ObtenerConexion();
            try
            {
                string sql = @"SELECT u.IdUsuario, u.Clave, u.Estado, u.Rol,
                                      p.IdPersona, p.NombreCompleto, p.Correo, p.Documento
                               FROM Usuario u
                               INNER JOIN Persona p ON u.IdPersona = p.IdPersona
                               WHERE p.Documento = @Documento";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@Documento", documento);
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    Usuario u = new Usuario();
                    u.IdUsuario   = (int)dr["IdUsuario"];
                    u.SetClave(dr["Clave"].ToString());
                    u.Estado      = (bool)dr["Estado"];
                    u.Rol         = dr["Rol"].ToString();
                    u.IdPersona   = (int)dr["IdPersona"];
                    u.NombreCompleto = dr["NombreCompleto"].ToString();
                    u.Correo      = dr["Correo"].ToString();
                    u.Documento   = dr["Documento"].ToString();
                    dr.Close();
                    return u;
                }
                dr.Close();
                return null;
            }
            finally { DAL_Conexion.Instancia.CerrarConexion(); }
        }
    }
}
