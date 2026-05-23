USE WardrobeFlowDB;
GO

-- ============================================================
-- WardrobeFlow — Migración v5.0
-- Agrega soporte multiidioma gestionado desde la base de datos.
-- Tablas: Idioma, Control, Traduccion
-- ============================================================

-- 1. Tabla Idioma
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
END
GO

-- 2. Tabla Control (claves de traduccion)
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Control')
BEGIN
    CREATE TABLE Control (
        IdControl  INT IDENTITY(1,1) PRIMARY KEY,
        Clave      VARCHAR(100) NOT NULL,
        Formulario VARCHAR(50)  NOT NULL DEFAULT 'General',
        CONSTRAINT UQ_Control_Clave UNIQUE (Clave)
    );
END
GO

-- 3. Tabla Traduccion (PK compuesta idControl + idIdioma)
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Traduccion')
BEGIN
    CREATE TABLE Traduccion (
        IdControl  INT            NOT NULL,
        IdIdioma   INT            NOT NULL,
        Texto      NVARCHAR(1000) NOT NULL DEFAULT '',
        CONSTRAINT PK_Traduccion PRIMARY KEY (IdControl, IdIdioma),
        CONSTRAINT FK_Trad_Control FOREIGN KEY (IdControl) REFERENCES Control(IdControl),
        CONSTRAINT FK_Trad_Idioma  FOREIGN KEY (IdIdioma)  REFERENCES Idioma(IdIdioma)
    );
END
GO

-- 4. Seed Idiomas (idempotente)
INSERT INTO Idioma (Codigo, Nombre, Activo, EsDefault)
SELECT v.Codigo, v.Nombre, v.Activo, v.EsDefault
FROM (VALUES
    ('ES', N'Español', 1, 1),
    ('EN', N'English', 1, 0),
    ('RU', N'Русский', 1, 0)
) AS v(Codigo, Nombre, Activo, EsDefault)
WHERE NOT EXISTS (SELECT 1 FROM Idioma i WHERE i.Codigo = v.Codigo);
GO

-- ============================================================
-- Las filas de Control y Traduccion se crean automaticamente
-- en el primer uso desde BLL.Idioma.SeedearDesdeHardcode().
-- ============================================================
