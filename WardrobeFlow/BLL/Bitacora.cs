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
        /// Busca registros de bitácora aplicando filtros combinados.
        /// Todos los parámetros son opcionales (null o vacíos = sin filtro).
        /// Cumple con el requisito de "búsquedas por datos almacenados de manera combinada".
        /// </summary>
        /// <param name="desde">Fecha de inicio del rango (null = sin límite inferior).</param>
        /// <param name="hasta">Fecha de fin del rango (null = sin límite superior).</param>
        /// <param name="usuario">ID de usuario a filtrar (0 = todos los usuarios).</param>
        /// <param name="actividad">Texto parcial de actividad a buscar (vacío = todas).</param>
        /// <param name="criticidad">Nivel de criticidad a filtrar (0 = todos los niveles).</param>
        /// <returns>DataTable con los registros que cumplen los filtros indicados.</returns>
        public DataTable BuscarPorFiltros(DateTime? desde, DateTime? hasta, int usuario, string actividad, int criticidad)
        {
            return bitacoraDAL.BuscarPorFiltros(desde, hasta, usuario, actividad, criticidad);
        }
    }
}
