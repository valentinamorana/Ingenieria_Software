using System;
using System.Collections.Generic;
using System.Data;

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
    ///   ObtenerHistorial()    — Devuelve el historial de cambios de un pedido
    ///   RestaurarOperacion()  — Restaura el pedido al estado previo a una operación
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
        int CrearPedido(string modulo, int idCliente, List<BE.Prenda> prendas);

        // Marca el pedido como Despachado. Solo válido desde estado Pendiente.
        void Despachar(string modulo, BE.Pedido pedido);

        // Marca el pedido como Entregado. Solo válido desde estado Despachado.
        void MarcarEntregado(string modulo, BE.Pedido pedido);

        // Registra la devolución de prendas por parte del cliente.
        // Libera las prendas a estado Disponible o EnLimpieza según configuración.
        void RegistrarDevolucion(string modulo, BE.Pedido pedido);

        // Cancela un pedido Pendiente con un motivo obligatorio.
        void Cancelar(string modulo, BE.Pedido pedido, string motivo);

        // Revierte la cancelación de un pedido si las prendas siguen disponibles.
        void DesCancelar(string modulo, BE.Pedido pedido);

        // Devuelve el historial de cambios de un pedido con filtros opcionales.
        System.Data.DataTable ObtenerHistorial(int idPedido, string accion = null,
                                               System.DateTime? desde = null,
                                               System.DateTime? hasta = null);

        // Restaura el pedido al estado previo a la operación indicada.
        void RestaurarOperacion(string modulo, int idPedido, int idOperacion);

        // Calcula el nivel de urgencia de un pedido según su estado y antigüedad.
        BE.NivelUrgencia CalcularNivelUrgencia(BE.Pedido pedido);
    }
}
