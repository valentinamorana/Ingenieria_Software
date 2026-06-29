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
--
-- NOTA DE CODIFICACIÓN: este archivo es UTF-8 y contiene acentos (Básico, Lucía…).
--   • En SSMS se ejecuta sin problemas.
--   • Con sqlcmd usar el codepage UTF-8:  sqlcmd -S .\SQLEXPRESS -E -f 65001 -i 02_Actualizar_BaseDeDatos.sql
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

-- Usuario_Seguridad (T07 — tabla ESPEJO de integridad)
-- Copia sombra de los campos que entran al DVH de cada usuario, más su DVH.
-- Se mantiene en sincronía con cada escritura LEGÍTIMA (la app la actualiza junto al DVH).
-- Permite, ante una manipulación: (1) diagnosticar QUÉ campo cambió comparando contra el
-- espejo y (2) REPARAR restaurando el valor legítimo sin necesitar un backup completo.
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Usuario_Seguridad')
BEGIN
    CREATE TABLE Usuario_Seguridad (
        IdUsuario          INT           PRIMARY KEY,   -- mismo Id que Usuario (NO identity: lo fija la app)
        Username           NVARCHAR(100) NOT NULL,
        Clave              NVARCHAR(500) NOT NULL,
        Rol                NVARCHAR(100) NULL,
        Perfil             NVARCHAR(100) NULL,
        Estado             BIT           NOT NULL DEFAULT 1,
        IntentosFallidos   INT           NOT NULL DEFAULT 0,
        DVH                INT           NULL,
        FechaActualizacion DATETIME      NOT NULL DEFAULT GETDATE()
    );
    PRINT 'Tabla Usuario_Seguridad (espejo de integridad) creada.';
END
ELSE
    PRINT 'Tabla Usuario_Seguridad ya existe — sin cambios.';
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

-- RolPermiso (asignación PLANA rol→patente) — LEGACY / SOLO SEED-BOOTSTRAP.
-- Se usa únicamente para sembrar el árbol: desde estas asignaciones se generan los nodos-rol y
-- las aristas de [PermisoRelacion], que es la ÚNICA fuente de verdad de autorización en runtime.
-- El sistema NO escribe esta tabla en runtime y solo la lee en fallbacks para BDs sin migrar.
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

-- T06 (revisión) — Snapshots de datos administrativos NO sensibles en el Historial de Cambios.
-- El historial pasa a versionar nombre/apellido/fecha nac./email (además del username);
-- la clave queda solo como trazabilidad interna y NUNCA se muestra ni se restaura.
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='HistorialUsuario' AND COLUMN_NAME='NombreSnap')
    ALTER TABLE HistorialUsuario ADD NombreSnap NVARCHAR(100) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='HistorialUsuario' AND COLUMN_NAME='ApellidoSnap')
    ALTER TABLE HistorialUsuario ADD ApellidoSnap NVARCHAR(100) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='HistorialUsuario' AND COLUMN_NAME='FechaNacSnap')
    ALTER TABLE HistorialUsuario ADD FechaNacSnap DATE NULL;
GO
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='HistorialUsuario' AND COLUMN_NAME='EmailSnap')
    ALTER TABLE HistorialUsuario ADD EmailSnap NVARCHAR(200) NULL;
GO
PRINT 'HistorialUsuario: columnas de snapshot administrativo verificadas.';
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

-- RF-10 — Baja lógica de usuarios (archivado) + purga diferida.
-- Activo:    1=activo, 0=archivado (no puede loguear ni aparece en la lista).
-- FechaBaja: cuándo se archivó (la purga física solo aplica a archivados con >1 año).
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME='Usuario' AND COLUMN_NAME='Activo')
BEGIN
    ALTER TABLE Usuario ADD Activo BIT NOT NULL DEFAULT 1;
    PRINT 'Columna Activo agregada a Usuario (RF-10 baja lógica).';
END
ELSE
    PRINT 'Usuario.Activo ya existe — sin cambios.';
GO
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME='Usuario' AND COLUMN_NAME='FechaBaja')
BEGIN
    ALTER TABLE Usuario ADD FechaBaja DATETIME NULL;
    PRINT 'Columna FechaBaja agregada a Usuario (RF-10 purga diferida).';
END
ELSE
    PRINT 'Usuario.FechaBaja ya existe — sin cambios.';
GO
-- Usuarios existentes quedan activos por defecto.
UPDATE Usuario SET Activo = 1 WHERE Activo IS NULL;
GO

-- Bloqueo de login PROGRESIVO (Etapa 3): escalado temporal de bloqueos por intentos fallidos.
-- CantidadBloqueos: cuántas veces se bloqueó la cuenta (define la duración: 1/5/15/60 min).
-- FechaBloqueo:     instante del último bloqueo (para calcular cuándo expira). NULL = no bloqueada
--                   por tiempo (o bloqueo manual del admin, que no auto-expira).
-- NO forman parte del DVH: son metadata operativa anti-fuerza-bruta, no identidad protegida.
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME='Usuario' AND COLUMN_NAME='CantidadBloqueos')
BEGIN
    ALTER TABLE Usuario ADD CantidadBloqueos INT NOT NULL DEFAULT 0;
    PRINT 'Columna CantidadBloqueos agregada a Usuario (bloqueo progresivo).';
END
ELSE
    PRINT 'Usuario.CantidadBloqueos ya existe — sin cambios.';
GO
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME='Usuario' AND COLUMN_NAME='FechaBloqueo')
BEGIN
    ALTER TABLE Usuario ADD FechaBloqueo DATETIME NULL;
    PRINT 'Columna FechaBloqueo agregada a Usuario (bloqueo progresivo).';
END
ELSE
    PRINT 'Usuario.FechaBloqueo ya existe — sin cambios.';
