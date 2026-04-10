using System;
using System.Collections.Generic;

namespace BE
{
    public class Outfit
    {
        #region Atributos
        private int idOutfit;
        private string nombre;
        private string descripcion;
        private string ocasion;
        private string temporada;
        private bool estado;
        private DateTime fechaCreacion;
        private int idUsuario;
        private List<DetalleOutfit> detalles;
        #endregion

        #region Propiedades
        public int IdOutfit { get { return idOutfit; } set { idOutfit = value; } }
        public string Nombre { get { return nombre; } set { nombre = value; } }
        public string Descripcion { get { return descripcion; } set { descripcion = value; } }
        public string Ocasion { get { return ocasion; } set { ocasion = value; } }
        public string Temporada { get { return temporada; } set { temporada = value; } }
        public bool Estado { get { return estado; } set { estado = value; } }
        public DateTime FechaCreacion { get { return fechaCreacion; } set { fechaCreacion = value; } }
        public int IdUsuario { get { return idUsuario; } set { idUsuario = value; } }
        public List<DetalleOutfit> Detalles { get { return detalles; } set { detalles = value; } }
        #endregion
    }
}
