# Patrones de Diseño en WardrobeFlow
## T04 — Composite | T06b — Control de Cambios | T05 — Observer

---

## 1. T04 — Gestión de Perfiles de Usuario (Patrón Composite)

### 1.1 Objetivo

Implementar un sistema de gestión de permisos por rol que permita al Administrador visualizar y modificar, en tiempo de ejecución y sin modificar el código, qué funcionalidades del sistema tiene habilitadas cada perfil de usuario. Se utiliza el patrón **Composite** para modelar la jerarquía de permisos como un árbol, y funciones **recursivas** para recorrerlo y mostrarlo en un control `TreeView`.

### 1.2 Descripción detallada de cómo funciona

El sistema define roles fijos (Administrador, Vendedor, OperadorLogistico, etc.) y un catálogo de permisos individuales almacenados en base de datos. Cada permiso tiene asociado un grupo (`TipoComponente`) que determina en qué rama del árbol aparece.

Al seleccionar un rol en `GestorPermisos`, el sistema:
1. Consulta todos los permisos del sistema (`Permiso`)
2. Consulta los permisos asignados al rol seleccionado (`RolPermiso`)
3. Construye el árbol Composite en memoria
4. Lo muestra recursivamente en un `TreeView` con checkboxes
5. Al guardar, recorre el árbol recursivamente y aplica los cambios (INSERT/DELETE en `RolPermiso`)

Al hacer login, los permisos del rol se cargan en la sesión y el menú principal se construye dinámicamente mostrando solo las opciones permitidas.

### 1.3 Estructura de clases

```
BE.Componente  (abstracta)
├── BE.Familia  (nodo compuesto — grupo de permisos)
└── BE.Patente  (hoja — permiso individual)
```

**`BE.Componente`** — clase abstracta base:

```csharp
public abstract class Componente
{
    public int    Id      { get; set; }
    public string Nombre  { get; set; }
    public TipoPermiso Permiso { get; set; }

    public abstract IList<Componente> Hijos { get; }
    public abstract void AgregarHijo(Componente c);
    public abstract void VaciarHijos();
}
```

**`BE.Familia`** — nodo compuesto, puede contener hijos de cualquier tipo:

```csharp
public class Familia : Componente
{
    private readonly List<Componente> _hijos = new List<Componente>();

    public override IList<Componente> Hijos => _hijos;

    public override void AgregarHijo(Componente c)
    {
        if (c != null && !_hijos.Contains(c))
            _hijos.Add(c);
    }

    public override void VaciarHijos() { _hijos.Clear(); }
}
```

**`BE.Patente`** — hoja, nunca tiene hijos. Agrega `Asignado` (si el permiso está activo para el rol) y `NombreMenu` (clave del ítem de menú):

```csharp
public class Patente : Componente
{
    public string NombreMenu { get; set; }
    public bool   Asignado   { get; set; }

    public override IList<Componente> Hijos => new List<Componente>(0);
    public override void AgregarHijo(Componente c) { }  // hoja — no hace nada
    public override void VaciarHijos() { }
}
```

### 1.4 Estructura del árbol en tiempo de ejecución

```
Familia (raíz = nombre del rol, ej: "Administrador")
├── Familia ("Inventario")
│   ├── Patente ("Ver Prendas"    | mnuPrendas           | Asignado: true)
│   └── Patente ("Ver Stock"      | mnuStock             | Asignado: false)
├── Familia ("Ventas")
│   ├── Patente ("Ver Clientes"   | mnuClientes          | Asignado: true)
│   ├── Patente ("Ver Planes"     | mnuPlanSuscripciones | Asignado: true)
│   ├── Patente ("Pedidos Venta"  | mnuPedidosVenta      | Asignado: true)
│   └── Patente ("Pedidos Real."  | mnuPedidosRealizados | Asignado: true)
├── Familia ("Administración")
│   └── Patente ("Usuarios"       | mnuUsuarios          | Asignado: true)
└── Familia ("Auditoría")
    └── Patente ("Bitácora"        | mnuAuditoria         | Asignado: true)
```

