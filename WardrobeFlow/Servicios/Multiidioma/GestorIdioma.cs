using System;
using System.Collections.Generic;

namespace Servicios.Multiidioma
{
    /// <summary>
    /// Sujeto (Subject) del patrón Observer para la gestión de idiomas.
    ///
    /// Equivalente a ManejadorDeSesion del ejemplo de cátedra — misma estructura
    /// y mismos métodos, adaptados a WardrobeFlow.
    ///
    /// Responsabilidades:
    ///   - Mantener la lista de formularios suscritos (observers).
    ///   - Recordar el idioma actualmente activo.
    ///   - Notificar a todos los formularios abiertos cuando el idioma cambia.
    ///
    /// El cambio de idioma es INMEDIATO y AUTOMÁTICO: no requiere abrir ningún
    /// formulario adicional ni reiniciar la aplicación.
    ///
    /// Uso típico en cada formulario:
    ///   Load  → GestorIdioma.SuscribirObservador(this)
    ///   Close → GestorIdioma.DesuscribirObservador(this)
    ///   UpdateLanguage(idioma) → llamar a Traducir(idioma)
    /// </summary>
    public static class GestorIdioma
    {
        // ── Estado interno ────────────────────────────────────────────────────

        /// <summary>Lista de formularios suscritos al evento de cambio de idioma.</summary>
        private static readonly IList<IIdiomaObserver> _observers = new List<IIdiomaObserver>();

        /// <summary>Idioma actualmente activo. Por defecto: Español.</summary>
        private static Idioma _idiomaActual = Traductor.ObtenerIdiomaDefault();

        // ── API pública ───────────────────────────────────────────────────────

        /// <summary>Devuelve el idioma actualmente seleccionado.</summary>
        public static Idioma IdiomaActual => _idiomaActual;

        /// <summary>
        /// Registra un formulario para recibir notificaciones de cambio de idioma.
        /// Equivalente a SuscribirObservador del ejemplo de cátedra.
        /// Llamar en el evento Load de cada formulario.
        /// </summary>
        public static void SuscribirObservador(IIdiomaObserver observer)
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);
        }

        /// <summary>
        /// Elimina un formulario de la lista de notificaciones.
        /// Equivalente a DesuscribirObservador del ejemplo de cátedra.
        /// Llamar en el evento FormClosing de cada formulario.
        /// </summary>
        public static void DesuscribirObservador(IIdiomaObserver observer)
        {
            _observers.Remove(observer);
        }

        /// <summary>
        /// Cambia el idioma activo y notifica a todos los formularios suscritos.
        /// Equivalente a CambiarIdioma del ejemplo de cátedra.
        /// Los formularios se traducen al instante sin reinicios ni ventanas adicionales.
        /// </summary>
        public static void CambiarIdioma(Idioma idioma)
        {
            _idiomaActual = idioma;
            Notificar(idioma);
        }

        // ── Notificación interna ──────────────────────────────────────────────

        /// <summary>
        /// Recorre la lista de observers y llama a UpdateLanguage en cada uno.
        /// Equivalente al método Notificar del ejemplo de cátedra.
        /// </summary>
        private static void Notificar(Idioma idioma)
        {
            // Iterar sobre copia para evitar problemas si un observer se desuscribe durante la notificación
            var copia = new List<IIdiomaObserver>(_observers);
            foreach (var observer in copia)
            {
                try
                {
                    observer.UpdateLanguage(idioma);
                }
                catch (Exception ex)
                {
                    // Un formulario con error no interrumpe la traducción del resto
                    System.Diagnostics.Debug.WriteLine(
                        $"[GestorIdioma] Error al notificar observer {observer.GetType().Name}: {ex.Message}");
                }
            }
        }
    }
}
