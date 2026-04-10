namespace BE
{
    // PATRON COMPOSITE - Contenedor (agrupa permisos por modulo)
    public class GrupoPermiso : Componente
    {
        #region Atributos
        private int idGrupoPermiso;
        private string nombreGrupo;
        #endregion

        #region Propiedades
        public int IdGrupoPermiso { get { return idGrupoPermiso; } set { idGrupoPermiso = value; } }
        public string NombreGrupo { get { return nombreGrupo; } set { nombreGrupo = value; } }
        #endregion
    }
}