### 1.5 Construcción del árbol — `BLL.Familia`

`BLL.Familia.ObtenerArbolPorRol(rol)` construye el árbol completo en memoria con solo dos consultas a la base de datos:

```csharp
public BE.Familia ObtenerArbolPorRol(string rol)
{
    List<BE.Permiso> todos     = permisoDAL.ObtenerTodos();      // todos los permisos del sistema
    List<BE.Permiso> asignados = permisoDAL.ObtenerPorRol(rol);  // los que tiene este rol

    var idsAsignados = new HashSet<int>();
    foreach (var p in asignados)
        idsAsignados.Add(p.Id);

    var raiz = new BE.Familia { Nombre = rol };

    // Agrupar por TipoComponente → una Familia por grupo
    var grupos      = new Dictionary<string, BE.Familia>();
    var ordenGrupos = new List<string>();

    foreach (var perm in todos)
    {
        string grupo = perm.TipoComponente ?? "General";
        if (!grupos.ContainsKey(grupo))
        {
            grupos[grupo] = new BE.Familia { Nombre = grupo };
            ordenGrupos.Add(grupo);
        }

        grupos[grupo].AgregarHijo(new BE.Patente
        {
            Id         = perm.Id,
            Nombre     = perm.Nombre,
            NombreMenu = perm.NombreMenu,
            Asignado   = idsAsignados.Contains(perm.Id)
        });
    }

    foreach (string g in ordenGrupos)
        raiz.AgregarHijo(grupos[g]);

    return raiz;
}
```

### 1.6 Persistencia — `DAL.Permiso`

| Tabla | Contenido |
|---|---|
| `Permiso` | `IdPermiso`, `Nombre`, `NombreMenu`, `TipoComponente`, `Estado` |
| `RolPermiso` | `Rol` (string), `IdPermiso` — relación M:N |

| Método DAL | Acción |
|---|---|
| `ObtenerTodos()` | SELECT sobre `Permiso`, ordenado por `TipoComponente` |
| `ObtenerPorRol(rol)` | JOIN entre `Permiso` y `RolPermiso` filtrando por rol |
| `AsignarPermiso(rol, id)` | INSERT con IF NOT EXISTS (idempotente) |
| `QuitarPermiso(rol, id)` | DELETE de `RolPermiso` |
| `ObtenerRoles()` | SELECT DISTINCT Rol FROM `RolPermiso` |

### 1.7 Funciones recursivas — `GUI.GestorPermisos`

**Mostrar el árbol (lectura):**

```csharp
private void MostrarPermisosRecursivo(BE.Componente componente, TreeNode nodoParent)
{
    foreach (BE.Componente hijo in componente.Hijos)
    {
        var nodo = new TreeNode(TraducirNombre(hijo)) { Tag = hijo };

        if (hijo is BE.Familia)
        {
            nodo.NodeFont  = new Font("Segoe UI", 9f, FontStyle.Bold);
            nodo.ForeColor = Color.FromArgb(40, 80, 140);
            // Los nodos Familia no son checkables (se cancela BeforeCheck)
        }
        else if (hijo is BE.Patente patente)
        {
            nodo.Checked = patente.Asignado;
        }

        if (nodoParent == null) _treeView.Nodes.Add(nodo);
        else                    nodoParent.Nodes.Add(nodo);

        MostrarPermisosRecursivo(hijo, nodo);  // ← recursión
    }
}
```

**Guardar cambios (escritura):**

```csharp
private void GuardarRecursivo(TreeNodeCollection nodos, string rol,
                               ref int asignados, ref int quitados)
{
    foreach (TreeNode nodo in nodos)
    {
        if (nodo.Tag is BE.Patente patente)
        {
            bool ahora    = nodo.Checked;
            bool anterior = patente.Asignado;

            if (ahora && !anterior)
            {
                _familiaBLL.AsignarPermiso(rol, patente.Id);
                asignados++;
            }
            else if (!ahora && anterior)
            {
                _familiaBLL.QuitarPermiso(rol, patente.Id);
                quitados++;
            }
        }
        GuardarRecursivo(nodo.Nodes, rol, ref asignados, ref quitados);  // ← recursión
    }
}
```

