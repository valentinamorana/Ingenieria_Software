-- ============================================================
-- WardrobeFlowDB — Parche: rellenar ip NULL en Bitácora
-- Solo toca los registros de seed data (ip IS NULL).
-- Les asigna la IP del servidor SQL actual como placeholder.
-- ============================================================

USE WardrobeFlowDB;
GO

-- Vista previa de cuántos registros se van a actualizar
SELECT COUNT(*) AS registros_sin_ip
FROM Bitacora
WHERE ip IS NULL;
GO

-- Actualizar: asigna la IP del servidor SQL como placeholder de "origen desconocido"
UPDATE Bitacora
SET    ip = CONVERT(VARCHAR(50),
               CONNECTIONPROPERTY('local_net_address'))
WHERE  ip IS NULL;

PRINT CONCAT(@@ROWCOUNT, ' registro(s) actualizados con IP del servidor.');
GO

-- Si CONNECTIONPROPERTY devuelve NULL (SQL Server local / named pipes),
-- caer a un valor por defecto:
UPDATE Bitacora
SET    ip = '127.0.0.1'
WHERE  ip IS NULL;

PRINT CONCAT(@@ROWCOUNT, ' registro(s) adicionales seteados a 127.0.0.1 (acceso local).');
GO

-- Verificación
SELECT 'Quedan sin IP' AS resultado, COUNT(*) AS total
FROM Bitacora
WHERE ip IS NULL;
GO
