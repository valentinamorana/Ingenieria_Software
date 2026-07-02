# Tercera Entrega — WardrobeFlow (Bolívar, Morán)

Esta carpeta completa los entregables documentales de la **3ª iteración** que faltaban
respecto de las consignas de la cátedra, y documenta las correcciones de código aplicadas.

## Contenido (DIAGRAMAS/)

Los diagramas se entregan como **fuente PlantUML (`.puml`)** — texto versionable y
renderable. Para obtener el PNG/SVG:

- Online: pegar el contenido en <https://www.plantuml.com/plantuml>
- VS Code: extensión *PlantUML* (Alt+D para previsualizar) → exportar PNG.
- CLI: `java -jar plantuml.jar "*.puml"`

| Archivo | Requisito | Descripción |
|---|---|---|
| `Diagrama de Componentes.puml` | **T01** | Componentes y dependencias reales entre capas (según los `.csproj`). |
| `G06 - Diagrama de Clases por Capa.puml` | **G06** | Clases agrupadas **por capa**, separando **negocio (amarillo)** de **técnico (celeste)**. |
| `G07 - Modelo de Datos Integrado.puml` | **G07** | DER integrado de todas las tablas, negocio vs técnico. Balanceado con `BD/01`. |
| `CU04 - Secuencia - Gestion de Perfiles.puml` | **T04** | Secuencia del Composite (árbol, TreeView recursivo, permisos efectivos). |
| `CU05 - Secuencia - Cambio de Idioma.puml` | **T05** | Secuencia del Observer (suscripción, `CambiarIdioma`, `UpdateLanguage`). |
| `CU07 - Secuencia - Verificacion Integridad en Arranque.puml` | **T07** | Nuevo flujo: verificación de integridad **antes del Login**. |

## Correcciones de código aplicadas en esta iteración

1. **T07 — Verificación de integridad ANTES del Login** (`GUI/Program.cs`).
   La verificación de DVH/DVV ahora se ejecuta al arrancar, **antes de mostrar la ventana
   de Login**, cumpliendo la consigna: *"al iniciar la aplicación, y antes de dar acceso a la
   ventana de log-in, se debe realizar el proceso de verificación de integridad"*.
   Si falla, se **registra en la bitácora** (informar al administrador). El **detalle** de las
   filas corruptas y la **reparación** siguen reservados a un Administrador autenticado
   (se enruta después del login, preservando la seguridad).

2. **Manejo de excepciones** (`DAL/Backup.cs`). Se reemplazaron `catch {}` silenciosos por
   registro en `Trace` (fallbacks de directorio temporal / ACL), para no ocultar errores.

## Estado de compilación

`build-and-test.ps1 -SkipTests` → **Build OK** (los 7 proyectos compilan).
Las pruebas unitarias (`Tests/`, 112 métodos) requieren una instancia de SQL Server con
`WardrobeFlowDB` creada (`BD/01_Crear_BaseDeDatos.sql`).
