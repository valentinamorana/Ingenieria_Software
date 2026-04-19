-- ============================================================
-- WardrobeFlowDB — Script de adaptación para coincidir con el código
-- Ejecutar en SQL Server Management Studio sobre WardrobeFlowDB
-- ============================================================

USE WardrobeFlowDB;
GO

-- ============================================================
-- 1. TABLA Bitacora (nueva)
--    El código usa esta tabla para auditoría de sesión.
--    Se crea junto a AuditoriaSesion que ya existe — no se toca.
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Bitacora')
BEGIN
    CREATE TABLE Bitacora (
        Id          INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
        fecha       DATETIME      NOT NULL DEFAULT GETDATE(),
        usuario     INT           NOT NULL,  -- FK → Usuario.IdUsuario
        modulo      VARCHAR(100)  NOT NULL,
        actividad   VARCHAR(100)  NOT NULL,
        detalle     VARCHAR(500)  NOT NULL,
        criticidad  INT           NOT NULL DEFAULT 0,
        CONSTRAINT FK_Bitacora_Usuario FOREIGN KEY (usuario) REFERENCES Usuario(IdUsuario)
    );
    PRINT 'Tabla Bitacora creada correctamente.';
END
ELSE
    PRINT 'La tabla Bitacora ya existe — no se modificó.';
GO

-- ============================================================
-- 2. Agregar columna Perfil a Usuario (si no existe)
--    El código usa Perfil para el rol del empleado.
-- ============================================================
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Usuario' AND COLUMN_NAME = 'Perfil'
)
BEGIN
    ALTER TABLE Usuario ADD Perfil VARCHAR(100) NULL;
    PRINT 'Columna Perfil agregada a Usuario.';
END
ELSE
    PRINT 'La columna Perfil ya existe en Usuario — no se modificó.';
GO

-- ============================================================
-- 3. Agregar columna Username a Usuario (si no existe)
--    IMPORTANTE: el GO después del ALTER TABLE es necesario para que
--    SQL Server confirme el cambio antes de parsear el INSERT que
--    referencia la columna nueva.
-- ============================================================
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Usuario' AND COLUMN_NAME = 'Username'
)
BEGIN
    ALTER TABLE Usuario ADD Username VARCHAR(50) NULL;
    PRINT 'Columna Username agregada a Usuario.';
END
ELSE
    PRINT 'La columna Username ya existe en Usuario — no se modificó.';
GO

-- ============================================================
-- 4. Usuario de prueba para login
--    Contraseña: admin123  (hash PBKDF2 SHA1, 100.000 iteraciones)
--    Solo se inserta si no existe ya un usuario con username 'admin'.
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM Usuario WHERE Username = 'admin')
BEGIN
    DECLARE @IdPersona INT;

    -- Crear la persona asociada si tampoco existe
    IF NOT EXISTS (SELECT 1 FROM Persona WHERE Correo = 'admin@wardrobeflow.com')
    BEGIN
        INSERT INTO Persona (NombreCompleto, Correo, Documento)
        VALUES ('Administrador', 'admin@wardrobeflow.com', '00000000');
        SET @IdPersona = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        SELECT @IdPersona = IdPersona FROM Persona WHERE Correo = 'admin@wardrobeflow.com';
    END

    INSERT INTO Usuario (IdPersona, Username, Clave, Rol, Estado, Perfil)
    VALUES (@IdPersona, 'admin',
            '2vLSvett1c1xKT6EjagpkE4u1tAhmv5LcjSeHbKA86fRWTXy',
            'Administrador', 1, 'Administrador');

    PRINT 'Usuario admin creado. Contraseña: admin123';
END
ELSE
    PRINT 'El usuario admin ya existe — no se modificó.';
GO

-- ============================================================
-- 5. Verificación final
-- ============================================================
SELECT 'Bitacora' AS Tabla, COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Bitacora'
UNION ALL
SELECT 'Usuario', COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Usuario'
ORDER BY Tabla, COLUMN_NAME;
GO

PRINT '=== Script ejecutado correctamente ===';
