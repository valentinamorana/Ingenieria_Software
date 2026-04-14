using BE;

namespace DAL
{
    // DAL concreto para la entidad Categoria.
    // Hereda todo el CRUD en memoria de AbstractDAL.
    public class CategoriaDAL : AbstractDAL<Categoria>
    {
        // Sin logica adicional: AbstractDAL provee GetAll, GetById, Save, Delete
    }
}
