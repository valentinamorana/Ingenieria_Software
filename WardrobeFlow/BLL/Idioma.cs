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
                        string formulario = InferirFormulario(kv.Key);
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

        // Asigna un módulo/formulario a cada clave de traducción basándose en el prefijo.
        private static string InferirFormulario(string clave)
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
                if (s.StartsWith("resetclave"))     return "ResetClave";
                if (s.StartsWith("cambioestado"))   return "CambioEstado";
                if (s.StartsWith("olvidepass"))      return "RecuperarClave";
                if (s.StartsWith("gestorpermisos")) return "GestorPermisos";
                if (s.StartsWith("idiomas"))        return "FormIdiomas";
            }
            if (clave.StartsWith("col.cli.") || clave.StartsWith("msg.cli.") ||
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
                clave.StartsWith("msg.planes.") ||
                clave == "lbl.nuevopla" || clave == "lbl.nombreplan" || clave == "lbl.limiteprendas" ||
                clave == "lbl.preciomensual" || clave == "btn.guardarplan" || clave == "btn.limpiar" ||
                clave == "lbl.acciones" || clave == "btn.desactivar" || clave == "btn.activar" ||
                clave == "lbl.planesreg" || clave == "lbl.editplan")           return "Planes";
            if (clave.StartsWith("tab.")  || clave.StartsWith("col.bit.") ||
                clave.StartsWith("col.neg.") || clave.StartsWith("stat.") ||
                clave.StartsWith("crit.")  || clave.StartsWith("tevt.") ||
                clave.StartsWith("msg.bit.") || clave.StartsWith("err.pdf.") ||
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
                clave == "err.ped.sinprendas")                                 return "NuevoPedido";
            if (clave.StartsWith("col.ped.") || clave.StartsWith("urg.") ||
                clave.StartsWith("est.")     || clave.StartsWith("col.det.") ||
                clave == "btn.despachar" || clave == "btn.entregado" ||
                clave == "btn.vernotificacion" || clave == "btn.devolucion" ||
                clave == "lbl.detallepedido" || clave == "lbl.ped.detalletitulo") return "PedidosRealizados";
            if (clave.StartsWith("lbl.hist.") || clave.StartsWith("combo.hist.") ||
                clave.StartsWith("btn.hist.") || clave.StartsWith("col.hist.") ||
                clave.StartsWith("accion.")   || clave == "err.hist.restaurar") return "Historial";
            if (clave.StartsWith("notif.") || clave.StartsWith("btn.copiar.") ||
                clave == "btn.copiado")                                         return "Notificacion";
            if (clave.StartsWith("lbl.recup.") || clave.StartsWith("err.recup.") ||
                clave.StartsWith("msg.recup.") || clave == "btn.enviar.solicitud") return "RecuperarClave";
            if (clave.StartsWith("lbl.permisos.") || clave.StartsWith("btn.permisos.") ||
                clave.StartsWith("msg.permisos.") || clave.StartsWith("perm."))           return "GestorPermisos";
            if (clave.StartsWith("lbl.idiomas.")  || clave.StartsWith("btn.idiomas."))  return "FormIdiomas";
            if (clave == "frm.backup"    || clave.StartsWith("btn.backup.") ||
                clave == "lbl.backup.info" || clave == "mnu.backup")          return "Backup";
            if (clave == "frm.historialusr" || clave.StartsWith("lbl.ver.") ||
                clave.StartsWith("btn.ver.")  || clave.StartsWith("col.ver.") ||
                clave == "mnu.historialusr")                                   return "VersionHistorial";
            if (clave == "lbl.usuario" || clave == "lbl.contrasena" ||
                clave == "btn.ingresar" || clave == "btn.salir" ||
                clave == "lnk.olvide"  || clave == "lbl.idioma" || clave == "lbl.subtitulo" ||
                clave == "lbl.iniciarsesion") return "Login";
            if (clave.StartsWith("msg.modulo.") || clave == "lbl.proximamente") return "Menu";
            return "General";
        }
    }
}
