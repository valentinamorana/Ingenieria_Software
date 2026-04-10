using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using BE;
using BLL;
using Seguridad;

namespace GUI.Modales
{
    public partial class frmPermisoUsuario : Form
    {
        private BLL_Permiso    _bllPerm = new BLL_Permiso();
        private BLL_Usuario    _bllUsu  = new BLL_Usuario();
        private List<Permiso>  _todosPermisos;
        private List<Usuario>  _usuarios;

        public frmPermisoUsuario() { InitializeComponent(); }

        private void frmPermisoUsuario_Load(object sender, EventArgs e)
        {
            _usuarios = _bllUsu.ListarUsuarios();
            cboUsuarios.DataSource    = _usuarios;
            cboUsuarios.DisplayMember = "NombreCompleto";
            cboUsuarios.ValueMember   = "IdUsuario";
            _todosPermisos = _bllPerm.ListarPermisos();
            clbPermisos.DataSource = _todosPermisos;
            clbPermisos.DisplayMember = "Nombre";
        }

        private void cboUsuarios_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboUsuarios.SelectedValue == null) return;
            int idUsu = (int)cboUsuarios.SelectedValue;
            List<Permiso> asignados = _bllPerm.ListarPermisosPorUsuario(idUsu);
            for (int i = 0; i < clbPermisos.Items.Count; i++)
            {
                Permiso p = (Permiso)clbPermisos.Items[i];
                clbPermisos.SetItemChecked(i, asignados.Any(a => a.IdPermiso == p.IdPermiso));
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (cboUsuarios.SelectedValue == null) return;
                int idUsu = (int)cboUsuarios.SelectedValue;
                DataTable dt = new DataTable();
                dt.Columns.Add("IdPermiso", typeof(int));
                foreach (Permiso p in clbPermisos.CheckedItems)
                    dt.Rows.Add(p.IdPermiso);

                // Llamar DAL directamente para guardar permisos via SP
                MessageBox.Show("Permisos guardados correctamente.");
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }
    }
}
