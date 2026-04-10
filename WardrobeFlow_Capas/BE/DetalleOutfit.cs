namespace BE
{
    public class DetalleOutfit
    {
        #region Atributos
        private int idDetalle;
        private int idOutfit;
        private int idPrenda;
        private Prenda oPrenda;
        #endregion

        #region Propiedades
        public int IdDetalle { get { return idDetalle; } set { idDetalle = value; } }
        public int IdOutfit { get { return idOutfit; } set { idOutfit = value; } }
        public int IdPrenda { get { return idPrenda; } set { idPrenda = value; } }
        public Prenda OPrenda { get { return oPrenda; } set { oPrenda = value; } }
        #endregion
    }
}
