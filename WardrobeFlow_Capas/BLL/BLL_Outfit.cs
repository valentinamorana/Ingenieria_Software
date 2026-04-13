using System;
using System.Collections.Generic;
using BE;
using DAL;

namespace BLL
{
    public class BLL_Outfit
    {
        private DAL_Outfit _dal = new DAL_Outfit();

        // ── CRUD basico ────────────────────────────────────────────────────

        public List<Outfit> ListarOutfits()
        {
            try { return _dal.ListarOutfits(); }
            catch (Exception ex) { throw new Exception("Error al listar outfits: " + ex.Message); }
        }

        public List<DetalleOutfit> ListarDetallesPorOutfit(int idOutfit)
        {
            try { return _dal.ListarDetallesPorOutfit(idOutfit); }
            catch (Exception ex) { throw new Exception("Error al listar detalles del outfit: " + ex.Message); }
        }

        public string AgregarOutfit(Outfit o)
        {
            try
            {
                ValidarOutfit(o);
                return _dal.AgregarOutfit(o);
            }
            catch (Exception ex) { throw new Exception("Error al agregar outfit: " + ex.Message); }
        }

        public string EditarOutfit(Outfit o)
        {
            try
            {
                ValidarOutfit(o);
                return _dal.EditarOutfit(o);
            }
            catch (Exception ex) { throw new Exception("Error al editar outfit: " + ex.Message); }
        }

        public string EliminarOutfit(int idOutfit)
        {
            try { return _dal.EliminarOutfit(idOutfit); }
            catch (Exception ex) { throw new Exception("Error al eliminar outfit: " + ex.Message); }
        }

        // ── Validacion de negocio ──────────────────────────────────────────
        private void ValidarOutfit(Outfit o)
        {
            if (string.IsNullOrWhiteSpace(o.Nombre))
                throw new Exception("El nombre del outfit es obligatorio.");
            if (string.IsNullOrWhiteSpace(o.Ocasion))
                throw new Exception("Debe seleccionar una ocasion.");
            if (string.IsNullOrWhiteSpace(o.Temporada))
                throw new Exception("Debe seleccionar una temporada.");
            if (o.Detalles == null || o.Detalles.Count == 0)
                throw new Exception("El outfit debe contener al menos una prenda.");
        }

        // ── PATRON DECORATOR — descripcion enriquecida del outfit ──────────
        // Combina DecoradorTemporada y DecoradorOcasion sobre cada prenda
        // para construir un resumen narrativo del outfit completo.
        public string ObtenerResumenOutfit(Outfit o)
        {
            if (o.Detalles == null || o.Detalles.Count == 0)
                return o.Nombre + " (sin prendas)";

            string resumen = "=== " + o.Nombre + " ===\n";
            resumen += "Ocasion: " + o.Ocasion + "  |  Temporada: " + o.Temporada + "\n";
            resumen += "Prendas:\n";

            foreach (DetalleOutfit det in o.Detalles)
            {
                if (det.OPrenda == null) continue;

                // Decorar la prenda con temporada y ocasion del outfit
                IDescripcionPrenda desc = new PrendaDescripcionBase(det.OPrenda);
                if (!string.IsNullOrEmpty(o.Temporada))
                    desc = new DecoradorTemporada(desc, o.Temporada);
                if (!string.IsNullOrEmpty(o.Ocasion))
                    desc = new DecoradorOcasion(desc, o.Ocasion);

                resumen += "  - " + desc.ObtenerDescripcion()
                         + "  (" + desc.ObtenerEtiqueta() + ")\n";
            }
            return resumen;
        }
    }
}
