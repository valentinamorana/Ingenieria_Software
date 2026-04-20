using System;
using System.Windows.Forms;

namespace GUI
{
    /// <summary>
    /// Capa de Presentación — Punto de Entrada de la Aplicación.
    ///
    /// FLUJO DE ARRANQUE:
    ///   1. Habilitar estilos visuales de Windows
    ///   2. Verificar conectividad con la base de datos (si falla → cierra la app)
    ///   3. Mostrar el formulario de Login como diálogo modal
    ///   4. Si el login es exitoso (DialogResult.OK) → abrir el Menú principal (MDI)
    ///   5. Si el usuario cancela → cerrar la aplicación
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Punto de entrada principal de la aplicación Windows Forms.
        /// [STAThread] es requerido por Windows Forms para el manejo correcto
        /// de COM y controles de interfaz gráfica.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Habilitar renderizado visual moderno de controles Windows
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Verificar conexión a BD ANTES de mostrar cualquier formulario.
            // Si la conexión falla, VerificarConexionDAL() muestra un error y llama Environment.Exit(1)
            BLL.Configuracion.VerificarConexionDAL();

            // Mostrar el Login como diálogo modal: la aplicación espera hasta que se cierre
            Login frmLogin = new Login();
            if (frmLogin.ShowDialog() == DialogResult.OK)
            {
                // Login exitoso → SessionManager tiene el usuario activo → abrir Menú MDI
                Application.Run(new Menu());
            }
            else
            {
                // El usuario canceló o cerró el Login sin autenticarse → cerrar aplicación
                Application.Exit();
            }
        }
    }
}
