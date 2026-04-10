namespace BE
{
    // PATRON COMPOSITE - Clase base del arbol de permisos
    public class Componente
    {
        #region Atributos
        private int idComponente;
        private string nombre;
        private string tipoComponente;
        private bool estado;
        #endregion

        #region Propiedades
        public int IdComponente { get { return idComponente; } set { idComponente = value; } }
        public string Nombre { get { return nombre; } set { nombre = value; } }
        public string TipoComponente { get { return tipoComponente; } set { tipoComponente = value; } }
        public bool Estado { get { return estado; } set { estado = value; } }
        #endregion
    }
}
