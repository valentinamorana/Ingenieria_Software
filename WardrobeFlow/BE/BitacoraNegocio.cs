using System;

namespace BE
{
    /// <summary>
    /// Entidad — BitacoraNegocio.
    /// Registra eventos de negocio: ventas, despachos, cambios de stock y gestión de clientes.
    /// Separada de BE.Bitacora (que registra eventos de seguridad del sistema).
    /// Mapea la tabla [BitacoraNegocio].
    /// </summary>
    public class BitacoraNegocio
    {
        public int IdEvento { get; set; }
        public DateTime Fecha { get; set; }
        public TipoEventoNegocio Tipo { get; set; }

        // int? (nullable): el evento no siempre involucra todas las entidades, el campo queda null cuando no aplica para ese tipo de evento.
        public int? IdUsuario { get; set; }

        // Eventos de venta/despacho/entrega/cancelación.
        public int? IdPedido { get; set; }

        // Eventos de inventario.
        public int? IdPrenda { get; set; }

        // Eventos de gestión de clientes.
        public int? IdCliente { get; set; }

        // Descripción completa del evento.
        public string Descripcion { get; set; }

        // Campos cargados por JOIN: no existen en la tabla, se obtienen al SELECT con JOIN a Usuario/Cliente para mostrar nombres en pantalla.
        // Username del usuario que generó el evento.
        public string UsernameUsuario { get; set; }

        // Nombre completo del cliente involucrado. 
        public string NombreCliente { get; set; }
    }
}
