using System.Collections.Generic;

namespace DAL
{
    /// <summary>Clase base abstracta para todos los DAL. Centraliza el acceso a BD.</summary>
    /// <typeparam name="T">Tipo de entidad que persiste este DAL.</typeparam>
    public abstract class BaseDAL<T> where T : class
    {
        protected readonly Acceso acceso = Acceso.GetInstance();

        public abstract List<T> ObtenerTodos();
        public abstract T ObtenerPorId(int id);
    }
}
