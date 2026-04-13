using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BE;

namespace DAL
{
    public class DAL_Outfit
    {
        // ── Listar todos los outfits (sin detalles) ────────────────────────
        public List<Outfit> ListarOutfits()
        {
            List<Outfit> lista = new List<Outfit>();
            SqlConnection con = DAL_Conexion.Instancia.ObtenerConexion();
            try
            {
                string sql = @"SELECT o.IdOutfit, o.Nombre, o.Descripcion, o.Ocasion,
                                      o.Temporada, o.Estado, o.FechaCreacion, o.IdUsuario,
                                      p.NombreCompleto AS NombreUsuario
                               FROM Outfit o
                               INNER JOIN Usuario u ON o.IdUsuario = u.IdUsuario
                               INNER JOIN Persona p ON u.IdPersona = p.IdPersona
                               ORDER BY o.FechaCreacion DESC";
                SqlCommand cmd = new SqlCommand(sql, con);
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    Outfit o = new Outfit();
                    o.IdOutfit      = (int)dr["IdOutfit"];
                    o.Nombre        = dr["Nombre"].ToString();
                    o.Descripcion   = dr["Descripcion"].ToString();
                    o.Ocasion       = dr["Ocasion"].ToString();
                    o.Temporada     = dr["Temporada"].ToString();
                    o.Estado        = (bool)dr["Estado"];
                    o.FechaCreacion = (DateTime)dr["FechaCreacion"];
                    o.IdUsuario     = (int)dr["IdUsuario"];
                    lista.Add(o);
                }
                dr.Close();
            }
            finally { DAL_Conexion.Instancia.CerrarConexion(); }
            return lista;
        }

