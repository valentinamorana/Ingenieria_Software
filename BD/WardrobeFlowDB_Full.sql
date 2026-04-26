-- ============================================================
-- WardrobeFlowDB — Script Completo v2.0
-- Base de datos nueva desde cero — alineada con el código C#
--
-- USUARIOS DE PRUEBA:
--   admin      / admin123       → Administrador    (ve TODO el menú)
--   supervisor / supervisor123  → Supervisor       (ve Bitácora)
--   operador   / operador123    → OperadorLogistico (ve Inventario + Pedidos Realizados)
--   vendedor   / vendedor123    → Vendedor         (ve Ventas)
--   stock      / stock123       → ControladorDeStock (ve Prendas + Stock)
--
-- DATOS DE PRUEBA INCLUIDOS:
--   3 Planes de suscripción
--   5 Usuarios + 4 Empleados vinculados
--   8 Clientes con diferentes planes
--   25 Prendas (variedad de estados: Disponible, EnUso, EnLimpieza, Baja)
--   8 Pedidos (Pendiente x2, Despachado x1, Entregado x3, Cancelado x2)
--   Bitácora del sistema con eventos variados (incluye intentos fallidos y bloqueo)
--   Bitácora de negocio con todos los tipos de evento
-- ============================================================

USE master;
GO

-- ============================================================
-- PASO 1: Crear base de datos
-- ============================================================
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'WardrobeFlowDB')
BEGIN
    ALTER DATABASE WardrobeFlowDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE WardrobeFlowDB;
    PRINT '--- BD anterior eliminada ---';
END
GO

CREATE DATABASE WardrobeFlowDB
    COLLATE Latin1_General_CI_AI;
GO

USE WardrobeFlowDB;
GO

PRINT '--- BD creada ---';

-- ============================================================
-- PASO 2: Tablas del sistema
-- ============================================================

-- Empleados del sistema (acceden con usuario/contraseña)
CREATE TABLE Usuario (
    IdUsuario  INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    Username   VARCHAR(50)   NOT NULL,
    Clave      VARCHAR(100)  NOT NULL,  -- PBKDF2-SHA256: Base64(Salt[16] + Hash[32])
    Rol        VARCHAR(100)  NOT NULL,
    Estado     BIT           NOT NULL DEFAULT 1,  -- 1=activo, 0=bloqueado
    Perfil     VARCHAR(100)  NOT NULL,
    CONSTRAINT UQ_Usuario_Username UNIQUE (Username)
);

-- Catálogo de permisos habilitables por rol
CREATE TABLE Permiso (
    IdPermiso      INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    Nombre         VARCHAR(100)  NOT NULL,
    NombreMenu     VARCHAR(100)  NOT NULL,
    TipoComponente VARCHAR(50)   NOT NULL,
    Estado         BIT           NOT NULL DEFAULT 1
);

-- Mapeo Rol → Lista de Permisos
CREATE TABLE RolPermiso (
    Rol       VARCHAR(100) NOT NULL,
    IdPermiso INT          NOT NULL,
    CONSTRAINT PK_RolPermiso PRIMARY KEY (Rol, IdPermiso),
    CONSTRAINT FK_RolPermiso_Permiso FOREIGN KEY (IdPermiso) REFERENCES Permiso(IdPermiso)
);

-- Auditoría de seguridad (login, bloqueos, resets, etc.)
-- usuario=NULL para eventos pre-sesión (intentos fallidos)
CREATE TABLE Bitacora (
    Id         INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    fecha      DATETIME      NOT NULL DEFAULT GETDATE(),
    usuario    INT           NULL,
    modulo     VARCHAR(100)  NOT NULL,
    actividad  VARCHAR(100)  NOT NULL,
    detalle    VARCHAR(500)  NOT NULL,
    criticidad INT           NOT NULL DEFAULT 0,
    CONSTRAINT FK_Bitacora_Usuario FOREIGN KEY (usuario) REFERENCES Usuario(IdUsuario)
);
GO

-- ============================================================
-- PASO 3: Tablas de negocio
-- ============================================================

-- Planes de suscripción (cupo de prendas simultáneas)
CREATE TABLE PlanSuscripcion (
    IdPlan        INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    Nombre        VARCHAR(100)  NOT NULL,
    LimitePrendas INT           NOT NULL,
    Precio        DECIMAL(10,2) NOT NULL DEFAULT 0,
    Estado        BIT           NOT NULL DEFAULT 1
);

-- Clientes del servicio (NO son usuarios del sistema)
CREATE TABLE Cliente (
    IdCliente   INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    Nombre      VARCHAR(100)  NOT NULL,
    Apellido    VARCHAR(100)  NOT NULL,
    DNI         VARCHAR(20)   NOT NULL,
    Email       VARCHAR(150)  NULL,
    MetodoPago  VARCHAR(50)   NOT NULL DEFAULT 'Efectivo',
    IdPlan      INT           NULL,
    FechaAlta   DATETIME      NOT NULL DEFAULT GETDATE(),
    CONSTRAINT UQ_Cliente_DNI  UNIQUE (DNI),
    CONSTRAINT FK_Cliente_Plan FOREIGN KEY (IdPlan) REFERENCES PlanSuscripcion(IdPlan)
);

-- Empleados (datos personales, vinculados a un Usuario)
CREATE TABLE Empleado (
    IdEmpleado    INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    Nombre        VARCHAR(100)  NOT NULL,
    Apellido      VARCHAR(100)  NOT NULL,
    DNI           VARCHAR(20)   NOT NULL,
    Email         VARCHAR(150)  NULL,
    FechaIngreso  DATE          NOT NULL DEFAULT GETDATE(),
    Puesto        VARCHAR(100)  NULL,
    Legajo        VARCHAR(20)   NULL,
    IdUsuario     INT           NULL,
    CONSTRAINT UQ_Empleado_DNI     UNIQUE (DNI),
    CONSTRAINT FK_Empleado_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(IdUsuario)
);

-- Catálogo de prendas
-- Estado: 0=Disponible | 1=EnUso | 2=EnLimpieza | 3=Baja
CREATE TABLE Prenda (
    IdPrenda        INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    Nombre          VARCHAR(150)  NOT NULL,
    Descripcion     VARCHAR(300)  NULL,
    Talle           VARCHAR(20)   NULL,
    Color           VARCHAR(50)   NULL,
    Categoria       VARCHAR(100)  NULL,
    Estado          INT           NOT NULL DEFAULT 0,
    IdClienteActual INT           NULL,
    FechaAlta       DATETIME      NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Prenda_Cliente FOREIGN KEY (IdClienteActual) REFERENCES Cliente(IdCliente)
);