GO
-- Cambio de clave OBLIGATORIO en el primer login tras un alta o un reset de contraseña.
-- RequiereCambioClave = 1 mientras la cuenta tenga una clave temporal/generada que el usuario
-- todavía no reemplazó. NO forma parte del DVH (metadata operativa, no identidad protegida).
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME='Usuario' AND COLUMN_NAME='RequiereCambioClave')
BEGIN
    ALTER TABLE Usuario ADD RequiereCambioClave BIT NOT NULL DEFAULT 0;
    PRINT 'Columna RequiereCambioClave agregada a Usuario (cambio de clave obligatorio).';
END
ELSE
    PRINT 'Usuario.RequiereCambioClave ya existe — sin cambios.';
GO
UPDATE Usuario SET CantidadBloqueos = 0 WHERE CantidadBloqueos IS NULL;
GO

-- ABM (revisión) — Datos administrativos NO sensibles del usuario (nombre, apellido,
-- fecha de nacimiento, email). Editables desde Administración de Usuarios y versionados
-- por el Historial de Cambios. NO entran al DVH (metadata administrativa).
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Usuario' AND COLUMN_NAME='Nombre')
    ALTER TABLE Usuario ADD Nombre NVARCHAR(100) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Usuario' AND COLUMN_NAME='Apellido')
    ALTER TABLE Usuario ADD Apellido NVARCHAR(100) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Usuario' AND COLUMN_NAME='FechaNacimiento')
    ALTER TABLE Usuario ADD FechaNacimiento DATE NULL;
GO
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Usuario' AND COLUMN_NAME='Email')
    ALTER TABLE Usuario ADD Email NVARCHAR(200) NULL;
GO
PRINT 'Usuario: columnas de datos administrativos (Nombre/Apellido/FechaNacimiento/Email) verificadas.';
GO

-- Completar datos administrativos de los usuarios semilla que estén vacíos (idempotente).
UPDATE u SET u.Nombre = v.Nombre, u.Apellido = v.Apellido, u.Email = v.Email, u.FechaNacimiento = v.FechaNac
FROM Usuario u
JOIN (VALUES
    ('admin',       N'Admin',     N'Sistema',  'admin@wardrobeflow.com',       '1985-01-15'),
    ('vendedor',    N'Valentina', N'Bolívar',  'vendedor@wardrobeflow.com',    '1995-06-20'),
    ('operador',    N'Oscar',     N'Pérez',    'operador@wardrobeflow.com',    '1990-03-10'),
    ('auditor',     N'Ana',       N'Díaz',     'auditor@wardrobeflow.com',     '1988-09-05'),
    ('gcomercial',  N'Gabriel',   N'Morán',    'gcomercial@wardrobeflow.com',  '1983-11-25'),
    ('ginventario', N'Gisela',    N'Ortiz',    'ginventario@wardrobeflow.com', '1986-07-30'),
    ('logistico',   N'Lucas',     N'Gómez',    'logistico@wardrobeflow.com',   '1992-02-18')
) AS v(Username, Nombre, Apellido, Email, FechaNac) ON u.Username = v.Username
WHERE u.Nombre IS NULL AND u.Apellido IS NULL;
GO

-- ============================================================
-- Etapa 4 — Permisos a nivel de CONTROL (mapeo patente ↔ control de un formulario)
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ControlMapeado')
BEGIN
    CREATE TABLE ControlMapeado (
        IdControlMapeado INT           IDENTITY(1,1) PRIMARY KEY,
        IdPermiso        INT           NOT NULL,
        Formulario       NVARCHAR(100) NOT NULL,
        NombreControl    NVARCHAR(100) NOT NULL,
        CONSTRAINT FK_ControlMapeado_Permiso FOREIGN KEY (IdPermiso) REFERENCES Permiso(IdPermiso),
        CONSTRAINT UQ_ControlMapeado UNIQUE (Formulario, NombreControl)
    );
    PRINT 'Tabla ControlMapeado creada (Etapa 4 - permisos por control).';
END
ELSE
    PRINT 'Tabla ControlMapeado ya existe — sin cambios.';
GO

-- Seed inicial: mapea cada patente con NombreMenu al item de menu correspondiente del Menu
-- principal, como punto de partida coherente con la visibilidad actual. Idempotente.
INSERT INTO ControlMapeado (IdPermiso, Formulario, NombreControl)
SELECT p.IdPermiso, v.Formulario, v.NombreControl
FROM (VALUES
    ('mnuPrendas',           'Menu', 'prendasToolStripMenuItem'),
    ('mnuClientes',          'Menu', 'clientesToolStripMenuItem'),
    ('mnuPlanSuscripciones', 'Menu', 'planesToolStripMenuItem'),
    ('mnuPedidosVenta',      'Menu', 'pedidosVentaToolStripMenuItem'),
    ('mnuPedidosRealizados', 'Menu', 'pedidosRealizadosToolStripMenuItem'),
    ('mnuUsuarios',          'Menu', 'usuariosToolStripMenuItem'),
    ('mnuAuditoria',         'Menu', 'bitacoraToolStripMenuItem')
) AS v(NombreMenu, Formulario, NombreControl)
INNER JOIN Permiso p ON p.NombreMenu = v.NombreMenu
                    AND ISNULL(p.EsFamilia,0) = 0 AND ISNULL(p.EsRol,0) = 0
WHERE NOT EXISTS (SELECT 1 FROM ControlMapeado x
                  WHERE x.Formulario = v.Formulario AND x.NombreControl = v.NombreControl);
PRINT 'Seed de ControlMapeado (mapeos de menu) aplicado.';
GO

