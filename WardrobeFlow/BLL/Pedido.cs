using System;
using System.Collections.Generic;
using System.Linq;

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
        public int CrearPedido(string modulo, int idCliente, List<BE.Prenda> prendas)
        {
            PermisosAccion.Exigir(BE.Patentes.PedidosVentaEditar, BE.Patentes.PedidosVenta);
            ValidarParametrosEntrada(prendas);

            var cliente = ObtenerClienteValidado(idCliente);

            // Bloquear si el cliente tiene un pedido despachado que aún no fue entregado
            var despachadoActivo = ObtenerTodos()
                .Find(p => p.IdCliente == idCliente && p.Estado == BE.EstadoPedido.Despachado);
            if (despachadoActivo != null)
                throw new BE.AppException("err.bll.pedido.ya_despachado",
                    "El cliente tiene el pedido #{0} despachado pendiente de entrega. " +
                    "Confirmá la entrega antes de crear uno nuevo.",
                    despachadoActivo.IdPedido);

            var plan    = ObtenerPlanValidado(cliente, prendas.Count);

            ValidarDisponibilidadPrendas(prendas);

            int idNuevo = PersistirPedido(idCliente, prendas);

            RegistrarHistorial(idNuevo, "CREAR", new List<(string, string, string)>
            {
                ("Estado",      null, BE.EstadoPedido.Pendiente.ToString()),
                ("FechaPedido", null, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
            });

            LogCrearPedido(modulo, idNuevo, cliente, plan, prendas.Count);

            return idNuevo;
        }

        // Despachar
        // Marca el pedido como Despachado.
        public void Despachar(string modulo, BE.Pedido pedido)
        {
            PermisosAccion.Exigir(BE.Patentes.PedidosVentaEditar, BE.Patentes.PedidosVenta);
            if (!pedido.PuedeDespachar())
                throw new BE.AppException("err.bll.pedido.despachar_estado",
                    "Solo se pueden despachar pedidos Pendientes. Este pedido está '{0}'.",
                    pedido.Estado);

            dalPedido.Despachar(pedido.IdPedido);

            RegistrarHistorial(pedido.IdPedido, "DESPACHAR", new List<(string, string, string)>
            {
                ("Estado",       pedido.Estado.ToString(),                          BE.EstadoPedido.Despachado.ToString()),
                ("FechaDespacho", pedido.FechaDespacho?.ToString("yyyy-MM-dd HH:mm:ss"), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
            });

            bitacora.Registrar(modulo,
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

        // Marcar Entregado
        // Marca el pedido como Entregado.
        public void MarcarEntregado(string modulo, BE.Pedido pedido)
        {
            PermisosAccion.Exigir(BE.Patentes.PedidosVentaEditar, BE.Patentes.PedidosVenta);
            if (!pedido.PuedeEntregarse())
                throw new BE.AppException("err.bll.pedido.entregar_estado",
                    "Solo se pueden marcar como entregados los pedidos Despachados. Este pedido está '{0}'.",
                    pedido.Estado);

            dalPedido.MarcarEntregado(pedido.IdPedido);

            RegistrarHistorial(pedido.IdPedido, "ENTREGAR", new List<(string, string, string)>
            {
                ("Estado",      pedido.Estado.ToString(),                         BE.EstadoPedido.Entregado.ToString()),
                ("FechaEntrega", pedido.FechaEntrega?.ToString("yyyy-MM-dd HH:mm:ss"), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
            });

            bitacora.Registrar(modulo,
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
        public void RegistrarDevolucion(string modulo, BE.Pedido pedido)
        {
            PermisosAccion.Exigir(BE.Patentes.PedidosRealizadosEditar, BE.Patentes.PedidosRealizados);
            if (pedido.Estado != BE.EstadoPedido.Entregado)
                throw new BE.AppException("err.bll.pedido.devolucion_estado",
                    "Solo se puede registrar la devolución de pedidos ya Entregados. Este pedido está '{0}'.",
                    pedido.Estado);

            int devueltas = dalPedido.RegistrarDevolucion(pedido.IdPedido, pedido.IdCliente);
            if (devueltas == 0)
                throw new BE.AppException("err.bll.pedido.devolucion_ya_hecha",
                    "El Pedido #{0} no tiene prendas en uso para devolver " +
                    "(es posible que la devolución ya se haya registrado).",
                    pedido.IdPedido);

            RegistrarHistorial(pedido.IdPedido, "DEVOLUCION", new List<(string, string, string)>
            {
                ("Prendas", "EnUso", $"EnLimpieza ({devueltas} prenda(s))")
            });

            bitacora.Registrar(modulo,
                $"Devolución Pedido #{pedido.IdPedido} — Cliente: {pedido.NombreCliente} — " +
                $"{devueltas} prenda(s) devuelta(s)",
                BE.Criticidad.Baja);

            bitacoraNeg.Registrar(
                BE.TipoEventoNegocio.Devolucion,
                $"Devolución pedido #{pedido.IdPedido} — Cliente: {pedido.NombreCliente} — " +
                $"{devueltas} prenda(s) pasan a EnLimpieza",
                idPedido:  pedido.IdPedido,
                idCliente: pedido.IdCliente);
        }

        // Cancelar
        // Cancela un pedido Pendiente. Requiere motivo.
        public void Cancelar(string modulo, BE.Pedido pedido, string motivo)
        {
            PermisosAccion.Exigir(BE.Patentes.PedidosVentaEditar, BE.Patentes.PedidosVenta);
            if (!pedido.PuedeCancelarse())
                throw new BE.AppException("err.bll.pedido.cancelar_estado",
                    "Solo se pueden cancelar pedidos en estado Pendiente. Este pedido está '{0}'.",
                    pedido.Estado);

            if (string.IsNullOrWhiteSpace(motivo))
                throw new BE.AppException("err.bll.pedido.cancelar_sin_motivo",
                    "Es obligatorio ingresar un motivo de cancelación.");

            dalPedido.Cancelar(pedido.IdPedido, motivo.Trim());

            RegistrarHistorial(pedido.IdPedido, "CANCELAR", new List<(string, string, string)>
            {
                ("Estado",             pedido.Estado.ToString(),            BE.EstadoPedido.Cancelado.ToString()),
                ("MotivoCancelacion",  pedido.MotivoCancelacion,           motivo.Trim())
            });

            bitacora.Registrar(modulo,
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
        public void DesCancelar(string modulo, BE.Pedido pedido)
        {
            PermisosAccion.Exigir(BE.Patentes.PedidosVentaEditar, BE.Patentes.PedidosVenta);
            if (!pedido.PuedeDesCancelarse())
                throw new BE.AppException("err.bll.pedido.descancelar_estado",
                    "Solo se pueden des-cancelar pedidos Cancelados. Este pedido está '{0}'.",
                    pedido.Estado);

            bool ok = dalPedido.DesCancelar(pedido.IdPedido, pedido.IdCliente);
            if (!ok)
                throw new BE.AppException("err.bll.pedido.descancelar_prendas",
                    "No se puede des-cancelar el Pedido #{0}. Una o más prendas ya no están disponibles.",
                    pedido.IdPedido);

            RegistrarHistorial(pedido.IdPedido, "DESCANCELAR", new List<(string, string, string)>
            {
                ("Estado",            pedido.Estado.ToString(),           BE.EstadoPedido.Pendiente.ToString()),
                ("MotivoCancelacion", pedido.MotivoCancelacion,          null)
            });

            bitacora.Registrar(modulo,
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
                throw new BE.AppException("err.bll.pedido.sin_prendas",
                    "Debe seleccionar al menos una prenda.");
        }

        // Busca el cliente y verifica que exista y tenga plan asignado.
        private BE.Cliente ObtenerClienteValidado(int idCliente)
        {
            var cliente = dalCliente.ObtenerPorId(idCliente);

            if (cliente == null)
                throw new BE.AppException("err.bll.pedido.cliente_inexistente",
                    "El cliente seleccionado no existe.");

            if (!cliente.TienePlan())
                throw new BE.AppException("err.bll.pedido.sin_plan",
                    "El cliente {0} no tiene plan asignado. Asignale un plan antes de crear un pedido.",
                    cliente.NombreCompleto);

            if (!cliente.SuscripcionVigente())
                throw new BE.AppException("err.bll.pedido.suscripcion_vencida",
                    "La suscripción de {0} venció el {1}. Renovar en el módulo de Clientes.",
                    cliente.NombreCompleto, cliente.FechaVencimiento.Value.ToString("dd/MM/yyyy"));

            return cliente;
        }

        // Verifica que el plan del cliente permita la cantidad de prendas pedidas.
        private BE.PlanSuscripcion ObtenerPlanValidado(BE.Cliente cliente, int cantidadPrendas)
        {
            if (!cliente.IdPlan.HasValue)
                throw new BE.AppException("err.bll.pedido.sin_plan",
                    "El cliente {0} no tiene plan asignado. Asignale un plan antes de crear un pedido.",
                    cliente.NombreCompleto);

            var plan = dalPlan.ObtenerPorId(cliente.IdPlan.Value);

            if (plan == null)
                throw new BE.AppException("err.bll.pedido.cliente_inexistente",
                    "No se pudo obtener el plan del cliente.");

            if (!cliente.PuedeSolicitarPrendas(cantidadPrendas))
                throw new BE.AppException("err.bll.pedido.limite_plan",
                    "El plan '{0}' permite {1} prenda(s). El cliente ya tiene {2} en uso y agrega {3} más. Máximo posible: {4}.",
                    plan.Nombre, plan.LimitePrendas, cliente.StockUtilizado, cantidadPrendas, cliente.PrendasDisponiblesEnPlan());

            return plan;
        }

        // Verifica que todas las prendas estén en estado Disponible.
        private void ValidarDisponibilidadPrendas(List<BE.Prenda> prendas)
        {
            foreach (var p in prendas)
            {
                if (!p.EstaDisponible())
                    throw new BE.AppException("err.bll.pedido.prenda_no_disponible",
                        "La prenda '{0}' ya no está disponible (estado: {1}). Actualizá la selección.",
                        p.Nombre, p.Estado);
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
        private void LogCrearPedido(string modulo, int idPedido,
                                    BE.Cliente cliente, BE.PlanSuscripcion plan, int cantidadPrendas)
        {
            bitacora.Registrar(modulo,
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
                throw new BE.AppException("err.bll.sesion_expirada",
                    "La sesión expiró. Volvé a iniciar sesión.");

            var usuario  = Seguridad.SessionManager.GetInstance().Usuario;
            var empleado = dalEmpleado.ObtenerPorUsuario(usuario.Id);
            if (empleado == null)
                throw new BE.AppException("err.bll.pedido.empleado_sin_vinculo",
                    "El usuario '{0}' no tiene un Empleado vinculado. " +
                    "Pedíle al Administrador que configure el vínculo.",
                    usuario.Username);
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
                var u = Seguridad.SessionManager.GetInstance().Usuario;
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

        /// <summary>
        /// Calcula el nivel de urgencia de un pedido según el tiempo transcurrido.
        /// Pendiente: Urgente > 3 días, Normal 1-3 días, Reciente &lt; 1 día.
        /// Despachado: Urgente > 5 días, Normal 2-5 días, Reciente &lt; 2 días.
        /// </summary>
        public BE.NivelUrgencia CalcularNivelUrgencia(BE.Pedido p)
        {
            if (p.Estado == BE.EstadoPedido.Entregado || p.Estado == BE.EstadoPedido.Cancelado)
                return BE.NivelUrgencia.NoAplica;

            double dias = (DateTime.Now - p.FechaPedido).TotalDays;

            if (p.Estado == BE.EstadoPedido.Pendiente)
            {
                if (dias > 3) return BE.NivelUrgencia.Urgente;
                if (dias > 1) return BE.NivelUrgencia.Normal;
                return BE.NivelUrgencia.Reciente;
            }

            if (p.Estado == BE.EstadoPedido.Despachado)
            {
                double diasDespacho = p.FechaDespacho.HasValue
                    ? (DateTime.Now - p.FechaDespacho.Value).TotalDays : dias;
                if (diasDespacho > 5) return BE.NivelUrgencia.Urgente;
                if (diasDespacho > 2) return BE.NivelUrgencia.Normal;
                return BE.NivelUrgencia.Reciente;
            }

            return BE.NivelUrgencia.NoAplica;
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
        public void RestaurarOperacion(string modulo, int idPedido, int idOperacion)
        {
            var cambios = dalHistorial.ObtenerPorOperacion(idPedido, idOperacion);
            if (cambios == null || cambios.Count == 0)
                throw new BE.AppException("err.bll.pedido.historial_vacio",
                    "No se encontraron cambios para la operación #{0} del Pedido #{1}.",
                    idOperacion, idPedido);

            string accionOriginal = cambios[0].Accion;

            // Restauración ATÓMICA: revertir todos los campos y reconciliar las prendas en UNA
            // sola transacción (si algo falla, no queda el pedido con un estado y las prendas con
            // otro). El DV se recalcula después: es recomputable y no debe abortar el rollback.
            dalPedido.RestaurarOperacionAtomica(idPedido,
                cambios.Select(c => (c.Campo, c.ValorAnterior)).ToList());
            // DV se recalcula fuera de la transacción: es recomputable y no debe
            // abortar ni enmascarar una restauración que ya se confirmó correctamente.
            try { dalPedido.RecalcularDV(); }
            catch { /* ignorado intencionalmente — el operador puede recalcular desde Diagnóstico */ }

            RegistrarHistorial(idPedido, "RESTAURAR",
                cambios.Select(c => (c.Campo, c.ValorNuevo, c.ValorAnterior)).ToList());

            bitacora.Registrar(modulo,
                $"Restaurar Pedido #{idPedido} — Revertida operación '{accionOriginal}' (op. #{idOperacion})",
                BE.Criticidad.Alta);

            bitacoraNeg.Registrar(
                BE.TipoEventoNegocio.Reactivacion,
                $"Pedido #{idPedido} restaurado — operación '{accionOriginal}' #{idOperacion} revertida",
                idPedido: idPedido);
        }
    }
}