### 1.8 Aplicación de permisos al login

Al hacer login, `BLL.Usuario.Login()` carga los permisos del rol en `BE.Usuario.Permisos`. Al abrir el `Menu`, `AplicarPermisos()` muestra u oculta cada ítem del menú comparando `NombreMenu` contra los permisos del usuario:

```csharp
var nombresMenu = new HashSet<string>();
foreach (var p in permisos)
    nombresMenu.Add(p.NombreMenu);

prendasToolStripMenuItem.Visible  = nombresMenu.Contains("mnuPrendas");
clientesToolStripMenuItem.Visible = nombresMenu.Contains("mnuClientes");
bitacoraToolStripMenuItem.Visible = nombresMenu.Contains("mnuAuditoria");
// etc.
```

### 1.9 Roles existentes en el sistema

| Rol | Permisos habilitados |
|---|---|
| Administrador | Todos (inventario, ventas, administrar, bitácora) |
| Vendedor | Clientes, Planes, Pedidos de Venta |
| OperadorLogistico | Prendas, Pedidos Realizados |
| ControladorDeStock | Prendas, Stock |
| OperadorDeInventario | Pedidos Realizados |
| Supervisor | Bitácora |

---

## 2. T06b — Control de Cambios

### 2.1 Objetivo

Proveer trazabilidad completa sobre todos los cambios realizados en el ciclo de vida de un **Pedido**, respondiendo a las preguntas: ¿quién realizó el cambio?, ¿cuándo?, y ¿qué cambió exactamente? El sistema debe mantener un historial con el valor anterior y el valor nuevo de cada campo modificado, y permitir **recomponer el estado anterior** de un pedido para cualquier operación registrada.

### 2.2 Descripción detallada de cómo funciona

Cada vez que un Pedido cambia de estado (se crea, despacha, entrega, cancela, etc.), `BLL.Pedido` registra uno o más registros en la tabla `PedidoHistorial`. Todos los campos modificados en una misma operación comparten el mismo `IdOperacion`, lo que permite agrupar y revertir el evento completo de forma atómica.

El usuario puede abrir el historial de cualquier pedido desde `PedidosVenta` o `PedidosRealizados`, filtrar por fecha y tipo de acción, y restaurar el pedido al estado previo a cualquier operación.

### 2.3 Estructura de la tabla `PedidoHistorial`

| Campo | Tipo | Descripción |
|---|---|---|
| `IdHistorial` | INT IDENTITY | PK, clave única del registro |
| `IdPedido` | INT | FK al pedido auditado |
| `IdOperacion` | INT | Agrupa todos los campos de un mismo evento |
| `Fecha` | DATETIME | Cuándo ocurrió el cambio |
| `IdUsuario` | INT (nullable) | Quién realizó el cambio |
| `NombreUsuario` | VARCHAR | Nombre del usuario en el momento del cambio |
| `Accion` | VARCHAR | Tipo de evento: CREAR, DESPACHAR, ENTREGAR, CANCELAR, DESCANCELAR, DEVOLUCION, RESTAURAR |
| `Campo` | VARCHAR | Qué campo cambió: Estado, FechaDespacho, FechaEntrega, MotivoCancelacion, Prendas |
| `ValorAnterior` | VARCHAR (nullable) | Valor del campo antes del cambio |
| `ValorNuevo` | VARCHAR (nullable) | Valor del campo después del cambio |

### 2.4 Ejemplo de registros para una operación

Al cancelar un pedido, se insertan dos registros con el mismo `IdOperacion`:

| IdOperacion | Accion | Campo | ValorAnterior | ValorNuevo |
|---|---|---|---|---|
| 3 | CANCELAR | Estado | Despachado | Cancelado |
| 3 | CANCELAR | MotivoCancelacion | (vacío) | Cliente no retiró |

