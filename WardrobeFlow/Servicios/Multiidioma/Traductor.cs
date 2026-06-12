using System.Collections.Generic;

namespace Servicios.Multiidioma
{
    /// <summary>
    /// Proveedor de traducciones (lógica, no datos).
    ///
    /// FUENTE DE VERDAD EN RUNTIME: la tabla [Traduccion] en BD (cargada en GestorIdioma.TradActuales
    /// por BLL.Idioma). Cuando hay cache de BD, ObtenerTraducciones usa esa capa.
    ///
    /// CORPUS / FALLBACK: el conjunto completo de traducciones vive en un RECURSO EMBEBIDO de datos
    /// (Multiidioma/traducciones.tsv — formato idioma\tclave\ttexto), NO en diccionarios hardcodeados
    /// en código. Se usa para (a) el auto-seed inicial de la BD y (b) el fallback offline en los 4
    /// idiomas. No se usan hojas .resx. Para agregar/editar textos: el .tsv (o, ya seedeada, la BD
    /// vía el módulo Idiomas) — sin recompilar el resto del sistema.
    ///
    /// Idiomas soportados: Español (ES), English (EN), Русский (RU), Português (PT).
    ///
    /// Para agregar un nuevo idioma sin cambiar código:
    ///   1. Insertar la fila en la tabla Idioma de la BD (Codigo, Nombre, Activo=1).
    ///   2. Insertar sus traducciones en la tabla Traduccion vía el módulo Idiomas.
    ///   La app carga los idiomas activos desde BD al iniciar y construye los botones dinámicamente.
    ///
    /// Claves de traducción (se asignan como Tag de cada control en el formulario):
    ///   frm.login           → título del formulario Login
    ///   lbl.usuario         → label "Usuario"
    ///   lbl.contrasena      → label "Contraseña"
    ///   btn.ingresar        → botón "Ingresar"
    ///   btn.salir           → botón "Salir"
    ///   lnk.olvide          → link "¿Olvidaste tu contraseña?"
    ///   mnu.inventario      → menú "Inventario"
    ///   mnu.prendas         → ítem "Prendas"
    ///   mnu.ventas          → menú "Ventas"
    ///   mnu.clientes        → ítem "Clientes"
    ///   mnu.planes          → ítem "Planes de Suscripción"
    ///   mnu.pedidosventa    → ítem "Pedidos de Venta"
    ///   mnu.pedidosreal     → ítem "Pedidos Realizados"
    ///   mnu.administrar     → menú "Administrar"
    ///   mnu.usuarios        → ítem "Usuarios"
    ///   mnu.bitacora        → ítem "Bitácora"
    ///   mnu.cerrarsesion    → ítem "Cerrar Sesión"
    /// </summary>
    public static class Traductor
    {
        // ── Idiomas disponibles ───────────────────────────────────────────────

        /// <summary>Devuelve la lista de idiomas activos: desde BD si está disponible, o hardcodeada como fallback.</summary>
        public static IList<Idioma> ObtenerIdiomas()
        {
            var cache = GestorIdioma.IdiomasDisponibles;
            if (cache != null && cache.Count > 0) return cache;
            return ObtenerIdiomasHardcode();
        }

        /// <summary>Lista de idiomas hardcodeada — usada como fallback y durante el seeding inicial.</summary>
        public static IList<Idioma> ObtenerIdiomasHardcode()
        {
            return new List<Idioma>
            {
                new Idioma { Id = "ES", Nombre = "Español",   EsDefault = true  },
                new Idioma { Id = "EN", Nombre = "English",   EsDefault = false },
                new Idioma { Id = "RU", Nombre = "Русский",   EsDefault = false },
                new Idioma { Id = "PT", Nombre = "Português", EsDefault = false }
            };
        }

        /// <summary>Devuelve el idioma marcado como predeterminado (Español).</summary>
        public static Idioma ObtenerIdiomaDefault()
        {
            foreach (var i in ObtenerIdiomasHardcode())
                if (i.EsDefault) return i;
            return null;
        }

        // ── Traducciones ──────────────────────────────────────────────────────

