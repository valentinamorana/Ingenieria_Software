# WardrobeFlow — Estructura de Capas

## Proyectos (.NET Framework 4.7.2)

```
WardrobeFlow.sln
│
├── BE/                        ← Class Library — Entidades del negocio
│   ├── Persona.cs
│   ├── Usuario.cs             ← T03: SHA256 + salt  |  T02: VerificarClave
│   ├── Componente.cs          ← Patrón Composite (base)
│   ├── Permiso.cs             ← Composite hoja
│   ├── GrupoPermiso.cs        ← Composite contenedor
│   ├── Categoria.cs
│   ├── Prenda.cs
│   ├── Outfit.cs
│   ├── DetalleOutfit.cs
│   ├── AuditoriaSesion.cs     ← T06a: Bitácora
│   ├── IDescripcionPrenda.cs  ← Patrón Decorator (interfaz)
│   ├── PrendaDescripcionBase.cs
│   ├── DecoradorPrenda.cs
│   ├── DecoradorTemporada.cs
│   └── DecoradorOcasion.cs
│
├── DAL/                       ← Class Library — Acceso a datos (SQL Server)
│   ├── DAL_Conexion.cs        ← Patrón Singleton (doble verificación)
│   ├── DAL_Usuario.cs
│   ├── DAL_Categoria.cs
│   └── DAL_Prenda.cs
│
├── BLL/                       ← Class Library — Lógica de negocio
│   ├── BLL_Usuario.cs         ← Genera hash antes de guardar
│   ├── BLL_Categoria.cs
│   └── BLL_Prenda.cs          ← ObtenerDescripcionCompleta() usa Decorator
│
├── Seguridad/                 ← Class Library — Permisos + Auditoría
│   ├── DAL_Permiso.cs
│   ├── DAL_AuditoriaSesion.cs
│   ├── BLL_Permiso.cs
│   └── BLL_AuditoriaSesion.cs
│
└── GUI/                       ← Windows Forms App
    ├── App.config             ← Cadena de conexión
    ├── InicioDeSesion.cs      ← T02: Login
    ├── Inicio.cs              ← MDI container + permisos de menú
    └── Modales/
        ├── frmUsuario.cs
        ├── frmCategoria.cs
        ├── frmPrenda.cs
        ├── frmPermisoUsuario.cs
        └── frmAuditoriaSesion.cs  ← T06a: vista bitácora

## Referencias entre proyectos
DAL      → BE
BLL      → BE + DAL
Seguridad → BE + DAL
GUI      → BE + BLL + Seguridad
```

## Patrones implementados
| Patrón    | Dónde                                          |
|-----------|------------------------------------------------|
| Singleton | DAL/DAL_Conexion.cs                            |
| Composite | BE/Componente.cs + Permiso.cs + GrupoPermiso.cs|
| Decorator | BE/IDescripcionPrenda → DecoradorTemporada/Ocasion |

## Entregas cubiertas
- T01 — Arquitectura en capas ✓
- T02 — Login/Logout + usuarios + Singleton ✓
- T03 — Encriptación SHA256 + salt ✓
- T06a — Bitácora de sesiones ✓
