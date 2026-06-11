# AUDITORÍA TÉCNICA — WardrobeFlow

**Auditor:** Revisión final universitaria / pre-producción
**Fecha:** 11/06/2026
**Alcance:** BE · BLL · DAL · GUI · Seguridad · Servicios · BD (23 tablas) · Tests (59 casos)
**Veredicto adelantado:** proyecto de calidad netamente superior a una entrega académica promedio. Arquitectura y seguridad de nivel profesional; las debilidades son de *deuda técnica acotada*, no de diseño roto.

---

## 1. Auditoría de Arquitectura

**Separación por capas — CUMPLE (ejemplar).**
Estructura física en proyectos independientes: `BE` (entidades), `DAL` (datos), `BLL` (negocio), `Servicios` (idioma/bitácora), `Seguridad` (cripto/sesión/DV), `GUI` (WinForms). Dependencias reales del `.csproj`:

- `GUI/GUI.csproj:305-320` referencia **solo** BE, BLL, Servicios, Seguridad — **NO referencia DAL**.
- `grep` de `SqlConnection`/`SqlCommand` en GUI → **0 resultados**. No hay acceso a BD desde la interfaz.
- BLL no referencia `System.Windows.Forms` ni GUI → la lógica no conoce la presentación.

| Observación | Sev. | Evidencia | Impacto | Recomendación |
|---|---|---|---|---|
| GUI acopla a `Seguridad.SessionManager` directamente | Baja | `FormBase.cs:55`, `Menu.cs`, `Program.cs:110` | El comentario `Menu.cs:30` afirma *"La GUI nunca accede directamente a Seguridad"* — contradice el código. Acoplamiento leve a un cross-cutting concern. | Exponer la sesión vía una fachada en BLL (`BLL.Sesion.ActualTienePermiso(...)`) y corregir el comentario. |
| Duplicación de la guarda de admin entre capas | Baja | `BLL/Usuario.cs:56 ValidarEsAdministrador`, `BLL/Backup.cs:18 ValidarAdministrador`, `BLL/Familia.cs:47 VerificarPuedeGestionar` | Tres copias casi idénticas del chequeo de rol. | Centralizar en un guard reutilizable (`Seguridad.Autorizacion.ExigirAdmin()`). |

No se detectó lógica de negocio en formularios ni acceso a BD desde GUI. **Sin violaciones reales de arquitectura multicapa.**

**Nota Arquitectura: 9/10**

---

## 2. Programación Orientada a Objetos

**Fortalezas:** encapsulamiento correcto (`Familia._hijos` privado expuesto como copia inmutable `=> _hijos.ToArray()`, `Componente.cs:15`); herencia con propósito (`Rol : Familia : Componente`, `FormBase : Form`); polimorfismo real (`Componente.RecolectarPatentes` resuelto distinto en hoja vs compuesto); abstracción limpia (`Componente` abstracta, `ICalculadorDV`, interfaces DAL).

| Problema | Sev. | Evidencia | Impacto | Recomendación |
|---|---|---|---|---|
| **God file** `Traductor.cs` | Alta | 4197 líneas, **3564 entradas** de diccionario hardcodeadas | Inmantenible; mezcla datos con código. | Es solo *fallback*; reducirlo al mínimo de arranque y delegar el resto 100% a BD (ya existe el camino). |
| Formularios grandes (God Forms) | Media | `DashboardForm.cs` 907 líneas / 50 miembros; `Menu.cs` 813; `Bitacora.cs` 812 | Baja cohesión en presentación, difícil de testear. | Extraer constructores de menú/dashboard a clases auxiliares (`MenuBuilder`). |
| Código duplicado | Media | guardas de admin (§1); `RealizarBackup`/`RealizarBackupInicial` casi idénticos (`Backup.cs:46,90`) | DRY. | Unificar con parámetro de "marcador". |
| Campo muerto | Baja | `Preferencia.Notificaciones` "placeholder sin efecto funcional" (`01_Crear…sql:431`) | Columna sin uso. | Eliminar o implementar. |

No hay God Object en BLL/BE (clases cohesivas). Sí God *Forms* en GUI.

**Calificación POO: 8/10**

---

## 3. Auditoría SOLID

