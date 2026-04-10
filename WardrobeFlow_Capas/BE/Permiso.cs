namespace BE
{
    // PATRON COMPOSITE - Hoja (permiso individual de menu)
    public class Permiso : Componente
    {
        #region Atributos
        private int idPermiso;
        private string nombreMenu;
        #endregion

        #region Propiedades
        public int IdPermiso { get { return idPermiso; } set { idPermiso = value; } }
        public string NombreMenu { get { return nombreMenu; } set { nombreMenu = value; } }
        #endregion
    }
}
