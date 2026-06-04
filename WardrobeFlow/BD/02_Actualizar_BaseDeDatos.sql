-- ============================================================
-- WardrobeFlow — 02. ACTUALIZAR BASE DE DATOS EXISTENTE
-- ------------------------------------------------------------
-- Aplica de forma IDEMPOTENTE todas las migraciones sobre una
-- WardrobeFlowDB ya creada:
--   • columnas nuevas (Permiso.EsFamilia/EsRol, Cliente.FechaVencimiento, etc.)
--   • tablas nuevas (PermisoRelacion, Control, Idioma, Traduccion,
--     HistorialUsuario, HistorialIntegridad, MantenimientoPrenda, …)
--   • migración Composite T04: nodos-rol + RolPermiso → PermisoRelacion.
-- NO inserta datos semilla base (la BD existente ya los tiene).
--
-- Para crear la BD de cero, usar: 01_Crear_BaseDeDatos.sql
-- ============================================================

USE WardrobeFlowDB;
GO

-- ============================================================
-- TABLAS BASE
-- ============================================================

-- Usuario
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Usuario')
BEGIN
    CREATE TABLE Usuario (
        IdUsuario        INT           IDENTITY(1,1) PRIMARY KEY,
        Username         NVARCHAR(100) NOT NULL,
        Clave            NVARCHAR(500) NOT NULL,
        Rol              NVARCHAR(100) NULL,
        Perfil           NVARCHAR(100) NULL,
        Estado           BIT           NOT NULL DEFAULT 1,
        IntentosFallidos INT           NOT NULL DEFAULT 0,
        DVH              INT           NULL,
        IdIdioma         VARCHAR(5)    NULL,
        CONSTRAINT UQ_Usuario_Username UNIQUE (Username)
    );
    PRINT 'Tabla Usuario creada.';
END
ELSE
    PRINT 'Tabla Usuario ya existe — sin cambios.';
GO

-- DVVertical (T07 Dígitos Verificadores)
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'DVVertical')
BEGIN
    CREATE TABLE DVVertical (
        Id           INT          IDENTITY(1,1) PRIMARY KEY,
        NombreTabla  VARCHAR(100) NOT NULL,
        DVV          INT          NOT NULL,
        FechaCalculo DATETIME     NOT NULL DEFAULT GETDATE(),
        CONSTRAINT UQ_DVVertical_Tabla UNIQUE (NombreTabla)
    );
    PRINT 'Tabla DVVertical creada.';
END
ELSE
    PRINT 'Tabla DVVertical ya existe — sin cambios.';
GO

-- PlanSuscripcion
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PlanSuscripcion')
BEGIN
    CREATE TABLE PlanSuscripcion (
        IdPlan        INT            IDENTITY(1,1) PRIMARY KEY,
        Nombre        NVARCHAR(100)  NOT NULL,
        LimitePrendas INT            NOT NULL DEFAULT 0,
        Precio        DECIMAL(10, 2) NOT NULL DEFAULT 0,
        Estado        BIT            NOT NULL DEFAULT 1
    );
    PRINT 'Tabla PlanSuscripcion creada.';
END
ELSE
    PRINT 'Tabla PlanSuscripcion ya existe — sin cambios.';
GO

-- Permiso (T04 Composite — EsFamilia discrimina Familia vs Patente)
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Permiso')
BEGIN
    CREATE TABLE Permiso (
        IdPermiso       INT           IDENTITY(1,1) PRIMARY KEY,
        Nombre          NVARCHAR(100) NOT NULL,
        NombreMenu      NVARCHAR(100) NULL,
        TipoComponente  NVARCHAR(100) NULL,
        Estado          BIT           NOT NULL DEFAULT 1,
        EsFamilia       BIT           NOT NULL DEFAULT 0,
        EsRol           BIT           NOT NULL DEFAULT 0
    );
    PRINT 'Tabla Permiso creada.';
