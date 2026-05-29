using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GUI
{
    /// <summary>
    /// Form B — Reparación manual de dígitos verificadores fila a fila.
    /// El admin ve la lista de filas con DVH inválido, elige cuáles reparar y confirma.
    /// </summary>
    public class ReparacionAsistidaForm : Form
    {
        private readonly List<DAL.FilaUsuarioDV> _filasRotas;

        private DataGridView _grid;
        private Button       _btnSelAll;
        private Button       _btnReparar;
        private Button       _btnCancelar;
        private Label        _lblResumen;

        public ReparacionAsistidaForm(List<DAL.FilaUsuarioDV> filasRotas)
        {
            _filasRotas = filasRotas;
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text            = "Reparación Manual de Dígitos Verificadores";
            this.Size            = new Size(680, 460);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.Font            = new Font("Segoe UI", 9f);

            // ── Header ────────────────────────────────────────────────────────
            var panelHeader = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 54,
                BackColor = Color.FromArgb(40, 40, 55),
                Padding   = new Padding(16, 0, 16, 0)
            };

            var lblTitulo = new Label
            {
                Text      = "Seleccione las filas a reparar",
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.White,
                Dock      = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            panelHeader.Controls.Add(lblTitulo);

            // ── Grilla ────────────────────────────────────────────────────────
            _grid = new DataGridView
            {
                Dock                  = DockStyle.Fill,
                AllowUserToAddRows    = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible     = false,
                AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor       = Color.White,
                BorderStyle           = BorderStyle.None,
                Font                  = new Font("Segoe UI", 9f)
            };

            var colCheck = new DataGridViewCheckBoxColumn
            {
                Name       = "colCheck",
                HeaderText = "",
                FillWeight = 6,
                Width      = 36,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            };
            _grid.Columns.Add(colCheck);
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colId",      HeaderText = "ID",             FillWeight = 8,  ReadOnly = true });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colUsuario",  HeaderText = "Usuario",        FillWeight = 28, ReadOnly = true });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDVHAlm",   HeaderText = "DVH Almacenado", FillWeight = 19, ReadOnly = true });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colEstado",   HeaderText = "Estado",         FillWeight = 30, ReadOnly = true });

            _grid.CellFormatting += Grid_CellFormatting;

            // ── Resumen ───────────────────────────────────────────────────────
            _lblResumen = new Label
            {
                Text      = $"{_filasRotas.Count} fila(s) con DVH inválido detectada(s).",
                Dock      = DockStyle.Top,
                Height    = 28,
                Padding   = new Padding(8, 4, 8, 0),
                ForeColor = Color.FromArgb(160, 60, 60),
                Font      = new Font("Segoe UI", 9f, FontStyle.Italic)
            };

            // ── Botones ───────────────────────────────────────────────────────
            var panelBotones = new FlowLayoutPanel
            {
                Dock          = DockStyle.Bottom,
                Height        = 50,
                FlowDirection = FlowDirection.RightToLeft,
                Padding       = new Padding(8, 6, 8, 6),
                BackColor     = Color.FromArgb(245, 245, 250)
            };

            _btnCancelar = new Button
            {
                Text      = "Cancelar",
                Width     = 90,
                Height    = 34,
                FlatStyle = FlatStyle.Flat
            };
            _btnCancelar.Click += (s, e) => this.Close();

            _btnReparar = new Button
            {
                Text      = "Reparar Seleccionadas",
                Width     = 170,
                Height    = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(60, 120, 180),
                ForeColor = Color.White
            };
            _btnReparar.FlatAppearance.BorderSize = 0;
            _btnReparar.Click += BtnReparar_Click;

            _btnSelAll = new Button
            {
                Text      = "Seleccionar Todas",
                Width     = 140,
                Height    = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(90, 90, 110),
                ForeColor = Color.White
            };
            _btnSelAll.FlatAppearance.BorderSize = 0;
            _btnSelAll.Click += BtnSelAll_Click;

            panelBotones.Controls.AddRange(new Control[] { _btnCancelar, _btnReparar, _btnSelAll });

            this.Controls.Add(_grid);
            this.Controls.Add(_lblResumen);
            this.Controls.Add(panelBotones);
            this.Controls.Add(panelHeader);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try { string ico = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico"); if (System.IO.File.Exists(ico)) this.Icon = new System.Drawing.Icon(ico); } catch { }
            CargarFilas();
        }

        private void CargarFilas()
        {
            _grid.Rows.Clear();
            foreach (var fila in _filasRotas)
            {
                string estado = fila.DVHAlmacenado == null ? "Sin DVH almacenado" : "DVH no coincide con datos actuales";
                int idx = _grid.Rows.Add(true, fila.Id, fila.Username, fila.DVHAlmacenado?.ToString() ?? "—", estado);
                _grid.Rows[idx].Tag = fila.Id;
            }
        }

        private void Grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var col = _grid.Columns[e.ColumnIndex];
            if (col.Name != "colEstado") return;

            string val = e.Value?.ToString() ?? "";
            bool sinDVH = val.Contains("Sin DVH");
            e.CellStyle.ForeColor = sinDVH ? Color.FromArgb(150, 100, 20) : Color.FromArgb(180, 50, 50);
            e.CellStyle.Font      = new Font("Segoe UI", 9f, FontStyle.Italic);
        }

        private void BtnSelAll_Click(object sender, EventArgs e)
        {
            bool marcarTodas = _btnSelAll.Text == "Seleccionar Todas";
            foreach (DataGridViewRow row in _grid.Rows)
                row.Cells["colCheck"].Value = marcarTodas;
            _btnSelAll.Text = marcarTodas ? "Deseleccionar Todas" : "Seleccionar Todas";
        }

        private void BtnReparar_Click(object sender, EventArgs e)
        {
            var idsSeleccionados = new List<int>();
            foreach (DataGridViewRow row in _grid.Rows)
            {
                if (row.Cells["colCheck"].Value is true)
                    idsSeleccionados.Add((int)row.Tag);
            }

            if (idsSeleccionados.Count == 0)
            {
                MessageBox.Show("Seleccione al menos una fila para reparar.", "Sin selección", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string msg = $"¿Reparar los dígitos verificadores de {idsSeleccionados.Count} fila(s) seleccionada(s)?\n\n" +
                          "Se recalcularán los DVH individuales y el DVV de la tabla.";

            if (MessageBox.Show(msg, "Confirmar Reparación", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                BLL.Configuracion.RepararFilas(idsSeleccionados);
                MessageBox.Show(
                    $"Se repararon {idsSeleccionados.Count} fila(s) con éxito.\nEl DVV de la tabla fue recalculado.",
                    "Reparación Exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al reparar:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
