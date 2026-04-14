using System.Linq;
using BE;
using DAL;

namespace BLL
{
    // BLL para la entidad Outfit.
    // Hereda el CRUD de AbstractBLL y agrega datos de prueba.
    public class OutfitBLL : AbstractBLL<Outfit>
    {
        // Referencia a PrendaBLL para usar el Decorator al armar outfits
        private readonly PrendaBLL _bllPrendas;

        // Constructor: recibe la BLL de prendas ya inicializada
        public OutfitBLL(PrendaBLL bllPrendas)
        {
            _crud       = new OutfitDAL();
            _bllPrendas = bllPrendas;
            SimularDatos();
        }

        // Carga outfits de ejemplo con sus prendas asignadas
        private void SimularDatos()
        {
            // Obtener prendas de prueba
            var prendas = _bllPrendas.GetAll().ToList();
            if (prendas.Count < 2) return;

            // Outfit 1: look casual
            var outfit = new Outfit();
            outfit.Nombre      = "Look casual diario";
            outfit.Descripcion = "Combinacion informal para el dia a dia";
            outfit.Ocasion     = "Casual";
            outfit.Temporada   = "Todo el ano";

            var det1 = new DetalleOutfit();
            det1.OPrenda = prendas[0]; // Camisa azul
            outfit.Detalles.Add(det1);

            var det2 = new DetalleOutfit();
            det2.OPrenda = prendas[1]; // Jean clasico
            outfit.Detalles.Add(det2);

            _crud.Save(outfit);
        }

        // Devuelve un resumen textual del outfit usando el Decorator en cada prenda.
        // Muestra la descripcion enriquecida de cada prenda componente.
        public string ObtenerResumenOutfit(Outfit outfit)
        {
            if (outfit == null) return string.Empty;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== " + outfit.Nombre + " ===");
            sb.AppendLine("Ocasion: " + outfit.Ocasion);
            sb.AppendLine("Prendas:");

            foreach (var det in outfit.Detalles)
            {
                if (det.OPrenda != null)
                {
                    // Usa el Decorator para obtener descripcion enriquecida
                    string desc = _bllPrendas.ObtenerDescripcionDecorada(det.OPrenda, outfit.Ocasion);
                    sb.AppendLine("  - " + desc);
                }
            }

            return sb.ToString();
        }
    }
}