| Principio | Estado | Evidencia / Clases | Riesgo | Refactor |
|---|---|---|---|---|
| **S** — Single Responsibility | Cumple parcial | Bien: `CuidadorHistorial`, `DigitoVerificador`, `Encriptador`. **Viola:** `Traductor` (datos+lógica+fallback), `DashboardForm`. | Mantenibilidad | Separar diccionario de la lógica de traducción. |
| **O** — Open/Closed | Cumple parcial | Bien: `ICalculadorDV`/`CalculadorDV.Crear()` permite cambiar algoritmo DV sin tocar consumidores. **Viola:** agregar un idioma o clave i18n exige editar `Traductor.cs`. | Extensibilidad | i18n íntegro por BD (ya casi logrado). |
| **L** — Liskov | Riesgo conocido | `Patente.AgregarHijo/QuitarHijo` lanzan `InvalidOperationException` (`Patente.cs:22-33`) rompiendo el contrato de `Componente`. | Un cliente que trate `Componente` uniformemente puede romper. | Es el dilema clásico Composite "transparente vs seguro"; está **documentado y testeado** → aceptable, pero idealmente segregar `IContenedor`. |
| **I** — Interface Segregation | Cumple | Interfaces finas y específicas: `IUsuarioDAL`, `IPermisoDAL`, `IClaveRecuperacionDAL`, `IClienteService`, `IPedidoService`, `IIdiomaObserver`. | — | — |
| **D** — Dependency Inversion | Cumple (bueno) | BLL depende de abstracciones DAL e **inyecta** dobles: `Usuario(IUsuarioDAL)`, `Familia(IPermisoDAL)`, `Backup(IBackupDAL)`; fakes `FakePermisoDAL`, `FakeBackupDAL`. | — | Extender DI al resto de DALs (varios siguen `new DAL.X()` interno). |

**SOLID general: 8/10** — DIP e ISP muy bien resueltos; SRP/OCP penalizados por `Traductor`; LSP con la excepción canónica del Composite.

---

## 4. Patrones de Diseño

| Patrón | Implementación | Veredicto |
|---|---|---|
| **Composite** | `Componente`/`Familia`/`Patente`/`Rol`; recursión real en `RecolectarPatentes` y `ObtenerPatentesEfectivas`; es el **motor real de autorización** (`BLL/Familia.cs:95-121`, cargado en `BLL/Usuario.cs:126`). | **Correcto y sustantivo** (no decorativo). Anti-ciclos en `Familia.AgregarHijo:27` + tope de profundidad `depth>50`. |
| **Observer** | `GestorIdioma` (Subject) + `IIdiomaObserver`; **27 formularios** suscriben/desuscriben (`grep` confirma `SuscribirObservador`×2 en cada form). Notificación *thread-safe* con copia bajo lock (`GestorIdioma.cs:73-90`). | **Correcto y completo.** |
| **Singleton** | `Acceso` (double-checked locking, `sealed`, `volatile`, `Acceso.cs:49-60`); `SessionManager`; `ContadorSesion`. | Correcto. Ver nota en SessionManager (excepciones genéricas). |
| **Memento** | `BE.Memento.IMemento`/`IOriginator`, `BE.VersionUsuario` (snapshot), `CuidadorHistorial` (Caretaker que **no lee el estado**, `CuidadorHistorial.cs:31`). | **Implementación de manual.** Soporta rollback (T06). |
| **Repository / DAL** | `BaseDAL<T>` abstracto + interfaces por entidad. | Repository correcto. **Unit of Work no existe como tal**, pero `Acceso.EjecutarTransaccion(Action<conn,tx>)` (`Acceso.cs:125`) cubre atomicidad multi-tabla. |
| **Factory** | `CalculadorDV.Crear()`, `Traductor`-factory de idioma default. | Presente (factory method simple). |
| **MVC/MVP** | No formal; WinForms code-behind con BLL como modelo. | Aceptable para el stack; no se exige MVP. |

**Sin patrones falsos ni incompletos.** El Composite y el Observer —los dos que más se simulan en entregas— acá son funcionales y centrales.

---

## 5. Base de Datos

Esquema sólido: **23 tablas, 24 FK, 23 PK, 6 UNIQUE/CHECK**, integridad referencial consistente, normalización 3FN en general, `ON DELETE CASCADE` correcto en `Preferencia`.

