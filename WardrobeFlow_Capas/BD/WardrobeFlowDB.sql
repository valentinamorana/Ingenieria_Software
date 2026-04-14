-- ============================================================
-- WardrobeFlow — Script de base de datos
-- Motor : SQL Server (Express o superior)
-- Instancia: .\SQLEXPRESS
-- ============================================================

USE master;
GO

IF DB_ID('WardrobeFlowDB') IS NOT NULL
BEGIN
    ALTER DATABASE WardrobeFlowDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE WardrobeFlowDB;
END
GO

CREATE DATABASE WardrobeFlowDB;
GO

USE WardrobeFlowDB;
GO

-- ============================================================
-- TABLAS
-- ============================================================

-- Persona (superentidad de Usuario)
CREATE TABLE Persona (
    IdPersona      INT IDENTITY(1,1) PRIMARY KEY,
    NombreCompleto VARCHAR(120) NOT NULL,
    Correo         VARCHAR(100) NOT NULL,
    Documento      VARCHAR(20)  NOT NULL UNIQUE
);
GO

-- Usuario
CREATE TABLE Usuario (
    IdUsuario  INT IDENTITY(1,1) PRIMARY KEY,
    IdPersona  INT          NOT NULL REFERENCES Persona(IdPersona),
    Clave      VARCHAR(200) NOT NULL,   -- SHA256 + salt  (hash|salt)
    Rol        VARCHAR(30)  NOT NULL,   -- Administrador / Empleado / Usuario
    Estado     BIT          NOT NULL DEFAULT 1
);
GO

-- Permiso  (hoja del arbol Composite)
-- TipoComponente agrupa permisos por modulo (ej: "Prendas", "Outfits", "Sistema")
CREATE TABLE Permiso (
    IdPermiso       INT IDENTITY(1,1) PRIMARY KEY,
    Nombre          VARCHAR(80)  NOT NULL,
    NombreMenu      VARCHAR(80)  NOT NULL,   -- nombre del item de menu en el Designer
    TipoComponente  VARCHAR(50)  NOT NULL DEFAULT 'General',
    Estado          BIT          NOT NULL DEFAULT 1
);
GO

-- Asignacion de permisos por usuario
CREATE TABLE UsuarioPermiso (
    IdUsuario  INT NOT NULL REFERENCES Usuario(IdUsuario),
    IdPermiso  INT NOT NULL REFERENCES Permiso(IdPermiso),
    PRIMARY KEY (IdUsuario, IdPermiso)
);
GO

-- Bitacora de sesiones (T06a)
CREATE TABLE AuditoriaSesion (
    IdAuditoria          INT IDENTITY(1,1) PRIMARY KEY,
    IdUsuario            INT          NOT NULL REFERENCES Usuario(IdUsuario),
    DescripcionAuditoria VARCHAR(200) NOT NULL,
    FechaHora            DATETIME     NOT NULL DEFAULT GETDATE()
);
GO

-- Categoria de prenda
CREATE TABLE Categoria (
    IdCategoria INT IDENTITY(1,1) PRIMARY KEY,
    Nombre      VARCHAR(80)  NOT NULL UNIQUE,
    Descripcion VARCHAR(200),
    Estado      BIT          NOT NULL DEFAULT 1
);
GO

-- Prenda del guardarropa
CREATE TABLE Prenda (
    IdPrenda      INT IDENTITY(1,1) PRIMARY KEY,
    Nombre        VARCHAR(120) NOT NULL,
    Color         VARCHAR(50)  NOT NULL,
    Talla         VARCHAR(10)  NOT NULL,
    Temporada     VARCHAR(20)  NOT NULL,   -- Verano/Otoño/Invierno/Primavera
    IdCategoria   INT          NOT NULL REFERENCES Categoria(IdCategoria),
    Estado        BIT          NOT NULL DEFAULT 1,
    FechaRegistro DATETIME     NOT NULL DEFAULT GETDATE()
);
GO

