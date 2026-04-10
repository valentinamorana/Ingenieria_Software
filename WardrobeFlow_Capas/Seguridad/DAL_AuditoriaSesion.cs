using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BE;
using DAL;

namespace Seguridad
{
    public class DAL_AuditoriaSesion
    {
        public void RegistrarAuditoriaSesion(AuditoriaSesion a)
        {
            SqlConnection con = DAL_Conexion.Instancia.ObtenerConexion();
            try
            {
                SqlCommand cmd = new SqlCommand("SP_RegistrarAuditoriaSesion", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdUsuario",           a.OUsuario.IdUsuario);
                cmd.Parameters.AddWithValue("@DescripcionAuditoria", a.DescripcionAuditoria);
                cmd.ExecuteNonQuery();
            }
            finally { DAL_Conexion.Instancia.CerrarConexion(); }
        }

        public List<AuditoriaSesion> ListarAuditorias()
        {
            List<AuditoriaSesion> lista = new List<AuditoriaSesion>();
            SqlConnection con = DAL_Conexion.Instancia.ObtenerConexion();
            try
            {
                string sql = @"SELECT a.IdAuditoria, a.DescripcionAuditoria, a.FechaHora,
                                      u.IdUsuario, p.NombreCompleto
                               FROM AuditoriaSesion a
                               INNER JOIN Usuario u ON a.IdUsuario = u.IdUsuario
                               INNER JOIN Persona p ON u.IdPersona = p.IdPersona
                               ORDER BY a.FechaHora DESC";
                SqlCommand cmd = new SqlCommand(sql, con);
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    AuditoriaSesion a = new AuditoriaSesion();
                    a.IdAuditoria          = (int)dr["IdAuditoria"];
                    a.DescripcionAuditoria = dr["DescripcionAuditoria"].ToString();
                    a.FechaHora            = (System.DateTime)dr["FechaHora"];
                    Usuario u = new Usuario();
                    u.IdUsuario      = (int)dr["IdUsuario"];
                    u.NombreCompleto = dr["NombreCompleto"].ToString();
                    a.OUsuario = u;
                    lista.Add(a);
                }
                dr.Close();
            }
            finally { DAL_Conexion.Instancia.CerrarConexion(); }
            return lista;
        }
    }
}
