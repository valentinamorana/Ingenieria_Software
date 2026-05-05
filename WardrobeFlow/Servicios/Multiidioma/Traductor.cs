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
        /// Devuelve el diccionario completo de traducciones para el idioma dado.
        /// Si el idioma es null devuelve el idioma por defecto (ES).
        ///
        /// Equivalente a Traductor.ObtenerTraducciones(idioma) del ejemplo de cátedra,
        /// pero usando diccionarios en memoria en lugar de consulta SQL.
        /// </summary>
        public static IDictionary<string, Traduccion> ObtenerTraducciones(Idioma idioma = null)
        {
            if (idioma == null)
                idioma = ObtenerIdiomaDefault();

            switch (idioma.Id)
            {
                case "EN": return _en;
                case "RU": return _ru;
                default:   return _es;  // ES es el fallback
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
            { "mnu.bitacora",     "Bitácora"                       },
            { "mnu.cerrarsesion", "Cerrar Sesion"                  },
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
            { "mnu.bitacora",     "Audit Log"                       },
            { "mnu.cerrarsesion", "Sign Out"                        },
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
        });

        // ── Diccionario Русский (RU) ──────────────────────────────────────────

        private static readonly IDictionary<string, Traduccion> _ru =
            Construir(new Dictionary<string, string>
        {
            // Вход
            { "frm.login",        "WardrobeFlow — Вход"            },
            { "lbl.usuario",      "Пользователь"                   },
            { "lbl.contrasena",   "Пароль"                         },
            { "btn.ingresar",     "Войти"                          },
            { "btn.salir",        "Выйти"                          },
            { "lnk.olvide",       "Забыли пароль?"                 },
            // Панель языка
            { "lbl.idioma",       "Язык:"                          },
            // Вход — подзаголовок
            { "lbl.subtitulo",    "ПОРТАЛ СОТРУДНИКОВ"             },
            // Главное меню
            { "mnu.perfil",       "Профиль"                        },
            { "mnu.inventario",   "Склад"                          },
            { "mnu.prendas",      "Одежда"                         },
            { "mnu.ventas",       "Продажи"                        },
            { "mnu.clientes",     "Клиенты"                        },
            { "mnu.planes",       "Тарифные планы"                 },
            { "mnu.pedidosventa", "Заказы на продажу"              },
            { "mnu.pedidosreal",  "Выполненные заказы"             },
            { "mnu.administrar",  "Администрирование"              },
            { "mnu.usuarios",     "Пользователи"                   },
            { "mnu.bitacora",     "Журнал аудита"                  },
            { "mnu.cerrarsesion", "Выйти из системы"               },
            // Клиенты
            { "frm.clientes",     "Управление клиентами"           },
            { "lbl.buscar",       "Поиск:"                         },
            { "btn.nuevocliente", "+ Новый клиент"                 },
            { "btn.editar",       "✎ Изменить"                     },
            { "btn.darbaja",      "✕ Удалить"                      },
            // Одежда
            { "frm.prendas",      "Каталог одежды"                 },
            { "lbl.estado",       "Статус:"                        },
            { "btn.nuevaprenda",  "+ Новая одежда"                 },
            { "btn.cambiarestado","⇄ Статус"                       },
            { "lbl.clienteenuso", "Клиент в использовании:"        },
            // Пользователи
            { "frm.gestion",      "Управление пользователями"      },
            { "lbl.nuevousuario", "Новый пользователь"             },
            { "lbl.nombreusuario","Имя пользователя:"              },
            { "lbl.perfilrol",    "Профиль (роль):"                },
            { "btn.agregar",      "Добавить пользователя"          },
            { "lbl.resettitulo",  "Сброс пароля"                   },
            { "lbl.resetinfo",    "Выберите пользователя\nиз списка и нажмите:" },
            { "btn.resetclave",   "Сбросить пароль"                },
            { "lbl.desbloqtitulo","Разблокировать аккаунт"         },
            { "lbl.desbloqinfo",  "Выберите заблокированного\nпользователя и нажмите:" },
            { "btn.desbloquear",  "Разблокировать аккаунт"         },
            { "lbl.listatitulo",  "Пользователи системы"           },
            // Планы
            { "frm.planes",       "Тарифные планы"                 },
            { "lbl.nuevopla",     "Новый план"                     },
            { "lbl.nombreplan",   "Название плана *"               },
            { "lbl.limiteprendas","Лимит одежды *"                 },
            { "lbl.preciomensual","Цена в месяц ($) *"             },
            { "btn.guardarplan",  "Сохранить план"                 },
            { "btn.limpiar",      "Очистить / Новый"               },
            { "lbl.acciones",     "Действия с выбранным планом"    },
            { "btn.desactivar",   "Деактивировать план"            },
            { "btn.activar",      "Активировать план"              },
            { "lbl.planesreg",    "Зарегистрированные планы"       },
            // Журнал аудита
            { "frm.bitacora",     "Журнал аудита"                  },
            { "tab.sistema",      "🔐  Системный журнал"           },
            { "tab.negocio",      "📦  Журнал операций"            },
            { "lbl.ultimos",      "Последние"                      },
            { "lbl.dias",         "дней  (0 = все)"                },
            { "btn.ver",          "Показать"                       },
            { "lbl.usuarioid",    "ID пользователя:"               },
            { "lbl.actividad",    "Активность:"                    },
            { "lbl.criticidad",   "Критичность:"                   },
            { "btn.buscar",       "Найти"                          },
            { "btn.limpiarfiltro","Очистить"                       },
            { "btn.exportar",     "Экспорт CSV"                    },
            { "lbl.tipoevento",   "Тип события:"                   },
            { "lbl.idpedido",     "ID заказа:"                     },
            { "lbl.idcliente",    "ID клиента:"                    },
            // Заказы на продажу
            { "frm.pedidosventa",  "Заказы на продажу"             },
            { "btn.nuevopedido",   "+ Новый заказ"                 },
            { "btn.cancelarpedido","✕ Отменить"                    },
            { "btn.descancelar",   "↩ Восстановить"                },
            { "lbl.prendaspedido", "Одежда выбранного заказа"      },
            // Отправка заказов
            { "frm.pedidosreal2",  "Отправка заказов"              },
            { "btn.despachar",     "📦 Отправить"                  },
            { "btn.entregado",     "✓ Доставлено"                  },
            { "btn.vernotificacion","✉ Уведомление"                },
            { "btn.devolucion",    "↩ Возврат"                     },
            { "lbl.detallepedido", "Детали выбранного заказа"      },
            // Отправка заказов — столбцы таблицы
            { "col.ped.urgencia",  "Срочность"                     },
            { "col.ped.fecha",     "Дата"                          },
            { "col.ped.cliente",   "Клиент"                        },
            { "col.ped.vendedor",  "Продавец"                      },
            { "col.ped.prendas",   "Одежда"                        },
            { "col.ped.estado",    "Статус"                        },
            { "col.ped.despacho",  "Отправка"                      },
            { "col.ped.entrega",   "Доставка"                      },
            // Отправка заказов — значения срочности
            { "urg.urgente",       "Срочно"                        },
            { "urg.normal",        "Обычный"                       },
            { "urg.reciente",      "Недавний"                      },
            // Отправка заказов — значения статуса
            { "est.pendiente",     "Ожидание"                      },
            { "est.despachado",    "Отправлен"                     },
            { "est.entregado",     "Доставлен"                     },
            { "est.cancelado",     "Отменён"                       },
            // Журнал аудита — столбцы системного журнала
            { "col.bit.id",        "Id"                            },
            { "col.bit.fecha",     "Дата"                          },
            { "col.bit.usuario",   "Пользователь"                  },
            { "col.bit.modulo",    "Модуль"                        },
            { "col.bit.actividad", "Активность"                    },
            { "col.bit.detalle",   "Детали"                        },
            { "col.bit.criticidad","Критичность"                   },
            { "col.bit.ip",        "IP"                            },
            // Журнал аудита — столбцы журнала операций
            { "col.neg.idevento",  "Id события"                    },
            { "col.neg.fecha",     "Дата"                          },
            { "col.neg.tipo",      "Тип"                           },
            { "col.neg.usuario",   "Пользователь"                  },
            { "col.neg.cliente",   "Клиент"                        },
            { "col.neg.idpedido",  "Id заказа"                     },
            { "col.neg.idprenda",  "Id одежды"                     },
            { "col.neg.idcliente", "Id клиента"                    },
            { "col.neg.desc",      "Описание"                      },
            // Журнал аудита — статистика критичности
            { "stat.ninguno",      "Нет"                           },
            { "stat.baja",         "Низкая"                        },
            { "stat.media",        "Средняя"                       },
            { "stat.alta",         "Высокая"                       },
            { "stat.intlogin",     "Вход"                          },
            { "stat.recupclave",   "Сброс пароля"                  },
            { "stat.bloqueos",     "Блокировки"                    },
        });

        // ── Helper ────────────────────────────────────────────────────────────

        /// <summary>
        /// Convierte un diccionario clave→texto en un diccionario clave→Traduccion.
        /// Evita repetir la construcción de Etiqueta/Traduccion en cada idioma.
        /// </summary>
        private static IDictionary<string, Traduccion> Construir(Dictionary<string, string> fuente)
        {
            var resultado = new Dictionary<string, Traduccion>();
            foreach (var par in fuente)
            {
                resultado[par.Key] = new Traduccion
                {
                    Etiqueta = new Etiqueta { Nombre = par.Key },
                    Texto    = par.Value
                };
            }
            return resultado;
        }
    }
}
