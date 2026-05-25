using Servicios.Multiidioma;
using System;
using System.Collections.Generic;

namespace BLL
{
    /// <summary>
    /// Lógica de negocio para la gestión de idiomas y traducciones.
    ///
    /// Responsabilidades:
    ///   - Cargar todas las traducciones de un idioma desde BD (un solo SELECT).
    ///   - Auto-seedear la BD desde los diccionarios hardcodeados en el primer uso.
    ///   - Exponer CRUD de traducciones para el FormIdiomas.
    ///
    /// Patrón Observer (T05): Este BLL NO sabe nada del Observer.
    /// La GUI llama a CargarTraducciones(), luego pasa el resultado a
    /// GestorIdioma.CambiarIdioma(idioma, traducciones) que notifica los observers.
    /// </summary>
    public class IdiomaService
    {
        private readonly DAL.Idioma     dalIdioma     = new DAL.Idioma();
        private readonly DAL.Traduccion dalTraduccion = new DAL.Traduccion();

        // Se vuelve true la primera vez que se seedea en esta sesión.
        // Evita re-seedear en cada cambio de idioma; InsertarSiNoExiste lo hace idempotente.
        private static bool _seededThisSession = false;

        // ── Idiomas ──────────────────────────────────────────────────────────

        public List<BE.Idioma> ObtenerIdiomasActivos()
        {
            return dalIdioma.ObtenerActivos();
        }

        public List<BE.Idioma> ObtenerTodosLosIdiomas()
        {
            return dalIdioma.ObtenerTodos();
        }

        public void ActivarIdioma(int idIdioma)
        {
            dalIdioma.Activar(idIdioma);
        }

        public void DesactivarIdioma(int idIdioma)
        {
            dalIdioma.Desactivar(idIdioma);
        }

        // ── Traducciones ─────────────────────────────────────────────────────

        // Devuelve todas las traducciones del idioma indicado como Dictionary<clave, texto>.
        // Si la BD no tiene datos aún, primero la seedea desde los dicts hardcodeados.
        // La GUI debe llamar a este método ANTES de llamar a GestorIdioma.CambiarIdioma().
        public Dictionary<string, string> CargarTraducciones(string codigoIdioma)
        {
            try
            {
                if (!_seededThisSession)
                {
                    SeedearDesdeHardcode();
                    _seededThisSession = true;
                }

                return dalTraduccion.ObtenerDiccionario(codigoIdioma);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BLL.Idioma.CargarTraducciones] {ex.Message}");
                return new Dictionary<string, string>();
            }
        }

        // Devuelve filas completas para el DataGridView del FormIdiomas.
        public List<BE.FilaTraduccion> ObtenerTraduccionesPorIdioma(int idIdioma)
        {
            return dalTraduccion.ObtenerPorIdioma(idIdioma);
        }

        // Persiste el texto editado de una traducción.
        public void GuardarTraduccion(int idControl, int idIdioma, string texto)
        {
            if (string.IsNullOrEmpty(texto)) return;
            dalTraduccion.GuardarTraduccion(idControl, idIdioma, texto);
        }

        // ── Seeding ──────────────────────────────────────────────────────────

        // Carga los diccionarios hardcodeados del Traductor y los persiste en BD.
        // Solo se ejecuta una vez (cuando la tabla Traduccion está vacía).
        public void SeedearDesdeHardcode()
        {
            try
            {
                var idiomas = Traductor.ObtenerIdiomas();

                foreach (var idioma in idiomas)
                {
                    int idIdioma = dalIdioma.ObtenerOCrearPorCodigo(idioma.Id, idioma.Nombre);

                    // ObtenerTraduccionesHardcode → lee dicts estáticos (independiente del cache)
                    var traducciones = Traductor.ObtenerTraduccionesHardcode(idioma);

                    foreach (var kv in traducciones)
                    {
                        string formulario = Traductor.InferirFormulario(kv.Key);
                        int idControl = dalTraduccion.ObtenerOCrearControl(kv.Key, formulario);
                        dalTraduccion.InsertarSiNoExiste(idControl, idIdioma, kv.Value.Texto);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BLL.Idioma.SeedearDesdeHardcode] {ex.Message}");
                throw;
            }
        }

    }
}