-- Outfit (combinacion de prendas)
CREATE TABLE Outfit (
    IdOutfit      INT IDENTITY(1,1) PRIMARY KEY,
    Nombre        VARCHAR(120) NOT NULL,
    Descripcion   VARCHAR(300),
    Ocasion       VARCHAR(50)  NOT NULL,   -- Casual/Formal/Deportivo/Fiesta/Trabajo
    Temporada     VARCHAR(20)  NOT NULL,
    Estado        BIT          NOT NULL DEFAULT 1,
    FechaCreacion DATETIME     NOT NULL DEFAULT GETDATE(),
    IdUsuario     INT          NOT NULL REFERENCES Usuario(IdUsuario)
);
GO

-- Detalle: prendas que conforman cada outfit
CREATE TABLE DetalleOutfit (
    IdDetalle INT IDENTITY(1,1) PRIMARY KEY,
    IdOutfit  INT NOT NULL REFERENCES Outfit(IdOutfit)  ON DELETE CASCADE,
    IdPrenda  INT NOT NULL REFERENCES Prenda(IdPrenda),
    UNIQUE (IdOutfit, IdPrenda)
);
GO

-- ============================================================
-- STORED PROCEDURES — Usuario
-- ============================================================

CREATE PROCEDURE SP_RegistrarUsuario
    @NombreCompleto VARCHAR(120),
    @Correo         VARCHAR(100),
    @Documento      VARCHAR(20),
    @Clave          VARCHAR(200),
    @Rol            VARCHAR(30),
    @Mensaje        VARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM Persona WHERE Documento = @Documento)
    BEGIN
        SET @Mensaje = 'Ya existe un usuario con ese documento.';
        RETURN;
    END
    DECLARE @IdPersona INT;
    INSERT INTO Persona (NombreCompleto, Correo, Documento)
    VALUES (@NombreCompleto, @Correo, @Documento);
    SET @IdPersona = SCOPE_IDENTITY();
    INSERT INTO Usuario (IdPersona, Clave, Rol, Estado)
    VALUES (@IdPersona, @Clave, @Rol, 1);
    SET @Mensaje = 'Usuario registrado correctamente.';
END
GO

CREATE PROCEDURE SP_EditarUsuario
    @IdUsuario      INT,
    @NombreCompleto VARCHAR(120),
    @Correo         VARCHAR(100),
    @Rol            VARCHAR(30),
    @Estado         BIT,
    @Mensaje        VARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @IdPersona INT;
    SELECT @IdPersona = IdPersona FROM Usuario WHERE IdUsuario = @IdUsuario;
    UPDATE Persona SET NombreCompleto = @NombreCompleto, Correo = @Correo
    WHERE IdPersona = @IdPersona;
    UPDATE Usuario SET Rol = @Rol, Estado = @Estado
    WHERE IdUsuario = @IdUsuario;
    SET @Mensaje = 'Usuario actualizado correctamente.';
END
GO

CREATE PROCEDURE SP_EliminarUsuario
    @IdUsuario INT,
    @Mensaje   VARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM Outfit WHERE IdUsuario = @IdUsuario)
    BEGIN
        SET @Mensaje = 'No se puede eliminar: el usuario tiene outfits asociados.';
        RETURN;
    END
    DECLARE @IdPersona INT;
    SELECT @IdPersona = IdPersona FROM Usuario WHERE IdUsuario = @IdUsuario;
    DELETE FROM UsuarioPermiso WHERE IdUsuario = @IdUsuario;
    DELETE FROM AuditoriaSesion WHERE IdUsuario = @IdUsuario;
    DELETE FROM Usuario WHERE IdUsuario = @IdUsuario;
    DELETE FROM Persona WHERE IdPersona = @IdPersona;
    SET @Mensaje = 'Usuario eliminado correctamente.';
END
GO

-- ============================================================
-- STORED PROCEDURES — Categoria
-- ============================================================

CREATE PROCEDURE SP_AgregarCategoria
    @Nombre      VARCHAR(80),
    @Descripcion VARCHAR(200),
    @Mensaje     VARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM Categoria WHERE Nombre = @Nombre)
    BEGIN
        SET @Mensaje = 'Ya existe una categoria con ese nombre.';
        RETURN;
    END
    INSERT INTO Categoria (Nombre, Descripcion, Estado) VALUES (@Nombre, @Descripcion, 1);
    SET @Mensaje = 'Categoria agregada correctamente.';