| Problema | Sev. | Evidencia | Impacto | Recomendación |
|---|---|---|---|---|
| **Tabla redundante** `RolPermiso` coexiste con `PermisoRelacion` | Media | `01_Crear…sql:322` y `:336`; doc reconoce a `RolPermiso` como *legacy* | Dos fuentes de verdad de permisos → riesgo de divergencia. | Deprecar `RolPermiso` formalmente o migrar y eliminar. |
| **Ciclos del Composite no impedidos en BD** | Media | `PermisoRelacion` sin CHECK que evite `IdPadre=IdHijo` ni ciclos | Una escritura SQL directa corrompe el árbol; la defensa vive **solo en código** (`BE.Familia`). | `CHECK (IdPadre<>IdHijo)` + trigger/validación de ciclo, o aceptar el riesgo documentándolo. |
| **Sin índices no-clustered** | Media | `grep CREATE INDEX` → 0 | Búsquedas frecuentes (`Bitacora.fecha`, `PedidoPrenda`, `PermisoRelacion.IdHijo`, `Traduccion`) sin cubrir; solo PK/UNIQUE indexan. | Crear índices en columnas FK y de filtrado por fecha. |
| Ambigüedad `Usuario.Rol` **y** `Usuario.Perfil` | Baja | `01_Crear…sql:44-45`, ambos NVARCHAR(100) | Dos columnas con semántica solapada de "rol". | Consolidar en una. |
| `Usuario.DVH` nullable | Baja | `:48` | Una fila con DVH NULL no se detecta como corrupta hasta recalcular. | `NOT NULL` tras backfill. |

**Nota BD: 7.5/10** — correcta y bien restringida, penalizada por redundancia legacy, falta de índices y ciclos no garantizados a nivel motor.

---

## 6. Verificación de Requerimientos

| Req | Estado | Evidencia |
|---|---|---|
| **RF-01** Integridad DVH/DVV | ✅ | `Seguridad/DigitoVerificador.cs` (DVH ponderado + DVV posicional, módulo primo 999.983). |
| **RF-02** Detección de corrupción (tabla/registro/tipo) | ✅ | `FilaDV` con `Descripcion="tabla #id"` (`DigitoVerificador.cs:164`); `HistorialIntegridad` registra tabla, filas corruptas. |
| **RF-03** Asistencia recuperación | ✅ | `RestauracionForm`, `DiagnosticoIntegridadForm`, "Recalcular Todo". |
| **RNF-01** Acceso restringido a admin | ✅ | Guardas `ValidarEsAdministrador` en todas las ops sensibles. |
| **RF-04** Validación integridad de backup | ✅ | `Backup.cs:32 VerificarIntegridadAntesDeRespaldar` cancela backup si la base está corrupta. |
| **RF-05** Wizard de restauración | ✅ | `RestauracionForm` / `RestauracionForm.Designer`. |
| **RF-06** Backup inicial | ✅ | `Backup.cs:90 RealizarBackupInicial` (marcador "Inicial"). |
| **RF-07** Políticas de recuperación | ✅ | Backup cifrado `.wfbak`, retención, claves. |
| **RF-08** Aviso pérdida parcial | ✅ | `Backup.cs ObtenerFechaBackup` informa la fecha **y** `ObtenerCambiosDesde`/`ContarCambiosPosterioresA` desglosan a nivel registro qué se perderá. |
| **RNF-02** Bloqueo de backup corrupto | ✅ | `DAL/Backup.cs:89` usa `RESTORE VERIFYONLY` **antes** de restaurar; lanza `err.dal.backup_corrupto`. |
| **RF-09** Mensajes de login seguros | ✅ | `BLL/Usuario.cs:95-161` — **siempre** "Usuario o contraseña incorrectos"; mismo mensaje/contador para usuario inexistente y clave errada. **No existe** el mensaje prohibido. |
| **RF-10** Protección último admin | ✅ | `Usuario.cs:302 ValidarPuedeArchivar` bloquea archivar el último admin y el auto-baja; testeado puro. |
| **RF-11** Roles jerárquicos vía Composite | ✅ | §4. Rol-dentro-de-rol soportado. |
| **RF-12** Prevención dependencias circulares | ✅ | `Familia.cs:27 ContieneDescendiente`; `BLL/Familia.cs:337 ValidarSinCiclo`. |
| **RF-13** Prevención recursividad infinita | ✅ | `HashSet visitados` + tope `depth>50` / `nivel>50`. |
| **RNF-03/04** Robustez / excepciones | ✅ | `AppException`/`LoginException` tipadas y traducibles; `SessionManager` ahora lanza `BE.SesionException` (subtipo de `AppException`, traducible y capturable), no `Exception` genérica. Claves i18n `err.seg.sesion_no_iniciada`/`err.seg.sesion_ya_iniciada` en los 4 idiomas; `Logout` idempotente. |
| **RF-14..18** Historial / versiones / rollback / auditoría / trazabilidad | ✅ | Memento + `HistorialUsuario`; snapshots antes de cada cambio (`Usuario.cs:225,253,285`). |
| **RF-19..24 / RNF-05** Multidioma | ✅ | Observer (RF-20), cambio dinámico en uso (RF-19), claves de traducción (RF-21), idioma por usuario `Usuario.IdIdioma` (RF-22/23), idioma en Login (RF-24), sesión estática por proceso → independencia entre instancias (RNF-05). |