-- Claves de emergencia (1 solo uso) para autodesbloqueo de un Administrador.
-- Hasheadas (PBKDF2); el .txt con las claves en texto plano es la copia del admin.
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ClaveRecuperacion')
BEGIN
    CREATE TABLE ClaveRecuperacion (
        IdClave       INT           IDENTITY(1,1) PRIMARY KEY,
        ClaveHash     NVARCHAR(500) NOT NULL,
        Usada         BIT           NOT NULL DEFAULT 0,
        UsadaPor      NVARCHAR(100) NULL,
        FechaUso      DATETIME      NULL,
        FechaCreacion DATETIME      NOT NULL DEFAULT GETDATE()
    );
    PRINT 'Tabla ClaveRecuperacion creada (claves de emergencia).';
END
ELSE
    PRINT 'Tabla ClaveRecuperacion ya existe — sin cambios.';
GO

-- Preferencia (preferencias de UI por usuario: fuente, tamaño, tema, formato de fecha…).
-- ON DELETE CASCADE: al purgar físicamente un usuario, su fila de preferencias se borra sola.
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Preferencia')
BEGIN
    CREATE TABLE Preferencia (
        IdUsuario      INT          NOT NULL PRIMARY KEY,
        FuenteFamilia  NVARCHAR(50) NULL,
        FuenteTamano   VARCHAR(10)  NULL,
        Tema           VARCHAR(20)  NULL,
        FormatoFecha   VARCHAR(20)  NULL,
        Notificaciones BIT          NULL,
        CONSTRAINT FK_Preferencia_Usuario FOREIGN KEY (IdUsuario)
            REFERENCES Usuario(IdUsuario) ON DELETE CASCADE
    );
    PRINT 'Tabla Preferencia creada.';
END
ELSE
    PRINT 'Tabla Preferencia ya existe — sin cambios.';
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
-- JERARQUÍA DE ROLES (T04) — roles nuevos con vistas + rol-dentro-de-rol
-- Idempotente. Crea los nodos-rol nuevos, les asigna sus patentes (vistas)
-- propias y arma las aristas rol→rol para que el padre HEREDE recursivamente
-- las vistas del hijo (demostración real del Composite con jerarquía de roles).
--
--   Auditor                                → Auditoría
--   GerenteComercial  ⊃ Vendedor           → (Vendedor) + Pedidos Realizados
--   EncargadoDeStock  ⊃ OperadorLogistico  → (despacho) + Prendas + Stock
--   GerenteInventario ⊃ EncargadoDeStock   → (lo anterior) + Categorías + Outfits
-- ============================================================

-- 1) Nodos-rol para los roles nuevos (si faltan).
INSERT INTO Permiso (Nombre, NombreMenu, TipoComponente, Estado, EsFamilia, EsRol)
SELECT v.Rol, v.Rol, 'Rol', 1, 1, 1
FROM (VALUES
    ('Auditor'), ('GerenteComercial'), ('OperadorLogistico'),
    ('EncargadoDeStock'), ('GerenteInventario')
) AS v(Rol)
WHERE NOT EXISTS (SELECT 1 FROM Permiso p WHERE p.Nombre = v.Rol AND p.EsRol = 1);
GO

-- 2) Patentes PROPIAS (vistas directas) de cada rol nuevo (aristas rol→patente).
INSERT INTO PermisoRelacion (IdPadre, IdHijo)
SELECT rol.IdPermiso, pat.IdPermiso
FROM (VALUES
    ('Auditor',           'mnuAuditoria'),
    ('GerenteComercial',  'mnuPedidosRealizados'),
    ('OperadorLogistico', 'mnuPedidosRealizados'),
    ('EncargadoDeStock',  'mnuPrendas'),
    ('EncargadoDeStock',  'mnuStock'),
    ('GerenteInventario', 'mnuCategorias'),
    ('GerenteInventario', 'mnuOutfits')
) AS v(Rol, NombreMenu)
INNER JOIN Permiso rol ON rol.Nombre = v.Rol AND rol.EsRol = 1
INNER JOIN Permiso pat ON pat.NombreMenu = v.NombreMenu
                       AND ISNULL(pat.EsFamilia,0) = 0 AND ISNULL(pat.EsRol,0) = 0
WHERE NOT EXISTS (SELECT 1 FROM PermisoRelacion x
                  WHERE x.IdPadre = rol.IdPermiso AND x.IdHijo = pat.IdPermiso);
GO

-- 3) Jerarquía rol→rol: el padre hereda recursivamente las vistas del hijo.
INSERT INTO PermisoRelacion (IdPadre, IdHijo)
SELECT padre.IdPermiso, hijo.IdPermiso
FROM (VALUES
    ('GerenteComercial',  'Vendedor'),
    ('EncargadoDeStock',  'OperadorLogistico'),
    ('GerenteInventario', 'EncargadoDeStock')
) AS v(Padre, Hijo)
INNER JOIN Permiso padre ON padre.Nombre = v.Padre AND padre.EsRol = 1
INNER JOIN Permiso hijo  ON hijo.Nombre  = v.Hijo  AND hijo.EsRol  = 1
WHERE NOT EXISTS (SELECT 1 FROM PermisoRelacion x
                  WHERE x.IdPadre = padre.IdPermiso AND x.IdHijo = hijo.IdPermiso);
GO

-- ============================================================
-- USUARIOS DEMO DE LOS ROLES NUEVOS + SUPERVISOR COMPUESTO
-- Idempotente. Crea un usuario por rol nuevo (claves en Contraseñas.txt) y convierte a
-- Supervisor en rol COMPUESTO (Supervisor ⊃ Vendedor) conservando su Auditoría propia:
-- mismos permisos efectivos que antes, pero ahora obtenidos por HERENCIA (no copiados).
-- Las claves se guardan hasheadas (PBKDF2). Texto plano: usuario1! (ver Contraseñas.txt).
-- ============================================================

