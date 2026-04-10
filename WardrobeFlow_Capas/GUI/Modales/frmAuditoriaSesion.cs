using System;
using System.Windows.Forms;
using Seguridad;

namespace GUI.Modales
{
    public partial class frmAuditoriaSesion : Form
    {
        private BLL_AuditoriaSesion _bll = new BLL_AuditoriaSesion();

        public frmAuditoriaSesion() { InitializeComponent(); }

        private void frmAuditoriaSesion_Load(object sender, EventArgs e)
        {
            CargarGrilla();
        }

        private void btnActualizar_Click(object sender, EventArgs e)
        {
            CargarGrilla();
        }

        private void CargarGrilla()
        {
            dgvAuditorias.DataSource = null;
            dgvAuditorias.DataSource = _bll.ListarAuditorias();
        }
    }
}
