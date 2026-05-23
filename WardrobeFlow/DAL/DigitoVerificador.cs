using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    /// <summary>
    /// Capa de Acceso a Datos — T07 Dígitos Verificadores.
    ///
    /// Opera sobre:
    ///   [Usuario].DVH      — dígito verificador horizontal por fila
    ///   [DVVertical]       — tabla de dígitos verificadores verticales por tabla
    ///
    /// Schema esperado (aplicar WardrobeFlowDB_Alter_v3.0.sql antes de usar):
    ///   ALTER TABLE Usuario ADD DVH INT NULL
    ///   CREATE TABLE DVVertical (Id INT IDENTITY PK, NombreTabla VARCHAR(100) UNIQUE,
    ///                            DVV INT NOT NULL, FechaCalculo DATETIME NOT NULL)
    /// </summary>
    public class DigitoVerificador
    {
        private readonly Acceso acceso = Acceso.GetInstance();

        // Lee el DVV almacenado para una tabla. Retorna null si no existe registro.
        public int? ObtenerDVV(string nombreTabla)
        {
            try
            {
                DataTable tabla = acceso.Leer(
                    "SELECT DVV FROM DVVertical WHERE NombreTabla = @tabla",
                    new SqlParameter[] { new SqlParameter("@tabla", nombreTabla) });

                if (tabla == null || tabla.Rows.Count == 0) return null;
                return Convert.ToInt32(tabla.Rows[0]["DVV"]);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener DVV de la tabla '{nombreTabla}'.", ex);
            }
        }

        // Almacena (upsert) el DVV calculado para una tabla junto con la fecha.
        public void GuardarDVV(string nombreTabla, int dvv)
        {
            try
            {
                acceso.Escribir(
                    "IF EXISTS (SELECT 1 FROM DVVertical WHERE NombreTabla = @tabla) " +
                    "    UPDATE DVVertical SET DVV = @dvv, FechaCalculo = GETDATE() WHERE NombreTabla = @tabla " +
                    "ELSE " +
                    "    INSERT INTO DVVertical (NombreTabla, DVV, FechaCalculo) VALUES (@tabla, @dvv, GETDATE())",
                    new SqlParameter[]
                    {
                        new SqlParameter("@tabla", nombreTabla),
                        new SqlParameter("@dvv",   dvv)
                    });
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al guardar DVV de la tabla '{nombreTabla}'.", ex);
            }
        }

        // Lee todas las filas de Usuario con sus DVH almacenados, ordenadas por IdUsuario.
        // Retorna lista de (id, username, clave, perfil, estado, intentos, dvhAlmacenado).
        public List<FilaUsuarioDV> ObtenerFilasUsuario()
        {
            var lista = new List<FilaUsuarioDV>();
            try
            {
                DataTable tabla = acceso.Leer(
                    "SELECT IdUsuario, Username, Clave, Perfil, Estado, IntentosFallidos, DVH " +
                    "FROM Usuario ORDER BY IdUsuario",
                    null);

                if (tabla == null) return lista;

                foreach (DataRow row in tabla.Rows)
                {
                    lista.Add(new FilaUsuarioDV
                    {
                        Id               = Convert.ToInt32(row["IdUsuario"]),
                        Username         = row["Username"].ToString(),
                        Clave            = row["Clave"].ToString(),
                        Perfil           = row["Perfil"] != DBNull.Value ? row["Perfil"].ToString() : "",
                        Estado           = row["Estado"] != DBNull.Value ? Convert.ToInt32(row["Estado"]).ToString() : "0",
                        IntentosFallidos = row["IntentosFallidos"] != DBNull.Value ? Convert.ToInt32(row["IntentosFallidos"]).ToString() : "0",
                        DVHAlmacenado    = row["DVH"] != DBNull.Value ? (int?)Convert.ToInt32(row["DVH"]) : null
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener filas de Usuario para verificación DV.", ex);
            }
            return lista;
        }

        // Actualiza el DVH de un usuario específico.
        public void ActualizarDVH(int idUsuario, int dvh)
        {
            try
            {
                acceso.Escribir(
                    "UPDATE Usuario SET DVH = @dvh WHERE IdUsuario = @id",
                    new SqlParameter[]
                    {
                        new SqlParameter("@dvh", dvh),
                        new SqlParameter("@id",  idUsuario)
                    });
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar DVH del usuario ID {idUsuario}.", ex);
            }
        }
    }

    // DTO interno para el cálculo DV — no expone la entidad BE completa para mantener aislamiento.
    public class FilaUsuarioDV
    {
        public int    Id               { get; set; }
        public string Username         { get; set; }
        public string Clave            { get; set; }
        public string Perfil           { get; set; }
        public string Estado           { get; set; }
        public string IntentosFallidos { get; set; }
        public int?   DVHAlmacenado    { get; set; }
    }
}
