# WardrobeFlow – Trabajo Práctico Integrador

## Descripción del Sistema

**WardrobeFlow** es un sistema de gestión diseñado para la administración de prendas bajo un modelo de suscripción.

Permite a los clientes acceder a un conjunto de prendas según el plan contratado, gestionar pedidos y mantener un control eficiente del stock. El sistema asegura una correcta rotación y trazabilidad de las prendas, optimizando su uso y disponibilidad.

---

## Arquitectura

Arquitectura en capas, con dependencias **acíclicas** (`GUI → BLL → Servicios → DAL → Seguridad → BE`) y **sin framework de persistencia** (ADO.NET con consultas parametrizadas).

| Capa | Proyecto | Responsabilidad |
|------|----------|----------------|
| Presentación | `GUI` | Formularios WinForms (MDI) |
| Lógica de Negocio | `BLL` | Reglas de negocio y validaciones |
| Acceso a Datos | `DAL` | Consultas SQL con ADO.NET (parametrizadas) |
| Entidades | `BE` | Clases de dominio (POCOs) |
| Servicios transversales | `Servicios` | Bitácora, Dígito Verificador, Multiidioma |
| Seguridad | `Seguridad` | Hash PBKDF2, cifrado AES, SessionManager, DV |

---

## Patrones de Diseño Implementados

### Singleton (T01)
- `Seguridad.SessionManager` — gestiona la sesión del usuario autenticado.
- `DAL.Acceso` — instancia única de acceso a la base de datos (con soporte de transacciones).
- `Seguridad.ContadorSesion` — control de intentos por sesión.

### Composite — Perfiles y Permisos (T04)
El patrón Composite es el **motor real de autorización**:
- `BE.Componente` (abstracto) → `BE.Patente` (hoja, permiso simple) + `BE.Familia` (nodo compuesto) + `BE.Rol : Familia` (rol asignable que puede contener familias, patentes y **otros roles**).
- La composición se persiste en la tabla **`PermisoRelacion`** (única fuente de verdad). Un rol es una fila de `Permiso` con `EsRol = 1`.
- Los **permisos efectivos** de un usuario se resuelven **recursivamente** (`BLL.Familia.ObtenerPermisosEfectivos`), recorriendo rol → roles/familias → patentes y deduplicando permisos repetidos.
- `GUI.GestorPermisos`: asignación por **dos listas** (Familias / Patentes), TreeView recursivo en vivo, CRUD de patentes/familias/roles y opción de **embeber un rol dentro de otro** (con prevención de referencias circulares).
- No se puede **eliminar un rol** si tiene usuarios asignados (se avisa cuáles).
- **Validación en dos niveles**: la GUI oculta menús según permisos *y* la BLL **re-valida** en el backend (`SessionManager.TienePermiso`) de forma *fail-closed* (sin sesión, se rechaza).

### Observer — Multiidioma (T05)
- `GestorIdioma` (Subject) notifica a todos los formularios abiertos al cambiar el idioma.
- Cada formulario implementa `IIdiomaObserver` (suscribe en `Load`, desuscribe en `FormClosing`). Cambio **dinámico e inmediato**, en login y en el menú principal.
- Modelo de datos: **Idioma** (`Id`, `Nombre`, `Activo`), **Control** (`Clave`, `Formulario`) y **Traducción** (`IdControl`, `IdIdioma`, `Texto`). No se usan recursos estáticos `.resx`.
- Administración (`FormIdiomas`): grillas de **Idiomas**, **Controles** y **Traducciones**.
- **Fallback por clave**: si falta una traducción, se usa el texto del idioma por defecto en lugar de dejar el control sin traducir. Al activar un idioma incompleto, se avisa.
- La preferencia de idioma se **persiste por usuario** (`Usuario.IdIdioma`) y se restaura al hacer login.

### Memento — Control de Cambios (T06)
- **Originator** → `BE.Usuario` (`CrearMemento` / `RestaurarDesde`).
- **Memento** → `BE.VersionUsuario : BE.Memento.IMemento` (cápsula del estado; expone solo metadatos al Caretaker).
- **Caretaker** → `BLL.CuidadorHistorial` (guarda/recupera el historial sin interpretar el estado; persiste en `HistorialUsuario`).
- Permite **deshacer** cambios (rollback) sobre un usuario. Antes de restaurar, se guarda un memento del estado actual → habilita el *rollback de un rollback*. Si no se puede guardar el snapshot, la operación se **aborta** (fail-safe).

### Dígitos Verificadores (T07)
- **DVH** (horizontal, por fila) y **DVV** (vertical, por tabla, en `DVVertical`), con módulo primo `999_983` (mucho más resistente a colisiones que mod 10).
- Protege las entidades sensibles: **Usuario, Cliente y Empleado** (implementación genérica y reutilizable: `DAL.DigitoVerificador.RecalcularTabla(tabla, pk, columnas)`).
- Verificación **al iniciar** la aplicación (antes del login), **antes de operaciones sensibles** de usuarios (alta/reset/desbloqueo) y por un *timer* periódico. Ante manipulación o error de verificación, **bloquea** el acceso (fail-safe).
- Recálculo manual desde **Administrar → Usuarios → Recalcular DV** y desde **Diagnóstico de Integridad**.