-- Pedidos de alquiler
-- Estado: 0=Pendiente | 1=Despachado | 2=Entregado | 3=Cancelado
CREATE TABLE Pedido (
    IdPedido       INT      NOT NULL IDENTITY(1,1) PRIMARY KEY,
    IdCliente      INT      NOT NULL,
    IdEmpleado     INT      NOT NULL,
    Estado         INT      NOT NULL DEFAULT 0,
    FechaPedido    DATETIME NOT NULL DEFAULT GETDATE(),
    FechaDespacho  DATETIME NULL,
    FechaEntrega   DATETIME NULL,
    CONSTRAINT FK_Pedido_Cliente  FOREIGN KEY (IdCliente)  REFERENCES Cliente(IdCliente),
    CONSTRAINT FK_Pedido_Empleado FOREIGN KEY (IdEmpleado) REFERENCES Empleado(IdEmpleado)
);

-- Tabla pivote: prendas incluidas en cada pedido
CREATE TABLE PedidoPrenda (
    IdPedido INT NOT NULL,
    IdPrenda INT NOT NULL,
    CONSTRAINT PK_PedidoPrenda PRIMARY KEY (IdPedido, IdPrenda),
    CONSTRAINT FK_PP_Pedido FOREIGN KEY (IdPedido) REFERENCES Pedido(IdPedido),
    CONSTRAINT FK_PP_Prenda FOREIGN KEY (IdPrenda) REFERENCES Prenda(IdPrenda)
);

-- Auditoría de eventos de negocio (ventas, despachos, stock, clientes)
CREATE TABLE BitacoraNegocio (
    IdEvento    INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    Fecha       DATETIME      NOT NULL DEFAULT GETDATE(),
    Tipo        VARCHAR(50)   NOT NULL,
    IdUsuario   INT           NULL,
    IdPedido    INT           NULL,
    IdPrenda    INT           NULL,
    IdCliente   INT           NULL,
    Descripcion VARCHAR(500)  NOT NULL,
    CONSTRAINT FK_BitNeg_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(IdUsuario),
    CONSTRAINT FK_BitNeg_Pedido  FOREIGN KEY (IdPedido)  REFERENCES Pedido(IdPedido),
    CONSTRAINT FK_BitNeg_Prenda  FOREIGN KEY (IdPrenda)  REFERENCES Prenda(IdPrenda),
    CONSTRAINT FK_BitNeg_Cliente FOREIGN KEY (IdCliente) REFERENCES Cliente(IdCliente)
);
GO

PRINT '--- Tablas creadas ---';

-- ============================================================
-- PASO 4: Permisos del sistema (10 permisos, IDs fijos)
-- ============================================================

SET IDENTITY_INSERT Permiso ON;
INSERT INTO Permiso (IdPermiso, Nombre, NombreMenu, TipoComponente, Estado) VALUES
( 1, 'Gestionar Usuarios',         'mnuUsuarios',          'Sistema',    1),
( 2, 'Ver Auditoria',              'mnuAuditoria',         'Sistema',    1),
( 3, 'Ver Prendas',                'mnuPrendas',           'Inventario', 1),
( 4, 'Ver Outfits',                'mnuOutfits',           'Inventario', 1),
( 5, 'Ver Categorias',             'mnuCategorias',        'Inventario', 1),
( 6, 'Gestionar Stock',            'mnuStock',             'Inventario', 1),
( 7, 'Gestionar Clientes',         'mnuClientes',          'Ventas',     1),
( 8, 'Gestionar PlanSuscripciones','mnuPlanSuscripciones', 'Ventas',     1),
( 9, 'Realizar Ventas',            'mnuPedidosVenta',      'Ventas',     1),
(10, 'Ver Pedidos Realizados',     'mnuPedidosRealizados', 'Ventas',     1);
SET IDENTITY_INSERT Permiso OFF;
GO

-- ============================================================
-- PASO 5: RolPermiso — asignación de permisos por rol
-- Documento G04: Administrador | OperadorLogistico | Supervisor
-- ============================================================

-- ── Administrador → TODOS LOS PERMISOS ───────────────────────
-- G04: "usuarios y permisos, clientes, suscripciones, planes,
--       reportes, backup, bitácora completa."
INSERT INTO RolPermiso (Rol, IdPermiso)
SELECT 'Administrador', IdPermiso FROM Permiso WHERE Estado = 1;
GO

-- ── Supervisor → Auditoría ────────────────────────────────────
-- G04: "reportes, métricas operativas, bitácora de auditoría."
INSERT INTO RolPermiso (Rol, IdPermiso) VALUES
('Supervisor', 2);
GO

-- ── OperadorLogistico → Catálogo + Stock + Logística ─────────
-- G04: "preparación de pedidos, despachos, devoluciones,
--       inspección de prendas, incidencias."
INSERT INTO RolPermiso (Rol, IdPermiso) VALUES
('OperadorLogistico',  3),   -- Ver Prendas
('OperadorLogistico',  4),   -- Ver Outfits
('OperadorLogistico',  5),   -- Ver Categorias
('OperadorLogistico',  6),   -- Gestionar Stock
('OperadorLogistico', 10);   -- Ver Pedidos Realizados (despachos / devoluciones)
GO

-- ── Vendedor → Gestión comercial ─────────────────────────────
INSERT INTO RolPermiso (Rol, IdPermiso) VALUES
('Vendedor', 7),   -- Gestionar Clientes
('Vendedor', 8),   -- Gestionar PlanSuscripciones
('Vendedor', 9);   -- Realizar Ventas
GO

-- ── ControladorDeStock → Inventario físico ───────────────────
INSERT INTO RolPermiso (Rol, IdPermiso) VALUES
('ControladorDeStock', 3),   -- Ver Prendas
('ControladorDeStock', 6);   -- Gestionar Stock
GO

-- ── OperadorDeInventario → Logística de pedidos ──────────────
INSERT INTO RolPermiso (Rol, IdPermiso) VALUES
('OperadorDeInventario', 10);
GO

-- ============================================================
-- PASO 6: Planes de suscripción
-- ============================================================

SET IDENTITY_INSERT PlanSuscripcion ON;
INSERT INTO PlanSuscripcion (IdPlan, Nombre, LimitePrendas, Precio, Estado) VALUES
(1, 'Basico',   3,  2999.00, 1),
(2, 'Estandar', 6,  4999.00, 1),
(3, 'Premium', 10,  7999.00, 1);
SET IDENTITY_INSERT PlanSuscripcion OFF;
GO

-- ============================================================
-- PASO 7: Usuarios de prueba
-- Contraseñas: usuario + "123"  (hashes PBKDF2-SHA256 frescos)
-- ============================================================

SET IDENTITY_INSERT Usuario ON;
INSERT INTO Usuario (IdUsuario, Username, Clave, Rol, Estado, Perfil) VALUES

