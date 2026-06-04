using System;
using System.Collections.Generic;

namespace BLL
{
    /// <summary>Lógica de negocio para gestión de prendas.</summary>
    public class Prenda : Interfaces.IPrendaService
    {
        private readonly DAL.Prenda                  dalPrenda        = new DAL.Prenda();
        private readonly DAL.MantenimientoPrenda     dalMantenimiento = new DAL.MantenimientoPrenda();
        private readonly Servicios.Bitacora          bitacora         = new Servicios.Bitacora();
        private readonly Servicios.BitacoraNegocio   bitacoraNeg      = new Servicios.BitacoraNegocio();

        public List<BE.Prenda> ObtenerTodos()                   => dalPrenda.ObtenerTodos();
        public List<BE.Prenda> ObtenerDisponibles()            => dalPrenda.ObtenerDisponibles();
        public List<BE.Prenda> ObtenerPorCliente(int id)       => dalPrenda.ObtenerPorCliente(id);
        public BE.Prenda       ObtenerPorId(int idPrenda)      => dalPrenda.ObtenerPorId(idPrenda);

        // Da de alta una nueva prenda. Estado inicial siempre Disponible.
        public void Alta(string modulo, BE.Prenda prenda)
        {
            ValidarPermiso(BE.Patentes.Stock);
            Validar(prenda);
            prenda.Estado    = BE.EstadoPrenda.Disponible;
            prenda.FechaAlta = DateTime.Now;

            int idNuevo = dalPrenda.Alta(prenda);
            prenda.IdPrenda = idNuevo;

            bitacora.Registrar(modulo,
                $"Alta Prenda: {prenda.Nombre} (Talle {prenda.Talle}, {prenda.Color})",
                BE.Criticidad.Baja);

            bitacoraNeg.Registrar(
                BE.TipoEventoNegocio.AltaPrenda,
                $"Nueva prenda: {prenda.Nombre} — Talle {prenda.Talle} — {prenda.Color} — {prenda.Categoria}",
                idPrenda: idNuevo);
        }

        // Modifica los datos descriptivos de una prenda.
        // No afecta estado ni cliente asignado.
        public void Modificar(string modulo, BE.Prenda prenda)
        {
            ValidarPermiso(BE.Patentes.Stock);
            Validar(prenda);
            dalPrenda.Modificar(prenda);

            bitacora.Registrar(modulo,
                $"Modificar Prenda ID {prenda.IdPrenda}: {prenda.Nombre}",
                BE.Criticidad.Baja);

            bitacoraNeg.Registrar(BE.TipoEventoNegocio.ModificacionPrenda,
                $"Modificación prenda: '{prenda.Nombre}' (ID {prenda.IdPrenda}) — Talle {prenda.Talle}, {prenda.Color}",
                idPrenda: prenda.IdPrenda);
        }

        // Cambia el estado de una prenda validando la transición.
        // Al entrar a EnLimpieza abre un registro de mantenimiento;
        // al volver a Disponible desde EnLimpieza lo cierra.
        public void CambiarEstado(string modulo, BE.Prenda prenda, BE.EstadoPrenda nuevoEstado)
        {
            ValidarPermiso(BE.Patentes.Stock);
            if (!prenda.TransicionPermitida(nuevoEstado))
            {
                // Se lanza una clave traducible por caso (antes el motivo era texto fijo en
                // español que el traductor no podía localizar).
                if (prenda.Estado == BE.EstadoPrenda.Baja)
                    throw new BE.AppException("err.bll.prenda.transicion_baja",
                        "Una prenda dada de baja no puede cambiar de estado.");
                if (prenda.Estado == BE.EstadoPrenda.EnUso)
                    throw new BE.AppException("err.bll.prenda.transicion_enuso",
                        "No se puede cambiar manualmente el estado de una prenda en uso.\n" +
                        "El estado se actualiza automáticamente al procesar pedidos.");
                throw new BE.AppException("err.bll.prenda.transicion_generica",
                    "La transición de '{0}' a '{1}' no está permitida.",
                    prenda.Estado.ToString(), nuevoEstado.ToString());
            }

            int? idCliente = nuevoEstado == BE.EstadoPrenda.EnUso
                ? prenda.IdClienteActual
                : null;

            dalPrenda.CambiarEstado(prenda.IdPrenda, nuevoEstado, idCliente);

            if (nuevoEstado == BE.EstadoPrenda.EnLimpieza)
            {
                string actor = Seguridad.SessionManager.IsLoggedIn
                    ? Seguridad.SessionManager.GetInstance().Usuario.Username
                    : null;
                dalMantenimiento.IniciarMantenimiento(prenda.IdPrenda, actor);
            }
            else if (prenda.Estado == BE.EstadoPrenda.EnLimpieza &&
                     nuevoEstado == BE.EstadoPrenda.Disponible)
            {
                dalMantenimiento.CerrarMantenimiento(prenda.IdPrenda);
            }

            bitacora.Registrar(modulo,
                $"Estado Prenda ID {prenda.IdPrenda} '{prenda.Nombre}': {prenda.Estado} → {nuevoEstado}",
                BE.Criticidad.Media);

            bitacoraNeg.Registrar(
                BE.TipoEventoNegocio.CambioEstadoPrenda,
                $"Prenda '{prenda.Nombre}' (ID {prenda.IdPrenda}): {prenda.Estado} → {nuevoEstado}",
                idPrenda: prenda.IdPrenda);
        }

        public List<BE.MantenimientoPrenda> ObtenerHistorialMantenimiento(int idPrenda)
            => dalMantenimiento.ObtenerPorPrenda(idPrenda);

        public List<BE.MantenimientoPrenda> ObtenerEnMantenimiento()
        {
            var todos    = dalMantenimiento.ObtenerTodos();
            var abiertos = new List<BE.MantenimientoPrenda>();
            foreach (var m in todos)
                if (m.EstaAbierto) abiertos.Add(m);
            return abiertos;
        }

        // Devuelve el resumen de ocupación del stock para el Dashboard.
        public BE.OcupacionStock ObtenerOcupacion()
        {
            var todas = dalPrenda.ObtenerTodos();
            int enUso      = 0, enLimpieza = 0, disponibles = 0;
            foreach (var p in todas)
            {
                if      (p.Estado == BE.EstadoPrenda.EnUso)      enUso++;
                else if (p.Estado == BE.EstadoPrenda.EnLimpieza) enLimpieza++;
                else if (p.Estado == BE.EstadoPrenda.Disponible) disponibles++;
            }
            return new BE.OcupacionStock
            {
                Total       = todas.Count,
                EnUso       = enUso,
                EnLimpieza  = enLimpieza,
                Disponibles = disponibles
            };
        }

        // Lanza AppException si el usuario en sesión no tiene el permiso indicado.
        // Administrador siempre pasa. Si no hay sesión activa, deja pasar (modo interno/test).
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

        private void Validar(BE.Prenda prenda)
        {
            if (prenda == null)
                throw new ArgumentNullException(nameof(prenda));

            if (string.IsNullOrWhiteSpace(prenda.Nombre))
                throw new BE.AppException("err.bll.prenda.nombre_requerido",
                    "El nombre de la prenda es obligatorio.");

            if (string.IsNullOrWhiteSpace(prenda.Talle))
                throw new BE.AppException("err.bll.prenda.talle_requerido",
                    "El talle es obligatorio.");

            if (string.IsNullOrWhiteSpace(prenda.Categoria))
                throw new BE.AppException("err.bll.prenda.categoria_requerida",
                    "La categoría es obligatoria.");
        }
    }
}
