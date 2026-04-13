using System.Collections.Generic;
using System.Data.SqlClient;
using BE;
using DAL;

namespace Seguridad
{
    public class DAL_Permiso
    {
        // ── Listar todos los permisos (incluye TipoComponente para el arbol) ─
        public List<Permiso> ListarPermisos()
        {
            List<Permiso> lista = new List<Permiso>();
            SqlConnection con = DAL_Conexion.Instancia.ObtenerConexion();
            try
            {
                SqlCommand cmd = new SqlCommand(
                    "SELECT IdPermiso, Nombre, NombreMenu, TipoComponente, Estado FROM Permiso", con);
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    Permiso p = new Permiso();
                    p.IdPermiso       = (int)dr["IdPermiso"];
                    p.Nombre          = dr["Nombre"].ToString();
                    p.NombreMenu      = dr["NombreMenu"].ToString();
                    p.TipoComponente  = dr["TipoComponente"].ToString();
                    p.Estado          = (bool)dr["Estado"];
                    lista.Add(p);
                }
                dr.Close();
            }
            finally { DAL_Conexion.Instancia.CerrarConexion(); }
            return lista;
        }

        // ── Listar permisos asignados a un usuario especifico ─────────────
        public List<Permiso> ListarPermisosPorUsuario(int idUsuario)
        {
            List<Permiso> lista = new List<Permiso>();
            SqlConnection con = DAL_Conexion.Instancia.ObtenerConexion();
            try
            {
                string sql = @"SELECT p.IdPermiso, p.Nombre, p.NombreMenu, p.TipoComponente, p.Estado
                               FROM Permiso p
                               INNER JOIN UsuarioPermiso up ON p.IdPermiso = up.IdPermiso
                               WHERE up.IdUsuario = @IdUsuario";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    Permiso p = new Permiso();
                    p.IdPermiso      = (int)dr["IdPermiso"];
                    p.Nombre         = dr["Nombre"].ToString();
                    p.NombreMenu     = dr["NombreMenu"].ToString();
                    p.TipoComponente = dr["TipoComponente"].ToString();
                    p.Estado         = (bool)dr["Estado"];
                    lista.Add(p);
                }
                dr.Close();
            }
            finally { DAL_Conexion.Instancia.CerrarConexion(); }
            return lista;
        }

        // ── Guardar (reemplazar) permisos de un usuario ────────────────────
        // Borra los permisos actuales e inserta los seleccionados
        public string GuardarPermisosUsuario(int idUsuario, List<int> idsPermisos)
        {
            SqlConnection con = DAL_Conexion.Instancia.ObtenerConexion();
            try
            {
                // Eliminar asignaciones previas
                SqlCommand cmdDel = new SqlCommand(
                    "DELETE FROM UsuarioPermiso WHERE IdUsuario = @IdUsuario", con);
                cmdDel.Parameters.AddWithValue("@IdUsuario", idUsuario);
                cmdDel.ExecuteNonQuery();

                // Insertar nuevas asignaciones
                foreach (int idPermiso in idsPermisos)
                {
                    SqlCommand cmdIns = new SqlCommand(
                        "INSERT INTO UsuarioPermiso (IdUsuario, IdPermiso) VALUES (@IdUsuario, @IdPermiso)", con);
                    cmdIns.Parameters.AddWithValue("@IdUsuario", idUsuario);
                    cmdIns.Parameters.AddWithValue("@IdPermiso", idPermiso);
                    cmdIns.ExecuteNonQuery();
                }
                return "Permisos guardados correctamente.";
            }
            finally { DAL_Conexion.Instancia.CerrarConexion(); }
        }
    }
}
