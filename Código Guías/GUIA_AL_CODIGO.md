# WardrobeFlow — Guía al Código (mapa para la defensa)

> Dónde está cada cosa, qué clase/método lo hace y qué función cumple.
> Pensada para tener las **tabs justas abiertas** y explicar los patrones de la 2ª entrega.
> Las líneas son orientativas (pueden correrse ±2 si editás el archivo).

## Cómo leer esta guía

Cada patrón se explica de **la entidad → la lógica → la UI / persistencia**. Ese es el orden
en que conviene mostrarlo en la defensa: primero la estructura, después quién la usa.

Recordá el sentido de las capas:
`GUI → BLL → DAL → BE` · `Servicios` (bitácora, idiomas) y `Seguridad` (sesión, hash, DV) son transversales.

---

## 1. 🌳 Composite — Perfiles y Permisos (T04)

**Qué resuelve:** los permisos forman un árbol (Patentes sueltas → agrupadas en Familias → agrupadas en Roles, y un Rol puede contener otro Rol). El Composite permite **tratar un nodo simple y uno compuesto igual** y resolver los permisos **recursivamente**.

| Archivo | Clase / Método | Línea | Qué hace / Rol en el patrón |
|---|---|---|---|
| `BE/Componente.cs` | `Componente` (abstract) | — | **Component**: base común. Define `Hijos`, `AgregarHijo`, `QuitarHijo` |
| | `ObtenerPatentesEfectivas()` | 27 | Operación uniforme: devuelve todas las hojas alcanzables sin duplicar |
| | `RecolectarPatentes()` (abstract) | 37 | Paso recursivo que cada nodo implementa a su manera |
| `BE/Patente.cs` | `Patente : Componente` | — | **Leaf** (hoja): un permiso atómico. `NombreMenu` es la clave que valida la sesión |
| | `AgregarHijo()` → lanza excepción | 22 | Una hoja **no** admite hijos (polimorfismo) |
| | `RecolectarPatentes()` | 39 | **Caso base** de la recursión: la hoja se aporta a sí misma |
| `BE/Familia.cs` | `Familia : Componente` | — | **Composite**: nodo con hijos (`_hijos`) |
| | `AgregarHijo()` | 17 | Agrega hijo **validando ciclos** (directo: hijo de sí mismo; indirecto: `ContieneDescendiente`) |
| | `ContieneDescendiente()` | 38 | Recorre el subárbol (tope de profundidad 50) para detectar referencia circular |
| | `RecolectarPatentes()` | 70 | **Paso recursivo**: junta las hojas de todos sus hijos |
| `BE/Rol.cs` | `Rol : Familia` | — | Un Rol **es** una Familia asignable → habilita **rol-dentro-de-rol** (hereda el anti-ciclos) |
| `BLL/Familia.cs` | `ObtenerArbol()` | 75 | Devuelve el árbol completo desde BD para visualizar |
| | `ObtenerPermisosEfectivos(rol)` | 84 | **Motor de autorización**: resuelve recursivamente todas las patentes del rol, deduplicadas. Es lo que se carga en la sesión al login |
| | `GuardarAsignacionRol(rol, ids)` | 164 | Compara seleccionados vs. actuales, aplica altas/bajas. Valida ciclos y anti-autobloqueo |
| | `AgregarComponente(idPadre, idHijo)` | 256 | Embebe un componente en otro (rechaza `idPadre == idHijo`) |
| | `ValidarSinCiclo(idPadre, idHijo)` | 333 | Reusa `Familia.AgregarHijo` sobre el árbol real para detectar ciclos antes de persistir |
| `DAL/Permiso.cs` | `ObtenerArbol()` | — | Construye el árbol: lee `Permiso` + las aristas de `PermisoRelacion` y arma los nodos |
| | `AgregarRelacion` / `QuitarRelacion` | 297 / 313 | INSERT/DELETE de aristas padre→hijo |
| `GUI/GestorPermisos.cs` | — | — | TreeView con checkboxes; recorre el árbol de forma recursiva para mostrar y para recolectar lo marcado |

**BD:** `Permiso` (un nodo; discriminadores `EsFamilia`, `EsRol`) + `PermisoRelacion` (`IdPadre`, `IdHijo` = aristas del árbol).

