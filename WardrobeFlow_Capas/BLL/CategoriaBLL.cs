using BE;
using DAL;

namespace BLL
{
    // BLL para la entidad Categoria de prendas.
    // Hereda el CRUD de AbstractBLL y agrega datos de prueba.
    public class CategoriaBLL : AbstractBLL<Categoria>
    {
        // Constructor: asigna el DAL e inicializa categorias de prueba
        public CategoriaBLL()
        {
            _crud = new CategoriaDAL();
            SimularDatos();
        }

        // Carga categorias de ejemplo para que el sistema tenga datos al iniciar
        private void SimularDatos()
        {
            var c = new Categoria();
            c.Nombre      = "Camisas";
            c.Descripcion = "Camisas formales e informales";
            _crud.Save(c);

            c = new Categoria();
            c.Nombre      = "Pantalones";
            c.Descripcion = "Pantalones de todo tipo";
            _crud.Save(c);

            c = new Categoria();
            c.Nombre      = "Calzado";
            c.Descripcion = "Zapatos, zapatillas y sandalias";
            _crud.Save(c);

            c = new Categoria();
            c.Nombre      = "Accesorios";
            c.Descripcion = "Cinturones, relojes, carteras";
            _crud.Save(c);
        }
    }
}
