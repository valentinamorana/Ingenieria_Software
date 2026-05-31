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

| Rol | Permisos |
|-----|----------|
| **Administrador** | Acceso total: Inventario, Ventas, Administrar, Bitácora, Perfiles, Backup |
| **Auditor** | Solo Bitácora |
| **GerenteComercial** | Ventas completas + Bitácora de negocio |
| **Vendedor** | Clientes, Planes, Pedidos de Venta |
| **GerenteInventario** | Inventario completo + Pedidos Realizados |
| **EncargadoDeStock** | Prendas (alta, modificación, cambio de estado) |
| **OperadorLogistico** | Prendas (solo lectura) + Pedidos Realizados |
| *(legacy)* Supervisor | Bitácora |
| *(legacy)* ControladorDeStock | Prendas |
| *(legacy)* OperadorDeInventario | Pedidos Realizados |

Los permisos se asignan desde **Administrar → Perfiles y Permisos** y se almacenan en `RolPermiso`. Se cargan en sesión al hacer login.

---

## Módulos

| Módulo | Descripción |
|--------|-------------|
| **Login / Logout** | Autenticación con bloqueo de cuenta tras 3 intentos fallidos (BD) y bloqueo de sesión en memoria |
| **Usuarios** | ABM de empleados; contraseñas generadas automáticamente (RNG criptográfico) y exportadas a `CredencialesGeneradas/` |
| **Perfiles y Permisos** | Árbol de permisos por rol con TreeView recursivo (Composite); asignación/remoción en tiempo real |
| **Clientes** | ABM de suscriptores con plan, vencimiento y columna `en uso / límite`; filas en ámbar cuando la suscripción vence en ≤ 7 días |
| **Prendas** | Inventario con estados (Disponible · EnUso · EnLimpieza · Baja) y transiciones validadas en BLL |
| **Planes de Suscripción** | ABM de planes; bloquea desactivación si hay clientes asignados; bloquea asignación de plan con límite menor al stock en uso |
| **Pedidos de Venta** | Creación de pedidos respetando límite del plan; bloquea nuevo pedido si el cliente ya tiene uno Despachado sin entregar; alerta proactiva si la suscripción vence pronto |
| **Pedidos Realizados** | Ciclo post-venta: Despachar → Marcar Entregado → Registrar Devolución; filtros por estado y días |
| **Bitácora** | Registro de eventos del sistema y de negocio con filtros, criticidad y exportación a PDF |
| **Historial de Cambios** | Snapshots de versiones de usuarios con restauración a estado anterior |
| **Idiomas** | ABM de traducciones directamente en la BD |
| **Dashboard** | KPIs en tiempo real: prendas disponibles, clientes, pedidos pendientes, ocupación del stock (%) con semáforo de color, días sin backup y actividad reciente |
| **Backup / Restauración** | Generación y restauración de `.bak`; lista con autor, fecha y tamaño de cada copia |
| **Reporte de Jornada** | Exportación PDF de actividad del día filtrable por rol |
| **Diagnóstico de Integridad** | Visualización y reparación asistida de filas con DVH/DVV corruptos |

---

## Patrones de diseño implementados

| Patrón | Dónde |
|--------|-------|
| **Singleton** | `SessionManager` (sesión activa) · `ContadorSesion` (intentos de login) · `DAL.Acceso` (conexión BD) |
| **Observer** | `GestorIdioma` (Subject) → formularios como observers — cambio de idioma dinámico en tiempo de ejecución |
| **Composite** | `Componente` → `Familia` (nodo) / `Patente` (hoja) — árbol jerárquico de permisos por rol |
| **Herencia** | `FormBase` → todos los formularios heredan `MostrarOk()`, `MostrarError()` y soporte de traducción de `AppException` |

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

- Contraseñas **nunca en texto plano**: PBKDF2-SHA256 con salt aleatorio de 16 bytes y 100.000 iteraciones
- Datos sensibles (DNI) encriptados con AES-128-CBC
- Bloqueo de cuenta tras 3 intentos fallidos consecutivos (persistido en BD)
- Bloqueo de sesión tras 3 intentos fallidos (en memoria, requiere reiniciar la app)
- **Dígitos verificadores** (DVH por fila + DVV por tabla) sobre la tabla `Usuario` — detecta manipulación directa en BD antes de permitir el login
- Contraseñas generadas automáticamente (RNG criptográfico + Fisher-Yates) al crear usuarios o resetear claves; se exportan a `CredencialesGeneradas/`
- Verificación periódica de integridad cada 30 minutos desde el `Menu` principal (Timer + BLL.Configuracion)

---

## Multiidioma

Soporta **Español · English · Русский** con cambio dinámico en tiempo de ejecución (sin reiniciar). Las traducciones se almacenan en la tabla `Traduccion` de la BD; el código hardcodeado actúa solo como fallback de primer arranque. La preferencia de idioma se persiste por usuario en la BD y se restaura automáticamente al hacer login. Agregar un nuevo idioma requiere únicamente insertar filas en la BD, sin tocar el código.

---

## Instalación y configuración

### Requisitos

- Visual Studio 2022 (o superior)
- .NET Framework 4.7.2
- SQL Server (local o remoto)

### Base de datos

Ejecutar los scripts SQL en orden desde SSMS:

```
SQL/WardrobeFlowDB_Create.sql       -- Creación de base y tablas iniciales
SQL/WardrobeFlowDB_Alter_v3.0.sql   -- Agrega DVH (Usuario) y tabla DVVertical
SQL/WardrobeFlowDB_Alter_v4.0.sql   -- Agrega IdIdioma (preferencia de idioma por usuario)
```

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
