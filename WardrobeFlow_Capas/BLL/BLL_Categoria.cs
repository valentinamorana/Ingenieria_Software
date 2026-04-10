using System;
using System.Collections.Generic;
using BE;
using DAL;

namespace BLL
{
    public class BLL_Categoria
    {
        private DAL_Categoria _dal = new DAL_Categoria();

        public List<Categoria> ListarCategorias()
        {
            try { return _dal.ListarCategorias(); }
            catch (Exception ex) { throw new Exception("Error al listar categorias: " + ex.Message); }
        }

        public string AgregarCategoria(Categoria c)
        {
            try { return _dal.AgregarCategoria(c); }
            catch (Exception ex) { throw new Exception("Error al agregar categoria: " + ex.Message); }
        }

        public string EditarCategoria(Categoria c)
        {
            try { return _dal.EditarCategoria(c); }
            catch (Exception ex) { throw new Exception("Error al editar categoria: " + ex.Message); }
        }

        public string EliminarCategoria(int id)
        {
            try { return _dal.EliminarCategoria(id); }
            catch (Exception ex) { throw new Exception("Error al eliminar categoria: " + ex.Message); }
        }
    }
}
