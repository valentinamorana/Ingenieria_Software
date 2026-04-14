using BE.Composite;

namespace DAL
{
    // DAL concreto para la entidad Patente (nodo hoja del arbol de permisos).
    // Tomado del proyecto de referencia, adaptado al namespace de WardrobeFlow.
    public class PatenteDAL : AbstractDAL<Patente>
    {
        // Sin logica adicional: AbstractDAL provee GetAll, GetById, Save, Delete
    }
}
