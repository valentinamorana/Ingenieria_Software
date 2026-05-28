using Seguridad;
using System;
using System.Collections.Generic;

namespace BLL
{
    public class VersionUsuario
    {
        private readonly DAL.VersionUsuario  _dalVersion = new DAL.VersionUsuario();
        private readonly DAL.Usuario         _dalUsuario = new DAL.Usuario();
        private readonly Servicios.Bitacora  _bitacora   = new Servicios.Bitacora();

        // Captura el estado actual del usuario y lo persiste como snapshot histórico.
        // Debe llamarse ANTES de cualquier operación que modifique al usuario.
        public void GrabarVersion(int idUsuario, string actor, string detalle)
        {
            var usuario = _dalUsuario.ObtenerPorId(idUsuario);
            if (usuario == null) return;

            _dalVersion.Insertar(new BE.VersionUsuario
            {
                IdUsuario        = idUsuario,
                Fecha            = DateTime.Now,
                Actor            = actor,
                Detalle          = detalle,
                UsernameSnapshot = usuario.Username,
                ClaveSnapshot    = usuario.Contraseña,
                EstadoSnapshot   = !usuario.Bloqueado,
                IntentosSnapshot = usuario.IntentosFallidos
            });
        }

        public List<BE.VersionUsuario> ObtenerPorUsuario(int idUsuario)
        {
            return _dalVersion.ObtenerPorUsuario(idUsuario);
        }

        public List<BE.VersionUsuario> ObtenerTodos()
        {
            return _dalVersion.ObtenerTodos();
        }

        // Restaura al usuario al estado de una versión histórica.
        // Graba un snapshot del estado actual antes de restaurar.
        // Solo puede ejecutarlo un Administrador.
        public void RestaurarVersion(string modulo, int idVersion)
        {
            if (!SessionManager.IsLoggedIn)
                throw new Exception("No hay sesión activa.");

            var version = _dalVersion.ObtenerPorId(idVersion);
            if (version == null)
                throw new Exception("La versión seleccionada no existe.");

            string actor = SessionManager.GetInstance().Usuario.Username;

            GrabarVersion(version.IdUsuario, actor,
                $"Snapshot automático antes de restaurar versión ID {idVersion}.");

            _dalUsuario.RestaurarVersion(version);

            _bitacora.Registrar(modulo,
                $"Restauración a versión ID {idVersion} — usuario ID {version.IdUsuario} ({version.UsernameSnapshot})",
                BE.Criticidad.Alta);
        }
    }
}
