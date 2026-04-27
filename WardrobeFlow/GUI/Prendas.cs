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

        private List<BE.Prenda> _prendas = new List<BE.Prenda>();

        public Prendas()
        {
            InitializeComponent();

            // Verificar si el usuario activo tiene permiso de stock
            var bllUsuario = new BLL.Usuario();
            var usuario    = bllUsuario.ObtenerUsuarioActivo();
            _tieneStock = usuario?.Permisos?.Exists(p => p.NombreMenu == "mnuStock") == true;

            // Ocultar botones de acción si el usuario no tiene permiso de stock
            btnNueva.Visible        = _tieneStock;
            btnEditar.Visible       = _tieneStock;
            btnCambiarEstado.Visible = _tieneStock;

            this.Load += new EventHandler(Prendas_Load);
        }

        // ── Eventos del Designer ──────────────────────────────────────────────

        private void Prendas_Load(object sender, EventArgs e)
        {
            CargarPrendas();
        }

        private void CmbEstadoFiltro_SelectedIndexChanged(object sender, EventArgs e)
        {
            AplicarFiltro();
        }

        private void TxtFiltro_TextChanged(object sender, EventArgs e)
        {
            AplicarFiltro();
        }

        private void BtnRefrescar_Click(object sender, EventArgs e)
        {
            CargarPrendas();
        }

        private void DgvPrendas_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (_tieneStock) BtnEditar_Click(sender, e);
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
                btnEditar.Enabled        = hay;
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
