# WardrobeFlow — Documentación Técnica Integral
**Materia:** Ingeniería de Software — UAI 2026  
**Proyecto:** WardrobeFlow — Sistema de Gestión de Suscripciones de Indumentaria  
**Stack:** C# 8.0 / .NET Framework 4.7.2 / Windows Forms MDI / SQL Server / ADO.NET puro  
**Autores:** Bolívar · Morana

---

## Índice

1. [Descripción del Sistema](#1-descripción-del-sistema)
2. [T01 — Arquitectura del Sistema](#2-t01--arquitectura-del-sistema)
3. [T02 — Login, Logout y Gestión de Usuarios](#3-t02--login-logout-y-gestión-de-usuarios)
4. [T03 — Gestión de Encriptado](#4-t03--gestión-de-encriptado)
5. [T04 — Gestión de Perfiles de Usuario (Patrón Composite)](#5-t04--gestión-de-perfiles-de-usuario--patrón-composite)
6. [T05 — Gestión de Múltiples Idiomas (Patrón Observer)](#6-t05--gestión-de-múltiples-idiomas--patrón-observer)
7. [T06a — Bitácora del Sistema y de Negocio](#7-t06a--bitácora-del-sistema-y-de-negocio)
8. [T06b — Control de Cambios e Historial (Patrón Memento)](#8-t06b--control-de-cambios-e-historial--patrón-memento)
9. [T07 — Dígitos Verificadores (DVH + DVV)](#9-t07--dígitos-verificadores-dvh--dvv)
10. [Dashboard por Rol](#10-dashboard-por-rol)
11. [Criterios Transversales de Evaluación](#11-criterios-transversales-de-evaluación)

---

## 1. Descripción del Sistema

**WardrobeFlow** es una aplicación de escritorio Windows Forms (MDI) para la gestión integral de un servicio de suscripción de indumentaria. Permite a una empresa de alquiler de ropa administrar clientes, prendas, planes de suscripción y pedidos de venta. El sistema está orientado exclusivamente a empleados internos con roles diferenciados.

### Entidades principales del dominio

| Entidad | Descripción |
|---------|-------------|
| **Usuario** | Empleado del sistema con rol, contraseña encriptada y estado (activo/bloqueado) |
| **Cliente** | Suscriptor del servicio con plan asociado, fecha de vencimiento y prendas en uso |
| **Prenda** | Ítem de indumentaria con estado (`Disponible`, `EnUso`, `EnLimpieza`, `Baja`) |
| **PlanSuscripcion** | Define el límite de prendas simultáneas y precio mensual |
| **PedidoVenta** | Solicitud de entrega de prendas para un cliente según su plan |
| **PedidoRealizado** | Ciclo post-venta: `Despachado → Entregado → Devuelto` |

### Flujo principal del negocio

```
Cliente con plan activo
    │
    ├─ Vendedor crea PedidoVenta (respeta límite del plan)
    │
    ├─ Operador despacha el pedido → asigna prendas individuales
    │
    ├─ Se registra la entrega
    │
    └─ Cliente devuelve las prendas → pasan a EnLimpieza → Disponible
```

---

## 2. T01 — Arquitectura del Sistema

### 2.1 Arquitectura en N capas sin ORM

El sistema implementa una arquitectura de capas con **dependencias acíclicas estrictas**. No usa Entity Framework, NHibernate ni ningún ORM. Todo el acceso a datos se realiza con **ADO.NET puro** (`SqlConnection`, `SqlCommand`, `SqlDataReader`, `SqlDataAdapter`).

```
┌─────────────────────────────────────────────────────────────┐
│  GUI (WinForms MDI)                                         │
│  Formularios, eventos de usuario, llamadas a BLL            │
└───────────────────────────┬─────────────────────────────────┘
                            │ solo llama a ↓
┌───────────────────────────▼─────────────────────────────────┐
│  BLL (Business Logic Layer)                                 │
│  Reglas de negocio, validaciones, autorización, snapshots   │
└──────────┬────────────────────────────────────┬─────────────┘
           │ llama a ↓                          │ usa ↓
┌──────────▼───────────┐            ┌───────────▼─────────────┐
│  DAL (Data Access)   │            │  Servicios              │
│  ADO.NET, SQL        │            │  Bitácora, Multiidioma  │
└──────────┬───────────┘            └─────────────────────────┘
           │ mapea a ↓
┌──────────▼───────────┐            ┌─────────────────────────┐
│  BE (Entities)       │            │  Seguridad              │
│  POCOs, sin SQL      │            │  Sesión, Hash, AES, DV  │
└──────────────────────┘            └─────────────────────────┘
```

| Proyecto | Rol | Accede a |
|----------|-----|----------|
| `BE.dll` | Entidades puras (POCOs). Sin lógica, sin SQL | — |
| `DAL.dll` | Consultas SQL parametrizadas, mapeo a BE | BE, Seguridad |
| `BLL.dll` | Toda la lógica de negocio | DAL, BE, Servicios, Seguridad |
| `GUI.exe` | Presentación WinForms MDI | BLL, Servicios (Observer) |
| `Servicios.dll` | Bitácora y multiidioma (transversales) | BE |
| `Seguridad.dll` | Sesión, encriptado, DV | BE |

**Verificación de la restricción:** ningún archivo de la capa GUI importa `System.Data.SqlClient` — la GUI nunca toca la base de datos directamente.

### 2.2 Flujo de arranque (Program.cs)

```
Program.Main()
    │
    ├─ 1. BLL.Configuracion.VerificarConexionDAL()
    │       ↳ Si falla: MessageBox + Application.Exit()
    │
    ├─ 2. BLL.Configuracion.SeedAdminSecundario()
    │       ↳ Garantiza que existe admin2 como respaldo
    │       ↳ Si es nuevo: guarda credenciales en bin/CredencialesGeneradas/
    │
    ├─ 3. BLL.Configuracion.VerificarIntegridadDV()      ← T07
    │       ↳ Si PASA: continúa al Login
    │       ↳ Si FALLA: abre RestauracionForm
    │                   → Administrador puede Recalcular DV o Restaurar Backup
    │                   → Si no repara: Application.Exit()
    │
    └─ 4. new LoginForm().ShowDialog()
            ↳ Si OK: Application.Run(new Menu())
            ↳ Si cancela: Application.Exit()
```

### 2.3 Mapa de navegación MDI

`Menu.cs` actúa como contenedor MDI. El menú se construye **dinámicamente según los permisos** del rol autenticado — los ítems visibles son exactamente los permisos asignados al rol:

```
WardrobeFlow MDI
├── Panel de Control (Dashboard — visible para todos)
├── Inventario
│   └── Prendas (requiere mnuPrendas)
├── Ventas
│   ├── Clientes          (requiere mnuClientes)
│   ├── Planes            (requiere mnuPlanSuscripciones)
│   ├── Pedidos de Venta  (requiere mnuPedidosVenta)
│   └── Pedidos Realizados(requiere mnuPedidosRealizados)
├── Administrar           (requiere mnuUsuarios)
│   ├── Usuarios
│   ├── Perfiles y Permisos
│   ├── Historial de Cambios
│   ├── Bitácora
│   ├── Gestión de Idiomas
│   ├── Backup y Restauración
│   └── Diagnóstico de Integridad
├── Bitácora              (requiere mnuAuditoria)
├── Reporte de Jornada
└── [Barra de idiomas: ES · EN · RU · PT]
```

### 2.4 Esquema de persistencia

Sin ORM — ADO.NET puro. Patrón de acceso:

```csharp
// Ejemplo representativo del patrón usado en toda la DAL
using (var con = Acceso.GetInstancia().ObtenerConexion())
using (var cmd = new SqlCommand("SELECT ... FROM Usuario WHERE IdUsuario=@id", con))
{
    cmd.Parameters.AddWithValue("@id", id);  // siempre parametrizado
    using (var reader = cmd.ExecuteReader())
    {
        // mapeo manual a objeto BE
    }
}
```

`DAL.Acceso` es un **Singleton** que gestiona la única instancia de conexión y soporta transacciones explícitas para operaciones que requieren atomicidad.

---

## 3. T02 — Login, Logout y Gestión de Usuarios

### 3.1 Patrón Singleton — SessionManager

`Seguridad/SessionManager.cs` implementa el **Singleton thread-safe** con _double-checked locking_ y campo `volatile` para garantizar visibilidad en entornos multi-hilo:

```csharp
// Seguridad/SessionManager.cs (fragmento ilustrativo)
private static volatile SessionManager _session;
private static readonly object _lock = new object();

public static void Login(BE.Usuario usuario)
{
    lock (_lock)
    {
        if (_session == null)
            _session = new SessionManager
            {
                Usuario     = usuario,
                FechaInicio = DateTime.Now
            };
        else
            throw new InvalidOperationException("Ya existe una sesión activa.");
    }
}

public static SessionManager GetInstancia()
{
    if (_session == null)
        throw new InvalidOperationException("No hay sesión activa.");
    return _session;
}

public static void Logout()
{
    lock (_lock) { _session = null; }
}
```

**Por qué Singleton aquí:** en toda la aplicación debe existir exactamente un usuario autenticado. El Singleton garantiza que ninguna parte del sistema pueda crear una segunda sesión en paralelo.

**`TienePermiso(nombreMenu)`** — guard de autorización del backend:
- Si no hay sesión activa → retorna `false` (fail-closed)
- Si el usuario es Administrador → retorna `true` para todo
- En otro caso → busca el `nombreMenu` en `BE.Usuario.Permisos` (cargados al hacer login)

La BLL llama a `TienePermiso` antes de cada operación sensible; si retorna `false`, lanza `AppException` con el mensaje de acceso denegado traducido.

### 3.2 Políticas de login

```
LoginForm
    │
    ├─ Campos vacíos ──────────────────────► LoginException(CamposVacios)
    │                                        [no consume intento fallido]
    │
    ├─ BLL.Usuario.Login(user, pass)
    │       │
    │       ├─ Usuario no existe en BD ────► ejecuta hash señuelo (anti-enumeración)
    │       │                                LoginException(UsuarioInvalido)
    │       │
    │       ├─ Cuenta bloqueada ──────────► LoginException(CuentaBloqueada)
    │       │
    │       ├─ Contraseña incorrecta ─────► IntentosFallidos++
    │       │       │                       Bitacora.RegistrarSinSesion(intento)
    │       │       └─ Si llega a 3 ──────► bloquear cuenta en BD
    │       │                               Bitacora.RegistrarSinSesion(bloqueo)
    │       │                               LoginException(CuentaBloqueada)
    │       │
    │       └─ Credenciales OK ──────────► SessionManager.Login(usuario)
    │                                       BLL.Idioma.CargarTraducciones()
    │                                       GestorIdioma.CambiarIdioma(pref. usuario)
    │                                       Bitacora.Registrar(inicio sesión)
    │                                       return usuario
    │
    └─ Abre Menu MDI
```

**Anti-enumeración:** cuando el usuario no existe, la BLL ejecuta `Encriptador.VerificarContrasena(ingresada, _hashSenuelo)` donde `_hashSenuelo` es un hash PBKDF2 precalculado. Esto hace que el tiempo de respuesta sea idéntico al caso donde el usuario existe pero la contraseña es incorrecta, cerrando el canal lateral de temporización.

**Mensaje de error unificado:** tanto "usuario no existe" como "contraseña incorrecta" muestran el mismo texto genérico al usuario de la GUI.

### 3.3 Proceso de logout

```
MenuForm → Cerrar Sesión
    │
    ├─ Bitacora.Registrar(cierre sesión)
    ├─ SessionManager.Logout()          ← destruye el Singleton
    ├─ GestorIdioma.LimpiarObservadores()
    ├─ Cerrar todos los formularios MDI hijo
    └─ this.Close() → Program.Main() vuelve a mostrar LoginForm
```

### 3.4 Gestión de Usuarios (ABM)

| Operación | Actor | Snapshots (T06b) | Log bitácora |
|-----------|-------|-----------------|--------------|
| Alta de usuario | Administrador | No | Sí (Baja) |
| Baja lógica (desactivar) | Administrador | No | Sí (Baja) |
| Reset de contraseña | Administrador | **Sí** (antes de cambiar) | Sí (Alta) |
| Desbloqueo de cuenta | Administrador | **Sí** (antes de cambiar) | Sí (Media) |
| Cambio de rol | Administrador | **Sí** (afecta al usuario) | Sí (Baja) |
| Auto-baja | Prohibido | — | — |

**Generación de contraseñas:** al crear un usuario o resetear su clave, el sistema genera una contraseña segura mediante `RNGCryptoServiceProvider` + mezcla Fisher-Yates, la exporta en texto plano al archivo `bin/Debug/CredencialesGeneradas/<usuario>_<timestamp>.txt` y la almacena como hash PBKDF2 en BD.

### 3.5 Procesos en el arranque del sistema

| Momento | Proceso |
|---------|---------|
| Inicio (pre-login) | Verificar conexión SQL, verificar DV, seed admin2 |
| Login | Autenticar, cargar permisos, cargar traducciones, restaurar idioma del usuario |
| Post-login | Abrir Menu MDI, abrir Dashboard, iniciar timer de verificación periódica (cada 30 min) |
| Logout | Log de cierre, destruir sesión, cerrar formularios hijo |
| Cierre | Sin proceso especial (sin datos en memoria que persistan) |

---

## 4. T03 — Gestión de Encriptado

### 4.1 Hash unidireccional — PBKDF2-SHA256 (contraseñas)

Las contraseñas **nunca se almacenan en texto plano**. El algoritmo es PBKDF2 con SHA-256 como función pseudoaleatoria:

```
Encriptador.Hash(contrasena)
    │
    ├─ salt ← RandomNumberGenerator.GetBytes(16)     [16 bytes aleatorios]
    ├─ hash ← Rfc2898DeriveBytes(contrasena, salt,
    │              iteraciones: 100.000,
    │              algoritmo:   SHA256,
    │              tamaño:      32 bytes)
    │
    └─ resultado almacenado en BD:
       Base64( salt[0..15] + hash[0..31] )   ← 48 bytes → ~64 chars base64
```

**Verificación de contraseña (tiempo constante):**

```
Encriptador.VerificarContrasena(ingresada, almacenada)
    │
    ├─ Decodificar base64 → bytes[48]
    ├─ Extraer salt: bytes[0..15]
    ├─ Extraer hashAlmacenado: bytes[16..47]
    ├─ Recalcular hashIngresado con mismo salt e iteraciones
    │
    └─ Comparar con XOR acumulado (tiempo constante):
       diferencia = 0
       for i in 0..31:
           diferencia |= hashIngresado[i] ^ hashAlmacenado[i]
       return diferencia == 0
```

La comparación byte a byte con XOR acumulado garantiza que el tiempo de ejecución no dependa de en qué posición difieren los hashes, evitando ataques de temporización.

**¿Por qué 100.000 iteraciones?** Cada iteración adicional multiplica el costo computacional para quien intenta un ataque de fuerza bruta por diccionario, sin impacto perceptible para el usuario legítimo (< 100 ms).

### 4.2 Cifrado simétrico reversible — AES-128-CBC (datos sensibles)

Para datos que deben poder leerse (DNI de clientes y empleados), se usa AES en modo CBC:

```
Encriptador.Encriptar(textoPlano)
    │
    ├─ iv ← RNG(16 bytes)                         [IV aleatorio por operación]
    ├─ clave ← CargarClaveDesdeArchivo("key.dat") [protegida con DPAPI]
    │       └─ Si key.dat no existe: genera nueva clave AES-128 y la guarda
    │
    ├─ cifrar con AES-128-CBC(textoPlano, clave, iv)
    │
    └─ resultado en BD:
       Base64( iv[0..15] + cipherText )
```

```
Encriptador.Desencriptar(cifrado)
    │
    ├─ Decodificar base64
    ├─ Extraer iv: bytes[0..15]
    ├─ Extraer cipherText: bytes[16..]
    ├─ clave ← CargarClaveDesdeArchivo("key.dat")
    └─ descifrar con AES-128-CBC(cipherText, clave, iv)
```

**DPAPI:** `key.dat` almacena la clave AES cifrada con `ProtectedData.Protect(key, null, DataProtectionScope.LocalMachine)`. Solo el mismo equipo puede descifrar el archivo. Si `key.dat` contiene texto plano (migración de versiones anteriores), se detecta y re-cifra automáticamente.

**IV aleatorio:** el mismo DNI cifrado dos veces producirá dos ciphertexts distintos. La unicidad de DNI no se puede verificar comparando los cifrados directamente; se verifica en BLL cargando todos los DNI descifrados en memoria y comparando.

### 4.3 Validación de contraseñas

`Encriptador.ValidarContrasena(contrasena)` verifica que cumpla la política:
- Longitud mínima: 8 caracteres
- Al menos 1 dígito
- Al menos 1 carácter especial (`!@#$%^&*`)

### 4.4 Protección de la clave AES (key.dat)

```
key.dat
    │
    ├─ Cifrado con DPAPI (DataProtectionScope.LocalMachine)
    ├─ Ubicado en bin/Debug/ junto al ejecutable
    └─ Si se mueve la instalación a otro equipo: los DNI cifrados
       quedan inaccesibles (la clave DPAPI es específica del equipo)
```

---

## 5. T04 — Gestión de Perfiles de Usuario — Patrón Composite

### 5.1 Motivación

Los permisos del sistema forman una jerarquía en árbol: los permisos atómicos (abrir una pantalla, ejecutar una operación) se agrupan en familias lógicas (Inventario, Ventas, etc.) y las familias se agrupan en roles asignables. Esta estructura es naturalmente jerárquica y el Patrón Composite permite tratarla de forma uniforme — se puede recorrer un rol con la misma función que recorre una familia o una patente individual.

### 5.2 Estructura del Patrón

```
BE/Componente.cs          ← Componente abstracto (Clase base)
│   + Id: int
│   + Nombre: string
│   + abstract Hijos: IList<Componente>
│   + abstract AgregarHijo(c: Componente)
│   + abstract QuitarHijo(c: Componente)
│   + abstract VaciarHijos()
│
├── BE/Familia.cs          ← Composite — nodo con hijos
│       - _hijos: List<Componente>
│       + Hijos: devuelve _hijos (lista mutable)
│       + AgregarHijo(): valida referencias circulares (profundidad máx. 50)
│       + QuitarHijo(): silencioso si no existe
│
├── BE/Patente.cs          ← Leaf — permiso atómico
│       + NombreMenu: string   ← clave para verificar en SessionManager
│       + Asignado: bool       ← marcado al cargar árbol por rol
│       + Hijos: siempre vacío
│       + AgregarHijo(): lanza InvalidOperationException (es hoja)
│
└── BE/Rol.cs (o Familia con EsRol=true)
        ← Un Rol ES una Familia especial asignable a usuarios
        ← Puede contener Familias, Patentes y otros Roles (anidamiento)
```

**En base de datos:**

| Tabla | Columnas clave | Rol |
|-------|---------------|-----|
| `Permiso` | `Id, Nombre, NombreMenu, EsFamilia, EsRol` | Un nodo del árbol |
| `PermisoRelacion` | `IdPadre, IdHijo` | Arista padre→hijo del árbol |

Toda la estructura del árbol se persiste en `PermisoRelacion`. No hay tablas separadas para Familia, Patente y Rol — la discriminación se hace con las columnas `EsFamilia` y `EsRol` de `Permiso`.

### 5.3 Construcción del árbol desde la BD

`DAL/Permiso.ObtenerArbol()`:

```
1. SELECT * FROM Permiso WHERE Estado=1
   → crear Dictionary<int, Componente> por Id
     • Si EsFamilia=1 → new Familia(...)
     • Si EsFamilia=0 → new Patente(...)

2. SELECT IdPadre, IdHijo FROM PermisoRelacion
   → para cada relación:
     padre = diccionario[IdPadre]
     hijo  = diccionario[IdHijo]
     padre.AgregarHijo(hijo)

3. Identificar raíces: nodos que no son hijo de nadie
   → retornar lista de raíces (árbol/bosque)
```

### 5.4 Funciones recursivas

**BLL — MarcarAsignados (marca patentes del rol en el árbol):**

```csharp
// BLL/Familia.cs
private void MarcarAsignados(IList<Componente> nodos, HashSet<int> idsAsignados)
{
    foreach (var nodo in nodos)
    {
        if (nodo is BE.Patente patente)
            patente.Asignado = idsAsignados.Contains(patente.Id);

        if (nodo.Hijos.Count > 0)
            MarcarAsignados(nodo.Hijos, idsAsignados);  // ← RECURSIÓN
    }
}
```

**BLL — ObtenerPermisosEfectivos (resuelve todos los permisos de un rol, sin duplicados):**

```csharp
// Recorre recursivamente toda la subjerarquía del rol
// eliminando permisos duplicados (un permiso puede aparecer
// en varias ramas si se embedieron roles dentro de otros roles)
private void ResolverPermisosRecursivo(
    Componente nodo,
    HashSet<int> visitados,
    List<BE.Patente> resultado)
{
    if (visitados.Contains(nodo.Id)) return;  // evita ciclos
    visitados.Add(nodo.Id);

    if (nodo is BE.Patente p)
    {
        resultado.Add(p);
        return;
    }
    foreach (var hijo in nodo.Hijos)
        ResolverPermisosRecursivo(hijo, visitados, resultado);
}
```

**GUI — MostrarPermisosRecursivo (construye TreeView):**

```csharp
// GUI/GestorPermisos.cs
private void MostrarPermisosRecursivo(
    TreeNodeCollection nodos,
    IList<Componente> componentes)
{
    foreach (var c in componentes)
    {
        var tn = new TreeNode(c.Nombre);
        if (c is BE.Familia)
        {
            tn.NodeFont = new Font("Segoe UI", 9, FontStyle.Bold);
            tn.ForeColor = Color.Navy;
            MostrarPermisosRecursivo(tn.Nodes, c.Hijos);  // ← RECURSIÓN
        }
        else if (c is BE.Patente pat)
        {
            tn.Checked = pat.Asignado;
        }
        nodos.Add(tn);
    }
}
```

**GUI — GuardarRecursivo (recolecta IDs de patentes marcadas):**

```csharp
private void GuardarRecursivo(TreeNodeCollection nodos, List<int> idsSeleccionados)
{
    foreach (TreeNode tn in nodos)
    {
        if (tn.Tag is BE.Patente pat && tn.Checked)
            idsSeleccionados.Add(pat.Id);

        GuardarRecursivo(tn.Nodes, idsSeleccionados);  // ← RECURSIÓN
    }
}
```

### 5.5 Flujo de asignación de permisos

```
GestorPermisos (GUI)
    │
    ├─ 1. ComboBox "Seleccionar Rol" → BLL.Familia.ObtenerRoles()
    │
    ├─ 2. Botón "Cargar" → BLL.Familia.ObtenerArbolPorRol(rol)
    │       ↳ construye árbol Composite completo
    │       ↳ marca Patente.Asignado = true en las asignadas al rol
    │       ↳ MostrarPermisosRecursivo() → llena TreeView con checkboxes
    │
    ├─ 3. Usuario marca/desmarca checkboxes en el TreeView
    │
    └─ 4. Botón "Guardar" → GuardarRecursivo() recolecta IDs marcados
                            → BLL.Familia.GuardarAsignacionRol(rol, ids)
                            ↳ compara ids nuevos vs. actuales en BD
                            ↳ INSERT los nuevos en PermisoRelacion
                            ↳ DELETE los quitados de PermisoRelacion
                            ↳ crea snapshots Memento para usuarios del rol
                            ↳ bitácora con detalle de cambios
```

### 5.6 Validaciones de negocio

- Solo el **Administrador** puede gestionar perfiles (verificado en BLL con `TienePermiso`)
- No se puede eliminar un rol si tiene usuarios asignados activos
- `AgregarHijo()` en `Familia` verifica referencias circulares con un recorrido de profundidad máxima de 50 niveles
- Un rol puede embeber otros roles (herencia de permisos), y los permisos efectivos se resuelven recursivamente deduplicando

### 5.7 Diagrama de clases simplificado

```
           ┌──────────────────────────────┐
           │  BE.Componente (abstract)    │
           │  - Id: int                   │
           │  - Nombre: string            │
           │  + Hijos: IList<Componente>  │
           │  + AgregarHijo(Componente)   │
           └────────────┬─────────────────┘
                        │ hereda
          ┌─────────────┴─────────────────┐
          │                               │
┌─────────▼──────────┐         ┌─────────▼──────────┐
│  BE.Familia        │         │  BE.Patente         │
│  - _hijos: List<>  │         │  - NombreMenu       │
│  (puede tener      │◄────────│  - Asignado: bool   │
│   Familia, Patente │  hijo   │  (no tiene hijos)   │
│   u otro Rol)      │         └─────────────────────┘
└────────────────────┘
```

---

## 6. T05 — Gestión de Múltiples Idiomas — Patrón Observer

### 6.1 Objetivo

Permitir que el sistema funcione en múltiples idiomas con cambio dinámico en tiempo de ejecución (sin reiniciar la aplicación), sin usar archivos `.resx` ni recursos estáticos, y con un modelo reutilizable desacoplado de la UI.

### 6.2 Idiomas soportados

| Código | Idioma | Estado |
|--------|--------|--------|
| `ES` | Español | Default |
| `EN` | English | Activo |
| `RU` | Русский | Activo |
| `PT` | Português | Activo |

### 6.3 Estructura del Patrón Observer

```
Servicios/Multiidioma/
│
├── IIdiomaObserver.cs      ← Interfaz Observer
│   └── void UpdateLanguage(Idioma idioma)
│
├── GestorIdioma.cs         ← Subject (Sujeto) — estático
│   - _observadores: List<IIdiomaObserver>
│   + IdiomaActual: Idioma
│   + TradActuales: Dictionary<string,string>  ← cache de BD
│   + IdiomasDisponibles: IList<Idioma>
│   + SuscribirObservador(IIdiomaObserver)
│   + DesuscribirObservador(IIdiomaObserver)
│   + CambiarIdioma(Idioma, Dictionary<string,string>)
│   + Notificar()
│
└── Traductor.cs            ← Fuente de traducciones
    + ObtenerIdiomas()
    + ObtenerTraducciones(Idioma)
    + ObtenerTraduccionesHardcode(Idioma)
    - _es, _en, _ru, _pt    ← diccionarios fallback (300+ claves)
```

### 6.4 Ciclo de vida de un formulario Observer

Cada uno de los **22+ formularios** que implementan `IIdiomaObserver` sigue el mismo patrón:

```csharp
public class MiFormulario : Form, IIdiomaObserver
{
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        GestorIdioma.SuscribirObservador(this);     // ← se registra al abrir
        Traducir(GestorIdioma.IdiomaActual);         // ← traduce al idioma actual
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        GestorIdioma.DesuscribirObservador(this);   // ← se desregistra al cerrar
        base.OnFormClosing(e);                       //   (evita memory leak)
    }

    public void UpdateLanguage(Idioma idioma)        // ← callback del Subject
        => Traducir(idioma);

    private void Traducir(Idioma idioma)
    {
        var t = Traductor.ObtenerTraducciones(idioma);
        btnGuardar.Text  = t["btn.guardar"].Texto;
        lblTitulo.Text   = t["frm.clientes"].Texto;
        // ... un mapeo por control
    }
}
```

### 6.5 Notificación del Subject

`GestorIdioma.Notificar()` usa una copia defensiva de la lista para evitar `InvalidOperationException` si un observer se desuscribe durante la iteración:

```csharp
private static void Notificar()
{
    // copia defensiva — evita modificación de la lista durante la iteración
    var copia = new List<IIdiomaObserver>(_observadores);
    foreach (var obs in copia)
    {
        try { obs.UpdateLanguage(IdiomaActual); }
        catch { /* un observer que falla no interrumpe a los demás */ }
    }
}
```

### 6.6 Secuencia de cambio de idioma

```
Usuario hace clic en botón "EN" en la barra del Menu
    │
    ├─ Menu.cs → BLL.Idioma.CargarTraducciones("EN")
    │       ↳ SELECT Clave, Texto FROM Traduccion
    │         WHERE IdIdioma = 'EN'
    │       ↳ retorna Dictionary<string, string>
    │
    ├─ GestorIdioma.CambiarIdioma(idiomaEN, dictEN)
    │       ↳ IdiomaActual = idiomaEN
    │       ↳ TradActuales = dictEN
    │       ↳ Notificar()
    │               ↳ UpdateLanguage(idiomaEN) en CADA formulario abierto
    │                       ↳ cada formulario llama a Traducir(EN)
    │                       ↳ reasigna el Text de cada control
    │
    └─ Cambio visible instantáneamente en todos los formularios abiertos
```

### 6.7 Fuentes de traducciones

```
Al traducir un control:
    │
    ├─ GestorIdioma.TradActuales (cache BD) ≠ null y ≠ vacío?
    │       ↳ SÍ: usa overlay sobre la base ES (fallback por clave)
    │              Si una clave no está en el idioma activo
    │              → se usa el texto del ES como fallback
    │
    └─ NO: usa Traductor.ObtenerTraduccionesHardcode(idioma)
           ↳ Diccionarios _es, _en, _ru, _pt en código C#
           ↳ Solo se llega aquí en el primer arranque o sin conexión
```

### 6.8 Modelo de datos (BD)

```
Idioma (Id PK, Nombre, Activo BIT, EsDefault BIT)
   │
   └─ FK ─► Traduccion (IdIdioma, Clave, Texto)
                         PK compuesta: (IdIdioma, Clave)
```

**Seeding automático:** si `Traduccion` está vacía al arrancar, `BLL.IdiomaService.SeedearDesdeHardcode()` puebla la tabla con los diccionarios hardcodeados de los 4 idiomas.

### 6.9 Administración de idiomas desde la UI

`GUI/FormIdiomas.cs` (solo Administrador) permite:
- Ver y activar/desactivar idiomas
- Editar cualquier traducción en la grilla directamente
- Al activar un idioma incompleto (con claves faltantes), el sistema lo avisa; las claves faltantes usan el texto ES como fallback

### 6.10 Preferencia de idioma por usuario

`Usuario.IdIdioma` (VARCHAR 5, nullable) guarda la preferencia del usuario autenticado. Al hacer login, si el usuario tiene preferencia guardada, el sistema la restaura automáticamente llamando a `GestorIdioma.CambiarIdioma()` con ese idioma.

---

## 7. T06a — Bitácora del Sistema y de Negocio

### 7.1 Dos bitácoras independientes

| | Bitácora del Sistema | Bitácora de Negocio |
|-|---------------------|---------------------|
| **Clase BE** | `BE.Bitacora` | `BE.BitacoraNegocio` |
| **Servicio** | `Servicios.Bitacora` | `Servicios.BitacoraNegocio` |
| **Tabla SQL** | `BitacoraSistema` | `BitacoraNegocio` |
| **Registra** | Login, logout, intentos fallidos, bloqueos, DV, backups, cambios de usuario | Altas/bajas/modificaciones de clientes, prendas, planes, pedidos |
| **Crítico** | Siempre activa (incluso sin sesión) | Solo con sesión activa |

La UI (`GUI/Bitacora.cs`) muestra ambas en **pestañas separadas** con los mismos filtros de búsqueda.

### 7.2 Datos de cada registro

```sql
CREATE TABLE BitacoraSistema (
    Id          INT IDENTITY PRIMARY KEY,
    Fecha       DATETIME NOT NULL DEFAULT GETDATE(),
    IdUsuario   INT NULL,           -- NULL si ocurre antes del login
    Modulo      NVARCHAR(100),      -- nombre del formulario o proceso
    Actividad   NVARCHAR(200),      -- descripción corta de la acción
    Detalle     NVARCHAR(MAX),      -- información contextual extensa
    Criticidad  INT NOT NULL,       -- 0=None, 1=Baja, 2=Media, 3=Alta
    IP          NVARCHAR(50)        -- IP del equipo local
)
```

### 7.3 Registro sin sesión activa

`Servicios.Bitacora.RegistrarSinSesion(...)` permite loguear eventos anteriores al login (intentos fallidos, bloqueos de cuenta) sin depender de `SessionManager`. Usa `IdUsuario = null` en BD.

### 7.4 Escala de criticidad

| Nivel | Eventos típicos |
|-------|----------------|
| `None` | Login exitoso, cierre de sesión |
| `Baja` | Alta de usuario, alta de cliente/prenda/pedido |
| `Media` | Desbloqueo de cuenta, bloqueo por intentos |
| `Alta` | Reset de contraseña, restauración de versión, recálculo DV, backup/restauración |

### 7.5 Filtros de búsqueda combinados

```sql
-- Consulta generada dinámicamente según filtros activos
SELECT * FROM BitacoraSistema
WHERE (@desde  IS NULL OR Fecha >= @desde)
  AND (@hasta  IS NULL OR Fecha <= @hasta)
  AND (@idUser IS NULL OR IdUsuario = @idUser)
  AND (@activ  IS NULL OR Actividad LIKE '%' + @activ + '%')
  AND (@crit   IS NULL OR Criticidad = @crit)
  AND (@dias   = 0     OR Fecha >= DATEADD(DAY, -@dias, GETDATE()))
ORDER BY Fecha DESC
```

Todos los parámetros son opcionales y combinables; si todos están en NULL/0, devuelve todos los registros.

### 7.6 Exportación a PDF

La bitácora puede exportarse a PDF usando `PrintDocument` + `PrintPreviewDialog`, con el encabezado "Reporte de Bitácora — WardrobeFlow" y cada campo del registro en columnas. El texto "Generado:" está traducido al idioma activo del usuario.

---

## 8. T06b — Control de Cambios e Historial — Patrón Memento

### 8.1 Objetivo

Mantener un historial completo de cambios sobre la entidad más sensible del sistema (`Usuario`), permitiendo conocer **quién** hizo un cambio, **cuándo**, **qué** cambió, y poder **revertir** el estado anterior del usuario.

### 8.2 Estructura del Patrón Memento

```
BE/VersionUsuario.cs    ← Memento (cápsula del estado)
    + Id
    + IdUsuario          ← a qué usuario pertenece este snapshot
    + Fecha              ← cuándo se tomó el snapshot
    + Actor              ← quién realizó la acción que provocó el snapshot
    + Detalle            ← descripción de la operación (ej: "Reset de contraseña")
    + UsernameSnapshot   ← username ANTES del cambio
    + ClaveSnapshot      ← hash PBKDF2 ANTES del cambio
    + EstadoSnapshot     ← activo/bloqueado ANTES del cambio
    + IntentosSnapshot   ← intentos fallidos ANTES del cambio
    + PerfilSnapshot     ← rol/perfil ANTES del cambio

BE/Usuario.cs           ← Originator
    + CrearMemento()     ← captura el estado actual en un VersionUsuario
    + RestaurarDesde(VersionUsuario) ← restaura el estado desde un snapshot

BLL/CuidadorHistorial.cs ← Caretaker
    + GuardarVersion(idUsuario, actor, detalle) ← pide snapshot a BLL, lo persiste
    + ObtenerVersiones(idUsuario)               ← recupera historial de la BD
    + RestaurarVersion(modulo, idVersion)       ← orquesta la restauración
```

### 8.3 Flujo de captura automática de snapshot

Antes de modificar un usuario, la BLL siempre captura el estado previo:

```
BLL.Usuario.ResetearClave(idUsuario, nuevaClave)
    │
    ├─ 1. usuarioActual ← DAL.Usuario.ObtenerPorId(idUsuario)
    │
    ├─ 2. memento ← usuarioActual.CrearMemento()     ← Originator crea snapshot
    │
    ├─ 3. DAL.HistorialUsuario.Guardar(memento, actor, "Reset de contraseña")
    │       ↳ INSERT INTO HistorialUsuario (IdUsuario, Fecha, Actor, Detalle,
    │                      UsernameSnapshot, ClaveSnapshot, EstadoSnapshot,
    │                      IntentosSnapshot, PerfilSnapshot)
    │
    ├─ 4. DAL.Usuario.ActualizarClave(idUsuario, hashNuevaClave)
    │
    └─ 5. Bitacora.Registrar("Reset Contrasena", Criticidad.Alta)
```

Lo mismo ocurre en `DesbloquearCuenta()` y en `BLL.Familia.GuardarAsignacionRol()` para todos los usuarios del rol modificado.

### 8.4 Flujo de restauración

```
GUI/VersionHistorialForm → "Restaurar versión seleccionada"
    │
    ├─ BLL.CuidadorHistorial.RestaurarVersion(modulo, idVersion)
    │       │
    │       ├─ 1. Verificar que idVersion existe en BD
    │       │
    │       ├─ 2. usuarioActual ← DAL.Usuario.ObtenerPorId(snapshot.IdUsuario)
    │       │
    │       ├─ 3. Crear snapshot del estado ACTUAL (antes de restaurar)
    │       │       ↳ Caretaker graba nuevo memento con detalle "Pre-restauración"
    │       │       ↳ Si falla este paso: ABORTAR (fail-safe)
    │       │
    │       ├─ 4. DAL.Usuario.RestaurarDesde(snapshot)
    │       │       ↳ UPDATE Usuario SET
    │       │             Username=snapshot.UsernameSnapshot,
    │       │             Clave=snapshot.ClaveSnapshot,
    │       │             Estado=snapshot.EstadoSnapshot,
    │       │             IntentosFallidos=snapshot.IntentosSnapshot,
    │       │             Perfil=snapshot.PerfilSnapshot
    │       │         WHERE IdUsuario=snapshot.IdUsuario
    │       │
    │       └─ 5. Bitacora.Registrar("Restauracion Version", Criticidad.Alta)
    │
    └─ La restauración en sí también es reversible (tiene su propio snapshot)
```

### 8.5 Tabla de historial en BD

```sql
CREATE TABLE HistorialUsuario (
    Id               INT IDENTITY PRIMARY KEY,
    IdUsuario        INT NOT NULL REFERENCES Usuario(IdUsuario),
    Fecha            DATETIME NOT NULL DEFAULT GETDATE(),
    Actor            NVARCHAR(100),   -- username de quien hizo el cambio
    Detalle          NVARCHAR(200),   -- descripción de la operación
    UsernameSnapshot NVARCHAR(100),
    ClaveSnapshot    NVARCHAR(300),   -- hash PBKDF2 (nunca texto plano)
    EstadoSnapshot   BIT,
    IntentosSnapshot INT,
    PerfilSnapshot   NVARCHAR(50)
)
```

### 8.6 UI — VersionHistorialForm

```
ComboBox "Seleccionar usuario"
    │
    Botón "Cargar"
    │   ↳ Grilla: ID | Fecha | Actor | Detalle | Estado | Intentos
    │
    ┌──────────────────────────────────────────┐
    │  ID | Fecha              | Actor | Det.. │
    │   3 | 2026-06-01 10:32  | admin | Reset  │  ← snapshot más reciente
    │   2 | 2026-05-28 09:15  | admin | Desbloq│
    │   1 | 2026-05-20 14:00  | admin | Alta   │
    └──────────────────────────────────────────┘
    │
    Botón "Restaurar Versión Seleccionada"
    │   ↳ MessageBox de confirmación
    │   ↳ BLL.CuidadorHistorial.RestaurarVersion(...)
    │   ↳ MostrarOk("Versión restaurada correctamente")
```

---

## 9. T07 — Dígitos Verificadores (DVH + DVV)

### 9.1 Objetivo

Detectar manipulaciones directas sobre la base de datos (ediciones con SSMS, intercambios de registros, inserciones/eliminaciones clandestinas) que no pasarían por las validaciones del sistema. Los DV constituyen una barrera de última instancia.

### 9.2 Entidades protegidas

| Tabla | Protección |
|-------|-----------|
| `Usuario` | DVH por fila + DVV de tabla (verificación pre-login) |
| `Cliente` | DVH por fila + DVV de tabla (verificación periódica) |
| `Empleado` | DVH por fila + DVV de tabla (verificación periódica) |
| `Pedido` | DVH por fila + DVV de tabla (verificación periódica) |

**Diseño genérico:** `DAL.DigitoVerificador.RecalcularTabla(tabla, pkCol, columnas)` opera sobre cualquier tabla indicando su nombre, PK y las columnas a incluir en el cálculo — reutilizable sin cambiar código.

### 9.3 Algoritmo DVH (Dígito Verificador Horizontal)

Calcula un dígito por **fila**, ponderando cada carácter por su posición absoluta dentro de la fila:

```
DVH(fila):
    suma     ← 0
    posicion ← 1

    para cada campo en [campo1, campo2, ..., campoN]:
        para cada caracter c en ToString(campo):
            suma     += ASCII(c) × posicion
            posicion += 1

    DVH ← suma mod 999.983
```

**Módulo 999.983** (número primo): usar un primo grande reduce drásticamente la probabilidad de colisiones (dos filas distintas con el mismo DVH) comparado con el clásico mod 10 que solo tiene 10 valores posibles.

**Qué detecta:**
- Modificación del valor de cualquier campo (cambia la suma)
- Intercambio de valores entre campos de la misma fila (la posición del carácter cambia)

**Para la tabla Usuario, los campos incluidos en DVH son:** `IdUsuario`, `Username`, `Clave`, `Perfil`, `Estado`, `IntentosFallidos` (los campos que definen la identidad y seguridad del usuario).

### 9.4 Algoritmo DVV (Dígito Verificador Vertical)

Calcula un dígito por **tabla** ponderando cada DVH por la posición de su fila:

```
DVV(tabla):
    suma ← 0
    i    ← 0   (0-indexed, orden por PK ascendente)

    para cada fila en tabla (ordenada por PK):
        suma += DVH_fila × (i + 1)
        i    += 1

    DVV ← suma mod 999.983
```

**Qué detecta:**
- Inserción de una fila nueva (aumenta el número de filas, cambia el DVV)
- Eliminación de una fila (reduce filas, cambia el DVV)
- Intercambio del orden de dos filas (sus DVH se multiplican por posiciones distintas, cambia el DVV)

### 9.5 Almacenamiento

```sql
-- DVH: columna en la propia tabla
ALTER TABLE Usuario ADD DVH INT NULL;

-- DVV: tabla separada (una fila por tabla protegida)
CREATE TABLE DVVertical (
    Id           INT IDENTITY PRIMARY KEY,
    NombreTabla  VARCHAR(100) NOT NULL UNIQUE,
    DVV          INT NOT NULL,
    FechaCalculo DATETIME NOT NULL DEFAULT GETDATE()
);
```

### 9.6 Verificación pre-login (secuencia completa)

```
Program.Main() → BLL.Configuracion.VerificarIntegridadDV(out resultado)
    │
    ├─ 1. filas ← DAL.DigitoVerificador.ObtenerFilasUsuario()
    │       cada fila: { IdUsuario, campos..., DVH_almacenado }
    │
    ├─ 2. Para cada fila:
    │       DVH_calculado ← Seguridad.DigitoVerificador.CalcularDVH(fila)
    │       Si DVH_calculado ≠ DVH_almacenado:
    │           resultado.FilasCorruptas.Add(fila.IdUsuario)
    │
    ├─ 3. DVV_calculado ← Seguridad.DigitoVerificador.CalcularDVV(
    │                           lista de DVH_calculados)
    │
    ├─ 4. DVV_almacenado ← DAL.DigitoVerificador.ObtenerDVV("Usuario")
    │
    ├─ 5. Si DVH_calculado ≠ DVH_almacenado para alguna fila: falla
    │     Si DVV_calculado ≠ DVV_almacenado: falla
    │
    ├─ CASO ESPECIAL — primer arranque (migración):
    │     Si todos DVH == 0 AND DVV_almacenado == 0:
    │         recalcular y guardar → continuar normalmente
    │     Si formato DV anterior detectado (v1, solo mod 10):
    │         recalcular en formato v2 → continuar normalmente
    │
    └─ Retorna:
         true  → continúa al Login
         false → abre RestauracionForm
```

### 9.7 Restauración ante fallo de integridad

Si la verificación falla, `Program.cs` abre `GUI/RestauracionForm`:

```
RestauracionForm
    │
    ├─ Opción A: Recalcular Dígitos Verificadores
    │       ↳ Solo si los DATOS son correctos pero los DV están desactualizados
    │       ↳ BLL.Configuracion.RecalcularIntegridadDV()
    │           → recalcula DVH de cada fila + DVV
    │           → guarda en BD
    │           → Bitacora.RegistrarSinSesion("Recalculo DV", Alta)
    │
    └─ Opción B: Restaurar desde Backup
            ↳ Seleccionar archivo .bak
            ↳ Reemplaza la BD por la copia de respaldo
            ↳ Solo disponible si existe al menos un backup
```

Ambas opciones requieren autenticación del Administrador dentro del propio formulario de restauración.

### 9.8 Recálculo administrativo periódico

Desde `GUI/Usuarios → Recalcular DV` (solo Administrador) o desde `GUI/DiagnosticoIntegridad`:

```
BLL.Configuracion.RecalcularIntegridadDV()
    │
    ├─ Para cada fila en Usuario:
    │       DVH_nuevo ← CalcularDVH(fila)
    │       DAL.DigitoVerificador.ActualizarDVH(id, DVH_nuevo)
    │
    ├─ DVV_nuevo ← CalcularDVV(todos los DVH)
    │   DAL.DigitoVerificador.GuardarDVV("Usuario", DVV_nuevo)
    │
    ├─ Mismo proceso para: Cliente, Empleado, Pedido
    │
    └─ Bitacora.Registrar("Recalculo DV", Criticidad.Alta)
```

### 9.9 Diagnóstico de Integridad

`GUI/DiagnosticoIntegridad.cs` muestra:
- Estado actual DVV de cada tabla protegida
- Lista de filas con DVH corrompido con el ID afectado
- Botón "Reparar filas seleccionadas" → `BLL.Configuracion.RepararFilas(ids)`

---

## 10. Dashboard por Rol

Cada rol del sistema tiene su **propio formulario de dashboard** adaptado a las métricas relevantes para su función:

| Formulario | Rol | KPIs mostrados |
|-----------|-----|----------------|
| `DashboardForm` | Administrador | Prendas disponibles, Clientes activos, Pedidos pendientes, Días sin backup (semáforo de color), Ocupación de stock (%) |
| `DashboardVendedor` | Vendedor | Pipeline de pedidos por estado (Pendiente / Despachado / Entregado), Clientes activos, Planes disponibles |
| `DashboardControlStock` | ControladorDeStock | Estado del stock por categoría, Prendas en mantenimiento, Prendas dadas de baja |
| `DashboardSupervisor` | Supervisor | Resumen de actividad de auditoría, últimos eventos de bitácora |
| `DashboardOperador` | OperadorDeInventario | Pedidos a despachar, entregas pendientes |

### Implementación técnica

- **Carga asíncrona:** cada dashboard usa `Task.Run(() => cargarDatos())` + `this.BeginInvoke()` para no bloquear el hilo UI mientras consulta la base de datos.
- **Auto-refresh:** un `System.Windows.Forms.Timer` con intervalo configurable refresca los KPIs automáticamente.
- **Observer:** todos los dashboards implementan `IIdiomaObserver` — las etiquetas se traducen al cambiar el idioma.
- **Apertura automática:** el `Menu.cs` detecta el rol del usuario autenticado al iniciar sesión y abre el dashboard correspondiente como formulario MDI hijo.

---

## 11. Criterios Transversales de Evaluación

### 11.1 Arquitectura 4 capas sin ORM

- Ningún archivo de `GUI/` importa `System.Data.SqlClient`
- Ningún archivo de `BLL/` ejecuta SQL
- Toda la lógica de negocio está en `BLL/`, no en eventos de formularios
- Todo el SQL está en `DAL/`, parametrizado

### 11.2 Principios POO aplicados

**Herencia:**
- `FormBase : Form` → 13 formularios de negocio heredan `MostrarOk()`, `MostrarError()`, traducción automática de `AppException`
- `Componente` → `Familia`, `Patente` (Composite)
- `AppException : Exception` (excepciones de dominio con clave de traducción)

**Encapsulamiento:**
- Campos privados en todas las entidades BE con propiedades públicas
- Constructor privado en Singletons (`SessionManager`, `DAL.Acceso`)
- Estado interno de `GestorIdioma` completamente privado, acceso solo por métodos

**Polimorfismo:**
- `Componente.Hijos`, `AgregarHijo()`: comportamiento diferente en `Familia` (lista real) vs `Patente` (no-op / excepción)
- `IIdiomaObserver.UpdateLanguage()`: implementado de forma diferente en cada formulario
- `FormBase.MensajeLabel`: propiedad virtual sobrescrita

**Cohesión alta / Acoplamiento bajo:**
- `BLL.Usuario` no conoce `SqlCommand` — delega en `DAL.Usuario`
- `GUI.ClienteForm` no conoce `SqlConnection` — delega en `BLL.Cliente`
- `Servicios.Bitacora` no conoce los formularios — recibe parámetros primitivos

### 11.3 Manejo de excepciones

```
Excepción de dominio (AppException):
    BLL lanza AppException(claveTraduccion, detalleOpcional)
        ↓
    FormBase captura en evento, llama MostrarError(T(clave))
    → mensaje en el idioma activo del usuario

Excepción técnica inesperada (Exception):
    FormBase.OnUnhandledException()
        ↓
    Bitacora.Registrar(modulo, "Error Inesperado", ex.Message, Criticidad.Alta)
    MostrarError("Error interno del sistema. Contacte al administrador.")
    → nunca se muestra el stack trace al usuario
```

### 11.4 Diseño de base de datos

- **3FN:** sin dependencias transitivas, sin redundancia
- **Integridad referencial:** claves foráneas declaradas con `REFERENCES`
- **Tablas de relación:** `PedidoPrenda` (M:N pedido-prenda), `PermisoRelacion` (árbol Composite)
- **Tabla separada para DV:** `DVVertical` (no mezcla DV con datos de negocio)
- **Tabla separada para historial:** `HistorialUsuario` (no mezcla versiones con el estado actual)
- **Discriminadores:** `EsFamilia`, `EsRol` en `Permiso` (herencia de tabla única para Composite)

### 11.5 Seguridad end-to-end

| Amenaza | Mitigación |
|---------|-----------|
| Robo de contraseñas (BD comprometida) | PBKDF2-SHA256, 100K iter, salt aleatorio |
| Ataque de diccionario sobre contraseñas | PBKDF2 costoso por diseño |
| Enumeración de usuarios (timing) | Hash señuelo + mensaje genérico |
| Robo de datos sensibles (DNI) | AES-128-CBC, IV aleatorio, clave DPAPI |
| Acceso no autorizado por rol | RBAC con re-validación fail-closed en BLL |
| Manipulación directa de BD | DVH + DVV, verificación pre-login |
| Brute-force de credenciales | Bloqueo tras 3 intentos (BD + memoria) |
| Acceso sin sesión a operaciones sensibles | `SessionManager.TienePermiso()` fail-closed |

---

*Documentación generada el 2026-06-10 — WardrobeFlow — Ingeniería de Software UAI 2026*
