using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace GUI
{
    /// <summary>
    /// Capa de Presentación — Módulo de Gestión de Prendas.
    ///
    /// Permite al ControladorDeStock administrar el catálogo de prendas:
    ///   ✓ Ver todas las prendas con estado, cliente actual y datos descriptivos
    ///   ✓ Filtrar por estado (Todos / Disponible / EnUso / EnLimpieza / Baja)
    ///   ✓ Filtrar por texto libre (nombre, categoría, color)
    ///   ✓ Agregar nueva prenda al catálogo
    ///   ✓ Editar datos descriptivos de una prenda
    ///   ✓ Cambiar estado (Disponible ↔ EnLimpieza, → Baja)
    ///   ✓ Ver detalle del cliente que tiene la prenda en uso
    ///
    /// El OperadorLogístico también accede (mnuPrendas) pero sin panel de acciones
    /// de stock (mnuStock). Los botones de cambio de estado están disponibles solo
    /// si el usuario tiene el permiso mnuStock.
    ///
    /// Accesible desde Menú → Inventario → Prendas.
    /// </summary>
    public partial class Prendas : Form
    {
        private readonly BLL.Prenda prendaBLL = new BLL.Prenda();

        // Determina si el usuario puede cambiar estados (ControladorDeStock)
        private readonly bool _tieneStock;

        // ── Controles ─────────────────────────────────────────────────────────
        private DataGridView dgvPrendas;
        private TextBox      txtFiltro;
        private ComboBox     cmbEstadoFiltro;
        private Button       btnNueva;
        private Button       btnEditar;
        private Button       btnCambiarEstado;
        private Button       btnRefrescar;
        private Label        lblMensaje;
        private Label        lblConteo;
        private Panel        panelDetalle;
        private Label        lblDetalleContenido;

        private List<BE.Prenda> _prendas = new List<BE.Prenda>();

        public Prendas()
        {
            InitializeComponent();
            this.Text        = "Catálogo de Prendas";
            this.ClientSize  = new Size(1000, 580);
            this.MinimumSize = new Size(820, 460);

            // Verificar si el usuario activo tiene permiso de stock
            var bllUsuario = new BLL.Usuario();
            var usuario    = bllUsuario.ObtenerUsuarioActivo();
            _tieneStock = usuario?.Permisos?.Exists(p => p.NombreMenu == "mnuStock") == true;

            ConstruirInterfaz();
            this.Load += (s, e) => CargarPrendas();
        }

        private void ConstruirInterfaz()
        {
            // ── Panel superior: filtros y acciones ────────────────────────────
            Panel panelTop = new Panel
            {
                Dock = DockStyle.Top, Height = 56,
                Padding = new Padding(8, 8, 8, 4),
                BackColor = Color.FromArgb(230, 230, 240)
            };

            panelTop.Controls.Add(new Label
            {
                Text = "Estado:", Left = 8, Top = 18, Width = 48,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            });

            cmbEstadoFiltro = new ComboBox
            {
                Left = 58, Top = 15, Width = 130,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbEstadoFiltro.Items.AddRange(new object[]
                { "Todos", "Disponible", "En Uso", "En Limpieza", "Baja" });
            cmbEstadoFiltro.SelectedIndex = 0;
            cmbEstadoFiltro.SelectedIndexChanged += (s, e) => AplicarFiltro();
            panelTop.Controls.Add(cmbEstadoFiltro);

            panelTop.Controls.Add(new Label
            {
                Text = "Buscar:", Left = 200, Top = 18, Width = 50,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            });
            txtFiltro = new TextBox { Left = 252, Top = 15, Width = 200 };
            txtFiltro.TextChanged += (s, e) => AplicarFiltro();
            panelTop.Controls.Add(txtFiltro);

            btnNueva = new Button
            {
                Text = "+ Nueva Prenda", Left = 470, Top = 13,
                Width = 130, Height = 28,
                BackColor = Color.SteelBlue, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Visible = _tieneStock
            };
            btnNueva.FlatAppearance.BorderSize = 0;
            btnNueva.Click += BtnNueva_Click;
            panelTop.Controls.Add(btnNueva);

            btnEditar = new Button
            {
                Text = "✎ Editar", Left = 610, Top = 13,
                Width = 80, Height = 28,
                FlatStyle = FlatStyle.Flat, Enabled = false,
                Visible = _tieneStock
            };
            btnEditar.Click += BtnEditar_Click;
            panelTop.Controls.Add(btnEditar);

            btnCambiarEstado = new Button
            {
                Text = "⇄ Estado", Left = 698, Top = 13,
                Width = 90, Height = 28,
                BackColor = Color.FromArgb(100, 140, 80), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Enabled = false,
                Visible = _tieneStock
            };
            btnCambiarEstado.FlatAppearance.BorderSize = 0;
            btnCambiarEstado.Click += BtnCambiarEstado_Click;
            panelTop.Controls.Add(btnCambiarEstado);

            btnRefrescar = new Button
            {
                Text = "↻", Left = 796, Top = 13,
                Width = 32, Height = 28, FlatStyle = FlatStyle.Flat
            };
            btnRefrescar.Click += (s, e) => CargarPrendas();
            panelTop.Controls.Add(btnRefrescar);

            lblConteo = new Label
            {
                Left = 836, Top = 18, Width = 200,
                ForeColor = Color.DimGray, Font = new Font("Segoe UI", 8.5f)
            };
            panelTop.Controls.Add(lblConteo);

            // ── Panel inferior: detalle del cliente en uso ────────────────────
            panelDetalle = new Panel
            {
                Dock = DockStyle.Bottom, Height = 70,
                BackColor = Color.FromArgb(255, 252, 235),
                Padding = new Padding(10, 6, 10, 6),
                Visible = false
            };
            var lblDetalleTitulo = new Label
            {
                Text = "Cliente en uso:",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Left = 10, Top = 8, Width = 120
            };
            lblDetalleContenido = new Label
            {
                Left = 130, Top = 8, Width = 800, Height = 50,
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(100, 60, 0)
            };
            panelDetalle.Controls.Add(lblDetalleTitulo);
            panelDetalle.Controls.Add(lblDetalleContenido);

            // ── Status bar ────────────────────────────────────────────────────
            Panel panelStatus = new Panel
            {
                Dock = DockStyle.Bottom, Height = 26,
                BackColor = Color.FromArgb(230, 230, 240),
                Padding = new Padding(8, 4, 8, 4)
            };
            lblMensaje = new Label { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 8.5f) };
            panelStatus.Controls.Add(lblMensaje);

            // ── DataGridView ──────────────────────────────────────────────────
            dgvPrendas = new DataGridView
            {
                Dock                            = DockStyle.Fill,
                ReadOnly                        = true,
                AllowUserToAddRows              = false,
                AllowUserToDeleteRows           = false,
                SelectionMode                   = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode             = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor                 = Color.White,
                RowHeadersVisible               = false,
                BorderStyle                     = BorderStyle.None,
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(248, 248, 255)
                }
            };
            dgvPrendas.SelectionChanged += DgvPrendas_SelectionChanged;
            dgvPrendas.CellDoubleClick  += (s, e) =>
            {
                if (_tieneStock) BtnEditar_Click(s, e);
            };

            this.Controls.Add(dgvPrendas);
            this.Controls.Add(panelDetalle);
            this.Controls.Add(panelStatus);
            this.Controls.Add(panelTop);
        }

        // ── Carga y filtrado ──────────────────────────────────────────────────

        private void CargarPrendas()
        {
            try
            {
                _prendas = prendaBLL.ObtenerTodas();
                AplicarFiltro();
                MostrarOk($"{_prendas.Count} prenda(s) en el catálogo.");
            }
            catch (Exception ex)
            {
                MostrarError($"Error al cargar prendas: {ex.Message}");
            }
        }

        private void AplicarFiltro()
        {
            string texto = txtFiltro.Text.Trim().ToLower();
            int    idx   = cmbEstadoFiltro.SelectedIndex;  // 0=Todos 1=Disp 2=EnUso 3=Limpieza 4=Baja

            var lista = _prendas.FindAll(p =>
            {
                // Filtro estado
                bool pasaEstado = idx == 0
                    || (idx == 1 && p.Estado == BE.EstadoPrenda.Disponible)
                    || (idx == 2 && p.Estado == BE.EstadoPrenda.EnUso)
                    || (idx == 3 && p.Estado == BE.EstadoPrenda.EnLimpieza)
                    || (idx == 4 && p.Estado == BE.EstadoPrenda.Baja);

                // Filtro texto
                bool pasaTexto = string.IsNullOrEmpty(texto)
                    || p.Nombre.ToLower().Contains(texto)
                    || (p.Categoria ?? "").ToLower().Contains(texto)
                    || (p.Color ?? "").ToLower().Contains(texto)
                    || (p.Talle ?? "").ToLower().Contains(texto);

                return pasaEstado && pasaTexto;
            });

            var tabla = new DataTable();
            tabla.Columns.Add("ID",        typeof(int));
            tabla.Columns.Add("Nombre",    typeof(string));
            tabla.Columns.Add("Categoría", typeof(string));
            tabla.Columns.Add("Talle",     typeof(string));
            tabla.Columns.Add("Color",     typeof(string));
            tabla.Columns.Add("Estado",    typeof(string));
            tabla.Columns.Add("Cliente",   typeof(string));
            tabla.Columns.Add("Alta",      typeof(string));

            foreach (var p in lista)
            {
                tabla.Rows.Add(
                    p.IdPrenda,
                    p.Nombre,
                    p.Categoria ?? "—",
                    p.Talle ?? "—",
                    p.Color ?? "—",
                    EstadoLabel(p.Estado),
                    p.NombreCliente ?? "—",
                    p.FechaAlta.ToString("dd/MM/yyyy"));
            }

            dgvPrendas.DataSource = tabla;

            // Colorear filas según estado
            ColorearFilas();

            if (dgvPrendas.Columns.Contains("ID"))
                dgvPrendas.Columns["ID"].Width = 40;

            lblConteo.Text = $"Mostrando {lista.Count} de {_prendas.Count}";
            panelDetalle.Visible = false;
        }

        private void ColorearFilas()
        {
            if (dgvPrendas.DataSource == null) return;
            foreach (DataGridViewRow row in dgvPrendas.Rows)
            {
                string estado = row.Cells["Estado"].Value?.ToString() ?? "";
                row.DefaultCellStyle.ForeColor = estado switch
                {
                    "En Uso"       => Color.FromArgb(30, 100, 170),
                    "En Limpieza"  => Color.FromArgb(160, 100, 0),
                    "Baja"         => Color.FromArgb(160, 50, 50),
                    _              => Color.Black
                };
            }
        }

        private void DgvPrendas_SelectionChanged(object sender, EventArgs e)
        {
            bool hay = dgvPrendas.SelectedRows.Count > 0;
            if (_tieneStock)
            {
                btnEditar.Enabled       = hay;
                btnCambiarEstado.Enabled = hay;
            }

            // Mostrar panel detalle si la prenda está en uso
            panelDetalle.Visible = false;
            if (!hay) return;

            var prenda = ObtenerPrendaSeleccionada();
            if (prenda == null) return;

            if (prenda.Estado == BE.EstadoPrenda.EnUso && !string.IsNullOrEmpty(prenda.NombreCliente))
            {
                lblDetalleContenido.Text =
                    $"{prenda.NombreCliente}   |   " +
                    $"Prenda: {prenda.Nombre}  —  {prenda.Talle}  —  {prenda.Color}";
                panelDetalle.Visible = true;
            }
        }

        // ── Eventos de botones ────────────────────────────────────────────────

        private void BtnNueva_Click(object sender, EventArgs e)
        {
            using (var form = new PrendaForm())
            {
                if (form.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    prendaBLL.Alta(this, form.PrendaEditada);
                    MostrarOk($"Prenda '{form.PrendaEditada.Nombre}' agregada al catálogo.");
                    CargarPrendas();
                }
                catch (Exception ex) { MostrarError(ex.Message); }
            }
        }

        private void BtnEditar_Click(object sender, EventArgs e)
        {
            var prenda = ObtenerPrendaSeleccionada();
            if (prenda == null) return;

            using (var form = new PrendaForm(prenda))
            {
                if (form.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    prendaBLL.Modificar(this, form.PrendaEditada);
                    MostrarOk($"Prenda '{form.PrendaEditada.Nombre}' actualizada.");
                    CargarPrendas();
                }
                catch (Exception ex) { MostrarError(ex.Message); }
            }
        }

        private void BtnCambiarEstado_Click(object sender, EventArgs e)
        {
            var prenda = ObtenerPrendaSeleccionada();
            if (prenda == null) return;

            // Construir opciones de transición válidas
            var opciones = new List<(string texto, BE.EstadoPrenda estado)>();

            switch (prenda.Estado)
            {
                case BE.EstadoPrenda.Disponible:
                    opciones.Add(("Enviar a Limpieza", BE.EstadoPrenda.EnLimpieza));
                    opciones.Add(("Dar de Baja",       BE.EstadoPrenda.Baja));
                    break;
                case BE.EstadoPrenda.EnLimpieza:
                    opciones.Add(("Marcar Disponible", BE.EstadoPrenda.Disponible));
                    opciones.Add(("Dar de Baja",       BE.EstadoPrenda.Baja));
                    break;
                case BE.EstadoPrenda.EnUso:
                    MostrarError("No se puede cambiar el estado: la prenda está en uso por un cliente.");
                    return;
                case BE.EstadoPrenda.Baja:
                    MostrarError("La prenda está dada de baja y no puede ser reactivada.");
                    return;
            }

            // Mostrar diálogo de selección de nuevo estado
            using (var dlg = new CambioEstadoDialog(prenda, opciones))
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    prendaBLL.CambiarEstado(this, prenda, dlg.EstadoSeleccionado);
                    MostrarOk($"Estado de '{prenda.Nombre}' actualizado a {dlg.EstadoSeleccionado}.");
                    CargarPrendas();
                }
                catch (Exception ex) { MostrarError(ex.Message); }
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private BE.Prenda ObtenerPrendaSeleccionada()
        {
            if (dgvPrendas.SelectedRows.Count == 0) return null;
            int id = Convert.ToInt32(dgvPrendas.SelectedRows[0].Cells["ID"].Value);
            return _prendas.Find(p => p.IdPrenda == id);
        }

        private string EstadoLabel(BE.EstadoPrenda estado)
        {
            switch (estado)
            {
                case BE.EstadoPrenda.Disponible:  return "Disponible";
                case BE.EstadoPrenda.EnUso:        return "En Uso";
                case BE.EstadoPrenda.EnLimpieza:   return "En Limpieza";
                case BE.EstadoPrenda.Baja:         return "Baja";
                default:                           return estado.ToString();
            }
        }

        private void MostrarOk(string msg)
        {
            lblMensaje.ForeColor = Color.DarkGreen;
            lblMensaje.Text      = $"✓ {msg}";
        }

        private void MostrarError(string msg)
        {
            lblMensaje.ForeColor = Color.DarkRed;
            lblMensaje.Text      = $"✗ {msg}";
        }
    }
}
