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
            // Login
            { "frm.login",        "WardrobeFlow — Acceso"         },
            { "lbl.usuario",      "Usuario"                        },
            { "lbl.contrasena",   "Contraseña"                     },
            { "btn.ingresar",     "Ingresar"                       },
            { "btn.salir",        "Salir"                          },
            { "lnk.olvide",       "¿Olvidaste tu contraseña?"      },
            // Menú principal
            { "mnu.inventario",   "Inventario"                     },
            { "mnu.prendas",      "Prendas"                        },
            { "mnu.ventas",       "Ventas"                         },
            { "mnu.clientes",     "Clientes"                       },
            { "mnu.planes",       "Planes de Suscripción"          },
            { "mnu.pedidosventa", "Pedidos de Venta"               },
            { "mnu.pedidosreal",  "Pedidos Realizados"             },
            { "mnu.administrar",  "Administrar"                    },
            { "mnu.usuarios",     "Usuarios"                       },
            { "mnu.bitacora",     "Bitácora"                       },
            { "mnu.cerrarsesion", "Cerrar Sesión"                  },
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
            // Main menu
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
            // Главное меню
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
