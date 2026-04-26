using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BLL
{
    /// <summary>
    /// Capa de Lógica de Negocio — Gestión de Pedidos de Venta.
    ///
    /// RESPONSABILIDADES:
    ///   - Validar que el cliente tenga plan asignado
    ///   - Validar que la cantidad de prendas no supere el límite del plan
    ///   - Resolver el Empleado activo desde la sesión
    ///   - Coordinar la creación atómica (cabecera + prendas + cambio de estado)
    ///   - Registrar en bitácora
    ///
    /// Roles con acceso:
    ///   Vendedor             → Alta y cancelación de pedidos propios
    ///   OperadorDeInventario → Despacho (Part 6)
    ///   Supervisor           → Solo lectura (Part 7)
    /// </summary>
    public class Pedido
    {
        private readonly DAL.Pedido                dalPedido     = new DAL.Pedido();
        private readonly DAL.Cliente               dalCliente    = new DAL.Cliente();
        private readonly DAL.Empleado              dalEmpleado   = new DAL.Empleado();
        private readonly DAL.PlanSuscripcion       dalPlan       = new DAL.PlanSuscripcion();
        private readonly Servicios.Bitacora        bitacora      = new Servicios.Bitacora();
        private readonly Servicios.BitacoraNegocio bitacoraNeg   = new Servicios.BitacoraNegocio();

        // ── Consultas ─────────────────────────────────────────────────────────

        public List<BE.Pedido> ObtenerTodos()       => dalPedido.ObtenerTodos();
        public List<BE.Pedido> ObtenerPendientes()  => dalPedido.ObtenerPendientes();
        public BE.Pedido       ObtenerPorId(int id) => dalPedido.ObtenerPorId(id);

        // ── Alta ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Crea un nuevo pedido de venta.
        ///
        /// Validaciones:
        ///   1. Debe haber al menos una prenda seleccionada.
        ///   2. El cliente debe tener plan asignado.
        ///   3. Prendas actuales en uso + nuevas prendas ≤ límite del plan.
        ///   4. Todas las prendas deben estar en estado Disponible.
        ///   5. El usuario activo debe tener un Empleado vinculado.
        /// </summary>
        public int Alta(Form formulario, int idCliente, List<BE.Prenda> prendas)
        {
            if (prendas == null || prendas.Count == 0)
                throw new Exception("Debe seleccionar al menos una prenda.");

            // Cargar cliente con stock actualizado
            var cliente = dalCliente.ObtenerPorId(idCliente);
            if (cliente == null)
                throw new Exception("El cliente seleccionado no existe.");

            if (!cliente.IdPlan.HasValue)
                throw new Exception(
                    $"El cliente {cliente.NombreCompleto} no tiene plan asignado.\n" +
                    "Asignale un plan antes de crear un pedido.");

            // Validar límite del plan
            var plan = dalPlan.ObtenerPorId(cliente.IdPlan.Value);
            if (plan == null)
                throw new Exception("No se pudo obtener el plan del cliente.");

            int totalConNuevas = cliente.StockUtilizado + prendas.Count;
            if (totalConNuevas > plan.LimitePrendas)
                throw new Exception(
                    $"El plan '{plan.Nombre}' permite {plan.LimitePrendas} prenda(s).\n" +
                    $"El cliente ya tiene {cliente.StockUtilizado} en uso y está agregando {prendas.Count} más.\n" +
                    $"Máximo posible en este pedido: {plan.LimitePrendas - cliente.StockUtilizado}.");

            // Verificar que todas las prendas siguen disponibles
            foreach (var p in prendas)
            {
                if (p.Estado != BE.EstadoPrenda.Disponible)
                    throw new Exception(
                        $"La prenda '{p.Nombre}' ya no está disponible (estado: {p.Estado}).\n" +
                        "Actualizá la selección y volvé a intentar.");
            }

            // Resolver Empleado del usuario activo
            int idEmpleado = ResolverEmpleadoActivo();

            var pedido = new BE.Pedido
            {
                IdCliente   = idCliente,
                IdEmpleado  = idEmpleado,
                Estado      = BE.EstadoPedido.Pendiente,
                FechaPedido = DateTime.Now,
                Prendas     = prendas
            };

            int idNuevo = dalPedido.Alta(pedido);

            bitacora.Registrar(formulario,
                $"Nuevo Pedido #{idNuevo} — Cliente: {cliente.NombreCompleto} — " +
                $"{prendas.Count} prenda(s) — Plan: {plan.Nombre}",
                BE.Criticidad.Media);

            bitacoraNeg.Registrar(
                BE.TipoEventoNegocio.Venta,
                $"Pedido #{idNuevo} — {cliente.NombreCompleto} — " +
                $"{prendas.Count} prenda(s) — Plan {plan.Nombre} — {pedido.FechaPedido:dd/MM/yyyy HH:mm}",
                idPedido:  idNuevo,
                idCliente: idCliente);

            return idNuevo;
        }

        // ── Despacho y entrega ────────────────────────────────────────────────

        /// <summary>
        /// Marca un pedido como Despachado.
        /// Solo aplica si el pedido está en estado Pendiente.
        /// El OperadorDeInventario es quien ejecuta esta acción.
        /// </summary>
        public void Despachar(Form formulario, BE.Pedido pedido)
        {
            if (pedido.Estado != BE.EstadoPedido.Pendiente)
                throw new Exception(
                    $"Solo se pueden despachar pedidos Pendientes.\n" +
                    $"Este pedido está '{pedido.Estado}'.");

            dalPedido.Despachar(pedido.IdPedido);

            bitacora.Registrar(formulario,
                $"Despachar Pedido #{pedido.IdPedido} — Cliente: {pedido.NombreCliente} — " +
                $"{pedido.CantidadPrendas} prenda(s)",
                BE.Criticidad.Media);

            bitacoraNeg.Registrar(
                BE.TipoEventoNegocio.Despacho,
                $"Pedido #{pedido.IdPedido} despachado — Cliente: {pedido.NombreCliente} — " +
                $"{pedido.CantidadPrendas} prenda(s)",
                idPedido:  pedido.IdPedido,
                idCliente: pedido.IdCliente);
        }

        /// <summary>
        /// Marca un pedido como Entregado al cliente.
        /// Solo aplica si el pedido está en estado Despachado.
        /// </summary>
        public void MarcarEntregado(Form formulario, BE.Pedido pedido)
        {
            if (pedido.Estado != BE.EstadoPedido.Despachado)
                throw new Exception(
                    $"Solo se pueden marcar como entregados los pedidos Despachados.\n" +
                    $"Este pedido está '{pedido.Estado}'.");

            dalPedido.MarcarEntregado(pedido.IdPedido);

            bitacora.Registrar(formulario,
                $"Entrega Pedido #{pedido.IdPedido} — Cliente: {pedido.NombreCliente}",
                BE.Criticidad.Baja);

            bitacoraNeg.Registrar(
                BE.TipoEventoNegocio.Entrega,
                $"Pedido #{pedido.IdPedido} entregado — Cliente: {pedido.NombreCliente}",
                idPedido:  pedido.IdPedido,
                idCliente: pedido.IdCliente);
        }

        // ── Cancelación ───────────────────────────────────────────────────────

        /// <summary>
        /// Cancela un pedido pendiente, guarda el motivo y libera las prendas a Disponible.
        /// Solo se puede cancelar si el estado es Pendiente.
        /// </summary>
        public void Cancelar(Form formulario, BE.Pedido pedido, string motivo)
        {
            if (pedido.Estado != BE.EstadoPedido.Pendiente)
                throw new Exception(
                    $"Solo se pueden cancelar pedidos en estado Pendiente.\n" +
                    $"Este pedido está '{pedido.Estado}'.");

            if (string.IsNullOrWhiteSpace(motivo))
                throw new Exception("Es obligatorio ingresar un motivo de cancelación.");

            dalPedido.Cancelar(pedido.IdPedido, motivo.Trim());

            bitacora.Registrar(formulario,
                $"Cancelar Pedido #{pedido.IdPedido} — Cliente: {pedido.NombreCliente} — Motivo: {motivo}",
                BE.Criticidad.Media);

            bitacoraNeg.Registrar(
                BE.TipoEventoNegocio.Cancelacion,
                $"Pedido #{pedido.IdPedido} cancelado — Cliente: {pedido.NombreCliente} — Motivo: {motivo}",
                idPedido:  pedido.IdPedido,
                idCliente: pedido.IdCliente);
        }

        /// <summary>
        /// Revierte la cancelación de un pedido, volviendo a Pendiente y re-asignando prendas.
        /// Solo posible si todas las prendas del pedido siguen Disponibles.
        /// </summary>
        public void DesCancelar(Form formulario, BE.Pedido pedido)
        {
            if (pedido.Estado != BE.EstadoPedido.Cancelado)
                throw new Exception(
                    $"Solo se pueden des-cancelar pedidos Cancelados.\n" +
                    $"Este pedido está '{pedido.Estado}'.");

            bool ok = dalPedido.DesCancelar(pedido.IdPedido, pedido.IdCliente);

            if (!ok)
                throw new Exception(
                    $"No se puede des-cancelar el Pedido #{pedido.IdPedido}.\n" +
                    "Una o más prendas del pedido ya no están disponibles\n" +
                    "(fueron asignadas a otro pedido o cambiaron de estado).");

            bitacora.Registrar(formulario,
                $"Des-cancelar Pedido #{pedido.IdPedido} — Cliente: {pedido.NombreCliente}",
                BE.Criticidad.Media);

            bitacoraNeg.Registrar(
                BE.TipoEventoNegocio.Venta,
                $"Pedido #{pedido.IdPedido} des-cancelado — regresó a Pendiente — Cliente: {pedido.NombreCliente}",
                idPedido:  pedido.IdPedido,
                idCliente: pedido.IdCliente);
        }

        // ── Helpers privados ──────────────────────────────────────────────────

        /// <summary>
        /// Busca el Empleado vinculado al usuario activo en la sesión.
        /// Lanza excepción si el usuario no tiene empleado asociado.
        /// </summary>
        private int ResolverEmpleadoActivo()
        {
            if (!Seguridad.SessionManager.IsLoggedIn)
                throw new Exception("No hay sesión activa.");

            int idUsuario = Seguridad.SessionManager.GetInstance.Usuario.Id;
            var empleado  = dalEmpleado.ObtenerPorUsuario(idUsuario);

            if (empleado == null)
                throw new Exception(
                    "El usuario activo no tiene un Empleado vinculado en el sistema.\n" +
                    "Contactá al Administrador para vincular tu usuario a un empleado.");

            return empleado.IdEmpleado;
        }
    }
}