INSERT INTO Usuario (Username, Clave, Rol, Perfil, Estado, IntentosFallidos, DVH, IdIdioma, Activo)
SELECT v.Username, v.Clave, v.Rol, v.Perfil, 1, 0, 0, 'ES', 1
FROM (VALUES
  ('auditor',     'SmnGp5hqSC+FXdbLiccYieNpC6vEaWn6nVpgZFrlyyeOXqx8yTjJYOgaw+oXAP7B', 'Auditor',           'Auditor'),
  ('gcomercial',  '1KWgyl8MkMcuisigf4fRBDb2f803LIsA/hvfKpzfwcMbRboUiS5CwuvFQq64GJft', 'GerenteComercial',  'GerenteComercial'),
  ('ginventario', 'G89lxogCulMeAK+WA5rNHSyWpuz5QKF/DBH8PSfiPbUv5SBKStFVQYXylikq+OMw', 'GerenteInventario', 'GerenteInventario'),
  ('encargado',   'H9LyJ5OBv0m2atrETHrimO5mBcVpKLn46uy/hiTgw4130wCo2H7/sgqAoZ/zroA8', 'EncargadoDeStock',  'EncargadoDeStock'),
  ('logistico',   'U3g703lDqDJfLgaXpOaNiAQpDOaBSq1LUMzEu3X7x8hh6097jTcMUNAsPfl1SCVG', 'OperadorLogistico', 'OperadorLogistico')
) AS v(Username, Clave, Rol, Perfil)
WHERE NOT EXISTS (SELECT 1 FROM Usuario u WHERE u.Username = v.Username);
PRINT 'Usuarios demo de roles nuevos inicializados.';
GO

-- Supervisor → COMPUESTO: se quitan las patentes de Vendedor asignadas directo (quedan por
-- herencia) y se agrega la arista Supervisor → Vendedor; conserva su Auditoría propia.
DELETE r FROM PermisoRelacion r
JOIN Permiso sup ON sup.IdPermiso = r.IdPadre AND sup.Nombre = 'Supervisor' AND sup.EsRol = 1
JOIN Permiso pat ON pat.IdPermiso = r.IdHijo
                AND pat.NombreMenu IN ('mnuPrendas','mnuClientes','mnuPlanSuscripciones','mnuPedidosVenta');
INSERT INTO PermisoRelacion (IdPadre, IdHijo)
SELECT sup.IdPermiso, ven.IdPermiso
FROM Permiso sup JOIN Permiso ven ON ven.Nombre = 'Vendedor' AND ven.EsRol = 1
WHERE sup.Nombre = 'Supervisor' AND sup.EsRol = 1
  AND NOT EXISTS (SELECT 1 FROM PermisoRelacion x WHERE x.IdPadre = sup.IdPermiso AND x.IdHijo = ven.IdPermiso);
PRINT 'Supervisor convertido en rol compuesto (Supervisor → Vendedor).';
GO

-- Si se insertaron usuarios nuevos en una BD ya inicializada, resetear el DV para que la app
-- lo recalcule limpio en el próximo arranque (evita una falsa alarma de integridad por mezcla).
IF EXISTS (SELECT 1 FROM Usuario WHERE Username IN ('auditor','gcomercial','ginventario','encargado','logistico') AND DVH = 0)
   AND EXISTS (SELECT 1 FROM Usuario WHERE DVH <> 0)
BEGIN
    UPDATE Usuario SET DVH = 0;
    UPDATE DVVertical SET DVV = 0 WHERE NombreTabla = 'Usuario';
    PRINT 'DV de Usuario reseteado para recálculo en el próximo arranque.';
END
GO

-- ============================================================
-- Simplificación de Permisos — ELIMINAR FAMILIAS del árbol Composite (revisión)
-- Los permisos (patentes) son un catálogo fijo y las FAMILIAS se retiran de la experiencia.
-- (1) Se materializan las relaciones Rol→Patente alcanzables a través de familias (por si un
--     usuario había anidado familias dentro de roles), (2) se quitan las aristas que tocan
--     familias y (3) se desactivan los nodos Familia. El anidamiento Rol→Rol se preserva
--     (el patrón Composite sigue vigente). Idempotente.
-- ============================================================
IF EXISTS (SELECT 1 FROM Permiso WHERE ISNULL(EsFamilia,0)=1 AND ISNULL(EsRol,0)=0 AND Estado=1)
BEGIN
    ;WITH Arbol AS (
        SELECT r.IdPermiso AS IdRol, pr.IdHijo AS IdNodo
        FROM Permiso r
        JOIN PermisoRelacion pr ON pr.IdPadre = r.IdPermiso
        WHERE r.EsRol = 1
        UNION ALL
        SELECT a.IdRol, pr.IdHijo
        FROM Arbol a
        JOIN Permiso n ON n.IdPermiso = a.IdNodo AND ISNULL(n.EsFamilia,0)=1 AND ISNULL(n.EsRol,0)=0
        JOIN PermisoRelacion pr ON pr.IdPadre = a.IdNodo
    )
    INSERT INTO PermisoRelacion (IdPadre, IdHijo)
    SELECT DISTINCT a.IdRol, a.IdNodo
    FROM Arbol a
    JOIN Permiso p ON p.IdPermiso = a.IdNodo AND ISNULL(p.EsFamilia,0)=0 AND ISNULL(p.EsRol,0)=0
    WHERE NOT EXISTS (SELECT 1 FROM PermisoRelacion x WHERE x.IdPadre=a.IdRol AND x.IdHijo=a.IdNodo)
    OPTION (MAXRECURSION 50);

    DELETE r FROM PermisoRelacion r
    JOIN Permiso p ON (p.IdPermiso = r.IdPadre OR p.IdPermiso = r.IdHijo)
    WHERE ISNULL(p.EsFamilia,0)=1 AND ISNULL(p.EsRol,0)=0;

    UPDATE Permiso SET Estado = 0 WHERE ISNULL(EsFamilia,0)=1 AND ISNULL(EsRol,0)=0;

    PRINT 'Familias eliminadas del Composite: roles aplanados a Rol->Patente.';