END
ELSE
BEGIN
    -- Si ya existe, asegurar que EsFamilia esté presente (migración v6.0)
    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                   WHERE TABLE_NAME = 'Permiso' AND COLUMN_NAME = 'EsFamilia')
    BEGIN
        ALTER TABLE Permiso ADD EsFamilia BIT NOT NULL DEFAULT 0;
        PRINT 'Columna EsFamilia agregada a Permiso (migración).';
    END
    -- EsRol: marca los nodos-rol del Composite (migración v7.0 — T04)
    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                   WHERE TABLE_NAME = 'Permiso' AND COLUMN_NAME = 'EsRol')
    BEGIN
        ALTER TABLE Permiso ADD EsRol BIT NOT NULL DEFAULT 0;
        PRINT 'Columna EsRol agregada a Permiso (migración).';
    END
END
GO

-- Idioma (T05 Multiidioma)
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Idioma')
BEGIN
    CREATE TABLE Idioma (
        IdIdioma  INT IDENTITY(1,1) PRIMARY KEY,
        Codigo    VARCHAR(5)    NOT NULL,
        Nombre    NVARCHAR(50)  NOT NULL,
        Activo    BIT           NOT NULL DEFAULT 1,
        EsDefault BIT           NOT NULL DEFAULT 0,
        CONSTRAINT UQ_Idioma_Codigo UNIQUE (Codigo)
    );
    PRINT 'Tabla Idioma creada.';
END
ELSE
    PRINT 'Tabla Idioma ya existe — sin cambios.';
GO

-- Control (claves de traducción — T05)
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Control')
BEGIN
    CREATE TABLE Control (
        IdControl  INT IDENTITY(1,1) PRIMARY KEY,
        Clave      VARCHAR(100) NOT NULL,
        Formulario VARCHAR(50)  NOT NULL DEFAULT 'General',
        CONSTRAINT UQ_Control_Clave UNIQUE (Clave)
    );
    PRINT 'Tabla Control creada.';
END
ELSE
    PRINT 'Tabla Control ya existe — sin cambios.';
GO

-- ============================================================
-- TABLAS CON FK
-- ============================================================

-- Bitacora (sistema — refs Usuario)
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Bitacora')
BEGIN
    CREATE TABLE Bitacora (
        Id         INT            IDENTITY(1,1) PRIMARY KEY,
        fecha      DATETIME       NOT NULL DEFAULT GETDATE(),
        usuario    INT            NULL REFERENCES Usuario(IdUsuario),
        modulo     NVARCHAR(100)  NULL,
        actividad  NVARCHAR(200)  NULL,
        detalle    NVARCHAR(1000) NULL,
        criticidad INT            NOT NULL DEFAULT 0,
        ip         NVARCHAR(50)   NULL
    );
    PRINT 'Tabla Bitacora creada.';
END
ELSE
    PRINT 'Tabla Bitacora ya existe — sin cambios.';
GO

-- Empleado (refs Usuario — FK nullable: empleado puede no tener usuario del sistema)
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Empleado')
BEGIN
    CREATE TABLE Empleado (
        IdEmpleado   INT           IDENTITY(1,1) PRIMARY KEY,
        Nombre       NVARCHAR(100) NOT NULL,
        Apellido     NVARCHAR(100) NOT NULL,
        DNI          NVARCHAR(200) NOT NULL,  -- T03: almacena el DNI CIFRADO (AES Base64)
        Email        NVARCHAR(200) NULL,
        FechaIngreso DATETIME      NOT NULL DEFAULT GETDATE(),
        Puesto       NVARCHAR(100) NULL,
        Legajo       NVARCHAR(50)  NULL,
        IdUsuario    INT           NULL REFERENCES Usuario(IdUsuario),
        DVH          INT           NULL              -- T07: dígito verificador horizontal
    );
    PRINT 'Tabla Empleado creada.';
END
ELSE
    PRINT 'Tabla Empleado ya existe — sin cambios.';
GO

