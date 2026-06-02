using System;
using System.Collections.Generic;
using BE.Memento;

namespace BLL
{
    /// <summary>
    /// Patrón MEMENTO — rol "Caretaker" (Cuidador).
    ///
    /// Administra el historial de Mementos de los usuarios: los guarda y los
    /// recupera, pero NO interpreta su estado interno (trabaja contra la abstracción
    /// <see cref="IMemento"/>, de la que solo lee metadatos). La persistencia se
    /// delega al DAL (tabla HistorialUsuario).
    ///
    /// Es REUTILIZABLE: cualquier IMemento puede almacenarse aquí; el detalle de
    /// cómo se serializa cada tipo concreto vive en el DAL correspondiente.
    /// </summary>
    public class CuidadorHistorial
    {
        private readonly DAL.VersionUsuario _dalVersion = new DAL.VersionUsuario();

        /// <summary>
        /// Guarda un Memento en el historial del usuario.
        /// FAIL-SAFE: si no se puede persistir, lanza AppException para abortar la
        /// operación en curso (no debe ejecutarse un cambio destructivo sin respaldo).
        /// </summary>
        public void Guardar(int idUsuario, IMemento memento)
        {
            try
            {
                // El Caretaker no lee el estado: entrega el Memento como unidad opaca
                // a la capa de persistencia, que sabe materializarlo.
                var snapshot = (BE.VersionUsuario)memento;
                snapshot.IdUsuario = idUsuario;
                _dalVersion.Insertar(snapshot);
            }
            catch (BE.AppException) { throw; }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(
                    $"[CuidadorHistorial.Guardar] No se pudo guardar el memento del usuario {idUsuario}: {ex.Message}");
                throw new BE.AppException("err.bll.snapshot_fallido",
                    "No se pudo guardar el estado previo del usuario (control de cambios); " +
                    "la operación se canceló para no perder el historial.");
            }
        }

        /// <summary>Historial completo de Mementos de un usuario (orden cronológico inverso del DAL).</summary>
        public List<IMemento> ObtenerHistorial(int idUsuario)
        {
            var lista = new List<IMemento>();
            foreach (var v in _dalVersion.ObtenerPorUsuario(idUsuario))
                lista.Add(v);
            return lista;
        }

        /// <summary>Recupera un Memento puntual por su identificador de persistencia.</summary>
        public IMemento Obtener(int idVersion)
        {
            return _dalVersion.ObtenerPorId(idVersion);
        }
    }
}
