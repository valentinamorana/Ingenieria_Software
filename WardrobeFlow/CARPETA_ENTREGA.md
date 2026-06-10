# WardrobeFlow — Documentación de Entrega (Iteración 2)
**Materia:** Ingeniería de Software — UAI 2026  
**Proyecto:** WardrobeFlow — Sistema de Gestión de Suscripciones de Indumentaria  
**Stack:** C# / .NET Framework 4.7.2 / Windows Forms / SQL Server / ADO.NET puro

---

## Índice

1. [Descripción del Sistema](#descripción-del-sistema)
2. [Arquitectura del Sistema (T01)](#t01--arquitectura-del-sistema)
3. [Login, Logout y Gestión de Usuarios (T02)](#t02--login-logout-y-gestión-de-usuarios)
4. [Gestión de Encriptado (T03)](#t03--gestión-de-encriptado)
5. [Gestión de Perfiles de Usuario (T04)](#t04--gestión-de-perfiles-de-usuario--patrón-composite)
6. [Gestión de Múltiples Idiomas (T05)](#t05--gestión-de-múltiples-idiomas--patrón-observer)
7. [Bitácora del Sistema y de Negocio (T06)](#t06--bitácora-del-sistema-y-de-negocio)
8. [Control de Cambios e Historial (T06b)](#t06b--control-de-cambios-e-historial)
9. [Dígitos Verificadores (T07)](#t07--dígitos-verificadores-dvh--dvv)
10. [Criterios Transversales de Evaluación](#criterios-transversales-de-evaluación)
11. [Preguntas para Aclarar con el Profesor](#preguntas-para-aclarar-con-el-profesor)

---

## Descripción del Sistema

**WardrobeFlow** es un sistema de escritorio (MDI) para la gestión de suscripciones de indumentaria. Permite a una empresa de alquiler de ropa administrar clientes, prendas, planes de suscripción y pedidos de venta. El sistema está dirigido a empleados internos, con una **jerarquía de roles** (Administrador, Auditor, Vendedor, Gerente Comercial, Operador de Inventario, Operador Logístico, Gerente de Inventario) y no tiene interfaz pública. Los roles son nodos del árbol Composite y pueden crearse/editarse/anidarse desde la administración.

**Entidades principales:**
- **Usuario** — empleado del sistema con rol y contraseña encriptada
- **Cliente** — suscriptor del servicio con plan asociado
- **Prenda** — ítem de indumentaria con estado (Disponible / En Uso / Mantenimiento / Baja)
- **Plan de Suscripción** — define el límite de prendas y precio mensual
- **Pedido de Venta** — asocia clientes con prendas en un período determinado

---

## T01 — Arquitectura del Sistema

### Arquitectura en 4 capas (sin frameworks de persistencia)

El sistema implementa una arquitectura de N capas estricta. No utiliza Entity Framework, NHibernate ni ningún ORM. Todo el acceso a datos se realiza con **ADO.NET puro** (`SqlConnection`, `SqlCommand`, `SqlDataAdapter`).

| Capa | Proyecto | Responsabilidad |
|------|----------|----------------|
| **BE** (Business Entities) | `BE.dll` | Clases de entidad puras — sin lógica, sin consultas. Solo propiedades y constructores. |
| **DAL** (Data Access Layer) | `DAL.dll` | Acceso a SQL Server mediante ADO.NET. Consultas parametrizadas, mapeo a objetos BE. No contiene reglas de negocio. |
| **BLL** (Business Logic Layer) | `BLL.dll` | Toda la lógica de negocio. Validaciones, cálculos de DV, control de acceso por rol, generación de snapshots. Nunca expone SQL ni conexiones. |
| **GUI** (Presentación) | `GUI.exe` | Formularios WinForms MDI. Solo llama a BLL. No tiene acceso a DAL ni a Seguridad directamente. |
| **Servicios** | `Servicios.dll` | Concerns transversales: bitácora del sistema, bitácora de negocio, sistema de multiidioma (Observer). |
| **Seguridad** | `Seguridad.dll` | Gestión de sesión (Singleton), encriptado (PBKDF2 + AES), conteo de intentos fallidos, algoritmo de dígitos verificadores. |

**Verificación:** Una búsqueda de `SqlConnection` en todos los archivos de la capa GUI devuelve cero resultados — la GUI nunca toca la base de datos directamente.

### Diagrama de flujo de arranque (Program.cs)

```
Program.Main()
  │
  ├─ 1. BLL.Configuracion.VerificarConexionDAL()
  │       Si falla → MessageBox + cierre
  │
  ├─ 2. BLL.Configuracion.VerificarIntegridadDV()   ← T07
  │       Si falla → RestauracionForm (admin puede reparar)
  │       Si el admin no repara → cierre
  │
  └─ 3. Login (ShowDialog)
          Si OK → Application.Run(Menu)   ← MDI principal
          Si cancela → Application.Exit()
```

### Mapa de navegación MDI

El formulario `Menu.cs` actúa como contenedor MDI. La barra de menú expone los módulos según el rol del usuario autenticado (los ítems se habilitan/deshabilitan en función de los permisos de la sesión activa). Los módulos accesibles son:

- **Inventario:** Prendas
- **Ventas:** Clientes · Planes de Suscripción · Pedidos de Venta · Pedidos Realizados
- **Administrar:** Usuarios · Perfiles y Permisos · Bitácora · Historial de Cambios · Gestión de Idiomas · Backup y Restauración
- **Dashboard** (vista resumen con actividad reciente)
- **Reporte de Jornada** (exportación PDF)

---

## T02 — Login, Logout y Gestión de Usuarios

### Patrón Singleton — SessionManager

La sesión del usuario autenticado se gestiona mediante `Seguridad/SessionManager.cs`, implementado como **Singleton** con constructor privado. La creación y destrucción de la sesión (`Login` / `Logout`) se hacen **bajo `lock`** para que dos hilos no puedan abrir o cerrar sesión a la vez; las lecturas (`GetInstance` / `IsLoggedIn`) son consultas simples del estado actual:

```csharp
// Seguridad/SessionManager.cs
private static SessionManager _session;
private static object _lock = new object();

public static void Login(Usuario usuario)
{
    lock (_lock)
    {
        if (_session == null)
            _session = new SessionManager { Usuario = usuario, FechaInicio = DateTime.Now };
        else
            throw new Exception("Sesión ya iniciada.");
    }
}
```

> `SessionManager.TienePermiso(...)` re-valida los permisos en el **backend** (el Administrador tiene acceso total; sin usuario, se rechaza), de forma que la GUI ocultando menús nunca es la única barrera.

- `SessionManager.IsLoggedIn` — verificación sin excepción (usada en guards de BLL)
- `SessionManager.GetInstance` — acceso a la sesión activa (lanza excepción si no hay sesión)
- `SessionManager.Logout()` — destruye la instancia y registra el evento en bitácora

### Políticas de login / logout

| Evento | Comportamiento |
|--------|---------------|
| Credenciales vacías | `LoginException(TipoLogin.CamposVacios)` — no consume intento |
| Usuario no encontrado | `LoginException(TipoLogin.UsuarioInvalido)` |
| Contraseña incorrecta | Incrementa contador de intentos fallidos. Al 3er intento: bloquea cuenta + loguea en bitácora |
| Cuenta bloqueada | `LoginException(TipoLogin.CuentaBloqueada)` — no permite ingresar aunque la clave sea correcta |
| Login exitoso | Crea SessionManager, carga traducciones desde BD, loguea en bitácora, abre Menu MDI |
| Logout | Destruye SessionManager, loguea en bitácora, cierra todos los formularios hijos, vuelve a Login |

### Gestión de Usuarios (ABM)

- **Alta:** Asignación de username, rol y contraseña inicial (hasheada con PBKDF2)
- **Reset de contraseña:** Solo Administrador; requiere confirmación de credenciales; genera snapshot automático antes de modificar (T06b)
- **Desbloqueo de cuenta:** Solo Administrador; genera snapshot automático (T06b)
- **Recálculo de DV:** Disponible para Administrador desde el módulo Usuarios
- El **Administrador no puede darse de baja a sí mismo** — verificado en BLL

---

## T03 — Gestión de Encriptado

### Algoritmo de hash unidireccional — PBKDF2-SHA256

Las contraseñas **nunca se almacenan en texto plano**. Se usa PBKDF2 con SHA-256:

```
Seguridad/Encriptador.cs — Hash()
  Salt aleatorio: 16 bytes (RandomNumberGenerator)
  Iteraciones:    100.000
  Tamaño hash:    32 bytes
  Formato en BD:  Base64( Salt[16] + Hash[32] )
```

El salt aleatorio garantiza que dos usuarios con la misma contraseña tengan hashes distintos. La verificación (`Verificar()`) extrae el salt del hash almacenado y recalcula.

### Algoritmo simétrico reversible — AES-128-CBC

Para datos sensibles que necesitan ser leídos (ej.: DNI de clientes), se usa AES-128 en modo CBC con IV aleatorio:

```
Seguridad/Encriptador.cs — Encriptar() / Desencriptar()
  Clave: 128 bits ALEATORIOS, generados la primera vez y guardados en key.dat
         junto al ejecutable, PROTEGIDOS con DPAPI (ProtectedData, ámbito del
         usuario actual). No hay clave embebida en el código.
  IV:    aleatorio por operación
  Formato: Base64( IV[16] + CipherText )
```

> `key.dat` queda fuera del control de versiones (`.gitignore`). Eliminarlo vuelve irrecuperables los DNI ya cifrados. Se migra automáticamente un `key.dat` legacy en texto plano a DPAPI sin cambiar la clave.

---

## T04 — Gestión de Perfiles de Usuario — Patrón Composite

### Motivación

El sistema de permisos necesita representar una jerarquía de permisos en árbol: los permisos se agrupan en familias (carpetas) y las patentes son los permisos atómicos (hojas). Un rol tiene asignado un subconjunto de patentes. Esta estructura es naturalmente un **Composite**.

### Implementación del patrón

```
BE/Componente.cs       ← Componente abstracto
├── BE/Familia.cs      ← Composite (nodo con hijos)
│   └── BE/Rol.cs      ← Rol : Familia (perfil asignable; puede contener familias,
│                         patentes y OTROS roles — rol-dentro-de-rol)
└── BE/Patente.cs      ← Leaf (permiso atómico)
```

**`Componente` (clase abstracta):**
```csharp
public abstract IList<Componente> Hijos { get; }
public abstract void AgregarHijo(Componente c);
public abstract void QuitarHijo(Componente c);
public abstract void VaciarHijos();

// Operación RECURSIVA del Composite (estilo GoF): cada nodo resuelve su subárbol.
public IList<Patente> ObtenerPatentesEfectivas();   // hojas alcanzables, sin duplicados
```

**`Familia`** implementa `Hijos` con una lista interna y `AgregarHijo()` efectivo (con **validación anti-ciclos**, así un rol no puede contenerse de forma circular).  
**`Patente`** implementa `Hijos` devolviendo una lista vacía y `AgregarHijo()` como no-op — es una hoja.  
**`Rol`** hereda de `Familia`: en la BD es una fila de `Permiso` con `EsFamilia=1` y `EsRol=1`.

### Construcción del árbol (DAL)

`DAL/Permiso.ObtenerArbol()` reconstruye el árbol desde dos tablas SQL:
- `Permiso` — columna `EsFamilia` discrimina nodo vs. hoja
- `PermisoRelacion` — tabla de relación padre-hijo (IdPadre, IdHijo)

El algoritmo crea primero todos los nodos, luego los enlaza por la tabla de relación.

### Funciones recursivas

**En BLL — `BLL/Familia.MarcarAsignados()`:**  
Recorre el árbol recursivamente y marca `Patente.Asignado = true` para cada permiso que tiene el rol seleccionado.

```csharp
private void MarcarAsignados(IList<Componente> nodos, HashSet<int> ids)
{
    foreach (var nodo in nodos)
    {
        var patente = nodo as BE.Patente;
        if (patente != null)
            patente.Asignado = ids.Contains(patente.Id);

        if (nodo.Hijos.Count > 0)
            MarcarAsignados(nodo.Hijos, ids);  // ← RECURSIÓN
    }
}
```

**En GUI — `GUI/GestorPermisos.MostrarPermisosRecursivo()`:**  
Construye el TreeView recursivamente. Las Familias se muestran en **negrita azul**; las Patentes con **checkbox** para asignar/quitar permisos.

**En GUI — `GUI/GestorPermisos.GuardarRecursivo()`:**  
Recorre el TreeView recursivamente y acumula los IDs de patentes a asignar o quitar para el rol.

### Acceso

- La gestión de perfiles y permisos la puede hacer el **Administrador** (acceso total por bypass) **o cualquier rol que tenga la patente *Gestión de Usuarios*** (delegación), siempre **re-validado en la BLL** antes de cada operación (*fail-closed*: sin sesión se rechaza).
- **Anti-autobloqueo:** un usuario no-admin no puede quitarse a sí mismo el acceso de gestión editando su propio rol.
- El TreeView muestra el árbol completo del sistema para el rol seleccionado; la asignación se maneja por **dos listas** (Familias / Patentes) y permite **embeber un rol dentro de otro** (con prevención de ciclos).
- Los **permisos efectivos** de un usuario se resuelven **recursivamente** en `BLL.Familia.ObtenerPermisosEfectivos` (rol → roles/familias → patentes, deduplicando).

---

## T05 — Gestión de Múltiples Idiomas — Patrón Observer

### Diseño

El sistema soporta **4 idiomas: Español (ES), English (EN), Русский (RU) y Português (PT)**, y se pueden **agregar idiomas nuevos en caliente** desde la administración (`FormIdiomas`). El cambio de idioma es **dinámico en tiempo de ejecución** — no requiere reiniciar la aplicación. No se usan archivos `.resx` ni recursos estáticos.

### Implementación del patrón Observer

```
Servicios/Multiidioma/
├── IIdiomaObserver.cs   ← Interfaz Observer
├── GestorIdioma.cs      ← Subject (Sujeto)
└── Traductor.cs         ← Fuente de traducciones (BD + fallback hardcodeado)
```

**`IIdiomaObserver` (interfaz Observer):**
```csharp
void UpdateLanguage(Idioma idioma);
```

**`GestorIdioma` (Subject estático):**
- Mantiene la lista de observadores suscritos
- `SuscribirObservador()` / `DesuscribirObservador()` — registro
- `CambiarIdioma(idioma, traducciones)` — actualiza el idioma activo y notifica a todos
- `Notificar()` — itera la lista con copia defensiva (evita `InvalidOperationException` si un observer se desuscribe durante la notificación); errores en observers individuales no interrumpen al resto

**Ciclo de vida en cada formulario:**
```csharp
protected override void OnLoad(EventArgs e)
{
    base.OnLoad(e);
    GestorIdioma.SuscribirObservador(this);   // ← suscribe al abrir
    Traducir(GestorIdioma.IdiomaActual);       // ← traduce al idioma actual
}

protected override void OnFormClosing(FormClosingEventArgs e)
{
    GestorIdioma.DesuscribirObservador(this);  // ← desuscribe al cerrar (evita memory leaks)
    base.OnFormClosing(e);
}

public void UpdateLanguage(Idioma idioma) => Traducir(idioma);  // ← callback del Observer
```

**19 formularios** implementan `IIdiomaObserver`: Login, Menu, Clientes, Prendas, Usuarios, Planes, Bitácora, PedidosVenta, PedidosRealizados, NuevoPedidoForm, OlvideContrasenaForm, GestorPermisos, FormIdiomas, BackupForm, ReporteJornadaForm, DashboardForm, VersionHistorialForm, RestauracionForm, PedidoHistorialForm.

### Gestión de traducciones

**Fuente primaria — BD:** `BLL/IdiomaService.CargarTraducciones()` lee todas las traducciones del idioma activo desde la tabla `Traduccion` de SQL Server y las entrega como `Dictionary<string, string>` a `GestorIdioma`.

**Fuente secundaria (fallback) — código:** Si la BD no responde o está vacía (primer arranque), `Servicios/Multiidioma/Traductor.cs` tiene los 4 diccionarios hardcodeados (ES, EN, RU, PT) con cientos de claves de traducción cada uno, y un **fallback por clave** (si falta una traducción se usa el texto del idioma por defecto).

**Seeding automático:** Si la tabla `Traduccion` está vacía, `BLL/IdiomaService.SeedearDesdeHardcode()` la puebla automáticamente desde los diccionarios hardcodeados al primer arranque.

### ABM de idiomas e idiomas desde el sistema

`GUI/FormIdiomas.cs` permite al Administrador:
- Ver la lista de idiomas registrados en la BD (activos/inactivos)
- Activar o desactivar idiomas (un idioma inactivo no aparece en el selector)
- Editar las traducciones de cualquier clave directamente en la grilla
- Los cambios se persisten en BD y se reflejan en la siguiente carga de idioma

---

## T06 — Bitácora del Sistema y de Negocio

### Separación Sistema vs. Negocio

El sistema implementa **dos bitácoras independientes**:

| | Bitácora del Sistema | Bitácora de Negocio |
|-|---------------------|---------------------|
| **Clase BE** | `BE/Bitacora.cs` | `BE/BitacoraNegocio.cs` |
| **Servicio** | `Servicios/Bitacora.cs` | `Servicios/BitacoraNegocio.cs` |
| **Tabla SQL** | `BitacoraSistema` | `BitacoraNegocio` |
| **Qué registra** | Autenticación, seguridad, cambios de usuarios, DV | Altas/bajas/modificaciones de clientes, prendas, pedidos, planes |
| **Quién la ve** | Administrador y Auditor | Administrador y Auditor |

La UI (`GUI/Bitacora.cs`) muestra las dos en **pestañas separadas** dentro del mismo formulario.

### Datos capturados por evento

Cada registro de bitácora incluye:
- **Fecha y hora** exacta del evento
- **Usuario** que realizó la acción (ID + username desde SessionManager)
- **Módulo** de origen (nombre del formulario)
- **Actividad** — descripción de la operación
- **Detalle** — información adicional contextual (ID del registro afectado, valores previos/nuevos)
- **Criticidad** — `None / Baja / Media / Alta` (enum `BE/Criticidad.cs`)
- **IP** del equipo donde se ejecutó la operación

### Registros sin sesión

`Servicios/Bitacora.RegistrarSinSesion()` permite registrar eventos que ocurren antes del login (intentos fallidos, bloqueos de cuenta, solicitudes de recuperación de clave) sin requerir `SessionManager` activo.

### Cobertura de eventos logueados (BLL)

| Evento | Criticidad |
|--------|-----------|
| Inicio de sesión | None |
| Cierre de sesión | None |
| Intento fallido | Baja |
| Cuenta bloqueada | Media |
| Alta de usuario | Baja |
| Reset de contraseña | Alta |
| Desbloqueo de cuenta | Media |
| Restauración de versión (T06b) | Alta |
| Alta / modificación / baja de cliente | Baja |
| Alta / modificación / baja de prenda | Baja |
| Alta / modificación de pedido | Baja |
| Despacho / entrega / devolución | Baja |
| Recálculo de DV | Alta |
| Generación / restauración de backup | Alta |

**Total: 41 llamadas a `Registrar()` distribuidas en 8 archivos BLL.**

### Filtros de búsqueda

La UI de bitácora permite filtrar por:
- **Rango de fechas** (desde / hasta, con DateTimePicker)
- **ID de usuario**
- **Actividad** (texto libre)
- **Criticidad** (ComboBox con enum)
- **Últimos N días** (con valor 0 = todos)

Todos los filtros son combinables y se aplican en una única consulta SQL parametrizada.

---

## T06b — Control de Cambios e Historial

### Entidad trazada: Usuario

La entidad más sensible del sistema (Usuario) tiene trazabilidad completa de sus modificaciones. Cada vez que se realiza una operación sobre un usuario (reset de contraseña, desbloqueo, restauración), se captura un **snapshot** del estado previo.

### Estructura del snapshot (BE/VersionUsuario.cs)

```csharp
public int    Id                { get; set; }  // ID del snapshot
public int    IdUsuario         { get; set; }  // usuario afectado
public DateTime Fecha           { get; set; }  // momento del cambio
public string Actor             { get; set; }  // quién hizo el cambio
public string Detalle           { get; set; }  // descripción de la operación
public string UsernameSnapshot  { get; set; }  // username ANTES del cambio
public string ClaveSnapshot     { get; set; }  // hash de contraseña ANTES del cambio
public bool   EstadoSnapshot    { get; set; }  // activo/bloqueado ANTES del cambio
public int    IntentosSnapshot  { get; set; }  // intentos fallidos ANTES del cambio
```

### Flujo de captura automática

El snapshot se graba **antes** de la operación, no después, para preservar el estado previo:

```
BLL/Usuario.ResetearClave()
  1. BLL.VersionUsuario.GrabarVersion(idUsuario, actor, "Reset de contraseña")  ← snapshot PREVIO
  2. DAL/Usuario.ResetearClave(idUsuario, nuevaClave)                             ← operación
  3. Bitacora.Registrar("Reset Contrasena", Criticidad.Alta)                     ← log
```

La misma lógica aplica para `DesbloquearCuenta()`.

### Restauración de versión

`BLL/VersionUsuario.RestaurarVersion(modulo, idVersion)`:
1. Verifica que hay sesión activa y que la versión existe
2. **Graba un snapshot del estado ACTUAL** antes de restaurar (trazabilidad de la restauración)
3. Restaura el usuario al estado del snapshot seleccionado via `DAL/Usuario.RestaurarVersion()`
4. Registra el evento en bitácora con `Criticidad.Alta`

Esto garantiza que la restauración en sí misma es reversible.

### UI — VersionHistorialForm

- Selector de usuario (ComboBox)
- Botón "Cargar" → grilla con ID, Fecha, Actor, Detalle, Estado del snapshot
- Botón "Restaurar Versión Seleccionada" → confirmación + restauración
- El formulario implementa `IIdiomaObserver` (traducción dinámica completa, incluyendo todos los MessageBox)

---

## T07 — Dígitos Verificadores (DVH + DVV)

### Motivación

Los dígitos verificadores permiten detectar manipulaciones externas a la base de datos (edición directa de SQL Server, intercambio de registros, inserción o eliminación de filas) que no pasarían por las validaciones del sistema.

### Algoritmo

Implementado en `Seguridad/DigitoVerificador.cs`:

> **Módulo:** se usa el primo **999_983** (no `mod 10`), lo que da hasta ~1.000.000 de valores posibles de DV en vez de 10 — muchísimo más resistente a colisiones. Cabe en el `INT` de SQL Server.

**DVH (Dígito Verificador Horizontal) — por fila:**
```
suma     = 0
campoIdx = 1
para cada campo de la fila:
    charPos = 1
    para cada carácter del campo:
        suma += ASCII(carácter) × charPos × campoIdx   // doble ponderación
        charPos++
    campoIdx++
DVH = suma mod 999_983
```
La doble ponderación (por posición de carácter **y** por índice de campo) hace que mover el mismo carácter entre campos, o intercambiar valores entre campos, produzca DVH distintos. Detecta: alteración de cualquier campo individual e intercambio de valores entre campos.

**DVV (Dígito Verificador Vertical) — por tabla:**
```
suma = 0
para cada fila i (0-indexed):
    suma += DVH_i × (i + 1)
DVV = suma mod 999_983
```
Detecta: inserción de filas, eliminación de filas, intercambio del orden de filas.

### Almacenamiento

- **DVH:** columna `DVH` en cada tabla protegida (se recalcula en cada INSERT/UPDATE)
- **DVV:** tabla separada `DVVertical` con columnas `NombreTabla` y `DVV` (una fila por tabla)
- **Alcance:** el DV protege **Usuario** (la entidad más sensible) y además **Cliente**, **Empleado** y **Pedido** (objeto multi-tabla: pedido + líneas). La implementación de recálculo/verificación es genérica y reutilizable (`DAL.DigitoVerificador.RecalcularTabla(tabla, pk, columnas)`).

### Verificación pre-login (Program.cs → BLL/Configuracion.cs)

Antes de mostrar el formulario de Login, `VerificarIntegridadDV()` ejecuta:

1. Lee todas las filas de `Usuario` con sus DVH almacenados
2. Recalcula el DVH de cada fila con los datos actuales
3. Compara DVH recalculado vs. DVH almacenado → detecta alteración de campos
4. Calcula DVV sobre los DVH recalculados
5. Compara DVV calculado vs. DVV en `DVVertical` → detecta inserciones/eliminaciones/permutas de filas
6. Si hay discrepancia: detalla qué filas fallaron, retorna `false` y **bloquea el Login**

**Caso borde — primer arranque:** Si todos los DVH son 0 y el DVV también es 0 (migración inicial sin DV), el sistema recalcula automáticamente y continúa.

### Restauración ante fallo de integridad

Si `VerificarIntegridadDV()` retorna `false`, `Program.cs` abre `GUI/RestauracionForm` que ofrece dos opciones (ambas requieren autenticación de Administrador):

1. **Recalcular Dígitos Verificadores** — útil si los datos son correctos pero los DV están desactualizados
2. **Restaurar desde Backup** — reemplaza la BD por un backup limpio

### Recálculo administrativo

Desde `GUI/Usuarios.cs`, el Administrador puede forzar el recálculo de todos los DVH y DVV del sistema. La operación queda registrada en la bitácora con `Criticidad.Alta`.

---

## Criterios Transversales de Evaluación

### 1. Arquitectura 4 capas sin ORM ✅
Verificado en código: ningún archivo de GUI contiene `SqlConnection`. ADO.NET puro en toda la capa DAL.

### 2. UI amigable y manejo de excepciones ✅
- `GUI/FormBase.cs` provee `MostrarOk()` (verde ✓) y `MostrarError()` (rojo ✗) heredados por todos los formularios
- Excepciones técnicas se capturan en la capa GUI y se muestran en lenguaje de usuario
- Excepciones de negocio usan tipos específicos (`LoginException` con `TipoLogin` enum) para feedback contextual
- Título "Error" y mensajes de error están traducidos a los 4 idiomas (ES/EN/RU/PT)

### 3. Calidad de código POO ✅

**Herencia:**
- `FormBase` (hereda de `Form`) → 13 formularios de negocio
- `Componente` → `Familia`, `Patente` (Composite)
- `BaseDAL<T>` → todas las clases DAL

**Encapsulamiento:**
- Campos privados con propiedades públicas en todas las entidades BE
- Constructor privado en Singleton (`Acceso`, `SessionManager`)
- `GestorIdioma` con estado interno privado, acceso controlado

**Polimorfismo:**
- `Componente.Hijos`, `AgregarHijo()`, `VaciarHijos()` — sobrescritos en `Familia` (implementación real) y `Patente` (no-op)
- `IIdiomaObserver.UpdateLanguage()` — implementado de forma diferente en cada formulario
- `FormBase.MensajeLabel` — propiedad virtual sobrescrita en cada form hijo

**Cohesión alta / Acoplamiento bajo:**
- Cada clase tiene una única responsabilidad
- GUI nunca conoce DAL — solo BLL
- BLL nunca conoce GUI — solo recibe parámetros primitivos

### 4. BD normalizada ✅
- Sin redundancia de datos
- Claves foráneas con integridad referencial declarada
- Tablas de relación para M:N (ej.: `PedidoPrenda`, `PermisoRelacion`)
- Tabla `DVVertical` separada para el DVV (no en la misma tabla que los datos)
- Tabla `Traduccion` con clave compuesta (IdIdioma, Clave)

### 5. Seguridad ✅
- Contraseñas hasheadas con PBKDF2-SHA256 (100.000 iteraciones, salt aleatorio)
- Datos sensibles encriptados con AES-128-CBC
- Bloqueo de cuenta tras 3 intentos fallidos
- Verificación de integridad DV antes de cada arranque
- Control de permisos por rol antes de cada operación sensible en BLL

---

## Preguntas para Aclarar con el Profesor

### Sobre T07 — Dígitos Verificadores

**Pregunta 1:** El sistema aplica DVH y DVV sobre `Usuario` (la entidad más sensible, por las credenciales) y además sobre `Cliente`, `Empleado` y `Pedido`. ¿Es adecuado este alcance o se espera cubrir aún más entidades de negocio?

> **Contexto:** El enunciado pide "al menos en la entidad más sensible". Se extendió a las demás entidades sensibles con una implementación genérica reutilizable (`RecalcularTabla`), midiendo que el impacto en el tiempo de verificación al arranque es despreciable.

**Pregunta 2:** En el caso de primer arranque (tabla con DVH = 0), el sistema recalcula automáticamente sin interrumpir el flujo. ¿Es correcto este comportamiento o debería siempre pedir intervención del administrador aunque sea la primera vez?

**Pregunta 3:** El campo `ClaveSnapshot` en la tabla `HistorialUsuario` almacena el hash PBKDF2 de la contraseña (no la contraseña en texto plano). Al restaurar, se restaura ese hash. ¿Es correcto este enfoque desde el punto de vista de seguridad para el T07?

---

### Sobre T06b — Control de Cambios

**Pregunta 4:** La trazabilidad completa (quién, cuándo, qué, estado anterior) se implementó para la entidad `Usuario`. ¿Se esperaba también para otras entidades de negocio como `Cliente` o `Prenda`? El enunciado dice "al menos en una entidad", pero ¿hay alguna preferencia?

**Pregunta 5:** La restauración de versión crea un nuevo snapshot del estado actual antes de restaurar (para que la restauración en sí misma sea trazable y reversible). ¿Es este nivel de trazabilidad el esperado, o es excesivo?

---

### Sobre T04 — Perfiles de Usuario

**Pregunta 6:** El patrón Composite se usa para representar el árbol de permisos (Familia → Patente). Los permisos se asignan por **rol** (no por usuario individual). ¿Es correcto este diseño o se esperaba asignación por usuario?

**Pregunta 7:** El sistema trae **7 roles** precargados con jerarquía (Composite rol-en-rol) y permite **crear, editar, anidar y eliminar roles desde la UI** (un rol no se puede eliminar si tiene usuarios asignados). ¿Este nivel de roles dinámicos es el esperado?

---

### Sobre T05 — Múltiples Idiomas

**Pregunta 8:** Las traducciones se almacenan en BD (tabla `Traduccion`) y se cachean en memoria al cambiar de idioma. El fallback hardcodeado en `Traductor.cs` existe solo para el primer arranque sin BD. ¿Se considera que esto cumple el requisito de "sin recursos estáticos"? Los diccionarios hardcodeados son código C#, no archivos `.resx`.

**Pregunta 9:** Los idiomas se gestionan desde el módulo `FormIdiomas`: activar/desactivar, editar traducciones clave por clave **y agregar idiomas completamente nuevos desde la UI** (con una columna "Referencia" que muestra el texto base para asistir la traducción). Hay 4 idiomas registrados (ES/EN/RU/PT). ¿Es suficiente este alcance?

---

### Sobre Arquitectura y Criterios Generales

**Pregunta 10:** Los módulos de negocio (Pedidos, Prendas, Clientes) también registran eventos en la `BitacoraNegocio`. ¿Alcanza con que las excepciones técnicas se muestren al usuario vía `MostrarError()`, o también deben loguearse en la bitácora (actualmente solo se loguean las operaciones exitosas de negocio)?

**Pregunta 11:** Las pruebas unitarias fueron parte de T01 (Entrega 1). ¿Siguen siendo evaluadas en la Entrega 2, y en ese caso aplican sobre qué módulos o clases específicas?

**Pregunta 12:** Los diagramas de clases, DER y casos de uso de la carpeta, ¿deben reflejar todos los módulos del sistema completo o solo los módulos de la Entrega 2 (T04, T05, T06, T06b, T07)?

---

*Documento generado para la Entrega 2 — WardrobeFlow — Ingeniería de Software UAI 2026*