-- Cliente (refs PlanSuscripcion)
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Cliente')
BEGIN
    CREATE TABLE Cliente (
        IdCliente   INT           IDENTITY(1,1) PRIMARY KEY,
        Nombre      NVARCHAR(100) NOT NULL,
        Apellido    NVARCHAR(100) NOT NULL,
        DNI         NVARCHAR(200) NOT NULL,  -- T03: almacena el DNI CIFRADO (AES Base64)
        Email       NVARCHAR(200) NULL,
        MetodoPago  NVARCHAR(100) NULL,
        IdPlan      INT           NULL REFERENCES PlanSuscripcion(IdPlan),
        FechaAlta   DATETIME      NOT NULL DEFAULT GETDATE(),
        Activo      BIT           NOT NULL DEFAULT 1,
        DVH         INT           NULL              -- T07: dígito verificador horizontal
    );
    PRINT 'Tabla Cliente creada.';
END
ELSE
    PRINT 'Tabla Cliente ya existe — sin cambios.';
GO

-- Prenda (refs Cliente — FK nullable: prenda disponible no tiene cliente)
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Prenda')
BEGIN
    CREATE TABLE Prenda (
        IdPrenda        INT           IDENTITY(1,1) PRIMARY KEY,
        Nombre          NVARCHAR(100) NOT NULL,
        Descripcion     NVARCHAR(500) NULL,
        Talle           NVARCHAR(20)  NULL,
        Color           NVARCHAR(50)  NULL,
        Categoria       NVARCHAR(100) NULL,
        Estado          INT           NOT NULL DEFAULT 0,
        IdClienteActual INT           NULL REFERENCES Cliente(IdCliente),
        FechaAlta       DATETIME      NOT NULL DEFAULT GETDATE()
    );
    PRINT 'Tabla Prenda creada.';
END
ELSE
    PRINT 'Tabla Prenda ya existe — sin cambios.';
GO

-- Pedido (refs Cliente + Empleado)
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Pedido')
BEGIN
    CREATE TABLE Pedido (
        IdPedido          INT           IDENTITY(1,1) PRIMARY KEY,
        IdCliente         INT           NOT NULL REFERENCES Cliente(IdCliente),
        IdEmpleado        INT           NOT NULL REFERENCES Empleado(IdEmpleado),
        Estado            INT           NOT NULL DEFAULT 0,
        FechaPedido       DATETIME      NOT NULL DEFAULT GETDATE(),
        FechaDespacho     DATETIME      NULL,
        FechaEntrega      DATETIME      NULL,
        MotivoCancelacion NVARCHAR(500) NULL,
        DVH               INT           NULL              -- T07: DV horizontal (incluye sus líneas)
    );
    PRINT 'Tabla Pedido creada.';
END
ELSE
    PRINT 'Tabla Pedido ya existe — sin cambios.';
GO

-- PedidoPrenda (junction Pedido-Prenda)
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PedidoPrenda')
BEGIN
    CREATE TABLE PedidoPrenda (
        IdPedido INT NOT NULL REFERENCES Pedido(IdPedido),
        IdPrenda INT NOT NULL REFERENCES Prenda(IdPrenda),
        CONSTRAINT PK_PedidoPrenda PRIMARY KEY (IdPedido, IdPrenda)
    );
    PRINT 'Tabla PedidoPrenda creada.';
END
ELSE
    PRINT 'Tabla PedidoPrenda ya existe — sin cambios.';
GO

-- PedidoHistorial (auditoría de cambios de pedidos — refs Pedido + Usuario)
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PedidoHistorial')
BEGIN
    CREATE TABLE PedidoHistorial (
        IdHistorial   INT            IDENTITY(1,1) PRIMARY KEY,
        IdPedido      INT            NOT NULL REFERENCES Pedido(IdPedido),
        IdOperacion   INT            NOT NULL,
        Fecha         DATETIME       NOT NULL DEFAULT GETDATE(),
        IdUsuario     INT            NULL REFERENCES Usuario(IdUsuario),
        NombreUsuario NVARCHAR(200)  NULL,
        Accion        NVARCHAR(200)  NOT NULL,
        Campo         NVARCHAR(100)  NOT NULL,
        ValorAnterior NVARCHAR(1000) NULL,
        ValorNuevo    NVARCHAR(1000) NULL
    );
    PRINT 'Tabla PedidoHistorial creada.';