        // ── Listar prendas de un outfit especifico ─────────────────────────
        public List<DetalleOutfit> ListarDetallesPorOutfit(int idOutfit)
        {
            List<DetalleOutfit> lista = new List<DetalleOutfit>();
            SqlConnection con = DAL_Conexion.Instancia.ObtenerConexion();
            try
            {
                string sql = @"SELECT d.IdDetalle, d.IdOutfit, d.IdPrenda,
                                      p.Nombre AS NombrePrenda, p.Color, p.Talla, p.Temporada,
                                      c.Nombre AS NombreCategoria
                               FROM DetalleOutfit d
                               INNER JOIN Prenda   p ON d.IdPrenda   = p.IdPrenda
                               INNER JOIN Categoria c ON p.IdCategoria = c.IdCategoria
                               WHERE d.IdOutfit = @IdOutfit";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@IdOutfit", idOutfit);
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    DetalleOutfit det = new DetalleOutfit();
                    det.IdDetalle = (int)dr["IdDetalle"];
                    det.IdOutfit  = (int)dr["IdOutfit"];
                    det.IdPrenda  = (int)dr["IdPrenda"];

                    Prenda p = new Prenda();
                    p.IdPrenda  = det.IdPrenda;
                    p.Nombre    = dr["NombrePrenda"].ToString();
                    p.Color     = dr["Color"].ToString();
                    p.Talla     = dr["Talla"].ToString();
                    p.Temporada = dr["Temporada"].ToString();
                    Categoria cat = new Categoria();
                    cat.Nombre  = dr["NombreCategoria"].ToString();
                    p.OCategoria = cat;
                    det.OPrenda = p;

                    lista.Add(det);
                }
                dr.Close();
            }
            finally { DAL_Conexion.Instancia.CerrarConexion(); }
            return lista;
        }

        // ── Agregar outfit + sus prendas ───────────────────────────────────
        public string AgregarOutfit(Outfit o)
        {
            SqlConnection con = DAL_Conexion.Instancia.ObtenerConexion();
            try
            {
                // 1. Insertar cabecera via SP
                SqlCommand cmd = new SqlCommand("SP_AgregarOutfit", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Nombre",      o.Nombre);
                cmd.Parameters.AddWithValue("@Descripcion", o.Descripcion);
                cmd.Parameters.AddWithValue("@Ocasion",     o.Ocasion);
                cmd.Parameters.AddWithValue("@Temporada",   o.Temporada);
                cmd.Parameters.AddWithValue("@IdUsuario",   o.IdUsuario);
                SqlParameter pIdNuevo  = cmd.Parameters.Add("@IdOutfitGenerado", SqlDbType.Int);
                pIdNuevo.Direction     = ParameterDirection.Output;
                SqlParameter pMensaje  = cmd.Parameters.Add("@Mensaje", SqlDbType.VarChar, 200);
                pMensaje.Direction     = ParameterDirection.Output;
                cmd.ExecuteNonQuery();

                string mensaje = pMensaje.Value.ToString();
                int idNuevo   = (int)pIdNuevo.Value;

                // 2. Insertar cada detalle
                if (idNuevo > 0 && o.Detalles != null)
                {
                    foreach (DetalleOutfit det in o.Detalles)
                    {
                        SqlCommand cmdDet = new SqlCommand("SP_AgregarDetalleOutfit", con);
                        cmdDet.CommandType = CommandType.StoredProcedure;
                        cmdDet.Parameters.AddWithValue("@IdOutfit", idNuevo);
                        cmdDet.Parameters.AddWithValue("@IdPrenda", det.IdPrenda);
                        cmdDet.ExecuteNonQuery();
                    }
                }
                return mensaje;
            }
            finally { DAL_Conexion.Instancia.CerrarConexion(); }
        }

        // ── Editar outfit + reemplazar prendas ─────────────────────────────
        public string EditarOutfit(Outfit o)
        {
            SqlConnection con = DAL_Conexion.Instancia.ObtenerConexion();
            try
            {
                // 1. Actualizar cabecera
                SqlCommand cmd = new SqlCommand("SP_EditarOutfit", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdOutfit",    o.IdOutfit);
                cmd.Parameters.AddWithValue("@Nombre",      o.Nombre);
                cmd.Parameters.AddWithValue("@Descripcion", o.Descripcion);
                cmd.Parameters.AddWithValue("@Ocasion",     o.Ocasion);
                cmd.Parameters.AddWithValue("@Temporada",   o.Temporada);
                cmd.Parameters.AddWithValue("@Estado",      o.Estado);
                SqlParameter pMensaje = cmd.Parameters.Add("@Mensaje", SqlDbType.VarChar, 200);
                pMensaje.Direction    = ParameterDirection.Output;
                cmd.ExecuteNonQuery();

                string mensaje = pMensaje.Value.ToString();

                // 2. Reemplazar detalles: borrar los viejos e insertar los nuevos
                if (o.Detalles != null)
                {
                    SqlCommand cmdDel = new SqlCommand(
                        "DELETE FROM DetalleOutfit WHERE IdOutfit = @IdOutfit", con);
                    cmdDel.Parameters.AddWithValue("@IdOutfit", o.IdOutfit);
                    cmdDel.ExecuteNonQuery();

                    foreach (DetalleOutfit det in o.Detalles)
                    {
                        SqlCommand cmdIns = new SqlCommand("SP_AgregarDetalleOutfit", con);
                        cmdIns.CommandType = CommandType.StoredProcedure;
                        cmdIns.Parameters.AddWithValue("@IdOutfit", o.IdOutfit);
                        cmdIns.Parameters.AddWithValue("@IdPrenda", det.IdPrenda);
                        cmdIns.ExecuteNonQuery();
                    }
                }
                return mensaje;
            }
            finally { DAL_Conexion.Instancia.CerrarConexion(); }
        }

        // ── Eliminar outfit (y sus detalles en cascada via SP) ─────────────
        public string EliminarOutfit(int idOutfit)
        {
            SqlConnection con = DAL_Conexion.Instancia.ObtenerConexion();
            try
            {
                SqlCommand cmd = new SqlCommand("SP_EliminarOutfit", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdOutfit", idOutfit);
                SqlParameter pMensaje = cmd.Parameters.Add("@Mensaje", SqlDbType.VarChar, 200);
                pMensaje.Direction    = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                return pMensaje.Value.ToString();
            }
            finally { DAL_Conexion.Instancia.CerrarConexion(); }
        }
    }
}
