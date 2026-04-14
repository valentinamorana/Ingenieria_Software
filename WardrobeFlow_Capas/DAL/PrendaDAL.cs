using BE;

namespace DAL
{
    // DAL concreto para la entidad Prenda.
    // Hereda todo el CRUD en memoria de AbstractDAL.
    public class PrendaDAL : AbstractDAL<Prenda>
    {
        // Sin logica adicional: AbstractDAL provee GetAll, GetById, Save, Delete
    }
}