END
ELSE
    PRINT 'No hay familias activas — árbol ya aplanado.';
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

-- FechaNacimiento en Cliente — OBLIGATORIA (NOT NULL) para validar mayoría de edad.
-- 1) Agregar la columna como NULL si todavía no existe.
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'Cliente' AND COLUMN_NAME = 'FechaNacimiento')
BEGIN
    ALTER TABLE Cliente ADD FechaNacimiento DATE NULL;
    PRINT 'Columna FechaNacimiento agregada a Cliente.';
END
GO
-- 2) Backfill de filas legacy sin fecha (placeholder mayor de edad) para poder imponer NOT NULL.
--    Revisá estos clientes en la app y cargales su fecha real.
UPDATE Cliente SET FechaNacimiento = '1990-01-01' WHERE FechaNacimiento IS NULL;
GO
-- 3) Imponer NOT NULL si la columna todavía admite nulos.
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
           WHERE TABLE_NAME='Cliente' AND COLUMN_NAME='FechaNacimiento' AND IS_NULLABLE='YES')
BEGIN
    ALTER TABLE Cliente ALTER COLUMN FechaNacimiento DATE NOT NULL;
    PRINT 'Cliente.FechaNacimiento ahora es NOT NULL (obligatoria).';
END
ELSE
    PRINT 'Cliente.FechaNacimiento ya es NOT NULL — sin cambios.';
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

-- ============================================================
-- CONSOLIDACIÓN DE ROLES (v8) — decisión de diseño 2da entrega
-- Va AL FINAL de todas las migraciones de roles (después de la última regeneración de
-- aristas desde RolPermiso) para que el estado objetivo no sea sobreescrito. Idempotente.
--   • Inventario — dos operadores con responsabilidades CLARAS, sin duplicados:
--       - OperadorLogistico    → pedidos / despacho       (Ver Pedidos Realizados)
--       - OperadorDeInventario → mantenimiento de prendas (Ver Prendas + Gestionar Stock)
--       - GerenteInventario ⊃ AMBOS                       (+ Categorías + Outfits)
--     Se RETIRAN EncargadoDeStock y ControladorDeStock (eran redundantes con lo anterior).
--   • Comercial — se RETIRA Supervisor; el jefe es GerenteComercial ⊃ Vendedor.
-- ============================================================

-- (a) Asegurar el nodo-rol OperadorDeInventario (por si la BD no lo tenía).
INSERT INTO Permiso (Nombre, NombreMenu, TipoComponente, Estado, EsFamilia, EsRol)
SELECT 'OperadorDeInventario', 'OperadorDeInventario', 'Rol', 1, 1, 1
WHERE NOT EXISTS (SELECT 1 FROM Permiso WHERE Nombre = 'OperadorDeInventario' AND EsRol = 1);
GO

-- (b) OperadorDeInventario: sus patentes propias deben ser EXACTAMENTE Prendas + Stock.
--     Se quita cualquier patente vieja (p. ej. el legacy 'Ver Pedidos Realizados') y se agregan las nuevas.
DELETE r FROM PermisoRelacion r
JOIN Permiso rol ON rol.IdPermiso = r.IdPadre AND rol.Nombre = 'OperadorDeInventario' AND rol.EsRol = 1
JOIN Permiso pat ON pat.IdPermiso = r.IdHijo  AND ISNULL(pat.EsRol,0) = 0
WHERE pat.NombreMenu NOT IN ('mnuPrendas','mnuStock');
INSERT INTO PermisoRelacion (IdPadre, IdHijo)
SELECT rol.IdPermiso, pat.IdPermiso
FROM (VALUES ('mnuPrendas'), ('mnuStock')) AS v(NombreMenu)
JOIN Permiso rol ON rol.Nombre = 'OperadorDeInventario' AND rol.EsRol = 1
JOIN Permiso pat ON pat.NombreMenu = v.NombreMenu AND ISNULL(pat.EsFamilia,0) = 0 AND ISNULL(pat.EsRol,0) = 0
WHERE NOT EXISTS (SELECT 1 FROM PermisoRelacion x WHERE x.IdPadre = rol.IdPermiso AND x.IdHijo = pat.IdPermiso);
GO

-- (c) GerenteInventario ⊃ OperadorLogistico + OperadorDeInventario (y se quita la arista vieja a EncargadoDeStock).
DELETE r FROM PermisoRelacion r
JOIN Permiso gi ON gi.IdPermiso = r.IdPadre AND gi.Nombre = 'GerenteInventario' AND gi.EsRol = 1
JOIN Permiso ed ON ed.IdPermiso = r.IdHijo  AND ed.Nombre = 'EncargadoDeStock'  AND ed.EsRol = 1;
INSERT INTO PermisoRelacion (IdPadre, IdHijo)
SELECT gi.IdPermiso, h.IdPermiso
FROM (VALUES ('OperadorLogistico'), ('OperadorDeInventario')) AS v(Hijo)
JOIN Permiso gi ON gi.Nombre = 'GerenteInventario' AND gi.EsRol = 1
JOIN Permiso h  ON h.Nombre  = v.Hijo AND h.EsRol = 1
WHERE NOT EXISTS (SELECT 1 FROM PermisoRelacion x WHERE x.IdPadre = gi.IdPermiso AND x.IdHijo = h.IdPermiso);
GO

