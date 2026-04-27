using System.Collections.Generic;
using System.Windows.Forms;

namespace BLL.Interfaces
{
    /// <summary>
    /// Contrato de negocio para la gestión del ciclo de vida de Pedidos.
    ///
    /// Casos de uso definidos:
    ///   CrearPedido()         — Vendedor genera un pedido para un cliente
    ///   Despachar()           — OperadorDeInventario despacha el pedido
    ///   MarcarEntregado()     — Se confirma la entrega al cliente
    ///   RegistrarDevolucion() — El cliente devuelve las prendas al finalizar
    ///   Cancelar()            — Se cancela un pedido Pendiente
    ///   DesCancelar()         — Se revierte la cancelación
    /// </summary>
    public interface IPedidoService
    {
        /// <summary>Devuelve todos los pedidos.</summary>
        List<BE.Pedido> ObtenerTodos();

        /// <summary>Devuelve solo los pedidos en estado Pendiente.</summary>
        List<BE.Pedido> ObtenerPendientes();

        /// <summary>Obtiene un pedido por ID con sus prendas asociadas.</summary>
        BE.Pedido ObtenerPorId(int id);

        /// <summary>
        /// Crea un nuevo pedido de venta validando plan, límites y disponibilidad de prendas.
        /// Devuelve el ID del pedido creado.
        /// </summary>
        int CrearPedido(Form formulario, int idCliente, List<BE.Prenda> prendas);

        /// <summary>Marca el pedido como Despachado. Solo válido desde estado Pendiente.</summary>
        void Despachar(Form formulario, BE.Pedido pedido);

        /// <summary>Marca el pedido como Entregado. Solo válido desde estado Despachado.</summary>
        void MarcarEntregado(Form formulario, BE.Pedido pedido);

        /// <summary>
        /// Registra la devolución de prendas por parte del cliente.
        /// Libera las prendas a estado Disponible o EnLimpieza según configuración.
        /// </summary>
        void RegistrarDevolucion(Form formulario, BE.Pedido pedido);

        /// <summary>Cancela un pedido Pendiente con un motivo obligatorio.</summary>
        void Cancelar(Form formulario, BE.Pedido pedido, string motivo);

        /// <summary>Revierte la cancelación de un pedido si las prendas siguen disponibles.</summary>
        void DesCancelar(Form formulario, BE.Pedido pedido);
    }
}
