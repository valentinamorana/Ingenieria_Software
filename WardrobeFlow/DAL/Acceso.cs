using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    /// <summary>
    /// Capa de Acceso a Datos — Punto de Acceso a la Base de Datos (Patrón Singleton).
    ///
    /// PATRÓN SINGLETON: Garantiza una única instancia de la clase Acceso en toda la
    /// aplicación, centralizando la gestión del acceso a datos.
    ///
    /// Implementación thread-safe con "double-checked locking":
    ///   - Primera verificación fuera del lock: evita overhead cuando ya existe instancia.
    ///   - Segunda verificación dentro del lock: previene condiciones de carrera.
    ///
    /// ESTRATEGIA DE CONEXIÓN — nueva conexión por operación:
    ///   Cada llamada a Leer() o Escribir() abre una SqlConnection propia dentro de un
    ///   bloque using, garantizando que se libere al terminar. ADO.NET gestiona
    ///   automáticamente un pool de conexiones, por lo que este patrón es eficiente
    ///   y evita conexiones colgadas por timeout o fallos de red.
    ///
    /// La cadena de conexión se lee de App.config (connectionStrings["WardrobeFlowDB"]),
    /// de modo que cambiar el servidor no requiere recompilar.
    ///
    /// La clase es sealed: impide la herencia, que podría romper el invariante Singleton.
    /// </summary>
    public sealed class Acceso
    {
        // ── Singleton ────────────────────────────────────────────────────────
        private static volatile Acceso _instance;
        private static readonly object _lock = new object();

        // Cadena de conexión leída una sola vez desde App.config al construir el Singleton
        private readonly string _cadenaConexion;

        /// <summary>
        /// Constructor privado: lee la connection string de App.config.
        /// La única forma de obtener una instancia es mediante GetInstance().
        /// </summary>
        private Acceso()
        {
            _cadenaConexion = ConfigurationManager
                .ConnectionStrings["WardrobeFlowDB"]
                .ConnectionString;
        }

        /// <summary>
        /// Punto de acceso global al Singleton de Acceso.
        /// Implementa "double-checked locking" para thread-safety con mínimo overhead.
        /// </summary>
        public static Acceso GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new Acceso();
                }
            }
            return _instance;
        }

        // ── Operaciones ──────────────────────────────────────────────────────

        /// <summary>
        /// Ejecuta una consulta SELECT y retorna los resultados en un DataTable.
        /// Abre una conexión nueva, la usa y la devuelve al pool al salir del using.
        /// </summary>
        public DataTable Leer(string consulta, SqlParameter[] parametros)
        {
            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            using (SqlCommand cmd = new SqlCommand(consulta, conexion))
            {
                if (parametros != null)
                    cmd.Parameters.AddRange(parametros);

                conexion.Open();
                DataTable tabla = new DataTable();
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    adapter.Fill(tabla);
                return tabla;
            }
        }

        /// <summary>
        /// Ejecuta una sentencia INSERT, UPDATE o DELETE.
        /// Abre una conexión nueva, la usa y la devuelve al pool al salir del using.
        /// </summary>
        public int Escribir(string consulta, SqlParameter[] parametros)
        {
            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
            using (SqlCommand cmd = new SqlCommand(consulta, conexion))
            {
                if (parametros != null)
                    cmd.Parameters.AddRange(parametros);

                conexion.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Verifica que la base de datos esté disponible intentando abrir una conexión.
        /// Se llama desde BLL.Configuracion al arranque de la aplicación.
        /// </summary>
        public bool VerificarConexion()
        {
            try
            {
                using (var conexion = new SqlConnection(_cadenaConexion))
                {
                    conexion.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Acceso.VerificarConexion] {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Ejecuta un conjunto de operaciones dentro de una transacción SQL atómica.
        /// Si cualquier operación lanza una excepción, se hace Rollback y se re-lanza.
        /// Si todas tienen éxito, se hace Commit.
        ///
        /// La acción recibe la SqlConnection y el SqlTransaction abiertos, y debe
        /// usarlos para construir sus SqlCommands (cmd.Transaction = tx).
        ///
        /// Uso típico: operaciones que involucran múltiples tablas (ej: Alta de Pedido
        /// que también actualiza Prenda y escribe en PedidoPrenda).
        /// </summary>
        public void EjecutarTransaccion(Action<SqlConnection, SqlTransaction> accion)
        {
            using (var conexion = new SqlConnection(_cadenaConexion))
            {
                conexion.Open();
                using (var tx = conexion.BeginTransaction())
                {
                    try
                    {
                        accion(conexion, tx);
                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Conservado por compatibilidad. Con la estrategia nueva-conexión-por-operación
        /// no hay una conexión persistente que cerrar; este método es un no-op intencional.
        /// </summary>
        public void CerrarConexion() { /* no-op: cada operación gestiona su propia conexión */ }
    }
}
