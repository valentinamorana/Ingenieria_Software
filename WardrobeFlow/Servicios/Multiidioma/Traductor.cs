using System.Collections.Generic;

namespace Servicios.Multiidioma
{
    /// <summary>
    /// Fuente de datos de traducciones — equivalente a la clase Traductor del ejemplo de cátedra.
    ///
    /// En el ejemplo de cátedra las traducciones venían de SQL Server.
    /// Aquí se implementan como diccionarios hardcodeados en código puro,
    /// cumpliendo el requisito de NO usar hojas de recursos estáticos (.resx).
    ///
    /// Idiomas soportados: Español (ES), English (EN), Русский (RU).
    ///
    /// Para agregar un nuevo idioma:
    ///   1. Agregar una entrada en ObtenerIdiomas().
    ///   2. Agregar un nuevo case en ObtenerTraducciones() con su diccionario.
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

        /// <summary>Devuelve la lista de idiomas soportados por el sistema.</summary>
        public static IList<Idioma> ObtenerIdiomas()
        {
            return new List<Idioma>
            {
                new Idioma { Id = "ES", Nombre = "Español",  EsDefault = true  },
                new Idioma { Id = "EN", Nombre = "English",  EsDefault = false },
                new Idioma { Id = "RU", Nombre = "Русский",  EsDefault = false }
            };
        }

        /// <summary>Devuelve el idioma marcado como predeterminado (Español).</summary>
        public static Idioma ObtenerIdiomaDefault()
        {
            foreach (var i in ObtenerIdiomas())
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
                return Construir(new System.Collections.Generic.Dictionary<string, string>(cache));

            // Fallback: dicts hardcodeados (primer arranque o sin conexión)
            return ObtenerTraduccionesHardcode(idioma);
        }

