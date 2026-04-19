using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    /// <summary>
    /// Capa de Acceso a Datos — Punto de Acceso a la Base de Datos (Patrón Singleton).
    ///
    /// PATRÓN SINGLETON: Garantiza que exista una única instancia de la conexión
    /// SQL Server en toda la aplicación. Esto evita abrir múltiples conexiones
    /// simultáneas y centraliza la gestión del acceso a datos.
    ///
    /// Implementación thread-safe con "double-checked locking":
    ///   - Primera verificación fuera del lock: evita overhead cuando ya existe instancia.
    ///   - Segunda verificación dentro del lock: previene condiciones de carrera en
    ///     entornos multi-hilo.
    ///
    /// La conexión se abre automáticamente al ejecutar una consulta y se puede
    /// cerrar explícitamente con CerrarConexion() (e.g., al hacer Logout).
    /// </summary>
    public class Acceso
    {
        // Instancia única (Singleton) — compartida por toda la capa DAL
        private static Acceso _instance;

        // Objeto de sincronización para garantizar thread-safety en la creación del Singleton
        private static readonly object _lock = new object();

        // Cadena de conexión a SQL Server Express con autenticación de Windows
        // Servidor: .\SQLEXPRESS  |  Base de datos: WardrobeFlow
        // TrustServerCertificate=True requerido en versiones recientes de SQL Server Express
        private readonly string cadenaConexion = "Data Source=.\\SQLEXPRESS;Initial Catalog=WardrobeFlowDB;Integrated Security=True;TrustServerCertificate=True";

        // Objeto de conexión SQL subyacente
        private SqlConnection _conexion;

        /// <summary>
        /// Propiedad privada que garantiza que la conexión esté siempre abierta antes de usarla.
        /// Se abre automáticamente si está cerrada o no inicializada.
        /// </summary>
        private SqlConnection Conexion
        {
            get
            {
                if (_conexion == null)
                {
                    _conexion = new SqlConnection(cadenaConexion);
                }
                // Si la conexión se cerró (e.g., por timeout), reabrirla automáticamente
                if (_conexion.State != ConnectionState.Open)
                {
                    _conexion.Open();
                }
                return _conexion;
            }
        }

        /// <summary>
        /// Constructor privado — impide la instanciación directa desde fuera de la clase.
        /// La única forma de obtener una instancia es a través de GetInstance().
        /// </summary>
        private Acceso()
        {
            _conexion = new SqlConnection(cadenaConexion);
        }

        /// <summary>
        /// Punto de acceso global al Singleton de Acceso.
        /// Implementa "double-checked locking" para thread-safety con mínimo overhead.
        /// </summary>
        /// <returns>La única instancia de Acceso en toda la aplicación.</returns>
        public static Acceso GetInstance()
        {
            // Primera verificación: si ya existe instancia, retornarla sin lock (rápido)
            if (_instance == null)
            {
                // Bloqueo para creación segura en entorno multi-hilo
                lock (_lock)
                {
                    // Segunda verificación: confirmar que ningún otro hilo creó la instancia
                    // mientras esperábamos el lock
                    if (_instance == null)
                    {
                        _instance = new Acceso();
                    }
                }
            }
            return _instance;
        }

        /// <summary>
        /// Ejecuta una consulta SELECT y retorna los resultados en un DataTable.
        /// Admite parámetros SQL para prevenir inyección SQL.
        /// </summary>
        /// <param name="consulta">Consulta SQL SELECT con parámetros opcionales (e.g., "@Username").</param>
        /// <param name="parametros">Array de SqlParameter con valores para los placeholders. Puede ser null.</param>
        /// <returns>DataTable con las filas resultantes de la consulta.</returns>
        public DataTable Leer(string consulta, SqlParameter[] parametros)
        {
            using (SqlCommand cmd = new SqlCommand(consulta, Conexion))
            {
                // Agregar parámetros sólo si se proporcionaron (previene NullReferenceException)
                if (parametros != null)
                {
                    cmd.Parameters.AddRange(parametros);
                }

                // SqlDataAdapter llena el DataTable sin mantener un SqlDataReader abierto
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    DataTable tabla = new DataTable();
                    adapter.Fill(tabla);
                    return tabla;
                }
            }
        }

        /// <summary>
        /// Ejecuta una consulta de escritura (INSERT, UPDATE, DELETE) y retorna
        /// el número de filas afectadas.
        /// </summary>
        /// <param name="consulta">Sentencia SQL de modificación con parámetros opcionales.</param>
        /// <param name="parametros">Array de SqlParameter con valores para los placeholders. Puede ser null.</param>
        /// <returns>Número de filas afectadas por la operación.</returns>
        public int Escribir(string consulta, SqlParameter[] parametros)
        {
            using (SqlCommand cmd = new SqlCommand(consulta, Conexion))
            {
                if (parametros != null)
                {
                    cmd.Parameters.AddRange(parametros);
                }

                // ExecuteNonQuery es el método correcto para INSERT/UPDATE/DELETE
                int afectadas = cmd.ExecuteNonQuery();
                return afectadas;
            }
        }

        /// <summary>
        /// Cierra la conexión a la base de datos.
        /// Se llama al hacer Logout para liberar recursos de la conexión.
        /// </summary>
        public void CerrarConexion()
        {
            try
            {
                if (_conexion != null && _conexion.State != ConnectionState.Closed)
                {
                    _conexion.Close();
                }
            }
            catch
            {
                // Ignorar errores al cerrar — la conexión puede ya estar en estado inválido
            }
        }
    }
}
