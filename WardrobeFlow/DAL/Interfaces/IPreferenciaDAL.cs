namespace DAL.Interfaces
{
    /// <summary>Contrato del acceso a datos de Preferencia (permite inyección y dobles de prueba).</summary>
    public interface IPreferenciaDAL
    {
        BE.Preferencia Obtener(int idUsuario);
        void           Guardar(BE.Preferencia pref);
    }
}
