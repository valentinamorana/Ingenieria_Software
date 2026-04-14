using BE;

namespace DAL
{
    // DAL concreto para la entidad Usuario.
    // Hereda todo el CRUD en memoria de AbstractDAL.
    // Tomado del proyecto de referencia, adaptado para Usuario de WardrobeFlow.
    public class UsuarioDAL : AbstractDAL<Usuario>
    {
        // Sin logica adicional: AbstractDAL provee GetAll, GetById, Save, Delete
    }
}