-- admin / admin123
(1, 'admin',
 'w/cDCsPvFKD0KDivEAZ65Z6Z0XCKoU0uFDBAcbffyxhllkV2AjZPdM+1dRvPinDZ',
 'Administrador', 1, 'Administrador'),

-- supervisor / supervisor123
(2, 'supervisor',
 'XjZ8ws7um3BTYdBSgF1R7EdXsZ9J2lTg1OvsJXQoW2XoS5XYJnjVMDa7RAZ2j96E',
 'Supervisor', 1, 'Supervisor'),

-- operador / operador123  (rol del documento G04)
(3, 'operador',
 'VJHzSAVZTPeoRecyhy93KKXtME21XKHbBLq0k23rpMi6hYzdV+et/UCqIpGjKtA+',
 'OperadorLogistico', 1, 'Operador Logistico'),

-- vendedor / vendedor123
(4, 'vendedor',
 '7rQnzqcrOsUA7xrCMD/yBb/s+oZOAEqHsSMdE22gyvSF1DZqDDHfCK5N0HdgEBHy',
 'Vendedor', 1, 'Vendedor'),

-- stock / stock123
(5, 'stock',
 'QI1rQlGT4tZQiaWeFDs3um8D0N9sDj6w4zw+2HaXW0L770ECDEgxjoj7BJq0fLj0',
 'ControladorDeStock', 1, 'Controlador de Stock');

SET IDENTITY_INSERT Usuario OFF;
GO

-- ============================================================
-- PASO 8: Empleados (vinculados a los usuarios)
-- IdEmpleado=1 → vendedor  (crea pedidos)
-- IdEmpleado=2 → operador  (despacha / entrega)
-- IdEmpleado=3 → stock     (gestiona prendas)
-- IdEmpleado=4 → supervisor
-- ============================================================

SET IDENTITY_INSERT Empleado ON;
INSERT INTO Empleado (IdEmpleado, Nombre, Apellido, DNI, Email, FechaIngreso, Puesto, Legajo, IdUsuario) VALUES
(1, 'Maria',    'Garcia',    '30000001', 'vendedor@wf.com',   DATEADD(YEAR,-3,GETDATE()), 'Vendedor',              'LEG-001', 4),
(2, 'Carlos',   'Lopez',     '30000002', 'operador@wf.com',   DATEADD(YEAR,-2,GETDATE()), 'Operador Logistico',    'LEG-002', 3),
(3, 'Roberto',  'Diaz',      '30000003', 'stock@wf.com',      DATEADD(YEAR,-4,GETDATE()), 'Controlador de Stock',  'LEG-003', 5),
(4, 'Laura',    'Sanchez',   '30000004', 'supervisor@wf.com', DATEADD(YEAR,-5,GETDATE()), 'Supervisora',           'LEG-004', 2);
SET IDENTITY_INSERT Empleado OFF;
GO

-- ============================================================
-- PASO 9: Clientes (8 clientes con planes variados)
--
-- Stock utilizado por cliente:
--   1 Sofia Ramirez    → Plan Basico   (max 3) — 2 prendas EnUso  (pedido entregado)
--   2 Valentina Torres → Plan Estandar (max 6) — 3 prendas EnUso  (pedido entregado)
--   3 Agustina Perez   → Plan Premium  (max 10)— 0 prendas EnUso  (devolvio, en limpieza)
--   4 Camila Rodriguez → Plan Basico   (max 3) — 1 prenda  EnUso  (pedido despachado)
--   5 Julieta Medina   → Plan Estandar (max 6) — 2 prendas EnUso  (pedido pendiente)
--   6 Lucia Fernandez  → Plan Premium  (max 10)— 4 prendas EnUso  (pedido entregado)
--   7 Marina Gomez     → Plan Basico   (max 3) — 2 prendas EnUso  (pedido pendiente)
--   8 Daniela Ruiz     → Plan Estandar (max 6) — 0 prendas         (pedido cancelado)
-- ============================================================

SET IDENTITY_INSERT Cliente ON;
INSERT INTO Cliente (IdCliente, Nombre, Apellido, DNI, Email, MetodoPago, IdPlan, FechaAlta) VALUES
(1, 'Sofia',     'Ramirez',   '11111111', 'sofia@mail.com',    'Tarjeta Debito',  1, DATEADD(DAY,-60,GETDATE())),
(2, 'Valentina', 'Torres',    '22222222', 'valen@mail.com',    'Tarjeta Credito', 2, DATEADD(DAY,-55,GETDATE())),
(3, 'Agustina',  'Perez',     '33333333', 'agus@mail.com',     'Transferencia',   3, DATEADD(DAY,-50,GETDATE())),
(4, 'Camila',    'Rodriguez', '44444444', 'cami@mail.com',     'Efectivo',        1, DATEADD(DAY,-40,GETDATE())),
(5, 'Julieta',   'Medina',    '55555555', 'juli@mail.com',     'Tarjeta Debito',  2, DATEADD(DAY,-30,GETDATE())),
(6, 'Lucia',     'Fernandez', '66666666', 'lucia@mail.com',    'Tarjeta Credito', 3, DATEADD(DAY,-45,GETDATE())),
(7, 'Marina',    'Gomez',     '77777777', 'marina@mail.com',   'Efectivo',        1, DATEADD(DAY,-20,GETDATE())),
(8, 'Daniela',   'Ruiz',      '88888888', 'dani@mail.com',     'Transferencia',   2, DATEADD(DAY,-15,GETDATE()));
SET IDENTITY_INSERT Cliente OFF;
GO

-- ============================================================
-- PASO 10: Prendas (25 prendas — variedad de estados)
--
-- Estado:  0=Disponible | 1=EnUso | 2=EnLimpieza | 3=Baja
--
-- Disponibles (libres para nuevos pedidos): 1,2,17,18,19,20,21,22,23,24
-- EnUso con cliente asignado:
--   Cliente 1 (Sofia)     → 3, 4
--   Cliente 2 (Valentina) → 5, 6, 7
--   Cliente 4 (Camila)    → 8
--   Cliente 5 (Julieta)   → 9, 10
--   Cliente 6 (Lucia)     → 11, 12, 13, 14
--   Cliente 7 (Marina)    → 15, 16
-- EnLimpieza (devueltas por Agustina, en proceso): 25, 26 → ids 25, 26 not used
-- EnLimpieza: 20, 21 from canceled order → no, those are Disponible
-- Actually EnLimpieza: separate prendas 20 and 21 not needed...
-- Let me use prendas 20 and 21 for EnLimpieza (returned by Agustina after old order)
-- Baja: prenda 25
-- ============================================================

