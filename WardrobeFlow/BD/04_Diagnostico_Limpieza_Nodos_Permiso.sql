-- ============================================================
-- WardrobeFlow — 04. DIAGNÓSTICO Y LIMPIEZA DE NODOS DEL ÁRBOL DE PERMISOS
-- ------------------------------------------------------------
-- Sirve para detectar y borrar nodos "de prueba" insertados a mano en la tabla
-- [Permiso] (por ejemplo 'prueba4'), que aparecen en el árbol de Roles y Permisos
-- "bloqueados": la app no los deja gestionar porque NO son roles (EsRol = 0), y los
-- permisos (patentes) son un catálogo FIJO del sistema.
--
-- Recordá:
--   • ROL      → EsRol = 1               (editable desde la app)
--   • FAMILIA  → EsFamilia = 1, EsRol=0  (retirada del modelo, catálogo)
--   • PATENTE  → EsRol = 0, EsFamilia=0  (permiso atómico; catálogo fijo, no editable)
--
-- La tabla [Permiso] NO tiene dígito verificador, así que borrar filas acá no rompe el DV.
--
-- USO: ejecutar por PARTES en SSMS. La PARTE A no modifica nada.
-- ============================================================

USE WardrobeFlowDB;
GO

-- ─────────────────────────────────────────────────────────────
-- PARTE A — DIAGNÓSTICO (solo lectura, no cambia nada)
-- ─────────────────────────────────────────────────────────────

-- A1. Todos los nodos activos del árbol, clasificados por tipo.
SELECT
    IdPermiso,
    Nombre,
    NombreMenu,
    Estado,
    CASE
        WHEN ISNULL(EsRol,0)     = 1 THEN 'ROL'
        WHEN ISNULL(EsFamilia,0) = 1 THEN 'FAMILIA'
        ELSE 'PATENTE (permiso)'
    END AS Tipo
FROM Permiso
ORDER BY Tipo, Nombre;

-- A2. Nodos SOSPECHOSOS (probables inserciones manuales / de prueba):
--     patentes que NO pertenecen al catálogo fijo. Una patente real SIEMPRE tiene
--     un NombreMenu tipo 'mnuXxx' (mnuUsuarios, mnuPrendas, ...). Si no lo tiene,
--     casi seguro es basura de prueba (como 'prueba4').
SELECT
    p.IdPermiso, p.Nombre, p.NombreMenu, p.EsFamilia, p.EsRol, p.Estado,
    (SELECT COUNT(*) FROM PermisoRelacion r WHERE r.IdHijo  = p.IdPermiso) AS VecesComoHijo,
    (SELECT COUNT(*) FROM PermisoRelacion r WHERE r.IdPadre = p.IdPermiso) AS VecesComoPadre
FROM Permiso p
WHERE ISNULL(p.EsRol,0) = 0
  AND ISNULL(p.EsFamilia,0) = 0
  AND (p.NombreMenu IS NULL OR p.NombreMenu NOT LIKE 'mnu%')
ORDER BY p.Nombre;
GO

-- ─────────────────────────────────────────────────────────────
-- PARTE B — LIMPIEZA de un nodo puntual por nombre
-- ------------------------------------------------------------
-- Cambiá @Nombre por el nodo que querés eliminar. Se ejecuta dentro de una
-- transacción con validaciones de seguridad:
--   • NO borra un ROL que tenga usuarios asignados (avisa y hace ROLLBACK).
--   • NO borra una PATENTE real del catálogo fijo (NombreMenu 'mnu...').
--   • Borra primero las relaciones (aristas) y después el nodo.
-- ─────────────────────────────────────────────────────────────
DECLARE @Nombre NVARCHAR(100) = N'prueba4';   -- << nodo a limpiar

BEGIN TRAN;

DECLARE @Id INT, @EsRol BIT, @EsFamilia BIT, @NombreMenu NVARCHAR(100);
SELECT @Id         = IdPermiso,
       @EsRol      = ISNULL(EsRol,0),
       @EsFamilia  = ISNULL(EsFamilia,0),
       @NombreMenu = NombreMenu
FROM Permiso
WHERE Nombre = @Nombre;

IF @Id IS NULL
BEGIN
    PRINT 'No existe ningún nodo llamado "' + @Nombre + '". Nada que hacer.';
    ROLLBACK TRAN;
END
ELSE IF @EsRol = 1 AND EXISTS (SELECT 1 FROM Usuario WHERE Rol = @Nombre OR Perfil = @Nombre)
BEGIN
    PRINT 'ABORTADO: "' + @Nombre + '" es un ROL asignado a usuarios. Reasigná esos usuarios a otro rol antes de borrarlo.';
    ROLLBACK TRAN;
END
ELSE IF @EsRol = 0 AND @EsFamilia = 0 AND @NombreMenu LIKE 'mnu%'
BEGIN
    PRINT 'ABORTADO: "' + @Nombre + '" es una PATENTE real del catálogo fijo (' + @NombreMenu + '). No se borra.';
    ROLLBACK TRAN;
END
ELSE
BEGIN
    DELETE FROM PermisoRelacion WHERE IdPadre = @Id OR IdHijo = @Id;
    DECLARE @rels INT = @@ROWCOUNT;

    DELETE FROM Permiso WHERE IdPermiso = @Id;

    PRINT 'OK: nodo "' + @Nombre + '" (Id ' + CAST(@Id AS VARCHAR(10)) + ') eliminado. '
        + 'Relaciones borradas: ' + CAST(@rels AS VARCHAR(10)) + '.';
    COMMIT TRAN;
END
GO