        // Expuesto para que BLL.Idioma pueda leer los dicts hardcodeados en el seeding.
        public static IDictionary<string, Traduccion> ObtenerTraduccionesHardcode(Idioma idioma)
        {
            if (idioma == null) idioma = ObtenerIdiomaDefault();
            switch (idioma.Id)
            {
                case "EN": return _en;
                case "RU": return _ru;
                default:   return _es;
            }
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
                clave == "err.ped.sinprendas")                                 return "NuevoPedido";
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
                clave == "lbl.divider"    || clave == "lbl.brand.desc") return "Login";
            if (clave.StartsWith("msg.modulo.") || clave == "lbl.proximamente") return "Menu";
            return "General";
        }

        // ── Diccionario Español (ES) ───────────────────────────────────────────

        private static readonly IDictionary<string, Traduccion> _es =
            Construir(new Dictionary<string, string>
        {
            // Login — textos idénticos al Designer para que ES no cambie nada visualmente
            { "frm.login",        "WardrobeFlow"                   },
            { "lbl.usuario",      "Usuario"                        },
            { "lbl.contrasena",   "Contraseña"                     },
            { "btn.ingresar",     "INGRESAR"                       },
            { "btn.salir",        "SALIR"                          },
            { "lnk.olvide",       "Olvidé mi contraseña"           },
            { "lbl.iniciarsesion","Iniciar sesión"                  },
            // Barra de idioma
            { "lbl.idioma",       "Idioma:"                        },
            // Login — subtítulo
            { "lbl.subtitulo",    "PORTAL DE EMPLEADOS"            },
            { "lbl.bienvenido",   "Bienvenido de nuevo"            },
            { "lbl.credenciales", "Ingresá tus credenciales para continuar" },
            { "lbl.divider",      "o"                              },
            { "lbl.brand.desc",   "Acceso seguro y centralizado\na todos los módulos del sistema." },
            // Menú principal
            { "mnu.perfil",       "Perfil"                         },
            { "mnu.inventario",   "Inventario"                     },
            { "mnu.prendas",      "Prendas"                        },
            { "mnu.ventas",       "Ventas"                         },
            { "mnu.clientes",     "Clientes"                       },
            { "mnu.planes",       "Planes de Suscripcion"          },
            { "mnu.pedidosventa", "Pedidos de Venta"               },
            { "mnu.pedidosreal",  "Pedidos Realizados"             },
            { "mnu.administrar",  "Administrar"                    },
            { "mnu.usuarios",     "Usuarios"                       },
            { "mnu.perfiles",     "Perfiles y Permisos"            },
            { "mnu.bitacora",     "Bitácora"                       },
            { "mnu.cerrarsesion", "Cerrar Sesion"                  },
            { "mnu.idiomas",      "Gestión de Idiomas"             },
            // Clientes
            { "frm.clientes",     "Gestión de Clientes"            },
            { "lbl.buscar",       "Buscar:"                        },
            { "btn.nuevocliente", "+ Nuevo Cliente"                },
            { "btn.editar",       "✎ Editar"                       },
            { "btn.darbaja",      "✕ Dar de Baja"                  },
            // Prendas
            { "frm.prendas",      "Catálogo de Prendas"            },
            { "lbl.estado",       "Estado:"                        },
            { "btn.nuevaprenda",  "+ Nueva Prenda"                 },
            { "btn.cambiarestado","⇄ Estado"                       },
            { "lbl.clienteenuso", "Cliente en uso:"                },
            // Usuarios
            { "frm.gestion",      "Gestión de Usuarios"            },
            { "lbl.nuevousuario", "Nuevo Usuario"                  },
            { "lbl.nombreusuario","Nombre de usuario:"             },
            { "lbl.perfilrol",    "Perfil (rol):"                  },
            { "btn.agregar",      "Agregar Usuario"                },
            { "lbl.resettitulo",  "Resetear Contraseña"            },
            { "lbl.resetinfo",    "Seleccioná un usuario\nen la lista y presioná:" },
            { "btn.resetclave",   "Resetear Contraseña"            },
            { "lbl.desbloqtitulo","Desbloquear Cuenta"             },
            { "lbl.desbloqinfo",  "Seleccioná un usuario\nbloqueado y presioná:" },
            { "btn.desbloquear",  "Desbloquear Cuenta"             },
            { "lbl.listatitulo",  "Usuarios registrados en el sistema" },
            // Planes
            { "frm.planes",       "Planes de Suscripción"          },
            { "lbl.nuevopla",     "Nuevo Plan"                     },
            { "lbl.nombreplan",   "Nombre del plan *"              },
            { "lbl.limiteprendas","Límite de prendas *"            },
            { "lbl.preciomensual","Precio mensual ($) *"           },
            { "btn.guardarplan",  "Guardar Plan"                   },
            { "btn.limpiar",      "Limpiar / Nuevo"                },
            { "lbl.acciones",     "Acciones sobre plan seleccionado" },
            { "btn.desactivar",   "Desactivar Plan"                },
            { "btn.activar",      "Activar Plan"                   },
            { "lbl.planesreg",    "Planes registrados"             },
            // Bitácora
            { "frm.bitacora",            "Auditoría — Bitácoras del Sistema"                              },
            { "frm.bitacora.subtitulo", "Registro de eventos del sistema y operaciones de negocio"    },
            { "tab.sistema",      "🔐  Bitácora del Sistema"       },
            { "tab.negocio",      "📦  Bitácora de Negocio"        },
            { "lbl.ultimos",      "Últimos"                        },
            { "lbl.dias",         "días  (0 = todos)"              },
            { "btn.ver",          "Ver"                            },
            { "lbl.usuarioid",    "Usuario ID:"                    },
            { "lbl.actividad",    "Actividad:"                     },
            { "lbl.criticidad",   "Criticidad:"                    },
            { "btn.buscar",       "Buscar"                         },
            { "btn.limpiarfiltro","Limpiar"                        },
            { "btn.exportar",     "Exportar CSV"                   },
            { "lbl.tipoevento",   "Tipo evento:"                   },
            { "lbl.idpedido",     "ID Pedido:"                     },
            { "lbl.idcliente",    "ID Cliente:"                    },
            // Pedidos de Venta
            { "frm.pedidosventa",  "Pedidos de Venta"              },
            { "btn.nuevopedido",   "+ Nuevo Pedido"                 },
            { "btn.cancelarpedido","✕ Cancelar"                    },
            { "btn.descancelar",   "↩ Des-cancelar"                },
            { "lbl.prendaspedido", "Prendas del pedido seleccionado" },
            // Pedidos Realizados
            { "frm.pedidosreal2",  "Despacho de Pedidos"           },
            { "btn.despachar",     "📦 Despachar"                  },
            { "btn.entregado",     "✓ Marcar Entregado"            },
            { "btn.vernotificacion","✉ Ver Notificación"           },
            { "btn.devolucion",    "↩ Registrar Devolución"        },
            { "lbl.detallepedido", "Detalle del pedido seleccionado" },
            // Pedidos Realizados — columnas de grilla
            { "col.ped.urgencia",  "Urgencia"                      },
            { "col.ped.fecha",     "Fecha"                         },
            { "col.ped.cliente",   "Cliente"                       },
            { "col.ped.vendedor",  "Vendedor"                      },
            { "col.ped.prendas",   "Prendas"                       },
            { "col.ped.estado",    "Estado"                        },
            { "col.ped.despacho",  "Despacho"                      },
            { "col.ped.entrega",   "Entrega"                       },
            // Pedidos Realizados — valores de urgencia
            { "urg.urgente",       "Urgente"                       },
            { "urg.normal",        "Normal"                        },
            { "urg.reciente",      "Reciente"                      },
            // Pedidos Realizados — valores de estado
            { "est.pendiente",     "Pendiente"                     },
            { "est.despachado",    "Despachado"                    },
            { "est.entregado",     "Entregado"                     },
            { "est.cancelado",     "Cancelado"                     },
            // Bitácora Sistema — columnas de grilla
            { "col.bit.id",        "Id"                            },
            { "col.bit.fecha",     "Fecha"                         },
            { "col.bit.usuario",   "Usuario"                       },
            { "col.bit.modulo",    "Módulo"                        },
            { "col.bit.actividad", "Actividad"                     },
            { "col.bit.detalle",   "Detalle"                       },
            { "col.bit.criticidad","Criticidad"                    },
            { "col.bit.ip",        "IP"                            },
            // Bitácora Negocio — columnas de grilla
            { "col.neg.idevento",  "Id Evento"                     },
            { "col.neg.fecha",     "Fecha"                         },
            { "col.neg.tipo",      "Tipo"                          },
            { "col.neg.usuario",   "Usuario"                       },
            { "col.neg.cliente",   "Cliente"                       },
            { "col.neg.idpedido",  "Id Pedido"                     },
            { "col.neg.idprenda",  "Id Prenda"                     },
            { "col.neg.idcliente", "Id Cliente"                    },
            { "col.neg.desc",      "Descripción"                   },
            // Bitácora — estadísticas de criticidad
            { "stat.ninguno",      "Ninguno"                       },
            { "stat.baja",         "Baja"                          },
            { "stat.media",        "Media"                         },
            { "stat.alta",         "Alta"                          },
            { "stat.intlogin",     "Int.Login"                     },
            { "stat.recupclave",   "Recup.Clave"                   },
            { "stat.bloqueos",     "Bloqueos"                      },
            { "stat.sindatos",     "Sin datos de criticidad"       },
            // Historial de cambios de Pedido
            { "frm.historial",        "Historial de Cambios — Pedido"      },
            { "lbl.hist.pedido",      "Pedido #"                           },
            { "lbl.hist.filtros",     "Filtros"                            },
            { "combo.hist.todas",     "— Todas —"                          },
            { "lbl.hist.desde",       "Desde:"                             },
            { "lbl.hist.hasta",       "Hasta:"                             },
            { "lbl.hist.accion",      "Acción:"                            },
            { "btn.hist.buscar",      "🔍 Buscar"                          },
            { "btn.hist.restaurar",   "⟲ Restaurar"                        },
            { "btn.hist.cerrar",      "Cerrar"                             },
            { "col.hist.op",          "Op."                                },
            { "col.hist.fecha",       "Fecha"                              },
            { "col.hist.usuario",     "Usuario"                            },
            { "col.hist.accion",      "Acción"                             },
            { "col.hist.campo",       "Campo"                              },
            { "col.hist.anterior",    "Valor Anterior"                     },
            { "col.hist.nuevo",       "Valor Nuevo"                        },
            // Cerrar sesión — diálogo personalizado
            { "dlg.cerrarsesion.titulo", "Cerrar Sesión"                   },
            { "dlg.cerrarsesion.msg", "¿Está seguro que desea cerrar la sesión?" },
            { "btn.si",               "Sí"                                 },
            { "btn.no",               "No"                                 },
            // Usuarios — botón refrescar
            { "btn.refrescar",        "↻ Refrescar Lista"                  },
            // Prendas — combo de estado
            { "combo.prenda.todos",   "Todos"                              },
            { "prenda.disponible",    "Disponible"                         },
            { "prenda.enuso",         "En Uso"                             },
            { "prenda.enlimpieza",    "En Limpieza"                        },
            { "prenda.baja",          "Baja"                               },
            // Prendas — columnas de grilla
            { "col.prenda.nombre",    "Nombre"                             },
            { "col.prenda.categoria", "Categoría"                          },
            { "col.prenda.talle",     "Talle"                              },
            { "col.prenda.color",     "Color"                              },
            { "col.prenda.estado",    "Estado"                             },
            { "col.prenda.cliente",   "Cliente"                            },
            { "col.prenda.alta",      "Alta"                               },
            // Prendas — mensajes
            { "msg.prenda.conteo",    "Mostrando {0} de {1}"              },
            { "msg.prenda.cargadas",  "{0} prenda(s) en el catálogo."     },
            // Planes — columnas de grilla
            { "col.plan.nombre",      "Nombre"                             },
            { "col.plan.prendas",     "Prendas"                            },
            { "col.plan.precio",      "Precio"                             },
            { "col.plan.estado",      "Estado"                             },
            { "plan.activo",          "Activo"                             },
            { "plan.inactivo",        "Inactivo"                           },
            { "msg.planes.cargados",  "{0} plan(es) cargado(s)."          },
            // ClienteForm
            { "frm.nuevocliente",     "Nuevo Cliente"                      },
            { "frm.editarcliente",    "Editar Cliente"                     },
            { "lbl.cli.nombre",       "Nombre *"                           },
            { "lbl.cli.apellido",     "Apellido *"                         },
            { "lbl.cli.dni",          "DNI * (7-8 dígitos)"               },
            { "lbl.cli.email",        "Email"                              },
            { "lbl.cli.metodopago",   "Método de Pago *"                   },
            { "lbl.cli.plan",         "Plan de Suscripción"                },
            { "combo.cli.sinplan",    "— Sin plan —"                       },
            { "btn.registrar.cliente","Registrar Cliente"                  },
            { "btn.guardar.cambios",  "Guardar Cambios"                    },
            { "btn.cancelar",         "Cancelar"                           },
            // Clientes — columnas de grilla
            { "col.cli.nombre",       "Nombre"                             },
            { "col.cli.apellido",     "Apellido"                           },
            { "col.cli.dni",          "DNI"                                },
            { "col.cli.email",        "Email"                              },
            { "col.cli.plan",         "Plan"                               },
            { "col.cli.prendas",      "Prendas"                            },
            { "col.cli.metodopago",   "Método de Pago"                     },
            { "col.cli.alta",         "Alta"                               },
            // Clientes — mensajes
            { "lbl.sinplan",          "Sin plan"                           },
            { "msg.cli.conteo",       "Mostrando {0} de {1}"               },
            { "msg.cli.cargados",     "{0} cliente(s) registrado(s)."      },
            // Usuarios — columnas de grilla
            { "col.usr.username",     "Usuario"                            },
            { "col.usr.perfil",       "Perfil"                             },
            { "col.usr.estado",       "Estado"                             },
            // Usuarios — valores y mensajes
            { "usr.activo",           "Activo"                             },
            { "usr.bloqueada",        "Bloqueada"                          },
            { "msg.usr.cargados",     "{0} usuario(s) registrado(s)."      },
            // OlvideContrasenaForm
            { "frm.olvidepass",       "Recuperar Contraseña"               },
            { "lbl.recup.titulo",     "Recuperar Contraseña"               },
            { "lbl.recup.desc",       "Ingresá tu nombre de usuario. Un administrador\npodrá resetear tu contraseña desde el sistema." },
            { "lbl.recup.usuario",    "Nombre de usuario:"                 },
            { "btn.enviar.solicitud", "Enviar solicitud"                   },
            // PedidosRealizados — detalle de prendas (columnas)
            { "col.det.prenda",       "Prenda"                             },
            // PrendaForm — títulos y etiquetas
            { "frm.nuevaprenda",      "Nueva Prenda"                       },
            { "frm.editarprenda",     "Editar Prenda"                      },
            { "lbl.prenda.nombre",    "Nombre *"                           },
            { "lbl.prenda.descrip",   "Descripción"                        },
            { "lbl.prenda.talle",     "Talle *"                            },
            { "lbl.prenda.color",     "Color"                              },
            { "lbl.prenda.categoria", "Categoría *"                        },
            { "btn.agregar.prenda",   "Agregar Prenda"                     },
            // NuevoPedidoForm
            { "frm.nuevopedido",      "Nuevo Pedido de Venta"               },
            { "paso1.texto",          "Paso 1 de 2 — Seleccionar Cliente"   },
            { "paso2.texto",          "Paso 2 de 2 — Seleccionar Prendas"   },
            { "lbl.ped.selcliente",   "Seleccioná el cliente para este pedido:" },
            { "combo.ped.placeholder","— Seleccioná un cliente —"           },
            { "lbl.ped.selprendas",   "Seleccioná las prendas para incluir en el pedido (checkbox):" },
            { "btn.siguiente",        "Siguiente →"                         },
            { "btn.volver",           "← Volver"                            },
            { "btn.confirmar.pedido", "✓ Confirmar Pedido"                  },
            { "btn.procesando",       "Procesando..."                       },
            // PedidoHistorialForm — etiquetas de acción
            { "accion.crear",         "Crear"                               },
            { "accion.despachar",     "Despachar"                           },
            { "accion.entregar",      "Entregar"                            },
            { "accion.cancelar",      "Cancelar"                            },
            { "accion.descancelar",   "Des-cancelar"                        },
            { "accion.devolucion",    "Devolución"                          },
            { "accion.restaurar",     "Restaurar"                           },
            // Bitácora — combo de criticidad
            { "crit.todas",           "Todas"                               },
            { "crit.ninguno",         "Ninguno (0)"                         },
            { "crit.baja",            "Baja (1)"                            },
            { "crit.media",           "Media (2)"                           },
            { "crit.alta",            "Alta (3)"                            },
            { "crit.intlogin",        "Intentos Login (4)"                  },
            { "crit.recupclave",      "Recuperacion Clave (5)"              },
            { "crit.bloqueos",        "Bloqueos Cuenta (6)"                 },
            // Bitácora — botón Exportar PDF
            { "btn.exportar.pdf",     "📄 Exportar PDF"                     },
            // PedidosVenta / PedidosRealizados — contadores y mensajes de estado
            { "msg.ped.conteo",       "{0} pedido(s)"                                           },
            { "msg.ped.cargados",     "{0} pedido(s) cargado(s)."                               },
            { "msg.ped.creado",       "Pedido #{0} creado. Estado: Pendiente."                   },
            { "msg.ped.cancelado",    "Pedido #{0} cancelado. Prendas liberadas."                },
            { "msg.ped.reactivado",   "Pedido #{0} reactivado — volvió a Pendiente."             },
            { "msg.ped.ensistema",    "{0} pedido(s) en el sistema."                             },
            { "msg.ped.mostrando",    "Mostrando {0} de {1}"                                     },
            { "msg.ped.despachado",   "Pedido #{0} despachado correctamente."                    },
            { "msg.ped.entregado",    "Pedido #{0} marcado como Entregado."                      },
            { "msg.ped.devolucion",   "Devolución registrada — {0} prenda(s) pasan a EnLimpieza."},
            // Botón Historial (dinámico)
            { "btn.historial",        "📋 Historial"                                             },
            // PedirTexto inline dialog
            { "btn.aceptar",          "Aceptar"                                                  },
            { "dlg.cancelped.titulo", "Motivo de Cancelación"                                    },
            { "msg.cancelped.req",    "La cancelación requiere un motivo."                        },
            { "col.ped.motivo",       "Motivo"                                                   },
            { "lbl.ped.seleccionado", "Pedido #{0} — {1} — {2}"                                  },
            { "lbl.motivo",           "Motivo:"                                                  },
            // Confirmaciones de pedidos
            { "conf.cancelped.titulo",   "Confirmar Cancelación"                                 },
            { "conf.cancelped.body",     "¿Cancelar el Pedido #{0} de {1}?\n\nMotivo: {2}\n\nLas prendas volverán a estado Disponible." },
            { "conf.descancelar.titulo", "Confirmar Des-cancelación"                             },
            { "conf.descancelar.body",   "¿Des-cancelar el Pedido #{0} de {1}?\n\nSe verificará que las prendas originales estén disponibles\ny el pedido volverá a estado Pendiente." },
            { "conf.despachar.titulo",   "Confirmar Despacho"                                    },
            { "conf.despachar.body",     "¿Despachar el Pedido #{0}?\n\nCliente: {1}\nPrendas: {2}\n\nEl pedido pasará a estado Despachado." },
            { "conf.entrega.titulo",     "Confirmar Entrega"                                     },
            { "conf.entrega.body",       "¿Confirmar entrega del Pedido #{0} a {1}?"             },
            { "conf.devolucion.titulo",  "Confirmar Devolución"                                  },
            { "conf.devolucion.body",    "¿Registrar devolución del Pedido #{0}?\n\nCliente: {1}\nPrendas: {2}\n\nLas prendas pasarán a estado EnLimpieza." },
            // Prendas — mensajes de operación
            { "msg.prenda.agregada",     "Prenda '{0}' agregada al catálogo."                    },
            { "msg.prenda.actualizada",  "Prenda '{0}' actualizada."                             },
            { "msg.prenda.estadoact",    "Estado de '{0}' actualizado a {1}."                    },
            { "opt.enviarlimpieza",      "Enviar a Limpieza"                                     },
            { "opt.darbaja",             "Dar de Baja"                                           },
            { "opt.marcardisp",          "Marcar Disponible"                                     },
            { "err.prenda.enuso",        "No se puede cambiar el estado: la prenda está en uso por un cliente." },
            { "err.prenda.baja",         "La prenda está dada de baja y no puede ser reactivada." },
            // CambioEstadoDialog
            { "lbl.cambioest.info",      "Prenda: {0}  —  Estado actual: {1}"                    },
            { "msg.cambioest.selecciona","Seleccioná una opción."                                  },
            { "msg.cambioest.bajairrev", "La baja es irreversible. ¿Confirmar?"                   },
            { "conf.baja.titulo",        "Dar de Baja"                                            },
            // ResetClaveDialog
            { "frm.resetclave",          "Resetear Contraseña"                                    },
            { "lbl.nueva.clave",         "Nueva contraseña (mín. 6 caracteres):"                  },
            { "lbl.confirmar.clave",     "Confirmar contraseña:"                                  },
            { "btn.confirmar.reset",     "Confirmar Reset"                                        },
            { "err.clave.longitud",      "La contraseña debe tener al menos 6 caracteres."        },
            { "err.clave.nomatch",       "Las contraseñas no coinciden."                          },
            // Notificación de despacho (BtnVerNotificacion)
            { "notif.titulo",            "NOTIFICACIÓN DE PEDIDO"                                 },
            { "notif.numero",            "Pedido #:"                                              },
            { "notif.msgbox.titulo",     "Notificación — Pedido #{0}"                             },
            // CambioEstadoDialog — controles del Designer
            { "frm.cambioestado",        "Cambiar Estado de Prenda"                               },
            { "lbl.nuevoestado",         "Nuevo estado:"                                          },
            { "btn.confirmar.cambio",    "Confirmar Cambio"                                       },
            // Validaciones menores
            { "err.ped.sinprendas",      "Seleccioná al menos una prenda."                        },
            { "err.hist.restaurar",      "Seleccioná una fila del historial para restaurar."      },
            { "err.usr.sinperfil",       "Seleccioná un perfil/rol."                              },
            // Usuarios — validaciones de username
            { "err.usr.nombre.req",      "El nombre de usuario es obligatorio."                   },
            { "err.usr.nombre.longitud", "El nombre de usuario debe tener al menos 3 caracteres." },
            // ClienteForm — validaciones
            { "err.cli.dni.numeros",     "✗ El DNI solo puede contener números."                  },
            // PedidosRealizados — título de detalle
            { "lbl.ped.detalletitulo",   "Pedido #{0}  ·  {1}  ·  {2}  ·  {3} {4}"              },
            // TipoEventoNegocio — combo Bitácora de Negocio
            { "tevt.todos",              "Todos"                                                   },
            { "tevt.venta",              "Venta"                                                   },
            { "tevt.cancelacion",        "Cancelación"                                             },
            { "tevt.despacho",           "Despacho"                                               },
            { "tevt.entrega",            "Entrega"                                                 },
            { "tevt.altaprenda",         "Alta Prenda"                                             },
            { "tevt.modprenda",          "Modificación Prenda"                                     },
            { "tevt.cambiostprenda",     "Cambio Estado Prenda"                                    },
            { "tevt.altacliente",        "Alta Cliente"                                            },
            { "tevt.modcliente",         "Modificación Cliente"                                    },
            { "tevt.bajacliente",        "Baja Cliente"                                            },
            // Planes — título de formulario al editar
            { "lbl.editplan",            "Editar Plan"                                             },
            // Bitácora — etiquetas de resultados
            { "msg.bit.registros",       "  {0} registro(s)"                                       },
            { "msg.bit.ultimos",         "últimos {0} días"                                        },
            { "msg.bit.todos",           "todos los registros"                                     },
            // NotificacionDespachoForm — UI
            { "notif.frm.titulo",        "Notificación — Pedido #{0}"                              },
            { "notif.header.entregado",  "✓  Pedido #{0} — ENTREGADO"                             },
            { "notif.header.despachado", "📦  Pedido #{0} — DESPACHADO"                            },
            { "btn.copiar.porta",        "Copiar al portapapeles"                                  },
            { "btn.copiado",             "✓ Copiado"                                              },
            // Menu — módulos no disponibles
            { "msg.modulo.outfits",      "El módulo de Outfits aún no está disponible."           },
            { "msg.modulo.categorias",   "El módulo de Categorías aún no está disponible."        },
            { "lbl.proximamente",        "Próximamente"                                           },
            // Bitacora — exportar PDF
            { "err.pdf.sinDatos",        "No hay datos para exportar."                            },
            { "lbl.exportarpdf",         "Exportar PDF"                                           },
            // NuevoPedidoForm — info de plan
            { "lbl.ped.infoplan",        "Cliente: {0}\nPlan: {1}\nPrendas en uso actualmente: {2}\nMétodo de pago: {3}\nAlta: {4}" },
            { "err.ped.sinplan",         "⚠ {0} no tiene plan asignado.\nAsigná un plan en el módulo de Clientes antes de crear un pedido." },
            // OlvideContrasenaForm — mensajes
            { "err.recup.nousername",    "Ingresá tu nombre de usuario."                          },
            { "err.recup.nousuario",     "No se encontró el usuario '{0}'.\nVerificá que escribiste tu nombre correctamente." },
            { "msg.recup.exito",         "Usuario '{0}' encontrado.\nContacta al administrador para que resetee\ntu contrasena desde Administrar -> Usuarios." },
            { "err.recup.verificar",     "Error al verificar el usuario: {0}"                    },
            // Usuarios — mensajes de operación
            { "msg.usr.creado",          "Usuario '{0}' [{1}] creado correctamente."              },
            { "err.usr.selecciona",      "Seleccioná un usuario de la lista."                     },
            { "dlg.resetclave.prompt",   "Nueva contraseña para '{0}' (mínimo 6 caracteres):"     },
            { "msg.usr.clave.reseteada", "Contraseña de '{0}' reseteada correctamente."           },
            { "err.usr.sel.bloqueado",   "Seleccioná un usuario bloqueado de la lista."           },
            { "conf.desbloquear.body",   "¿Desbloquear la cuenta de '{0}'?"                      },
            { "conf.desbloquear.titulo", "Confirmar Desbloqueo"                                   },
            { "msg.usr.desbloqueada",    "Cuenta '{0}' desbloqueada correctamente."               },
            { "conf.resetmasivo.titulo", "Resetear todas las claves"                              },
            { "conf.resetmasivo.body",   "Esto va a resetear la contraseña de TODOS los usuarios a:\n\n   {0}\n\nComunicate con cada empleado para que la cambien.\n\n¿Confirmar?" },
            { "msg.usr.resetmasivo",     "Todas las claves fueron reseteadas a: {0}"              },
            // GestorPermisos
            { "frm.gestorpermisos",      "Gestor de Perfiles — Permisos"                          },
            { "lbl.permisos.titulo",     "Perfiles y Permisos"                                    },
            { "lbl.permisos.rol",        "Rol:"                                                   },
            { "btn.permisos.guardar",    "Guardar cambios"                                        },
            { "btn.permisos.cerrar",     "Cerrar"                                                 },
            { "msg.permisos.mostrando",  "Mostrando permisos del rol '{0}'."                      },
            { "msg.permisos.guardados",  "Cambios guardados: {0} permiso(s) asignado(s), {1} quitado(s)." },
            // GestorPermisos — roles, grupos y patentes
            { "perm.rol.administrador",             "Administrador"                              },
            { "perm.rol.vendedor",                  "Vendedor"                                   },
            { "perm.rol.operadorlogistico",         "Operador Logístico"                         },
            { "perm.rol.supervisor",                "Supervisor"                                 },
            { "perm.rol.controladordestock",        "Controlador de Stock"                       },
            { "perm.rol.operadordeinventario",      "Operador de Inventario"                     },
            { "perm.grp.inventario",                "Inventario"                                 },
            { "perm.grp.sistema",                   "Sistema"                                    },
            { "perm.grp.ventas",                    "Ventas"                                     },
            { "perm.pat.gestionarstock",            "Gestionar Stock"                            },
            { "perm.pat.vercategorias",             "Ver Categorías"                             },
            { "perm.pat.veroutfits",                "Ver Outfits"                                },
            { "perm.pat.verprendas",                "Ver Prendas"                                },
            { "perm.pat.gestionarusuarios",         "Gestionar Usuarios"                         },
            { "perm.pat.verauditoria",              "Ver Auditoría"                              },
            { "perm.pat.gestionarclientes",         "Gestionar Clientes"                         },
            { "perm.pat.gestionarplansuscripciones","Gestionar Plan Suscripciones"               },
            { "perm.pat.realizarventas",            "Realizar Ventas"                            },
            { "perm.pat.verpedidosrealizados",      "Ver Pedidos Realizados"                     },
            // FormIdiomas
            { "frm.idiomas",             "Gestión de Idiomas"                                     },
            { "lbl.idiomas.titulo",      "Idiomas del sistema"                                    },
            { "lbl.idiomas.trad",        "Traducciones del idioma seleccionado"                   },
            { "btn.idiomas.activar",     "✔ Activar"                                              },
            { "btn.idiomas.desactivar",  "✕ Desactivar"                                           },
            { "btn.idiomas.guardar",     "💾 Guardar cambios"                                     },
            // Backup y Restauración
            { "frm.backup",              "Backup y Restauración"                                  },
            { "btn.backup.crear",        "Generar Copia de Seguridad (.bak)"                      },
            { "btn.backup.restaurar",    "Restaurar Copia de Seguridad (.bak)"                    },
            { "btn.backup.eliminar",     "Eliminar"                                               },
            { "btn.backup.externo",      "Desde archivo..."                                       },
            { "col.backup.archivo",      "Archivo"                                                },
            { "col.backup.fecha",        "Fecha"                                                  },
            { "col.backup.autor",        "Autor"                                                  },
            { "col.backup.tamanio",      "Tamaño"                                                 },
            { "lbl.backup.info",         "Nota: la restauración cierra las conexiones activas y reinicia la aplicación." },
            { "lbl.backup.ubicacion",    "Ubicación de copias:"                                   },
            { "msg.backup.tituloeliminar","Confirmar Eliminación"                                 },
            { "msg.backup.confirmeliminar","¿Eliminar la copia de seguridad?\n\"{0}\"\n\nEsta acción no se puede deshacer." },
            { "msg.backup.titulorestaura","Confirmar Restauración"                                },
            { "msg.backup.confirmrestaura","¿Restaurar la base de datos desde:\n\"{0}\"?\n\nEsta operación sobrescribirá todos los datos actuales\ny reiniciará la aplicación." },
            { "mnu.backup",              "Backup y Restauración"                                  },
            { "msg.backup.creadoexito",  "Copia de seguridad generada con éxito:\n{0}"           },
            { "msg.backup.restauradaexito","Base de datos restaurada con éxito.\nLa aplicación se reiniciará." },
            { "msg.backup.restauradatitulo","Restauración Exitosa"                                },
            { "lbl.backup.sincopias",    "Sin copias de seguridad generadas aún."                 },
            { "lbl.backup.conteo",       "{0} copia(s) disponible(s). La más reciente: {1}"      },
            // RestauracionForm (Integridad DV)
            { "frm.restauracion",        "Integridad del Sistema"                                 },
            { "lbl.rest.titulo",         "Integridad del Sistema Comprometida"                    },
            { "lbl.rest.subtitulo",      "Se detectaron discrepancias en los dígitos verificadores. El acceso está bloqueado." },
            { "lbl.rest.detalle",        "Detalle del error:"                                     },
            { "btn.rest.recalcular",     "Recalcular Dígitos Verificadores"                       },
            { "btn.rest.backup",         "Restaurar desde Backup"                                 },
            { "btn.rest.salir",          "Salir"                                                  },
            // Menú — Panel de Control + Bitácoras divididas
            { "mnu.dashboard",           "Panel de Control"                                       },
            { "mnu.bitacora.sistema",    "Bitácora del Sistema"                                   },
            { "mnu.bitacora.negocio",    "Bitácora de Negocio"                                    },
            { "mnu.reportejornada",      "Reporte de Jornada"                                     },
            // Dashboard — Panel de Control
            { "frm.dashboard",           "Panel de Control"                                       },
            { "dash.prendas",            "Prendas\ndisponibles"                                   },
            { "dash.clientes",           "Clientes\nregistrados"                                  },
            { "dash.pedidos",            "Pedidos\npendientes"                                    },
            { "dash.backup",             "días sin\nbackup"                                       },
            { "dash.btn.refrescar",      "↻ Actualizar"                                           },
            // Reporte de Jornada
            { "frm.reportejornada",      "Reporte de Jornada — WardrobeFlow"                      },
            { "rpt.generar",             "Generar"                                                },
            { "rpt.comparar",            "Comparar jornadas"                                      },
            { "rpt.exportartxt",         "Exportar TXT"                                           },
            { "rpt.exportarpdf",         "Exportar PDF"                                           },
            { "rpt.fecha",               "Jornada:"                                               },
            { "rpt.fecha2",              "Comparar con:"                                          },
            { "rpt.kpi.prendas",         "Prendas disponibles"                                    },
            { "rpt.kpi.clientes",        "Clientes registrados"                                   },
            { "rpt.kpi.eventos",         "Eventos del día"                                        },
            { "rpt.kpi.backup",          "días sin backup"                                        },
            { "rpt.sinDatos",            "Sin eventos registrados para la jornada seleccionada."  },
            { "rpt.limpiar",             "Limpiar"                                                },
            { "rpt.subtitulo",           "Eventos de negocio por jornada con exportación a TXT"  },
            // Reporte de Jornada — cuerpo del texto generado
            { "rpt.txt.titulo",          "REPORTE DE JORNADA"                                    },
            { "rpt.txt.resumen",         "RESUMEN DEL SISTEMA"                                   },
            { "rpt.txt.prendas",         "Prendas disponibles"                                   },
            { "rpt.txt.clientes",        "Clientes registrados"                                  },
            { "rpt.txt.diassinbkp",      "Días sin backup"                                       },
            { "rpt.txt.sinbackups",      "Sin backups"                                            },
            { "rpt.txt.eventos",         "EVENTOS DE NEGOCIO DEL DÍA"                            },
            { "rpt.txt.sinevt",          "(sin eventos registrados para esta jornada)"            },
            { "rpt.txt.usuario",         "Usuario"                                               },
            { "rpt.txt.cliente",         "Cliente"                                               },
            { "rpt.txt.totalevt",        "TOTAL EVENTOS"                                         },
            { "rpt.txt.generado",        "Generado"                                              },
            { "rpt.txt.comparacion",     "COMPARACIÓN DE JORNADAS"                               },
            { "rpt.txt.jornada",         "JORNADA"                                               },
            { "rpt.txt.sinevtjorn",      "(sin eventos registrados en esta jornada)"              },
            { "rpt.txt.comparfinal",     "COMPARATIVO FINAL"                                     },
            { "rpt.txt.fecha",           "Fecha"                                                 },
            { "rpt.txt.eventostot",      "Eventos totales"                                       },
            { "rpt.txt.masmasa",         "tuvo más actividad"                                    },
            { "rpt.txt.ninguna",         "Ninguna jornada tuvo eventos registrados."              },
            { "rpt.txt.iguales",         "Ambas jornadas tuvieron la misma cantidad de eventos." },
            { "rpt.txt.rptoegenerado",   "Reporte generado"                                      },
            { "rpt.txt.compgenerada",    "Comparación generada"                                  },
            { "rpt.txt.impresionenv",    "Impresión enviada"                                     },
            { "bit.pdf.titulosistema",   "Bitácora del Sistema — WardrobeFlow"                   },
            { "bit.pdf.titulonegocio",   "Bitácora de Negocio — WardrobeFlow"                   },
            { "bit.pdf.pagina",          "WardrobeFlow — Página {0}"                             },
            { "bit.pdf.vistaprevia",     "Vista Previa"                                          },
            { "dash.backup.hoy",         "Hoy"                                                   },
            { "dash.sesion.iniciada",    "Sesión iniciada:"                                      },
            // Dashboard — actividad reciente y mini-stats
            { "dash.actividad.titulo",  "Actividad reciente"                                    },
            { "dash.stats.titulo",      "Resumen de eventos"                                    },
            { "dash.col.fecha",         "Fecha"                                                 },
            { "dash.col.evento",        "Evento"                                                },
            { "dash.col.usuario",       "Usuario"                                               },
            // Dashboard — avisos de backup y diálogo de configuración
            { "dash.aviso.sinbackup",   "⚠  Sin backups. Generá uno desde Administrar → Backup."           },
            { "dash.aviso.vencido",     "⚠  Hace {0} día(s) sin backup — recordatorio cada {1} días."      },
            { "dash.cfg.titulo",        "Recordatorio de Backup"                                            },
            { "dash.cfg.recada",        "Recordarme cada:"                                                  },
            { "dash.cfg.dias",          "días"                                                              },
            { "dash.cfg.guardar",       "Guardar"                                                           },
            // Reporte de Jornada — menús de exportación y diálogos
            { "rpt.menu.guardartxt",    "Guardar como .TXT"                                                 },
            { "rpt.menu.imprimir",      "Imprimir / Exportar PDF"                                           },
            { "rpt.menu.guardarcmp",    "Guardar comparación como .TXT"                                     },
            { "rpt.dlg.guardartxt",     "Guardar como TXT"                                                  },
            { "rpt.dlg.exito.titulo",   "Éxito"                                                             },
            { "rpt.dlg.exito.msg",      "Archivo guardado:\n{0}"                                            },
            // Historial de Cambios de Usuarios (T06)
            { "frm.historialusr",        "Historial de Cambios de Usuarios"                       },
            { "lbl.ver.usuario",         "Usuario:"                                               },
            { "btn.ver.cargar",          "Cargar"                                                 },
            { "btn.ver.restaurar",       "Restaurar Versión Seleccionada"                         },
            { "col.ver.id",              "ID"                                                     },
            { "col.ver.fecha",           "Fecha"                                                  },
            { "col.ver.actor",           "Modificado por"                                         },
            { "col.ver.detalle",         "Detalle"                                                },
            { "col.ver.estado",          "Estado"                                                 },
            { "mnu.historialusr",        "Historial de Cambios"                                   },
            // Mensajes ConfirmarAdminForm
            { "msg.confirmar.vacio",         "Ingrese usuario y contraseña."                                              },
            { "msg.confirmar.invalido",      "Credenciales inválidas o el usuario no es Administrador."                  },
            // Mensajes VersionHistorialForm
            { "msg.historial.sinseleccion",  "Seleccioná una versión de la grilla."                                      },
            { "msg.historial.atencion",      "Atención"                                                                  },
            { "msg.historial.confirmar",     "¿Restaurar al usuario '{0}' al estado del {1}?\n\nDetalle del snapshot: {2}\n\nEsta acción es reversible (se graba un nuevo snapshot antes de restaurar)." },
            { "msg.historial.restaurado",    "Versión restaurada correctamente."                                         },
            // Clientes — operación y confirmación de baja
            { "msg.cli.registrado",          "Cliente '{0}' registrado correctamente."                                  },
            { "msg.cli.actualizado",         "Cliente '{0}' actualizado."                                               },
            { "msg.cli.eliminado",           "Cliente '{0}' eliminado."                                                 },
            { "conf.baja.cli.msg",           "¿Dar de baja a {0} (DNI {1})?\n\nEsta acción no se puede deshacer."      },
            { "conf.baja.cli.titulo",        "Confirmar Baja"                                                           },
            { "err.cli.errorplanes",         "— Error al cargar planes —"                                              },
            // Planes — operación y confirmaciones
            { "msg.planes.creado",           "Plan '{0}' creado."                                                       },
            { "msg.planes.actualizado",      "Plan '{0}' actualizado."                                                  },
            { "msg.planes.desactivado",      "Plan '{0}' desactivado."                                                  },
            { "msg.planes.reactivado",       "Plan '{0}' reactivado."                                                   },
            { "conf.planes.desat.msg",       "¿Desactivar el plan '{0}'?\n\nLos clientes con este plan no serán afectados." },
            { "conf.planes.desat.tit",       "Confirmar Desactivación"                                                  },
            { "conf.planes.act.msg",         "¿Reactivar el plan '{0}'?\n\nEl plan volverá a estar disponible para nuevas suscripciones." },
            { "conf.planes.act.tit",         "Confirmar Activación"                                                     },
            // FormIdiomas — mensajes
            { "msg.idiomas.activado",        "Idioma activado."                                                         },
            { "msg.idiomas.desactivado",     "Idioma desactivado."                                                      },
            { "conf.idiomas.desactivar",     "¿Desactivar este idioma? Los usuarios no podrán seleccionarlo."          },
            { "conf.idiomas.titulo",         "Confirmar"                                                                 },
            // PedidoHistorialForm — restauración
            { "msg.hist.restaurado",         "Pedido #{0} restaurado correctamente."                                    },
            { "conf.hist.restaurar.msg",     "¿Restaurar el pedido #{0} al estado anterior a '{1}' (op. #{2})?\n\n⚠ Nota: esta operación modifica el estado del Pedido en la base de datos.\nEl estado de las Prendas asociadas NO se revierte automáticamente.\n\n¿Confirmar?" },
            // Usuarios — DV
            { "msg.usr.dvrecalculados",      "DVH y DVV recalculados correctamente para todos los usuarios."            },
            { "msg.usr.errordv",             "Error al recalcular DV: {0}"                                              },
            // RestauracionForm — mensajes adicionales
            { "msg.rest.dvexito",            "Dígitos verificadores recalculados con éxito.\nYa puede ingresar al sistema." },
            { "msg.rest.dvtitulo",           "Integridad restaurada"                                                    },
            { "conf.rest.sobreescribir",     "¿Está seguro? Esta operación sobrescribirá todos los datos actuales y reiniciará la aplicación." },
            { "msg.rest.errorrecalcular",    "Error al recalcular: {0}"                                                 },
            { "msg.rest.errorrestaurar",     "Error al restaurar: {0}"                                                  },
            // BackupForm — errores en catch
            { "msg.backup.errorgenerar",     "Error al generar copia de seguridad:\n{0}"                               },
            { "msg.backup.erroreliminar",    "Error al eliminar:\n{0}"                                                  },
            { "msg.backup.errorrestaurar",   "Error al restaurar:\n{0}"                                                 },
            // VersionHistorialForm — errores en catch
            { "msg.historial.errorcargar",   "Error al cargar historial:\n{0}"                                          },
            { "msg.historial.errorrestaur",  "Error al restaurar versión:\n{0}"                                         },
            // Título genérico de error
            { "msg.error.titulo",            "Error"                                                                     },
        });

        // ── Diccionario English (EN) ──────────────────────────────────────────

        private static readonly IDictionary<string, Traduccion> _en =
            Construir(new Dictionary<string, string>
        {
            // Login
            { "frm.login",        "WardrobeFlow — Login"           },
            { "lbl.usuario",      "Username"                        },
            { "lbl.contrasena",   "Password"                        },
            { "btn.ingresar",     "Sign In"                         },
            { "btn.salir",        "Exit"                            },
            { "lnk.olvide",       "Forgot your password?"           },
            { "lbl.iniciarsesion","Sign In"                          },
            // Language bar
            { "lbl.idioma",       "Language:"                       },
            // Login — subtitle
            { "lbl.subtitulo",    "EMPLOYEE PORTAL"                 },
            { "lbl.bienvenido",   "Welcome back"                    },
            { "lbl.credenciales", "Enter your credentials to continue" },
            { "lbl.divider",      "or"                              },
            { "lbl.brand.desc",   "Secure and centralized access\nto all system modules." },
            // Main menu
            { "mnu.perfil",       "Profile"                         },
            { "mnu.inventario",   "Inventory"                       },
            { "mnu.prendas",      "Garments"                        },
            { "mnu.ventas",       "Sales"                           },
            { "mnu.clientes",     "Clients"                         },
            { "mnu.planes",       "Subscription Plans"              },
            { "mnu.pedidosventa", "Sales Orders"                    },
            { "mnu.pedidosreal",  "Fulfilled Orders"                },
            { "mnu.administrar",  "Administration"                  },
            { "mnu.usuarios",     "Users"                           },
            { "mnu.perfiles",     "Profiles & Permissions"          },
            { "mnu.bitacora",     "Audit Log"                       },
            { "mnu.cerrarsesion", "Sign Out"                        },
            { "mnu.idiomas",      "Language Management"            },
            // Clients
            { "frm.clientes",     "Client Management"              },
            { "lbl.buscar",       "Search:"                        },
            { "btn.nuevocliente", "+ New Client"                   },
            { "btn.editar",       "✎ Edit"                         },
            { "btn.darbaja",      "✕ Delete"                       },
            // Garments
            { "frm.prendas",      "Garment Catalog"                },
            { "lbl.estado",       "Status:"                        },
            { "btn.nuevaprenda",  "+ New Garment"                  },
            { "btn.cambiarestado","⇄ Status"                       },
            { "lbl.clienteenuso", "Client in use:"                 },
            // Users
            { "frm.gestion",      "User Management"                },
            { "lbl.nuevousuario", "New User"                       },
            { "lbl.nombreusuario","Username:"                      },
            { "lbl.perfilrol",    "Profile (role):"                },
            { "btn.agregar",      "Add User"                       },
            { "lbl.resettitulo",  "Reset Password"                 },
            { "lbl.resetinfo",    "Select a user\nfrom the list and press:" },
            { "btn.resetclave",   "Reset Password"                 },
            { "lbl.desbloqtitulo","Unlock Account"                 },
            { "lbl.desbloqinfo",  "Select a locked\nuser and press:" },
            { "btn.desbloquear",  "Unlock Account"                 },
            { "lbl.listatitulo",  "Users registered in the system" },
            // Plans
            { "frm.planes",       "Subscription Plans"             },
            { "lbl.nuevopla",     "New Plan"                       },
            { "lbl.nombreplan",   "Plan name *"                    },
            { "lbl.limiteprendas","Garment limit *"                },
            { "lbl.preciomensual","Monthly price ($) *"            },
            { "btn.guardarplan",  "Save Plan"                      },
            { "btn.limpiar",      "Clear / New"                    },
            { "lbl.acciones",     "Actions on selected plan"       },
            { "btn.desactivar",   "Deactivate Plan"                },
            { "btn.activar",      "Activate Plan"                  },
            { "lbl.planesreg",    "Registered plans"               },
            // Audit Log
            { "frm.bitacora",            "Audit Log"                                                  },
            { "frm.bitacora.subtitulo", "Record of system events and business operations"            },
            { "tab.sistema",      "🔐  System Log"                 },
            { "tab.negocio",      "📦  Business Log"               },
            { "lbl.ultimos",      "Last"                           },
            { "lbl.dias",         "days  (0 = all)"                },
            { "btn.ver",          "View"                           },
            { "lbl.usuarioid",    "User ID:"                       },
            { "lbl.actividad",    "Activity:"                      },
            { "lbl.criticidad",   "Severity:"                      },
            { "btn.buscar",       "Search"                         },
            { "btn.limpiarfiltro","Clear"                          },
            { "btn.exportar",     "Export CSV"                     },
            { "lbl.tipoevento",   "Event type:"                    },
            { "lbl.idpedido",     "Order ID:"                      },
            { "lbl.idcliente",    "Client ID:"                     },
            // Sales Orders
            { "frm.pedidosventa",  "Sales Orders"                  },
            { "btn.nuevopedido",   "+ New Order"                   },
            { "btn.cancelarpedido","✕ Cancel"                      },
            { "btn.descancelar",   "↩ Restore"                     },
            { "lbl.prendaspedido", "Garments of the selected order" },
            // Order Dispatch
            { "frm.pedidosreal2",  "Order Dispatch"                },
            { "btn.despachar",     "📦 Dispatch"                   },
            { "btn.entregado",     "✓ Mark Delivered"              },
            { "btn.vernotificacion","✉ View Notification"          },
            { "btn.devolucion",    "↩ Register Return"             },
            { "lbl.detallepedido", "Selected order detail"         },
            // Order Dispatch — grid columns
            { "col.ped.urgencia",  "Urgency"                       },
            { "col.ped.fecha",     "Date"                          },
            { "col.ped.cliente",   "Client"                        },
            { "col.ped.vendedor",  "Seller"                        },
            { "col.ped.prendas",   "Garments"                      },
            { "col.ped.estado",    "Status"                        },
            { "col.ped.despacho",  "Dispatch"                      },
            { "col.ped.entrega",   "Delivery"                      },
            // Order Dispatch — urgency values
            { "urg.urgente",       "Urgent"                        },
            { "urg.normal",        "Normal"                        },
            { "urg.reciente",      "Recent"                        },
            // Order Dispatch — status values
            { "est.pendiente",     "Pending"                       },
            { "est.despachado",    "Dispatched"                    },
            { "est.entregado",     "Delivered"                     },
            { "est.cancelado",     "Cancelled"                     },
            // Audit Log Sistema — grid columns
            { "col.bit.id",        "Id"                            },
            { "col.bit.fecha",     "Date"                          },
            { "col.bit.usuario",   "User"                          },
            { "col.bit.modulo",    "Module"                        },
            { "col.bit.actividad", "Activity"                      },
            { "col.bit.detalle",   "Detail"                        },
            { "col.bit.criticidad","Severity"                      },
            { "col.bit.ip",        "IP"                            },
            // Audit Log Negocio — grid columns
            { "col.neg.idevento",  "Event Id"                      },
            { "col.neg.fecha",     "Date"                          },
            { "col.neg.tipo",      "Type"                          },
            { "col.neg.usuario",   "User"                          },
            { "col.neg.cliente",   "Client"                        },
            { "col.neg.idpedido",  "Order Id"                      },
            { "col.neg.idprenda",  "Garment Id"                    },
            { "col.neg.idcliente", "Client Id"                     },
            { "col.neg.desc",      "Description"                   },
            // Audit Log — severity statistics
            { "stat.ninguno",      "None"                          },
            { "stat.baja",         "Low"                           },
            { "stat.media",        "Medium"                        },
            { "stat.alta",         "High"                          },
            { "stat.intlogin",     "Login Att."                    },
            { "stat.recupclave",   "Pwd Reset"                     },
            { "stat.bloqueos",     "Lockouts"                      },
            { "stat.sindatos",     "No severity data"              },
            // Order Change History
            { "frm.historial",        "Order Change History"               },
            { "lbl.hist.pedido",      "Order #"                            },
            { "lbl.hist.filtros",     "Filters"                            },
            { "combo.hist.todas",     "— All —"                            },
            { "lbl.hist.desde",       "From:"                              },
            { "lbl.hist.hasta",       "To:"                                },
            { "lbl.hist.accion",      "Action:"                            },
            { "btn.hist.buscar",      "🔍 Search"                          },
            { "btn.hist.restaurar",   "⟲ Restore"                          },
            { "btn.hist.cerrar",      "Close"                              },
            { "col.hist.op",          "Op."                                },
            { "col.hist.fecha",       "Date"                               },
            { "col.hist.usuario",     "User"                               },
            { "col.hist.accion",      "Action"                             },
            { "col.hist.campo",       "Field"                              },
            { "col.hist.anterior",    "Previous Value"                     },
            { "col.hist.nuevo",       "New Value"                          },
            // Sign out dialog
            { "dlg.cerrarsesion.titulo", "Sign Out"                        },
            { "dlg.cerrarsesion.msg", "Are you sure you want to sign out?" },
            { "btn.si",               "Yes"                                },
            { "btn.no",               "No"                                 },
            // Users — refresh button
            { "btn.refrescar",        "↻ Refresh List"                    },
            // Garments — status combo
            { "combo.prenda.todos",   "All"                                },
            { "prenda.disponible",    "Available"                          },
            { "prenda.enuso",         "In Use"                             },
            { "prenda.enlimpieza",    "In Cleaning"                        },
            { "prenda.baja",          "Retired"                            },
            // Garments — grid columns
            { "col.prenda.nombre",    "Name"                               },
            { "col.prenda.categoria", "Category"                           },
            { "col.prenda.talle",     "Size"                               },
            { "col.prenda.color",     "Color"                              },
            { "col.prenda.estado",    "Status"                             },
            { "col.prenda.cliente",   "Client"                             },
            { "col.prenda.alta",      "Registered"                         },
            // Garments — messages
            { "msg.prenda.conteo",    "Showing {0} of {1}"                },
            { "msg.prenda.cargadas",  "{0} garment(s) in catalog."        },
            // Plans — grid columns
            { "col.plan.nombre",      "Name"                               },
            { "col.plan.prendas",     "Garments"                           },
            { "col.plan.precio",      "Price"                              },
            { "col.plan.estado",      "Status"                             },
            { "plan.activo",          "Active"                             },
            { "plan.inactivo",        "Inactive"                           },
            { "msg.planes.cargados",  "{0} plan(s) loaded."               },
            // ClienteForm
            { "frm.nuevocliente",     "New Client"                         },
            { "frm.editarcliente",    "Edit Client"                        },
            { "lbl.cli.nombre",       "First name *"                       },
            { "lbl.cli.apellido",     "Last name *"                        },
            { "lbl.cli.dni",          "DNI * (7-8 digits)"                },
            { "lbl.cli.email",        "Email"                              },
            { "lbl.cli.metodopago",   "Payment method *"                   },
            { "lbl.cli.plan",         "Subscription plan"                  },
            { "combo.cli.sinplan",    "— No plan —"                        },
            { "btn.registrar.cliente","Register Client"                    },
            { "btn.guardar.cambios",  "Save Changes"                       },
            { "btn.cancelar",         "Cancel"                             },
            // Clients — grid columns
            { "col.cli.nombre",       "First Name"                         },
            { "col.cli.apellido",     "Last Name"                          },
            { "col.cli.dni",          "DNI"                                },
            { "col.cli.email",        "Email"                              },
            { "col.cli.plan",         "Plan"                               },
            { "col.cli.prendas",      "Garments"                           },
            { "col.cli.metodopago",   "Payment Method"                     },
            { "col.cli.alta",         "Registered"                         },
            // Clients — messages
            { "lbl.sinplan",          "No plan"                            },
            { "msg.cli.conteo",       "Showing {0} of {1}"                 },
            { "msg.cli.cargados",     "{0} client(s) registered."          },
            // Users — grid columns
            { "col.usr.username",     "Username"                           },
            { "col.usr.perfil",       "Role"                               },
            { "col.usr.estado",       "Status"                             },
            // Users — values and messages
            { "usr.activo",           "Active"                             },
            { "usr.bloqueada",        "Locked"                             },
            { "msg.usr.cargados",     "{0} user(s) registered."            },
            // Password recovery form
            { "frm.olvidepass",       "Password Recovery"                  },
            { "lbl.recup.titulo",     "Password Recovery"                  },
            { "lbl.recup.desc",       "Enter your username. An administrator\ncan reset your password from the system." },
            { "lbl.recup.usuario",    "Username:"                          },
            { "btn.enviar.solicitud", "Submit request"                     },
            // Password recovery — messages
            { "err.recup.nousername",    "Enter your username."                                                           },
            { "err.recup.nousuario",     "User '{0}' not found.\nCheck that you typed your username correctly."          },
            { "msg.recup.exito",         "User '{0}' found.\nContact the administrator to reset\nyour password from Manage → Users." },
            { "err.recup.verificar",     "Error verifying user: {0}"                                                     },
            // Order detail — garment columns
            { "col.det.prenda",       "Garment"                            },
            // PrendaForm — titles and labels
            { "frm.nuevaprenda",      "New Garment"                        },
            { "frm.editarprenda",     "Edit Garment"                       },
            { "lbl.prenda.nombre",    "Name *"                             },
            { "lbl.prenda.descrip",   "Description"                        },
            { "lbl.prenda.talle",     "Size *"                             },
            { "lbl.prenda.color",     "Color"                              },
            { "lbl.prenda.categoria", "Category *"                         },
            { "btn.agregar.prenda",   "Add Garment"                        },
            // New Sales Order form
            { "frm.nuevopedido",      "New Sales Order"                     },
            { "paso1.texto",          "Step 1 of 2 — Select Client"         },
            { "paso2.texto",          "Step 2 of 2 — Select Garments"       },
            { "lbl.ped.selcliente",   "Select the client for this order:"   },
            { "combo.ped.placeholder","— Select a client —"                 },
            { "lbl.ped.selprendas",   "Select the garments to include in the order (checkbox):" },
            { "btn.siguiente",        "Next →"                              },
            { "btn.volver",           "← Back"                              },
            { "btn.confirmar.pedido", "✓ Confirm Order"                     },
            { "btn.procesando",       "Processing..."                       },
            // Order Change History — action labels
            { "accion.crear",         "Create"                              },
            { "accion.despachar",     "Dispatch"                            },
            { "accion.entregar",      "Deliver"                             },
            { "accion.cancelar",      "Cancel"                              },
            { "accion.descancelar",   "Un-cancel"                           },
            { "accion.devolucion",    "Return"                              },
            { "accion.restaurar",     "Restore"                             },
            // Audit Log — severity combo
            { "crit.todas",           "All"                                 },
            { "crit.ninguno",         "None (0)"                            },
            { "crit.baja",            "Low (1)"                             },
            { "crit.media",           "Medium (2)"                          },
            { "crit.alta",            "High (3)"                            },
            { "crit.intlogin",        "Login Attempts (4)"                  },
            { "crit.recupclave",      "Password Recovery (5)"               },
            { "crit.bloqueos",        "Account Lockouts (6)"                },
            // Audit Log — Export PDF button
            { "btn.exportar.pdf",     "📄 Export PDF"                       },
            // Sales Orders / Fulfilled Orders — counters and status messages
            { "msg.ped.conteo",       "{0} order(s)"                                            },
            { "msg.ped.cargados",     "{0} order(s) loaded."                                    },
            { "msg.ped.creado",       "Order #{0} created. Status: Pending."                    },
            { "msg.ped.cancelado",    "Order #{0} cancelled. Garments released."                },
            { "msg.ped.reactivado",   "Order #{0} reactivated — back to Pending."               },
            { "msg.ped.ensistema",    "{0} order(s) in the system."                             },
            { "msg.ped.mostrando",    "Showing {0} of {1}"                                      },
            { "msg.ped.despachado",   "Order #{0} dispatched successfully."                     },
            { "msg.ped.entregado",    "Order #{0} marked as Delivered."                         },
            { "msg.ped.devolucion",   "Return registered — {0} garment(s) sent to Cleaning."   },
            // History button (dynamic)
            { "btn.historial",        "📋 History"                                              },
            // PedirTexto inline dialog
            { "btn.aceptar",          "Accept"                                                  },
            { "dlg.cancelped.titulo", "Cancellation Reason"                                     },
            { "msg.cancelped.req",    "Cancellation requires a reason."                         },
            { "col.ped.motivo",       "Reason"                                                  },
            { "lbl.ped.seleccionado", "Order #{0} — {1} — {2}"                                  },
            { "lbl.motivo",           "Reason:"                                                 },
            // Order confirmations
            { "conf.cancelped.titulo",   "Confirm Cancellation"                                 },
            { "conf.cancelped.body",     "Cancel Order #{0} for {1}?\n\nReason: {2}\n\nGarments will return to Available status." },
            { "conf.descancelar.titulo", "Confirm Restore"                                      },
            { "conf.descancelar.body",   "Restore Order #{0} for {1}?\n\nAvailability of original garments will be verified\nand the order will return to Pending." },
            { "conf.despachar.titulo",   "Confirm Dispatch"                                     },
            { "conf.despachar.body",     "Dispatch Order #{0}?\n\nClient: {1}\nGarments: {2}\n\nOrder will move to Dispatched status." },
            { "conf.entrega.titulo",     "Confirm Delivery"                                     },
            { "conf.entrega.body",       "Confirm delivery of Order #{0} to {1}?"               },
            { "conf.devolucion.titulo",  "Confirm Return"                                       },
            { "conf.devolucion.body",    "Register return of Order #{0}?\n\nClient: {1}\nGarments: {2}\n\nGarments will move to Cleaning status." },
            // Garments — operation messages
            { "msg.prenda.agregada",     "Garment '{0}' added to catalog."                      },
            { "msg.prenda.actualizada",  "Garment '{0}' updated."                               },
            { "msg.prenda.estadoact",    "Status of '{0}' updated to {1}."                      },
            { "opt.enviarlimpieza",      "Send to Cleaning"                                     },
            { "opt.darbaja",             "Retire"                                               },
            { "opt.marcardisp",          "Mark Available"                                       },
            { "err.prenda.enuso",        "Cannot change status: garment is currently in use by a client." },
            { "err.prenda.baja",         "The garment is retired and cannot be reactivated."    },
            // CambioEstadoDialog
            { "lbl.cambioest.info",      "Garment: {0}  —  Current status: {1}"                },
            { "msg.cambioest.selecciona","Select an option."                                     },
            { "msg.cambioest.bajairrev", "Retirement is irreversible. Confirm?"                 },
            { "conf.baja.titulo",        "Retire Garment"                                       },
            // ResetClaveDialog
            { "frm.resetclave",          "Reset Password"                                       },
            { "lbl.nueva.clave",         "New password (min. 6 chars):"                         },
            { "lbl.confirmar.clave",     "Confirm password:"                                    },
            { "btn.confirmar.reset",     "Confirm Reset"                                        },
            { "err.clave.longitud",      "Password must be at least 6 characters."              },
            { "err.clave.nomatch",       "Passwords do not match."                              },
            // Dispatch notification (BtnVerNotificacion)
            { "notif.titulo",            "ORDER NOTIFICATION"                                   },
            { "notif.numero",            "Order #:"                                             },
            { "notif.msgbox.titulo",     "Notification — Order #{0}"                            },
            // CambioEstadoDialog — Designer controls
            { "frm.cambioestado",        "Change Garment Status"                                },
            { "lbl.nuevoestado",         "New status:"                                          },
            { "btn.confirmar.cambio",    "Confirm Change"                                       },
            // Minor validations
            { "err.ped.sinprendas",      "Select at least one garment."                        },
            { "err.hist.restaurar",      "Select a history row to restore."                    },
            { "err.usr.sinperfil",       "Select a profile/role."                              },
            // Users — username validations
            { "err.usr.nombre.req",      "Username is required."                               },
            { "err.usr.nombre.longitud", "Username must be at least 3 characters."             },
            // ClienteForm — validations
            { "err.cli.dni.numeros",     "✗ DNI can only contain numbers."                     },
            // PedidosRealizados — detail title
            { "lbl.ped.detalletitulo",   "Order #{0}  ·  {1}  ·  {2}  ·  {3} {4}"              },
            // Business event type — Bitácora combo
            { "tevt.todos",              "All"                                                    },
            { "tevt.venta",              "Sale"                                                   },
            { "tevt.cancelacion",        "Cancellation"                                           },
            { "tevt.despacho",           "Dispatch"                                              },
            { "tevt.entrega",            "Delivery"                                              },
            { "tevt.altaprenda",         "New Garment"                                           },
            { "tevt.modprenda",          "Garment Edit"                                          },
            { "tevt.cambiostprenda",     "Garment Status Change"                                 },
            { "tevt.altacliente",        "New Client"                                            },
            { "tevt.modcliente",         "Client Edit"                                           },
            { "tevt.bajacliente",        "Client Removed"                                        },
            // Plans — form title on edit
            { "lbl.editplan",            "Edit Plan"                                             },
            // Bitácora — result labels
            { "msg.bit.registros",       "  {0} record(s)"                                       },
            { "msg.bit.ultimos",         "last {0} days"                                         },
            { "msg.bit.todos",           "all records"                                           },
            // NotificacionDespachoForm — UI
            { "notif.frm.titulo",        "Notification — Order #{0}"                             },
            { "notif.header.entregado",  "✓  Order #{0} — DELIVERED"                            },
            { "notif.header.despachado", "📦  Order #{0} — DISPATCHED"                           },
            { "btn.copiar.porta",        "Copy to clipboard"                                     },
            { "btn.copiado",             "✓ Copied"                                             },
            // Menu — unavailable modules
            { "msg.modulo.outfits",      "The Outfits module is not yet available."            },
            { "msg.modulo.categorias",   "The Categories module is not yet available."         },
            { "lbl.proximamente",        "Coming Soon"                                         },
            // Bitacora — export PDF
            { "err.pdf.sinDatos",        "No data to export."                                  },
            { "lbl.exportarpdf",         "Export PDF"                                          },
            // NuevoPedidoForm — plan info
            { "lbl.ped.infoplan",        "Client: {0}\nPlan: {1}\nGarments currently in use: {2}\nPayment method: {3}\nSince: {4}" },
            { "err.ped.sinplan",         "⚠ {0} has no plan assigned.\nAssign a plan in the Clients module before creating an order." },
            // Users — operation messages
            { "msg.usr.creado",          "User '{0}' [{1}] created successfully."              },
            { "err.usr.selecciona",      "Select a user from the list."                        },
            { "dlg.resetclave.prompt",   "New password for '{0}' (minimum 6 characters):"      },
            { "msg.usr.clave.reseteada", "Password for '{0}' reset successfully."              },
            { "err.usr.sel.bloqueado",   "Select a locked user from the list."                 },
            { "conf.desbloquear.body",   "Unlock the account of '{0}'?"                        },
            { "conf.desbloquear.titulo", "Confirm Unlock"                                       },
            { "msg.usr.desbloqueada",    "Account '{0}' unlocked successfully."                },
            { "conf.resetmasivo.titulo", "Reset all passwords"                                  },
            { "conf.resetmasivo.body",   "This will reset the password of ALL users to:\n\n   {0}\n\nNotify each employee to change it.\n\nConfirm?" },
            { "msg.usr.resetmasivo",     "All passwords have been reset to: {0}"               },
            // GestorPermisos
            { "frm.gestorpermisos",      "Profile and Permission Manager"                        },
            { "lbl.permisos.titulo",     "Profiles and Permissions"                              },
            { "lbl.permisos.rol",        "Role:"                                                 },
            { "btn.permisos.guardar",    "Save changes"                                          },
            { "btn.permisos.cerrar",     "Close"                                                 },
            { "msg.permisos.mostrando",  "Showing permissions for role '{0}'."                   },
            { "msg.permisos.guardados",  "Changes saved: {0} permission(s) assigned, {1} removed." },
            // GestorPermisos — roles, groups and permissions
            { "perm.rol.administrador",             "Administrator"                              },
            { "perm.rol.vendedor",                  "Salesperson"                                },
            { "perm.rol.operadorlogistico",         "Logistics Operator"                         },
            { "perm.rol.supervisor",                "Supervisor"                                 },
            { "perm.rol.controladordestock",        "Stock Controller"                           },
            { "perm.rol.operadordeinventario",      "Inventory Operator"                         },
            { "perm.grp.inventario",                "Inventory"                                  },
            { "perm.grp.sistema",                   "System"                                     },
            { "perm.grp.ventas",                    "Sales"                                      },
            { "perm.pat.gestionarstock",            "Manage Stock"                               },
            { "perm.pat.vercategorias",             "View Categories"                            },
            { "perm.pat.veroutfits",                "View Outfits"                               },
            { "perm.pat.verprendas",                "View Garments"                              },
            { "perm.pat.gestionarusuarios",         "Manage Users"                               },
            { "perm.pat.verauditoria",              "View Audit Log"                             },
            { "perm.pat.gestionarclientes",         "Manage Clients"                             },
            { "perm.pat.gestionarplansuscripciones","Manage Subscription Plans"                  },
            { "perm.pat.realizarventas",            "Process Sales"                              },
            { "perm.pat.verpedidosrealizados",      "View Fulfilled Orders"                      },
            // FormIdiomas
            { "frm.idiomas",             "Language Management"                                   },
            { "lbl.idiomas.titulo",      "System Languages"                                      },
            { "lbl.idiomas.trad",        "Translations for the selected language"                },
            { "btn.idiomas.activar",     "✔ Activate"                                            },
            { "btn.idiomas.desactivar",  "✕ Deactivate"                                          },
            { "btn.idiomas.guardar",     "💾 Save changes"                                       },
            // Backup & Restore
            { "frm.backup",              "Backup & Restore"                                       },
            { "btn.backup.crear",        "Create Backup (.bak)"                                   },
            { "btn.backup.restaurar",    "Restore Backup (.bak)"                                  },
            { "btn.backup.eliminar",     "Delete"                                                 },
            { "btn.backup.externo",      "From file..."                                           },
            { "col.backup.archivo",      "File"                                                   },
            { "col.backup.fecha",        "Date"                                                   },
            { "col.backup.autor",        "Author"                                                 },
            { "col.backup.tamanio",      "Size"                                                   },
            { "lbl.backup.info",         "Note: restore will close active connections and restart the application." },
            { "lbl.backup.ubicacion",    "Backup location:"                                       },
            { "msg.backup.tituloeliminar","Confirm Deletion"                                      },
            { "msg.backup.confirmeliminar","Delete the backup file?\n\"{0}\"\n\nThis action cannot be undone." },
            { "msg.backup.titulorestaura","Confirm Restore"                                       },
            { "msg.backup.confirmrestaura","Restore the database from:\n\"{0}\"?\n\nThis will overwrite all current data\nand restart the application." },
            { "mnu.backup",              "Backup & Restore"                                       },
            { "msg.backup.creadoexito",  "Backup created successfully:\n{0}"                     },
            { "msg.backup.restauradaexito","Database restored successfully.\nThe application will restart." },
            { "msg.backup.restauradatitulo","Restore Successful"                                  },
            { "lbl.backup.sincopias",    "No backups created yet."                               },
            { "lbl.backup.conteo",       "{0} backup(s) available. Most recent: {1}"             },
            // RestauracionForm (DV Integrity)
            { "frm.restauracion",        "System Integrity"                                       },
            { "lbl.rest.titulo",         "System Integrity Compromised"                           },
            { "lbl.rest.subtitulo",      "Discrepancies were found in the check digits. Access is blocked." },
            { "lbl.rest.detalle",        "Error detail:"                                          },
            { "btn.rest.recalcular",     "Recalculate Check Digits"                               },
            { "btn.rest.backup",         "Restore from Backup"                                    },
            { "btn.rest.salir",          "Exit"                                                   },
            // Menu — Dashboard + split Audit Logs
            { "mnu.dashboard",           "Dashboard"                                              },
            { "mnu.bitacora.sistema",    "System Log"                                             },
            { "mnu.bitacora.negocio",    "Business Log"                                           },
            // Dashboard — Control Panel
            { "frm.dashboard",           "Control Panel"                                          },
            { "dash.prendas",            "Garments\navailable"                                    },
            { "dash.clientes",           "Clients\nregistered"                                    },
            { "dash.pedidos",            "Orders\npending"                                        },
            { "dash.backup",             "days without\nbackup"                                   },
            { "dash.btn.refrescar",      "↻ Refresh"                                              },
            { "mnu.reportejornada",      "Daily Report"                                           },
            // Daily Report
            { "frm.reportejornada",      "Daily Report — WardrobeFlow"                            },
            { "rpt.generar",             "Generate"                                               },
            { "rpt.comparar",            "Compare shifts"                                         },
            { "rpt.exportartxt",         "Export TXT"                                             },
            { "rpt.exportarpdf",         "Export PDF"                                             },
            { "rpt.fecha",               "Shift:"                                                 },
            { "rpt.fecha2",              "Compare with:"                                          },
            { "rpt.kpi.prendas",         "Available garments"                                     },
            { "rpt.kpi.clientes",        "Registered clients"                                     },
            { "rpt.kpi.eventos",         "Events today"                                           },
            { "rpt.kpi.backup",          "days since backup"                                      },
            { "rpt.sinDatos",            "No events recorded for the selected shift."             },
            { "rpt.limpiar",             "Clear"                                                  },
            { "rpt.subtitulo",           "Business events per day with TXT export"               },
            // Daily Report — generated report body text
            { "rpt.txt.titulo",          "DAILY REPORT"                                          },
            { "rpt.txt.resumen",         "SYSTEM SUMMARY"                                        },
            { "rpt.txt.prendas",         "Available garments"                                    },
            { "rpt.txt.clientes",        "Registered clients"                                    },
            { "rpt.txt.diassinbkp",      "Days without backup"                                   },
            { "rpt.txt.sinbackups",      "No backups"                                            },
            { "rpt.txt.eventos",         "BUSINESS EVENTS OF THE DAY"                            },
            { "rpt.txt.sinevt",          "(no events recorded for this shift)"                   },
            { "rpt.txt.usuario",         "User"                                                  },
            { "rpt.txt.cliente",         "Client"                                                },
            { "rpt.txt.totalevt",        "TOTAL EVENTS"                                         },
            { "rpt.txt.generado",        "Generated"                                             },
            { "rpt.txt.comparacion",     "SHIFT COMPARISON"                                      },
            { "rpt.txt.jornada",         "SHIFT"                                                 },
            { "rpt.txt.sinevtjorn",      "(no events recorded for this shift)"                   },
            { "rpt.txt.comparfinal",     "FINAL SUMMARY"                                         },
            { "rpt.txt.fecha",           "Date"                                                  },
            { "rpt.txt.eventostot",      "Total events"                                          },
            { "rpt.txt.masmasa",         "had more activity"                                     },
            { "rpt.txt.ninguna",         "Neither shift had any events recorded."                },
            { "rpt.txt.iguales",         "Both shifts had the same number of events."            },
            { "rpt.txt.rptoegenerado",   "Report generated"                                      },
            { "rpt.txt.compgenerada",    "Comparison generated"                                  },
            { "rpt.txt.impresionenv",    "Print sent"                                            },
            { "bit.pdf.titulosistema",   "System Log — WardrobeFlow"                            },
            { "bit.pdf.titulonegocio",   "Business Log — WardrobeFlow"                         },
            { "bit.pdf.pagina",          "WardrobeFlow — Page {0}"                              },
            { "bit.pdf.vistaprevia",     "Print Preview"                                         },
            { "dash.backup.hoy",         "Today"                                                 },
            { "dash.sesion.iniciada",    "Session started:"                                      },
            // Dashboard — recent activity and mini-stats
            { "dash.actividad.titulo",  "Recent activity"                                       },
            { "dash.stats.titulo",      "Event summary"                                         },
            { "dash.col.fecha",         "Date"                                                  },
            { "dash.col.evento",        "Event"                                                 },
            { "dash.col.usuario",       "User"                                                  },
            // Dashboard — backup notices and config dialog
            { "dash.aviso.sinbackup",   "⚠  No backups. Generate one from Administration → Backup."        },
            { "dash.aviso.vencido",     "⚠  {0} day(s) without backup — reminder every {1} days."          },
            { "dash.cfg.titulo",        "Backup Reminder"                                                   },
            { "dash.cfg.recada",        "Remind me every:"                                                  },
            { "dash.cfg.dias",          "days"                                                              },
            { "dash.cfg.guardar",       "Save"                                                              },
            // Daily Report — export menus and dialogs
            { "rpt.menu.guardartxt",    "Save as .TXT"                                                      },
            { "rpt.menu.imprimir",      "Print / Export PDF"                                                },
            { "rpt.menu.guardarcmp",    "Save comparison as .TXT"                                           },
            { "rpt.dlg.guardartxt",     "Save as TXT"                                                       },
            { "rpt.dlg.exito.titulo",   "Success"                                                           },
            { "rpt.dlg.exito.msg",      "File saved:\n{0}"                                                  },
            // User Change History (T06)
            { "frm.historialusr",        "User Change History"                                    },
            { "lbl.ver.usuario",         "User:"                                                  },
            { "btn.ver.cargar",          "Load"                                                   },
            { "btn.ver.restaurar",       "Restore Selected Version"                               },
            { "col.ver.id",              "ID"                                                     },
            { "col.ver.fecha",           "Date"                                                   },
            { "col.ver.actor",           "Modified by"                                            },
            { "col.ver.detalle",         "Detail"                                                 },
            { "col.ver.estado",          "Status"                                                 },
            { "mnu.historialusr",        "Change History"                                         },
            // ConfirmarAdmin messages
            { "msg.confirmar.vacio",         "Enter username and password."                                               },
            { "msg.confirmar.invalido",      "Invalid credentials or the user is not an Administrator."                  },
            // VersionHistorial messages
            { "msg.historial.sinseleccion",  "Select a version from the grid."                                           },
            { "msg.historial.atencion",      "Attention"                                                                 },
            { "msg.historial.confirmar",     "Restore user '{0}' to the state from {1}?\n\nSnapshot detail: {2}\n\nThis action is reversible (a new snapshot is saved before restoring)." },
            { "msg.historial.restaurado",    "Version restored successfully."                                            },
            // Clients — operation and deletion confirmation
            { "msg.cli.registrado",          "Client '{0}' registered successfully."                                    },
            { "msg.cli.actualizado",         "Client '{0}' updated."                                                    },
            { "msg.cli.eliminado",           "Client '{0}' deleted."                                                    },
            { "conf.baja.cli.msg",           "Deactivate {0} (DNI {1})?\n\nThis action cannot be undone."              },
            { "conf.baja.cli.titulo",        "Confirm Deletion"                                                          },
            { "err.cli.errorplanes",         "— Error loading plans —"                                                  },
            // Plans — operation and confirmations
            { "msg.planes.creado",           "Plan '{0}' created."                                                      },
            { "msg.planes.actualizado",      "Plan '{0}' updated."                                                      },
            { "msg.planes.desactivado",      "Plan '{0}' deactivated."                                                  },
            { "msg.planes.reactivado",       "Plan '{0}' reactivated."                                                  },
            { "conf.planes.desat.msg",       "Deactivate plan '{0}'?\n\nExisting clients with this plan will not be affected." },
            { "conf.planes.desat.tit",       "Confirm Deactivation"                                                     },
            { "conf.planes.act.msg",         "Reactivate plan '{0}'?\n\nThe plan will be available for new subscriptions again." },
            { "conf.planes.act.tit",         "Confirm Activation"                                                       },
            // Language management — messages
            { "msg.idiomas.activado",        "Language activated."                                                       },
            { "msg.idiomas.desactivado",     "Language deactivated."                                                     },
            { "conf.idiomas.desactivar",     "Deactivate this language? Users will no longer be able to select it."    },
            { "conf.idiomas.titulo",         "Confirm"                                                                   },
            // Order history — restore
            { "msg.hist.restaurado",         "Order #{0} successfully restored."                                        },
            { "conf.hist.restaurar.msg",     "Restore order #{0} to the state before '{1}' (op. #{2})?\n\n⚠ Note: this operation modifies the Order status in the database.\nThe status of associated Garments is NOT automatically reverted.\n\nConfirm?" },
            // Users — DV
            { "msg.usr.dvrecalculados",      "DVH and DVV successfully recalculated for all users."                     },
            { "msg.usr.errordv",             "Error recalculating DV: {0}"                                              },
            // RestauracionForm — additional messages
            { "msg.rest.dvexito",            "Check digits successfully recalculated.\nYou can now log in to the system." },
            { "msg.rest.dvtitulo",           "Integrity Restored"                                                        },
            { "conf.rest.sobreescribir",     "Are you sure? This operation will overwrite all current data and restart the application." },
            { "msg.rest.errorrecalcular",    "Error recalculating: {0}"                                                  },
            { "msg.rest.errorrestaurar",     "Error restoring: {0}"                                                      },
            // BackupForm — catch errors
            { "msg.backup.errorgenerar",     "Error creating backup:\n{0}"                                              },
            { "msg.backup.erroreliminar",    "Error deleting:\n{0}"                                                      },
            { "msg.backup.errorrestaurar",   "Error restoring:\n{0}"                                                     },
            // VersionHistorialForm — catch errors
            { "msg.historial.errorcargar",   "Error loading history:\n{0}"                                              },
            { "msg.historial.errorrestaur",  "Error restoring version:\n{0}"                                            },
            // Generic error title
            { "msg.error.titulo",            "Error"                                                                     },
        });

        // ── Diccionario Русский (RU) ──────────────────────────────────────────

        private static readonly IDictionary<string, Traduccion> _ru =
            Construir(new Dictionary<string, string>
        {
            // Вход
            { "frm.login",        "WardrobeFlow — Вход"                    },
            { "lbl.usuario",      "Пользователь"                           },
            { "lbl.contrasena",   "Пароль"                                 },
            { "btn.ingresar",     "Войти"                                  },
            { "btn.salir",        "Выйти"                                  },
            { "lnk.olvide",       "Забыли пароль?"                         },
            { "lbl.iniciarsesion","Войти в систему"                         },
            // Панель языка
            { "lbl.idioma",       "Язык:"                                  },
            // Вход — подзаголовок
            { "lbl.subtitulo",    "ПОРТАЛ СОТРУДНИКОВ"                     },
            { "lbl.bienvenido",   "Добро пожаловать"                       },
            { "lbl.credenciales", "Введите данные для входа"               },
            { "lbl.divider",      "или"                                     },
            { "lbl.brand.desc",   "Безопасный доступ\nко всем модулям системы." },
            // Главное меню
            { "mnu.perfil",       "Профиль"                                },
            { "mnu.inventario",   "Склад"                                  },
            { "mnu.prendas",      "Одежда"                                 },
            { "mnu.ventas",       "Продажи"                                },
            { "mnu.clientes",     "Клиенты"                                },
            { "mnu.planes",       "Тарифные планы"                         },
            { "mnu.pedidosventa", "Заказы на продажу"                      },
            { "mnu.pedidosreal",  "Выполненные заказы"                     },
            { "mnu.administrar",  "Администрирование"                      },
            { "mnu.usuarios",     "Пользователи"                           },
            { "mnu.perfiles",     "Профили и права"                        },
            { "mnu.bitacora",     "Журнал аудита"                          },
            { "mnu.cerrarsesion", "Выйти из системы"                       },
            { "mnu.idiomas",      "Управление языками"                     },
            // Клиенты
            { "frm.clientes",     "Управление клиентами"                   },
            { "lbl.buscar",       "Поиск:"                                 },
            { "btn.nuevocliente", "+ Новый клиент"                         },
            { "btn.editar",       "✎ Изменить"                             },
            { "btn.darbaja",      "✕ Удалить"                              },
            // Одежда
            { "frm.prendas",      "Каталог одежды"                         },
            { "lbl.estado",       "Состояние:"                             },
            { "btn.nuevaprenda",  "+ Новая одежда"                         },
            { "btn.cambiarestado","⇄ Состояние"                            },
            { "lbl.clienteenuso", "Клиент (в использовании):"              },
            // Пользователи
            { "frm.gestion",      "Управление пользователями"              },
            { "lbl.nuevousuario", "Новый пользователь"                     },
            { "lbl.nombreusuario","Имя пользователя:"                      },
            { "lbl.perfilrol",    "Профиль (роль):"                        },
            { "btn.agregar",      "Добавить пользователя"                  },
            { "lbl.resettitulo",  "Сброс пароля"                           },
            { "lbl.resetinfo",    "Выберите пользователя\nиз списка и нажмите:" },
            { "btn.resetclave",   "Сбросить пароль"                        },
            { "lbl.desbloqtitulo","Разблокировать аккаунт"                 },
            { "lbl.desbloqinfo",  "Выберите заблокированного\nпользователя и нажмите:" },
            { "btn.desbloquear",  "Разблокировать аккаунт"                 },
            { "lbl.listatitulo",  "Зарегистрированные пользователи"        },
            // Тарифные планы
            { "frm.planes",       "Тарифные планы"                         },
            { "lbl.nuevopla",     "Новый план"                             },
            { "lbl.nombreplan",   "Название плана *"                       },
            { "lbl.limiteprendas","Лимит одежды *"                         },
            { "lbl.preciomensual","Цена в месяц ($) *"                     },
            { "btn.guardarplan",  "Сохранить план"                         },
            { "btn.limpiar",      "Очистить / Новый"                       },
            { "lbl.acciones",     "Действия над выбранным планом"          },
            { "btn.desactivar",   "Деактивировать план"                    },
            { "btn.activar",      "Активировать план"                      },
            { "lbl.planesreg",    "Зарегистрированные планы"               },
            // Журнал аудита
            { "frm.bitacora",            "Аудит — Журналы системы"                                    },
            { "frm.bitacora.subtitulo", "Журнал системных событий и бизнес-операций"                 },
            { "tab.sistema",      "🔐  Системный журнал"                   },
            { "tab.negocio",      "📦  Бизнес-журнал"                      },
            { "lbl.ultimos",      "Последние"                              },
            { "lbl.dias",         "дней  (0 = все)"                        },
            { "btn.ver",          "Просмотр"                               },
            { "lbl.usuarioid",    "ID пользователя:"                       },
            { "lbl.actividad",    "Активность:"                            },
            { "lbl.criticidad",   "Критичность:"                           },
            { "btn.buscar",       "Поиск"                                  },
            { "btn.limpiarfiltro","Очистить"                               },
            { "btn.exportar",     "Экспорт CSV"                            },
            { "lbl.tipoevento",   "Тип события:"                           },
            { "lbl.idpedido",     "ID заказа:"                             },
            { "lbl.idcliente",    "ID клиента:"                            },
            // Заказы на продажу
            { "frm.pedidosventa",  "Заказы на продажу"                    },
            { "btn.nuevopedido",   "+ Новый заказ"                         },
            { "btn.cancelarpedido","✕ Отменить"                            },
            { "btn.descancelar",   "↩ Восстановить"                        },
            { "lbl.prendaspedido", "Одежда выбранного заказа"              },
            // Выполненные заказы
            { "frm.pedidosreal2",  "Выдача заказов"                        },
            { "btn.despachar",     "📦 Отправить"                          },
            { "btn.entregado",     "✓ Отметить доставленным"               },
            { "btn.vernotificacion","✉ Посмотреть уведомление"             },
            { "btn.devolucion",    "↩ Зарегистрировать возврат"            },
            { "lbl.detallepedido", "Детали выбранного заказа"              },
            // Выполненные заказы — столбцы таблицы
            { "col.ped.urgencia",  "Срочность"                             },
            { "col.ped.fecha",     "Дата"                                  },
            { "col.ped.cliente",   "Клиент"                                },
            { "col.ped.vendedor",  "Продавец"                              },
            { "col.ped.prendas",   "Одежда"                                },
            { "col.ped.estado",    "Статус"                                },
            { "col.ped.despacho",  "Отправка"                              },
            { "col.ped.entrega",   "Доставка"                              },
            { "col.ped.motivo",    "Причина"                               },
            { "lbl.ped.seleccionado", "Заказ #{0} — {1} — {2}"            },
            // Выполненные заказы — значения срочности
            { "urg.urgente",       "Срочно"                                },
            { "urg.normal",        "Норма"                                 },
            { "urg.reciente",      "Недавно"                               },
            // Выполненные заказы — значения статуса
            { "est.pendiente",     "Ожидание"                              },
            { "est.despachado",    "Отправлен"                             },
            { "est.entregado",     "Доставлен"                             },
            { "est.cancelado",     "Отменён"                               },
            // Системный журнал — столбцы таблицы
            { "col.bit.id",        "Id"                                    },
            { "col.bit.fecha",     "Дата"                                  },
            { "col.bit.usuario",   "Пользователь"                          },
            { "col.bit.modulo",    "Модуль"                                },
            { "col.bit.actividad", "Активность"                            },
            { "col.bit.detalle",   "Детали"                                },
            { "col.bit.criticidad","Критичность"                           },
            { "col.bit.ip",        "IP"                                    },
            // Бизнес-журнал — столбцы таблицы
            { "col.neg.idevento",  "Id события"                            },
            { "col.neg.fecha",     "Дата"                                  },
            { "col.neg.tipo",      "Тип"                                   },
            { "col.neg.usuario",   "Пользователь"                          },
            { "col.neg.cliente",   "Клиент"                                },
            { "col.neg.idpedido",  "Id заказа"                             },
            { "col.neg.idprenda",  "Id одежды"                             },
            { "col.neg.idcliente", "Id клиента"                            },
            { "col.neg.desc",      "Описание"                              },
            // Журнал — статистика критичности
            { "stat.ninguno",      "Нет"                                   },
            { "stat.baja",         "Низкая"                                },
            { "stat.media",       "Средняя"                               },
            { "stat.alta",         "Высокая"                               },
            { "stat.intlogin",     "Попытки входа"                         },
            { "stat.recupclave",   "Сброс пароля"                          },
            { "stat.bloqueos",     "Блокировки"                            },
            { "stat.sindatos",     "Нет данных критичности"                },
            // История изменений заказа
            { "frm.historial",        "История изменений заказа"           },
            { "lbl.hist.pedido",      "Заказ #"                            },
            { "lbl.hist.filtros",     "Фильтры"                            },
            { "combo.hist.todas",     "— Все —"                            },
            { "lbl.hist.desde",       "С:"                                 },
            { "lbl.hist.hasta",       "По:"                                },
            { "lbl.hist.accion",      "Действие:"                          },
            { "btn.hist.buscar",      "🔍 Поиск"                           },
            { "btn.hist.restaurar",   "⟲ Восстановить"                     },
            { "btn.hist.cerrar",      "Закрыть"                            },
            { "col.hist.op",          "Op."                                },
            { "col.hist.fecha",       "Дата"                               },
            { "col.hist.usuario",     "Пользователь"                       },
            { "col.hist.accion",      "Действие"                           },
            { "col.hist.campo",       "Поле"                               },
            { "col.hist.anterior",    "Было"                               },
            { "col.hist.nuevo",       "Стало"                              },
            // Диалог выхода
            { "dlg.cerrarsesion.titulo", "Выход из системы"               },
            { "dlg.cerrarsesion.msg", "Вы уверены, что хотите выйти?"     },
            { "btn.si",               "Да"                                 },
            { "btn.no",               "Нет"                                },
            // Пользователи — кнопка обновления
            { "btn.refrescar",        "↻ Обновить список"                  },
            // Одежда — фильтр состояния
            { "combo.prenda.todos",   "Все"                                },
            { "prenda.disponible",    "Доступна"                           },
            { "prenda.enuso",         "В использовании"                    },
            { "prenda.enlimpieza",    "На чистке"                          },
            { "prenda.baja",          "Выбыла"                             },
            // Одежда — столбцы таблицы
            { "col.prenda.nombre",    "Название"                           },
            { "col.prenda.categoria", "Категория"                          },
            { "col.prenda.talle",     "Размер"                             },
            { "col.prenda.color",     "Цвет"                               },
            { "col.prenda.estado",    "Статус"                             },
            { "col.prenda.cliente",   "Клиент"                             },
            { "col.prenda.alta",      "Добавлено"                          },
            // Одежда — сообщения
            { "msg.prenda.conteo",    "Показано {0} из {1}"               },
            { "msg.prenda.cargadas",  "{0} вещей в каталоге."             },
            // Планы — столбцы таблицы
            { "col.plan.nombre",      "Название"                           },
            { "col.plan.prendas",     "Одежда"                             },
            { "col.plan.precio",      "Цена"                               },
            { "col.plan.estado",      "Статус"                             },
            { "plan.activo",          "Активен"                            },
            { "plan.inactivo",        "Неактивен"                          },
            { "msg.planes.cargados",  "{0} план(ов) загружено."           },
            // ClienteForm
            { "frm.nuevocliente",     "Новый клиент"                       },
            { "frm.editarcliente",    "Редактировать клиента"              },
            { "lbl.cli.nombre",       "Имя *"                              },
            { "lbl.cli.apellido",     "Фамилия *"                          },
            { "lbl.cli.dni",          "ИНН * (7-8 цифр)"                  },
            { "lbl.cli.email",        "Email"                              },
            { "lbl.cli.metodopago",   "Способ оплаты *"                    },
            { "lbl.cli.plan",         "Тарифный план"                      },
            { "combo.cli.sinplan",    "— Без плана —"                      },
            { "btn.registrar.cliente","Зарегистрировать"                   },
            { "btn.guardar.cambios",  "Сохранить изменения"                },
            { "btn.cancelar",         "Отмена"                             },
            // Клиенты — столбцы таблицы
            { "col.cli.nombre",       "Имя"                                },
            { "col.cli.apellido",     "Фамилия"                            },
            { "col.cli.dni",          "ИНН"                                },
            { "col.cli.email",        "Email"                              },
            { "col.cli.plan",         "План"                               },
            { "col.cli.prendas",      "Одежда"                             },
            { "col.cli.metodopago",   "Способ оплаты"                      },
            { "col.cli.alta",         "Добавлен"                           },
            // Клиенты — сообщения
            { "lbl.sinplan",          "Без плана"                          },
            { "msg.cli.conteo",       "Показано {0} из {1}"                },
            { "msg.cli.cargados",     "{0} клиент(ов) зарегистрировано."   },
            // Пользователи — столбцы таблицы
            { "col.usr.username",     "Пользователь"                       },
            { "col.usr.perfil",       "Роль"                               },
            { "col.usr.estado",       "Статус"                             },
            // Пользователи — значения и сообщения
            { "usr.activo",           "Активен"                            },
            { "usr.bloqueada",        "Заблокирован"                       },
            { "msg.usr.cargados",     "{0} пользователей зарегистрировано." },
            // Форма восстановления пароля
            { "frm.olvidepass",       "Восстановление пароля"              },
            { "lbl.recup.titulo",     "Восстановление пароля"              },
            { "lbl.recup.desc",       "Введите имя пользователя. Администратор\nсможет сбросить ваш пароль через систему." },
            { "lbl.recup.usuario",    "Имя пользователя:"                  },
            { "btn.enviar.solicitud", "Отправить запрос"                   },
            // Восстановление пароля — сообщения
            { "err.recup.nousername",    "Введите имя пользователя."                                                      },
            { "err.recup.nousuario",     "Пользователь '{0}' не найден.\nПроверьте правильность написания имени."        },
            { "msg.recup.exito",         "Пользователь '{0}' найден.\nОбратитесь к администратору для сброса\nпароля через Управление → Пользователи." },
            { "err.recup.verificar",     "Ошибка при проверке пользователя: {0}"                                         },
            // Детали заказа — столбцы одежды
            { "col.det.prenda",       "Одежда"                             },
            // PrendaForm — заголовки и метки
            { "frm.nuevaprenda",      "Новая одежда"                       },
            { "frm.editarprenda",     "Редактировать одежду"               },
            { "lbl.prenda.nombre",    "Название *"                         },
            { "lbl.prenda.descrip",   "Описание"                           },
            { "lbl.prenda.talle",     "Размер *"                           },
            { "lbl.prenda.color",     "Цвет"                               },
            { "lbl.prenda.categoria", "Категория *"                        },
            { "btn.agregar.prenda",   "Добавить одежду"                    },
            // Форма нового заказа на продажу
            { "frm.nuevopedido",      "Новый заказ на продажу"              },
            { "paso1.texto",          "Шаг 1 из 2 — Выбрать клиента"       },
            { "paso2.texto",          "Шаг 2 из 2 — Выбрать одежду"        },
            { "lbl.ped.selcliente",   "Выберите клиента для этого заказа:"  },
            { "combo.ped.placeholder","— Выберите клиента —"                },
            { "lbl.ped.selprendas",   "Выберите одежду для включения в заказ (флажок):" },
            { "btn.siguiente",        "Далее →"                             },
            { "btn.volver",           "← Назад"                             },
            { "btn.confirmar.pedido", "✓ Подтвердить заказ"                 },
            { "btn.procesando",       "Обработка..."                        },
            // История изменений — метки действий
            { "accion.crear",         "Создать"                             },
            { "accion.despachar",     "Отправить"                           },
            { "accion.entregar",      "Доставить"                           },
            { "accion.cancelar",      "Отменить"                            },
            { "accion.descancelar",   "Снять отмену"                        },
            { "accion.devolucion",    "Возврат"                             },
            { "accion.restaurar",     "Восстановить"                        },
            // Журнал аудита — combo критичности
            { "crit.todas",           "Все"                                 },
            { "crit.ninguno",         "Нет (0)"                             },
            { "crit.baja",            "Низкая (1)"                          },
            { "crit.media",           "Средняя (2)"                         },
            { "crit.alta",            "Высокая (3)"                         },
            { "crit.intlogin",        "Попытки входа (4)"                   },
            { "crit.recupclave",      "Сброс пароля (5)"                    },
            { "crit.bloqueos",        "Блокировки аккаунта (6)"             },
            // Журнал аудита — кнопка экспорта PDF
            { "btn.exportar.pdf",     "📄 Экспорт PDF"                      },
            // Заказы на продажу / Выполненные заказы — счётчики и сообщения
            { "msg.ped.conteo",       "{0} заказ(ов)"                                           },
            { "msg.ped.cargados",     "{0} заказ(ов) загружено."                                },
            { "msg.ped.creado",       "Заказ #{0} создан. Статус: Ожидание."                    },
            { "msg.ped.cancelado",    "Заказ #{0} отменён. Вещи освобождены."                   },
            { "msg.ped.reactivado",   "Заказ #{0} реактивирован — статус: Ожидание."            },
            { "msg.ped.ensistema",    "{0} заказ(ов) в системе."                                },
            { "msg.ped.mostrando",    "Показано {0} из {1}"                                     },
            { "msg.ped.despachado",   "Заказ #{0} успешно отправлен."                           },
            { "msg.ped.entregado",    "Заказ #{0} отмечен как доставленный."                    },
            { "msg.ped.devolucion",   "Возврат зарегистрирован — {0} вещей отправлено на чистку."},
            // Кнопка Истории (динамическая)
            { "btn.historial",        "📋 История"                                              },
            // Встроенный диалог PedirTexto
            { "btn.aceptar",          "Принять"                                                 },
            { "dlg.cancelped.titulo", "Причина отмены"                                          },
            { "msg.cancelped.req",    "Для отмены требуется причина."                           },
            { "lbl.motivo",           "Причина:"                                                },
            // Подтверждения заказов
            { "conf.cancelped.titulo",   "Подтвердить отмену"                                   },
            { "conf.cancelped.body",     "Отменить заказ #{0} для {1}?\n\nПричина: {2}\n\nВещи вернутся в статус Доступна." },
            { "conf.descancelar.titulo", "Подтвердить восстановление"                           },
            { "conf.descancelar.body",   "Восстановить заказ #{0} для {1}?\n\nДоступность оригинальных вещей будет проверена\nи заказ вернётся в статус Ожидание." },
            { "conf.despachar.titulo",   "Подтвердить отправку"                                 },
            { "conf.despachar.body",     "Отправить заказ #{0}?\n\nКлиент: {1}\nВещей: {2}\n\nЗаказ перейдёт в статус Отправлен." },
            { "conf.entrega.titulo",     "Подтвердить доставку"                                 },
            { "conf.entrega.body",       "Подтвердить доставку заказа #{0} клиенту {1}?"        },
            { "conf.devolucion.titulo",  "Подтвердить возврат"                                  },
            { "conf.devolucion.body",    "Зарегистрировать возврат заказа #{0}?\n\nКлиент: {1}\nВещей: {2}\n\nВещи перейдут в статус На чистке." },
            // Одежда — сообщения об операциях
            { "msg.prenda.agregada",     "Вещь '{0}' добавлена в каталог."                      },
            { "msg.prenda.actualizada",  "Вещь '{0}' обновлена."                                },
            { "msg.prenda.estadoact",    "Статус '{0}' обновлён на {1}."                        },
            { "opt.enviarlimpieza",      "Отправить на чистку"                                  },
            { "opt.darbaja",             "Списать"                                              },
            { "opt.marcardisp",          "Пометить доступной"                                   },
            { "err.prenda.enuso",        "Нельзя изменить статус: вещь используется клиентом."  },
            { "err.prenda.baja",         "Вещь списана и не может быть реактивирована."         },
            // CambioEstadoDialog
            { "lbl.cambioest.info",      "Вещь: {0}  —  Текущий статус: {1}"                   },
            { "msg.cambioest.selecciona","Выберите вариант."                                     },
            { "msg.cambioest.bajairrev", "Списание необратимо. Подтвердить?"                    },
            { "conf.baja.titulo",        "Списать"                                              },
            // ResetClaveDialog
            { "frm.resetclave",          "Сброс пароля"                                         },
            { "lbl.nueva.clave",         "Новый пароль (мин. 6 символов):"                      },
            { "lbl.confirmar.clave",     "Подтвердить пароль:"                                  },
            { "btn.confirmar.reset",     "Подтвердить сброс"                                    },
            { "err.clave.longitud",      "Пароль должен содержать не менее 6 символов."         },
            { "err.clave.nomatch",       "Пароли не совпадают."                                 },
            // Уведомление об отправке (BtnVerNotificacion)
            { "notif.titulo",            "УВЕДОМЛЕНИЕ О ЗАКАЗЕ"                                 },
            { "notif.numero",            "Заказ #:"                                             },
            { "notif.msgbox.titulo",     "Уведомление — Заказ #{0}"                             },
            // CambioEstadoDialog — элементы из Designer
            { "frm.cambioestado",        "Изменить статус вещи"                                 },
            { "lbl.nuevoestado",         "Новый статус:"                                        },
            { "btn.confirmar.cambio",    "Подтвердить изменение"                                },
            // Второстепенные проверки
            { "err.ped.sinprendas",      "Выберите хотя бы одну вещь."                          },
            { "err.hist.restaurar",      "Выберите строку истории для восстановления."           },
            { "err.usr.sinperfil",       "Выберите профиль/роль."                               },
            // Пользователи — проверки имени пользователя
            { "err.usr.nombre.req",      "Имя пользователя обязательно."                        },
            { "err.usr.nombre.longitud", "Имя пользователя должно содержать минимум 3 символа." },
            // ClienteForm — проверки
            { "err.cli.dni.numeros",     "✗ DNI может содержать только цифры."                  },
            // PedidosRealizados — заголовок детали
            { "lbl.ped.detalletitulo",   "Заказ #{0}  ·  {1}  ·  {2}  ·  {3} {4}"              },
            // Тип события — комбо Журнала
            { "tevt.todos",              "Все"                                                    },
            { "tevt.venta",              "Продажа"                                               },
            { "tevt.cancelacion",        "Отмена"                                                },
            { "tevt.despacho",           "Отправка"                                              },
            { "tevt.entrega",            "Доставка"                                              },
            { "tevt.altaprenda",         "Новая вещь"                                            },
            { "tevt.modprenda",          "Изменение вещи"                                        },
            { "tevt.cambiostprenda",     "Изменение статуса вещи"                                },
            { "tevt.altacliente",        "Новый клиент"                                          },
            { "tevt.modcliente",         "Изменение клиента"                                     },
            { "tevt.bajacliente",        "Удаление клиента"                                      },
            // Планы — заголовок формы при редактировании
            { "lbl.editplan",            "Редактировать план"                                    },
            // Журнал — подписи результатов
            { "msg.bit.registros",       "  {0} запись(ей)"                                     },
            { "msg.bit.ultimos",         "последние {0} дней"                                   },
            { "msg.bit.todos",           "все записи"                                            },
            // NotificacionDespachoForm — UI
            { "notif.frm.titulo",        "Уведомление — Заказ #{0}"                             },
            { "notif.header.entregado",  "✓  Заказ #{0} — ДОСТАВЛЕН"                           },
            { "notif.header.despachado", "📦  Заказ #{0} — ОТПРАВЛЕН"                           },
            { "btn.copiar.porta",        "Копировать в буфер обмена"                             },
            { "btn.copiado",             "✓ Скопировано"                                       },
            // Меню — недоступные модули
            { "msg.modulo.outfits",      "Модуль Наряды ещё недоступен."                        },
            { "msg.modulo.categorias",   "Модуль Категории ещё недоступен."                     },
            { "lbl.proximamente",        "Скоро"                                                },
            // Журнал — экспорт PDF
            { "err.pdf.sinDatos",        "Нет данных для экспорта."                             },
            { "lbl.exportarpdf",         "Экспорт PDF"                                          },
            // NuevoPedidoForm — информация о плане
            { "lbl.ped.infoplan",        "Клиент: {0}\nПлан: {1}\nВещей в использовании: {2}\nМетод оплаты: {3}\nС: {4}" },
            { "err.ped.sinplan",         "⚠ У {0} не назначен план.\nНазначьте план в модуле Клиентов перед созданием заказа." },
            // Пользователи — сообщения об операциях
            { "msg.usr.creado",          "Пользователь '{0}' [{1}] успешно создан."             },
            { "err.usr.selecciona",      "Выберите пользователя из списка."                     },
            { "dlg.resetclave.prompt",   "Новый пароль для '{0}' (минимум 6 символов):"         },
            { "msg.usr.clave.reseteada", "Пароль для '{0}' успешно сброшен."                    },
            { "err.usr.sel.bloqueado",   "Выберите заблокированного пользователя из списка."    },
            { "conf.desbloquear.body",   "Разблокировать аккаунт '{0}'?"                        },
            { "conf.desbloquear.titulo", "Подтвердить разблокировку"                            },
            { "msg.usr.desbloqueada",    "Аккаунт '{0}' разблокирован."                        },
            { "conf.resetmasivo.titulo", "Сбросить все пароли"                                  },
            { "conf.resetmasivo.body",   "Все пароли будут сброшены к:\n\n   {0}\n\nСообщите каждому сотруднику о необходимости их изменить.\n\nПодтвердить?" },
            { "msg.usr.resetmasivo",     "Все пароли сброшены к: {0}"                           },
            // GestorPermisos
            { "frm.gestorpermisos",      "Менеджер профилей — права"                             },
            { "lbl.permisos.titulo",     "Профили и права"                                       },
            { "lbl.permisos.rol",        "Роль:"                                                 },
            { "btn.permisos.guardar",    "Сохранить"                                             },
            { "btn.permisos.cerrar",     "Закрыть"                                               },
            { "msg.permisos.mostrando",  "Показ прав роли '{0}'."                               },
            { "msg.permisos.guardados",  "Изменения сохранены: назначено {0}, убрано {1}."      },
            // GestorPermisos — роли, группы и права
            { "perm.rol.administrador",             "Администратор"                              },
            { "perm.rol.vendedor",                  "Продавец"                                   },
            { "perm.rol.operadorlogistico",         "Логист"                                     },
            { "perm.rol.supervisor",                "Супервайзер"                                },
            { "perm.rol.controladordestock",        "Контролёр склада"                           },
            { "perm.rol.operadordeinventario",      "Оператор склада"                            },
            { "perm.grp.inventario",                "Склад"                                      },
            { "perm.grp.sistema",                   "Система"                                    },
            { "perm.grp.ventas",                    "Продажи"                                    },
            { "perm.pat.gestionarstock",            "Управление запасами"                        },
            { "perm.pat.vercategorias",             "Просмотр категорий"                         },
            { "perm.pat.veroutfits",                "Просмотр нарядов"                           },
            { "perm.pat.verprendas",                "Просмотр одежды"                            },
            { "perm.pat.gestionarusuarios",         "Управление пользователями"                  },
            { "perm.pat.verauditoria",              "Просмотр журнала аудита"                    },
            { "perm.pat.gestionarclientes",         "Управление клиентами"                       },
            { "perm.pat.gestionarplansuscripciones","Управление тарифными планами"               },
            { "perm.pat.realizarventas",            "Оформление продаж"                          },
            { "perm.pat.verpedidosrealizados",      "Просмотр выполненных заказов"               },
            // FormIdiomas
            { "frm.idiomas",             "Управление языками"                                    },
            { "lbl.idiomas.titulo",      "Языки системы"                                         },
            { "lbl.idiomas.trad",        "Переводы для выбранного языка"                         },
            { "btn.idiomas.activar",     "✔ Включить"                                            },
            { "btn.idiomas.desactivar",  "✕ Отключить"                                           },
            { "btn.idiomas.guardar",     "💾 Сохранить"                                          },
            // Резервное копирование
            { "frm.backup",              "Резервное копирование"                                  },
            { "btn.backup.crear",        "Создать резервную копию (.bak)"                         },
            { "btn.backup.restaurar",    "Восстановить из копии (.bak)"                           },
            { "btn.backup.eliminar",     "Удалить"                                                },
            { "btn.backup.externo",      "Из файла..."                                            },
            { "col.backup.archivo",      "Файл"                                                   },
            { "col.backup.fecha",        "Дата"                                                   },
            { "col.backup.autor",        "Автор"                                                  },
            { "col.backup.tamanio",      "Размер"                                                 },
            { "lbl.backup.info",         "Примечание: восстановление закроет активные соединения и перезапустит приложение." },
            { "lbl.backup.ubicacion",    "Расположение копий:"                                   },
            { "msg.backup.tituloeliminar","Подтверждение удаления"                               },
            { "msg.backup.confirmeliminar","Удалить резервную копию?\n\"{0}\"\n\nЭто действие нельзя отменить." },
            { "msg.backup.titulorestaura","Подтверждение восстановления"                         },
            { "msg.backup.confirmrestaura","Восстановить базу данных из:\n\"{0}\"?\n\nЭто перезапишет все текущие данные\nи перезапустит приложение." },
            { "mnu.backup",              "Резервная копия"                                        },
            { "msg.backup.creadoexito",  "Резервная копия создана успешно:\n{0}"                 },
            { "msg.backup.restauradaexito","База данных успешно восстановлена.\nПриложение перезапустится." },
            { "msg.backup.restauradatitulo","Восстановление выполнено"                            },
            { "lbl.backup.sincopias",    "Резервных копий нет."                                  },
            { "lbl.backup.conteo",       "{0} резервная(ых) копия(й). Последняя: {1}"            },
            // RestauracionForm (Целостность ЦК)
            { "frm.restauracion",        "Целостность системы"                                    },
            { "lbl.rest.titulo",         "Целостность системы нарушена"                           },
            { "lbl.rest.subtitulo",      "Обнаружены расхождения в контрольных цифрах. Доступ заблокирован." },
            { "lbl.rest.detalle",        "Детали ошибки:"                                        },
            { "btn.rest.recalcular",     "Пересчитать контрольные цифры"                         },
            { "btn.rest.backup",         "Восстановить из резервной копии"                        },
            { "btn.rest.salir",          "Выйти"                                                  },
            // Меню — Панель управления + разделённые журналы
            { "mnu.dashboard",           "Панель управления"                                      },
            { "mnu.bitacora.sistema",    "Системный журнал"                                       },
            { "mnu.bitacora.negocio",    "Бизнес-журнал"                                         },
            // Панель управления — Dashboard
            { "frm.dashboard",           "Панель управления"                                      },
            { "dash.prendas",            "Товаров\nдоступно"                                      },
            { "dash.clientes",           "Клиентов\nзарегистрировано"                             },
            { "dash.pedidos",            "Заказов\nв ожидании"                                    },
            { "dash.backup",             "дней без\nрезервной копии"                              },
            { "dash.btn.refrescar",      "↻ Обновить"                                             },
            { "mnu.reportejornada",      "Отчёт за день"                                          },
            // Отчёт за день
            { "frm.reportejornada",      "Отчёт за день — WardrobeFlow"                           },
            { "rpt.generar",             "Создать"                                                },
            { "rpt.comparar",            "Сравнить дни"                                           },
            { "rpt.exportartxt",         "Экспорт TXT"                                            },
            { "rpt.exportarpdf",         "Экспорт PDF"                                            },
            { "rpt.fecha",               "День:"                                                  },
            { "rpt.fecha2",              "Сравнить с:"                                            },
            { "rpt.kpi.prendas",         "Доступная одежда"                                       },
            { "rpt.kpi.clientes",        "Клиентов"                                              },
            { "rpt.kpi.eventos",         "Событий за день"                                        },
            { "rpt.kpi.backup",          "дней без резервной копии"                               },
            { "rpt.sinDatos",            "Нет событий за выбранный день."                         },
            { "rpt.limpiar",             "Очистить"                                              },
            { "rpt.subtitulo",           "Бизнес-события за день с экспортом в TXT"             },
            // Отчёт за день — тексты сгенерированного отчёта
            { "rpt.txt.titulo",          "ОТЧЁТ ЗА ДЕНЬ"                                        },
            { "rpt.txt.resumen",         "СВОДКА СИСТЕМЫ"                                        },
            { "rpt.txt.prendas",         "Доступная одежда"                                      },
            { "rpt.txt.clientes",        "Клиентов"                                              },
            { "rpt.txt.diassinbkp",      "Дней без резервной копии"                              },
            { "rpt.txt.sinbackups",      "Нет резервных копий"                                   },
            { "rpt.txt.eventos",         "БИЗНЕС-СОБЫТИЯ ЗА ДЕНЬ"                               },
            { "rpt.txt.sinevt",          "(нет событий за эту смену)"                            },
            { "rpt.txt.usuario",         "Пользователь"                                          },
            { "rpt.txt.cliente",         "Клиент"                                                },
            { "rpt.txt.totalevt",        "ВСЕГО СОБЫТИЙ"                                        },
            { "rpt.txt.generado",        "Создан"                                                },
            { "rpt.txt.comparacion",     "СРАВНЕНИЕ СМЕН"                                        },
            { "rpt.txt.jornada",         "СМЕНА"                                                 },
            { "rpt.txt.sinevtjorn",      "(нет событий за эту смену)"                            },
            { "rpt.txt.comparfinal",     "ИТОГОВАЯ СВОДКА"                                       },
            { "rpt.txt.fecha",           "Дата"                                                  },
            { "rpt.txt.eventostot",      "Всего событий"                                         },
            { "rpt.txt.masmasa",         "имела больше активности"                               },
            { "rpt.txt.ninguna",         "Ни в одной смене событий не зарегистрировано."         },
            { "rpt.txt.iguales",         "Обе смены имели одинаковое количество событий."        },
            { "rpt.txt.rptoegenerado",   "Отчёт создан"                                          },
            { "rpt.txt.compgenerada",    "Сравнение создано"                                     },
            { "rpt.txt.impresionenv",    "Печать отправлена"                                     },
            { "bit.pdf.titulosistema",   "Системный журнал — WardrobeFlow"                      },
            { "bit.pdf.titulonegocio",   "Бизнес-журнал — WardrobeFlow"                        },
            { "bit.pdf.pagina",          "WardrobeFlow — Страница {0}"                          },
            { "bit.pdf.vistaprevia",     "Предварительный просмотр"                             },
            { "dash.backup.hoy",         "Сегодня"                                              },
            { "dash.sesion.iniciada",    "Сеанс начат:"                                         },
            // Панель управления — последние события и мини-статистика
            { "dash.actividad.titulo",  "Последние события"                                    },
            { "dash.stats.titulo",      "Сводка событий"                                       },
            { "dash.col.fecha",         "Дата"                                                 },
            { "dash.col.evento",        "Событие"                                              },
            { "dash.col.usuario",       "Пользователь"                                         },
            // Панель управления — уведомления о резервной копии и диалог настройки
            { "dash.aviso.sinbackup",   "⚠  Резервных копий нет. Создайте через Администрирование → Резервная копия." },
            { "dash.aviso.vencido",     "⚠  {0} дней без резервной копии — напоминание каждые {1} дней."  },
            { "dash.cfg.titulo",        "Напоминание о резервной копии"                                    },
            { "dash.cfg.recada",        "Напоминать каждые:"                                               },
            { "dash.cfg.dias",          "дней"                                                             },
            { "dash.cfg.guardar",       "Сохранить"                                                        },
            // Отчёт за день — меню экспорта и диалоги
            { "rpt.menu.guardartxt",    "Сохранить как .TXT"                                               },
            { "rpt.menu.imprimir",      "Печать / Экспорт PDF"                                             },
            { "rpt.menu.guardarcmp",    "Сохранить сравнение как .TXT"                                     },
            { "rpt.dlg.guardartxt",     "Сохранить как TXT"                                               },
            { "rpt.dlg.exito.titulo",   "Успех"                                                            },
            { "rpt.dlg.exito.msg",      "Файл сохранён:\n{0}"                                              },
            // История изменений пользователей (T06)
            { "frm.historialusr",        "История изменений пользователей"                        },
            { "lbl.ver.usuario",         "Пользователь:"                                          },
            { "btn.ver.cargar",          "Загрузить"                                              },
            { "btn.ver.restaurar",       "Восстановить выбранную версию"                          },
            { "col.ver.id",              "ID"                                                     },
            { "col.ver.fecha",           "Дата"                                                   },
            { "col.ver.actor",           "Изменено кем"                                           },
            { "col.ver.detalle",         "Детали"                                                 },
            { "col.ver.estado",          "Состояние"                                              },
            { "mnu.historialusr",        "История изменений"                                      },
            // Сообщения ConfirmarAdmin
            { "msg.confirmar.vacio",         "Введите имя пользователя и пароль."                                        },
            { "msg.confirmar.invalido",      "Неверные учётные данные или пользователь не является Администратором."    },
            // Сообщения VersionHistorial
            { "msg.historial.sinseleccion",  "Выберите версию из таблицы."                                               },
            { "msg.historial.atencion",      "Внимание"                                                                  },
            { "msg.historial.confirmar",     "Восстановить пользователя '{0}' к состоянию от {1}?\n\nДетали снимка: {2}\n\nЭто действие обратимо (новый снимок сохраняется перед восстановлением)." },
            { "msg.historial.restaurado",    "Версия успешно восстановлена."                                             },
            // Клиенты — операция и подтверждение удаления
            { "msg.cli.registrado",          "Клиент '{0}' успешно зарегистрирован."                                    },
            { "msg.cli.actualizado",         "Клиент '{0}' обновлён."                                                   },
            { "msg.cli.eliminado",           "Клиент '{0}' удалён."                                                     },
            { "conf.baja.cli.msg",           "Удалить клиента {0} (ИНН {1})?\n\nЭто действие нельзя отменить."         },
            { "conf.baja.cli.titulo",        "Подтверждение удаления"                                                   },
            { "err.cli.errorplanes",         "— Ошибка загрузки планов —"                                              },
            // Планы — операция и подтверждения
            { "msg.planes.creado",           "План '{0}' создан."                                                       },
            { "msg.planes.actualizado",      "План '{0}' обновлён."                                                     },
            { "msg.planes.desactivado",      "План '{0}' деактивирован."                                                },
            { "msg.planes.reactivado",       "План '{0}' реактивирован."                                                },
            { "conf.planes.desat.msg",       "Деактивировать план '{0}'?\n\nКлиенты с этим планом не пострадают."      },
            { "conf.planes.desat.tit",       "Подтверждение деактивации"                                               },
            { "conf.planes.act.msg",         "Реактивировать план '{0}'?\n\nПлан снова станет доступен для новых подписок." },
            { "conf.planes.act.tit",         "Подтверждение активации"                                                  },
            // Управление языками — сообщения
            { "msg.idiomas.activado",        "Язык активирован."                                                        },
            { "msg.idiomas.desactivado",     "Язык деактивирован."                                                      },
            { "conf.idiomas.desactivar",     "Деактивировать этот язык? Пользователи не смогут его выбрать."           },
            { "conf.idiomas.titulo",         "Подтверждение"                                                            },
            // История заказов — восстановление
            { "msg.hist.restaurado",         "Заказ #{0} успешно восстановлен."                                         },
            { "conf.hist.restaurar.msg",     "Восстановить заказ #{0} к состоянию до '{1}' (оп. #{2})?\n\n⚠ Примечание: эта операция изменяет статус Заказа в базе данных.\nСтатус связанных Предметов одежды НЕ откатывается автоматически.\n\nПодтвердить?" },
            // Пользователи — ЦК
            { "msg.usr.dvrecalculados",      "DVH и DVV успешно пересчитаны для всех пользователей."                   },
            { "msg.usr.errordv",             "Ошибка при пересчёте ЦК: {0}"                                            },
            // RestauracionForm — дополнительные сообщения
            { "msg.rest.dvexito",            "Контрольные цифры успешно пересчитаны.\nТеперь вы можете войти в систему." },
            { "msg.rest.dvtitulo",           "Целостность восстановлена"                                                },
            { "conf.rest.sobreescribir",     "Вы уверены? Эта операция перезапишет все текущие данные и перезапустит приложение." },
            { "msg.rest.errorrecalcular",    "Ошибка при пересчёте: {0}"                                               },
            { "msg.rest.errorrestaurar",     "Ошибка при восстановлении: {0}"                                          },
            // BackupForm — ошибки в catch
            { "msg.backup.errorgenerar",     "Ошибка при создании резервной копии:\n{0}"                               },
            { "msg.backup.erroreliminar",    "Ошибка при удалении:\n{0}"                                               },
            { "msg.backup.errorrestaurar",   "Ошибка при восстановлении:\n{0}"                                         },
            // VersionHistorialForm — ошибки в catch
            { "msg.historial.errorcargar",   "Ошибка загрузки истории:\n{0}"                                           },
            { "msg.historial.errorrestaur",  "Ошибка восстановления версии:\n{0}"                                      },
            // Заголовок общей ошибки
            { "msg.error.titulo",            "Ошибка"                                                                   },
        });

        // ── Constructor de diccionario ────────────────────────────────────────

        /// <summary>
        /// Convierte un Dictionary&lt;string, string&gt; en IDictionary&lt;string, Traduccion&gt;.
        /// </summary>
        private static IDictionary<string, Traduccion> Construir(Dictionary<string, string> raw)
        {
            var result = new Dictionary<string, Traduccion>();
            foreach (var kv in raw)
                result[kv.Key] = new Traduccion { Clave = kv.Key, Texto = kv.Value };
            return result;
        }
    }
}