-- (d) Migrar usuarios de los roles que se retiran + normalizar Perfil a código interno.
DECLARE @rolesCambiados INT = 0;
UPDATE Usuario SET Rol = 'OperadorDeInventario', Perfil = 'OperadorDeInventario'
WHERE Rol IN ('EncargadoDeStock','ControladorDeStock') OR Perfil IN ('EncargadoDeStock','ControladorDeStock','Controlador de Stock');
SET @rolesCambiados = @rolesCambiados + @@ROWCOUNT;
UPDATE Usuario SET Rol = 'GerenteComercial', Perfil = 'GerenteComercial'
WHERE Rol = 'Supervisor' OR Perfil = 'Supervisor';
SET @rolesCambiados = @rolesCambiados + @@ROWCOUNT;
-- Normalizar perfiles legacy guardados con el NOMBRE VISIBLE (con espacios) al código interno,
-- para que el switch de dashboard por Perfil y las comparaciones de rol funcionen igual que con los nuevos.
UPDATE Usuario SET Perfil = 'OperadorDeInventario' WHERE Perfil = 'Operador de Inventario';
SET @rolesCambiados = @rolesCambiados + @@ROWCOUNT;
UPDATE Usuario SET Perfil = 'OperadorLogistico' WHERE Perfil = 'Operador Logístico';
SET @rolesCambiados = @rolesCambiados + @@ROWCOUNT;

-- (e) Retirar los roles redundantes: quitar TODAS sus aristas (como padre o como hijo) y desactivar el nodo.
DELETE r FROM PermisoRelacion r
JOIN Permiso p ON (p.IdPermiso = r.IdPadre OR p.IdPermiso = r.IdHijo)
WHERE p.EsRol = 1 AND p.Nombre IN ('EncargadoDeStock','ControladorDeStock','Supervisor');
UPDATE Permiso SET Estado = 0 WHERE EsRol = 1 AND Nombre IN ('EncargadoDeStock','ControladorDeStock','Supervisor');

-- (f) Si cambió el Rol/Perfil de algún usuario, el DVH (que incluye el Rol) quedó desfasado:
--     resetear el DV de Usuario para que la app lo recalcule limpio en el próximo arranque.
IF @rolesCambiados > 0
BEGIN
    UPDATE Usuario SET DVH = 0;
    UPDATE DVVertical SET DVV = 0 WHERE NombreTabla = 'Usuario';
    PRINT 'Roles consolidados; DV de Usuario reseteado para recálculo en el próximo arranque.';
END
PRINT 'Consolidación de roles (v8) aplicada.';
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

-- ============================================================
-- DATOS DEMO (presentación / pruebas)
-- ------------------------------------------------------------
-- Idempotente: cada bloque sólo inserta si el registro falta.
-- El DNI se guarda en TEXTO PLANO; la app lo tolera (TryDesencriptar
-- acepta registros legacy sin cifrar) y lo muestra tal cual.
-- DVH=0 → recalcular el DV desde la app (Usuarios → Recalcular DV)
-- antes del primer uso para que la verificación de integridad cierre.
-- ============================================================

-- Planes de suscripción
INSERT INTO PlanSuscripcion (Nombre, LimitePrendas, Precio, Estado)
SELECT v.Nombre, v.Limite, v.Precio, 1
FROM (VALUES
    (N'Básico',   5,  8000.00),
    (N'Estándar', 15, 15000.00),
    (N'Premium',  30, 25000.00)
) AS v(Nombre, Limite, Precio)
WHERE NOT EXISTS (SELECT 1 FROM PlanSuscripcion p WHERE p.Nombre = v.Nombre);
PRINT 'Demo: planes de suscripción.';
GO

-- Empleados (vinculados a los usuarios del sistema por Username)
INSERT INTO Empleado (Nombre, Apellido, DNI, Email, FechaIngreso, Puesto, Legajo, IdUsuario, DVH)
SELECT v.Nombre, v.Apellido, v.DNI, v.Email, GETDATE(), v.Puesto, v.Legajo,
       (SELECT TOP 1 IdUsuario FROM Usuario u WHERE u.Username = v.Username), 0
FROM (VALUES
    (N'Valentina', N'Morana', '33111000', 'vendedor@wardrobeflow.com', N'Vendedora',           'L-001', 'vendedor'),
    (N'Bruno',     N'Díaz',   '31222000', 'operador@wardrobeflow.com', N'Operador Inventario', 'L-002', 'operador'),
    (N'Carla',     N'Méndez', '32333000', 'stock@wardrobeflow.com',    N'Controlador Stock',   'L-003', 'stock')
) AS v(Nombre, Apellido, DNI, Email, Puesto, Legajo, Username)
WHERE NOT EXISTS (SELECT 1 FROM Empleado e WHERE e.Legajo = v.Legajo);
PRINT 'Demo: empleados.';
GO

-- Clientes (todos mayores de edad; FechaNacimiento obligatoria)
INSERT INTO Cliente (Nombre, Apellido, DNI, Email, MetodoPago, IdPlan, FechaAlta, FechaNacimiento, Activo, DVH)
SELECT v.Nombre, v.Apellido, v.DNI, v.Email, v.MetodoPago,
       (SELECT TOP 1 IdPlan FROM PlanSuscripcion p WHERE p.Nombre = v.PlanNom),
       GETDATE(), v.FechaNac, 1, 0
