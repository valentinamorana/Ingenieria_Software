using System.Collections.Generic;
using DAL.Interfaces;

namespace Tests.Fakes
{
    /// <summary>
    /// Doble de prueba de IPermisoDAL (sin base de datos). Devuelve un árbol Composite
    /// controlado en memoria para testear la resolución recursiva de permisos:
    ///
    ///   Rol "Gerente"
    ///     └─ Rol "Admin"
    ///          ├─ Familia "Ventas"
    ///          │     ├─ Patente "Clientes" (Id 1)
    ///          │     └─ Patente "Stock"    (Id 2)
    ///          └─ Patente "Clientes" (Id 1)   ← mismo permiso por un segundo camino
    /// </summary>
    public class FakePermisoDAL : IPermisoDAL
    {
        public List<BE.Componente> ObtenerArbol()
        {
            var p1 = new BE.Patente { Id = 1, Nombre = "Clientes", NombreMenu = "mnuClientes" };
            var p2 = new BE.Patente { Id = 2, Nombre = "Stock",    NombreMenu = "mnuStock" };

            var ventas = new BE.Familia { Id = 10, Nombre = "Ventas" };
            ventas.AgregarHijo(p1);
            ventas.AgregarHijo(p2);

            var admin = new BE.Rol { Id = 100, Nombre = "Admin" };
            admin.AgregarHijo(ventas);
            admin.AgregarHijo(p1);     // segundo camino al mismo permiso → debe deduplicarse

            var gerente = new BE.Rol { Id = 200, Nombre = "Gerente" };
            gerente.AgregarHijo(admin); // rol dentro de rol

            return new List<BE.Componente> { gerente };
        }

        public List<string>     ObtenerRoles()                  => new List<string> { "Admin", "Gerente" };
        public List<BE.Permiso> ObtenerPorRol(string rol)       => new List<BE.Permiso>();   // fallback vacío
        public int              ObtenerIdRol(string rolNombre)  => 0;
        public List<int>        ObtenerIdsHijos(int idPadre)    => new List<int>();
        public void             AgregarRelacion(int p, int h)   { }
        public void             QuitarRelacion(int p, int h)    { }
        public int              AltaComponente(string n, string m, bool f, bool r, string t) => 0;
        public void             ModificarComponente(int id, string n, string m) { }
        public void             BajaComponente(int id)          { }
        public int              ContarUsuariosPorRol(string r)  => 0;
        public List<string>     ObtenerUsuariosPorRol(string r) => new List<string>();
    }
}