**Frase para la defensa:** *"El Rol no sabe si su hijo es una Patente, una Familia u otro Rol: a todos les pide `RecolectarPatentes` y cada uno resuelve su parte. Esa es la uniformidad del Composite."*

---

## 2. 🌐 Observer — Multiidioma (T05)

**Qué resuelve:** cambiar el idioma en runtime y que **todas las ventanas abiertas se actualicen solas**, sin reiniciar y sin que el Subject conozca a los formularios concretos.

| Archivo | Clase / Método | Línea | Qué hace / Rol en el patrón |
|---|---|---|---|
| `Servicios/Multiidioma/IIdiomaObserver.cs` | `IIdiomaObserver` | — | **Observer**: contrato que implementa cada form |
| | `UpdateLanguage(idioma)` | 17 | Callback que el Subject invoca al cambiar idioma |
| `Servicios/Multiidioma/GestorIdioma.cs` | `GestorIdioma` (static) | — | **Subject (Sujeto)**: mantiene la lista de observers y el idioma actual |
| | `SuscribirObservador` / `Desuscribir` | 43 / 52 | Alta/baja de observers (bajo `lock`) |
| | `CambiarIdioma(idioma, tradus)` | 59 | Cambia idioma y dispara `Notificar` |
| | `Notificar(idioma)` | 73 | Recorre una **copia defensiva** y llama `UpdateLanguage` en cada observer (un fallo no corta a los demás) |
| `Servicios/Multiidioma/Traductor.cs` | `ObtenerTraducciones(idioma)` | — | Fuente de textos: overlay de BD sobre el corpus; **fallback por clave** al idioma default si falta una |
| `GUI/FormIdiomas.cs` | `OnLoad` → `SuscribirObservador` | 42 | Se registra como observer al abrir |
| | `OnFormClosing` → `Desuscribir` | 149 | Se da de baja al cerrar (evita memory leak) |
| | `UpdateLanguage` / `Traducir` | 155 / 157 | Reasigna el `.Text` de cada control al idioma nuevo |
| | `BtnGuardar_Click` | 417 | **Editar traducción a mano → BD.** El `EndEdit()` (430) vuelca la celda antes de guardar |
| `BLL/Idioma.cs` | `GuardarTraduccion(...)` | 117 | Persiste la traducción (corta si el texto está vacío) |
| `DAL/Traduccion.cs` | `GuardarTraduccion(...)` | 64 | **UPSERT** en la tabla `Traduccion` (UPDATE si existe, si no INSERT) |
| `GUI/Clientes.cs` | `Traducir()` | — | Ejemplo de un **form de negocio cualquiera** que también es observer |

**BD:** `Control` (textos traducibles: `Clave`, `Formulario`) × `Idioma` (código, activo, default) → `Traduccion` (`IdControl`, `IdIdioma`, `Texto`). La última es la que se escribe al traducir a mano.

**Frase para la defensa:** *"El `GestorIdioma` no conoce ningún formulario; solo conoce la interfaz `IIdiomaObserver`. Por eso puedo agregar ventanas nuevas sin tocar el Subject."*

---

## 3. 🔒 Singleton

**Qué resuelve:** garantizar **una sola instancia** de recursos globales (la sesión del usuario, el punto de acceso a BD).

| Archivo | Clase / Método | Línea | Qué hace / Rol |
|---|---|---|---|
| `Seguridad/SessionManager.cs` | `SessionManager` (sealed) | — | **Singleton de la sesión**: el único usuario autenticado a la vez |
| | `Login(usuario)` | 45 | Crea la sesión bajo `lock`; rechaza si ya hay una |
| | `TienePermiso(menu)` | ~38 | Guard de autorización (Admin pasa; si no, busca el permiso). Fail-closed |
| | `Logout()` | 63 | Destruye la sesión |
| `DAL/Acceso.cs` | `Acceso` (sealed) | — | **Singleton de acceso a BD**: lee la cadena de conexión una sola vez |
| | `GetInstance()` | 51 | **Double-checked locking** con campo `volatile` (thread-safe) |
| | `Leer` / `Escribir` | 66 / 84 | Una `SqlConnection` nueva por operación (pooling de ADO.NET) |
| | `EjecutarTransaccion(...)` | 127 | Operaciones multi-tabla atómicas (commit/rollback) |

