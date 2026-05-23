-- ============================================================
-- WardrobeFlow — Script de migración v3.0
-- T07 Dígitos Verificadores (DVH horizontal + DVV vertical)
-- Aplicar sobre WardrobeFlowDB ANTES de iniciar la aplicación.
-- ============================================================

USE WardrobeFlowDB;
GO

-- ── 1. Agregar columna DVH a la tabla Usuario ────────────────
-- DVH: dígito verificador horizontal por fila.
-- Calculado como: suma(ASCII(char_i) × posición_i) % 10
-- sobre los campos: IdUsuario, Username, Clave, Perfil, Estado, IntentosFallidos.
-- Se actualiza automáticamente en cada escritura (Alta, Desbloquear, ResetearClave).

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Usuario' AND COLUMN_NAME = 'DVH'
)
BEGIN
    ALTER TABLE Usuario ADD DVH INT NULL;
    PRINT 'Columna DVH agregada a Usuario.';
END
ELSE
    PRINT 'Columna DVH ya existe en Usuario — sin cambios.';
GO

-- ── 2. Crear tabla DVVertical ────────────────────────────────
-- DVV: dígito verificador vertical por tabla.
-- Calculado como: suma(DVH_i × posición_i) % 10
-- sobre todas las filas de la tabla, ordenadas por clave primaria.
-- Detecta inserciones, eliminaciones y permutas de filas completas.

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_NAME = 'DVVertical'
)
BEGIN
    CREATE TABLE DVVertical (
        Id           INT          IDENTITY(1,1) PRIMARY KEY,
        NombreTabla  VARCHAR(100) NOT NULL UNIQUE,
        DVV          INT          NOT NULL,
        FechaCalculo DATETIME     NOT NULL DEFAULT GETDATE()
    );
    PRINT 'Tabla DVVertical creada.';
END
ELSE
    PRINT 'Tabla DVVertical ya existe — sin cambios.';
GO

-- ── 3. Inicializar DVH en cero para filas existentes ────────
-- La aplicación recalculará los valores reales en el próximo inicio
-- una vez que el Administrador ejecute la recarga de DVH/DVV.
-- (Los valores NULL se tratan como discrepancias en la verificación de inicio.)

UPDATE Usuario SET DVH = 0 WHERE DVH IS NULL;
PRINT 'DVH inicializado en 0 para filas existentes sin valor.';
GO

-- ── 4. Insertar DVV inicial en cero para la tabla Usuario ───
IF NOT EXISTS (SELECT 1 FROM DVVertical WHERE NombreTabla = 'Usuario')
BEGIN
    INSERT INTO DVVertical (NombreTabla, DVV, FechaCalculo)
    VALUES ('Usuario', 0, GETDATE());
    PRINT 'DVV inicial insertado para tabla Usuario.';
END
GO

PRINT '=== Migración v3.0 completada. ===';
PRINT 'IMPORTANTE: Ejecutar recálculo de DVH/DVV desde la aplicación';
PRINT '            (Administrar → Usuarios → Recalcular DV) antes del primer uso.';
GO
