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
        private readonly DAL.Control    dalControl    = new DAL.Control();

        // Se vuelve true la primera vez que se seedea en esta sesión.
        // Evita re-seedear en cada cambio de idioma; InsertarSiNoExiste lo hace idempotente.
        private static bool _seededThisSession = false;

        // ── Idiomas ──────────────────────────────────────────────────────────

        public List<BE.Idioma> ObtenerIdiomasActivos()
        {
            return dalIdioma.ObtenerActivos();
        }

        // Devuelve los idiomas activos ya convertidos al tipo que usa el sistema de idiomas.
        // Centraliza el mapeo BE.Idioma → Servicios.Multiidioma.Idioma para que la GUI no lo haga.
        public List<Idioma> ObtenerIdiomasActivosComoIdioma()
        {
            var resultado = new List<Idioma>();
            foreach (var b in dalIdioma.ObtenerActivos())
                resultado.Add(new Idioma { Id = b.Codigo, Nombre = b.Nombre, EsDefault = b.EsDefault });
            return resultado;
        }

        public List<BE.Idioma> ObtenerTodosLosIdiomas()
        {
            return dalIdioma.ObtenerTodos();
        }

        // Crea un idioma nuevo (queda inactivo hasta que el admin lo active).
        public void CrearIdioma(string codigo, string nombre)
        {
            if (string.IsNullOrWhiteSpace(codigo) || string.IsNullOrWhiteSpace(nombre))
                throw new BE.AppException("err.bll.idioma.campos",
                    "El código y el nombre del idioma son obligatorios.");
            codigo = codigo.Trim().ToUpperInvariant();
            if (codigo.Length > 5)
                throw new BE.AppException("err.bll.idioma.codigo_largo",
                    "El código de idioma no puede superar los 5 caracteres.");
            if (dalIdioma.ExisteCodigo(codigo))
                throw new BE.AppException("err.bll.idioma.duplicado",
                    "Ya existe un idioma con el código '{0}'.", codigo);
            dalIdioma.Crear(codigo, nombre.Trim());
        }

        // Renombra un idioma existente.
        public void ModificarIdioma(int idIdioma, string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new BE.AppException("err.bll.idioma.nombre",
                    "El nombre del idioma es obligatorio.");
            dalIdioma.Modificar(idIdioma, nombre.Trim());
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
                System.Diagnostics.Trace.TraceError($"[BLL.Idioma.CargarTraducciones] {ex.Message}");
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
            // No se persisten traducciones en blanco/whitespace: una clave sin completar debe
            // quedar SIN traducción en BD para que el Traductor use el fallback por clave (texto del
            // idioma por defecto), en vez de guardar "" y dejar el control vacío en pantalla.
            if (string.IsNullOrWhiteSpace(texto)) return;
            dalTraduccion.GuardarTraduccion(idControl, idIdioma, texto.Trim());
        }

        // ── Controles (textos traducibles del sistema) — T05 ─────────────────────

        // Todos los controles traducibles, para el grid de Controles del FormIdiomas.
        public List<BE.Control> ObtenerControles()
        {
            return dalControl.ObtenerTodos();
        }

        // Cantidad de controles SIN traducción cargada (texto vacío o inexistente) para un idioma.
        // Usado para advertir al activar un idioma con cobertura incompleta (T05).
        public int ContarTraduccionesFaltantes(int idIdioma)
        {
            int totalControles = dalControl.ObtenerTodos().Count;
            int conTexto = 0;
            foreach (var f in dalTraduccion.ObtenerPorIdioma(idIdioma))
                if (!string.IsNullOrWhiteSpace(f.Texto)) conTexto++;
            int faltantes = totalControles - conTexto;
            return faltantes < 0 ? 0 : faltantes;
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
                System.Diagnostics.Trace.TraceError($"[BLL.Idioma.SeedearDesdeHardcode] {ex.Message}");
                throw;
            }
        }

    }
}