### 2.5 Registro de cambios — `DAL.PedidoHistorial`

```csharp
public void RegistrarCambios(List<BE.PedidoHistorial> cambios)
{
    acceso.EjecutarTransaccion((conn, tx) =>
    {
        foreach (var c in cambios)
        {
            using (var cmd = new SqlCommand(
                "INSERT INTO PedidoHistorial " +
                "(IdPedido, IdOperacion, Fecha, IdUsuario, NombreUsuario, " +
                " Accion, Campo, ValorAnterior, ValorNuevo) " +
                "VALUES (@IdPedido, @IdOperacion, @Fecha, @IdUsuario, " +
                "        @NombreUsuario, @Accion, @Campo, @ValorAnterior, @ValorNuevo)",
                conn, tx))
            {
                // parámetros...
                cmd.ExecuteNonQuery();
            }
        }
    });
}
```

Todos los registros de un evento se insertan en una única transacción, garantizando consistencia.

### 2.6 Algoritmo de restauración

Al restaurar una operación, el sistema:
1. Obtiene todos los registros del `IdOperacion` seleccionado
2. Por cada registro, revierte el campo al `ValorAnterior` en la tabla `Pedido`
3. Registra un nuevo evento `RESTAURAR` en el historial

```csharp
// En BLL.Pedido.RestaurarOperacion():
var cambios = historialDAL.ObtenerPorOperacion(idPedido, idOperacion);

foreach (var c in cambios)
{
    if (c.Campo == "Prendas") continue;  // informativo — no se modifica directamente
    historialDAL.RestaurarCampo(idPedido, c.Campo, c.ValorAnterior);
}

// Registra la restauración en el historial
LogRestaurar(modulo, idPedido, idOperacion, cambios);
```

La restauración de campos tipados convierte el string almacenado al tipo correcto:

```csharp
// DAL.PedidoHistorial.RestaurarCampo():
case "Estado":
    Enum.TryParse(valorAnterior, out BE.EstadoPedido estado);
    sql = "UPDATE Pedido SET Estado = @Valor WHERE IdPedido = @IdPedido";
    break;

case "FechaDespacho":
    object fecha = string.IsNullOrEmpty(valorAnterior)
        ? (object)DBNull.Value
        : DateTime.ParseExact(valorAnterior, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
    sql = "UPDATE Pedido SET FechaDespacho = @Valor WHERE IdPedido = @IdPedido";
    break;
// etc.
```

### 2.7 Interfaz de usuario — `PedidoHistorialForm`

Accesible desde PedidosVenta y PedidosRealizados mediante el botón "📋 Historial". Permite:

- Ver todos los eventos del pedido ordenados por fecha descendente
- Filtrar por rango de fechas (con checkboxes habilitadores) y por tipo de acción
- Seleccionar una fila y restaurar el pedido al estado anterior a esa operación

### 2.8 Acciones registradas

| Acción | Cuándo se registra | Campos que registra |
|---|---|---|
| CREAR | Al crear el pedido | Estado, Prendas (lista de prendas incluidas) |
| DESPACHAR | Al cambiar estado a Despachado | Estado, FechaDespacho |
| ENTREGAR | Al registrar la entrega | Estado, FechaEntrega |
| CANCELAR | Al cancelar el pedido | Estado, MotivoCancelacion |
| DESCANCELAR | Al reactivar un pedido cancelado | Estado, MotivoCancelacion |
| DEVOLUCION | Al registrar una devolución | Estado |
| RESTAURAR | Al revertir una operación anterior | (referencia a la operación revertida) |

---

## 3. T05 — Gestión de Múltiples Idiomas (Patrón Observer)

### 3.1 Objetivo

Implementar un sistema de traducción de la interfaz de usuario que permita cambiar el idioma de todos los formularios en tiempo de ejecución, sin cerrarlos ni recargarlos, y sin utilizar hojas de recursos estáticos. Las traducciones deben almacenarse en base de datos y poder gestionarse desde el propio sistema. Se utiliza el patrón **Observer** para notificar automáticamente a todos los formularios abiertos cuando el idioma cambia.

