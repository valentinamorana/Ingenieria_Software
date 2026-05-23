-- ============================================================
-- WardrobeFlow — Script de migración v4.0
-- T05 Multiidioma — preferencia de idioma por usuario
-- Aplicar sobre WardrobeFlowDB ANTES de iniciar la aplicación.
-- ============================================================

USE WardrobeFlowDB;
GO

-- ── 1. Agregar columna IdIdioma a la tabla Usuario ──────────
-- Almacena la preferencia de idioma de cada usuario ('ES', 'EN', 'RU').
-- Se actualiza cuando el usuario cambia el idioma desde el menú principal.
-- Se carga en sesión al hacer Login para restaurar la preferencia.

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Usuario' AND COLUMN_NAME = 'IdIdioma'
)
BEGIN
    ALTER TABLE Usuario ADD IdIdioma VARCHAR(5) NULL;
    PRINT 'Columna IdIdioma agregada a Usuario.';
END
ELSE
    PRINT 'Columna IdIdioma ya existe en Usuario — sin cambios.';
GO

-- ── 2. Inicializar a 'ES' (Español) para filas existentes ───
UPDATE Usuario SET IdIdioma = 'ES' WHERE IdIdioma IS NULL;
PRINT 'IdIdioma inicializado en ''ES'' para filas existentes.';
GO

PRINT '=== Migración v4.0 completada. ===';
PRINT 'Cada usuario puede cambiar su idioma desde el menú principal.';
PRINT 'La preferencia se persiste en la columna IdIdioma de la tabla Usuario.';
GO
