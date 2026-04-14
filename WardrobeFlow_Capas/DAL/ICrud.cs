using System;
using System.Collections.Generic;
using BE;

namespace DAL
{
    // Interfaz generica CRUD que define las operaciones de acceso a datos.
    // Tomada del proyecto de referencia, adaptada al namespace de WardrobeFlow.
    // T debe heredar de Entity (tiene Guid Id).
    public interface ICrud<T> where T : Entity
    {
        // Busca una entidad por su Id unico
        T GetById(Guid id);

        // Devuelve todas las entidades almacenadas
        IList<T> GetAll();

        // Guarda una entidad (agrega si es nueva, actualiza si ya existe)
        void Save(T entity);

        // Elimina una entidad de la coleccion
        void Delete(T entity);
    }
}