### 3.2 Descripción detallada de cómo funciona

El sistema soporta tres idiomas: Español (ES), Inglés (EN) y Ruso (RU). Las traducciones se almacenan en la base de datos y se cargan en memoria al cambiar el idioma (un único SELECT). Cuando el usuario selecciona un idioma, el sujeto (`GestorIdioma`) notifica a todos los formularios registrados como observers, que actualizan sus controles de forma inmediata.

Desde `FormIdiomas`, el Administrador puede activar/desactivar idiomas y editar cualquier traducción. Si el idioma editado es el actualmente activo, el sistema recarga el diccionario y notifica a todos los forms abiertos en el acto.

### 3.3 Roles del patrón

| Rol Observer | Clase en el sistema | Capa |
|---|---|---|
| **Subject (Sujeto)** | `Servicios.Multiidioma.GestorIdioma` | Servicios |
| **Observer (Interfaz)** | `Servicios.Multiidioma.IIdiomaObserver` | Servicios |
| **Observers concretos** | Todos los formularios (ver sección 3.5) | GUI |
| **Dato de notificación** | `Servicios.Multiidioma.Idioma` | Servicios |

### 3.4 El Subject — `GestorIdioma`

```csharp
public static class GestorIdioma
{
    private static readonly IList<IIdiomaObserver> _observers = new List<IIdiomaObserver>();
    private static Idioma _idiomaActual;
    private static Dictionary<string, string> _tradActuales = null;

    // Attach
    public static void SuscribirObservador(IIdiomaObserver observer)
    {
        if (!_observers.Contains(observer))
            _observers.Add(observer);
    }

    // Detach
    public static void DesuscribirObservador(IIdiomaObserver observer)
    {
        _observers.Remove(observer);
    }

    // Notify — con traducciones precargadas desde BD (un solo SELECT)
    public static void CambiarIdioma(Idioma idioma, Dictionary<string, string> traducciones)
    {
        _idiomaActual = idioma;
        _tradActuales = traducciones;  // cache en memoria
        Notificar(idioma);
    }

    private static void Notificar(Idioma idioma)
    {
        // Itera sobre una copia para evitar errores si un observer
        // se desuscribe durante la notificación
        var copia = new List<IIdiomaObserver>(_observers);
        foreach (var observer in copia)
        {
            try { observer.UpdateLanguage(idioma); }
            catch (Exception ex) { Debug.WriteLine(...); }
        }
    }
}
```

### 3.5 La interfaz Observer — `IIdiomaObserver`

```csharp
public interface IIdiomaObserver
{
    void UpdateLanguage(Idioma idioma);
}
```

### 3.6 Observers concretos — formularios del sistema

Todos los formularios implementan `IIdiomaObserver` con el mismo patrón:

```csharp
public partial class Clientes : FormBase, IIdiomaObserver
{
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        GestorIdioma.SuscribirObservador(this);   // Attach al abrir
        Traducir(GestorIdioma.IdiomaActual);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        GestorIdioma.DesuscribirObservador(this);  // Detach al cerrar
        base.OnFormClosing(e);
    }

    public void UpdateLanguage(Idioma idioma) => Traducir(idioma);  // Update

    private void Traducir(Idioma idioma)
    {
        var t = Traductor.ObtenerTraducciones(idioma);
        string T(string k, string fb) => t.ContainsKey(k) ? t[k].Texto : fb;

        this.Text     = T("frm.clientes", "Gestión de Clientes");
        btnNuevo.Text = T("btn.nuevo",    "Nuevo");
        // ...
    }
}
```

Formularios que implementan `IIdiomaObserver`:

