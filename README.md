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

> **Revisión 11/06.** Se aplicaron las correcciones de la revisión: permisos (patentes) como **catálogo fijo** (no editable) con **roles administrables** y **Familias retiradas** del modelo; **dos paneles** de usuarios (operaciones de cuenta vs. ABM de datos); **Historial de Cambios a nivel de campo** (sin contraseñas) con rollback registrado como nueva entrada; **cambio de rol** con protección del último admin; **acceso a DV exclusivo de Administrador** (fail-closed + registro); **mensaje de error genérico** ante fallos no controlados; y **traducciones**: no se persisten en blanco y se completó la cobertura de dashboards/paneles.

### Composite — Perfiles y Permisos (T04)
El patrón Composite es el **motor real de autorización**:
- **Revisión 11/06:** las **Familias** se eliminaron del modelo (la migración aplana `Rol → Patente` y desactiva los nodos Familia, preservando los permisos efectivos). Los **permisos** son un **catálogo fijo** (solo se asignan a roles); los **roles** se crean/renombran/eliminan y admiten *rol-en-rol* (Composite vigente).
- `BE.Componente` (abstracto) → `BE.Patente` (hoja, permiso simple) + `BE.Familia` (nodo compuesto) + `BE.Rol : Familia` (rol asignable que puede contener familias, patentes y **otros roles**).
- La composición se persiste en la tabla **`PermisoRelacion`** (única fuente de verdad). Un rol es una fila de `Permiso` con `EsRol = 1`.
- Los **permisos efectivos** de un usuario se resuelven **recursivamente** (`BLL.Familia.ObtenerPermisosEfectivos`), recorriendo rol → roles/familias → patentes y deduplicando permisos repetidos.
- `GUI.GestorPermisos`: asignación por **dos listas** (Familias / Patentes), TreeView recursivo en vivo, CRUD de patentes/familias/roles y opción de **embeber un rol dentro de otro** (con prevención de referencias circulares).
- No se puede **eliminar un rol** si tiene usuarios asignados (se avisa cuáles).
- **Validación en dos niveles**: la GUI oculta menús según permisos *y* la BLL **re-valida** en el backend (`SessionManager.TienePermiso`) de forma *fail-closed* (sin sesión, se rechaza).
- **Permisos a nivel de control**: además del menú, una patente puede gobernar **cualquier control** (botón, ítem de menú) de cualquier formulario, mapeado desde la UI (**Administrar → Perfiles → "Mapear controles"**). El `GUI.ManejadorSeguridad` muestra/oculta los controles mapeados según las patentes efectivas del usuario; el mapeo se persiste en la tabla **`ControlMapeado`**. El Administrador siempre ve todo (bypass).
- **Re-aplicación en vivo**: al cambiar permisos, la seguridad se re-aplica a los formularios abiertos sin reiniciar (`ManejadorSeguridad.ActualizarSeguridadFormulariosAbiertos`).
- La gestión de permisos puede **delegarse**: la realiza el Administrador o cualquier rol que tenga la patente *Gestión de Usuarios*. Un usuario no-admin **no puede quitarse a sí mismo** ese acceso (anti-autobloqueo).

### Observer — Multiidioma (T05)
- `GestorIdioma` (Subject) notifica a todos los formularios abiertos al cambiar el idioma.
- Cada formulario implementa `IIdiomaObserver` (suscribe en `Load`, desuscribe en `FormClosing`). Cambio **dinámico e inmediato**, en login y en el menú principal.
- Idiomas soportados: **Español, English, Русский, Português**. Se pueden **agregar idiomas nuevos** en caliente desde la administración.
- Modelo de datos: **Idioma** (`Id`, `Nombre`, `Activo`), **Control** (`Clave`, `Formulario`) y **Traducción** (`IdControl`, `IdIdioma`, `Texto`). No se usan recursos estáticos `.resx`.
- Administración (`FormIdiomas`): grillas de **Idiomas**, **Controles** y **Traducciones**. La grilla de traducciones muestra una columna **"Referencia (por defecto)"** con el texto del idioma base, para que un traductor complete un idioma nuevo viendo el original al lado (las claves sin traducir aparecen con texto vacío, listas para completar).
- **Fallback por clave**: si falta una traducción, se usa el texto del idioma por defecto en lugar de dejar el control sin traducir. Al activar un idioma incompleto, se avisa. Al activar/desactivar un idioma, los selectores de idioma se actualizan al instante.
- La preferencia de idioma se **persiste por usuario** (`Usuario.IdIdioma`) y se **restaura al hacer login** (la pantalla de login también respeta el idioma elegido).

