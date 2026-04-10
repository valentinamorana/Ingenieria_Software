using System.Configuration;
using System.Data.SqlClient;

namespace DAL
{
    public class DAL_Conexion
    {
        private static DAL_Conexion _instancia;
        private static readonly object _bloqueo = new object();
        private SqlConnection _conexion;

        private DAL_Conexion() { }

        public static DAL_Conexion Instancia
        {
            get
            {
                if (_instancia == null)
                {
                    lock (_bloqueo)
                    {
                        if (_instancia == null)
                            _instancia = new DAL_Conexion();
                    }
                }
                return _instancia;
            }
        }

        public SqlConnection ObtenerConexion()
        {
            string cadena = ConfigurationManager.ConnectionStrings["cadenaConexion"].ConnectionString;
            _conexion = new SqlConnection(cadena);
            _conexion.Open();
            return _conexion;
        }

        public void CerrarConexion()
        {
            if (_conexion \!= null && _conexion.State == System.Data.ConnectionState.Open)
            {
                _conexion.Close();
                _conexion.Dispose();
            }
        }
    }
}
