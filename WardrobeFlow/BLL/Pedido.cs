using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace BLL
{
    /// <summary>
    /// Lógica de negocio para gestión del ciclo de vida de pedidos.
    /// </summary>
    public class Pedido : Interfaces.IPedidoService
    {
        private readonly DAL.Pedido dalPedido = new DAL.Pedido();
        private readonly DAL.Cliente dalCliente = new DAL.Cliente();
        private readonly DAL.Empleado dalEmpleado = new DAL.Empleado();
        private readonly DAL.PlanSuscripcion dalPlan = new DAL.PlanSuscripcion();
        private readonly Servicios.Bitacora bitacora = new Servicios.Bitacora();
        private readonly Servicios.BitacoraNegocio bitacoraNeg = new Servicios.BitacoraNegocio();
        private readonly DAL.PedidoHistorial dalHistorial = new DAL.PedidoHistorial();

        // Consultas 
        public List<BE.Pedido> ObtenerTodos() => dalPedido.ObtenerTodos();
        public List<BE.Pedido> ObtenerPendientes() => dalPedido.ObtenerPendientes();
        public BE.Pedido ObtenerPorId(int id) => dalPedido.ObtenerPorId(id);

        // Crear Pedido 
        // Crea un nuevo pedido para un cliente. Devuelve el ID generado.
        public int CrearPedido(Form formulario, int idCliente, List<BE.Prenda> prendas)
        {
            ValidarParametrosEntrada(prendas);

            var cliente = ObtenerClienteValidado(idCliente);
            var plan    = ObtenerPlanValidado(cliente, prendas.Count);

            ValidarDisponibilidadPrendas(prendas);

            int idNuevo = PersistirPedido(idCliente, prendas);

            RegistrarHistorial(idNuevo, "CREAR", new List<(string, string, string)>
            {
                ("Estado",      null, BE.EstadoPedido.Pendiente.ToString()),
                ("FechaPedido", null, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
            });

            LogCrearPedido(formulario, idNuevo, cliente, plan, prendas.Count);

            return idNuevo;
        }

        // Despachar
        // Marca el pedido como Despachado.
        public void Despachar(Form formulario, BE.Pedido pedido)
        {
            if (!pedido.PuedeDespachar())
                throw new Exception(
                    $"Solo se pueden despachar pedidos Pendientes.\n" +
                    $"Este pedido está '{pedido.Estado}'.");

            dalPedido.Despachar(pedido.IdPedido);

            RegistrarHistorial(pedido.IdPedido, "DESPACHAR", new List<(string, string, string)>
            {
                ("Estado",       pedido.Estado.ToString(),                          BE.EstadoPedido.Despachado.ToString()),
                ("FechaDespacho", pedido.FechaDespacho?.ToString("yyyy-MM-dd HH:mm:ss"), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
            });

            bitacora.Registrar(formulario.Text,
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

        //Marcar Entregado 
        // Marca el pedido como Entregado.
        public void MarcarEntregado(Form formulario, BE.Pedido pedido)
        {
            if (!pedido.PuedeEntregarse())
                throw new Exception(
                    $"Solo se pueden marcar como entregados los pedidos Despachados.\n" +
                    $"Este pedido está '{pedido.Estado}'.");

            dalPedido.MarcarEntregado(pedido.IdPedido);

            RegistrarHistorial(pedido.IdPedido, "ENTREGAR", new List<(string, string, string)>
            {
                ("Estado",      pedido.Estado.ToString(),                         BE.EstadoPedido.Entregado.ToString()),
                ("FechaEntrega", pedido.FechaEntrega?.ToString("yyyy-MM-dd HH:mm:ss"), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
            });

            bitacora.Registrar(formulario.Text,
                $"Entrega Pedido #{pedido.IdPedido} — Cliente: {pedido.NombreCliente}",
                BE.Criticidad.Baja);

            bitacoraNeg.Registrar(
                BE.TipoEventoNegocio.Entrega,
                $"Pedido #{pedido.IdPedido} entregado — Cliente: {pedido.NombreCliente}",
                idPedido:  pedido.IdPedido,
                idCliente: pedido.IdCliente);
        }

        // Registrar Devolución
        // Registra la devolución de prendas de un pedido Entregado.
        public void RegistrarDevolucion(Form formulario, BE.Pedido pedido)
        {
            if (pedido.Estado != BE.EstadoPedido.Entregado)
                throw new Exception(
                    "Solo se puede registrar la devolución de pedidos ya Entregados.\n" +
                    $"Este pedido está '{pedido.Estado}'.");

            dalPedido.RegistrarDevolucion(pedido.IdPedido);

            RegistrarHistorial(pedido.IdPedido, "DEVOLUCION", new List<(string, string, string)>
            {
                ("Prendas", "EnUso", $"EnLimpieza ({pedido.CantidadPrendas} prenda(s))")
            });

            bitacora.Registrar(formulario.Text,
                $"Devolución Pedido #{pedido.IdPedido} — Cliente: {pedido.NombreCliente} — " +
                $"{pedido.CantidadPrendas} prenda(s) devuelta(s)",
                BE.Criticidad.Baja);

            bitacoraNeg.Registrar(
                BE.TipoEventoNegocio.Devolucion,
                $"Devolución pedido #{pedido.IdPedido} — Cliente: {pedido.NombreCliente} — " +
                $"{pedido.CantidadPrendas} prenda(s) pasan a EnLimpieza",
                idPedido:  pedido.IdPedido,
                idCliente: pedido.IdCliente);
        }

        // Cancelar
        // Cancela un pedido Pendiente. Requiere motivo.
        public void Cancelar(Form formulario, BE.Pedido pedido, string motivo)
        {
            if (!pedido.PuedeCancelarse())
                throw new Exception(
                    $"Solo se pueden cancelar pedidos en estado Pendiente.\n" +
                    $"Este pedido está '{pedido.Estado}'.");

            if (string.IsNullOrWhiteSpace(motivo))
                throw new Exception("Es obligatorio ingresar un motivo de cancelación.");

            dalPedido.Cancelar(pedido.IdPedido, motivo.Trim());

            RegistrarHistorial(pedido.IdPedido, "CANCELAR", new List<(string, string, string)>
            {
                ("Estado",             pedido.Estado.ToString(),            BE.EstadoPedido.Cancelado.ToString()),
                ("MotivoCancelacion",  pedido.MotivoCancelacion,           motivo.Trim())
            });

            bitacora.Registrar(formulario.Text,
                $"Cancelar Pedido #{pedido.IdPedido} — Cliente: {pedido.NombreCliente} — Motivo: {motivo}",
                BE.Criticidad.Media);

            bitacoraNeg.Registrar(
                BE.TipoEventoNegocio.Cancelacion,
                $"Pedido #{pedido.IdPedido} cancelado — Cliente: {pedido.NombreCliente} — Motivo: {motivo}",
                idPedido:  pedido.IdPedido,
                idCliente: pedido.IdCliente);
        }

        // Des-Cancelar
        // Revierte la cancelación si todas las prendas siguen Disponibles.
        public void DesCancelar(Form formulario, BE.Pedido pedido)
        {
            if (!pedido.PuedeDesCancelarse())
                throw new Exception(
                    $"Solo se pueden des-cancelar pedidos Cancelados.\n" +
                    $"Este pedido está '{pedido.Estado}'.");

            bool ok = dalPedido.DesCancelar(pedido.IdPedido, pedido.IdCliente);
            if (!ok)
                throw new Exception(
                    $"No se puede des-cancelar el Pedido #{pedido.IdPedido}.\n" +
                    "Una o más prendas del pedido ya no están disponibles.");

            RegistrarHistorial(pedido.IdPedido, "DESCANCELAR", new List<(string, string, string)>
            {
                ("Estado",            pedido.Estado.ToString(),           BE.EstadoPedido.Pendiente.ToString()),
                ("MotivoCancelacion", pedido.MotivoCancelacion,          null)
            });

            bitacora.Registrar(formulario.Text,
                $"Des-cancelar Pedido #{pedido.IdPedido} — Cliente: {pedido.NombreCliente}",
                BE.Criticidad.Media);

            bitacoraNeg.Registrar(
                BE.TipoEventoNegocio.Reactivacion,
                $"Pedido #{pedido.IdPedido} des-cancelado — vuelve a Pendiente — Cliente: {pedido.NombreCliente}",
                idPedido:  pedido.IdPedido,
                idCliente: pedido.IdCliente);
        }

        // Valida que la lista de prendas no sea nula ni vacía.
        private void ValidarParametrosEntrada(List<BE.Prenda> prendas)
        {
            if (prendas == null || prendas.Count == 0)
                throw new Exception("Debe seleccionar al menos una prenda.");
        }

        // Busca el cliente y verifica que exista y tenga plan asignado.
        private BE.Cliente ObtenerClienteValidado(int idCliente)
        {
            var cliente = dalCliente.ObtenerPorId(idCliente);

            if (cliente == null)
                throw new Exception("El cliente seleccionado no existe.");

            if (!cliente.TienePlan())
                throw new Exception(
                    $"El cliente {cliente.NombreCompleto} no tiene plan asignado.\n" +
                    "Asignale un plan antes de crear un pedido.");

            return cliente;
        }

        // Verifica que el plan del cliente permita la cantidad de prendas pedidas.
        private BE.PlanSuscripcion ObtenerPlanValidado(BE.Cliente cliente, int cantidadPrendas)
        {
            if (!cliente.IdPlan.HasValue)
                throw new Exception(
                    $"El cliente {cliente.NombreCompleto} no tiene plan asignado.\n" +
                    "Asignale un plan antes de crear un pedido.");

            var plan = dalPlan.ObtenerPorId(cliente.IdPlan.Value);

            if (plan == null)
                throw new Exception("No se pudo obtener el plan del cliente.");

            if (!cliente.PuedeSolicitarPrendas(cantidadPrendas))
                throw new Exception(
                    $"El plan '{plan.Nombre}' permite {plan.LimitePrendas} prenda(s).\n" +
                    $"El cliente ya tiene {cliente.StockUtilizado} en uso y está agregando {cantidadPrendas} más.\n" +
                    $"Máximo posible en este pedido: {cliente.PrendasDisponiblesEnPlan()}.");

            return plan;
        }

        // Verifica que todas las prendas estén en estado Disponible.
        private void ValidarDisponibilidadPrendas(List<BE.Prenda> prendas)
        {
            foreach (var p in prendas)
            {
                if (!p.EstaDisponible())
                    throw new Exception(
                        $"La prenda '{p.Nombre}' ya no está disponible (estado: {p.Estado}).\n" +
                        "Actualizá la selección y volvé a intentar.");
            }
        }

        // Construye el pedido y lo persiste en BD. Devuelve el ID generado.
        private int PersistirPedido(int idCliente, List<BE.Prenda> prendas)
        {
            int idEmpleado = ResolverEmpleadoActivo();

            var pedido = new BE.Pedido
            {
                IdCliente  = idCliente,
                IdEmpleado = idEmpleado,
                Estado     = BE.EstadoPedido.Pendiente,
                FechaPedido = DateTime.Now,
                Prendas    = prendas
            };

            return dalPedido.Alta(pedido);
        }

        // Registra la creación del pedido en bitácora del sistema y de negocio.
        private void LogCrearPedido(Form formulario, int idPedido,
                                    BE.Cliente cliente, BE.PlanSuscripcion plan, int cantidadPrendas)
        {
            bitacora.Registrar(formulario.Text,
                $"CrearPedido #{idPedido} — Cliente: {cliente.NombreCompleto} — " +
                $"{cantidadPrendas} prenda(s) — Plan: {plan.Nombre}",
                BE.Criticidad.Media);

            bitacoraNeg.Registrar(
                BE.TipoEventoNegocio.Venta,
                $"Pedido #{idPedido} — {cliente.NombreCompleto} — " +
                $"{cantidadPrendas} prenda(s) — Plan {plan.Nombre} — {DateTime.Now:dd/MM/yyyy HH:mm}",
                idPedido:  idPedido,
                idCliente: cliente.IdCliente);
        }

        // Obtiene el IdEmpleado del usuario en sesión.
        private int ResolverEmpleadoActivo()
        {
            if (!Seguridad.SessionManager.IsLoggedIn)
                throw new Exception("La sesión expiró. Volvé a iniciar sesión.");

            var usuario  = Seguridad.SessionManager.GetInstance.Usuario;
            var empleado = dalEmpleado.ObtenerPorUsuario(usuario.Id);
            if (empleado == null)
                throw new Exception(
                    $"El usuario '{usuario.Username}' no tiene un Empleado vinculado.\n" +
                    "Pedíle al Administrador que configure el vínculo.");
            return empleado.IdEmpleado;
        }

        // ── Historial ─────────────────────────────────────────────────────────

        /// <summary>
        /// Crea y persiste registros de historial para todos los campos cambiados en un evento.
        /// Obtiene el usuario de la sesión activa; si no hay sesión registra como anónimo.
        /// </summary>
        private void RegistrarHistorial(int idPedido, string accion,
                                        List<(string Campo, string Anterior, string Nuevo)> campos)
        {
            int?   idUsuario     = null;
            string nombreUsuario = null;

            if (Seguridad.SessionManager.IsLoggedIn)
            {
                var u = Seguridad.SessionManager.GetInstance.Usuario;
                idUsuario     = u.Id;
                nombreUsuario = u.Username;
            }

            int idOp = dalHistorial.ObtenerSiguienteIdOperacion(idPedido);

            var registros = campos.Select(c => new BE.PedidoHistorial
            {
                IdPedido      = idPedido,
                IdOperacion   = idOp,
                Fecha         = DateTime.Now,
                IdUsuario     = idUsuario,
                NombreUsuario = nombreUsuario,
                Accion        = accion,
                Campo         = c.Campo,
                ValorAnterior = c.Anterior,
                ValorNuevo    = c.Nuevo
            }).ToList();

            dalHistorial.RegistrarCambios(registros);
        }

        /// <summary>Devuelve el historial de cambios de un pedido con filtros opcionales.</summary>
        public System.Data.DataTable ObtenerHistorial(
            int idPedido, string accion = null,
            DateTime? desde = null, DateTime? hasta = null)
            => dalHistorial.ObtenerPorPedido(idPedido, accion, desde, hasta);

        /// <summary>
        /// Restaura el pedido al estado previo a la operación indicada (por IdOperacion).
        /// Revierte cada campo al ValorAnterior registrado y escribe un evento RESTAURAR.
        /// </summary>
        public void RestaurarOperacion(Form formulario, int idPedido, int idOperacion)
        {
            var cambios = dalHistorial.ObtenerPorOperacion(idPedido, idOperacion);
            if (cambios == null || cambios.Count == 0)
                throw new Exception(
                    $"No se encontraron cambios para la operación #{idOperacion} del Pedido #{idPedido}.");

            string accionOriginal = cambios[0].Accion;

            foreach (var c in cambios)
                dalHistorial.RestaurarCampo(idPedido, c.Campo, c.ValorAnterior);

            RegistrarHistorial(idPedido, "RESTAURAR",
                cambios.Select(c => (c.Campo, c.ValorNuevo, c.ValorAnterior)).ToList());

            bitacora.Registrar(formulario.Text,
                $"Restaurar Pedido #{idPedido} — Revertida operación '{accionOriginal}' (op. #{idOperacion})",
                BE.Criticidad.Alta);

            bitacoraNeg.Registrar(
                BE.TipoEventoNegocio.Reactivacion,
                $"Pedido #{idPedido} restaurado — operación '{accionOriginal}' #{idOperacion} revertida",
                idPedido: idPedido);
        }
    }
}
