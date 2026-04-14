using System;
using System.Collections.Generic;
using BE;
using DAL;

namespace BLL
{
    // BLL abstracta generica que implementa ICrud<T> delegando al DAL.
    // Tomada del proyecto de referencia sin modificaciones de logica.
    // Todas las BLL concretas heredan de esta clase.
    public abstract class AbstractBLL<T> : ICrud<T> where T : Entity
    {
        // Referencia a la capa de acceso a datos (inyectada por las subclases)
        protected ICrud<T> _crud;

        // Elimina la entidad delegando al DAL
        public void Delete(T entity)
        {
            _crud.Delete(entity);
        }

        // Devuelve todas las entidades delegando al DAL
        public IList<T> GetAll()
        {
            return _crud.GetAll();
        }

        // Busca por Id delegando al DAL
        public T GetById(Guid id)
        {
            return _crud.GetById(id);
        }

        // Guarda la entidad delegando al DAL
        public void Save(T entity)
        {
            _crud.Save(entity);
        }
    }
}
