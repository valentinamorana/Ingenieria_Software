using BE;

namespace DAL
{
    // DAL concreto para la entidad Outfit.
    // Hereda todo el CRUD en memoria de AbstractDAL.
    public class OutfitDAL : AbstractDAL<Outfit>
    {
        // Sin logica adicional: AbstractDAL provee GetAll, GetById, Save, Delete
    }
}
