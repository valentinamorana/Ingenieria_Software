using System;
using System.Collections.Generic;

namespace BLL
{
    /// <summary>
    /// Capa de Lógica de Negocio — Gestión de Clientes.
    /// Los clientes son suscriptores del servicio (NO usuarios del sistema).
    /// El Vendedor es el único rol que puede crear y gestionar clientes.
    /// </summary>
    public class Cliente : Interfaces.IClienteService
    {
        private readonly DAL.Cliente               dalCliente  = new DAL.Cliente();
        private readonly Servicios.Bitacora        bitacora    = new Servicios.Bitacora();
        private readonly Servicios.BitacoraNegocio bitacoraNeg = new Servicios.BitacoraNegocio();

        /// <summary>Devuelve todos los clientes con plan y stock utilizado.</summary>
        public List<BE.Cliente> ObtenerTodos()
        {
            return dalCliente.ObtenerTodos();
        }

        /// <summary>Obtiene un cliente por ID.</summary>
        public BE.Cliente ObtenerPorId(int idCliente)
        {
            return dalCliente.ObtenerPorId(idCliente);
        }

        /// <summary>
        /// Registra un nuevo cliente.
        /// Valida campos obligatorios y unicidad de DNI.
        /// </summary>
        public void Alta(System.Windows.Forms.Form formulario, BE.Cliente cliente)
        {
            Validar(cliente);

            if (dalCliente.ExisteDNI(cliente.DNI))
                throw new Exception($"Ya existe un cliente con DNI {cliente.DNI}.");

            cliente.FechaAlta = DateTime.Now;
            int idNuevo = dalCliente.Alta(cliente);
            cliente.IdCliente = idNuevo;

            bitacora.Registrar(formulario.Text, $"Alta Cliente: {cliente.NombreCompleto} (DNI {cliente.DNI})", BE.Criticidad.Baja);
            bitacoraNeg.Registrar(BE.TipoEventoNegocio.AltaCliente,
                $"Nuevo cliente: {cliente.NombreCompleto} — DNI {cliente.DNI} — Plan: {cliente.NombrePlan ?? "Sin plan"}",
                idCliente: cliente.IdCliente);
        }

        /// <summary>
        /// Modifica los datos de un cliente existente.
        /// Valida unicidad de DNI excluyendo el propio registro.
        /// </summary>
        public void Modificar(System.Windows.Forms.Form formulario, BE.Cliente cliente)
        {
            Validar(cliente);

            // Verificar que el DNI no pertenezca a OTRO cliente
            var existente = dalCliente.ObtenerTodos()
                .Find(c => c.DNI == cliente.DNI && c.IdCliente != cliente.IdCliente);
            if (existente != null)
                throw new Exception($"El DNI {cliente.DNI} ya está registrado para otro cliente.");

            dalCliente.Modificar(cliente);
            bitacora.Registrar(formulario.Text, $"Modificar Cliente ID {cliente.IdCliente}: {cliente.NombreCompleto}", BE.Criticidad.Baja);
            bitacoraNeg.Registrar(BE.TipoEventoNegocio.ModificacionCliente,
                $"Modificación cliente: {cliente.NombreCompleto} — DNI {cliente.DNI}",
                idCliente: cliente.IdCliente);
        }

        /// <summary>
        /// Da de baja a un cliente.
        /// No se puede eliminar si tiene prendas actualmente en uso.
        /// </summary>
        public void Baja(System.Windows.Forms.Form formulario, BE.Cliente cliente)
        {
            // Bloquear baja si el cliente tiene prendas en uso actualmente
            if (cliente.StockUtilizado > 0)
                throw new Exception(
                    $"No se puede eliminar a {cliente.NombreCompleto}: " +
                    $"tiene {cliente.StockUtilizado} prenda(s) en uso. " +
                    "Primero registrá la devolución de las prendas.");

            dalCliente.Baja(cliente.IdCliente);
            bitacora.Registrar(formulario.Text, $"Baja Cliente ID {cliente.IdCliente}: {cliente.NombreCompleto}", BE.Criticidad.Media);
            bitacoraNeg.Registrar(BE.TipoEventoNegocio.BajaCliente,
                $"Baja cliente: {cliente.NombreCompleto} — DNI {cliente.DNI}",
                idCliente: cliente.IdCliente);
        }

        // ── Validaciones ─────────────────────────────────────────────────────

        private void Validar(BE.Cliente cliente)
        {
            if (cliente == null)
                throw new ArgumentNullException(nameof(cliente));

            if (string.IsNullOrWhiteSpace(cliente.Nombre))
                throw new Exception("El nombre del cliente es obligatorio.");

            if (string.IsNullOrWhiteSpace(cliente.Apellido))
                throw new Exception("El apellido del cliente es obligatorio.");

            if (string.IsNullOrWhiteSpace(cliente.DNI))
                throw new Exception("El DNI del cliente es obligatorio.");

            if (cliente.DNI.Length < 7 || cliente.DNI.Length > 8)
                throw new Exception("El DNI debe tener entre 7 y 8 dígitos.");

            foreach (char c in cliente.DNI)
                if (!char.IsDigit(c))
                    throw new Exception("El DNI solo puede contener números.");
        }
    }
}