FROM (VALUES
    (N'Lucía',  N'Fernández', '30111222', 'lucia.fernandez@mail.com', 'Efectivo',      N'Premium',  CONVERT(date,'1990-03-15')),
    (N'Martín', N'Gómez',     '28999111', 'martin.gomez@mail.com',    'Crédito',       N'Estándar', CONVERT(date,'1985-07-22')),
    (N'Sofía',  N'Rossi',     '35444555', 'sofia.rossi@mail.com',     'Débito',        N'Básico',   CONVERT(date,'1998-11-02')),
    (N'Diego',  N'Paz',       '27333444', 'diego.paz@mail.com',       'Transferencia', N'Estándar', CONVERT(date,'1982-01-30')),
    (N'Camila', N'Torres',    '40222333', 'camila.torres@mail.com',   'Efectivo',      N'Premium',  CONVERT(date,'2001-06-10'))
) AS v(Nombre, Apellido, DNI, Email, MetodoPago, PlanNom, FechaNac)
WHERE NOT EXISTS (SELECT 1 FROM Cliente c WHERE c.Nombre = v.Nombre AND c.Apellido = v.Apellido);
PRINT 'Demo: clientes.';
GO

-- Prendas (catálogo de stock; todas Disponible inicialmente)
INSERT INTO Prenda (Nombre, Descripcion, Talle, Color, Categoria, Estado, IdClienteActual, FechaAlta)
SELECT v.Nombre, v.Descripcion, v.Talle, v.Color, v.Categoria, 0, NULL, GETDATE()
FROM (VALUES
    (N'Vestido Largo Negro',    N'Vestido de fiesta largo',  'M',  N'Negro',      N'Vestido'),
    (N'Blazer Beige',           N'Blazer entallado',         'L',  N'Beige',      N'Saco'),
    (N'Camisa Blanca Clásica',  N'Camisa de algodón',        'M',  N'Blanco',     N'Camisa'),
    (N'Pantalón Sastre Gris',   N'Pantalón de vestir',       '42', N'Gris',       N'Pantalón'),
    (N'Abrigo Largo Camel',     N'Tapado de paño',           'L',  N'Camel',      N'Abrigo'),
    (N'Vestido Floral',         N'Vestido estampado verano', 'S',  N'Estampado',  N'Vestido'),
    (N'Camisa Celeste',         N'Camisa de lino',           'L',  N'Celeste',    N'Camisa'),
    (N'Jean Recto Azul',        N'Jean clásico',             '40', N'Azul',       N'Pantalón'),
    (N'Saco a Cuadros',         N'Saco príncipe de Gales',   'M',  N'Multicolor', N'Saco'),
    (N'Falda Plisada Negra',    N'Falda midi plisada',       'S',  N'Negro',      N'Falda'),
    (N'Sweater Oversize Crema', N'Sweater de lana',          'L',  N'Crema',      N'Sweater'),
    (N'Gabardina Verde',        N'Gabardina impermeable',    'M',  N'Verde',      N'Abrigo')
) AS v(Nombre, Descripcion, Talle, Color, Categoria)
WHERE NOT EXISTS (SELECT 1 FROM Prenda pr WHERE pr.Nombre = v.Nombre);
PRINT 'Demo: prendas (stock).';
GO

-- Pedidos demo (sólo si aún no hay pedidos) + prendas EnUso asignadas a su cliente,
-- replicando lo que hace la app al crear/despachar (Prenda.Estado=EnUso, IdClienteActual).
IF NOT EXISTS (SELECT 1 FROM Pedido)
   AND EXISTS (SELECT 1 FROM Empleado WHERE Legajo = 'L-001')
   AND EXISTS (SELECT 1 FROM Cliente WHERE Nombre = N'Lucía' AND Apellido = N'Fernández')
BEGIN
    DECLARE @emp     INT = (SELECT TOP 1 IdEmpleado FROM Empleado WHERE Legajo = 'L-001');
    DECLARE @cLucia  INT = (SELECT TOP 1 IdCliente FROM Cliente WHERE Nombre=N'Lucía'  AND Apellido=N'Fernández');
    DECLARE @cMartin INT = (SELECT TOP 1 IdCliente FROM Cliente WHERE Nombre=N'Martín' AND Apellido=N'Gómez');
    DECLARE @cSofia  INT = (SELECT TOP 1 IdCliente FROM Cliente WHERE Nombre=N'Sofía'  AND Apellido=N'Rossi');

    DECLARE @pr1 INT = (SELECT IdPrenda FROM Prenda WHERE Nombre=N'Vestido Largo Negro');
    DECLARE @pr2 INT = (SELECT IdPrenda FROM Prenda WHERE Nombre=N'Blazer Beige');
    DECLARE @pr3 INT = (SELECT IdPrenda FROM Prenda WHERE Nombre=N'Camisa Blanca Clásica');
    DECLARE @pr4 INT = (SELECT IdPrenda FROM Prenda WHERE Nombre=N'Pantalón Sastre Gris');

    -- Pedido 1: Lucía — Entregado (2 prendas)
    INSERT INTO Pedido (IdCliente, IdEmpleado, Estado, FechaPedido, FechaDespacho, FechaEntrega, DVH)
    VALUES (@cLucia, @emp, 2, DATEADD(day,-20,GETDATE()), DATEADD(day,-18,GETDATE()), DATEADD(day,-15,GETDATE()), 0);
    DECLARE @ped1 INT = SCOPE_IDENTITY();
    INSERT INTO PedidoPrenda (IdPedido, IdPrenda) VALUES (@ped1, @pr1), (@ped1, @pr2);

    -- Pedido 2: Martín — Despachado (1 prenda)
    INSERT INTO Pedido (IdCliente, IdEmpleado, Estado, FechaPedido, FechaDespacho, DVH)
    VALUES (@cMartin, @emp, 1, DATEADD(day,-5,GETDATE()), DATEADD(day,-3,GETDATE()), 0);
    DECLARE @ped2 INT = SCOPE_IDENTITY();
    INSERT INTO PedidoPrenda (IdPedido, IdPrenda) VALUES (@ped2, @pr3);

    -- Pedido 3: Sofía — Pendiente (1 prenda)
    INSERT INTO Pedido (IdCliente, IdEmpleado, Estado, FechaPedido, DVH)
    VALUES (@cSofia, @emp, 0, DATEADD(day,-1,GETDATE()), 0);
    DECLARE @ped3 INT = SCOPE_IDENTITY();
    INSERT INTO PedidoPrenda (IdPedido, IdPrenda) VALUES (@ped3, @pr4);

    -- Marcar prendas EnUso (Estado=1) con su cliente
    UPDATE Prenda SET Estado=1, IdClienteActual=@cLucia  WHERE IdPrenda IN (@pr1,@pr2);
    UPDATE Prenda SET Estado=1, IdClienteActual=@cMartin WHERE IdPrenda=@pr3;
    UPDATE Prenda SET Estado=1, IdClienteActual=@cSofia  WHERE IdPrenda=@pr4;

    PRINT 'Demo: 3 pedidos (Entregado/Despachado/Pendiente) + prendas asignadas.';