        /// <summary>
        /// Devuelve el diccionario de traducciones para el idioma dado.
        ///
        /// Flujo con BD activa:
        ///   GestorIdioma.TradActuales tiene el dict cargado por BLL desde SQL.
        ///   Este método lo envuelve en IDictionary&lt;string, Traduccion&gt; para
        ///   mantener la firma que esperan todos los formularios existentes.
        ///
        /// Fallback hardcodeado:
        ///   Si TradActuales está vacío (primer arranque o error de BD),
        ///   devuelve los diccionarios hardcodeados originales.
        /// </summary>
        public static IDictionary<string, Traduccion> ObtenerTraducciones(Idioma idioma = null)
        {
            if (idioma == null)
                idioma = ObtenerIdiomaDefault();

            // Prioridad: cache cargado desde BD por BLL.Idioma.CargarTraducciones()
            var cache = GestorIdioma.TradActuales;
            if (cache != null && cache.Count > 0)
            {
                // T05 — FALLBACK por-clave: si el idioma activo no tiene cargada una
                // traducción, se usa el texto por defecto (idioma default, completo)
                // en lugar de dejar el control sin traducir. Esto permite activar un
                // idioma incompleto sin que la UI quede con textos faltantes/stale.
                var merged = new System.Collections.Generic.Dictionary<string, string>(
                    System.StringComparer.OrdinalIgnoreCase);
                // base: idioma default completo (desde el corpus embebido)
                foreach (var kv in ObtenerTraduccionesHardcode(ObtenerIdiomaDefault()))
                    merged[kv.Key] = kv.Value.Texto;
                // overlay: idioma activo, pero SOLO con textos no vacíos. Una traducción en blanco
                // (clave sin completar en ese idioma) NO debe pisar el texto por defecto, o el control
                // quedaría vacío en pantalla. Tratar vacío/whitespace como "sin traducción".
                foreach (var kv in cache)
                    if (!string.IsNullOrWhiteSpace(kv.Value)) merged[kv.Key] = kv.Value;
                return Construir(merged);
            }

            // Fallback: dicts hardcodeados (primer arranque o sin conexión)
            return ObtenerTraduccionesHardcode(idioma);
        }

        // Corpus de traducciones del idioma pedido, leído del recurso embebido (traducciones.tsv).
        // Lo usa BLL.Idioma en el auto-seed y este mismo Traductor como fallback offline.
        // Si el idioma no está en el corpus, cae al idioma default; si tampoco, dict vacío.
        public static IDictionary<string, Traduccion> ObtenerTraduccionesHardcode(Idioma idioma)
        {
            if (idioma == null) idioma = ObtenerIdiomaDefault();
            var corpus = Corpus();
            if (idioma != null && corpus.TryGetValue(idioma.Id, out var dict)) return dict;
            var def = ObtenerIdiomaDefault();
            if (def != null && corpus.TryGetValue(def.Id, out var dictDef)) return dictDef;
            return new Dictionary<string, Traduccion>();
        }

