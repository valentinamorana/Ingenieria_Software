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
        private readonly DAL.Interfaces.IClienteDAL dalCliente;
        private readonly DAL.PlanSuscripcion       dalPlan     = new DAL.PlanSuscripcion();
        private readonly Servicios.Bitacora        bitacora    = new Servicios.Bitacora();
        private readonly Servicios.BitacoraNegocio bitacoraNeg = new Servicios.BitacoraNegocio();

        // DI: el constructor por defecto usa el DAL real; el otro permite inyectar un doble.
        public Cliente() : this(new DAL.Cliente()) { }
        public Cliente(DAL.Interfaces.IClienteDAL dalCliente)
        {
            this.dalCliente = dalCliente;
        }

        // Lanza AppException si el usuario en sesión no posee el permiso indicado.
        private static void ValidarPermiso(string nombrePatente)
        {
            // Fail-closed: sin sesión NO se permite la operación (apunte T04).
            if (!Seguridad.SessionManager.IsLoggedIn)
                throw new BE.AppException("err.bll.sesion_expirada",
                    "La sesión expiró. Volvé a iniciar sesión.");
            if (!Seguridad.SessionManager.GetInstance().TienePermiso(nombrePatente))
                throw new BE.AppException("err.bll.sin_permiso",
                    "No tiene permiso para ejecutar esta operación ('{0}').", nombrePatente);
        }

        // Devuelve todos los clientes con plan y stock utilizado.
        public List<BE.Cliente> ObtenerTodos()
        {
            return dalCliente.ObtenerTodos();
        }

        // Obtiene un cliente por ID.
        public BE.Cliente ObtenerPorId(int idCliente)
        {
            return dalCliente.ObtenerPorId(idCliente);
        }

        // Registra un nuevo cliente.
        // Valida campos obligatorios y unicidad de DNI.
        public void Alta(string modulo, BE.Cliente cliente)
        {
            ValidarPermiso(BE.Patentes.Clientes);
            Validar(cliente);

            if (dalCliente.ExisteDNI(cliente.DNI))
                throw new BE.AppException("err.bll.cliente.dni_duplicado",
                    "Ya existe un cliente con DNI {0}.", cliente.DNI);

            cliente.FechaAlta = DateTime.Now;
            int idNuevo = dalCliente.Alta(cliente);
            cliente.IdCliente = idNuevo;

            bitacora.Registrar(modulo, $"Alta Cliente: {cliente.NombreCompleto} (DNI {cliente.DNI})", BE.Criticidad.Baja);
            bitacoraNeg.Registrar(BE.TipoEventoNegocio.AltaCliente,
                $"Nuevo cliente: {cliente.NombreCompleto} — DNI {cliente.DNI} — Plan: {cliente.NombrePlan ?? "Sin plan"}",
                idCliente: cliente.IdCliente);
        }

        // Modifica los datos de un cliente existente.
        // Valida unicidad de DNI excluyendo el propio registro.
        // Bloquea si el nuevo plan tiene menos capacidad que las prendas actualmente en uso.
        public void Modificar(string modulo, BE.Cliente cliente)
        {
            ValidarPermiso(BE.Patentes.Clientes);
            Validar(cliente);

            if (dalCliente.ExisteDNIParaOtro(cliente.DNI, cliente.IdCliente))
                throw new BE.AppException("err.bll.cliente.dni_duplicado_otro",
                    "El DNI {0} ya está registrado para otro cliente.", cliente.DNI);

            var actual = dalCliente.ObtenerPorId(cliente.IdCliente);

            // Bloquear si el nuevo plan tiene menos capacidad que prendas en uso
            if (actual != null && cliente.IdPlan.HasValue && cliente.IdPlan != actual.IdPlan)
            {
                var nuevoPlan = dalPlan.ObtenerPorId(cliente.IdPlan.Value);
                if (nuevoPlan != null && actual.StockUtilizado > nuevoPlan.LimitePrendas)
                    throw new BE.AppException("err.bll.cliente.plan_insuficiente",
                        "No se puede asignar el plan '{0}': el cliente tiene {1} prenda(s) en uso " +
                        "y ese plan solo permite {2}. Registrá las devoluciones primero.",
                        nuevoPlan.Nombre, actual.StockUtilizado, nuevoPlan.LimitePrendas);
            }

            // Construir detalle de auditoría incluyendo cambio de plan si ocurrió
            string detalle = $"Modificación cliente: {cliente.NombreCompleto} — DNI {cliente.DNI}";
            if (actual != null && cliente.IdPlan != actual.IdPlan)
            {
                string planAnterior = actual.NombrePlan ?? (actual.IdPlan.HasValue ? $"ID {actual.IdPlan}" : "Sin plan");
                string planNuevo    = cliente.NombrePlan ?? (cliente.IdPlan.HasValue ? $"ID {cliente.IdPlan}" : "Sin plan");
                detalle += $" | Plan: {planAnterior} → {planNuevo}";
            }

            dalCliente.Modificar(cliente);
            bitacora.Registrar(modulo, $"Modificar Cliente ID {cliente.IdCliente}: {cliente.NombreCompleto}", BE.Criticidad.Baja);
            bitacoraNeg.Registrar(BE.TipoEventoNegocio.ModificacionCliente, detalle, idCliente: cliente.IdCliente);
        }

        // Da de baja a un cliente.
        // No se puede eliminar si tiene prendas actualmente en uso.
        public void Baja(string modulo, BE.Cliente cliente)
        {
            ValidarPermiso(BE.Patentes.Clientes);
            // Bloquear baja si el cliente tiene prendas en uso actualmente
            if (cliente.StockUtilizado > 0)
                throw new BE.AppException("err.bll.cliente.baja_prendas",
                    "No se puede eliminar a {0}: tiene {1} prenda(s) en uso. Registrá la devolución primero.",
                    cliente.NombreCompleto, cliente.StockUtilizado);

            dalCliente.Baja(cliente.IdCliente);
            bitacora.Registrar(modulo, $"Baja Cliente ID {cliente.IdCliente}: {cliente.NombreCompleto}", BE.Criticidad.Media);
            bitacoraNeg.Registrar(BE.TipoEventoNegocio.BajaCliente,
                $"Baja cliente: {cliente.NombreCompleto} — DNI {cliente.DNI}",
                idCliente: cliente.IdCliente);
        }

        // Evalúa si un cliente puede crear un pedido con la cantidad de prendas indicada.
        // Devuelve un DTO listo para que la GUI lo muestre sin interpretar reglas de negocio.
        public BE.EstadoComercialCliente ObtenerEstadoComercial(BE.Cliente cliente, int prendasSolicitadas)
        {
            if (cliente == null) throw new ArgumentNullException(nameof(cliente));

            if (!cliente.TienePlan())
                return new BE.EstadoComercialCliente
                {
                    PuedeProceder  = false,
                    MotivoBloqueo  = "SIN_PLAN",
                    MetodoPago     = cliente.MetodoPago,
                    FechaAlta      = cliente.FechaAlta
                };

            if (!cliente.SuscripcionVigente())
                return new BE.EstadoComercialCliente
                {
                    PuedeProceder    = false,
                    MotivoBloqueo    = "SUSCRIPCION_VENCIDA",
                    NombrePlan       = cliente.NombrePlan,
                    FechaVencimiento = cliente.FechaVencimiento,
                    MetodoPago       = cliente.MetodoPago,
                    FechaAlta        = cliente.FechaAlta
                };

            bool superaLimite    = !cliente.PuedeSolicitarPrendas(prendasSolicitadas);
            int  disponibles     = cliente.PrendasDisponiblesEnPlan();
            int  exceso          = superaLimite
                ? (cliente.StockUtilizado + prendasSolicitadas) - cliente.LimitePrendas
                : 0;

            return new BE.EstadoComercialCliente
            {
                PuedeProceder    = true,
                NombrePlan       = cliente.NombrePlan,
                StockUtilizado   = cliente.StockUtilizado,
                LimitePrendas    = cliente.LimitePrendas,
                MetodoPago       = cliente.MetodoPago,
                FechaAlta        = cliente.FechaAlta,
                FechaVencimiento = cliente.FechaVencimiento,
                SuperaLimite     = superaLimite,
                PrendasDisponibles = disponibles,
                Exceso           = exceso,
                SuscripcionProximaAVencer = cliente.SuscripcionProximaAVencer(),
                DiasHastaVencimiento      = cliente.DiasHastaVencimiento()
            };
        }

        // Validaciones
        private void Validar(BE.Cliente cliente)
        {
            if (cliente == null)
                throw new ArgumentNullException(nameof(cliente));

            if (string.IsNullOrWhiteSpace(cliente.Nombre))
                throw new BE.AppException("err.bll.cliente.nombre_requerido",
                    "El nombre del cliente es obligatorio.");

            if (string.IsNullOrWhiteSpace(cliente.Apellido))
                throw new BE.AppException("err.bll.cliente.apellido_requerido",
                    "El apellido del cliente es obligatorio.");

            if (string.IsNullOrWhiteSpace(cliente.DNI))
                throw new BE.AppException("err.bll.cliente.dni_requerido",
                    "El DNI del cliente es obligatorio.");

            if (cliente.DNI.Length < 7 || cliente.DNI.Length > 8)
                throw new BE.AppException("err.bll.cliente.dni_formato",
                    "El DNI debe tener entre 7 y 8 dígitos.");

            foreach (char c in cliente.DNI)
                if (!char.IsDigit(c))
                    throw new BE.AppException("err.bll.cliente.dni_numeros",
                        "El DNI solo puede contener números.");

            if (!cliente.FechaNacimiento.HasValue)
                throw new BE.AppException("err.bll.cliente.fechanac_requerida",
                    "La fecha de nacimiento es obligatoria.");

            if (cliente.FechaNacimiento.Value.Date > DateTime.Today.AddYears(-18))
                throw new BE.AppException("err.bll.cliente.menor_edad",
                    "El cliente debe ser mayor de 18 años.");
        }
    }
}
