namespace BE
{
    public class Categoria
    {
        #region Atributos
        private int idCategoria;
        private string nombre;
        private string descripcion;
        private bool estado;
        #endregion

        #region Propiedades
        public int IdCategoria { get { return idCategoria; } set { idCategoria = value; } }
        public string Nombre { get { return nombre; } set { nombre = value; } }
        public string Descripcion { get { return descripcion; } set { descripcion = value; } }
        public bool Estado { get { return estado; } set { estado = value; } }
        #endregion
    }
}