**Cumplimiento: 9.5/10** — único pendiente real: excepciones genéricas en `SessionManager`.

---

## 7. Casos de Prueba Especiales

| Caso | Resultado | Justificación |
|---|---|---|
| Eliminar el último administrador | **Cumple** | `ValidarPuedeArchivar` + `ContarAdministradoresActivos`; test en `CasosPruebaEspecialesTests`. |
| Restaurar backup corrupto | **Cumple** | `RESTORE VERIFYONLY` previo (`DAL/Backup.cs`). |
| Detectar corrupción DVH/DVV | **Cumple** | `VerificarIntegridadDV` al arranque (`Program.cs:34`). |
| Generar dependencia circular de roles | **Cumple** | Bloqueado en BE y BLL; *pero* no a nivel BD (§5) → **cumple a nivel aplicación**. |
| Múltiples niveles de rollback | **Cumple** | Memento con historial completo por usuario. |
| Cambiar idioma con múltiples usuarios | **Cumple** | Idioma por usuario + sesión por proceso. |
| Cambiar idioma en caliente | **Cumple** | Observer notifica a 27 forms en vivo. |
| Restaurar base con info posterior al backup | **Cumple** | Antes de restaurar se informa la fecha del backup **y el desglose por entidad de los registros que se perderán** (pedidos, clientes, prendas, mantenimientos, bitácoras…), contando las filas vivas posteriores a `BackupFinishDate`. Implementado en `DAL.Backup.ContarCambiosPosterioresA` → `BLL.Backup.ObtenerCambiosDesde` → diálogo de confirmación en `BackupForm` y `RestauracionForm`. |

---

## 8. Seguridad — (el punto más fuerte)

- **Hashing:** PBKDF2-SHA256, salt de 16 bytes por usuario, 100.000 iteraciones, **comparación en tiempo constante** vía XOR (`Encriptador.cs:62-65`). Profesional.
- **Anti-enumeración:** hash *señuelo* precalculado para igualar el costo temporal cuando el usuario no existe (`Encriptador.cs:72`, usado en `Usuario.cs:88`). Excelente.
- **SQL Injection:** consultas 100% parametrizadas; donde el identificador no puede ir parametrizado (DV genérico), **whitelist `[A-Za-z_][A-Za-z0-9_]*` + corchetes y fail-closed** (`DigitoVerificador.cs:128`). No se encontró un solo vector explotable.
- **Cifrado de datos:** AES-CBC con IV aleatorio por operación; clave en `key.dat` **protegida con DPAPI** (CurrentUser) y migración de legacy plano.
- **Control de permisos / elevación:** re-validación en backend (`SessionManager.TienePermiso`, guardas BLL), anti-autobloqueo al editar el propio rol (`Familia.cs:188`).
- **Robustez login:** bloqueo **progresivo** 1/5/15/60 min y luego permanente; claves de emergencia de un solo uso hasheadas.

| Riesgo residual | Sev. | Evidencia | Recomendación |
|---|---|---|---|
| Credenciales generadas exportadas en **.txt plano** en disco (`CredencialesGeneradas/`) | Media | `Usuario.cs:203,232` `ExportarCredenciales` | Aceptable por flujo, pero mostrar en pantalla de un solo uso o cifrar el archivo. |
| ~~Clave temporal **hardcodeada** `"Wardrobe1!"`~~ **(RESUELTO)** | Media | `Usuario.cs` | La clave temporal salió a `App.config` (`appSettings["ClaveTemporalDefault"]`, con fallback). Además, tras un alta o reset la cuenta queda con `RequiereCambioClave=1` y el login **fuerza el cambio** antes de abrir el sistema (`CambioClaveObligatorioForm` → `BLL.Usuario.CambiarClavePropia`). |
| AES-128 (no 256) y PBKDF2 100k | Baja | `Encriptador.cs:16,125` | Subir a AES-256 y ~210k iteraciones (OWASP 2023). |
| Bypass de admin por *string* "Administrador" | Baja | `SessionManager.cs:36` | Si se crea un rol con ese nombre, gana acceso total. Identificar admin por flag/ID, no por texto. |

**Nota Seguridad: 9/10** — supera ampliamente lo esperable en contexto académico.

---

## 9. Calidad de Código

**Fortalezas:** documentación XML extensa y *con sentido* (explica el *por qué*, no el *qué*); nomenclatura consistente en español, clara y de dominio; manejo de errores en capas con tipos propios traducibles (`AppException.Clave` + `Args`, `FormBase.MostrarError(Exception)`); handler **global** de excepciones que las bitacoriza (`Program.cs:105`); `using` correcto en todo recurso desechable.

