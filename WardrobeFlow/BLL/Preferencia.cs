namespace BLL
{
    /// <summary>
    /// Lógica de negocio — Preferencias de UI por usuario. Fachada simple sobre el DAL.
    /// </summary>
    public class Preferencia
    {
        private readonly DAL.Preferencia _dal = new DAL.Preferencia();

        public BE.Preferencia Obtener(int idUsuario)
        {
            return _dal.Obtener(idUsuario) ?? new BE.Preferencia { IdUsuario = idUsuario };
        }

        public void Guardar(BE.Preferencia pref)
        {
            if (pref == null)
                throw new BE.AppException("err.bll.pref.nula", "No hay preferencias para guardar.");
            _dal.Guardar(pref);
        }
    }
}
