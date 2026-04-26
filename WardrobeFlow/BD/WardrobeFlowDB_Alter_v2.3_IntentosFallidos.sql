-- ============================================================
-- WardrobeFlow — Parche v2.3: IntentosFallidos en tabla Usuario
-- Aplica sobre: WardrobeFlowDB (cualquier instalación existente)
-- ============================================================

USE WardrobeFlowDB;
GO

-- Agregar columna IntentosFallidos si todavía no existe
IF NOT EXISTS (
    SELECT 1
    FROM   INFORMATION_SCHEMA.COLUMNS
    WHERE  TABLE_NAME   = 'Usuario'
      AND  COLUMN_NAME  = 'IntentosFallidos'
)
BEGIN
    ALTER TABLE Usuario
        ADD IntentosFallidos INT NOT NULL DEFAULT 0;

    PRINT 'Columna IntentosFallidos agregada a la tabla Usuario.';
END
ELSE
BEGIN
    PRINT 'La columna IntentosFallidos ya existe — no se realizaron cambios.';
END
GO

-- Asegurarse de que todos los registros existentes tengan valor 0
UPDATE Usuario
SET    IntentosFallidos = 0
WHERE  IntentosFallidos IS NULL;
GO