### Memento — Control de Cambios (T06)
- **Originator** → `BE.Usuario` (`CrearMemento` / `RestaurarDesde`).
- **Memento** → `BE.VersionUsuario : BE.Memento.IMemento` (cápsula del estado; expone solo metadatos al Caretaker).
- **Caretaker** → `BLL.CuidadorHistorial` (guarda/recupera el historial sin interpretar el estado; persiste en `HistorialUsuario`).
- Permite **deshacer** cambios (rollback) sobre un usuario. Antes de restaurar, se guarda un memento del estado actual → habilita el *rollback de un rollback*. Si no se puede guardar el snapshot, la operación se **aborta** (fail-safe).
- **Revisión 11/06:** el Memento versiona los **datos administrativos NO sensibles** (nombre, apellido, nombre de usuario, fecha de nacimiento, email). La **contraseña no se versiona ni se revierte**. El **Historial de Cambios** se muestra a **nivel de campo** (identificador del registro, campo, valor anterior, valor nuevo, quién y cuándo) y el **rollback queda registrado como una nueva entrada**.

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
- **Bloqueo de login progresivo**: tras 3 intentos fallidos la cuenta se bloquea por un tiempo **escalonado** (1 → 5 → 15 → 60 minutos según cuántas veces se bloqueó); al expirar se reactiva sola en el próximo login. Superada la escala, queda bloqueada de forma permanente (requiere Administrador o clave de emergencia). Los bloqueos manuales del Administrador no auto-expiran.
- **Claves de emergencia (autodesbloqueo)**: set de claves de un solo uso (tipo códigos de respaldo) que permiten a un Administrador desbloquear su propia cuenta desde el login sin depender de otro admin.
- **Autorización de operaciones sensibles**: re-validada en el backend de forma *fail-closed* (sin sesión, se rechaza). La gestión de usuarios (`BLL.Usuario`) es exclusiva del Administrador; la gestión de perfiles/permisos (`BLL.Familia`) la puede hacer el Administrador o un rol con la patente *Gestión de Usuarios*.
- **Confirmación de Administrador**: las operaciones críticas (restaurar backup, recalcular DV) piden credenciales en `ConfirmarAdminForm`, que además acepta una **Clave Maestra de Recuperación** (hash en `App.config`, desactivada si está vacía) como "break glass" cuando la BD está corrupta y no se puede iniciar sesión.
- **Manejo de excepciones**: las excepciones de dominio (`BE.AppException`) llevan clave de traducción y se muestran en el idioma activo; las inesperadas se registran en la bitácora con criticidad alta. Un **handler global** (`Program.cs`) captura cualquier excepción no controlada, la registra y avisa, evitando cierres mudos.

### Backups y Restauración
- **Backups cifrados con contraseña**: las copias nuevas se generan cifradas (`.wfbak`) con **AES-128** y clave derivada de la contraseña por **PBKDF2** (`Seguridad.CifradorArchivos`). La restauración pide la contraseña y descifra (mensaje claro si es incorrecta); se mantiene compatibilidad para restaurar `.bak` planos anteriores.
- **Validación previa**: antes de generar un backup se verifica la integridad (DVH/DVV) para no respaldar datos corruptos; no se permite restaurar un backup ilegible/corrupto (`RESTORE VERIFYONLY`). Antes de restaurar se informa el **alcance de la pérdida** (fecha del backup).
- Los archivos de credenciales/claves generadas se guardan en **`Documentos\WardrobeFlow\`** (persistente y visible).

---

## Funcionalidades Principales

- Gestión de clientes (alta, baja y modificación) con DNI cifrado.
- Administración de planes de suscripción.
- Control de stock de prendas y seguimiento de su estado (con historial).
- Creación y gestión de pedidos; asignación de prendas según plan.
- Bitácora de auditoría (sistema y negocio) con búsquedas combinadas.
- Gestión de usuarios con **bloqueo de login progresivo**, baja lógica (archivado) + purga diferida y **control de cambios** (rollback).
- Gestor de perfiles y permisos por rol (Composite, motor real de autorización) con **mapeo de controles** y delegación de la gestión.
- Soporte multiidioma dinámico: Español, English, Русский, Português (Observer), con alta de idiomas y traducción asistida.
- **Mi Perfil**: preferencias de UI por usuario (idioma, tipografía, tamaño, tema, formato de fecha) aplicadas en vivo, con opción de volver a valores de fábrica.
- Backups **cifrados** con verificación de integridad + asistente de restauración.
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

> Los backups generados desde la app son **cifrados** (`.wfbak`) y se crean en la carpeta `Backups/` del ejecutable; las credenciales/claves generadas se guardan en `Documentos\WardrobeFlow\`. Los `.bak` planos quedan fuera del control de versiones (ver `.gitignore`).

### Usuarios iniciales (script de BD)

| Usuario | Contraseña | Rol |
|---|---|---|
| `admin` | `administrador1!` | Administrador |
| `gcomercial` | `usuario1!` | Gerente Comercial |
| `vendedor` | `vendedor1!` | Vendedor |
| `ginventario` | `usuario1!` | Gerente de Inventario |
| `operador` | `operador1!` | Operador de Inventario |
| `logistico` | `usuario1!` | Operador Logístico |
| `auditor` | `usuario1!` | Auditor |

> Los usuarios `supervisor`, `stock` y `encargado` de versiones anteriores se **migran automáticamente** a los roles vigentes al actualizar la BD (Supervisor → Gerente Comercial; Controlador de Stock / Encargado de Stock → Operador de Inventario).

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

| Rol | Acceso | Jerarquía (Composite) |
|-----|--------|-----------------------|
| Administrador | Todo: Inventario, Ventas, Administrar, Bitácora, Perfiles, Diagnóstico | — (acceso total) |
| Auditor | Bitácora / Auditoría | rol plano |
| Vendedor | Prendas, Clientes, Planes, Realizar Ventas | rol base comercial |
| Gerente Comercial | lo de Vendedor + Ver Pedidos Realizados | ⊃ Vendedor |
| Operador Logístico | Ver Pedidos Realizados (despacho) | rol base inventario |
| Operador de Inventario | Ver Prendas, Gestionar Stock (mantenimiento) | rol base inventario |
| Gerente de Inventario | lo de ambos operadores + Categorías/Outfits | ⊃ Operador Logístico + Operador de Inventario |

> Los roles son nodos del árbol Composite y pueden crearse/editarse (y anidarse entre sí) desde **Administrar → Perfiles**. Ver detalle en `WardrobeFlow/BD/ROLES_Y_VISTAS.txt`.
