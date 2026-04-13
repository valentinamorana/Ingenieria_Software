using System.Collections.Generic;

namespace BE
{
    // PATRON COMPOSITE - Contenedor (agrupa permisos por modulo)
    // Puede contener hojas (Permiso) u otros GrupoPermiso anidados
    public class GrupoPermiso : Componente
    {
        #region Atributos
        private int idGrupoPermiso;
        private string nombreGrupo;
        private List<Componente> hijos;
        #endregion

        #region Constructor
        public GrupoPermiso()
        {
            hijos = new List<Componente>();
        }
        #endregion

        #region Propiedades
        public int IdGrupoPermiso { get { return idGrupoPermiso; } set { idGrupoPermiso = value; } }
        public string NombreGrupo { get { return nombreGrupo; } set { nombreGrupo = value; } }
        public List<Componente> Hijos { get { return hijos; } set { hijos = value; } }
        #endregion

        #region Metodos Composite
        // Agrega un hijo (hoja o contenedor) al grupo
        public void AgregarHijo(Componente c)
        {
            hijos.Add(c);
        }

        // Elimina un hijo del grupo
        public void EliminarHijo(Componente c)
        {
            hijos.Remove(c);
        }

        // Retorna true si el grupo contiene el permiso con ese NombreMenu
        public bool ContienePermiso(string nombreMenu)
        {
            foreach (Componente hijo in hijos)
            {
                if (hijo is Permiso p && p.NombreMenu == nombreMenu)
                    return true;
            }
            return false;
        }
        #endregion
    }
}
