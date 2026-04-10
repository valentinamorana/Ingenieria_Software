using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using BE;
using Seguridad;

namespace GUI
{
    public partial class Inicio : Form
    {
        private Usuario _usuarioActual;
        private Form    _formActivo;

        public Inicio(Usuario usuario)
        {
            InitializeComponent();
            _usuarioActual = usuario;
        }

        private void Inicio_Load(object sender, EventArgs e)
        {
            lblUsuario.Text = _usuarioActual.NombreCompleto + " [" + _usuarioActual.Rol + "]";
            AplicarPermisos();
        }

        // Oculta / muestra items del menú según permisos — T02
        private void AplicarPermisos()
        {
            List<Permiso> permisos = _usuarioActual.GetPermisos();
            foreach (ToolStripMenuItem item in menuPrincipal.Items)
            {
                AplicarPermisosRecursivo(item, permisos);
            }
        }

        private void AplicarPermisosRecursivo(ToolStripMenuItem item, List<Permiso> permisos)
        {
            if (item.DropDownItems.Count == 0)
            {
                item.Visible = permisos.Any(p => p.NombreMenu == item.Name);
            }
            else
            {
                foreach (ToolStripItem sub in item.DropDownItems)
                {
                    if (sub is ToolStripMenuItem subItem)
                        AplicarPermisosRecursivo(subItem, permisos);
                }
                item.Visible = item.DropDownItems
                    .OfType<ToolStripMenuItem>().Any(s => s.Visible);
            }
        }

        private void AbrirFormulario(Form form)
        {
            _formActivo?.Close();
            _formActivo = form;
            form.TopLevel   = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock       = DockStyle.Fill;
            panelContenedor.Controls.Clear();
            panelContenedor.Controls.Add(form);
            form.Show();
        }

        // ── Menú: Prendas ──────────────────────────────────────────────────
        private void mnuPrendas_Click(object sender, EventArgs e)
        {
            AbrirFormulario(new Modales.frmPrenda(_usuarioActual));
        }

        // ── Menú: Categorías ───────────────────────────────────────────────
        private void mnuCategorias_Click(object sender, EventArgs e)
        {
            AbrirFormulario(new Modales.frmCategoria());
        }

        // ── Menú: Usuarios ─────────────────────────────────────────────────
        private void mnuUsuarios_Click(object sender, EventArgs e)
        {
            AbrirFormulario(new Modales.frmUsuario());
        }

        // ── Menú: Permisos ─────────────────────────────────────────────────
        private void mnuPermisos_Click(object sender, EventArgs e)
        {
            AbrirFormulario(new Modales.frmPermisoUsuario());
        }

        // ── Menú: Auditoría ────────────────────────────────────────────────
        private void mnuAuditoria_Click(object sender, EventArgs e)
        {
            AbrirFormulario(new Modales.frmAuditoriaSesion());
        }

        // ── Menú: Cerrar sesión ────────────────────────────────────────────
        private void mnuCerrarSesion_Click(object sender, EventArgs e)
        {
            BLL_AuditoriaSesion bllAud = new BLL_AuditoriaSesion();
            AuditoriaSesion aud = new AuditoriaSesion();
            aud.OUsuario             = _usuarioActual;
            aud.DescripcionAuditoria = "Cierre de sesion";
            bllAud.RegistrarAuditoriaSesion(aud);

            new InicioDeSesion().Show();
            this.Close();
        }
    }
}