END
ELSE
    PRINT 'Demo: pedidos ya existen o faltan datos base — sin cambios.';
GO

-- ============================================================
-- ÍNDICES NO-CLUSTERED Y RESTRICCIONES DE INTEGRIDAD
-- Idempotente: solo crea lo que falta. Mejoran los joins por FK y los filtros por fecha,
-- y el CHECK respalda en el motor la prohibición de auto-referencia del árbol Composite.
-- ============================================================

-- Índices sobre columnas FK que NO son la columna LÍDER de su PK (la PK ya indexa su primera
-- columna) y sobre columnas usadas para filtrar/ordenar (fechas).
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Bitacora_fecha' AND object_id = OBJECT_ID('Bitacora'))
    CREATE NONCLUSTERED INDEX IX_Bitacora_fecha ON Bitacora(fecha);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Bitacora_usuario' AND object_id = OBJECT_ID('Bitacora'))
    CREATE NONCLUSTERED INDEX IX_Bitacora_usuario ON Bitacora(usuario);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_BitacoraNegocio_Fecha' AND object_id = OBJECT_ID('BitacoraNegocio'))
    CREATE NONCLUSTERED INDEX IX_BitacoraNegocio_Fecha ON BitacoraNegocio(Fecha);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PermisoRelacion_IdHijo' AND object_id = OBJECT_ID('PermisoRelacion'))
    CREATE NONCLUSTERED INDEX IX_PermisoRelacion_IdHijo ON PermisoRelacion(IdHijo);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_RolPermiso_IdPermiso' AND object_id = OBJECT_ID('RolPermiso'))
    CREATE NONCLUSTERED INDEX IX_RolPermiso_IdPermiso ON RolPermiso(IdPermiso);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Traduccion_IdIdioma' AND object_id = OBJECT_ID('Traduccion'))
    CREATE NONCLUSTERED INDEX IX_Traduccion_IdIdioma ON Traduccion(IdIdioma);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Pedido_IdCliente' AND object_id = OBJECT_ID('Pedido'))
    CREATE NONCLUSTERED INDEX IX_Pedido_IdCliente ON Pedido(IdCliente);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Pedido_IdEmpleado' AND object_id = OBJECT_ID('Pedido'))
    CREATE NONCLUSTERED INDEX IX_Pedido_IdEmpleado ON Pedido(IdEmpleado);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Pedido_FechaPedido' AND object_id = OBJECT_ID('Pedido'))
    CREATE NONCLUSTERED INDEX IX_Pedido_FechaPedido ON Pedido(FechaPedido);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PedidoPrenda_IdPrenda' AND object_id = OBJECT_ID('PedidoPrenda'))
    CREATE NONCLUSTERED INDEX IX_PedidoPrenda_IdPrenda ON PedidoPrenda(IdPrenda);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PedidoHistorial_IdPedido' AND object_id = OBJECT_ID('PedidoHistorial'))
    CREATE NONCLUSTERED INDEX IX_PedidoHistorial_IdPedido ON PedidoHistorial(IdPedido);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Prenda_IdClienteActual' AND object_id = OBJECT_ID('Prenda'))
    CREATE NONCLUSTERED INDEX IX_Prenda_IdClienteActual ON Prenda(IdClienteActual);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_HistorialUsuario_IdUsuario' AND object_id = OBJECT_ID('HistorialUsuario'))
    CREATE NONCLUSTERED INDEX IX_HistorialUsuario_IdUsuario ON HistorialUsuario(IdUsuario);
PRINT 'Índices no-clustered verificados/creados.';
GO

-- CHECK anti auto-referencia del árbol Composite: un permiso no puede ser su propio hijo.
-- (La validación de ciclos completa vive en BE.Familia; esto la respalda en el motor por si
-- alguien escribe por SQL directo.) Se limpian filas inválidas preexistentes para que no falle.
IF EXISTS (SELECT 1 FROM PermisoRelacion WHERE IdPadre = IdHijo)
BEGIN
    DELETE FROM PermisoRelacion WHERE IdPadre = IdHijo;
    PRINT 'PermisoRelacion: filas auto-referenciadas (IdPadre = IdHijo) eliminadas.';
END
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_PermisoRelacion_NoAutoref')
BEGIN
    ALTER TABLE PermisoRelacion ADD CONSTRAINT CK_PermisoRelacion_NoAutoref CHECK (IdPadre <> IdHijo);
    PRINT 'CHECK CK_PermisoRelacion_NoAutoref agregado (IdPadre <> IdHijo).';
END
ELSE
    PRINT 'CHECK CK_PermisoRelacion_NoAutoref ya existe — sin cambios.';
GO

PRINT '';
PRINT '=== WardrobeFlowDB deploy completo. ===';
PRINT 'IMPORTANTE: Ejecutar recálculo de DVH/DVV desde la aplicación';
PRINT '            antes del primer uso (Administrar → Usuarios → Recalcular DV).';
PRINT 'Las traducciones se seedean automáticamente en el primer uso de la app.';
GO
