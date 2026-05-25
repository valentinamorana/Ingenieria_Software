-- ============================================================
-- WardrobeFlow — Migración v6.0
-- T06 Historial de cambios de usuarios (patrón Memento/Versión)
-- T04 Composite relacional de permisos (EsFamilia + PermisoRelacion)
--
-- Aplicar sobre WardrobeFlowDB después de v5.0.
-- Todos los bloques son idempotentes (IF NOT EXISTS).
-- ============================================================

USE WardrobeFlowDB;
GO

-- ============================================================
-- PARTE 1 — T06: Historial de cambios de usuarios
-- Guarda un snapshot del usuario cada vez que el Administrador
-- modifica sus datos (alta, edicion, desbloqueo, reset de clave).
-- ============================================================

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_NAME = 'HistorialUsuario'
)
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

-- ============================================================
-- PARTE 2 — T04: Composite relacional de permisos
-- El árbol Familia → Patente se almacena en BD mediante:
--   - Permiso.EsFamilia BIT: discrimina nodo compuesto (1) de hoja (0)
--   - PermisoRelacion: tabla de relaciones padre → hijo
-- ============================================================

-- 2.1 Agregar columna EsFamilia a Permiso
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Permiso' AND COLUMN_NAME = 'EsFamilia'
)
BEGIN
    ALTER TABLE Permiso ADD EsFamilia BIT NOT NULL DEFAULT 0;
    PRINT 'Columna EsFamilia agregada a Permiso.';
END
ELSE
    PRINT 'Columna EsFamilia ya existe en Permiso — sin cambios.';
GO

-- 2.2 Crear tabla de relaciones padre → hijo
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_NAME = 'PermisoRelacion'
)
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

-- 2.3 Crear nodos Familia por cada grupo TipoComponente existente
-- Solo se ejecuta si no hay Familias cargadas aún.
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

    PRINT 'Nodos Familia creados desde grupos TipoComponente existentes.';

    -- 2.4 Vincular cada Patente con su Familia correspondiente
    INSERT INTO PermisoRelacion (IdPadre, IdHijo)
    SELECT m.IdFamilia, p.IdPermiso
    FROM   Permiso p
    INNER JOIN @mapa m ON p.TipoComponente = m.Grupo
    WHERE  p.EsFamilia = 0;

    PRINT 'Relaciones Familia → Patente insertadas en PermisoRelacion.';
END
ELSE
    PRINT 'Familias ya existentes — migración de árbol omitida.';
GO

PRINT '=== Migración v6.0 completada. ===';
PRINT 'T06: HistorialUsuario lista para registrar cambios de usuarios.';
PRINT 'T04: EsFamilia + PermisoRelacion listos para el arbol Composite.';
GO
