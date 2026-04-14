using System;
using System.Windows.Forms;

namespace GUI
{
    // Punto de entrada de la aplicacion WardrobeFlow.
    // Igual al del proyecto de referencia: inicia el formulario MDI principal.
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // Arranca el formulario MDI principal (igual que en el proyecto referencia)
            Application.Run(new frmMdiPrincipal());
        }
    }
}