END
ELSE
    PRINT 'Tabla PedidoHistorial ya existe — sin cambios.';
GO

-- BitacoraNegocio (eventos de negocio — refs Usuario, Pedido, Prenda, Cliente)
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BitacoraNegocio')
BEGIN
    CREATE TABLE BitacoraNegocio (
        IdEvento    INT           IDENTITY(1,1) PRIMARY KEY,
        Fecha       DATETIME      NOT NULL DEFAULT GETDATE(),
        Tipo        NVARCHAR(100) NOT NULL,
        IdUsuario   INT           NULL REFERENCES Usuario(IdUsuario),
        IdPedido    INT           NULL REFERENCES Pedido(IdPedido),
        IdPrenda    INT           NULL REFERENCES Prenda(IdPrenda),
        IdCliente   INT           NULL REFERENCES Cliente(IdCliente),
        Descripcion NVARCHAR(500) NOT NULL
    );
    PRINT 'Tabla BitacoraNegocio creada.';
END
ELSE
    PRINT 'Tabla BitacoraNegocio ya existe — sin cambios.';
GO

-- RolPermiso (asignación de permisos a roles)
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'RolPermiso')
BEGIN
    CREATE TABLE RolPermiso (
        Rol       NVARCHAR(100) NOT NULL,
        IdPermiso INT           NOT NULL REFERENCES Permiso(IdPermiso),
        CONSTRAINT PK_RolPermiso PRIMARY KEY (Rol, IdPermiso)
    );
    PRINT 'Tabla RolPermiso creada.';
END
ELSE
    PRINT 'Tabla RolPermiso ya existe — sin cambios.';
GO

-- PermisoRelacion (árbol Composite padre → hijo — T04)
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PermisoRelacion')
BEGIN
    CREATE TABLE PermisoRelacion (
        IdPadre INT NOT NULL REFERENCES Permiso(IdPermiso),
        IdHijo  INT NOT NULL REFERENCES Permiso(IdPermiso),
        CONSTRAINT PK_PermisoRelacion PRIMARY KEY (IdPadre, IdHijo)
    );
    PRINT 'Tabla PermisoRelacion creada.';
END
ELSE
    PRINT 'Tabla PermisoRelacion ya existe — sin cambios.';
GO

-- Traduccion (PK compuesta — T05)
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Traduccion')
BEGIN
    CREATE TABLE Traduccion (
        IdControl INT            NOT NULL REFERENCES Control(IdControl),
        IdIdioma  INT            NOT NULL REFERENCES Idioma(IdIdioma),
        Texto     NVARCHAR(1000) NOT NULL DEFAULT '',
        CONSTRAINT PK_Traduccion PRIMARY KEY (IdControl, IdIdioma)
    );
    PRINT 'Tabla Traduccion creada.';
END
ELSE
    PRINT 'Tabla Traduccion ya existe — sin cambios.';
GO

-- HistorialUsuario (snapshots de cambios de usuarios — T06)
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'HistorialUsuario')
BEGIN
    CREATE TABLE HistorialUsuario (
        IdVersion    INT           IDENTITY(1,1) PRIMARY KEY,
        IdUsuario    INT           NOT NULL REFERENCES Usuario(IdUsuario),
        Fecha        DATETIME      NOT NULL DEFAULT GETDATE(),
        Actor        NVARCHAR(100) NOT NULL,
        Detalle      NVARCHAR(500) NOT NULL,
        UsernameSnap NVARCHAR(100) NOT NULL,
        ClaveSnap    NVARCHAR(500) NOT NULL,
        EstadoSnap   BIT           NOT NULL,
        IntentosSnap INT           NOT NULL
    );
    PRINT 'Tabla HistorialUsuario creada.';
END
ELSE
    PRINT 'Tabla HistorialUsuario ya existe — sin cambios.';
