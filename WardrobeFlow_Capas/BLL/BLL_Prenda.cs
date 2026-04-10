using System;
using System.Collections.Generic;
using BE;
using DAL;

namespace BLL
{
    public class BLL_Prenda
    {
        private DAL_Prenda _dal = new DAL_Prenda();

        public List<Prenda> ListarPrendas()
        {
            try { return _dal.ListarPrendas(); }
            catch (Exception ex) { throw new Exception("Error al listar prendas: " + ex.Message); }
        }

        public string AgregarPrenda(Prenda p)
        {
            try { return _dal.AgregarPrenda(p); }
            catch (Exception ex) { throw new Exception("Error al agregar prenda: " + ex.Message); }
        }

        public string EditarPrenda(Prenda p)
        {
            try { return _dal.EditarPrenda(p); }
            catch (Exception ex) { throw new Exception("Error al editar prenda: " + ex.Message); }
        }

        public string EliminarPrenda(int id)
        {
            try { return _dal.EliminarPrenda(id); }
            catch (Exception ex) { throw new Exception("Error al eliminar prenda: " + ex.Message); }
        }

        // Decorator pattern — construye descripcion enriquecida
        public string ObtenerDescripcionCompleta(Prenda p)
        {
            IDescripcionPrenda desc = new PrendaDescripcionBase(p);
            if (\!string.IsNullOrEmpty(p.Temporada))
                desc = new DecoradorTemporada(desc, p.Temporada);
            desc = new DecoradorOcasion(desc, "General");
            return desc.ObtenerDescripcion();
        }
    }
}
