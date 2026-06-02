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
        // T06 — FAIL-SAFE: si no se puede grabar el snapshot, ABORTA la operación lanzando
        // AppException, en lugar de seguir sin respaldo (lo que rompería la garantía de rollback).
        public void GrabarVersion(int idUsuario, string actor, string detalle)
        {
            try
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
            catch (BE.AppException) { throw; }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(
                    $"[BLL.VersionUsuario.GrabarVersion] No se pudo grabar snapshot para usuario ID {idUsuario}: {ex.Message}");
                throw new BE.AppException("err.bll.snapshot_fallido",
                    "No se pudo guardar el estado previo del usuario (control de cambios); " +
                    "la operación se canceló para no perder el historial.");
            }
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
                throw new BE.AppException("err.bll.sesion_expirada",
                    "La sesión expiró. Volvé a iniciar sesión.");

            var version = _dalVersion.ObtenerPorId(idVersion);
            if (version == null)
                throw new BE.AppException("err.bll.version.no_existe",
                    "La versión seleccionada no existe.");

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