**Debilidades:** `Traductor.cs` (4197 líneas) lastra cualquier métrica de complejidad/duplicación; forms de 800-900 líneas; algo de duplicación (guardas, backups); ~28k líneas totales con concentración de complejidad en pocos archivos.

**Tests:** 59 pruebas unitarias cubriendo **lo crítico** (Composite, DV, Encriptador, Memento, claves de emergencia, casos especiales, i18n, preview de pérdida de backup, excepciones de sesión, cambio de clave obligatorio) con *fakes* inyectados. Buena disciplina de testing para una entrega académica.

**Nota Calidad: 8/10**

---

## 10. Resultado Final

### Resumen Ejecutivo
WardrobeFlow es un sistema WinForms .NET multicapa **maduro y coherente**. La separación de capas es estricta y verificable, los patrones exigidos (Composite, Observer, Singleton, Memento, Repository) están implementados de forma **sustantiva y funcional** —no decorativa— y la seguridad alcanza un nivel raramente visto en trabajos universitarios (PBKDF2 con comparación en tiempo constante, anti-enumeración por temporización, defensa SQL en profundidad, cifrado DPAPI, bloqueo progresivo). Todos los RF/RNF están cubiertos con evidencia. La deuda técnica es **acotada y de mantenibilidad**, no estructural.

### Fortalezas
1. Arquitectura multicapa sin fugas (GUI no toca DAL ni SQL).
2. Composite como motor real de autorización recursiva + anti-ciclos.
3. Seguridad de grado profesional.
4. Inyección de dependencias + interfaces + tests con fakes.
5. Documentación e i18n completos; Observer cableado en 27 formularios.

### Debilidades
1. `Traductor.cs` — God file de 4197 líneas (SRP/OCP).
2. Forms de presentación demasiado grandes (Dashboard/Menu/Bitácora).
3. BD: `RolPermiso` redundante, sin índices, ciclos no garantizados en motor.
4. Excepciones genéricas en `SessionManager` (RNF-04 parcial).
5. Duplicación de guardas de autorización y clave temporal hardcodeada.

### Riesgos Críticos
- Ninguno de severidad **Crítica**. El de mayor prioridad es la **doble fuente de verdad de permisos** (`RolPermiso` vs `PermisoRelacion`): si ambas se usan, puede haber divergencia de autorización.

### Recomendaciones Prioritarias
1. Vaciar `Traductor.cs` dejándolo como fallback mínimo; i18n 100% BD.
2. Deprecar/eliminar `RolPermiso` y agregar `CHECK`/validación de ciclos en `PermisoRelacion`.
3. Reemplazar `throw new Exception` de `SessionManager` por excepciones tipadas.
4. Centralizar la guarda de admin (DRY) y forzar cambio de la clave temporal.
5. Crear índices no-clustered en FK y columnas de filtrado.

### Calificación General

| Dimensión | Nota |
|---|---|
| Arquitectura | **9.0 / 10** |
| POO | **8.0 / 10** |
| SOLID | **8.0 / 10** |
| Seguridad | **9.0 / 10** |
| Base de Datos | **7.5 / 10** |
| Calidad de Código | **8.0 / 10** |
| Cumplimiento de Requerimientos | **9.5 / 10** |

### Nota Final — evaluación de entrega de Ingeniería de Software

## **9 / 10 — Sobresaliente con observaciones**

**Justificación.** Como profesor evaluando una entrega final, lo que distingue este trabajo no es que "tenga los patrones", sino que estén **bien usados y conectados al dominio**: el Composite *es* el sistema de permisos (no un árbol de adorno), el Observer mueve idioma en caliente sobre toda la GUI, y el Memento sostiene un rollback real. La seguridad demuestra comprensión de amenazas concretas (timing attacks, enumeración de usuarios, inyección en identificadores) y las mitiga correctamente —esto es trabajo de nivel profesional, no de cátedra. El cumplimiento de requerimientos es casi total y **demostrable**.

No llega a 10 por deuda técnica genuina y evitable: un archivo de 4197 líneas que viola SRP/OCP, formularios sobredimensionados, redundancia heredada en la BD (`RolPermiso`), ausencia de índices, ciclos del Composite garantizados solo en la capa de aplicación, y excepciones genéricas que contradicen el propio estándar de manejo de errores del proyecto. Son defectos de **mantenibilidad y prolijidad**, no de diseño: ninguno compromete la corrección funcional ni la seguridad, y todos son corregibles en horas. Resueltos los cinco puntos prioritarios, este proyecto es defendible en un entorno productivo real.
