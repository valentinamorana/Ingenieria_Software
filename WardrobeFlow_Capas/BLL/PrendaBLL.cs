using BE;
using BE.Decorator;
using DAL;

namespace BLL
{
    // BLL para la entidad Prenda.
    // Hereda el CRUD de AbstractBLL, agrega datos de prueba
    // y aplica el patron DECORATOR para generar descripciones enriquecidas.
    public class PrendaBLL : AbstractBLL<Prenda>
    {
        // Referencia a la BLL de categorias para asignar categorias a las prendas
        private readonly CategoriaBLL _bllCategorias;

        // Constructor: recibe la BLL de categorias ya inicializada
        public PrendaBLL(CategoriaBLL bllCategorias)
        {
            _crud          = new PrendaDAL();
            _bllCategorias = bllCategorias;
            SimularDatos();
        }

        // Carga prendas de ejemplo con sus categorias asignadas
        private void SimularDatos()
        {
            // Buscar categorias por nombre para asignarlas
            Categoria catCamisas    = null;
            Categoria catPantalones = null;
            Categoria catCalzado    = null;
            foreach (var cat in _bllCategorias.GetAll())
            {
                if (cat.Nombre == "Camisas")     catCamisas    = cat;
                if (cat.Nombre == "Pantalones")  catPantalones = cat;
                if (cat.Nombre == "Calzado")     catCalzado    = cat;
            }

            // Prenda 1
            var p = new Prenda();
            p.Nombre     = "Camisa azul manga larga";
            p.Color      = "Azul";
            p.Talla      = "M";
            p.Temporada  = "Invierno";
            p.OCategoria = catCamisas;
            _crud.Save(p);

            // Prenda 2
            p = new Prenda();
            p.Nombre     = "Jean clasico";
            p.Color      = "Azul oscuro";
            p.Talla      = "32";
            p.Temporada  = "Todo el ano";
            p.OCategoria = catPantalones;
            _crud.Save(p);

            // Prenda 3
            p = new Prenda();
            p.Nombre     = "Zapatillas blancas";
            p.Color      = "Blanco";
            p.Talla      = "42";
            p.Temporada  = "Verano";
            p.OCategoria = catCalzado;
            _crud.Save(p);
        }

        // Aplica el patron DECORATOR para generar una descripcion completa de la prenda.
        // Encadena: descripcion base -> temporada -> ocasion.
        // Uso: OutfitBLL llama a este metodo para mostrar detalles enriquecidos.
        public string ObtenerDescripcionDecorada(Prenda prenda, string ocasion)
        {
            // 1. Componente base: solo nombre y color/talla
            IDescripcionPrenda descripcion = new PrendaDescripcionBase(prenda);

            // 2. Decorador de temporada: agrega "[Temporada: X]"
            descripcion = new DecoradorTemporada(descripcion, prenda.Temporada);

            // 3. Decorador de ocasion: agrega "[Ocasion: X]"
            descripcion = new DecoradorOcasion(descripcion, ocasion);

            // Retorna la descripcion completa encadenada
            return descripcion.ObtenerDescripcion();
        }
    }
}
