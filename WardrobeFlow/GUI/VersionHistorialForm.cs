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
                dgv.Columns["colDetalle"].HeaderText  = T("col.ver.detalle",  "Cambios realizados");
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

        // Muestra el historial como VERSIONES (snapshots) del usuario: una fila por cada GUARDADO.
        // Cada fila es un estado completo en un instante dado (modelo snapshot/Memento), con su Id de
        // versión, fecha, autor y el detalle de qué cambió en ese guardado. "Restaurar" deja al usuario
        // EXACTAMENTE en el estado de la versión elegida (punto en el tiempo), sin arrastrar ni perder
        // los demás guardados. Solo datos administrativos NO sensibles (la contraseña nunca se versiona).
        private void CargarGrilla()
        {
            dgv.Rows.Clear();

            var t = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
            string Campo(string k, string fb) => t.ContainsKey(k) ? t[k].Texto : fb;

            // Más reciente primero (el DAL ya devuelve ORDER BY Fecha DESC; se reordena por las dudas).
            var orden = new List<BE.VersionUsuario>(_versiones);
            orden.Sort((a, b) =>
            {
                int c = b.Fecha.CompareTo(a.Fecha);
                return c != 0 ? c : b.Id.CompareTo(a.Id);
            });

            foreach (var v in orden)
            {
                string fechaTxt    = v.Fecha.ToString("dd/MM/yyyy HH:mm:ss");
                string registroTxt = $"{v.UsernameSnapshot} (ID {v.IdUsuario})";
                // Si esta versión proviene de una restauración, se marca en "Modificado por".
                string actorTxt = EsRestauracion(v)
                    ? $"{v.Actor} · {Campo("hist.rollback", "rollback")}"
                    : v.Actor;

                // colId = Id de ESTA versión: "Restaurar" deja al usuario en el estado de este snapshot.
                dgv.Rows.Add(new object[] { v.Id, registroTxt, fechaTxt, actorTxt, v.Detalle ?? "" });
            }
        }

        // Una versión proviene de un rollback si su detalle es una restauración (formato nuevo
        // "Restauración a versión..." o el legacy "...antes de restaurar..."). Marcador interno
        // en español: no es texto de UI, solo se usa para detectar el origen.
        private static bool EsRestauracion(BE.VersionUsuario v)
        {
            if (string.IsNullOrEmpty(v.Detalle)) return false;
            return v.Detalle.IndexOf("Restauración a versión", StringComparison.OrdinalIgnoreCase) >= 0
                || v.Detalle.IndexOf("antes de restaurar",     StringComparison.OrdinalIgnoreCase) >= 0;
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

            // colId = Id de la versión seleccionada: restaurarla deja al usuario EXACTAMENTE en el
            // estado de ese snapshot (punto en el tiempo), sin tocar los demás guardados del historial.
            int idVersion = Convert.ToInt32(dgv.CurrentRow.Cells["colId"].Value);
            var version = _versiones.Find(v => v.Id == idVersion);
            if (version == null) return;

            string fechaTxt = version.Fecha.ToString("dd/MM/yyyy HH:mm:ss");
            string fnac     = version.FechaNacSnapshot?.ToString("dd/MM/yyyy") ?? "—";
            // Estado completo al que volverá el usuario (datos administrativos no sensibles).
            string estado =
                $"Usuario: {version.UsernameSnapshot}\n" +
                $"Nombre: {version.NombreSnapshot}\n" +
                $"Apellido: {version.ApellidoSnapshot}\n" +
                $"Email: {version.EmailSnapshot}\n" +
                $"Fecha nac.: {fnac}";

            string tpl = T("msg.historial.confirmar.restaurar",
                "¿Restaurar al usuario al estado del {0}?\n\n{1}\n\nEl usuario quedará exactamente en este estado. Es reversible (queda registrado como un nuevo cambio en el historial).");
            string msg = string.Format(tpl, fechaTxt, estado);

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
