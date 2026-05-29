using System;
using System.Windows.Forms;

namespace GUI
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (!BLL.Configuracion.VerificarConexionDAL(out string errConexion))
            {
                MessageBox.Show(errConexion, "Error de Conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // T07 — Verificar integridad DVH/DVV antes de mostrar el Login.
            // Si se detecta manipulación, abre RestauracionForm para que el admin pueda reparar.
            if (!BLL.Configuracion.VerificarIntegridadDV(out BLL.ResultadoIntegridad resultadoDV))
            {
                using (var restForm = new RestauracionForm(resultadoDV))
                {
                    Application.Run(restForm);
                    if (!restForm.RestauradoExitosamente)
                        return;
                }
            }

            // Garantiza que exista admin2 (admin de respaldo para desbloquear al admin1 si se bloquea).
            string rutaAdmin2 = BLL.Configuracion.SeedAdminSecundario();
            if (rutaAdmin2 != null)
                MessageBox.Show(
                    "Se creó el usuario administrador de respaldo 'admin2'.\n" +
                    "Sus credenciales fueron guardadas en:\n\n" + rutaAdmin2 +
                    "\n\nGuardá ese archivo en un lugar seguro.",
                    "Administrador de respaldo creado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

            using (var frmLogin = new Login())
            {
                if (frmLogin.ShowDialog() == DialogResult.OK)
                    Application.Run(new Menu());
                else
                    Application.Exit();
            }
        }
    }
}
