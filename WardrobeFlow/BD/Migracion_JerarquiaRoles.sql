-- ============================================================
-- Migracion_NuevosRoles.sql  (antes: Migracion_JerarquiaRoles.sql)
-- T04 — Semilla de permisos para roles nuevos del sistema
--
-- Ejecutar UNA VEZ sobre WardrobeFlowDB.
-- Idempotente: puede ejecutarse varias veces sin errores.
--
-- NO crea tablas nuevas. Solo inserta filas en [RolPermiso].
-- La tabla [RolJerarquia] ya NO se utiliza — fue eliminada del código.
--
-- Roles nuevos que se agregan:
--   Auditor         → Ver Auditoría (reemplaza Supervisor con nombre más específico)
--   GerenteComercial → Ver Prendas + Gestionar Clientes + Gestionar Planes + Pedidos Venta + Pedidos Realizados
--   GerenteInventario → Ver Prendas + Gestionar Stock + Ver Pedidos Realizados
--   EncargadoDeStock → igual que ControladorDeStock (prendas + stock)
--   OperadorLogistico → Ver Prendas + Ver Pedidos Realizados
--
-- Roles legacy conservados sin cambios:
--   Administrador, Supervisor, Vendedor, ControladorDeStock, OperadorDeInventario
-- ============================================================

USE WardrobeFlowDB;
GO

-- ============================================================
-- Auditor: mismos permisos que Supervisor (mnuAuditoria)
-- ============================================================
INSERT INTO RolPermiso (Rol, IdPermiso)
SELECT 'Auditor', rp.IdPermiso
FROM RolPermiso rp
WHERE rp.Rol = 'Supervisor'
  AND NOT EXISTS (
    SELECT 1 FROM RolPermiso x WHERE x.Rol = 'Auditor' AND x.IdPermiso = rp.IdPermiso
  );
PRINT 'Permisos Auditor sincronizados.';
GO

-- ============================================================
-- GerenteComercial: prendas + clientes + planes + pedidos venta + pedidos realizados
-- ============================================================
INSERT INTO RolPermiso (Rol, IdPermiso)
SELECT 'GerenteComercial', p.IdPermiso
FROM Permiso p
WHERE p.NombreMenu IN (
    'mnuPrendas',
    'mnuClientes',
    'mnuPlanSuscripciones',
    'mnuPedidosVenta',
    'mnuPedidosRealizados'
)
  AND ISNULL(p.EsFamilia, 0) = 0
  AND p.Estado = 1
  AND NOT EXISTS (
    SELECT 1 FROM RolPermiso x WHERE x.Rol = 'GerenteComercial' AND x.IdPermiso = p.IdPermiso
  );
PRINT 'Permisos GerenteComercial sincronizados.';
GO

-- ============================================================
-- GerenteInventario: prendas + stock + pedidos realizados
-- ============================================================
INSERT INTO RolPermiso (Rol, IdPermiso)
SELECT 'GerenteInventario', p.IdPermiso
FROM Permiso p
WHERE p.NombreMenu IN (
    'mnuPrendas',
    'mnuStock',
    'mnuPedidosRealizados'
)
  AND ISNULL(p.EsFamilia, 0) = 0
  AND p.Estado = 1
  AND NOT EXISTS (
    SELECT 1 FROM RolPermiso x WHERE x.Rol = 'GerenteInventario' AND x.IdPermiso = p.IdPermiso
  );
PRINT 'Permisos GerenteInventario sincronizados.';
GO

-- ============================================================
-- EncargadoDeStock: mismos permisos que ControladorDeStock
-- ============================================================
INSERT INTO RolPermiso (Rol, IdPermiso)
SELECT 'EncargadoDeStock', rp.IdPermiso
FROM RolPermiso rp
WHERE rp.Rol = 'ControladorDeStock'
  AND NOT EXISTS (
    SELECT 1 FROM RolPermiso x WHERE x.Rol = 'EncargadoDeStock' AND x.IdPermiso = rp.IdPermiso
  );
PRINT 'Permisos EncargadoDeStock sincronizados.';
GO

-- ============================================================
-- OperadorLogistico: prendas + pedidos realizados
-- ============================================================
INSERT INTO RolPermiso (Rol, IdPermiso)
SELECT 'OperadorLogistico', p.IdPermiso
FROM Permiso p
WHERE p.NombreMenu IN (
    'mnuPrendas',
    'mnuPedidosRealizados'
)
  AND ISNULL(p.EsFamilia, 0) = 0
  AND p.Estado = 1
  AND NOT EXISTS (
    SELECT 1 FROM RolPermiso x WHERE x.Rol = 'OperadorLogistico' AND x.IdPermiso = p.IdPermiso
  );
PRINT 'Permisos OperadorLogistico sincronizados.';
GO

PRINT '';
PRINT '=== Migración de nuevos roles completada. ===';
PRINT 'Roles creados/actualizados: Auditor, GerenteComercial, GerenteInventario,';
PRINT '                            EncargadoDeStock, OperadorLogistico';
PRINT 'Roles legacy sin cambios:   Administrador, Supervisor, Vendedor,';
PRINT '                            ControladorDeStock, OperadorDeInventario';
GO
