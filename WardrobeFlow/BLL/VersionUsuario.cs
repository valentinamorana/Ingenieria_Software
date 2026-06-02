using Seguridad;
using System.Collections.Generic;

namespace BLL
{
    /// <summary>
    /// Control de Cambios de Usuarios — orquesta el patrón MEMENTO.
    ///
    ///   Originator → BE.Usuario        (crea y restaura su propio estado)
    ///   Memento    → BE.VersionUsuario (cápsula del estado, implementa IMemento)
    ///   Caretaker  → BLL.CuidadorHistorial (guarda/recupera el historial)
    ///
    /// Esta clase es la fachada que usan la GUI y la BLL: delega la captura del
    /// estado en el Originator y su almacenamiento en el Caretaker.
    /// </summary>
    public class VersionUsuario
    {
        private readonly CuidadorHistorial   _caretaker  = new CuidadorHistorial();
        private readonly DAL.VersionUsuario  _dalVersion = new DAL.VersionUsuario();
        private readonly DAL.Usuario         _dalUsuario = new DAL.Usuario();
        private readonly Servicios.Bitacora  _bitacora   = new Servicios.Bitacora();

        /// <summary>
        /// Captura el estado actual del usuario (Originator) en un Memento y lo guarda
        /// en el historial (Caretaker). Debe llamarse ANTES de una operación destructiva.
        /// FAIL-SAFE: si el Caretaker no puede guardar, lanza AppException y aborta.
        /// </summary>
        public void GrabarVersion(int idUsuario, string actor, string detalle)
        {
            BE.Usuario originator = _dalUsuario.ObtenerPorId(idUsuario);
            if (originator == null) return;

            BE.Memento.IMemento memento = originator.CrearMemento(actor, detalle);
            _caretaker.Guardar(idUsuario, memento);
        }

        public List<BE.VersionUsuario> ObtenerPorUsuario(int idUsuario)
        {
            return _dalVersion.ObtenerPorUsuario(idUsuario);
        }

        public List<BE.VersionUsuario> ObtenerTodos()
        {
            return _dalVersion.ObtenerTodos();
        }

        /// <summary>
        /// Restaura un usuario al estado de una versión histórica (deshacer).
        /// Antes de restaurar, guarda un Memento del estado actual (para poder deshacer
        /// la propia restauración — "rollback de un rollback"). Solo Administrador.
        /// </summary>
        public void RestaurarVersion(string modulo, int idVersion)
        {
            if (!SessionManager.IsLoggedIn)
                throw new BE.AppException("err.bll.sesion_expirada",
                    "La sesión expiró. Volvé a iniciar sesión.");

            // Memento elegido (el Caretaker lo recupera como abstracción IMemento).
            var memento = _caretaker.Obtener(idVersion) as BE.VersionUsuario;
            if (memento == null)
                throw new BE.AppException("err.bll.version.no_existe",
                    "La versión seleccionada no existe.");

            string actor = SessionManager.GetInstance().Usuario.Username;

            // Originator en su estado ACTUAL.
            BE.Usuario originator = _dalUsuario.ObtenerPorId(memento.IdUsuario);
            if (originator == null)
                throw new BE.AppException("err.bll.usuario.no_existe",
                    "El usuario de la versión ya no existe.");

            // 1) Guardar el estado actual como Memento (permite deshacer la restauración).
            //    Fail-safe: si falla, aborta antes de modificar nada.
            _caretaker.Guardar(originator.Id,
                originator.CrearMemento(actor,
                    $"Snapshot automático antes de restaurar versión ID {idVersion}."));

            // 2) El Originator restaura su estado desde el Memento elegido.
            originator.RestaurarDesde(memento);

            // 3) Persistir el estado restaurado.
            _dalUsuario.RestaurarVersion(memento);

            _bitacora.Registrar(modulo,
                $"Restauración a versión ID {idVersion} — usuario ID {memento.IdUsuario} ({memento.UsernameSnapshot})",
                BE.Criticidad.Alta);
        }
    }
}
