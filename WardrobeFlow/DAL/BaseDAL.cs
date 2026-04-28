using System.Collections.Generic;

namespace DAL
{
    /// <summary>
    /// Clase abstracta genérica base para todas las clases de Acceso a Datos.
    ///
    /// PATRÓN HERENCIA:
    ///   Centraliza el campo <see cref="acceso"/> (Singleton de BD) que todos
    ///   los DAL concretos necesitan. Sin esta clase, cada DAL declararía
    ///   su propio campo — código duplicado sin razón.
    ///
    ///   Jerarquía de herencia:
    ///     BaseDAL<Cliente>  ← DAL.Cliente
    ///     BaseDAL<Prenda>   ← DAL.Prenda
    ///     BaseDAL<Pedido>   ← DAL.Pedido
    ///     BaseDAL<Usuario>  ← DAL.Usuario
    ///     BaseDAL<BE.Bitacora> ← DAL.Bitacora
    ///
    /// MÉTODOS ABSTRACTOS:
    ///   ObtenerTodos() y ObtenerPorId() definen el contrato mínimo de lectura.
    ///   Cada subclase los implementa con su SQL específico.
    ///   Alta(), Modificar() y Baja() son opcionales — no todos los DAL los tienen.
    ///
    /// Restricción de tipo: T debe ser una clase (no valor primitivo).
    /// Esto garantiza que solo entidades del modelo puedan parametrizar este DAL.
    /// </summary>
    /// <typeparam name="T">Tipo de entidad que persiste este DAL.</typeparam>
    public abstract class BaseDAL<T> where T : class
    {
        /// <summary>
        /// Instancia única del Singleton de acceso a BD.
        /// Disponible para todas las subclases sin que cada una lo declare por separado.
        /// </summary>
        protected readonly Acceso acceso = Acceso.GetInstance();

        /// <summary>
        /// Retorna todos los registros de la tabla correspondiente.
        /// Cada subclase implementa el SELECT con sus columnas y JOINs específicos.
        /// </summary>
        public abstract List<T> ObtenerTodos();

        /// <summary>
        /// Retorna un registro por su clave primaria.
        /// Cada subclase implementa el WHERE con su columna de clave primaria.
        /// </summary>
        public abstract T ObtenerPorId(int id);
    }
}
