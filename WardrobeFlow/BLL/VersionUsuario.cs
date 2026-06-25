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

            // 1) Guardar el estado actual como Memento ANTES de restaurar. El detalle describe
            //    el CAMBIO que produce el rollback ("campo: 'antes' → 'después'"), igual que una
            //    edición normal: un rollback es en sí mismo un cambio y debe quedar trazado como tal.
            //    Fail-safe: si falla, aborta antes de modificar nada.
            string diff    = DescribirRestauracion(originator, memento);
            string detalle = diff == null
                ? $"Restauración a versión ID {idVersion}: sin cambios de datos."
                : $"Restauración a versión ID {idVersion} — {diff}";
            _caretaker.Guardar(originator.Id, originator.CrearMemento(actor, detalle));

            // 2) El Originator restaura su estado desde el Memento elegido.
            originator.RestaurarDesde(memento);

            // 3) Persistir el estado restaurado.
            _dalUsuario.RestaurarVersion(memento);

            _bitacora.Registrar(modulo,
                $"Restauración a versión ID {idVersion} — usuario ID {memento.IdUsuario} ({memento.UsernameSnapshot})",
                BE.Criticidad.Alta);
        }

        // Describe el cambio que produce un rollback comparando el estado ACTUAL (antes) con el
        // estado DESTINO de la versión a restaurar (después). Mismo formato "campo: 'a' → 'b'; ..."
        // que usa una edición normal, así el historial muestra QUÉ cambió el rollback (no solo que
        // ocurrió). Solo datos administrativos no sensibles. Devuelve null si no cambia nada.
        private static string DescribirRestauracion(BE.Usuario actual, BE.VersionUsuario destino)
        {
            var partes = new System.Collections.Generic.List<string>();
            void Cmp(string campo, string viejo, string nuevo)
            {
                if (!string.Equals(viejo ?? "", nuevo ?? "", System.StringComparison.Ordinal))
                    partes.Add($"{campo}: '{viejo ?? ""}' → '{nuevo ?? ""}'");
            }
            Cmp("Usuario",  actual.Username, destino.UsernameSnapshot);
            Cmp("Nombre",   actual.Nombre,   destino.NombreSnapshot);
            Cmp("Apellido", actual.Apellido, destino.ApellidoSnapshot);
            Cmp("Email",    actual.Email,    destino.EmailSnapshot);

            string fechaVieja = actual.FechaNacimiento?.ToString("yyyy-MM-dd") ?? "";
            string fechaNueva = destino.FechaNacSnapshot?.ToString("yyyy-MM-dd") ?? "";
            if (!string.Equals(fechaVieja, fechaNueva, System.StringComparison.Ordinal))
                partes.Add($"Fecha nac.: '{fechaVieja}' → '{fechaNueva}'");

            return partes.Count == 0 ? null : string.Join("; ", partes);
        }
    }
}
