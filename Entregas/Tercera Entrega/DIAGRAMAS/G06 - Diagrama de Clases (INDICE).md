# G06 — Diagrama de Clases de la Solución WardrobeFlow

Diagramas de clases UML (PlantUML) de **toda** la solución, organizados **por capa de la
arquitectura** y **separando los aspectos de negocio de los técnicos/transversales**.

## Convención de color
- 🟨 **Amarillo (`#FFF7CC`) = NEGOCIO** → GUI (forms), BLL, BE (entidades de dominio).
- 🟦 **Celeste (`#DDF3FF`) = TÉCNICO / transversal** → DAL, Servicios, Seguridad e
  infraestructura de patrones dentro de cada capa.

## Archivos (un diagrama por capa + integración)

| Archivo | Contenido |
|---------|-----------|
| `G06 - 00 - Vista General (Paquetes).puml` | Integración de alto nivel: paquetes por capa y dependencias permitidas. |
| `G06 - 01 - Capa BE (Entidades).puml` | Entidades de negocio + infraestructura de patrones (Composite, Memento) + enums. |
| `G06 - 02 - Capa BLL (Negocio).puml` | Servicios de negocio (contra `I*Service`), soporte (Backup, Historial, Config) y utilitarios. |
| `G06 - 03 - Capa DAL (Datos).puml` | `Acceso` (Singleton), `BaseDAL<T>` (Template Method), contratos `I*DAL` y DAOs. |
| `G06 - 04 - Capa GUI (Presentacion).puml` | Forms agrupados por módulo + infraestructura (FormBase, seguridad por control, Factory de exportación). |
| `G06 - 05 - Capa Servicios (Transversal).puml` | Bitácora, Multiidioma (Observer) y Generador de Credenciales. |
| `G06 - 06 - Capa Seguridad (Transversal).puml` | Sesión (Singleton), criptografía y Dígitos Verificadores (Factory). |
| `G06 - Diagrama de Clases por Capa.puml` | *(existente)* Vista integrada compacta de todas las capas en un solo lienzo. |

## Cómo renderizar
- **VS Code:** extensión *PlantUML* (jebbs) → abrir el `.puml` → `Alt+D` (preview).
- **Web:** pegar el contenido en https://www.plantuml.com/plantuml
- **Export:** con el `plantuml.jar` → `java -jar plantuml.jar "G06 - *.puml"` genera los PNG/SVG.

## Patrones reflejados en los diagramas
| Patrón | Dónde (diagrama) |
|--------|------------------|
| **Singleton** | `SessionManager`, `ContadorSesion` (06) · `Acceso` (03) |
| **Composite** | `Componente → Patente / Familia / Rol` (01) |
| **Memento** | `Usuario (Originator) / VersionUsuario (Memento) / CuidadorHistorial (Caretaker)` (01, 02) |
| **Observer** | `GestorIdioma (Subject) → IIdiomaObserver` (05, 04) |
| **Factory Method** | `GeneradorReporte/Exportador` (04) · `CalculadorDV` (06) |
| **Template Method** | `BaseDAL<T>` (03) |
| **Herencia** | `FormBase → todos los forms` (04) |

---

## ⚠️ Clases legacy / que conviene revisar

Durante el armado detecté estas situaciones (ninguna rompe nada, pero conviene documentarlas
o limpiarlas para la entrega):

1. **`BE.Familia` / `BLL.Familia` — concepto retirado, clase viva.**
   El README (revisión 11/06) dice que *"las Familias se retiraron del modelo"*, pero la clase
   sigue existiendo como **nodo base del Composite** (`Rol : Familia`) y se instancia en
   `DAL.Permiso` como nodo compuesto genérico. Ya **no es un concepto de negocio**, es plumbing
   técnico. Recomendación: **renombrar** `Familia → NodoComposite`/`ComponenteCompuesto` (o al
   menos aclararlo con un comentario), para que el nombre no sugiera una entidad de dominio que
   ya no existe. Rastro relacionado: `TipoPermiso.Ninguno` documentado como *"valor por defecto
   para Familia"* y `Rol.EsFijo`.

2. **Nombre `DigitoVerificador` duplicado en dos capas.**
   Existe `Seguridad.DigitoVerificador` (calcula DVH/DVV) **y** `DAL.DigitoVerificador`
   (persiste/recalcula filas). No es un error —son responsabilidades distintas— pero el nombre
   idéntico confunde al leer. Sugerencia: `DAL.DigitoVerificadorDAL` o `RepositorioDV`.

3. **`BE.Control` (multiidioma) vs `Servicios.Multiidioma`.**
   `BE.Control` mapea la tabla `[Control]` de textos traducibles y convive con
   `Servicios.Multiidioma.Etiqueta` / `Traduccion`. Hay cierta superposición conceptual entre la
   entidad de persistencia y las clases del servicio de idioma. Funciona, pero vale la pena
   revisar si `Etiqueta` sigue aportando o quedó como resto.

> El resto de las clases revisadas (Alertas → `Menu`/`AlertasForm`/`BLL.PanelAlertas`/`BE.Alerta`,
> Integridad → `HistorialIntegridad` en las 4 capas, DTOs como `EstadoComercialCliente`,
> `OcupacionStock`, `CambioPosterior`) **están correctamente cableadas y en uso**.
