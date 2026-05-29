-- Migracion_HistorialIntegridad.sql
-- Agregar tabla para historial de verificaciones de integridad DV (T07 Plus)
-- Ejecutar una sola vez sobre la BD WardrobeFlowDB

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'HistorialIntegridad')
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
    )

    PRINT 'Tabla HistorialIntegridad creada correctamente.'
END
ELSE
BEGIN
    PRINT 'La tabla HistorialIntegridad ya existe — sin cambios.'
END
