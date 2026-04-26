using System;
using System.Windows.Forms;

namespace BLL
{
    /// <summary>
    /// Capa de Lógica de Negocio — Configuración del Sistema.
    /// Verifica que la base de datos esté disponible antes de mostrar el Login.
    /// </summary>
    public class Configuracion
    {
        /// <summary>
        /// Verifica la conexión a SQL Server usando DAL.Acceso.VerificarConexion().
        /// Si falla, muestra un mensaje de error y termina el proceso.
        /// Se invoca desde Program.Main() antes de mostrar cualquier formulario.
        /// </summary>
        public static void VerificarConexionDAL()
        {
            try
            {
                bool ok = DAL.Acceso.GetInstance().VerificarConexion();

                if (!ok)
                {
                    MessageBox.Show(
                        "No se pudo conectar a la base de datos.\nVerifique que SQL Server esté en ejecución.",
                        "Error de Conexión",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    Environment.Exit(1);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al inicializar la conexión:\n{ex.Message}\n\nVerifique la cadena de conexión en App.config.",
                    "Error de Conexión",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Environment.Exit(1);
            }
        }
    }
}