        // Asigna el módulo/formulario a cada clave de traducción según su prefijo.
        // Vive aquí (Servicios) porque es metadata de las claves — no lógica de negocio.
        public static string InferirFormulario(string clave)
        {
            if (clave.StartsWith("mnu.")) return "Menu";
            if (clave.StartsWith("frm."))
            {
                var s = clave.Substring(4);
                if (s.StartsWith("login"))          return "Login";
                if (s.StartsWith("clientes"))       return "Clientes";
                if (s.StartsWith("prendas"))        return "Prendas";
                if (s.StartsWith("gestion"))        return "GestionUsuarios";
                if (s.StartsWith("planes"))         return "Planes";
                if (s.StartsWith("bitacora"))       return "Bitacora";
                if (s.StartsWith("pedidosventa"))   return "PedidosVenta";
                if (s.StartsWith("pedidosreal"))    return "PedidosRealizados";
                if (s.StartsWith("historial"))      return "Historial";
                if (s.StartsWith("nuevocliente") || s.StartsWith("editarcliente")) return "NuevoCliente";
                if (s.StartsWith("nuevaprenda")  || s.StartsWith("editarprenda"))  return "NuevaPrenda";
                if (s.StartsWith("nuevopedido"))    return "NuevoPedido";
                if (s.StartsWith("mantenimiento")) return "MantenimientoHistorial";
                if (s.StartsWith("resetclave"))     return "ResetClave";
                if (s.StartsWith("cambioestado"))   return "CambioEstado";
                if (s.StartsWith("olvidepass"))     return "RecuperarClave";
                if (s.StartsWith("gestorpermisos")) return "GestorPermisos";
                if (s.StartsWith("idiomas"))        return "FormIdiomas";
            }
            if (clave.StartsWith("col.cli.") || clave.StartsWith("msg.cli.") ||
                clave.StartsWith("conf.baja.cli.") ||
                clave == "lbl.sinplan" || clave == "lbl.buscar")              return "Clientes";
            if (clave.StartsWith("lbl.cli.")  || clave.StartsWith("combo.cli.") ||
                clave.StartsWith("err.cli."))                                  return "NuevoCliente";
            if (clave.StartsWith("col.prenda.") || clave.StartsWith("msg.prenda.") ||
                clave.StartsWith("prenda.")     || clave.StartsWith("combo.prenda.") ||
                clave.StartsWith("opt.")        || clave.StartsWith("err.prenda."))  return "Prendas";
            if (clave.StartsWith("lbl.prenda.") || clave == "btn.agregar.prenda")   return "NuevaPrenda";
            if (clave.StartsWith("lbl.cambioest.") || clave.StartsWith("msg.cambioest.") ||
                clave.StartsWith("conf.baja.") || clave == "lbl.nuevoestado" ||
                clave == "btn.confirmar.cambio")                               return "CambioEstado";
            if (clave.StartsWith("col.usr.")  || clave.StartsWith("usr.") ||
                clave.StartsWith("msg.usr.")  || clave.StartsWith("err.usr.") ||
                clave.StartsWith("conf.desbloquear.") || clave.StartsWith("conf.resetmasivo.") ||
                clave.StartsWith("dlg.resetclave.") || clave == "btn.refrescar") return "GestionUsuarios";
            if (clave.StartsWith("err.clave.") || clave == "lbl.nueva.clave" ||
                clave == "lbl.confirmar.clave" || clave == "btn.confirmar.reset") return "ResetClave";
            if (clave.StartsWith("col.plan.")  || clave.StartsWith("plan.") ||
                clave.StartsWith("msg.planes.") || clave.StartsWith("conf.planes.") ||
                clave == "lbl.nuevopla" || clave == "lbl.nombreplan" || clave == "lbl.limiteprendas" ||
                clave == "lbl.preciomensual" || clave == "btn.guardarplan" || clave == "btn.limpiar" ||
                clave == "lbl.acciones" || clave == "btn.desactivar" || clave == "btn.activar" ||
                clave == "lbl.planesreg" || clave == "lbl.editplan")           return "Planes";
            if (clave == "frm.bitacora" ||
                clave.StartsWith("tab.")  || clave.StartsWith("col.bit.") ||
                clave.StartsWith("col.neg.") || clave.StartsWith("stat.") ||
                clave.StartsWith("crit.")  || clave.StartsWith("tevt.") ||
                clave.StartsWith("msg.bit.") || clave.StartsWith("err.pdf.") ||
                clave.StartsWith("bit.pdf.") ||
                clave == "btn.buscar" || clave == "btn.limpiarfiltro" ||
                clave == "btn.exportar" || clave == "btn.exportar.pdf" ||
                clave == "btn.ver" || clave == "lbl.exportarpdf" ||
                clave == "lbl.ultimos" || clave == "lbl.dias" ||
                clave == "lbl.usuarioid" || clave == "lbl.actividad" ||
                clave == "lbl.criticidad" || clave == "lbl.tipoevento" ||
                clave == "lbl.idpedido"  || clave == "lbl.idcliente")         return "Bitacora";
            if (clave.StartsWith("msg.ped.")  || clave.StartsWith("conf.cancelped.") ||
                clave.StartsWith("conf.descancelar.") || clave.StartsWith("conf.despachar.") ||
                clave.StartsWith("conf.entrega.") || clave.StartsWith("conf.devolucion.") ||
                clave.StartsWith("dlg.cancelped.") || clave == "msg.cancelped.req" ||
                clave == "btn.nuevopedido" || clave == "btn.cancelarpedido" ||
                clave == "btn.descancelar" || clave == "lbl.prendaspedido" ||
                clave == "btn.historial"  || clave == "col.ped.motivo" ||
                clave == "lbl.ped.seleccionado" || clave == "lbl.motivo")     return "PedidosVenta";
            if (clave.StartsWith("paso") || clave == "lbl.ped.selcliente" ||
                clave.StartsWith("combo.ped.") || clave == "lbl.ped.selprendas" ||
                clave == "btn.siguiente" || clave == "btn.volver" ||
                clave == "btn.confirmar.pedido" || clave == "btn.procesando" ||
                clave == "lbl.ped.infoplan" || clave == "err.ped.sinplan" ||
                clave == "err.ped.sinprendas" || clave == "err.ped.suscvencida" ||
                clave.StartsWith("lbl.ped.res.") ||
                clave == "conf.ped.titulo" || clave == "conf.ped.msg")         return "NuevoPedido";
            if (clave == "frm.mantenimiento" || clave == "btn.mantenimiento" ||
                clave == "btn.mant.cerrar"    ||
                clave.StartsWith("col.mant.")  || clave.StartsWith("mant.")  ||
                clave.StartsWith("msg.mant."))                                 return "MantenimientoHistorial";
            if (clave.StartsWith("col.ped.") || clave.StartsWith("urg.") ||
                clave.StartsWith("est.")     || clave.StartsWith("col.det.") ||
                clave == "btn.despachar" || clave == "btn.entregado" ||
                clave == "btn.vernotificacion" || clave == "btn.devolucion" ||
                clave == "lbl.detallepedido" || clave == "lbl.ped.detalletitulo") return "PedidosRealizados";
            if (clave.StartsWith("lbl.hist.") || clave.StartsWith("combo.hist.") ||
                clave.StartsWith("btn.hist.") || clave.StartsWith("col.hist.") ||
                clave.StartsWith("msg.hist.") || clave.StartsWith("conf.hist.") ||
                clave.StartsWith("accion.")   || clave == "err.hist.restaurar") return "Historial";
            if (clave.StartsWith("notif.") || clave.StartsWith("btn.copiar.") ||
                clave == "btn.copiado")                                         return "Notificacion";
            if (clave.StartsWith("lbl.recup.") || clave.StartsWith("err.recup.") ||
                clave.StartsWith("msg.recup.") || clave == "btn.enviar.solicitud") return "RecuperarClave";
            if (clave.StartsWith("lbl.permisos.") || clave.StartsWith("btn.permisos.") ||
                clave.StartsWith("msg.permisos.") || clave.StartsWith("perm."))    return "GestorPermisos";
            if (clave.StartsWith("lbl.idiomas.")  || clave.StartsWith("btn.idiomas.") ||
                clave.StartsWith("msg.idiomas.")  || clave.StartsWith("conf.idiomas."))  return "FormIdiomas";
            if (clave == "frm.backup"    || clave.StartsWith("btn.backup.") ||
                clave.StartsWith("col.backup.") || clave.StartsWith("msg.backup.") ||
                clave == "lbl.backup.info" || clave == "lbl.backup.ubicacion" ||
                clave == "mnu.backup")                                          return "Backup";
            if (clave == "frm.restauracion" || clave.StartsWith("lbl.rest.") ||
                clave.StartsWith("btn.rest.") || clave.StartsWith("msg.rest.") ||
                clave.StartsWith("conf.rest."))                                  return "Restauracion";
            if (clave == "frm.dashboard" || clave.StartsWith("dash."))           return "Dashboard";
            if (clave.StartsWith("rpt."))                                       return "ReporteJornada";
            if (clave == "frm.historialusr" || clave.StartsWith("lbl.ver.") ||
                clave.StartsWith("btn.ver.")  || clave.StartsWith("col.ver.") ||
                clave.StartsWith("msg.historial.") || clave == "mnu.historialusr") return "VersionHistorial";
            if (clave == "lbl.usuario"    || clave == "lbl.contrasena"  ||
                clave == "btn.ingresar"   || clave == "btn.salir"       ||
                clave == "lnk.olvide"     || clave == "lbl.idioma"      ||
                clave == "lbl.subtitulo"  || clave == "lbl.iniciarsesion" ||
                clave == "lbl.bienvenido" || clave == "lbl.credenciales" ||
                clave == "lbl.divider"    || clave == "lbl.brand.desc"  ||
                clave.StartsWith("err.login.") || clave.StartsWith("dlg.login.")) return "Login";
            if (clave.StartsWith("msg.modulo.") || clave == "lbl.proximamente") return "Menu";
            return "General";
        }

