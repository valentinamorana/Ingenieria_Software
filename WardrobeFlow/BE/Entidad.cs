using System;

namespace BE
{
    /// <summary>
    /// Clase abstracta base para todas las entidades del dominio.
    ///
    /// PATRÓN HERENCIA:
    ///   Define el contrato mínimo que toda entidad persistida debe cumplir:
    ///   un identificador único y la fecha en que fue dada de alta en el sistema.
    ///
    ///   Las clases concretas que heredan:
    ///     Cliente  → agrega datos del suscriptor (nombre, DNI, plan)
    ///     Prenda   → agrega datos del artículo (talle, color, estado)
    ///
    ///   Las clases que NO heredan (tienen ciclo de vida diferente):
    ///     Usuario  → no tiene FechaAlta expuesta; su identidad es el Username
    ///     Pedido   → tiene FechaPedido en lugar de FechaAlta
    ///     Bitacora → es un registro de auditoría, no una entidad de negocio
    ///
    /// La clase es abstract: no puede instanciarse directamente.
    /// Obliga a las subclases a proveer su propio identificador concreto.
    /// </summary>
    public abstract class Entidad
    {
        /// <summary>
        /// Fecha y hora en que la entidad fue registrada en el sistema.
        /// Se asigna en el momento del alta y no se modifica después.
        /// </summary>
        public DateTime FechaAlta { get; set; }

        /// <summary>
        /// Retorna el identificador único de la entidad.
        /// Cada subclase lo implementa con su clave primaria concreta
        /// (IdCliente, IdPrenda, etc.) para mantener compatibilidad con la BD.
        /// </summary>
        public abstract int GetId();
    }
}