SET IDENTITY_INSERT Prenda ON;
INSERT INTO Prenda (IdPrenda, Nombre, Descripcion, Talle, Color, Categoria, Estado, IdClienteActual, FechaAlta) VALUES
-- ─── Disponibles ──────────────────────────────────────────────────────────────
( 1, 'Vestido floral verano',      'Vestido midi con estampado floral, ideal eventos diurnos', 'M',  'Rosa',        'Vestidos',   0, NULL, DATEADD(DAY,-90,GETDATE())),
( 2, 'Blazer clasico negro',       'Blazer entallado, forro interior satinado, botones dorados','L',  'Negro',       'Blazers',    0, NULL, DATEADD(DAY,-85,GETDATE())),
-- ─── EnUso — Sofia Ramirez (Cliente 1) ────────────────────────────────────────
( 3, 'Vestido coctel azul marino', 'Vestido sin mangas con escote en V, ideal para coctel',    'S',  'Azul marino', 'Vestidos',   1,    1, DATEADD(DAY,-80,GETDATE())),
( 4, 'Top con pedreria blanco',    'Top de seda con detalles de pedreria en el escote',         'XS', 'Blanco',      'Tops',       1,    1, DATEADD(DAY,-80,GETDATE())),
-- ─── EnUso — Valentina Torres (Cliente 2) ─────────────────────────────────────
( 5, 'Pantalon palazzo beige',     'Pantalon ancho de tiro alto, corte fluido y elegante',     'M',  'Beige',       'Pantalones', 1,    2, DATEADD(DAY,-75,GETDATE())),
( 6, 'Vestido rojo fiesta',        'Vestido largo de gasa con abertura lateral y escote',      'M',  'Rojo',        'Vestidos',   1,    2, DATEADD(DAY,-75,GETDATE())),
( 7, 'Blazer gris satinado',       'Blazer oversized con botones dorados y corte italiano',    'M',  'Gris perla',  'Blazers',    1,    2, DATEADD(DAY,-75,GETDATE())),
-- ─── EnUso — Camila Rodriguez (Cliente 4) ─────────────────────────────────────
( 8, 'Vestido verde esmeralda',    'Vestido ajustado manga larga con cuello alto y cierre',    'S',  'Verde',       'Vestidos',   1,    4, DATEADD(DAY,-60,GETDATE())),
-- ─── EnUso — Julieta Medina (Cliente 5) ───────────────────────────────────────
( 9, 'Camisa satinada dorada',     'Camisa de seda satinada con botones de nacar dorados',     'S',  'Dorado',      'Tops',       1,    5, DATEADD(DAY,-50,GETDATE())),
(10, 'Jumpsuit negro elegante',    'Enterito de manga corta con cinturon incluido y bolsillos', 'M',  'Negro',       'Conjuntos',  1,    5, DATEADD(DAY,-50,GETDATE())),
-- ─── EnUso — Lucia Fernandez (Cliente 6) ──────────────────────────────────────
(11, 'Vestido lentejuelas plata',  'Vestido corto cubierto de lentejuelas plateadas',          'M',  'Plateado',    'Vestidos',   1,    6, DATEADD(DAY,-70,GETDATE())),
(12, 'Abrigo camel estructurado',  'Abrigo largo con solapas anchas y botones forrados',       'L',  'Camel',       'Abrigos',    1,    6, DATEADD(DAY,-70,GETDATE())),
(13, 'Falda plisada midi',         'Falda midi plisada con cintura elastica y cierre lateral', 'M',  'Blanco',      'Faldas',     1,    6, DATEADD(DAY,-70,GETDATE())),
(14, 'Blazer oversized coral',     'Blazer de lana con hombros estructurados y manga larga',   'S',  'Coral',       'Blazers',    1,    6, DATEADD(DAY,-70,GETDATE())),
-- ─── EnUso — Marina Gomez (Cliente 7) ─────────────────────────────────────────
(15, 'Vestido lino verano',        'Vestido camisero de lino, fresco, cinturon incluido',      'L',  'Blanco roto', 'Vestidos',   1,    7, DATEADD(DAY,-18,GETDATE())),
(16, 'Conjunto lino beige',        'Blazer + pantalon de lino, set completo con bolsillos',    'M',  'Beige',       'Conjuntos',  1,    7, DATEADD(DAY,-18,GETDATE())),
-- ─── Disponibles ──────────────────────────────────────────────────────────────
(17, 'Pantalon cuero sintetico',   'Pantalon ajustado efecto cuero con cremallera lateral',    'S',  'Negro',       'Pantalones', 0, NULL, DATEADD(DAY,-45,GETDATE())),
(18, 'Blusa romantica manga globo','Blusa de gasa con manga globo y lazo al cuello delicado',  'M',  'Rosa palo',   'Tops',       0, NULL, DATEADD(DAY,-40,GETDATE())),
(19, 'Falda cuero midi',           'Falda midi efecto cuero con abertura trasera discreta',    'M',  'Camel',       'Faldas',     0, NULL, DATEADD(DAY,-35,GETDATE())),
-- ─── Disponibles — devueltas del pedido cancelado de Daniela ──────────────────
(22, 'Camisa seda marfil',         'Camisa de seda natural con cuello italiano y puños',       'M',  'Marfil',      'Tops',       0, NULL, DATEADD(DAY,-30,GETDATE())),
(23, 'Blazer tweed clasico',       'Blazer de tweed azul petroleo, botonadura simple',         'L',  'Azul petroleo','Blazers',   0, NULL, DATEADD(DAY,-30,GETDATE())),
(24, 'Vestido animal print',       'Vestido corto con estampado animal print y cinturon',      'M',  'Animal print', 'Vestidos',  0, NULL, DATEADD(DAY,-20,GETDATE())),
-- ─── EnLimpieza — devueltas por Agustina Perez (pedido ya entregado) ───────────
(20, 'Top bordado artesanal',      'Top sin mangas con bordados a mano en el pecho, azul',     'S',  'Azul cielo',  'Tops',       2, NULL, DATEADD(DAY,-55,GETDATE())),
(21, 'Vestido midi floreado',      'Vestido de lino con estampado de flores pequenas',         'L',  'Verde oliva', 'Vestidos',   2, NULL, DATEADD(DAY,-55,GETDATE())),
-- ─── Baja (deteriorada, fuera de servicio) ────────────────────────────────────
(25, 'Crop top lycra rosa',        'Crop top de lycra deteriorado — dado de baja definitiva',  'XS', 'Rosa coral',  'Tops',       3, NULL, DATEADD(DAY,-120,GETDATE()));
SET IDENTITY_INSERT Prenda OFF;
GO

