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
            ValidarPermiso("mnuStock");
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
            ValidarPermiso("mnuStock");
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
            ValidarPermiso("mnuStock");
            if (!prenda.TransicionPermitida(nuevoEstado))
            {
                string motivo = prenda.MotivoTransicionNoPermitida(nuevoEstado)
                                ?? $"Transición no permitida: {prenda.Estado} → {nuevoEstado}.";
                throw new BE.AppException("err.bll.prenda.transicion_invalida", motivo);
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

        // Lanza AppException si el usuario en sesión no tiene el permiso indicado.
        // Administrador siempre pasa. Si no hay sesión activa, deja pasar (modo interno/test).
        private static void ValidarPermiso(string nombrePatente)
        {
            if (!Seguridad.SessionManager.IsLoggedIn) return;
            var usuario = Seguridad.SessionManager.GetInstance().Usuario;
            if (usuario.Perfil == "Administrador") return;
            bool tiene = usuario.Permisos?.Exists(p => p.NombreMenu == nombrePatente) == true;
            if (!tiene)
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
