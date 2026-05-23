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
            // Si se detecta manipulación externa, bloquea el acceso al sistema.
            if (!BLL.Configuracion.VerificarIntegridadDV(out string errIntegridad))
            {
                MessageBox.Show(errIntegridad, "Integridad comprometida — Acceso bloqueado", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

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
