# WardrobeFlow

Sistema de escritorio MDI para la gestión de suscripciones de indumentaria.  
Desarrollado en C# / .NET Framework 4.7.2 / Windows Forms / SQL Server.

**Materia:** Ingeniería de Software — UAI 2026  
**Integrantes:** Bolívar · Morana

---

## Descripción

WardrobeFlow permite a una empresa de alquiler de ropa administrar clientes, prendas, planes de suscripción y pedidos de venta. El acceso está restringido a empleados internos con roles diferenciados, cada uno con visibilidad y operaciones acotadas a su función.

---

## Stack tecnológico

| Componente | Tecnología |
|-----------|-----------|
| Lenguaje | C# (.NET Framework 4.7.2) |
| UI | Windows Forms (MDI) |
| Base de datos | SQL Server |
| Acceso a datos | ADO.NET puro (sin ORM) |
| Encriptado | PBKDF2-SHA256 (contraseñas) · AES-128-CBC (datos sensibles) |

---

## Arquitectura en capas

```
GUI (WinForms MDI)
 └── BLL (lógica de negocio)
      ├── DAL (ADO.NET → SQL Server)
      ├── BE  (entidades de dominio + DTOs)
      ├── Servicios (bitácora · multiidioma · generador de credenciales)
      └── Seguridad (sesión · encriptado · dígitos verificadores)
```

La GUI nunca accede a DAL ni a Seguridad directamente. Toda la lógica de negocio y las validaciones viven en BLL. Los formularios solo capturan eventos, invocan BLL y muestran resultados.

---

## Roles del sistema

| Rol | Permisos | Jerarquía (Composite) |
|-----|----------|-----------------------|
| **Administrador** | Acceso total: Inventario, Ventas, Administrar, Bitácora, Perfiles, Backup | — (acceso total) |
| **Auditor** | Solo Bitácora / Auditoría | rol plano |
| **Vendedor** | Prendas, Clientes, Planes, Realizar Ventas | rol base comercial |
| **GerenteComercial** | lo de Vendedor + Ver Pedidos Realizados | ⊃ Vendedor |
| **OperadorLogistico** | Ver Pedidos Realizados (despacho) | rol base inventario |
| **OperadorDeInventario** | Ver Prendas + Gestionar Stock (mantenimiento) | rol base inventario |
| **GerenteInventario** | lo de ambos operadores + Categorías/Outfits | ⊃ OperadorLogistico + OperadorDeInventario |

Los permisos se resuelven recursivamente desde el árbol Composite (tabla `PermisoRelacion`) y se cargan en sesión al hacer login. Se gestionan desde **Administrar → Perfiles y Permisos**.

> **Revisión 11/06 — simplificación de roles y permisos.** Los **permisos (patentes)** son ahora un **catálogo fijo** del sistema: no se crean, editan ni eliminan desde la UI; solo se **asignan** a roles. Los **roles** sí son administrables (crear, renombrar, eliminar y asignarles permisos u otros roles — *rol-en-rol*, que mantiene vivo el patrón Composite). Las **Familias** se **retiraron** del modelo: la migración aplana las relaciones `Rol → Patente` y desactiva los nodos Familia conservando los permisos efectivos de cada rol. Los roles `Supervisor`, `ControladorDeStock` y `EncargadoDeStock` de versiones previas se **migran automáticamente** a los vigentes.

**Cambio de rol de un usuario.** Se realiza desde **Administrar → Administración de Usuarios → Cambiar rol** (solo Administrador). La operación valida que no se quite el rol al **último Administrador activo**, graba un snapshot (Memento) y queda registrada en bitácora.

---

## Módulos

