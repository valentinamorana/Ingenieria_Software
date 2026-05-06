using System;

namespace BE
{
    /// <summary>
    /// Entidad — Registro de un cambio de campo sobre un Pedido.
    /// Mapea la tabla [PedidoHistorial].
    ///
    /// Estrategia por campo: cada fila representa un único campo modificado.
    /// IdOperacion agrupa todos los campos cambiados en el mismo evento de negocio
    /// (por ejemplo, al Despachar se generan dos filas con el mismo IdOperacion:
    /// una para Estado y otra para FechaDespacho).
    ///
    /// Acciones posibles (campo Accion):
    ///   CREAR       — creación del pedido
    ///   DESPACHAR   — pedido pasó a Despachado
    ///   ENTREGAR    — pedido pasó a Entregado
    ///   CANCELAR    — pedido fue cancelado
    ///   DESCANCELAR — cancelación revertida
    ///   DEVOLUCION  — prendas devueltas
    ///   RESTAURAR   — estado restaurado desde historial
    ///
    /// Campos rastreados (campo Campo):
    ///   Estado | FechaDespacho | FechaEntrega | MotivoCancelacion | Prendas
    /// </summary>
    public class PedidoHistorial
    {
        public int      IdHistorial   { get; set; }
        public int      IdPedido      { get; set; }

        /// <summary>
        /// Agrupa todos los campos cambiados en un mismo evento de negocio.
        /// Permite restaurar de forma atómica todos los campos de una operación.
        /// </summary>
        public int      IdOperacion   { get; set; }

        public DateTime Fecha         { get; set; }
        public int?     IdUsuario     { get; set; }
        public string   NombreUsuario { get; set; }

        /// <summary>Tipo de operación: CREAR, DESPACHAR, ENTREGAR, CANCELAR, DESCANCELAR, DEVOLUCION, RESTAURAR.</summary>
        public string   Accion        { get; set; }

        /// <summary>Nombre del campo modificado: Estado, FechaDespacho, FechaEntrega, MotivoCancelacion, Prendas.</summary>
        public string   Campo         { get; set; }

        public string   ValorAnterior { get; set; }
        public string   ValorNuevo    { get; set; }
    }
}
