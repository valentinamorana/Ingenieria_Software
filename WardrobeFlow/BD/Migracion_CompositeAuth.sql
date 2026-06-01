/* ============================================================================
 * MIGRACIÓN — T04: Composite como MOTOR REAL de autorización
 * ----------------------------------------------------------------------------
 * Objetivo: que los Roles dejen de ser un string suelto y pasen a ser NODOS
 *           del árbol Composite. A partir de esta migración, [PermisoRelacion]
 *           es la ÚNICA fuente de verdad de la composición de permisos
 *           (rol→rol, rol→familia, rol→patente, familia→familia, familia→patente).
 *
 * La resolución de permisos efectivos del usuario se hace recorriendo este
 * árbol de forma RECURSIVA (ver BLL.Permiso.ObtenerPermisosEfectivos).
 *
 * Idempotente: se puede ejecutar varias veces sin duplicar datos.
 * Ejecutar sobre WardrobeFlowDB.
 * ==========================================================================*/

-- 1) Discriminador EsRol en Permiso ------------------------------------------
--    Un nodo del árbol puede ser: Patente (hoja), Familia (compuesto) o
--    Rol (compuesto asignable a un Usuario). EsRol=1 marca los nodos-rol.
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'Permiso' AND COLUMN_NAME = 'EsRol')
BEGIN
    ALTER TABLE Permiso ADD EsRol BIT NOT NULL DEFAULT 0;
    PRINT 'Columna EsRol agregada a Permiso.';
END
ELSE
    PRINT 'Columna EsRol ya existe — sin cambios.';
GO

-- 2) Crear un nodo-Rol por cada rol existente en [RolPermiso] -----------------
--    El Nombre del nodo-rol debe coincidir con Usuario.Rol / RolPermiso.Rol
--    para que el login resuelva sus permisos efectivos.
INSERT INTO Permiso (Nombre, NombreMenu, TipoComponente, Estado, EsFamilia, EsRol)
SELECT DISTINCT rp.Rol, NULL, 'Rol', 1, 1, 1
FROM   RolPermiso rp
WHERE  NOT EXISTS (
    SELECT 1 FROM Permiso p
    WHERE  p.Nombre = rp.Rol AND p.EsRol = 1
);
PRINT 'Nodos-rol creados desde RolPermiso (los que faltaban).';
GO

-- 3) Migrar asignaciones planas [RolPermiso] → [PermisoRelacion] --------------
--    Cada (Rol, IdPermiso) se convierte en una arista nodoRol → permiso.
INSERT INTO PermisoRelacion (IdPadre, IdHijo)
SELECT pr.IdRol, rp.IdPermiso
FROM   RolPermiso rp
INNER JOIN Permiso pr ON pr.Nombre = rp.Rol AND pr.EsRol = 1
                     AND pr.IdPermiso <> rp.IdPermiso          -- evita auto-arista
WHERE  NOT EXISTS (
    SELECT 1 FROM PermisoRelacion x
    WHERE  x.IdPadre = pr.IdRol AND x.IdHijo = rp.IdPermiso
);
PRINT 'Asignaciones RolPermiso migradas a PermisoRelacion.';
GO

/* Nota: la tabla [RolPermiso] se conserva (no se elimina) por seguridad de
 * rollback y trazabilidad. El código ya NO la usa para resolver permisos;
 * la composición vive 100% en [PermisoRelacion]. Puede eliminarse en una
 * limpieza posterior una vez validada la migración. */

PRINT '=== Migración Composite completada ===';
GO
