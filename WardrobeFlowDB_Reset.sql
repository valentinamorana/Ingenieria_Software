-- ============================================================
-- WardrobeFlowDB — Script de Reset Completo (Iteración 3)
-- Roles: Administrador, OperadorLogistico, Supervisor,
--        Vendedor, ControladorDeStock, OperadorDeInventario
-- ============================================================

USE WardrobeFlowDB;
GO

-- ============================================================
-- PASO 1: Eliminar TODOS los FK constraints
-- ============================================================
DECLARE @nombreTabla  NVARCHAR(256);
DECLARE @nombreFK     NVARCHAR(256);
DECLARE @sql          NVARCHAR(512);

DECLARE curFK CURSOR FOR
    SELECT OBJECT_NAME(parent_object_id), name
    FROM sys.foreign_keys;

OPEN curFK;
FETCH NEXT FROM curFK INTO @nombreTabla, @nombreFK;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @sql = 'ALTER TABLE [' + @nombreTabla + '] DROP CONSTRAINT [' + @nombreFK + ']';
    EXEC(@sql);
    FETCH NEXT FROM curFK INTO @nombreTabla, @nombreFK;
END
CLOSE curFK;
DEALLOCATE curFK;
GO

PRINT '--- FK constraints eliminados ---';
GO

-- ============================================================
-- PASO 2: Eliminar TODAS las tablas
-- ============================================================
DECLARE @tabla NVARCHAR(256);
DECLARE @sql2  NVARCHAR(512);

DECLARE curTablas CURSOR FOR
    SELECT name FROM sys.tables WHERE type = 'U';

OPEN curTablas;
FETCH NEXT FROM curTablas INTO @tabla;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @sql2 = 'DROP TABLE [' + @tabla + ']';
    EXEC(@sql2);
    FETCH NEXT FROM curTablas INTO @tabla;
END
CLOSE curTablas;
DEALLOCATE curTablas;
GO

PRINT '--- Tablas eliminadas ---';
GO

-- ============================================================
-- PASO 3: Crear tablas del sistema base
-- ============================================================

-- Usuario: empleados que acceden al sistema interno
CREATE TABLE Usuario (
    IdUsuario  INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    Username   VARCHAR(50)   NOT NULL,
    Clave      VARCHAR(100)  NOT NULL, -- PBKDF2-SHA256 Base64(salt[16]+hash[32])
    Rol        VARCHAR(100)  NOT NULL,
    Estado     BIT           NOT NULL DEFAULT 1,
    Perfil     VARCHAR(100)  NOT NULL,
    CONSTRAINT UQ_Usuario_Username UNIQUE (Username)
);
GO

-- Permiso: catálogo de funcionalidades habilitables por rol
CREATE TABLE Permiso (
    IdPermiso      INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    Nombre         VARCHAR(100)  NOT NULL,
    NombreMenu     VARCHAR(100)  NOT NULL,
    TipoComponente VARCHAR(50)   NOT NULL,
    Estado         BIT           NOT NULL DEFAULT 1
);
GO

-- RolPermiso: mapeo Rol → Permisos
CREATE TABLE RolPermiso (
    Rol       VARCHAR(100) NOT NULL,
    IdPermiso INT          NOT NULL,
    CONSTRAINT PK_RolPermiso PRIMARY KEY (Rol, IdPermiso),
    CONSTRAINT FK_RolPermiso_Permiso FOREIGN KEY (IdPermiso) REFERENCES Permiso(IdPermiso)
);
GO

-- Bitacora: auditoría de seguridad del sistema
-- usuario es NULL para eventos pre-sesión (intentos fallidos, forgot password)
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
-- PASO 4: Crear tablas de negocio
-- ============================================================

-- PlanSuscripcion: tipos de suscripción con límite de prendas
CREATE TABLE PlanSuscripcion (
    IdPlan        INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    Nombre        VARCHAR(100)  NOT NULL,
    LimitePrendas INT           NOT NULL,
    Precio        DECIMAL(10,2) NOT NULL DEFAULT 0,
    Estado        BIT           NOT NULL DEFAULT 1
);
GO

-- Cliente: suscriptores del servicio (NO son usuarios del sistema)
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
GO

-- Empleado: datos personales del empleado, vinculado a un Usuario del sistema
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
    CONSTRAINT UQ_Empleado_DNI    UNIQUE (DNI),
    CONSTRAINT FK_Empleado_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(IdUsuario)
);
GO

-- Prenda: unidad de stock del catálogo
-- Estado: 0=Disponible, 1=EnUso, 2=EnLimpieza, 3=Baja
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
GO

