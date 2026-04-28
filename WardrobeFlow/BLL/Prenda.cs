using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BLL
{
    /// <summary>
    /// Capa de Lógica de Negocio — Gestión de Prendas.
    /// Implementa <see cref="Interfaces.IPrendaService"/>.
    ///
    /// RESPONSABILIDADES:
    ///   - Validar datos antes de persistir (campos obligatorios)
    ///   - Delegar validación de transiciones de estado a la entidad Prenda
    ///   - Registrar eventos en bitácora del sistema y de negocio
    ///
    /// Roles con acceso:
    ///   ControladorDeStock → CRUD completo + cambio de estado
    ///   OperadorLogistico  → solo lectura
    ///   Vendedor           → solo lectura (selección en pedidos)
    /// </summary>
    public class Prenda : Interfaces.IPrendaService
    {
        private readonly DAL.Prenda                dalPrenda   = new DAL.Prenda();
        private readonly Servicios.Bitacora        bitacora    = new Servicios.Bitacora();
        private readonly Servicios.BitacoraNegocio bitacoraNeg = new Servicios.BitacoraNegocio();

        // ── Consultas ─────────────────────────────────────────────────────────

        public List<BE.Prenda> ObtenerTodos()                   => dalPrenda.ObtenerTodos();
        public List<BE.Prenda> ObtenerDisponibles()            => dalPrenda.ObtenerDisponibles();
        public List<BE.Prenda> ObtenerPorCliente(int id)       => dalPrenda.ObtenerPorCliente(id);
        public BE.Prenda       ObtenerPorId(int idPrenda)      => dalPrenda.ObtenerPorId(idPrenda);

        // ── Alta ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Da de alta una nueva prenda. Estado inicial siempre Disponible.
        /// </summary>
        public void Alta(Form formulario, BE.Prenda prenda)
        {
            Validar(prenda);
            prenda.Estado    = BE.EstadoPrenda.Disponible;
            prenda.FechaAlta = DateTime.Now;

            int idNuevo = dalPrenda.Alta(prenda);
            prenda.IdPrenda = idNuevo;

            bitacora.Registrar(formulario.Text,
                $"Alta Prenda: {prenda.Nombre} (Talle {prenda.Talle}, {prenda.Color})",
                BE.Criticidad.Baja);

            bitacoraNeg.Registrar(
                BE.TipoEventoNegocio.AltaPrenda,
                $"Nueva prenda: {prenda.Nombre} — Talle {prenda.Talle} — {prenda.Color} — {prenda.Categoria}",
                idPrenda: idNuevo);
        }

        // ── Modificar ─────────────────────────────────────────────────────────

        /// <summary>
        /// Modifica los datos descriptivos de una prenda.
        /// No afecta estado ni cliente asignado.
        /// </summary>
        public void Modificar(Form formulario, BE.Prenda prenda)
        {
            Validar(prenda);
            dalPrenda.Modificar(prenda);

            bitacora.Registrar(formulario.Text,
                $"Modificar Prenda ID {prenda.IdPrenda}: {prenda.Nombre}",
                BE.Criticidad.Baja);

            bitacoraNeg.Registrar(BE.TipoEventoNegocio.ModificacionPrenda,
                $"Modificación prenda: '{prenda.Nombre}' (ID {prenda.IdPrenda}) — Talle {prenda.Talle}, {prenda.Color}",
                idPrenda: prenda.IdPrenda);
        }

        // ── Cambiar Estado ────────────────────────────────────────────────────

        /// <summary>
        /// Cambia el estado de una prenda.
        /// La validación de transición es delegada a <see cref="BE.Prenda.TransicionPermitida"/>,
        /// manteniendo las reglas de negocio centralizadas en la entidad.
        /// </summary>
        public void CambiarEstado(Form formulario, BE.Prenda prenda, BE.EstadoPrenda nuevoEstado)
        {
            if (!prenda.TransicionPermitida(nuevoEstado))
            {
                string motivo = prenda.MotivoTransicionNoPermitida(nuevoEstado)
                                ?? $"Transición no permitida: {prenda.Estado} → {nuevoEstado}.";
                throw new Exception(motivo);
            }

            int? idCliente = nuevoEstado == BE.EstadoPrenda.EnUso
                ? prenda.IdClienteActual
                : null;

            dalPrenda.CambiarEstado(prenda.IdPrenda, nuevoEstado, idCliente);

            bitacora.Registrar(formulario.Text,
                $"Estado Prenda ID {prenda.IdPrenda} '{prenda.Nombre}': {prenda.Estado} → {nuevoEstado}",
                BE.Criticidad.Media);

            bitacoraNeg.Registrar(
                BE.TipoEventoNegocio.CambioEstadoPrenda,
                $"Prenda '{prenda.Nombre}' (ID {prenda.IdPrenda}): {prenda.Estado} → {nuevoEstado}",
                idPrenda: prenda.IdPrenda);
        }

        // ── Validaciones internas ─────────────────────────────────────────────

        private void Validar(BE.Prenda prenda)
        {
            if (prenda == null)
                throw new ArgumentNullException(nameof(prenda));

            if (string.IsNullOrWhiteSpace(prenda.Nombre))
                throw new Exception("El nombre de la prenda es obligatorio.");

            if (string.IsNullOrWhiteSpace(prenda.Talle))
                throw new Exception("El talle es obligatorio.");

            if (string.IsNullOrWhiteSpace(prenda.Categoria))
                throw new Exception("La categoría es obligatoria.");
        }
    }
}
