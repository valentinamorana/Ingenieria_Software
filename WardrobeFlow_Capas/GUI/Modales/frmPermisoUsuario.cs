using System;
using System.Collections.Generic;
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
                if (cboUsuarios.SelectedValue == null)
                {
                    MessageBox.Show("Seleccione un usuario.", "Aviso",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int idUsu = (int)cboUsuarios.SelectedValue;

                // Recolectar IDs de los permisos marcados
                List<int> idsSeleccionados = new List<int>();
                foreach (Permiso p in clbPermisos.CheckedItems)
                    idsSeleccionados.Add(p.IdPermiso);

                // Guardar via BLL (reemplaza permisos actuales del usuario)
                string msg = _bllPerm.GuardarPermisosUsuario(idUsu, idsSeleccionados);
                MessageBox.Show(msg, "Exito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