        // ── Corpus de traducciones (DATOS, no código) ──────────────────────────
        // Se carga UNA sola vez desde el recurso embebido 'traducciones.tsv'
        // (formato: idioma<TAB>clave<TAB>texto, con \n y \t escapados). Es la fuente
        // del auto-seed (BLL.Idioma) y el fallback offline para los 4 idiomas.
        // Sustituye a los antiguos diccionarios hardcodeados (~3950 líneas): el corpus
        // ya no vive en código sino en un archivo de datos.
        private static readonly object _lockCorpus = new object();
        private static Dictionary<string, IDictionary<string, Traduccion>> _corpus;

        private static Dictionary<string, IDictionary<string, Traduccion>> Corpus()
        {
            if (_corpus != null) return _corpus;
            lock (_lockCorpus)
            {
                if (_corpus == null) _corpus = CargarCorpus();
            }
            return _corpus;
        }

        private static Dictionary<string, IDictionary<string, Traduccion>> CargarCorpus()
        {
            var mapa = new Dictionary<string, IDictionary<string, Traduccion>>(
                System.StringComparer.OrdinalIgnoreCase);
            var asm = typeof(Traductor).Assembly;
            string recurso = asm.GetName().Name + ".Multiidioma.traducciones.tsv";
            using (var stream = asm.GetManifestResourceStream(recurso))
            {
                if (stream == null) return mapa; // sin recurso: los call-sites usan su literal inline
                using (var sr = new System.IO.StreamReader(stream, System.Text.Encoding.UTF8))
                {
                    string linea;
                    while ((linea = sr.ReadLine()) != null)
                    {
                        if (linea.Length == 0) continue;
                        int t1 = linea.IndexOf('\t');
                        if (t1 < 0) continue;
                        int t2 = linea.IndexOf('\t', t1 + 1);
                        if (t2 < 0) continue;
                        string lang  = linea.Substring(0, t1);
                        string clave = linea.Substring(t1 + 1, t2 - t1 - 1);
                        string texto = Desescapar(linea.Substring(t2 + 1));
                        if (!mapa.TryGetValue(lang, out var dict))
                        {
                            dict = new Dictionary<string, Traduccion>();
                            mapa[lang] = dict;
                        }
                        dict[clave] = new Traduccion { Clave = clave, Texto = texto };
                    }
                }
            }
            return mapa;
        }

        // Revierte el escapado del .tsv (\\n, \\t, \\\\) a sus caracteres reales.
        private static string Desescapar(string s)
        {
            if (s.IndexOf('\\') < 0) return s;
            var sb = new System.Text.StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c == '\\' && i + 1 < s.Length)
                {
                    char n = s[++i];
                    if (n == 'n') sb.Append('\n');
                    else if (n == 't') sb.Append('\t');
                    else if (n == '\\') sb.Append('\\');
                    else { sb.Append('\\'); sb.Append(n); }
                }
                else sb.Append(c);
            }
            return sb.ToString();
        }

        private static IDictionary<string, Traduccion> Construir(Dictionary<string, string> raw)
        {
            var result = new Dictionary<string, Traduccion>();
            foreach (var kv in raw)
                result[kv.Key] = new Traduccion { Clave = kv.Key, Texto = kv.Value };
            return result;
        }
    }
}
