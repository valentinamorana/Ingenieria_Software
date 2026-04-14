using System.Windows.Forms;
using BE;
using BE.Composite;
using BLL;

namespace GUI
{
    // Gestor de permisos: muestra el arbol de permisos de cada usuario.
    // Tomado directamente del proyecto de referencia, adaptado a WardrobeFlow.
    public partial class frmGestorPermisos : Form
    {
        // BLL de usuarios para llenar el combo
        private readonly UsuarioBLL _bllUsuarios;

        // Usuario actualmente seleccionado en el combo
        private Usuario _usuario;

        // Constructor: inicializa el combo con los usuarios del sistema
        public frmGestorPermisos()
        {
            _bllUsuarios = new UsuarioBLL();
            InitializeComponent();
            // Cargar todos los usuarios en el ComboBox
            this.cboUsuarios.DataSource = _bllUsuarios.GetAll();
        }

        // Cuando cambia el usuario seleccionado, actualiza el TreeView
        private void cboUsuarios_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            _usuario = (Usuario)this.cboUsuarios.SelectedItem;
            MostrarPermisos();
        }

        // Crea un nodo del TreeView a partir de un permiso compuesto
        private TreeNode CrearNodo(PermisoCompuesto item)
        {
            TreeNode tn = new TreeNode(item.Nombre);
            tn.Tag = item;
            return tn;
        }

        // Recorre recursivamente los hijos de un nodo y los agrega al TreeView
        private void MostrarPermisosRecursivo(PermisoCompuesto p, TreeNode tn)
        {
            foreach (var item in p.ObtenerHijos())
            {
                var tnn = CrearNodo(item);
                tn.Nodes.Add(tnn);
                // Si el item tambien tiene hijos, seguir recursivamente
                if (item.ObtenerHijos().Count > 0)
                    MostrarPermisosRecursivo(item, tnn);
            }
        }

        // Construye el arbol de permisos del usuario seleccionado en el TreeView
        private void MostrarPermisos()
        {
            if (_usuario == null) return;

            this.treeView1.Nodes.Clear();

            // Nodo raiz con el nombre del usuario
            TreeNode raiz = new TreeNode("Permisos de " + _usuario.NombreCompleto);
            this.treeView1.Nodes.Add(raiz);

            // Agregar cada permiso del usuario (puede ser Familia o Patente)
            foreach (var item in _usuario.Permisos)
            {
                var tn = CrearNodo(item);
                raiz.Nodes.Add(tn);
                // Si tiene hijos, expandir recursivamente
                if (item.ObtenerHijos().Count > 0)
                    MostrarPermisosRecursivo(item, tn);
            }

            // Expandir todo el arbol para mejor visualizacion
            this.treeView1.ExpandAll();
        }
    }
}