-- ============================================================
-- PASO 11: Pedidos (8 pedidos con todos los estados posibles)
-- IdEmpleado=1 (Maria Garcia — Vendedor crea el pedido)
--
--  1 Sofia Ramirez     → ENTREGADO  (2 prendas: 3, 4)
--  2 Valentina Torres  → ENTREGADO  (3 prendas: 5, 6, 7)
--  3 Agustina Perez    → ENTREGADO  (2 prendas: 20, 21 — ya en limpieza)
--  4 Camila Rodriguez  → DESPACHADO (1 prenda: 8)
--  5 Julieta Medina    → PENDIENTE  (2 prendas: 9, 10)
--  6 Lucia Fernandez   → ENTREGADO  (4 prendas: 11, 12, 13, 14)
--  7 Daniela Ruiz      → CANCELADO  (2 prendas: 22, 23 — liberadas)
--  8 Marina Gomez      → PENDIENTE  (2 prendas: 15, 16)
-- ============================================================

SET IDENTITY_INSERT Pedido ON;
INSERT INTO Pedido (IdPedido, IdCliente, IdEmpleado, Estado, FechaPedido, FechaDespacho, FechaEntrega) VALUES
-- Pedido 1: Sofia Ramirez — ENTREGADO
(1, 1, 1, 2,
 DATEADD(DAY,-25,GETDATE()),
 DATEADD(DAY,-23,GETDATE()),
 DATEADD(DAY,-20,GETDATE())),

-- Pedido 2: Valentina Torres — ENTREGADO
(2, 2, 1, 2,
 DATEADD(DAY,-20,GETDATE()),
 DATEADD(DAY,-18,GETDATE()),
 DATEADD(DAY,-15,GETDATE())),

-- Pedido 3: Agustina Perez — ENTREGADO (prendas 20 y 21 ya fueron devueltas y puestas en limpieza)
(3, 3, 1, 2,
 DATEADD(DAY,-45,GETDATE()),
 DATEADD(DAY,-43,GETDATE()),
 DATEADD(DAY,-40,GETDATE())),

-- Pedido 4: Camila Rodriguez — DESPACHADO (en camino)
(4, 4, 1, 1,
 DATEADD(DAY, -5,GETDATE()),
 DATEADD(DAY, -3,GETDATE()),
 NULL),

-- Pedido 5: Julieta Medina — PENDIENTE (esperando despacho)
(5, 5, 1, 0,
 DATEADD(DAY, -2,GETDATE()),
 NULL, NULL),

-- Pedido 6: Lucia Fernandez — ENTREGADO
(6, 6, 1, 2,
 DATEADD(DAY,-35,GETDATE()),
 DATEADD(DAY,-33,GETDATE()),
 DATEADD(DAY,-30,GETDATE())),

-- Pedido 7: Daniela Ruiz — CANCELADO (prendas 22 y 23 liberadas → Disponible)
(7, 8, 1, 3,
 DATEADD(DAY,-10,GETDATE()),
 NULL, NULL),

-- Pedido 8: Marina Gomez — PENDIENTE (esperando despacho)
(8, 7, 1, 0,
 DATEADD(DAY, -1,GETDATE()),
 NULL, NULL);
SET IDENTITY_INSERT Pedido OFF;
GO

-- ============================================================
-- PASO 12: PedidoPrenda (prendas incluidas en cada pedido)
-- ============================================================

INSERT INTO PedidoPrenda (IdPedido, IdPrenda) VALUES
-- Pedido 1 — Sofia (Entregado)
(1,  3), (1,  4),
-- Pedido 2 — Valentina (Entregado)
(2,  5), (2,  6), (2,  7),
-- Pedido 3 — Agustina (Entregado — prendas ahora en limpieza)
(3, 20), (3, 21),
-- Pedido 4 — Camila (Despachado)
(4,  8),
-- Pedido 5 — Julieta (Pendiente)
(5,  9), (5, 10),
-- Pedido 6 — Lucia (Entregado)
(6, 11), (6, 12), (6, 13), (6, 14),
-- Pedido 7 — Daniela (Cancelado — prendas liberadas)
(7, 22), (7, 23),
-- Pedido 8 — Marina (Pendiente)
(8, 15), (8, 16);
GO

-- ============================================================
-- PASO 13: Bitácora del Sistema
-- criticidad: 0=None | 1=Baja | 2=Media | 3=Alta
--             4=IntentosLogin | 5=RecuperacionClave | 6=BloqueosCuenta
-- usuario=NULL → evento pre-sesión (intentos fallidos)
-- ============================================================

INSERT INTO Bitacora (fecha, usuario, modulo, actividad, detalle, criticidad) VALUES

-- Logins exitosos (criticidad None)
(DATEADD(DAY,-60,GETDATE()), 4, 'Login', 'Inicio Sesion',
 'Login exitoso. Usuario: vendedor a las 09:15:00.', 0),
(DATEADD(DAY,-55,GETDATE()), 4, 'Login', 'Inicio Sesion',
 'Login exitoso. Usuario: vendedor a las 09:02:33.', 0),
(DATEADD(DAY,-50,GETDATE()), 5, 'Login', 'Inicio Sesion',
 'Login exitoso. Usuario: stock a las 08:50:11.', 0),
(DATEADD(DAY,-45,GETDATE()), 3, 'Login', 'Inicio Sesion',
 'Login exitoso. Usuario: operador a las 10:30:00.', 0),
(DATEADD(DAY,-40,GETDATE()), 4, 'Login', 'Inicio Sesion',
 'Login exitoso. Usuario: vendedor a las 09:10:45.', 0),
(DATEADD(DAY,-30,GETDATE()), 2, 'Login', 'Inicio Sesion',
 'Login exitoso. Usuario: supervisor a las 14:00:00.', 0),
(DATEADD(DAY,-15,GETDATE()), 1, 'Login', 'Inicio Sesion',
 'Login exitoso. Usuario: admin a las 08:00:00.', 0),

-- Altas de usuarios por Admin (criticidad Media)
(DATEADD(DAY,-60,GETDATE()), 1, 'Gestion de Usuarios', 'Alta Usuario',
 'Alta Usuario: ''vendedor'' [Vendedor]', 2),
(DATEADD(DAY,-60,GETDATE()), 1, 'Gestion de Usuarios', 'Alta Usuario',
 'Alta Usuario: ''stock'' [ControladorDeStock]', 2),
(DATEADD(DAY,-59,GETDATE()), 1, 'Gestion de Usuarios', 'Alta Usuario',
 'Alta Usuario: ''operador'' [OperadorLogistico]', 2),
(DATEADD(DAY,-59,GETDATE()), 1, 'Gestion de Usuarios', 'Alta Usuario',
 'Alta Usuario: ''supervisor'' [Supervisor]', 2),

-- Altas de clientes (criticidad Baja)
(DATEADD(DAY,-60,GETDATE()), 4, 'Clientes', 'Alta Cliente',
 'Alta Cliente: Sofia Ramirez (DNI 11111111)', 1),
