# WardrobeFlow

Sistema de escritorio MDI para la gestión de suscripciones de indumentaria.  
Desarrollado en C# / .NET Framework 4.7.2 / Windows Forms / SQL Server.

**Materia:** Ingeniería de Software — UAI 2026  
**Integrantes:** Bolívar · Morana

---

## Descripción

WardrobeFlow permite a una empresa de alquiler de ropa administrar clientes, prendas, planes de suscripción y pedidos de venta. El acceso está restringido a empleados internos con roles diferenciados (Administrador, Supervisor, Vendedor, entre otros).

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
      ├── BE  (entidades de dominio)
      ├── Servicios (bitácora · multiidioma · generador de credenciales)
      └── Seguridad (sesión · encriptado · dígitos verificadores)
```

La GUI nunca accede a DAL ni a Seguridad directamente. Toda la lógica vive en BLL.

---

## Módulos

| Módulo | Descripción |
|--------|-------------|
| **Login / Logout** | Autenticación con bloqueo tras 3 intentos fallidos |
| **Usuarios** | ABM de empleados con generación automática de contraseñas |
| **Perfiles y Permisos** | Árbol de permisos por rol (Patrón Composite) |
| **Clientes** | ABM de suscriptores con plan asociado |
| **Prendas** | Inventario de indumentaria con estados y cambios de estado |
| **Planes de Suscripción** | ABM de planes con límite de prendas y precio |
| **Pedidos de Venta** | Asignación de prendas a clientes |
| **Pedidos Realizados** | Historial de pedidos y gestión de despacho/devolución |
| **Bitácora** | Registro de eventos del sistema y de negocio con filtros |
| **Historial de Cambios** | Snapshots de usuarios con restauración a versiones previas |
| **Idiomas** | ABM de traducciones en la BD |
| **Dashboard** | Vista resumen con actividad reciente |
| **Backup / Restauración** | Copia de seguridad y restauración de la base de datos |
| **Reporte de Jornada** | Exportación PDF de actividad |

---

## Patrones de diseño implementados

| Patrón | Dónde |
|--------|-------|
| **Singleton** | `SessionManager` (sesión activa) · `ContadorSesion` (intentos de login) |
| **Observer** | `GestorIdioma` (Subject) → 19 formularios como observers — cambio de idioma en tiempo de ejecución |
| **Composite** | `Componente` → `Familia` (nodo) / `Patente` (hoja) — árbol de permisos |

---

## Características de seguridad

- Contraseñas **nunca en texto plano**: PBKDF2-SHA256 con salt aleatorio de 16 bytes y 100.000 iteraciones
- Datos sensibles (DNI) encriptados con AES-128-CBC
- Bloqueo de cuenta tras 3 intentos fallidos consecutivos (persistido en BD)
- Bloqueo de sesión tras 3 intentos fallidos (en memoria, requiere reiniciar la app)
- **Dígitos verificadores** (DVH por fila + DVV por tabla) sobre la tabla `Usuario` — detecta manipulación directa en BD antes de permitir el login
- Contraseñas generadas automáticamente (RNG criptográfico + Fisher-Yates) al crear usuarios o resetear claves; se exportan a `CredencialesGeneradas/`

---

## Multiidioma

Soporta **Español · English · Русский** con cambio dinámico en tiempo de ejecución (sin reiniciar). Las traducciones se almacenan en la tabla `Traduccion` de la BD; el código hardcodeado actúa solo como fallback de primer arranque. Agregar un nuevo idioma requiere únicamente insertar filas en la BD, sin tocar el código.

---

## Requisitos para ejecutar

- Visual Studio 2022 (o superior)
- .NET Framework 4.7.2
- SQL Server (local o remoto)
- Cadena de conexión configurada en `DAL/ConexionBD.cs`

### Pasos

```
1. Clonar el repositorio
2. Abrir IngSoftware-Bolivar,Morana.slnx en Visual Studio
3. Configurar la cadena de conexión en DAL/ConexionBD.cs
4. Ejecutar el script de creación de base de datos (si es primera vez)
5. Compilar y ejecutar GUI como proyecto de inicio
```

En el primer arranque, el sistema seedea automáticamente las tablas de traducciones, idiomas y permisos.

---

## Documentación técnica

Ver [`CARPETA_ENTREGA.md`](CARPETA_ENTREGA.md) para la documentación completa de cada tema (T01–T07): arquitectura, patrones, algoritmos, diagramas de flujo y criterios de evaluación.