END
GO

CREATE PROCEDURE SP_EditarCategoria
    @IdCategoria INT,
    @Nombre      VARCHAR(80),
    @Descripcion VARCHAR(200),
    @Estado      BIT,
    @Mensaje     VARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM Categoria WHERE Nombre = @Nombre AND IdCategoria <> @IdCategoria)
    BEGIN
        SET @Mensaje = 'Ya existe otra categoria con ese nombre.';
        RETURN;
    END
    UPDATE Categoria
    SET Nombre = @Nombre, Descripcion = @Descripcion, Estado = @Estado
    WHERE IdCategoria = @IdCategoria;
    SET @Mensaje = 'Categoria actualizada correctamente.';
END
GO

CREATE PROCEDURE SP_EliminarCategoria
    @IdCategoria INT,
    @Mensaje     VARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM Prenda WHERE IdCategoria = @IdCategoria)
    BEGIN
        SET @Mensaje = 'No se puede eliminar: la categoria tiene prendas asociadas.';
        RETURN;
    END
    DELETE FROM Categoria WHERE IdCategoria = @IdCategoria;
    SET @Mensaje = 'Categoria eliminada correctamente.';
END
GO

-- ============================================================
-- STORED PROCEDURES — Prenda
-- ============================================================

CREATE PROCEDURE SP_AgregarPrenda
    @Nombre      VARCHAR(120),
    @Color       VARCHAR(50),
    @Talla       VARCHAR(10),
    @Temporada   VARCHAR(20),
    @IdCategoria INT,
    @Mensaje     VARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Prenda (Nombre, Color, Talla, Temporada, IdCategoria, Estado, FechaRegistro)
    VALUES (@Nombre, @Color, @Talla, @Temporada, @IdCategoria, 1, GETDATE());
    SET @Mensaje = 'Prenda agregada correctamente.';
END
GO

CREATE PROCEDURE SP_EditarPrenda
    @IdPrenda    INT,
    @Nombre      VARCHAR(120),
    @Color       VARCHAR(50),
    @Talla       VARCHAR(10),
    @Temporada   VARCHAR(20),
    @IdCategoria INT,
    @Estado      BIT,
    @Mensaje     VARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Prenda
    SET Nombre = @Nombre, Color = @Color, Talla = @Talla,
        Temporada = @Temporada, IdCategoria = @IdCategoria, Estado = @Estado
    WHERE IdPrenda = @IdPrenda;
    SET @Mensaje = 'Prenda actualizada correctamente.';
END
GO

CREATE PROCEDURE SP_EliminarPrenda
    @IdPrenda INT,
    @Mensaje  VARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM DetalleOutfit WHERE IdPrenda = @IdPrenda)
    BEGIN
        SET @Mensaje = 'No se puede eliminar: la prenda pertenece a uno o mas outfits.';
        RETURN;
    END
    DELETE FROM Prenda WHERE IdPrenda = @IdPrenda;
    SET @Mensaje = 'Prenda eliminada correctamente.';
END
GO

-- ============================================================
-- STORED PROCEDURES — Outfit
-- ============================================================

CREATE PROCEDURE SP_AgregarOutfit
    @Nombre           VARCHAR(120),
    @Descripcion      VARCHAR(300),
    @Ocasion          VARCHAR(50),
    @Temporada        VARCHAR(20),
    @IdUsuario        INT,
    @IdOutfitGenerado INT          OUTPUT,
    @Mensaje          VARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Outfit (Nombre, Descripcion, Ocasion, Temporada, Estado, FechaCreacion, IdUsuario)
    VALUES (@Nombre, @Descripcion, @Ocasion, @Temporada, 1, GETDATE(), @IdUsuario);
    SET @IdOutfitGenerado = SCOPE_IDENTITY();
    SET @Mensaje = 'Outfit creado correctamente.';
END
GO