GO

-- HistorialIntegridad (historial de verificaciones de integridad DV — T07)
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'HistorialIntegridad')
BEGIN
    CREATE TABLE HistorialIntegridad (
        Id                INT          IDENTITY(1,1) PRIMARY KEY,
        NombreTabla       VARCHAR(100) NOT NULL,
        DVVAlmacenado     INT          NULL,
        DVVCalculado      INT          NOT NULL,
        Resultado         BIT          NOT NULL,
        FilasCorruptas    INT          NOT NULL DEFAULT 0,
        FechaVerificacion DATETIME     NOT NULL DEFAULT GETDATE(),
        DisparadoPor      VARCHAR(50)  NOT NULL
            CONSTRAINT CHK_HistInteg_Origen CHECK (DisparadoPor IN ('Arranque', 'Timer', 'Manual'))
    );
    PRINT 'Tabla HistorialIntegridad creada.';
END
ELSE
    PRINT 'Tabla HistorialIntegridad ya existe — sin cambios.';
GO

-- ============================================================
-- SEEDS INICIALES
-- ============================================================

-- Idiomas (ES, EN, RU)
INSERT INTO Idioma (Codigo, Nombre, Activo, EsDefault)
SELECT v.Codigo, v.Nombre, v.Activo, v.EsDefault
FROM (VALUES
    ('ES', N'Español', 1, 1),
    ('EN', N'English', 1, 0),
    ('RU', N'Русский', 1, 0)
) AS v(Codigo, Nombre, Activo, EsDefault)
WHERE NOT EXISTS (SELECT 1 FROM Idioma WHERE Codigo = v.Codigo);
PRINT 'Idiomas inicializados (ES, EN, RU).';
GO

-- DVV inicial para la tabla Usuario (en 0 — recalcular desde la app)
IF NOT EXISTS (SELECT 1 FROM DVVertical WHERE NombreTabla = 'Usuario')
BEGIN
    INSERT INTO DVVertical (NombreTabla, DVV, FechaCalculo)
    VALUES ('Usuario', 0, GETDATE());
    PRINT 'DVV inicial insertado para tabla Usuario.';
END
GO

-- DVH = 0 para usuarios existentes sin DVH
UPDATE Usuario SET DVH = 0 WHERE DVH IS NULL;
GO

-- IdIdioma = ES para usuarios existentes sin preferencia
UPDATE Usuario SET IdIdioma = 'ES' WHERE IdIdioma IS NULL;
GO

-- ============================================================
-- MIGRACIÓN COMPOSITE (T04)
-- Genera nodos Familia desde TipoComponente si no existen aún.
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM Permiso WHERE EsFamilia = 1)
BEGIN
    DECLARE @mapa TABLE (Grupo NVARCHAR(100), IdFamilia INT);

    INSERT INTO Permiso (Nombre, NombreMenu, TipoComponente, Estado, EsFamilia)
    OUTPUT INSERTED.Nombre, INSERTED.IdPermiso INTO @mapa (Grupo, IdFamilia)
    SELECT DISTINCT
        TipoComponente,
        TipoComponente,
        TipoComponente,
        1,
        1
    FROM Permiso
    WHERE TipoComponente IS NOT NULL
      AND LTRIM(RTRIM(TipoComponente)) <> ''
      AND EsFamilia = 0;

    INSERT INTO PermisoRelacion (IdPadre, IdHijo)
    SELECT m.IdFamilia, p.IdPermiso
    FROM   Permiso p
    INNER JOIN @mapa m ON p.TipoComponente = m.Grupo
    WHERE  p.EsFamilia = 0;

    PRINT 'Árbol Composite generado desde grupos TipoComponente.';
END
ELSE
    PRINT 'Árbol Composite ya inicializado — sin cambios.';
GO

-- ============================================================
-- MIGRACIÓN COMPOSITE (T04) — Roles como NODOS del árbol
-- A partir de v7.0 [PermisoRelacion] es la única fuente de verdad de la
-- composición. Se crea un nodo-rol por cada rol de [RolPermiso] y se migran
-- las asignaciones planas a aristas rol→permiso.
-- ============================================================