**Frase para la defensa:** *"Constructor privado + `GetInstance()` con doble verificación bajo lock: ninguna parte del sistema puede crear una segunda sesión ni un segundo punto de acceso."*

---

## 4. 🧠 Memento — Control de Cambios de Usuario (T06b)

**Qué resuelve:** guardar **snapshots** del estado de un usuario antes de cada cambio sensible, para poder **deshacer** (y deshacer el deshacer).

| Archivo | Clase / Método | Línea | Qué hace / Rol en el patrón |
|---|---|---|---|
| `BE/Usuario.cs` | `Usuario : Memento.IOriginator` | — | **Originator**: crea y restaura su propio estado. `EsAdministrador` (34) |
| | `CrearMemento(actor, detalle)` | — | Captura el estado actual en un Memento |
| `BE/VersionUsuario.cs` | `VersionUsuario : Memento.IMemento` | 14 | **Memento**: cápsula inmutable del estado (clave, estado, intentos, perfil…) |
| `BLL/CuidadorHistorial.cs` | `CuidadorHistorial` | — | **Caretaker**: guarda y recupera los mementos (no mira su contenido) |
| `BLL/VersionUsuario.cs` | `GrabarVersion(id, actor, detalle)` | 28 | Fachada: pide el memento al Originator y lo manda al Caretaker. **Fail-safe**: si no puede guardar, aborta |
| | `RestaurarVersion(modulo, idVersion)` | 52 | Antes de restaurar, graba un snapshot del estado actual (rollback de un rollback) |

**BD:** `HistorialUsuario` (un memento por fila). **Se dispara** antes de: reset de clave, desbloqueo, archivar usuario y cambio de permisos del rol.

**Frase para la defensa:** *"El Caretaker (`CuidadorHistorial`) guarda los mementos pero nunca conoce sus campos internos; solo el Originator (`BE.Usuario`) sabe crearlos y aplicarlos."*

---

## 5. 🏭 Factory

**Qué resuelve:** **encapsular la creación** de objetos para que el que los usa dependa de una abstracción y no de clases concretas.

| Archivo | Clase / Método | Línea | Qué hace / Rol |
|---|---|---|---|
| `GUI/Menu.cs` | `_dashboardFactory` (Dictionary) | 368 | Mapa `rol → Func<Form>`: qué Dashboard crear según el perfil |
| | `CrearDashboardDelRol()` | 378 | **Factory method**: devuelve el Dashboard correcto (`DashboardVendedor`, `DashboardControlStock`, `DashboardOperador`…) o el genérico `DashboardForm`. Agregar un rol = una línea en el diccionario (**Open/Closed**) |
| `Seguridad/CalculadorDV.cs` | `CalculadorDV.Crear()` | 10 | **Simple Factory**: devuelve un `ICalculadorDV` ocultando la implementación concreta. Cambiar el algoritmo de DV de todo el sistema = un solo `return` |
| `Seguridad/ICalculadorDV.cs` | `ICalculadorDV` | — | La abstracción que la factory entrega |

**Frase para la defensa:** *"El `Menu` no hace un `switch` gigante con `new` por todos lados: la factory decide la clase concreta por el rol, y para sumar un dashboard nuevo no toco el método, solo el diccionario."*

---

## 6. 🛡️ Dígitos Verificadores — DVH + DVV (T07)

**Qué resuelve:** detectar **manipulación directa de la BD** (ediciones por SSMS, filas insertadas/borradas) que se saltearían las validaciones de la app.

- **DVH** (horizontal): un dígito por **fila** → detecta que **cambiaron un campo**.
- **DVV** (vertical): un dígito por **tabla** (en `DVVertical`) → detecta que **agregaron/borraron/reordenaron filas**.