(DATEADD(DAY,-55,GETDATE()), 4, 'Clientes', 'Alta Cliente',
 'Alta Cliente: Valentina Torres (DNI 22222222)', 1),
(DATEADD(DAY,-50,GETDATE()), 4, 'Clientes', 'Alta Cliente',
 'Alta Cliente: Agustina Perez (DNI 33333333)', 1),
(DATEADD(DAY,-40,GETDATE()), 4, 'Clientes', 'Alta Cliente',
 'Alta Cliente: Camila Rodriguez (DNI 44444444)', 1),

-- Intentos fallidos de login (criticidad IntentosLogin)
(DATEADD(DAY,-20,GETDATE()), NULL, 'Login', 'Intento Fallido Login',
 'Intento fallido #1/3 para ''vendedor'' a las 11:23:10.', 4),
(DATEADD(DAY,-20,GETDATE()), NULL, 'Login', 'Intento Fallido Login',
 'Intento fallido #2/3 para ''vendedor'' a las 11:23:45.', 4),
(DATEADD(DAY,-20,GETDATE()), NULL, 'Login', 'Intento Fallido Login',
 'Intento fallido #3/3 para ''vendedor'' a las 11:24:02.', 4),

-- Bloqueo de cuenta (criticidad BloqueosCuenta)
(DATEADD(DAY,-20,GETDATE()), NULL, 'Login', 'Bloqueo de Cuenta',
 'Cuenta ''vendedor'' bloqueada automaticamente tras 3 intentos fallidos a las 11:24:02.', 6),

-- Desbloqueo por Admin (criticidad Alta)
(DATEADD(DAY,-20,GETDATE()), 1, 'Gestion de Usuarios', 'Desbloqueo de Cuenta',
 'Desbloqueo de Cuenta: ''vendedor'' — solicitado por Administrador.', 3),

-- Reset de contraseña por Admin (criticidad RecuperacionClave)
(DATEADD(DAY,-10,GETDATE()), 1, 'Gestion de Usuarios', 'Reset Contrasena',
 'Contrasena reseteada para usuario ID 4 (vendedor).', 5),

-- Modificaciones de datos (criticidad Media)
(DATEADD(DAY,-35,GETDATE()), 4, 'Clientes', 'Modificar Cliente',
 'Modificar Cliente ID 3: Agustina Perez', 2),
(DATEADD(DAY,-25,GETDATE()), 5, 'Prendas', 'Modificar Prenda',
 'Modificar Prenda ID 1: Vestido floral verano', 2),

-- Cambios de estado de prendas (criticidad Media)
(DATEADD(DAY,-15,GETDATE()), 3, 'Prendas', 'Estado Prenda',
 'Estado Prenda ID 20 ''Top bordado artesanal'': EnUso → EnLimpieza', 2),
(DATEADD(DAY,-15,GETDATE()), 3, 'Prendas', 'Estado Prenda',
 'Estado Prenda ID 21 ''Vestido midi floreado'': EnUso → EnLimpieza', 2),

-- Baja de prenda (criticidad Alta)
(DATEADD(DAY,-30,GETDATE()), 5, 'Prendas', 'Estado Prenda',
 'Estado Prenda ID 25 ''Crop top lycra rosa'': Disponible → Baja', 3),

-- Cierres de sesión
(DATEADD(DAY,-60,GETDATE()), 4, 'Login', 'Cierre Sesion',
 'Cierre de sesion. Usuario: vendedor a las 18:30:00.', 0),
(DATEADD(DAY,-15,GETDATE()), 1, 'Login', 'Cierre Sesion',
 'Cierre de sesion. Usuario: admin a las 17:00:00.', 0),
(DATEADD(DAY,-30,GETDATE()), 2, 'Login', 'Cierre Sesion',
 'Cierre de sesion. Usuario: supervisor a las 16:45:00.', 0),

-- Login actual (hoy)
(GETDATE(), 1, 'Login', 'Inicio Sesion',
 'Login exitoso. Usuario: admin a las 08:00:00.', 0);
GO

-- ============================================================
-- PASO 14: BitácoraNegocio (eventos de negocio)
-- Tipo: AltaCliente | ModificacionCliente | BajaCliente
--       AltaPrenda  | ModificacionPrenda  | CambioEstadoPrenda
--       Venta | Despacho | Entrega | Cancelacion
-- ============================================================

INSERT INTO BitacoraNegocio (Fecha, Tipo, IdUsuario, IdPedido, IdPrenda, IdCliente, Descripcion) VALUES

-- ─── Altas de prendas (IdUsuario=5 stock) ─────────────────────────────────────
(DATEADD(DAY,-90,GETDATE()), 'AltaPrenda', 5, NULL,  1, NULL,
 'Nueva prenda: Vestido floral verano — Talle M — Rosa — Vestidos'),
(DATEADD(DAY,-85,GETDATE()), 'AltaPrenda', 5, NULL,  2, NULL,
 'Nueva prenda: Blazer clasico negro — Talle L — Negro — Blazers'),
(DATEADD(DAY,-80,GETDATE()), 'AltaPrenda', 5, NULL,  3, NULL,
 'Nueva prenda: Vestido coctel azul marino — Talle S — Azul marino — Vestidos'),
(DATEADD(DAY,-80,GETDATE()), 'AltaPrenda', 5, NULL,  4, NULL,
 'Nueva prenda: Top con pedreria blanco — Talle XS — Blanco — Tops'),
(DATEADD(DAY,-75,GETDATE()), 'AltaPrenda', 5, NULL,  5, NULL,
 'Nueva prenda: Pantalon palazzo beige — Talle M — Beige — Pantalones'),
(DATEADD(DAY,-75,GETDATE()), 'AltaPrenda', 5, NULL,  6, NULL,
 'Nueva prenda: Vestido rojo fiesta — Talle M — Rojo — Vestidos'),
(DATEADD(DAY,-75,GETDATE()), 'AltaPrenda', 5, NULL,  7, NULL,
 'Nueva prenda: Blazer gris satinado — Talle M — Gris perla — Blazers'),
(DATEADD(DAY,-70,GETDATE()), 'AltaPrenda', 5, NULL, 11, NULL,
 'Nueva prenda: Vestido lentejuelas plata — Talle M — Plateado — Vestidos'),
(DATEADD(DAY,-70,GETDATE()), 'AltaPrenda', 5, NULL, 12, NULL,
 'Nueva prenda: Abrigo camel estructurado — Talle L — Camel — Abrigos'),
(DATEADD(DAY,-45,GETDATE()), 'AltaPrenda', 5, NULL, 17, NULL,
 'Nueva prenda: Pantalon cuero sintetico — Talle S — Negro — Pantalones'),
