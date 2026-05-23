# WardrobeFlow – Trabajo Práctico Integrador

## Descripción del Sistema

**WardrobeFlow** es un sistema de gestión diseñado para la administración de prendas bajo un modelo de suscripción.

Permite a los clientes acceder a un conjunto de prendas según el plan contratado, gestionar pedidos y mantener un control eficiente del stock. El sistema asegura una correcta rotación y trazabilidad de las prendas, optimizando su uso y disponibilidad.

---

## Arquitectura

El sistema sigue una arquitectura en capas:

| Capa | Proyecto | Responsabilidad |
|------|----------|----------------|
| Presentación | `GUI` | Formularios WinForms (MDI) |
| Lógica de Negocio | `BLL` | Reglas de negocio y validaciones |
| Acceso a Datos | `DAL` | Consultas SQL con ADO.NET |
| Entidades | `BE` | Clases de dominio (POCOs) |
| Servicios transversales | `Servicios` | Bitácora, Dígito Verificador, Multiidioma |
| Seguridad | `Seguridad` | Encriptación PBKDF2-SHA256, SessionManager |

---

## Patrones de Diseño Implementados

### Singleton (T01)
- `SessionManager` — gestiona la sesión del usuario autenticado.
- `DAL.Acceso` — instancia única de conexión a base de datos.

### Composite (T04)
- `BE.Componente` (abstracto) → `BE.Familia` (nodo) + `BE.Patente` (hoja).
- `GUI.GestorPermisos` muestra el árbol de permisos con un TreeView recursivo.
- Permite asignar/quitar permisos por rol de forma visual y jerárquica.

### Observer (T05)
- `GestorIdioma` (Subject estático) notifica a todos los formularios abiertos al cambiar idioma.
- Cada formulario implementa `IIdiomaObserver` y se suscribe en `Load` / desuscribe en `FormClosing`.
- El cambio de idioma es **dinámico e inmediato** — sin reinicios ni ventanas adicionales.
- La preferencia de idioma se **persiste por usuario** en la columna `IdIdioma` de la tabla `Usuario` y se restaura automáticamente al hacer login.

### Dígitos Verificadores (T07)
- **DVH** (horizontal): calculado por fila como `Σ(ASCII(char_i) × posición_i) % 10` sobre los campos críticos del usuario. Se recalcula en cada escritura.
- **DVV** (vertical): calculado sobre todos los DVH de la tabla. Se almacena en la tabla `DVVertical`.
- Verificación al inicio de la aplicación: si hay discrepancia, se bloquea el acceso y se alerta al Administrador.
- El Administrador puede recalcular los DVs desde **Administrar → Usuarios → Recalcular DV**.

---

## Funcionalidades Principales

- Gestión de clientes (alta, baja y modificación).
- Administración de planes de suscripción.
- Control de stock de prendas.
- Creación y gestión de pedidos.
- Asignación de prendas según disponibilidad y plan contratado.
- Seguimiento del estado de las prendas (con historial de cambios).
- Registro de acciones en la bitácora (auditoría).
- Gestión de usuarios con control de bloqueo por intentos fallidos.
- Gestor de perfiles y permisos por rol (Composite).
- Soporte multiidioma: Español, English, Русский (Observer).

---

## Configuración e Instalación

### Requisitos
- Visual Studio 2019 o superior
- SQL Server (cualquier edición local)
- .NET Framework 4.7.2

### Base de Datos

Ejecutar los scripts SQL en orden desde SQL Server Management Studio (SSMS):

```
SQL/WardrobeFlowDB_Create.sql       -- Creación de la base y tablas iniciales
SQL/WardrobeFlowDB_Alter_v3.0.sql   -- Agrega DVH (Usuario) y tabla DVVertical
SQL/WardrobeFlowDB_Alter_v4.0.sql   -- Agrega IdIdioma (preferencia de idioma por usuario)
```

Luego de aplicar los scripts, al iniciar la aplicación por primera vez:
1. Hacer login con el usuario Administrador.
2. Ir a **Administrar → Usuarios → Recalcular DV** para inicializar los dígitos verificadores.

### String de conexión

Configurar en `DAL/App.config` (o `GUI/App.config`) el servidor SQL:

```xml
<connectionStrings>
  <add name="WardrobeFlowDB"
       connectionString="Data Source=.;Initial Catalog=WardrobeFlowDB;Integrated Security=True"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

---

## Roles del Sistema

| Rol | Acceso |
|-----|--------|
| Administrador | Todo: Inventario, Ventas, Administrar, Bitácora, Perfiles |
| Supervisor | Bitácora |
| Vendedor | Clientes, Planes, Pedidos de Venta |
| OperadorLogistico | Prendas, Pedidos Realizados |
| ControladorDeStock | Prendas |
| OperadorDeInventario | Pedidos Realizados |
