using System;
using System.Data;
using System.Windows.Forms;

namespace BLL
{
    /// <summary>
    /// Capa de Lógica de Negocio — Configuración del Sistema.
    /// Contiene validaciones de infraestructura que se ejecutan al arranque de la aplicación,
    /// como verificar que la base de datos esté disponible antes de mostrar el Login.
    /// </summary>
    public class Configuracion
    {
        /// <summary>
        /// Verifica que la conexión a la base de datos SQL Server esté disponible.
        /// Se invoca desde Program.Main() antes de mostrar cualquier formulario.
        /// BUG FIX: ahora termina la aplicación correctamente si la BD no responde,
        /// en lugar de continuar con la app en estado inválido.
        /// </summary>
        public static void VerificarConexionDAL()
        {
            try
            {
                // Intentar obtener la instancia Singleton del acceso a datos
                DAL.Acceso acceso = DAL.Acceso.GetInstance();

                // Ejecutar una consulta mínima para verificar conectividad real
                DataTable tabla = acceso.Leer("SELECT 1", null);

                if (tabla == null || tabla.Rows.Count == 0)
                {
                    MessageBox.Show(
                        "No se obtuvo respuesta válida de la base de datos.\nLa aplicación se cerrará.",
                        "Error de Conexión",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    // BUG FIX: antes sólo hacía 'return', lo que dejaba la app corriendo sin BD.
                    // Ahora termina el proceso correctamente.
                    Environment.Exit(1);
                }

                // Cerrar la conexión de prueba; se reabrirá cuando sea necesaria
                acceso.CerrarConexion();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"No se pudo conectar a la base de datos:\n{ex.Message}\n\nVerifique que SQL Server esté en ejecución.",
                    "Error de Conexión",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                // BUG FIX: terminar el proceso ante error de conexión irrecuperable
                Environment.Exit(1);
            }
        }
    }
}