-- Crear nodo-rol faltante por cada rol existente en RolPermiso
INSERT INTO Permiso (Nombre, NombreMenu, TipoComponente, Estado, EsFamilia, EsRol)
SELECT DISTINCT rp.Rol, rp.Rol, 'Rol', 1, 1, 1
FROM   RolPermiso rp
WHERE  NOT EXISTS (SELECT 1 FROM Permiso p WHERE p.Nombre = rp.Rol AND p.EsRol = 1);

-- Migrar asignaciones planas a aristas Composite (idempotente)
INSERT INTO PermisoRelacion (IdPadre, IdHijo)
SELECT pr.IdPermiso, rp.IdPermiso
FROM   RolPermiso rp
INNER JOIN Permiso pr ON pr.Nombre = rp.Rol AND pr.EsRol = 1 AND pr.IdPermiso <> rp.IdPermiso
WHERE  NOT EXISTS (SELECT 1 FROM PermisoRelacion x
                   WHERE x.IdPadre = pr.IdPermiso AND x.IdHijo = rp.IdPermiso);
PRINT 'Nodos-rol y aristas rol→permiso migrados (T04 v7.0).';
GO

-- ============================================================
-- MIGRACIONES INCREMENTALES
-- ============================================================

-- T03 — Preparar DNI para almacenar el valor CIFRADO (AES Base64 ~44 chars).
-- 1) Quitar la constraint UNIQUE sobre DNI: el cifrado usa IV aleatorio (mismo DNI →
--    distinto texto), por lo que la unicidad a nivel BD ya no aplica. La unicidad se
--    valida en la capa de negocio (DAL.Cliente/Empleado.ExisteDNI, descifrando).
DECLARE @uqCli SYSNAME = (SELECT TOP 1 tc.CONSTRAINT_NAME
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
    JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE ccu ON tc.CONSTRAINT_NAME=ccu.CONSTRAINT_NAME
    WHERE tc.CONSTRAINT_TYPE='UNIQUE' AND tc.TABLE_NAME='Cliente' AND ccu.COLUMN_NAME='DNI');
IF @uqCli IS NOT NULL EXEC('ALTER TABLE Cliente DROP CONSTRAINT ' + @uqCli);

DECLARE @uqEmp SYSNAME = (SELECT TOP 1 tc.CONSTRAINT_NAME
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
    JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE ccu ON tc.CONSTRAINT_NAME=ccu.CONSTRAINT_NAME
    WHERE tc.CONSTRAINT_TYPE='UNIQUE' AND tc.TABLE_NAME='Empleado' AND ccu.COLUMN_NAME='DNI');
IF @uqEmp IS NOT NULL EXEC('ALTER TABLE Empleado DROP CONSTRAINT ' + @uqEmp);
GO

-- 2) Ensanchar la columna para el texto cifrado.
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
           WHERE TABLE_NAME='Cliente' AND COLUMN_NAME='DNI' AND CHARACTER_MAXIMUM_LENGTH < 200)
BEGIN
    ALTER TABLE Cliente ALTER COLUMN DNI NVARCHAR(200) NOT NULL;
    PRINT 'Cliente.DNI ensanchado a NVARCHAR(200) (cifrado T03).';
END
GO
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
           WHERE TABLE_NAME='Empleado' AND COLUMN_NAME='DNI' AND CHARACTER_MAXIMUM_LENGTH < 200)
BEGIN
    ALTER TABLE Empleado ALTER COLUMN DNI NVARCHAR(200) NOT NULL;
    PRINT 'Empleado.DNI ensanchado a NVARCHAR(200) (cifrado T03).';
END
GO

-- T07 — Columna DVH (dígito verificador horizontal) en Cliente y Empleado.
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Cliente' AND COLUMN_NAME='DVH')
BEGIN
    ALTER TABLE Cliente ADD DVH INT NULL;
    PRINT 'Columna DVH agregada a Cliente (T07).';
