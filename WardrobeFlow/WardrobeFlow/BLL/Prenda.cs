using System;
using System.Collections.Generic;

namespace BLL
{
    /// <summary>
    /// Capa de Lógica de Negocio — Gestión de Prendas.
    ///
    /// RESPONSABILIDADES:
    ///   - Validar datos antes de persistir
    ///   - Controlar cambios de estado (reglas de negocio)
    ///   - Registrar eventos en bitácora
    ///
    /// Roles con acceso:
    ///   ControladorDeStock → CRUD completo + cambio de estado
    ///   OperadorLogistico  → solo lectura (mnuPrendas sin mnuStock)
    ///   Vendedor           → solo lectura para selección en pedidos
    /// </summary>
    public class Prenda
    {
        private readonly DAL.Prenda                dalPrenda   = new DAL.Prenda();
        private readonly Servicios.Bitacora        bitacora    = new Servicios.Bitacora();
        private readonly Servicios.BitacoraNegocio bitacoraNeg = new Servicios.BitacoraNegocio();

        /// <summary>Devuelve todas las prendas con cliente actual (JOIN).</summary>
        public List<BE.Prenda> ObtenerTodas()
        {
            return dalPrenda.ObtenerTodas();
        }

        /// <summary>Devuelve solo las prendas disponibles (para selección en pedidos).</summary>
        public List<BE.Prenda> ObtenerDisponibles()
        {
            return dalPrenda.ObtenerDisponibles();
        }

        /// <summary>Devuelve las prendas actualmente asignadas a un cliente.</summary>
        public List<BE.Prenda> ObtenerPorCliente(int idCliente)
        {
            return dalPrenda.ObtenerPorCliente(idCliente);
        }

        /// <summary>Obtiene una prenda por ID.</summary>
        public BE.Prenda ObtenerPorId(int idPrenda)
        {
            return dalPrenda.ObtenerPorId(idPrenda);
        }

        /// <summary>
        /// Da de alta una nueva prenda en el catálogo.
        /// Estado inicial siempre Disponible.
        /// </summary>
        public void Alta(System.Windows.Forms.Form formulario, BE.Prenda prenda)
        {
            Validar(prenda);
            prenda.Estado    = BE.EstadoPrenda.Disponible;
            prenda.FechaAlta = DateTime.Now;

            int idNuevo = dalPrenda.Alta(prenda);
            prenda.IdPrenda = idNuevo;

            bitacora.Registrar(formulario,
                $"Alta Prenda: {prenda.Nombre} (Talle {prenda.Talle}, {prenda.Color})",
                BE.Criticidad.Baja);

            bitacoraNeg.Registrar(
                BE.TipoEventoNegocio.AltaPrenda,
                $"Nueva prenda: {prenda.Nombre} — Talle {prenda.Talle} — {prenda.Color} — {prenda.Categoria}",
                idPrenda: idNuevo);
        }

        /// <summary>
        /// Modifica los datos descriptivos de una prenda.
        /// No afecta estado ni cliente asignado.
        /// </summary>
        public void Modificar(System.Windows.Forms.Form formulario, BE.Prenda prenda)
        {
            Validar(prenda);
            dalPrenda.Modificar(prenda);

            bitacora.Registrar(formulario,
                $"Modificar Prenda ID {prenda.IdPrenda}: {prenda.Nombre}",
                BE.Criticidad.Baja);
        }

        /// <summary>
        /// Cambia el estado de una prenda con validación de transiciones permitidas.
        ///
        /// Transiciones válidas:
        ///   Disponible  → EnLimpieza, Baja
        ///   EnLimpieza  → Disponible, Baja
        ///   EnUso       → (solo desde DAL cuando se cancela/devuelve un pedido)
        ///   Baja        → (irreversible desde la UI)
        /// </summary>
        public void CambiarEstado(System.Windows.Forms.Form formulario,
                                   BE.Prenda prenda, BE.EstadoPrenda nuevoEstado)
        {
            ValidarTransicion(prenda.Estado, nuevoEstado);

            // Al pasar a Disponible o Baja, limpiar cliente asignado
            int? idCliente = nuevoEstado == BE.EstadoPrenda.EnUso
                ? prenda.IdClienteActual
                : null;

            dalPrenda.CambiarEstado(prenda.IdPrenda, nuevoEstado, idCliente);

            bitacora.Registrar(formulario,
                $"Estado Prenda ID {prenda.IdPrenda} '{prenda.Nombre}': " +
                $"{prenda.Estado} → {nuevoEstado}",
                BE.Criticidad.Media);

            bitacoraNeg.Registrar(
                BE.TipoEventoNegocio.CambioEstadoPrenda,
                $"Prenda '{prenda.Nombre}' (ID {prenda.IdPrenda}): {prenda.Estado} → {nuevoEstado}",
                idPrenda: prenda.IdPrenda);
        }

        // ── Validaciones ─────────────────────────────────────────────────────

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

        private void ValidarTransicion(BE.EstadoPrenda actual, BE.EstadoPrenda nuevo)
        {
            if (actual == nuevo) return;

            if (actual == BE.EstadoPrenda.Baja)
                throw new Exception("Una prenda dada de baja no puede cambiar de estado.");

            if (actual == BE.EstadoPrenda.EnUso)
                throw new Exception(
                    "No se puede cambiar el estado de una prenda en uso.\n" +
                    "Primero debe ser devuelta por el cliente.");
        }
    }
}
