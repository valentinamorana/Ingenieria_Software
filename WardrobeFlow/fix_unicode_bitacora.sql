-- ============================================================
-- fix_unicode_bitacora.sql
-- Migra las columnas de texto de Bitacora y BitacoraNegocio
-- de VARCHAR a NVARCHAR para soportar caracteres con tilde,
-- ñ y cualquier caracter Unicode (ES/EN/RU).
--
-- Ejecutar UNA sola vez en WardrobeFlowDB.
-- Compatible con SQL Server 2016+ / SQL Server Express.
-- ============================================================

USE WardrobeFlowDB;
GO

-- ── Tabla Bitacora ──────────────────────────────────────────
--  Columnas afectadas: modulo, actividad, detalle, ip
--  Los datos existentes se conservan; solo cambia el tipo.

ALTER TABLE Bitacora ALTER COLUMN modulo    NVARCHAR(200);
ALTER TABLE Bitacora ALTER COLUMN actividad NVARCHAR(500);
ALTER TABLE Bitacora ALTER COLUMN detalle   NVARCHAR(MAX);
ALTER TABLE Bitacora ALTER COLUMN ip        NVARCHAR(50);

PRINT 'Bitacora: columnas migradas a NVARCHAR OK';
GO

-- ── Tabla BitacoraNegocio ────────────────────────────────────
--  Columna afectada: Descripcion  (Tipo es VARCHAR pero guarda
--  valores del enum, sin acentos — igual se migra por consistencia)

ALTER TABLE BitacoraNegocio ALTER COLUMN Descripcion NVARCHAR(MAX);

PRINT 'BitacoraNegocio: columna Descripcion migrada a NVARCHAR OK';
GO

PRINT '=== Migracion completada. Los nuevos registros ya no mostraran ???? ===';
GO
