using BE;
using System;

namespace Servicios
{
    /// <summary>
    /// Capa de Servicios — BitacoraNegocio.
    /// Centraliza el registro de eventos de negocio desde BLL.
    /// Resuelve automáticamente el IdUsuario desde la sesión activa.
    ///
    /// Los eventos de negocio se almacenan en la tabla [BitacoraNegocio],
    /// separada de la tabla [Bitacora] que registra eventos de seguridad del sistema.
    /// </summary>
    public class BitacoraNegocio
    {
        private readonly DAL.BitacoraNegocio dal = new DAL.BitacoraNegocio();

        /// <summary>
        /// Registra un evento de negocio.
        /// IdUsuario se resuelve automáticamente desde SessionManager si hay sesión activa.
        /// </summary>
        public void Registrar(
            TipoEventoNegocio tipo,
            string descripcion,
            int? idPedido  = null,
            int? idPrenda  = null,
            int? idCliente = null)
        {
            try
            {
                int? idUsuario = null;
                if (Seguridad.SessionManager.IsLoggedIn)
                    idUsuario = Seguridad.SessionManager.GetInstance.Usuario.Id;

                var evento = new BE.BitacoraNegocio
                {
                    Fecha       = DateTime.Now,
                    Tipo        = tipo,
                    IdUsuario   = idUsuario,
                    IdPedido    = idPedido,
                    IdPrenda    = idPrenda,
                    IdCliente   = idCliente,
                    Descripcion = descripcion
                };

                dal.Registrar(evento);
            }
            catch
            {
                // No interrumpir el flujo de negocio por error de bitácora
            }
        }
    }
}
