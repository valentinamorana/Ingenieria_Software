using System;
using System.Linq;
using BE;
using BE.Composite;
using DAL;
using Seguridad;

namespace BLL
{
    // BLL para la entidad Usuario.
    // Maneja login, logout y datos de prueba.
    // Ahora utiliza SessionManagerSL (Singleton de sesion) y BitacoraSL (auditoria).
    public class UsuarioBLL : AbstractBLL<Usuario>
    {
        // Referencia a la BLL de familias para asignar permisos a usuarios de prueba
        private readonly FamiliaBLL _bllFamilias = new FamiliaBLL();

        // Constructor: asigna el DAL e inicializa usuarios de prueba
        public UsuarioBLL()
        {
            _crud = new UsuarioDAL();
            SimularDatos();
        }

        // Crea usuarios de prueba con sus permisos asignados.
        // Sigue el mismo patron que el proyecto de referencia.
        private void SimularDatos()
        {
            // Usuario administrador: tiene todos los permisos
            var admin = new Usuario();
            admin.NombreCompleto = "Administrador";
            admin.Documento      = "admin";
            admin.Correo         = "admin@wardrobeflow.com";
            admin.Password       = Encriptador.Hash("1234");
            admin.Rol            = "Administrador";
            var fAdmin = _bllFamilias.GetAll()
                .Where(f => f.Nombre.Contains("Administradores")).FirstOrDefault();
            if (fAdmin != null) admin.Permisos.Add(fAdmin);
            _crud.Save(admin);

            // Usuario empleado: solo puede gestionar prendas y categorias
            var empleado = new Usuario();
            empleado.NombreCompleto = "Empleado Demo";
            empleado.Documento      = "empleado";
            empleado.Correo         = "empleado@wardrobeflow.com";
            empleado.Password       = Encriptador.Hash("1234");
            empleado.Rol            = "Empleado";
            var fPren = _bllFamilias.GetAll()
                .Where(f => f.Nombre.Contains("Gestores de prendas")).FirstOrDefault();
            if (fPren != null) empleado.Permisos.Add(fPren);
            var fCat = _bllFamilias.GetAll()
                .Where(f => f.Nombre.Contains("Gestores de categorias")).FirstOrDefault();
            if (fCat != null) empleado.Permisos.Add(fCat);
            _crud.Save(empleado);
        }

        // Realiza el login validando documento y clave hasheada.
        // Usa SessionManagerSL para gestionar la sesion activa.
        // Registra el evento en BitacoraSL (auditoria).
        public LoginResult Login(string documento, string password)
        {
            // Verificar que no haya ya una sesion activa (mediante el Singleton de sesion)
            if (SessionManagerSL.Instancia.TieneSesionActiva())
                throw new Exception("Ya hay una sesion iniciada");

            // Buscar usuario por documento
            var user = _crud.GetAll()
                .Where(u => u.Documento.Equals(documento)).FirstOrDefault();

            // Documento no existe: registrar intento fallido en bitacora y lanzar excepcion
            if (user == null)
            {
                BitacoraSL.Instancia.RegistrarLoginFallido(documento);
                throw new LoginException(LoginResult.InvalidUsername);
            }

            // Verificar clave hasheada; si no coincide: registrar intento fallido
            if (!Encriptador.Hash(password).Equals(user.Password))
            {
                BitacoraSL.Instancia.RegistrarLoginFallido(documento);
                throw new LoginException(LoginResult.InvalidPassword);
            }

            // Login exitoso: iniciar sesion en el SessionManager y registrar en bitacora
            SessionManagerSL.Instancia.IniciarSesion(user);
            BitacoraSL.Instancia.RegistrarLogin(documento);
            return LoginResult.ValidUser;
        }

        // Cierra la sesion actual del usuario y registra el evento en la bitacora
        public void Logout()
        {
            if (!SessionManagerSL.Instancia.TieneSesionActiva())
                throw new Exception("No hay sesion iniciada");

            // Obtener el nombre antes de cerrar la sesion para el registro
            string nombre = Sesion.ObtenerNombreUsuario();
            SessionManagerSL.Instancia.CerrarSesion();
            BitacoraSL.Instancia.RegistrarLogout(nombre);
        }
    }
}