-- Pedido: orden de entrega generada por el Vendedor
-- Estado: 0=Pendiente, 1=Despachado, 2=Entregado, 3=Cancelado
CREATE TABLE Pedido (
    IdPedido       INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    IdCliente      INT           NOT NULL,
    IdEmpleado     INT           NOT NULL,  -- FK → Empleado (Vendedor que creó el pedido)
    Estado         INT           NOT NULL DEFAULT 0,
    FechaPedido    DATETIME      NOT NULL DEFAULT GETDATE(),
    FechaDespacho  DATETIME      NULL,
    FechaEntrega   DATETIME      NULL,
    CONSTRAINT FK_Pedido_Cliente  FOREIGN KEY (IdCliente)  REFERENCES Cliente(IdCliente),
    CONSTRAINT FK_Pedido_Empleado FOREIGN KEY (IdEmpleado) REFERENCES Empleado(IdEmpleado)
);
GO

-- PedidoPrenda: prendas incluidas en cada pedido (tabla pivote)
CREATE TABLE PedidoPrenda (
    IdPedido  INT NOT NULL,
    IdPrenda  INT NOT NULL,
    CONSTRAINT PK_PedidoPrenda PRIMARY KEY (IdPedido, IdPrenda),
    CONSTRAINT FK_PP_Pedido FOREIGN KEY (IdPedido) REFERENCES Pedido(IdPedido),
    CONSTRAINT FK_PP_Prenda FOREIGN KEY (IdPrenda) REFERENCES Prenda(IdPrenda)
);
GO

