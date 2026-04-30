using System.Collections.Generic;
using System.Windows.Forms;

namespace BLL.Interfaces
{
    /// <summary>
    /// Gestión del ciclo de vida de Pedidos.
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
        // Devuelve todos los pedidos.
        List<BE.Pedido> ObtenerTodos();

        // Devuelve solo los pedidos en estado Pendiente.
        List<BE.Pedido> ObtenerPendientes();

        // Obtiene un pedido por ID con sus prendas asociadas.
        BE.Pedido ObtenerPorId(int id);

        // Crea un nuevo pedido de venta validando plan, límites y disponibilidad de prendas.
        // Devuelve el ID del pedido creado.
        int CrearPedido(Form formulario, int idCliente, List<BE.Prenda> prendas);

        // Marca el pedido como Despachado. Solo válido desde estado Pendiente.
        void Despachar(Form formulario, BE.Pedido pedido);

        // Marca el pedido como Entregado. Solo válido desde estado Despachado.
        void MarcarEntregado(Form formulario, BE.Pedido pedido);

        // Registra la devolución de prendas por parte del cliente.
        // Libera las prendas a estado Disponible o EnLimpieza según configuración.
        void RegistrarDevolucion(Form formulario, BE.Pedido pedido);

        // Cancela un pedido Pendiente con un motivo obligatorio.
        void Cancelar(Form formulario, BE.Pedido pedido, string motivo);

        // Revierte la cancelación de un pedido si las prendas siguen disponibles.
        void DesCancelar(Form formulario, BE.Pedido pedido);
    }
}