| Módulo | Descripción |
|--------|-------------|
| **Login / Logout** | Autenticación con **bloqueo de login progresivo** (1/5/15/60 min tras 3 intentos), claves de emergencia de autodesbloqueo y bloqueo de sesión en memoria |
| **Usuarios** (operaciones de cuenta) | Panel "grueso": alta de empleados, **reset de contraseña**, **desbloqueo**, **archivado** (baja lógica) y **purga** diferida; contraseñas generadas automáticamente (RNG criptográfico) y exportadas a `Documentos\WardrobeFlow\` |
| **Administración de Usuarios** (ABM de datos) | Panel de edición de datos administrativos **no sensibles**: modificar nombre, apellido, nombre de usuario, fecha de nacimiento y email; **búsqueda/filtros** por nombre, apellido o email; **cambiar rol**; y **ver el Historial de Cambios**. Nunca edita ni muestra la contraseña |
| **Perfiles y Permisos** | Árbol Composite por rol con TreeView recursivo; **ABM de roles** y asignación/remoción de permisos en tiempo real; los permisos son un catálogo fijo (no editable); **mapeo de controles** por patente; gestión delegable con anti-autobloqueo |
| **Mi Perfil** | Preferencias de UI por usuario (idioma, tipografía, tamaño, tema, formato de fecha) aplicadas en vivo, con "volver a valores de fábrica" |
| **Clientes** | ABM de suscriptores con plan, vencimiento y columna `en uso / límite`; filas en ámbar cuando la suscripción vence en ≤ 7 días |
| **Prendas** | Inventario con estados (Disponible · EnUso · EnLimpieza · Baja) y transiciones validadas en BLL |
| **Planes de Suscripción** | ABM de planes; bloquea desactivación si hay clientes asignados; bloquea asignación de plan con límite menor al stock en uso |
| **Pedidos de Venta** | Creación de pedidos respetando límite del plan; bloquea nuevo pedido si el cliente ya tiene uno Despachado sin entregar; alerta proactiva si la suscripción vence pronto |
| **Pedidos Realizados** | Ciclo post-venta: Despachar → Marcar Entregado → Registrar Devolución; filtros por estado y días |
| **Bitácora** | Registro de eventos del sistema y de negocio con filtros, criticidad y exportación a PDF |
| **Historial de Cambios** | Cambios de datos administrativos por usuario a **nivel de campo** (campo · valor anterior · valor nuevo · quién modificó · fecha/hora); **rollback** que se registra como una **nueva entrada**. No incluye ni revierte contraseñas |
| **Idiomas** | ABM de traducciones directamente en la BD |
| **Dashboard** | Panel de control personalizado por rol: el Administrador ve KPIs globales (prendas, clientes, pedidos, backup, ocupación de stock con semáforo); el Vendedor ve su pipeline de pedidos por estado; el ControladorDeStock sus métricas de stock; el Supervisor su resumen de auditoría; el Operador su vista de pedidos. Todos con auto-refresh periódico y carga asíncrona (Task.Run + BeginInvoke) |
| **Backup / Restauración** | Generación de copias **cifradas con contraseña** (`.wfbak`, AES+PBKDF2) con verificación de integridad previa; restauración con contraseña (compatible con `.bak` planos legacy); lista con autor, fecha y tamaño |
| **Reporte de Jornada** | Exportación PDF de actividad del día filtrable por rol |
| **Diagnóstico de Integridad** | Visualización y reparación asistida de filas con DVH/DVV corruptos |

---

## Patrones de diseño implementados

| Patrón | Dónde |
|--------|-------|
| **Singleton** | `SessionManager` (sesión activa) · `ContadorSesion` (intentos de login) · `DAL.Acceso` (conexión BD) |
| **Observer** | `GestorIdioma` (Subject) → formularios como observers — cambio de idioma dinámico en tiempo de ejecución |
| **Composite** | `Componente` → `Patente` (hoja) / `Rol` (nodo compuesto, anidable *rol-en-rol*) — árbol de permisos; resolución recursiva con dedup y anti-ciclos. Las Familias se retiraron del modelo (revisión 11/06); el patrón sigue vigente vía Rol. Permisos también a **nivel de control** (`ControlMapeado` + `ManejadorSeguridad`) |
| **Memento** | `BE.Usuario` (Originator) + `BE.VersionUsuario` (Memento) + `BLL.CuidadorHistorial` (Caretaker) — versiona los **datos administrativos no sensibles** (nombre/apellido/usuario/fecha nac./email), persiste en `HistorialUsuario` y permite rollback. La contraseña **no** se versiona ni se revierte |
| **Herencia** | `FormBase` → todos los formularios heredan `MostrarOk()`, `MostrarError()`, traducción de `AppException` y aplicación de seguridad por control |

---

## Validaciones de negocio en BLL

Las siguientes reglas están implementadas y validadas en la capa BLL (nunca en GUI):

- **Bloqueo de reducción de plan:** si un cliente tiene prendas en uso y se intenta asignarle un plan con menor límite, la operación falla con mensaje descriptivo.
- **Bloqueo de pedido duplicado despachado:** no se puede crear un nuevo pedido si el cliente ya tiene uno en estado `Despachado` pendiente de entrega.
- **Bloqueo de desactivación de plan:** no se puede desactivar un plan si tiene clientes activos asignados.
- **Alerta de suscripción próxima a vencer:** se detecta y propaga en todo el flujo (lista de clientes, combo de nuevo pedido, panel de confirmación) cuando la suscripción vence en ≤ 7 días.
- **Auditoría de cambio de plan:** cada modificación de plan queda registrada en `BitacoraNegocio` con el detalle `Plan: anterior → nuevo`.
- **Validación de permisos por operación:** `BLL.Cliente`, `BLL.Pedido`, `BLL.Prenda` y `BLL.Backup` validan el permiso del usuario en sesión antes de ejecutar cualquier operación de escritura.

---

## Características de seguridad

- Contraseñas **nunca en texto plano**: PBKDF2-SHA256 con salt aleatorio de 16 bytes y 100.000 iteraciones; verificación en **tiempo constante** y login resistente a enumeración (hash señuelo)
- Datos sensibles (DNI) encriptados con AES-128-CBC
- **Bloqueo de login progresivo**: 1 → 5 → 15 → 60 min según reincidencia; auto-reactivación al expirar; **claves de emergencia** de un solo uso para autodesbloqueo del admin
- Bloqueo de sesión en memoria + **handler global** de excepciones no controladas: registra el detalle técnico en bitácora y muestra al usuario un **mensaje genérico** (*"Ha ocurrido un error inesperado. Por favor, contacte al administrador del sistema."*), sin exponer información técnica
- **Acceso a Dígitos Verificadores (fail-closed):** recalcular/reparar DV es exclusivo del **Administrador**. Un usuario autenticado sin permiso queda **bloqueado**, el intento se **registra** en bitácora (visible para el administrador) y se muestra un mensaje genérico; el flujo de reparación de arranque (break-glass) sigue autorizado por `ConfirmarAdminForm`/Clave Maestra
- **Backups cifrados** con contraseña (AES-128 + PBKDF2) y **Clave Maestra de Recuperación** opcional (hash en `App.config`) para `ConfirmarAdminForm`
- **Dígitos verificadores** (DVH por fila + DVV por tabla) sobre `Usuario`, `Cliente` y `Empleado` — detecta manipulación directa en BD antes de permitir el login
- Contraseñas generadas automáticamente (RNG criptográfico + Fisher-Yates) al crear usuarios o resetear claves; se exportan a `Documentos\WardrobeFlow\`
- Verificación periódica de integridad cada 30 minutos desde el `Menu` principal (Timer + BLL.Configuracion)

---

## Multiidioma

Soporta **Español · English · Русский · Português** con cambio dinámico en tiempo de ejecución (sin reiniciar). Las traducciones se almacenan en la tabla `Traduccion` de la BD; un corpus embebido (`traducciones.tsv`) actúa como fallback **por clave** (un idioma incompleto cae al idioma por defecto). La preferencia de idioma se persiste por usuario en la BD y se restaura automáticamente al hacer login. Se pueden **agregar idiomas nuevos** desde la administración; la grilla de traducciones muestra el texto del idioma por defecto como **referencia** para completar el nuevo idioma a mano.

> **Revisión 11/06 — consistencia de traducciones.** Una traducción **en blanco ya no se persiste** ni pisa el texto por defecto: las claves sin completar caen al fallback en vez de mostrarse vacías. Se completó además la cobertura de los **dashboards** y se sumaron las claves del panel de Administración de Usuarios y del Historial de Cambios en los 4 idiomas.

---

## Instalación y configuración

### Requisitos

- Visual Studio 2022 (o superior)
- .NET Framework 4.7.2
- SQL Server (local o remoto)

### Base de datos

Ejecutar el script correspondiente desde SSMS (ambos idempotentes, en `WardrobeFlow/BD/`):

```
BD/01_Crear_BaseDeDatos.sql       -- Instalación NUEVA: estructura + datos semilla + árbol Composite
BD/02_Actualizar_BaseDeDatos.sql  -- BD EXISTENTE: aplica migraciones (columnas/tablas nuevas + datos)
```

- **Instalación nueva** → ejecutar `01_Crear_BaseDeDatos.sql`.
- **Actualizar una BD existente** → ejecutar `02_Actualizar_BaseDeDatos.sql`.

### Cadena de conexión

Configurar en `GUI/App.config`:

```xml
<connectionStrings>
  <add name="WardrobeFlowDB"
       connectionString="Data Source=.;Initial Catalog=WardrobeFlowDB;Integrated Security=True"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

### Pasos

```
1. Clonar el repositorio
2. Abrir IngSoftware-Bolivar,Morana.slnx en Visual Studio
3. Configurar la cadena de conexión en GUI/App.config
4. Ejecutar los scripts SQL en orden
5. Compilar y ejecutar GUI como proyecto de inicio
```

En el primer arranque el sistema seedea automáticamente las tablas de traducciones, idiomas, permisos y el usuario `admin2` de respaldo.

---

## Documentación técnica

Ver [`CARPETA_ENTREGA.md`](CARPETA_ENTREGA.md) para la documentación completa de cada tema (T01–T07): arquitectura, patrones, algoritmos, diagramas de flujo y criterios de evaluación.
