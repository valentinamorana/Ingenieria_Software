using BE.Composite;
using DAL;

namespace BLL
{
    // BLL para la entidad Patente (hoja del arbol de permisos).
    // Basada en PatenteBLL del proyecto de referencia.
    public class PatenteBLL : AbstractBLL<Patente>
    {
        // Constructor: asigna el DAL concreto e inicializa datos de prueba
        public PatenteBLL()
        {
            _crud = new PatenteDAL();
            SimularDatos();
        }

        // Carga las patentes (permisos hoja) del sistema WardrobeFlow.
        // Cada patente representa una accion concreta que se puede habilitar.
        public void SimularDatos()
        {
            // Patente: gestionar categorias de prendas
            var p = new Patente();
            p.Nombre = "Puede gestionar categorias";
            p.Tipo   = TipoPermiso.GestorCategorias;
            _crud.Save(p);

            // Patente: gestionar el inventario de prendas
            p = new Patente();
            p.Nombre = "Puede gestionar prendas";
            p.Tipo   = TipoPermiso.GestorPrendas;
            _crud.Save(p);

            // Patente: crear y editar outfits
            p = new Patente();
            p.Nombre = "Puede gestionar outfits";
            p.Tipo   = TipoPermiso.GestorOutfits;
            _crud.Save(p);

            // Patente: administrar usuarios del sistema
            p = new Patente();
            p.Nombre = "Puede gestionar usuarios";
            p.Tipo   = TipoPermiso.GestorUsuarios;
            _crud.Save(p);

            // Patente: asignar permisos a usuarios
            p = new Patente();
            p.Nombre = "Puede gestionar permisos";
            p.Tipo   = TipoPermiso.GestorPermisos;
            _crud.Save(p);
        }
    }
}
