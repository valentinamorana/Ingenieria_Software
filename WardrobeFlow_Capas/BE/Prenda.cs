using System;

namespace BE
{
    public class Prenda
    {
        #region Atributos
        private int idPrenda;
        private string nombre;
        private string color;
        private string talla;
        private string temporada;
        private int idCategoria;
        private Categoria oCategoria;
        private bool estado;
        private DateTime fechaRegistro;
        #endregion

        #region Propiedades
        public int IdPrenda { get { return idPrenda; } set { idPrenda = value; } }
        public string Nombre { get { return nombre; } set { nombre = value; } }
        public string Color { get { return color; } set { color = value; } }
        public string Talla { get { return talla; } set { talla = value; } }
        public string Temporada { get { return temporada; } set { temporada = value; } }
        public int IdCategoria { get { return idCategoria; } set { idCategoria = value; } }
        public Categoria OCategoria { get { return oCategoria; } set { oCategoria = value; } }
        public bool Estado { get { return estado; } set { estado = value; } }
        public DateTime FechaRegistro { get { return fechaRegistro; } set { fechaRegistro = value; } }
        #endregion
    }
}