(DATEADD(DAY,-35,GETDATE()), 'AltaPrenda', 5, NULL, 19, NULL,
 'Nueva prenda: Falda cuero midi — Talle M — Camel — Faldas'),

-- ─── Baja de prenda deteriorada ───────────────────────────────────────────────
(DATEADD(DAY,-30,GETDATE()), 'CambioEstadoPrenda', 5, NULL, 25, NULL,
 'Prenda ''Crop top lycra rosa'' (ID 25): Disponible → Baja. Motivo: deterioro por uso'),

-- ─── Modificación de prenda ───────────────────────────────────────────────────
(DATEADD(DAY,-25,GETDATE()), 'ModificacionPrenda', 5, NULL,  1, NULL,
 'Modificacion prenda: ''Vestido floral verano'' (ID 1) — Talle M, Rosa'),

-- ─── Altas de clientes (IdUsuario=4 vendedor) ─────────────────────────────────
(DATEADD(DAY,-60,GETDATE()), 'AltaCliente', 4, NULL, NULL, 1,
 'Nuevo cliente: Sofia Ramirez — DNI 11111111 — Plan: Basico'),
(DATEADD(DAY,-55,GETDATE()), 'AltaCliente', 4, NULL, NULL, 2,
 'Nuevo cliente: Valentina Torres — DNI 22222222 — Plan: Estandar'),
(DATEADD(DAY,-50,GETDATE()), 'AltaCliente', 4, NULL, NULL, 3,
 'Nuevo cliente: Agustina Perez — DNI 33333333 — Plan: Premium'),
(DATEADD(DAY,-40,GETDATE()), 'AltaCliente', 4, NULL, NULL, 4,
 'Nuevo cliente: Camila Rodriguez — DNI 44444444 — Plan: Basico'),
(DATEADD(DAY,-30,GETDATE()), 'AltaCliente', 4, NULL, NULL, 5,
 'Nuevo cliente: Julieta Medina — DNI 55555555 — Plan: Estandar'),
(DATEADD(DAY,-45,GETDATE()), 'AltaCliente', 4, NULL, NULL, 6,
 'Nuevo cliente: Lucia Fernandez — DNI 66666666 — Plan: Premium'),
(DATEADD(DAY,-20,GETDATE()), 'AltaCliente', 4, NULL, NULL, 7,
 'Nuevo cliente: Marina Gomez — DNI 77777777 — Plan: Basico'),
(DATEADD(DAY,-15,GETDATE()), 'AltaCliente', 4, NULL, NULL, 8,
 'Nuevo cliente: Daniela Ruiz — DNI 88888888 — Plan: Estandar'),

-- ─── Modificacion de cliente ──────────────────────────────────────────────────
(DATEADD(DAY,-35,GETDATE()), 'ModificacionCliente', 4, NULL, NULL, 3,
 'Modificacion cliente: Agustina Perez — DNI 33333333 (actualizacion email)'),

-- ─── Ventas — pedidos creados ─────────────────────────────────────────────────
(DATEADD(DAY,-25,GETDATE()), 'Venta', 4, 1, NULL, 1,
 'Pedido #1 — Sofia Ramirez — 2 prendas — Plan Basico — 25/03/2026'),
(DATEADD(DAY,-45,GETDATE()), 'Venta', 4, 3, NULL, 3,
 'Pedido #3 — Agustina Perez — 2 prendas — Plan Premium'),
(DATEADD(DAY,-35,GETDATE()), 'Venta', 4, 6, NULL, 6,
 'Pedido #6 — Lucia Fernandez — 4 prendas — Plan Premium'),
(DATEADD(DAY,-20,GETDATE()), 'Venta', 4, 2, NULL, 2,
 'Pedido #2 — Valentina Torres — 3 prendas — Plan Estandar'),
(DATEADD(DAY,-10,GETDATE()), 'Venta', 4, 7, NULL, 8,
 'Pedido #7 — Daniela Ruiz — 2 prendas — Plan Estandar'),
(DATEADD(DAY, -5,GETDATE()), 'Venta', 4, 4, NULL, 4,
 'Pedido #4 — Camila Rodriguez — 1 prenda — Plan Basico'),
(DATEADD(DAY, -2,GETDATE()), 'Venta', 4, 5, NULL, 5,
 'Pedido #5 — Julieta Medina — 2 prendas — Plan Estandar'),
(DATEADD(DAY, -1,GETDATE()), 'Venta', 4, 8, NULL, 7,
 'Pedido #8 — Marina Gomez — 2 prendas — Plan Basico'),

-- ─── Despachos (IdUsuario=3 operador) ────────────────────────────────────────
(DATEADD(DAY,-23,GETDATE()), 'Despacho', 3, 1, NULL, 1,
 'Pedido #1 despachado — Sofia Ramirez — 2 prendas'),
(DATEADD(DAY,-43,GETDATE()), 'Despacho', 3, 3, NULL, 3,
 'Pedido #3 despachado — Agustina Perez — 2 prendas'),
(DATEADD(DAY,-33,GETDATE()), 'Despacho', 3, 6, NULL, 6,
 'Pedido #6 despachado — Lucia Fernandez — 4 prendas'),
(DATEADD(DAY,-18,GETDATE()), 'Despacho', 3, 2, NULL, 2,
 'Pedido #2 despachado — Valentina Torres — 3 prendas'),
(DATEADD(DAY, -3,GETDATE()), 'Despacho', 3, 4, NULL, 4,
 'Pedido #4 despachado — Camila Rodriguez — 1 prenda'),

-- ─── Entregas ─────────────────────────────────────────────────────────────────
(DATEADD(DAY,-20,GETDATE()), 'Entrega', 3, 1, NULL, 1,
 'Pedido #1 entregado — Sofia Ramirez'),
(DATEADD(DAY,-40,GETDATE()), 'Entrega', 3, 3, NULL, 3,
 'Pedido #3 entregado — Agustina Perez'),
(DATEADD(DAY,-30,GETDATE()), 'Entrega', 3, 6, NULL, 6,
 'Pedido #6 entregado — Lucia Fernandez'),
(DATEADD(DAY,-15,GETDATE()), 'Entrega', 3, 2, NULL, 2,
 'Pedido #2 entregado — Valentina Torres'),

-- ─── Cancelacion ──────────────────────────────────────────────────────────────
(DATEADD(DAY,-10,GETDATE()), 'Cancelacion', 4, 7, NULL, 8,
 'Pedido #7 cancelado — Daniela Ruiz. Prendas liberadas: 22, 23.'),

-- ─── Prendas en limpieza luego de devolución de Agustina ─────────────────────
(DATEADD(DAY,-15,GETDATE()), 'CambioEstadoPrenda', 3, NULL, 20, NULL,
 'Prenda ''Top bordado artesanal'' (ID 20): EnUso → EnLimpieza (devuelta por Agustina Perez)'),
