using System;
using System.Collections.Generic;
using System.Linq;
using BE;

namespace DAL
{
    // Implementacion abstracta del CRUD usando una lista en memoria.
    // Tomada del proyecto de referencia sin modificaciones de logica.
    // Todas las clases DAL concretas heredan de esta.
    public abstract class AbstractDAL<T> : ICrud<T> where T : Entity
    {
        // Lista interna que actua como "base de datos" en memoria
        protected IList<T> dataContext;

        // Constructor: inicializa la lista vacia
        public AbstractDAL()
        {
            dataContext = new List<T>();
        }

        // Elimina la entidad de la lista
        public void Delete(T entity)
        {
            dataContext.Remove(entity);
        }

        // Devuelve todas las entidades de la lista
        public IList<T> GetAll()
        {
            return dataContext;
        }

        // Busca por Guid usando LINQ
        public T GetById(Guid id)
        {
            return dataContext.Where(i => i.Id.Equals(id)).FirstOrDefault();
        }

        // Si la entidad ya existe en la lista (por referencia), no hace nada.
        // Si es nueva, la agrega. (En un sistema real, aqui iria la llamada a la BD)
        public void Save(T entity)
        {
            if (dataContext.Contains(entity))
            {
                // La entidad ya existe; en persistencia real se actualizaria aqui
            }
            else
            {
                // Entidad nueva: se agrega a la coleccion
                dataContext.Add(entity);
            }
        }
    }
}
