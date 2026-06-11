using System;
using System.Drawing;
using System.Windows.Forms;
using Servicios.Multiidioma;

namespace GUI
{
    /// <summary>
    /// Cambio de contraseña OBLIGATORIO tras el login cuando la cuenta tiene una clave
    /// temporal/generada (RequiereCambioClave = 1): alta de usuario o reset por un admin.
    /// Modal, sin Designer (estilo InputDialog). Solo cierra con OK si el cambio fue exitoso;
    /// ante un error muestra el detalle y permanece abierto. La cancelación la maneja el Login
    /// (cierra la sesión y vuelve al ingreso).
    /// </summary>
    public class CambioClaveObligatorioForm : Form
    {
        private readonly BLL.Usuario _usuarioBLL = new BLL.Usuario();
        private readonly TextBox _txtNueva;
        private readonly TextBox _txtRepetir;
        private readonly Label   _lblError;

        public CambioClaveObligatorioForm()
        {
            string T(string k, string fb)
            {
                var t = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
                return t.ContainsKey(k) ? t[k].Texto : fb;
            }

            this.Text            = T("frm.cambioclave.titulo", "Cambio de contraseña obligatorio");
            this.ClientSize      = new Size(430, 250);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.BackColor       = Color.White;
            this.Font            = new Font("Segoe UI", 9f);
            try { string ico = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico"); if (System.IO.File.Exists(ico)) this.Icon = new Icon(ico); } catch { }

            var lblInfo = new Label
            {
                Text     = T("lbl.cambioclave.info",
                             "Tu contraseña es temporal. Definí una nueva para continuar."),
                Location = new Point(16, 14), Size = new Size(398, 36), AutoSize = false
            };

            var lblNueva = new Label
            {
                Text = T("lbl.cambioclave.nueva", "Nueva contraseña:"),
                Location = new Point(16, 58), Size = new Size(398, 18), AutoSize = false
            };
            _txtNueva = new TextBox { Location = new Point(18, 78), Size = new Size(394, 24), UseSystemPasswordChar = true };

            var lblRepetir = new Label
            {
                Text = T("lbl.cambioclave.repetir", "Repetir contraseña:"),
                Location = new Point(16, 110), Size = new Size(398, 18), AutoSize = false
            };
            _txtRepetir = new TextBox { Location = new Point(18, 130), Size = new Size(394, 24), UseSystemPasswordChar = true };

            var lblReglas = new Label
            {
                Text      = T("lbl.cambioclave.reglas",
                              "Mínimo 8 caracteres, con al menos un número y un carácter especial."),
                Location  = new Point(16, 158), Size = new Size(398, 18), AutoSize = false,
                ForeColor = Color.FromArgb(150, 130, 142),
                Font      = new Font("Segoe UI", 8f)
            };

            _lblError = new Label
            {
                Text = string.Empty, Location = new Point(16, 178), Size = new Size(398, 18),
                AutoSize = false, ForeColor = Color.FromArgb(180, 50, 50)
            };

            var btnCambiar = new Button
            {
                Text = T("btn.cambioclave.cambiar", "Cambiar"),
                Location = new Point(238, 206), Size = new Size(84, 30),
                FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(176, 62, 96),
                ForeColor = Color.White, Cursor = Cursors.Hand
            };
            btnCambiar.FlatAppearance.BorderSize = 0;
            btnCambiar.Click += (s, e) => Confirmar();

            var btnCancelar = new Button
            {
                Text = T("btn.cambioclave.cancelar", "Cancelar"), DialogResult = DialogResult.Cancel,
                Location = new Point(328, 206), Size = new Size(84, 30),
                FlatStyle = FlatStyle.Flat, BackColor = Color.White,
                ForeColor = Color.FromArgb(90, 90, 100), Cursor = Cursors.Hand
            };
            btnCancelar.FlatAppearance.BorderColor = Color.FromArgb(210, 180, 195);

            this.Controls.AddRange(new Control[]
            {
                lblInfo, lblNueva, _txtNueva, lblRepetir, _txtRepetir, lblReglas, _lblError, btnCambiar, btnCancelar
            });
            this.AcceptButton = btnCambiar;
            this.CancelButton  = btnCancelar;
        }

        private void Confirmar()
        {
            string T(string k, string fb)
            {
                var t = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
                return t.ContainsKey(k) ? t[k].Texto : fb;
            }

            _lblError.Text = string.Empty;

            if (_txtNueva.Text != _txtRepetir.Text)
            {
                _lblError.Text = T("err.cambioclave.nocoincide", "Las contraseñas no coinciden.");
                _txtRepetir.Clear();
                _txtRepetir.Focus();
                return;
            }

            try
            {
                // La BLL valida requisitos, que difiera de la actual, persiste y baja el flag.
                _usuarioBLL.CambiarClavePropia(this.Text, _txtNueva.Text);
                MessageBox.Show(
                    T("msg.cambioclave.exito", "Contraseña actualizada. Ya podés usar el sistema."),
                    T("rpt.dlg.exito.titulo", "Éxito"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (BE.AppException ex)
            {
                // Error de negocio esperado (requisitos / clave igual): mostrar traducido inline.
                var t = Traductor.ObtenerTraducciones(GestorIdioma.IdiomaActual);
                _lblError.Text = t.ContainsKey(ex.Clave) ? t[ex.Clave].Texto : ex.Message;
            }
            catch (Exception ex)
            {
                _lblError.Text = ex.Message;
            }
        }
    }
}