-- BitacoraNegocio: auditoría de eventos de negocio
-- Separada de Bitacora (sistema) — registra ventas, despachos, stock
CREATE TABLE BitacoraNegocio (
    IdEvento    INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    Fecha       DATETIME      NOT NULL DEFAULT GETDATE(),
    Tipo        VARCHAR(50)   NOT NULL,  -- 'Venta','Despacho','Entrega','Cancelacion','AltaPrenda','CambioEstado','AltaCliente','BajaCliente'
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
GO

-- ============================================================
-- PASO 5: Permisos
--
-- ID  NombreMenu             Tipo        Rol(es)
--  1  mnuUsuarios            Sistema     Administrador
--  2  mnuAuditoria           Sistema     Supervisor, Administrador
--  3  mnuPrendas             Inventario  OperadorLogistico, ControladorDeStock
--  4  mnuOutfits             Inventario  OperadorLogistico
--  5  mnuCategorias          Inventario  OperadorLogistico
--  6  mnuStock               Inventario  ControladorDeStock
--  7  mnuClientes            Ventas      Vendedor
--  8  mnuPlanSuscripciones   Ventas      Vendedor
--  9  mnuPedidosVenta        Ventas      Vendedor
-- 10  mnuPedidosRealizados   Ventas      OperadorDeInventario
-- ============================================================
SET IDENTITY_INSERT Permiso ON;

INSERT INTO Permiso (IdPermiso, Nombre, NombreMenu, TipoComponente, Estado) VALUES
(1,  'Gestionar Usuarios',        'mnuUsuarios',           'Sistema',    1),
(2,  'Ver Auditoria',             'mnuAuditoria',          'Sistema',    1),
(3,  'Ver Prendas',               'mnuPrendas',            'Inventario', 1),
(4,  'Ver Outfits',               'mnuOutfits',            'Inventario', 1),
(5,  'Ver Categorias',            'mnuCategorias',         'Inventario', 1),
(6,  'Gestionar Stock',           'mnuStock',              'Inventario', 1),
(7,  'Gestionar Clientes',        'mnuClientes',           'Ventas',     1),
(8,  'Gestionar PlanSuscripciones','mnuPlanSuscripciones', 'Ventas',     1),
(9,  'Realizar Ventas',           'mnuPedidosVenta',       'Ventas',     1),
(10, 'Ver Pedidos Realizados',    'mnuPedidosRealizados',  'Ventas',     1);

SET IDENTITY_INSERT Permiso OFF;
GO

-- ============================================================
-- PASO 6: RolPermiso
-- ============================================================

-- Administrador → Usuarios + Auditoria
INSERT INTO RolPermiso (Rol, IdPermiso) VALUES
('Administrador', 1),
('Administrador', 2);
GO

-- Supervisor → Auditoria
INSERT INTO RolPermiso (Rol, IdPermiso) VALUES
('Supervisor', 2);
GO

-- OperadorLogistico → Inventario (Prendas, Outfits, Categorias)
INSERT INTO RolPermiso (Rol, IdPermiso) VALUES
('OperadorLogistico', 3),
('OperadorLogistico', 4),
('OperadorLogistico', 5);
GO

-- Vendedor → Clientes, PlanSuscripciones, Ventas
INSERT INTO RolPermiso (Rol, IdPermiso) VALUES
('Vendedor', 7),
('Vendedor', 8),
('Vendedor', 9);
GO

-- ControladorDeStock → Prendas + Stock
INSERT INTO RolPermiso (Rol, IdPermiso) VALUES
('ControladorDeStock', 3),
('ControladorDeStock', 6);
GO

-- OperadorDeInventario → Pedidos Realizados
INSERT INTO RolPermiso (Rol, IdPermiso) VALUES
('OperadorDeInventario', 10);
GO

-- ============================================================
-- PASO 7: Planes de suscripción semilla
-- ============================================================
SET IDENTITY_INSERT PlanSuscripcion ON;

INSERT INTO PlanSuscripcion (IdPlan, Nombre, LimitePrendas, Precio, Estado) VALUES
(1, 'Basico',    3,  2999.00, 1),
(2, 'Estandar',  6,  4999.00, 1),
(3, 'Premium',  10,  7999.00, 1);

SET IDENTITY_INSERT PlanSuscripcion OFF;
GO

-- ============================================================
-- PASO 8: Usuarios de prueba
-- ============================================================
INSERT INTO Usuario (Username, Clave, Rol, Estado, Perfil) VALUES
('admin',
 'hExifJ/2/dAQdmyspOkNDqBiXBB1/CnnAxSZWQipz2ouUJ3g97l+Tbw/E4JrJ0m3',
 'Administrador', 1, 'Administrador'),

('operador',
 'WOQdf5S386D6WWeHwrxe781QdxNn/gEo2UxZ/oldutqKaaTStIaUuMfNabURY4b7',
 'OperadorLogistico', 1, 'OperadorLogistico'),

('supervisor',
 'vCHo8PNtPRa76y+fYaHayKFePhSWQEdbkezfPOWBnBkpxSSzVNEM2twKlOis4t6j',
 'Supervisor', 1, 'Supervisor'),

('vendedor',
 '36+1c2ViEQOCdnOcIZ2QscwbnFQZ1Ugyzoqx4aK+Yhe5/VRZQ9DTv8bcyzdt1qxg',
 'Vendedor', 1, 'Vendedor'),

('stock',
 'ey08ghMWBGSQNFuczePtDA2w/Wv3jyQL003sOQAQcGg7qtl+77C6+D0di4vwwdMC',
 'ControladorDeStock', 1, 'ControladorDeStock'),

('inventario',
 'n/BLFwcDqBaXfmKp5foXJ5mXABzp2+QsBWARaSp9L+uh6MEeYxibRwxkocHqzHvv',
 'OperadorDeInventario', 1, 'OperadorDeInventario');
GO

-- ============================================================
-- PASO 9: Empleados de prueba (vinculados a los usuarios)
-- Necesarios para que el Vendedor pueda crear pedidos
-- ============================================================
INSERT INTO Empleado (Nombre, Apellido, DNI, Email, FechaIngreso, Puesto, Legajo, IdUsuario)
SELECT 'Juan',  'Pérez',      '20000001', 'vendedor@wf.com',    GETDATE(), 'Vendedor',             'LEG-001', IdUsuario FROM Usuario WHERE Username = 'vendedor';

INSERT INTO Empleado (Nombre, Apellido, DNI, Email, FechaIngreso, Puesto, Legajo, IdUsuario)
SELECT 'Ana',   'González',   '20000002', 'inventario@wf.com',  GETDATE(), 'Operador Inventario',  'LEG-002', IdUsuario FROM Usuario WHERE Username = 'inventario';

INSERT INTO Empleado (Nombre, Apellido, DNI, Email, FechaIngreso, Puesto, Legajo, IdUsuario)
SELECT 'Luis',  'Martínez',   '20000003', 'stock@wf.com',       GETDATE(), 'Controlador de Stock', 'LEG-003', IdUsuario FROM Usuario WHERE Username = 'stock';

INSERT INTO Empleado (Nombre, Apellido, DNI, Email, FechaIngreso, Puesto, Legajo, IdUsuario)
SELECT 'Marta', 'López',      '20000004', 'supervisor@wf.com',  GETDATE(), 'Supervisora',          'LEG-004', IdUsuario FROM Usuario WHERE Username = 'supervisor';

INSERT INTO Empleado (Nombre, Apellido, DNI, Email, FechaIngreso, Puesto, Legajo, IdUsuario)
SELECT 'Carlos','Rodríguez',  '20000005', 'operador@wf.com',    GETDATE(), 'Operador Logístico',   'LEG-005', IdUsuario FROM Usuario WHERE Username = 'operador';
GO

-- ============================================================
-- PASO 10: Verificación final
-- ============================================================
SELECT 'Tablas creadas:' AS Info;
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME;
GO

SELECT 'Permisos por rol:' AS Info;
SELECT rp.Rol, p.Nombre, p.NombreMenu
FROM RolPermiso rp
JOIN Permiso p ON rp.IdPermiso = p.IdPermiso
ORDER BY rp.Rol, p.TipoComponente, p.Nombre;
GO

SELECT 'Usuarios:' AS Info;
SELECT u.IdUsuario, u.Username, u.Rol,
       e.Nombre + ' ' + e.Apellido AS Empleado,
       e.Legajo
FROM Usuario u
LEFT JOIN Empleado e ON e.IdUsuario = u.IdUsuario
ORDER BY u.IdUsuario;
GO

PRINT '=== Reset completado (Iteración 3) ===';
