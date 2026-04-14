using BE.Composite;

namespace DAL
{
    // DAL concreto para la entidad Familia (nodo contenedor del arbol de permisos).
    // Tomado del proyecto de referencia, adaptado al namespace de WardrobeFlow.
    public class FamiliaDAL : AbstractDAL<Familia>
    {
        // Sin logica adicional: AbstractDAL provee GetAll, GetById, Save, Delete
    }
}
