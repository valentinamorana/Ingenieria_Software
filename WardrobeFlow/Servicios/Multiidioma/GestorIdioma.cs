using System;
using System.Collections.Generic;

namespace Servicios.Multiidioma
{
    /// <summary>
    /// Sujeto (Subject) del patrón Observer para la gestión de idiomas.
    ///
    /// En la versión con BD:
    ///   - BLL.Idioma.CargarTraducciones() carga el diccionario desde SQL.
    ///   - La GUI llama a GestorIdioma.CambiarIdioma(idioma, traducciones)
    ///     que guarda el dict en _tradActuales y notifica a todos los observers.
    ///   - Cada formulario traduce sus controles consultando GestorIdioma.TradActuales
    ///     via Traductor.ObtenerTraducciones() — sin ir a BD por cada control.
    ///
    /// Si _tradActuales está vacío (primer arranque o error de BD),
    /// Traductor cae al diccionario hardcodeado como fallback.
    /// </summary>
    public static class GestorIdioma
    {
        private static readonly IList<IIdiomaObserver> _observers = new List<IIdiomaObserver>();
        // Protege la lista de observadores: suscribir/desuscribir y la copia previa a notificar
        // ocurren bajo lock, para que un alta/baja concurrente no corrompa la lista.
        private static readonly object _lockObs = new object();
        private static Idioma _idiomaActual = Traductor.ObtenerIdiomaDefault();
        private static Dictionary<string, string> _tradActuales = null;
        private static IList<Idioma> _idiomasDisponibles = null;

        public static Idioma IdiomaActual => _idiomaActual;

        // Traducciones cargadas desde BD para el idioma activo.
        // Null o vacío → Traductor usa fallback hardcodeado.
        public static Dictionary<string, string> TradActuales => _tradActuales;

        // Idiomas activos cargados desde BD. Null → Traductor usa lista hardcodeada.
        public static IList<Idioma> IdiomasDisponibles => _idiomasDisponibles;

        public static void SetIdiomasDisponibles(IList<Idioma> idiomas)
        {
            _idiomasDisponibles = idiomas;
        }

        public static void SuscribirObservador(IIdiomaObserver observer)
        {
            lock (_lockObs)
            {
                if (!_observers.Contains(observer))
                    _observers.Add(observer);
            }
        }

        public static void DesuscribirObservador(IIdiomaObserver observer)
        {
            lock (_lockObs)
                _observers.Remove(observer);
        }

        // Sobrecarga con traducciones desde BD (flujo normal en producción).
        public static void CambiarIdioma(Idioma idioma, Dictionary<string, string> traducciones)
        {
            _idiomaActual = idioma;
            _tradActuales = traducciones;
            Notificar(idioma);
        }

        // Sobrecarga sin dict — usa traducciones ya cargadas o fallback hardcodeado.
        public static void CambiarIdioma(Idioma idioma)
        {
            _idiomaActual = idioma;
            Notificar(idioma);
        }

        private static void Notificar(Idioma idioma)
        {
            // Copia defensiva bajo lock: se notifica FUERA del lock para no bloquear si un
            // observer tarda o se desuscribe a sí mismo durante UpdateLanguage.
            List<IIdiomaObserver> copia;
            lock (_lockObs)
                copia = new List<IIdiomaObserver>(_observers);

            foreach (var observer in copia)
            {
                try { observer.UpdateLanguage(idioma); }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError(
                        $"[GestorIdioma] Error notificando {observer.GetType().Name}: {ex.Message}");
                }
            }
        }
    }
}