| Formulario | Momento de suscripción |
|---|---|
| `Login` | Al iniciar la aplicación |
| `Menu` | Al ingresar al sistema |
| `Clientes` | Al abrir el módulo |
| `Prendas` | Al abrir el módulo |
| `Usuarios` | Al abrir el módulo |
| `Planes` | Al abrir el módulo |
| `Bitacora` | Al abrir el módulo |
| `PedidosVenta` | Al abrir el módulo |
| `PedidosRealizados` | Al abrir el módulo |
| `NuevoPedidoForm` | Al abrir el formulario |
| `PedidoHistorialForm` | Al abrir el formulario |
| `FormIdiomas` | Al abrir el módulo |
| `GestorPermisos` | Al abrir el módulo |
| `OlvideContrasenaForm` | Al abrir el formulario |

### 3.7 Flujo completo de un cambio de idioma

```
Usuario clickea "EN" (botón en Menu o en Login)
        │
        ▼
BLL.IdiomaService.CargarTraducciones("EN")
        │   UN solo SELECT a BD → Dictionary<clave, texto>
        ▼
GestorIdioma.CambiarIdioma(idiomaEN, dictionary)
        │   guarda idioma y traducciones en memoria (_tradActuales)
        ▼
GestorIdioma.Notificar(idiomaEN)
        │   itera sobre copia de _observers
        ▼
observer.UpdateLanguage(idiomaEN)  ← llamado en CADA formulario abierto
        │
        ▼
Formulario llama Traductor.ObtenerTraducciones(idioma)
        │   lee de _tradActuales en memoria — sin tocar BD
        ▼
Reasigna .Text de cada control visible
```

### 3.8 Traducciones en base de datos

| Tabla | Contenido |
|---|---|
| `Idioma` | `IdIdioma`, `Codigo` (ES/EN/RU), `Nombre`, `Activo`, `EsDefault` |
| `Control` | `IdControl`, `Clave` (ej: `"btn.nuevo"`), `Formulario` |
| `Traduccion` | `IdControl`, `IdIdioma`, `Texto` — PK compuesta |

Al primer arranque, `BLL.IdiomaService.SeedearDesdeHardcode()` carga los diccionarios del código a BD automáticamente. A partir de ese momento, las traducciones se editan desde `FormIdiomas`.

### 3.9 Edición en vivo de traducciones — `FormIdiomas`

```csharp
// Después de guardar traducciones con éxito:
if (idiomaEditado.Codigo == GestorIdioma.IdiomaActual.Id)
{
    var dictActualizado = _bllIdioma.CargarTraducciones(idiomaActual.Id);
    GestorIdioma.CambiarIdioma(idiomaActual, dictActualizado);
    // → todos los formularios abiertos se retraducen en el acto
}
```

### 3.10 Fallback ante falta de traducciones

Si una clave no tiene traducción en BD, `Traductor` usa el diccionario hardcodeado. Si tampoco existe, cada control tiene un valor de fallback en el código:

```csharp
string T(string key, string fallback) =>
    t.ContainsKey(key) ? t[key].Texto : fallback;

this.Text = T("frm.clientes", "Gestión de Clientes");
//                              ↑ aparece si no hay traducción
```

---

## 4. Relación entre los tres patrones

**Composite y Observer** coexisten en `GestorPermisos`:
- Implementa **Composite** para mostrar y editar el árbol de permisos
- Implementa **Observer** para traducir los nombres de roles, grupos y patentes al cambiar el idioma

```csharp
public class GestorPermisos : FormBase, IIdiomaObserver
{
    public void UpdateLanguage(Idioma idioma) => Traducir(idioma);

    private void Traducir(Idioma idioma)
    {
        // traduce etiquetas y botones...
        if (_cmbRol.SelectedIndex >= 0)
            MostrarPermisos();  // reconstruye el árbol con nombres en el nuevo idioma
    }

    private void MostrarPermisosRecursivo(BE.Componente c, TreeNode parent) { ... }
}
```

**Control de Cambios y Observer** coexisten en `PedidoHistorialForm`:
- Implementa **T06b** para mostrar y restaurar el historial del pedido
- Implementa **Observer** para traducirse automáticamente al cambiar el idioma

Los tres patrones comparten la misma arquitectura de capas: la lógica vive en BLL/DAL/Servicios, la presentación en GUI, y las entidades en BE — sin dependencias cruzadas indebidas.
