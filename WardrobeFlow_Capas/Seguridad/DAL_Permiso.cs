using System.Collections.Generic;
using System.Data.SqlClient;
using BE;
using DAL;

namespace Seguridad
{
    public class DAL_Permiso
    {
        public List<Permiso> ListarPermisos()
        {
            List<Permiso> lista = new List<Permiso>();
            SqlConnection con = DAL_Conexion.Instancia.ObtenerConexion();
            try
            {
                SqlCommand cmd = new SqlCommand(
                    "SELECT IdPermiso, Nombre, NombreMenu, Estado FROM Permiso", con);
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    Permiso p = new Permiso();
                    p.IdPermiso   = (int)dr["IdPermiso"];
                    p.Nombre      = dr["Nombre"].ToString();
                    p.NombreMenu  = dr["NombreMenu"].ToString();
                    p.Estado      = (bool)dr["Estado"];
                    lista.Add(p);
                }
                dr.Close();
            }
            finally { DAL_Conexion.Instancia.CerrarConexion(); }
            return lista;
        }

        public List<Permiso> ListarPermisosPorUsuario(int idUsuario)
        {
            List<Permiso> lista = new List<Permiso>();
            SqlConnection con = DAL_Conexion.Instancia.ObtenerConexion();
            try
            {
                string sql = @"SELECT p.IdPermiso, p.Nombre, p.NombreMenu, p.Estado
                               FROM Permiso p
                               INNER JOIN UsuarioPermiso up ON p.IdPermiso = up.IdPermiso
                               WHERE up.IdUsuario = @IdUsuario";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    Permiso p = new Permiso();
                    p.IdPermiso  = (int)dr["IdPermiso"];
                    p.Nombre     = dr["Nombre"].ToString();
                    p.NombreMenu = dr["NombreMenu"].ToString();
                    p.Estado     = (bool)dr["Estado"];
                    lista.Add(p);
                }
                dr.Close();
            }
            finally { DAL_Conexion.Instancia.CerrarConexion(); }
            return lista;
        }
    }
}