CREATE PROCEDURE SP_AgregarDetalleOutfit
    @IdOutfit INT,
    @IdPrenda INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM DetalleOutfit WHERE IdOutfit = @IdOutfit AND IdPrenda = @IdPrenda)
        INSERT INTO DetalleOutfit (IdOutfit, IdPrenda) VALUES (@IdOutfit, @IdPrenda);
END
GO

CREATE PROCEDURE SP_EditarOutfit
    @IdOutfit    INT,
    @Nombre      VARCHAR(120),
    @Descripcion VARCHAR(300),
    @Ocasion     VARCHAR(50),
    @Temporada   VARCHAR(20),
    @Estado      BIT,
    @Mensaje     VARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Outfit
    SET Nombre = @Nombre, Descripcion = @Descripcion, Ocasion = @Ocasion,
        Temporada = @Temporada, Estado = @Estado
    WHERE IdOutfit = @IdOutfit;
    SET @Mensaje = 'Outfit actualizado correctamente.';
END
GO

CREATE PROCEDURE SP_EliminarOutfit
    @IdOutfit INT,
    @Mensaje  VARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    -- Los detalles se eliminan en cascada (ON DELETE CASCADE en DetalleOutfit)
    DELETE FROM Outfit WHERE IdOutfit = @IdOutfit;
    SET @Mensaje = 'Outfit eliminado correctamente.';
END
GO

-- ============================================================
-- STORED PROCEDURE — Auditoria
-- ============================================================

CREATE PROCEDURE SP_RegistrarAuditoriaSesion
    @IdUsuario            INT,
    @DescripcionAuditoria VARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO AuditoriaSesion (IdUsuario, DescripcionAuditoria, FechaHora)
    VALUES (@IdUsuario, @DescripcionAuditoria, GETDATE());
END
GO

-- ============================================================
-- DATOS INICIALES
-- ============================================================

-- Categorias de ejemplo
INSERT INTO Categoria (Nombre, Descripcion, Estado) VALUES
    ('Camisas',   'Camisas y blusas',             1),
    ('Pantalones','Pantalones y jeans',            1),
    ('Vestidos',  'Vestidos y faldas',             1),
    ('Calzado',   'Zapatos, zapatillas y botas',   1),
    ('Accesorios','Cinturones, bolsos y bijouterie',1);
GO

-- Permisos del sistema  (TipoComponente = modulo del Composite)
INSERT INTO Permiso (Nombre, NombreMenu, TipoComponente, Estado) VALUES
    ('Ver Prendas',   'mnuPrendas',     'Inventario', 1),
    ('Ver Outfits',   'mnuOutfits',     'Inventario', 1),
    ('Ver Categorias','mnuCategorias',  'Inventario', 1),
    ('Ver Usuarios',  'mnuUsuarios',    'Sistema',    1),
    ('Ver Permisos',  'mnuPermisos',    'Sistema',    1),
    ('Ver Auditoria', 'mnuAuditoria',   'Sistema',    1);
GO

-- Usuario Administrador inicial  (clave: Admin123  — debera cambiarse en primer login)
-- Hash generado por BE.Usuario.GenerarClaveHash("Admin123")
-- Se inserta con hash de ejemplo; en produccion usar la app para registrar correctamente
INSERT INTO Persona (NombreCompleto, Correo, Documento)
VALUES ('Administrador', 'admin@wardrobeflow.com', '00000000');

INSERT INTO Usuario (IdPersona, Clave, Rol, Estado)
VALUES (
    (SELECT IdPersona FROM Persona WHERE Documento = '00000000'),
    'CAMBIAR_EJECUTAR_APP_PRIMERO|salt',   -- Ejecutar SP_RegistrarUsuario desde la app
    'Administrador',
    1
);
GO

-- Asignar todos los permisos al administrador
INSERT INTO UsuarioPermiso (IdUsuario, IdPermiso)
SELECT
    (SELECT IdUsuario FROM Usuario u
     INNER JOIN Persona p ON u.IdPersona = p.IdPersona
     WHERE p.Documento = '00000000'),
    IdPermiso
FROM Permiso;
GO

PRINT 'Base de datos WardrobeFlowDB creada correctamente.';
GO
