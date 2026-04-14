using System;
using System.Collections.Generic;
using BE;
using BE.Composite;

namespace Seguridad
{
    // PATRON SINGLETON — Gestor de sesion unico en todo el sistema.
    // Basado en SessionManagerSL del ejemplo de referencia (Nacho/Codigo),
    // adaptado para la entidad Usuario de WardrobeFlow (Guid Id, Composite permisos).
    // Reemplaza al anterior SingletonSesion para tener mayor funcionalidad.
    public sealed class SessionManagerSL
    {
        // Instancia unica del gestor (volatile para garantizar visibilidad entre hilos)
        private static SessionManagerSL _instancia;

        // Objeto de bloqueo para garantizar hilo-seguridad en la creacion
        private static readonly object _lock = new object();

        // Sesion activa actual (null si no hay sesion)
        private SesionBE _sesionActual;

        // Constructor privado: impide instanciacion externa (Singleton)
        private SessionManagerSL()
        {
            _sesionActual = null;
        }

        // Propiedad estatica que devuelve la instancia unica.
        // Usa double-check locking para garantizar hilo-seguridad.
        public static SessionManagerSL Instancia
        {
            get
            {
                if (_instancia == null)
                {
                    lock (_lock)
                    {
                        if (_instancia == null)
                            _instancia = new SessionManagerSL();
                    }
                }
                return _instancia;
            }
        }

        // Inicia la sesion para el usuario indicado.
        // Si ya habia una sesion abierta, la cierra primero.
        public void IniciarSesion(Usuario usuario)
        {
            if (usuario == null)
                throw new ArgumentNullException("usuario", "El usuario no puede ser nulo");

            // Cerrar sesion previa si existia
            if (_sesionActual != null && _sesionActual.SesionActiva)
                _sesionActual.CerrarSesion();

            // Crear nueva sesion y arrancarla
            _sesionActual = new SesionBE();
            _sesionActual.IniciarSesion(usuario);
        }

        // Cierra la sesion activa actual
        public void CerrarSesion()
        {
            if (_sesionActual != null)
            {
                _sesionActual.CerrarSesion();
                _sesionActual = null;
            }
        }

        // Indica si hay una sesion activa en este momento
        public bool TieneSesionActiva()
        {
            return _sesionActual != null && _sesionActual.SesionActiva;
        }

        // Devuelve el usuario actualmente logueado, o null si no hay sesion
        public Usuario ObtenerUsuarioActual()
        {
            return _sesionActual != null ? _sesionActual.UsuarioActual : null;
        }

        // Devuelve el objeto SesionBE completo (para acceder a fechas, maquina, etc.)
        public SesionBE ObtenerSesionActual()
        {
            return _sesionActual;
        }

        // Calcula cuanto tiempo lleva activa la sesion actual
        public TimeSpan ObtenerDuracionSesion()
        {
            if (!TieneSesionActiva())
                return TimeSpan.Zero;

            return _sesionActual.ObtenerDuracionSesion();
        }

        // Verifica si el usuario actual tiene un permiso especifico (usa el Composite).
        // Recorre recursivamente el arbol de permisos del usuario.
        public bool IsInRole(TipoPermiso tipoPermiso)
        {
            if (!TieneSesionActiva()) return false;

            var usuario = ObtenerUsuarioActual();
            if (usuario == null) return false;

            bool valid = false;
            foreach (var p in usuario.Permisos)
            {
                if (p is Patente && ((Patente)p).Tipo.Equals(tipoPermiso))
                {
                    valid = true;
                }
                else
                {
                    valid = IsInRoleRecursivo(p, tipoPermiso, valid);
                }
            }
            return valid;
        }

        // Recorre recursivamente el arbol Composite buscando el permiso
        private bool IsInRoleRecursivo(PermisoCompuesto p, TipoPermiso tipoPermiso, bool valid)
        {
            foreach (var item in p.ObtenerHijos())
            {
                if (item is Patente && ((Patente)item).Tipo.Equals(tipoPermiso))
                    valid = true;
                else
                    valid = IsInRoleRecursivo(item, tipoPermiso, valid);
            }
            return valid;
        }

        // Devuelve un diccionario con informacion de la sesion activa para mostrar
        public Dictionary<string, string> ObtenerInformacion()
        {
            var info = new Dictionary<string, string>();
            if (TieneSesionActiva())
            {
                var u = ObtenerUsuarioActual();
                var s = ObtenerSesionActual();
                info["Estado"]      = "Activa";
                info["Usuario"]     = u != null ? u.NombreCompleto : "N/A";
                info["Documento"]   = u != null ? u.Documento : "N/A";
                info["Rol"]         = u != null ? u.Rol : "N/A";
                info["Inicio"]      = s.FechaInicioSesion.ToString("dd/MM/yyyy HH:mm:ss");
                info["Duracion"]    = s.ObtenerDuracionSesion().ToString(@"hh\:mm\:ss");
                info["Maquina"]     = s.NombreMaquina;
            }
            else
            {
                info["Estado"] = "Inactiva";
            }
            return info;
        }
    }
}
