# Patrones de Diseño en WardrobeFlow
## T04 — Composite y T05 — Observer

---

## 1. Patrón Composite — Gestión de Perfiles de Usuario (T04)

### 1.1 Propósito en el sistema

El patrón Composite permite modelar los permisos del sistema como una jerarquía árbol-hoja. Cada **rol de usuario** (Administrador, Vendedor, etc.) tiene asociado un árbol de permisos organizado en grupos (`Familia`) y permisos individuales (`Patente`). La GUI puede recorrer ese árbol de forma uniforme sin distinguir si está procesando un nodo compuesto o una hoja.

### 1.2 Estructura de clases

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

**`BE.Patente`** — hoja, nunca tiene hijos. Agrega `Asignado` (si el permiso está activo para el rol) y `NombreMenu` (clave de menú):

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

### 1.3 Estructura del árbol en tiempo de ejecución

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

### 1.4 Construcción del árbol — `BLL.Familia`

`BLL.Familia.ObtenerArbolPorRol(rol)` construye el árbol completo en memoria con solo dos consultas a la base de datos:

1. `ObtenerTodos()` — todos los permisos del sistema
2. `ObtenerPorRol(rol)` — los permisos asignados al rol seleccionado

```csharp
public BE.Familia ObtenerArbolPorRol(string rol)
{
    List<BE.Permiso> todos     = permisoDAL.ObtenerTodos();
    List<BE.Permiso> asignados = permisoDAL.ObtenerPorRol(rol);

    var idsAsignados = new HashSet<int>();
    foreach (var p in asignados)
        idsAsignados.Add(p.Id);

    var raiz = new BE.Familia { Nombre = rol };

    // Agrupar permisos por TipoComponente → una Familia por grupo
    var grupos = new Dictionary<string, BE.Familia>();
    foreach (var perm in todos)
    {
        string grupo = perm.TipoComponente ?? "General";
        if (!grupos.ContainsKey(grupo))
            grupos[grupo] = new BE.Familia { Nombre = grupo };

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

### 1.5 Persistencia — `DAL.Permiso`

| Tabla | Contenido |
|---|---|
| `Permiso` | Catálogo de permisos: `IdPermiso`, `Nombre`, `NombreMenu`, `TipoComponente`, `Estado` |
| `RolPermiso` | Relación M:N entre rol (string) y permiso: `Rol`, `IdPermiso` |

Operaciones del DAL:

| Método | Acción |
|---|---|
| `ObtenerTodos()` | SELECT sobre `Permiso`, ordenado por `TipoComponente` |
| `ObtenerPorRol(rol)` | JOIN entre `Permiso` y `RolPermiso` filtrando por rol |
| `AsignarPermiso(rol, id)` | INSERT con IF NOT EXISTS (idempotente) |
| `QuitarPermiso(rol, id)` | DELETE de `RolPermiso` |
| `ObtenerRoles()` | SELECT DISTINCT sobre `RolPermiso` |

### 1.6 Función recursiva — `GUI.GestorPermisos`

La función recursiva es el requisito central de T04. Se implementan dos: una para mostrar el árbol y otra para guardar los cambios.

**Mostrar el árbol (lectura):**

```csharp
private void MostrarPermisosRecursivo(BE.Componente componente, TreeNode nodoParent)
{
    foreach (BE.Componente hijo in componente.Hijos)
    {
        var nodo = new TreeNode(TraducirNombre(hijo)) { Tag = hijo };

        if (hijo is BE.Familia)
        {
            // Nodo de grupo — visual diferenciado, no checkable
            nodo.NodeFont  = new Font("Segoe UI", 9f, FontStyle.Bold);
            nodo.ForeColor = Color.FromArgb(40, 80, 140);
        }
        else if (hijo is BE.Patente patente)
        {
            // Hoja — el checkbox refleja si el permiso está asignado
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

Los nodos `Familia` no son checkables (el evento `BeforeCheck` se cancela si el nodo es una `Familia`). Solo las `Patente` (hojas) generan cambios en base de datos.

### 1.7 Aplicación de permisos al login

Al hacer login, `BLL.Usuario.Login()` carga los permisos del rol en `BE.Usuario.Permisos`. Al abrir el `Menu`, el método `AplicarPermisos()` muestra u oculta cada ítem comparando `NombreMenu` contra los permisos del usuario:

```csharp
var nombresMenu = new HashSet<string>();
foreach (var p in permisos)
    nombresMenu.Add(p.NombreMenu);

prendasToolStripMenuItem.Visible    = nombresMenu.Contains("mnuPrendas");
clientesToolStripMenuItem.Visible   = nombresMenu.Contains("mnuClientes");
bitacoraToolStripMenuItem.Visible   = nombresMenu.Contains("mnuAuditoria");
// etc.
```

### 1.8 Roles existentes en el sistema

| Rol | Permisos |
|---|---|
| Administrador | Todos (inventario, ventas, administrar, bitácora) |
| Vendedor | Clientes, Planes, Pedidos de Venta |
| OperadorLogistico | Prendas, Pedidos Realizados |
| ControladorDeStock | Prendas, Stock |
| OperadorDeInventario | Pedidos Realizados |
| Supervisor | Bitácora |

---

## 2. Patrón Observer — Gestión de Múltiples Idiomas (T05)

### 2.1 Propósito en el sistema

El patrón Observer permite que todos los formularios abiertos se traduzcan automáticamente cuando el usuario cambia el idioma, sin necesidad de cerrarlos ni recargarlos. El cambio de idioma ocurre en caliente (en vivo), mientras la aplicación está en uso.

### 2.2 Roles del patrón

| Rol Observer | Clase en el sistema |
|---|---|
| **Subject (Sujeto)** | `Servicios.Multiidioma.GestorIdioma` |
| **Observer (Interfaz)** | `Servicios.Multiidioma.IIdiomaObserver` |
| **Observers concretos** | Todos los formularios (ver lista en sección 2.5) |
| **Dato de notificación** | `Servicios.Multiidioma.Idioma` |

### 2.3 El Subject — `GestorIdioma`

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

    // Notify — con traducciones precargadas desde BD
    public static void CambiarIdioma(Idioma idioma, Dictionary<string, string> traducciones)
    {
        _idiomaActual = idioma;
        _tradActuales = traducciones;  // cache en memoria — evita N selects
        Notificar(idioma);
    }

    private static void Notificar(Idioma idioma)
    {
        // Se itera sobre una copia para evitar errores si un observer
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

### 2.4 La interfaz Observer — `IIdiomaObserver`

```csharp
public interface IIdiomaObserver
{
    void UpdateLanguage(Idioma idioma);
}
```

### 2.5 Observers concretos — formularios del sistema

Todos los formularios implementan `IIdiomaObserver` siguiendo el mismo patrón: suscripción en `OnLoad`, desuscripción en `OnFormClosing`, traducción en `UpdateLanguage`.

```csharp
public partial class Clientes : FormBase, IIdiomaObserver
{
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        GestorIdioma.SuscribirObservador(this);   // Attach
        Traducir(GestorIdioma.IdiomaActual);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        GestorIdioma.DesuscribirObservador(this);  // Detach
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

### 2.6 Flujo completo de un cambio de idioma

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

### 2.7 Traducciones en base de datos

Las traducciones se persisten en tres tablas:

| Tabla | Contenido |
|---|---|
| `Idioma` | `IdIdioma`, `Codigo` (ES/EN/RU), `Nombre`, `Activo`, `EsDefault` |
| `Control` | `IdControl`, `Clave` (ej: `"btn.nuevo"`), `Formulario` |
| `Traduccion` | `IdControl`, `IdIdioma`, `Texto` — PK compuesta |

Al primer arranque, `BLL.IdiomaService.SeedearDesdeHardcode()` carga los diccionarios hardcodeados del `Traductor` a BD automáticamente. Desde ese momento, las traducciones se editan directamente desde `FormIdiomas`.

### 2.8 Edición en vivo de traducciones — `FormIdiomas`

`FormIdiomas` permite activar/desactivar idiomas y editar traducciones en una grilla. Al guardar, si el idioma editado es el actualmente activo, recarga el diccionario desde BD y notifica a todos los observers:

```csharp
// Después de guardar con éxito:
if (idiomaEditado.Codigo == GestorIdioma.IdiomaActual.Id)
{
    var dictActualizado = _bllIdioma.CargarTraducciones(idiomaActual.Id);
    GestorIdioma.CambiarIdioma(idiomaActual, dictActualizado);
    // → todos los formularios abiertos se retraducen en el acto
}
```

### 2.9 Fallback ante falta de traducciones

Si una clave no tiene traducción en BD, `Traductor.ObtenerTraducciones()` usa el diccionario hardcodeado como respaldo. Si tampoco existe, cada control tiene un valor de fallback en el código:

```csharp
string T(string key, string fallback) =>
    t.ContainsKey(key) ? t[key].Texto : fallback;

this.Text = T("frm.clientes", "Gestión de Clientes");
//                              ↑ aparece si no hay traducción en BD ni en hardcode
```

---

## 3. Relación entre ambos patrones

Ambos patrones coexisten en el mismo formulario `GestorPermisos`:

- Implementa **Composite** para mostrar y editar el árbol de permisos
- Implementa **Observer** para traducirse automáticamente al cambiar el idioma (incluyendo los nombres de roles y grupos del árbol)

```csharp
public class GestorPermisos : FormBase, IIdiomaObserver
{
    // Observer: se traduce cuando cambia el idioma
    public void UpdateLanguage(Idioma idioma) => Traducir(idioma);

    // Al traducir, reconstruye el combo de roles y el árbol TreeView
    // con los nombres en el nuevo idioma
    private void Traducir(Idioma idioma)
    {
        // ...traduce etiquetas y botones...
        if (_cmbRol.SelectedIndex >= 0)
            MostrarPermisos();  // reconstruye el árbol con nombres traducidos
    }

    // Composite: recorre el árbol recursivamente
    private void MostrarPermisosRecursivo(BE.Componente c, TreeNode parent) { ... }
}
```

Lo mismo ocurre en `FormIdiomas`, que es observer de sí mismo: cuando se guarda una traducción del idioma activo, el propio formulario se retraduce junto con el resto del sistema.