END
GO
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Empleado' AND COLUMN_NAME='DVH')
BEGIN
    ALTER TABLE Empleado ADD DVH INT NULL;
    PRINT 'Columna DVH agregada a Empleado (T07).';
END
GO
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Pedido' AND COLUMN_NAME='DVH')
BEGIN
    ALTER TABLE Pedido ADD DVH INT NULL;
    PRINT 'Columna DVH agregada a Pedido (T07 multi-tabla).';
END
GO

-- FechaVencimiento en Cliente (suscripción con fecha de vencimiento)
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'Cliente' AND COLUMN_NAME = 'FechaVencimiento')
BEGIN
    ALTER TABLE Cliente ADD FechaVencimiento DATE NULL;
    PRINT 'Columna FechaVencimiento agregada a Cliente.';
END
ELSE
    PRINT 'FechaVencimiento ya existe en Cliente — sin cambios.';
GO

-- FechaNacimiento en Cliente (para validar mayoría de edad)
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'Cliente' AND COLUMN_NAME = 'FechaNacimiento')
BEGIN
    ALTER TABLE Cliente ADD FechaNacimiento DATE NULL;
    PRINT 'Columna FechaNacimiento agregada a Cliente.';
END
ELSE
    PRINT 'FechaNacimiento ya existe en Cliente — sin cambios.';
GO

-- Permisos adicionales para Supervisor (mismo acceso que Vendedor + auditoría ya tenía)
INSERT INTO RolPermiso (Rol, IdPermiso)
SELECT r.Rol, p.IdPermiso
FROM (VALUES
    ('Supervisor','mnuPrendas'),
    ('Supervisor','mnuClientes'),
    ('Supervisor','mnuPlanSuscripciones'),
    ('Supervisor','mnuPedidosVenta')
) AS r(Rol, NombreMenu)
JOIN Permiso p ON p.NombreMenu = r.NombreMenu AND ISNULL(p.EsFamilia,0) = 0
WHERE NOT EXISTS (SELECT 1 FROM RolPermiso x WHERE x.Rol = r.Rol AND x.IdPermiso = p.IdPermiso);

-- Regenerar aristas Composite para el nodo Supervisor
INSERT INTO PermisoRelacion (IdPadre, IdHijo)
SELECT pr.IdPermiso, rp.IdPermiso
FROM   RolPermiso rp
INNER JOIN Permiso pr ON pr.Nombre = rp.Rol AND pr.EsRol = 1 AND pr.IdPermiso <> rp.IdPermiso
WHERE  NOT EXISTS (SELECT 1 FROM PermisoRelacion x
                   WHERE x.IdPadre = pr.IdPermiso AND x.IdHijo = rp.IdPermiso);
PRINT 'Permisos de Supervisor actualizados (mnuPrendas/mnuClientes/mnuPlanSuscripciones/mnuPedidosVenta).';
GO

-- MantenimientoPrenda (historial de limpieza/mantenimiento por prenda)
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'MantenimientoPrenda')
BEGIN
    CREATE TABLE MantenimientoPrenda (
        IdMantenimiento INT           IDENTITY(1,1) PRIMARY KEY,
        IdPrenda        INT           NOT NULL REFERENCES Prenda(IdPrenda),
        FechaEntrada    DATETIME      NOT NULL DEFAULT GETDATE(),
        FechaSalida     DATETIME      NULL,
        Actor           NVARCHAR(100) NULL
    );
    PRINT 'Tabla MantenimientoPrenda creada.';
END
ELSE
    PRINT 'Tabla MantenimientoPrenda ya existe — sin cambios.';
GO

PRINT '';
PRINT '=== WardrobeFlowDB deploy completo. ===';
PRINT 'IMPORTANTE: Ejecutar recálculo de DVH/DVV desde la aplicación';
PRINT '            antes del primer uso (Administrar → Usuarios → Recalcular DV).';
PRINT 'Las traducciones se seedean automáticamente en el primer uso de la app.';
GO
