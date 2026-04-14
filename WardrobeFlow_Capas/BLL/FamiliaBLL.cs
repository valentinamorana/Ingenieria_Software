using System.Linq;
using BE.Composite;
using DAL;

namespace BLL
{
    // BLL para la entidad Familia (nodo contenedor del arbol de permisos).
    // Basada en FamiliaBLL del proyecto de referencia.
    public class FamiliaBLL : AbstractBLL<Familia>
    {
        // Referencia a la BLL de patentes para armar el arbol
        private readonly PatenteBLL _bllPatentes = new PatenteBLL();

        // Constructor: asigna el DAL e inicializa el arbol de permisos
        public FamiliaBLL()
        {
            _crud = new FamiliaDAL();
            SimularDatos();
        }

        // Arma el arbol de familias de permisos para WardrobeFlow.
        // Estructura: Administradores contiene todas las sub-familias.
        public void SimularDatos()
        {
            // Familia: gestores de categorias
            var fCat = new Familia();
            fCat.Nombre = "Gestores de categorias";
            var pCat = _bllPatentes.GetAll()
                .Where(pp => pp.Tipo == TipoPermiso.GestorCategorias).FirstOrDefault();
            if (pCat != null) fCat.AgregarPermiso(pCat);
            _crud.Save(fCat);

            // Familia: gestores de prendas
            var fPren = new Familia();
            fPren.Nombre = "Gestores de prendas";
            var pPren = _bllPatentes.GetAll()
                .Where(pp => pp.Tipo == TipoPermiso.GestorPrendas).FirstOrDefault();
            if (pPren != null) fPren.AgregarPermiso(pPren);
            _crud.Save(fPren);

            // Familia: gestores de outfits
            var fOut = new Familia();
            fOut.Nombre = "Gestores de outfits";
            var pOut = _bllPatentes.GetAll()
                .Where(pp => pp.Tipo == TipoPermiso.GestorOutfits).FirstOrDefault();
            if (pOut != null) fOut.AgregarPermiso(pOut);
            _crud.Save(fOut);

            // Familia: gestores de usuarios
            var fUsu = new Familia();
            fUsu.Nombre = "Gestores de usuarios";
            var pUsu = _bllPatentes.GetAll()
                .Where(pp => pp.Tipo == TipoPermiso.GestorUsuarios).FirstOrDefault();
            if (pUsu != null) fUsu.AgregarPermiso(pUsu);
            _crud.Save(fUsu);

            // Familia: gestores de permisos
            var fPerm = new Familia();
            fPerm.Nombre = "Gestores de permisos";
            var pPerm = _bllPatentes.GetAll()
                .Where(pp => pp.Tipo == TipoPermiso.GestorPermisos).FirstOrDefault();
            if (pPerm != null) fPerm.AgregarPermiso(pPerm);
            _crud.Save(fPerm);

            // Familia raiz: Administradores (contiene todas las familias anteriores)
            var fAdmin = new Familia();
            fAdmin.Nombre = "Administradores";
            fAdmin.AgregarPermiso(fCat);
            fAdmin.AgregarPermiso(fPren);
            fAdmin.AgregarPermiso(fOut);
            fAdmin.AgregarPermiso(fUsu);
            fAdmin.AgregarPermiso(fPerm);
            _crud.Save(fAdmin);
        }
    }
}
