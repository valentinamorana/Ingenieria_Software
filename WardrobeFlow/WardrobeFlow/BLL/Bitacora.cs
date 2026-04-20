using System;
using System.Data;

namespace BLL
{
    /// <summary>
    /// Capa de Lógica de Negocio — Consultas de Bitácora.
    /// Provee a la capa GUI los métodos necesarios para consultar y filtrar
    /// los registros de auditoría del sistema.
    ///
    /// Esta clase actúa como intermediario entre la GUI y la DAL:
    ///   GUI/Bitacora.cs → BLL.Bitacora → DAL.Bitacora → SQL Server
    ///
    /// No agrega lógica de negocio compleja, pero centraliza el acceso a datos
    /// de bitácora desde la presentación, respetando la arquitectura en capas.
    /// </summary>
    public class Bitacora
    {
        // Instancia de la clase DAL de Bitácora para operaciones de consulta
        private readonly DAL.Bitacora bitacoraDAL = new DAL.Bitacora();

        /// <summary>
        /// Obtiene todos los registros de la bitácora, ordenados por fecha descendente.
        /// Usado para la carga inicial del formulario de Bitácora.
        /// </summary>
        /// <returns>DataTable con todos los registros de auditoría.</returns>
        public DataTable ObtenerTodos()
        {
            return bitacoraDAL.ObtenerTodos();
        }

        /// <summary>
        /// Devuelve los registros de los últimos <paramref name="dias"/> días.
        /// Valida que el número sea positivo antes de delegar a la DAL.
        /// </summary>
        /// <param name="dias">Cantidad de días a consultar (mínimo 1).</param>
        public DataTable ObtenerUltimosNDias(int dias)
        {
            if (dias < 1) dias = 1;
            return bitacoraDAL.ObtenerUltimosNDias(dias);
        }

        /// <summary>
        /// Búsqueda combinada: fecha, usuario, actividad y criticidad.
        /// Cumple T06a: búsquedas por datos almacenados de manera combinada.
        /// </summary>
        public DataTable BuscarPorFiltros(DateTime? desde, DateTime? hasta, int idUsuario, string actividad, int criticidad)
        {
            return bitacoraDAL.BuscarPorFiltros(desde, hasta, idUsuario, actividad, criticidad);
        }
    }
}