(DATEADD(DAY,-15,GETDATE()), 'CambioEstadoPrenda', 3, NULL, 21, NULL,
 'Prenda ''Vestido midi floreado'' (ID 21): EnUso → EnLimpieza (devuelta por Agustina Perez)');
GO

PRINT '--- Datos de prueba cargados ---';

-- ============================================================
-- PASO 15: Verificacion final
-- ============================================================

PRINT '=== VERIFICACION FINAL ===';

SELECT 'USUARIOS' AS Seccion;
SELECT u.IdUsuario, u.Username, u.Rol, u.Perfil,
       CASE u.Estado WHEN 1 THEN 'Activo' ELSE 'BLOQUEADO' END AS Estado,
       ISNULL(e.Nombre + ' ' + e.Apellido, '(sin empleado)') AS Empleado,
       ISNULL(e.Legajo, '-') AS Legajo,
       (SELECT COUNT(*) FROM RolPermiso rp WHERE rp.Rol = u.Rol) AS CantPermisos
FROM Usuario u
LEFT JOIN Empleado e ON e.IdUsuario = u.IdUsuario
ORDER BY u.IdUsuario;
GO

SELECT 'PERMISOS POR ROL' AS Seccion;
SELECT rp.Rol,
       COUNT(*)                                           AS Total,
       STRING_AGG(p.NombreMenu, ', ')
           WITHIN GROUP (ORDER BY p.IdPermiso)           AS Permisos
FROM RolPermiso rp
JOIN Permiso p ON rp.IdPermiso = p.IdPermiso
GROUP BY rp.Rol
ORDER BY rp.Rol;
GO

SELECT 'CLIENTES Y STOCK' AS Seccion;
SELECT c.IdCliente,
       c.Nombre + ' ' + c.Apellido                             AS Cliente,
       ISNULL(p.Nombre,'Sin plan')                             AS Plan,
       ISNULL(CAST(p.LimitePrendas AS VARCHAR),'-')            AS Limite,
       c.MetodoPago,
       (SELECT COUNT(*) FROM Prenda pr
        WHERE pr.IdClienteActual = c.IdCliente)                AS PrendasEnUso
FROM Cliente c
LEFT JOIN PlanSuscripcion p ON c.IdPlan = p.IdPlan
ORDER BY c.IdCliente;
GO

SELECT 'PRENDAS POR ESTADO' AS Seccion;
SELECT CASE Estado
       WHEN 0 THEN '0 - Disponible'
       WHEN 1 THEN '1 - EnUso'
       WHEN 2 THEN '2 - EnLimpieza'
       WHEN 3 THEN '3 - Baja'
       END AS Estado,
       COUNT(*) AS Cantidad
FROM Prenda
GROUP BY Estado
ORDER BY Estado;
GO

SELECT 'DETALLE PRENDAS' AS Seccion;
SELECT p.IdPrenda, p.Nombre, p.Talle, p.Color, p.Categoria,
       CASE p.Estado
       WHEN 0 THEN 'Disponible'
       WHEN 1 THEN 'EnUso'
       WHEN 2 THEN 'EnLimpieza'
       WHEN 3 THEN 'Baja' END AS Estado,
       ISNULL(c.Nombre + ' ' + c.Apellido, '-') AS ClienteActual
FROM Prenda p
LEFT JOIN Cliente c ON c.IdCliente = p.IdClienteActual
ORDER BY p.Estado, p.IdPrenda;
GO

SELECT 'PEDIDOS' AS Seccion;
SELECT ped.IdPedido,
       c.Nombre + ' ' + c.Apellido                           AS Cliente,
       pl.Nombre                                             AS Plan,
       CASE ped.Estado
       WHEN 0 THEN '0 - Pendiente'
       WHEN 1 THEN '1 - Despachado'
       WHEN 2 THEN '2 - Entregado'
       WHEN 3 THEN '3 - Cancelado' END                      AS Estado,
       CONVERT(VARCHAR,ped.FechaPedido,103)                  AS FechaPedido,
       ISNULL(CONVERT(VARCHAR,ped.FechaDespacho,103), '-')   AS FechaDespacho,
       ISNULL(CONVERT(VARCHAR,ped.FechaEntrega,103),  '-')   AS FechaEntrega,
       (SELECT COUNT(*) FROM PedidoPrenda pp
        WHERE pp.IdPedido = ped.IdPedido)                    AS Prendas
FROM Pedido ped
JOIN Cliente c          ON c.IdCliente     = ped.IdCliente
JOIN PlanSuscripcion pl ON pl.IdPlan       = c.IdPlan
ORDER BY ped.IdPedido;
GO

SELECT 'BITACORA SISTEMA (ultimos registros)' AS Seccion;
SELECT TOP 20
       b.Id,
       CONVERT(VARCHAR,b.fecha,103) + ' ' + CONVERT(VARCHAR,b.fecha,108) AS Fecha,
       ISNULL(u.Username, '(anonimo)') AS Usuario,
       b.modulo, b.actividad,
       CASE b.criticidad
       WHEN 0 THEN 'None'  WHEN 1 THEN 'Baja'
       WHEN 2 THEN 'Media' WHEN 3 THEN 'Alta'
       WHEN 4 THEN 'IntentosLogin'
       WHEN 5 THEN 'RecupClave'
       WHEN 6 THEN 'BloqueosCuenta' END AS Criticidad
FROM Bitacora b
LEFT JOIN Usuario u ON u.IdUsuario = b.usuario
ORDER BY b.fecha DESC;
GO

SELECT 'BITACORA NEGOCIO (ultimos registros)' AS Seccion;
SELECT TOP 20
       bn.IdEvento,
       CONVERT(VARCHAR,bn.Fecha,103)            AS Fecha,
       ISNULL(u.Username,'?')                   AS Usuario,
       bn.Tipo,
       ISNULL('Pedido #' + CAST(bn.IdPedido AS VARCHAR), '-')  AS Pedido,
       ISNULL(c.Nombre + ' ' + c.Apellido, '-') AS Cliente,
       LEFT(bn.Descripcion, 70)                 AS Descripcion
FROM BitacoraNegocio bn
LEFT JOIN Usuario u ON u.IdUsuario = bn.IdUsuario
LEFT JOIN Cliente  c ON  c.IdCliente  = bn.IdCliente
ORDER BY bn.Fecha DESC;
GO

PRINT '=== WardrobeFlowDB lista para pruebas (v2.0) ===';
PRINT '';
PRINT 'Usuarios:  admin/admin123 | supervisor/supervisor123';
PRINT '           operador/operador123 | vendedor/vendedor123 | stock/stock123';