| Archivo | Clase / Método | Línea | Qué hace / Rol |
|---|---|---|---|
| `Seguridad/DigitoVerificador.cs` | `CalcularDVH(params string[])` | 42 | **El algoritmo**: suma `ASCII(c) × posición`, `mod 999.983` (primo, pocas colisiones) |
| | `CalcularDVV(IList<int>)` | 76 | Combina los DVH de la tabla ponderados por posición |
| `Seguridad/ICalculadorDV.cs` / `CalculadorDV.cs` | — | 10 | Abstracción + factory del algoritmo (ver Factory) |
| `BE/FilaUsuarioDV.cs` | `CamposParaDVH()` | 35 | Qué campos de `Usuario` entran al DVH: `Id, Username, Clave, **Rol**, Perfil, Estado, IntentosFallidos` |
| `DAL/DigitoVerificador.cs` | `ObtenerFilasUsuario()` | 66 | Lee las filas con su DVH guardado |
| | `ObtenerFilas(tabla, pk, cols)` | 141 | Lectura **genérica** para cualquier tabla protegida |
| | `RecalcularTabla(tabla, pk, cols)` | 180 | Recalcula DVH de cada fila + DVV de la tabla |
| | `Id(identificador)` | 128 | **Anti-inyección**: valida nombres de tabla/columna contra lista blanca y los encierra en `[ ]` |
| `BLL/Configuracion.cs` | `VerificarIntegridadDV(...)` | 122 | **Verificación pre-login** de `Usuario` (corre al arrancar) |
| | `AsegurarIntegridadUsuarios()` | 249 | Re-chequea antes de operaciones sensibles y de backups |
| `GUI/DiagnosticoIntegridadForm.cs` | — | — | Diagnostica las 4 tablas y permite reparar |
| `GUI/RestauracionForm.cs` | — | — | Si la verificación pre-login falla: recalcular DV o restaurar backup |

**Tablas protegidas y sus campos de DVH:**

| Tabla | PK | Campos del DVH | Constante en código |
|---|---|---|---|
| `Usuario` | `IdUsuario` | Username, Clave, Rol, Perfil, Estado, IntentosFallidos | `BE/FilaUsuarioDV.cs` |
| `Cliente` | `IdCliente` | Nombre, Apellido, DNI, Email, MetodoPago | `DAL/Cliente.cs` → `DV_Columnas` |
| `Empleado` | `IdEmpleado` | Nombre, Apellido, DNI, Email, Puesto, Legajo | `DAL/Empleado.cs` → `DV_Columnas` |
| `Pedido` | `IdPedido` | IdCliente, IdEmpleado, Estado + huella de líneas (`PedidoPrenda`) | `DAL/Pedido.cs` → `ObtenerFilasDV` |

**Almacenamiento:** columna `DVH` en cada tabla + tabla `DVVertical` (`NombreTabla`, `DVV`, `FechaCalculo`).

**Frase para la defensa:** *"Cualquier UPDATE a un campo cubierto que no recalcule el DVH deja la fila inconsistente; al arrancar, la app recomputa y compara, detecta la manipulación y bloquea el acceso hasta reparar o restaurar."*

---

## 7. ➕ Apoyos transversales (por si los preguntan)

| Tema | Archivo principal | Qué cumple |
|---|---|---|
| **Encriptado** (T03) | `Seguridad/Encriptador.cs` | Hash PBKDF2-SHA256 (claves) + AES-CBC (datos sensibles) |
| **Bitácora** (T06a) | `Servicios/Bitacora.cs` | Log del sistema (login, bloqueos, DV, backups) |
| **Guardas de autorización** | `BLL/BLLHelper.cs` | Centraliza fail-closed: `ValidarPermiso`, `ExigirAdministrador`, `ExigirGestionUsuarios` |
| **Arranque** | `GUI/Program.cs` | Verifica conexión → integridad DV → login |

---

## Resumen de tabs por defensa

- **Composite:** `BE/Componente.cs`, `BE/Patente.cs`, `BE/Familia.cs`, `BE/Rol.cs`, `BLL/Familia.cs`, `DAL/Permiso.cs`, `GUI/GestorPermisos.cs`
- **Observer:** `Servicios/Multiidioma/IIdiomaObserver.cs`, `GestorIdioma.cs`, `Traductor.cs`, `GUI/FormIdiomas.cs`, `BLL/Idioma.cs`, `DAL/Traduccion.cs`
- **Singleton:** `Seguridad/SessionManager.cs`, `DAL/Acceso.cs`
- **Memento:** `BE/Usuario.cs`, `BE/VersionUsuario.cs`, `BLL/CuidadorHistorial.cs`, `BLL/VersionUsuario.cs`
- **Factory:** `GUI/Menu.cs`, `Seguridad/CalculadorDV.cs`
- **Dígitos Verificadores:** `Seguridad/DigitoVerificador.cs`, `DAL/DigitoVerificador.cs`, `BLL/Configuracion.cs`, `BE/FilaUsuarioDV.cs`
