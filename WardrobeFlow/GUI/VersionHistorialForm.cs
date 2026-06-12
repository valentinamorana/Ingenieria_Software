using Servicios.Multiidioma;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace GUI
{
    public partial class VersionHistorialForm : Form, IIdiomaObserver
    {
        private readonly BLL.VersionUsuario _bll         = new BLL.VersionUsuario();
        private readonly BLL.Usuario        _bllUsuario  = new BLL.Usuario();

        private List<BE.VersionUsuario> _versiones = new List<BE.VersionUsuario>();
        private int _preseleccionarId = 0;   // si >0, se preselecciona ese usuario al abrir

        public VersionHistorialForm()
        {
            InitializeComponent();
        }

        // Abre el historial ya filtrado a un usuario concreto (desde Administración de Usuarios).
        public VersionHistorialForm(int idUsuario) : this()
        {
            _preseleccionarId = idUsuario;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try { string ico = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico"); if (System.IO.File.Exists(ico)) this.Icon = new System.Drawing.Icon(ico); } catch { }
            GestorIdioma.SuscribirObservador(this);
            Traducir(GestorIdioma.IdiomaActual);
            CargarUsuarios();

            // Si se abrió para un usuario concreto, preseleccionarlo y cargar su historial.
            if (_preseleccionarId > 0)
            {
                cboUsuario.SelectedValue = _preseleccionarId;
                if (cboUsuario.SelectedValue != null) btnCargar_Click(null, EventArgs.Empty);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            GestorIdioma.DesuscribirObservador(this);
            base.OnFormClosing(e);
        }

        public void UpdateLanguage(Idioma idioma) => Traducir(idioma);

        private void Traducir(Idioma idioma)
        {
            var t = Traductor.ObtenerTraducciones(idioma);
            string T(string key, string fallback) => t.ContainsKey(key) ? t[key].Texto : fallback;

            this.Text           = T("frm.historialusr",    "Historial de Cambios de Usuarios");
            lblTitulo.Text      = T("frm.historialusr",    "Historial de Cambios de Usuarios");
            lblUsuario.Text     = T("lbl.ver.usuario",     "Usuario:");
            btnCargar.Text      = T("btn.ver.cargar",      "Cargar");
            btnRestaurar.Text   = T("btn.ver.restaurar",   "Restaurar Versión Seleccionada");

            if (dgv.Columns.Count > 0)
            {
                dgv.Columns["colRegistro"].HeaderText = T("col.ver.registro", "Usuario (ID)");
                dgv.Columns["colFecha"].HeaderText    = T("col.ver.fecha",    "Fecha");
                dgv.Columns["colActor"].HeaderText    = T("col.ver.actor",    "Modificado por");
                dgv.Columns["colCampo"].HeaderText    = T("col.ver.campo",    "Campo");
                dgv.Columns["colAnterior"].HeaderText = T("col.ver.anterior", "Valor anterior");
                dgv.Columns["colNuevo"].HeaderText    = T("col.ver.nuevo",    "Valor nuevo");
            }
        }

        private void CargarUsuarios()
        {
            try
            {
                var usuarios = _bllUsuario.ObtenerTodos();
                cboUsuario.DisplayMember = "Username";
                cboUsuario.ValueMember   = "Id";
                cboUsuario.DataSource    = usuarios;
                cboUsuario.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"[VersionHistorialForm.CargarUsuarios] {ex.Message}");
            }
        }

        private void btnCargar_Click(object sender, EventArgs e)
        {
            if (cboUsuario.SelectedValue == null) return;

            try
            {
                int idUsuario = (int)cboUsuario.SelectedValue;
                _versiones = _bll.ObtenerPorUsuario(idUsuario);
                CargarGrilla();
            }
            catch (Exception ex)
            {
                var tErr = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
                string titErr = tErr.ContainsKey("msg.error.titulo") ? tErr["msg.error.titulo"].Texto : "Error";
                string fmtErr = tErr.ContainsKey("msg.historial.errorcargar") ? tErr["msg.historial.errorcargar"].Texto : "Error al cargar historial:\n{0}";
                MessageBox.Show(string.Format(fmtErr, ex.Message), titErr, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Muestra el historial como CAMBIOS a nivel de campo (Identificador del registro = IdUsuario,
        // Campo, Valor anterior, Valor nuevo, Usuario que modificó y Fecha/hora). Se computa el diff
        // entre cada versión y la anterior; solo se listan datos administrativos NO sensibles (la
        // contraseña nunca se versiona ni se muestra).
        private void CargarGrilla()
        {
            dgv.Rows.Clear();

            // Ordenar ascendente por fecha (luego por Id) para comparar transiciones consecutivas.
            var asc = new List<BE.VersionUsuario>(_versiones);
            asc.Sort((a, b) =>
            {
                int c = a.Fecha.CompareTo(b.Fecha);
                return c != 0 ? c : a.Id.CompareTo(b.Id);
            });

            var t = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
            string Campo(string k, string fb) => t.ContainsKey(k) ? t[k].Texto : fb;

            string Fnac(DateTime? f) => f?.ToString("dd/MM/yyyy") ?? "";

            var filas = new List<object[]>();
            for (int i = 1; i < asc.Count; i++)
            {
                var prev = asc[i - 1];
                var cur  = asc[i];
                string fechaTxt = cur.Fecha.ToString("dd/MM/yyyy HH:mm:ss");
                // Identificador del registro afectado: usuario + ID.
                string registroTxt = $"{cur.UsernameSnapshot} (ID {cur.IdUsuario})";

                void Cmp(string campo, string viejo, string nuevo)
                {
                    if (!string.Equals(viejo ?? "", nuevo ?? "", StringComparison.Ordinal))
                        filas.Add(new object[] { cur.Id, registroTxt, fechaTxt, cur.Actor, campo, viejo ?? "", nuevo ?? "" });
                }

                Cmp(Campo("col.ver.f.usuario",  "Usuario"),    prev.UsernameSnapshot, cur.UsernameSnapshot);
                Cmp(Campo("col.ver.f.nombre",   "Nombre"),     prev.NombreSnapshot,   cur.NombreSnapshot);
                Cmp(Campo("col.ver.f.apellido", "Apellido"),   prev.ApellidoSnapshot, cur.ApellidoSnapshot);
                Cmp(Campo("col.ver.f.email",    "Email"),      prev.EmailSnapshot,    cur.EmailSnapshot);
                Cmp(Campo("col.ver.f.fechanac", "Fecha nac."), Fnac(prev.FechaNacSnapshot), Fnac(cur.FechaNacSnapshot));
            }

            // Mostrar lo más reciente primero.
            filas.Reverse();
            foreach (var f in filas) dgv.Rows.Add(f);
        }

        private void btnRestaurar_Click(object sender, EventArgs e)
        {
            var t = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
            string T(string k, string fb) => t.ContainsKey(k) ? t[k].Texto : fb;

            if (dgv.CurrentRow == null)
            {
                MessageBox.Show(
                    T("msg.historial.sinseleccion", "Seleccioná una versión de la grilla."),
                    T("msg.historial.atencion",     "Atención"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int idVersion = Convert.ToInt32(dgv.CurrentRow.Cells["colId"].Value);
            var ver = _versiones.Find(v => v.Id == idVersion);
            if (ver == null) return;

            string tpl = T("msg.historial.confirmar",
                "¿Restaurar al usuario '{0}' al estado del {1}?\n\nDetalle del snapshot: {2}\n\nEsta acción es reversible (se graba un nuevo snapshot antes de restaurar).");
            string msg = string.Format(tpl, ver.UsernameSnapshot, ver.Fecha.ToString("dd/MM/yyyy HH:mm"), ver.Detalle);

            if (MessageBox.Show(msg, T("msg.backup.titulorestaura", "Confirmar Restauración"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                _bll.RestaurarVersion(this.Text, idVersion);
                MessageBox.Show(
                    T("msg.historial.restaurado",  "Versión restaurada correctamente."),
                    T("rpt.dlg.exito.titulo",       "Éxito"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                btnCargar_Click(null, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                var tErr = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
                string titErr = tErr.ContainsKey("msg.error.titulo") ? tErr["msg.error.titulo"].Texto : "Error";
                string fmtErr = tErr.ContainsKey("msg.historial.errorrestaur") ? tErr["msg.historial.errorrestaur"].Texto : "Error al restaurar versión:\n{0}";
                MessageBox.Show(string.Format(fmtErr, ex.Message), titErr, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
