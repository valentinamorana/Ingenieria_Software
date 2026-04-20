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
        public int                 IdEvento    { get; set; }
        public DateTime            Fecha       { get; set; }
        public TipoEventoNegocio   Tipo        { get; set; }

        /// <summary>FK → Usuario. El empleado que realizó la acción.</summary>
        public int?                IdUsuario   { get; set; }

        /// <summary>FK → Pedido. Solo aplica para eventos de venta/despacho/entrega/cancelación.</summary>
        public int?                IdPedido    { get; set; }

        /// <summary>FK → Prenda. Solo aplica para eventos de inventario.</summary>
        public int?                IdPrenda    { get; set; }

        /// <summary>FK → Cliente. Solo aplica para eventos de gestión de clientes.</summary>
        public int?                IdCliente   { get; set; }

        /// <summary>Descripción completa del evento.</summary>
        public string              Descripcion { get; set; }

        // ── Campos cargados por JOIN (no persisten) ──────────────────────────

        /// <summary>Username del usuario que generó el evento.</summary>
        public string UsernameUsuario { get; set; }

        /// <summary>Nombre completo del cliente involucrado.</summary>
        public string NombreCliente  { get; set; }
    }
}
