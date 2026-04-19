using Seguridad;
using Servicios;
using System;
using System.Windows.Forms;

namespace BLL
{
    /// <summary>
    /// Capa de Lógica de Negocio — Gestión de Usuarios.
    /// Implementa todas las reglas de negocio relacionadas con autenticación,
    /// gestión de sesión y administración de usuarios del sistema.
    ///
    /// RESPONSABILIDADES:
    ///   - Validar credenciales de Login usando hash seguro (PBKDF2)
    ///   - Establecer y destruir la sesión via SessionManager (Singleton)
    ///   - Registrar eventos de login/logout en la Bitácora
    ///   - Crear nuevos usuarios con contraseñas hasheadas
    ///
    /// FLUJO DE LOGIN:
    ///   1. Validar que usuario y contraseña no estén vacíos
    ///   2. Buscar usuario en BD (DAL)
    ///   3. Verificar contraseña con PBKDF2 (Encriptador)
    ///   4. Si es válido: establecer sesión (SessionManager) + log bitácora
    ///   5. Retornar resultado al formulario GUI
    ///
    /// PATRÓN SINGLETON: usa SessionManager.GetInstance() para gestión de sesión,
    /// garantizando un único estado de sesión en toda la aplicación.
    /// </summary>
    public class Usuario
    {
        // Dependencias de capas inferiores: DAL para persistencia, Servicios para auditoría
        private readonly DAL.Usuario usuarioDAL    = new DAL.Usuario();
        private readonly Servicios.Bitacora bitacora = new Servicios.Bitacora();

        /// <summary>
        /// Autentica un usuario contra la base de datos y establece la sesión si es válido.
        /// Implementa el proceso completo de login: validación → consulta → verificación → sesión → auditoría.
        /// </summary>
        /// <param name="formulario">Formulario de Login (usado como referencia de módulo en bitácora).</param>
        /// <param name="username">Nombre de usuario ingresado en el formulario.</param>
        /// <param name="contraseña">Contraseña en texto plano ingresada por el usuario.</param>
        /// <returns>true si las credenciales son correctas y la sesión fue establecida; false si son inválidas.</returns>
        /// <exception cref="Exception">Si alguno de los campos está vacío o vacío de espacios.</exception>
        public bool Login(Form formulario, string username, string contraseña)
        {
            // Validación de entrada: ambos campos son obligatorios
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(contraseña))
            {
                throw new Exception("Usuario y contraseña son obligatorios.");
            }

            // Buscar el usuario en la BD por su nombre de usuario
            BE.Usuario usuario = usuarioDAL.ObtenerPorUsername(username);

            // Si no existe el usuario, retornar false sin dar pistas sobre cuál campo falló
            // (evita ataques de enumeración de usuarios)
            if (usuario == null) return false;

            // Verificar la contraseña usando PBKDF2: extrae el salt del hash almacenado,
            // rehashea la contraseña ingresada y compara byte a byte
            bool esValido = Encriptador.VerificarContraseña(contraseña, usuario.Contraseña);

            if (esValido)
            {
                // Crear la sesión via método estático Login() — patrón del profesor.
                // Login() crea la instancia Singleton internamente (no GetInstance).
                SessionManager.Login(usuario);

                // Registrar el evento de inicio de sesión en la bitácora de auditoría
                bitacora.Registrar(formulario, "Inicio Sesion", BE.Criticidad.None);
            }

            return esValido;
        }

        /// <summary>
        /// Cierra la sesión del usuario activo: registra el evento, cierra la conexión BD
        /// y limpia el estado del SessionManager.
        /// Llamado desde Menu.cs al seleccionar "Cerrar Sesión".
        /// </summary>
        /// <param name="formulario">Formulario del Menú principal (referencia de módulo en bitácora).</param>
        public void Logout(Form formulario)
        {
            // Primero registrar el cierre de sesión ANTES de destruir el SessionManager,
            // ya que Registrar() necesita el usuario activo para el log
            bitacora.Registrar(formulario, "Cierre Sesion", BE.Criticidad.None);

            // Cerrar la conexión a la BD
            usuarioDAL.Logout();

            // Destruir la sesión via método estático Logout() — patrón del profesor.
            // Después de esto, SessionManager.GetInstance lanza excepción (correcto).
            SessionManager.Logout();
        }

        /// <summary>
        /// Crea un nuevo usuario en el sistema con su contraseña debidamente hasheada.
        /// La contraseña se hashea con PBKDF2 antes de enviarse a la capa DAL,
        /// garantizando que NUNCA se almacene en texto plano en la BD.
        /// </summary>
        /// <param name="username">Nombre de usuario único para el nuevo registro.</param>
        /// <param name="contraseña">Contraseña en texto plano que será hasheada antes de guardarse.</param>
        public void Alta(string username, string contraseña)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(contraseña))
                throw new Exception("El nombre de usuario y contraseña son obligatorios.");

            // Hashear la contraseña con PBKDF2 antes de enviarla a la DAL
            string contraseñaHasheada = Encriptador.Hash(contraseña);

            // Persistir el nuevo usuario con la contraseña hasheada
            usuarioDAL.Alta(username, contraseñaHasheada);
        }

        /// <summary>
        /// Retorna el usuario actualmente en sesión, obtenido del SessionManager (Singleton).
        /// Usa IsLoggedIn antes de GetInstance para evitar la excepción que lanza
        /// GetInstance cuando no hay sesión (comportamiento del patrón del profesor).
        /// </summary>
        /// <returns>Entidad BE.Usuario activa, o null si no hay sesión.</returns>
        public BE.Usuario ObtenerUsuarioActivo()
        {
            // Verificar con IsLoggedIn antes de GetInstance para no lanzar excepción
            if (!SessionManager.IsLoggedIn) return null;
            return SessionManager.GetInstance.Usuario;
        }

        /// <summary>
        /// Obtiene la lista completa de usuarios del sistema para mostrar en el formulario
        /// de Gestión de Usuarios. Las contraseñas NO se incluyen en el resultado.
        /// </summary>
        /// <returns>Lista de entidades BE.Usuario (sin contraseñas).</returns>
        public System.Collections.Generic.List<BE.Usuario> ObtenerTodos()
        {
            return usuarioDAL.ObtenerTodos();
        }
    }
}
