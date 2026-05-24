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
            { "frm.bitacora",     "Auditoría — Bitácoras del Sistema" },
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
            { "frm.bitacora",     "Audit Log"                      },
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
            { "frm.bitacora",     "Аудит — Журналы системы"                },
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