### Herencia
- `GUI.FormBase` centraliza comportamiento común de los formularios (feedback `MostrarOk`/`MostrarError`, traducción de `AppException`, **registro de excepciones inesperadas en la bitácora**).

---

## Seguridad y Datos Sensibles (T03)

- **Contraseñas**: hash **PBKDF2-SHA256** con salt aleatorio y 100.000 iteraciones (`Seguridad.Encriptador`). Nunca se almacenan en texto plano. La verificación compara los hashes en **tiempo constante** (XOR acumulado sobre los 32 bytes), para no filtrar información por temporización.
- **DNI** de Cliente y Empleado: **cifrado AES-128-CBC** (IV aleatorio por registro). Se descifra al leer; la unicidad de DNI se valida en la capa de negocio (comparando en memoria, ya que el cifrado no es determinista).
- **Login resistente a enumeración de usuarios**: ante un usuario inexistente el login ejecuta igualmente un PBKDF2 contra un **hash señuelo** (iguala el costo temporal del caso real, cerrando el canal lateral de temporización), cuenta el intento en la sesión (`ContadorSesion`) y lo registra en la bitácora. La GUI muestra el **mismo mensaje genérico** ("Usuario o contraseña incorrectos") tanto si el usuario no existe como si la contraseña es incorrecta, sin revelar cuál de los dos falló.
- **Autorización de operaciones sensibles**: la gestión de usuarios (`BLL.Usuario`) y del árbol de perfiles/permisos (`BLL.Familia`) es **exclusiva del Administrador**, re-validada en el backend de forma *fail-closed* (sin sesión, se rechaza).
- **Manejo de excepciones**: las excepciones de dominio (`BE.AppException`) llevan clave de traducción y se muestran en el idioma activo; las excepciones inesperadas se registran en la bitácora con criticidad alta.

---

## Funcionalidades Principales

- Gestión de clientes (alta, baja y modificación) con DNI cifrado.
- Administración de planes de suscripción.
- Control de stock de prendas y seguimiento de su estado (con historial).
- Creación y gestión de pedidos; asignación de prendas según plan.
- Bitácora de auditoría (sistema y negocio) con búsquedas combinadas.
- Gestión de usuarios con bloqueo por intentos fallidos y **control de cambios** (rollback).
- Gestor de perfiles y permisos por rol (Composite, motor real de autorización).
- Soporte multiidioma dinámico: Español, English, Русский (Observer).
- Dígitos verificadores de integridad en Usuario, Cliente y Empleado.

---

## Configuración e Instalación

### Requisitos
- Visual Studio 2019 o superior
- SQL Server (cualquier edición local; p. ej. SQL Server Express)
- .NET Framework 4.7.2

### Base de Datos

Los scripts SQL están consolidados en **`WardrobeFlow/BD/`** (idempotentes):

```
01_Crear_BaseDeDatos.sql      -- Crea la BD COMPLETA desde cero: estructura,
                                 datos semilla (permisos, roles, usuarios,
                                 idiomas) y árbol Composite.
02_Actualizar_BaseDeDatos.sql -- Aplica las migraciones sobre una BD existente
                                 (columnas/tablas nuevas + migración Composite).
```

- **Instalación nueva** → ejecutar `01_Crear_BaseDeDatos.sql`.
- **Actualizar una BD existente** → ejecutar `02_Actualizar_BaseDeDatos.sql`.

Los dígitos verificadores se inicializan solos en el primer arranque; también pueden recalcularse desde **Administrar → Usuarios → Recalcular DV**.

> Los backups (`.bak`) se conservan en `Ingenieria_Software/BD/`.

### Usuarios iniciales (script `01_Crear`)

| Usuario | Contraseña | Rol |
|---|---|---|
| `admin` | `administrador1!` | Administrador |
| `supervisor` | `supervisor1!` | Supervisor |
| `vendedor` | `vendedor1!` | Vendedor |
| `stock` | `controladorstock1!` | Controlador de Stock |
| `operador` | `operador1!` | Operador de Inventario |

### String de conexión

Configurar en `GUI/App.config` el servidor SQL:

```xml
<connectionStrings>
  <add name="WardrobeFlowDB"
       connectionString="Data Source=.\SQLEXPRESS;Initial Catalog=WardrobeFlowDB;Integrated Security=True;TrustServerCertificate=True"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

---

## Roles del Sistema

| Rol | Acceso |
|-----|--------|
| Administrador | Todo: Inventario, Ventas, Administrar, Bitácora, Perfiles, Diagnóstico |
| Supervisor | Bitácora / Auditoría |
| Vendedor | Prendas, Clientes, Planes, Pedidos de Venta |
| ControladorDeStock | Prendas, Stock |
| OperadorDeInventario | Pedidos Realizados |

> Los roles son nodos del árbol Composite y pueden crearse/editarse desde **Administrar → Perfiles**.
